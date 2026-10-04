# Authored character geometry preflight

2026-10-04. This completes an actionable body/skin loader gate for creators.
Original character visual/contact acceptance and the broader creator roadmap
remain open; this is not a complete arbitrary-rig authoring workflow.

## Source and contract

`Assets/Scripts/Eclipse/Modding/ModCharacterGeometry.cs` checks the composed
model document list before native parsing. `ModelLoader.Load` opts in for
non-core namespaced `EclipseBodyModel`/`EclipseSkinModels` paths. Core handles,
unqualified archival paths and recovered equipment keep their existing parsing
and first-definition node rules. Ignored legacy node/edge types cannot satisfy
authored bindings. Only authored documents receive strict type/number/reference
checks; inherited native definitions supply composition context.

Authored checks cover Scene/Figures, unique nodes/expanded edges and per-document
figures, helper dependency order/count/weights, positive COM child mass,
finite bounded numbers and nonnegative lengths/radii. Composed limits are 4096
nodes and 8192 expanded edges. Iterations permit 1..32 and expand using the
native `NameCI1` convention. Errors are `InvalidDataException` containing the
model path, XML element and field. All documents are checked before any
recovered Parse call. Existing prepared-model cleanup handles rejection;
existing actor spawn receipts carry that failure without adding Lua members.

`Tools/Animation/CharacterPipeline.py` now checks the same helper, sign and
iteration rules, including inherited expanded-edge references. Native point
animation format, move definitions, model handle schema and asset decoding
formats are unchanged. Public character-authoring, asset and warrior references,
tool guides and editor guide are updated in this change. Editor asset-kind
diagnostics remain distinct from runtime composed geometry checks.

## Evidence

- `TestAuthoredGeometry.ps1`: 34 production validator/loader checks with a
  controlled native parser. Tests include exact path/field diagnostics,
  composition bounds, repeated-edge collisions, late-overlay failure before
  any parse, archival duplicate bindings and ignored-type rejection.
- Python `TestCharacterPipeline.py`: 6 tests; `TestPackageCharacter.py`: 7 tests.
  Canonical recovered `mdl_skeleton.xml` also passed the export validator.
- `TestPreparedFormModel.ps1`: production ownership/loading/cleanup regression.
- `TestAuthoredGeometryUnity.ps1`: 11 full-game Unity 6.6 checks using isolated
  Campaign Tournament 3 and injected cached body/skin documents. Real native
  prepared model gains an authored MacroNode/triangle, stays hidden, owns a
  separate native body, disposes idempotently, and rejects a missing helper
  binding with the precise path/field while preserving the main fighter.
  Final log: `validation-f127377546704b57bd630d1d8e9ebfc2.log` in the marked
  `FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8` Temp fixture.
  The first run was invalidated by an unrelated Unity Editor Search startup
  exception; the probe now filters runtime model/fight exceptions like existing
  acceptance probes, and exits directly instead of deferring exit during
  play-mode shutdown. The final fresh run passed.
- All four managed assemblies compile through the existing portable Unity 6.6
  reference projects. Wiki builds 63 pages, validates 6820 links/assets and
  covers 246 public functions/aliases/callbacks. Editor generation/check and
  all 57 project tests pass; its generated API contract does not change.

## Limits

The native fixture injects cached XML deliberately: it tests actual loader and
native model construction without pretending to exercise public mod registration
or a Blender/Gymnast export. It does not validate newly authored animation,
attack contact, appearance, arbitrary skeletons/outfits, every character spawn
route, exported players or other platforms. Geometry validity does not imply
animation binding order, a usable move, collision quality or pleasing art.
Core-only rendering/gameplay regression and procedural rendering review are
separate fixtures.
