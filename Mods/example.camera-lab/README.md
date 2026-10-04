# Camera Lab

Install this folder as a loose mod, then enter Act I tournament fight 3 (standard
or Eclipse Mode). The HUD offers player following with zoom, a Lua animated
arena sweep, the automatic native view while retaining ownership, and release.
The camera starts released. Another camera mod can reject acquisition; its error
appears in the HUD instead of silently losing ownership.

`fighter:acquire_camera(settings)` is called inside `on_tick`. The retained handle
uses `sf2.world.set_camera`, `is_camera_active` and `release_camera`. Settings are
complete replacements; `{}` restores native follow and zoom but holds the lease.
`center_x` is absolute arena X; `offset_y` is positive-down pan, and `zoom` is the
absolute game-layer scale. Native viewport/arena clamps still apply. The sweep
uses simulation frames and stops while paused. Buttons request a mode change for
the next tick; Release removes ownership immediately, even while paused.

The `presentation.camera` capability is independent of `presentation.visuals`.
This sample changes presentation and never moves or damages fighters. Closing the
HUD or ending the round/fight releases its transient camera handle. The engine
also expires ownership on owner death, round/session change or script disposal.
Do not save handles in persistent state.

The editor template `camera-lab` mirrors these files for manual copying. It is
outside the Training Blade command's automatic scaffolding. Native acceptance
and platform verification are recorded separately in the engineering report.
