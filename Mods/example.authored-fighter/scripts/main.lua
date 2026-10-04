local sf2 = require("sf2")

-- The body keeps the standard ordered rig, with authored silhouette geometry.
-- Sash helpers append after equipment and do not change the clip's 67 points.
local character = sf2.warriors.register {
    id = "sash_fighter", level = 1, skeleton = "Skeleton",
    template = sf2.warriors.get_template("core:warrior-templates/man_kungfu"),
    body_model = sf2.assets.model("models/body"),
    skin_models = { sf2.assets.model("models/sash") },
}
-- A registered comparison form, not a saved loadout or original-body snapshot.
local core_form = sf2.warriors.register {
    id = "core_comparison", level = 1, skeleton = "Skeleton",
    template = sf2.warriors.get_template("core:warrior-templates/man_kungfu"),
}
local strike = sf2.moves.register {
    id = "sash_strike", animation = sf2.assets.binary("animations/strike"),
    type = "ATTACK", priority = 150, mid_frames = 0,
    first_frame = 0, end_frame = 60, mirror_node = "NHeel_1", direction = "face_enemy",
    locks = { { item = "Skeleton", subtype = "Skeleton" } },
    events = "controlled",
    conditions = {
        { character = character }, { key = "Punch" }, { controllable = true },
    },
    align = { axes = { "X", "Z" }, pivot = { node = "NHeel_2" }, position = { pivot = "Me" } },
    intervals = {
        { type = "Uninterrupt", from = 0, to = 60 },
        { type = "Attack", from = 18, to = 30, attack = {
            edges = { "EForearm_1", "EHand_1", "EFingers_1" },
            damage = 0.11, damage_terms = { UnarmedDamage = -10 },
            hit = "High", impulse = { x = 0 }, id = 553,
            options = { ignores_block = true, no_critical = true },
        } },
    },
}
local hud, command, paired, automatic, reforming, formation_origin
local form_request, form_message
local pending = {}
local host = sf2.behaviors.register {
    id = "authored_body",
    state = { lifetime = "round", fields = {
        id = { type = "string", default = "" },
        cooldown = { type = "integer", default = 0 },
        starts = { type = "integer", default = 0 },
    } },
    on_actor_spawn = function(self, fighter)
        self.state.id = fighter.actor_id
        sf2.log.info("AUTHORED-FIGHTER:born:" .. fighter.actor_id)
    end,
    on_tick = function(self, fighter)
        assert(self.state.id == fighter.actor_id)
        self.state.cooldown = math.max(0, self.state.cooldown - 1)
        local request = pending[fighter.actor_id]
        if request and request.status ~= "queued" then
            sf2.log.info("AUTHORED-FIGHTER:receipt:" .. fighter.actor_id .. ":" .. request.status)
            if request.error then sf2.log.warn(request.error) end
            pending[fighter.actor_id] = nil
            self.state.cooldown = 120
        end
        if automatic and paired and not reforming and not pending[fighter.actor_id] then
            local view = assert(fighter:snapshot())
            if view.self.animation and view.self.animation.type == "attack" then return end
            if view.opponent then
                local delta = view.opponent.position.x - view.self.position.x
                if math.abs(delta) > 95 then
                    assert(fighter.actor:move_by(delta > 0 and 3 or -3, 0))
                    return
                end
            end
            if self.state.cooldown == 0 then pending[fighter.actor_id] = fighter.actor:play_move(strike) end
        end
    end,
    on_animation_start = function(self, fighter, event)
        if event.target == "self" and event.animation_name == sf2.mod.id .. ":moves/sash_strike" then
            self.state.starts = self.state.starts + 1
            sf2.log.info("AUTHORED-FIGHTER:start:" .. fighter.actor_id .. ":" .. self.state.starts)
        end
    end,
    on_damage_dealt = function(_, fighter, event)
        assert(event.attack.actor_id == fighter.actor_id)
        assert(event.attack.animation_name == sf2.mod.id .. ":moves/sash_strike")
        sf2.log.info("AUTHORED-FIGHTER:dealt:" .. fighter.actor_id .. ":" .. event.damage)
    end,
    on_actor_end = function(_, fighter)
        pending[fighter.actor_id] = nil
        sf2.log.info("AUTHORED-FIGHTER:ended:" .. fighter.actor_id .. ":" .. fighter.actor_end_reason)
    end,
}
local left = sf2.actors.register { id = "left", character = character, behavior = host, ai = false, max_health = 10, lifetime_frames = 1800 }
local right = sf2.actors.register { id = "right", character = character, behavior = host, ai = false, team = "opponent", max_health = 10, lifetime_frames = 1800 }
local function close()
    if hud and sf2.ui.is_open(hud) then sf2.ui.close(hud) end
    hud, command, paired, automatic, pending = nil, nil, false, false, {}
    reforming, formation_origin = false, nil
    form_request, form_message = nil, ""
end
local function form_pair(actors)
    local complete = true
    for _, actor in ipairs(actors) do
        local view = assert(actor:snapshot())
        local destination = formation_origin + (view.team == "opponent" and 100 or 0)
        local delta = destination - view.position.x
        if math.abs(delta) > 0.5 then
            -- Actor motion permits at most 100 units per axis per request.
            -- Re-form over successive simulation ticks after a large reaction.
            assert(actor:move_by(math.max(-100, math.min(100, delta)), 0))
            complete = false
        end
    end
    return complete
end
local controller = sf2.behaviors.register {
    id = "lab",
    on_round_begin = function()
        close()
        hud = sf2.ui.open { id = "authored", mount = "hud", placement = { anchor = "top_right", x = -24, y = 220 },
            root = { id = "root", kind = "column", width = 300, height = 400, children = {
                { id = "status", kind = "text", text = "Authored Fighter Lab ready", height = 40 },
                { id = "form_status", kind = "text", text = "Player form: unchanged", height = 40 },
                { id = "player", kind = "button", text = "Try authored player (Punch)", height = 40 },
                { id = "core", kind = "button", text = "Try core comparison form", height = 40 },
                { id = "summon", kind = "button", text = "Summon authored pair", height = 40 },
                { id = "reform", kind = "button", text = "Reset pair spacing", height = 40 },
                { id = "left", kind = "button", text = "Left fighter: authored strike", height = 40 },
                { id = "right", kind = "button", text = "Right fighter: authored strike", height = 40 },
                { id = "auto", kind = "button", text = "Toggle repeated strikes", height = 40 },
                { id = "dismiss", kind = "button", text = "Dismiss pair", height = 40 },
            } }, on_click = function(_, widget) command = widget end,
        }
    end,
    on_tick = function(_, fighter)
        if not hud or not sf2.ui.is_open(hud) then return end
        if form_request and form_request.status ~= "queued" then
            form_message = form_request.status
            if form_request.error then form_message = form_message .. ": " .. form_request.error end
            sf2.log.info("AUTHORED-FIGHTER:form:" .. form_request.status)
            form_request = nil
        end
        local actors = assert(fighter:actors())
        if (command == "player" or command == "core") and not form_request then
            form_request = fighter:change_form(command == "player" and character or core_form)
            form_message = "queued"
        elseif command == "summon" and #actors == 0 then
            fighter:spawn_actor(left, 180, 0)
            fighter:spawn_actor(right, 280, 0)
        elseif command == "dismiss" then
            automatic = false
            for _, actor in ipairs(actors) do assert(actor:remove()) end
        elseif command == "auto" then automatic = not automatic
        elseif command == "reform" then
            automatic, reforming = false, true
            formation_origin = assert(fighter:snapshot()).self.position.x + 180
        elseif command == "left" or command == "right" then
            local team = command == "left" and "player" or "opponent"
            for _, actor in ipairs(actors) do
                local view = assert(actor:snapshot())
                if view.team == team and not pending[view.id] then
                    pending[view.id] = actor:play_move(strike)
                    sf2.log.info("AUTHORED-FIGHTER:queued:" .. view.id)
                end
            end
        end
        command = nil
        if #actors == 2 and not paired then
            assert(actors[1]:set_target(actors[2]))
            assert(actors[2]:set_target(actors[1]))
            -- Core initialization can reposition a compatible rig during its
            -- first stance. Form the pair after both spawn receipts have settled.
            reforming = true
            formation_origin = assert(fighter:snapshot()).self.position.x + 180
            paired = true
        elseif #actors < 2 then paired, reforming = false, false end
        if reforming then reforming = not form_pair(actors) end
        sf2.ui.set_text(hud, "status", "Authored fighters: " .. #actors .. (reforming and " | spacing" or automatic and " | repeating" or " | manual"))
        sf2.ui.set_text(hud, "form_status", "Player form: " .. form_message)
    end,
    on_round_end = close, on_fight_end = close,
    on_animation_start = function(_, _, event)
        if event.target == "self" and event.animation_name == sf2.mod.id .. ":moves/sash_strike" then
            sf2.log.info("AUTHORED-FIGHTER:player-start")
        end
    end,
    on_damage_dealt = function(_, fighter, event)
        if event.attack and event.attack.animation_name == sf2.mod.id .. ":moves/sash_strike" then
            assert(fighter.actor_id == nil and event.attack.actor_id == nil)
            sf2.log.info("AUTHORED-FIGHTER:player-dealt")
        end
    end,
}
local rule = sf2.rules.behavior { id = "lab", behavior = controller, target = sf2.rules.PLAYER }
for _, fight in ipairs { "core:fights/zone_1/tournament/3", "core:fights/zone_1/tournament_eclipsemode/3" } do
    sf2.fights.patch { target = fight, append_rules = { rule } }
end

-- The owned encounter starts with this character; no form request is needed.
sf2.localization.register { id = "entry", language = "eng", value = "Authored Fighter Lab" }
local entry = sf2.mod.id .. ":localization/entry"
local zone = sf2.zones.register { id = "lab", file = "Map1.1", start = false }
local battle = sf2.battles.register {
    id = "playable", zone = zone, type = sf2.battles.STORY, x = 0, y = 0,
    alias = entry, title = entry, description = entry, location = "arena",
}
local no_reward = sf2.rewards.register { id = "lab_loss", items = {} }
local win_reward = sf2.rewards.register { id = "lab_win", items = {} }
local playable = sf2.fights.register {
    id = "playable", battle = battle, location = "arena", rounds = 1, round_time = 99,
    player_character = character, warriors = { core_form }, rules = { rule },
    rewards = { no_reward, win_reward },
}
sf2.modes.register { id = "playable", fights = { playable }, repeatable = true }
sf2.quests.register { id = "entry", place = "map", events = { "session" },
    actions = { { type = "show_battle", battle = battle, locked = false } } }
