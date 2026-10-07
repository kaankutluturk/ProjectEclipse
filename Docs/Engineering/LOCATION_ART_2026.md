# Location art 2026 (owner drop, 2026-10-07)

The owner supplied new art for every fight location in
`ResearchSources/new_assets/new_assets/locations/` (84 folders, 1038 PNGs, no layouts) and asked
for all previous location art to be removed. `Tools/Recovery/ImportLocationArt2026.py` installed it:

- 1631 old location sprites and atlases were removed from 33 art bundles, including their Cocos
  atlas data. Animated effect atlases under `Textures/Locations/<id>/atlases/` stay, because
  layouts still play them and the drop does not replace them. Shared `Textures/Location_effects/`
  (birds and similar) are not location art and stay too.
- Every drop PNG is a full-quad sprite at `Textures/Locations/<id>/<file name>` in
  CORE_LOCATIONS, with the drop's own import settings (1 pixel per unit, centre pivot, the
  `.meta` filter mode).
- Nine locations were left with no art and were removed with their layouts and versus arenas:
  `dojo_india25`, `dojo_summer_event`, `freeze_raid`, `freeze_raid0`, `hunter_raid`, `lava`,
  `new_year_24_china_dojo`, `new_year_china_dojo_raid` and `new_year_dojo`. The owner later
  supplied `dojo_india25` and `hunter_raid` (`ResearchSources/locs remaining/`); both are back,
  with their previous layouts, Unity `.meta` GUIDs and (for dojo_india25) versus arena. No stage, raid or Lua
  content referenced them as locations. The DE128 dojo picker choice for
  `new_year_24_china_dojo` was removed in the mod. `bridge` stays: Location.cs redirects it to
  `night_bridge`.

Layouts can be checked and edited visually with
[the Location Params Editor](../../Tools/LocationParamsEditor/README.md).

## Layout status

A layout (`params`) places each picture in a box of fixed size. A box that does not match the
new PNG's proportions stretches the picture. The drop contains no layouts, so each location uses
one of these:

### Official upscaled layouts (58): fit

The Definitive Edition layouts from the earlier drop (`ResearchSources/de128_assets`), made for
this art: arena, arkhos_raid, autumn, bamboo_grove, battlefield, burning_town, capsules,
castle_and_bridge, cave, chess_yard, dark_room, dock, dojo, dojo_hw22, eggs, emerald_forest,
factory, fatum_raid, flooded_village, flowers_field, flying_rocks, flying_rocks_small, fuji,
fungus_raid, graveyard_ships, haloween_dojo_2019, heaven, hoaxen_raid, ice_cave, lamps_on_water,
magic_rocks, megalith_raid, moon, mountain, neural_network, night_bridge, pink_lake, road,
ruins_village, ruins_village_small, sakura, shadow_gate, ships, skyport, snowy_peak, spaceship,
spaceship_thorny, statue, stone_dragon, stone_forest, stone_forest_thorny, swamp, village, volcano,
vortex_raid, vulcan_raid, waterfall, waterfall_small.

Five PNGs changed size since that drop and keep their pixel scale (their boxes were resized):
the `background_1` of dojo, fungus_raid and vortex_raid (2600 to 3072 wide, the same painting
with extended edges), and emerald_forest `left`/`right` (3200 to 1600 wide, a tighter crop; the
outer screen edge was kept). **Check emerald_forest in game.**

Pictures these layouts name but no drop contains (skipped at runtime, as before the import):
arena `flags_1`, fungus_raid `layer_0_2`, moon `layer_3`, and road's leaf particles.

### Converted layouts that fit (6)

The previous layout with its atlas references removed; every new picture matches its box:
american_event_24, blackness_raid, dojo_cny25, fall_event_24, hw24_village_night,
hw24_waterfall_night. The last two were not installed before; their layouts come from the
earlier drop's plain (not upscaled) folders.

### Converted layouts to check (6)

Most pictures fit, a few are visibly stretched: fear_raid (`fog1`), hunger_raid (`eyes_effect`,
`fire_effect`, `floor`), mountains_ny_25 (`forground`, `ground`), saturn_raid (6 pictures),
war_raid (lightning, plain and sky pictures). hunter_raid's layout, written for these files,
draws every picture at half its pixel width but most at 0.6 of its pixel height (a uniform
1.2× vertical squash); check whether that is intended.

### Locations that need new layouts (21)

The new art is an upscaled redesign. Most dojo variants now share the `dojo` floor, walls and
punch-bag holder, but their layouts are the old ones, so those pictures are stretched, unplaced
or missing. The art is installed and the location loads with its old layout until a new one is
written:

| Location | Problem |
| --- | --- |
| dojo_american_event_26 | No layout at all (new location, not loadable) |
| two_faced_city | No layout at all (new location, not loadable) |
| dojo_8_march | Shared dojo pieces stretched; own copies unplaced |
| dojo_american_event | Background stretched; floor, walls, holder unplaced; 4 old pieces missing |
| dojo_american_event_22 | As dojo_american_event |
| dojo_hw21 | Background stretched; floor, walls, holder unplaced; 5 old pieces missing |
| dojo_hw24 | 7 new pieces unplaced; 7 old pieces missing |
| dojo_india24 | Dojo pieces stretched |
| dojo_india25 | Its own 11 pictures fit; the shared dojo floor, walls and holder are stretched |
| dojo_indian_event | Background stretched; floor, walls, holder unplaced; 5 old pieces missing |
| dojo_indian_event_22 | Dojo pieces and `fields` stretched |
| dojo_new_year_22 | Background and dojo pieces stretched; `layer_4` missing |
| haloween_dojo | Background stretched; 6 pieces unplaced; 7 old pieces missing |
| new_year_china_dojo | Dojo pieces stretched; own copies unplaced |
| gatekeeper_raid | 5 dojo pieces unplaced; `bg` missing |
| hw24_ancient_temple | 16 numbered pieces (`01`–`16`) unplaced |
| ritual_battle_raid | 10 pictures stretched |
| ritual_fungus_raid | Draws fungus_raid's upscaled pieces into the old boxes (stretched) |
| ritual_hunger_raid | Draws hunger_raid's pieces; 3 stretched |
| ritual_vulcan_raid | Draws vulcan_raid's upscaled pieces into the old boxes (7 stretched) |
| whisper_raid | 6 new pieces unplaced; 6 old pieces missing |

`python Tools/Recovery/ImportLocationArt2026.py plan` prints this review from the installed state.
