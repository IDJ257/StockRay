import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { App } from './App';
import { store } from './store';

// Wait for both page styles to load before rendering, so SPA navigation cannot flash
// an unstyled page. media="not all" loads inactive styles without applying them.
async function boot() {
  if (import.meta.hot?.data.shutdown) await import.meta.hot.data.shutdown;
  await Promise.all([...document.querySelectorAll<HTMLLinkElement>('link[rel="stylesheet"]')].map(link => {
    if (link.sheet) return Promise.resolve();
    return new Promise<void>(resolve => {
      link.addEventListener('load', () => resolve(), { once: true });
      link.addEventListener('error', () => resolve(), { once: true });
    });
  }));
  const root = createRoot(document.getElementById('root')!);
  root.render(<BrowserRouter><App /></BrowserRouter>);
  void store.start();
  const onPageHide = (event: PageTransitionEvent) => {
    if (!event.persisted) void store.stop();
  };
  window.addEventListener('pagehide', onPageHide);
  if (import.meta.hot) import.meta.hot.dispose(data => {
    window.removeEventListener('pagehide', onPageHide);
    root.unmount(); data.shutdown = store.stop();
  });
}
void boot();
