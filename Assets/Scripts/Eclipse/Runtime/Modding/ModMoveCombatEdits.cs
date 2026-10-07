using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse.Modding
{
    /// <summary>
    /// Selects exactly one authored non-attack interval of a native move by its
    /// identity: Type attribute, Name attribute, Start and End. A null End means the
    /// interval has no End attribute (it runs to the end of the move).
    /// </summary>
    public sealed class ModMoveIntervalSelector : IEquatable<ModMoveIntervalSelector>
    {
        public string Type { get; }
        public string Name { get; }
        public int Start { get; }
        public int? End { get; }

        public ModMoveIntervalSelector(string type, string name, int start, int? end)
        {
            Type = type ?? string.Empty; Name = name ?? string.Empty;
            if (Type.Length == 0 && Name.Length == 0)
                throw new ModContentException("An interval selector needs a type or a name.");
            if (Type.Length != 0) MoveCombatPatch.ValidateName(Type);
            if (Name.Length != 0) MoveCombatPatch.ValidateName(Name);
            if (Type == "Attack") throw new ModContentException("Select attack intervals through attacks by id, not intervals.");
            ModMoveIntervalEdit.ValidateBounds(start, end);
            Start = start; End = end;
        }

        public bool Equals(ModMoveIntervalSelector other) =>
            other != null && Type == other.Type && Name == other.Name && Start == other.Start && End == other.End;
        public override bool Equals(object obj) => Equals(obj as ModMoveIntervalSelector);
        public override int GetHashCode() => (Type + "\n" + Name + "\n" + Start + "\n" + End).GetHashCode();
        public override string ToString() => (Type.Length != 0 ? Type : "") + (Name.Length != 0 ? "/" + Name : "") + " " + Start + ".." + (End?.ToString() ?? "open");
    }

    public enum ModMoveIntervalEditKind { Bounds, Remove, Add }

    /// <summary>One interval change in a guarded move patch: new bounds, removal, or addition.</summary>
    public sealed class ModMoveIntervalEdit
    {
        // Non-attack interval types the runtime constructs for an added interval.
        internal static readonly string[] AddableTypes = { "", "Block", "Invulnerable", "Invisible", "Throwable" };

        public ModMoveIntervalEditKind Kind { get; }
        public ModMoveIntervalSelector Select { get; }
        /// <summary>Bounds: replacement start (null keeps it). Add: start.</summary>
        public int? Start { get; }
        /// <summary>Bounds: replacement end (null keeps it). Add: end, or null for an open end.</summary>
        public int? End { get; }
        public string AddType { get; }
        public string AddName { get; }

        private ModMoveIntervalEdit(ModMoveIntervalEditKind kind, ModMoveIntervalSelector select, int? start, int? end, string addType, string addName)
        {
            Kind = kind; Select = select; Start = start; End = end; AddType = addType ?? string.Empty; AddName = addName ?? string.Empty;
        }

        public static ModMoveIntervalEdit Bounds(ModMoveIntervalSelector select, int? start, int? end)
        {
            if (select == null) throw new ModContentException("An interval bounds edit needs select.");
            if (!start.HasValue && !end.HasValue) throw new ModContentException("An interval bounds edit needs start or end.");
            int newStart = start ?? select.Start; int? newEnd = end ?? select.End;
            ValidateBounds(newStart, newEnd);
            if (newStart == select.Start && newEnd == select.End) throw new ModContentException("An interval bounds edit must change start or end.");
            return new ModMoveIntervalEdit(ModMoveIntervalEditKind.Bounds, select, start, end, null, null);
        }

        public static ModMoveIntervalEdit Removal(ModMoveIntervalSelector select)
        {
            if (select == null) throw new ModContentException("An interval removal needs select.");
            return new ModMoveIntervalEdit(ModMoveIntervalEditKind.Remove, select, null, null, null, null);
        }

        public static ModMoveIntervalEdit Addition(string type, string name, int start, int? end)
        {
            type = type ?? string.Empty; name = name ?? string.Empty;
            if (Array.IndexOf(AddableTypes, type) < 0)
                throw new ModContentException("An added interval type must be Block, Invulnerable, Invisible, Throwable or omitted.");
            if (type.Length == 0 && name.Length == 0) throw new ModContentException("An added interval needs a type or a name.");
            if (name.Length != 0) MoveCombatPatch.ValidateName(name);
            ValidateBounds(start, end);
            return new ModMoveIntervalEdit(ModMoveIntervalEditKind.Add, null, start, end, type, name);
        }

        internal static void ValidateBounds(int start, int? end)
        {
            if (start < 0 || start > 100000 || (end.HasValue && (end.Value < start || end.Value > 100000)))
                throw new ModContentException("Interval bounds must be ordered frames in 0..100000.");
        }
    }

    /// <summary>
    /// A new attack added to an existing move: an attack interval with its own id, frames,
    /// damage, damage terms, attacking edges, impulse and hit reaction, built like the attacks
    /// of moves registered from Lua.
    /// </summary>
    public sealed class ModMoveAttackAddition
    {
        public const int MaxId = 100000;
        public int Id { get; }
        public int Start { get; }
        public int End { get; }
        public double Damage { get; }
        /// <summary>Damage terms keyed by type (Shift values), 1..4 of them.</summary>
        public IReadOnlyDictionary<string, double> Terms { get; }
        public IReadOnlyList<string> Edges { get; }
        /// <summary>[x, y, z].</summary>
        public IReadOnlyList<double> Impulse { get; }
        public string Hit { get; }

        public ModMoveAttackAddition(int id, int start, int end, double damage, IReadOnlyDictionary<string, double> terms,
            IReadOnlyList<string> edges, IReadOnlyList<double> impulse, string hit)
        {
            if (id < 0 || id > MaxId) throw new ModContentException("A new attack id must be 0.." + MaxId + ".");
            ModMoveIntervalEdit.ValidateBounds(start, end);
            if (double.IsNaN(damage) || damage < 0 || damage > 16) throw new ModContentException("A new attack's damage must be 0..16.");
            if (terms == null || terms.Count < 1 || terms.Count > 4) throw new ModContentException("A new attack needs 1..4 damage terms.");
            foreach (var term in terms) new ModMoveDamageTerm(term.Key, term.Value);
            if (edges == null || edges.Count < 1 || edges.Count > 64 || edges.Distinct(StringComparer.Ordinal).Count() != edges.Count)
                throw new ModContentException("A new attack needs 1..64 distinct attacking edges.");
            foreach (string edge in edges) MoveCombatPatch.ValidateName(edge);
            if (impulse == null || impulse.Count != 3 || impulse.Any(v => double.IsNaN(v) || double.IsInfinity(v) || Math.Abs(v) > 100000))
                throw new ModContentException("A new attack's impulse must be [x, y, z] in -100000..100000.");
            if (Array.IndexOf(ModMoveAttack.NativeHitReactions, hit) < 0) throw new ModContentException("Unknown hit reaction '" + hit + "'.");
            Id = id; Start = start; End = end; Damage = damage;
            Terms = new Dictionary<string, double>(terms, StringComparer.Ordinal);
            Edges = edges.ToList().AsReadOnly(); Impulse = impulse.ToList().AsReadOnly(); Hit = hit;
        }
    }

    /// <summary>A guarded value: the native value the patch expects, and its replacement.</summary>
    public sealed class ModMoveGuard<T>
    {
        public T Expected { get; }
        public T Value { get; }
        public ModMoveGuard(T expected, T value) { Expected = expected; Value = value; }
    }

    /// <summary>
    /// Guarded edits to one attack interval, selected by its authored ID. Every field
    /// names the native value it expects so changed base data fails explicitly.
    /// </summary>
    public sealed class ModMoveAttackEdit
    {
        // Term types that change blockability or use a raid resource; edits keep them as authored.
        private static readonly string[] SpecialTerms = { "RangedDamage", "MagicDamage", "RaidChargeDamage" };

        public int Id { get; }
        public ModMoveGuard<int> Start { get; }
        public ModMoveGuard<int> End { get; }
        public ModMoveGuard<double> Damage { get; }
        /// <summary>Damage terms keyed by type (Shift values); order is not significant.</summary>
        public ModMoveGuard<IReadOnlyDictionary<string, double>> DamageTerms { get; }
        public ModMoveGuard<IReadOnlyList<string>> Edges { get; }
        public ModMoveGuard<IReadOnlyList<double>> Impulse { get; }
        public ModMoveGuard<string> Hit { get; }

        public ModMoveAttackEdit(int id, ModMoveGuard<int> start = null, ModMoveGuard<int> end = null,
            ModMoveGuard<double> damage = null, ModMoveGuard<IReadOnlyDictionary<string, double>> damageTerms = null,
            ModMoveGuard<IReadOnlyList<string>> edges = null, ModMoveGuard<IReadOnlyList<double>> impulse = null,
            ModMoveGuard<string> hit = null)
        {
            if (id < 0 || id > 999) throw new ModContentException("Attack edit id must be 0..999.");
            if (start == null && end == null && damage == null && damageTerms == null && edges == null && impulse == null && hit == null)
                throw new ModContentException("Attack edit " + id + " must change at least one field.");
            foreach (var frame in new[] { start, end })
                if (frame != null && (frame.Expected < 0 || frame.Expected > 100000 || frame.Value < 0 || frame.Value > 100000 || frame.Expected == frame.Value))
                    throw new ModContentException("Attack start/end edits need distinct frames in 0..100000.");
            if (start != null && end != null && start.Value > end.Value) throw new ModContentException("Attack start must not be after its end.");
            if (damage != null)
                foreach (double value in new[] { damage.Expected, damage.Value })
                    if (double.IsNaN(value) || double.IsInfinity(value) || value < 0 || value > 16)
                        throw new ModContentException("Attack damage must be 0..16.");
            if (damageTerms != null)
            {
                if (damageTerms.Expected == null || damageTerms.Value == null || damageTerms.Expected.Count == 0 ||
                    damageTerms.Value.Count < 1 || damageTerms.Value.Count > 4)
                    throw new ModContentException("damage_terms edits need expected terms and 1..4 replacement terms.");
                foreach (var pair in damageTerms.Expected)
                {
                    MoveCombatPatch.ValidateName(pair.Key);
                    if (double.IsNaN(pair.Value) || double.IsInfinity(pair.Value)) throw new ModContentException("Damage term shifts must be finite.");
                }
                foreach (var pair in damageTerms.Value) new ModMoveDamageTerm(pair.Key, pair.Value);
                foreach (string special in SpecialTerms)
                    if (damageTerms.Expected.ContainsKey(special) != damageTerms.Value.ContainsKey(special))
                        throw new ModContentException("damage_terms edits cannot add or remove " + special + ".");
            }
            if (edges != null)
            {
                if (edges.Expected == null || edges.Value == null || edges.Value.Count < 1 || edges.Value.Count > 64)
                    throw new ModContentException("Attack edge edits need expected edges and 1..64 replacement edges.");
                foreach (string edge in edges.Expected.Concat(edges.Value))
                    if (string.IsNullOrWhiteSpace(edge) || edge.Length > 128) throw new ModContentException("Invalid attack edge name.");
                if (new HashSet<string>(edges.Value, StringComparer.Ordinal).Count != edges.Value.Count)
                    throw new ModContentException("Attack edge edits cannot repeat an edge.");
            }
            if (impulse != null)
            {
                if (impulse.Expected == null || impulse.Value == null || impulse.Expected.Count != 3 || impulse.Value.Count != 3)
                    throw new ModContentException("Attack impulse edits need x, y and z.");
                foreach (double value in impulse.Expected.Concat(impulse.Value))
                    if (double.IsNaN(value) || double.IsInfinity(value) || Math.Abs(value) > 100000)
                        throw new ModContentException("Invalid attack impulse.");
            }
            if (hit != null && (Array.IndexOf(ModMoveAttack.NativeHitReactions, hit.Expected) < 0 ||
                Array.IndexOf(ModMoveAttack.NativeHitReactions, hit.Value) < 0 || hit.Expected == hit.Value))
                throw new ModContentException("Attack hit edits need distinct native hit reactions.");
            Id = id; Start = start; End = end; Damage = damage; DamageTerms = damageTerms; Edges = edges; Impulse = impulse; Hit = hit;
        }

        public bool ChangesBounds => Start != null || End != null;
    }
}

namespace Eclipse.Modding
{
    /// <summary>Canonical order for written damage terms (the Lua short-form order).</summary>
    public static class ModMoveCombatTermOrder
    {
        public static readonly string[] Order = { "WeaponDamage", "RangedDamage", "MagicDamage", "UnarmedDamage" };

        public static IEnumerable<string> Ordered(IEnumerable<string> types) =>
            types.OrderBy(type => { int index = Array.IndexOf(Order, type); return index < 0 ? int.MaxValue : index; })
                 .ThenBy(type => type, StringComparer.Ordinal);
    }
}
