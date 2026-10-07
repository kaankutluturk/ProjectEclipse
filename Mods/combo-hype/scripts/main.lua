-- Combo Hype: wear the Showman's Lotus and every combo turns the fight up.
--   3 hits NICE, 5 GREAT, 8 SAVAGE, 12 UNSTOPPABLE.
-- Each tier is richer and louder than the last: colour, contrast, a golden glow,
-- the camera squeezing in and the rest of the fight sinking under you. Drop the
-- combo and it all drains away. The game's own combo counter drives it.
local sf2 = require("sf2")
local key = sf2.localization.key

local function tier_grade(id, values)
    values.id, values.trigger = id, "script"
    values.hold, values.duration = values.hold or 1.6, values.duration or 0.7
    return sf2.fx.screen(values)
end

-- Held while the combo lasts: each hit refreshes the current tier.
local TIERS = {
    { at = 3, word = "NICE",
        grade = tier_grade("tier_1", { saturation = 1.15, contrast = 1.06 }) },
    { at = 5, word = "GREAT",
        grade = tier_grade("tier_2", { saturation = 1.3, contrast = 1.1, vignette = 0.2, zoom = 1.04, zoom_offset_y = -10 }) },
    { at = 8, word = "SAVAGE",
        grade = tier_grade("tier_3", { saturation = 1.45, contrast = 1.18, vignette = 0.35, halation = 0.35,
            halation_color = "#FF7A30", zoom = 1.08, zoom_offset_y = -20, muffle = 0.25 }) },
    { at = 12, word = "UNSTOPPABLE",
        grade = tier_grade("tier_4", { saturation = 1.6, contrast = 1.25, vignette = 0.45, halation = 0.7,
            halation_color = "#FFC860", tint = "#FFD27A", tint_strength = 0.18, zoom = 1.12, zoom_offset_y = -30, muffle = 0.45 }) },
}
-- Stingers on reaching a tier, and a cold drain when the combo breaks.
local STINGS = {
    nil,
    sf2.fx.screen { id = "sting_2", trigger = "script", brightness = 0.12, hold = 0.03, duration = 0.2 },
    sf2.fx.screen { id = "sting_3", trigger = "script", brightness = 0.18, hold = 0.05, duration = 0.3,
        time_scale = 0.5, sound = "snd_super_hit1", sound_volume = 0.6 },
    sf2.fx.screen { id = "sting_4", trigger = "script", brightness = 0.25, hold = 0.12, duration = 0.5,
        time_scale = 0.3, sound = "snd_gong", sound_volume = 0.8 },
}
local drain = sf2.fx.screen { id = "drain", trigger = "script", saturation = 0.6, contrast = 0.95, hold = 0.05, duration = 0.5 }

local function tier_for(combo)
    local found = 0
    for index, tier in ipairs(TIERS) do if combo >= tier.at then found = index end end
    return found
end

-- The counter: big hit count with the tier word under it, on the right.
local hud, hide_in
local function close()
    if hud and sf2.ui.is_open(hud) then sf2.ui.close(hud) end
    hud, hide_in = nil, 0
end
local function show(count_text, word)
    if not hud or not sf2.ui.is_open(hud) then
        hud = sf2.ui.open {
            id = "hype", mount = "hud", placement = { anchor = "right", x = -40, y = -40 },
            root = { id = "root", kind = "column", width = 360, height = 150, gap = 0, children = {
                { id = "count", kind = "text", width = 360, height = 96, text = count_text,
                    style = { font_size = 80, text_align = "right", text_color = "#FFFFFF" } },
                { id = "word", kind = "text", width = 360, height = 50, text = word,
                    style = { font_size = 36, text_align = "right", text_color = "#FFE08A" } },
            } },
        }
    else
        sf2.ui.set_text(hud, "count", count_text)
        sf2.ui.set_text(hud, "word", word)
    end
    hide_in = 0
end

local hype = sf2.behaviors.register {
    id = "hype",
    state = { lifetime = "round", fields = { tier = { type = sf2.behaviors.INTEGER, default = 0 } } },
    on_round_begin = function() close() end,
    on_combo_changed = function(self, fighter, event)
        if event.combo <= 0 then
            -- The combo broke: drain the colour and leave the final count up a moment.
            if self.state.tier > 0 then
                sf2.fx.play(drain)
                show(event.last_combo .. " HITS", TIERS[self.state.tier].word .. "!")
                hide_in = 100
            end
            self.state.tier = 0
            return
        end
        local tier = tier_for(event.combo)
        if tier == 0 then return end
        local view = fighter:snapshot()
        local focus = view and view.opponent and { x = view.opponent.position.x } or nil
        sf2.fx.play(TIERS[tier].grade, focus)
        if tier > self.state.tier and STINGS[tier] then sf2.fx.play(STINGS[tier], focus) end
        self.state.tier = tier
        show(event.combo .. " HITS", TIERS[tier].word)
    end,
    on_tick = function()
        if hide_in > 0 then
            hide_in = hide_in - 1
            if hide_in == 0 then close() end
        end
    end,
    on_round_end = function() close() end,
    on_fight_end = function() close() end,
}

local perk = sf2.perks.register {
    id = "combo_hype", kind = sf2.perks.SINGLE, behavior = hype,
    display_name = key("perk.combo_hype"), description = key("perk.combo_hype.description"),
    icon = sf2.assets.sprite("core:UI/Items/Armor14.img_armor_red_lotus"),
}
local armor = sf2.items.register_armor {
    id = "showmans_lotus", display_name = key("armor.showmans_lotus"),
    icon = sf2.assets.sprite("core:UI/Items/Armor14.img_armor_red_lotus"),
    model = sf2.assets.model("core:gamedata/models/mdl_armor_emerald_breastplate"),
}
sf2.items.set_innate_perks { item = armor, entries = { { perk = perk } } }
sf2.shop.addItem { section = sf2.shop.ARMOR, item = armor, level = 2, price = sf2.price.coins(1) }
