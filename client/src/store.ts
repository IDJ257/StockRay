import * as api from './api';
import { Realtime } from './realtime';
import { mergeUpdates, type Stock, type StockUpdate } from './types';

export const authMessage = 'You are not authorized to see the contents of the page Please register or login. Click close for redirect';

interface State {
  authenticated: boolean | null;
  publicStocks: Stock[];
  privateStocks: Stock[];
  privateError: string;
}

class Store {
  private state: State = { authenticated: null, publicStocks: [], privateStocks: [], privateError: '' };
  private listeners = new Set<() => void>();
  private active = false;
  private privateActivated = false;
  private epoch = 0;
  private publicRequest = 0;
  private privateRequest = 0;
  private publicLive = new Map<number, StockUpdate>();
  private privateLive = new Map<number, StockUpdate>();
  private interval?: ReturnType<typeof setInterval>;
  private mutation = Promise.resolve();
  private accountRequest?: Promise<void>;

  readonly realtime = new Realtime(
    updates => {
      updates.forEach(update => this.publicLive.set(update.id, update));
      this.update({ publicStocks: mergeUpdates(this.state.publicStocks, updates) });
    },
    updates => {
      if (!this.state.authenticated || !this.privateActivated) return;
      updates.forEach(update => this.privateLive.set(update.id, update));
      this.update({ privateStocks: mergeUpdates(this.state.privateStocks, updates) });
    },
    async () => {
      await this.refreshAccount();
      await this.loadPublic();
      if (this.state.authenticated && this.privateActivated) await this.loadPrivate();
    }
  );

  getSnapshot = (): State => this.state;
  subscribe = (listener: () => void): (() => void) => {
    this.listeners.add(listener);
    return () => this.listeners.delete(listener);
  };
  private update(values: Partial<State>): void {
    this.state = { ...this.state, ...values };
    this.listeners.forEach(listener => listener());
  }

  async start(): Promise<void> {
    if (this.active) return;
    this.active = true;
    api.onUnauthorized(() => this.setAuthentication(false));
    window.addEventListener('focus', this.onFocus);
    this.interval = setInterval(() => {
      if (this.privateActivated) void this.refreshAccount();
    }, 60000);
    void this.loadPublic();
    await this.refreshAccount();
    if (this.active) void this.realtime.start();
  }

  private onFocus = (): void => { void this.refreshAccount(); };

  refreshAccount(): Promise<void> {
    if (this.accountRequest) return this.accountRequest;
    const epoch = this.epoch;
    const request = api.checkAccount().then(authenticated => {
      if (this.active && epoch === this.epoch) this.setAuthentication(authenticated);
    }).catch(error => console.error(error)).finally(() => {
      if (this.accountRequest === request) this.accountRequest = undefined;
    });
    this.accountRequest = request;
    return request;
  }

  private setAuthentication(authenticated: boolean, force = false): void {
    if (!force && this.state.authenticated === authenticated) return;
    const wasKnown = this.state.authenticated !== null;
    ++this.epoch;
    api.resetCsrf();
    this.privateActivated = false;
    this.privateLive.clear();
    void this.realtime.setGroups([]);
    this.update({ authenticated, privateStocks: [], privateError: authenticated ? '' : authMessage });
    if (wasKnown && this.active) void this.realtime.restart();
  }

  async loadPublic(): Promise<void> {
    const request = ++this.publicRequest;
    this.publicLive.clear();
    try {
      const stocks = await api.getPublic();
      if (this.active && request === this.publicRequest) {
        this.update({ publicStocks: mergeUpdates(stocks, [...this.publicLive.values()]) });
      }
    } catch (error) { console.error(error); }
  }

  async visitPrivate(): Promise<void> {
    await this.refreshAccount();
    if (!this.active || !this.state.authenticated) return;
    this.privateActivated = true;
    await this.loadPrivate();
  }

  private async loadPrivate(): Promise<void> {
    const request = ++this.privateRequest;
    const epoch = this.epoch;
    this.privateLive.clear();
    try {
      const stocks = await api.getPrivate();
      if (this.active && this.state.authenticated && epoch === this.epoch && request === this.privateRequest) {
        this.update({ privateStocks: mergeUpdates(stocks, [...this.privateLive.values()]), privateError: '' });
        // Do not await the service's queue from its recovery callback.
        void this.realtime.setGroups(stocks.map(stock => stock.name));
      }
    } catch (error) {
      if (epoch === this.epoch) this.update({ privateError: error instanceof api.ApiError && error.status === 401 ? authMessage : 'Endpoint issues' });
    }
  }

  async login(email: string, password: string): Promise<void> {
    await api.mutate('/login', 'POST', { Email: email, Password: password });
    this.setAuthentication(true, true);
    // A previous identity's account check must not overwrite this login.
    this.accountRequest = undefined;
  }

  async logout(): Promise<void> {
    try { await api.mutate('/logout', 'POST'); }
    catch (error) {
      if (!(error instanceof api.ApiError && error.status === 401)) throw error;
    }
    this.setAuthentication(false);
    this.accountRequest = undefined;
  }

  private changeWatchlist(work: (epoch: number) => Promise<void>): Promise<void> {
    const epoch = this.epoch;
    const operation = this.mutation.then(async () => {
      if (!this.active || !this.state.authenticated || epoch !== this.epoch) return;
      // Invalidate snapshots issued before this mutation.
      ++this.privateRequest;
      await work(epoch);
    });
    this.mutation = operation.catch(() => {});
    return operation;
  }

  add(ids: number[]): Promise<void> {
    return this.changeWatchlist(async epoch => {
      if (!ids.length) return;
      const added = await api.mutate('/AddSymbol', 'POST', { symbolIds: ids }) as Stock[];
      if (epoch !== this.epoch || !this.active) return;
      ++this.privateRequest;
      const stocks = [...this.state.privateStocks];
      added.forEach(stock => {
        if (!stocks.some(existing => existing.id === stock.id)) stocks.push(stock);
      });
      this.update({ privateStocks: mergeUpdates(stocks, [...this.privateLive.values()]) });
      await this.realtime.setGroups(stocks.map(stock => stock.name));
    });
  }

  remove(stock: Stock): Promise<void> {
    return this.changeWatchlist(async epoch => {
      await api.mutate('/RemoveSymbol', 'DELETE', { symbolIds: [stock.id] });
      if (epoch !== this.epoch || !this.active) return;
      ++this.privateRequest;
      this.privateLive.delete(stock.id);
      const stocks = this.state.privateStocks.filter(existing => existing.id !== stock.id);
      this.update({ privateStocks: stocks });
      await this.realtime.setGroups(stocks.map(existing => existing.name));
    });
  }

  stop(): Promise<void> {
    this.active = false;
    ++this.epoch;
    if (this.interval) clearInterval(this.interval);
    window.removeEventListener('focus', this.onFocus);
    return this.realtime.stop();
  }
}

export const store = new Store();
