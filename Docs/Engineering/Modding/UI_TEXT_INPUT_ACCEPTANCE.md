# Owned editable mod UI text

Date: 2026-10-04. Public `kind = "text_input"` and `sf2.ui.get_text` add
name/search/seed/form entry to existing owned menu, modal and HUD surfaces.
This narrows E4/E8; it does not close the creator-platform objective.

## Implemented contract and source

- `ModUiRuntime.cs` appends the widget enum, validates default/explicit limits,
  valid UTF-16, controls and line breaks, commits user values before bounded
  notifications, and clears callbacks at teardown. Focus eligibility remains
  valid during notification; recursive user dispatch remains rejected.
- `MoonSharpScriptRuntimeUi.cs` accepts only text-input fields max_chars,
  placeholder and multiline, rejects malformed tables/values, exposes the owned
  getter, and sends string values through the existing Lua on_change callback.
  Defaults: max_chars 128 (range 1..8192 UTF-16 units), empty placeholder,
  single-line, empty initial value. Script setters are silent and reject invalid
  values before mutation. Getter accepts text, button, toggle and text_input IDs.
- `ModUiView.cs` builds native InputField components with game font/parchment,
  selection, plaintext value/placeholder, character limits and multiline mode.
  Model refresh preserves typing focus. Invalid native values restore the model.
  Input fields join existing focus traversal and ancestor interaction checks.
- `ModUiCoordinator.cs` captures gameplay for focused HUD editing, stops editing
  on background/blocked/disabled/hidden states, and updates navigation before the
  native input module. `ModUiGameBridge.cs` leaves arrows/Space/Enter to the field,
  routes Tab traversal and consumes Back to leave editing before dismissing views.
  Existing modal/menu lifetime input capture and gameplay gate are reused.
- Installed `Mods/example.text-input-lab` and mirrored manual editor starter use
  public APIs for a name HUD and multiline introduction modal. Apply reads the
  current string; values remain in Lua memory and never rename native fighters or
  save profiles. Round/fight/context cleanup closes both surfaces.
- Public UI reference, examples, editor guide, authored schema, generated defs/API
  metadata and actual LuaLS/VS Code checks update together. No asset/save format,
  runtime capability or existing Unity GUID changes.

## Verification

- `TestModUiRuntime.ps1`: 203 engine-independent ownership/state/input/teardown
  checks and existing native-control/Back/dialog source guards. New cases include
  limits/defaults, invalid UTF-16, Unicode/ASCII newlines, controls, silent setters,
  commit-before-notification, recursion, callback error, foreground and ancestors.
- `TestModUiLua.ps1`: 996 actual MoonSharp validation/lifetime/budget/example
  checks; text cases verify string callback/getter, silent setters, strict field
  applicability/types/ranges, forged/closed handles, argument counts, multiline,
  malformed edit preservation and failed/over-budget callback closure. The test
  resolves the archived Charged Strike example rather than its obsolete path.
- Four Unity 6.6 Windows-reference assemblies compile with ignored
  Temp/CreatorPlatformCompile projects (`dotnet msbuild`); the normal Visual
  Studio MSBuild SDK resolver is unavailable on this machine.
- Editor generate/check pass: 255 public bindings, 296 structures. 61 project
  checks, actual LuaLS completion/complete-example diagnostics and 26 isolated
  VS Code extension integration checks pass. These are not game playtests.
- Wiki build/types/search/link checks pass: 64 pages, 7036 local links/assets,
  64 tracked current-branch source links and three source-link tests. Existing
  duplicate-404 warning remains.
- `TestTextInputUnity.ps1` runs the installed mod in a marked full-game Unity 6.6
  fixture and fresh isolated profile. 35 checks pass: actual native InputField
  keyboard Event processing into Lua, plain text/placeholder/limits, multiline,
  focus retention across model notifications, silent updates, rejected controls,
  HUD capture and release, arrows/Tab/Back, disabled ancestors, native blocking,
  draft retention, modal/owner cleanup and callback diagnostics. Fresh result and
  successful editor exit required; native screenshot reviewed separately below.

## Native evidence and limits

The native fixture uses synthetic Unity keyboard Events through the production
InputField, not physical OS keyboard/IME input. The production Input System UI
module sends updateSelected before its navigation gate, allowing normal native
text editing while custom mod navigation is suspended. Physical keyboards,
composition/IME, clipboard behavior, mobile software keyboards, glyph/emoji
coverage, screen-reader/controller text entry, exported platforms and large-pack
performance remain unaccepted. Getter/model Unicode validation is not proof that
the recovered font can display every accepted code point.

Controlled core Tournament 3, canonical rig and fresh profile bound native
acceptance. General loadout/seed/search applications are Lua workflows; this
widget does not supply engine-wide custom save/configuration semantics.


Final native log: `Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-ee6384828f8f40b4b69146fd93a163be.log`.
The 35-check run exits zero with a fresh result. Capture waits for the entrance
fade to reach visible opacity and for the end-of-frame capture to finish before
Apply closes the modal. `Temp/TextInputReview-20261004/form.png` is the reviewed
native multiline modal screenshot. These ignored files are local evidence, not
shipped assets. Earlier synthetic runs passed behavior while their screenshot
caught the modal's zero-alpha entrance; that image did not prove visible modal
rendering and is not the final visual acceptance.
