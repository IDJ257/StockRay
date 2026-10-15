import { useEffect, useState, useSyncExternalStore } from 'react';
import { useNavigate } from 'react-router-dom';
import { getCatalog } from './api';
import { store } from './store';
import { formatNumber, type CatalogStock, type Stock } from './types';

function StockCard({ stock, remove }: { stock: Stock; remove?: () => void }) {
  return <div className="bubble">
    {remove && <button className="bubble-close" onClick={remove}>X</button>}
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

export function ErrorDisplay({ message, close }: { message: string; close: () => void }) {
  return <div className="error-blimp">
    <div className="error-text">
      <div className="error-title">Something went wrong</div>
      <div className="error-message">{message}</div>
    </div>
    <button className="error-close-btn" onClick={close}>Close</button>
  </div>;
}

export function PrivateDashboard() {
  const { privateStocks, privateError, authenticated } = useSyncExternalStore(store.subscribe, store.getSnapshot);
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const [catalog, setCatalog] = useState<CatalogStock[]>([]);
  const [selected, setSelected] = useState<number[]>([]);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (authenticated === true) void store.visitPrivate();
  }, [authenticated]);
  useEffect(() => {
    if (!authenticated) { setOpen(false); setSelected([]); }
  }, [authenticated]);

  async function openStocks() {
    try {
      const stocks = await getCatalog();
      if (!store.getSnapshot().authenticated) return;
      setCatalog(stocks); setSelected([]); setOpen(true);
    } catch (error) { console.error(error); }
  }
  function closeStocks() { setOpen(false); setSelected([]); }
  function toggle(id: number) {
    setSelected(ids => ids.includes(id) ? ids.filter(existing => existing !== id) : [...ids, id]);
  }
  async function add() {
    if (busy) return;
    setBusy(true);
    try { await store.add(selected); closeStocks(); }
    catch (error) { console.error(error); }
    finally { setBusy(false); }
  }
  async function remove(stock: Stock) {
    try { await store.remove(stock); }
    catch (error) { console.error(error); setError('Endpoint issues'); }
  }

  return <>
    <main id="mainRoot">
      {privateError ? <ErrorDisplay message={privateError} close={() => navigate('/PublicDash/')} /> : authenticated === null ? null : <>
        <div id="stocks" className="privateDash">
          <div className="bubble-add" onClick={() => void openStocks()}><div className="bubble-add-content">+</div></div>
          {privateStocks.map(stock => <StockCard key={stock.id} stock={stock} remove={() => void remove(stock)} />)}
        </div>
        <div className="indicator-arrow" />
        {error && <ErrorDisplay message={error} close={() => setError('')} />}
      </>}
    </main>
    <div className={`modal-overlay${open ? '' : ' hidden'}`} id="modal">
      <div className="modal">
        <div className="modal-header"><h2>Add Items</h2><p>Select items from the list</p></div>
        <ul className="item-list" id="itemList">
          {catalog.map(stock => <li key={stock.id} className="item" onClick={() => toggle(stock.id)}>
            <span className="item-name">{stock.name}</span>
            <input type="checkbox" checked={selected.includes(stock.id)} onClick={event => event.stopPropagation()} onChange={() => toggle(stock.id)} />
          </li>)}
        </ul>
        <div className="modal-footer">
          <button className="btn btn-cancel" id="cancelBtn" onClick={closeStocks}>Cancel</button>
          <button className="btn btn-add" id="addBtn" onClick={() => void add()} disabled={busy}>Add</button>
        </div>
      </div>
    </div>
  </>;
}
