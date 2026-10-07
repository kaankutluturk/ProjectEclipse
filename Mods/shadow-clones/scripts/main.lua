-- Shadow Clones: wear the Faceless Mask and press RaidCharge. The fighter tears
-- three shadows out of the dark; they fight beside you for ten seconds, then fade.
local sf2 = require("sf2")
local key = sf2.localization.key

local CLONE_FRAMES = 600      -- how long each clone fights (60 frames = 1 second)
local COOLDOWN_FRAMES = 1200  -- from one summon to the next
local SPAWN_DELAY = 14        -- frames into the cast when the shadows appear

local framing = sf2.settings.toggle {
    id = "framing", label = "Frame the clones", default = true,
    description = "While clones fight, the camera pulls back to keep every fighter in view.",
}
local flash = sf2.settings.toggle {
    id = "summon_flash", label = "Summon flash", default = true,
    description = "The picture sinks into ink and time stalls for a moment as the shadows rise.",
}

-- The summon: the world drains to ink and violet, time stalls and the camera
-- leans in on the caster. sf2.fx.play fires it from the cast below.
local rise = sf2.fx.screen {
    id = "shadow_rise", setting = flash, trigger = "script",
    saturation = 0.2, contrast = 1.4, brightness = -0.15, vignette = 0.75,
    tint = "#6A4A9C", tint_strength = 0.35, accent = "#9A6BFF", accent_strength = 0.8,
    hold = 0.3, duration = 1.1, time_scale = 0.3, zoom = 1.3, zoom_offset_y = -40,
    sound = "snd_time_shift", sound_volume = 0.6, muffle = 0.5,
}
-- A clone that runs out of time leaves with a pale blink.
local vanish = sf2.fx.screen {
    id = "shadow_vanish", setting = flash, trigger = "script",
    brightness = 0.1, saturation = 0.5, contrast = 1.1, duration = 0.4,
}

-- Each clone is a full fighter with its own weapon and the game's own AI.
local clone = sf2.behaviors.register {
    id = "clone",
    on_actor_end = function(_, fighter)
        if fighter.actor_end_reason == "expired" then sf2.fx.play(vanish) end
    end,
}
local clones = {}
for index, template in ipairs { "man_long_katana", "girl_kusarigama", "man_swords" } do
    local warrior = sf2.warriors.register {
        id = "shadow_" .. index, template = sf2.warriors.get_template("core:warrior-templates/" .. template),
        level = 30, tactic = "Standard",
    }
    clones[index] = sf2.actors.register {
        id = "shadow_" .. index, character = warrior, team = "owner",
        lifetime_frames = CLONE_FRAMES, max_health = 0.4, behavior = clone,
    }
end
-- Where each shadow rises, relative to the caster: two behind, one dropping in.
local placements = { { x = -170, y = 0 }, { x = -320, y = 0 }, { x = -70, y = -320 } }

-- The cast is the Grasp of Darkness boss pose (an unchanged copy of the base
-- game's darkness_hug_player.bytes), bound to RaidCharge. The mask's behavior
-- sets the "ready" flag, so only a masked fighter off cooldown can cast it.
local READY = sf2.mod.id .. ":behaviors/mask:ready"
local summon = sf2.moves.register {
    id = "summon", animation = "animations/darkness_hug_player",
    type = "ATTACK", priority = 1000, mid_frames = 2, first_frame = 2,
    mirror_node = "NHeel_1", no_wall_repulsion = true, direction = "face_enemy",
    align = { axes = { "X", "Z" }, pivot = { node = "NHeel_2" }, position = { pivot = "Me" } },
    events = { "key_pressed" },
    conditions = {
        { key = "RaidCharge" }, { mod = READY },
        { not_mod = "Concussion" }, { not_mod = "Stun" }, { controllable = true },
    },
    locks = { { item = "Skeleton", subtype = "Skeleton" } },
    intervals = {
        { name = "Uninterrupt", from = 9, to = 39 },
        { type = "Invulnerable", name = "Evade", from = 14, to = 33 },
    },
    timeline = { [9] = { sound = "snd_shadow_grasp" } },
}
local SUMMON = sf2.mod.id .. ":moves/summon"

-- Camera: while clones fight, frame everyone. Kept in Lua memory, never saved.
local camera, view_x, view_zoom
local function release()
    if camera then sf2.world.release_camera(camera) end
    camera, view_x, view_zoom = nil, nil, nil
end

local function frame(fighter, actors)
    local view = fighter:snapshot()
    if not view then return end
    local low, high = view.self.position.x, view.self.position.x
    local function include(x) low, high = math.min(low, x), math.max(high, x) end
    if view.opponent then include(view.opponent.position.x) end
    for _, actor in ipairs(actors) do
        local seen = actor:snapshot()
        if seen then include(seen.position.x) end
    end
    -- The game clamps zoom so the arena art always fills the screen.
    local target_x = (low + high) / 2
    local target_zoom = math.max(0.6, math.min(1, 1250 / (high - low + 450)))
    view_x = view_x and view_x + (target_x - view_x) * 0.08 or target_x
    view_zoom = view_zoom and view_zoom + (target_zoom - view_zoom) * 0.05 or 1
    local settings = { center_x = view_x, zoom = view_zoom }
    if camera and sf2.world.is_camera_active(camera) then
        sf2.world.set_camera(camera, settings)
    else
        local failure
        camera, failure = fighter:acquire_camera(settings)
        if not camera then sf2.log.debug("Clone framing unavailable: " .. tostring(failure)) end
    end
end

local mask = sf2.behaviors.register {
    id = "mask",
    state = { lifetime = "round", fields = {
        armed = { type = sf2.behaviors.BOOLEAN, default = false },
        cooldown = { type = sf2.behaviors.INTEGER, default = 0 },
        spawn_in = { type = sf2.behaviors.INTEGER, default = -1 },
        facing = { type = sf2.behaviors.INTEGER, default = 1 },
    } },
    on_round_begin = function() release() end,
    on_tick = function(self, fighter, tick)
        local state = self.state
        if not state.armed then
            state.armed = true
            fighter:set_control_visible("raid_charge", true)
            fighter:set_flag("ready")
        end
        if state.cooldown > 0 then
            state.cooldown = state.cooldown - tick.delta_frames
            if state.cooldown <= 0 then
                state.cooldown = 0
                fighter:set_flag("ready")
            end
        end
        if state.spawn_in >= 0 then
            state.spawn_in = state.spawn_in - tick.delta_frames
            if state.spawn_in < 0 then
                for index, definition in ipairs(clones) do
                    local at = placements[index]
                    local receipt = fighter:spawn_actor(definition, at.x * state.facing, at.y)
                    if receipt.status == "failed" then sf2.log.warn("Shadow did not rise: " .. tostring(receipt.error)) end
                end
            end
        end
        local actors = fighter:actors() or {}
        if #actors > 0 and sf2.settings.get(framing) then frame(fighter, actors)
        elseif camera then release() end
    end,
    on_animation_start = function(self, fighter, event)
        if event.target ~= "self" or event.animation_name ~= SUMMON then return end
        fighter:clear_flag("ready")
        fighter:set_button_cooldown("raid_charge", COOLDOWN_FRAMES)
        self.state.cooldown = COOLDOWN_FRAMES
        self.state.spawn_in = SPAWN_DELAY
        local view = fighter:snapshot()
        if view then
            local animation = view.self.animation
            self.state.facing = animation and animation.facing or 1
            sf2.fx.play(rise, { x = view.self.position.x })
        end
    end,
    on_round_end = function() release() end,
    on_fight_end = function() release() end,
}

local perk = sf2.perks.register {
    id = "shadow_clones", kind = sf2.perks.SINGLE, behavior = mask,
    display_name = key("perk.shadow_clones"), description = key("perk.shadow_clones.description"),
    icon = sf2.assets.sprite("core:UI/Items/Helm25.img_helm_faceless_mask"),
}
local helm = sf2.items.register_helm {
    id = "faceless_mask", display_name = key("helm.faceless_mask"),
    icon = sf2.assets.sprite("core:UI/Items/Helm25.img_helm_faceless_mask"),
    model = sf2.assets.model("core:gamedata/models/mdl_helm_faceless_mask"),
}
sf2.items.set_innate_perks { item = helm, entries = { { perk = perk } } }
sf2.shop.addItem { section = sf2.shop.HELMETS, item = helm, level = 2, price = sf2.price.coins(1) }
