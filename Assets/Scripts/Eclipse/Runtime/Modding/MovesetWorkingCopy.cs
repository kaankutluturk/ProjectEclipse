using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse.Modding
{
    /// <summary>
    /// The base-game values of one move, as the Moveset Lab reads them with every mod's
    /// move edits removed. Guards in a saved moveset compare against these values.
    /// </summary>
    public sealed class MovesetBaselineMove
    {
        public string Name { get; set; }
        public string File { get; set; } = string.Empty;
        public int Priority { get; set; }
        public int MidFrames { get; set; }
        public int FrameCount { get; set; }
        /// <summary>First keyframe; interval frames are absolute keyframe numbers from here.</summary>
        public int FirstFrame { get; set; }
        public bool Looped { get; set; }
        public bool Physics { get; set; }
        public List<string> Templates { get; } = new List<string>();
        /// <summary>Positive weapon lock groups: subtypes that share the move.</summary>
        public List<List<string>> WeaponGroups { get; } = new List<List<string>>();
        /// <summary>Item names the move is explicitly locked to (item forks) or excluded from.</summary>
        public List<string> WeaponItems { get; } = new List<string>();
        public bool PlayerSkeleton { get; set; }
        public List<MovesetBaselineInterval> Intervals { get; } = new List<MovesetBaselineInterval>();

        public int MaxRatePermille => Looped || Physics ? Eclipse.Runtime.PlaybackTiming.Normal :
            Math.Min(Eclipse.Runtime.PlaybackTiming.Maximum, (MidFrames + 1) * 1000);
        public IEnumerable<MovesetBaselineInterval> Attacks => Intervals.Where(i => i.Attack != null);
    }

    public sealed class MovesetBaselineInterval
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Start { get; set; }
        /// <summary>Null for an open-ended interval.</summary>
        public int? End { get; set; }
        public MovesetBaselineAttack Attack { get; set; }
        public ModMoveIntervalSelector Selector => new ModMoveIntervalSelector(Type, Name, Start, End);
        public string Label => (Type.Length != 0 ? Type : Name) + (Type.Length != 0 && Name.Length != 0 ? "/" + Name : string.Empty);
    }

    public sealed class MovesetBaselineAttack
    {
        public int Id { get; set; }
        public double Damage { get; set; }
        public Dictionary<string, double> Terms { get; } = new Dictionary<string, double>(StringComparer.Ordinal);
        public List<string> Edges { get; } = new List<string>();
        public double[] Impulse { get; set; } = new double[3];
        /// <summary>The single full-interval reaction, or null when partial/multiple.</summary>
        public string Hit { get; set; }
        public bool HasPartialHits { get; set; }
    }

    /// <summary>
    /// The Moveset Lab's editable document. Every setter takes the base-game value and the
    /// wanted value; the entry keeps only real changes (a value set back to its baseline is
    /// dropped), so a saved file stays minimal and each guard names the base-game value.
    /// Edits target a move name, which is a native move or one of this document's forks.
    /// </summary>
    public sealed class MovesetWorkingCopy
    {
        private readonly List<string> _undo = new List<string>();
        private readonly List<string> _redo = new List<string>();
        private const int MaxUndo = 100;
        public ModMovesetDocument Document { get; private set; }
        public string ModId { get; }
        public bool IsDirty { get; private set; }
        public event Action Changed;

        public MovesetWorkingCopy(string modId, ModMovesetDocument document = null)
        {
            ModId = modId ?? throw new ArgumentNullException(nameof(modId));
            Document = document ?? new ModMovesetDocument();
        }

        public bool CanUndo => _undo.Count != 0;
        public bool CanRedo => _redo.Count != 0;
        public string Json => ModMovesetJson.Write(Document);

        public void Undo() => Restore(_undo, _redo);
        public void Redo() => Restore(_redo, _undo);
        public void MarkSaved() => IsDirty = false;

        private void Restore(List<string> from, List<string> to)
        {
            if (from.Count == 0) return;
            to.Add(Json);
            string snapshot = from[from.Count - 1];
            from.RemoveAt(from.Count - 1);
            Document = ModMovesetJson.Parse(snapshot, "undo");
            IsDirty = true;
            Changed?.Invoke();
        }

        /// <summary>Runs one user edit as a single undo step.</summary>
        public void Edit(Action<MovesetWorkingCopy> change)
        {
            string before = Json;
            try { change(this); }
            catch { Document = ModMovesetJson.Parse(before, "rollback"); throw; }
            Prune();
            if (Json == before) return;
            _undo.Add(before);
            if (_undo.Count > MaxUndo) _undo.RemoveAt(0);
            _redo.Clear();
            IsDirty = true;
            Changed?.Invoke();
        }

        // ---- Lookups ----

        public string ForkName(ModMovesetFork fork) => ModId + "." + fork.Id;
        public ModMovesetFork FindFork(string runtimeName) => Document.Forks.FirstOrDefault(f => ForkName(f) == runtimeName);
        /// <summary>The native move a move name finally copies (itself when it is native).</summary>
        public string NativeSource(string move)
        {
            for (int guard = 0; guard < 64; guard++)
            {
                var fork = FindFork(move);
                if (fork == null) return move;
                move = fork.Move;
            }
            throw new InvalidOperationException("Fork chain is too deep.");
        }

        /// <summary>The entry holding edits of <paramref name="move"/> (native move or fork).</summary>
        public ModMovesetMove Entry(string move, bool create)
        {
            var fork = FindFork(move);
            if (fork != null) return fork;
            var entry = Document.Moves.FirstOrDefault(m => m.Move == move);
            if (entry == null && create) { entry = new ModMovesetMove { Move = move }; Document.Moves.Add(entry); }
            return entry;
        }

        // ---- Forks ----

        public ModMovesetFork CreateSubtypeFork(string source, string subtype) => CreateFork(source, subtype, null);
        public ModMovesetFork CreateItemFork(string source, string itemId) => CreateFork(source, null, itemId);

        private ModMovesetFork CreateFork(string source, string subtype, string itemId)
        {
            string scope = subtype ?? itemId.Substring(itemId.LastIndexOf('/') + 1);
            string baseId = Sanitize(source + "_" + scope);
            string id = baseId;
            for (int n = 2; Document.Forks.Any(f => f.Id == id); n++) id = baseId + "_" + n;
            var fork = new ModMovesetFork { Id = id, Move = source, Subtype = subtype, Item = itemId };
            Document.Forks.Add(fork);
            return fork;
        }

        public void RemoveFork(string runtimeName)
        {
            var fork = FindFork(runtimeName);
            if (fork == null) return;
            // Forks copying this fork go with it.
            foreach (var child in Document.Forks.Where(f => f.Move == runtimeName).ToList()) RemoveFork(ForkName(child));
            Document.Forks.Remove(fork);
        }

        private static string Sanitize(string value)
        {
            var chars = value.Select(c => char.IsLetterOrDigit(c) && c < 128 || c == '_' || c == '-' ? c : '_').ToArray();
            string result = new string(chars);
            return result.Length > 80 ? result.Substring(0, 80) : result;
        }

        // ---- Scalar fields ----

        public void SetPriority(string move, int baseline, int value) => Entry(move, true).Priority = value == baseline ? null : new ModMoveGuard<int>(baseline, value);

        /// <summary>Speed multiplier; 1.0 removes the edit.</summary>
        public void SetPlaybackRate(string move, double value)
        {
            int permille = (int)Math.Round(value * 1000);
            Entry(move, true).PlaybackRate = permille == Eclipse.Runtime.PlaybackTiming.Normal ? null : new ModMoveGuard<double>(1.0, permille / 1000.0);
        }

        public void SetDisabled(string move, bool disabled)
        {
            var entry = Entry(move, true);
            if (disabled) ClearEdits(entry);
            entry.Disable = disabled;
        }

        public void SetNativeAnimation(string move, string baselineFile, string file) =>
            Entry(move, true).Animation = file == baselineFile ? null : new ModMovesetAnimation { Expected = baselineFile, NativeValue = file };

        public void SetNote(string move, string note) => Entry(move, true).Note = note ?? string.Empty;

        // ---- Intervals ----

        public void SetIntervalBounds(string move, MovesetBaselineInterval baseline, int start, int? end)
        {
            var entry = Entry(move, true);
            var selector = baseline.Selector;
            entry.Intervals.RemoveAll(e => e.Select != null && e.Select.Equals(selector));
            bool changed = start != baseline.Start || end != baseline.End;
            if (changed) entry.Intervals.Add(ModMoveIntervalEdit.Bounds(selector, start != baseline.Start ? start : (int?)null, end != baseline.End ? end : null));
        }

        public void SetIntervalRemoved(string move, MovesetBaselineInterval baseline, bool removed)
        {
            var entry = Entry(move, true);
            var selector = baseline.Selector;
            entry.Intervals.RemoveAll(e => e.Select != null && e.Select.Equals(selector));
            if (removed) entry.Intervals.Add(ModMoveIntervalEdit.Removal(selector));
        }

        public void AddInterval(string move, string type, string name, int start, int? end) =>
            Entry(move, true).Intervals.Add(ModMoveIntervalEdit.Addition(type, name, start, end));

        public void RemoveAddedInterval(string move, ModMoveIntervalEdit added) => Entry(move, true).Intervals.Remove(added);

        // ---- Attacks ----

        /// <summary>The current attack edit with every field the user changed, rebuilt from parts.</summary>
        public void SetAttack(string move, MovesetBaselineAttack baseline, int start, int end, int baselineStart, int baselineEnd,
            double damage, IReadOnlyDictionary<string, double> terms, IReadOnlyList<string> edges, IReadOnlyList<double> impulse, string hit)
        {
            var entry = Entry(move, true);
            entry.Attacks.RemoveAll(a => a.Id == baseline.Id);
            bool SameTerms(IReadOnlyDictionary<string, double> a, IDictionary<string, double> b) => a.Count == b.Count && a.All(p => b.TryGetValue(p.Key, out double v) && (float)v == (float)p.Value);
            var startGuard = start != baselineStart ? new ModMoveGuard<int>(baselineStart, start) : null;
            var endGuard = end != baselineEnd ? new ModMoveGuard<int>(baselineEnd, end) : null;
            var damageGuard = (float)damage != (float)baseline.Damage ? new ModMoveGuard<double>(baseline.Damage, damage) : null;
            var termsGuard = terms != null && !SameTerms(terms, baseline.Terms)
                ? new ModMoveGuard<IReadOnlyDictionary<string, double>>(new Dictionary<string, double>(baseline.Terms), terms) : null;
            var edgesGuard = edges != null && !edges.SequenceEqual(baseline.Edges)
                ? new ModMoveGuard<IReadOnlyList<string>>(baseline.Edges.ToList().AsReadOnly(), edges) : null;
            var impulseGuard = impulse != null && !impulse.Select(v => (float)v).SequenceEqual(baseline.Impulse.Select(v => (float)v))
                ? new ModMoveGuard<IReadOnlyList<double>>(baseline.Impulse.ToList().AsReadOnly(), impulse) : null;
            var hitGuard = hit != null && baseline.Hit != null && hit != baseline.Hit ? new ModMoveGuard<string>(baseline.Hit, hit) : null;
            if (startGuard == null && endGuard == null && damageGuard == null && termsGuard == null && edgesGuard == null && impulseGuard == null && hitGuard == null) return;
            entry.Attacks.Add(new ModMoveAttackEdit(baseline.Id, startGuard, endGuard, damageGuard, termsGuard, edgesGuard, impulseGuard, hitGuard));
        }

        public ModMoveAttackEdit AttackEdit(string move, int id) => Entry(move, false)?.Attacks.FirstOrDefault(a => a.Id == id);

        // ---- Housekeeping ----

        private static void ClearEdits(ModMovesetMove entry)
        {
            entry.Priority = null; entry.PlaybackRate = null; entry.Animation = null; entry.Input = null; entry.SoundFrame = null;
            entry.Intervals.Clear(); entry.Attacks.Clear();
        }

        /// <summary>Drops move entries left without edits (forks stay: they matter by themselves).</summary>
        private void Prune() => Document.Moves.RemoveAll(m => !m.HasEdits);
    }

    /// <summary>Writes an editor-owned, data-only mod folder atomically.</summary>
    public static class MovesetModWriter
    {
        public const string MovesetFile = "movesets/moveset.json";

        /// <param name="dependencies">Other mods the file needs (owners of forked items), as id and minimum version.</param>
        public static string Save(string modsRoot, string modId, string displayName, ModMovesetDocument document, bool needsCore,
            IReadOnlyDictionary<string, string> dependencies = null)
        {
            dependencies = dependencies ?? new Dictionary<string, string>();
            ModId.Parse(modId);
            string root = System.IO.Path.Combine(modsRoot, modId);
            string manifestPath = System.IO.Path.Combine(root, "mod.toml");
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(root, "movesets"));
            if (System.IO.File.Exists(manifestPath))
            {
                var manifest = ModManifestReader.ReadExternalFile(manifestPath);
                if (manifest.Id.Value != modId) throw new ModContentException("Folder " + modId + " holds a different mod: " + manifest.Id + ".");
                if (manifest.HasEntrypoint) throw new ModContentException("Mod " + modId + " has a Lua entrypoint; the Moveset Lab only edits data-only mods.");
                bool hasPatch = manifest.Capabilities.Contains("content.patch");
                bool hasCore = manifest.Dependencies.Any(d => d.Id.Value == "core");
                bool hasOthers = dependencies.Keys.All(id => manifest.Dependencies.Any(d => d.Id.Value == id));
                if (!hasPatch || needsCore && !hasCore || !hasOthers)
                {
                    // Edit the author's file in place, so fields this writer does not know survive.
                    var missing = new Dictionary<string, string>(StringComparer.Ordinal);
                    if (needsCore && !hasCore) missing["core"] = ">=1.0 <2.0";
                    foreach (var pair in dependencies)
                        if (!manifest.Dependencies.Any(d => d.Id.Value == pair.Key)) missing[pair.Key] = ">=" + pair.Value;
                    string text = System.IO.File.ReadAllText(manifestPath).Replace("\r\n", "\n");
                    if (!hasPatch) text = AddPatchCapability(text);
                    if (!text.EndsWith("\n", StringComparison.Ordinal)) text += "\n";
                    foreach (var pair in missing.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                        text += "\n[[dependencies]]\nid = \"" + pair.Key + "\"\nversion = \"" + pair.Value + "\"\n";
                    WriteAtomic(manifestPath, text);
                    ModManifestReader.ReadExternalFile(manifestPath);
                }
            }
            else WriteAtomic(manifestPath, Manifest(modId, string.IsNullOrWhiteSpace(displayName) ? modId : displayName, "1.0.0", new[] { "Moveset Lab" }, true,
                dependencies.ToDictionary(pair => pair.Key, pair => ">=" + pair.Value)));
            WriteAtomic(System.IO.Path.Combine(root, MovesetFile.Replace('/', System.IO.Path.DirectorySeparatorChar)), ModMovesetJson.Write(document));
            return root;
        }

        /// <summary>Adds "content.patch" to a one-line capabilities array, or adds the key before the first table.</summary>
        private static string AddPatchCapability(string text)
        {
            var lines = text.Split('\n').ToList();
            int index = lines.FindIndex(line => System.Text.RegularExpressions.Regex.IsMatch(line, @"^\s*capabilities\s*=\s*\[.*\]\s*(#.*)?$"));
            if (index >= 0)
            {
                string line = lines[index];
                int open = line.IndexOf('[');
                bool empty = System.Text.RegularExpressions.Regex.IsMatch(line.Substring(open), @"^\[\s*\]");
                lines[index] = line.Substring(0, open + 1) + "\"content.patch\"" + (empty ? "" : ", ") + line.Substring(open + 1).TrimStart();
                return string.Join("\n", lines);
            }
            if (lines.Any(line => System.Text.RegularExpressions.Regex.IsMatch(line, @"^\s*capabilities\s*=")))
                throw new ModContentException("Add \"content.patch\" to capabilities in mod.toml; the Moveset Lab cannot edit a multi-line capabilities list.");
            int table = lines.FindIndex(line => line.TrimStart().StartsWith("[", StringComparison.Ordinal));
            lines.Insert(table < 0 ? lines.Count : table, "capabilities = [\"content.patch\"]");
            return string.Join("\n", lines);
        }

        private static string Manifest(string id, string name, string version, IEnumerable<string> authors, bool core, IDictionary<string, string> others)
        {
            string Quote(string s) => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
            var text = new System.Text.StringBuilder()
                .Append("schema = 1\n")
                .Append("id = ").Append(Quote(id)).Append('\n')
                .Append("name = ").Append(Quote(name)).Append('\n')
                .Append("version = ").Append(Quote(version)).Append('\n')
                .Append("authors = [").Append(string.Join(", ", authors.Select(Quote))).Append("]\n")
                .Append("capabilities = [\"content.patch\"]\n");
            if (core) text.Append("\n[[dependencies]]\nid = \"core\"\nversion = \">=1.0 <2.0\"\n");
            foreach (var pair in others.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                text.Append("\n[[dependencies]]\nid = ").Append(Quote(pair.Key)).Append("\nversion = ").Append(Quote(pair.Value)).Append('\n');
            return text.ToString();
        }

        private static void WriteAtomic(string path, string text)
        {
            string temp = path + ".eclipse-write";
            System.IO.File.WriteAllText(temp, text, new System.Text.UTF8Encoding(false));
            if (System.IO.File.Exists(path)) System.IO.File.Replace(temp, path, null);
            else System.IO.File.Move(temp, path);
        }

        public static ModMovesetDocument Load(string modsRoot, string modId)
        {
            string path = System.IO.Path.Combine(modsRoot, modId, MovesetFile.Replace('/', System.IO.Path.DirectorySeparatorChar));
            return System.IO.File.Exists(path) ? ModMovesetJson.Parse(System.IO.File.ReadAllText(path), MovesetFile) : new ModMovesetDocument();
        }
    }
}
