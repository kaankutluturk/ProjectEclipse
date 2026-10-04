local sf2 = require("sf2")
local actor = sf2.mod.id .. ".dart"
local dart_item = sf2.items.get("core:items/ranged/RANGED_C2_Z2_MONK_SHURIKEN")

-- A typed native missile rig, equipment model and flight move.
-- The original binary rotates the geometry; ordinary Lua supplies its path.
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
local burst = sf2.projectiles.register {
    id = "dart", name = actor, core_skeleton = "SkeletonMissile",
    item = dart_item, start_move = flight, lifetime_frames = 120,
}
local hud, requested, pending
local paths = {}
local function close()
    if hud and sf2.ui.is_open(hud) then sf2.ui.close(hud) end
    hud, requested, pending = nil, false, nil
    paths = {}
end
local behavior = sf2.behaviors.register {
    id = "burst",
    state = { lifetime = "round", fields = {
        cooldown = { type = "integer", default = 0 },
        hits = { type = "integer", default = 0 },
    } },
    on_round_begin = function()
        close()
        hud = sf2.ui.open { id = "burst", mount = "hud",
            placement = { anchor = "top_right", x = -24, y = 328 },
            root = { id = "root", kind = "column", width = 300, height = 84,
                style = { background_color = "#fff4dbf0" }, children = {
                    { id = "status", kind = "text", text = "Burst ready", height = 32,
                        style = { text_color = "#2b2119", font_size = 18 } },
                    { id = "fire", kind = "button", text = "Spawn three darts", height = 48 },
                } },
            on_click = function(_, widget) if widget == "fire" then requested = true end end,
        }
    end,
    on_tick = function(self, fighter, tick)
        if not hud or not sf2.ui.is_open(hud) then requested = false; return end
        local snapshot = fighter:snapshot()
        if not snapshot or not snapshot.opponent then return end
        if pending then
            local waiting = false
            for _, request in ipairs(pending.requests) do
                if request.status == "queued" then waiting = true
                elseif request.status == "applied" and not paths[request.projectile_id] then
                    paths[request.projectile_id] = { dx = pending.direction * 10 }
                elseif request.status == "failed" then
                    sf2.log.warn(request.error or "Burst spawn failed")
                end
            end
            if not waiting then pending = nil end
        end
        -- A receipt supplies only an ID. Reacquire the matching live handle.
        local projectiles, error = fighter:projectiles()
        if not projectiles then sf2.log.warn(error or "Projectiles unavailable"); return end
        local seen = {}
        for _, projectile in ipairs(projectiles) do
            local view = projectile:snapshot()
            if view then
                seen[view.id] = true
                local path = paths[view.id]
                if path then projectile:move_by(path.dx, 0) end
            end
        end
        for id in pairs(paths) do if not seen[id] then paths[id] = nil end end
        self.state.cooldown = math.max(0, self.state.cooldown - tick.delta_frames)
        if not pending and self.state.cooldown == 0 then
            sf2.ui.set_text(hud, "status", "Burst ready | Hits: " .. self.state.hits)
            sf2.ui.set_enabled(hud, "fire", true)
        end
        if not requested then return end
        requested = false
        if pending or self.state.cooldown > 0 then return end
        local direction = snapshot.opponent.position.x >= snapshot.self.position.x and 1 or -1
        local requests = {}
        -- Procedural pattern: three native actors, no caster animation required.
        -- Offsets use world axes; Y is positive down. Each receipt is independent.
        for index = 1, 3 do
            requests[index] = fighter:spawn_projectile(burst, direction * (index - 1) * 30, (index - 2) * 20)
        end
        pending = { requests = requests, direction = direction }
        self.state.cooldown = 180
        sf2.ui.set_enabled(hud, "fire", false)
        sf2.ui.set_text(hud, "status", "Burst queued (3 seconds)")
    end,
    on_damage_dealt = function(self, _, event)
        local attack = event.attack
        if attack and attack.kind == "projectile" and attack.projectile_owner == sf2.mod.id and attack.model_name == actor then
            self.state.hits = self.state.hits + 1
            if hud and sf2.ui.is_open(hud) then sf2.ui.set_text(hud, "status", "Native hits: " .. self.state.hits) end
        end
    end,
    on_round_end = close, on_fight_end = close,
}
local rule = sf2.rules.behavior { id = "burst", behavior = behavior, target = sf2.rules.PLAYER }
for _, fight in ipairs { "core:fights/zone_1/tournament/3", "core:fights/zone_1/tournament_eclipsemode/3" } do
    sf2.fights.patch { target = fight, append_rules = { rule } }
end
