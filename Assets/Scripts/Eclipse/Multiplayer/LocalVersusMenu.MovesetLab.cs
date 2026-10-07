using System;
using System.Collections.Generic;
using System.Linq;
using Eclipse.Modding;
using UnityEngine;

namespace Eclipse.Multiplayer
{
    // The Moveset Lab: browse moves by weapon subtype (or one weapon), watch them on a live
    // fighter with the attacking edges drawn, tune them, and save the result as a data-only
    // mod (movesets/moveset.json). Applying swaps the game's move overlay for the saved
    // edits, so the preview and "test in training" use exactly what the mod will load.
    // This file holds the Lab's document, scopes and edit routing; the editor view is in
    // LocalVersusMenu.MovesetLabView.cs.
    public sealed partial class LocalVersusMenu
    {
        public const string MovesetLabDefaultMod = "local.moveset-lab";
        private const string LabShared = "";
        private const string LabUnarmed = "Fists";
        private static bool labOnEntry;

        private Dictionary<string, MovesetBaselineMove> labBaseline;
        private MovesetWorkingCopy labCopy;
        private string labModId = MovesetLabDefaultMod;
        private string labModName = "Moveset Lab";
        private readonly List<string> labScopes = new List<string>();
        private int labScope;
        private readonly List<LabWeapon> labWeapons = new List<LabWeapon>();
        private int labWeapon = -1;
        private string labMove;
        private readonly HashSet<string> labWholeFamily = new HashSet<string>(StringComparer.Ordinal);
        private bool labOverlayApplied, labTesting, labReturning, labDiscardArmed, labSwitchArmed;
        private List<string> labClips;
        private readonly Dictionary<string, int> labClipFrames = new Dictionary<string, int>(StringComparer.Ordinal);
        // Clips imported into the Lab's mod and not yet written: asset path -> bytes. APPLY writes them.
        private readonly Dictionary<string, byte[]> labPendingClips = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> labClipNodes = new Dictionary<string, int>(StringComparer.Ordinal);
        // Picker keys for the mod's own clips: this prefix and the asset path ("animations/name").
        private const string LabAssetClip = "asset:";
        private static readonly string[] LabAddTypes = { "Block", "Invulnerable", "Invisible", "Throwable" };

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
            EndLabClipPreview();
            ModRuntime.RemoveLabItems();
            labTesting = false;
            labPreview = null;
            labVictim = null;
            if (!labOverlayApplied) return;
            labOverlayApplied = false;
            ModRuntime.ClearMoveOverlay();
        }

        private void LoadLabMod(string modId)
        {
            string manifest = System.IO.Path.Combine(ModRuntime.Host.ModsRoot, modId, "mod.toml");
            var copy = new MovesetWorkingCopy(modId, MovesetModWriter.Load(ModRuntime.Host.ModsRoot, modId));
            EndLabClipPreview();
            labModId = modId;
            labCopy = copy;
            labPendingClips.Clear();
            ForgetLabAssetClips();
            if (System.IO.File.Exists(manifest)) labModName = ModManifestReader.ReadExternalFile(manifest).Name;
            labWholeFamily.Clear();
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

        /// <summary>True when unsaved edits may be dropped: the first try only warns.</summary>
        private bool LabMayDiscard(string action)
        {
            if (labCopy == null || !labCopy.IsDirty || labSwitchArmed) { labSwitchArmed = false; return true; }
            labSwitchArmed = true;
            SetStatus("Unsaved edits in " + labModName + ". Choose again to discard them and " + action + ", or APPLY first.");
            return false;
        }

        private void SwitchLabMod(LabMod next)
        {
            if (next.Id == labModId) return;
            if (!LabMayDiscard("switch mods")) return;
            try { LoadLabMod(next.Id); }
            catch (Exception exception) { SetStatus("Could not open " + next.Name + ": " + exception.Message); return; }
            labModName = next.Name;
            AfterLabModChanged(next.Enabled ? "Editing " + next.Name + " (" + next.Id + ")." :
                "Editing " + next.Name + ". It is switched off in Mods; switch it on to APPLY.");
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
            CloseLabPopup();
            labModId = id;
            labModName = name;
            labCopy = new MovesetWorkingCopy(id);
            labWholeFamily.Clear();
            AfterLabModChanged("New mod " + name + " (" + id + "). It is saved in the Mods folder on APPLY.");
        }

        private void AfterLabModChanged(string message)
        {
            labDiscardArmed = false;
            LoadLabWeaponsLive();
            OnLabScopeChanged();
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
        private string LabScopeName(string subtype) => subtype == LabShared ? "Shared (every fighter)" : subtype == LabUnarmed ? "Unarmed" : ClassName(subtype);
        private string LabScopeName() => LabScopeName(LabSubtype);
        private bool LabScopeHasWeapons => LabSubtype != LabShared && LabSubtype != LabUnarmed;
        private string LabWeaponName() => !LabScopeHasWeapons ? "—" : LabCurrentWeapon == null ? "Every " + ClassName(LabSubtype) : LabCurrentWeapon.Name;

        private void OnLabScopeChanged()
        {
            labWeapons.Clear();
            if (LabScopeHasWeapons) labWeapons.AddRange(LabWeaponsOf(LabSubtype));
            if (labWeapon >= labWeapons.Count) labWeapon = -1;
            labPreview?.Show(LabLoadout(), true);
            labEdgesKnown = false;
            var moves = LabMoves();
            if (labMove == null || !moves.Contains(labMove)) labMove = moves.FirstOrDefault();
            RefreshLab();
            labReplayAt = Time.unscaledTime + .3f;
        }

        /// <summary>Weapons of a subtype, from core and enabled mods (and this mod's new ones), that the preview can wear.</summary>
        private List<LabWeapon> LabWeaponsOf(string subtype)
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
            foreach (var weapon in labModWeapons)
                if (weapon.Subtype == subtype && !result.Any(w => w.Id == weapon.Id) && ListSF.GetItems().GetItemByName(weapon.Id) != null)
                    result.Add(new LabWeapon { Id = weapon.Id, RuntimeName = weapon.Id, Name = weapon.Name + " (" + labModId + ")", Owner = labModId });
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

        /// <summary>
        /// The moves this view lists, with this mod's forks in place of their sources. A chosen
        /// weapon lists its subtype's moves it can use and moves locked to that weapon; the
        /// shared moves every fighter has stay under the Shared scope.
        /// </summary>
        private List<string> LabMoves()
        {
            string subtype = LabSubtype;
            var weapon = LabCurrentWeapon;
            var result = new List<string>();
            foreach (var move in labBaseline.Values)
            {
                bool shared = move.WeaponGroups.Count == 0 && move.WeaponItems.Count == 0 && move.PlayerSkeleton;
                bool inSubtype = move.WeaponGroups.Any(group => group.Contains(subtype));
                bool forWeapon = weapon != null && move.WeaponItems.Contains(weapon.RuntimeName);
                bool include = subtype == LabShared ? shared
                    : weapon == null ? inSubtype
                    // A move locked to named items is for those items only.
                    : forWeapon || inSubtype && move.WeaponItems.Count == 0;
                if (include) result.Add(move.Name);
            }
            result.Sort(StringComparer.Ordinal);
            foreach (var fork in labCopy.Document.Forks)
            {
                if (fork.Add) continue;
                bool applies = fork.Subtype != null ? fork.Subtype == subtype && subtype != LabShared : weapon != null && fork.Item == weapon.Id;
                if (!applies) continue;
                int index = result.IndexOf(fork.Move);
                if (index >= 0) result[index] = labCopy.ForkName(fork);
                else result.Add(labCopy.ForkName(fork));
            }
            // New moves sit right after their base move, for the fighters they are made for.
            foreach (var fork in labCopy.Document.Forks)
            {
                if (!fork.Add) continue;
                int source = result.IndexOf(fork.Move);
                bool applies = fork.Subtype != null ? fork.Subtype == subtype && subtype != LabShared
                    : fork.Item != null ? weapon != null && fork.Item == weapon.Id
                    : source >= 0;
                if (!applies) continue;
                if (source >= 0) result.Insert(source + 1, labCopy.ForkName(fork));
                else result.Add(labCopy.ForkName(fork));
            }
            return result;
        }

        private MovesetBaselineMove LabBaselineOf(string move) =>
            move != null && labBaseline.TryGetValue(labCopy.NativeSource(move), out var baseline) ? baseline : null;

        /// <summary>A fork by its local id (the runtime name adds the mod id in front).</summary>
        private string LabDisplayName(string move) => labCopy.FindFork(move)?.Id ?? move;

        private string LabBadge(string move)
        {
            var fork = labCopy.FindFork(move);
            if (fork != null) return fork.Add ? "new move" : fork.Subtype != null ? "subtype copy" : "weapon copy";
            var baseline = LabBaselineOf(move);
            if (baseline == null) return string.Empty;
            if (baseline.WeaponGroups.Count == 0) return "shared";
            var group = baseline.WeaponGroups.FirstOrDefault(g => g.Contains(LabSubtype));
            return group != null && group.Count > 1 ? "family of " + group.Count : string.Empty;
        }

        private string LabDescribeUsers(MovesetBaselineMove baseline)
        {
            if (baseline.WeaponGroups.Count == 0) return "Shared: every fighter can use this move, armed or not.";
            var group = baseline.WeaponGroups.FirstOrDefault(g => g.Contains(LabSubtype)) ?? baseline.WeaponGroups[0];
            return group.Count > 1 ? "Shared by " + string.Join(", ", group.Select(ClassName)) + "." : "Only " + ClassName(group[0]) + " uses this move.";
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
            ShowLabScopePrompt(move, choices, change);
        }

        private List<(string label, Func<string> target)> LabScopeChoices(string move)
        {
            var choices = new List<(string, Func<string>)>();
            var baseline = LabBaselineOf(move);
            if (labCopy.FindFork(move) != null || baseline == null || labWholeFamily.Contains(move) || labCopy.Entry(move, false) != null)
            {
                choices.Add(("Edit", () => move));
                return choices;
            }
            var weapon = LabCurrentWeapon;
            var group = baseline.WeaponGroups.FirstOrDefault(g => g.Contains(LabSubtype));
            bool shared = baseline.WeaponGroups.Count == 0;
            string everyone = shared ? "Every fighter" : group != null && group.Count > 1
                ? "Whole family (" + string.Join(", ", group.Select(ClassName)) + ")" : "All " + LabScopeName();
            choices.Add((everyone, () => { labWholeFamily.Add(move); return move; }));
            if (!shared && group != null && group.Count > 1)
                choices.Add(("Only " + LabScopeName() + " (makes a copy)", () => labCopy.ForkName(labCopy.CreateSubtypeFork(move, LabSubtype))));
            if (weapon != null)
                choices.Add(("Only " + weapon.Name + " (makes a copy)", () => labCopy.ForkName(labCopy.CreateItemFork(move, weapon.Id))));
            return choices;
        }

        /// <summary>Runs a scope choice's edit; the move the edit landed on becomes the selection.</summary>
        private void LabRunScopedEdit(Func<string> choose, Action<MovesetWorkingCopy, string> change)
        {
            string target;
            try { target = null; labCopy.Edit(copy => { target = choose(); change(copy, target); }); }
            catch (Exception exception) { SetStatus(exception.Message); return; }
            if (target != null) labMove = target;
            AfterLabEdit();
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
            SetStatus("Edited. APPLY (F5) saves and plays the change on the fighter.");
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
            foreach (var interval in LabNewAttackIntervals())
                result.Add(new LabInterval { Baseline = interval, Start = interval.Start, End = interval.End });
            return result;
        }

        // New attacks are shown through interval records like the native ones, so the timeline,
        // tabs and previews treat them alike; editing one rewrites its new_attacks entry.
        private readonly System.Runtime.CompilerServices.ConditionalWeakTable<ModMoveAttackAddition, MovesetBaselineInterval> labNewAttackViews =
            new System.Runtime.CompilerServices.ConditionalWeakTable<ModMoveAttackAddition, MovesetBaselineInterval>();
        private readonly Dictionary<MovesetBaselineInterval, int> labNewAttackIds = new Dictionary<MovesetBaselineInterval, int>();

        private List<MovesetBaselineInterval> LabNewAttackIntervals()
        {
            var result = new List<MovesetBaselineInterval>();
            var entry = LabEntry;
            if (entry == null) return result;
            foreach (var addition in entry.NewAttacks)
            {
                var interval = labNewAttackViews.GetValue(addition, a =>
                {
                    var attack = new MovesetBaselineAttack { Id = a.Id, Damage = a.Damage, Impulse = a.Impulse.ToArray(), Hit = a.Hit };
                    foreach (var term in a.Terms) attack.Terms[term.Key] = term.Value;
                    attack.Edges.AddRange(a.Edges);
                    return new MovesetBaselineInterval { Type = "Attack", Start = a.Start, End = a.End, Attack = attack };
                });
                labNewAttackIds[interval] = addition.Id;
                result.Add(interval);
            }
            return result;
        }

        private bool LabIsNewAttack(MovesetBaselineInterval interval) => interval != null && labNewAttackIds.ContainsKey(interval);

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
            if (labNewAttackIds.TryGetValue(interval, out int newId))
            {
                LabEdit((copy, target) =>
                {
                    var state = LabAttackState(interval);
                    change(state);
                    copy.ReplaceNewAttack(target, newId, new ModMoveAttackAddition(newId, state.Start, state.End, state.Damage, state.Terms,
                        state.Edges, state.Impulse, state.Hit));
                });
                return;
            }
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

        /// <summary>The keyframes a native move plays. Without an authored EndFrame it plays to its clip's end.</summary>
        private ModMoveClipRange LabNativeRange(MovesetBaselineMove baseline)
        {
            int last = baseline.FirstFrame + baseline.FrameCount - 1;
            if (last <= baseline.FirstFrame)
            {
                int frames = LabClipFrames(baseline.File);
                last = frames > baseline.FirstFrame + 1 ? frames - 1 : baseline.FirstFrame + 1;
            }
            return new ModMoveClipRange(baseline.FirstFrame, last);
        }

        /// <summary>The last keyframe of the clip the move plays now (a swapped clip's own length).</summary>
        private int LabClipLast(MovesetBaselineMove baseline)
        {
            string clip = LabCurrentClip(null);
            int frames = clip != null ? LabClipFrames(clip) : -1;
            return frames > baseline.FirstFrame ? frames - 1 : LabNativeRange(baseline).Last;
        }

        /// <summary>The move's first keyframe, after a trim.</summary>
        private int LabFirstFrame(MovesetBaselineMove baseline) => LabEntry?.ClipRange?.Value.First ?? baseline.FirstFrame;

        /// <summary>The move's last keyframe, after a trim or with a swapped clip's own length.</summary>
        private int LabLastFrame(MovesetBaselineMove baseline) => LabEntry?.ClipRange?.Value.Last ?? LabClipLast(baseline);

        // ---- Animation clips ----

        /// <summary>The picker key of the clip the selected move plays now, or <paramref name="original"/> when unswapped.</summary>
        private string LabCurrentClip(string original)
        {
            var animation = LabEntry?.Animation;
            if (animation?.NativeValue != null) return animation.NativeValue;
            if (animation?.AssetValue != null) return LabAssetClip + animation.AssetValue;
            return original;
        }

        private static bool IsLabAssetClip(string clip) => clip.StartsWith(LabAssetClip, StringComparison.Ordinal);

        /// <summary>How a clip key reads in the Lab: native file name, or the mod asset's name.</summary>
        private static string LabClipLabel(string clip) => IsLabAssetClip(clip) ? clip.Substring(LabAssetClip.Length) + "  (this mod)" : clip;

        private int LabClipFrames(string clip)
        {
            if (labClipFrames.TryGetValue(clip, out int frames)) return frames;
            if (IsLabAssetClip(clip))
            {
                var data = LabAssetClipBytes(clip.Substring(LabAssetClip.Length));
                int nodes = -1;
                frames = data != null && MovesetClipFile.Inspect(data, out int count, out nodes) == null ? count : -1;
                if (frames > 0) labClipNodes[clip] = nodes;
            }
            else
            {
                try { frames = InfoAnimation.ReadClipFrameCount(clip); }
                catch (Exception) { frames = -1; }
            }
            labClipFrames[clip] = frames;
            return frames;
        }

        /// <summary>Nodes per keyframe of a clip, or -1: a clip made for another rig has a different count.</summary>
        private int LabClipNodes(string clip)
        {
            if (labClipNodes.TryGetValue(clip, out int nodes)) return nodes;
            nodes = -1;
            if (IsLabAssetClip(clip)) { LabClipFrames(clip); return labClipNodes.TryGetValue(clip, out nodes) ? nodes : -1; }
            try
            {
                var data = ResourceManager.GetBinary(SF2Paths.GetBinaryAnimationsPath() + "/" + clip);
                if (data != null && MovesetClipFile.Inspect(data, out _, out int count) == null) nodes = count;
            }
            catch (Exception) { nodes = -1; }
            labClipNodes[clip] = nodes;
            return nodes;
        }

        private byte[] LabAssetClipBytes(string assetPath)
        {
            if (labPendingClips.TryGetValue(assetPath, out var pending)) return pending;
            try
            {
                string file = MovesetClipFile.PathOf(ModRuntime.Host.ModsRoot, labModId, assetPath);
                return System.IO.File.Exists(file) ? System.IO.File.ReadAllBytes(file) : null;
            }
            catch (Exception) { return null; }
        }

        private void ForgetLabAssetClips()
        {
            foreach (string key in labClipFrames.Keys.Where(IsLabAssetClip).ToList()) labClipFrames.Remove(key);
            foreach (string key in labClipNodes.Keys.Where(IsLabAssetClip).ToList()) labClipNodes.Remove(key);
        }

        /// <summary>The Lab mod's clips (saved and imported), as picker keys.</summary>
        private List<string> LabModClips()
        {
            var paths = new SortedSet<string>(StringComparer.Ordinal);
            try { foreach (string path in MovesetClipFile.InMod(ModRuntime.Host.ModsRoot, labModId)) paths.Add(path); }
            catch (Exception) { }
            foreach (string path in labPendingClips.Keys) paths.Add(path);
            return paths.Select(p => LabAssetClip + p).ToList();
        }

        /// <summary>
        /// Reads a native clip file into the Lab's mod as assets/animations/&lt;name&gt;.bytes (written
        /// on APPLY). Returns its picker key, or null with <paramref name="error"/>.
        /// </summary>
        private string ImportLabClip(string file, MovesetBaselineMove baseline, out string error)
        {
            error = null;
            byte[] data;
            try
            {
                if (new System.IO.FileInfo(file).Length > MovesetClipFile.MaxBytes) { error = "The file is larger than " + MovesetClipFile.MaxBytes / (1024 * 1024) + " MB."; return null; }
                data = System.IO.File.ReadAllBytes(file);
            }
            catch (Exception exception) { error = "Could not read the file: " + exception.Message; return null; }
            string problem = MovesetClipFile.Inspect(data, out _, out int nodes);
            if (problem != null) { error = problem; return null; }
            int rig = LabClipNodes(baseline.File);
            if (rig > 0 && nodes != rig) { error = "The clip moves " + nodes + " nodes; this move's fighter has " + rig + ". It was made for a different rig."; return null; }
            string name = MovesetClipFile.SafeName(file);
            string path = MovesetClipFile.Folder + "/" + name;
            for (int i = 2; ; i++)
            {
                var existing = LabAssetClipBytes(path);
                if (existing == null || existing.SequenceEqual(data)) break;
                path = MovesetClipFile.Folder + "/" + name + "_" + i;
            }
            labPendingClips[path] = data;
            labClipFrames.Remove(LabAssetClip + path);
            labClipNodes.Remove(LabAssetClip + path);
            return LabAssetClip + path;
        }

        /// <summary>Writes imported clips into the Lab mod's folder.</summary>
        private void WriteLabPendingClips()
        {
            foreach (var pair in labPendingClips)
            {
                string file = MovesetClipFile.PathOf(ModRuntime.Host.ModsRoot, labModId, pair.Key);
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(file));
                string temp = file + ".write";
                System.IO.File.WriteAllBytes(temp, pair.Value);
                if (System.IO.File.Exists(file)) System.IO.File.Delete(file);
                System.IO.File.Move(temp, file);
            }
            labPendingClips.Clear();
        }

        /// <summary>Native clips, those the current view's moves use first.</summary>
        private List<string> LabClipOrder(out int scoped)
        {
            if (labClips == null)
                labClips = labBaseline.Values.Select(m => m.File).Where(f => !string.IsNullOrEmpty(f)).Distinct(StringComparer.Ordinal)
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToList();
            var first = LabModClips();
            foreach (string move in LabMoves())
            {
                var baseline = LabBaselineOf(move);
                if (baseline != null && baseline.File.Length != 0 && !first.Contains(baseline.File)) first.Add(baseline.File);
            }
            first.Sort((a, b) => IsLabAssetClip(a) != IsLabAssetClip(b) ? (IsLabAssetClip(a) ? -1 : 1) : StringComparer.OrdinalIgnoreCase.Compare(a, b));
            scoped = first.Count;
            first.AddRange(labClips.Where(c => !first.Contains(c)));
            return first;
        }

        /// <summary>What a swap to <paramref name="clip"/> would leave outside the clip, or why it cannot be used.</summary>
        private List<string> LabClipProblems(MovesetBaselineMove baseline, string clip, out bool blocked)
        {
            var problems = new List<string>();
            int frames = LabClipFrames(clip);
            blocked = frames <= baseline.FirstFrame;
            if (frames < 0) { problems.Add("The clip could not be read."); return problems; }
            if (blocked) { problems.Add("This move starts at keyframe " + baseline.FirstFrame + "; the clip has only " + frames + "."); return problems; }
            int nodes = LabClipNodes(clip), rig = LabClipNodes(baseline.File);
            if (nodes > 0 && rig > 0 && nodes != rig)
            {
                blocked = true;
                problems.Add("The clip moves " + nodes + " nodes; this move's fighter has " + rig + ". It was made for a different rig.");
                return problems;
            }
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

        // ---- Apply, save and training ----

        /// <summary>Saves the working copy as the Lab's mod and applies every enabled mod's move edits.</summary>
        /// <summary>
        /// Saves the mod with its Eclipse core range declared and zips it, ready for Mods >
        /// Install ZIP or a mod.io upload, into "Eclipse mods" on the desktop.
        /// </summary>
        private void ExportLab()
        {
            if (labCopy == null) return;
            if (!ApplyLab(true)) return;
            try
            {
                string root = System.IO.Path.Combine(ModRuntime.Host.ModsRoot, labModId);
                var manifest = ModManifestReader.ReadExternalFile(System.IO.Path.Combine(root, "mod.toml"));
                string zip = CharacterImporter.ExportZip(root, "Eclipse mods", labModId + "-" + manifest.Version);
                CharacterImporter.Reveal(zip);
                SetStatus("Exported " + System.IO.Path.GetFileName(zip) + " to Eclipse mods on the desktop. Raise the version in mod.toml before uploading an update.");
            }
            catch (Exception exception) { SetStatus("Not exported: " + exception.Message); }
        }

        private bool ApplyLab(bool declareCore = false)
        {
            if (labCopy == null) return false;
            EndLabClipPreview();
            string root = ModRuntime.Host.ModsRoot;
            var dependencies = new Dictionary<string, string>(StringComparer.Ordinal);
            bool needsCore = declareCore;
            foreach (var fork in labCopy.Document.Forks)
            {
                if (fork.Item == null) continue;
                string owner = fork.Item.Substring(0, Math.Max(0, fork.Item.IndexOf(':')));
                if (owner == "core") { needsCore = true; continue; }
                var mod = ModRuntime.Scripts?.ActiveMods.FirstOrDefault(m => m.Id.Value == owner);
                if (mod != null && owner != labModId) dependencies[owner] = mod.Version.ToString();
            }
            try
            {
                MovesetModWriter.Save(root, labModId, labModName ?? "Moveset Lab", labCopy.Document, needsCore, dependencies);
                WriteLabPendingClips();
                ForgetLabAssetClips();
            }
            catch (Exception exception) { SetStatus("Not saved: " + exception.Message); return false; }
            labCopy.MarkSaved();
            labDiscardArmed = false;
            if (!ModRuntime.TryApplyMoveOverlay(labModId, out string report)) { SetStatus("Saved, but " + report); return false; }
            labOverlayApplied = true;
            labPreview?.Refresh();
            labVictim?.Refresh();
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
    }
}
