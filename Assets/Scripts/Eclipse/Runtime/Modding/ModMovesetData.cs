using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Eclipse.Modding
{
    /// <summary>
    /// A declarative moveset file (<c>movesets/*.json</c>): guarded edits of native moves,
    /// the same operations as <c>sf2.moves.patch</c>, so editor-made mods need no Lua.
    /// </summary>
    public sealed class ModMovesetDocument
    {
        public const int CurrentSchema = 1;
        public const string DocumentKind = "eclipse.moveset";
        public string Name { get; set; } = string.Empty;
        public List<ModMovesetMove> Moves { get; } = new List<ModMovesetMove>();
        public List<ModMovesetFork> Forks { get; } = new List<ModMovesetFork>();
    }

    /// <summary>Edits of one native move. Every field except <see cref="Move"/> is optional.</summary>
    public class ModMovesetMove
    {
        public string Move { get; set; }
        public string Note { get; set; } = string.Empty;
        public bool Disable { get; set; }
        public ModMoveGuard<int> Priority { get; set; }
        /// <summary>Speed multiplier guard (1.0 = authored speed).</summary>
        public ModMoveGuard<double> PlaybackRate { get; set; }
        public ModMovesetAnimation Animation { get; set; }
        public ModMoveGuard<ModMoveInput> Input { get; set; }
        public ModMovesetSoundFrame SoundFrame { get; set; }
        public List<ModMoveIntervalEdit> Intervals { get; } = new List<ModMoveIntervalEdit>();
        public List<ModMoveAttackEdit> Attacks { get; } = new List<ModMoveAttackEdit>();

        public bool HasEdits => Disable || Priority != null || PlaybackRate != null || Animation != null || Input != null ||
            SoundFrame != null || Intervals.Count != 0 || Attacks.Count != 0;
    }

    /// <summary>
    /// A copy of a native move used only by one weapon subtype or one weapon item; the
    /// source move stops matching that subtype or item. Edits apply to the copy. With
    /// <see cref="Add"/> it is a new move beside the source instead (the source is unchanged),
    /// for the subtype or item when one is given, else for the same fighters as the source.
    /// </summary>
    public sealed class ModMovesetFork : ModMovesetMove
    {
        /// <summary>Local fork ID; the runtime move is named "&lt;mod&gt;.&lt;id&gt;".</summary>
        public string Id { get; set; }
        public string Subtype { get; set; }
        /// <summary>Definition ID of a weapon item, e.g. core:items/weapon/weapon_katana.</summary>
        public string Item { get; set; }
        /// <summary>A new move copied from <see cref="ModMovesetMove.Move"/>, not a replacement of it.</summary>
        public bool Add { get; set; }
    }

    public sealed class ModMovesetAnimation
    {
        public string Expected { get; set; }
        /// <summary>Another native .bytes clip, or null.</summary>
        public string NativeValue { get; set; }
        /// <summary>A binary asset of this mod (path or qualified ID), or null.</summary>
        public string AssetValue { get; set; }
    }

    public sealed class ModMovesetSoundFrame
    {
        public string Name { get; set; }
        public int Expected { get; set; }
        public int Value { get; set; }
    }

    /// <summary>Strict reader and canonical writer for moveset documents.</summary>
    public static class ModMovesetJson
    {
        public const int MaxBytes = 2 * 1024 * 1024;
        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        public static ModMovesetDocument Parse(string json, string source = "moveset")
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            if (Encoding.UTF8.GetByteCount(json) > MaxBytes) throw Fail(source, "is larger than " + MaxBytes / 1024 + " KiB");
            ModJsonNode root;
            try { root = ModJsonNode.Parse(json, 16); }
            catch (FormatException exception) { throw Fail(source, "is not valid JSON: " + exception.Message); }
            Obj(root, source);
            Fields(root, source, "schema", "kind", "name", "moves", "forks");
            if (Int(Required(root, "schema", source), source + ".schema") != ModMovesetDocument.CurrentSchema)
                throw Fail(source, "schema must be " + ModMovesetDocument.CurrentSchema);
            if (Str(Required(root, "kind", source), source + ".kind") != ModMovesetDocument.DocumentKind)
                throw Fail(source, "kind must be \"" + ModMovesetDocument.DocumentKind + "\"");
            var document = new ModMovesetDocument();
            if (root["name"] != null) document.Name = Str(root["name"], source + ".name");
            var moves = root["moves"] == null ? new List<ModJsonNode>() : Arr(root["moves"], source + ".moves");
            if (moves.Count > 2048) throw Fail(source, "lists more than 2048 moves");
            for (int i = 0; i < moves.Count; i++)
            {
                string where = source + ".moves[" + i + "]";
                var entry = Obj(moves[i], where);
                Fields(entry, where, MoveFields);
                var move = new ModMovesetMove();
                ReadMove(entry, move, where);
                document.Moves.Add(move);
            }
            var forks = root["forks"] == null ? new List<ModJsonNode>() : Arr(root["forks"], source + ".forks");
            if (forks.Count > 512) throw Fail(source, "lists more than 512 forks");
            for (int i = 0; i < forks.Count; i++)
            {
                string where = source + ".forks[" + i + "]";
                var entry = Obj(forks[i], where);
                Fields(entry, where, MoveFields.Concat(new[] { "id", "subtype", "item", "add" }).ToArray());
                var fork = new ModMovesetFork { Id = Str(Required(entry, "id", where), where + ".id") };
                MoveCombatPatch.ValidateName(fork.Id);
                if (entry["subtype"] != null) fork.Subtype = Str(entry["subtype"], where + ".subtype");
                if (entry["item"] != null) fork.Item = Str(entry["item"], where + ".item");
                if (entry["add"] != null) fork.Add = Bool(entry["add"], where + ".add");
                if (fork.Subtype != null && fork.Item != null) throw Fail(where, "takes at most one of subtype or item");
                if (!fork.Add && fork.Subtype == null && fork.Item == null) throw Fail(where, "needs exactly one of subtype or item (or \"add\": true for a new move)");
                ReadMove(entry, fork, where);
                document.Forks.Add(fork);
            }
            return document;
        }

        private static readonly string[] MoveFields = { "move", "note", "disable", "priority", "playback_rate", "animation", "input", "sound_frame", "intervals", "attacks" };

        private static void ReadMove(ModJsonNode entry, ModMovesetMove move, string where)
        {
            try
            {
                move.Move = Str(Required(entry, "move", where), where + ".move");
                MoveCombatPatch.ValidateName(move.Move);
                if (entry["note"] != null) move.Note = Str(entry["note"], where + ".note");
                if (move.Note.Length > 2000) throw Fail(where, "note is longer than 2000 characters");
                if (entry["disable"] != null) move.Disable = Bool(entry["disable"], where + ".disable");
                if (entry["priority"] != null) move.Priority = Guard(entry["priority"], where + ".priority", Int);
                if (entry["playback_rate"] != null)
                {
                    move.PlaybackRate = Guard(entry["playback_rate"], where + ".playback_rate", Num);
                    foreach (double rate in new[] { move.PlaybackRate.Expected, move.PlaybackRate.Value })
                        if (rate < Eclipse.Runtime.PlaybackTiming.Minimum / 1000.0 || rate > Eclipse.Runtime.PlaybackTiming.Maximum / 1000.0)
                            throw Fail(where, "playback_rate must be 0.5..2.0");
                }
                if (entry["input"] != null)
                {
                    move.Input = Guard(entry["input"], where + ".input", Input);
                    if (move.Input.Expected.SameAs(move.Input.Value)) throw Fail(where, "input must change the input");
                }
                if (entry["sound_frame"] != null)
                {
                    var sound = Obj(entry["sound_frame"], where + ".sound_frame");
                    Fields(sound, where + ".sound_frame", "name", "expected", "value");
                    move.SoundFrame = new ModMovesetSoundFrame { Name = Str(Required(sound, "name", where), where + ".sound_frame.name"),
                        Expected = Int(Required(sound, "expected", where), where + ".sound_frame.expected"), Value = Int(Required(sound, "value", where), where + ".sound_frame.value") };
                }
                if (entry["animation"] != null)
                {
                    var animation = Obj(entry["animation"], where + ".animation");
                    Fields(animation, where + ".animation", "expected", "value");
                    var result = new ModMovesetAnimation { Expected = Str(Required(animation, "expected", where), where + ".animation.expected") };
                    var value = Required(animation, "value", where);
                    if (value.Kind == ModJsonKind.String) result.NativeValue = Str(value, where + ".animation.value");
                    else
                    {
                        var asset = Obj(value, where + ".animation.value");
                        Fields(asset, where + ".animation.value", "asset");
                        result.AssetValue = Str(Required(asset, "asset", where), where + ".animation.value.asset");
                    }
                    move.Animation = result;
                }
                if (entry["intervals"] != null)
                {
                    var intervals = Arr(entry["intervals"], where + ".intervals");
                    for (int i = 0; i < intervals.Count; i++) move.Intervals.Add(ReadInterval(intervals[i], where + ".intervals[" + i + "]"));
                }
                if (entry["attacks"] != null)
                {
                    var attacks = Arr(entry["attacks"], where + ".attacks");
                    for (int i = 0; i < attacks.Count; i++) move.Attacks.Add(ReadAttack(attacks[i], where + ".attacks[" + i + "]"));
                }
                if (!(move is ModMovesetFork) && !move.HasEdits) throw Fail(where, "must change at least one field");
            }
            catch (ModContentException exception) when (!exception.Message.StartsWith(where, StringComparison.Ordinal))
            {
                throw new ModContentException(where + ": " + exception.Message, exception);
            }
        }

        private static ModMoveIntervalSelector ReadSelector(ModJsonNode node, string where)
        {
            var select = Obj(node, where);
            Fields(select, where, "type", "name", "start", "end");
            return new ModMoveIntervalSelector(select["type"] == null ? "" : Str(select["type"], where + ".type"),
                select["name"] == null ? "" : Str(select["name"], where + ".name"),
                select["start"] == null ? 0 : Int(select["start"], where + ".start"),
                select["end"] == null ? (int?)null : Int(select["end"], where + ".end"));
        }

        private static ModMoveIntervalEdit ReadInterval(ModJsonNode node, string where)
        {
            var entry = Obj(node, where);
            Fields(entry, where, "select", "add", "remove", "start", "end");
            if (entry["add"] != null)
            {
                if (entry.Count != 1) throw Fail(where, "uses add alone; put start and end inside add");
                var add = Obj(entry["add"], where + ".add");
                Fields(add, where + ".add", "type", "name", "start", "end");
                return ModMoveIntervalEdit.Addition(add["type"] == null ? "" : Str(add["type"], where + ".add.type"),
                    add["name"] == null ? "" : Str(add["name"], where + ".add.name"),
                    Int(Required(add, "start", where + ".add"), where + ".add.start"),
                    add["end"] == null ? (int?)null : Int(add["end"], where + ".add.end"));
            }
            var select = ReadSelector(Required(entry, "select", where), where + ".select");
            if (entry["remove"] != null && Bool(entry["remove"], where + ".remove"))
            {
                if (entry["start"] != null || entry["end"] != null) throw Fail(where, "cannot both remove and set bounds");
                return ModMoveIntervalEdit.Removal(select);
            }
            return ModMoveIntervalEdit.Bounds(select, entry["start"] == null ? (int?)null : Int(entry["start"], where + ".start"),
                entry["end"] == null ? (int?)null : Int(entry["end"], where + ".end"));
        }

        private static ModMoveAttackEdit ReadAttack(ModJsonNode node, string where)
        {
            var entry = Obj(node, where);
            Fields(entry, where, "id", "start", "end", "damage", "damage_terms", "edges", "impulse", "hit");
            return new ModMoveAttackEdit(Int(Required(entry, "id", where), where + ".id"),
                entry["start"] == null ? null : Guard(entry["start"], where + ".start", Int),
                entry["end"] == null ? null : Guard(entry["end"], where + ".end", Int),
                entry["damage"] == null ? null : Guard(entry["damage"], where + ".damage", Num),
                entry["damage_terms"] == null ? null : Guard(entry["damage_terms"], where + ".damage_terms", Terms),
                entry["edges"] == null ? null : Guard(entry["edges"], where + ".edges", Strings),
                entry["impulse"] == null ? null : Guard(entry["impulse"], where + ".impulse", Impulse),
                entry["hit"] == null ? null : Guard(entry["hit"], where + ".hit", Str));
        }

        // ---- Canonical writer: fixed key order, invariant numbers, stable diffs. ----

        public static string Write(ModMovesetDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            var root = ModJsonNode.NewObject()
                .Set("schema", ModJsonNode.Of(ModMovesetDocument.CurrentSchema))
                .Set("kind", ModJsonNode.Of(ModMovesetDocument.DocumentKind));
            if (!string.IsNullOrEmpty(document.Name)) root.Set("name", ModJsonNode.Of(document.Name));
            var moves = ModJsonNode.NewArray();
            foreach (var move in document.Moves) moves.Add(WriteMove(ModJsonNode.NewObject(), move));
            root.Set("moves", moves);
            if (document.Forks.Count != 0)
            {
                var forks = ModJsonNode.NewArray();
                foreach (var fork in document.Forks)
                {
                    var node = ModJsonNode.NewObject().Set("id", ModJsonNode.Of(fork.Id));
                    if (fork.Subtype != null) node.Set("subtype", ModJsonNode.Of(fork.Subtype));
                    if (fork.Item != null) node.Set("item", ModJsonNode.Of(fork.Item));
                    if (fork.Add) node.Set("add", ModJsonNode.Of(true));
                    forks.Add(WriteMove(node, fork));
                }
                root.Set("forks", forks);
            }
            return root.ToJson();
        }

        private static ModJsonNode WriteMove(ModJsonNode node, ModMovesetMove move)
        {
            node.Set("move", ModJsonNode.Of(move.Move));
            if (!string.IsNullOrEmpty(move.Note)) node.Set("note", ModJsonNode.Of(move.Note));
            if (move.Disable) node.Set("disable", ModJsonNode.Of(true));
            WriteGuard(node, "priority", move.Priority, ModJsonNode.Of);
            WriteGuard(node, "playback_rate", move.PlaybackRate, ModJsonNode.Of);
            if (move.Animation != null)
                node.Set("animation", ModJsonNode.NewObject().Set("expected", ModJsonNode.Of(move.Animation.Expected))
                    .Set("value", move.Animation.NativeValue != null ? ModJsonNode.Of(move.Animation.NativeValue)
                        : ModJsonNode.NewObject().Set("asset", ModJsonNode.Of(move.Animation.AssetValue))));
            WriteGuard(node, "input", move.Input, WriteInput);
            if (move.SoundFrame != null)
                node.Set("sound_frame", ModJsonNode.NewObject().Set("name", ModJsonNode.Of(move.SoundFrame.Name))
                    .Set("expected", ModJsonNode.Of(move.SoundFrame.Expected)).Set("value", ModJsonNode.Of(move.SoundFrame.Value)));
            if (move.Intervals.Count != 0)
            {
                var intervals = ModJsonNode.NewArray();
                foreach (var edit in move.Intervals)
                {
                    var entry = ModJsonNode.NewObject();
                    if (edit.Kind == ModMoveIntervalEditKind.Add)
                    {
                        var add = ModJsonNode.NewObject();
                        if (edit.AddType.Length != 0) add.Set("type", ModJsonNode.Of(edit.AddType));
                        if (edit.AddName.Length != 0) add.Set("name", ModJsonNode.Of(edit.AddName));
                        add.Set("start", ModJsonNode.Of(edit.Start.Value));
                        if (edit.End.HasValue) add.Set("end", ModJsonNode.Of(edit.End.Value));
                        entry.Set("add", add);
                    }
                    else
                    {
                        var select = ModJsonNode.NewObject();
                        if (edit.Select.Type.Length != 0) select.Set("type", ModJsonNode.Of(edit.Select.Type));
                        if (edit.Select.Name.Length != 0) select.Set("name", ModJsonNode.Of(edit.Select.Name));
                        select.Set("start", ModJsonNode.Of(edit.Select.Start));
                        if (edit.Select.End.HasValue) select.Set("end", ModJsonNode.Of(edit.Select.End.Value));
                        entry.Set("select", select);
                        if (edit.Kind == ModMoveIntervalEditKind.Remove) entry.Set("remove", ModJsonNode.Of(true));
                        else
                        {
                            if (edit.Start.HasValue) entry.Set("start", ModJsonNode.Of(edit.Start.Value));
                            if (edit.End.HasValue) entry.Set("end", ModJsonNode.Of(edit.End.Value));
                        }
                    }
                    intervals.Add(entry);
                }
                node.Set("intervals", intervals);
            }
            if (move.Attacks.Count != 0)
            {
                var attacks = ModJsonNode.NewArray();
                foreach (var attack in move.Attacks)
                {
                    var entry = ModJsonNode.NewObject().Set("id", ModJsonNode.Of(attack.Id));
                    WriteGuard(entry, "start", attack.Start, ModJsonNode.Of);
                    WriteGuard(entry, "end", attack.End, ModJsonNode.Of);
                    WriteGuard(entry, "damage", attack.Damage, ModJsonNode.Of);
                    WriteGuard(entry, "damage_terms", attack.DamageTerms, terms =>
                    {
                        var map = ModJsonNode.NewObject();
                        foreach (string type in ModMoveCombatTermOrder.Ordered(terms.Keys)) map.Set(type, ModJsonNode.Of(terms[type]));
                        return map;
                    });
                    WriteGuard(entry, "edges", attack.Edges, edges => { var list = ModJsonNode.NewArray(); foreach (string edge in edges) list.Add(ModJsonNode.Of(edge)); return list; });
                    WriteGuard(entry, "impulse", attack.Impulse, axes => { var list = ModJsonNode.NewArray(); foreach (double axis in axes) list.Add(ModJsonNode.Of(axis)); return list; });
                    WriteGuard(entry, "hit", attack.Hit, ModJsonNode.Of);
                    attacks.Add(entry);
                }
                node.Set("attacks", attacks);
            }
            return node;
        }

        /// <summary>A single tapped key stays a plain string, as in the original input form.</summary>
        private static ModJsonNode WriteInput(ModMoveInput input)
        {
            if (input.SingleKey != null) return ModJsonNode.Of(input.SingleKey);
            var chords = ModJsonNode.NewArray();
            foreach (var chord in input.Chords)
            {
                var keys = ModJsonNode.NewArray();
                foreach (var key in chord)
                    keys.Add(key.Press == "Tap" ? ModJsonNode.Of(key.Key)
                        : ModJsonNode.NewObject().Set("key", ModJsonNode.Of(key.Key)).Set("press", ModJsonNode.Of(key.Press)));
                chords.Add(keys);
            }
            return chords;
        }

        private static void WriteGuard<T>(ModJsonNode node, string name, ModMoveGuard<T> guard, Func<T, ModJsonNode> write)
        {
            if (guard == null) return;
            node.Set(name, ModJsonNode.NewObject().Set("expected", write(guard.Expected)).Set("value", write(guard.Value)));
        }

        // ---- Strict value helpers ----

        private static ModContentException Fail(string where, string message) => new ModContentException(where + " " + message + ".");
        private static void Fields(ModJsonNode value, string where, params string[] allowed)
        {
            foreach (var member in value.Members)
                if (Array.IndexOf(allowed, member.Key) < 0) throw Fail(where, "has unknown field '" + member.Key + "'");
        }
        private static ModJsonNode Required(ModJsonNode value, string name, string where) => value[name] ?? throw Fail(where, "requires " + name);
        private static ModJsonNode Obj(ModJsonNode node, string where) => node.Kind == ModJsonKind.Object ? node : throw Fail(where, "must be an object");
        private static List<ModJsonNode> Arr(ModJsonNode node, string where) => node.Kind == ModJsonKind.Array ? node.Items : throw Fail(where, "must be an array");
        private static string Str(ModJsonNode node, string where) =>
            node.Kind == ModJsonKind.String && node.String.Length != 0 ? node.String : throw Fail(where, "must be a non-empty string");
        private static bool Bool(ModJsonNode node, string where) => node.Kind == ModJsonKind.Boolean ? node.Boolean : throw Fail(where, "must be true or false");
        private static int Int(ModJsonNode node, string where) =>
            node.Kind == ModJsonKind.Number && node.IsInteger && node.Number >= int.MinValue && node.Number <= int.MaxValue ? (int)node.Number : throw Fail(where, "must be an integer");
        private static double Num(ModJsonNode node, string where) => node.Kind == ModJsonKind.Number ? node.Number : throw Fail(where, "must be a number");
        private static IReadOnlyList<string> Strings(ModJsonNode node, string where) =>
            Arr(node, where).Select((item, i) => Str(item, where + "[" + i + "]")).ToList().AsReadOnly();
        private static IReadOnlyList<double> Impulse(ModJsonNode node, string where)
        {
            var items = Arr(node, where);
            if (items.Count != 3) throw Fail(where, "must be [x, y, z]");
            return items.Select((item, i) => Num(item, where + "[" + i + "]")).ToList().AsReadOnly();
        }
        private static IReadOnlyDictionary<string, double> Terms(ModJsonNode node, string where)
        {
            var map = Obj(node, where);
            var result = new Dictionary<string, double>(StringComparer.Ordinal);
            foreach (var member in map.Members) result[member.Key] = Num(member.Value, where + "." + member.Key);
            return result;
        }
        /// <summary>
        /// A key input: one tapped key as a string ("Kick"), or an array of alternative chords,
        /// each an array of keys; a key is a name (tapped) or { "key", "press" }. [] is no input.
        /// </summary>
        private static ModMoveInput Input(ModJsonNode node, string where)
        {
            try
            {
                if (node.Kind == ModJsonKind.String) return ModMoveInput.Single(Str(node, where));
                var chords = new List<List<ModMoveKey>>();
                var alternatives = Arr(node, where);
                for (int c = 0; c < alternatives.Count; c++)
                {
                    string at = where + "[" + c + "]";
                    var keys = new List<ModMoveKey>();
                    foreach (var (item, k) in Arr(alternatives[c], at).Select((item, k) => (item, k)))
                    {
                        if (item.Kind == ModJsonKind.String) { keys.Add(new ModMoveKey(Str(item, at + "[" + k + "]"))); continue; }
                        var key = Obj(item, at + "[" + k + "]");
                        Fields(key, at + "[" + k + "]", "key", "press");
                        keys.Add(new ModMoveKey(Str(Required(key, "key", at), at + "[" + k + "].key"),
                            key["press"] == null ? "Tap" : Str(key["press"], at + "[" + k + "].press")));
                    }
                    chords.Add(keys);
                }
                return new ModMoveInput(chords);
            }
            catch (ModContentException exception) when (!exception.Message.StartsWith(where, StringComparison.Ordinal))
            {
                throw Fail(where, exception.Message.TrimEnd('.'));
            }
        }

        private static ModMoveGuard<T> Guard<T>(ModJsonNode node, string where, Func<ModJsonNode, string, T> read)
        {
            var guard = Obj(node, where);
            Fields(guard, where, "expected", "value");
            return new ModMoveGuard<T>(read(Required(guard, "expected", where), where + ".expected"), read(Required(guard, "value", where), where + ".value"));
        }

        internal static string Decode(byte[] data, string source)
        {
            if (data.Length > MaxBytes) throw Fail(source, "is larger than " + MaxBytes / 1024 + " KiB");
            try { return StrictUtf8.GetString(data); }
            catch (DecoderFallbackException exception) { throw new ModContentException(source + " is not valid UTF-8.", exception); }
        }
    }

    /// <summary>Loads <c>movesets/*.json</c> into a mod's registration transaction.</summary>
    public static class ModMovesetLoader
    {
        public const string Folder = "movesets/";

        public static int Load(ModDescriptor mod, AssetResolver assets, ModRegistrationTransaction registration)
        {
            if (mod == null) throw new ArgumentNullException(nameof(mod));
            if (assets == null) throw new ArgumentNullException(nameof(assets));
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (!assets.TryGetProvider(mod.Id, out var provider) || !(provider is IAssetEnumerableProvider enumerable)) return 0;
            var files = new List<AssetMetadata>();
            foreach (AssetMetadata metadata in enumerable.Assets)
                if (metadata.Kind == AssetKind.Text && metadata.Format == ".json" && metadata.Id.Path.StartsWith(Folder, StringComparison.Ordinal))
                    files.Add(metadata);
            files.Sort((a, b) => string.CompareOrdinal(a.Id.Path, b.Id.Path));
            int count = 0;
            foreach (var metadata in files)
            {
                string source = metadata.Id.Path + ".json";
                if (!assets.TryRead(metadata.Id, out AssetBytes bytes)) throw new ModContentException("Moveset file disappeared: " + source + ".");
                var document = ModMovesetJson.Parse(ModMovesetJson.Decode(bytes.Data, source), source);
                if ((document.Moves.Count != 0 || document.Forks.Count != 0) && !HasCapability(mod, "content.patch"))
                    throw new ModContentException(source + " edits moves; declare the content.patch capability in mod.toml.");
                foreach (var move in document.Moves) { Register(mod, assets, registration, move, source); count++; }
                foreach (var fork in document.Forks) { registration.ForkMove(fork.Id, fork.Move, fork.Subtype, fork.Item, fork.Add); if (fork.HasEdits) Register(mod, assets, registration, fork, source, registration.ForkRuntimeName(fork.Id)); count++; }
                if (document.Moves.Count != 0 || document.Forks.Count != 0) registration.RecordMovesetFile(source);
            }
            return count;
        }

        private static bool HasCapability(ModDescriptor mod, string capability)
        {
            foreach (string declared in mod.Manifest.Capabilities) if (declared == capability) return true;
            return false;
        }

        private static void Register(ModDescriptor mod, AssetResolver assets, ModRegistrationTransaction registration,
            ModMovesetMove move, string source, string runtimeName = null)
        {
            try
            {
                ModMoveAnimationPatch animation = null;
                if (move.Animation != null)
                {
                    if (move.Animation.NativeValue != null) animation = new ModMoveAnimationPatch(move.Animation.Expected, move.Animation.NativeValue);
                    else
                    {
                        AssetId asset = assets.Qualify(mod.Id, move.Animation.AssetValue);
                        if (asset.Namespace != mod.Id) throw new ModContentException("animation asset must belong to this mod");
                        if (!assets.TryDescribe(asset, out AssetMetadata metadata) || metadata.Kind != AssetKind.Binary)
                            throw new ModContentException("animation asset '" + asset + "' is not a binary file in this mod");
                        animation = new ModMoveAnimationPatch(move.Animation.Expected, asset);
                    }
                }
                ModMoveGuard<int> rate = move.PlaybackRate == null ? null :
                    new ModMoveGuard<int>((int)Math.Round(move.PlaybackRate.Expected * 1000), (int)Math.Round(move.PlaybackRate.Value * 1000));
                registration.PatchMove(runtimeName ?? move.Move, null, null, null,
                    move.SoundFrame == null ? null : new ModMoveFramePatch(move.SoundFrame.Name, move.SoundFrame.Expected, move.SoundFrame.Value),
                    move.Disable, move.Input == null ? null : new ModMoveInputPatch(move.Input.Expected, move.Input.Value),
                    move.Priority == null ? null : new ModMovePriorityPatch(move.Priority.Expected, move.Priority.Value),
                    null, animation, null, null, new ModMoveCombatExtras(move.Intervals, move.Attacks, rate));
            }
            catch (ModContentException exception)
            {
                throw new ModContentException(source + " (" + move.Move + "): " + exception.Message, exception);
            }
        }
    }
}
