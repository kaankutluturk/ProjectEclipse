-- The Mirror: an opponent that fights with your own weapon and learns from you.
--   Echo  - every attack you land, it throws straight back a moment later.
--   Read  - use the same attack three times and it starts stepping out of it.
-- A rule on the Mirror watches each move you start; a Lua tactic (on_decide)
-- turns that record into its choices. What it has learned lasts the whole fight.
local sf2 = require("sf2")
local function text(name) return sf2.mod.id .. ":localization/" .. name end

local ECHO_DELAY = 0.45   -- seconds before it answers your attack
local ECHO_WINDOW = 2.5   -- after this it lets the chance go
local READ_AFTER = 3      -- uses of one attack before it reads it
local DISPLAY_FRAMES = 90

-- One representative core weapon for each weapon type, so the Mirror carries the
-- same move set as you. Your weapon's type is read when you enter the fight.
local WEAPONS = {
    Axes = "WEAPON_SUPER_AXES", Batons = "WEAPON_STEEL_BATONS", BattleHammers = "WEAPON_SUPER_HAMMERS",
    BigSwords = "WEAPON_BIG_SWORDS", ButcherKnives = "WEAPON_BUTCHER_KNIVES_PAID", ChineseSabers = "WEAPON_CHINESE_SABERS",
    Claws = "WEAPON_MERCENARY_CLAWS", CompositeScythe = "WEAPON_COMPOSITE_SCYTHE", CompositeSpear = "WEAPON_COMPOSITE_SPEAR",
    CompositeStaff = "WEAPON_COMPOSITE_STAFF", CompositeSword = "WEAPON_SUPER_COMPOSITE_SWORD", CrescentKnives = "WEAPON_CRESCENT_KNIVES",
    Cudgel = "WEAPON_TWO_HANDED_CUDGEL", Daggers = "WEAPON_TEC_KNIVES", DoubleScythe = "WEAPON_SECTIONAL_SCYTHE",
    ElectroHammers = "WEAPON_ELECTRO_HAMMERS", Fans = "WEAPON_SUPER_FANS", FireBatons = "WEAPON_FIRE_BATONS",
    Glaive = "WEAPON_GLAIVE", Glaivebow = "WEAPON_GLAIVEBOW", HermitSwords = "WEAPON_CHNY21_JIAN",
    HunterClaws = "WEAPON_HUNTER_CLAWS", Katana = "WEAPON_GOLDEN_KATANA", Katars = "WEAPON_INDIAN_KATAR",
    Keris = "WEAPON_KERIS", Knives = "WEAPON_KNIVES", Knobsticks = "WEAPON_KNOBSTICKS", Knuckles = "WEAPON_KNUCKLES",
    Kusarigama = "WEAPON_SUPER_KUSARIGAMA", Machete = "WEAPON_MACHETE", MagariYari = "WEAPON_MAGARI_YARI",
    MonkKatars = "WEAPON_C2_Z2_MONK_KATAR", Musket = "WEAPON_NY2024_MUSKET", Naginata = "WEAPON_BG_NAGINATA",
    NinjaSword = "WEAPON_NINJA_SWORD", Nunchaku = "WEAPON_STEEL_NUNCHAKU", OneHandedSawblade = "WEAPON_SAWBLADE",
    OneHandedSword = "WEAPON_SWORD_NY23_PAID_OFFER", PowerFists = "WEAPON_POWER_FISTS", Rifle = "WEAPON_RIFLE",
    Sai = "WEAPON_SAI", Scythe = "WEAPON_HW14_SCYTHE", ShockerClaws = "WEAPON_SHOCKER_CLAWS",
    ShogunKatana = "WEAPON_SUPER_DAISHO", ShuangGou = "WEAPON_SHUANG_GOU", Sickles = "WEAPON_SICKLES",
    SilverGlaive = "WEAPON_SUPER_GLAIVE", Spear = "WEAPON_SUPER_SPEAR", Staff = "WEAPON_STAFF",
    SteelClaws = "WEAPON_STEEL_CLAWS", Swords = "WEAPON_SWORDS", Tonfa = "WEAPON_TONFA", TonfaGuns = "WEAPON_TONFA_GUNS",
    Trident = "WEAPON_TRIDENT", TwoHanded = "WEAPON_SUPER_BIG_SWORD", TwoHandedBlunt = "WEAPON_TWO_HANDED_MACE",
    Wakidzashi = "WEAPON_WAKIDZASHI", WandererStaff = "WEAPON_C4_Z1_WARLOCK_STAFF",
}

-- What the Mirror knows, shared by its rule and its tactic for this fight.
local seen, recent, learned = {}, {}, 0
local note -- { kind = "echo" | "read", name = ..., shown = false }
local function reset() seen, recent, learned, note = {}, {}, 0, nil end

local function pretty(name)
    name = name:match("[^/]+$") or name
    name = name:gsub("_", " "):gsub("(%l)(%u)", "%1 %2")
    return name:upper()
end

local function has_input(action, control)
    for _, input in ipairs(action.inputs or {}) do
        if input.control == control then return true end
    end
    return false
end

local function attacking(animation)
    for _, interval in ipairs(animation.intervals or {}) do
        if interval.type == "attack" then return true end
    end
    return false
end

local tactic = sf2.tactics.register {
    id = "mirror", template = "Standard",
    on_decide = function(_, event)
        local now = event.seconds
        -- Read: your favourite attack is coming, so step out of it.
        local theirs = event.opponent and event.opponent.animation
        if theirs and (seen[theirs.name] or 0) >= READ_AFTER and attacking(theirs) then
            for _, action in ipairs(event.actions) do
                if action.type ~= "attack" and has_input(action, "Back") then
                    note = { kind = "read", name = theirs.name }
                    return action
                end
            end
        end
        -- Echo: answer your most recent attack with the very same move.
        for index = #recent, 1, -1 do
            local entry = recent[index]
            local age = now - entry.at
            if age > ECHO_WINDOW then
                table.remove(recent, index)
            elseif age >= ECHO_DELAY then
                for _, action in ipairs(event.actions) do
                    if action.type == "attack" and action.name == entry.name then
                        table.remove(recent, index)
                        note = { kind = "echo", name = entry.name }
                        return action
                    end
                end
            end
        end
        return nil -- the native tactic fills the gaps
    end,
}

local mirrors, unarmed = {}, nil
local function mirror(id, items)
    return sf2.warriors.register {
        id = id, template = sf2.warriors.get_template("core:warrior-templates/default"),
        first_name = text("mirror.name"), last_name = "", level = 1,
        tactic = sf2.tactics.name(tactic), items = items,
    }
end
unarmed = mirror("mirror", {})
for subtype, item in pairs(WEAPONS) do
    mirrors[subtype] = mirror("mirror_" .. subtype:lower(), { sf2.items.get("core:items/weapon/" .. item) })
end

-- Screen moments: a cold blink when it echoes you, a stall when it reads you.
local echo = sf2.fx.screen {
    id = "echo", trigger = "script",
    tint = "#9FE8FF", tint_strength = 0.35, saturation = 0.7, contrast = 1.1, hold = 0.05, duration = 0.45,
}
local read = sf2.fx.screen {
    id = "read", trigger = "script",
    saturation = 0.3, contrast = 1.3, vignette = 0.5, tint = "#9FE8FF", tint_strength = 0.25,
    hold = 0.25, duration = 0.6, time_scale = 0.35, zoom = 1.12, zoom_offset_y = -30,
    sound = "snd_time_shift", sound_volume = 0.5,
}
local awaken = sf2.fx.screen {
    id = "awaken", trigger = "script",
    saturation = 0.2, contrast = 1.3, brightness = -0.15, vignette = 0.7, tint = "#9FE8FF", tint_strength = 0.3,
    hold = 1, duration = 1.2, time_scale = 0.35, zoom = 1.25, zoom_offset_y = -40,
    sound = "snd_gong", sound_volume = 0.7,
}

local hud, flash_frames, round_number
local function close()
    if hud and sf2.ui.is_open(hud) then sf2.ui.close(hud) end
    hud, flash_frames = nil, 0
end
local function learned_text()
    if learned == 0 then return "IT IS WATCHING YOU" end
    return "IT HAS LEARNED " .. learned .. (learned == 1 and " MOVE" or " MOVES")
end

-- Is this animation an attack? Prefer the live observation; fall back to its name.
local NOT_ATTACKS = { "Stance", "Step", "Walk", "Idle", "Jump", "Roll", "Block", "Hit", "Fall", "Recoil",
    "Stand", "Sit", "Turn", "Run", "Dash", "Evade", "Win", "Lose", "Death", "Wall", "Physical", "Start" }
local function is_attack(fighter, name)
    local view = fighter:snapshot()
    local animation = view and view.opponent and view.opponent.animation
    if animation and animation.name == name then return animation.type == "attack" end
    for _, word in ipairs(NOT_ATTACKS) do if name:find(word, 1, true) then return false end end
    return true
end

local watcher = sf2.behaviors.register {
    id = "watcher",
    state = { lifetime = "round", fields = { frames = { type = sf2.behaviors.INTEGER, default = 0 } } },
    on_fight_begin = function() reset() end,
    on_round_begin = function(_, _, event)
        close()
        round_number = event.round
        hud = sf2.ui.open {
            id = "mirror", mount = "hud", placement = { anchor = "top", y = 150 },
            root = { id = "root", kind = "column", width = 700, height = 84, gap = 2, children = {
                { id = "learned", kind = "text", width = 700, height = 40, text = learned_text(),
                    style = { font_size = 30, text_color = "#CFF6FF" } },
                { id = "flash", kind = "text", width = 700, height = 40, text = "",
                    style = { font_size = 26, text_color = "#FFFFFF" } },
            } },
        }
    end,
    on_animation_start = function(_, fighter, event)
        if event.target ~= "opponent" or not is_attack(fighter, event.animation_name) then return end
        local name = event.animation_name
        seen[name] = (seen[name] or 0) + 1
        if seen[name] == READ_AFTER then learned = learned + 1 end
        recent[#recent + 1] = { name = name, at = event.frame / 60 }
        if #recent > 6 then table.remove(recent, 1) end
    end,
    on_tick = function(self, fighter, tick)
        self.state.frames = self.state.frames + tick.delta_frames
        local view = fighter:snapshot()
        -- It wakes once, at the start of the fight.
        if self.state.frames == 1 and round_number == 1 and view then
            sf2.fx.play(awaken, { x = view.self.position.x })
        end
        if not hud or not sf2.ui.is_open(hud) then return end
        if note and not note.shown then
            note.shown = true
            local label = note.kind == "echo" and "ECHO: " or "READ YOU: "
            sf2.ui.set_text(hud, "flash", label .. pretty(note.name))
            if view then sf2.fx.play(note.kind == "echo" and echo or read, { x = view.self.position.x }) end
            flash_frames = DISPLAY_FRAMES
        elseif flash_frames > 0 then
            flash_frames = flash_frames - tick.delta_frames
            if flash_frames <= 0 then sf2.ui.set_text(hud, "flash", "") end
        end
        sf2.ui.set_text(hud, "learned", learned_text())
    end,
    on_round_end = function() close() end,
    on_fight_end = function() close() end,
}
local rule = sf2.rules.behavior { id = "watcher", behavior = watcher, target = sf2.rules.OPPONENT }

local zone = sf2.zones.register { id = "mirror", file = "Map1.1", start = false }
local battle = sf2.battles.register {
    id = "mirror", zone = zone, type = sf2.battles.STORY, x = 0, y = 0,
    alias = text("battle.title"), title = text("battle.title"), description = text("battle.description"),
    location = "lamps_on_water", music = "fight5_ninja_in_the_night",
}
local loss = sf2.rewards.register { id = "loss", items = {} }
local win = sf2.rewards.register { id = "win", items = {}, gems = 30, experience = 400 }
local fight = sf2.fights.register {
    id = "mirror", battle = battle, warriors = { unarmed }, rules = { rule }, rewards = { loss, win },
    rounds = 3, round_time = 99, location = "lamps_on_water", music = "fight5_ninja_in_the_night",
}
-- On entry: hand the Mirror your weapon type and your level.
sf2.modes.register {
    id = "mirror", fights = { fight }, repeatable = true,
    on_prepare = function()
        local chosen = unarmed
        for _, item in ipairs(sf2.profile.equipment()) do
            if item.owned and item.type == "Weapon" and item.subtype and mirrors[item.subtype] then
                chosen = mirrors[item.subtype]
            end
        end
        return { warriors = { chosen }, level = math.max(1, sf2.profile.level()) }
    end,
}
sf2.quests.register {
    id = "entry", place = "map", events = { "session" },
    actions = { { type = "show_battle", battle = battle, locked = false } },
}
