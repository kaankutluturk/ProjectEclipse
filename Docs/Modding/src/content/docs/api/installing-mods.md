---
title: "Installing and enabling mods"
description: "Installing and enabling mods in the Eclipse modding API."
---

## Install a ZIP

On Windows or Android, open **Mods** from the title screen and choose **Install
ZIP**. Select a `.zip` file. Eclipse shows the mod's name, version, and ID before
you confirm. If the ID is already installed, **Replace mod** replaces that mod's
files; saved mod progress is kept. After installation, choose **Apply & Restart**
and enter Campaign. This also works for a ZIP containing one enclosing folder
around `mod.toml`. Each ZIP must contain one mod, including its declared
`scripts/*.lua` entrypoint. The installer rejects unsafe paths and symbolic links,
and limits packages to 10,000 entries, 256 MiB per file, and 1 GiB unpacked.
Older builds enforce a 512 MiB total limit; update the game before importing a
larger package such as DE128 with its upscaled map buttons.

On Android, the system file picker can select a ZIP from Downloads or another
document provider. You do not need to browse into `Android/data` or grant Eclipse
all-files access. Eclipse copies the chosen archive into its cache, validates it,
and installs its contents into its app-specific Mods directory. The ZIP is not
kept after installation. ZIP installation is currently available in the Unity
editor, Windows player, and Android player.

## Install a folder manually

You can still place a complete mod folder in your installation's `Mods`
directory, with `mod.toml` directly inside that folder. The folder name must
exactly match the manifest ID. A standalone Windows installation uses `Mods`
beside the executable; the Unity editor uses the project root `Mods` folder.
A launcher-managed installation may configure a shared Mods directory outside
its version folders. Android uses `Application.persistentDataPath/Mods`, normally
`Android/data/<package-id>/files/Mods`; the ZIP installer avoids the need to
access that location from a file manager. Restart Eclipse after manually adding
or updating mod files.

The Windows Eclipse launcher keeps shared mods in `<launcher folder>/Mods`
when it starts the game. Incremental game updates assemble a separate game
version and leave that shared folder and saves untouched. Keep your mods there
rather than modifying files inside a launcher's `versions` directory: game
updates verify and restore the published game files. Rolling back a game version
does not roll back mod files or saves.

Toggle the installed mods, then choose **Apply & Restart**. The game saves and reloads to the title screen;
enter Campaign to load the new selection. **Back / Cancel** discards unapplied
changes. In game, open the main **Menu** and choose **Return to Title** to reach
the mod list again.

Enabling a mod also enables its dependencies. Disabling a dependency disables
the mods that require it. Core remains enabled. Unmet requirements appear under
**Details**, and must be resolved before applying. New mods default to enabled;
selections persist across launches without moving or deleting mod folders.
Mod-owned saved progress is retained while a mod is disabled.

## Community mods (mod.io)

Players can also browse and install mods from
[Project Eclipse on mod.io](https://mod.io/g/project-eclipse) without leaving the
game. The browser is **off by default**: turn on **Options > Mod settings >
Community mods (mod.io)**, and a **Community mods** button appears at the top of
the **Mods** screen. No mod.io account or login is needed. Definitive Edition
ships with the game and is never listed there.

The browser lists mods that have a downloadable file. You can search by name,
sort by **Popular**, **Newest**, **Recently updated** or **Top rated**, and open
**Details** for the description, author, version, size and tags. **View on
mod.io** opens the mod's page, where you can rate or report it. Community mods are
made by players, not by the Eclipse or Definitive Edition team.

**Install** downloads the mod's ZIP, checks its size and checksum, and installs it
exactly like [Install a ZIP](#install-a-zip), with the same safety limits. Choose
**Apply & Restart** on the Mods screen to load it. **Remove** deletes the mod's
folder.

| Situation | What happens |
| --- | --- |
| The mod needs a different Eclipse core version | It is not installed; the message names the version it needs. |
| The mod names no `core` version | It installs, with a note that it may not work. |
| The mod ships `movesets/` files | It installs, with a note that online versus is off while it is enabled. |
| A mod with the same ID is already installed by hand | It is not installed; remove the existing mod first. |

### Updates

While the opt-in is on, Eclipse checks installed community mods once per launch,
when the title screen first opens, and installs newer files automatically. A mod
that is already loaded in this session cannot be replaced while running: its
update is downloaded now and installed the next time the game starts, before mods
load. Only mods installed through the browser are updated; a hand-installed mod
with the same ID is never touched.

Eclipse remembers which mods came from mod.io in `.eclipse-modio/installs.json`,
beside (not inside) the Mods folder. Deleting a mod's folder by hand also stops
its updates.

### Publishing to mod.io

Upload one mod per mod.io entry, as a ZIP laid out like any
[installable ZIP](#install-a-zip) (`mod.toml` at the root or inside one folder).
Give the file a version on mod.io, and declare the Eclipse core range your mod
was made for in `mod.toml`, so players are told when it will not work:

```toml
[[dependencies]]
id = "core"
version = ">=1.0 <2.0"
```

Keep the same mod `id` in every file you upload: an update whose `mod.toml` has a
different ID is refused. Mods that change movesets work offline; online versus
support for them is planned.

If a mod does not appear or cannot load, follow [Troubleshooting](../../guides/troubleshooting/).
