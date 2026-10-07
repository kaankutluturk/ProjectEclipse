using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Eclipse.Modding;
using Eclipse.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Eclipse.Multiplayer
{
    // The Moveset Lab's editor view: a flat tool layout rather than the menu pages' painted
    // style. Top bar: what is being edited and the document actions. Left: the move list.
    // Centre: the attacker and a victim that plays the hit reaction, transport controls and
    // a frame timeline whose bars drag. Right: one inspector tab at a time.
    public sealed partial class LocalVersusMenu
    {
        private static readonly Color LabBg = new Color32(18, 17, 20, 255);
        private static readonly Color LabPanel = new Color32(26, 24, 28, 255);
        private static readonly Color LabRaised = new Color32(42, 39, 45, 255);
        private static readonly Color LabWell = new Color32(12, 11, 14, 255);
        private static readonly Color LabLine = new Color32(56, 51, 58, 255);
        private static readonly Color LabFore = new Color32(232, 225, 213, 255);
        private static readonly Color LabDim = new Color32(152, 143, 134, 255);
        private static readonly Color LabFaint = new Color32(98, 91, 87, 255);
        private static readonly Color LabHot = new Color32(222, 84, 62, 255);
        private static readonly Color LabWindow = new Color32(112, 124, 140, 255);
        private const float LabTopHeight = 48f, LabStatusHeight = 26f, LabListWidth = 236f, LabInspectorWidth = 412f;
        private const float LabTimelineHeight = 200f, LabTransportHeight = 36f, LabLaneLabelWidth = 132f, LabLaneHeight = 22f, LabRulerHeight = 20f;

        private enum LabTab { Move, Attack, Windows, Hitbox }
        private enum LabStyle { Normal, Primary, Danger, Quiet, On }
        private enum LabDragMode { Move, Start, End }

        private VersusFighterPreview labPreview, labVictim;
        private RectTransform labRoot, labListRows, labInspector, labTimelineLanes, labRuler, labPlayheadLayer, labPlayhead, labPopup, labAttackerStage;
        private InputField labFilter;
        private Text labModValue, labScopeValue, labWeaponValue, labMoveCount, labDirty, labFrameReadout, labStageCaption, labVictimCaption, labHoverCaption, labTimelineHint;
        private Button labWeaponButton, labPlayButton;
        private readonly List<(Button button, LabTab tab)> labTabButtons = new List<(Button, LabTab)>();
        private readonly List<(Button button, float speed)> labSpeedButtons = new List<(Button, float)>();
        private Button labLoopButton, labVictimButton;
        private LabTab labTab = LabTab.Attack;
        private int labAttackId = -1, labWindowIndex = -1, labAddType;
        private bool labEditedOnly, labShowBracing, labEdgesKnown, labLoop = true, labUserPaused, labVictimOn = true, labPickingClip;
        private float labSpeed = 1f, labStepClock, labReplayAt = -1f, labPlayedAt;
        private string labPlaying, labHoverEdge, labClipChoice;
        private int labTimelineFrom, labTimelineTo, labLastKeyframe = -1, labReactionChoice;
        private float labWallGap = 60f;
        private readonly HashSet<int> labContacted = new HashSet<int>();
        private static Dictionary<string, List<(string move, string type)>> labReactionMoves;

        // ---- Page ----

        public void ShowMovesetLab()
        {
            EnsureEventSystem();
            labReturning = false;
            labTesting = false;
            try
            {
                if (labBaseline == null) labBaseline = MovesetBaselineReader.ReadAll();
                if (labCopy == null) LoadLabMod(labModId);
                if (labScopes.Count == 0) BuildLabScopes();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                ShowModeSelect();
                SetStatus("The Moveset Lab could not open: " + exception.Message);
                return;
            }
            page = Page.MovesetLab;
            panel.gameObject.SetActive(true);
            liveLabels.Clear();
            shortcuts.Clear();
            backAction = LeaveMovesetLab;
            for (int i = panel.childCount - 1; i >= 0; i--) { panel.GetChild(i).gameObject.SetActive(false); Destroy(panel.GetChild(i).gameObject); }
            SetImage(panel, new Color(0, 0, 0, 0));
            EnterBackdrop();
            labTabButtons.Clear();
            labSpeedButtons.Clear();
            labPopup = null;

            labRoot = Rect(panel, "Moveset Lab"); Stretch(labRoot);
            AddImage(labRoot, LabBg);
            BuildLabTopBar(LabArea(labRoot, "Top bar", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -LabTopHeight), Vector2.zero, LabPanel));
            BuildLabStatusBar(LabArea(labRoot, "Status bar", new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, LabStatusHeight), LabPanel));
            BuildLabMoveList(LabArea(labRoot, "Moves", new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, LabStatusHeight + 1), new Vector2(LabListWidth, -LabTopHeight - 1), LabPanel));
            BuildLabInspectorFrame(LabArea(labRoot, "Inspector", new Vector2(1, 0), new Vector2(1, 1), new Vector2(-LabInspectorWidth, LabStatusHeight + 1), new Vector2(0, -LabTopHeight - 1), LabPanel));
            var centre = LabArea(labRoot, "Centre", Vector2.zero, Vector2.one, new Vector2(LabListWidth + 1, LabStatusHeight + 1), new Vector2(-LabInspectorWidth - 1, -LabTopHeight - 1), null);
            BuildLabTimelineFrame(LabArea(centre, "Timeline", new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, LabTimelineHeight), LabPanel));
            BuildLabTransport(LabArea(centre, "Transport", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, LabTimelineHeight + 1), new Vector2(0, LabTimelineHeight + 1 + LabTransportHeight), LabPanel));
            BuildLabStage(LabArea(centre, "Stage", Vector2.zero, Vector2.one, new Vector2(0, LabTimelineHeight + LabTransportHeight + 2), Vector2.zero, null));

            IsShowing = true; Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
            EclipseUiAudio.SuppressFocusSound();
            EclipseUiAudio.Play(UiSound.Open);
            OnLabScopeChanged();
            SetStatus(labOverlayApplied ? "Your saved edits are applied." : "Pick a move. APPLY (F5) saves " + labModName + " (" + labModId + ") and plays your edits on the fighter.");
        }

        private RectTransform LabArea(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color? background)
        {
            var rect = Rect(parent, name);
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
            if (background.HasValue) { var image = rect.gameObject.AddComponent<Image>(); image.color = background.Value; image.raycastTarget = true; }
            return rect;
        }

        // ---- Top bar ----

        private void BuildLabTopBar(RectTransform bar)
        {
            var row = LabHBox(bar, 0, 6, new RectOffset(12, 8, 6, 6)); Stretch(row);
            var title = LabText(row, "MOVESET LAB", 18, Gold, TextAnchor.MiddleLeft); LabSize(title, 128);
            labModValue = LabSelector(row, "MOD", 170, LabModItems, out _);
            LabBtn(row, "NEW", ShowLabNewMod, 44, LabStyle.Quiet, 11, "Start a new data-only mod");
            LabSpacer(row, 6);
            labScopeValue = LabSelector(row, "FIGHTERS", 160, LabScopeItems, out _);
            labWeaponValue = LabSelector(row, "WEAPON", 160, LabWeaponItems, out labWeaponButton);
            LabSpacer(row, 0, 1);
            LabBtn(row, "UNDO", () => LabHistory(true), 50, LabStyle.Normal, 12);
            LabBtn(row, "REDO", () => LabHistory(false), 50, LabStyle.Normal, 12);
            labDirty = LabText(row, "", 12, Gold, TextAnchor.MiddleRight); LabSize(labDirty, 104);
            LabBtn(row, "APPLY  F5", () => ApplyLab(), 88, LabStyle.Primary, 13);
            LabBtn(row, "TEST  T", TestLabInTraining, 66, LabStyle.Normal, 13);
            LabBtn(row, "CLOSE", LeaveMovesetLab, 56, LabStyle.Quiet, 12);
        }

        /// <summary>A captioned dropdown button; the list comes from <paramref name="items"/> when opened.</summary>
        private Text LabSelector(RectTransform parent, string caption, float width, Func<List<LabChoice>> items, out Button button)
        {
            var box = LabVBox(parent, 1, null); LabSize(box, width);
            var label = LabText(box, caption, 10, LabFaint, TextAnchor.LowerLeft); LabSize(label, -1, 12);
            Text value = null;
            button = LabBtn(box, "", () => OpenLabChoices(value.rectTransform.parent as RectTransform, items(), width), -1, LabStyle.Normal, 13);
            LabSize(button, -1, 22);
            value = button.GetComponentInChildren<Text>();
            value.alignment = TextAnchor.MiddleLeft; value.rectTransform.offsetMin = new Vector2(8, 0); value.rectTransform.offsetMax = new Vector2(-18, 0);
            value.horizontalOverflow = HorizontalWrapMode.Overflow;
            LabChevron(button.transform);
            return value;
        }

        private sealed class LabChoice
        {
            public string Label, Detail;
            public bool Current;
            public Action Pick;
        }

        private List<LabChoice> LabModItems() => LabMods().Select(mod => new LabChoice
        {
            Label = mod.Name, Detail = mod.Id + (mod.Enabled ? "" : "  (off)"), Current = mod.Id == labModId, Pick = () => SwitchLabMod(mod),
        }).ToList();

        private List<LabChoice> LabScopeItems()
        {
            var items = new List<LabChoice>();
            for (int i = 0; i < labScopes.Count; i++)
            {
                int index = i;
                items.Add(new LabChoice { Label = LabScopeName(labScopes[i]), Current = i == labScope, Pick = () => { labScope = index; labWeapon = -1; OnLabScopeChanged(); } });
            }
            return items;
        }

        private List<LabChoice> LabWeaponItems()
        {
            var items = new List<LabChoice>();
            if (!LabScopeHasWeapons) return items;
            items.Add(new LabChoice { Label = "Every " + ClassName(LabSubtype), Detail = "edits reach the whole subtype", Current = labWeapon < 0, Pick = () => { labWeapon = -1; OnLabScopeChanged(); } });
            for (int i = 0; i < labWeapons.Count; i++)
            {
                int index = i;
                items.Add(new LabChoice { Label = labWeapons[i].Name, Detail = labWeapons[i].Owner, Current = i == labWeapon, Pick = () => { labWeapon = index; OnLabScopeChanged(); } });
            }
            return items;
        }

        private void RefreshLabTopBar()
        {
            if (labModValue != null) labModValue.text = labModName ?? labModId;
            if (labScopeValue != null) labScopeValue.text = LabScopeName();
            if (labWeaponValue != null) labWeaponValue.text = LabWeaponName();
            if (labWeaponButton != null) labWeaponButton.interactable = LabScopeHasWeapons;
        }

        // ---- Status bar ----

        private void BuildLabStatusBar(RectTransform bar)
        {
            var hints = LabText(bar, Hints("Space", "Play", "Left/Right", "Frame", "Up/Down", "Move", "Ctrl+Z/Y", "Undo", "F5", "Apply", "Esc", "Back"), 11, LabDim, TextAnchor.MiddleLeft);
            hints.horizontalOverflow = HorizontalWrapMode.Overflow;
            hints.rectTransform.offsetMin = new Vector2(12, 0); hints.rectTransform.anchorMax = new Vector2(0, 1); hints.rectTransform.offsetMax = new Vector2(560, 0);
            status = LabText(bar, string.Empty, 12, LabHot, TextAnchor.MiddleRight);
            status.horizontalOverflow = HorizontalWrapMode.Overflow;
            status.rectTransform.offsetMin = new Vector2(580, 0); status.rectTransform.offsetMax = new Vector2(-12, 0);
        }

        // ---- Move list ----

        private void BuildLabMoveList(RectTransform area)
        {
            var header = LabArea(area, "Header", new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -28), new Vector2(-10, -6), null);
            LabText(header, "MOVES", 12, LabDim, TextAnchor.MiddleLeft);
            labMoveCount = LabText(header, "", 11, LabFaint, TextAnchor.MiddleRight);
            var filterRect = LabArea(area, "Filter", new Vector2(0, 1), new Vector2(1, 1), new Vector2(8, -58), new Vector2(-8, -32), null);
            labFilter = LabInput(filterRect, "", "Search moves", InputField.ContentType.Standard);
            labFilter.onValueChanged.AddListener(_ => RefreshLabList());
            var toggles = LabArea(area, "Filters", new Vector2(0, 1), new Vector2(1, 1), new Vector2(8, -84), new Vector2(-8, -62), null);
            var toggleRow = LabHBox(toggles, 0, 4, null); Stretch(toggleRow);
            Button all = null, edited = null;
            all = LabBtn(toggleRow, "ALL", () => { labEditedOnly = false; SetLabOn(all, true); SetLabOn(edited, false); RefreshLabList(); }, -1, labEditedOnly ? LabStyle.Normal : LabStyle.On, 11);
            edited = LabBtn(toggleRow, "EDITED", () => { labEditedOnly = true; SetLabOn(all, false); SetLabOn(edited, true); RefreshLabList(); }, -1, labEditedOnly ? LabStyle.On : LabStyle.Normal, 11);
            var create = LabArea(area, "New move", new Vector2(0, 1), new Vector2(1, 1), new Vector2(8, -112), new Vector2(-8, -88), null);
            var createRow = LabHBox(create, 0, 0, null); Stretch(createRow);
            LabBtn(createRow, "+ NEW MOVE", ShowLabNewMove, -1, LabStyle.Primary, 11, "Add a new move based on an existing one");
            var scroll = LabArea(area, "List", Vector2.zero, Vector2.one, new Vector2(4, 4), new Vector2(-4, -118), null);
            labListRows = LabScroll(scroll, 1);
        }

        private List<string> LabVisibleMoves()
        {
            string filter = labFilter != null ? labFilter.text.Trim() : string.Empty;
            return LabMoves().Where(move => (filter.Length == 0 || LabDisplayName(move).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                && (!labEditedOnly || labCopy.Entry(move, false)?.HasEdits == true || labCopy.FindFork(move) != null)).ToList();
        }

        private void RefreshLabList()
        {
            if (labListRows == null) return;
            for (int i = labListRows.childCount - 1; i >= 0; i--) Destroy(labListRows.GetChild(i).gameObject);
            var moves = LabVisibleMoves();
            if (labMoveCount != null) labMoveCount.text = moves.Count + " of " + LabMoves().Count;
            foreach (string move in moves)
            {
                var captured = move;
                var entry = labCopy.Entry(move, false);
                bool edited = entry != null && entry.HasEdits;
                bool disabled = entry != null && entry.Disable;
                var row = Rect(labListRows, move); LabSize(row, -1, 24);
                var back = row.gameObject.AddComponent<Image>(); back.color = move == labMove ? new Color(Gold.r, Gold.g, Gold.b, .22f) : new Color(1, 1, 1, 0); back.raycastTarget = true;
                if (move == labMove) { var bar = LabArea(row, "Selected", new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(3, 0), Gold); bar.GetComponent<Image>().raycastTarget = false; }
                var name = LabText(row, (edited ? "<color=#D6AA4E>●</color> " : "") + (disabled ? "<color=#625B57>" + LabDisplayName(move) + "</color>" : LabDisplayName(move)), 13, move == labMove ? LabFore : new Color(LabFore.r, LabFore.g, LabFore.b, .86f), TextAnchor.MiddleLeft);
                var nameClip = LabArea(row, "Name", Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-74, 0), null);
                nameClip.gameObject.AddComponent<RectMask2D>();
                name.rectTransform.SetParent(nameClip, false); Stretch(name.rectTransform);
                name.horizontalOverflow = HorizontalWrapMode.Overflow;
                var badge = LabText(row, disabled ? "disabled" : LabBadge(move), 10, LabFaint, TextAnchor.MiddleRight);
                badge.rectTransform.offsetMax = new Vector2(-8, 0);
                var button = row.gameObject.AddComponent<Button>(); button.targetGraphic = back; button.navigation = new Navigation { mode = Navigation.Mode.None };
                var colors = button.colors; colors.highlightedColor = new Color(1, 1, 1, 1); colors.normalColor = Color.white; colors.selectedColor = Color.white; button.colors = colors;
                LabHover(row.gameObject, () => { if (captured != labMove) back.color = new Color(1, 1, 1, .05f); }, () => { if (captured != labMove) back.color = new Color(1, 1, 1, 0); });
                button.onClick.AddListener(() => { EclipseUiAudio.Play(UiSound.Toggle); SelectLabMove(captured); });
            }
            if (moves.Count == 0) { var none = LabText(labListRows, labEditedOnly ? "No edited moves in this view." : "No move matches.", 12, LabFaint, TextAnchor.MiddleCenter); LabSize(none, -1, 40); }
        }

        private void SelectLabMove(string move)
        {
            if (move == null) return;
            labMove = move;
            labPickingClip = false;
            labAttackId = -1;
            labWindowIndex = -1;
            labReactionChoice = 0;
            RefreshLab();
            labReplayAt = Time.unscaledTime;
        }

        private void StepLabMove(int step)
        {
            var moves = LabVisibleMoves();
            if (moves.Count == 0) return;
            int index = moves.IndexOf(labMove);
            SelectLabMove(moves[Mathf.Clamp(index < 0 ? 0 : index + step, 0, moves.Count - 1)]);
        }

        private void RefreshLab()
        {
            RefreshLabTopBar();
            RefreshLabList();
            RefreshLabInspector();
            RefreshLabTimeline();
        }

        // ---- Stage ----

        private void BuildLabStage(RectTransform stage)
        {
            var attacker = LabArea(stage, "Attacker", new Vector2(0, 0), new Vector2(.6f, 1), new Vector2(0, 0), new Vector2(-1, 0), LabWell);
            var victim = LabArea(stage, "Victim", new Vector2(.6f, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, LabWell);
            labAttackerStage = LabArea(attacker, "Fighter", Vector2.zero, Vector2.one, new Vector2(6, 6), new Vector2(-6, -26), null);
            labPreview = VersusFighterPreview.Create(labAttackerStage, false, new Color32(205, 196, 182, 255));
            labPreview.ShowAttackEdges = true;
            labStageCaption = LabText(attacker, "", 12, LabDim, TextAnchor.UpperLeft);
            labStageCaption.rectTransform.offsetMin = new Vector2(10, 0); labStageCaption.rectTransform.offsetMax = new Vector2(-10, -6);
            labHoverCaption = LabText(attacker, "", 12, Gold, TextAnchor.LowerLeft);
            labHoverCaption.rectTransform.offsetMin = new Vector2(10, 8);
            // Click the fighter in the HITBOX tab to toggle the part under the pointer.
            var hit = labAttackerStage.gameObject.AddComponent<Image>(); hit.color = new Color(0, 0, 0, 0); hit.raycastTarget = true;
            LabOn(labAttackerStage.gameObject, EventTriggerType.PointerClick, data =>
            {
                if (labTab != LabTab.Hitbox || labPreview == null) return;
                string edge = labPreview.EdgeAt(data.position, data.pressEventCamera, 16f);
                if (edge != null) ToggleLabEdge(edge);
            });

            var victimStage = LabArea(victim, "Fighter", Vector2.zero, Vector2.one, new Vector2(6, 6), new Vector2(-6, -26), null);
            labVictim = VersusFighterPreview.Create(victimStage, true, new Color32(150, 142, 132, 255));
            labVictim.Show(trainingDummy ?? VersusLoadouts.Load(VersusLoadouts.Dummy), true);
            labVictimCaption = LabText(victim, "", 12, LabDim, TextAnchor.UpperLeft);
            labVictimCaption.rectTransform.offsetMin = new Vector2(10, 0); labVictimCaption.rectTransform.offsetMax = new Vector2(-10, -6);
        }

        // ---- Transport ----

        private void BuildLabTransport(RectTransform bar)
        {
            var row = LabHBox(bar, 0, 4, new RectOffset(8, 8, 5, 5)); Stretch(row);
            LabBtn(row, "|<", () => LabSeekEdge(false), 32, LabStyle.Normal, 12, "First frame (Home)");
            LabBtn(row, "<", () => LabStepFrame(-1), 30, LabStyle.Normal, 12, "Previous frame (Left)");
            labPlayButton = LabBtn(row, "PLAY", LabTogglePlay, 64, LabStyle.Primary, 12, "Play or pause (Space)");
            LabBtn(row, ">", () => LabStepFrame(1), 30, LabStyle.Normal, 12, "Next frame (Right)");
            LabBtn(row, ">|", () => LabSeekEdge(true), 32, LabStyle.Normal, 12, "Last frame (End)");
            LabBtn(row, "RESTART", LabPlay, 62, LabStyle.Normal, 11, "Play from the first frame");
            LabSpacer(row, 6);
            labLoopButton = LabBtn(row, "LOOP", () => { labLoop = !labLoop; SetLabOn(labLoopButton, labLoop); }, 50, labLoop ? LabStyle.On : LabStyle.Normal, 11, "Repeat the move");
            LabSpacer(row, 6);
            foreach (float speed in new[] { .1f, .25f, .5f, 1f })
            {
                float captured = speed;
                var button = LabBtn(row, speed == 1f ? "1x" : speed.ToString("0.##", CultureInfo.InvariantCulture) + "x", () => SetLabSpeed(captured), 38, speed == labSpeed ? LabStyle.On : LabStyle.Normal, 11, "Preview speed");
                labSpeedButtons.Add((button, speed));
            }
            LabSpacer(row, 6);
            labVictimButton = LabBtn(row, "VICTIM", () => { labVictimOn = !labVictimOn; SetLabOn(labVictimButton, labVictimOn); }, 58, labVictimOn ? LabStyle.On : LabStyle.Normal, 11, "Play the hit reaction on the second fighter");
        }

        private void SetLabSpeed(float speed)
        {
            labSpeed = speed;
            labStepClock = 0f;
            foreach (var (button, value) in labSpeedButtons) SetLabOn(button, value == speed);
        }

        // ---- Playback ----

        /// <summary>The name the preview plays for the selected move: a copy exists in the game only once applied.</summary>
        private string LabPlayableName() =>
            labMove == null ? null : AnimationData.GetAnimationByName(labMove, false) != null ? labMove : labCopy.NativeSource(labMove);

        private bool LabIsPlaying => labPreview != null && labPlaying != null && labPreview.PlayingMove == labPlaying;

        private void LabPlay()
        {
            if (labPreview == null || labMove == null) return;
            string name = LabPlayableName();
            labUserPaused = false;
            labContacted.Clear();
            labLastKeyframe = -1;
            labPreview.ResetPosition();
            if (!labPreview.PlayMove(name)) { labReplayAt = labPreview.IsReady ? -1f : Time.unscaledTime + .2f; if (labPreview.IsReady) SetStatus("This fighter cannot play " + name + "."); return; }
            labPlaying = name;
            labPlayedAt = Time.unscaledTime;
        }

        private void LabTogglePlay()
        {
            if (!LabIsPlaying) { LabPlay(); return; }
            labUserPaused = !labUserPaused;
        }

        private void LabSeek(int frame)
        {
            if (labPreview == null || labMove == null) return;
            var baseline = LabBaselineOf(labMove);
            if (baseline == null) return;
            string name = labPlaying ?? LabPlayableName();
            frame = Mathf.Clamp(frame, baseline.FirstFrame, LabLastFrame(baseline));
            labUserPaused = true;
            if (labPreview.SeekMove(name, frame) < 0) { SetStatus("This fighter cannot play " + name + "."); return; }
            labPlaying = name;
            labPlayedAt = Time.unscaledTime;
            // The victim shows the reaction of the latest attack begun by this frame.
            labContacted.Clear();
            LabAttackView latest = null;
            foreach (var attack in LabAttacks(baseline))
                if (attack.State.Start <= frame) { labContacted.Add(attack.Id); if (latest == null || attack.State.Start >= latest.State.Start) latest = attack; }
            labLastKeyframe = frame;
            if (labVictimOn && latest != null && labVictim != null)
            {
                var option = LabReactionOption(latest);
                var animation = option != null ? AnimationData.GetAnimationByName(option.Move, false) : null;
                if (animation != null)
                {
                    labVictim.SetWallBehind(option.Wall ? labWallGap : (float?)null);
                    labVictim.SeekMove(option.Move, animation.FirstFrame + (frame - latest.State.Start));
                }
            }
            else labVictim?.ResetPosition();
        }

        /// <summary>
        /// Forward steps simulate tick by tick to the next keyframe and stop at the move's last
        /// one; backward steps replay the move up to the earlier keyframe.
        /// </summary>
        private void LabStepFrame(int step)
        {
            var baseline = LabBaselineOf(labMove);
            if (baseline == null) return;
            if (!LabIsPlaying) { LabSeek(baseline.FirstFrame); return; }
            if (step < 0) { LabSeek(Math.Max(baseline.FirstFrame, labPreview.Keyframe + step)); return; }
            labUserPaused = true;
            int last = LabLastFrame(baseline);
            for (int i = 0; i < step && labPreview.Keyframe < last; i++)
            {
                int ticks = labPreview.StepKeyframe();
                if (ticks < 0) { LabSeek(last); return; }
                labVictim?.Step(ticks);
                LabCheckContact();
            }
            if (labPreview.Keyframe >= last) SetStatus("Last keyframe (" + last + ").");
        }

        private void LabSeekEdge(bool last)
        {
            var baseline = LabBaselineOf(labMove);
            if (baseline != null) LabSeek(last ? LabLastFrame(baseline) : baseline.FirstFrame);
        }

        private void UpdateMovesetLab()
        {
            // The Lab is a mouse and keyboard tool: its own keys replace menu navigation.
            inputFrame = Time.frameCount;
            if (labPreview == null) return;
            if (labReplayAt >= 0f && Time.unscaledTime >= labReplayAt && labPreview.IsReady) { labReplayAt = -1f; LabPlay(); }
            if (!labEdgesKnown && labPreview.IsReady && labPreview.Edges().Count > 0) { labEdgesKnown = true; if (labTab == LabTab.Hitbox) RefreshLabInspector(); }
            HandleLabKeys();

            bool playing = LabIsPlaying;
            // Slow speeds and pause step both fighters by hand; full speed runs on the fixed clock.
            bool manual = labUserPaused || labSpeed < 1f;
            labPreview.Paused = manual;
            if (labVictim != null) labVictim.Paused = manual;
            if (!labUserPaused && labSpeed < 1f)
            {
                labStepClock += Time.unscaledDeltaTime * labSpeed;
                int ticks = Mathf.Min(4, (int)(labStepClock / Time.fixedDeltaTime));
                if (ticks > 0)
                {
                    labStepClock -= ticks * Time.fixedDeltaTime;
                    labPreview.Step(ticks);
                    labVictim?.Step(ticks);
                }
            }
            if (playing) LabCheckContact();
            if (labLoop && labPlayedAt > 0f && !playing && !labUserPaused && Time.unscaledTime > labPlayedAt + .3f)
            {
                labPlayedAt = 0f;
                labReplayAt = Time.unscaledTime + .5f;
            }

            var baseline = LabBaselineOf(labMove);
            if (labFrameReadout != null)
                labFrameReadout.text = playing && baseline != null
                    ? "FRAME <color=#D6AA4E>" + labPreview.Keyframe + "</color> / " + baseline.FirstFrame + "–" + LabLastFrame(baseline) + "    tick " + labPreview.MoveTick
                    : baseline == null ? "" : "frames " + baseline.FirstFrame + "–" + LabLastFrame(baseline);
            if (labPlayButton != null) labPlayButton.GetComponentInChildren<Text>().text = playing && !labUserPaused ? "PAUSE" : "PLAY";
            if (labDirty != null) labDirty.text = labCopy == null ? "" : labCopy.IsDirty ? "● Unapplied edits" : labOverlayApplied ? "Applied" : "";
            if (labStageCaption != null)
                labStageCaption.text = labMove == null ? "" : LabDisplayName(labMove) + (labCopy.IsDirty ? "   <color=#D6AA4E>preview shows the last applied version</color>" : "");
            if (labPlayhead != null)
            {
                labPlayhead.gameObject.SetActive(playing && labTimelineTo > labTimelineFrom);
                if (playing)
                {
                    float width = labPlayheadLayer.rect.width;
                    float cell = width / Mathf.Max(1, labTimelineTo - labTimelineFrom);
                    labPlayhead.anchoredPosition = new Vector2(LabFrameX(labPreview.Keyframe, width) + cell / 2f, 0);
                }
            }
            UpdateLabHitboxHover();
        }

        /// <summary>When the playhead enters an attack, the victim plays that attack's reaction.</summary>
        private void LabCheckContact()
        {
            int frame = labPreview.Keyframe;
            if (frame < labLastKeyframe) labContacted.Clear();
            labLastKeyframe = frame;
            var baseline = LabBaselineOf(labMove);
            if (baseline == null || !labVictimOn || labVictim == null) return;
            foreach (var attack in LabAttacks(baseline))
            {
                if (labContacted.Contains(attack.Id) || frame < attack.State.Start || frame > attack.State.End) continue;
                labContacted.Add(attack.Id);
                PlayLabVictim(LabReactionOption(attack));
            }
        }

        private void HandleLabKeys()
        {
            var events = EventSystem.current;
            var selected = events != null ? events.currentSelectedGameObject : null;
            if (selected != null && selected.GetComponent<InputField>() != null && selected.GetComponent<InputField>().isFocused) return;
            bool ctrl = Eclipse.Input.EclipseInput.GetKey(KeyCode.LeftControl) || Eclipse.Input.EclipseInput.GetKey(KeyCode.RightControl);
            bool shift = Eclipse.Input.EclipseInput.GetKey(KeyCode.LeftShift) || Eclipse.Input.EclipseInput.GetKey(KeyCode.RightShift);
            bool Down(KeyCode key) => Eclipse.Input.EclipseInput.GetKeyDown(key);
            if (ctrl)
            {
                if (Down(KeyCode.Z)) LabHistory(!shift);
                else if (Down(KeyCode.Y)) LabHistory(false);
                else if (Down(KeyCode.F) && labFilter != null) { labFilter.Select(); labFilter.ActivateInputField(); }
                return;
            }
            if (labPopup != null) return;
            if (Down(KeyCode.Space)) LabTogglePlay();
            else if (Down(KeyCode.LeftArrow)) LabStepFrame(shift ? -5 : -1);
            else if (Down(KeyCode.RightArrow)) LabStepFrame(shift ? 5 : 1);
            else if (Down(KeyCode.Home)) LabSeekEdge(false);
            else if (Down(KeyCode.End)) LabSeekEdge(true);
            else if (Down(KeyCode.UpArrow)) StepLabMove(-1);
            else if (Down(KeyCode.DownArrow)) StepLabMove(1);
            else if (Down(KeyCode.F5)) ApplyLab();
            else if (Down(KeyCode.T)) TestLabInTraining();
            else if (Down(KeyCode.Alpha1)) SetLabTab(LabTab.Move);
            else if (Down(KeyCode.Alpha2)) SetLabTab(LabTab.Attack);
            else if (Down(KeyCode.Alpha3)) SetLabTab(LabTab.Windows);
            else if (Down(KeyCode.Alpha4)) SetLabTab(LabTab.Hitbox);
        }

        // ---- Attacks and reactions ----

        private sealed class LabAttackView
        {
            public int Id;
            public MovesetBaselineInterval Interval;
            public LabAttack State;
        }

        private List<LabAttackView> LabAttacks(MovesetBaselineMove baseline) =>
            baseline.Intervals.Where(i => i.Attack != null).Select(i => new LabAttackView { Id = i.Attack.Id, Interval = i, State = LabAttackState(i) }).ToList();

        private LabAttackView LabSelectedAttack(MovesetBaselineMove baseline)
        {
            var attacks = LabAttacks(baseline);
            var attack = attacks.FirstOrDefault(a => a.Id == labAttackId) ?? attacks.FirstOrDefault();
            labAttackId = attack?.Id ?? -1;
            return attack;
        }

        /// <summary>Victim moves whose Hit event matches a reaction name: plain hits first, then typed (critical, shock), blocks last.</summary>
        private static List<(string move, string type)> LabReactionCandidates(string reaction)
        {
            if (labReactionMoves == null)
            {
                labReactionMoves = new Dictionary<string, List<(string, string)>>(StringComparer.Ordinal);
                foreach (var animation in AnimationData.Animations)
                {
                    var events = animation?.MoveData?.Events;
                    if (events == null) continue;
                    foreach (var item in events)
                    {
                        if (!(item is EventHit hit) || string.IsNullOrEmpty(hit.AnimationName)) continue;
                        if (!labReactionMoves.TryGetValue(hit.AnimationName, out var list)) labReactionMoves[hit.AnimationName] = list = new List<(string, string)>();
                        if (!list.Any(entry => entry.Item1 == animation.Name && entry.Item2 == (hit.HitType ?? string.Empty))) list.Add((animation.Name, hit.HitType ?? string.Empty));
                    }
                }
                foreach (var pair in labReactionMoves)
                {
                    bool titanReaction = pair.Key.StartsWith("Titan", StringComparison.Ordinal);
                    pair.Value.Sort((a, b) =>
                    {
                        int Rank((string move, string type) entry) =>
                            (entry.type.Length != 0 ? 2 : 0) + (entry.move.IndexOf("Block", StringComparison.Ordinal) >= 0 ? 4 : 0) +
                            (entry.move.StartsWith("Titan", StringComparison.Ordinal) != titanReaction ? 8 : 0);
                        int rank = Rank(a).CompareTo(Rank(b));
                        return rank != 0 ? rank : string.CompareOrdinal(a.move, b.move);
                    });
                }
            }
            return reaction != null && labReactionMoves.TryGetValue(reaction, out var found) ? found : new List<(string, string)>();
        }

        /// <summary>One way the victim can answer a reaction: a hit move, optionally against a wall right behind it.</summary>
        private sealed class LabVictimOption
        {
            public string Move, Type;
            public bool Wall;
            public string Label => Move + (Type.Length != 0 ? "  <color=#988F86>(" + Type.ToLowerInvariant() + ")</color>" : "") + (Wall ? "  <color=#D6AA4E>near wall</color>" : "");
        }

        /// <summary>
        /// The victim moves for a reaction, then the same moves near a wall when they can turn
        /// into a wall hit (their CanWallHit or CanWallHitFall window). The game's own wall rule
        /// then picks WallHit or WallHitFall, as it does in a fight.
        /// </summary>
        private List<LabVictimOption> LabVictimOptions(string reaction)
        {
            var candidates = LabReactionCandidates(reaction);
            var options = candidates.Select(c => new LabVictimOption { Move = c.move, Type = c.type }).ToList();
            foreach (var candidate in candidates)
                if (labBaseline.TryGetValue(candidate.move, out var move) && move.Intervals.Any(i => i.Name == "CanWallHit" || i.Name == "CanWallHitFall"))
                    options.Add(new LabVictimOption { Move = candidate.move, Type = candidate.type, Wall = true });
            return options;
        }

        /// <summary>The victim option previewed for an attack: the chosen one for the selected attack, else the first.</summary>
        private LabVictimOption LabReactionOption(LabAttackView attack)
        {
            var options = LabVictimOptions(attack.State.Hit);
            if (options.Count == 0) return null;
            int choice = attack.Id == labAttackId ? Mathf.Clamp(labReactionChoice, 0, options.Count - 1) : 0;
            return options[choice];
        }

        private bool PlayLabVictim(LabVictimOption option)
        {
            if (option == null || labVictim == null) return false;
            labVictim.SetWallBehind(option.Wall ? labWallGap : (float?)null);
            labVictim.ResetPosition();
            // PlayMove shows the reaction's first frame even while the preview is held.
            return labVictim.PlayMove(option.Move);
        }

        private void PreviewLabReaction(LabAttackView attack)
        {
            var option = LabReactionOption(attack);
            if (option == null || labVictim == null) { SetStatus("No victim move answers this reaction."); return; }
            if (!PlayLabVictim(option)) SetStatus("The victim cannot play " + option.Move + ".");
        }

        // ---- Inspector ----

        private void BuildLabInspectorFrame(RectTransform area)
        {
            var tabs = LabArea(area, "Tabs", new Vector2(0, 1), new Vector2(1, 1), new Vector2(8, -36), new Vector2(-8, -6), null);
            var row = LabHBox(tabs, 0, 2, null); Stretch(row);
            foreach (var (tab, text) in new[] { (LabTab.Move, "1  MOVE"), (LabTab.Attack, "2  ATTACK"), (LabTab.Windows, "3  WINDOWS"), (LabTab.Hitbox, "4  HITBOX") })
            {
                var captured = tab;
                labTabButtons.Add((LabBtn(row, text, () => SetLabTab(captured), -1, tab == labTab ? LabStyle.On : LabStyle.Quiet, 11), tab));
            }
            LabArea(area, "Rule", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -38), new Vector2(0, -37), LabLine);
            var scroll = LabArea(area, "Fields", Vector2.zero, Vector2.one, new Vector2(0, 0), new Vector2(0, -39), null);
            labInspector = LabScroll(scroll, 6);
            labInspector.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(12, 12, 10, 16);
        }

        private void SetLabTab(LabTab tab)
        {
            labTab = tab;
            labPickingClip = false;
            foreach (var (button, value) in labTabButtons) SetLabOn(button, value == tab);
            RefreshLabInspector();
        }

        private void RefreshLabInspector()
        {
            if (labInspector == null) return;
            for (int i = labInspector.childCount - 1; i >= 0; i--) Destroy(labInspector.GetChild(i).gameObject);
            if (labPreview != null) labPreview.EdgeOverlay = labTab == LabTab.Hitbox ? LabEdgeColor : null;
            var baseline = LabBaselineOf(labMove);
            if (baseline == null) { LabNoteText(labInspector, "No move selected."); return; }
            var entry = LabEntry;
            var fork = labCopy.FindFork(labMove);

            var title = LabText(labInspector, LabDisplayName(labMove), 18, LabFore, TextAnchor.MiddleLeft); LabSize(title, -1, 26);
            LabNoteText(labInspector, fork == null ? LabDescribeUsers(baseline)
                : fork.Add ? "A new move based on " + fork.Move + ", for " + (fork.Subtype != null ? ClassName(fork.Subtype) : fork.Item ?? "the same fighters as " + fork.Move) +
                    ". It plays " + fork.Move + "'s animation with its own input and edits; " + fork.Move + " itself is unchanged."
                : "A copy of " + fork.Move + " for " + (fork.Subtype != null ? ClassName(fork.Subtype) : fork.Item) + ". The original is unchanged for everyone else.");
            if (entry != null && entry.Disable)
            {
                LabNoteText(labInspector, "<color=#DE543E>This move is disabled: fighters can no longer pick it.</color>");
                LabBtn(labInspector, "ENABLE AGAIN", () => LabEdit((copy, target) => copy.SetDisabled(target, false)), -1, LabStyle.Primary, 12);
                return;
            }
            switch (labTab)
            {
                case LabTab.Move: BuildLabMoveTab(baseline, entry, fork); break;
                case LabTab.Attack: BuildLabAttackTab(baseline); break;
                case LabTab.Windows: BuildLabWindowsTab(baseline); break;
                case LabTab.Hitbox: BuildLabHitboxTab(baseline); break;
            }
        }

        private void BuildLabMoveTab(MovesetBaselineMove baseline, ModMovesetMove entry, ModMovesetFork fork)
        {
            if (labPickingClip) { BuildLabClipPicker(baseline); return; }
            BuildLabInputSection(baseline, entry);
            LabSection(labInspector, "PLAYBACK");
            double rate = entry?.PlaybackRate?.Value ?? 1.0;
            if (baseline.Looped || baseline.Physics) LabNoteText(labInspector, "Speed is fixed: looped and physics moves keep their timing.");
            else LabNumber(labInspector, "Speed", rate, 1.0, "0.00", .05, Eclipse.Runtime.PlaybackTiming.Minimum / 1000.0, baseline.MaxRatePermille / 1000.0,
                value => LabEdit((copy, target) => copy.SetPlaybackRate(target, Math.Round(value * 20) / 20)), "x", "Playback multiplier. 1.00 is the original speed; the most this clip allows is " + (baseline.MaxRatePermille / 1000.0).ToString("0.00", CultureInfo.InvariantCulture) + "x.");
            int priority = entry?.Priority?.Value ?? baseline.Priority;
            LabNumber(labInspector, "Priority", priority, baseline.Priority, "0", 1, 0, 100000,
                value => LabEdit((copy, target) => copy.SetPriority(target, baseline.Priority, (int)Math.Round(value))), null, "Which move wins when several could start: higher wins.");

            LabSection(labInspector, "ANIMATION");
            string clipName = entry?.Animation?.NativeValue ?? (entry?.Animation?.AssetValue != null ? "mod asset " + entry.Animation.AssetValue : baseline.File);
            LabField(labInspector, "Clip", clipName, entry?.Animation != null);
            LabField(labInspector, "Keyframes", baseline.FirstFrame + "–" + LabLastFrame(baseline) + "  (" + (LabLastFrame(baseline) - baseline.FirstFrame + 1) + ")", false);
            if (entry?.Animation != null) LabField(labInspector, "Original", baseline.File, false);
            if (entry?.Animation?.AssetValue == null)
            {
                var row = LabHBox(labInspector, 26, 6, null);
                LabBtn(row, "CHANGE CLIP...", () => { labPickingClip = true; labClipChoice = null; RefreshLabInspector(); }, -1, LabStyle.Normal, 12);
                if (entry?.Animation != null) LabBtn(row, "RESTORE ORIGINAL", () => LabEdit((copy, target) => copy.SetNativeAnimation(target, baseline.File, baseline.File)), -1, LabStyle.Normal, 12);
            }

            LabSection(labInspector, "MOVE");
            var actions = LabHBox(labInspector, 26, 6, null);
            if (fork != null) LabBtn(actions, fork.Add ? "DELETE THIS MOVE" : "DELETE THIS COPY", () => LabEdit((copy, target) => copy.RemoveFork(target)), -1, LabStyle.Danger, 12);
            else
            {
                if (entry != null) LabBtn(actions, "RESET TO BASE GAME", () => LabEdit((copy, target) =>
                {
                    var current = copy.Entry(target, false);
                    if (current != null) copy.Document.Moves.Remove(current);
                }), -1, LabStyle.Normal, 12);
                LabBtn(actions, "DISABLE MOVE", () => LabEdit((copy, target) => copy.SetDisabled(target, true)), -1, LabStyle.Danger, 12);
            }
        }

        private void BuildLabAttackTab(MovesetBaselineMove baseline)
        {
            var attack = LabSelectedAttack(baseline);
            if (attack == null) { LabNoteText(labInspector, "This move has no attack. Frame windows are in WINDOWS."); return; }
            LabAttackChooser(baseline, attack);
            var interval = attack.Interval;
            var state = attack.State;
            var original = interval.Attack;
            int last = LabLastFrame(baseline);

            LabSection(labInspector, "TIMING");
            LabNumber(labInspector, "Starts at frame", state.Start, interval.Start, "0", 1, 0, state.End, value => LabEditAttack(interval, s => s.Start = Mathf.Clamp((int)Math.Round(value), 0, s.End)), null, "First keyframe that can hit. Drag the bar's left edge on the timeline too.");
            LabNumber(labInspector, "Ends at frame", state.End, interval.End ?? interval.Start, "0", 1, state.Start, last, value => LabEditAttack(interval, s => s.End = Mathf.Clamp((int)Math.Round(value), s.Start, last)), null, "Last keyframe that can hit.");

            LabSection(labInspector, "DAMAGE");
            LabNumber(labInspector, "Base damage", state.Damage, original.Damage, "0.000", .005, 0, 16, value => LabEditAttack(interval, s => s.Damage = Math.Round(value, 3)), null,
                "The attack's damage value (0–16). Vanilla hits sit around 0.06–0.45; fighter attributes and blocking scale it.");
            foreach (string term in new[] { "WeaponDamage", "RangedDamage", "MagicDamage", "UnarmedDamage" }.Where(t => state.Terms.ContainsKey(t)))
            {
                string captured = term;
                original.Terms.TryGetValue(term, out double baseShift);
                LabNumber(labInspector, LabSplitWords(term) + " shift", state.Terms[term], baseShift, "0", 1, -1000, 1000, value => LabEditAttack(interval, s => s.Terms[captured] = Math.Round(value)), null,
                    "Added to the attacker's " + LabSplitWords(term).ToLowerInvariant() + " attribute before it is weighed against the defender's defence." +
                    (term == "RangedDamage" || term == "MagicDamage" ? " This term makes the attack unblockable." : ""));
            }

            LabSection(labInspector, "HIT REACTION");
            if (original.Hit == null) LabNoteText(labInspector, "This attack uses several reactions depending on the frame it lands (" + (original.HasPartialHits ? "partial hits" : "multiple hits") + "). Edit them in Lua.");
            else
            {
                var hitRow = LabHBox(labInspector, 26, 6, null);
                var caption = LabText(hitRow, (state.Hit != original.Hit ? "<color=#D6AA4E>●</color> " : "") + "Reaction", 12, LabDim, TextAnchor.MiddleLeft); LabSize(caption, 118);
                Button picker = null;
                picker = LabBtn(hitRow, state.Hit, () => OpenLabChoices(picker.transform as RectTransform, ModMoveAttack.NativeHitReactions.Select(name => new LabChoice
                {
                    Label = name, Detail = LabReactionCandidates(name).FirstOrDefault().move ?? "no victim move", Current = name == state.Hit,
                    Pick = () => { labReactionChoice = 0; LabEditAttack(interval, s => s.Hit = name); },
                }).ToList(), 300), -1, LabStyle.Normal, 12);
                LabChevron(picker.transform);
                if (state.Hit != original.Hit) LabBtn(hitRow, "RESET", () => LabEditAttack(interval, s => s.Hit = original.Hit), 50, LabStyle.Quiet, 10);
                var candidates = LabVictimOptions(state.Hit);
                if (candidates.Count == 0) LabNoteText(labInspector, "No victim move listens for this reaction.");
                else
                {
                    int choice = Mathf.Clamp(labReactionChoice, 0, candidates.Count - 1);
                    var victimRow = LabHBox(labInspector, 26, 6, null);
                    var victimCaption = LabText(victimRow, "Victim plays", 12, LabDim, TextAnchor.MiddleLeft); LabSize(victimCaption, 118);
                    LabBtn(victimRow, "<", () => { labReactionChoice = (choice - 1 + candidates.Count) % candidates.Count; RefreshLabInspector(); PreviewLabReaction(attack); }, 26, LabStyle.Normal, 12);
                    var shown = LabText(victimRow, candidates[choice].Label + "  <color=#625B57>" + (choice + 1) + "/" + candidates.Count + "</color>", 12, LabFore, TextAnchor.MiddleCenter); LabSize(shown, -1, -1, 1);
                    LabBtn(victimRow, ">", () => { labReactionChoice = (choice + 1) % candidates.Count; RefreshLabInspector(); PreviewLabReaction(attack); }, 26, LabStyle.Normal, 12);
                    LabBtn(victimRow, "PLAY", () => PreviewLabReaction(attack), 48, LabStyle.Primary, 11);
                    if (candidates[choice].Wall)
                        LabNumber(labInspector, "Wall behind victim", labWallGap, null, "0", 5, 0, 1000, value => { labWallGap = (float)value; RefreshLabInspector(); PreviewLabReaction(attack); }, null,
                            "Distance from the victim's start to the wall behind it. The game turns the recoil into WallHit or WallHitFall if the victim reaches the wall during the move's wall-hit window. Preview only, not saved.");
                    LabNoteText(labInspector, "The game picks among these by the victim's state (blocking, critical hits, stance) and whether a wall is behind it. Browse them to see each one.");
                }
            }

            LabSection(labInspector, "KNOCKBACK");
            string[] axes = { "Push X (away)", "Push Y (up)", "Push Z" };
            for (int axis = 0; axis < Math.Min(3, state.Impulse.Length); axis++)
            {
                int captured = axis;
                LabNumber(labInspector, axes[axis], state.Impulse[axis], original.Impulse.Length > axis ? original.Impulse[axis] : 0, "0.0", 5, -100000, 100000,
                    value => LabEditAttack(interval, s => s.Impulse[captured] = Math.Round(value, 2)), null, axis == 0 ? "Impulse given to the victim on hit." : null);
            }

            LabSection(labInspector, "HITBOX");
            LabNoteText(labInspector, state.Edges.Count + " part" + (state.Edges.Count == 1 ? "" : "s") + " hit: " + string.Join(", ", state.Edges.Select(LabEdgeLabel)));
            LabBtn(labInspector, "EDIT HITBOX", () => SetLabTab(LabTab.Hitbox), -1, LabStyle.Normal, 12);
        }

        private void LabAttackChooser(MovesetBaselineMove baseline, LabAttackView selected)
        {
            var attacks = LabAttacks(baseline);
            if (attacks.Count < 2) { LabSection(labInspector, "ATTACK " + selected.Id + "   ·   FRAMES " + selected.State.Start + "–" + selected.State.End); return; }
            LabSection(labInspector, "ATTACKS");
            var row = LabHBox(labInspector, 26, 4, null);
            foreach (var attack in attacks)
            {
                var captured = attack;
                LabBtn(row, "#" + attack.Id + "  " + attack.State.Start + "–" + attack.State.End, () => { labAttackId = captured.Id; labReactionChoice = 0; RefreshLabInspector(); RefreshLabTimeline(); }, -1,
                    attack.Id == selected.Id ? LabStyle.On : LabStyle.Normal, 11);
            }
        }

        private void BuildLabWindowsTab(MovesetBaselineMove baseline)
        {
            int last = LabLastFrame(baseline);
            var windows = LabIntervals(baseline).Where(i => i.Baseline?.Attack == null).ToList();
            LabSection(labInspector, "FRAME WINDOWS");
            LabNoteText(labInspector, "Frames where the fighter can block, be thrown, is invulnerable or cannot be interrupted. Drag the bars on the timeline, or type exact frames here.");
            if (windows.Count == 0) LabNoteText(labInspector, "This move has no frame windows.");
            for (int index = 0; index < windows.Count; index++)
            {
                var item = windows[index];
                int captured = index;
                bool selected = index == labWindowIndex;
                var card = LabVBox(labInspector, 3, new RectOffset(8, 8, 6, 6));
                var back = card.gameObject.AddComponent<Image>(); back.color = selected ? new Color(Gold.r, Gold.g, Gold.b, .12f) : new Color(1, 1, 1, .03f);
                var head = LabHBox(card, 22, 6, null);
                string state = item.Removed ? "  <color=#DE543E>removed</color>" : item.Added != null ? "  <color=#D6AA4E>added</color>" : LabWindowChanged(item) ? "  <color=#D6AA4E>edited</color>" : "";
                var name = LabText(head, item.Label + state, 13, item.Removed ? LabFaint : LabFore, TextAnchor.MiddleLeft); LabSize(name, -1, -1, 1);
                var focus = LabBtn(head, "SELECT", () => { labWindowIndex = captured; RefreshLabInspector(); RefreshLabTimeline(); }, 54, selected ? LabStyle.On : LabStyle.Quiet, 10);
                if (item.Removed)
                {
                    LabBtn(head, "RESTORE", () => LabEdit((copy, target) => copy.SetIntervalRemoved(target, item.Baseline, false)), 64, LabStyle.Normal, 10);
                    continue;
                }
                if (item.Added != null) LabBtn(head, "DELETE", () => LabEdit((copy, target) => copy.RemoveAddedInterval(target, item.Added)), 56, LabStyle.Danger, 10);
                else LabBtn(head, "REMOVE", () => LabEdit((copy, target) => copy.SetIntervalRemoved(target, item.Baseline, true)), 60, LabStyle.Danger, 10);
                LabNumber(card, "Start", item.Start, item.Baseline?.Start, "0", 1, 0, item.End ?? last, value => LabSetWindow(baseline, item, (int)Math.Round(value), item.End), null, null);
                if (item.End.HasValue)
                    LabNumber(card, "End", item.End.Value, item.Baseline?.End, "0", 1, item.Start, last, value => LabSetWindow(baseline, item, item.Start, (int)Math.Round(value)), null, null);
                else LabField(card, "End", "runs to the end of the move", false);
            }

            LabSection(labInspector, "ADD A WINDOW");
            var row = LabHBox(labInspector, 26, 6, null);
            Button type = null;
            type = LabBtn(row, LabAddTypes[labAddType], () => OpenLabChoices(type.transform as RectTransform, LabAddTypes.Select((name, i) => new LabChoice
            {
                Label = name, Current = i == labAddType, Pick = () => { labAddType = i; RefreshLabInspector(); },
            }).ToList(), 180), 130, LabStyle.Normal, 12);
            LabChevron(type.transform);
            int start = LabAddStart(baseline);
            LabBtn(row, "ADD AT FRAME " + start, () => LabEdit((copy, target) => copy.AddInterval(target, LabAddTypes[labAddType], string.Empty, start, Math.Min(last, start + 3))), -1, LabStyle.Primary, 12);
            LabNoteText(labInspector, "New windows start at the playhead and last four frames.");
        }

        private static bool LabWindowChanged(LabInterval item) => item.Baseline != null && (item.Start != item.Baseline.Start || item.End != item.Baseline.End);

        private void LabSetWindow(MovesetBaselineMove baseline, LabInterval item, int start, int? end)
        {
            int last = LabLastFrame(baseline);
            start = Mathf.Clamp(start, 0, last);
            if (end.HasValue) end = Mathf.Clamp(end.Value, start, last);
            if (item.Added != null)
            {
                var added = item.Added;
                LabEdit((copy, target) => { copy.RemoveAddedInterval(target, added); copy.AddInterval(target, added.AddType, added.AddName, start, end); });
            }
            else LabEdit((copy, target) => copy.SetIntervalBounds(target, item.Baseline, start, end));
        }

        private int LabAddStart(MovesetBaselineMove baseline)
        {
            int frame = LabIsPlaying ? labPreview.Keyframe : baseline.FirstFrame;
            return Mathf.Clamp(frame, 0, LabLastFrame(baseline));
        }

        // ---- Hitbox ----

        private struct LabEdgeKind
        {
            public string Group, Part, Variant;
            public int Side;
            public bool Brace;
        }

        private static readonly string[] LabArmParts = { "Clavicle", "Forearm", "Arm", "Wrist", "Hand", "Knuckles", "Fingers" };
        private static readonly string[] LabLegParts = { "Thigh", "Calf", "Shin", "Heel", "Heep", "Foot", "Instep", "Toe" };
        private static readonly string[] LabCoreParts = { "Head", "Neck", "Chest", "Stomach", "Pelvis", "Groin", "Spine" };

        /// <summary>
        /// Groups an edge for the editor. Side _1/_2 is structural (mirrored moves swap it);
        /// body parts come from the skeleton's edge naming; edges without a collision radius
        /// are rig braces, hidden unless asked for.
        /// </summary>
        private static LabEdgeKind ClassifyLabEdge(string name, bool body, float radius)
        {
            var kind = new LabEdgeKind { Brace = body && radius <= 0f };
            string stem = System.Text.RegularExpressions.Regex.Replace(name, "CI\\d+$", "");
            if (stem.EndsWith("_1", StringComparison.Ordinal)) { kind.Side = 1; stem = stem.Substring(0, stem.Length - 2); }
            else if (stem.EndsWith("_2", StringComparison.Ordinal)) { kind.Side = 2; stem = stem.Substring(0, stem.Length - 2); }
            if (!body) { kind.Group = "Weapon"; kind.Part = name; kind.Variant = ""; return kind; }
            if (stem.StartsWith("E", StringComparison.Ordinal)) stem = stem.Substring(1);
            string part = LabArmParts.Concat(LabLegParts).Concat(LabCoreParts).Where(p => stem.StartsWith(p, StringComparison.Ordinal)).OrderByDescending(p => p.Length).FirstOrDefault();
            kind.Part = part ?? stem;
            kind.Variant = part == null ? "" : stem.Substring(part.Length);
            string side = kind.Side == 0 ? "" : " · side " + kind.Side;
            kind.Group = part == null ? "Other" : LabArmParts.Contains(part) ? "Arm" + side : LabLegParts.Contains(part) ? "Leg" + side : "Head and torso";
            return kind;
        }

        private string LabEdgeLabel(string name)
        {
            var info = labPreview != null ? labPreview.Edges().FirstOrDefault(e => e.Name == name) : default;
            if (info.Name == null) return name;
            var kind = ClassifyLabEdge(name, info.Body, info.Radius);
            return info.Body ? kind.Part + (kind.Variant.Length != 0 ? " " + kind.Variant : "") + (kind.Side != 0 ? " " + kind.Side : "") : name;
        }

        private HashSet<string> LabSelectedEdges()
        {
            var baseline = LabBaselineOf(labMove);
            var attack = baseline == null ? null : LabSelectedAttack(baseline);
            return attack == null ? new HashSet<string>() : new HashSet<string>(attack.State.Edges, StringComparer.Ordinal);
        }

        private Color? LabEdgeColor(string name)
        {
            if (name == labHoverEdge) return new Color(1f, .82f, .3f, 1f);
            if (labHitboxSelection != null && labHitboxSelection.Contains(name)) return new Color(1f, .24f, .18f, .95f);
            if (labHitboxCandidates != null && labHitboxCandidates.Contains(name)) return new Color(.75f, .75f, .8f, .28f);
            return null;
        }

        private HashSet<string> labHitboxSelection, labHitboxCandidates;

        private void BuildLabHitboxTab(MovesetBaselineMove baseline)
        {
            var attack = LabSelectedAttack(baseline);
            labHitboxSelection = null; labHitboxCandidates = null;
            if (attack == null) { LabNoteText(labInspector, "This move has no attack, so nothing hits."); return; }
            LabAttackChooser(baseline, attack);
            var interval = attack.Interval;
            var selected = new HashSet<string>(attack.State.Edges, StringComparer.Ordinal);
            labHitboxSelection = selected;
            LabNoteText(labInspector, "The parts that deal this attack's hit. Click a part on the fighter, or a chip below. Red parts hit; hover to find a part.");

            var tools = LabHBox(labInspector, 26, 6, null);
            LabBtn(tools, "MIRROR SIDES", () => LabEditAttack(interval, s =>
            {
                s.Edges = s.Edges.Select(e => e.EndsWith("_1", StringComparison.Ordinal) ? e.Substring(0, e.Length - 2) + "_2" : e.EndsWith("_2", StringComparison.Ordinal) ? e.Substring(0, e.Length - 2) + "_1" : e).Distinct().ToList();
            }), -1, LabStyle.Normal, 11);
            bool changed = !attack.State.Edges.SequenceEqual(interval.Attack.Edges);
            if (changed) LabBtn(tools, "RESET HITBOX", () => LabEditAttack(interval, s => s.Edges = interval.Attack.Edges.ToList()), -1, LabStyle.Normal, 11);
            LabBtn(tools, "SHOW BRACES", () => { labShowBracing = !labShowBracing; RefreshLabInspector(); }, -1, labShowBracing ? LabStyle.On : LabStyle.Quiet, 11,
                "Rig brace edges have no collision size; they rarely make sense as hitting parts.");

            var edges = labPreview != null ? labPreview.Edges() : new List<VersusFighterPreview.EdgeInfo>();
            if (edges.Count == 0) { LabNoteText(labInspector, "Waiting for the fighter..."); return; }
            var known = new HashSet<string>(edges.Select(e => e.Name), StringComparer.Ordinal);
            var groups = new List<(string group, List<(string name, LabEdgeKind kind)> items)>();
            foreach (var edge in edges)
            {
                var kind = ClassifyLabEdge(edge.Name, edge.Body, edge.Radius);
                if (kind.Brace && !labShowBracing && !selected.Contains(edge.Name)) continue;
                var bucket = groups.FirstOrDefault(g => g.group == kind.Group);
                if (bucket.items == null) { bucket = (kind.Group, new List<(string, LabEdgeKind)>()); groups.Add(bucket); }
                bucket.items.Add((edge.Name, kind));
            }
            labHitboxCandidates = new HashSet<string>(groups.SelectMany(g => g.items.Select(i => i.name)), StringComparer.Ordinal);
            string[] order = { "Weapon", "Arm · side 1", "Arm · side 2", "Leg · side 1", "Leg · side 2", "Head and torso", "Arm", "Leg", "Other" };
            groups.Sort((a, b) => { int x = Array.IndexOf(order, a.group), y = Array.IndexOf(order, b.group); return (x < 0 ? 99 : x).CompareTo(y < 0 ? 99 : y); });
            foreach (var (group, items) in groups)
            {
                int on = items.Count(i => selected.Contains(i.name));
                LabSection(labInspector, group.ToUpperInvariant() + (on > 0 ? "   <color=#DE543E>" + on + " hitting</color>" : ""));
                var grid = Rect(labInspector, group);
                var layout = grid.gameObject.AddComponent<GridLayoutGroup>(); layout.cellSize = new Vector2(124, 24); layout.spacing = new Vector2(4, 4);
                layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount; layout.constraintCount = 3;
                LabSize(grid, -1, (items.Count + 2) / 3 * 28);
                foreach (var (name, kind) in items.OrderBy(i => Array.IndexOf(LabArmParts.Concat(LabLegParts).Concat(LabCoreParts).ToArray(), i.kind.Part)).ThenBy(i => i.kind.Variant.Length).ThenBy(i => i.name, StringComparer.Ordinal))
                {
                    string captured = name;
                    string text = kind.Group == "Weapon" ? name : kind.Part + (kind.Variant.Length != 0 ? " <color=#988F86>" + kind.Variant + "</color>" : "") + (kind.Brace ? " <color=#625B57>brace</color>" : "");
                    var chip = LabBtn(grid, text, () => ToggleLabEdge(captured), -1, selected.Contains(name) ? LabStyle.Danger : LabStyle.Normal, 11, name);
                    LabHover(chip.gameObject, () => labHoverEdge = captured, () => { if (labHoverEdge == captured) labHoverEdge = null; });
                }
            }
            var missing = selected.Where(name => !known.Contains(name)).ToList();
            if (missing.Count > 0)
            {
                LabSection(labInspector, "NOT ON THIS FIGHTER");
                LabNoteText(labInspector, "These parts belong to another weapon or rig. They still hit when a fighter that has them uses the move.");
                foreach (string name in missing) { string captured = name; LabBtn(labInspector, name + "   (remove)", () => ToggleLabEdge(captured), -1, LabStyle.Danger, 11); }
            }
        }

        private void ToggleLabEdge(string edge)
        {
            var baseline = LabBaselineOf(labMove);
            var attack = baseline == null ? null : LabSelectedAttack(baseline);
            if (attack == null) return;
            if (attack.State.Edges.Count == 1 && attack.State.Edges[0] == edge) { SetStatus("An attack needs at least one hitting part."); return; }
            LabEditAttack(attack.Interval, s =>
            {
                if (s.Edges.Contains(edge)) { if (s.Edges.Count > 1) s.Edges.Remove(edge); }
                else s.Edges.Add(edge);
            });
        }

        private void UpdateLabHitboxHover()
        {
            if (labHoverCaption == null) return;
            if (labTab != LabTab.Hitbox || labPreview == null || labAttackerStage == null) { labHoverCaption.text = ""; return; }
            var canvas = labAttackerStage.GetComponentInParent<Canvas>();
            var camera = canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 mouse = Eclipse.Input.EclipseInput.mousePosition;
            bool over = RectTransformUtility.RectangleContainsScreenPoint(labAttackerStage, mouse, camera);
            if (over) labHoverEdge = labPreview.EdgeAt(mouse, camera, 16f);
            string hover = labHoverEdge;
            labHoverCaption.text = hover == null ? "Click a part to make it hit" :
                hover + "   " + (labHitboxSelection != null && labHitboxSelection.Contains(hover) ? "<color=#DE543E>hits</color> · click to remove" : "click to add");
        }

        // ---- Animation picker ----

        private RectTransform labClipRows, labClipInfo;
        private InputField labClipFilter;
        private const int LabClipRowLimit = 160;

        private void BuildLabClipPicker(MovesetBaselineMove baseline)
        {
            LabSection(labInspector, "CHANGE CLIP");
            LabNoteText(labInspector, "Click a clip to watch it on the fighter, then USE THIS CLIP. The move keeps its frame windows, attacks and sounds, which may need moving to fit the new clip.");
            var filterRow = Rect(labInspector, "Clip filter"); LabSize(filterRow, -1, 26);
            labClipFilter = LabInput(filterRow, "", "Search clips", InputField.ContentType.Standard);
            labClipFilter.characterLimit = 64;
            labClipFilter.onValueChanged.AddListener(_ => RefreshLabClipRows(baseline));
            labClipInfo = LabVBox(labInspector, 3, null);
            var buttons = LabHBox(labInspector, 26, 6, null);
            LabBtn(buttons, "USE THIS CLIP", () => UseLabClip(baseline, labClipChoice), -1, LabStyle.Primary, 12);
            LabBtn(buttons, "ORIGINAL", () => UseLabClip(baseline, baseline.File), -1, LabStyle.Normal, 12);
            LabBtn(buttons, "CANCEL", () => { labPickingClip = false; RefreshLabInspector(); }, -1, LabStyle.Quiet, 12);
            labClipRows = LabVBox(labInspector, 1, null);
            RefreshLabClipInfo(baseline);
            RefreshLabClipRows(baseline);
        }

        private void RefreshLabClipRows(MovesetBaselineMove baseline)
        {
            if (labClipRows == null) return;
            for (int i = labClipRows.childCount - 1; i >= 0; i--) Destroy(labClipRows.GetChild(i).gameObject);
            string filter = labClipFilter != null ? labClipFilter.text.Trim() : string.Empty;
            var clips = LabClipOrder(out int scoped);
            string current = LabEntry?.Animation?.NativeValue ?? baseline.File;
            int shown = 0, matches = 0;
            for (int i = 0; i < clips.Count; i++)
            {
                string clip = clips[i];
                if (filter.Length > 0 && clip.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) continue;
                matches++;
                if (shown >= LabClipRowLimit) continue;
                if (shown == 0 && i < scoped && filter.Length == 0) LabSection(labClipRows, "USED IN THIS VIEW");
                if (i == scoped && filter.Length == 0) LabSection(labClipRows, "ALL CLIPS");
                string captured = clip;
                var row = LabBtn(labClipRows, clip + (clip == current ? "   <color=#D6AA4E>current</color>" : ""), () => PreviewLabClip(baseline, captured), -1, clip == labClipChoice ? LabStyle.On : LabStyle.Quiet, 12);
                var text = row.GetComponentInChildren<Text>(); text.alignment = TextAnchor.MiddleLeft; text.rectTransform.offsetMin = new Vector2(8, 0);
                LabSize(row, -1, 22);
                shown++;
            }
            if (matches > shown) LabNoteText(labClipRows, (matches - shown) + " more. Type part of a name to search all " + clips.Count + " clips.");
            if (matches == 0) LabNoteText(labClipRows, "No clip matches.");
        }

        private void PreviewLabClip(MovesetBaselineMove baseline, string clip)
        {
            labClipChoice = clip;
            RefreshLabClipInfo(baseline);
            RefreshLabClipRows(baseline);
            // Watch the clip through a native move that plays it, preferring one from this view.
            var view = LabMoves();
            var player = labBaseline.Values.Where(m => m.File == clip).OrderBy(m => view.Contains(m.Name) ? 0 : 1).ThenBy(m => m.Name, StringComparer.Ordinal).FirstOrDefault();
            if (player == null || labPreview == null) return;
            labUserPaused = false;
            if (labPreview.PlayMove(player.Name)) { labPlaying = player.Name; labPlayedAt = Time.unscaledTime; }
        }

        private void RefreshLabClipInfo(MovesetBaselineMove baseline)
        {
            if (labClipInfo == null) return;
            for (int i = labClipInfo.childCount - 1; i >= 0; i--) Destroy(labClipInfo.GetChild(i).gameObject);
            if (labClipChoice == null) { LabNoteText(labClipInfo, "No clip chosen yet."); return; }
            int frames = LabClipFrames(labClipChoice);
            LabField(labClipInfo, labClipChoice, frames >= 0 ? Math.Max(0, frames - baseline.FirstFrame) + " keyframes for this move" : "", false);
            var problems = LabClipProblems(baseline, labClipChoice, out bool blocked);
            if (problems.Count == 0) LabNoteText(labClipInfo, "Every attack and frame window fits inside this clip.");
            foreach (string problem in problems.Take(6)) LabNoteText(labClipInfo, "<color=#DE543E>" + (blocked ? "Cannot use: " : "Check: ") + "</color>" + problem);
            if (problems.Count > 6) LabNoteText(labClipInfo, "...and " + (problems.Count - 6) + " more.");
        }

        private void UseLabClip(MovesetBaselineMove baseline, string clip)
        {
            if (clip == null) { SetStatus("Click a clip first."); return; }
            LabClipProblems(baseline, clip, out bool blocked);
            if (blocked && clip != baseline.File) { SetStatus("That clip is too short for this move."); return; }
            labPickingClip = false;
            LabEdit((copy, target) => copy.SetNativeAnimation(target, baseline.File, clip));
            RefreshLabInspector();
        }

        // ---- Timeline ----

        private void BuildLabTimelineFrame(RectTransform area)
        {
            var header = LabArea(area, "Header", new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -22), new Vector2(-10, -2), null);
            var heading = LabText(header, "TIMELINE", 11, LabDim, TextAnchor.MiddleLeft); heading.rectTransform.anchorMax = new Vector2(0, 1); heading.rectTransform.offsetMax = new Vector2(LabLaneLabelWidth - 10, 0);
            labFrameReadout = LabText(header, "", 12, LabFore, TextAnchor.MiddleLeft);
            labFrameReadout.horizontalOverflow = HorizontalWrapMode.Overflow;
            labFrameReadout.rectTransform.anchorMax = new Vector2(0, 1); labFrameReadout.rectTransform.offsetMin = new Vector2(LabLaneLabelWidth - 10, 0); labFrameReadout.rectTransform.offsetMax = new Vector2(LabLaneLabelWidth + 220, 0);
            labTimelineHint = LabText(header, LabTimelineIdleHint, 11, LabFaint, TextAnchor.MiddleRight);
            labTimelineHint.horizontalOverflow = HorizontalWrapMode.Overflow;
            labTimelineHint.rectTransform.offsetMin = new Vector2(LabLaneLabelWidth + 230, 0);
            var body = LabArea(area, "Body", Vector2.zero, Vector2.one, new Vector2(0, 4), new Vector2(-8, -24), null);
            labRuler = LabArea(body, "Ruler", new Vector2(0, 1), new Vector2(1, 1), new Vector2(LabLaneLabelWidth, -LabRulerHeight), Vector2.zero, LabWell);
            LabScrub(labRuler);
            var lanes = LabArea(body, "Lanes", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0, -LabRulerHeight - 2), null);
            labTimelineLanes = LabScroll(lanes, 2);
            labTimelineLanes.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(0, 0, 0, 0);
            labPlayheadLayer = LabArea(body, "Playhead layer", Vector2.zero, Vector2.one, new Vector2(LabLaneLabelWidth, 0), Vector2.zero, null);
            labPlayhead = LabArea(labPlayheadLayer, "Playhead", new Vector2(0, 0), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(1, 0), Gold);
            labPlayhead.GetComponent<Image>().raycastTarget = false;
        }

        private const string LabTimelineIdleHint = "Drag the ruler to scrub · drag bars to move, ends to resize";

        private float LabFrameX(int frame, float width) => (frame - labTimelineFrom) * width / Mathf.Max(1, labTimelineTo - labTimelineFrom);

        private int LabFrameAt(RectTransform track, PointerEventData data)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(track, data.position, data.pressEventCamera ?? data.enterEventCamera, out var local);
            float t = (local.x - track.rect.xMin) / Mathf.Max(1f, track.rect.width);
            return labTimelineFrom + Mathf.FloorToInt(Mathf.Clamp01(t) * (labTimelineTo - labTimelineFrom - .001f));
        }

        private void LabScrub(RectTransform track)
        {
            if (track.GetComponent<Image>() == null) { var image = track.gameObject.AddComponent<Image>(); image.color = new Color(0, 0, 0, 0); }
            LabOn(track.gameObject, EventTriggerType.PointerDown, data => LabSeek(LabFrameAt(track, data)));
            LabOn(track.gameObject, EventTriggerType.Drag, data => LabSeek(LabFrameAt(track, data)));
        }

        private void RefreshLabTimeline()
        {
            if (labTimelineLanes == null) return;
            for (int i = labTimelineLanes.childCount - 1; i >= 0; i--) Destroy(labTimelineLanes.GetChild(i).gameObject);
            for (int i = labRuler.childCount - 1; i >= 0; i--) Destroy(labRuler.GetChild(i).gameObject);
            var baseline = LabBaselineOf(labMove);
            if (baseline == null) return;
            var intervals = LabIntervals(baseline);
            int last = LabLastFrame(baseline);
            labTimelineFrom = Math.Min(baseline.FirstFrame, intervals.Count == 0 ? baseline.FirstFrame : intervals.Min(i => i.Start));
            labTimelineTo = Math.Max(last, intervals.Count == 0 ? 0 : intervals.Max(i => i.End ?? i.Start)) + 1;
            Canvas.ForceUpdateCanvases();
            float width = labRuler.rect.width > 0 ? labRuler.rect.width : 480f;
            int frames = labTimelineTo - labTimelineFrom;
            float cell = width / Mathf.Max(1, frames);
            int labelEvery = cell >= 22 ? 1 : cell >= 11 ? 2 : cell >= 5 ? 5 : 10;
            for (int frame = labTimelineFrom; frame < labTimelineTo; frame++)
            {
                bool label = (frame - labTimelineFrom) % labelEvery == 0;
                var tick = LabArea(labRuler, "Tick", new Vector2(0, 0), new Vector2(0, label ? .45f : .25f), new Vector2(LabFrameX(frame, width), 0), new Vector2(LabFrameX(frame, width) + 1, 0), LabLine);
                tick.GetComponent<Image>().raycastTarget = false;
                if (frame < baseline.FirstFrame || frame > last) { var shade = LabArea(labRuler, "Outside", Vector2.zero, new Vector2(0, 1), new Vector2(LabFrameX(frame, width), 0), new Vector2(LabFrameX(frame + 1, width), 0), new Color(0, 0, 0, .35f)); shade.GetComponent<Image>().raycastTarget = false; }
                if (!label) continue;
                var number = LabText(labRuler, frame.ToString(CultureInfo.InvariantCulture), 10, LabDim, TextAnchor.UpperCenter);
                number.rectTransform.anchorMin = new Vector2(0, 0); number.rectTransform.anchorMax = new Vector2(0, 1);
                number.rectTransform.offsetMin = new Vector2(LabFrameX(frame, width), 0); number.rectTransform.offsetMax = new Vector2(LabFrameX(frame + 1, width) + 12, 0);
                number.alignment = TextAnchor.UpperLeft; number.rectTransform.offsetMin += new Vector2(2, 0);
            }

            var attackLookup = LabAttacks(baseline).ToDictionary(a => a.Interval, a => a);
            int windowIndex = 0;
            foreach (var item in intervals)
            {
                bool attack = item.Baseline?.Attack != null;
                var view = attack ? attackLookup[item.Baseline] : null;
                int index = attack ? -1 : windowIndex++;
                int start = attack ? view.State.Start : item.Start;
                int? end = attack ? view.State.End : item.End;
                bool selected = attack ? view.Id == labAttackId || labAttackId < 0 && view == attackLookup.Values.FirstOrDefault() : index == labWindowIndex;
                bool edited = attack ? labCopy.AttackEdit(labMove, view.Id) != null : item.Added != null || item.Removed || LabWindowChanged(item);

                var lane = Rect(labTimelineLanes, item.Label); LabSize(lane, -1, LabLaneHeight);
                var laneBack = lane.gameObject.AddComponent<Image>(); laneBack.color = selected ? new Color(Gold.r, Gold.g, Gold.b, .08f) : new Color(1, 1, 1, .02f);
                var name = LabText(lane, (edited ? "<color=#D6AA4E>●</color> " : "") + (attack ? "Attack #" + view.Id : item.Label), 11, item.Removed ? LabFaint : attack ? LabHot : LabFore, TextAnchor.MiddleLeft);
                name.rectTransform.anchorMax = new Vector2(0, 1); name.rectTransform.offsetMin = new Vector2(8, 0); name.rectTransform.offsetMax = new Vector2(LabLaneLabelWidth - 4, 0);
                var track = LabArea(lane, "Track", Vector2.zero, Vector2.one, new Vector2(LabLaneLabelWidth, 0), new Vector2(0, 0), new Color(0, 0, 0, .18f));
                LabScrub(track);
                int shownEnd = end ?? labTimelineTo - 1;
                var bar = LabArea(track, "Bar", new Vector2(0, 0), new Vector2(0, 1), new Vector2(LabFrameX(start, width) + 1, 3), new Vector2(LabFrameX(shownEnd + 1, width) - 1, -3),
                    item.Removed ? new Color(1, 1, 1, .06f) : attack ? LabHot : item.Added != null ? Gold : LabWindow);
                var barImage = bar.GetComponent<Image>();
                if (selected && !item.Removed) { var outline = bar.gameObject.AddComponent<Outline>(); outline.effectColor = new Color(1, 1, 1, .85f); outline.effectDistance = new Vector2(1, -1); }
                var barText = LabText(bar, start + "–" + (end?.ToString(CultureInfo.InvariantCulture) ?? "end"), 10, attack ? Color.white : LabBg, TextAnchor.MiddleCenter);
                barText.horizontalOverflow = HorizontalWrapMode.Overflow;
                if (item.Removed) { barText.color = LabFaint; barText.text = "removed"; continue; }
                Action select = attack ? (Action)(() => { labAttackId = view.Id; labReactionChoice = 0; if (labTab != LabTab.Hitbox) labTab = LabTab.Attack; SetLabTab(labTab); RefreshLabTimeline(); })
                    : () => { labWindowIndex = index; SetLabTab(LabTab.Windows); RefreshLabTimeline(); };
                Action<int, int?> commit = attack ? (Action<int, int?>)((s, e) => LabEditAttack(item.Baseline, st => { st.Start = s; st.End = e ?? s; }))
                    : (s, e) => LabSetWindow(baseline, item, s, e);
                LabBarDrag(bar, track, LabDragMode.Move, start, end, last, width, barText, select, commit);
                var left = LabArea(bar, "Start handle", new Vector2(0, 0), new Vector2(0, 1), new Vector2(-3, 0), new Vector2(5, 0), new Color(1, 1, 1, .001f));
                LabBarDrag(left, track, LabDragMode.Start, start, end, last, width, barText, select, commit, bar);
                if (end.HasValue)
                {
                    var right = LabArea(bar, "End handle", new Vector2(1, 0), new Vector2(1, 1), new Vector2(-5, 0), new Vector2(3, 0), new Color(1, 1, 1, .001f));
                    LabBarDrag(right, track, LabDragMode.End, start, end, last, width, barText, select, commit, bar);
                }
                LabHover(bar.gameObject, () => { if (labTimelineHint != null) labTimelineHint.text = (attack ? "Attack #" + view.Id : item.Label) + "  frames " + start + "–" + (end?.ToString(CultureInfo.InvariantCulture) ?? "end") + "   drag to move, ends to resize"; },
                    () => { if (labTimelineHint != null) labTimelineHint.text = LabTimelineIdleHint; });
            }
            if (intervals.Count == 0) { var none = LabText(labTimelineLanes, "This move has no attacks or frame windows.", 12, LabFaint, TextAnchor.MiddleCenter); LabSize(none, -1, 40); }
        }

        /// <summary>Drag on a timeline bar (or one of its end handles); the edit is one undo step on release.</summary>
        private void LabBarDrag(RectTransform handle, RectTransform track, LabDragMode mode, int start, int? end, int last, float width, Text readout, Action select, Action<int, int?> commit, RectTransform bar = null)
        {
            bar = bar ?? handle;
            int pressFrame = 0, newStart = start;
            int? newEnd = end;
            bool dragging = false;
            LabOn(handle.gameObject, EventTriggerType.PointerDown, data => { pressFrame = LabFrameAt(track, data); });
            LabOn(handle.gameObject, EventTriggerType.BeginDrag, data => { dragging = true; });
            LabOn(handle.gameObject, EventTriggerType.Drag, data =>
            {
                int delta = LabFrameAt(track, data) - pressFrame;
                int length = (end ?? start) - start;
                switch (mode)
                {
                    case LabDragMode.Move:
                        newStart = Mathf.Clamp(start + delta, 0, (end.HasValue ? last - length : last));
                        newEnd = end.HasValue ? newStart + length : (int?)null;
                        break;
                    case LabDragMode.Start:
                        newStart = Mathf.Clamp(start + delta, 0, end ?? last);
                        newEnd = end;
                        break;
                    case LabDragMode.End:
                        newStart = start;
                        newEnd = Mathf.Clamp((end ?? start) + delta, start, last);
                        break;
                }
                int shownEnd = newEnd ?? labTimelineTo - 1;
                bar.offsetMin = new Vector2(LabFrameX(newStart, width) + 1, bar.offsetMin.y);
                bar.offsetMax = new Vector2(LabFrameX(shownEnd + 1, width) - 1, bar.offsetMax.y);
                readout.text = newStart + "–" + (newEnd?.ToString(CultureInfo.InvariantCulture) ?? "end");
                if (labTimelineHint != null) labTimelineHint.text = "frames " + readout.text + "   release to apply";
            });
            LabOn(handle.gameObject, EventTriggerType.EndDrag, data =>
            {
                dragging = false;
                if (newStart != start || newEnd != end) commit(newStart, newEnd);
                else RefreshLabTimeline();
            });
            LabOn(handle.gameObject, EventTriggerType.PointerClick, data => { if (!dragging && !data.dragging) select(); });
        }

        // ---- Popups ----

        private void CloseLabPopup()
        {
            if (labPopup != null) Destroy(labPopup.gameObject);
            labPopup = null;
            backAction = LeaveMovesetLab;
        }

        private RectTransform OpenLabLayer(bool dim)
        {
            CloseLabPopup();
            labPopup = Rect(labRoot, "Popup"); Stretch(labPopup);
            var shade = labPopup.gameObject.AddComponent<Image>(); shade.color = new Color(0, 0, 0, dim ? .55f : .001f); shade.raycastTarget = true;
            var close = labPopup.gameObject.AddComponent<Button>(); close.transition = Selectable.Transition.None; close.navigation = new Navigation { mode = Navigation.Mode.None };
            close.onClick.AddListener(CloseLabPopup);
            backAction = CloseLabPopup;
            return labPopup;
        }

        /// <summary>A dropdown list under <paramref name="anchor"/>, with a search field for long lists.</summary>
        private void OpenLabChoices(RectTransform anchor, List<LabChoice> items, float width)
        {
            if (items.Count == 0) return;
            var layer = OpenLabLayer(false);
            bool search = items.Count > 12;
            float height = Mathf.Min(420f, items.Count * 25f + (search ? 34f : 0f) + 10f);
            width = Mathf.Max(width, anchor.rect.width);
            var corners = new Vector3[4];
            anchor.GetWorldCorners(corners);
            Vector2 topLeft = layer.InverseTransformPoint(corners[1]), bottomLeft = layer.InverseTransformPoint(corners[0]);
            var bounds = layer.rect;
            float x = Mathf.Clamp(bottomLeft.x, bounds.xMin + 4, bounds.xMax - width - 4);
            float y = bottomLeft.y - 2 - height >= bounds.yMin + 4 ? bottomLeft.y - 2 : topLeft.y + 2 + height;
            var card = Rect(layer, "Choices");
            card.anchorMin = card.anchorMax = new Vector2(.5f, .5f); card.pivot = new Vector2(0, 1);
            card.anchoredPosition = new Vector2(x - bounds.center.x, y - bounds.center.y);
            card.sizeDelta = new Vector2(width, height);
            var back = card.gameObject.AddComponent<Image>(); back.color = LabRaised; back.raycastTarget = true;
            card.gameObject.AddComponent<Outline>().effectColor = new Color(0, 0, 0, .6f);
            RectTransform rows = null;
            InputField filter = null;
            if (search)
            {
                var filterRect = LabArea(card, "Filter", new Vector2(0, 1), new Vector2(1, 1), new Vector2(6, -32), new Vector2(-6, -6), null);
                filter = LabInput(filterRect, "", "Type to filter", InputField.ContentType.Standard);
            }
            var list = LabArea(card, "List", Vector2.zero, Vector2.one, new Vector2(4, 4), new Vector2(-4, search ? -36 : -4), null);
            rows = LabScroll(list, 1);
            void Fill()
            {
                for (int i = rows.childCount - 1; i >= 0; i--) Destroy(rows.GetChild(i).gameObject);
                string text = filter != null ? filter.text.Trim() : "";
                foreach (var item in items)
                {
                    if (text.Length > 0 && item.Label.IndexOf(text, StringComparison.OrdinalIgnoreCase) < 0 && (item.Detail ?? "").IndexOf(text, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    var captured = item;
                    var row = LabBtn(rows, (item.Current ? "<color=#D6AA4E>" + item.Label + "</color>" : item.Label), () => { CloseLabPopup(); captured.Pick(); }, -1, item.Current ? LabStyle.On : LabStyle.Quiet, 12);
                    LabSize(row, -1, 24);
                    var label = row.GetComponentInChildren<Text>(); label.alignment = TextAnchor.MiddleLeft; label.rectTransform.offsetMin = new Vector2(8, 0);
                    if (!string.IsNullOrEmpty(item.Detail)) { var detail = LabText(row.transform, item.Detail, 10, LabFaint, TextAnchor.MiddleRight); detail.rectTransform.offsetMax = new Vector2(-8, 0); }
                }
            }
            Fill();
            if (filter != null) { filter.onValueChanged.AddListener(_ => Fill()); filter.Select(); filter.ActivateInputField(); }
        }

        private void ShowLabNewMod()
        {
            if (!LabMayDiscard("start a new mod")) return;
            var layer = OpenLabLayer(true);
            var card = LabModal(layer, "NEW MOVESET MOD", 460, 196);
            LabNoteText(card, "Its folder and ID are made from the name, for example \"Heavy Katana\" becomes local.heavy-katana.");
            var fieldRect = Rect(card, "Name"); LabSize(fieldRect, -1, 28);
            var field = LabInput(fieldRect, "", "Mod name", InputField.ContentType.Standard);
            field.characterLimit = 48;
            field.onSubmit.AddListener(text => CreateLabMod(text));
            var buttons = LabHBox(card, 28, 6, null);
            LabSpacer(buttons, 0, 1);
            LabBtn(buttons, "CANCEL", CloseLabPopup, 90, LabStyle.Quiet, 12);
            LabBtn(buttons, "CREATE", () => CreateLabMod(field.text), 90, LabStyle.Primary, 12);
            field.Select(); field.ActivateInputField();
        }

        private void ShowLabScopePrompt(string move, List<(string label, Func<string> target)> choices, Action<MovesetWorkingCopy, string> change)
        {
            var layer = OpenLabLayer(true);
            var card = LabModal(layer, "WHO GETS THIS CHANGE?", 500, 120 + choices.Count * 34);
            LabNoteText(card, LabDisplayName(move) + " is used by more than this view. A copy changes only the chosen fighters; the original stays for everyone else.");
            foreach (var choice in choices)
            {
                var captured = choice;
                LabSize(LabBtn(card, choice.label, () => { CloseLabPopup(); LabRunScopedEdit(captured.target, change); }, -1, LabStyle.Normal, 13), -1, 28);
            }
            LabSize(LabBtn(card, "CANCEL", CloseLabPopup, -1, LabStyle.Quiet, 12), -1, 26);
        }

        private RectTransform LabModal(RectTransform layer, string title, float width, float height)
        {
            var card = Rect(layer, "Card");
            card.anchorMin = card.anchorMax = card.pivot = new Vector2(.5f, .5f); card.sizeDelta = new Vector2(width, height);
            var back = card.gameObject.AddComponent<Image>(); back.color = LabPanel; back.raycastTarget = true;
            card.gameObject.AddComponent<Outline>().effectColor = new Color(Gold.r, Gold.g, Gold.b, .4f);
            var body = LabVBox(card, 8, new RectOffset(18, 18, 14, 14)); Stretch(body);
            var heading = LabText(body, title, 16, Gold, TextAnchor.MiddleLeft); LabSize(heading, -1, 24);
            UiReveal.Play(card, 0f, .18f, new Vector2(0, -10), .97f);
            return body;
        }

        // ---- Widgets ----

        private Text LabText(Transform parent, string text, int size, Color color, TextAnchor alignment)
        {
            var label = Label(parent, text, size, color, alignment);
            label.supportRichText = true;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        private void LabNoteText(RectTransform parent, string text)
        {
            var label = LabText(parent, text, 12, LabDim, TextAnchor.UpperLeft);
            label.lineSpacing = 1.05f;
        }

        private void LabSection(RectTransform parent, string text)
        {
            var rect = Rect(parent, "Section"); LabSize(rect, -1, 26);
            var label = LabText(rect, text, 11, Gold, TextAnchor.LowerLeft);
            var rule = LabArea(rect, "Rule", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, -2), new Vector2(0, -1), new Color(Gold.r, Gold.g, Gold.b, .25f));
            rule.GetComponent<Image>().raycastTarget = false;
        }

        /// <summary>A read-only caption and value row.</summary>
        private void LabField(RectTransform parent, string caption, string value, bool changed)
        {
            var row = LabHBox(parent, 22, 6, null);
            var name = LabText(row, (changed ? "<color=#D6AA4E>●</color> " : "") + caption, 12, LabDim, TextAnchor.MiddleLeft); LabSize(name, 118);
            var shown = LabText(row, value, 12, LabFore, TextAnchor.MiddleLeft); LabSize(shown, -1, -1, 1);
        }

        /// <summary>
        /// A number with − / + (Shift ×10, Ctrl ×0.1) and a typed field (Enter commits). A changed
        /// value shows a gold dot, its base-game value, and RESET.
        /// </summary>
        private void LabNumber(RectTransform parent, string caption, double value, double? baseline, string format, double step, double min, double max, Action<double> commit, string unit, string help)
        {
            bool changed = baseline.HasValue && Math.Abs(value - baseline.Value) > 1e-9;
            var block = LabVBox(parent, 1, null);
            var row = LabHBox(block, 26, 4, null);
            var name = LabText(row, (changed ? "<color=#D6AA4E>●</color> " : "") + caption, 12, changed ? LabFore : LabDim, TextAnchor.MiddleLeft); LabSize(name, -1, -1, 1);
            if (changed) LabBtn(row, "RESET", () => commit(baseline.Value), 46, LabStyle.Quiet, 10, "Back to the base-game value " + baseline.Value.ToString(format, CultureInfo.InvariantCulture));
            void Nudge(int direction)
            {
                bool shift = Eclipse.Input.EclipseInput.GetKey(KeyCode.LeftShift) || Eclipse.Input.EclipseInput.GetKey(KeyCode.RightShift);
                bool ctrl = Eclipse.Input.EclipseInput.GetKey(KeyCode.LeftControl) || Eclipse.Input.EclipseInput.GetKey(KeyCode.RightControl);
                double amount = step * (shift ? 10 : ctrl && step < 1 ? .1 : 1);
                double next = Math.Max(min, Math.Min(max, value + direction * amount));
                if (Math.Abs(next - value) > 1e-12) commit(next);
            }
            LabBtn(row, "−", () => Nudge(-1), 24, LabStyle.Normal, 14);
            var fieldRect = Rect(row, "Value"); LabSize(fieldRect, 78, 22);
            var field = LabInput(fieldRect, value.ToString(format, CultureInfo.InvariantCulture), "", InputField.ContentType.Standard);
            field.textComponent.alignment = TextAnchor.MiddleCenter;
            field.textComponent.color = changed ? Gold : LabFore;
            field.onEndEdit.AddListener(text =>
            {
                if (!double.TryParse((text ?? "").Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double typed))
                { field.text = value.ToString(format, CultureInfo.InvariantCulture); SetStatus("Not a number: " + text); return; }
                typed = Math.Max(min, Math.Min(max, typed));
                if (Math.Abs(typed - value) > 1e-12) commit(typed);
                else field.text = value.ToString(format, CultureInfo.InvariantCulture);
            });
            LabBtn(row, "+", () => Nudge(1), 24, LabStyle.Normal, 14);
            var suffix = LabText(row, unit ?? "", 11, LabFaint, TextAnchor.MiddleLeft); LabSize(suffix, 14);
            if (changed || !string.IsNullOrEmpty(help))
            {
                string text = (changed ? "base " + baseline.Value.ToString(format, CultureInfo.InvariantCulture) + (string.IsNullOrEmpty(help) ? "" : "   ·   ") : "") + (help ?? "");
                var note = LabText(block, text, 10, LabFaint, TextAnchor.UpperLeft);
            }
        }

        private InputField LabInput(RectTransform rect, string value, string placeholder, InputField.ContentType type)
        {
            var image = rect.gameObject.AddComponent<Image>(); image.color = LabWell; image.raycastTarget = true;
            rect.gameObject.AddComponent<Outline>().effectColor = LabLine;
            var text = Label(rect, "", 13, LabFore, TextAnchor.MiddleLeft); text.supportRichText = false;
            text.rectTransform.offsetMin = new Vector2(8, 0); text.rectTransform.offsetMax = new Vector2(-8, 0);
            var hint = Label(rect, placeholder, 12, LabFaint, TextAnchor.MiddleLeft); hint.fontStyle = FontStyle.Italic;
            hint.rectTransform.offsetMin = new Vector2(8, 0); hint.rectTransform.offsetMax = new Vector2(-8, 0);
            var field = rect.gameObject.AddComponent<InputField>();
            field.textComponent = text; field.placeholder = hint; field.targetGraphic = image;
            field.lineType = InputField.LineType.SingleLine; field.contentType = type; field.characterLimit = 32;
            field.customCaretColor = true; field.caretColor = Gold; field.selectionColor = new Color(Gold.r, Gold.g, Gold.b, .35f);
            field.navigation = new Navigation { mode = Navigation.Mode.None };
            field.text = value;
            return field;
        }

        private Button LabBtn(Transform parent, string text, Action action, float width, LabStyle style, int size, string tooltip = null)
        {
            var rect = Rect(parent, text.Length > 24 ? text.Substring(0, 24) : text);
            var element = rect.gameObject.AddComponent<LayoutElement>();
            if (width > 0) element.preferredWidth = element.minWidth = width; else element.flexibleWidth = 1;
            element.preferredHeight = 24;
            var back = rect.gameObject.AddComponent<Image>(); back.raycastTarget = true;
            var label = LabText(rect, text, size, LabFore, TextAnchor.MiddleCenter);
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = back;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            var colors = button.colors;
            colors.normalColor = Color.white; colors.highlightedColor = new Color(1.35f, 1.3f, 1.25f, 1f); colors.pressedColor = new Color(.8f, .8f, .8f, 1f);
            colors.selectedColor = Color.white; colors.disabledColor = new Color(1, 1, 1, .35f); colors.fadeDuration = .05f;
            button.colors = colors;
            StyleLabButton(button, style);
            button.onClick.AddListener(() => { EclipseUiAudio.Play(UiSound.Toggle); action(); });
            if (!string.IsNullOrEmpty(tooltip) && tooltip != text)
                LabHover(rect.gameObject, () => { if (status != null && labPopup == null) status.text = "<color=#988F86>" + tooltip + "</color>"; }, null);
            return button;
        }

        private static void StyleLabButton(Button button, LabStyle style)
        {
            var back = (Image)button.targetGraphic;
            var label = button.GetComponentInChildren<Text>();
            switch (style)
            {
                case LabStyle.Primary: back.color = Gold; label.color = LabBg; break;
                case LabStyle.Danger: back.color = new Color32(150, 46, 36, 255); label.color = LabFore; break;
                case LabStyle.Quiet: back.color = new Color(1, 1, 1, .035f); label.color = LabDim; break;
                case LabStyle.On: back.color = new Color(Gold.r, Gold.g, Gold.b, .26f); label.color = Gold; break;
                default: back.color = LabRaised; label.color = LabFore; break;
            }
        }

        private static void SetLabOn(Button button, bool on) { if (button != null) StyleLabButton(button, on ? LabStyle.On : LabStyle.Normal); }

        /// <summary>A small downward chevron at a button's right edge.</summary>
        private static void LabChevron(Transform button)
        {
            var clip = Rect(button, "Chevron");
            clip.anchorMin = clip.anchorMax = new Vector2(1, .5f); clip.pivot = new Vector2(1, .5f);
            clip.anchoredPosition = new Vector2(-7, -1); clip.sizeDelta = new Vector2(10, 5);
            clip.gameObject.AddComponent<RectMask2D>();
            var square = Rect(clip, "Mark");
            square.anchorMin = square.anchorMax = new Vector2(.5f, 1); square.sizeDelta = new Vector2(7, 7); square.localRotation = Quaternion.Euler(0, 0, 45);
            var image = square.gameObject.AddComponent<Image>(); image.color = LabDim; image.raycastTarget = false;
        }

        private static RectTransform LabHBox(Transform parent, float height, float spacing, RectOffset padding)
        {
            var rect = Rect(parent, "Row");
            var layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing; layout.padding = padding ?? new RectOffset();
            layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandWidth = false; layout.childForceExpandHeight = true;
            layout.childAlignment = TextAnchor.MiddleLeft;
            if (height > 0) LabSize(rect, -1, height);
            return rect;
        }

        private static RectTransform LabVBox(Transform parent, float spacing, RectOffset padding)
        {
            var rect = Rect(parent, "Column");
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing; layout.padding = padding ?? new RectOffset();
            layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
            return rect;
        }

        private static void LabSpacer(RectTransform row, float width, float flexible = 0)
        {
            var rect = Rect(row, "Space");
            var element = rect.gameObject.AddComponent<LayoutElement>(); element.preferredWidth = width; element.flexibleWidth = flexible;
        }

        private static void LabSize(Component component, float width, float height = -1, float flexibleWidth = -1)
        {
            var element = component.GetComponent<LayoutElement>() ?? component.gameObject.AddComponent<LayoutElement>();
            if (width > 0) element.preferredWidth = element.minWidth = width;
            if (height > 0) element.preferredHeight = element.minHeight = height;
            if (flexibleWidth >= 0) element.flexibleWidth = flexibleWidth;
        }

        private static string LabSplitWords(string value) => System.Text.RegularExpressions.Regex.Replace(value, "(?<=[a-z])(?=[A-Z])", " ");

        private RectTransform LabScroll(RectTransform area, float spacing)
        {
            var scroll = area.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 32f; scroll.inertia = false;
            var viewport = Rect(area, "Viewport"); Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            var hit = viewport.gameObject.AddComponent<Image>(); hit.color = new Color(0, 0, 0, 0); hit.raycastTarget = true;
            var content = Rect(viewport, "Content");
            content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.pivot = new Vector2(.5f, 1); content.sizeDelta = Vector2.zero;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing; layout.padding = new RectOffset(2, 2, 2, 2);
            layout.childControlHeight = layout.childControlWidth = true; layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport; scroll.content = content;
            return content;
        }

        /// <summary>
        /// Adds a pointer callback. EventTrigger takes every pointer event, so scroll wheel
        /// input is passed on to the enclosing scroll list.
        /// </summary>
        private static void LabOn(GameObject target, EventTriggerType type, Action<PointerEventData> action)
        {
            var trigger = target.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = target.AddComponent<EventTrigger>();
                var scroll = new EventTrigger.Entry { eventID = EventTriggerType.Scroll };
                scroll.callback.AddListener(data =>
                {
                    var parent = target.transform.parent != null ? target.transform.parent.GetComponentInParent<ScrollRect>() : null;
                    if (parent != null) parent.OnScroll((PointerEventData)data);
                });
                trigger.triggers.Add(scroll);
            }
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(data => action((PointerEventData)data));
            trigger.triggers.Add(entry);
        }

        private static void LabHover(GameObject target, Action enter, Action exit)
        {
            if (enter != null) LabOn(target, EventTriggerType.PointerEnter, _ => enter());
            if (exit != null) LabOn(target, EventTriggerType.PointerExit, _ => exit());
        }
    }
}
