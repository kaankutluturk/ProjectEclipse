local sf2 = require("sf2")
local actor = sf2.mod.id .. ".dart"
local dart_item = sf2.items.get("core:items/ranged/RANGED_C2_Z2_MONK_SHURIKEN")

-- This is a real native child weapon actor, with a four-node missile rig.
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
-- The native launch clip shares the caster's animation origin. Its baked
-- missile coordinates follow the throwing hand; flight then retains that pose.
local launch = sf2.moves.register {
    id = "launch", animation = "animations/ranged_light_weapon",
    core_templates = { "RangedMissile", "RangedMissileStart", "MissileStart" },
    priority = 500, mid_frames = 2, first_frame = 6,
    no_interpolation_frames = true, no_wall_repulsion = true, no_magic_recharge = true,
    locks = { { item = "Weapon", subtype = "Shuriken" },
        { item = "Skeleton", subtype = "SkeletonMissile" } },
    conditions = { { actor = actor } },
    direction = { from = { wall = "Back", player = "Parent" },
        to = { wall = "Front", player = "Parent" } },
    align = { axes = { "X", "Z" }, pivot = { animation = true },
        position = { animation = "Parent" }, shift_model_node = "Ranged-Node2_1" },
    timeline = { animation_end = { play_animation = flight, player = "Me" } },
}
local cast = sf2.moves.register {
    id = "cast", animation = "animations/ranged_light_player",
    type = "ATTACK", priority = 110, mid_frames = 2, first_frame = 1,
    mirror_node = "NHeel_1", direction = "face_enemy",
    locks = { { item = "Skeleton", subtype = "Skeleton" } },
    align = { axes = { "X", "Z" }, pivot = { node = "NHeel_2" }, position = { pivot = "Me" } },
    intervals = { { name = "Uninterrupt", to = 30 } },
    timeline = { [5] = {
        { sound = "snd_knife" },
        { projectile = { name = actor, core_skeleton = "SkeletonMissile",
            item = dart_item, start_move = launch, lifetime_frames = 120 } },
    } },
}

local hud, requested, pending
local tracked = {}
local function close()
    if hud and sf2.ui.is_open(hud) then sf2.ui.close(hud) end
    hud, requested, pending = nil, false, nil
    tracked = {}
end
local function update_counts(state)
    if hud and sf2.ui.is_open(hud) then
        sf2.ui.set_text(hud, "counts", "Flights: " .. state.flights .. " | Hits: " .. state.hits)
    end
end
local behavior = sf2.behaviors.register {
    id = "ability",
    state = { lifetime = "round", fields = {
        cooldown = { type = "integer", default = 0 },
        flights = { type = "integer", default = 0 },
        hits = { type = "integer", default = 0 },
    } },
    on_round_begin = function(self)
        close()
        hud = sf2.ui.open { id = "ability", mount = "hud",
            placement = { anchor = "top_right", x = -24, y = 328 },
            root = { id = "root", kind = "column", width = 300, height = 116,
                style = { background_color = "#fff4dbf0" }, children = {
                    { id = "status", kind = "text", text = "Return Dart ready", height = 32,
                        style = { text_color = "#2b2119", font_size = 18 } },
                    { id = "counts", kind = "text", text = "", height = 28,
                        style = { text_color = "#2b2119", font_size = 16 } },
                    { id = "cast", kind = "button", text = "Throw Return Dart", height = 48 },
                } },
            on_click = function(_, widget) if widget == "cast" then requested = true end end,
        }
        update_counts(self.state)
    end,
    on_tick = function(self, fighter, tick)
        if not hud or not sf2.ui.is_open(hud) then requested = false; return end
        -- Native flight has zero forward velocity; Lua owns its trajectory.
        -- Reacquire references each tick. Only copied IDs/positions survive.
        local projectiles, failure = fighter:projectiles()
        if not projectiles then sf2.log.warn(failure or "Projectile query unavailable"); return end
        local fighter_view = fighter:snapshot()
        if not fighter_view or not fighter_view.opponent then return end
        local seen = {}
        for _, projectile in ipairs(projectiles) do
            local view = projectile:snapshot()
            if view and view.animation_name == sf2.mod.id .. ":moves/flight" then
                seen[view.id] = true
                local path = tracked[view.id]
                if not path then
                    local direction = fighter_view.opponent.position.x >= fighter_view.self.position.x and 1 or -1
                    path = { origin = view.position.x, direction = direction, steps = 0 }
                    tracked[view.id] = path
                end
                path.steps = path.steps + tick.delta_frames
                local returning = path.steps > 28
                local dx = returning and path.origin - view.position.x or 10 * path.direction
                if returning and math.abs(dx) <= 12 then
                    projectile:remove()
                else
                    dx = math.max(-10, math.min(10, dx))
                    local accepted, reason = projectile:move_by(dx, 0)
                    if not accepted then sf2.log.warn(reason or "Projectile motion rejected") end
                end
            end
        end
        for id in pairs(tracked) do if not seen[id] then tracked[id] = nil end end
        self.state.cooldown = math.max(0, self.state.cooldown - tick.delta_frames)
        if pending and pending.status ~= "queued" then
            if pending.status == "applied" then
                self.state.cooldown = 180
                sf2.ui.set_text(hud, "status", "Cast started (3 seconds)")
            else
                sf2.ui.set_text(hud, "status", "Cast unavailable")
                if pending.error then sf2.log.warn(pending.error) end
                sf2.ui.set_enabled(hud, "cast", true)
            end
            pending = nil
        elseif not pending and self.state.cooldown == 0 then
            sf2.ui.set_text(hud, "status", "Return Dart ready")
            sf2.ui.set_enabled(hud, "cast", true)
        end
        if not requested then return end
        requested = false
        if pending or self.state.cooldown > 0 then return end
        local view = fighter:snapshot()
        if not view or not view.round_active then return end
        if view.self.animation and view.self.animation.type == "attack" then return end
        pending = fighter:play_move(cast)
        sf2.ui.set_text(hud, "status", "Cast queued")
        sf2.ui.set_enabled(hud, "cast", false)
    end,
    on_animation_start = function(self, _, event)
        if event.target == "other" and event.animation_name == sf2.mod.id .. ":moves/flight" then
            self.state.flights = self.state.flights + 1
            update_counts(self.state)
        end
    end,
    on_damage_dealt = function(self)
        -- Counts all damage attributed to this fighter, including normal attacks.
        -- This is an observation, not a command to damage the target.
        self.state.hits = self.state.hits + 1
        update_counts(self.state)
    end,
    on_round_end = close, on_fight_end = close,
}
local rule = sf2.rules.behavior { id = "ability", behavior = behavior, target = sf2.rules.PLAYER }
for _, fight in ipairs { "core:fights/zone_1/tournament/3", "core:fights/zone_1/tournament_eclipsemode/3" } do
    sf2.fights.patch { target = fight, append_rules = { rule } }
end
