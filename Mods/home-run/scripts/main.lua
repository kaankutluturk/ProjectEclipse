-- Home Run: a war hammer whose RaidCharge swing launches the opponent across the
-- arena. Time stalls on contact, the camera leans in, and a banner reports how
-- far they flew. Every other hammer move is the game's own two-handed set.
local sf2 = require("sf2")
local key = sf2.localization.key

local COOLDOWN_FRAMES = 60          -- frames between home runs (60 = one second)
-- The launch. The native heavy swing this copies uses x = 800, y = 850.
local LAUNCH = { x = 45000, y = 26000, z = 0 }
local SWING_DAMAGE = 0.3            -- weapon-damage multiplier of the swing
local UNITS_PER_METRE = 165         -- a standing fighter (~300 units) is about 1.8 m

local streaks = sf2.settings.toggle {
    id = "launch_streak", label = "Launch streak", default = true,
    description = "A launched fighter leaves a streak through the air.",
}

-- Batter up: a beat of slow motion as the hammer winds back.
local wind_up = sf2.fx.screen {
    id = "wind_up", trigger = "script",
    contrast = 1.15, vignette = 0.4, hold = 0.2, duration = 0.35, time_scale = 0.55,
    zoom = 1.12, zoom_offset_y = -30,
}
-- Contact: a white pop, time nearly stops, the camera leans in on the victim.
local contact = sf2.fx.screen {
    id = "contact", trigger = "script",
    brightness = 0.3, contrast = 1.35, saturation = 0.55, vignette = 0.55,
    hold = 0.45, duration = 1.3, time_scale = 0.1, zoom = 1.4, zoom_offset_y = -50,
    sound = { "snd_super_hit1", "snd_super_hit2", "snd_super_hit3" }, sound_volume = 1, muffle = 0.45,
}
-- Anyone moving faster than a normal knockback draws a streak behind the chest.
sf2.fx.trail {
    id = "launch_streak", setting = streaks, nodes = { "NStomach", "NTop" },
    color = "#FFF1D6", blend = "additive", lifetime = 0.35, min_speed = 2200, full_speed = 4200,
    alpha = 0.6, start_alpha = 0.2,
}

-- The swing: the base game's two-handed heavy slash (an unchanged copy of
-- two_hand_heavy_slash.bytes) with a full physical-fall hit and a launch impulse.
local READY = sf2.mod.id .. ":behaviors/hammer:ready"
local EDGES = {
    "WEAPON_TWO-HAND_SWORD-Edge1", "WEAPON_TWO-HAND_SWORD-Edge2", "WEAPON_TWO-HAND_SWORD-Edge3",
    "WEAPON_HEAVY_HAMMER-Edge2", "WEAPON_HEAVY_HAMMER-Edge3",
    "WEAPON_TWO-HAND_MACE-Edge2", "WEAPON_TWO-HAND_MACE-Edge3",
}
sf2.moves.register {
    id = "home_run", animation = "animations/two_hand_heavy_slash",
    type = "ATTACK", priority = 1000, mid_frames = 2, first_frame = 2,
    mirror_node = "NHeel_1", direction = "face_enemy",
    align = { axes = { "X", "Z" }, pivot = { node = "NHeel_2" }, position = { pivot = "Me" } },
    events = { "key_pressed" },
    conditions = {
        { key = "RaidCharge" }, { mod = READY },
        { not_mod = "Concussion" }, { not_mod = "Stun" }, { controllable = true },
    },
    locks = { { item = "Weapon", subtype = "TwoHandedBlunt" }, { item = "Skeleton", subtype = "Skeleton" } },
    intervals = {
        { name = "Uninterrupt", to = 34 }, { type = "Block", from = 35 }, { name = "Throwable", from = 35 },
        { type = "Attack", from = 20, to = 22, attack = {
            edges = EDGES, damage = SWING_DAMAGE, damage_terms = { WeaponDamage = 0, UnarmedDamage = -10 },
            hit = "Physycal", impulse = LAUNCH, id = 146,
            options = { ignores_block = true },
        } },
    },
    timeline = {
        [20] = { play_sound = "snd_m_pl_attack3", voice = "Male" },
        [21] = { sound = "snd_swish6" },
    },
}
local HOME_RUN = sf2.mod.id .. ":moves/home_run"

-- Lua memory for this round: the banner and the flight being measured.
local banner, banner_frames, flight
local function close()
    if banner and sf2.ui.is_open(banner) then sf2.ui.close(banner) end
    banner, banner_frames, flight = nil, 0, nil
end
local function announce(title, subtitle)
    if banner and sf2.ui.is_open(banner) then sf2.ui.close(banner) end
    banner = sf2.ui.open {
        id = "home_run", mount = "hud", placement = { anchor = "top", y = 150 },
        root = { id = "root", kind = "column", width = 700, height = 116, gap = 2, children = {
            { id = "title", kind = "text", width = 700, height = 72, text = title,
                style = { font_size = 68, text_color = "#FFE08A" } },
            { id = "distance", kind = "text", width = 700, height = 40, text = subtitle,
                style = { font_size = 28, text_color = "#FFF6E0" } },
        } },
    }
    banner_frames = 200
end

local hammer = sf2.behaviors.register {
    id = "hammer",
    state = { lifetime = "round", fields = {
        armed = { type = sf2.behaviors.BOOLEAN, default = false },
        cooldown = { type = sf2.behaviors.INTEGER, default = 0 },
    } },
    on_round_begin = function() close() end,
    on_tick = function(self, fighter, tick)
        local state = self.state
        if not state.armed then
            state.armed = true
            fighter:set_control_visible("raid_charge", true)
            fighter:set_flag("ready")
        end
        if state.cooldown > 0 then
            state.cooldown = state.cooldown - tick.delta_frames
            if state.cooldown <= 0 then state.cooldown = 0; fighter:set_flag("ready") end
        end
        if banner_frames > 0 then
            banner_frames = banner_frames - tick.delta_frames
            if banner_frames <= 0 then close() end
        end
        -- Measure the flight: furthest point from the contact until they come to rest.
        if flight then
            local view = fighter:snapshot()
            local target = view and view.opponent
            if not target then flight = nil; return end
            local x = target.position.x
            flight.best = math.max(flight.best, math.abs(x - flight.start))
            flight.age = flight.age + tick.delta_frames
            local moved = math.abs(x - flight.last)
            flight.last = x
            flight.still = moved < 2 and flight.still + 1 or 0
            if flight.age > 20 and flight.still > 12 or flight.age > 240 then
                if banner and sf2.ui.is_open(banner) then
                    sf2.ui.set_text(banner, "distance", string.format("%.1f m", flight.best / UNITS_PER_METRE))
                    banner_frames = 150
                end
                flight = nil
            elseif banner and sf2.ui.is_open(banner) then
                sf2.ui.set_text(banner, "distance", string.format("%.1f m ...", flight.best / UNITS_PER_METRE))
            end
        end
    end,
    on_animation_start = function(self, fighter, event)
        if event.target ~= "self" or event.animation_name ~= HOME_RUN then return end
        fighter:clear_flag("ready")
        fighter:set_button_cooldown("raid_charge", COOLDOWN_FRAMES)
        self.state.cooldown = COOLDOWN_FRAMES
        local view = fighter:snapshot()
        if view then sf2.fx.play(wind_up, { x = view.self.position.x }) end
    end,
    -- Our swing connected: stop time, lean in, and start measuring the flight.
    on_post_hit = function(_, fighter, event)
        local attack = event.attack
        if event.target ~= "opponent" or not attack or attack.animation_name ~= HOME_RUN then return end
        local view = fighter:snapshot()
        local target = view and view.opponent
        if not target then return end
        sf2.fx.play(contact, { x = target.position.x })
        announce("HOME RUN!", "")
        flight = { start = target.position.x, last = target.position.x, best = 0, age = 0, still = 0 }
    end,
    on_round_end = function() close() end,
    on_fight_end = function() close() end,
}

local perk = sf2.perks.register {
    id = "home_run", kind = sf2.perks.SINGLE, behavior = hammer,
    display_name = key("perk.home_run"), description = key("perk.home_run.description"),
    icon = sf2.assets.sprite("core:UI/Items/Weapon10.img_weapon_northern_hammer"),
}
local weapon = sf2.items.register_weapon {
    id = "slugger", display_name = key("weapon.slugger"),
    icon = sf2.assets.sprite("core:UI/Items/Weapon10.img_weapon_northern_hammer"),
    model = sf2.assets.model("core:gamedata/models/mdl_weapon_northern_hammer"),
    subtype = "TwoHandedBlunt",
}
sf2.items.set_innate_perks { item = weapon, entries = { { perk = perk } } }
sf2.shop.addItem { section = sf2.shop.WEAPONS, item = weapon, level = 1, price = sf2.price.coins(1) }
