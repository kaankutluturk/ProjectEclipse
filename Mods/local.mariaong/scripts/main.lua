local sf2 = require("sf2")
local authored = require("character")
local function key(name) return sf2.mod.id .. ":localization/" .. name end
local arena = sf2.locations.register {
    id = "arena", color = "0x1b2230", wall = 200, floor = 80,
    width = 1936, height = 512, min_width = 1936,
    layers = {
        { type = 1, factor = 1, images = {
            { sprite = sf2.assets.sprite("core:Textures/Locations/battlefield/battlefield_bg1.back_1"),
              x = 0, y = 0, width = 1936, height = 1024 },
        } },
        { type = 2, factor = 1, fighters = { player_x = 868, player_y = -94, enemy_x = 1068, enemy_y = -94 } },
    },
}
local zone = sf2.zones.register { id = "preview", file = "Map1.1", start = false }
local location = sf2.locations.name(arena)
local battle = sf2.battles.register {
    id = "preview", zone = zone, type = sf2.battles.STORY, x = 0, y = 0,
    alias = key("title"), title = key("title"), description = key("description"), location = location,
}
local loss = sf2.rewards.register { id = "loss", items = {} }
local win = sf2.rewards.register { id = "win", items = {} }
local fight = sf2.fights.register {
    id = "preview", battle = battle, rounds = 1, round_time = 99,
    location = location, player_character = authored.warrior, warriors = { authored.warrior }, rewards = { loss, win },
}
sf2.modes.register { id = "preview", fights = { fight }, repeatable = true }
sf2.quests.register {
    id = "entry", place = "map", events = { "session" },
    actions = { { type = "show_battle", battle = battle, locked = false } },
}
