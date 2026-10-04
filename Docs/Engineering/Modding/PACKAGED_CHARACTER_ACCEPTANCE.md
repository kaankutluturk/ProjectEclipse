# Playable exported character packages

2026-10-04. This connects the existing Blender/Gymnast authoring workflow to
owned playable encounters. It advances character creation and creator tooling;
arbitrary rigs and the complete Minecraft-style objective remain open.

## Source and implemented contract

- `Tools/Animation/PackageCharacter.py` adds optional `playable=False` and CLI
  `--playable`. The generated fight sets `player_character = authored.warrior`
  when enabled. Its opponent uses the same warrior and generated Lua tactic.
  The default retains the normal campaign player. The parameter requires an
  actual boolean. Fresh output protection, atomic publication, all-clip/skin
  validation, limits, hashes, timing sidecars and exact native payload copies
  remain in place. Additional clips retain their individual control and spacing.
- `GymnastBridge.py` forwards the option only for `export --package`; other uses
  fail before opening/exporting the scene. The bridge still compares every
  exported sample against the evaluated Blender source pose.
- Both packager move generators and the low-level `BlenderCharacter.py`
  generator now emit current short-form conditions and interval bounds, native
  controlled events, readiness guards and `direction = "face_enemy"`. The prior
  long-form conditions were rejected by the current Lua binding. Without the
  explicit direction, native input played the clip but preserved the old facing
  after the fighters exchanged sides. Primary Punch and additional controls
  remain character-specific. Preview clips contain no attack intervals; core
  inherited attacks may still cause damage.
- `LegacyContentAdapterP1D.BuildTacticNode` creates empty QuickAttacks/Evades
  containers for a tactic without a core template when those lists are absent.
  The actual native Tactic constructor requires both containers. Previously an
  otherwise valid standalone Lua tactic registered and then broke native startup.
  Templated omissions stay absent so the native compiler can inherit core lists;
  authored nonempty lists are preserved. No core fallback is silently introduced.
- Reader acceptance now uses the matching Unity 6.6 editor and the existing
  descriptive `AnimationEndFrame` field. The production recovered reader is
  extracted unchanged. No public Lua binding, capability, saved-state format,
  shipped asset or Unity GUID changes.

## Verification

- Nine packaging tests pass, covering playable/default roles, byte preservation,
  strict option types, actual CLI multi-clip controls/spacing, rejection before
  publishing and existing package protection/asset validation.
- The complete Gymnast pipeline passes with Blender 5.0.1 and upstream Gymnast
  1.1.5 at `b44dea8ae549ff52ec8d08d7d1ad86f53db80702`: prepare the supplied
  visual body/IK scene, author hand motion, export a weighted test skin, compare
  all 60 samples and 67 points with evaluated poses, package watched, spaced and
  playable variants, execute their real Lua registration/map/fight/mode/AI,
  and read the unchanged payload in Unity. Watched/playable clip hashes match.
  The reader passes 16,145 checks. Final pipeline artifacts:
  `Temp/GymnastPipeline-ea42ddf0fe2d4ae88a6edd0ed50d4882`;
  reader log: `Temp/AnimationReader-b4b1870f777344159a70ec97c04e2f67/validation.log`.
- The low-level Blender 3.6 export/model/reader pipeline and repaired real Lua
  character/attack projection fixture pass separately. Those checks establish
  format/registration, not low-level package full-game acceptance.
- Eleven actual production tactic projection/native compiler/parser checks pass:
  standalone random/tabular empty lists and retained Standard inheritance.
  Existing AI regressions pass 14 shortlist/priority, 56 snapshot, 33 real native
  parser/clip/metadata and 95 actual Lua policy/lifetime/budget checks. These
  controlled tests are separate from native gameplay.
- All four Unity-reference managed assemblies compile using the ignored
  Temp/CreatorPlatformCompile projects. The machine's regular Visual Studio
  MSBuild SDK resolver remains unavailable.
- Editor generate/check, 61 project tests, actual LuaLS integration and 26
  isolated VS Code integration checks pass (255 bindings, 296 structures).
  No new public binding or schema member is introduced. Wiki build/types/search
  and link validation pass; the existing duplicate-404 warning remains.

## Native acceptance scope

`TestPackagedCharacterUnity.ps1` installs an unchanged generated package into a
marked isolated Unity project with a fresh profile. Its probe uses the integration
scene's canonical rig, original triangle skin and 60-sample hand motion. It
checks owned encounter roles, player controller flags, native Punch selection,
both facings, point count, wrist motion, native weighted skin helpers, generated
Lua opponent selection and surrender cleanup. `-OpponentOnly` checks the default
package role separately. Inputs, AI and spacing are controlled.

Native macro nodes update current Start from child Starts through
`Vector3f.GLGNIMKANCA`, which sums only XY. The helper check therefore verifies
planar binding, including native zero Z, rather than claiming animated 3D skin
depth. End holds the previous helper pose. Earlier failed assertions compared
the wrong pose/depth and are not acceptance evidence.

The unchanged generated playable package passes 28 full-game checks and exits
zero with a fresh result. Log:
`Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-95a8a2fb504d4e60a8b9bbcbdac6eb6e.log`.
The default watched package passes 16 checks and exits zero with a fresh result:
`Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-71345aae882245ccab3c9ecaa85127f5.log`.
The reviewed playable capture is
`Temp/PackagedCharacterReview-20261004/playable.png`. These ignored artifacts
are local evidence. The small triangle above the head is the original integration
test skin; this is not acceptance of a complete original character outfit.

This does not establish arbitrary skeletons, general mesh/material import,
complex outfits, full custom controllers, exported attack contacts, physical
input devices, mobile/exported players or large-pack performance. The generated
preview has no authored damage intervals. Experimental 3D remains deferred.
