-- Final Blow: a kill cam for every fight. The knockout hit throws the camera in
-- on the fallen fighter while time crawls; critical hits and wall slams get a
-- short punch-in. No Lua runs during the fight: the engine reads these tables.
-- Pairs with Chiaroscuro, whose knockout drains the colour and adds the impact.
local sf2 = require("sf2")

local function toggle(id, label, description)
    return sf2.settings.toggle { id = id, label = label, description = description, default = true }
end
local knockout = toggle("knockout_camera", "Knockout camera",
    "The final hit pulls the camera in on the fallen fighter in slow motion.")
local critical = toggle("critical_punch", "Critical punch-in", "Critical hits jolt the camera in for an instant.")
local wall = toggle("wall_slam", "Wall slam", "Being smashed into the arena wall jolts the camera.")

-- The kill cam: slam in, hold on the body while time nearly stops, then drift
-- back out as the speed returns.
sf2.fx.screen {
    id = "knockout_camera", setting = knockout, trigger = "ko",
    zoom = 1.65, zoom_offset_y = -70, hold = 0.7, duration = 2.2, time_scale = 0.12,
    vignette = 0.5, contrast = 1.12,
}
-- A heavy critical stops the world for a few frames.
sf2.fx.screen {
    id = "critical_punch", setting = critical, trigger = "critical",
    zoom = 1.14, zoom_offset_y = -20, hold = 0.06, duration = 0.32, time_scale = 0.4, contrast = 1.08,
}
sf2.fx.screen {
    id = "wall_slam", setting = wall, trigger = "wall",
    zoom = 1.1, zoom_offset_y = -15, hold = 0.04, duration = 0.3, time_scale = 0.6,
}
