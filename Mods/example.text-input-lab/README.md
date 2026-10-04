# Text Input Lab

Enable this mod, restart Eclipse, and enter Tournament 3 in normal or Eclipse
mode. Click the HUD name field and type; its status updates through a string
`on_change` callback. Edit introduction opens a multiline modal. Apply reads its
current value with `sf2.ui.get_text` and closes the modal. Back first leaves text
editing, then dismisses a menu/modal. Tab visits the next control. HUD buttons
use pointer input.

Combat continues behind the UI. Typing captures gameplay input while the field
is focused; a menu/modal captures input for its lifetime. Values live only in
this Lua context, survive another round here, and reset on script reload. This
example does not rename the native fighter or write a save. The native game
font determines which glyphs can be displayed; emoji support is not promised.
