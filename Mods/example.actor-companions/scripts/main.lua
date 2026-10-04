local sf2 = require("sf2")
local character = sf2.warriors.register {
    id = "companion", template = sf2.warriors.get_template("core:warrior-templates/default"),
    level = 1, tactic = "Standard", skeleton = "Skeleton",
    items = { sf2.items.get("core:items/weapon/WEAPON_KNIVES") },
}
local ally = sf2.actors.register { id = "ally", character = character, lifetime_frames = 900 }
local challenger = sf2.actors.register { id = "challenger", character = character, team = "opponent", lifetime_frames = 900 }
local brief = sf2.actors.register { id = "brief", character = character, ai = false, lifetime_frames = 60 }
local strike = sf2.moves.register {
    id = "companion_strike", animation = "animations/high_punch", type = "ATTACK", priority = 110,
    mid_frames = 2, first_frame = 1, mirror_node = "NHeel_1", direction = "face_enemy",
    locks = { { item = "Skeleton", subtype = "Skeleton" } },
    intervals = { { name = "Uninterrupt", to = 9 }, { type = "Block", from = 10 },
        { name = "Throwable", from = 10 }, { type = "Attack", from = 4, to = 5, attack = {
            edges = { "EForearm_2", "EHand_2", "EFingers_2" }, damage = 0.11,
            damage_terms = { UnarmedDamage = -10 }, impulse = { x = 245 }, hit = "High", id = 21,
        } } },
}
local hud, command, pending, last_event
local function close()
    if hud and sf2.ui.is_open(hud) then sf2.ui.close(hud) end
    hud, command, pending, last_event = nil, nil, nil, 0
end
local behavior = sf2.behaviors.register {
    id = "companions",
    on_round_begin = function()
        close()
        hud = sf2.ui.open { id = "companions", mount = "hud", placement = { anchor = "top_right", x = -24, y = 328 },
            root = { id = "root", kind = "column", width = 300, height = 352, children = {
                { id = "status", kind = "text", text = "Actors ready", height = 32 },
                { id = "summon", kind = "button", text = "Summon sparring pair", height = 40 },
                { id = "guide", kind = "button", text = "Move owned actors right", height = 40 },
                { id = "dismiss", kind = "button", text = "Dismiss owned actors", height = 40 },
                { id = "brief", kind = "button", text = "Summon for one second", height = 40 },
                { id = "damage", kind = "button", text = "Damage first actor", height = 40 },
                { id = "strike", kind = "button", text = "First actor: punch", height = 40 },
                { id = "defeat", kind = "button", text = "Defeat first actor", height = 40 },
            } },
            on_click = function(_, widget) command = widget end,
        }
    end,
    on_tick = function(_, fighter)
        if not hud or not sf2.ui.is_open(hud) then return end
        local actors = fighter:actors()
        if not actors then return end
        if command == "summon" then
            pending = { fighter:spawn_actor(ally, 180, 0), fighter:spawn_actor(challenger, 330, 0) }
        elseif command == "brief" then
            pending = { fighter:spawn_actor(brief, 180, 0) }
        elseif command == "dismiss" then
            for _, actor in ipairs(actors) do actor:remove() end
        elseif command == "guide" then
            for _, actor in ipairs(actors) do
                local ok, error = actor:move_by(10, 0)
                if not ok then sf2.log.warn(error) end
            end
            sf2.log.info("ACTOR-COMMAND:move")
        elseif command == "damage" and actors[1] then
            assert(actors[1]:change_health(-0.25))
            sf2.log.info("ACTOR-COMMAND:health")
        elseif command == "strike" and actors[1] then
            if actors[2] then assert(actors[1]:set_target(actors[2])) end
            pending = { actors[1]:play_move(strike) }
        elseif command == "defeat" and actors[1] then
            local view = actors[1]:snapshot()
            if view then assert(actors[1]:change_health(-view.max_health)) end
        end
        command = nil
        if pending then
            local waiting = false
            for _, receipt in ipairs(pending) do
                if receipt.status == "failed" then sf2.log.warn(receipt.error)
                elseif receipt.status == "queued" then waiting = true end
                if receipt.status == "applied" then sf2.log.info("ACTOR-COMMAND:applied") end
            end
            if not waiting then pending = nil end
        end
        local text = "Actors: " .. #actors
        for _, actor in ipairs(actors) do
            local view = actor:snapshot()
            if view then text = text .. " | " .. view.id .. " " .. string.format("%.2f", view.health) end
        end
        sf2.ui.set_text(hud, "status", text)
        for _, event in ipairs(fighter:actor_events() or {}) do
            if event.sequence > last_event then
                sf2.log.info("ACTOR-EVENT:" .. event.kind .. ":" .. event.actor_id)
                last_event = event.sequence
            end
        end
    end,
    on_round_end = close, on_fight_end = close,
}
local rule = sf2.rules.behavior { id = "companions", behavior = behavior, target = sf2.rules.PLAYER }
for _, fight in ipairs { "core:fights/zone_1/tournament/3", "core:fights/zone_1/tournament_eclipsemode/3" } do
    sf2.fights.patch { target = fight, append_rules = { rule } }
end
