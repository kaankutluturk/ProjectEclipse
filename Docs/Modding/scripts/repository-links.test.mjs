import assert from 'node:assert/strict';
import { execFileSync } from 'node:child_process';
import { existsSync, mkdirSync, mkdtempSync, rmSync, unlinkSync, writeFileSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import test from 'node:test';
import { checkRepositoryLink, repositoryInventory } from './repository-links.mjs';

const repository = 'https://github.com/dawc17/ProjectEclipse';
const check = (value, inventory) => checkRepositoryLink(new URL(value), repository, 'main', inventory);

test('source links require tracked files/directories, with exact Git casing and URL decoding', () => {
  const inventory = {
    files: new Set(['ArchivedMods/example.sample/mod.toml', 'Docs/Modding/My guide.md']),
    directories: new Set(['', 'ArchivedMods', 'ArchivedMods/example.sample', 'Docs/Modding']),
  };
  assert.deepEqual(check(`${repository}/tree/main/ArchivedMods/example.sample`, inventory), {});
  assert.deepEqual(check(`${repository}/blob/main/Docs/Modding/My%20guide.md#example`, inventory), {});
  assert.match(check(`${repository}/tree/main/Mods/example.sample`, inventory).error, /missing tracked/);
  assert.match(check(`${repository}/tree/%6dain/Mods/example.sample`, inventory).error, /missing tracked/);
  assert.match(check(`${repository}/blob/main/archivedmods/example.sample/mod.toml`, inventory).error, /missing tracked/);
  assert.match(check(`${repository}/blob/main/ArchivedMods/example.sample`, inventory).error, /file/);
  assert.match(check(`${repository}/tree/main/ArchivedMods/example.sample/mod.toml`, inventory).error, /directory/);
  assert.match(check(`${repository}/blob/main/Docs/%ZZ`, inventory).error, /encoded/);
});

test('other repositories and pinned snapshots cannot prove current bundled source', () => {
  const inventory = { files: new Set(), directories: new Set() };
  assert.equal(check('https://github.com/another/project/tree/main/Mods/example.sample', inventory), null);
  assert.equal(check(`${repository}/issues/1`, inventory), null);
  assert.deepEqual(check(`${repository}/tree/379c6726f09ceb3605837989e58dea7aea6c25db/Mods/de128`, inventory), { historical: true });
  assert.match(check(`${repository}/tree/main/Mods/de128`, inventory).error, /missing tracked/);
  assert.match(check(`${repository}/tree/old-branch/Mods/de128`, inventory).error, /unverified repository ref/);
});

test('sparse checkout inventory accepts omitted tracked source and rejects local/deleted files', () => {
  const temporary = fileURLToPath(new URL('../../../Temp/', import.meta.url));
  mkdirSync(temporary, { recursive: true });
  const fixture = mkdtempSync(path.join(temporary, 'ModdingRepositoryLinks-'));
  const git = (...args) => execFileSync('git', args, { cwd: fixture, stdio: 'pipe' });
  try {
    git('init', '-q');
    for (const filename of ['Docs/Modding/guide.md', 'ArchivedMods/example.sample/mod.toml']) {
      mkdirSync(path.dirname(path.join(fixture, filename)), { recursive: true });
      writeFileSync(path.join(fixture, filename), 'fixture\n');
    }
    git('add', '.');
    git('-c', 'user.name=Wiki test', '-c', 'user.email=wiki-test@example.invalid', 'commit', '-qm', 'fixture');
    git('sparse-checkout', 'init', '--cone');
    git('sparse-checkout', 'set', 'Docs/Modding');
    assert.equal(existsSync(path.join(fixture, 'ArchivedMods/example.sample/mod.toml')), false);
    writeFileSync(path.join(fixture, 'Docs/Modding/local.md'), 'not published\n');
    unlinkSync(path.join(fixture, 'Docs/Modding/guide.md'));
    const inventory = repositoryInventory(fixture);
    assert.deepEqual(check(`${repository}/tree/main/ArchivedMods/example.sample`, inventory), {});
    assert.match(check(`${repository}/blob/main/Docs/Modding/local.md`, inventory).error, /missing tracked/);
    assert.match(check(`${repository}/blob/main/Docs/Modding/guide.md`, inventory).error, /missing tracked/);
  } finally {
    // The recursive removal is confined to the exact fixture created above.
    assert.equal(path.dirname(path.resolve(fixture)), path.resolve(temporary));
    rmSync(fixture, { recursive: true, force: true });
  }
});
