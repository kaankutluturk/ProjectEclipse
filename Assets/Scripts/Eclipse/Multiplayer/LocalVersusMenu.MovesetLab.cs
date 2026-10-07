using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Eclipse.Modding;
using Eclipse.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Eclipse.Multiplayer
{
    // The Moveset Lab: browse moves by weapon subtype (or one weapon), watch them on a live
    // fighter with the attacking edges drawn, tune them, and save the result as a data-only
    // mod (movesets/moveset.json). Applying swaps the game's move overlay for the saved
    // edits, so the preview and "test in training" use exactly what the mod will load.
    public sealed partial class LocalVersusMenu
    {
        public const string MovesetLabDefaultMod = "local.moveset-lab";
        private const string LabShared = "";
        private const string LabUnarmed = "Fists";
        private static bool labOnEntry;

        private Dictionary<string, MovesetBaselineMove> labBaseline;
        private MovesetWorkingCopy labCopy;
        private string labModId = MovesetLabDefaultMod;
        private readonly List<string> labScopes = new List<string>();
        private int labScope;
        private readonly List<LabWeapon> labWeapons = new List<LabWeapon>();
        private int labWeapon = -1;
        private string labMove;
        private readonly HashSet<string> labWholeFamily = new HashSet<string>(StringComparer.Ordinal);
        private VersusFighterPreview labPreview;
        private RectTransform labList, labInspector, labTimeline, labPlayhead, labPrompt;
        private InputField labFilter;
        private Text labReadout, labScopeLabel, labWeaponLabel, labTitle;
        private bool labEdgesKnown;
        private bool labOverlayApplied, labTesting, labReturning, labLoop = true, labDiscardArmed;
        private float labReplayAt = -1f, labPlayedAt;
        private string labPlaying;
        private int labTimelineFrom, labTimelineTo;
        private int labAddType;
        private static readonly string[] LabAddTypes = { "Block", "Invulnerable", "Invisible", "Throwable" };

        private string labModName = "Moveset Lab";
        private bool labSwitchArmed;
        private Text labModLabel;
        private bool labPickingClip;
        private string labClipChoice;
        private RectTransform labClipRows, labClipInfo;
        private InputField labClipFilter;
        private List<string> labClips;
        private readonly Dictionary<string, int> labClipFrames = new Dictionary<string, int>(StringComparer.Ordinal);
        private const int LabClipRowLimit = 120;

        private sealed class LabWeapon { public string Id, RuntimeName, Name, Owner; }
        private sealed class LabMod { public string Id, Name; public bool Enabled; }

        private sealed class LabInterval
        {
            public MovesetBaselineInterval Baseline;
            public ModMoveIntervalEdit Added;
            public int Start;
            public int? End;
            public bool Removed;
            public string Label => Baseline?.Label ?? ((Added.AddType.Length != 0 ? Added.AddType : Added.AddName) + " (added)");
        }

        private sealed class LabAttack
        {
            public int Start, End;
            public double Damage;
            public Dictionary<string, double> Terms;
            public List<string> Edges;
            public double[] Impulse;
            public string Hit;
        }

        /// <summary>Opens the Moveset Lab when the multiplayer data next becomes ready (Mods screen shortcut).</summary>
        public static void OpenMovesetLabOnEntry() => labOnEntry = true;

        internal static bool ConsumeMovesetLabEntry()
        {
            bool open = labOnEntry;
            labOnEntry = false;
            return open;
        }

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
            labPrompt = null;
            RebuildScreen("MOVESET LAB", null, Hints("P K L", "Play, pause, step", "Z Y", "Undo, redo", "F5", "Apply", "Esc", "Back"), LeaveMovesetLab, content =>
            {
                // Left: scope, weapon, filter and the move list on dark glass.
                var browser = Place(content, "Browser", new Vector2(0, 1), new Vector2(0, 0), new Vector2(300, 510));
                var glass = browser.gameObject.AddComponent<Image>(); glass.color = GlassInk; glass.raycastTarget = true;
                labModLabel = LabCycler(browser, -10, LabModTitle, CycleLabMod, "NEW", NewLabMod);
                labScopeLabel = LabCycler(browser, -48, () => LabScopeName(), step => { labScope = (labScope + step + labScopes.Count) % labScopes.Count; OnLabScopeChanged(); });
                labWeaponLabel = LabCycler(browser, -86, () => LabWeaponName(), step => { if (labWeapons.Count == 0) return; labWeapon = (labWeapon + 1 + step + labWeapons.Count + 1) % (labWeapons.Count + 1) - 1; OnLabScopeChanged(); });
                var filterRect = Place(browser, "Filter", new Vector2(.5f, 1), new Vector2(0, -126), new Vector2(276, 32));
                labFilter = AddFilterField(filterRect);
                labFilter.onValueChanged.AddListener(_ => RefreshLabList());
                var scroll = Place(browser, "Moves", new Vector2(.5f, 1), new Vector2(0, -164), new Vector2(284, 338));
                labList = BuildScrollList(scroll);
                UiReveal.Play(browser, .04f, .34f, new Vector2(-30, 0), .97f);

                // Centre: the fighter, playback controls and the timeline.
                var stageCard = Place(content, "Stage", new Vector2(0, 1), new Vector2(312, 0), new Vector2(372, 510));
                var paper = stageCard.gameObject.AddComponent<PaperPanel>(); paper.color = Paper; paper.raycastTarget = true;
                labTitle = Label(stageCard, "", 18, Red, TextAnchor.MiddleCenter);
                Anchor(labTitle.rectTransform, new Vector2(.5f, 1), new Vector2(0, -8), new Vector2(350, 26));
                var stage = Place(stageCard, "Fighter", new Vector2(.5f, 1), new Vector2(0, -34), new Vector2(340, 250));
                labPreview = VersusFighterPreview.Create(stage, false);
                labPreview.ShowAttackEdges = true;
                labReadout = Label(stageCard, "", 15, Ink, TextAnchor.MiddleCenter);
                Anchor(labReadout.rectTransform, new Vector2(.5f, 1), new Vector2(0, -286), new Vector2(350, 22));
                var controls = Place(stageCard, "Controls", new Vector2(.5f, 1), new Vector2(0, -312), new Vector2(350, 36));
                var row = controls.gameObject.AddComponent<HorizontalLayoutGroup>(); row.spacing = 6; row.childControlWidth = row.childControlHeight = true; row.childForceExpandWidth = true;
                LabSmallButton(controls, "PLAY", LabPlay);
                LabSmallButton(controls, "PAUSE", LabTogglePause);
                LabSmallButton(controls, "STEP", () => LabStep(1));
                LabSmallButton(controls, "LOOP", () => { labLoop = !labLoop; SetStatus(labLoop ? "The move repeats." : "The move plays once."); });
                labTimeline = Place(stageCard, "Timeline", new Vector2(.5f, 0), new Vector2(0, 62), new Vector2(340, 92));
                var lane = labTimeline.gameObject.AddComponent<Image>(); lane.color = new Color(Ink.r, Ink.g, Ink.b, .1f); lane.raycastTarget = false;
                var actions = Place(stageCard, "Actions", new Vector2(.5f, 0), new Vector2(0, 12), new Vector2(350, 40));
                var actionRow = actions.gameObject.AddComponent<HorizontalLayoutGroup>(); actionRow.spacing = 6; actionRow.childControlWidth = actionRow.childControlHeight = true; actionRow.childForceExpandWidth = true;
                LabSmallButton(actions, "UNDO", () => LabHistory(true));
                LabSmallButton(actions, "REDO", () => LabHistory(false));
                var apply = LabSmallButton(actions, "APPLY", () => ApplyLab());
                apply.GetComponent<EclipseUiButton>()?.SetColors(Red, RedBright, Paper, Paper);
                LabSmallButton(actions, "TEST", TestLabInTraining);
                UiReveal.Play(stageCard, .08f, .36f, new Vector2(0, -24), .96f);

                // Right: the inspector.
                var inspector = Place(content, "Inspector", new Vector2(1, 1), new Vector2(0, 0), new Vector2(488, 510));
                var inspectorGlass = inspector.gameObject.AddComponent<Image>(); inspectorGlass.color = GlassInk; inspectorGlass.raycastTarget = true;
                var inspectorScroll = Place(inspector, "Fields", new Vector2(.5f, .5f), Vector2.zero, new Vector2(472, 494));
                labInspector = BuildScrollList(inspectorScroll);
                UiReveal.Play(inspector, .12f, .36f, new Vector2(30, 0), .97f);

                shortcuts.Add((KeyCode.P, LabPlay));
                shortcuts.Add((KeyCode.K, LabTogglePause));
                shortcuts.Add((KeyCode.L, () => LabStep(1)));
                shortcuts.Add((KeyCode.Z, () => LabHistory(true)));
                shortcuts.Add((KeyCode.Y, () => LabHistory(false)));
                shortcuts.Add((KeyCode.F5, () => ApplyLab()));
                shortcuts.Add((KeyCode.T, TestLabInTraining));
            });
            OnLabScopeChanged();
            SetStatus(labOverlayApplied ? "Your saved edits are applied." : "Pick a move. Edits apply to the preview after APPLY, which also saves " + labModName + " (" + labModId + ").");
        }

        /// <summary>Leaving the Lab drops its overlay; the game keeps the edits it started with.</summary>
        private void LeaveMovesetLab()
        {
            if (labCopy != null && labCopy.IsDirty && !labDiscardArmed)
            {
                labDiscardArmed = true;
                SetStatus("Unsaved edits. Press Esc again to discard them, or APPLY to save.");
                return;
            }
            if (labCopy != null && labCopy.IsDirty) labCopy = null;
            labDiscardArmed = false;
            ShowModeSelect();
        }

        /// <summary>Called whenever the multiplayer home opens: the Lab's overlay ends with the Lab.</summary>
        private void EndMovesetLab()
        {
            if (labReturning) return;
            labTesting = false;
            labPreview = null;
            if (!labOverlayApplied) return;
            labOverlayApplied = false;
            ModRuntime.ClearMoveOverlay();
        }

        private void LoadLabMod(string modId)
        {
            string manifest = System.IO.Path.Combine(ModRuntime.Host.ModsRoot, modId, "mod.toml");
            var copy = new MovesetWorkingCopy(modId, MovesetModWriter.Load(ModRuntime.Host.ModsRoot, modId));
            labModId = modId;
            labCopy = copy;
            if (System.IO.File.Exists(manifest)) labModName = ModManifestReader.ReadExternalFile(manifest).Name;
            labWholeFamily.Clear();
            labPickingClip = false;
        }

        // ---- Mods ----

        /// <summary>Data-only mods in the Mods folder the Lab can edit, plus the current one (maybe not saved yet).</summary>
        private List<LabMod> LabMods()
        {
            var mods = new List<LabMod>();
            string root = ModRuntime.Host.ModsRoot;
            ModSelection selection;
            try { selection = ModSelection.Load(ModHost.GetSelectionPath(root)); }
            catch (Exception) { selection = new ModSelection(); }
            if (System.IO.Directory.Exists(root))
                foreach (string folder in System.IO.Directory.GetDirectories(root))
                {
                    string path = System.IO.Path.Combine(folder, "mod.toml");
                    if (!System.IO.File.Exists(path)) continue;
                    try
                    {
                        var manifest = ModManifestReader.ReadExternalFile(path);
                        if (manifest.HasEntrypoint || mods.Any(m => m.Id == manifest.Id.Value)) continue;
                        mods.Add(new LabMod { Id = manifest.Id.Value, Name = manifest.Name, Enabled = selection.IsEnabled(manifest.Id) });
                    }
                    catch (Exception) { /* Broken manifests are listed in Mods > Details, not here. */ }
                }
            if (!mods.Any(m => m.Id == labModId)) mods.Add(new LabMod { Id = labModId, Name = labModName, Enabled = true });
            mods.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            return mods;
        }

        private string LabModTitle()
        {
            var current = LabMods().FirstOrDefault(m => m.Id == labModId);
            return (labModName ?? labModId).ToUpperInvariant() + (current != null && !current.Enabled ? "  (OFF)" : string.Empty);
        }

        /// <summary>True when unsaved edits may be dropped: the first try only warns.</summary>
        private bool LabMayDiscard(string action)
        {
            if (labCopy == null || !labCopy.IsDirty || labSwitchArmed) { labSwitchArmed = false; return true; }
            labSwitchArmed = true;
            SetStatus("Unsaved edits in " + labModName + ". Press again to discard them and " + action + ", or APPLY first.");
            return false;
        }

        private void CycleLabMod(int step)
        {
            var mods = LabMods();
            if (mods.Count < 2) { SetStatus("No other data-only mod to edit. NEW starts one."); return; }
            if (!LabMayDiscard("switch mods")) return;
            int index = mods.FindIndex(m => m.Id == labModId);
            var next = mods[(index + step + mods.Count) % mods.Count];
            try { LoadLabMod(next.Id); }
            catch (Exception exception) { SetStatus("Could not open " + next.Name + ": " + exception.Message); return; }
            labModName = next.Name;
            AfterLabModChanged(next.Enabled ? "Editing " + next.Name + " (" + next.Id + ")." :
                "Editing " + next.Name + ". It is switched off in Mods; switch it on to APPLY.");
        }

        private void NewLabMod()
        {
            if (!LabMayDiscard("start a new mod")) return;
            CloseLabPrompt();
            labPrompt = Rect(panel, "New mod prompt"); Stretch(labPrompt);
            var shade = labPrompt.gameObject.AddComponent<Image>(); shade.color = new Color(0, 0, 0, .55f); shade.raycastTarget = true;
            var card = Place(labPrompt, "Card", new Vector2(.5f, .5f), Vector2.zero, new Vector2(640, 280));
            var paper = card.gameObject.AddComponent<PaperPanel>(); paper.color = Paper; paper.raycastTarget = true;
            var title = Label(card, "NEW MOVESET MOD", 26, Red, TextAnchor.MiddleCenter);
            Anchor(title.rectTransform, new Vector2(.5f, 1), new Vector2(0, -16), new Vector2(600, 36));
            var caption = Label(card, "Its folder and ID are made from the name, for example \"Heavy Katana\" becomes local.heavy-katana.", 17, Ink, TextAnchor.MiddleCenter);
            caption.horizontalOverflow = HorizontalWrapMode.Wrap;
            Anchor(caption.rectTransform, new Vector2(.5f, 1), new Vector2(0, -54), new Vector2(580, 44));
            var list = Place(card, "Fields", new Vector2(.5f, 1), new Vector2(0, -108), new Vector2(580, 150));
            var layout = list.gameObject.AddComponent<VerticalLayoutGroup>(); layout.spacing = 10; layout.childControlHeight = layout.childControlWidth = true; layout.childForceExpandHeight = false;
            var field = AddTextField(list, "NAME", string.Empty, 48, "e.g. Heavy Katana", 380);
            var buttons = AddRow(list);
            AddButton(buttons, "CREATE", () => CreateLabMod(field.text), 0, UiSound.Begin);
            AddButton(buttons, "CANCEL", CloseLabPrompt, 0, UiSound.Back);
            UiReveal.Play(card, 0f, .25f, new Vector2(0, -14), .96f);
            field.Select(); field.ActivateInputField();
            backAction = CloseLabPrompt;
        }

        private void CreateLabMod(string name)
        {
            name = (name ?? string.Empty).Trim();
            if (name.Length == 0) { SetStatus("Type a name for the new mod."); return; }
            var slug = new System.Text.StringBuilder();
            foreach (char c in name.ToLowerInvariant())
            {
                bool keep = c >= 'a' && c <= 'z' || c >= '0' && c <= '9';
                if (keep) slug.Append(c);
                else if (slug.Length > 0 && slug[slug.Length - 1] != '-') slug.Append('-');
            }
            string baseId = "local." + slug.ToString().Trim('-');
            if (baseId.Length > 56) baseId = baseId.Substring(0, 56).TrimEnd('-');
            if (baseId == "local.") { SetStatus("Use at least one letter or digit in the name."); return; }
            string root = ModRuntime.Host.ModsRoot, id = baseId;
            for (int n = 2; System.IO.Directory.Exists(System.IO.Path.Combine(root, id)) || LabMods().Any(m => m.Id == id); n++) id = baseId + "-" + n;
            if (!ModId.TryParse(id, out _)) { SetStatus("'" + id + "' is not a valid mod ID."); return; }
            CloseLabPrompt();
            labModId = id;
            labModName = name;
            labCopy = new MovesetWorkingCopy(id);
            labWholeFamily.Clear();
            labPickingClip = false;
            AfterLabModChanged("New mod " + name + " (" + id + "). It is saved in the Mods folder on APPLY.");
        }

        private void AfterLabModChanged(string message)
        {
            labDiscardArmed = false;
            if (labModLabel != null) labModLabel.text = LabModTitle();
            var moves = LabMoves();
            if (labMove == null || !moves.Contains(labMove)) labMove = moves.FirstOrDefault();
            RefreshLab();
            SetStatus(message);
        }

        // ---- Scopes ----

        private void BuildLabScopes()
        {
            labScopes.Clear();
            labScopes.Add(LabShared);
            labScopes.Add(LabUnarmed);
            var subtypes = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var move in labBaseline.Values)
                foreach (var group in move.WeaponGroups)
                    foreach (string subtype in group)
                        if (subtype != LabUnarmed && LabPreviewWeapon(subtype) != null) subtypes.Add(subtype);
            labScopes.AddRange(subtypes);
            labScope = Mathf.Clamp(labScopes.IndexOf("Katana"), 1, labScopes.Count - 1);
        }

        private string LabSubtype => labScopes.Count == 0 ? LabShared : labScopes[labScope];
        private LabWeapon LabCurrentWeapon => labWeapon >= 0 && labWeapon < labWeapons.Count ? labWeapons[labWeapon] : null;
        private string LabScopeName() => LabSubtype == LabShared ? "SHARED (EVERY FIGHTER)" : LabSubtype == LabUnarmed ? "UNARMED" : ClassName(LabSubtype).ToUpperInvariant();
        private string LabWeaponName() => LabSubtype == LabShared || LabSubtype == LabUnarmed ? "-" : LabCurrentWeapon == null ? "WHOLE SUBTYPE" : LabCurrentWeapon.Name.ToUpperInvariant();

        private void OnLabScopeChanged()
        {
            labWeapons.Clear();
            if (LabSubtype != LabShared && LabSubtype != LabUnarmed) labWeapons.AddRange(LabWeaponsOf(LabSubtype));
            if (labWeapon >= labWeapons.Count) labWeapon = -1;
            if (labScopeLabel != null) labScopeLabel.text = LabScopeName();
            if (labWeaponLabel != null) labWeaponLabel.text = LabWeaponName();
            labPreview?.Show(LabLoadout(), true);
            labEdgesKnown = false;
            var moves = LabMoves();
            if (labMove == null || !moves.Contains(labMove)) labMove = moves.FirstOrDefault();
            RefreshLab();
            labReplayAt = Time.unscaledTime + .3f;
        }

        /// <summary>Weapons of a subtype, from core and enabled mods, that the preview can wear.</summary>
        private static List<LabWeapon> LabWeaponsOf(string subtype)
        {
            var result = new List<LabWeapon>();
            var content = ModRuntime.Scripts?.Content;
            if (content == null) return result;
            foreach (var weapon in content.Weapons)
            {
                if (weapon.SubType != subtype) continue;
                string runtime = weapon.IsCore && !string.IsNullOrEmpty(weapon.LegacyName) ? weapon.LegacyName : weapon.Id.ToString();
                if (ListSF.GetItems().GetItemByName(runtime) == null) continue;
                string name = VersusRoster.Find(LoadoutSlot.Weapon, runtime)?.Name ?? LocalizationManager.GetStringOrDefault(runtime, runtime);
                result.Add(new LabWeapon { Id = weapon.Id.ToString(), RuntimeName = runtime, Name = weapon.IsCore ? name : name + " (" + weapon.Id.Namespace.Value + ")", Owner = weapon.Id.Namespace.Value });
            }
            result.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return result;
        }

        /// <summary>A weapon of the subtype to preview with: the roster's first, else any item of it.</summary>
        private static string LabPreviewWeapon(string subtype)
        {
            if (subtype == LabShared || subtype == LabUnarmed) return GameUtils.GetDefaultItem("Weapon");
            foreach (var item in VersusRoster.Items(LoadoutSlot.Weapon)) if (item.SubType == subtype) return item.Id;
            foreach (var item in ListSF.GetItems().GetItemsByType("Weapon") ?? new List<ItemInfo>())
                if (item != null && item.SubType == subtype && !string.IsNullOrEmpty(item.ModelFileName) && Eclipse.Content.PackagedArtCatalog.HasModel(item.ModelFileName)) return item.Name;
            return null;
        }

        private VersusLoadout LabLoadout()
        {
            var basis = trainingPlayer ?? VersusLoadouts.Load(VersusLoadouts.PlayerOne);
            string weapon = LabCurrentWeapon?.RuntimeName ?? LabPreviewWeapon(LabSubtype) ?? GameUtils.GetDefaultItem("Weapon");
            return basis.With(LoadoutSlot.Weapon, weapon);
        }

        /// <summary>The moves this view's fighter uses, with this mod's forks in place of their sources.</summary>
        private List<string> LabMoves()
        {
            string subtype = LabSubtype;
            var weapon = LabCurrentWeapon;
            var result = new List<string>();
            foreach (var move in labBaseline.Values)
            {
                bool shared = move.WeaponGroups.Count == 0 && move.WeaponItems.Count == 0 && move.PlayerSkeleton;
                bool inSubtype = move.WeaponGroups.Any(group => group.Contains(subtype));
                if (subtype == LabShared ? shared : inSubtype || weapon != null && shared) result.Add(move.Name);
            }
            result.Sort(StringComparer.Ordinal);
            foreach (var fork in labCopy.Document.Forks)
            {
                bool applies = fork.Subtype != null ? fork.Subtype == subtype && subtype != LabShared : weapon != null && fork.Item == weapon.Id;
                if (!applies) continue;
                int index = result.IndexOf(fork.Move);
                if (index >= 0) result[index] = labCopy.ForkName(fork);
                else result.Add(labCopy.ForkName(fork));
            }
            return result;
        }

        private MovesetBaselineMove LabBaselineOf(string move) =>
            move != null && labBaseline.TryGetValue(labCopy.NativeSource(move), out var baseline) ? baseline : null;

        // ---- Move list ----

        private void RefreshLab()
        {
            RefreshLabList();
            RefreshLabInspector();
            RefreshLabTimeline();
        }

        private void RefreshLabList()
        {
            if (labList == null) return;
            for (int i = labList.childCount - 1; i >= 0; i--) Destroy(labList.GetChild(i).gameObject);
            string filter = labFilter != null ? labFilter.text.Trim() : string.Empty;
            foreach (string move in LabMoves())
            {
                if (filter.Length > 0 && move.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) continue;
                var captured = move;
                string badge = LabBadge(move);
                var edited = labCopy.Entry(move, false);
                string text = (edited != null && edited.HasEdits ? "<color=#D6AA4E>●</color> " : "") + LabDisplayName(move) +
                    (badge.Length != 0 ? "  <size=13><color=#C4B292>" + badge + "</color></size>" : "");
                var item = LabListItem(labList, text, move == labMove, () => { labMove = captured; labPickingClip = false; RefreshLab(); labReplayAt = Time.unscaledTime; });
                if (move == labMove && UnityEngine.EventSystems.EventSystem.current != null && !(labFilter != null && labFilter.isFocused))
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(item.gameObject);
            }
        }

        /// <summary>A fork by its local id (the runtime name adds the mod id in front).</summary>
        private string LabDisplayName(string move) => labCopy.FindFork(move)?.Id ?? move;

        private string LabBadge(string move)
        {
            var fork = labCopy.FindFork(move);
            if (fork != null) return fork.Subtype != null ? "subtype copy" : "weapon copy";
            var baseline = LabBaselineOf(move);
            if (baseline == null) return string.Empty;
            if (baseline.WeaponGroups.Count == 0) return "shared";
            var group = baseline.WeaponGroups.FirstOrDefault(g => g.Contains(LabSubtype));
            return group != null && group.Count > 1 ? "family of " + group.Count : string.Empty;
        }

        // ---- Edit targets ----

        /// <summary>
        /// Runs <paramref name="change"/> on the move the user means. Editing a native move that
        /// other subtypes or weapons also use asks first: everyone, this subtype, or this weapon.
        /// </summary>
        private void LabEdit(Action<MovesetWorkingCopy, string> change)
        {
            string move = labMove;
            if (move == null) return;
            var choices = LabScopeChoices(move);
            if (choices.Count <= 1) { LabApplyEdit(move, change); return; }
            ShowLabPrompt(move, choices, change);
        }

        private List<(string label, Func<string> target)> LabScopeChoices(string move)
        {
            var choices = new List<(string, Func<string>)>();
            var baseline = LabBaselineOf(move);
            if (labCopy.FindFork(move) != null || baseline == null || labWholeFamily.Contains(move) || labCopy.Entry(move, false) != null)
            {
                choices.Add(("EDIT", () => move));
                return choices;
            }
            var weapon = LabCurrentWeapon;
            var group = baseline.WeaponGroups.FirstOrDefault(g => g.Contains(LabSubtype));
            bool shared = baseline.WeaponGroups.Count == 0;
            string everyone = shared ? "EVERY FIGHTER" : group != null && group.Count > 1
                ? "WHOLE FAMILY (" + string.Join(", ", group.Select(ClassName)).ToUpperInvariant() + ")" : "ALL " + LabScopeName();
            choices.Add((everyone, () => { labWholeFamily.Add(move); return move; }));
            if (!shared && group != null && group.Count > 1)
                choices.Add(("ONLY " + LabScopeName() + " (COPY)", () => labCopy.ForkName(labCopy.CreateSubtypeFork(move, LabSubtype))));
            if (weapon != null)
                choices.Add(("ONLY " + weapon.Name.ToUpperInvariant() + " (COPY)", () => labCopy.ForkName(labCopy.CreateItemFork(move, weapon.Id))));
            return choices;
        }

        private void ShowLabPrompt(string move, List<(string label, Func<string> target)> choices, Action<MovesetWorkingCopy, string> change)
        {
            CloseLabPrompt();
            labPrompt = Rect(panel, "Scope prompt"); Stretch(labPrompt);
            var shade = labPrompt.gameObject.AddComponent<Image>(); shade.color = new Color(0, 0, 0, .55f); shade.raycastTarget = true;
            var card = Place(labPrompt, "Card", new Vector2(.5f, .5f), Vector2.zero, new Vector2(640, 150 + choices.Count * 56));
            var paper = card.gameObject.AddComponent<PaperPanel>(); paper.color = Paper; paper.raycastTarget = true;
            var title = Label(card, "WHO GETS THIS CHANGE?", 26, Red, TextAnchor.MiddleCenter);
            Anchor(title.rectTransform, new Vector2(.5f, 1), new Vector2(0, -16), new Vector2(600, 36));
            var caption = Label(card, move + " is used by more than this view. A copy changes only the chosen fighters; the original stays for everyone else.", 17, Ink, TextAnchor.MiddleCenter);
            caption.horizontalOverflow = HorizontalWrapMode.Wrap;
            Anchor(caption.rectTransform, new Vector2(.5f, 1), new Vector2(0, -54), new Vector2(580, 50));
            var list = Place(card, "Choices", new Vector2(.5f, 1), new Vector2(0, -112), new Vector2(580, choices.Count * 56));
            var layout = list.gameObject.AddComponent<VerticalLayoutGroup>(); layout.spacing = 8; layout.childControlHeight = layout.childControlWidth = true; layout.childForceExpandHeight = false;
            Button first = null;
            foreach (var choice in choices)
            {
                var captured = choice;
                var button = AddButton(list, choice.label, () =>
                {
                    CloseLabPrompt();
                    string target;
                    try { target = null; labCopy.Edit(copy => { target = captured.target(); change(copy, target); }); }
                    catch (Exception exception) { SetStatus(exception.Message); return; }
                    if (target != null) labMove = target;
                    AfterLabEdit();
                }, 0);
                if (first == null) first = button;
            }
            AddButton(list, "CANCEL", CloseLabPrompt, 0, UiSound.Back);
            UiReveal.Play(card, 0f, .25f, new Vector2(0, -14), .96f);
            if (first != null && UnityEngine.EventSystems.EventSystem.current != null) UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(first.gameObject);
            backAction = CloseLabPrompt;
        }

        private void CloseLabPrompt()
        {
            if (labPrompt != null) Destroy(labPrompt.gameObject);
            labPrompt = null;
            backAction = LeaveMovesetLab;
        }

        private void LabApplyEdit(string move, Action<MovesetWorkingCopy, string> change)
        {
            try { labCopy.Edit(copy => change(copy, move)); }
            catch (Exception exception) { SetStatus(exception.Message); return; }
            AfterLabEdit();
        }

        private void AfterLabEdit()
        {
            labDiscardArmed = false;
            RefreshLab();
            SetStatus("Edited. APPLY to save and see the change on the fighter.");
        }

        private void LabHistory(bool undo)
        {
            if (undo ? !labCopy.CanUndo : !labCopy.CanRedo) { SetStatus(undo ? "Nothing to undo." : "Nothing to redo."); return; }
            if (undo) labCopy.Undo(); else labCopy.Redo();
            if (labMove != null && LabBaselineOf(labMove) == null || !LabMoves().Contains(labMove)) labMove = LabMoves().FirstOrDefault();
            RefreshLab();
            SetStatus(undo ? "Undone." : "Redone.");
        }

        // ---- Effective values ----

        private ModMovesetMove LabEntry => labMove == null ? null : labCopy.Entry(labMove, false);

        private List<LabInterval> LabIntervals(MovesetBaselineMove baseline)
        {
            var entry = LabEntry;
            var result = new List<LabInterval>();
            foreach (var interval in baseline.Intervals)
            {
                var item = new LabInterval { Baseline = interval, Start = interval.Start, End = interval.End };
                if (interval.Attack != null)
                {
                    var edit = entry?.Attacks.FirstOrDefault(a => a.Id == interval.Attack.Id);
                    if (edit?.Start != null) item.Start = edit.Start.Value;
                    if (edit?.End != null) item.End = edit.End.Value;
                }
                else if (entry != null)
                    foreach (var edit in entry.Intervals)
                    {
                        if (edit.Select == null || !edit.Select.Equals(interval.Selector)) continue;
                        if (edit.Kind == ModMoveIntervalEditKind.Remove) item.Removed = true;
                        else { if (edit.Start.HasValue) item.Start = edit.Start.Value; if (edit.End.HasValue) item.End = edit.End; }
                    }
                result.Add(item);
            }
            if (entry != null)
                foreach (var edit in entry.Intervals)
                    if (edit.Kind == ModMoveIntervalEditKind.Add) result.Add(new LabInterval { Added = edit, Start = edit.Start ?? 0, End = edit.End });
            return result;
        }

        private LabAttack LabAttackState(MovesetBaselineInterval interval)
        {
            var baseline = interval.Attack;
            var edit = labCopy.AttackEdit(labMove, baseline.Id);
            return new LabAttack
            {
                Start = edit?.Start?.Value ?? interval.Start,
                End = edit?.End?.Value ?? interval.End ?? interval.Start,
                Damage = edit?.Damage?.Value ?? baseline.Damage,
                Terms = new Dictionary<string, double>(edit?.DamageTerms != null ? edit.DamageTerms.Value.ToDictionary(p => p.Key, p => p.Value) : baseline.Terms),
                Edges = (edit?.Edges?.Value ?? baseline.Edges).ToList(),
                Impulse = (edit?.Impulse?.Value ?? baseline.Impulse).ToArray(),
                Hit = edit?.Hit?.Value ?? baseline.Hit,
            };
        }

        private void LabEditAttack(MovesetBaselineInterval interval, Action<LabAttack> change)
        {
            LabEdit((copy, target) =>
            {
                string previous = labMove;
                labMove = target;
                var state = LabAttackState(interval);
                labMove = previous;
                change(state);
                copy.SetAttack(target, interval.Attack, state.Start, state.End, interval.Start, interval.End ?? interval.Start,
                    state.Damage, state.Terms, state.Edges, state.Impulse, state.Hit);
            });
        }

        /// <summary>The move's last keyframe, with a swapped native clip's own length.</summary>
        private int LabLastFrame(MovesetBaselineMove baseline)
        {
            string clip = LabEntry?.Animation?.NativeValue;
            int frames = clip != null ? LabClipFrames(clip) : -1;
            return frames > baseline.FirstFrame ? frames - 1 : baseline.FirstFrame + Math.Max(1, baseline.FrameCount) - 1;
        }

        // ---- Inspector ----

        private void RefreshLabInspector()
        {
            if (labInspector == null) return;
            for (int i = labInspector.childCount - 1; i >= 0; i--) Destroy(labInspector.GetChild(i).gameObject);
            var baseline = LabBaselineOf(labMove);
            if (labTitle != null) labTitle.text = labMove == null ? string.Empty : LabDisplayName(labMove).ToUpperInvariant();
            if (baseline == null) { LabHeading(labInspector, "No move selected"); return; }
            var entry = LabEntry;
            var fork = labCopy.FindFork(labMove);
            LabHeading(labInspector, LabDisplayName(labMove));
            LabNote(labInspector, fork != null
                ? "A copy of " + fork.Move + " for " + (fork.Subtype != null ? ClassName(fork.Subtype) : fork.Item) + ". The original is unchanged for everyone else."
                : LabDescribeUsers(baseline));
            if (entry != null && entry.Disable)
            {
                LabNote(labInspector, "This move is disabled: fighters can no longer pick it.");
                LabRowButton(labInspector, "ENABLE AGAIN", () => LabEdit((copy, target) => copy.SetDisabled(target, false)));
                return;
            }

            if (labPickingClip) { BuildLabClipPicker(baseline); return; }

            double rate = entry?.PlaybackRate?.Value ?? 1.0;
            int maxRate = baseline.MaxRatePermille;
            if (baseline.Looped || baseline.Physics) LabNote(labInspector, "Speed is fixed: looped and physics moves keep their timing.");
            else LabStepper(labInspector, "SPEED", rate.ToString("0.00", CultureInfo.InvariantCulture) + "x", step =>
                LabEdit((copy, target) => copy.SetPlaybackRate(target, Math.Round(Mathf.Clamp((float)(rate + step * .05), Eclipse.Runtime.PlaybackTiming.Minimum / 1000f, maxRate / 1000f) * 20) / 20)));
            int priority = entry?.Priority?.Value ?? baseline.Priority;
            LabStepper(labInspector, "PRIORITY", priority.ToString(CultureInfo.InvariantCulture), step =>
                LabEdit((copy, target) => copy.SetPriority(target, baseline.Priority, Mathf.Clamp(priority + step, 0, 100000))));
            string clipName = entry?.Animation?.NativeValue ?? (entry?.Animation?.AssetValue != null ? "mod asset " + entry.Animation.AssetValue : baseline.File);
            LabNote(labInspector, "Animation: " + clipName + "  (" + (LabLastFrame(baseline) - baseline.FirstFrame + 1) + " keyframes)" +
                (entry?.Animation != null ? "   — swapped from " + baseline.File : string.Empty));
            if (entry?.Animation?.AssetValue == null)
                LabRowButton(labInspector, "CHANGE ANIMATION", () => { labPickingClip = true; labClipChoice = null; RefreshLabInspector(); });

            var intervals = LabIntervals(baseline);
            int last = LabLastFrame(baseline);
            foreach (var item in intervals.Where(i => i.Baseline?.Attack != null))
            {
                var interval = item.Baseline;
                var state = LabAttackState(interval);
                LabHeading(labInspector, "ATTACK " + interval.Attack.Id + "   frames " + state.Start + "–" + state.End);
                LabStepper(labInspector, "DAMAGE", state.Damage.ToString("0.000", CultureInfo.InvariantCulture), step =>
                    LabEditAttack(interval, s => s.Damage = Math.Round(Math.Max(0, Math.Min(16, s.Damage + step * .005)), 3)));
                LabStepper(labInspector, "STARTS", state.Start.ToString(CultureInfo.InvariantCulture), step =>
                    LabEditAttack(interval, s => s.Start = Mathf.Clamp(s.Start + step, 0, s.End)));
                LabStepper(labInspector, "ENDS", state.End.ToString(CultureInfo.InvariantCulture), step =>
                    LabEditAttack(interval, s => s.End = Mathf.Clamp(s.End + step, s.Start, last)));
                if (interval.Attack.Hit != null)
                    LabStepper(labInspector, "HIT REACTION", state.Hit, step => LabEditAttack(interval, s =>
                    {
                        var names = ModMoveAttack.NativeHitReactions;
                        s.Hit = names[(Array.IndexOf(names, s.Hit) + step + names.Length) % names.Length];
                    }));
                else LabNote(labInspector, "Hit reaction: several per attack (edit them in Lua).");
                LabStepper(labInspector, "PUSH  X", state.Impulse[0].ToString("0.0", CultureInfo.InvariantCulture), step => LabEditAttack(interval, s => s.Impulse[0] = Math.Round(s.Impulse[0] + step * 5, 2)));
                LabStepper(labInspector, "PUSH  Y", state.Impulse[1].ToString("0.0", CultureInfo.InvariantCulture), step => LabEditAttack(interval, s => s.Impulse[1] = Math.Round(s.Impulse[1] + step * 5, 2)));
                LabEdges(interval, state);
            }

            LabHeading(labInspector, "FRAME WINDOWS");
            foreach (var item in intervals.Where(i => i.Baseline?.Attack == null))
            {
                var captured = item;
                string range = item.Start + "–" + (item.End?.ToString(CultureInfo.InvariantCulture) ?? "end");
                if (item.Removed)
                {
                    LabStepper(labInspector, item.Label + "  (removed)", range, null);
                    LabRowButton(labInspector, "RESTORE " + item.Label.ToUpperInvariant(), () => LabEdit((copy, target) => copy.SetIntervalRemoved(target, captured.Baseline, false)));
                    continue;
                }
                if (item.Added != null)
                {
                    LabStepper(labInspector, item.Label, range, null);
                    LabRowButton(labInspector, "DELETE ADDED WINDOW", () => LabEdit((copy, target) => copy.RemoveAddedInterval(target, captured.Added)));
                    continue;
                }
                LabStepper(labInspector, item.Label + " START", item.Start.ToString(CultureInfo.InvariantCulture), step =>
                    LabEdit((copy, target) => copy.SetIntervalBounds(target, captured.Baseline, Mathf.Clamp(captured.Start + step, 0, captured.End ?? last), captured.End)));
                if (item.End.HasValue)
                    LabStepper(labInspector, item.Label + " END", item.End.Value.ToString(CultureInfo.InvariantCulture), step =>
                        LabEdit((copy, target) => copy.SetIntervalBounds(target, captured.Baseline, captured.Start, Mathf.Clamp(captured.End.Value + step, captured.Start, last))));
                LabRowButton(labInspector, "REMOVE " + item.Label.ToUpperInvariant(), () => LabEdit((copy, target) => copy.SetIntervalRemoved(target, captured.Baseline, true)));
            }
            LabStepper(labInspector, "NEW WINDOW TYPE", LabAddTypes[labAddType], step => { labAddType = (labAddType + step + LabAddTypes.Length) % LabAddTypes.Length; RefreshLabInspector(); });
            LabRowButton(labInspector, "ADD " + LabAddTypes[labAddType].ToUpperInvariant() + " AT FRAME " + LabAddStart(baseline), () =>
            {
                int start = LabAddStart(baseline);
                LabEdit((copy, target) => copy.AddInterval(target, LabAddTypes[labAddType], string.Empty, start, Math.Min(last, start + 3)));
            });

            LabHeading(labInspector, "MOVE");
            if (fork != null) LabRowButton(labInspector, "DELETE THIS COPY", () => LabEdit((copy, target) => copy.RemoveFork(target)));
            else
            {
                if (entry != null) LabRowButton(labInspector, "RESET TO BASE GAME", () => LabEdit((copy, target) =>
                {
                    var current = copy.Entry(target, false);
                    if (current != null) copy.Document.Moves.Remove(current);
                }));
                LabRowButton(labInspector, "DISABLE MOVE", () => LabEdit((copy, target) => copy.SetDisabled(target, true)));
            }
        }

        private int LabAddStart(MovesetBaselineMove baseline)
        {
            int frame = labPreview != null && labPreview.PlayingMove == labPlaying ? labPreview.Keyframe : baseline.FirstFrame;
            return Mathf.Clamp(frame, 0, LabLastFrame(baseline));
        }

        private string LabDescribeUsers(MovesetBaselineMove baseline)
        {
            if (baseline.WeaponGroups.Count == 0) return "Shared: every fighter can use this move, armed or not.";
            var group = baseline.WeaponGroups.FirstOrDefault(g => g.Contains(LabSubtype)) ?? baseline.WeaponGroups[0];
            return group.Count > 1 ? "Shared by " + string.Join(", ", group.Select(ClassName)) + "." : "Only " + ClassName(group[0]) + " uses this move.";
        }

        /// <summary>Attacking edges as toggles: the body and weapon parts that hit.</summary>
        private void LabEdges(MovesetBaselineInterval interval, LabAttack state)
        {
            var names = new List<string>(interval.Attack.Edges);
            if (labPreview != null) foreach (string name in labPreview.EdgeNames()) if (!names.Contains(name)) names.Add(name);
            var grid = Rect(labInspector, "Edges");
            var fitter = grid.gameObject.AddComponent<GridLayoutGroup>(); fitter.cellSize = new Vector2(110, 26); fitter.spacing = new Vector2(4, 4);
            fitter.constraint = GridLayoutGroup.Constraint.FixedColumnCount; fitter.constraintCount = 4;
            grid.gameObject.AddComponent<LayoutElement>().preferredHeight = (names.Count + 3) / 4 * 30;
            foreach (string name in names)
            {
                string captured = name;
                bool on = state.Edges.Contains(name);
                LabToggle(grid, name, on, () => LabEditAttack(interval, s =>
                {
                    if (s.Edges.Contains(captured)) { if (s.Edges.Count > 1) s.Edges.Remove(captured); }
                    else s.Edges.Add(captured);
                }));
            }
        }

        // ---- Animation picker ----

        private int LabClipFrames(string clip)
        {
            if (labClipFrames.TryGetValue(clip, out int frames)) return frames;
            try { frames = InfoAnimation.ReadClipFrameCount(clip); }
            catch (Exception) { frames = -1; }
            labClipFrames[clip] = frames;
            return frames;
        }

        /// <summary>Native clips, those the current view's moves use first.</summary>
        private List<string> LabClipOrder(out int scoped)
        {
            if (labClips == null)
                labClips = labBaseline.Values.Select(m => m.File).Where(f => !string.IsNullOrEmpty(f)).Distinct(StringComparer.Ordinal)
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToList();
            var first = new List<string>();
            foreach (string move in LabMoves())
            {
                var baseline = LabBaselineOf(move);
                if (baseline != null && baseline.File.Length != 0 && !first.Contains(baseline.File)) first.Add(baseline.File);
            }
            first.Sort(StringComparer.OrdinalIgnoreCase);
            scoped = first.Count;
            first.AddRange(labClips.Where(c => !first.Contains(c)));
            return first;
        }

        private void BuildLabClipPicker(MovesetBaselineMove baseline)
        {
            LabNote(labInspector, "Click a clip to watch it on the fighter, then USE THIS CLIP. The move keeps its frame windows, attacks and sounds, which may need moving to fit the new clip.");
            var filterRow = Rect(labInspector, "Clip filter"); filterRow.gameObject.AddComponent<LayoutElement>().preferredHeight = 32;
            var filterRect = Rect(filterRow, "Field"); Stretch(filterRect);
            labClipFilter = AddFilterField(filterRect);
            labClipFilter.characterLimit = 64;
            labClipFilter.onValueChanged.AddListener(_ => RefreshLabClipRows(baseline));
            labClipInfo = LabSection(labInspector, "Clip info");
            var buttons = Rect(labInspector, "Clip buttons"); buttons.gameObject.AddComponent<LayoutElement>().preferredHeight = 34;
            var row = buttons.gameObject.AddComponent<HorizontalLayoutGroup>(); row.spacing = 6; row.childControlWidth = row.childControlHeight = true; row.childForceExpandWidth = true;
            LabSmallButton(buttons, "USE THIS CLIP", () => UseLabClip(baseline, labClipChoice));
            LabSmallButton(buttons, "ORIGINAL", () => UseLabClip(baseline, baseline.File));
            LabSmallButton(buttons, "CANCEL", () => { labPickingClip = false; RefreshLabInspector(); });
            labClipRows = LabSection(labInspector, "Clips");
            RefreshLabClipInfo(baseline);
            RefreshLabClipRows(baseline);
        }

        private RectTransform LabSection(RectTransform parent, string name)
        {
            var section = Rect(parent, name);
            var layout = section.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 3; layout.childControlHeight = layout.childControlWidth = true; layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
            return section;
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
                if (shown == 0 && i < scoped && filter.Length == 0) LabHeading(labClipRows, "USED IN THIS VIEW");
                if (i == scoped && filter.Length == 0) LabHeading(labClipRows, "ALL CLIPS");
                string captured = clip;
                string text = clip + (clip == current ? "  <color=#D6AA4E>current</color>" : string.Empty);
                LabListItem(labClipRows, text, clip == labClipChoice, () => PreviewLabClip(baseline, captured));
                shown++;
            }
            if (matches > shown) LabNote(labClipRows, (matches - shown) + " more. Type part of a name to search all " + clips.Count + " clips.");
            if (matches == 0) LabNote(labClipRows, "No clip matches.");
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
            labPreview.Paused = false;
            if (labPreview.PlayMove(player.Name)) { labPlaying = player.Name; labPlayedAt = Time.unscaledTime; }
        }

        /// <summary>What a swap to <paramref name="clip"/> would leave outside the clip, or why it cannot be used.</summary>
        private List<string> LabClipProblems(MovesetBaselineMove baseline, string clip, out bool blocked)
        {
            var problems = new List<string>();
            int frames = LabClipFrames(clip);
            blocked = frames <= baseline.FirstFrame;
            if (frames < 0) { problems.Add("The clip could not be read."); return problems; }
            if (blocked) { problems.Add("This move starts at keyframe " + baseline.FirstFrame + "; the clip has only " + frames + "."); return problems; }
            int last = frames - 1;
            foreach (var item in LabIntervals(baseline))
            {
                if (item.Removed) continue;
                int end = item.Baseline?.Attack != null ? LabAttackState(item.Baseline).End : item.End ?? item.Start;
                int start = item.Baseline?.Attack != null ? LabAttackState(item.Baseline).Start : item.Start;
                if (start > last || end > last)
                    problems.Add((item.Baseline?.Attack != null ? "Attack " + item.Baseline.Attack.Id : item.Label) + " (" + start + "–" + end + ") ends after the clip's last keyframe " + last + ".");
            }
            return problems;
        }

        private void RefreshLabClipInfo(MovesetBaselineMove baseline)
        {
            if (labClipInfo == null) return;
            for (int i = labClipInfo.childCount - 1; i >= 0; i--) Destroy(labClipInfo.GetChild(i).gameObject);
            if (labClipChoice == null) { LabNote(labClipInfo, "No clip chosen yet."); return; }
            int frames = LabClipFrames(labClipChoice);
            LabHeading(labClipInfo, labClipChoice + (frames >= 0 ? "   " + Math.Max(0, frames - baseline.FirstFrame) + " keyframes for this move" : string.Empty));
            var problems = LabClipProblems(baseline, labClipChoice, out bool blocked);
            if (problems.Count == 0) LabNote(labClipInfo, "Every attack and frame window fits inside this clip.");
            foreach (string problem in problems.Take(6)) LabNote(labClipInfo, (blocked ? "Cannot use: " : "Check: ") + problem);
            if (problems.Count > 6) LabNote(labClipInfo, "…and " + (problems.Count - 6) + " more.");
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

        private void RefreshLabTimeline()
        {
            if (labTimeline == null) return;
            for (int i = labTimeline.childCount - 1; i >= 0; i--) Destroy(labTimeline.GetChild(i).gameObject);
            labPlayhead = null;
            var baseline = LabBaselineOf(labMove);
            if (baseline == null) return;
            var intervals = LabIntervals(baseline);
            labTimelineFrom = Math.Min(baseline.FirstFrame, intervals.Count == 0 ? baseline.FirstFrame : intervals.Min(i => i.Start));
            labTimelineTo = Math.Max(LabLastFrame(baseline), intervals.Count == 0 ? 0 : intervals.Max(i => i.End ?? i.Start)) + 1;
            float width = labTimeline.rect.width > 0 ? labTimeline.rect.width : 340f, height = labTimeline.rect.height > 0 ? labTimeline.rect.height : 92f;
            int rows = Math.Max(1, intervals.Count);
            float rowHeight = Mathf.Min(14f, (height - 6f) / rows);
            for (int i = 0; i < intervals.Count; i++)
            {
                var item = intervals[i];
                bool attack = item.Baseline?.Attack != null;
                int end = item.End ?? labTimelineTo - 1;
                float x0 = LabFrameX(item.Start, width), x1 = LabFrameX(end + 1, width);
                var bar = Rect(labTimeline, item.Label);
                bar.anchorMin = bar.anchorMax = bar.pivot = new Vector2(0, 1);
                bar.anchoredPosition = new Vector2(x0, -3 - i * rowHeight);
                bar.sizeDelta = new Vector2(Mathf.Max(2f, x1 - x0), rowHeight - 2f);
                var image = bar.gameObject.AddComponent<Image>(); image.raycastTarget = false;
                image.color = item.Removed ? new Color(Ink.r, Ink.g, Ink.b, .18f) : attack ? Red : item.Added != null ? Gold : new Color(Ink.r, Ink.g, Ink.b, .62f);
                if (rowHeight >= 11f)
                {
                    var text = Label(bar, item.Label, 10, attack ? Paper : PaperWarm, TextAnchor.MiddleLeft);
                    text.rectTransform.offsetMin = new Vector2(3, 0);
                    text.horizontalOverflow = HorizontalWrapMode.Overflow;
                }
            }
            labPlayhead = Rect(labTimeline, "Playhead");
            labPlayhead.anchorMin = new Vector2(0, 0); labPlayhead.anchorMax = new Vector2(0, 1); labPlayhead.pivot = new Vector2(.5f, .5f);
            labPlayhead.sizeDelta = new Vector2(2, 0);
            var line = labPlayhead.gameObject.AddComponent<Image>(); line.color = Gold; line.raycastTarget = false;
        }

        private float LabFrameX(int frame, float width) =>
            (frame - labTimelineFrom) * width / Mathf.Max(1, labTimelineTo - labTimelineFrom);

        // ---- Playback ----

        private void LabPlay()
        {
            if (labPreview == null || labMove == null) return;
            // A copy exists in the game only once applied; until then the original plays.
            string name = AnimationData.GetAnimationByName(labMove, false) != null ? labMove : labCopy.NativeSource(labMove);
            labPreview.Paused = false;
            if (!labPreview.PlayMove(name)) { labReplayAt = labPreview.IsReady ? -1f : Time.unscaledTime + .2f; return; }
            labPlaying = name;
            labPlayedAt = Time.unscaledTime;
        }

        private void LabTogglePause()
        {
            if (labPreview == null) return;
            labPreview.Paused = !labPreview.Paused;
            SetStatus(labPreview.Paused ? "Paused. STEP advances one tick." : string.Empty);
        }

        private void LabStep(int ticks)
        {
            if (labPreview == null) return;
            labPreview.Paused = true;
            labPreview.Step(ticks);
        }

        private void UpdateMovesetLab()
        {
            if (labPreview == null) return;
            if (labReplayAt >= 0f && Time.unscaledTime >= labReplayAt && labPreview.IsReady) { labReplayAt = -1f; LabPlay(); }
            // The edge toggles list the fighter's own edges, known once it is built.
            if (!labEdgesKnown && labPreview.IsReady && labPreview.EdgeNames().Count > 0) { labEdgesKnown = true; RefreshLabInspector(); }
            bool playing = labPlaying != null && labPreview.PlayingMove == labPlaying;
            if (labLoop && labPlayedAt > 0f && !playing && !labPreview.Paused && Time.unscaledTime > labPlayedAt + .3f)
            {
                labPlayedAt = 0f;
                labReplayAt = Time.unscaledTime + .5f;
            }
            if (labReadout != null)
                labReadout.text = playing ? "tick " + labPreview.MoveTick + "   ·   keyframe " + labPreview.Keyframe + (labPreview.Paused ? "   ·   paused" : string.Empty)
                    : labPlaying == null ? "PLAY shows the move" : "idle";
            if (labPlayhead != null)
            {
                labPlayhead.gameObject.SetActive(playing);
                if (playing) labPlayhead.anchoredPosition = new Vector2(LabFrameX(labPreview.Keyframe, labTimeline.rect.width), 0);
            }
        }

        // ---- Apply, save and training ----

        /// <summary>Saves the working copy as the Lab's mod and applies every enabled mod's move edits.</summary>
        private bool ApplyLab()
        {
            if (labCopy == null) return false;
            string root = ModRuntime.Host.ModsRoot;
            var dependencies = new Dictionary<string, string>(StringComparer.Ordinal);
            bool needsCore = false;
            foreach (var fork in labCopy.Document.Forks)
            {
                if (fork.Item == null) continue;
                string owner = fork.Item.Substring(0, Math.Max(0, fork.Item.IndexOf(':')));
                if (owner == "core") { needsCore = true; continue; }
                var mod = ModRuntime.Scripts?.ActiveMods.FirstOrDefault(m => m.Id.Value == owner);
                if (mod != null && owner != labModId) dependencies[owner] = mod.Version.ToString();
            }
            try { MovesetModWriter.Save(root, labModId, labModName ?? "Moveset Lab", labCopy.Document, needsCore, dependencies); }
            catch (Exception exception) { SetStatus("Not saved: " + exception.Message); return false; }
            labCopy.MarkSaved();
            labDiscardArmed = false;
            if (!ModRuntime.TryApplyMoveOverlay(labModId, out string report)) { SetStatus("Saved, but " + report); return false; }
            labOverlayApplied = true;
            labPreview?.Refresh();
            labReplayAt = Time.unscaledTime + .35f;
            RefreshLab();
            SetStatus("Saved to " + labModId + ". " + report);
            return true;
        }

        private void TestLabInTraining()
        {
            if ((labCopy.IsDirty || !labOverlayApplied) && !ApplyLab()) return;
            if (trainingDummy == null) trainingDummy = VersusLoadouts.Load(VersusLoadouts.Dummy);
            if (!VersusRoster.IsArena(trainingArena)) trainingArena = VersusRoster.IsArena("dojo") ? "dojo" : VersusRoster.Arenas.Count > 0 ? VersusRoster.Arenas[0].Id : "dojo";
            trainingPlayer = LabLoadout();
            labTesting = true;
            StartTraining();
        }

        private void ReturnToMovesetLab()
        {
            labReturning = true;
            LocalVersusSession.ShowMultiplayerHome();
            ShowMovesetLab();
        }

        // ---- Widgets ----

        private Text LabCycler(RectTransform parent, float y, Func<string> value, Action<int> step, string extraText = null, Action extra = null)
        {
            var row = Place(parent, "Cycler", new Vector2(.5f, 1), new Vector2(0, y), new Vector2(276, 34));
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>(); layout.spacing = 4; layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandWidth = false;
            LabSmallButton(row, "<", () => step(-1), 34);
            var box = Rect(row, "Value"); box.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var text = Label(box, value(), 17, Gold, TextAnchor.MiddleCenter);
            text.resizeTextForBestFit = true; text.resizeTextMinSize = 11; text.resizeTextMaxSize = 17;
            LabSmallButton(row, ">", () => step(1), 34);
            if (extra != null) LabSmallButton(row, extraText, extra, 58);
            return text;
        }

        private Button LabSmallButton(RectTransform parent, string text, Action action, float width = 0)
        {
            var button = AddButton(parent, text, action, width, UiSound.Toggle);
            var element = button.GetComponent<LayoutElement>(); element.preferredHeight = 34;
            var label = button.GetComponentInChildren<Text>(); label.fontSize = 17;
            return button;
        }

        private RectTransform BuildScrollList(RectTransform area)
        {
            var scroll = area.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 40f;
            var viewport = Rect(area, "Viewport"); Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            var hit = viewport.gameObject.AddComponent<Image>(); hit.color = new Color(0, 0, 0, 0); hit.raycastTarget = true;
            var content = Rect(viewport, "Content");
            content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.pivot = new Vector2(.5f, 1); content.sizeDelta = Vector2.zero;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 3; layout.padding = new RectOffset(6, 6, 6, 6);
            layout.childControlHeight = layout.childControlWidth = true; layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport; scroll.content = content;
            area.gameObject.AddComponent<ScrollToSelection>().Scroll = scroll;
            return content;
        }

        private Button LabListItem(RectTransform parent, string text, bool selected, Action pick)
        {
            var rect = Rect(parent, "Move");
            rect.gameObject.AddComponent<LayoutElement>().preferredHeight = 28;
            var back = rect.gameObject.AddComponent<Image>(); back.color = selected ? Red : new Color(Paper.r, Paper.g, Paper.b, .06f); back.raycastTarget = true;
            var label = Label(rect, text, 16, Paper, TextAnchor.MiddleLeft); label.supportRichText = true;
            label.rectTransform.offsetMin = new Vector2(10, 0);
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = back;
            var colors = button.colors; colors.highlightedColor = colors.selectedColor = new Color(1.6f, 1.4f, 1.2f, 1f); colors.fadeDuration = .06f; button.colors = colors;
            button.onClick.AddListener(() => { EclipseUiAudio.Play(UiSound.Toggle); pick(); });
            return button;
        }

        private void LabHeading(RectTransform parent, string text)
        {
            var rect = Rect(parent, "Heading"); rect.gameObject.AddComponent<LayoutElement>().preferredHeight = 30;
            var label = Label(rect, text, 18, Gold, TextAnchor.LowerLeft); label.rectTransform.offsetMin = new Vector2(4, 0);
            label.resizeTextForBestFit = true; label.resizeTextMinSize = 12; label.resizeTextMaxSize = 18;
        }

        private void LabNote(RectTransform parent, string text)
        {
            var rect = Rect(parent, "Note");
            var label = Label(rect, text, 14, new Color(Paper.r, Paper.g, Paper.b, .78f), TextAnchor.UpperLeft);
            label.horizontalOverflow = HorizontalWrapMode.Wrap; label.rectTransform.offsetMin = new Vector2(4, 0);
            rect.gameObject.AddComponent<LayoutElement>().preferredHeight = text.Length > 60 ? 38 : 20;
        }

        /// <summary>A caption, the current value, and − / + buttons (Shift-free: one step per press).</summary>
        private void LabStepper(RectTransform parent, string caption, string value, Action<int> step)
        {
            var row = Rect(parent, caption); row.gameObject.AddComponent<LayoutElement>().preferredHeight = 32;
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>(); layout.spacing = 4; layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandWidth = false;
            var name = Label(row, caption, 16, Paper, TextAnchor.MiddleLeft); name.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            name.resizeTextForBestFit = true; name.resizeTextMinSize = 11; name.resizeTextMaxSize = 16;
            if (step != null) LabSmallButton(row, "−", () => step(-1), 34);
            var shown = Label(row, value ?? "-", 16, Gold, TextAnchor.MiddleCenter); var element = shown.gameObject.AddComponent<LayoutElement>(); element.minWidth = element.preferredWidth = 130;
            shown.resizeTextForBestFit = true; shown.resizeTextMinSize = 10; shown.resizeTextMaxSize = 16;
            if (step != null) LabSmallButton(row, "+", () => step(1), 34);
        }

        private void LabRowButton(RectTransform parent, string text, Action action)
        {
            var button = LabSmallButton(parent, text, action);
            button.GetComponentInChildren<Text>().fontSize = 15;
        }

        private void LabToggle(RectTransform parent, string text, bool on, Action toggle)
        {
            var rect = Rect(parent, text);
            var back = rect.gameObject.AddComponent<Image>(); back.color = on ? Red : new Color(Paper.r, Paper.g, Paper.b, .1f); back.raycastTarget = true;
            var label = Label(rect, text, 12, on ? Paper : new Color(Paper.r, Paper.g, Paper.b, .7f), TextAnchor.MiddleCenter);
            label.resizeTextForBestFit = true; label.resizeTextMinSize = 8; label.resizeTextMaxSize = 12;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = back;
            button.onClick.AddListener(() => { EclipseUiAudio.Play(UiSound.Toggle); toggle(); });
        }
    }
}
