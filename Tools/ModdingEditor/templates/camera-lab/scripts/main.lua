local sf2 = require("sf2")
local camera, hud, mode, frames
local function release()
    if camera then sf2.world.release_camera(camera) end
    camera = nil
end
local function close()
    release()
    if hud and sf2.ui.is_open(hud) then sf2.ui.close(hud) end
    hud, mode = nil, "released"
end
local function choose(value)
    mode, frames = value, 0
    if value == "released" then release() end
end
local function button(id, label)
    return { id = id, kind = "button", text = label, height = 36 }
end
local behavior = sf2.behaviors.register {
    id = "camera",
    on_round_begin = function()
        close()
        mode, frames = "released", 0
        hud = sf2.ui.open { id = "camera", mount = "hud",
            placement = { anchor = "bottom_left", x = 24, y = -24 },
            root = { id = "root", kind = "column", width = 280, height = 194,
                style = { background_color = "#fff4dbf0" }, children = {
                    { id = "status", kind = "text", text = "Camera: released", height = 36,
                        style = { text_color = "#2b2119", font_size = 16 } },
                    button("focus", "Follow player + zoom"),
                    button("sweep", "Sweep the arena"),
                    button("native", "Native view (owned)"),
                    button("release", "Release camera"),
                } },
            on_click = function(_, widget) choose(widget == "release" and "released" or widget) end,
            on_close = function() release(); mode = "released" end,
        }
    end,
    on_tick = function(_, fighter, tick)
        if not hud or not sf2.ui.is_open(hud) then return end
        if mode == "released" then
            sf2.ui.set_text(hud, "status", "Camera: released")
            return
        end
        local view = fighter:snapshot()
        if not view then return end
        local settings = {}
        if mode == "focus" then
            settings = { center_x = view.self.position.x, offset_y = -40, zoom = 1.3 }
        elseif mode == "sweep" then
            local center = view.self.position.x
            if view.opponent then center = (center + view.opponent.position.x) / 2 end
            settings = { center_x = center + 180 * math.sin(frames * math.pi / 180),
                offset_y = -20 + 20 * math.sin(frames * math.pi / 90), zoom = 1.2 }
        end
        frames = frames + tick.delta_frames
        local failure
        if not camera or not sf2.world.is_camera_active(camera) then
            camera, failure = fighter:acquire_camera(settings)
        else
            local changed
            changed, failure = sf2.world.set_camera(camera, settings)
            if not changed and not failure then failure = "Camera update rejected" end
        end
        sf2.ui.set_text(hud, "status", failure or ("Camera: " .. mode))
    end,
    on_round_end = close, on_fight_end = close,
}
local rule = sf2.rules.behavior { id = "camera", behavior = behavior, target = sf2.rules.PLAYER }
for _, fight in ipairs { "core:fights/zone_1/tournament/3", "core:fights/zone_1/tournament_eclipsemode/3" } do
    sf2.fights.patch { target = fight, append_rules = { rule } }
end
