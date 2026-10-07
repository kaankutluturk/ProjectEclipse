using System;
using System.Collections.Generic;
using System.Linq;
using Eclipse.Modding;
using UnityEngine;
using UnityEngine.UI;

namespace Eclipse.Multiplayer
{
    // The Moveset Lab's input editor (every move's keys) and its "new move" dialog.
    public sealed partial class LocalVersusMenu
    {
        private static readonly string[] LabKeys =
            { "Punch", "Kick", "Ranged", "Magic", "RaidCharge", "Super", "Forward", "Back", "Up", "Down", "Up-Forward", "Up-Back", "Down-Forward", "Down-Back" };
        private static readonly string[] LabPresses = { "Tap", "Hold", "Release" };

        // ---- Input ----

        /// <summary>
        /// The move's keys: alternative chords (any one starts the move), each the keys pressed
        /// together with how each is pressed. Every change is one undoable edit.
        /// </summary>
        private void BuildLabInputSection(MovesetBaselineMove baseline, ModMovesetMove entry)
        {
            LabSection(labInspector, "INPUT");
            if (baseline.Input == null)
            {
                LabNoteText(labInspector, "This move's input can't be edited here: " + baseline.InputProblem + ". Edit it in Lua with sf2.moves.patch.");
                return;
            }
            var input = entry?.Input?.Value ?? baseline.Input;
            bool changed = entry?.Input != null;
            var summary = LabHBox(labInspector, 24, 6, null);
            var text = LabText(summary, (changed ? "<color=#D6AA4E>●</color> " : "") + LabDescribeInput(input), 13, LabFore, TextAnchor.MiddleLeft);
            LabSize(text, -1, -1, 1);
            if (changed) LabBtn(summary, "RESET", () => LabSetInput(baseline, baseline.Input), 50, LabStyle.Quiet, 10, "Back to the base game's input: " + LabDescribeInput(baseline.Input));
            var chords = input.Chords.Select(c => c.ToList()).ToList();

            if (chords.Count == 0)
            {
                LabNoteText(labInspector, "No input: the game starts this move by itself (a hit reaction, a combo step or an AI action).");
                LabBtn(labInspector, "ADD AN INPUT", () => LabSetInput(baseline, new ModMoveInput(new[] { new[] { new ModMoveKey("Punch") } })), -1, LabStyle.Normal, 12,
                    "Make this move start only when its keys are pressed");
            }
            for (int c = 0; c < chords.Count; c++)
            {
                int chordIndex = c;
                var chord = chords[c];
                var head = LabHBox(labInspector, 22, 6, null);
                var title = LabText(head, chords.Count > 1 ? "Alternative " + (c + 1) : "Keys pressed together", 11, LabDim, TextAnchor.MiddleLeft);
                LabSize(title, -1, -1, 1);
                if (chords.Count > 1)
                    LabBtn(head, "REMOVE", () => LabSetInput(baseline, LabInputWithout(chords, chordIndex)), 60, LabStyle.Quiet, 10, "Remove this alternative");
                var grid = Rect(labInspector, "Chord " + c);
                var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
                layout.cellSize = new Vector2(188, 24); layout.spacing = new Vector2(6, 4);
                layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount; layout.constraintCount = 2;
                LabSize(grid, -1, (chord.Count + 2) / 2 * 28);
                for (int k = 0; k < chord.Count; k++)
                {
                    int keyIndex = k;
                    var cell = LabHBox(grid, 0, 2, null);
                    Button press = null, key = null;
                    press = LabBtn(cell, chord[k].Press.ToUpperInvariant(), () => OpenLabChoices(press.transform as RectTransform, LabPresses.Select(p => new LabChoice
                    {
                        Label = p, Detail = p == "Tap" ? "press once" : p == "Hold" ? "keep held" : "let go", Current = p == chord[keyIndex].Press,
                        Pick = () => LabSetInput(baseline, LabInputWithKey(chords, chordIndex, keyIndex, new ModMoveKey(chord[keyIndex].Key, p))),
                    }).ToList(), 150), 62, LabStyle.Quiet, 10);
                    key = LabBtn(cell, chord[k].Key, () => OpenLabChoices(key.transform as RectTransform, LabKeys.Select(name => new LabChoice
                    {
                        Label = name, Current = name == chord[keyIndex].Key,
                        Pick = () => LabSetInput(baseline, LabInputWithKey(chords, chordIndex, keyIndex, new ModMoveKey(name, chord[keyIndex].Press))),
                    }).ToList(), 160), -1, LabStyle.Normal, 12);
                    LabChevron(key.transform);
                    if (chord.Count > 1)
                        LabBtn(cell, "x", () => LabSetInput(baseline, LabInputWithKey(chords, chordIndex, keyIndex, null)), 22, LabStyle.Quiet, 11, "Remove this key");
                }
                if (chord.Count < ModMoveInput.MaxKeys)
                    LabBtn(labInspector, "+ KEY", () => LabSetInput(baseline, LabInputWithKey(chords, chordIndex, chord.Count, new ModMoveKey("Punch"))), -1, LabStyle.Quiet, 11,
                        "Add a key to this chord (repeated taps of the same key are a sequence)");
            }
            if (chords.Count != 0)
            {
                var actions = LabHBox(labInspector, 26, 6, null);
                if (chords.Count < ModMoveInput.MaxChords)
                    LabBtn(actions, "+ ALTERNATIVE", () => LabSetInput(baseline, new ModMoveInput(chords.Concat(new[] { new List<ModMoveKey> { new ModMoveKey("Punch") } }))), -1, LabStyle.Normal, 11,
                        "Another key chord that also starts this move");
                LabBtn(actions, "REMOVE INPUT", () => LabSetInput(baseline, ModMoveInput.None), -1, LabStyle.Danger, 11, "The move no longer needs keys");
            }
            if (baseline.Input.IsNone && !input.IsNone)
                LabNoteText(labInspector, "<color=#DE543E>The base game starts this move without keys.</color> With an input it starts only when the keys are pressed, which can stop hit reactions and combo steps from playing.");
            if (!baseline.Input.IsNone && input.IsNone)
                LabNoteText(labInspector, "<color=#DE543E>Without an input this move starts whenever its other conditions are met.</color>");
            LabNoteText(labInspector, "Directions are written for a fighter facing right; the game mirrors them when facing left. What the move must follow (a combo step, being in the air) stays as in its base.");
        }

        private static string LabDescribeInput(ModMoveInput input)
        {
            if (input.IsNone) return "No input";
            return string.Join("   or   ", input.Chords.Select(chord => string.Join(" + ",
                chord.Select(key => key.Press == "Tap" ? key.Key : key.Press.ToLowerInvariant() + " " + key.Key))));
        }

        /// <summary>The input with one key replaced, appended (index = count) or removed (key null).</summary>
        private static ModMoveInput LabInputWithKey(List<List<ModMoveKey>> chords, int chord, int index, ModMoveKey key)
        {
            var copy = chords.Select(c => c.ToList()).ToList();
            if (key == null) copy[chord].RemoveAt(index);
            else if (index >= copy[chord].Count) copy[chord].Add(key);
            else copy[chord][index] = key;
            return new ModMoveInput(copy);
        }

        private static ModMoveInput LabInputWithout(List<List<ModMoveKey>> chords, int chord) =>
            new ModMoveInput(chords.Where((_, i) => i != chord));

        private void LabSetInput(MovesetBaselineMove baseline, ModMoveInput value)
        {
            try { LabEdit((copy, target) => copy.SetInput(target, baseline.Input, value)); }
            catch (ModContentException exception) { SetStatus(exception.Message); }
        }

        // ---- New move ----

        private string labNewBase;
        private int labNewScope;

        /// <summary>Who a new move is for: (label, subtype, item id); both null is "same fighters as the base".</summary>
        private List<(string label, string subtype, string item)> LabNewMoveScopes(string baseMove)
        {
            var scopes = new List<(string, string, string)> { ("Same fighters as " + baseMove, null, null) };
            if (LabSubtype != LabShared) scopes.Add(("Only " + LabScopeName(), LabSubtype, null));
            if (LabCurrentWeapon != null) scopes.Add(("Only " + LabCurrentWeapon.Name, null, LabCurrentWeapon.Id));
            return scopes;
        }

        private void ShowLabNewMove()
        {
            labNewBase = labMove != null ? labCopy.NativeSource(labMove) : LabMoves().FirstOrDefault();
            if (labNewBase == null) { SetStatus("Pick a move to base the new one on."); return; }
            labNewScope = LabSubtype == LabShared ? 0 : LabCurrentWeapon != null ? 2 : 1;
            BuildLabNewMove(string.Empty);
        }

        private void BuildLabNewMove(string name)
        {
            var layer = OpenLabLayer(true);
            var card = LabModal(layer, "NEW MOVE", 520, 330);
            LabNoteText(card, "A new move starts as a copy of an existing one: its animation, attacks and frame windows. The original stays as it is. Give the new move its own input afterwards.");
            var nameRow = LabHBox(card, 28, 8, null);
            var nameCaption = LabText(nameRow, "Name", 12, LabDim, TextAnchor.MiddleLeft); LabSize(nameCaption, 90);
            var nameRect = Rect(nameRow, "Name"); LabSize(nameRect, -1, 26, 1);
            var field = LabInput(nameRect, name, "e.g. rising_slash", InputField.ContentType.Standard);
            field.characterLimit = 64;

            var baseRow = LabHBox(card, 28, 8, null);
            var baseCaption = LabText(baseRow, "Based on", 12, LabDim, TextAnchor.MiddleLeft); LabSize(baseCaption, 90);
            Button pick = null;
            pick = LabBtn(baseRow, labNewBase, () => OpenLabChoices(pick.transform as RectTransform,
                labBaseline.Keys.OrderBy(m => LabMoves().Contains(m) ? 0 : 1).ThenBy(m => m, StringComparer.Ordinal).Select(m => new LabChoice
                {
                    Label = m, Detail = LabMoves().Contains(m) ? "in this view" : "", Current = m == labNewBase,
                    Pick = () => { labNewBase = m; BuildLabNewMove(field.text); },
                }).ToList(), 340), -1, LabStyle.Normal, 12);
            LabChevron(pick.transform);

            var scopes = LabNewMoveScopes(labNewBase);
            labNewScope = Mathf.Clamp(labNewScope, 0, scopes.Count - 1);
            var forCaption = LabText(card, "Who can use it", 11, LabDim, TextAnchor.LowerLeft); LabSize(forCaption, -1, 16);
            var forRow = LabHBox(card, 26, 4, null);
            for (int i = 0; i < scopes.Count; i++)
            {
                int index = i;
                LabBtn(forRow, scopes[i].label, () => { labNewScope = index; BuildLabNewMove(field.text); }, -1, i == labNewScope ? LabStyle.On : LabStyle.Normal, 11);
            }
            var buttons = LabHBox(card, 28, 6, null);
            LabSpacer(buttons, 0, 1);
            LabBtn(buttons, "CANCEL", CloseLabPopup, 90, LabStyle.Quiet, 12);
            LabBtn(buttons, "CREATE", () => CreateLabNewMove(field.text, scopes[labNewScope]), 90, LabStyle.Primary, 12);
            field.onSubmit.AddListener(text => CreateLabNewMove(text, scopes[labNewScope]));
            field.Select(); field.ActivateInputField();
        }

        private void CreateLabNewMove(string name, (string label, string subtype, string item) scope)
        {
            ModMovesetFork fork = null;
            try { labCopy.Edit(copy => fork = copy.CreateAddedMove(name, labNewBase, scope.subtype, scope.item)); }
            catch (ArgumentException exception) { SetStatus(exception.Message); return; }
            CloseLabPopup();
            labMove = labCopy.ForkName(fork);
            labTab = LabTab.Move;
            labPickingClip = false;
            foreach (var (button, value) in labTabButtons) SetLabOn(button, value == labTab);
            labDiscardArmed = false;
            RefreshLab();
            SetStatus("New move " + fork.Id + " based on " + fork.Move + ". Set its input under INPUT, then APPLY (F5) to try it.");
        }
    }
}
