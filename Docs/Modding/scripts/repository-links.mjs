import { execFileSync } from 'node:child_process';

// Use the Git index so sparse CI checkouts can validate source outside the
// materialized docs tree. Ignored/local files cannot make a broken public link pass.
export function repositoryInventory(root) {
  const git = (args) => execFileSync('git', args, {
    cwd: root, encoding: 'utf8', maxBuffer: 16 * 1024 * 1024,
  }).split('\0').filter(Boolean);
  const deleted = new Set(git(['ls-files', '--deleted', '-z']));
  const files = new Set(git(['ls-files', '-z']).filter((name) => !deleted.has(name)));
  const directories = new Set(['']);
  for (const filename of files) {
    const parts = filename.split('/');
    for (let i = 1; i < parts.length; i++) directories.add(parts.slice(0, i).join('/'));
  }
  return { files, directories };
}

export function checkRepositoryLink(url, repository, branch, inventory) {
  const origin = new URL(repository);
  const prefix = `${origin.pathname.replace(/\/$/, '')}/`;
  if (url.origin !== origin.origin || !url.pathname.startsWith(prefix)) return null;
  const [encodedKind, encodedRef, ...parts] = url.pathname.slice(prefix.length).split('/');
  let kind, ref;
  try { kind = decodeURIComponent(encodedKind); ref = decodeURIComponent(encodedRef); }
  catch { return { error: `invalid encoded repository ref: ${url.href}` }; }
  if (kind !== 'tree' && kind !== 'blob') return null;
  // A pinned historical snapshot is not a claim that its source is bundled now.
  // Its existence must be reviewed separately; do not fetch history/network in CI.
  if (ref !== branch) return /^[0-9a-f]{40}$/i.test(ref) ? { historical: true } :
    { error: `unverified repository ref (use ${branch} or a pinned full commit): ${url.href}` };
  let target;
  try { target = decodeURIComponent(parts.join('/')).replace(/\/$/, ''); }
  catch { return { error: `invalid encoded repository path: ${url.href}` }; }
  const available = kind === 'blob' ? inventory.files.has(target) : inventory.directories.has(target);
  return available ? {} : { error: `missing tracked repository ${kind === 'blob' ? 'file' : 'directory'}: ${url.href}` };
}
