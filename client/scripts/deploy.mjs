import { mkdir, readFile, writeFile, cp, access } from 'node:fs/promises';
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
  catch { await cp(destination, saved); }
}
// Keep hashed bundles from previous builds for tabs already running the SPA.
await cp(path.join(client, 'dist/spa'), path.join(webRoot, 'spa'), { recursive: true });
for (const entry of entries) {
  await writeFile(path.join(webRoot, entry, 'index.html'), html);
}
console.log('Deployed the shared SPA to the four existing static entry paths.');
