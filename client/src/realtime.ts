import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import type { StockUpdate } from './types';

// The application owns this service. Pages never start or stop a connection.
export interface Connection {
  state: HubConnectionState;
  start(): Promise<void>;
  stop(): Promise<void>;
  invoke(method: string, argument: string | string[]): Promise<void>;
  on(event: string, handler: (updates: StockUpdate[]) => void): void;
  onreconnecting(handler: () => void): void;
  onreconnected(handler: () => void): void;
  onclose(handler: () => void): void;
}

export function createConnection(): Connection {
  return new HubConnectionBuilder().withUrl('/sym-notif').withAutomaticReconnect().build();
}

export class Realtime {
  private connection?: Connection;
  private desired = new Set<string>();
  private joined = new Set<string>();
  private queue = Promise.resolve();
  private version = 0;
  private membershipVersion = 0;
  private running = false;
  private retry?: ReturnType<typeof setTimeout>;

  constructor(
    private publicUpdate: (updates: StockUpdate[]) => void,
    private privateUpdate: (updates: StockUpdate[]) => void,
    private restore: () => Promise<void>,
    private factory = createConnection
  ) {}

  private enqueue(work: () => Promise<void>): Promise<void> {
    this.queue = this.queue.then(work).catch(error => {
      console.error('SignalR:', error);
      // An invocation may have reached the server before its acknowledgement was
      // lost. Retire this generation immediately; never retry a join on it.
      if (this.running) { ++this.version; this.scheduleRestart(); }
    });
    return this.queue;
  }

  private scheduleRestart(): void {
    if (this.retry || !this.running) return;
    this.retry = setTimeout(() => { this.retry = undefined; void this.restart(); }, 5000);
  }

  start(): Promise<void> {
    if (this.running) return this.queue;
    this.running = true;
    return this.restart();
  }

  restart(): Promise<void> {
    const version = ++this.version;
    if (this.retry) clearTimeout(this.retry);
    this.retry = undefined;
    return this.enqueue(async () => {
      const old = this.connection;
      this.connection = undefined;
      if (old) await old.stop();
      this.joined.clear();
      if (!this.running || version !== this.version) return;
      ++this.membershipVersion;
      const connection = this.factory();
      this.connection = connection;
      const current = () => this.running && this.connection === connection && version === this.version;
      connection.on('ReceivePublicUpdate', updates => {
        if (current() && Array.isArray(updates)) this.publicUpdate(updates);
      });
      connection.on('ReceiveGroupUpdate', updates => {
        if (current() && Array.isArray(updates)) this.privateUpdate(updates);
      });
      connection.onreconnecting(() => {
        if (current()) { ++this.membershipVersion; this.joined.clear(); }
      });
      connection.onreconnected(() => {
        if (current()) void this.enqueue(() => this.connected(connection, version));
      });
      connection.onclose(() => { if (current()) this.scheduleRestart(); });
      await connection.start();
      if (current()) await this.connected(connection, version);
    });
  }

  private async connected(connection: Connection, version: number): Promise<void> {
    if (version !== this.version || !this.running) return;
    const membershipVersion = this.membershipVersion;
    this.joined.clear();
    await connection.invoke('JoinGroup', 'Public');
    if (version !== this.version || membershipVersion !== this.membershipVersion) return;
    await this.restore();
    if (version === this.version && membershipVersion === this.membershipVersion) {
      await this.sync(connection, version, membershipVersion);
    }
  }

  setGroups(groups: string[]): Promise<void> {
    this.desired = new Set(groups);
    const version = this.version;
    const membershipVersion = this.membershipVersion;
    return this.enqueue(async () => {
      const connection = this.connection;
      if (!this.retry && version === this.version && membershipVersion === this.membershipVersion && connection?.state === HubConnectionState.Connected) {
        await this.sync(connection, version, membershipVersion);
      }
    });
  }

  private async sync(connection: Connection, version: number, membershipVersion: number): Promise<void> {
    for (const group of this.joined) {
      if (!this.desired.has(group)) {
        await connection.invoke('LeaveGroup', group);
        if (version !== this.version || membershipVersion !== this.membershipVersion) return;
        this.joined.delete(group);
      }
    }
    const missing = [...this.desired].filter(group => !this.joined.has(group));
    if (missing.length) {
      // params string[] is one array argument, including for a single stock.
      await connection.invoke('JoinGroups', missing);
      if (version !== this.version || membershipVersion !== this.membershipVersion) return;
      missing.forEach(group => this.joined.add(group));
    }
  }

  stop(): Promise<void> {
    this.running = false;
    ++this.version;
    this.desired.clear();
    if (this.retry) clearTimeout(this.retry);
    this.retry = undefined;
    return this.enqueue(async () => {
      const connection = this.connection;
      this.connection = undefined;
      if (connection) await connection.stop();
      this.joined.clear();
    });
  }
}
