# Final Blow

A kill cam for every fight. On the knockout hit the camera slams in on the fallen
fighter while time nearly stops, holds, then drifts back out as the speed
returns. Critical hits and wall slams get a short punch-in.

It is built only from `sf2.fx.screen` grades with `zoom` and `zoom_offset_y`, so
no Lua runs during the fight. Each effect has its own switch under
**Options > Mod settings**: Knockout camera, Critical punch-in and Wall slam.

Enable it together with Chiaroscuro: Chiaroscuro drains the colour and adds the
impact sound, Final Blow moves the camera. Their slow motion combines (the
slowest wins) and so does the grading.

Verification: the definitions load headlessly through the real Lua runtime.
Framing, crop and timing need a playtest; tune `zoom`, `zoom_offset_y`, `hold`
and `duration` in `scripts/main.lua` to taste.
