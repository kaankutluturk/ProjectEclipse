local sf2 = require("sf2")
local actor = sf2.mod.id .. ".dart"
local dart_item = sf2.items.get("core:items/ranged/RANGED_C2_Z2_MONK_SHURIKEN")

-- Each independent fighter creates its own native four-node missile child.
-- The copied base flight binary rotates its geometry; Lua declares its motion,
-- contact attack and bounded lifetime. Lua guides the live child each tick.
local flight = sf2.moves.register {
    id = "flight", animation = "animations/shuriken_fly",
    core_templates = { "RangedMissile", "RangedMissileFly", "SoundStrike" },
    priority = 500, mid_frames = 2, first_frame = 1,
    no_interpolation_frames = true, no_wall_repulsion = true, no_magic_recharge = true,
    locks = { { item = "Weapon", subtype = "Shuriken" },
        { item = "Skeleton", subtype = "SkeletonMissile" } },
    conditions = { { actor = actor } },
    velocity = { x = 0 }, looped = true,
    direction = { from = { wall = "Back", player = "Parent" },
        to = { wall = "Front", player = "Parent" } },
    align = { axes = { "X", "Y", "Z" },
        pivot = { node = "Ranged-Node2_1", player = "Me" },
        position = { node = "Ranged-Node2_1", player = "Me" } },
    intervals = { { type = "Attack", attack = {
        edges = { "RANGED-Edge1", "RANGED-Edge2" }, damage = 0.075,
        damage_terms = { RangedDamage = 0, UnarmedDamage = -10 },
        hit = "High", impulse = { x = 450 }, id = 441,
        options = { ignores_block = true, no_critical = true,
            ignores_invulnerable = { "Evade", "Dash" } },
    } } },
    timeline = {
        [1] = { sound = "snd_shuriken_fly" },
        strike = { delete_actor = "Me" },
    },
}
local dart = sf2.projectiles.register {
    id = "dart", name = actor, core_skeleton = "SkeletonMissile",
    item = dart_item, start_move = flight, lifetime_frames = 100,
}
local receipts, known = {}, {}
local held = false
local function contact(_, fighter, event)
    local attack = event.attack
    if not attack or attack.kind ~= "projectile" then return end
    assert(attack.projectile_owner == sf2.mod.id and attack.actor_owner == sf2.mod.id)
    assert(attack.actor_id and attack.projectile_id)
    assert(known[attack.actor_id] and known[attack.actor_id][attack.projectile_id])
    sf2.log.info("RANGED-ACTOR:contact:" .. event.type .. ":" .. fighter.actor_id .. ":" .. attack.actor_id .. ":" .. attack.projectile_id)
end
local ranged = sf2.behaviors.register {
    id = "ranged",
    state = { lifetime = "round", fields = {
        id = { type = "string", default = "" },
        cooldown = { type = "integer", default = 40 },
        shots = { type = "integer", default = 0 },
        hits = { type = "integer", default = 0 },
    } },
    on_actor_spawn = function(self, fighter)
        assert(self.state.id == "" and self.state.shots == 0 and self.state.hits == 0)
        self.state.id = fighter.actor_id
        known[fighter.actor_id] = {}
        sf2.log.info("RANGED-ACTOR:born:" .. fighter.actor_id)
    end,
    on_tick = function(self, fighter)
        assert(self.state.id == fighter.actor_id)
        local list = assert(fighter:projectiles())
        local body = assert(fighter:snapshot())
        for _, child in ipairs(list) do
            local view = child:snapshot()
            if view then
                known[fighter.actor_id][view.id] = true
                if body.opponent and body.opponent.actor then
                    local direction = body.opponent.position.x >= view.position.x and 1 or -1
                    assert(child:move_by(direction * 6.5, 0))
                end
            end
        end
        local pending = receipts[fighter.actor_id]
        if pending then
            local waiting = false
            for _, request in ipairs(pending) do
                assert(request.status ~= "failed", request.error)
                if request.status == "queued" then waiting = true
                else
                    assert(request.projectile_id and known[fighter.actor_id][request.projectile_id])
                    sf2.log.info("RANGED-ACTOR:applied:" .. fighter.actor_id .. ":" .. request.projectile_id)
                end
            end
            if not waiting then receipts[fighter.actor_id] = nil end
        end
        self.state.cooldown = self.state.cooldown - 1
        if not held and self.state.cooldown <= 0 and #list < 4 and body.opponent and body.opponent.actor then
            local direction = body.opponent.position.x >= body.self.position.x and 1 or -1
            local request = fighter:spawn_projectile(dart, direction * 75, 0)
            assert(request.status == "queued")
            receipts[fighter.actor_id] = { request }
            self.state.shots = self.state.shots + 1
            self.state.cooldown = 90
            sf2.log.info("RANGED-ACTOR:queued:" .. fighter.actor_id .. ":" .. self.state.shots)
        end
    end,
    on_hit_post_crit = contact, on_post_hit = contact,
    on_damage_dealing = contact, on_damage_resolving = contact,
    on_damage_dealt = function(self, fighter, event)
        contact(self, fighter, event)
        self.state.hits = self.state.hits + 1
        assert(event.damage > 0)
        sf2.log.info("RANGED-ACTOR:dealt:" .. fighter.actor_id .. ":" .. self.state.hits .. ":" .. event.damage)
    end,
    on_damage_received = function(self, fighter, event)
        contact(self, fighter, event)
        assert(event.damage > 0)
        sf2.log.info("RANGED-ACTOR:received:" .. fighter.actor_id .. ":" .. event.damage .. ":" .. event.health_before .. ":" .. event.health_after)
    end,
    on_actor_end = function(_, fighter)
        receipts[fighter.actor_id] = nil
        sf2.log.info("RANGED-ACTOR:ended:" .. fighter.actor_id .. ":" .. fighter.actor_end_reason)
    end,
}
local character = sf2.warriors.register {
    id = "ranger", template = sf2.warriors.get_template("core:warrior-templates/default"),
    level = 1, skeleton = "Skeleton",
    items = { sf2.items.get("core:items/weapon/WEAPON_KNIVES") },
}
local ally = sf2.actors.register { id = "ally", character = character, behavior = ranged, ai = false, max_health = 10, lifetime_frames = 1800 }
local rival = sf2.actors.register { id = "rival", character = character, behavior = ranged, ai = false, team = "opponent", max_health = 10, lifetime_frames = 1800 }
local hud, command, paired
local function close()
    if hud and sf2.ui.is_open(hud) then sf2.ui.close(hud) end
    hud, command, paired, held = nil, nil, false, false
    receipts, known = {}, {}
end
local controller = sf2.behaviors.register {
    id = "controller",
    on_round_begin = function()
        close()
        hud = sf2.ui.open { id = "ranged", mount = "hud", placement = { anchor = "top_right", x = -24, y = 328 },
            root = { id = "root", kind = "column", width = 320, height = 176, children = {
                { id = "status", kind = "text", text = "Ranged pair ready", height = 40 },
                { id = "summon", kind = "button", text = "Summon ranged pair", height = 40 },
                { id = "hold", kind = "button", text = "Hold fire", height = 40 },
                { id = "dismiss", kind = "button", text = "Dismiss pair", height = 40 },
            } }, on_click = function(_, widget) command = widget end,
        }
    end,
    on_tick = function(_, fighter)
        if not hud or not sf2.ui.is_open(hud) then return end
        local actors = assert(fighter:actors())
        -- Main-root observations cannot reach this pair's children.
        assert(#assert(fighter:projectiles()) == 0)
        if command == "summon" and #actors == 0 then
            held = false
            fighter:spawn_actor(ally, 200, 0)
            fighter:spawn_actor(rival, 520, 0)
        elseif command == "dismiss" then
            for _, fighter_actor in ipairs(actors) do assert(fighter_actor:remove()) end
        elseif command == "hold" then held = not held end
        command = nil
        if #actors == 2 and not paired then
            assert(actors[1]:set_target(actors[2]))
            assert(actors[2]:set_target(actors[1]))
            paired = true
        elseif #actors < 2 then paired = false end
        sf2.ui.set_text(hud, "status", "Ranged actors: " .. #actors .. (held and " | holding" or " | firing"))
    end,
    on_round_end = close, on_fight_end = close,
}
local rule = sf2.rules.behavior { id = "controller", behavior = controller, target = sf2.rules.PLAYER }
for _, fight in ipairs { "core:fights/zone_1/tournament/3", "core:fights/zone_1/tournament_eclipsemode/3" } do
    sf2.fights.patch { target = fight, append_rules = { rule } }
end
