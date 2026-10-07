# Location Params Editor

A standalone Windows program for viewing and editing fight-location layouts: the
`<id>_params.xml` files in `Assets/vanillaXml/locations/<id>/` and the identical
`params.txt` fallbacks in `Assets/Resources/gamedata/locations/<id>/`. It draws every
picture the way the game's `Location.cs` places it, so you can arrange layers by eye
instead of guessing coordinates.

## Run it

Hand someone the self-contained exe; it needs no .NET install:

```powershell
dotnet publish Tools/LocationParamsEditor -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o Tools/LocationParamsEditor/bin/publish
```

Then run `Tools/LocationParamsEditor/bin/publish/LocationParamsEditor.exe` (optionally with a
params file as its argument, or drop a file on the window). For development:
`dotnet run --project Tools/LocationParamsEditor`.

## Use it

1. **File > Open** a params file (`.xml` or `params.txt`).
2. Choose the **images folder**: the folder with the pictures the layout names
   (`ClassName` + `.png`), for example a location folder from the owner's art drop. The
   editor guesses a folder next to the file and remembers your choice per file.
3. Layers with `Path="Locations/<name>/"` draw another location's pictures (most dojo
   variants borrow the dojo floor and walls). Keep that location's folder, named `<name>`
   or `<name>_new`, next to the images folder, or add its parent with **File > Add extra
   search folder**.
4. Edit, then **Save** (or **Save as** for the `params.txt` copy). Saving keeps every
   attribute and element, including ones the editor does not know, and writes the file in
   place atomically.

| Area | What it does |
| --- | --- |
| Left | Layers in draw order (top is drawn first) and their pictures. Untick a layer to hide it. Missing pictures are red. |
| Centre | The layout. Wheel zooms, right or middle drag pans, left click selects the front-most picture, drag moves it, the handles resize it (Shift keeps proportions, Ctrl snaps to whole units). Arrow keys nudge by 1 (Shift: 10). |
| Right, top | Every attribute of the selected element; edit values in place, **Add attribute** / **Remove attribute**. |
| Right, bottom | **Pictures in folder** (unused ones in gold; double-click to place one in the selected layer at the view centre, sized by **Art scale**) and **Checks** (missing pictures, and pictures stretched more than 5% by their box). |
| Toolbar | Undo/redo, add layer, duplicate, delete, draw order, **Fit box to picture** (width from the picture's proportions, keeping the height), **Art scale** (layout units per pixel for new pictures; 0.5 for the 2x art), and the **Camera** slider. |

Overlays (View menu): the location bounds (`Width` × `Height`), the floor line at
`Height/2 − Floor`, the walls at `±(Width/2 − Wall)`, and the fighter start points from
`ModelsViewer` (drawn on the floor, horizontal position only). The **Camera** slider
previews parallax: a layer moves by −camera × `Factor`, so `Factor="1"` moves with the
fighters and smaller factors stay further back.

### Game viewport

**Game viewport** on the toolbar shows the location through the game's fight camera on a
screen of the chosen shape (21:9 by default; also 16:9, 18:9, 32:9, 16:10 and 4:3).
Everything outside the screen is darkened, and the line under it gives the camera zoom and
how much of the location the screen shows. It follows `Render.cs`:

- at zoom 1 the location's `Height` fills the screen height, so the screen is
  `Height × aspect` units wide;
- the zoom follows the fighters (`min(screen width / (distance + 300), 1)`), but the screen
  never shows more than `MaxWidth` (1100, `CameraSettings` in `internalSettings.xml`) units
  across nor more than the location's `Width`. On 21:9 the 1100 cap usually applies, so the
  game zooms in and crops the top and bottom of the location;
- the game layer and `Scaling="1"` layers zoom; other layers instead move down by
  `(Height/2 − Floor)/2 × (1 − zoom)`; every layer pans by the camera times its `Factor`,
  and the pan stops before the location's edge (less `MaxWidthDelta`, 50).

**Drive the fighters** to watch the camera react: click the canvas, then hold **A** / **D**
to walk player 1 and **Left** / **Right** to walk player 2 (hold **Shift** to run). They
start at the layout's start positions, stop at the walls and cannot walk through each
other. The camera centres between them and zooms with their distance; when the zoom is
capped (usually on 21:9), it follows player 1, the game camera's target, keeping them
`BindingLength` (100) inside the screen edge, so player 2 can leave the screen.

**Fighter distance** and **Camera** place the fighters by number instead (0 distance uses
the start positions); changing either resets driven fighters. You can keep editing in this
view; moves and resizes are converted back to layout units. Arrow keys nudge the selected
picture only in the normal editor view.

Animated effects (`SimpleEffect Type="Sequention"`) and particles are drawn as blue
placeholders; picture effects (`Type="Picture"`) are drawn as their picture without their
animation. Edit their timing in the attribute grid or by hand.

## Layout rules it follows

- The origin is the middle of the location; `X` points right and `Y` points down.
- A picture is centred at (`X`, `Y`) in its layer and stretched to `Width` × `Height`.
  `FlipX`/`FlipY` mirror it; `Color` tints it (multiply).
- Draw order is document order: later layers, and later pictures in a layer, are in front.
- `Scaling="1"` layers zoom with the game camera; the editor shows the unzoomed view.

## Check a layout from the command line

```powershell
LocationParamsEditor.exe --render <params file> <images folder> <out.png> [scale] [extra folder...]
```

renders the whole layout with its overlays to a PNG (scale 0.5 by default), for quick
reviews or for comparing before and after an edit. Add `--viewport 21:9` (and optionally
`--distance <units>` and `--camera <units>`) to render the game viewport instead.
