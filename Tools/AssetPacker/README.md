# SF2DE AssetPacker

Small authoring tool for the runtime `.tar.lz4` art format.

```powershell
dotnet run --project Tools/AssetPacker -- pack .\my_asset .\my_asset.tar.lz4
dotnet run --project Tools/AssetPacker -- list .\my_asset.tar.lz4
dotnet run --project Tools/AssetPacker -- verify .\my_asset.tar.lz4
dotnet run --project Tools/AssetPacker -- extract .\my_asset.tar.lz4 .\unpacked
dotnet run --project Tools/AssetPacker -- info .\my_asset.tar.lz4
dotnet run --project Tools/AssetPacker -- compress .\my_asset.tar .\my_asset.tar.lz4
```

## Whole-catalog workspace (regroup / delete / repack)

For auditing or reorganizing all art at once, unpack every group into one folder, edit it, then
repack. Run from the repository root (archives are Git LFS, so `git lfs pull` first). A full unpack
needs about 3 GB of free disk space; use `--only` to unpack just some groups.

```powershell
$ws = "..\art-ws"   # any folder outside Assets/
dotnet run --project Tools/AssetPacker -- unpack-all $ws        # every group -> $ws\<GROUP>\
dotnet run --project Tools/AssetPacker -- report $ws ..\art.csv --refs Assets/vanillaXml --refs Assets/Scripts
dotnet run --project Tools/AssetPacker -- move $ws "UI/Items/**" ITEMS --dry-run
dotnet run --project Tools/AssetPacker -- move $ws "UI/Users/**" AVATARS
dotnet run --project Tools/AssetPacker -- move $ws "Textures/Locations/**" LOCATIONS
dotnet run --project Tools/AssetPacker -- delete $ws "UI/Items/old_icon_*"
dotnet run --project Tools/AssetPacker -- prune $ws
dotnet run --project Tools/AssetPacker -- check $ws
dotnet run --project Tools/AssetPacker -- repack-all $ws --dry-run
dotnet run --project Tools/AssetPacker -- repack-all $ws
python Tools/Audits/AuditNativeContent.py --deep
```

- Each folder in the workspace is one catalog group and becomes `<GROUP>.tar.lz4`. Folder names
  may use letters, digits, `_` and `-`. A new folder is a new group.
- `move`/`delete` work on logical asset **addresses** (`*` = one path segment, `**` = any depth,
  `?` = one character, case-insensitive). They move the `.meta` descriptor together with its
  texture/audio/model payload, copy shared atlas textures, rename on file-name collisions, and
  delete payloads that nothing references any more. `--from GROUP` limits the source group.
- Manual editing in Explorer also works: keep each `assets/*.meta` with the file named by its
  `texture=`/`file=` line. `check` reports broken references; `prune` deletes unreferenced
  payloads and folders with no descriptors.
- `report` writes a CSV of every descriptor (group, type, address, name, payload, size). With
  `--refs`, `name_mentioned` says whether the address or name appears in those text files. It is
  only a hint: names built at runtime (for example `"Map" + n`) show `no` but are still used.
- Lookup priority: when several groups ship the same address, the first group in catalog order
  wins. Order is the `groups` list in `workspace.json`; new groups are appended. Moving a
  duplicate keeps the higher-priority copy. `check` warns if an address now resolves to different
  content than at unpack time.
- `repack-all` validates first, packs only changed groups (deterministic output), keeps groups that
  were not unpacked, deletes archives (and their Unity `.meta`) of removed groups, and rewrites
  `catalog.json` with sizes, hashes and address lists. Unity creates `.meta` files for new
  archives on the next import. Fonts stay loose and are untouched.

Bundles are ordinary USTAR archives compressed as a standard LZ4 Frame. The runtime only discovers
logical assets through `*.meta` files. Other files are payloads and are ignored unless a descriptor
references them. References are always relative to the same archive.

## v1 descriptor format

Descriptors deliberately use a tiny `key=value` format. Empty lines and lines starting with `#` are
ignored. Unknown fields are safe for future tooling to preserve.

Sprite:

```ini
type=sprite
namespace=core
address=UI/Items/AgnisSeal
name=AgnisSeal
texture=textures/AgnisSeal.png
rect=0,0,256,256
pivot=0.5,0.5
border=0,0,0,0
pixels_per_unit=100
filter=1
aniso=1
wrap_u=1
wrap_v=1
mipmaps=false
vertices=-1,-1;1,-1;1,1;-1,1
triangles=0,1,2,2,3,0
uv=0,0;1,0;1,1;0,1
```

Audio:

```ini
type=audio
namespace=core
address=Sounds/UI/click
name=click
file=audio/click.wav
```

`sound` and `music` are accepted aliases for `audio`. The current player decoder intentionally only
accepts PCM16 WAV because that is what the recovered native art currently contains. XML and model
descriptors are rejected by v1 so configuration/model loading cannot accidentally migrate into this
experiment.

`AssetPacker verify` rejects unsafe paths, case-colliding entries, non-regular TAR entries, XML,
unknown asset types, missing required fields, and references to payloads outside the archive.
`AssetPacker info` prints the compressed size, decoded TAR size, and SHA-256 values used by
`Assets/Resources/SF2Content/Art/catalog.json`.

## GUI

`Tools/AssetPackerGui` is a Windows app for the same workspace workflow. It calls the
commands above in-process, so results match the CLI exactly.

```powershell
dotnet run --project Tools/AssetPackerGui
```

1. Set **Game project** (auto-detected when started inside the repo) and an empty **Workspace
   folder** outside `Assets/`, then click **1. Unpack all bundles**.
2. Browse groups on the left; filter by address or name (`*`/`**` globs work). Selecting a sprite
   shows its texture with the sprite rect outlined in red (atlas members share one texture);
   audio has **Play sound**; models and atlas data show their text.
3. Select assets (Ctrl/Shift-click, Ctrl+A, **Select all shown**), then **Move selected to
   group...** (type a new name to create a group) or **Delete selected** / the Delete key. In a
   specific group view, only that group is affected; in **(all groups)** every copy of the address is.
4. **Delete unused files**, **Check for problems**, **Export CSV report...** as needed.
5. **2. Repack into game** rewrites the changed archives and `catalog.json` in the game project.

The last workspace folder is remembered in `%LOCALAPPDATA%\EclipseAssetPacker\workspace.txt`.

## Standalone exe

To hand the tools to someone without the .NET SDK, publish a single self-contained exe
(`Tools/AssetPackerGui` for the app, `Tools/AssetPacker` for the CLI):

```powershell
dotnet publish Tools/AssetPackerGui -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o Tools/AssetPackerGui/bin/publish
dotnet publish Tools/AssetPacker -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o Tools/AssetPacker/bin/publish
```

Then use `AssetPacker.exe <command> ...` from the repository root.
