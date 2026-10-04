using System;
using System.Collections.Generic;

namespace Eclipse.Modding
{
    public static class ModActorLimits
    {
        public const int DefaultLifetimeFrames = 1800, MaximumLifetimeFrames = 36000;
        public const int MaximumPerMod = 4, MaximumPerFight = 8, MaximumQueriesPerCallback = 32;
        public const int MaximumEvents = 64;
    }
    public sealed class ActorDefinition
    {
        public DefinitionId Id { get; }
        public DefinitionId Character { get; }
        public bool OpposingTeam { get; }
        public bool AiControlled { get; }
        public int LifetimeFrames { get; }
        public float MaxHealth { get; }
        internal ActorDefinition(DefinitionId id, DefinitionId character, bool opposingTeam, bool ai, int lifetime, double maxHealth)
        {
            if (character.Category != "warriors" || lifetime < 1 || lifetime > ModActorLimits.MaximumLifetimeFrames)
                throw new ModContentException("Actors require a warrior and lifetime_frames in 1..36000.");
            if (double.IsNaN(maxHealth)||double.IsInfinity(maxHealth)||maxHealth<.01||maxHealth>100)
                throw new ModContentException("Actor max_health must be finite in 0.01..100.");
            Id = id; Character = character; OpposingTeam = opposingTeam; AiControlled = ai; LifetimeFrames = lifetime;
            MaxHealth=(float)maxHealth;
        }
    }
    public sealed class ModActorSnapshot
    {
        public string Id { get; } public string Definition { get; } public string Team { get; } public string Target { get; }
        public ModFighterSnapshot Fighter { get; }
        public int AgeFrames { get; } public int LifetimeFrames { get; }
        public ModActorSnapshot(string id, string definition, string team, string target, ModFighterSnapshot fighter, int age, int lifetime)
        { Id = id; Definition = definition; Team = team; Target = target; Fighter = fighter; AgeFrames = age; LifetimeFrames = lifetime; }
    }
    public sealed class ModActorEvent
    {
        public long Sequence { get; } public string Id { get; } public string Kind { get; } public int Frame { get; }
        public ModActorEvent(long sequence, string id, string kind, int frame)
        { Sequence = sequence; Id = id; Kind = kind; Frame = frame; }
    }
    public interface IModActor
    {
        bool TrySnapshot(out ModActorSnapshot snapshot, out string error);
        bool TryMoveBy(double x, double y, double z, out string error);
        bool TryChangeHealth(double amount, out string error);
        bool TryPlayMove(DefinitionId move, Action<bool,string> complete, out string error);
        bool TrySetTarget(string mainTarget, IModActor actorTarget, out string error);
        bool TryRemove(out string error);
    }
    public interface IModFighterActors
    {
        bool TrySpawnActor(ModId owner, DefinitionId definition, double x, double y, double z, Action<string,string> complete, out string error);
        bool TryGetActors(ModId owner, out IReadOnlyList<IModActor> actors, out string error);
        bool TryGetActorEvents(ModId owner, out IReadOnlyList<ModActorEvent> events, out string error);
    }
    public sealed partial class ModContentCatalog
    {
        private readonly DefinitionRegistry<ActorDefinition> _actors = new DefinitionRegistry<ActorDefinition>(v => v.Id);
        public IReadOnlyList<ActorDefinition> Actors => _actors.Values;
        public bool TryGetActor(DefinitionId id, out ActorDefinition definition) => _actors.TryGet(id, out definition);
        internal void ValidateActors(ActorDefinition[] definitions) => _actors.ValidateCanAdd(definitions);
        internal void AddActors(ActorDefinition[] definitions) => _actors.AddRange(definitions);
    }
    public sealed partial class ModRegistrationTransaction
    {
        private readonly Dictionary<DefinitionId,ActorDefinition> _pendingActors = new Dictionary<DefinitionId,ActorDefinition>();
        public ActorDefinition RegisterActor(string localId, DefinitionId character, bool opposingTeam, bool ai, int lifetime, double maxHealth)
        {
            ThrowIfCompleted(); var id = Qualify("actors", localId);
            ValidateDefinitionReferences(new[] { character }, "warriors", id, "character");
            var definition = new ActorDefinition(id, character, opposingTeam, ai, lifetime, maxHealth);
            AddP1D(_pendingActors, id, definition); return definition;
        }
        private void ValidateActorCommit() => _catalog.ValidateActors(SortedValues(_pendingActors));
        private void ApplyActorCommit() => _catalog.AddActors(SortedValues(_pendingActors));
    }
    public sealed partial class ModApiFacade
    {
        public ActorDefinition RegisterActor(string id, DefinitionId character, bool opposingTeam, bool ai, int lifetime, double maxHealth)
        { RequireCapability("content.register"); return RequireRegistration().RegisterActor(id, character, opposingTeam, ai, lifetime, maxHealth); }
    }
}
