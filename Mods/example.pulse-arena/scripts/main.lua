local sf2 = require("sf2")
-- Native arena coordinates: positive Y points down. Lua moves the warning art
-- and queries the same rectangle; sprites carry presentation, not damage logic.
local frames = {
    sf2.assets.sprite("sprites/pulse_1"), sf2.assets.sprite("sprites/pulse_2"),
    sf2.assets.sprite("sprites/pulse_3"), sf2.assets.sprite("sprites/pulse_4"),
}
local region
local marker, hud, last_phase, origin_x, last_art_frame
local function close()
    if marker then sf2.world.remove_marker(marker) end
    if hud and sf2.ui.is_open(hud) then sf2.ui.close(hud) end
    marker, hud, last_phase, region, origin_x, last_art_frame = nil, nil, nil, nil, nil, nil
end
local behavior = sf2.behaviors.register {
    id = "pulse",
    state = { lifetime = "round", fields = {
        frames = { type = "integer", default = 0 },
        contacts = { type = "integer", default = 0 },
    } },
    on_round_begin = function()
        close()
        hud = sf2.ui.open { id = "pulse", mount = "hud",
            placement = { anchor = "bottom_left", x = 24, y = -24 },
            root = { id = "root", kind = "column", width = 300, height = 80,
                style = { background_color = "#fff4dbf0" }, children = {
                    { id = "status", kind = "text", text = "Pulse Arena", height = 36,
                        style = { text_color = "#2b2119", font_size = 18 } },
                    { id = "contacts", kind = "text", text = "Contacts: 0", height = 32,
                        style = { text_color = "#2b2119", font_size = 16 } },
                } },
        }
    end,
    on_tick = function(self, fighter, tick)
        if not hud or not sf2.ui.is_open(hud) then return end
        if not region then
            local view = fighter:snapshot()
            if not view then return end
            local center = view.self.position.x
            if view.opponent then center = (center + view.opponent.position.x) / 2 end
            origin_x = center - 160
            region = { x = origin_x, y = -420, width = 320, height = 440 }
        end
        local frame = self.state.frames % 360
        self.state.frames = self.state.frames + tick.delta_frames
        -- One full left/right sweep per six-second hazard cycle.
        region.x = origin_x + 24 * math.sin(frame * math.pi / 180)
        local art_frame = math.floor(frame / 12) % #frames + 1
        local phase = frame < 120 and "warning" or frame < 240 and "active" or "safe"
        if phase ~= last_phase then
            if phase == "warning" then
                local failure
                marker, failure = fighter:mark_sprite(frames[art_frame], region, "#ffcc3366")
                if not marker then sf2.log.warn(failure or "Marker unavailable") end
                sf2.ui.set_text(hud, "status", "Warning: leave the column")
            elseif phase == "active" then
                if marker then sf2.world.set_marker_color(marker, "#ff332299") end
                sf2.ui.set_text(hud, "status", "Pulse active")
            else
                if marker then sf2.world.remove_marker(marker) end
                marker = nil
                sf2.ui.set_text(hud, "status", "Safe: pulse recovering")
            end
            last_phase = phase
        end
        if marker then
            local moved, failure = sf2.world.set_marker_rect(marker, region)
            if not moved and failure then sf2.log.warn(failure) end
            if art_frame ~= last_art_frame then
                local changed, error = sf2.world.set_marker_sprite(marker, frames[art_frame])
                if not changed and error then sf2.log.warn(error) end
                last_art_frame = art_frame
            end
        end
        if phase ~= "active" or frame % 30 ~= 0 then return end
        -- This is environmental direct health loss. It does not make an attack,
        -- invoke armor/block/critical-hit calculations, or cause a hit reaction.
        for _, body in ipairs { fighter, fighter.opponent } do
            local inside, failure = body:overlaps_rect(region)
            if inside == nil then sf2.log.warn(failure or "Sensor unavailable")
            elseif inside then
                body:change_health(-0.025)
                self.state.contacts = self.state.contacts + 1
            end
        end
        sf2.ui.set_text(hud, "contacts", "Contacts: " .. self.state.contacts)
    end,
    on_round_end = close, on_fight_end = close,
}
local rule = sf2.rules.behavior { id = "pulse", behavior = behavior, target = sf2.rules.PLAYER }
for _, fight in ipairs { "core:fights/zone_1/tournament/3", "core:fights/zone_1/tournament_eclipsemode/3" } do
    sf2.fights.patch { target = fight, append_rules = { rule } }
end
