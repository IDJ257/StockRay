import { useEffect, useRef, useState, useSyncExternalStore } from 'react';
import { useNavigate } from 'react-router-dom';
import { getCatalog } from './api';
import { authMessage, store } from './store';
import { formatNumber, type CatalogStock, type Stock } from './types';

function StockCard({ stock, remove }: { stock: Stock; remove?: () => void }) {
  return <div className="bubble">
    {remove && <button className="bubble-close" type="button" aria-label={`Remove ${stock.name} from your watchlist`} onClick={remove}>&times;</button>}
    <h3>{stock.name}</h3>
    <div className="micro-grid">
      <div className="current-price">{formatNumber(stock.currentPrice)}</div>
      <div className="micro-pill open">Open <span>{formatNumber(stock.open)}</span></div>
      <div className="micro-pill high">High <span>{formatNumber(stock.high)}</span></div>
      <div className="micro-pill low">Low <span>{formatNumber(stock.low)}</span></div>
    </div>
  </div>;
}

export function PublicDashboard() {
  const { publicStocks } = useSyncExternalStore(store.subscribe, store.getSnapshot);
  const featured = publicStocks.slice(0, 3);
  const remaining = publicStocks.slice(3);
  const movements = publicStocks.reduce((counts, stock) => {
    if (!Number.isFinite(stock.currentPrice) || !Number.isFinite(stock.open)) return counts;
    if (stock.currentPrice > stock.open) counts.above++;
    else if (stock.currentPrice < stock.open) counts.below++;
    else counts.unchanged++;
    return counts;
  }, { above: 0, below: 0, unchanged: 0 });
  const hasStocks = publicStocks.length > 0;

  return <main className="page">
    <header className="dashboard-header">
      <div className="dashboard-brand">StockRay<span>Public overview</span></div>
      <div className="dashboard-intro">
        <h1>Market Overview</h1>
        <p>A closer look at current prices, opening values, and daily highs and lows.</p>
      </div>
      <dl className="dashboard-summary" aria-label="Summary of displayed stocks">
        <div><dt>Symbols in view</dt><dd>{hasStocks ? publicStocks.length : '—'}</dd></div>
        <div className="summary-above"><dt>Above open</dt><dd>{hasStocks ? movements.above : '—'}</dd></div>
        <div className="summary-below"><dt>Below open</dt><dd>{hasStocks ? movements.below : '—'}</dd></div>
        <div><dt>Unchanged</dt><dd>{hasStocks ? movements.unchanged : '—'}</dd></div>
      </dl>
    </header>
    <section className="dashboard-section" aria-labelledby="featured-heading">
      <div className="dashboard-section-heading">
        <h2 id="featured-heading">At a glance</h2>
        <span>Current · Open · High · Low</span>
      </div>
      {hasStocks ? <div id="dashboardGrid" className="grid">
        {featured.map(stock => <StockCard key={stock.id} stock={stock} />)}
      </div> : <p className="dashboard-waiting" role="status">Waiting for stock data…</p>}
    </section>
    {remaining.length > 0 && <>
      <hr className="divider" />
      <section className="dashboard-section dashboard-more" tabIndex={0} aria-labelledby="more-heading">
        <div className="dashboard-section-heading">
          <h2 id="more-heading">More symbols</h2>
          <span className="dashboard-reveal-hint">Hover or focus to explore</span>
        </div>
        <div className="grid">
          {remaining.map(stock => <StockCard key={stock.id} stock={stock} />)}
        </div>
      </section>
    </>}
    <footer className="dashboard-footer"><span>StockRay</span><p>Simulated prices · A view of the stocks in focus.</p></footer>
    <div className="indicator-arrow" aria-hidden="true" />
  </main>;
}

export function ErrorDisplay({ message, close, page = false }: { message: string; close: () => void; page?: boolean }) {
  const unauthorized = message === authMessage;
  const Heading = page ? 'h1' : 'h2';
  return <section className={page ? 'error-page' : 'error-notice'} aria-label="Dashboard message">
    {page && <header className="error-masthead">StockRay<span>Private dashboard</span></header>}
    <div className="error-blimp">
      <div className="error-context"><span className="error-mark" aria-hidden="true">!</span><span>{unauthorized ? 'Account access' : 'Dashboard update'}</span></div>
      <div className="error-text" role="alert">
        <Heading className="error-title">{unauthorized ? 'Your dashboard is waiting.' : 'Something went wrong.'}</Heading>
        <p className="error-message">{unauthorized ? 'Log in or create an account to view your private dashboard and follow your saved symbols.' : message}</p>
      </div>
      <div className="error-actions">
        <p>{page ? 'You can return to the public overview using the button below.' : 'Close this message to return to your dashboard.'}</p>
        <button className="error-close-btn" type="button" onClick={close}>{page ? 'Back to public dashboard' : 'Close message'}<span aria-hidden="true">&rarr;</span></button>
      </div>
    </div>
    {page && <footer className="error-footer">StockRay<span>Keep your perspective on the market.</span></footer>}
  </section>;
}

export function PrivateDashboard() {
  const { privateStocks, privateError, authenticated } = useSyncExternalStore(store.subscribe, store.getSnapshot);
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const [catalog, setCatalog] = useState<CatalogStock[]>([]);
  const [selected, setSelected] = useState<number[]>([]);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const [addError, setAddError] = useState('');
  const dialog = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (authenticated === true) void store.visitPrivate();
  }, [authenticated]);
  useEffect(() => {
    if (!authenticated) { setOpen(false); setSelected([]); }
  }, [authenticated]);
  useEffect(() => {
    if (!open) return;
    const previousFocus = document.activeElement;
    const focusable = () => Array.from(dialog.current?.querySelectorAll<HTMLElement>('button:not(:disabled), input:not(:disabled)') ?? []);
    focusable()[0]?.focus();
    const onKey = (event: KeyboardEvent) => {
      if (event.key === 'Escape') { event.preventDefault(); setOpen(false); setSelected([]); }
      if (event.key === 'Tab') {
        const controls = focusable();
        const first = controls[0];
        const last = controls[controls.length - 1];
        if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last?.focus(); }
        else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first?.focus(); }
      }
    };
    document.addEventListener('keydown', onKey);
    document.body.classList.add('watchlist-modal-open');
    return () => {
      document.removeEventListener('keydown', onKey);
      document.body.classList.remove('watchlist-modal-open');
      if (previousFocus instanceof HTMLElement && previousFocus.isConnected) previousFocus.focus();
    };
  }, [open]);

  async function openStocks() {
    try {
      const stocks = await getCatalog();
      if (!store.getSnapshot().authenticated) return;
      setCatalog(stocks); setSelected([]); setAddError(''); setOpen(true);
    } catch (error) { console.error(error); }
  }
  function closeStocks() { setOpen(false); setSelected([]); setAddError(''); }
  function toggle(id: number) {
    setAddError('');
    setSelected(ids => ids.includes(id) ? ids.filter(existing => existing !== id) : [...ids, id]);
  }
  async function add() {
    if (busy || !selected.length) return;
    const duplicates = store.getSnapshot().privateStocks.filter(stock => selected.includes(stock.id));
    if (duplicates.length) {
      setAddError(`${duplicates.map(stock => stock.name).join(', ')} ${duplicates.length === 1 ? 'is' : 'are'} already in your watchlist. Deselect ${duplicates.length === 1 ? 'this symbol' : 'these symbols'} to continue.`);
      return;
    }
    setAddError('');
    setBusy(true);
    try { await store.add(selected); closeStocks(); }
    catch (error) { console.error(error); setAddError('Could not add the selected symbols. Please try again.'); }
    finally { setBusy(false); }
  }
  async function remove(stock: Stock) {
    try { await store.remove(stock); }
    catch (error) { console.error(error); setError('Endpoint issues'); }
  }

  const movements = privateStocks.reduce((counts, stock) => {
    if (!Number.isFinite(stock.currentPrice) || !Number.isFinite(stock.open)) return counts;
    if (stock.currentPrice > stock.open) counts.above++;
    else if (stock.currentPrice < stock.open) counts.below++;
    else counts.unchanged++;
    return counts;
  }, { above: 0, below: 0, unchanged: 0 });

  return <>
    <main id="mainRoot">
      {privateError ? <ErrorDisplay page message={privateError} close={() => navigate('/PublicDash/')} /> : authenticated === null ? null : <>
        <div className="watchlist-page">
          <header>
            <div className="watchlist-brand">StockRay<span>Private dashboard</span></div>
            <div className="watchlist-intro">
              <div><h1>Your watchlist</h1><p>The symbols you follow, together in one place.</p></div>
              <button className="watchlist-add watchlist-add-primary" type="button" onClick={() => void openStocks()}>
                <svg className="watchlist-add-stripe" aria-hidden="true" focusable="false"><rect x="1" y="1" rx="13" pathLength="100" /></svg>
                <span aria-hidden="true">+</span>Add symbols
              </button>
            </div>
            <dl className="watchlist-summary" aria-label="Summary of your saved stocks">
              <div><dt>Saved symbols</dt><dd>{privateStocks.length}</dd></div>
              <div className="watchlist-above"><dt>Above open</dt><dd>{movements.above}</dd></div>
              <div className="watchlist-below"><dt>Below open</dt><dd>{movements.below}</dd></div>
              <div><dt>Unchanged</dt><dd>{movements.unchanged}</dd></div>
            </dl>
          </header>
          <section aria-labelledby="watchlist-heading">
            <div className="watchlist-section-heading"><h2 id="watchlist-heading">Following</h2><span>Current · Open · High · Low</span></div>
            {privateStocks.length > 0 ? <div id="stocks" className="privateDash">
              {privateStocks.map(stock => <StockCard key={stock.id} stock={stock} remove={() => void remove(stock)} />)}
            </div> : <div id="stocks" className="watchlist-empty">
              <span className="watchlist-empty-mark" aria-hidden="true">+</span>
              <h3>A little space for your perspective.</h3>
              <p>Add the symbols you want to follow. Their prices, highs, and lows will appear here.</p>
              <button className="watchlist-add" type="button" onClick={() => void openStocks()}>Add your first symbols<span aria-hidden="true">&rarr;</span></button>
            </div>}
          </section>
          <footer className="watchlist-footer"><span>StockRay</span><p>Simulated prices · Your personal market overview.</p></footer>
        </div>
        {error && <ErrorDisplay message={error} close={() => setError('')} />}
      </>}
      <div className="indicator-arrow" aria-hidden="true" />
    </main>
    <div className={`modal-overlay${open ? '' : ' hidden'}`} id="modal" inert={!open} aria-hidden={!open}
      onClick={event => { if (event.target === event.currentTarget) closeStocks(); }}>
      <div className="modal" ref={dialog} role="dialog" aria-modal="true" aria-labelledby="addSymbolsTitle" aria-describedby="addSymbolsDescription">
        <div className="modal-header">
          <div className="watchlist-modal-brand">StockRay<span>Your watchlist</span></div>
          <button className="watchlist-modal-close" type="button" aria-label="Close symbol selection" onClick={closeStocks}>&times;</button>
          <h2 id="addSymbolsTitle">Make room for more.</h2><p id="addSymbolsDescription">Select the symbols you would like to follow.</p>
        </div>
        <ul className="item-list" id="itemList">
          {catalog.map(stock => <li key={stock.id}><label className="item">
            <span className="item-name">{stock.name}{privateStocks.some(saved => saved.id === stock.id) && <span className="item-saved">Already in your watchlist</span>}</span>
            <input type="checkbox" checked={selected.includes(stock.id)} onChange={() => toggle(stock.id)} />
          </label></li>)}
        </ul>
        {addError && <p className="watchlist-add-error" role="alert">{addError}</p>}
        <div className="modal-footer">
          <span className="watchlist-selection" role="status">{selected.length} selected</span>
          <button className="btn btn-cancel" type="button" id="cancelBtn" onClick={closeStocks}>Cancel</button>
          <button className="btn btn-add" type="button" id="addBtn" onClick={() => void add()} disabled={busy || !selected.length}>{busy ? 'Adding…' : 'Add symbols'}</button>
        </div>
      </div>
    </div>
  </>;
}
