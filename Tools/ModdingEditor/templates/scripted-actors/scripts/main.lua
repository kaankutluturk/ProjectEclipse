local sf2 = require("sf2")
local function input(action, control)
    for _, key in ipairs(action.inputs or {}) do
        if key.control == control then return true end
    end
    return false
end
local brain = sf2.tactics.register {
    id = "sparring", template = "Standard",
    on_decide = function(memory, event)
        local self = event.self and event.self.actor
        local target = event.opponent and event.opponent.actor
        if not self or not target or self.team == target.team then return "wait" end
        -- Memory belongs to this native controller. Script-level variables are shared.
        memory.id = memory.id or self.id
        assert(memory.id == self.id, "Actor AI memory crossed between controllers")
        memory.decisions = (memory.decisions or 0) + 1
        if memory.decisions == 1 then
            sf2.log.info("SCRIPTED-ACTOR:born:" .. self.id .. ":" .. target.id .. ":" .. self.team)
        end
        if event.frame < (memory.ready or 0) then return "wait" end
        local distance = math.abs(event.self.position.x - event.opponent.position.x)
        local facing = event.self.animation and event.self.animation.facing or 1
        local toward = (event.opponent.position.x - event.self.position.x) * facing >= 0 and "Forward" or "Back"
        local best, duration
        for _, action in ipairs(event.actions) do
            if distance > 95 then
                -- Forward/back are facing-relative. Crossing the target must not
                -- turn approach into running away; prefer the shortest eligible move.
                if action.type == "move" and input(action, toward) and not input(action, "Up") then
                    local frames = action.timing and action.timing.nominal_frames or 100000
                    if not duration or frames < duration then best, duration = action, frames end
                end
            elseif action.type == "attack" and action.timing and not action.timing.looped
                and (input(action, "Punch") or input(action, "Kick")) then
                if not duration or action.timing.nominal_frames < duration then best, duration = action, action.timing.nominal_frames end
            end
        end
        if best then
            if best.type == "attack" then memory.ready = event.frame + 70 end
            sf2.log.info("SCRIPTED-ACTOR:choose:" .. self.id .. ":" .. target.id .. ":" .. best.type .. ":" .. best.name)
            return best -- Return this decision's original candidate, never a copied table.
        end
        return "wait"
    end,
}
local character = sf2.warriors.register {
    id = "sparring", template = sf2.warriors.get_template("core:warrior-templates/default"),
    level = 1, tactic = sf2.tactics.name(brain), skeleton = "Skeleton",
    items = { sf2.items.get("core:items/weapon/WEAPON_KNIVES") },
}
local reactive = sf2.behaviors.register {
    id = "reactive_sparring",
    parameters = { bonus = { type = "number", default = 0.01 } },
    state = { lifetime = "round", fields = {
        ticks = { type = "integer", default = 0 },
        hits = { type = "integer", default = 0 },
        id = { type = "string", default = "" },
    } },
    on_actor_spawn = function(self, fighter)
        assert(self.state.id == "" and self.state.ticks == 0 and self.state.hits == 0)
        self.state.id = fighter.actor_id
        local own = assert(fighter.actor:snapshot())
        assert(own.id == fighter.actor_id and own.health == own.max_health)
        sf2.log.info("SCRIPTED-ACTOR:host_spawn:" .. fighter.actor_id)
    end,
    on_tick = function(self, fighter)
        assert(self.state.id == fighter.actor_id, "Actor behavior state crossed between instances")
        self.state.ticks = self.state.ticks + 1
        local view = assert(fighter:snapshot())
        assert(view.self.actor.id == fighter.actor_id)
        if self.state.ticks == 1 then sf2.log.info("SCRIPTED-ACTOR:host_tick:" .. fighter.actor_id) end
    end,
    on_damage_dealing = function(self, fighter, event)
        assert(event.attack.actor_id == fighter.actor_id)
        fighter:add_outgoing_damage(self.params.bonus)
        sf2.log.info("SCRIPTED-ACTOR:host_outgoing:" .. fighter.actor_id .. ":" .. event.damage .. ":" .. self.params.bonus)
    end,
    on_damage_resolving = function(_, fighter, event)
        fighter:scale_incoming_damage(0.5)
        sf2.log.info("SCRIPTED-ACTOR:host_resolving:" .. fighter.actor_id .. ":" .. event.attack.actor_id .. ":" .. event.damage)
    end,
    on_damage_received = function(self, fighter, event)
        self.state.hits = self.state.hits + 1
        assert(event.damage > 0 and event.attack.actor_id ~= fighter.actor_id)
        sf2.log.info("SCRIPTED-ACTOR:host_received:" .. fighter.actor_id .. ":" .. event.damage .. ":" .. event.health_before .. ":" .. event.health_after)
    end,
    on_damage_dealt = function(_, fighter, event)
        assert(event.damage > 0 and event.attack.actor_id == fighter.actor_id)
        sf2.log.info("SCRIPTED-ACTOR:host_dealt:" .. fighter.actor_id)
    end,
    on_hit_post_crit = function(_, fighter, event)
        sf2.log.info("SCRIPTED-ACTOR:host_contact:" .. fighter.actor_id .. ":" .. event.type .. ":" .. event.target)
    end,
    on_post_hit = function(_, fighter, event)
        sf2.log.info("SCRIPTED-ACTOR:host_contact:" .. fighter.actor_id .. ":" .. event.type .. ":" .. event.target)
    end,
    on_animation_start = function(_, fighter, event)
        if event.target == "self" then sf2.log.info("SCRIPTED-ACTOR:host_animation:" .. fighter.actor_id) end
    end,
    on_actor_end = function(self, fighter)
        assert(self.state.id == fighter.actor_id)
        sf2.log.info("SCRIPTED-ACTOR:host_end:" .. fighter.actor_id .. ":" .. fighter.actor_end_reason)
    end,
}
local ally = sf2.actors.register { id = "ally", character = character, behavior = reactive, max_health = 10, lifetime_frames = 1800 }
local rival = sf2.actors.register { id = "rival", character = character, behavior = reactive, parameters = { bonus = 0.02 }, team = "opponent", max_health = 10, lifetime_frames = 1800 }
local hud, command, pending, last_event, paired
local function close()
    if hud and sf2.ui.is_open(hud) then sf2.ui.close(hud) end
    hud, command, pending, last_event, paired = nil, nil, nil, 0, false
end
local controller = sf2.behaviors.register {
    id = "sparring",
    on_round_begin = function()
        close()
        hud = sf2.ui.open { id = "sparring", mount = "hud", placement = { anchor = "top_right", x = -24, y = 328 },
            root = { id = "root", kind = "column", width = 320, height = 176, children = {
                { id = "status", kind = "text", text = "Scripted actors ready", height = 40 },
                { id = "summon", kind = "button", text = "Summon scripted sparring pair", height = 40 },
                { id = "dismiss", kind = "button", text = "Dismiss sparring pair", height = 40 },
            } }, on_click = function(_, widget) command = widget end,
        }
    end,
    on_tick = function(_, fighter)
        if not hud or not sf2.ui.is_open(hud) then return end
        local actors = fighter:actors()
        if not actors then return end
        if command == "summon" and #actors == 0 then
            pending = { fighter:spawn_actor(ally, 180, 0), fighter:spawn_actor(rival, 260, 0) }
        elseif command == "dismiss" then
            for _, actor in ipairs(actors) do actor:remove() end
        end
        command = nil
        -- Keep the sparring partner even when a main fighter moves closer.
        -- Targets switch when the native animation permits it.
        if #actors == 2 and not paired then
            assert(actors[1]:set_target(actors[2]))
            assert(actors[2]:set_target(actors[1]))
            paired = true
        elseif #actors < 2 then paired = false end
        if pending then
            local waiting = false
            for _, receipt in ipairs(pending) do
                if receipt.status == "failed" then sf2.log.warn(receipt.error)
                elseif receipt.status == "queued" then waiting = true end
            end
            if not waiting then pending = nil end
        end
        local text = "Scripted actors: " .. #actors
        for _, actor in ipairs(actors) do
            local view = actor:snapshot()
            if view then text = text .. " | " .. view.id .. " " .. string.format("%.2f", view.health) end
        end
        sf2.ui.set_text(hud, "status", text)
        for _, event in ipairs(fighter:actor_events() or {}) do
            if event.sequence > last_event then
                sf2.log.info("SCRIPTED-ACTOR:event:" .. event.kind .. ":" .. event.actor_id)
                last_event = event.sequence
            end
        end
    end,
    on_round_end = close, on_fight_end = close,
}
local rule = sf2.rules.behavior { id = "sparring", behavior = controller, target = sf2.rules.PLAYER }
for _, fight in ipairs { "core:fights/zone_1/tournament/3", "core:fights/zone_1/tournament_eclipsemode/3" } do
    sf2.fights.patch { target = fight, append_rules = { rule } }
end
