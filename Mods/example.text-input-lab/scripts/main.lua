local sf2 = require("sf2")
local hud, form
local name, notes = "", ""
local function close()
    if form and sf2.ui.is_open(form) then sf2.ui.close(form) end
    if hud and sf2.ui.is_open(hud) then sf2.ui.close(hud) end
    form, hud = nil, nil
end
local function edit_notes()
    if form and sf2.ui.is_open(form) then return end
    form = sf2.ui.open { id = "notes", mount = "modal",
        root = { id = "root", kind = "column", width = 480, height = 284, gap = 8,
            children = {
                { id = "title", kind = "text", height = 40, text = "Write a fighter introduction" },
                { id = "notes", kind = "text_input", height = 124, text = notes,
                  placeholder = "Two lines are welcome", multiline = true, max_chars = 128 },
                { id = "count", kind = "text", height = 32, text = "Plain text, up to 128 UTF-16 units",
                  style = { font_size = 16 } },
                { id = "apply", kind = "button", height = 44, text = "Apply introduction" },
            } },
        on_change = function(view, id, value)
            if id == "notes" then
                assert(type(value) == "string" and sf2.ui.get_text(view, id) == value)
                sf2.ui.set_text(view, "count", "Edited introduction")
            end
        end,
        on_click = function(view, id)
            if id ~= "apply" then return end
            notes = sf2.ui.get_text(view, "notes")
            sf2.ui.set_text(hud, "status", "Introduction applied")
            sf2.ui.close(view)
        end,
        on_close = function() form = nil end,
    }
end
local behavior = sf2.behaviors.register {
    id = "text_input",
    on_round_begin = function()
        close()
        hud = sf2.ui.open { id = "text_input", mount = "hud",
            placement = { anchor = "bottom_left", x = 24, y = -24 },
            root = { id = "root", kind = "column", width = 320, height = 164, gap = 8,
                style = { background_color = "#fff4dbf0" }, children = {
                    { id = "status", kind = "text", height = 32, text = "Type a fighter name",
                      style = { text_color = "#2b2119", font_size = 16 } },
                    { id = "name", kind = "text_input", height = 44, text = name,
                      placeholder = "Fighter name", max_chars = 24 },
                    { id = "edit", kind = "button", height = 44, text = "Edit introduction" },
                } },
            on_change = function(view, id, value)
                if id == "name" then
                    assert(type(value) == "string" and sf2.ui.get_text(view, id) == value)
                    name = value
                    sf2.ui.set_text(view, "status", value == "" and "Type a fighter name" or ("Name: " .. value))
                end
            end,
            on_click = function(_, id) if id == "edit" then edit_notes() end end,
            on_close = function()
                if form and sf2.ui.is_open(form) then sf2.ui.close(form) end
                form, hud = nil, nil
            end,
        }
    end,
    on_round_end = close, on_fight_end = close,
}
local rule = sf2.rules.behavior { id = "text_input", behavior = behavior, target = sf2.rules.PLAYER }
for _, fight in ipairs { "core:fights/zone_1/tournament/3", "core:fights/zone_1/tournament_eclipsemode/3" } do
    sf2.fights.patch { target = fight, append_rules = { rule } }
end
