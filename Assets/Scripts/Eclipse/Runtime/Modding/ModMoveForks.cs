using System;
using System.Collections.Generic;

namespace Eclipse.Modding
{
    /// <summary>
    /// Removes one weapon subtype (or other item subtype) from a move's positive item
    /// lock group, so fighters using that subtype no longer get the move.
    /// </summary>
    public sealed class MoveItemLockRemoval
    {
        public string MoveName { get; }
        public string ItemType { get; }
        public string Subtype { get; }
        public MoveItemLockRemoval(string moveName, string itemType, string subtype)
        {
            MoveCombatPatch.ValidateName(moveName); MoveCombatPatch.ValidateName(itemType); MoveCombatPatch.ValidateName(subtype);
            MoveName = moveName; ItemType = itemType; Subtype = subtype;
        }
        internal string ConflictKey => MoveName + "\n" + ItemType + "\n" + Subtype;
    }

    /// <summary>Adds a negated item-name lock, so fighters using that one item no longer get the move.</summary>
    public sealed class MoveItemExclusion
    {
        public string MoveName { get; }
        public string ItemType { get; }
        public DefinitionId Item { get; }
        public string RuntimeItemName { get; }
        public MoveItemExclusion(string moveName, string itemType, DefinitionId item, string runtimeItemName)
        {
            MoveCombatPatch.ValidateName(moveName); MoveCombatPatch.ValidateName(itemType);
            if (string.IsNullOrWhiteSpace(runtimeItemName)) throw new ModContentException("Item exclusion requires a runtime item name.");
            MoveName = moveName; ItemType = itemType; Item = item; RuntimeItemName = runtimeItemName;
        }
        internal string ConflictKey => MoveName + "\n" + ItemType + "\n" + RuntimeItemName;
    }

    /// <summary>
    /// A copy of a move for one item subtype or one item. The copy keeps the source's
    /// definition and template names (so combos naming the source also match it), its
    /// lock is narrowed to the subtype or item, and the source stops matching them.
    /// An added move (<see cref="Adds"/>) is a copy beside its source instead: the source is
    /// untouched, the copy answers only to its own name and templates, and it may keep the
    /// source's locks (no subtype or item).
    /// </summary>
    public sealed class MoveForkDefinition
    {
        public ModId Owner { get; }
        public string LocalId { get; }
        public string RuntimeName { get; }
        public string Source { get; }
        public string ItemType { get; }
        /// <summary>Subtype scope, or null for an item fork.</summary>
        public string Subtype { get; }
        /// <summary>Item scope, or null for a subtype fork.</summary>
        public DefinitionId? Item { get; }
        public string RuntimeItemName { get; }
        /// <summary>A new move beside its source rather than a replacement of it.</summary>
        public bool Adds { get; }

        public MoveForkDefinition(ModId owner, string localId, string source, string itemType, string subtype, DefinitionId? item, string runtimeItemName,
            bool adds = false)
        {
            MoveCombatPatch.ValidateName(localId); MoveCombatPatch.ValidateName(source); MoveCombatPatch.ValidateName(itemType);
            if (subtype != null && item != null || !adds && subtype == null && item == null)
                throw new ModContentException(adds ? "A new move takes at most one of subtype or item." : "A move fork needs exactly one of subtype or item.");
            if (subtype != null) MoveCombatPatch.ValidateName(subtype);
            Owner = owner; LocalId = localId; Source = source; ItemType = itemType; Subtype = subtype; Item = item; RuntimeItemName = runtimeItemName; Adds = adds;
            RuntimeName = RuntimeNameFor(owner, localId);
            if (RuntimeName.Length > 128) throw new ModContentException("Move fork id is too long: " + localId);
        }

        public static string RuntimeNameFor(ModId owner, string localId) => owner.Value + "." + localId;
    }

    /// <summary>A moveset file that registered content (used by the online play policy).</summary>
    public sealed class ModMovesetFileRecord
    {
        public ModId Owner { get; }
        public string Path { get; }
        public ModMovesetFileRecord(ModId owner, string path) { Owner = owner; Path = path; }
    }

    public sealed partial class ModContentCatalog
    {
        private readonly List<ModMovesetFileRecord> _movesetFiles = new List<ModMovesetFileRecord>();
        public IReadOnlyList<ModMovesetFileRecord> MovesetFiles => _movesetFiles.AsReadOnly();
        internal void AddMovesetFiles(IEnumerable<ModMovesetFileRecord> files) => _movesetFiles.AddRange(files);
        private readonly List<MoveForkDefinition> _moveForks = new List<MoveForkDefinition>();
        private readonly List<MoveItemLockRemoval> _moveItemLockRemovals = new List<MoveItemLockRemoval>();
        private readonly List<MoveItemExclusion> _moveItemExclusions = new List<MoveItemExclusion>();
        public IReadOnlyList<MoveForkDefinition> MoveForks => _moveForks.AsReadOnly();
        public IReadOnlyList<MoveItemLockRemoval> MoveItemLockRemovals => _moveItemLockRemovals.AsReadOnly();
        public IReadOnlyList<MoveItemExclusion> MoveItemExclusions => _moveItemExclusions.AsReadOnly();

        internal bool HasMoveFork(string runtimeName)
        {
            foreach (var fork in _moveForks) if (fork.RuntimeName == runtimeName) return true;
            return false;
        }

        internal void ValidateForks(IReadOnlyList<MoveForkDefinition> forks, IReadOnlyList<MoveItemLockRemoval> removals, IReadOnlyList<MoveItemExclusion> exclusions)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var fork in _moveForks) names.Add(fork.RuntimeName);
            foreach (var fork in forks) if (!names.Add(fork.RuntimeName)) throw new ModContentException("Move fork already registered: " + fork.RuntimeName);
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var removal in _moveItemLockRemovals) keys.Add(removal.ConflictKey);
            foreach (var removal in removals)
                if (!keys.Add(removal.ConflictKey))
                    throw new ModContentException("Item lock removal already owned: " + removal.MoveName + " / " + removal.Subtype + ". Another mod already forks or narrows this move for that subtype.");
            keys.Clear();
            foreach (var exclusion in _moveItemExclusions) keys.Add(exclusion.ConflictKey);
            foreach (var exclusion in exclusions)
                if (!keys.Add(exclusion.ConflictKey))
                    throw new ModContentException("Item exclusion already owned: " + exclusion.MoveName + " / " + exclusion.RuntimeItemName + ". Another mod already forks this move for that item.");
        }

        internal void AddForks(IEnumerable<MoveForkDefinition> forks, IEnumerable<MoveItemLockRemoval> removals, IEnumerable<MoveItemExclusion> exclusions)
        {
            _moveForks.AddRange(forks); _moveItemLockRemovals.AddRange(removals); _moveItemExclusions.AddRange(exclusions);
        }
    }

    public sealed partial class ModRegistrationTransaction
    {
        private readonly List<ModMovesetFileRecord> _movesetFiles = new List<ModMovesetFileRecord>();
        public void RecordMovesetFile(string path) { ThrowIfCompleted(); _movesetFiles.Add(new ModMovesetFileRecord(Mod.Id, path)); }
        private readonly List<MoveForkDefinition> _moveForks = new List<MoveForkDefinition>();
        private readonly List<MoveItemLockRemoval> _moveItemLockRemovals = new List<MoveItemLockRemoval>();
        private readonly List<MoveItemExclusion> _moveItemExclusions = new List<MoveItemExclusion>();
        private int ForkRegistrationCount => _moveForks.Count + _moveItemLockRemovals.Count + _moveItemExclusions.Count;

        public string ForkRuntimeName(string localId) => MoveForkDefinition.RuntimeNameFor(Mod.Id, localId);

        public void RemoveMoveItemLock(string moveName, string itemType, string subtype)
        {
            ThrowIfCompleted();
            var removal = new MoveItemLockRemoval(moveName, itemType, subtype);
            foreach (var prior in _moveItemLockRemovals)
                if (prior.ConflictKey == removal.ConflictKey) throw new ModContentException("Duplicate item lock removal: " + moveName + " / " + subtype);
            EnsureCapacityForNewRegistration();
            _moveItemLockRemovals.Add(removal);
        }

        public void ExcludeMoveItem(string moveName, string itemReference)
        {
            ThrowIfCompleted();
            ResolveLockItem(itemReference, out var item, out string itemType, out string runtimeName);
            var exclusion = new MoveItemExclusion(moveName, itemType, item, runtimeName);
            foreach (var prior in _moveItemExclusions)
                if (prior.ConflictKey == exclusion.ConflictKey) throw new ModContentException("Duplicate item exclusion: " + moveName + " / " + item);
            EnsureCapacityForNewRegistration();
            _moveItemExclusions.Add(exclusion);
        }

        /// <summary>
        /// Registers a fork of <paramref name="source"/> (a native move, or an earlier fork's
        /// runtime name) for one subtype or one item, and the matching source lock edit. With
        /// <paramref name="adds"/>, registers a new move copied from the source instead: the
        /// source keeps its locks, and the subtype or item (both optional) limit only the copy.
        /// </summary>
        public MoveForkDefinition ForkMove(string localId, string source, string subtype, string itemReference, bool adds = false)
        {
            ThrowIfCompleted();
            if (subtype != null && itemReference != null || !adds && subtype == null && itemReference == null)
                throw new ModContentException(adds ? "A new move takes at most one of subtype or item." : "A move fork needs exactly one of subtype or item.");
            MoveForkDefinition fork;
            if (itemReference == null)
            {
                fork = new MoveForkDefinition(Mod.Id, localId, source, "Weapon", subtype, null, null, adds);
                foreach (var prior in _moveForks)
                    if (prior.RuntimeName == fork.RuntimeName) throw new ModContentException("Duplicate move fork: " + localId);
                if (!adds) RemoveMoveItemLock(source, "Weapon", subtype);
            }
            else
            {
                ResolveLockItem(itemReference, out var item, out string itemType, out string runtimeName);
                fork = new MoveForkDefinition(Mod.Id, localId, source, itemType, null, item, runtimeName, adds);
                foreach (var prior in _moveForks)
                    if (prior.RuntimeName == fork.RuntimeName) throw new ModContentException("Duplicate move fork: " + localId);
                if (!adds) ExcludeMoveItem(source, itemReference);
            }
            EnsureCapacityForNewRegistration();
            _moveForks.Add(fork);
            return fork;
        }

        private void ResolveLockItem(string reference, out DefinitionId id, out string itemType, out string runtimeName)
        {
            ItemDefinition item = GetItem(reference);
            id = item.Id;
            itemType = item is WeaponDefinition ? "Weapon" : item is ArmorDefinition ? "Armor" : item is HelmDefinition ? "Helm" :
                item is RangedDefinition ? "Ranged" : item is MagicDefinition ? "Magic" :
                throw new ModContentException("Move locks can name weapon, armor, helm, ranged or magic items, not '" + item.Id + "'.");
            runtimeName = item.IsCore && !string.IsNullOrEmpty(item.LegacyName) ? item.LegacyName : item.Id.ToString();
        }

        private void ValidateForkCommit()
        {
            // Forks of this mod's own forks must follow the fork they copy; other sources are
            // native moves or dependency forks, resolved when the batch is applied.
            var known = new HashSet<string>(StringComparer.Ordinal);
            foreach (var fork in _moveForks)
            {
                if (fork.Source.StartsWith(Mod.Id.Value + ".", StringComparison.Ordinal) && !known.Contains(fork.Source))
                    throw new ModContentException("Move fork source '" + fork.Source + "' must be registered earlier in this mod.");
                known.Add(fork.RuntimeName);
            }
            _catalog.ValidateForks(_moveForks, _moveItemLockRemovals, _moveItemExclusions);
        }

        private void ApplyForkCommit() { _catalog.AddForks(_moveForks, _moveItemLockRemovals, _moveItemExclusions); _catalog.AddMovesetFiles(_movesetFiles); }
        private void ClearForkPending() { _moveForks.Clear(); _moveItemLockRemovals.Clear(); _moveItemExclusions.Clear(); _movesetFiles.Clear(); }
    }

    public sealed partial class ModApiFacade
    {
        public string ForkMove(string localId, string source, string subtype, string itemReference)
        {
            RequireCapability("content.patch");
            return RequireRegistration().ForkMove(localId, source, subtype, itemReference).RuntimeName;
        }

        public void RemoveMoveItemLock(string moveName, string itemType, string subtype)
        {
            RequireCapability("content.patch");
            RequireRegistration().RemoveMoveItemLock(moveName, itemType, subtype);
        }

        public void ExcludeMoveItem(string moveName, string itemReference)
        {
            RequireCapability("content.patch");
            RequireRegistration().ExcludeMoveItem(moveName, itemReference);
        }
    }
}
