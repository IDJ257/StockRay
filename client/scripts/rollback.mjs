import { readFile, writeFile, mkdir, unlink, cp } from 'node:fs/promises';
import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const repo = fileURLToPath(new URL('../..', import.meta.url));
const baseline = (await readFile(new URL('../mpa-baseline.txt', import.meta.url), 'utf8')).trim();
const files = execFileSync('git', ['ls-tree', '-r', '--name-only', baseline, 'StockRay/wwwRoot'], { cwd: repo, encoding: 'utf8' }).trim().split('\n');
// Preserve the current entries before restoring the original committed MPA.
const backup = path.join(repo, 'client/.local-backup/pre-rollback');
await mkdir(backup, { recursive: true });
for (const entry of ['index', 'PublicDash/index', 'PrivateDash/index', 'Login/index', 'Register/index']) {
  try {
    const html = await readFile(path.join(repo, `StockRay/wwwRoot/${entry}.html`));
    await writeFile(path.join(backup, `${entry.replaceAll('/', '-')}.html`), html);
  } catch (error) { if (error.code !== 'ENOENT') throw error; }
}
for (const file of files.filter(file => /\.(html|js)$/.test(file))) {
  const original = execFileSync('git', ['show', `${baseline}:${file}`], { cwd: repo });
  await mkdir(path.dirname(path.join(repo, file)), { recursive: true });
  await writeFile(path.join(repo, file), original);
}
// The original HTML expects these paths; keep the current client-owned assets.
for (const [source, destination] of [['Styles', 'Styles'], ['images', 'Assets']]) {
  await cp(path.join(repo, 'client/public/spa', source),
    path.join(repo, 'StockRay/wwwRoot', destination), { recursive: true });
}
// Let the original directory entries and root redirect handle the restored MPA.
try { await unlink(path.join(repo, 'StockRay/wwwRoot/index.html')); }
catch (error) { if (error.code !== 'ENOENT') throw error; }
console.log('Restored committed MPA HTML and JavaScript; CSS and assets were untouched.');
