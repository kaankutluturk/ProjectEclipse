-- Umbra, the Last Eclipse: a three-phase boss at the Gate of Shadows.
--   Phase I   (full health)  a samurai lord with a katana.
--   Phase II  (two thirds)   time stops, he laughs, takes up a greatsword and
--                            calls two shadows to fight for him.
--   Phase III (one third)    the sun goes out. He becomes the storm hermit and
--                            lightning hunts the player across the arena.
-- The boss logic is one Lua rule attached to the opponent in this fight only.
-- Each phase is guaranteed: damage cannot carry him past the next threshold
-- until that phase's transformation has played, and he cannot be hurt during it.
local sf2 = require("sf2")
local function text(name) return sf2.mod.id .. ":localization/" .. name end
local template = sf2.warriors.get_template

-- Screen moments, fired from the rule with sf2.fx.play.
-- The opening: the fight starts in slow motion, the colour sinks and the
-- camera leans toward Umbra (it always keeps both fighters in view).
local intro = sf2.fx.screen {
    id = "intro", trigger = "script",
    saturation = 0.3, contrast = 1.3, brightness = -0.2, vignette = 0.8,
    hold = 1.4, duration = 1.4, time_scale = 0.3, zoom = 1.3, zoom_offset_y = -50,
    sound = "snd_gong", sound_volume = 0.8,
}
local shift = sf2.fx.screen {
    id = "phase_shift", trigger = "script",
    saturation = 0, contrast = 1.5, brightness = -0.25, vignette = 0.9,
    accent = "#B01010", accent_strength = 1, accent_width = 0.06,
    hold = 1, duration = 1.6, time_scale = 0.2, zoom = 1.45, zoom_offset_y = -60,
    sound = "snd_titan_laugh", sound_volume = 0.9, muffle = 0.8,
}
-- Phase III's darkness: refired before its hold runs out, so it stays down.
local eclipse = sf2.fx.screen {
    id = "eclipse", trigger = "script",
    saturation = 0.35, contrast = 1.25, brightness = -0.2, vignette = 0.85,
    tint = "#3A2E5C", tint_strength = 0.5, hold = 10, duration = 2.5,
}
local lightning = sf2.fx.screen {
    id = "lightning", trigger = "script",
    brightness = 0.55, saturation = 0.2, contrast = 1.6, tint = "#D8E4FF", tint_strength = 0.4,
    hold = 0.05, duration = 0.35, zoom = 1.06,
    sound = { "snd_hermit_lightning", "snd_hermit_lightning2" }, sound_volume = 0.9,
}

-- The three forms share a name and six health bars, two per phase; a form
-- change keeps the health that is left.
local HEALTH_BARS = 6
local function form(id, core_template)
    return sf2.warriors.register {
        id = id, template = template("core:warrior-templates/" .. core_template),
        first_name = text("boss.name"), last_name = "", avatar = "boss_shogun", level = 40, tactic = "Aggressive", health_bars = HEALTH_BARS,
    }
end
local lord = form("umbra", "shogun_katana")
local forms = { nil, form("umbra_greatsword", "man_big_sword"), form("umbra_storm", "hermit_swords") }

local minions = {}
for index, core_template in ipairs { "man_night", "girl_sai" } do
    local warrior = sf2.warriors.register {
        id = "shade_" .. index, template = template("core:warrior-templates/" .. core_template),
        level = 30, tactic = "Standard",
    }
    minions[index] = sf2.actors.register {
        -- They stay until beaten (the ten-minute cap is far beyond the round).
        id = "shade_" .. index, character = warrior, team = "owner", lifetime_frames = 36000, max_health = 0.3,
    }
end

local BANNERS = {
    { title = "UMBRA", subtitle = "THE LAST ECLIPSE" },
    { title = "PHASE II", subtitle = "THE GREATSWORD WAKES" },
    { title = "PHASE III", subtitle = "THE SUN GOES OUT" },
}
local STRIKE_EVERY, STRIKE_WARNING, STRIKE_FLASH = 150, 60, 12
local COLUMN_WIDTH, STRIKE_DAMAGE = 170, 0.06
local TRANSFORM_FRAMES = 150      -- untouchable while he changes form
local SHOCKWAVE_FRAMES, SHOCKWAVE_STEP = 24, 14  -- the change shoves the player back
-- Health fraction where each phase begins.
local THRESHOLDS = { nil, 2 / 3, 1 / 3 }

-- Lua memory for this round: HUD and marker handles are never saved.
local banner, banner_frames, marker, strike_x, strike_frames
local function close()
    if banner and sf2.ui.is_open(banner) then sf2.ui.close(banner) end
    if marker then sf2.world.remove_marker(marker) end
    banner, banner_frames, marker, strike_x, strike_frames = nil, 0, nil, nil, 0
end

local function announce(phase)
    if banner and sf2.ui.is_open(banner) then sf2.ui.close(banner) end
    local lines = BANNERS[phase]
    banner = sf2.ui.open {
        id = "banner", mount = "hud", placement = { anchor = "top", y = 150 },
        root = { id = "root", kind = "column", width = 760, height = 120, gap = 4, children = {
            { id = "title", kind = "text", width = 760, height = 70, text = lines.title,
                style = { font_size = 64, text_color = "#F2E9FF" } },
            { id = "subtitle", kind = "text", width = 760, height = 40, text = lines.subtitle,
                style = { font_size = 26, text_color = "#B9A4E8" } },
        } },
    }
    banner_frames = 180
end

local function column(x) return { x = x - COLUMN_WIDTH / 2, y = -720, width = COLUMN_WIDTH, height = 740 } end

local function storm(self, fighter, view, tick)
    self.state.storm = self.state.storm + tick.delta_frames
    if self.state.storm % 300 == 1 then sf2.fx.play(eclipse) end
    if strike_x and strike_frames > 0 then
        strike_frames = strike_frames - tick.delta_frames
        if strike_frames == STRIKE_FLASH then
            -- The bolt lands: flash, thunder, and damage if the player is still inside.
            if marker then sf2.world.set_marker_color(marker, "#EEF4FFEE") end
            sf2.fx.play(lightning, { x = strike_x })
            local inside = fighter.opponent and fighter.opponent:overlaps_rect(column(strike_x))
            if inside then fighter.opponent:change_health(-STRIKE_DAMAGE) end
        elseif strike_frames <= 0 then
            if marker then sf2.world.remove_marker(marker) end
            marker, strike_x = nil, nil
        end
    elseif self.state.storm % STRIKE_EVERY == 0 and view.opponent then
        -- A pale column marks where the next bolt will fall: right on the player.
        strike_x = view.opponent.position.x
        strike_frames = STRIKE_WARNING + STRIKE_FLASH
        local failure
        marker, failure = fighter:mark_rect(column(strike_x), "#8FB0FF40")
        if not marker then sf2.log.debug("Strike marker unavailable: " .. tostring(failure)) end
    end
end

local boss = sf2.behaviors.register {
    id = "boss",
    state = { lifetime = "round", fields = {
        frames = { type = sf2.behaviors.INTEGER, default = 0 },
        phase = { type = sf2.behaviors.INTEGER, default = 1 },
        storm = { type = sf2.behaviors.INTEGER, default = 0 },
        locked = { type = sf2.behaviors.INTEGER, default = 0 },
        shove = { type = sf2.behaviors.INTEGER, default = 0 },
    } },
    on_round_begin = function() close() end,
    -- The phase gate: no hit may take him below the next phase's threshold
    -- before that phase has begun, and nothing hurts him while he transforms.
    on_damage_resolving = function(self, fighter, event)
        if event.damage <= 0 then return end
        if self.state.locked > 0 then fighter:scale_incoming_damage(0); return end
        local floor = THRESHOLDS[self.state.phase + 1]
        local view = fighter:snapshot()
        if not floor or not view or view.self.max_health <= 0 then return end
        -- Incoming damage is measured in one bar; health is the whole pool.
        local loss = event.damage / view.self.health_bars
        local room = view.self.health - floor * view.self.max_health
        if loss > room then fighter:scale_incoming_damage(math.max(0, room / loss)) end
    end,
    on_tick = function(self, fighter, tick)
        local state = self.state
        state.frames = state.frames + tick.delta_frames
        local view = fighter:snapshot()
        if not view then return end
        if banner_frames > 0 then
            banner_frames = banner_frames - tick.delta_frames
            if banner_frames <= 0 and banner and sf2.ui.is_open(banner) then sf2.ui.close(banner); banner = nil end
        end
        if state.frames == 1 then
            sf2.fx.play(intro, { x = view.self.position.x })
            announce(1)
        end
        if state.locked > 0 then state.locked = state.locked - tick.delta_frames end
        if state.shove > 0 and fighter.opponent and view.opponent then
            state.shove = state.shove - tick.delta_frames
            local away = view.opponent.position.x >= view.self.position.x and 1 or -1
            fighter.opponent:move_by(away * SHOCKWAVE_STEP, 0)
        end
        local next_phase = state.phase + 1
        local health = view.self.max_health > 0 and view.self.health / view.self.max_health or 1
        if THRESHOLDS[next_phase] and state.locked <= 0 and health <= THRESHOLDS[next_phase] + 0.002 then
            state.phase, state.locked, state.shove = next_phase, TRANSFORM_FRAMES, SHOCKWAVE_FRAMES
            fighter:add_damage_shield("transform", 1, TRANSFORM_FRAMES)
            fighter:change_form(forms[next_phase])
            sf2.fx.play(shift, { x = view.self.position.x })
            announce(next_phase)
            local facing = view.self.animation and view.self.animation.facing or 1
            for index, definition in ipairs(minions) do
                fighter:spawn_actor(definition, (index == 1 and -220 or 260) * facing, 0)
            end
            if next_phase == 3 then sf2.fx.play(eclipse) end
        end
        if state.phase == 3 and state.locked <= 0 then storm(self, fighter, view, tick) end
    end,
    on_round_end = function() close() end,
    on_fight_end = function() close() end,
}
local rule = sf2.rules.behavior { id = "boss", behavior = boss, target = sf2.rules.OPPONENT }

local zone = sf2.zones.register { id = "eclipse", file = "Map1.1", start = false }
local battle = sf2.battles.register {
    id = "umbra", zone = zone, type = sf2.battles.STORY, x = 0, y = 0,
    alias = text("battle.title"), title = text("battle.title"), description = text("battle.description"),
    location = "shadow_gate", music = "fight37_Titan_Epic_Fight",
}
local loss = sf2.rewards.register { id = "loss", items = {} }
local win = sf2.rewards.register { id = "win", items = {}, gems = 50, experience = 500 }
local fight = sf2.fights.register {
    id = "umbra", battle = battle, warriors = { lord }, rules = { rule }, rewards = { loss, win },
    rounds = 1, round_time = 240, location = "shadow_gate", music = "fight37_Titan_Epic_Fight",
}
sf2.modes.register { id = "umbra", fights = { fight }, repeatable = true }
sf2.quests.register {
    id = "entry", place = "map", events = { "session" },
    actions = { { type = "show_battle", battle = battle, locked = false } },
}

-- The story. Before the fight: a narrator card and a conversation (the full scene
-- the first time, just Umbra's challenge after that). After a win: an epilogue
-- the next time the map opens. Story dialogs only: the game's own full-screen
-- entry card plays as the fight loads, so an act screen here would collide with it.
local key = sf2.localization.key
sf2.state.register {
    version = 1,
    fields = {
        prologue_seen = { type = sf2.state.BOOLEAN, default = false },
        epilogue_pending = { type = sf2.state.BOOLEAN, default = false },
    },
}
local faces = {
    sensei = sf2.assets.sprite("core:ui/users/character_sensei"),
    umbra = sf2.assets.sprite("core:ui/users/boss_shogun"),
    shadow = sf2.assets.sprite("core:ui/users/avatar_hero"),
}
local function say(who, lines, button, mirrored)
    local pages = {}
    for index, line in ipairs(lines) do pages[index] = { text = key(line) } end
    return { dialog = { title = key("speaker." .. who), portrait = faces[who], mirrored = mirrored or false,
        lines = pages, button = key(button or "button.next") } }
end
-- A narrator card: no speaker, no portrait.
local function narrate(lines, button)
    local pages = {}
    for index, line in ipairs(lines) do pages[index] = { text = key(line) } end
    return { dialog = { lines = pages, button = key(button or "button.next") } }
end
local PROLOGUE = {
    narrate({ "prologue.card" }),
    say("sensei", { "prologue.sensei_1", "prologue.sensei_2" }),
    say("umbra", { "prologue.umbra_1", "prologue.umbra_2" }, nil, true),
    say("shadow", { "prologue.shadow_1" }, "button.fight"),
}
local CHALLENGE = { say("umbra", { "prologue.umbra_again" }, "button.fight", true) }
local replay = sf2.settings.toggle {
    id = "full_prologue", label = "Full prologue every time", default = false,
    description = "Play Umbra's whole opening conversation on every entry, not just the first.",
}

sf2.story.before_fight(fight, function(request)
    local accepted = sf2.story.play_sequence {
        steps = (sf2.state.get("prologue_seen") and not sf2.settings.get(replay)) and CHALLENGE or PROLOGUE,
        on_step = function() return sf2.story.fight_pending(request) end,
        on_complete = function()
            sf2.state.set { prologue_seen = true }
            if not sf2.story.resume_fight(request) then sf2.story.cancel_fight(request) end
        end,
        on_cancel = function() sf2.story.cancel_fight(request) end,
    }
    if not accepted then return true end -- nothing could be shown: just fight
    return nil
end)

local FIGHT_ID = sf2.mod.id .. ":fights/umbra"
sf2.story.on("battle_result", function(event)
    if event.fight == FIGHT_ID and event.outcome == "win" then sf2.state.set { epilogue_pending = true } end
end)
sf2.story.on("scene_enter", function(event)
    if event.scene ~= "map" or not sf2.state.get("epilogue_pending") then return end
    sf2.story.play_sequence {
        steps = {
            narrate({ "epilogue.card" }),
            say("sensei", { "epilogue.sensei_1", "epilogue.sensei_2" }, "button.end"),
        },
        on_complete = function() sf2.state.set { epilogue_pending = false } end,
    }
end)
