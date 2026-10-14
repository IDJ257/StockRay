import { mkdir, readFile, writeFile, cp, access, unlink } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const client = fileURLToPath(new URL('..', import.meta.url));
const webRoot = path.resolve(client, '../StockRay/wwwRoot');
const backup = path.join(client, '.local-backup');
const entries = ['PublicDash', 'PrivateDash', 'Login', 'Register'];
const html = await readFile(path.join(client, 'dist/index.html'), 'utf8');
await mkdir(backup, { recursive: true });
for (const entry of entries) {
  const destination = path.join(webRoot, entry, 'index.html');
  const saved = path.join(backup, `${entry}.html`);
  try { await access(saved); }
  catch {
    try { await cp(destination, saved); }
    catch (error) { if (error.code !== 'ENOENT') throw error; }
  }
}
// Deploy the bundles, styles, and images produced by Vite from client sources.
// Keep hashed bundles from previous builds for tabs already running the SPA.
await cp(path.join(client, 'dist/spa'), path.join(webRoot, 'spa'), { recursive: true });
await writeFile(path.join(webRoot, 'index.html'), html);
// ASP.NET serves the shared entry for frontend routes, including direct visits.
for (const entry of entries) {
  try { await unlink(path.join(webRoot, entry, 'index.html')); }
  catch (error) { if (error.code !== 'ENOENT') throw error; }
}
console.log('Deployed the SPA to the shared wwwRoot/index.html entry.');
