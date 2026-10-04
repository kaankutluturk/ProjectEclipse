local sf2 = require("sf2")
local label = sf2.localization.key("fighter")
local icon = sf2.assets.sprite("core:ui/users/character_savage")
local empty = sf2.assets.model("models/empty")
local armor = sf2.items.register_armor {
    id = "imported_body", display_name = label, icon = icon, model = empty,
    initial_stats = { unarmed_damage = 0, body_defense = 0 },
}
local helm = sf2.items.register_helm {
    id = "imported_head", display_name = label, icon = icon, model = empty,
    initial_stats = { head_defense = 0 },
}
local character = sf2.warriors.register {
    id = "authored_character", level = 1,
    template = sf2.warriors.get_template("core:warrior-templates/man_kungfu"),
    first_name = sf2.mod.id .. ":localization/fighter", last_name = "", voice = "Male",
    skeleton = "Skeleton", items = { sf2.items.get("core:items/weapon/Fists"), armor, helm },
    body_model = sf2.assets.model("models/body"),
    skin_models = { sf2.assets.model("models/skin1") }, tactic = "Standard",
}
return { warrior = character }
