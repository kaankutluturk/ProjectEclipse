-- The Gauntlet: five fighters, one fight, no break. Each health bar is a
-- different opponent. Empty one and time freezes, the next fighter leaps in,
-- you catch your breath, and the next wave begins.
local sf2 = require("sf2")
local function text(name) return sf2.mod.id .. ":localization/" .. name end

local WAVES = {
    { template = "girl_sai", name = "wave.1" },
    { template = "girl_kusarigama", name = "wave.2" },
    { template = "man_big_hammer", name = "wave.3" },
    { template = "lynx_claws", name = "wave.4", avatar = "boss_lynx" },
    { template = "butcher_backswords", name = "wave.5", avatar = "boss_butcher" },
}
local TITLES = { "SAI DANCER", "CHAIN WIDOW", "THE HAMMER", "LYNX", "THE BUTCHER" }
local SWAP_FRAMES = 90        -- untouchable while the next fighter enters
local LEAP_FRAMES, LEAP_STEP = 16, 18  -- the newcomer lands this far back
local BREATHER = 0.12         -- health returned to the player between waves

-- Every wave shares the five-bar pool, so the bar you see empties wave by wave.
local fighters = {}
for index, wave in ipairs(WAVES) do
    fighters[index] = sf2.warriors.register {
        id = "wave_" .. index, template = sf2.warriors.get_template("core:warrior-templates/" .. wave.template),
        first_name = text(wave.name), last_name = "", avatar = wave.avatar or "",
        level = 30 + index * 4, tactic = "Aggressive", health_bars = #WAVES,
    }
end

local opening = sf2.fx.screen {
    id = "opening", trigger = "script",
    saturation = 0.4, contrast = 1.3, vignette = 0.7, hold = 1, duration = 1.2, time_scale = 0.35,
    zoom = 1.2, zoom_offset_y = -40, sound = "snd_gong", sound_volume = 0.7,
}
-- The fall of a wave: a hard white frame, near-frozen time, then the newcomer.
local fallen = sf2.fx.screen {
    id = "fallen", trigger = "script",
    brightness = 0.35, saturation = 0.2, contrast = 1.5, vignette = 0.6,
    accent = "#B01010", accent_strength = 1, accent_width = 0.05,
    hold = 0.5, duration = 1, time_scale = 0.12, zoom = 1.35, zoom_offset_y = -50,
    sound = { "snd_boss_hit_1", "snd_boss_hit_2", "snd_boss_hit_3" }, sound_volume = 0.9, muffle = 0.6,
}

local banner, banner_frames
local function close()
    if banner and sf2.ui.is_open(banner) then sf2.ui.close(banner) end
    banner, banner_frames = nil, 0
end
local function announce(title, subtitle)
    if banner and sf2.ui.is_open(banner) then sf2.ui.close(banner) end
    banner = sf2.ui.open {
        id = "wave", mount = "hud", placement = { anchor = "top", y = 150 },
        root = { id = "root", kind = "column", width = 760, height = 116, gap = 2, children = {
            { id = "title", kind = "text", width = 760, height = 70, text = title,
                style = { font_size = 60, text_color = "#FFF2D6" } },
            { id = "subtitle", kind = "text", width = 760, height = 40, text = subtitle,
                style = { font_size = 28, text_color = "#E8B860" } },
        } },
    }
    banner_frames = 150
end
local function wave_banner(index)
    announce("WAVE " .. index .. " / " .. #WAVES, TITLES[index])
end

-- Health fraction where wave n begins: 1, 0.8, 0.6, 0.4, 0.2.
local function threshold(wave) return (#WAVES - wave + 1) / #WAVES end

local gauntlet = sf2.behaviors.register {
    id = "gauntlet",
    state = { lifetime = "round", fields = {
        frames = { type = sf2.behaviors.INTEGER, default = 0 },
        wave = { type = sf2.behaviors.INTEGER, default = 1 },
        locked = { type = sf2.behaviors.INTEGER, default = 0 },
        leap = { type = sf2.behaviors.INTEGER, default = 0 },
    } },
    on_round_begin = function() close() end,
    -- No hit can carry the fight past a wave before the next fighter has entered.
    on_damage_resolving = function(self, fighter, event)
        if event.damage <= 0 then return end
        if self.state.locked > 0 then fighter:scale_incoming_damage(0); return end
        if self.state.wave >= #WAVES then return end
        local view = fighter:snapshot()
        if not view or view.self.max_health <= 0 then return end
        local loss = event.damage / view.self.health_bars
        local room = view.self.health - threshold(self.state.wave + 1) * view.self.max_health
        if loss > room then fighter:scale_incoming_damage(math.max(0, room / loss)) end
    end,
    on_tick = function(self, fighter, tick)
        local state = self.state
        state.frames = state.frames + tick.delta_frames
        local view = fighter:snapshot()
        if not view then return end
        if state.frames == 1 then
            sf2.fx.play(opening, { x = view.self.position.x })
            announce("THE GAUNTLET", "FIVE FIGHTERS. ONE LIFE.")
        elseif state.frames == 150 then
            wave_banner(1)
        end
        if banner_frames > 0 then
            banner_frames = banner_frames - tick.delta_frames
            if banner_frames <= 0 then close() end
        end
        if state.locked > 0 then state.locked = state.locked - tick.delta_frames end
        -- The newcomer lands back at fighting distance.
        if state.leap > 0 and view.opponent then
            state.leap = state.leap - tick.delta_frames
            local away = view.self.position.x >= view.opponent.position.x and 1 or -1
            fighter:move_by(away * LEAP_STEP, 0)
        end
        local next_wave = state.wave + 1
        local health = view.self.max_health > 0 and view.self.health / view.self.max_health or 1
        if fighters[next_wave] and state.locked <= 0 and health <= threshold(next_wave) + 0.002 then
            state.wave, state.locked, state.leap = next_wave, SWAP_FRAMES, LEAP_FRAMES
            fighter:add_damage_shield("swap", 1, SWAP_FRAMES)
            sf2.fx.play(fallen, { x = view.self.position.x })
            fighter:change_form(fighters[next_wave])
            if fighter.opponent then fighter.opponent:change_health(BREATHER) end
            wave_banner(next_wave)
        end
    end,
    on_round_end = function() close() end,
    on_fight_end = function() close() end,
}
local rule = sf2.rules.behavior { id = "gauntlet", behavior = gauntlet, target = sf2.rules.OPPONENT }

local zone = sf2.zones.register { id = "gauntlet", file = "Map1.1", start = false }
local battle = sf2.battles.register {
    id = "gauntlet", zone = zone, type = sf2.battles.STORY, x = 0, y = 0,
    alias = text("battle.title"), title = text("battle.title"), description = text("battle.description"),
    location = "arena", music = "fight2_blade_dance",
}
local loss = sf2.rewards.register { id = "loss", items = {} }
local win = sf2.rewards.register { id = "win", items = {}, gems = 40, experience = 600 }
local fight = sf2.fights.register {
    id = "gauntlet", battle = battle, warriors = { fighters[1] }, rules = { rule }, rewards = { loss, win },
    rounds = 1, round_time = 300, location = "arena", music = "fight2_blade_dance",
}
sf2.modes.register { id = "gauntlet", fights = { fight }, repeatable = true }
sf2.quests.register {
    id = "entry", place = "map", events = { "session" },
    actions = { { type = "show_battle", battle = battle, locked = false } },
}
