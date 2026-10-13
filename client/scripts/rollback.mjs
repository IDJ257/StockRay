import { readFile, writeFile, mkdir } from 'node:fs/promises';
import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const repo = fileURLToPath(new URL('../..', import.meta.url));
const baseline = (await readFile(new URL('../mpa-baseline.txt', import.meta.url), 'utf8')).trim();
const files = execFileSync('git', ['ls-tree', '-r', '--name-only', baseline, 'StockRay/wwwRoot'], { cwd: repo, encoding: 'utf8' }).trim().split('\n');
// Preserve the current entries before restoring the original committed MPA.
const backup = path.join(repo, 'client/.local-backup/pre-rollback');
await mkdir(backup, { recursive: true });
for (const entry of ['PublicDash', 'PrivateDash', 'Login', 'Register']) {
  const html = await readFile(path.join(repo, `StockRay/wwwRoot/${entry}/index.html`));
  await writeFile(path.join(backup, `${entry}.html`), html);
}
for (const file of files.filter(file => /\.(html|js)$/.test(file))) {
  const original = execFileSync('git', ['show', `${baseline}:${file}`], { cwd: repo });
  await mkdir(path.dirname(path.join(repo, file)), { recursive: true });
  await writeFile(path.join(repo, file), original);
}
console.log('Restored committed MPA HTML and JavaScript; CSS and assets were untouched.');
