import { Fragment, useEffect, useState, useSyncExternalStore } from 'react';
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
  return <main className="page">
    <section id="dashboardGrid" className="grid" aria-live="polite">
      {publicStocks.map(stock => <Fragment key={stock.id}>
        <StockCard stock={stock} />
        {stock.id === 3 && <div className="divider" />}
      </Fragment>)}
    </section>
    <div className="indicator-arrow" />
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
