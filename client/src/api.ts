import type { CatalogStock, PublicDashboard, Stock } from './types';

export class ApiError extends Error {
  constructor(public status: number, message: string) { super(message); }
}

interface ErrorPayload {
  message?: string;
  detail?: string;
  title?: string;
  errors?: Record<string, string[]>;
}

let tokenPromise: Promise<string> | undefined;
let tokenVersion = 0;
let unauthorized: (() => void) | undefined;

export function onUnauthorized(handler: () => void): void { unauthorized = handler; }
export function resetCsrf(): void { tokenVersion++; tokenPromise = undefined; }

async function readResponse(response: Response): Promise<unknown> {
  if (!response.ok) {
    if (response.status === 401) unauthorized?.();
    const data: ErrorPayload = await response.json().catch(() => ({}));
    const errors = data.errors ? Object.values(data.errors).flat().join(' ') : '';
    throw new ApiError(response.status, data.detail || data.message || errors || data.title || 'Endpoint issues');
  }
  if (response.status === 204 || response.headers.get('content-length') === '0') return undefined;
  const body = await response.text();
  return body ? JSON.parse(body) : undefined;
}

async function getToken(): Promise<string> {
  if (!tokenPromise) {
    const version = tokenVersion;
    tokenPromise = fetch('/antiforgery/token', { credentials: 'same-origin', cache: 'no-store' })
      .then(async response => {
        const data = await readResponse(response) as { requestToken: string };
        return data.requestToken;
      }).catch(error => {
        if (version === tokenVersion) tokenPromise = undefined;
        throw error;
      });
  }
  return tokenPromise;
}

export async function read(url: string): Promise<unknown> {
  return readResponse(await fetch(url, { credentials: 'same-origin', cache: 'no-store' }));
}

export async function mutate(url: string, method: string, payload?: object): Promise<unknown> {
  const token = await getToken();
  return readResponse(await fetch(url, {
    method, credentials: 'same-origin',
    headers: { 'X-CSRF-TOKEN': token, ...(payload ? { 'Content-Type': 'application/json' } : {}) },
    body: payload ? JSON.stringify(payload) : undefined
  }));
}

export async function checkAccount(): Promise<boolean> {
  const response = await fetch('/account', { credentials: 'same-origin', cache: 'no-store' });
  if (response.status === 401 || response.status === 403) return false;
  if (!response.ok) throw new ApiError(response.status, 'Could not check account.');
  return true;
}

export const getPublic = async () => (await read('/public') as PublicDashboard).symbols;
export const getPrivate = async () => await read('/GetSymbols') as Stock[];
export const getCatalog = async () => await read('/GetAllSymbols') as CatalogStock[];
