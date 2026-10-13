export interface Stock {
  id: number;
  name: string;
  open: number;
  high: number;
  low: number;
  currentPrice: number;
}

export interface StockUpdate {
  id: number;
  open: number;
  high: number;
  low: number;
  currentPrice: number;
}

export interface CatalogStock {
  id: number;
  name: string;
}

export interface PublicDashboard { symbols: Stock[] }

export function formatNumber(value: number | null | undefined): string {
  return value == null || Number.isNaN(value) ? '--' : Number(value).toFixed(2);
}

export function mergeUpdates(stocks: Stock[], updates: StockUpdate[]): Stock[] {
  const byId = new Map(updates.map(update => [update.id, update]));
  return stocks.map(stock => ({ ...stock, ...byId.get(stock.id) }));
}
