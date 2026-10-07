using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;
using Nekki.SF2.Core.Fights.Controller;
using Nekki.SF2.GUI.Fight;
using Eclipse.Modding;
using Eclipse.Underworld;
using UnityEngine;

public partial class Fight
{
    internal bool IsTitleSparring { get; private set; }
    internal float GetTitleSparringCenterX(float alpha)
    {
        Eclipse.Rendering.Interpolation.FightInterpolation.SamplePosition(_playerModel.GetBodyObject().GetCenterOfMassNode(), alpha,
            out float leftX, out _, out _);
        Eclipse.Rendering.Interpolation.FightInterpolation.SamplePosition(_enemyModel.GetBodyObject().GetCenterOfMassNode(), alpha,
            out float rightX, out _, out _);
        return (leftX + rightX) * .5f;
    }
    private float _titleOldLeftWall, _titleOldRightWall;
    private int _titleOldSpeed;
    private bool _titleOldAiOn;

    // A presentation encounter: no campaign boot, HUD, controller, rules, rewards,
    // music or Lua lifecycle. Models still use native AI, animation and hit physics.
    internal static Fight CreateTitleSparring(Location location, Render render,
        ModelParameters left, ModelParameters right, float leftX, float rightX, float minX, float maxX)
    {
        if (GetCurrentFight() != null) throw new InvalidOperationException("A fight already owns the combat engine.");
        var fight = new Fight();
        fight.IsTitleSparring = true;
        fight._titleOldLeftWall = GameUtils.GetLeftWall();
        fight._titleOldRightWall = GameUtils.GetRightWall();
        fight._titleOldSpeed = GameUtils.GetSlowMode();
        fight._titleOldAiOn = ModelAi.get_AiOn();
        _currentFight = fight;
        try
        {
            fight._UnityObject = new GameObject("Title sparring simulation");
            fight._location = location;
            fight.FightDefinition = new FightList { TrackFightProgress = false, HealthRecovery = 1f };
            fight.FightDefinition.set_Type(BattleType.FightPVP);
            fight.playerParameters = left;
            fight.enemyParameters = right;
            fight.enemyParametersList = new List<ModelParameters> { right };
            fight._rulesInspector = new RulesInspector(fight, fight.FightDefinition);
            left.SpawnPosition.Set(leftX, location.playerStartPosition.GetY(), 0f);
            right.SpawnPosition.Set(rightX, location.enemyStartPosition.GetY(), 0f);
            GameUtils.SetLeftWall(minX);
            GameUtils.SetRightWall(maxX);
            GameUtils.SetSlowMode(1);
            fight._Camera = new Camera(fight._UnityObject.transform);
            fight._Camera.InitTitleBackdrop(location, render);
            fight._playerModel = fight.AddModel(left);
            fight._enemyModel = fight.AddModel(right);
            fight.ResetParameters();
            left.set_IsImmortalityEnabled(true);
            right.set_IsImmortalityEnabled(true);
            fight.round.round = 1;
            fight.round.processing = true;
            fight.SetStage(StageType.Stage.STAGE_FIGHT);
            fight.ActionModels(true);
            // Gameplay normally enters through the start-stance stage. A title
            // encounter skips that presentation, so seed native idle selection
            // explicitly; the AI cannot decide until its first move exists.
            fight._SelectAnimation.PrepareFormAnimation(fight._playerModel);
            fight._SelectAnimation.PrepareFormAnimation(fight._enemyModel);
            ModelAi.set_AiOn(true);
            return fight;
        }
        catch
        {
            fight.DisposeTitleSparring();
            throw;
        }
    }

    private Fight() { }

    internal bool AdvanceTitleSparring()
    {
        if (!IsTitleSparring || GetCurrentFight() != this) return false;
        fightTimeInFrame++;
        perksStage.Render();
        foreach (var model in ActiveModels) model.Render();
        if (pendingModels.Count > 0)
        {
            ActiveModels.AddRange(pendingModels);
            pendingModels.Clear();
        }
        RenderCollisions();
        _SelectAnimation.UpdateConditions();
        foreach (var model in ActiveModels) model.RenderAi();
        _SelectAnimation.Render();
        perksStage.ResetInfoPerks();
        // Native magic effects are manually ticked by RenderFight, rather than
        // CocosAnimation.Update. Title combat needs the same advance/expiry pass.
        _Camera.GetRender().GetBackgroundEffects().UpdateEffects();
        _Camera.GetRender().GetForegroundEffects().UpdateEffects();
        _Camera.GetRender().UpdateHitEffect();
        ProcessRemovedModels();
        ResetModelsHitData();
        frame++;
        // Native immortality preserves hit reactions while keeping both CPUs alive.
        // Presentation sparring has no round timer or knockout/rematch interruption.
        return true;
    }

    internal void DisposeTitleSparring()
    {
        if (!IsTitleSparring) return;
        Eclipse.UI.EclipseUiAudio.StopTitleFightSounds();
        try
        {
            // The renderer survives title rematches; its old looping effects
            // must not survive the fighters that owned them.
            _Camera?.GetRender()?.UpdateEffects();
            var models = new HashSet<Model>(ActiveModels);
            models.UnionWith(pendingModels);
            models.UnionWith(modelsToRemove);
            foreach (var model in models)
            {
                try { RemoveModel(model); }
                catch (Exception error) { UnityEngine.Debug.LogWarning("[Title] Fighter cleanup: " + error); }
            }
            ActiveModels.Clear();
            pendingModels.Clear();
            modelsToRemove.Clear();
            _SelectAnimation.ClearModelsAndEvents();
            perksStage.Reset();
        }
        finally
        {
            if (_currentFight == this)
            {
                _currentFight = null;
                GameUtils.SetLeftWall(_titleOldLeftWall);
                GameUtils.SetRightWall(_titleOldRightWall);
                GameUtils.SetSlowMode(_titleOldSpeed);
                ModelAi.set_AiOn(_titleOldAiOn);
            }
            IsTitleSparring = false;
            if (_UnityObject != null) UnityEngine.Object.Destroy(_UnityObject);
        }
    }

    public bool IsLocalVersus => FightDefinition is Eclipse.Multiplayer.LocalVersusMatch;
    internal sealed class PreparedFormModel : IDisposable
    {
        private Model _model;
        internal Model Model => _model;

        internal PreparedFormModel(ModelParameters destination)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            // Never let preparation mutate a catalog definition or the live fighter's parameters.
            var parameters = new ModelParameters(destination);
            parameters.SceneType = SceneTypes.SceneFight;
            ModelLoader.RequireModelDocuments(parameters.ModelDocuments);
            var model = new Model(parameters);
            try
            {
                model.GetGameObject().SetActive(false);
                model.AttachToParent();
                _model = model;
            }
            catch
            {
                model.DestroyModel();
                throw;
            }
        }

        // Call only after registration succeeds; failed/unclaimed preparation owns cleanup.
        internal Model Take()
        {
            if (_model == null) throw new InvalidOperationException("Prepared form was already consumed or disposed.");
            var model = _model;
            _model = null;
            return model;
        }

        public void Dispose()
        {
            var model = _model;
            _model = null;
            if (model != null) model.DestroyModel();
        }
    }

    private sealed class PendingModelTransition
    {
        internal Model Model;
        internal int Round;
        internal Action Apply;
        internal Action<Exception> Complete;
    }

    // A reversible registration stage for the form coordinator. Resource
    // ownership and visibility remain with the caller until it commits.
    internal sealed class FormRenderBindings : IDisposable
    {
        private Fight _fight;
        private readonly Model _expected, _replacement;
        private readonly bool _player, _actor;
        private bool _camera, _animation, _rules;
        private Action _restoreAnimationEvents;
        private Action _restoreQueuedPerks;
        private Action _restoreActiveEffects;
        private Action _restorePerkRegistration;
        private Action _restoreParticipant;
        private Action _restoreActorOwners;
        private Action _restoreCombatState;
        private Action _restorePresentation;
        private readonly List<Action> _restoreEnemyTargets = new List<Action>();

        internal FormRenderBindings(Fight fight, Model expected, Model replacement)
        {
            if (fight == null || expected == null || replacement == null || expected == replacement)
                throw new ArgumentException("Form binding requires two distinct models and a fight.");
            if (!fight.IsEclipseFormParticipant(expected))
                throw new InvalidOperationException("The original fighter is no longer active.");
            _fight = fight; _expected = expected; _replacement = replacement;
            _player = expected == fight._playerModel;
            _actor = fight.IsEclipseActorModel(expected);
            try
            {
                var rules = _actor ? null : fight._rulesInspector.PrepareModelRebind(expected, replacement);
                _restoreAnimationEvents = fight._SelectAnimation.CapturePendingEvents();
                if (!fight._Camera.ReplaceModel(expected, replacement, _player))
                    throw new InvalidOperationException("Camera rejected the form replacement.");
                _camera = true;
                var observers = new HashSet<Model>(fight.ActiveModels);
                foreach (var model in fight.ActiveModels)
                    if (model != null)
                        foreach (var weapon in model.GetWeaponModels()) observers.Add(weapon);
                foreach (var observer in observers)
                    if (observer != null && observer != replacement && observer.GetRootModel() != expected &&
                        observer._Enemies.Contains(expected))
                        _restoreEnemyTargets.Add(observer.ReplaceEnemyForm(expected, replacement));
                if (!fight._SelectAnimation.ReplaceModel(expected, replacement))
                    throw new InvalidOperationException("Animation selection rejected the form replacement.");
                _animation = true;
                _restoreQueuedPerks = fight.perksStage.RebindQueuedFormActions(expected, replacement);
                _restoreActiveEffects = fight.perksStage.TransferFormEffects(expected, replacement);
                _restorePerkRegistration = fight.perksStage.ReplaceFormRegistration(expected, replacement);
                if (rules != null) { rules(); _rules = true; }
                _restoreCombatState = expected.TransferFormCombatState(replacement);
                _restoreParticipant = _actor ? fight.BindEclipseActorFormParticipant(expected, replacement) :
                    fight.BindFormParticipant(expected, replacement);
                _restoreActorOwners = fight.BindEclipseActorOwnerForm(expected, replacement);
                _restorePresentation = fight.BindFormPresentation(expected, replacement, _player, _actor);
            }
            catch (Exception original)
            {
                try { Dispose(); }
                catch (Exception rollback) { throw new AggregateException("Form binding and restoration failed.", original, rollback); }
                throw;
            }
        }

        internal void Commit()
        {
            if (_fight == null) throw new InvalidOperationException("Form bindings were already completed.");
            _fight = null;
        }

        internal bool Owns(Fight fight, Model expected, Model replacement)
        {
            return _fight == fight && _expected == expected && _replacement == replacement;
        }

        public void Dispose()
        {
            var fight = _fight;
            _fight = null;
            if (fight == null) return;
            var failures = new List<Exception>();
            if (_restorePresentation != null) try { _restorePresentation(); }
                catch (Exception exception) { failures.Add(exception); }
            if (_restoreActorOwners != null) try { _restoreActorOwners(); }
                catch (Exception exception) { failures.Add(exception); }
            if (_restoreParticipant != null) try { _restoreParticipant(); }
                catch (Exception exception) { failures.Add(exception); }
            if (_restoreCombatState != null) try { _restoreCombatState(); }
                catch (Exception exception) { failures.Add(exception); }
            if (_restorePerkRegistration != null) try { _restorePerkRegistration(); }
                catch (Exception exception) { failures.Add(exception); }
            if (_restoreActiveEffects != null) try { _restoreActiveEffects(); }
                catch (Exception exception) { failures.Add(exception); }
            if (_restoreQueuedPerks != null) try { _restoreQueuedPerks(); }
                catch (Exception exception) { failures.Add(exception); }
            if (_rules) try { fight._rulesInspector.PrepareModelRebind(_replacement, _expected)(); }
                catch (Exception exception) { failures.Add(exception); }
            for (int index = _restoreEnemyTargets.Count - 1; index >= 0; index--)
                try { _restoreEnemyTargets[index](); }
                catch (Exception exception) { failures.Add(exception); }
            if (_animation) try {
                if (!fight._SelectAnimation.ReplaceModel(_replacement, _expected))
                    throw new InvalidOperationException("Could not restore animation selection.");
                _restoreAnimationEvents();
            } catch (Exception exception) { failures.Add(exception); }
            if (_camera) try {
                if (!fight._Camera.ReplaceModel(_replacement, _expected, _player))
                    throw new InvalidOperationException("Could not restore camera registration.");
            } catch (Exception exception) { failures.Add(exception); }
            if (failures.Count != 0) throw new AggregateException("Could not restore form registrations.", failures);
        }
    }

    private readonly Dictionary<Model, PendingModelTransition> _modelTransitions = new Dictionary<Model, PendingModelTransition>();
    private readonly HashSet<Model> _retiredFormBodies = new HashSet<Model>();
    private bool _drainingModelTransitions;
    private bool _modelTransitionsClosed;
    private PendingModelTransition _applyingModelTransition;

    // Host-only: the caller must prepare/validate replacement resources first.
    // Success here means queued, not applied. Completion runs at the frame boundary.
    internal bool QueueModelTransition(Model model, Action apply, Action<Exception> complete)
    {
        if (_modelTransitionsClosed || model == null || apply == null || complete == null || !round.processing ||
            _eclipseFightEndDispatched || !IsEclipseFormParticipant(model) ||
            model.GetLife() <= 0 || _modelTransitions.ContainsKey(model)) return false;
        _modelTransitions.Add(model, new PendingModelTransition {
            Model = model, Round = round.round, Apply = apply, Complete = complete });
        return true;
    }

    private void DrainModelTransitions()
    {
        if (_drainingModelTransitions || _modelTransitions.Count == 0) return;
        // Form swaps call back into mod code and toggle bodies; they run on confirmed input only.
        if (IsLocalVersus && Eclipse.Multiplayer.VersusTickDriver.Barrier()) return;
        _drainingModelTransitions = true;
        try
        {
            // Requests made by completion handlers wait for another simulation step.
            var pending = new List<PendingModelTransition>(_modelTransitions.Values);
            foreach (var request in pending)
            {
                if (!_modelTransitions.TryGetValue(request.Model, out var current) || current != request) continue;
                Exception failure = null;
                try
                {
                    if (_modelTransitionsClosed || !round.processing || _eclipseFightEndDispatched || request.Round != round.round ||
                        !IsEclipseFormParticipant(request.Model) || request.Model.GetLife() <= 0)
                        throw new OperationCanceledException("Fighter or round ended before the model transition.");
                    _applyingModelTransition = request;
                    request.Apply();
                }
                catch (Exception exception) { failure = exception; }
                finally { _applyingModelTransition = null; _modelTransitions.Remove(request.Model); }
                try { request.Complete(failure); }
                catch (Exception exception) { UnityEngine.Debug.LogException(exception); }
            }
        }
        finally { _drainingModelTransitions = false; }
    }

    private void CloseModelTransitions()
    {
        _eclipseCamera.Clear();
        CancelEclipseProjectiles();
        CancelEclipseActors("round_ended");
        CancelEclipseFighterMotion();
        CancelEclipseFighterPlayback();
        _modelTransitionsClosed = true;
        var pending = new List<PendingModelTransition>(_modelTransitions.Values);
        foreach (var request in pending)
        {
            if (request == _applyingModelTransition) continue;
            if (!_modelTransitions.Remove(request.Model)) continue;
            try { request.Complete(new OperationCanceledException("Fight unloaded before the model transition.")); }
            catch (Exception exception) { UnityEngine.Debug.LogException(exception); }
        }
    }

    // Participant identity must move with the body: round results and behavior
    // state otherwise keep addressing the retired fighter after a visual swap.
    internal Action BindFormParticipant(Model expected, Model replacement)
    {
        if (expected == null || replacement == null || expected == replacement)
            throw new ArgumentException("Form participants must be distinct.");
        bool player = expected == _playerModel;
        int index = ActiveModels.IndexOf(expected);
        if ((!player && expected != _enemyModel) || index < 0 || ActiveModels.Contains(replacement))
            throw new InvalidOperationException("Form participant identity is stale.");
        var originalParameters = expected.Parameters;
        var parameters = replacement.Parameters;
        if (parameters == originalParameters || parameters.IsPlayer != player ||
            (player ? playerParameters : enemyParameters) != originalParameters ||
            (!player && (currentEnemyIndex < 0 || currentEnemyIndex >= enemyParametersList.Count ||
                enemyParametersList[currentEnemyIndex] != originalParameters)))
            throw new InvalidOperationException("Form parameters do not match the active participant.");
        if (_eclipseShields.ContainsKey(replacement))
            throw new InvalidOperationException("Replacement already owns combat state.");
        var opponentState = CaptureFormBehaviorKeys(_eclipseOpponentInstances, expected, replacement);
        var innateState = CaptureFormBehaviorKeys(_eclipseInnateInstances, expected, replacement);
        var tactic = enemyTactic;
        bool hasShield = _eclipseShields.TryGetValue(expected, out var shield);
        ActiveModels[index] = replacement;
        if (player) { _playerModel = replacement; playerParameters = parameters; }
        else
        {
            _enemyModel = replacement; enemyParameters = parameters;
            enemyParametersList[currentEnemyIndex] = parameters; enemyTactic = parameters.FightTactic;
        }
        if (hasShield) { _eclipseShields.Remove(expected); _eclipseShields.Add(replacement, shield); }
        MoveFormBehaviorKeys(_eclipseOpponentInstances, opponentState, expected, replacement);
        MoveFormBehaviorKeys(_eclipseInnateInstances, innateState, expected, replacement);
        bool restored = false;
        return () =>
        {
            if (restored) return;
            restored = true;
            ActiveModels[index] = expected;
            if (player) { _playerModel = expected; playerParameters = originalParameters; }
            else
            {
                _enemyModel = expected; enemyParameters = originalParameters;
                enemyParametersList[currentEnemyIndex] = originalParameters; enemyTactic = tactic;
            }
            if (hasShield) { _eclipseShields.Remove(replacement); _eclipseShields.Add(expected, shield); }
            MoveFormBehaviorKeys(_eclipseOpponentInstances, opponentState, replacement, expected);
            MoveFormBehaviorKeys(_eclipseInnateInstances, innateState, replacement, expected);
        };
    }

    private static List<DefinitionId> CaptureFormBehaviorKeys(
        Dictionary<(Model, DefinitionId), System.Xml.XmlNode> instances, Model expected, Model replacement)
    {
        var keys = new List<DefinitionId>();
        foreach (var pair in instances)
        {
            if (pair.Key.Item1 == replacement)
                throw new InvalidOperationException("Replacement already owns behavior state.");
            if (pair.Key.Item1 == expected) keys.Add(pair.Key.Item2);
        }
        return keys;
    }

    private static void MoveFormBehaviorKeys(Dictionary<(Model, DefinitionId), System.Xml.XmlNode> instances,
        List<DefinitionId> keys, Model expected, Model replacement)
    {
        foreach (var key in keys)
        {
            var state = instances[(expected, key)];
            instances.Remove((expected, key));
            instances.Add((replacement, key), state);
        }
    }

	private class MagicBuffer
	{
		public int MagicCharges;

		public float MagicChargeFraction;
	}

	private class FightListParametersBuffer
	{
		public int ParameterA;

		public int ParameterB;

		public int ParameterC;
	}

	private class FightIntPair
	{
		public int First;

		public int Second;
	}

	private class GameOverParameters
	{
		public ModelParameters Winner;

		public ModelParameters Loser;

		public GameOverTypes GameOverType;
	}

	private class RoundParam
	{
		public float Life;

		public float MagicChargeFraction;

		public int MagicCharges;

		public int RaidCharges;

		public float DamageMultiplier;

		public int RoundsWon;
	}

		private sealed class EclipseFighterOperations : IModFighterOperations, IModDamageEventSource, IModFighterTargets, IModIncomingHitSource, IModFighterEffects, IModCombatSnapshotSource, IModCombatActivitySource, IModFighterForms, IModFighterStatusIcons, IModAnimationLifecycleSource, IModFighterFlags, IModFighterControls, IModFighterButtons, IModRoundOutcomes, IModFighterMotion, IModFighterPlayback, IModFighterRegions, IModFighterArtwork, IModFighterCamera, IModFighterProjectiles, IModFighterProjectileSpawning, IModFighterActors, IModActorBehaviorSource
	{
		private readonly Fight _fight;
		private readonly Model _model;
        private readonly bool _controlSetup;
        public IModActor Actor => _fight != null && _model != null && _fight._eclipseActors.TryGetValue(_model,out var actor) ? actor : null;
        private bool ArenaAvailable => _fight != null && GetCurrentFight() == _fight && !_fight.IsLocalVersus && !_fight.IsTitleSparring &&
            _fight.FightDefinition != null && _fight.FightDefinition.get_Type() != BattleType.FightNone &&
            _fight.FightDefinition.get_Type() != BattleType.FightPVP && (!_fight.get_IsRaidFight() || ModModeRuntime.IsRaid(_fight.FightDefinition)) &&
            _fight.round.processing && !_fight._eclipseFightEndDispatched && _fight._eclipseEndedRound != _fight.round.round &&
            _model != null && (_model == _fight.GetPlayerModel() || _model == _fight.GetEnemyModel());
        public bool TryOverlapRect(ModArenaRect rect, out bool overlaps, out string error)
        {
            if (!ArenaAvailable) { overlaps = false; error = "Arena geometry requires a current main fighter in an active offline round."; return false; }
            return ModArenaGeometry.TryOverlap(_model, rect, out overlaps, out error);
        }
        public bool TryAcquireCamera(ModId owner, ModCameraSettings settings, out IModCameraControl camera, out string error)
        { return _fight.TryAcquireEclipseCamera(_model, owner, settings, out camera, out error); }
        public bool TryMarkRect(ModArenaRect rect, ModUiColor color, out IModArenaMarker marker, out string error)
        {
            return TryCreateArenaArtwork(rect, color, null, out marker, out error);
        }
        public bool TryMarkSprite(AssetId sprite, ModArenaRect rect, ModUiColor color, out IModArenaMarker marker, out string error)
        {
            return TryCreateArenaArtwork(rect, color, sprite, out marker, out error);
        }
        private bool TryCreateArenaArtwork(ModArenaRect rect, ModUiColor color, AssetId? sprite, out IModArenaMarker marker, out string error)
        {
            if (!ArenaAvailable) { marker = null; error = "Arena markers require a current main fighter in an active offline round."; return false; }
            int roundNumber = _fight.round.round;
            return ModArenaMarkerRenderer.TryCreate(rect, color,
                () => GetCurrentFight() == _fight && _fight.round.processing && _fight.round.round == roundNumber &&
                    !_fight._eclipseFightEndDispatched && _fight._eclipseEndedRound != roundNumber,
                () => _fight.GetPlayerModel()?.GetRenderObject()?.transform, out marker, out error, sprite);
        }

        public bool TrySpawnActor(ModId owner, DefinitionId definition, double x, double y, double z, Action<string,string> complete, out string error)
        { return _fight.TryQueueEclipseActor(_model, owner, definition, x, y, z, complete, out error); }
        public bool TryGetActors(ModId owner, out IReadOnlyList<IModActor> actors, out string error)
        { return _fight.TryGetEclipseActors(_model, owner, out actors, out error); }
        public bool TryGetActorEvents(ModId owner, out IReadOnlyList<ModActorEvent> events, out string error)
        { return _fight.TryGetEclipseActorEvents(_model, owner, out events, out error); }

        public bool TrySpawnProjectile(ModId owner, DefinitionId definition, double x, double y, double z,
            Action<string, string> complete, out string error)
        {
            if (_fight == null) { error = "Fight is unavailable."; return false; }
            return _fight.TryQueueEclipseProjectileSpawn(_model, owner, definition, x, y, z, complete, out error);
        }

        public bool TryGetProjectiles(ModId owner, out IReadOnlyList<IModProjectile> projectiles, out string error)
        {
            if (_fight != null) return _fight.TryGetEclipseProjectiles(_model, owner, out projectiles, out error);
            projectiles = null; error = "Fight is unavailable."; return false;
        }

        public bool TryPlayMove(DefinitionId move, Action<bool, string> complete, out string error)
        {
            if (_fight == null) { error = "Fight is unavailable."; return false; }
            if (Actor is OwnedActor actor) return actor.TryPlayMove(move,complete,out error);
            return _fight.TryQueueEclipseFighterPlayback(_model, move, complete, out error);
        }
        public bool TryMoveBy(double x, double y, double z, out string error)
        {
            if (_fight == null) { error = "Fight is unavailable."; return false; }
            if (Actor is OwnedActor actor) return actor.TryMoveBy(x,y,z,out error);
            return _fight.TryQueueEclipseFighterMotion(_model, x, y, z, out error);
        }
        public bool TryEndRound(DefinitionId rule, bool playerWins, out string error)
        {
            if (_fight == null || _model == null || (_model != _fight.GetPlayerModel() && _model != _fight.GetEnemyModel()))
            { error = "A main fighter is required."; return false; }
            return _fight.TryQueueRoundOutcome(rule, playerWins, out error);
        }
        public bool TryChangeForm(DefinitionId character, Action<bool, string> complete, out string error)
        {
            if (_fight == null || _model == null || complete == null)
            { error = "Fighter is unavailable."; return false; }
            return _fight.TryQueueCharacterForm(_model, character,
                failure => complete(failure == null, failure?.Message ?? string.Empty), out error);
        }

        public bool TrySetControlBlocked(object owner, string control, bool blocked, out string error)
        {
            try
            {
                if (_fight == null || _model == null || _model != _fight._playerModel ||
                    _fight.IsLocalVersus || (!_fight.round.processing && !_controlSetup) ||
                    _fight._eclipseEndedRound == _fight.round.round || _fight._eclipseFightEndDispatched || _fight.Controller == null)
                    throw new InvalidOperationException("Control restrictions require the player during an active round.");
                FightCID action;
                switch (control)
                {
                    case "punch": action = FightCID.Punch; break;
                    case "kick": action = FightCID.Kick; break;
                    case "ranged": action = FightCID.MissileButton; break;
                    case "magic": action = FightCID.MagicButton; break;
                    case "raid_charge": action = FightCID.RaidChargeButton; break;
                    default: throw new ArgumentException("Unknown combat control.", nameof(control));
                }
                _fight.Controller.SetScriptControlBlocked(owner, action, blocked);
                error = string.Empty;
                return true;
            }
            catch (Exception exception) { error = exception.Message; return false; }
        }

        public bool TrySetButtonCooldown(string control, int frames, out string error)
        {
            try
            {
                if (_fight == null || _model == null || !_fight.round.processing || _fight._eclipseFightEndDispatched ||
                    (_model != _fight._playerModel && _model != _fight._enemyModel))
                    throw new InvalidOperationException("Button cooldowns require an active fighter and round.");
                if (frames < 1 || frames > 3600) throw new ArgumentOutOfRangeException(nameof(frames), "frames must be 1..3600.");
                FightCID action;
                switch (control)
                {
                    case "punch": action = FightCID.Punch; break;
                    case "kick": action = FightCID.Kick; break;
                    case "ranged": action = FightCID.MissileButton; break;
                    case "raid_charge": action = FightCID.RaidChargeButton; break;
                    default: throw new ArgumentException("Unknown cooldown control.", nameof(control));
                }
                // Same sequence as the legacy SetCooldown perk action (InfoPerk).
                _model.ResetButtonCooldown(action, 0);
                _model.StartButtonCooldown(action, frames);
                error = string.Empty;
                return true;
            }
            catch (Exception exception) { error = exception.Message; return false; }
        }

        public bool TrySetControlVisible(string control, bool visible, out string error)
        {
            try
            {
                if (_fight == null || _model == null || !_fight.round.processing || _fight._eclipseFightEndDispatched ||
                    (_model != _fight._playerModel && _model != _fight._enemyModel))
                    throw new InvalidOperationException("Button visibility requires an active fighter and round.");
                if (control != "raid_charge") throw new ArgumentException("Only the raid_charge button visibility can be changed.", nameof(control));
                error = string.Empty;
                // Only the single-player HUD draws buttons, and only for the player.
                if (_model != _fight._playerModel || _fight.IsLocalVersus || _fight.Controller == null) return true;
                // Fight setup (FELJFJOEJNC) recomputes raid visibility for the next fight.
                _fight.Controller.GetActionButtons().ShowRaidChargeAbility(visible);
                error = string.Empty;
                return true;
            }
            catch (Exception exception) { error = exception.Message; return false; }
        }

        private InfoPerk FlagContainer(object owner, string behavior, bool create)
        {
            if (_fight == null || _model == null || !_fight.round.processing || _fight._eclipseFightEndDispatched ||
                (_model != _fight._playerModel && _model != _fight._enemyModel))
                throw new InvalidOperationException("Flags require an active fighter and round.");
            return _fight.perksStage.GetScriptFlagContainer(_model, owner, behavior, create);
        }
        public bool TrySetFlag(object owner, string behavior, string name, out string error)
        {
            try { FlagContainer(owner, behavior, true).SetScriptFlag(_model, name); error = string.Empty; return true; }
            catch (Exception exception) { error = exception.Message; return false; }
        }
        public bool TryClearFlag(object owner, string behavior, string name, out string error)
        {
            try { FlagContainer(owner, behavior, false)?.ClearScriptFlag(name); error = string.Empty; return true; }
            catch (Exception exception) { error = exception.Message; return false; }
        }
        public bool TryHasFlag(object owner, string behavior, string name, out bool exists, out string error)
        {
            try { exists = FlagContainer(owner, behavior, false)?.HasScriptFlag(name) ?? false; error = string.Empty; return true; }
            catch (Exception exception) { exists = false; error = exception.Message; return false; }
        }

		public ModDamageEvent DamageEvent { get; }
        public ModIncomingHit IncomingHit { get; }
        public ModCombatActivityEvent ActivityEvent { get; }
        public ModAnimationLifecycleEvent AnimationEvent { get; }
        public ModCombatSnapshot CaptureCombatSnapshot()
        {
            if (_fight == null || _model == null || _model.Parameters == null) return null;
            var self = Capture(_model);
            if (self == null) return null;
            var opponent = _model.GetCombatTarget();
            return new ModCombatSnapshot(self, Capture(opponent), _fight.fightTimeInFrame, _fight.round.processing);
        }
        internal static ModFighterSnapshot Capture(Model model)
        {
            if (model == null || model.Parameters == null || model.GetBodyObject() == null) return null;
            var position = model.GetPosition();
            if (position == null) return null;
            var parameters = model.Parameters;
            return new ModFighterSnapshot(model.GetLife(), parameters.MaxLife,
                parameters.HealthBarCount, position.GetX(), position.GetY(), position.GetZ(),
                ModRuntime.CaptureAnimationSnapshot(model),GetCurrentFight()?.CaptureEclipseActorIdentity(model));
        }
        public double Health => _model == null ? 0 : _model.GetLife();
        public IModFighterOperations Opponent => _fight == null || Actor is OwnedActor actor && actor.Removing ? null :
            new EclipseFighterOperations(_fight, _model?.GetCombatTarget());
		public EclipseFighterOperations(Fight fight, Model model, ModDamageEvent damageEvent = null, ModIncomingHit incomingHit = null, ModCombatActivityEvent activity = null, ModAnimationLifecycleEvent animation = null, bool controlSetup = false)
		{
			_fight = fight;
			_model = model;
            _controlSetup = controlSetup;
			DamageEvent = damageEvent;
            IncomingHit = incomingHit;
            ActivityEvent = activity;
            AnimationEvent = animation;
		}

		public bool TrySetDamageShield(object key, double fraction, int frames, out string error)
        {
            if (Actor is OwnedActor actor && !_fight.ActorValid(actor,true,out error)) return false;
            if (_fight == null || _model == null || _model.GetLife() <= 0) { error = "Fighter is unavailable."; return false; }
            if (!_fight._eclipseShields.TryGetValue(_model, out var shields)) _fight._eclipseShields[_model] = shields = new ModDamageShields();
            return shields.TrySet(key, fraction, frames, _fight.fightTimeInFrame, out error);
        }
		public bool TryRemoveDamageShield(object key, out string error)
        {
            error = "";
            if (_fight != null && _model != null && _fight._eclipseShields.TryGetValue(_model, out var shields)) shields.Remove(key);
            return true;
        }

		public bool TryShowStatusIcon(object key, AssetId sprite, int frames, int stacks, out string error)
		{
            if (Actor is OwnedActor actor && !_fight.ActorValid(actor,true,out error)) return false;
			if (_fight == null || _model == null || key == null || string.IsNullOrEmpty(sprite.Path) ||
				frames < 1 || frames > 3600 || stacks < 0 || stacks > 10000)
			{ error = "Invalid or unavailable status icon."; return false; }
			return _fight.TryShowEclipseStatusIcon(_model, key, sprite, frames, stacks, out error);
		}

		public bool TryClearStatusIcon(object key, out string error)
		{
			if (_fight == null || _model == null || key == null) { error = "Fighter is unavailable."; return false; }
			return _fight.TryClearEclipseStatusIcon(_model, key, out error);
		}

		public bool TryChangeHealth(double amount, out string error)
		{
			error = string.Empty;
            if (Actor is OwnedActor actor) return actor.TryChangeHealth(amount,out error);
			if (_model != null && _model.GetLife() <= 0)
			{
				error = "A resolved lethal hit cannot be reversed by a damage callback.";
				return false;
			}
			if (_fight == null || _model == null)
			{
				error = "The fighter is no longer available.";
				return false;
			}
			if (double.IsNaN(amount) || double.IsInfinity(amount) || amount < -float.MaxValue || amount > float.MaxValue)
			{
				error = "Health change must be a finite single-precision number.";
				return false;
			}
			try
			{
				// This is the same recovered health path used by Lifesteal and ModHealthChange.
				_fight.UpdateLife(_model, (float)amount);
				return true;
			}
			catch (Exception exception)
			{
				error = exception.Message;
				return false;
			}
		}

		public bool TryAddMagicCharge(double amount, out string error)
		{
            if (Actor is OwnedActor actor && !_fight.ActorValid(actor,true,out error)) return false;
			error = string.Empty;
			if (_model == null)
			{
				error = "The fighter is no longer available.";
				return false;
			}
			if (double.IsNaN(amount) || double.IsInfinity(amount) || amount < -float.MaxValue || amount > float.MaxValue)
			{
				error = "Magic charge change must be a finite single-precision number.";
				return false;
			}
			try
			{
				// Mirror PerkActionAddMagicCharge: mutate charge, then normalize/update its fight UI state.
				_model.AddMagicChargeFraction((float)amount);
				_model.UpdateMagicButton();
				return true;
			}
			catch (Exception exception)
			{
				error = exception.Message;
				return false;
			}
		}
	}

	private const float DefaultSlowdownFactor = 2f;

	private const int DefaultWallAlignX = 100;

	private const int DefaultWallAlignY = 30;

	private MagicBuffer magicBuffer = new MagicBuffer();

	private FightListParametersBuffer parametersBuffer;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isPaused;

	private static Fight _currentFight;

	private int frame;

	private int fightTimeInFrame;

	// best guess for name
	private FightList FightDefinition;

	private int endStanceCounter;

	public List<Model> ActiveModels = new List<Model>();

	private List<Model> pendingModels = new List<Model>();

	private List<Model> modelsToRemove = new List<Model>();

	private FightIntPair intPairBuffer;

	private Round round = new Round();

	private bool isFirstStrike;

	private bool isRenderCamera;

	private bool isRenderFight;

	private Location _location;

	private Camera _Camera;

	private bool unusedFlagA;

	private Queue<global::Pair<string, int>> messageQueue;

	private bool unusedFlagB;

	private bool isGameOver;

	private bool isStopFight;

	private bool isFightInitialized;

	private bool unusedFlagC;

	private bool unusedFlagD;

	private bool isRaidEndFight;

	private List<ModelParameters> enemyParametersList;

	private ModelParameters playerParameters;

	private ModelParameters enemyParameters;

	private ModelParameters itemRuleParameters;

	private Model _playerModel;

	private Model _enemyModel;

	private Tactic enemyTactic;

	private PlayersFightData fightData = new PlayersFightData();

	private InFightRule _endFightRule;

	private EndRoundType _endRoundType;

	private CountersFight counters = new CountersFight();

	private bool hasPendingAchievement;

	private List<Achievement> pendingAchievements = new List<Achievement>();

	private bool isShowingAchievement;

	private bool isRoundResultPending;

	private bool _isRoundOver;

	private bool isAchievementBlocking;

	private bool hadEveryFrameEvent;

	private readonly Dictionary<Model, ModDamageShields> _eclipseShields = new Dictionary<Model, ModDamageShields>();
	private sealed class EclipseStatusIcon
	{
		public PerksStage.ActionPerk Action;
		public int ExpiresAt;
	}
	private readonly Dictionary<(Model, object), EclipseStatusIcon> _eclipseStatusIcons =
		new Dictionary<(Model, object), EclipseStatusIcon>();
	private bool _eclipseFightBeginDispatched;
	private string _eclipsePlayerResult = "none";
	private string _eclipseFightId = Guid.NewGuid().ToString("N");
	private bool _eclipseFightEndDispatched;
	private int _eclipseEndedRound;

	private GameOverParameters gameOverParameters = new GameOverParameters();

	private EquippedItemsStruct playerEquippedItems = new EquippedItemsStruct();

	private EquippedItemsStruct enemyEquippedItems = new EquippedItemsStruct();

	private uint unusedValueA;

	private uint unusedValueB;

	private long _testStartTime;

	private int unusedCounterA;

	private bool unusedFlagE;

	private bool unusedFlagF;

	private bool isControlsInverted;

	private int unusedCounterB;

	private float averageFps;

	private bool hasNotLostRound;

	private bool isPerkAreaActive;

	private bool isPerkAreaEnabled;

	private float perkAreaMinX;

	private float perkAreaMaxX;

	private float perkAreaWidth;

	private int stepFrameCount;

	private bool isEndRound;

	private bool isSlowMotion;

	private bool isHealthRestored;

	private bool isSlowModeKeyToggled;

	private bool isSlowMotionRequested;

	private bool showDebugPerks;

	private bool isInputEnabled;

	private RoundParam playerRoundParam;

	private RoundParam enemyRoundParam;

	private bool wasPlayerShocked;

	private bool killOpponentOnStart;

	private bool retryKillOpponent;

	private int currentEnemyIndex;

	private bool unusedFlagG;

	private GameObject _UnityObject;

	public StageType.Stage stageType;

	public PreFight preFight;

	// best guess for name
	public GameController Controller;

	public SelectAnimation _SelectAnimation = new SelectAnimation();

	public RulesInspector _rulesInspector;

	private PerksStage perksStage = new PerksStage();

	public Model.StrikeResult LastStrikeResult;

	public bool IsFightPaused
	{
		get
		{
			return IsPaused();
		}
		set
		{
			SetPaused(value);
		}
	}

	public static Fight ActiveFight
	{
		get
		{
			return GetCurrentFight();
		}
		set
		{
			set_CurrentFight(value);
		}
	}

	public FightList Definition
	{
		get
		{
			return GetFightDefinition();
		}
		set
		{
			SetFightDefinition(value);
		}
	}

	public GameObject UnityObject
	{
		get
		{
			return GetUnityObject();
		}
	}

	public PerksStage Perks
	{
		get
		{
			return GetPerksStage();
		}
	}

	private bool IsRoundFinished
	{
		get
		{
			return GetIsRoundFinished();
		}
	}

	private bool IsPauseButtonVisible
	{
		get
		{
			return GetIsPauseButtonVisible();
		}
	}

	public BattleType FightType
	{
		get
		{
			return GetFightType();
		}
	}

	public bool IsFightStage
	{
		get
		{
			return GetIsFightStage();
		}
	}

	public bool IsStageNone
	{
		get
		{
			return GetIsStageNone();
		}
	}

	public Model PlayerModel
	{
		get
		{
			return GetPlayerModel();
		}
	}

	public Model EnemyModel
	{
		get
		{
			return GetEnemyModel();
		}
	}

	public bool InvertedControls
	{
		set
		{
			SetControlsInverted(value);
		}
	}

	public int CurrentRoundNumber
	{
		get
		{
			return get_RoundNumber();
		}
	}

	public int TimeLeftSeconds
	{
		get
		{
			return get_RoundTimeLeft();
		}
	}

	public int FramesRemaining
	{
		get
		{
			return get_RoundTimeLeftFrames();
		}
	}

	public int FramesElapsed
	{
		get
		{
			return get_RoundTimePassedFrames();
		}
	}

	public int TimeTotalFrames
	{
		get
		{
			return get_RoundTimeTotalFrames();
		}
	}

	public bool IsFightNone
	{
		get
		{
			return get_isFightNone();
		}
	}

	public bool IsRaid
	{
		get
		{
			return get_IsRaidFight();
		}
	}

	public bool IsGameOver
	{
		get
		{
			return get_IsFightOver();
		}
	}

	public int ElapsedFrames
	{
		get
		{
			return get_FightTimeInFrames();
		}
	}

	public Fight(object data, ModelParameters AIFLOMMDGJB, List<ModelParameters> ELGGAEBPCHI, PreFight preFight = null, GameController LPGANKOAPJL = null)
	{
		_currentFight = this;
		_UnityObject = new GameObject("Fight");
		SetPaused(false);
		FightDefinition = (FightList)data;
		currentEnemyIndex = 0;
		playerParameters = AIFLOMMDGJB;
		enemyParametersList = ELGGAEBPCHI;
		endStanceCounter = 0;
		isGameOver = false;
		isStopFight = false;
		isSlowMotion = false;
		unusedFlagD = false;
		isRaidEndFight = false;
		isSlowModeKeyToggled = false;
		isFirstStrike = false;
		isEndRound = false;
		stageType = StageType.Stage.STAGE_NONE;
		isRenderFight = true;
		isRenderCamera = true;
		isFightInitialized = false;
		hasPendingAchievement = false;
		isHealthRestored = false;
		_endFightRule = null;
		frame = 0;
		fightTimeInFrame = 0;
		LastStrikeResult = null;
		isAchievementBlocking = false;
		isShowingAchievement = false;
		isRoundResultPending = false;
		unusedFlagC = false;
		isSlowMotionRequested = false;
		unusedFlagE = false;
		unusedFlagF = false;
		unusedCounterA = 0;
		_testStartTime = 0L;
		_playerModel = null;
		_enemyModel = null;
		_rulesInspector = null;
		_endRoundType = EndRoundType.EndRoundTypeNone;
		unusedCounterB = 0;
		averageFps = 0f;
		stepFrameCount = 0;
		playerRoundParam = new RoundParam();
		enemyRoundParam = new RoundParam();
		_isRoundOver = false;
		enemyTactic = null;
		isControlsInverted = false;
		perkAreaMinX = float.MinValue;
		perkAreaMaxX = float.MinValue;
		perkAreaWidth = 0f;
		isPerkAreaActive = false;
		isPerkAreaEnabled = false;
		showDebugPerks = false;
		hadEveryFrameEvent = false;
		_eclipseFightBeginDispatched = false;
		_eclipseFightEndDispatched = false;
		_eclipseEndedRound = 0;
		_eclipseFightId = Guid.NewGuid().ToString("N");
        _eclipseOpponentInstances.Clear();
        _eclipseInnateInstances.Clear();
        _eclipseBattleRules = new ModBattleRuleInstances();
        _eclipseRoundOutcomes.BeginRound(-1, null);
        CancelEclipseProjectiles();
        CancelEclipseActors("round_ended");
        CancelEclipseFighterMotion();
        CancelEclipseFighterPlayback();
		hasNotLostRound = true;
		isInputEnabled = true;
		killOpponentOnStart = false;
		retryKillOpponent = false;
		messageQueue = new Queue<global::Pair<string, int>>();
		unusedFlagB = true;
		GameUtils.OnFightPrepared(FightDefinition.Battle);
		if (!GameUtils.GetFightFlag())
		{
		}
		if (GameUtils.ReduceFps)
		{
			SystemProperties.SetTargetFrameRate(GameUtils.FrameRate / GameUtils.FpsReductionDivisor);
		}
		if (data == null)
		{
			GameLog.Error("Fight::Fight - data == 0");
		}
		List<InfoAnimation> list = AnimationData.GetAnimations();
		foreach (InfoAnimation item in list)
		{
			item.ResetModelBindings();
		}
		List<Trigger> list2 = AnimationData.GetTriggers();
		foreach (Trigger item2 in list2)
		{
			item2.ResetState();
		}
		enemyParameters = enemyParametersList[currentEnemyIndex];
		enemyTactic = enemyParameters.FightTactic;
		SaveEquippedItems();
		Zone locationZone = FightDefinition.Battle == null ? null : FightDefinition.Battle.ParentZone;
		bool raidLayout = UnderworldZonePolicy.IsRaidZone(locationZone);
		_location = new Location(Location.ResolveEntryLocation(FightDefinition.get_Type(), FightDefinition.Location),
			FightDefinition.Music, raidLayout);
		_location.init();
		playerParameters.SpawnPosition.Set(_location.playerStartPosition);
		enemyParameters.SpawnPosition.Set(_location.enemyStartPosition);
		bool flag = false;
		CreateRulesInspector();
		InitRules();
		CheckChangeFightRules();
		_rulesInspector.ApplyAvatarAndNameRules(playerParameters);
		_rulesInspector.ApplyNoAnimationRules(playerParameters);
		_rulesInspector.ApplyNoPerksRules(playerParameters, _rulesInspector.GetPlayerNoPerks());
		_rulesInspector.ApplyNoPerksRules(enemyParameters, _rulesInspector.GetEnemyNoPerks());
		GameUtils.SetLeftWall(_location.wallWidth);
		GameUtils.SetRightWall(_location.width - _location.wallWidth);
		if (!flag)
		{
			counters.Init(IsLocalVersus ? new Dictionary<string, Counter>() : GameUtils.ModeCounters.GetCountersForFight(FightDefinition), playerParameters, FightDefinition.get_Type(), GameUtils.GetFightDifficulty(FightDefinition));
			counters.AddEventListener(0, OnCounterIncrement);
		}
		_Camera = new Camera(_UnityObject.transform);
		_Camera.Init(_location);
		_Camera.AddEventListener(0, OnCameraTransitionStart);
		_Camera.AddEventListener(1, OnCameraTransitionEnd);
		CreateFighters();
		isFightInitialized = true;
		this.preFight = preFight;
		if (this.preFight != null)
		{
			this.preFight.Init(FightDefinition);
			this.preFight.ViewerPauseVisible(GetIsPauseButtonVisible());
			this.preFight.OnStopScreen.AddListener(OnStopPreFight);
			this.preFight.OnButtonClick.AddListener(OnButtonClick);
			this.preFight.OnAchievementMessageHide.AddListener(OnAchievementMessageHidden);
		}
		perksStage.AddEventListener(11, OnModExpired);
		SetupController(LPGANKOAPJL);
		_Camera.AddPreFight(preFight);
		_Camera.SetFightVisible(false);
		round.round = 0;
		OnFightInitialized();
		if (FightDefinition.get_Type() != BattleType.FightNone)
		{
			FightDefinition.set_IsInFight(true);
			Sound.PlayMusic(_location.GetRandomMusic());
			SoundController.IsBackgroundMusicIntro = false;
			StartVS();
		}
		else
		{
			SoundController.StartBackgroundMusic();
			StartPunchbag();
		}
		GameUtils.SetSlowMode(1);
		if (!AssemblyController.GetAiEnabled())
		{
			enemyParameters.AiControlled = false;
		}
		isHealthRestored = false;
		ModelAi.set_AiOn(true);
	}

	// best guess for name
	public bool IsPaused()
	{
		return isPaused;
	}

	// best guess for name
	public void SetPaused(bool value)
	{
		if (IsLocalVersus && Controller != null)
		{
			if (value) Controller.StopController();
			else if (!isGameOver && stageType == StageType.Stage.STAGE_FIGHT)
				Controller.StartController();
		}
		isPaused = value;
	}

	// best guess for name
	public static Fight GetCurrentFight()
	{
		return _currentFight;
	}

	public static void set_CurrentFight(Fight value)
	{
		if (value != _currentFight && (_currentFight == null || value == null))
		{
			_currentFight = value;
		}
		else
		{
			GameLog.Error("Fight::setCurrentFight - fight not NULL");
		}
	}

	// best guess for name
	public FightList GetFightDefinition()
	{
		return FightDefinition;
	}

	public void SetFightDefinition(FightList value)
	{
		FightDefinition = value;
	}

	public GameObject GetUnityObject()
	{
		return _UnityObject;
	}

	public PerksStage GetPerksStage()
	{
		return perksStage;
	}

	private bool GetIsRoundFinished()
	{
		return playerParameters.RoundEnded;
	}

	private bool GetIsPauseButtonVisible()
	{
		return !AssemblyController.GetGamepadEnabled();
	}

	public BattleType GetFightType()
	{
		return FightDefinition.get_Type();
	}

	public bool GetIsFightStage()
	{
		return stageType == StageType.Stage.STAGE_FIGHT;
	}

	public bool GetIsStageNone()
	{
		return stageType == StageType.Stage.STAGE_NONE;
	}

	// best guess for name
	public Model GetPlayerModel()
	{
		return _playerModel;
	}

	// best guess for name
	public Model GetEnemyModel()
	{
		return _enemyModel;
	}

	public void SetControlsInverted(bool value)
	{
		isControlsInverted = value;
	}

	public int get_RoundNumber()
	{
		return round.round;
	}

	public int get_RoundTimeLeft()
	{
		return (preFight != null) ? preFight.get_TimeLeft() : 0;
	}

	public int get_RoundTimeLeftFrames()
	{
		return (preFight != null) ? preFight.get_TimeLeftFrames() : 0;
	}

	public int get_RoundTimePassedFrames()
	{
		return (preFight != null) ? preFight.get_TimePassedFrames() : 0;
	}

	public int get_RoundTimeTotalFrames()
	{
		return (preFight != null) ? preFight.get_TimeTotalRoundFrames() : 0;
	}

	public bool get_isFightNone()
	{
		if (FightDefinition != null)
		{
			return FightDefinition.get_Type() == BattleType.FightNone;
		}
		return false;
	}

	public bool get_IsRaidFight()
	{
		return FightDefinition.get_Type() == BattleType.FightRaid;
	}

	public bool get_IsFightOver()
	{
		return isGameOver;
	}

	public int get_FightTimeInFrames()
	{
		return fightTimeInFrame;
	}

	public void Unload()
	{
        CloseModelTransitions();
		if (GameUtils.ReduceFps && !SystemProperties.IsMetroArmPlatform() && !SystemProperties.IsWindowsStorePlatform())
		{
			SystemProperties.SetTargetFrameRate(GameUtils.FrameRate);
		}
		set_CurrentFight(null);
		OnFightUnloading(FightDefinition);
		FightDefinition.set_IsInFight(false);
		FightDefinition.ApplyPendingRandomReset();
		ResetParameters();
		GameUtils.SetSlowMode(1);
		Controller.RemoveEventListener(0, ControlPress);
		Controller.RemoveEventListener(1, ControlRelease);
		Controller.ResetController();
		perksStage.RemoveEventListener(11, OnModExpired);
		_rulesInspector.ClearRules();
		foreach (Model item in ActiveModels)
		{
			RemoveModel(item);
		}
		_SelectAnimation.ClearModelsAndEvents();
		ModelLoader.ClearDocumentCache();
		if (FightDefinition.get_Type() != BattleType.FightNone)
		{
			SoundController.StartBackgroundMusic();
		}
		_Camera.RemoveAllEventListener();
		_Camera.Clear();
		_Camera = null;
		if (counters != null)
		{
			counters.RemoveEventListener(0, OnCounterIncrement);
		}
		perksStage.RemoveEventListener(11, OnModExpired);
		AiData.ClearTables();
		List<InfoAnimation> list = AnimationData.GetAnimations();
		foreach (InfoAnimation item2 in list)
		{
			item2.ResetModelBindings();
		}
		List<Trigger> list2 = AnimationData.GetTriggers();
		foreach (Trigger item3 in list2)
		{
			item3.ResetState();
		}
		LocationSpriteCache.Clear();
	}

	public void RandomizeObscuredVars()
	{
		enemyParametersList.ForEach((ModelParameters DHDMNHCIPEH) =>
		{
			DHDMNHCIPEH.RandomizeObscuredVars();
		});
		if (playerParameters != null)
		{
			playerParameters.RandomizeObscuredVars();
		}
		if (enemyParameters != null)
		{
			enemyParameters.RandomizeObscuredVars();
		}
		if (preFight != null && preFight.get_ViewerFight() != null)
		{
			preFight.get_ViewerFight().RandomizeObscuredVars();
		}
	}

	public void ApplyDamageFromServer(string INGHMAIMCMJ, int AFMMKADPHGM, int CACNLKMAHBO)
	{
	}

	public void Draw()
	{
		Eclipse.Rendering.Interpolation.FightInterpolation.MarkDrawStep();
		// Versus ticks are paced by their input source (lockstep stalls or catch-up).
		int num = IsLocalVersus ? Eclipse.Multiplayer.VersusTickDriver.StepsFor(this) : ((!GameUtils.ReduceFps) ? 1 : 2);
		for (int i = 0; i < num; i++)
		{
			if ((bool)preFight)
			{
				preFight.SetPause(IsPaused());
			}
			if (IsPaused())
			{
				break;
			}
			if (IsLocalVersus && !Eclipse.Multiplayer.VersusTickDriver.BeforeTick(this))
			{
				break;
			}
			Eclipse.Diagnostics.PerformanceOverlay.BeginFightSimulation();
			Render();
			Eclipse.Diagnostics.PerformanceOverlay.EndFightSimulation();
			if (IsLocalVersus)
			{
				Eclipse.Multiplayer.VersusTickDriver.AfterTick(this);
			}
		}
	}

	public void ReleaseAnyKey(FightCID PBFPKFPMFCI)
	{
		if (IsLocalVersus) return;
		if ((stageType != StageType.Stage.STAGE_FIGHT && PBFPKFPMFCI != FightCID.NextFrameButton && PBFPKFPMFCI != FightCID.PauseButton) || (!Application.isEditor && !SystemProperties.IsDebug() && !UnityEngine.Debug.isDebugBuild))
		{
			return;
		}
		switch (PBFPKFPMFCI)
		{
		case FightCID.PauseButton:
			SetPaused(!IsPaused());
			break;
		case FightCID.NextFrameButton:
			if (IsPaused())
			{
				Render();
			}
			break;
		case FightCID.EnableMinScale:
			_Camera.ToggleMinScale();
			break;
		case FightCID.WinRoundButton:
			KillModel(false, false);
			break;
		case FightCID.WinFightButton:
			KillModel(false, true);
			break;
		case FightCID.LossRoundButton:
			KillModel(true, false);
			break;
		case FightCID.LossFightButton:
			KillModel(true, true);
			break;
		case FightCID.ResetRoundButton:
			ResetRound();
			break;
		case FightCID.ResetFightButton:
			ResetFight();
			break;
		case FightCID.RechargeMagic:
			_playerModel.AddMagicCharges(1);
			_playerModel.UpdateMagicButton();
			break;
		case FightCID.IncreaseComboHit:
			_playerModel.RegisterComboHit();
			break;
		case FightCID.IncreaseStyle:
		{
			ScreenModel screenModel = ((!(preFight.get_ViewerFight() != null)) ? null : preFight.get_ViewerFight().get_LeftModel());
			if (screenModel != null)
			{
				screenModel.IncreaseStyleByValue(1f);
			}
			break;
		}
		case FightCID.SetPlayerAllHitsCritical:
			_enemyModel.SetForceCritical(!_enemyModel.IsForceCritical());
			break;
		case FightCID.SetPlayerImmortality:
			_playerModel.Parameters.set_IsImmortalityEnabled(!_playerModel.Parameters.GetIsImmortalityEnabled());
			break;
		case FightCID.SetBotImmortality:
			_enemyModel.Parameters.set_IsImmortalityEnabled(!_enemyModel.Parameters.GetIsImmortalityEnabled());
			break;
		case FightCID.ShowEdgesButton:
			break;
		case FightCID.ShowDebugPerksButton:
			showDebugPerks = !showDebugPerks;
			break;
		case FightCID.SlowModeKey:
			isSlowModeKeyToggled = !isSlowModeKeyToggled;
			SetSlowMotion(isSlowModeKeyToggled);
			break;
		case FightCID.SoundMuteButton:
		case FightCID.TestTactic:
		case FightCID.StartBenchmarkKey:
		case FightCID.StartSuper:
		case FightCID.FullscreenMode:
			break;
		}
	}

	public void OnIntervalStart(object data)
	{
		Model.EventModel oJDOHGBGPFK = (Model.EventModel)data;
		IntervalAnimation mNOIEOBBCMI = (IntervalAnimation)oJDOHGBGPFK.Data;
		if (mNOIEOBBCMI.Type == IntervalAnimation.IntervalType.INTERVAL_INVISIBLE)
		{
			_Camera.GetRender().GetViewerModel().SetModelActive(oJDOHGBGPFK.KJDFJPBIGJC.GetBodyObject(), false);
		}
		CheckLethalSlowMotion(oJDOHGBGPFK);
	}

	public void OnIntervalEnd(object data)
	{
		Model.EventModel oJDOHGBGPFK = (Model.EventModel)data;
		IntervalAnimation mNOIEOBBCMI = (IntervalAnimation)oJDOHGBGPFK.Data;
		if (mNOIEOBBCMI.Type == IntervalAnimation.IntervalType.INTERVAL_INVISIBLE)
		{
			_Camera.GetRender().GetViewerModel().SetModelActive(oJDOHGBGPFK.KJDFJPBIGJC.GetBodyObject(), true);
		}
		perksStage.GetPerkMap()["Interval"] = mNOIEOBBCMI;
		perksStage.FireEvent(oJDOHGBGPFK.KJDFJPBIGJC, PerkEvent.PerkEventType.EVENT_INTERVAL_END, true);
		StopSlowMotionAfterAttack(oJDOHGBGPFK);
	}

	public void OnAnimationStart(object data)
	{
		if (stageType == StageType.Stage.STAGE_END_STANCE && !isEndRound)
		{
			ModelParameters kIKOGDEPGHB = GetWinner(true);
			switch (kIKOGDEPGHB.EndRoundType)
			{
			case EndRoundType.EndRoundTypeTimeOut:
				if (preFight != null)
				{
					preFight.CreateTimesUp();
				}
				break;
			case EndRoundType.EndRoundTypeRingOut:
				if (preFight != null)
				{
					preFight.CreateRingOut();
				}
				break;
			case EndRoundType.EndRoundTypeLose:
				if (kIKOGDEPGHB.IsPlayer)
				{
					if (preFight != null)
					{
						preFight.CreateYouWin();
					}
				}
				else if (preFight != null)
				{
					preFight.CreateYouLose();
				}
				break;
			default:
				if (!kIKOGDEPGHB.IsPlayer)
				{
					break;
				}
				if (kIKOGDEPGHB.RewardsEnabled)
				{
					if (preFight != null)
					{
						preFight.CreateWinner(true);
					}
				}
				else if (kIKOGDEPGHB.GetLifeRatio() <= GameUtils.GetGreatMaxHealth() && preFight != null)
				{
					preFight.CreateWinner(false);
				}
				break;
			}
			isEndRound = true;
		}
		Model.EventModel oJDOHGBGPFK = (Model.EventModel)data;
		InfoAnimation value = (InfoAnimation)oJDOHGBGPFK.Data;
		perksStage.GetPerkMap()["Animation"] = value;
		perksStage.FireEvent(oJDOHGBGPFK.KJDFJPBIGJC, PerkEvent.PerkEventType.EVENT_ANIMATION_START, true);
        NotifyEclipseAnimation(oJDOHGBGPFK.SourceModel, value, ModEffectEvent.AnimationStart);
		CheckFightRules(FightEvent.AnimationStartEvent, ((Model.EventModel)data).KJDFJPBIGJC.IsPlayerModel() ? RuleAppliance.AppliancePlayer : RuleAppliance.ApplianceOpponent);
		if (!oJDOHGBGPFK.KJDFJPBIGJC.IsCameraAttached())
		{
			_Camera.GetRender().GetViewerModel().SetModelActive(oJDOHGBGPFK.KJDFJPBIGJC.GetBodyObject(), true);
			oJDOHGBGPFK.KJDFJPBIGJC.SetCameraAttached(true);
		}
	}

	public void OnAnimationEnd(object data)
	{
		Model.EventModel oJDOHGBGPFK = (Model.EventModel)data;
		InfoAnimation value = (InfoAnimation)oJDOHGBGPFK.Data;
		perksStage.GetPerkMap()["Animation"] = value;
		perksStage.FireEvent(oJDOHGBGPFK.KJDFJPBIGJC, PerkEvent.PerkEventType.EVENT_ANIMATION_END, true);
        NotifyEclipseAnimation(oJDOHGBGPFK.SourceModel, value, ModEffectEvent.AnimationEnd);
		Model fGCODGKLHED = oJDOHGBGPFK.KJDFJPBIGJC.GetCombatTarget();
		bool flag = oJDOHGBGPFK.KJDFJPBIGJC.IsFinished() && fGCODGKLHED != null && fGCODGKLHED.IsFinished();
		if (stageType == StageType.Stage.STAGE_START_STANCE && flag)
		{
			ClearModelsStanceFlag();
			if (FightDefinition.get_Type() != BattleType.FightNone)
			{
				StartFight();
			}
			else
			{
				SetStage(StageType.Stage.STAGE_FIGHT);
			}
		}
		if (stageType == StageType.Stage.STAGE_END_STANCE && flag)
		{
			ClearModelsStanceFlag();
			FinishRound();
		}
	}

	public void OnModelPhysicsStart(Model.EventModel EGHPHELLOGO)
	{
		if (EGHPHELLOGO.KJDFJPBIGJC.GetParentModel() == null)
		{
			CheckFightRules(FightEvent.PhysicsStartEvent, EGHPHELLOGO.KJDFJPBIGJC.IsPlayerModel() ? RuleAppliance.AppliancePlayer : RuleAppliance.ApplianceOpponent);
		}
	}

	public void OnUnusedModelEvent(object data)
	{
	}

	public void OnEveryFrame(object data)
	{
		Model kJDFJPBIGJC = ((Model.EventModel)data).KJDFJPBIGJC;
		perksStage.GetPerkMap()["StepFrame"] = stepFrameCount;
		perksStage.FireEvent(kJDFJPBIGJC, PerkEvent.PerkEventType.EVENT_EVERY_FRAME, true);
		hadEveryFrameEvent = true;
	}

	public void OnModelPerkEvent(object data)
	{
		perksStage.OnDisarm(data);
	}

	public void NotifyAnimationSelector(Model.EventModel EGHPHELLOGO)
	{
		_SelectAnimation.OnRandomKeyPress(EGHPHELLOGO);
	}

	private void SetStage(StageType.Stage LFLGCDNKNJI)
	{
		if (IsLocalVersus && LFLGCDNKNJI != stageType && Eclipse.Multiplayer.VersusTickDriver.Barrier())
		{
			return;
		}
		switch (LFLGCDNKNJI)
		{
		case StageType.Stage.STAGE_FIGHT:
			Controller?.StartController();
			break;
		case StageType.Stage.STAGE_END_STANCE:
			Controller?.StopController();
			break;
		}
		stageType = LFLGCDNKNJI;
		foreach (Model item in ActiveModels)
		{
			item.EventData.Data = LFLGCDNKNJI;
			item.RoundStage = (int)LFLGCDNKNJI;
			perksStage.FireEvent(item, PerkEvent.PerkEventType.EVENT_ROUND_STAGE_START);
			item.EventData.Data = LFLGCDNKNJI;
			item.RoundStage = (int)LFLGCDNKNJI;
			_SelectAnimation.CheckEvent(EventAnimation.EventAnimationType.EVENT_ROUND_STAGE, item.EventData);
		}
	}

	public void OnFightInitialized()
	{
	}

	public void OnFightFinalized()
	{
	}

	public void Render()
	{
		if (isRenderFight)
		{
			RenderFight();
            DrainModelTransitions();
			Eclipse.Rendering.Interpolation.FightInterpolation.MarkFightStep();
		}
		if (isRenderCamera)
		{
			RenderCamera();
			Eclipse.Rendering.Interpolation.FightInterpolation.MarkCameraStep();
		}
		frame++;
	}

		private void RenderFight()
		{
			if (round.processing)
			{
				fightTimeInFrame++;
                UpdateEclipseProjectiles();
                UpdateEclipseActors();
				UpdateEclipseStatusIcons();
	            // Simulation time only: pause disables RenderFight, and round boundaries
            // disable processing. Run before model/collision updates for this frame.
            if (_eclipseFightBeginDispatched && ModRuntime.Scripts != null &&
                ModRuntime.Scripts.HasHandlers(ModEffectEvent.Tick))
            {
                DispatchEclipseCombatEvent(ModEffectEvent.Tick);
                if (round.processing) DispatchEclipseOpponent(ModEffectEvent.Tick);
                if (round.processing) DispatchEclipseActorTicks();
            }
		}
		if (retryKillOpponent)
		{
			ProcessKillRetry(GetEnemyModel());
			retryKillOpponent = false;
		}
		List<Model> list = new List<Model>();
		hadEveryFrameEvent = false;
		perksStage.Render();
		foreach (Model item in ActiveModels)
		{
			bool flag = item.HasPendingAnimation();
			item.Render();
			if (flag)
			{
				InfoAnimation pJAHIOELGGD = item.GetAnimationModule().GetCurrentInfo();
				if (pJAHIOELGGD != null && pJAHIOELGGD.GetCameraStage() == stageType)
				{
					list.Add(item);
				}
			}
		}
		if (hadEveryFrameEvent)
		{
			stepFrameCount++;
		}
		if (list.Count > 1)
		{
			AlignCameraOnModels(list);
		}
		if (pendingModels.Count > 0)
		{
			foreach (Model item2 in pendingModels)
			{
				item2.Render();
				ActiveModels.Add(item2);
			}
			pendingModels.Clear();
		}
		ApplyPendingSlowMotion();
        InitializeEclipseActorBirths();
        RefreshEclipseActorTeams();
		if (!GetIsRoundFinished())
		{
			RenderCollisions();
			_SelectAnimation.UpdateConditions();
			foreach (Model item3 in ActiveModels)
			{
				Eclipse.Diagnostics.PerformanceOverlay.BeginAi();
				item3.RenderAi();
				Eclipse.Diagnostics.PerformanceOverlay.EndAi();
			}
		}
		UpdatePerkAreaModels();
        ApplyEclipseProjectileSpawns();
        ApplyEclipseActorSpawns();
		_SelectAnimation.Render();
        InitializeEclipseProjectileBirths();
        InitializeEclipseActorBirths();
		perksStage.ResetInfoPerks();
		if (isFightInitialized)
		{
			CheckFightRules(FightEvent.RenderEvent, RuleAppliance.ApplianceAll);
			_Camera.RenderBloodEffects();
			_Camera.RenderLocationLayers();
			_Camera.GetRender().GetBackgroundEffects().UpdateEffects();
			_Camera.GetRender().GetForegroundEffects().UpdateEffects();
			_Camera.GetRender().UpdateHitEffect();
		}
		if (hasPendingAchievement)
		{
			hasPendingAchievement = false;
			ShowNextAchievement();
		}
		ApplyEclipseProjectiles();
        ApplyEclipseActors();
		ApplyEclipseFighterMotion();
		ApplyEclipseFighterPlayback();
		RenderRound();
		if (preFight != null)
		{
			preFight.Render();
		}
		OnRenderCompleted();
		ProcessRemovedModels();
		ResetModelsHitData();
		OnRenderFinished();
	}

	private void RenderCamera()
	{
		if (preFight != null)
		{
			preFight.RenderComboModel();
		}
		if (isEndRound)
		{
			RenderEndRoundEffect(2f);
		}
		_Camera.Render();
	}

	public void RenderCollisions()
	{
		PrepareModelsCollisions();
		bool fHPKEJMDFLK = false;
		bool flag = frame % 2 == 0;
		int count = ActiveModels.Count;
		if (flag)
		{
			for (int i = 0; i < count; i++)
			{
				fHPKEJMDFLK = ActiveModels[i].RenderCollision(fHPKEJMDFLK);
			}
			return;
		}
		for (int num = count - 1; num >= 0; num--)
		{
			fHPKEJMDFLK = ActiveModels[num].RenderCollision(fHPKEJMDFLK);
		}
	}

	public override string ToString()
	{
		return string.Empty;
	}

	// best guess for name
	public void TogglePauseMenu(bool ONFJJLFGNCH = false)
	{
		if (FightDefinition.get_Type() != BattleType.FightNone)
		{
			if (IsPaused())
			{
				ClosePauseScreen();
			}
			else
			{
				OpenPauseScreen();
			}
		}
	}

	public void CreateRingout(float HIKKOEOGMEK, float NMMCJGHAJBB, float DKJCJBAGKIL, string AJBGJNMLMKE)
	{
		_Camera.GetRender().CreateRingOutSprites(HIKKOEOGMEK, NMMCJGHAJBB, DKJCJBAGKIL, AJBGJNMLMKE);
	}

	public void RemoveRingout()
	{
		_Camera.GetRender().RemoveRingOutSprites();
	}

	public void CreateHotGround(string AJBGJNMLMKE, float ABKMCKDJCGB)
	{
	}

	public void RemoveHotGround()
	{
	}

	public void CreatePerkActivationArea(float JMLAKAKDBBL, string KHPKDMGDMAB, string ADONPNOBBDE)
	{
		isPerkAreaActive = true;
		perkAreaWidth = JMLAKAKDBBL;
		_Camera.GetRender().CreatePerkActivationArea(JMLAKAKDBBL, KHPKDMGDMAB, ADONPNOBBDE);
	}

	public void UpdatePerkActivationArea(float MGMMDGFPBLP, float KGJALFLDIBG, bool EBKPFEFCIIH)
	{
		perkAreaMinX = MGMMDGFPBLP - perkAreaWidth / 2f + _location.width / 2f;
		perkAreaMaxX = MGMMDGFPBLP + perkAreaWidth / 2f + _location.width / 2f;
		isPerkAreaEnabled = EBKPFEFCIIH;
		_Camera.GetRender().UpdatePerkActivationArea(MGMMDGFPBLP, KGJALFLDIBG);
	}

	public void RemovePerkActivationArea()
	{
		isPerkAreaActive = false;
		_Camera.GetRender().DestroyPerkActivationArea();
	}

	public void SetHealthBarVisible(RuleAppliance EJPOJJKKICO, bool KFIECNIMAOA)
	{
		if (preFight != null)
		{
			preFight.SetHealthBarVisible(EJPOJJKKICO, KFIECNIMAOA);
		}
	}

	public bool UpdateLife(Model ACENLMONNPA, float AACBFABMADJ)
	{
		if (FightDefinition.get_Type() == BattleType.FightNone)
		{
			return false;
		}
		ACENLMONNPA.ChangeLife(AACBFABMADJ);
		if (ACENLMONNPA.Parameters.GetLifeDepleted())
		{
			ACENLMONNPA.Parameters.IsDead = true;
		}
		return !ACENLMONNPA.IsAlive();
	}

	public void SetLife(Model ACENLMONNPA, float DLEDDPFNPOH)
	{
		ACENLMONNPA.SetLife(DLEDDPFNPOH);
	}

	public bool UpdateLife(RuleAppliance EJPOJJKKICO, float AACBFABMADJ)
	{
		Model fGCODGKLHED = null;
		switch (EJPOJJKKICO)
		{
		case RuleAppliance.AppliancePlayer:
			fGCODGKLHED = _playerModel;
			break;
		case RuleAppliance.ApplianceOpponent:
			fGCODGKLHED = _enemyModel;
			break;
		default:
			GameLog.Error("Fight::updateLife: wrong RuleAppliance - %i", EJPOJJKKICO);
			return false;
		}
		return UpdateLife(fGCODGKLHED, AACBFABMADJ);
	}

	public void SetDarknessAlpha(float KGJALFLDIBG)
	{
		_Camera.GetRender().SetDarknessAlpha(KGJALFLDIBG);
	}

	public void CreateDarkness()
	{
		_Camera.GetRender().CreateDarknessOverlay();
	}

	public void RemoveDarkness()
	{
		_Camera.GetRender().DestroyDarknessOverlay();
	}

	public void CreateLightInTheDarkness()
	{
		_Camera.GetRender().CreateLightInTheDarkness();
	}

	public void UpdateLightInTheDarkness(RuleAppliance target, float radius, float shape)
	{
		Model model = target == RuleAppliance.ApplianceOpponent ? _enemyModel : _playerModel;
		if (model != null)
		{
			_Camera.GetRender().UpdateLightInTheDarkness(model, radius, shape);
		}
	}

	public void RemoveLightInTheDarkness()
	{
		_Camera.GetRender().RemoveLightInTheDarkness();
	}

	public void OnPerkTriggered(Model ACENLMONNPA, PerkTrigger CPBHKJFPFJB)
	{
	}

	public void SetModelVisible(Model ACENLMONNPA, bool CCBEDPIHKAD)
	{
		_Camera.GetRender().GetViewerModel().SetModelActive(ACENLMONNPA.GetBodyObject(), CCBEDPIHKAD);
	}

	public void UpdatePerkIcon(Model ACENLMONNPA, PerksStage.ActionPerk IBODMPMJELJ, bool CCBEDPIHKAD)
	{
		ScreenModel screenModel = null;
		if (preFight != null && preFight.get_ViewerFight() != null)
		{
			screenModel = ((!ACENLMONNPA.IsPlayerModel()) ? preFight.get_ViewerFight().get_RightModel() : preFight.get_ViewerFight().get_LeftModel());
		}
		if (screenModel != null)
		{
			if (CCBEDPIHKAD)
			{
				screenModel.RemoveActivePerk(IBODMPMJELJ);
			}
			else
			{
				screenModel.AddActivePerk(IBODMPMJELJ);
			}
		}
	}

	public void ReplacePerkIcon(Model ACENLMONNPA, PerksStage.ActionPerk CKOEFOCPMGK, PerksStage.ActionPerk IBODMPMJELJ)
	{
		ScreenModel screenModel = null;
		if (preFight != null && preFight.get_ViewerFight() != null)
		{
			screenModel = ((!ACENLMONNPA.IsPlayerModel()) ? preFight.get_ViewerFight().get_RightModel() : preFight.get_ViewerFight().get_LeftModel());
		}
		if (screenModel != null)
		{
			screenModel.AddEffectPerk(CKOEFOCPMGK, IBODMPMJELJ);
		}
	}

	private bool TryShowEclipseStatusIcon(Model model, object key, AssetId sprite, int frames, int stacks, out string error)
	{
		error = string.Empty;
		if (model == null || key == null || string.IsNullOrEmpty(sprite.Path) || frames < 1 || frames > 3600 || stacks < 0 || stacks > 10000)
		{ error = "Invalid status icon request."; return false; }
		TryClearEclipseStatusIcon(model, key, out _);
		var action = new PerksStage.ActionPerk
		{
			TargetModel = model,
			SourceModel = model,
			IconPath = sprite.ToString(),
			ShowExpiration = true,
			ElapsedFrames = 0,
			DurationFrames = frames,
			EclipseStackCount = stacks
		};
		UpdatePerkIcon(model, action, false);
		_eclipseStatusIcons[(model, key)] = new EclipseStatusIcon
		{
			Action = action,
			ExpiresAt = checked(fightTimeInFrame + frames)
		};
		return true;
	}

	private bool TryClearEclipseStatusIcon(Model model, object key, out string error)
	{
		error = string.Empty;
		if (model == null || key == null) { error = "Invalid status icon request."; return false; }
		if (_eclipseStatusIcons.TryGetValue((model, key), out var entry))
		{
			UpdatePerkIcon(model, entry.Action, true);
			_eclipseStatusIcons.Remove((model, key));
		}
		return true;
	}

	private void UpdateEclipseStatusIcons()
	{
		if (_eclipseStatusIcons.Count == 0) return;
		var expired = new List<(Model, object)>();
		foreach (var pair in _eclipseStatusIcons)
		{
			var entry = pair.Value;
			entry.Action.ElapsedFrames = Math.Min(entry.Action.DurationFrames,
				Math.Max(0, entry.Action.DurationFrames - (entry.ExpiresAt - fightTimeInFrame)));
			if (fightTimeInFrame >= entry.ExpiresAt) expired.Add(pair.Key);
		}
		foreach (var key in expired)
		{
			if (_eclipseStatusIcons.TryGetValue(key, out var entry)) UpdatePerkIcon(key.Item1, entry.Action, true);
			_eclipseStatusIcons.Remove(key);
		}
	}

	private void ClearEclipseStatusIcons()
	{
		if (_eclipseStatusIcons.Count == 0) return;
		foreach (var pair in _eclipseStatusIcons) UpdatePerkIcon(pair.Key.Item1, pair.Value.Action, true);
		_eclipseStatusIcons.Clear();
	}

	public void OnPerkInfoItemUsed(Model ACENLMONNPA, PerkInfoItem AEFFHJGMNFI)
	{
	}

	public void SetEndFightRule(InFightRule HNBFMAKFJAM)
	{
		_endFightRule = HNBFMAKFJAM;
		if (_endFightRule != null)
		{
			switch (_endFightRule.get_Type())
			{
			case Rule.RuleType.RuleRingout:
				_endRoundType = EndRoundType.EndRoundTypeRingOut;
				break;
			case Rule.RuleType.RuleHotGround:
			case Rule.RuleType.RuleLoseFall:
			case Rule.RuleType.RuleCrazy:
			case Rule.RuleType.RuleTimeoutWin:
			case Rule.RuleType.RulePoints:
			case Rule.RuleType.RuleWinStyle:
			case Rule.RuleType.RuleWinCombo:
			case Rule.RuleType.RuleWinShock:
				_endRoundType = EndRoundType.EndRoundTypeLose;
				break;
			case Rule.RuleType.RuleRegeneration:
			case Rule.RuleType.RuleLifeSteal:
				_endRoundType = EndRoundType.EndRoundTypeZeroHealth;
				break;
			}
		}
	}

	public void SetLifeToZero(RuleAppliance IGFNCCEHFEK)
	{
		Model fGCODGKLHED = null;
		switch (IGFNCCEHFEK)
		{
		case RuleAppliance.AppliancePlayer:
			fGCODGKLHED = _playerModel;
			break;
		case RuleAppliance.ApplianceOpponent:
			fGCODGKLHED = _enemyModel;
			break;
		default:
			GameLog.Error("Fight::resetLife: wrong RuleAppliance - %i", IGFNCCEHFEK);
			break;
		}
		if (fGCODGKLHED != null)
		{
			fGCODGKLHED.SetLife(0f);
		}
	}

	public void OnFightStateChanged()
	{
	}

	public void DispatchHitPerkEvent(Model ACENLMONNPA, Model.StrikeResult BNBAOJOJDGJ, PerkEvent.PerkEventType LFLGCDNKNJI)
	{
		string text = ((BNBAOJOJDGJ.VictimEdge == null) ? string.Empty : BNBAOJOJDGJ.VictimEdge.GetDefense());
		InfoAnimation pBPDKJNKFCJ = BNBAOJOJDGJ.AttackAnimation;
		perksStage.GetPerkMap()["Defense"] = BNBAOJOJDGJ.DefenceAttribute;
		perksStage.GetPerkMap()["Animation"] = pBPDKJNKFCJ;
		perksStage.GetPerkMap()["Critical"] = BNBAOJOJDGJ.IsCritical;
		perksStage.GetPerkMap()["Shock"] = BNBAOJOJDGJ.IsShock;
		perksStage.GetPerkMap()["Block"] = BNBAOJOJDGJ.IsBlocked;
		perksStage.GetPerkMap()["Damage"] = BNBAOJOJDGJ.FinalDamage;
		perksStage.FireEvent(ACENLMONNPA, LFLGCDNKNJI, true);
	}

	public void OnModelPreCrit(Model.EventModel EGHPHELLOGO)
	{
		Model.StrikeResult gHHCDAFIKJE = EGHPHELLOGO.KJDFJPBIGJC.LastStrike;
		DispatchHitPerkEvent(EGHPHELLOGO.KJDFJPBIGJC, gHHCDAFIKJE, PerkEvent.PerkEventType.EVENT_HIT_PRECRIT);
	}

	public void OnModelPostCrit(Model.EventModel EGHPHELLOGO)
	{
		Model.StrikeResult gHHCDAFIKJE = EGHPHELLOGO.KJDFJPBIGJC.LastStrike;
		DispatchHitPerkEvent(EGHPHELLOGO.KJDFJPBIGJC, gHHCDAFIKJE, PerkEvent.PerkEventType.EVENT_HIT_POSTCRIT);
		DispatchEclipseHitPhase(EGHPHELLOGO, gHHCDAFIKJE, ModEffectEvent.HitPostCrit);
	}

	public void OnModelHit(Model.EventModel EGHPHELLOGO)
	{
		Model.StrikeResult gHHCDAFIKJE = EGHPHELLOGO.KJDFJPBIGJC.LastStrike;
		IntervalAttack hFIIPNLCIEE = EGHPHELLOGO.Data as IntervalAttack;
		// Only Lua attribution uses the root fighter. Native calculations retain
		// the actual contact actor, its animation, equipment and collision edges.
		Model eclipseAttacker = (gHHCDAFIKJE.AttackerModel ?? EGHPHELLOGO.Opponent)?.GetRootModel();
        bool eclipseActorContact = eclipseAttacker != null && _eclipseActors.ContainsKey(eclipseAttacker) || _eclipseActors.ContainsKey(EGHPHELLOGO.KJDFJPBIGJC.GetRootModel());
        ModAttackSource eclipseAttackSource = CaptureEclipseAttackSource(gHHCDAFIKJE.AttackerModel ?? EGHPHELLOGO.Opponent, gHHCDAFIKJE);
		if (hFIIPNLCIEE.GetNoCritical())
		{
			gHHCDAFIKJE.IsCritical = false;
		}
		DispatchHitPerkEvent(EGHPHELLOGO.KJDFJPBIGJC, gHHCDAFIKJE, PerkEvent.PerkEventType.EVENT_POST_HIT);
		DispatchEclipseHitPhase(EGHPHELLOGO, gHHCDAFIKJE, ModEffectEvent.PostHit, eclipseAttackSource);
		if (hFIIPNLCIEE.GetNoCritical())
		{
			gHHCDAFIKJE.IsCritical = false;
		}
		if (FightDefinition.get_Type() == BattleType.FightNone)
		{
			gHHCDAFIKJE.IsCritical = false;
			gHHCDAFIKJE.IsShock = false;
			gHHCDAFIKJE.IsDisarm = false;
		}
		if (gHHCDAFIKJE.IsShock)
		{
			if (EGHPHELLOGO.KJDFJPBIGJC.IsInShock())
			{
				gHHCDAFIKJE.IsShock = false;
			}
			else
			{
				EGHPHELLOGO.KJDFJPBIGJC.set_IsShock(true);
			}
		}
		if (gHHCDAFIKJE.IsDisarm)
		{
			ItemInfo dJKEECEOCJB = ListSF.GetItems().GetItemByName(GameUtils.ShockSettings.WeaponName);
			bool flag = dJKEECEOCJB != null && EGHPHELLOGO.KJDFJPBIGJC.Parameters.Weapon.Name == dJKEECEOCJB.Name;
			if (EGHPHELLOGO.KJDFJPBIGJC.WasDisarmed() || flag)
			{
				gHHCDAFIKJE.IsDisarm = false;
			}
			else
			{
				EGHPHELLOGO.KJDFJPBIGJC.SetDisarmed(true);
				EGHPHELLOGO.KJDFJPBIGJC.ScheduleDisarm();
			}
		}
		gHHCDAFIKJE.IsFirstStrike = !isFirstStrike;
		if (gHHCDAFIKJE.AttackerEdge != null)
		{
			ModelNode lCDGOCIAIDK = gHHCDAFIKJE.AttackerEdge.GetStartNode();
			ModelNode lCDGOCIAIDK2 = gHHCDAFIKJE.AttackerEdge.GetEndNode();
			Vector3f nBMEGFBPGFE = lCDGOCIAIDK.GetStart();
			Vector3f aKKEJFKBIHF = lCDGOCIAIDK.GetEnd();
			Vector3f nBMEGFBPGFE2 = lCDGOCIAIDK2.GetStart();
			Vector3f aKKEJFKBIHF2 = lCDGOCIAIDK2.GetEnd();
			float num = 1f / 120f;
			Vector3f kKIKIDNALOL = Vector3f.op_Addition(Vector3f.op_Subtraction(nBMEGFBPGFE, aKKEJFKBIHF), Vector3f.op_Subtraction(nBMEGFBPGFE2, aKKEJFKBIHF2));
			IntervalAttack hFIIPNLCIEE2 = EGHPHELLOGO.Opponent.GetAnimationModule().FindInterval(IntervalAnimation.IntervalType.INTERVAL_ATTACK) as IntervalAttack;
			if (hFIIPNLCIEE2.GetHasEffect())
			{
				EGHPHELLOGO.KJDFJPBIGJC.SetHitData(gHHCDAFIKJE.Point, kKIKIDNALOL, (!gHHCDAFIKJE.IsCritical) ? num : (2f * num));
			}
			if (gHHCDAFIKJE.IsCritical)
			{
				_Camera.QueueBloodEffect(gHHCDAFIKJE.Point, gHHCDAFIKJE.Impulse);
			}
		}
		if (!gHHCDAFIKJE.IsBlocked)
		{
			EGHPHELLOGO.KJDFJPBIGJC.RemoveInterval(IntervalAnimation.IntervalType.INTERVAL_BLOCK);
			isFirstStrike = true;
		}
		ModelParameters kMMJCHDKBDO = EGHPHELLOGO.KJDFJPBIGJC.Parameters;
        // Native hit/critical/block calculations are complete. Defense and health application follow.
        if (_eclipseFightBeginDispatched)
        {
            var outgoing = new ModIncomingHit(() => gHHCDAFIKJE.FinalDamage,
                amount => gHHCDAFIKJE.FinalDamage = (float)amount, gHHCDAFIKJE.IsBlocked, gHHCDAFIKJE.IsCritical, attackSource: eclipseAttackSource);
            if (eclipseAttacker == _playerModel)
                DispatchEclipseCombatEvent(ModEffectEvent.DamageDealing, null, outgoing);
            else if (eclipseAttacker == _enemyModel)
                DispatchEclipseOpponent(ModEffectEvent.DamageDealing, null, outgoing);
            else DispatchEclipseActor(eclipseAttacker,ModEffectEvent.DamageDealing,incoming:outgoing);
        }
		if (preFight != null && !eclipseActorContact)
		{
			preFight.ViewerStrike(gHHCDAFIKJE.AttackAnimation, gHHCDAFIKJE.FinalDamage, gHHCDAFIKJE.Target, gHHCDAFIKJE.IsFirstStrike, gHHCDAFIKJE.IsHeadHit, gHHCDAFIKJE.IsCritical, gHHCDAFIKJE.IsBlocked, gHHCDAFIKJE.IsShock);
		}
		if (EGHPHELLOGO.KJDFJPBIGJC.IsDamageImmune())
		{
			gHHCDAFIKJE.FinalDamage = 0f;
		}
        if (_eclipseShields.TryGetValue(EGHPHELLOGO.KJDFJPBIGJC, out var eclipseShields))
            gHHCDAFIKJE.FinalDamage *= (float)eclipseShields.Scale(fightTimeInFrame);
		if (_eclipseFightBeginDispatched && EGHPHELLOGO.KJDFJPBIGJC == _playerModel)
			DispatchEclipseCombatEvent(ModEffectEvent.DamageResolving, null,
				new ModIncomingHit(() => gHHCDAFIKJE.FinalDamage, amount => gHHCDAFIKJE.FinalDamage = (float)amount, gHHCDAFIKJE.IsBlocked, gHHCDAFIKJE.IsCritical, attackSource: eclipseAttackSource));
        if (_eclipseFightBeginDispatched && EGHPHELLOGO.KJDFJPBIGJC == _enemyModel)
            DispatchEclipseOpponent(ModEffectEvent.DamageResolving, null,
                new ModIncomingHit(() => gHHCDAFIKJE.FinalDamage, amount => gHHCDAFIKJE.FinalDamage = (float)amount, gHHCDAFIKJE.IsBlocked, gHHCDAFIKJE.IsCritical, attackSource: eclipseAttackSource));
        if (_eclipseFightBeginDispatched && _eclipseActors.ContainsKey(EGHPHELLOGO.KJDFJPBIGJC))
            DispatchEclipseActor(EGHPHELLOGO.KJDFJPBIGJC,ModEffectEvent.DamageResolving,incoming:
                new ModIncomingHit(() => gHHCDAFIKJE.FinalDamage, amount => gHHCDAFIKJE.FinalDamage = (float)amount,
                    gHHCDAFIKJE.IsBlocked,gHHCDAFIKJE.IsCritical,attackSource:eclipseAttackSource));
		if (IsLocalVersus && gHHCDAFIKJE.IsBlocked)
			gHHCDAFIKJE.FinalDamage = Eclipse.Multiplayer.PvpBalanceCombat.ClampBlocked(this, EGHPHELLOGO.KJDFJPBIGJC, gHHCDAFIKJE.FinalDamage);
		EGHPHELLOGO.KJDFJPBIGJC.LogDamage(gHHCDAFIKJE.FinalDamage, GetAttackLogName(hFIIPNLCIEE), gHHCDAFIKJE.DefenceAttribute);
		float eclipseHealthBefore = EGHPHELLOGO.KJDFJPBIGJC.GetLife();
		UpdateLife(EGHPHELLOGO.KJDFJPBIGJC, 0f - gHHCDAFIKJE.FinalDamage);
		if (IsLocalVersus)
			Eclipse.Multiplayer.PvpBalanceCombat.AfterStrike(this, EGHPHELLOGO.KJDFJPBIGJC, EGHPHELLOGO.Opponent, gHHCDAFIKJE.IsBlocked, eclipseHealthBefore);
		// Eclipse training and replay readouts (damage, combos, frame advantage).
		if (IsLocalVersus && Eclipse.Multiplayer.VersusTraining.Observing)
			Eclipse.Multiplayer.VersusTraining.OnHit(EGHPHELLOGO.Opponent, EGHPHELLOGO.KJDFJPBIGJC, gHHCDAFIKJE.FinalDamage,
				gHHCDAFIKJE.IsBlocked, gHHCDAFIKJE.IsCritical, gHHCDAFIKJE.AttackAnimation != null ? gHHCDAFIKJE.AttackAnimation.Name : null);
		// Presentation only: sf2.fx hit bursts and hit/critical/ko screen effects.
		Eclipse.Rendering.FighterParticles.Hit(EGHPHELLOGO.KJDFJPBIGJC, gHHCDAFIKJE.Point, gHHCDAFIKJE.IsCritical, gHHCDAFIKJE.IsBlocked,
			eclipseHealthBefore > 0f && EGHPHELLOGO.KJDFJPBIGJC.GetLife() <= 0f, EGHPHELLOGO.Opponent, gHHCDAFIKJE.Impulse);
		if (_eclipseFightBeginDispatched)
		{
			var observation = new ModDamageEvent(round.round, eclipseHealthBefore,
				EGHPHELLOGO.KJDFJPBIGJC.GetLife(), gHHCDAFIKJE.IsBlocked, gHHCDAFIKJE.IsCritical, eclipseAttackSource);
            NotifyEclipseAppliedContact(EGHPHELLOGO.KJDFJPBIGJC,eclipseAttacker,observation);
		}
		ApplyLifeSteal(eclipseActorContact ? eclipseAttacker : EGHPHELLOGO.KJDFJPBIGJC.GetCombatTarget(), gHHCDAFIKJE.FinalDamage);
		if (!gHHCDAFIKJE.AttackAnimation.GetNoMagicRecharge())
		{
			float num2 = EGHPHELLOGO.KJDFJPBIGJC.GetMagicCharges();
			float num3 = EGHPHELLOGO.Opponent.GetMagicCharges();
			float cKKFKEIELCP = hFIIPNLCIEE.GetDamage();
			EGHPHELLOGO.KJDFJPBIGJC.UpdateMagicCharge(cKKFKEIELCP, EGHPHELLOGO.Opponent, gHHCDAFIKJE.IsBlocked, gHHCDAFIKJE.IsCritical, false);
			EGHPHELLOGO.Opponent.UpdateMagicCharge(cKKFKEIELCP, EGHPHELLOGO.KJDFJPBIGJC, gHHCDAFIKJE.IsBlocked, gHHCDAFIKJE.IsCritical, true);
			if (num2 < 1f && EGHPHELLOGO.KJDFJPBIGJC.GetMagicCharges() >= 1)
			{
				perksStage.FireEvent(EGHPHELLOGO.KJDFJPBIGJC, PerkEvent.PerkEventType.EVENT_MAGIC_CHARGED, true);
			}
			if (num3 < 1f && EGHPHELLOGO.Opponent.GetMagicCharges() >= 1)
			{
				perksStage.FireEvent(EGHPHELLOGO.Opponent, PerkEvent.PerkEventType.EVENT_MAGIC_CHARGED, true);
			}
		}
		if (EGHPHELLOGO.KJDFJPBIGJC.Parameters.GetLifeDepleted())
		{
			EGHPHELLOGO.KJDFJPBIGJC.Parameters.IsDead = true;
		}
		SetSlowMotion(false);
		if (gHHCDAFIKJE.IsCritical || (gHHCDAFIKJE.IsHeadHit && !gHHCDAFIKJE.IsBlocked) || gHHCDAFIKJE.IsShock)
		{
			GameUtils.HitEffect pIHIIMOOICM = FindHitEffect(gHHCDAFIKJE.IsCritical, gHHCDAFIKJE.IsHeadHit && !gHHCDAFIKJE.IsBlocked, gHHCDAFIKJE.IsShock);
			if (pIHIIMOOICM != null)
			{
				_Camera.ApplyHitEffect(pIHIIMOOICM);
			}
		}
		// Eclipse: the archival DE CriticalEffect trigger plays snd_crit with the critical hit
		// effect; the shipped moves data only carries the effect, so play the sound once here.
        if (gHHCDAFIKJE.IsCritical && !gHHCDAFIKJE.IsBlocked)
		{
			Sound.PlaySound("snd_crit");
		}
		EGHPHELLOGO.KJDFJPBIGJC.ReceivedCritical = gHHCDAFIKJE.IsCritical;
		EGHPHELLOGO.KJDFJPBIGJC.set_IsShock(gHHCDAFIKJE.IsShock);
		RuleAppliance eJPOJJKKICO = ((!EGHPHELLOGO.KJDFJPBIGJC.IsPlayerModel()) ? RuleAppliance.AppliancePlayer : RuleAppliance.ApplianceOpponent);
		if (!eclipseActorContact) UpdateFightDataDamage(gHHCDAFIKJE, eJPOJJKKICO);
		_SelectAnimation.CheckEvent(EventAnimation.EventAnimationType.EVENT_HIT, EGHPHELLOGO);
		_SelectAnimation.CheckEvent(EventAnimation.EventAnimationType.EVENT_STRIKE, EGHPHELLOGO);
		if (!Module.GetInstance().IsUserTutorialComplete() && EGHPHELLOGO.KJDFJPBIGJC.HitCounter == GameUtils.CounterPunches)
		{
			EGHPHELLOGO.KJDFJPBIGJC.ReleaseWeakNodes();
		}
        // Extra fighters retain native hit reactions, health, perks and move events.
        // The archival duel rules/counters cannot represent an additional side.
        if (eclipseActorContact) return;
		CheckFightRules(FightEvent.HitEvent, EGHPHELLOGO.KJDFJPBIGJC.IsPlayerModel() ? RuleAppliance.AppliancePlayer : RuleAppliance.ApplianceOpponent);
		CheckFightRules(FightEvent.StrikeEvent, (!EGHPHELLOGO.KJDFJPBIGJC.IsPlayerModel()) ? RuleAppliance.AppliancePlayer : RuleAppliance.ApplianceOpponent);
		bool lGNDOAHHHNP = (ObscuredFloat)(kMMJCHDKBDO.GetCurrentLife()) == 0f;
		if (EGHPHELLOGO.Opponent.IsPlayerModel())
		{
			InfoAnimation dBOLBEOCEME = EGHPHELLOGO.Opponent.GetCurrentAnimation();
			counters.OnAnimationHit(dBOLBEOCEME, gHHCDAFIKJE.IsHeadHit, gHHCDAFIKJE.IsFirstStrike, gHHCDAFIKJE.IsDisarm, lGNDOAHHHNP, gHHCDAFIKJE.IsBlocked, gHHCDAFIKJE.IsShock);
			return;
		}
		counters.OnBlock(gHHCDAFIKJE.IsBlocked);
		if (gHHCDAFIKJE.IsShock)
		{
			wasPlayerShocked = true;
		}
	}

	public void CreateHitEffect(Vector3f NAAPALOFBCI, Vector3f KKIKIDNALOL, float time, string AJBGJNMLMKE, float NOOOCHHKECH)
	{
		_Camera.PlayEffectAnimation(NAAPALOFBCI, KKIKIDNALOL, time, false, AJBGJNMLMKE, NOOOCHHKECH);
	}

	public void UpdateModelAnimationParameters(Model ACENLMONNPA)
	{
		ACENLMONNPA.UpdateAnimationParameters(ActiveModels);
		ACENLMONNPA.UpdateAnimationParameters(pendingModels);
	}

	public void ResetRangedButton()
	{
		UpdateRangedButtonVisibility();
	}

	public void OnActionButtonPercentage(object data)
	{
		if (IsTitleSparring) return;
		Model.EventActBtnSettings bOMCDIIDKPD = (Model.EventActBtnSettings)data;
		// Versus magic is refreshed from the locally controlled fighter by the
		// session; recovered model events report only the campaign player's charge.
		if (IsLocalVersus && bOMCDIIDKPD.Button == FightCID.MagicButton) return;
		float num = bOMCDIIDKPD.Value * 100f;
		if (bOMCDIIDKPD.Button == FightCID.MagicButton && num > 97f && num < 100f)
		{
			num = 97f;
		}
		ActionButtons actionButtons = Controller.GetActionButtons();
		actionButtons.SetNeededPercentageToActBtn(bOMCDIIDKPD.Button, num, bOMCDIIDKPD.FrameCount);
	}

	public void OnActionButtonBulletsCount(object data)
	{
		if (IsTitleSparring) return;
		Model.EventActBtnSettings bOMCDIIDKPD = (Model.EventActBtnSettings)data;
		ActionButtons actionButtons = Controller.GetActionButtons();
		actionButtons.SetBulletsCountToActBtn(bOMCDIIDKPD.Button, bOMCDIIDKPD.BulletsCount);
	}

	public Model GetModelByAppliance(RuleAppliance EJPOJJKKICO)
	{
		switch (EJPOJJKKICO)
		{
		case RuleAppliance.AppliancePlayer:
			return _playerModel;
		case RuleAppliance.ApplianceOpponent:
			return _enemyModel;
		case RuleAppliance.ApplianceAll:
			GameLog.Error("Fight::getModelByAppliance ERROR - wrong appliance {0}", EJPOJJKKICO);
			break;
		}
		return null;
	}

	public void CreatePointsTable(float FNDOOJNDJDC, float GBCONNBABLL, PointsTableType NOPJGLHKJPG, int LOMKKEAMMIG, float CFMPJLLNCFF = 100f)
	{
		if (preFight != null)
		{
			preFight.CreatePointsTable(FNDOOJNDJDC, GBCONNBABLL, (int)CFMPJLLNCFF, NOPJGLHKJPG, LOMKKEAMMIG);
		}
	}

	public void UpdatePointsTable(int BBNOPLBAOCF, int HBIKJBGFFBM)
	{
		if (preFight != null)
		{
			preFight.UpdatePointsTable(BBNOPLBAOCF, HBIKJBGFFBM);
		}
	}

	public void RemovePointsTable()
	{
		if (preFight != null)
		{
			preFight.RemovePointsTable();
		}
	}

	public void RemoveCombo()
	{
		if (preFight != null && preFight.get_ViewerFight() != null)
		{
			preFight.get_ViewerFight().RemoveCombo();
		}
	}

	public void RechargeMagic(RuleAppliance EJPOJJKKICO)
	{
		Model fGCODGKLHED = null;
		switch (EJPOJJKKICO)
		{
		case RuleAppliance.AppliancePlayer:
			fGCODGKLHED = _playerModel;
			break;
		case RuleAppliance.ApplianceOpponent:
			fGCODGKLHED = _enemyModel;
			break;
		}
		fGCODGKLHED.InitializeMagicCharge();
	}

	public void ReloadPerks()
	{
		perksStage.ClearModels();
		ReloadModelPerks(GetPlayerModel());
		ReloadModelPerks(GetEnemyModel());
	}

	public void ReloadModelPerks(Model ACENLMONNPA)
	{
		ACENLMONNPA.Parameters.RefreshPerks();
		List<PerkInfoItem> list = GetRulePerks(ACENLMONNPA.IsPlayerModel());
		foreach (PerkInfoItem item in list)
		{
			ACENLMONNPA.Parameters.Perks.Add(item);
		}
		ACENLMONNPA.ReloadTriggers();
		List<NoPerksRule> gOMIMEDNKHH = GetRuleNoPerks(ACENLMONNPA.IsPlayerModel());
		_rulesInspector.ApplyNoPerksRules(ACENLMONNPA.Parameters, gOMIMEDNKHH);
		if (!ACENLMONNPA.IsPlayerModel() || FightDefinition.get_Type() == BattleType.FightRaid)
		{
		}
		perksStage.AddModel(ACENLMONNPA);
	}

	public List<PerkInfoItem> GetRulePerks(bool EKBOGDKIHIH)
	{
		return (!EKBOGDKIHIH) ? _rulesInspector.GetEnemyPerks() : _rulesInspector.GetPlayerPerks();
	}

	public List<NoPerksRule> GetRuleNoPerks(bool EKBOGDKIHIH)
	{
		return (!EKBOGDKIHIH) ? _rulesInspector.GetEnemyNoPerks() : _rulesInspector.GetPlayerNoPerks();
	}

	public void SortAchievements()
	{
		pendingAchievements.Sort((Achievement LHBNIMGFKIB, Achievement AAOIAEJJINO) => AAOIAEJJINO.Priority.CompareTo(LHBNIMGFKIB.Priority));
	}

	public void SetBotTactic(string BHNDJOGLEOI)
	{
		_enemyModel.SetTactic(BHNDJOGLEOI);
	}

	public FightCID MirrorControl(FightCID IHNNCICNEJE)
	{
		if (!isControlsInverted)
		{
			return IHNNCICNEJE;
		}
		switch (IHNNCICNEJE)
		{
		case FightCID.QuadrantUp:
			return FightCID.QuadrantDown;
		case FightCID.QuadrantUpForward:
			return FightCID.QuadrantDownBack;
		case FightCID.QuadrantForward:
			return FightCID.QuadrantBack;
		case FightCID.QuadrantDownForward:
			return FightCID.QuadrantUpBack;
		case FightCID.QuadrantDown:
			return FightCID.QuadrantUp;
		case FightCID.QuadrantDownBack:
			return FightCID.QuadrantUpForward;
		case FightCID.QuadrantBack:
			return FightCID.QuadrantForward;
		case FightCID.QuadrantUpBack:
			return FightCID.QuadrantDownForward;
		default:
			return IHNNCICNEJE;
		}
	}

	// best guess for name

	public void RefreshControllerLayout()
	{
		_Camera.RefreshControllerScale();
	}

	public virtual void OnUnusedEvent(object data)
	{
	}

	public void SetInputEnabled(bool state)
	{
		isInputEnabled = state;
	}

	public void RestartFight()
	{
		ResetFight();
		GameUtils.StartFight(FightDefinition, false, FightDefinition.Battle);
	}

	public void OnReservedHook()
	{
	}

	private void ProcessKillRetry(Model ACENLMONNPA)
	{
	}

	private bool KillModel(bool EKBOGDKIHIH, bool KIDOEGEPDKL)
	{
		if (FightDefinition.get_Type() == BattleType.FightNone)
		{
			return false;
		}
		List<InfoAnimation> list = new List<InfoAnimation>();
		AnimationData.AddTemplateAnimations("PhysicalFall", list);
		if (list.Count == 0)
		{
			return false;
		}
		InfoAnimation cMGIPKIPIPA = list[0];
		bool flag = false;
		foreach (Model item in ActiveModels)
		{
			if (item.IsPlayerModel() == EKBOGDKIHIH && item.HasEnemies() && !item.Parameters.RoundEnded && item.GetAnimationModule().GetIsPlaying() && item.GetAnimationModule().FindInterval(IntervalAnimation.IntervalType.INTERVAL_INVULNERABLE) == null)
			{
				item.EventData.Data = null;
				item.Parameters.IsDead = true;
				item.SetLife(0f);
				item.SetDelayedStrike(cMGIPKIPIPA, true);
				flag = true;
				if (KIDOEGEPDKL)
				{
					item.GetCombatTarget().Parameters.RoundsWon = round.roundTotal;
				}
			}
		}
		if (flag && KIDOEGEPDKL && !EKBOGDKIHIH && FightDefinition.HasMultipleOpponentsAndRounds())
		{
			currentEnemyIndex = enemyParametersList.Count - 1;
		}
		return flag;
	}

	// Narrow hook for Eclipse-owned developer tooling. This deliberately uses
	// the established win-fight path so rules, animations and rewards observe
	// the same state transition as the original debug control.
	public bool DebugDefeatOpponent()
	{
		// Models can become vulnerable during the start stance, before the FIGHT
		// inscription finishes and PlayFight advances the round state. Ending the
		// round there bypasses that transition and leaves the fight state machine
		// stranded, so debug victories must obey the same live-combat gate as the
		// recovered cheat buttons.
		if (stageType != StageType.Stage.STAGE_FIGHT)
		{
			return false;
		}
		return KillModel(false, true);
	}

	private void ChangeModelsSpeed(bool AMKJEICFNFL)
	{
		for (int i = 0; i < ActiveModels.Count; i++)
		{
			if (AMKJEICFNFL)
			{
				ActiveModels[i].ChangeSpeed(GameUtils.SlowModeSpeed);
			}
			else
			{
				ActiveModels[i].ChangeSpeed(1f / (float)GameUtils.SlowModeSpeed);
			}
		}
	}

	private void ResetFight()
	{
		if (FightDefinition.get_Type() == BattleType.FightNone)
		{
			return;
		}
		isGameOver = false;
		isStopFight = false;
		ResetParameters();
		round.round = 0;
		RemoveActiveModel(_enemyModel);
		foreach (ModelParameters item in enemyParametersList)
		{
			item.RoundsWon = 0;
			item.AddLife();
			item.IsWinner = false;
			item.IsDead = false;
			item.RewardsEnabled = true;
			item.RoundEnded = false;
			item.MovesInitialized = false;
			item.IsUntouched = true;
			item.UnusedRoundFlag = false;
			item.EndRoundType = EndRoundType.EndRoundTypeNone;
		}
		enemyParameters = enemyParametersList[0];
		enemyTactic = enemyParameters.FightTactic;
		currentEnemyIndex = 0;
		if (!AssemblyController.GetAiEnabled())
		{
			enemyParameters.AiControlled = false;
		}
		_enemyModel = AddModel(enemyParameters);
		RecreatePlayerModel();
		ModelParameters kMMJCHDKBDO = _playerModel.Parameters;
		kMMJCHDKBDO.AddLife();
		kMMJCHDKBDO.RoundsWon = 0;
		_Camera.SetFightVisible(false);
		perksStage.SetModels(ActiveModels);
		_SelectAnimation.set_Models(ActiveModels);
		if (preFight != null)
		{
			preFight.Reset();
			preFight.InitPreFight();
			preFight.ViewerPauseVisible(GetIsPauseButtonVisible());
		}
		SetStage(StageType.Stage.STAGE_NONE);
		StartVS();
	}

	private void ResetRound()
	{
		if (FightDefinition.get_Type() != BattleType.FightNone)
		{
			isGameOver = false;
			isStopFight = false;
			ResetParameters();
			round.round--;
			RemoveActiveModel(_enemyModel);
			enemyParametersList[currentEnemyIndex].SetCurrentLife(enemyRoundParam.Life);
			enemyParametersList[currentEnemyIndex].RoundsWon = enemyRoundParam.RoundsWon;
			enemyParameters = enemyParametersList[currentEnemyIndex];
			enemyTactic = enemyParameters.FightTactic;
			if (!AssemblyController.GetAiEnabled())
			{
				enemyParameters.AiControlled = false;
			}
			_enemyModel = AddModel(enemyParameters);
			_enemyModel.SetMagicChargeFraction(enemyRoundParam.MagicChargeFraction);
			_enemyModel.SetMagicCharges(enemyRoundParam.MagicCharges);
			_enemyModel.SetRaidBullets(enemyRoundParam.RaidCharges);
			_enemyModel.SetPowerMultiplier(enemyRoundParam.DamageMultiplier);
			RecreatePlayerModel();
			ModelParameters kMMJCHDKBDO = _playerModel.Parameters;
			kMMJCHDKBDO.SetCurrentLife(playerRoundParam.Life);
			kMMJCHDKBDO.RoundsWon = playerRoundParam.RoundsWon;
			_playerModel.SetMagicChargeFraction(playerRoundParam.MagicChargeFraction);
			_playerModel.SetMagicCharges(playerRoundParam.MagicCharges);
			_playerModel.SetRaidBullets(playerRoundParam.RaidCharges);
			_playerModel.SetPowerMultiplier(playerRoundParam.DamageMultiplier);
			_Camera.SetFightVisible(false);
			perksStage.SetModels(ActiveModels);
			_SelectAnimation.set_Models(ActiveModels);
			if (preFight != null)
			{
				preFight.Reset();
				preFight.InitPreFight();
				preFight.ViewerPauseVisible(GetIsPauseButtonVisible());
			}
			SetStage(StageType.Stage.STAGE_NONE);
			StartVS();
			if (preFight != null)
			{
				preFight.ViewerUpdateVictorys();
			}
		}
	}

	private void OnRenderCompleted()
	{
	}

	private void SetupController(GameController GOPFBDGGNGI)
	{
		if (!(GOPFBDGGNGI == null))
		{
			Controller = GOPFBDGGNGI;
			Controller.AddEventListener(0, ControlPress);
			Controller.AddEventListener(1, ControlRelease);
			Controller.ResetController();
			if (FightDefinition is Eclipse.Multiplayer.LocalVersusMatch localMatch)
				Controller.ConfigureLocalVersusInput(true, localMatch.Settings.KeyboardPlayerOne);
			Controller.Init();
			_Camera.SetController(Controller);
			Controller.IsShowController(AssemblyController.GetShowController());
			UpdateControlButtons(!get_isFightNone());
		}
	}

	private void PreloadEffects()
	{
		for (int i = 0; i < ActiveModels.Count; i++)
		{
			PreloadModelWeapons(ActiveModels[i]);
		}
	}

	private void PreloadModelWeapons(Model ACENLMONNPA)
	{
		List<InfoAnimation> list = ACENLMONNPA.GetAvailableAnimations();
		for (int i = 0; i < list.Count; i++)
		{
			InfoAnimation pJAHIOELGGD = list[i];
			for (int j = 0; j < pJAHIOELGGD.MoveData.Actions.Count; j++)
			{
				ActionAnimation gELPMIAIGDF = pJAHIOELGGD.MoveData.Actions[j];
				if (gELPMIAIGDF.get_Type() == ActionAnimation.ActionType.CREATE_MODEL)
				{
					ActionCreateModel kPFLDMNAFAP = (ActionCreateModel)gELPMIAIGDF;
					Model fGCODGKLHED = SpawnWeaponModel(ACENLMONNPA, kPFLDMNAFAP.GetCopyItems(), kPFLDMNAFAP.GetModelName());
					RequestModelRemoval(fGCODGKLHED);
					pendingModels.Remove(fGCODGKLHED);
				}
			}
		}
		ProcessRemovedModels();
	}

	private void ApplyLifeSteal(Model ACENLMONNPA, float CKKFKEIELCP)
	{
		string nJFGLOECJEK = GameUtils.GetLifesteal().Attribute;
		int OEMALIFPGPO = 0;
		if (ACENLMONNPA.Parameters.FinalAttributes.Get(nJFGLOECJEK, ref OEMALIFPGPO))
		{
			float num = (float)OEMALIFPGPO * GameUtils.GetLifesteal().Base * CKKFKEIELCP * (ACENLMONNPA.GetCombatTarget().GetPowerMultiplier() / ACENLMONNPA.GetPowerMultiplier());
			if (num != 0f)
			{
				UpdateLife(ACENLMONNPA, num);
			}
		}
	}

	private string GetModelInfoA(Model ACENLMONNPA)
	{
		return string.Empty;
	}

	private string GetModelInfoB(Model ACENLMONNPA)
	{
		return null;
	}

	private string GetModelInfoC(Model ACENLMONNPA)
	{
		return null;
	}

	private void CreateFighters()
	{
		_playerModel = AddModel(playerParameters);
		_enemyModel = AddModel(enemyParameters);
		LoadAiTactics();
		PreloadEffects();
	}

	private Model AddModel(ModelParameters JCICKLIMBEF)
	{
		JCICKLIMBEF.SceneType = SceneTypes.SceneFight;
		Model fGCODGKLHED = new Model(JCICKLIMBEF);
		fGCODGKLHED.AttachToParent();
		foreach (Model item in ActiveModels)
		{
			if (item != fGCODGKLHED)
			{
				if (item == null)
				{
					GameLog.Error("enemy is null");
				}
				fGCODGKLHED.AddEnemy(item);
				item.AddEnemy(fGCODGKLHED);
			}
		}
		fGCODGKLHED.Index = _Camera.AddModel(fGCODGKLHED, JCICKLIMBEF.IsPlayer, true);
		SetModelOnListening(fGCODGKLHED);
		perksStage.AddModel(fGCODGKLHED);
		_SelectAnimation.AddModel(fGCODGKLHED);
		ActiveModels.Add(fGCODGKLHED);
		return fGCODGKLHED;
	}

    private Model[] NativeActorModels() => ActiveModels.Concat(pendingModels).Distinct().ToArray();

    // Prepared models own their native resources until this registration commits.
    // Actor team identity must not replace the canonical camera focus identity.
    private Action RegisterActorNative(Model model)
    {
        bool camera = false, perks = false, animation = false;
        Action undo = () =>
        {
            ActiveModels.Remove(model);
            if (animation) _SelectAnimation.RemoveModel(model);
            if (perks) perksStage.RemoveModel(model);
            if (camera) _Camera.RemoveObject(model);
            model.RemoveAllEventListener();
        };
        try
        {
            model.Index = _Camera.AddModel(model, false, true); camera = true;
            SetModelOnListening(model);
            perksStage.AddModel(model); perks = true;
            _SelectAnimation.AddModel(model); animation = true;
            ActiveModels.Add(model);
            model.RoundStage = (int)stageType;
            model.SetInputEnabled(true);
        }
        catch { undo(); throw; }
        return undo;
    }

    private void PrepareActorNative(Model model) => _SelectAnimation.PrepareFormAnimation(model);

	private void OnReservedPrivateHook()
	{
	}

	private void ResolveRoundWinner()
	{
		_playerModel.GetStatistics().SetStyle(GetMaxStyle(0));
		_enemyModel.GetStatistics().SetStyle(GetMaxStyle(1));
		int num = 0;
		ComboStatistic aBPJBNADBLA = null;
		ComboStatistic aBPJBNADBLA2 = null;
		if (preFight != null)
		{
			num = preFight.get_TimeLeft();
			aBPJBNADBLA = preFight.GetStatistic(0);
			aBPJBNADBLA2 = preFight.GetStatistic(1);
		}
		ModelParameters kIKOGDEPGHB;
		ModelParameters lEBLJJCFKOP;
		if (playerParameters.IsWinner)
		{
			kIKOGDEPGHB = playerParameters;
			lEBLJJCFKOP = enemyParameters;
			if (kIKOGDEPGHB.RewardsEnabled)
			{
				_playerModel.GetStatistics().RegisterWin();
			}
			gameOverParameters.GameOverType = GameOverTypes.GAME_OVER_WIN;
		}
		else
		{
			hasNotLostRound = false;
			kIKOGDEPGHB = enemyParameters;
			lEBLJJCFKOP = playerParameters;
			if (kIKOGDEPGHB.RewardsEnabled)
			{
				_enemyModel.GetStatistics().RegisterWin();
			}
			if (num <= 0 && FightDefinition.get_Type() == BattleType.FightRaid)
			{
				gameOverParameters.GameOverType = GameOverTypes.GAME_OVER_RAID_ROUND_TIMEOUT;
			}
			else
			{
				gameOverParameters.GameOverType = GameOverTypes.GAME_OVER_LOSS;
			}
		}
		CheckCountersEndRound(kIKOGDEPGHB, lEBLJJCFKOP);
		isRoundResultPending = true;
		gameOverParameters.Winner = kIKOGDEPGHB;
		gameOverParameters.Loser = lEBLJJCFKOP;
	}

	private void StartNextEnemy(bool IHBIGLMLKKG = true)
	{
		ResetParameters();
		SaveMagicBuffer(_enemyModel);
		int fCOALLOHJNP = enemyParameters.RoundsWon;
		if (IHBIGLMLKKG)
		{
			_enemyModel.SetTactic(enemyTactic);
			RemoveActiveModel(_enemyModel);
			currentEnemyIndex++;
			enemyParameters = enemyParametersList[currentEnemyIndex];
			enemyParameters.SpawnPosition = _location.enemyStartPosition;
			enemyParameters.RoundsWon = fCOALLOHJNP;
			enemyTactic = enemyParameters.FightTactic;
			if (!AssemblyController.GetAiEnabled())
			{
				enemyParameters.AiControlled = false;
			}
			_rulesInspector.ApplyNoPerksRules(enemyParameters, _rulesInspector.GetEnemyNoPerks());
			_enemyModel = AddModel(enemyParameters);
			_isRoundOver = true;
			PreloadEffects();
			RestoreMagicBuffer(_enemyModel);
		}
		enemyParameters.CopyEquippedItemsTo(enemyEquippedItems);
		perksStage.SetModels(ActiveModels);
		_SelectAnimation.set_Models(ActiveModels);
		isGameOver = false;
		isStopFight = false;
		if (preFight != null)
		{
			ComboStatistic statistic = preFight.GetStatistic(0);
			ComboStatistic statistic2 = preFight.GetStatistic(1);
			preFight.Reset();
			preFight.InitPreFight(statistic, statistic2);
			preFight.ViewerPauseVisible(GetIsPauseButtonVisible());
		}
		SetStage(StageType.Stage.STAGE_NONE);
		StartVS();
	}

	// best guess for name
	private void RequestModelRemoval(object data)
	{
		Model fGCODGKLHED = (Model)data;
		if (!fGCODGKLHED.IsSlowMotionAllowed())
		{
			SetSlowMotion(false);
		}
		modelsToRemove.AddIfNotExist(fGCODGKLHED);
	}

	private void RemoveModelByIndex(int index, Model LEKHCMIFJAO = null)
	{
		int count = ActiveModels.Count;
		Model fGCODGKLHED = null;
		if (count == 0 || index < 0 || count - 1 < index)
		{
			fGCODGKLHED = LEKHCMIFJAO;
		}
		else
		{
			fGCODGKLHED = ActiveModels[index];
			ActiveModels.Remove(fGCODGKLHED);
		}
		RemoveModel(fGCODGKLHED);
	}

	private void RemoveActiveModel(Model ACENLMONNPA)
	{
		int num = 0;
		foreach (Model item in ActiveModels)
		{
			if (item == ACENLMONNPA)
			{
				RemoveModelByIndex(num);
				return;
			}
			num++;
		}
		RemoveModel(ACENLMONNPA);
	}

	private void UpdatePerkAreaModels()
	{
		if (isPerkAreaActive)
		{
			UpdateModelPerkArea(_playerModel);
			UpdateModelPerkArea(_enemyModel);
		}
	}

	private void UpdateModelPerkArea(Model ACENLMONNPA)
	{
		float num = ACENLMONNPA.GetBodyObject().GetPivotNode().GetStart()
			.GetX();
		if (!ACENLMONNPA.IsInsideArea())
		{
			if (isPerkAreaEnabled && num >= perkAreaMinX && num <= perkAreaMaxX)
			{
				ACENLMONNPA.SetInsideArea(true);
				perksStage.FireEvent(ACENLMONNPA, PerkEvent.PerkEventType.EVENT_AREA_ENTER);
			}
		}
		else if (!isPerkAreaEnabled || num < perkAreaMinX || num > perkAreaMaxX)
		{
			ACENLMONNPA.SetInsideArea(false);
			perksStage.FireEvent(ACENLMONNPA, PerkEvent.PerkEventType.EVENT_AREA_EXIT);
		}
	}

	private void ActionModels(bool value)
	{
		foreach (Model item in ActiveModels)
		{
			item.SetInputEnabled(value);
		}
	}

	private void ResetModels(bool ABFHKKILGOP)
	{
        CancelEclipseActors("round_ended");
		foreach (Model item in ActiveModels)
		{
			item.ResetToStartPosition();
			if (item.GetParentModel() != null)
			{
				RequestModelRemoval(item);
			}
		}
		perksStage.Reset();
		_SelectAnimation.Reset();
		ProcessRemovedModels();
	}

	private void StartPunchbag()
	{
		PrepareRound();
		ReloadPerks();
		UpdateControlButtons(!get_isFightNone());
		StartStance();
		ActionModels(true);
		NetworkController.GetInstance().Ledger.Check();
	}

	private void StartVS()
	{
		round.processing = false;
		round.roundTotal = FightDefinition.RoundsToWin;
		round.time = 0;
		round.timeTotal = FightDefinition.EffectiveRoundTime;
		List<ModelParameters> list = null;
		int num = 0;
		Battle cNAOMDMIGLJ = FightDefinition.Battle;
		bool bBBNBKIMHJC = false;
		bool flag = true;
		bool flag2 = true;
		flag2 = !FightDefinition.HasMultipleOpponentsAndRounds() || (playerParameters.RoundsWon == 0 && enemyParameters.RoundsWon == 0);
		flag = flag2;
		if (cNAOMDMIGLJ.get_Type() == BattleType.FightBosses || cNAOMDMIGLJ.get_Type() == BattleType.FightBossesReplayable || cNAOMDMIGLJ.get_Type() == BattleType.FightFinalTitan)
		{
			list = new List<ModelParameters>();
			foreach (FightList item in cNAOMDMIGLJ.GetFights())
			{
				List<ModelParameters> list2 = GameUtils.CreateOpponentParameters(item.GetOpponents());
				if (list2.Count > 0)
				{
					list.Add(list2[0]);
				}
			}
			num = FightDefinition.Index;
			bBBNBKIMHJC = true;
		}
		else
		{
			list = enemyParametersList;
			num = currentEnemyIndex;
		}
		if (cNAOMDMIGLJ.get_Type() == BattleType.FightSurvival || cNAOMDMIGLJ.get_Type() == BattleType.FightRaid)
		{
			flag = false;
			flag2 = false;
		}
		bool eNCAKAAMEPN = UnderworldZonePolicy.ShouldShowRoundPips(cNAOMDMIGLJ);
		if (preFight != null)
		{
			preFight.CreateVS(playerParameters, list, num, bBBNBKIMHJC, flag2, flag);
			preFight.ViewerInit(round, playerParameters, enemyParameters, eNCAKAAMEPN);
			ScreenModel screenModel = ((!(preFight.get_ViewerFight() != null)) ? null : preFight.get_ViewerFight().get_LeftModel());
			if (screenModel != null)
			{
				screenModel.AddEventListener(0, OnStyleChanged);
			}
			else
			{
				GameLog.Error("Fight - Cant listen to ScreenModel user");
			}
			ScreenModel screenModel2 = ((!(preFight.get_ViewerFight() != null)) ? null : preFight.get_ViewerFight().get_RightModel());
			if (screenModel2 != null)
			{
				screenModel2.AddEventListener(0, OnStyleChanged);
			}
			else
			{
				GameLog.Error("Fight - Cant listen to ScreenModel bot");
			}
		}
	}

	private void NextRound()
	{
        _eclipseShields.Clear();
		// Let the runtime schedule collection; forcing it here stalls the round transition.
		playerRoundParam.Life = (ObscuredFloat)(playerParameters.GetCurrentLife());
		playerRoundParam.MagicChargeFraction = _playerModel.GetMagicChargeFraction();
		playerRoundParam.MagicCharges = _playerModel.GetMagicCharges();
		playerRoundParam.RaidCharges = _playerModel.GetRaidBullets();
		playerRoundParam.DamageMultiplier = _playerModel.GetPowerMultiplier();
		playerRoundParam.RoundsWon = playerParameters.RoundsWon;
		enemyRoundParam.Life = (ObscuredFloat)(enemyParametersList[currentEnemyIndex].GetCurrentLife());
		enemyRoundParam.MagicChargeFraction = _enemyModel.GetMagicChargeFraction();
		enemyRoundParam.MagicCharges = _enemyModel.GetMagicCharges();
		enemyRoundParam.RaidCharges = _enemyModel.GetRaidBullets();
		enemyRoundParam.DamageMultiplier = _enemyModel.GetPowerMultiplier();
		enemyRoundParam.RoundsWon = enemyParametersList[currentEnemyIndex].RoundsWon;
		Sound.StopLoopedSounds();
		_Camera.GetRender().UpdateEffects();
		ClearRuleVisuals();
		isStopFight = false;
		isFirstStrike = false;
		isEndRound = false;
		unusedFlagC = false;
		isControlsInverted = false;
		_endRoundType = EndRoundType.EndRoundTypeNone;
		_endFightRule = null;
		endStanceCounter = 0;
		wasPlayerShocked = false;
		round.round++;
        _eclipseRoundOutcomes.BeginRound(-1, null);
        CancelEclipseProjectiles();
        CancelEclipseActors("round_ended");
        CancelEclipseFighterMotion();
        CancelEclipseFighterPlayback();
		if (preFight != null)
		{
			preFight.ClearInscription();
			if (FightDefinition.get_Type() == BattleType.FightRaid)
			{
				preFight.CreateSkipRound();
			}
			else
			{
				preFight.CreateRound(round.round, false);
			}
		}
		PrepareRound();
		if (_isRoundOver)
		{
			LoadAiTactics();
			_isRoundOver = false;
		}
		playerParameters.SaveCurrentLife();
		foreach (Model item in ActiveModels)
		{
			item.NextRound(round.round);
			item.Parameters.EnableAllPerks();
		}
		perksStage.EnableAllPerks();
		// Presentation only: each round starts on a clean floor.
		Eclipse.Rendering.FighterParticles.ClearStains();
		DispatchEclipseCombatEvent();
		DispatchEclipseCombatEvent(ModEffectEvent.RoundBegin);
        DispatchEclipseOpponent(ModEffectEvent.FightBegin);
        DispatchEclipseOpponent(ModEffectEvent.RoundBegin);
		SetSlowMotion(false);
		_isRoundOver = false;
	}

	private readonly Dictionary<(Model, DefinitionId), System.Xml.XmlNode> _eclipseOpponentInstances = new Dictionary<(Model, DefinitionId), System.Xml.XmlNode>();
	private readonly Dictionary<(Model, DefinitionId), System.Xml.XmlNode> _eclipseInnateInstances = new Dictionary<(Model, DefinitionId), System.Xml.XmlNode>();
	private void DispatchEclipseHitPhase(Model.EventModel eventModel, Model.StrikeResult strike, ModEffectEvent effectEvent, ModAttackSource attackSource = null)
	{
		if (IsLocalVersus || !_eclipseFightBeginDispatched || eventModel == null || strike == null || ModRuntime.Scripts == null) return;
		Model target = eventModel.KJDFJPBIGJC;
		// Post-critical dispatch precedes refresh of the reusable EventModel target.
        // The current strike supplies the actual attacker in every hit phase.
		Model attacker = (strike.AttackerModel ?? eventModel.Opponent)?.GetRootModel();
		attackSource = attackSource ?? CaptureEclipseAttackSource(strike.AttackerModel ?? eventModel.Opponent, strike);
		InfoAnimation animation = strike.AttackAnimation;
		bool weapon = animation != null && animation.HasName("Weapon");
		bool unarmed = animation != null && animation.HasName("Unarmed");
		bool ranged = animation != null && animation.HasName("RangedMissile");
		bool magic = animation != null && animation.HasName("MagicMissile");
		var attackerHit = new ModIncomingHit(() => strike.FinalDamage, amount => strike.FinalDamage = (float)amount,
			strike.IsBlocked, strike.IsCritical, new ModHitEvent(false, weapon, unarmed, ranged, magic), attackSource);
		var targetHit = new ModIncomingHit(() => strike.FinalDamage, amount => strike.FinalDamage = (float)amount,
			strike.IsBlocked, strike.IsCritical, new ModHitEvent(true, weapon, unarmed, ranged, magic), attackSource);
		if (attacker == _playerModel) DispatchEclipseCombatEvent(effectEvent, null, attackerHit);
		else if (attacker == _enemyModel) DispatchEclipseOpponent(effectEvent, null, attackerHit);
        else DispatchEclipseActor(attacker,effectEvent,incoming:attackerHit);
		if (target == _playerModel) DispatchEclipseCombatEvent(effectEvent, null, targetHit);
		else if (target == _enemyModel) DispatchEclipseOpponent(effectEvent, null, targetHit);
        else DispatchEclipseActor(target,effectEvent,incoming:targetHit);
	}

	private bool _eclipseOpponentDispatching;
    private readonly Queue<(int Round, ModAnimationLifecycleEvent Player, ModAnimationLifecycleEvent Opponent, List<(OwnedActor Actor, ModAnimationLifecycleEvent Event)> Actors)> _eclipseAnimationEvents =
        new Queue<(int, ModAnimationLifecycleEvent, ModAnimationLifecycleEvent, List<(OwnedActor, ModAnimationLifecycleEvent)>)>();
    private bool _drainingEclipseAnimationEvents;

    private void NotifyEclipseAnimation(Model actor, InfoAnimation animation, ModEffectEvent kind)
    {
        if (IsLocalVersus || !_eclipseFightBeginDispatched || _eclipseFightEndDispatched ||
            !round.processing || actor == null || string.IsNullOrEmpty(animation?.Name) ||
            ModRuntime.Scripts == null || !ModRuntime.Scripts.HasHandlers(kind)) return;
        // Capture both perspectives now: a callback can replace a fighter's body.
        var player = new ModAnimationLifecycleEvent(kind, animation.Name,
            actor == _playerModel ? "self" : actor == _enemyModel ? "opponent" : "other", fightTimeInFrame);
        var opponent = new ModAnimationLifecycleEvent(kind, animation.Name,
            actor == _enemyModel ? "self" : actor == _playerModel ? "opponent" : "other", fightTimeInFrame);
        if (_eclipseAnimationEvents.Count >= 256)
        {
            UnityEngine.Debug.LogWarning("[ModCombat] Animation callback queue limit reached; event discarded.");
            return;
        }
        _eclipseAnimationEvents.Enqueue((round.round, player, opponent, CaptureEclipseActorAnimations(actor,player)));
        DrainEclipseAnimationEvents();
    }

    private void DrainEclipseAnimationEvents()
    {
        if (_drainingEclipseAnimationEvents || _eclipseCombatDispatching || _eclipseOpponentDispatching || _eclipseActorDispatching ||
            _eclipseAnimationEvents.Count == 0) return;
        _drainingEclipseAnimationEvents = true;
        try
        {
            int delivered = 0;
            while (_eclipseAnimationEvents.Count != 0)
            {
                if (++delivered > 256)
                {
                    _eclipseAnimationEvents.Clear();
                    UnityEngine.Debug.LogWarning("[ModCombat] Animation callback cascade limit reached; remaining events discarded.");
                    break;
                }
                var next = _eclipseAnimationEvents.Dequeue();
                if (!round.processing || _eclipseFightEndDispatched || next.Round != round.round) continue;
                DispatchEclipseCombatEvent(next.Player.Type, animation: next.Player);
                if (round.processing && !_eclipseFightEndDispatched && next.Round == round.round)
                    DispatchEclipseOpponent(next.Opponent.Type, animation: next.Opponent);
                if (round.processing && !_eclipseFightEndDispatched && next.Round == round.round)
                    DispatchEclipseActorAnimations(next.Actors);
            }
        }
        finally { _drainingEclipseAnimationEvents = false; }
    }

    private void DispatchEclipseOpponent(ModEffectEvent effectEvent, ModDamageEvent damage = null, ModIncomingHit incoming = null, ModCombatActivityEvent activity = null, ModAnimationLifecycleEvent animation = null)
    {
        if (IsLocalVersus) return;
        if (effectEvent == ModEffectEvent.Tick && (_eclipseEndedRound == round.round || _eclipseFightEndDispatched)) return;
        if (_eclipseOpponentDispatching || _eclipseCombatDispatching || _eclipseActorDispatching || _enemyModel == null || ModRuntime.Scripts == null) return;
        if (effectEvent == ModEffectEvent.FightBegin && round.round != 1) return;
        _eclipseOpponentDispatching = true;
        try
        {
            var scripts = ModRuntime.Scripts;
            ModRuntime.DispatchBattleRules(_eclipseBattleRules, FightDefinition.FightId.ToString(), false,
                round.round, ListSF.GetRoster().IsEclipseMode(), _eclipseFightId, _eclipsePlayerResult, effectEvent,
                new EclipseFighterOperations(this, _enemyModel, damage, incoming, activity, animation));
            var active = new HashSet<DefinitionId>();
            foreach (var runtimePerk in _enemyModel.Parameters.Perks)
            {
                if (runtimePerk == null || !DefinitionId.TryParse(runtimePerk.Name, out var id) || !active.Add(id) ||
                    !scripts.Content.TryGetPerk(id, out var perk) || !perk.HasBehavior ||
                    !scripts.HasBehaviorHandler(perk.Behavior, effectEvent)) continue;
                if (!_eclipseOpponentInstances.TryGetValue((_enemyModel, id), out var node))
                {
                    var document = new System.Xml.XmlDocument(); document.LoadXml("<Perk/>");
                    _eclipseOpponentInstances[(_enemyModel, id)] = node = document.DocumentElement;
                }
                var context = new Dictionary<string,string>
                {
                    { "side", "opponent" }, { "source", "warrior" }, { "perk_id", id.ToString() },
                    { "fight_id", _eclipseFightId }, { "round", round.round.ToString() }, { "player_result", _eclipsePlayerResult }
                };
                scripts.Content.TryGetBehavior(perk.Behavior, out var behavior);
                var fighter = new ModInstanceFighter(new EclipseFighterOperations(this, _enemyModel, damage, incoming, activity, animation), node);
                if (!scripts.TryInvokeBehavior(perk.Behavior, effectEvent, behavior.Parameters.ResolveValues(perk.InitialParameters), context, fighter, out var error))
                    UnityEngine.Debug.LogWarning("[ModCombat] " + effectEvent + " failed for opponent perk " + id + ": " + error);
            }
        }
        catch (Exception exception) { UnityEngine.Debug.LogWarning("[ModCombat] Opponent dispatch failed: " + exception.Message); }
        finally { _eclipseOpponentDispatching = false; DrainEclipseAnimationEvents(); }
    }

	private ModBattleRuleInstances _eclipseBattleRules = new ModBattleRuleInstances();
    private readonly ModRoundOutcomeState _eclipseRoundOutcomes = new ModRoundOutcomeState();

    private bool TryQueueRoundOutcome(DefinitionId rule, bool playerWins, out string error)
    {
        if (GetCurrentFight() != this || IsLocalVersus || IsTitleSparring || FightDefinition == null ||
            FightDefinition.get_Type() == BattleType.FightNone || FightDefinition.get_Type() == BattleType.FightPVP ||
            get_IsRaidFight() && !ModModeRuntime.IsRaid(FightDefinition) ||
            stageType != StageType.Stage.STAGE_FIGHT || !round.processing || IsPaused() ||
            isEndRound || isGameOver || isStopFight || _eclipseFightEndDispatched ||
            _endFightRule != null || preFight != null && preFight.IsTimeOut() ||
            GetPlayerModel() == null || GetEnemyModel() == null ||
            new EclipseFighterOperations(this, GetPlayerModel()).Health <= 0 ||
            new EclipseFighterOperations(this, GetEnemyModel()).Health <= 0)
        { error = "Round outcomes require an active offline round without a pending native result."; return false; }
        try
        {
            var scripts = ModRuntime.Scripts;
            if (scripts == null) { error = "Mod scripts are unavailable."; return false; }
            if (_eclipseRoundOutcomes.Round != round.round)
                _eclipseRoundOutcomes.BeginRound(round.round, _eclipseBattleRules.OutcomeAuthority(scripts.Content,
                    FightDefinition.FightId.ToString(), round.round, ListSF.GetRoster().IsEclipseMode(),
                    ModModeRuntime.ActiveRules(FightDefinition.FightId.ToString())));
            return _eclipseRoundOutcomes.TryRequest(rule, playerWins, out error);
        }
        catch (Exception exception) { error = exception.Message; return false; }
    }
	private bool _eclipseCombatDispatching;
	private void DispatchEclipseCombatEvent(ModEffectEvent effectEvent = ModEffectEvent.FightBegin, ModDamageEvent damageEvent = null, ModIncomingHit incomingHit = null, ModCombatActivityEvent activity = null, ModAnimationLifecycleEvent animation = null)
	{
		if (IsLocalVersus) return;
        if (effectEvent == ModEffectEvent.Tick && (_eclipseEndedRound == round.round || _eclipseFightEndDispatched)) return;
		if (_eclipseCombatDispatching || _eclipseOpponentDispatching || _eclipseActorDispatching) return;
		if (effectEvent == ModEffectEvent.FightBegin)
		{
			if (_eclipseFightBeginDispatched || round.round != 1) return;
			_eclipseFightBeginDispatched = true;
		}
		_eclipseCombatDispatching = true;

		try
		{
				ModScriptSession scripts = ModRuntime.Scripts;
				if (scripts == null || playerParameters == null || !playerParameters.IsPlayer || _playerModel == null) return;
				var fighterOperations = new EclipseFighterOperations(this, _playerModel, damageEvent, incomingHit, activity, animation,
                    effectEvent == ModEffectEvent.FightBegin || effectEvent == ModEffectEvent.RoundBegin);
                ModRuntime.DispatchBattleRules(_eclipseBattleRules, FightDefinition.FightId.ToString(), true,
                    round.round, ListSF.GetRoster().IsEclipseMode(), _eclipseFightId, _eclipsePlayerResult, effectEvent, fighterOperations);

				var activeRuntimePerks = new HashSet<string>(StringComparer.Ordinal);
			foreach (PerkInfoItem perk in playerParameters.Perks)
			{
					if (perk != null && !string.IsNullOrEmpty(perk.Name)) activeRuntimePerks.Add(perk.Name);
				}

				// Learned/profile perks are a separate provenance source from item enchantments. Intersect
				// them with the final active runtime set so recovered NoPerks/rule filtering still wins.
				var dispatchedPerks = new HashSet<DefinitionId>();
				foreach (PerkInfoItem learnedPerk in playerParameters.LearnedPerks)
				{
					if (learnedPerk == null || string.IsNullOrEmpty(learnedPerk.Name) ||
						!activeRuntimePerks.Contains(learnedPerk.Name)) continue;
					DefinitionId perkId;
					if (!DefinitionId.TryParse(learnedPerk.Name, out perkId) || perkId.Category != "perks" ||
						perkId.Namespace.Value == "core" || dispatchedPerks.Contains(perkId)) continue;
					PerkDefinition perkDefinition;
					if (!scripts.Content.TryGetPerk(perkId, out perkDefinition) || !perkDefinition.HasBehavior ||
                        !scripts.HasBehaviorHandler(perkDefinition.Behavior, effectEvent)) continue;

					var perkContext = new Dictionary<string, string>(StringComparer.Ordinal)
					{
						{ "side", "player" },
						{ "fight_id", _eclipseFightId },
						{ "round", round.round.ToString() }, { "player_result", _eclipsePlayerResult },
						{ "source", "perk" },
						{ "perk_id", perkId.ToString() },
					};
					RosterPerk savedPerk = ListSF.GetRoster().GetPerks()?.FindPerk(learnedPerk.Name);
					if (savedPerk == null || savedPerk.Node == null) continue;
					dispatchedPerks.Add(perkId);
					string perkError;
					if (!ModRuntime.TryInvokeSavedPerkFightBegin(savedPerk.Node, perkContext, fighterOperations, out perkError, effectEvent))
						UnityEngine.Debug.LogWarning("[ModCombat] " + effectEvent + " failed for perk '" + perkId + "': " + perkError);
				}

                foreach (ItemInfo equipment in playerParameters.GetEquippedItems())
                {
                    if (equipment == null) continue;
                    foreach (PerkInfoItem innate in equipment.InnatePerks)
                    {
                        if (innate == null || !activeRuntimePerks.Contains(innate.Name) ||
                            !DefinitionId.TryParse(innate.Name, out var perkId) || perkId.Category != "perks" ||
                            perkId.Namespace.Value == "core" ||
                            !scripts.Content.TryGetPerk(perkId, out var definition) || !definition.HasBehavior ||
                            !scripts.HasBehaviorHandler(definition.Behavior, effectEvent) || !dispatchedPerks.Add(perkId)) continue;
                        if (!_eclipseInnateInstances.TryGetValue((_playerModel, perkId), out var node))
                        {
                            var document = new System.Xml.XmlDocument();
                            var element = document.CreateElement("Perk"); document.AppendChild(element);
                            element.SetAttribute("Name", perkId.ToString());
                            _eclipseInnateInstances[(_playerModel, perkId)] = node = element;
                        }
                        var context = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            { "side", "player" }, { "source", "innate" }, { "perk_id", perkId.ToString() },
                            { "item_type", equipment.Type ?? string.Empty }, { "item_id", equipment.Name ?? string.Empty },
                            { "fight_id", _eclipseFightId }, { "round", round.round.ToString() }, { "player_result", _eclipsePlayerResult }
                        };
                        if (!ModRuntime.TryInvokeSavedPerkFightBegin(node, context, fighterOperations, out var error, effectEvent))
                            UnityEngine.Debug.LogWarning("[ModCombat] " + effectEvent + " failed for innate perk '" + perkId + "': " + error);
                    }
                }
				UserItems userItems = ListSF.GetRoster().GetInventory();
			if (userItems == null) return;
			foreach (ItemInfo item in playerParameters.GetEquippedItems())
			{
				// Mirror ModelParameters.JBIOECDAAKP(): rule-created/replaced item clones do not
				// consume the player's saved UserItem enchantments.
				if (item == null || item.IgnoreInventoryEnchantments) continue;
				UserItem userItem = userItems.FindItem(item);
				System.Xml.XmlNode enchantments = userItem?.Node?["Enchantments"];
				if (enchantments == null) continue;

					var context = new Dictionary<string, string>(StringComparer.Ordinal)
					{
						{ "side", "player" },
						{ "fight_id", _eclipseFightId },
						{ "round", round.round.ToString() }, { "player_result", _eclipsePlayerResult },
						{ "source", "enchantment" },
						{ "item_type", item.Type ?? string.Empty },
					{ "item_id", item.Name ?? string.Empty },
				};

				var savedPerks = new List<System.Xml.XmlNode>();
				foreach (System.Xml.XmlNode perkNode in enchantments.ChildNodes)
				{
					if (perkNode.NodeType == System.Xml.XmlNodeType.Element && perkNode.Name == "Perk")
						savedPerks.Add(perkNode);
				}

				foreach (System.Xml.XmlNode perkNode in savedPerks)
				{
					try
					{
						string runtimeName = perkNode.Attributes?["Name"]?.Value;
						if (string.IsNullOrEmpty(runtimeName) || !activeRuntimePerks.Contains(runtimeName)) continue;

						DefinitionId enchantmentId;
						string savedId = perkNode.Attributes?[PerkStruct.EclipseEnchantmentAttribute]?.Value;
						// Forge recipes can also install behavior-backed perk definitions directly.
						// Their saved Name is the identity; they have no EclipseEnchantment marker.
						if (string.IsNullOrEmpty(savedId))
						{
							DefinitionId perkId;
							PerkDefinition perkDefinition;
							if (!DefinitionId.TryParse(runtimeName, out perkId) || perkId.Category != "perks" ||
								perkId.Namespace.Value == "core" || !scripts.Content.TryGetPerk(perkId, out perkDefinition) ||
								!perkDefinition.HasBehavior || !scripts.HasBehaviorHandler(perkDefinition.Behavior, effectEvent) ||
                                !dispatchedPerks.Add(perkId)) continue;
							var perkContext = new Dictionary<string, string>(context, StringComparer.Ordinal);
							perkContext.Remove("enchantment_id");
							perkContext["perk_id"] = perkId.ToString();
							string perkError;
							if (!ModRuntime.TryInvokeSavedPerkFightBegin(perkNode, perkContext, fighterOperations, out perkError, effectEvent))
								UnityEngine.Debug.LogWarning("[ModCombat] " + effectEvent + " failed for perk '" + perkId +
									"' on item '" + item.Name + "': " + perkError);
							continue;
						}
						if (!DefinitionId.TryParse(savedId, out enchantmentId) ||
							enchantmentId.Category != "enchantments" || enchantmentId.Namespace.Value == "core") continue;

						EnchantmentDefinition definition;
						if (!scripts.Content.TryGetEnchantment(enchantmentId, out definition) || !definition.HasBehavior ||
                            !scripts.HasBehaviorHandler(definition.Behavior, effectEvent)) continue;

						context["enchantment_id"] = enchantmentId.ToString();
						string error;
							if (!ModRuntime.TryInvokeSavedEnchantmentFightBegin(perkNode, context, fighterOperations, out error, effectEvent))
						{
							UnityEngine.Debug.LogWarning("[ModCombat] " + effectEvent + " failed for '" + enchantmentId +
								"' on item '" + item.Name + "': " + error);
						}
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogWarning("[ModCombat] " + effectEvent + " node dispatch failed on item '" +
							item.Name + "': " + exception.Message);
					}
				}
			}
		}
		catch (Exception exception)
		{
			// Mod combat dispatch must never break the recovered fight state machine.
			UnityEngine.Debug.LogWarning("[ModCombat] " + effectEvent + " dispatch failed: " + exception);
		}
		finally { _eclipseCombatDispatching = false; DrainEclipseAnimationEvents(); }
	}

	private void StartStance()
	{
		_Camera.SetFightVisible(true);
		SetStage(StageType.Stage.STAGE_START_STANCE);
	}

	private void FinishStance(ModelParameters ABKBEJBICOA, ModelParameters LEBLJJCFKOP, EndRoundType LFLGCDNKNJI)
	{
		endStanceCounter = 0;
		SetStage(StageType.Stage.STAGE_END_STANCE);
	}

	private void StartFight()
	{
		if (preFight != null)
		{
			preFight.CreateFight();
		}
	}

	private void PlayFight()
	{
		SetStage(StageType.Stage.STAGE_FIGHT);
		if (preFight != null)
		{
			preFight.ViewerPlay();
		}
		ActionModels(true);
		_rulesInspector.RulesActive = true;
		ApplyHeldKeys();
		if (killOpponentOnStart)
		{
			retryKillOpponent = !KillModel(false, false);
			killOpponentOnStart = false;
		}
	}

	private void OnStopPreFight(ScreenFightType data)
	{
		if (preFight == null)
		{
			return;
		}
		switch (preFight.get_Type())
		{
		case ScreenFightType.TYPE_INFO_VS:
			NextRound();
			break;
		case ScreenFightType.TYPE_INFO_ROUND:
		case ScreenFightType.TYPE_INFO_SKIP_ROUND:
			StartStance();
			if (_currentFight.GetFightDefinition() != null && _currentFight.GetFightDefinition().get_Type() == BattleType.FightRaid && _currentFight.GetFightDefinition().GetDescription() != string.Empty && preFight != null)
			{
				preFight.CreateFightRule();
			}
			break;
		case ScreenFightType.TYPE_INFO_FIGHT:
			PlayFight();
			break;
		case ScreenFightType.TYPE_INFO_FIGHT_RULE:
			break;
		}
	}

	private void OnButtonClick(ViewerFight.ViewerButton LFLGCDNKNJI)
	{
		switch (LFLGCDNKNJI)
		{
		case ViewerFight.ViewerButton.ButtonPause:
			OpenPauseScreen();
			break;
		case ViewerFight.ViewerButton.ButtonPauseSurrender:
			DialogsOpener.OpenSurrenderDialog(SurrenderButtonCallback);
			break;
		case ViewerFight.ViewerButton.ButtonPausePlay:
			ClosePauseScreen();
			break;
		default:
			OnCheatClicked(LFLGCDNKNJI);
			break;
		}
	}

	private void SurrenderButtonCallback(object data)
	{
		ClosePauseScreen();
		Surrender();
	}

	private void OnCheatClicked(ViewerFight.ViewerButton LFLGCDNKNJI)
	{
		if (stageType == StageType.Stage.STAGE_FIGHT)
		{
			switch (LFLGCDNKNJI)
			{
			case ViewerFight.ViewerButton.ButtonCheatLoseFight:
				KillModel(true, true);
				break;
			case ViewerFight.ViewerButton.ButtonCheatWinFight:
				KillModel(false, true);
				break;
			case ViewerFight.ViewerButton.ButtonCheatStartBenchmark:
				break;
			case ViewerFight.ViewerButton.ButtonCheatWinRound:
			case ViewerFight.ViewerButton.ButtonCheatLoseRound:
				break;
			}
		}
	}

	private bool RenderRaidEndFight()
	{
		return isRaidEndFight;
	}

	private void RenderRound()
	{
		if (isRoundResultPending)
		{
			bool flag = gameOverParameters.Winner.RoundsWon >= round.roundTotal;
			if (get_IsRaidFight() && flag)
			{
				isRoundResultPending = false;
				isHealthRestored = false;
				GameOver(gameOverParameters.Winner, gameOverParameters.Loser);
			}
			else
			{
				if (isAchievementBlocking || isShowingAchievement)
				{
					return;
				}
				isRoundResultPending = false;
				bool flag2 = gameOverParameters.Winner.IsPlayer && currentEnemyIndex < enemyParametersList.Count - 1;
				bool flag3 = FightDefinition.HasMultipleOpponentsAndRounds();
				isHealthRestored = false;
				if ((!flag && flag3) || (flag && flag2))
				{
					_Camera.SetFightVisible(false);
					ResetModels(false);
					bool flag4 = true;
					flag4 = !flag3 || (flag3 && playerParameters == gameOverParameters.Winner);
					StartNextEnemy(flag4);
					if (flag3)
					{
						preFight.ViewerUpdateVictorys();
					}
					else
					{
						playerParameters.RoundsWon = 0;
					}
				}
				else if (flag)
				{
					GameOver(gameOverParameters.Winner, gameOverParameters.Loser);
				}
				else
				{
					_Camera.SetFightVisible(false);
					ResetModels(false);
					ResetParameters();
					NextRound();
				}
			}
		}
		else if (isGameOver)
		{
			if (get_IsRaidFight())
			{
				EndFightRaid();
			}
			else if (!isAchievementBlocking && !isShowingAchievement)
			{
				EndFight();
			}
		}
		else if (isStopFight && !isGameOver)
		{
			ResolveRoundWinner();
		}
		else if (FightDefinition.get_Type() != BattleType.FightNone && round.processing && (playerParameters.IsDead || enemyParameters.IsDead || (preFight != null && preFight.IsTimeOut()) || _endFightRule != null))
		{
			// A round never ends on a predicted input; rollback re-runs this tick once confirmed.
			if (IsLocalVersus && Eclipse.Multiplayer.VersusTickDriver.Barrier())
			{
				return;
			}
			if (isSlowMotion)
			{
				SetSlowMotion(false);
			}
			ActionModels(false);
			round.processing = false;
			_eclipseRoundOutcomes.Cancel();
			if (preFight != null && preFight.IsTimeOut())
			{
				_endRoundType = EndRoundType.EndRoundTypeTimeOut;
				UpdateFightData(FightEvent.TimeoutEvent);
				_rulesInspector.CheckEvent(FightEvent.TimeoutEvent, RuleAppliance.ApplianceAll, fightData);
			}
			EndRound(GetWinner(true), GetWinner(false), _endRoundType);
		}
        else if (round.processing && stageType == StageType.Stage.STAGE_FIGHT &&
            GetCurrentFight() == this && !_eclipseFightEndDispatched && !IsPaused() &&
            _eclipseRoundOutcomes.Round == round.round && _eclipseRoundOutcomes.PendingPlayerWins.HasValue &&
            new EclipseFighterOperations(this, GetPlayerModel()).Health > 0 &&
            new EclipseFighterOperations(this, GetEnemyModel()).Health > 0 &&
            _eclipseRoundOutcomes.TryConsume(out var playerWins))
        {
            // Queue requests during callbacks; settle only at this simulation boundary.
            // Native lethal/timeout/rule branches above always win the same-frame race.
            ActionModels(false);
            round.processing = false;
            _endRoundType = EndRoundType.EndRoundTypeWin;
            var winner = playerWins ? GetPlayerModel().Parameters : GetEnemyModel().Parameters;
            var loser = playerWins ? GetEnemyModel().Parameters : GetPlayerModel().Parameters;
            EndRound(winner, loser, _endRoundType);
        }
	}

	private void EndRound(ModelParameters ABKBEJBICOA, ModelParameters LEBLJJCFKOP, EndRoundType LFLGCDNKNJI)
	{
		_rulesInspector.RulesActive = false;
		_rulesInspector.StopRules();
		if (IsLocalVersus && Eclipse.Multiplayer.LocalVersusRoundRules.ResolveWinner(
			playerParameters.GetLifeRatio(), enemyParameters.GetLifeRatio()) < 0)
		{
			Controller.StopController();
			_Camera.SetFightVisible(false);
			ResetModels(false);
			isHealthRestored = false;
			ResetParameters();
			NextRound();
			UnityEngine.Debug.Log("[Local Versus] Draw. Scores unchanged.");
			return;
		}
		if (!IsLocalVersus && (LFLGCDNKNJI == EndRoundType.EndRoundTypeTimeOut || LFLGCDNKNJI == EndRoundType.EndRoundTypeRingOut || LFLGCDNKNJI == EndRoundType.EndRoundTypeLose))
		{
			bool flag = false;
			if (_endFightRule != null)
			{
				switch (_endFightRule.GetWinnerAppliance())
				{
				case RuleAppliance.AppliancePlayer:
					flag = true;
					break;
				case RuleAppliance.ApplianceOpponent:
					flag = false;
					break;
				}
			}
			if (flag)
			{
				ABKBEJBICOA = playerParameters;
				LEBLJJCFKOP = enemyParameters;
			}
			else
			{
				hasNotLostRound = false;
				LEBLJJCFKOP = playerParameters;
				ABKBEJBICOA = enemyParameters;
			}
		}
		ABKBEJBICOA.RoundsWon++;
		ABKBEJBICOA.IsWinner = true;
		ABKBEJBICOA.RoundEnded = true;
		ABKBEJBICOA.EndRoundType = LFLGCDNKNJI;
		LEBLJJCFKOP.IsWinner = false;
		LEBLJJCFKOP.RoundEnded = true;
		LEBLJJCFKOP.EndRoundType = LFLGCDNKNJI;
		endStanceCounter = 0;
		if (preFight != null)
		{
			preFight.ViewerUpdateVictorys();
		}
		isSlowModeKeyToggled = false;
		SetSlowMotion(false);
		FinishStance(ABKBEJBICOA, LEBLJJCFKOP, LFLGCDNKNJI);
	}

	private void FinishRound()
	{
        CancelEclipseProjectiles();
        CancelEclipseActors("round_ended");
		if (_eclipseFightBeginDispatched && _eclipseEndedRound != round.round)
		{
			_eclipseEndedRound = round.round;
			DispatchEclipseCombatEvent(ModEffectEvent.RoundEnd);
            DispatchEclipseOpponent(ModEffectEvent.RoundEnd);
		}
		isStopFight = true;
	}

	private void GameOver(ModelParameters ABKBEJBICOA, ModelParameters LEBLJJCFKOP)
	{
		LockLifeUpdate(true);
		if (FightDefinition.get_Type() != BattleType.FightRaid)
		{
			ResetParameters();
		}
		isGameOver = true;
		FightDefinition.RewardIndex = currentEnemyIndex;
		CheckCountersStopFight(ABKBEJBICOA, LEBLJJCFKOP);
		if (pendingAchievements.Count > 0 || isShowingAchievement)
		{
			isAchievementBlocking = true;
		}
	}

	private ModelParameters GetWinner(bool PLGGPKEJPPJ)
	{
        if (!IsLocalVersus && _eclipseRoundOutcomes.Round == round.round && _eclipseRoundOutcomes.ResolvedPlayerWins.HasValue)
            return _eclipseRoundOutcomes.ResolvedPlayerWins.Value == PLGGPKEJPPJ ? GetPlayerModel().Parameters : GetEnemyModel().Parameters;
		if (IsLocalVersus)
		{
			int winner = Eclipse.Multiplayer.LocalVersusRoundRules.ResolveWinner(
				playerParameters.GetLifeRatio(), enemyParameters.GetLifeRatio());
			return (winner == 0) == PLGGPKEJPPJ ? playerParameters : enemyParameters;
		}
		// Offline raids are won by exhausting the boss pool, never by having a
		// higher remaining health percentage when the long timer expires.
		if (Eclipse.Modding.ModModeRuntime.IsRaid(FightDefinition))
		{
			bool bossDefeated = (ObscuredFloat)enemyParameters.GetCurrentLife() <= 0f;
			return bossDefeated == PLGGPKEJPPJ ? playerParameters : enemyParameters;
		}
		if (_endRoundType != EndRoundType.EndRoundTypeZeroHealth && _endFightRule != null)
		{
			switch (_endFightRule.GetWinnerAppliance())
			{
			case RuleAppliance.AppliancePlayer:
				return playerParameters;
			case RuleAppliance.ApplianceOpponent:
				return enemyParameters;
			}
		}
		if ((ObscuredFloat)(playerParameters.GetCurrentLife()) <= (ObscuredFloat)(enemyParameters.GetCurrentLife()))
		{
			return (!PLGGPKEJPPJ) ? playerParameters : enemyParameters;
		}
		return (!PLGGPKEJPPJ) ? enemyParameters : playerParameters;
	}

	private void ResetParameters()
	{
		foreach (Model item in ActiveModels)
		{
			item.SetDisarmed(false);
			item.set_IsShock(false);
			ModelParameters kMMJCHDKBDO = item.Parameters;
			if (!isHealthRestored)
			{
				kMMJCHDKBDO.AddLife(FightDefinition.HealthRecovery);
			}
			kMMJCHDKBDO.IsWinner = false;
			kMMJCHDKBDO.IsDead = false;
			kMMJCHDKBDO.RewardsEnabled = true;
			kMMJCHDKBDO.RoundEnded = false;
			kMMJCHDKBDO.MovesInitialized = false;
			kMMJCHDKBDO.IsUntouched = true;
			kMMJCHDKBDO.UnusedRoundFlag = false;
			kMMJCHDKBDO.EndRoundType = EndRoundType.EndRoundTypeNone;
			kMMJCHDKBDO.CalculateAttributes();
		}
		if (!isHealthRestored)
		{
			isHealthRestored = true;
		}
		ScreenModel screenModel = null;
		ScreenModel screenModel2 = null;
		if (preFight != null && preFight.get_ViewerFight() != null)
		{
			screenModel = preFight.get_ViewerFight().get_LeftModel();
			screenModel2 = preFight.get_ViewerFight().get_RightModel();
		}
		if (screenModel != null)
		{
			screenModel.DestroyAllActivePerks();
		}
		if (screenModel2 != null)
		{
			screenModel2.DestroyAllActivePerks();
		}
	}

	private void CheckLethalSlowMotion(Model.EventModel EGHPHELLOGO)
	{
		if (IsTitleSparring) return;
        if (_eclipseActors.ContainsKey(EGHPHELLOGO.KJDFJPBIGJC.GetRootModel()) ||
            EGHPHELLOGO.Opponent != null && _eclipseActors.ContainsKey(EGHPHELLOGO.Opponent.GetRootModel())) return;
		if (!round.processing || FightDefinition.get_Type() == BattleType.FightNone || isSlowMotion || isSlowMotionRequested || !EGHPHELLOGO.KJDFJPBIGJC.IsSlowMotionAllowed())
		{
			return;
		}
		IntervalAnimation mNOIEOBBCMI = (IntervalAnimation)EGHPHELLOGO.Data;
		if (mNOIEOBBCMI.Type != IntervalAnimation.IntervalType.INTERVAL_ATTACK)
		{
			return;
		}
		if (EGHPHELLOGO.Opponent == null)
		{
			GameLog.Error("Enemy for slowmode not found");
			return;
		}
		float num = EGHPHELLOGO.Opponent.GetTotalDamage((IntervalAttack)mNOIEOBBCMI, false, false, null);
		float num2 = EGHPHELLOGO.Opponent.Parameters.RemainingHealthInDamageUnits;
		if (num2 <= num)
		{
			EGHPHELLOGO.KJDFJPBIGJC.SetSlowMotionAllowed(false);
			isSlowMotionRequested = true;
		}
	}

	private void StopSlowMotionAfterAttack(Model.EventModel EGHPHELLOGO)
	{
		IntervalAnimation mNOIEOBBCMI = (IntervalAnimation)EGHPHELLOGO.Data;
		bool flag = mNOIEOBBCMI.Type == IntervalAnimation.IntervalType.INTERVAL_ATTACK;
		flag = flag;
		if (isSlowMotion && flag)
		{
			SetSlowMotion(false);
		}
	}

	public void SetSlowMotion(bool value)
	{
		if (isSlowMotion != value)
		{
			isSlowMotion = value || isSlowModeKeyToggled;
			GameUtils.SetSlowMode((!isSlowMotion) ? 1 : GameUtils.SlowModeSpeed);
			if (isSlowMotion)
			{
				isSlowMotionRequested = false;
				ChangeModelsSpeed(!isSlowMotion);
			}
			else
			{
				ChangeModelsSpeed(isSlowMotion);
			}
		}
	}

	private void ApplyPendingSlowMotion()
	{
		if (isSlowMotionRequested)
		{
			SetSlowMotion(true);
		}
	}

	private FightStatistics.FightStyle GetMaxStyle(int index)
	{
		if (preFight != null && preFight.get_ViewerFight() != null)
		{
			return preFight.get_ViewerFight().GetScreenModel(index).MaxStyle;
		}
		return FightStatistics.FightStyle.STYLE_AGGRESSIVE;
	}

	private void OnCameraTransitionStart(object data)
	{
		if (preFight != null)
		{
			preFight.OnFightPause(true);
		}
		isRenderFight = false;
	}

	private void OnCameraTransitionEnd(object data)
	{
		if (preFight != null)
		{
			preFight.OnFightPause(false);
		}
		isRenderFight = true;
	}

	private void ClearModelsStanceFlag()
	{
		foreach (Model item in ActiveModels)
		{
			item.EndStageReached = false;
		}
	}

	private void PrepareModelsCollisions()
	{
		foreach (Model item in ActiveModels)
		{
			item.RefreshEdgeGeometry();
		}
	}

	private void CreateRulesInspector()
	{
		_rulesInspector = new RulesInspector(this, FightDefinition);
		_rulesInspector.CurrentRound = round.round;
	}

	private void PrepareRound()
	{
		_enemyModel.SetTactic(enemyTactic);
		if (round.round > 1)
		{
			InitRules();
		}
		ResetFightData();
		_rulesInspector.ApplyNoAnimationRules(playerParameters);
		RefreshPlayerEquipment();
		RefreshEnemyEquipment();
		enemyParameters.CalculateAttributes();
		ApplyRules();
		UpdateControlButtons();
		if (Controller != null)
		{
			Controller.ClearButtonsAppearance();
			_rulesInspector.CheckButtonRules(Controller);
		}
		ReloadPerks();
	}

	private void RecreatePlayerModel()
	{
		SaveMagicBuffer(_playerModel);
		modelsToRemove.AddIfNotExist(_playerModel);
		ProcessRemovedModels();
		_playerModel = AddModel(playerParameters);
		RestoreMagicBuffer(_playerModel);
		PreloadModelWeapons(_playerModel);
		int num = 0;
		foreach (Model item in ActiveModels)
		{
			item.Index = num;
			num++;
		}
		pendingModels.Clear();
		ProcessRemovedModels();
		ResetModels(false);
		ResetParameters();
		_isRoundOver = true;
	}

	private void RefreshPlayerEquipment()
	{
		EquippedItemsStruct pFMMOILIHMP = new EquippedItemsStruct();
		EquippedItemsStruct pFMMOILIHMP2 = new EquippedItemsStruct();
		playerParameters.CopyEquippedItemsTo(pFMMOILIHMP);
		playerParameters.SetEquippedItemsFrom(playerEquippedItems);
		playerParameters.CalculateAttributes();
		ApplyItemRules(playerParameters);
		playerParameters.CopyEquippedItemsTo(pFMMOILIHMP2);
		if (!pFMMOILIHMP2.Compare(pFMMOILIHMP))
		{
			RecreatePlayerModel();
			playerParameters.FinalAttributes = itemRuleParameters.FinalAttributes;
		}
	}

	private void RecreateEnemyModel()
	{
		SaveMagicBuffer(_enemyModel);
		modelsToRemove.AddIfNotExist(_enemyModel);
		ProcessRemovedModels();
		_enemyModel = AddModel(enemyParameters);
		RestoreMagicBuffer(_enemyModel);
		PreloadModelWeapons(_enemyModel);
		int num = 0;
		foreach (Model item in ActiveModels)
		{
			item.Index = num;
			num++;
		}
		pendingModels.Clear();
		ProcessRemovedModels();
		ResetModels(false);
		ResetParameters();
		_isRoundOver = true;
	}

	private void RefreshEnemyEquipment()
	{
		EquippedItemsStruct pFMMOILIHMP = new EquippedItemsStruct();
		EquippedItemsStruct pFMMOILIHMP2 = new EquippedItemsStruct();
		enemyParameters.CopyEquippedItemsTo(pFMMOILIHMP);
		enemyParameters.SetEquippedItemsFrom(enemyEquippedItems);
		enemyParameters.CalculateAttributes();
		ApplyItemRules(enemyParameters);
		enemyParameters.CopyEquippedItemsTo(pFMMOILIHMP2);
		if (!pFMMOILIHMP2.Compare(pFMMOILIHMP))
		{
			RecreateEnemyModel();
			enemyParameters.FinalAttributes = itemRuleParameters.FinalAttributes;
		}
	}

	private void CheckFightRules(FightEvent KOJNCHKPLLN, RuleAppliance EJPOJJKKICO)
	{
		if (IsTitleSparring) return;
		UpdateFightData(KOJNCHKPLLN);
		if (_rulesInspector != null)
		{
			_rulesInspector.CheckEvent(KOJNCHKPLLN, EJPOJJKKICO, fightData);
		}
	}

	private void RechargeAllMagic()
	{
		foreach (Model item in ActiveModels)
		{
			item.InitializeMagicCharge();
		}
	}

	private void UpdateControlButtons(bool APDPBLADDCN = true)
	{
		if (!(Controller == null))
		{
			if (!APDPBLADDCN || round.round == 1)
			{
				ResetMagicButton();
				ResetRangedButton();
				ResetRaidChargeButton();
			}
			else
			{
				UpdateMagicButtonVisibility();
				UpdateRangedButtonVisibility();
				UpdateRaidChargeButtonVisibility();
			}
		}
	}

	private void SetModelOnListening(Model ACENLMONNPA)
	{
		bool flag = ACENLMONNPA.IsWeapon();
		ACENLMONNPA.SetWalls(GameUtils.GetLeftWall(), GameUtils.GetRightWall(), (!flag) ? 100 : 0, (!flag) ? 30 : 0);
		ACENLMONNPA.AddEventListener(2, OnAnimationStart);
		ACENLMONNPA.AddEventListener(3, OnAnimationEnd);
		ACENLMONNPA.AddEventListener(0, OnIntervalStart);
		ACENLMONNPA.AddEventListener(1, OnIntervalEnd);
		ACENLMONNPA.AddEventListener(4, OnEveryFrame);
		ACENLMONNPA.AddEventListener(5, RequestModelRemoval);
		ACENLMONNPA.AddEventListener(6, OnModelSpawned);
		ACENLMONNPA.AddEventListener(12, OnActionButtonPercentage);
		ACENLMONNPA.AddEventListener(18, OnActionButtonBulletsCount);
		ACENLMONNPA.AddEventListener(13, OnUserComboIncrease);
		ACENLMONNPA.AddEventListener(15, OnShakeScreen);
		ACENLMONNPA.AddEventListener(16, OnModelPerkEvent);
		ACENLMONNPA.AddEventListener(17, OnZoomEffect);
	}

    internal bool TryQueueCharacterForm(Model expected, DefinitionId character, Action<Exception> complete, out string error)
    {
        if (IsEclipseActorModel(expected)) return TryQueueEclipseActorForm(expected, character, complete, out error);
        error = string.Empty;
        if (expected == null || complete == null || !round.processing || _modelTransitionsClosed ||
            _eclipseFightEndDispatched || (expected != _playerModel && expected != _enemyModel) ||
            expected.GetLife() <= 0 || _modelTransitions.ContainsKey(expected))
        { error = "Fighter is not available for a form change."; return false; }
        PreparedFormModel prepared = null;
        try
        {
            var parameters = ModRuntime.BuildFormParameters(character, expected == _playerModel);
            GameUtils.InitializeFormParameters(parameters, expected.Parameters);
            var itemRules = expected == _playerModel ? _rulesInspector.GetPlayerItemRules() : _rulesInspector.GetEnemyItemRules();
            _rulesInspector.PrepareItemRules(itemRules);
            parameters.SetItemsFromRules(itemRules, false, Math.Max(1, round.round));
            parameters.SetItemsFromRules(itemRules, true, Math.Max(1, round.round));
            parameters.BuildModelDocuments();
            parameters.CalculateAttributes();
            prepared = new PreparedFormModel(parameters);
            if (QueuePreparedFighterForm(expected, prepared, complete)) return true;
            error = "Fighter became unavailable while preparing the form.";
        }
        catch (Exception exception) { error = exception.Message; }
        prepared?.Dispose();
        return false;
    }

    // Acceptance transfers preparation ownership to the queued request. Rejection
    // leaves it with the caller. Native construction and content validation must
    // finish before entering this boundary; no Lua callbacks run during a swap.
    internal bool QueuePreparedFighterForm(Model expected, PreparedFormModel prepared, Action<Exception> complete)
    {
        var replacement = prepared == null ? null : prepared.Model;
        if (expected == null || replacement == null || complete == null || expected == replacement ||
            expected.Parameters.IsPlayer != replacement.Parameters.IsPlayer) return false;
        return QueueModelTransition(expected, () =>
        {
            var original = expected.Parameters;
            var parameters = replacement.Parameters;
            if (original.MaxLife <= 0 || parameters.MaxLife <= 0)
                throw new InvalidOperationException("Form health pools must be positive.");
            parameters.SetCurrentLife(expected.GetLife() / original.MaxLife * parameters.MaxLife);
            parameters.RoundsWon = original.RoundsWon;
            parameters.IsWinner = original.IsWinner;
            replacement.SetModelPosition(new Vector3f(expected.GetPosition()));
            replacement.Sign = expected.GetFacingSign();
            var enemies = new HashSet<Model>();
            foreach (var enemy in expected._Enemies)
                if (enemy != null && enemy.GetRootModel() != expected &&
                    enemy.GetRootModel() == enemy && enemies.Add(enemy))
                    replacement.AddEnemy(enemy);
            using (var bindings = new FormRenderBindings(this, expected, replacement))
            {
                _SelectAnimation.PrepareFormAnimation(replacement);
                CommitPreparedForm(expected, prepared, bindings);
            }
        }, failure =>
        {
            try { prepared.Dispose(); }
            finally { complete(failure); }
        });
    }

    internal void CommitPreparedForm(Model expected, PreparedFormModel prepared, FormRenderBindings bindings)
    {
        var replacement = prepared == null ? null : prepared.Model;
        if (expected == null || replacement == null || bindings == null ||
            !bindings.Owns(this, expected, replacement) ||
            !IsEclipseFormParticipant(replacement) ||
            expected == _playerModel || expected == _enemyModel || _retiredFormBodies.Contains(expected))
            throw new InvalidOperationException("Prepared form does not own the active replacement.");

        var retired = new HashSet<Model> { expected };
        foreach (var model in ActiveModels)
            if (model != null && model.GetRootModel() == expected) retired.Add(model);
        foreach (var model in pendingModels)
            if (model != null && model.GetRootModel() == expected) retired.Add(model);
        foreach (var model in modelsToRemove)
            if (model != null && model.GetRootModel() == expected) retired.Add(model);
        var bodies = new List<Model> { expected };
        foreach (var body in retired) if (body != expected) bodies.Add(body);
        for (int index = 0; index < bodies.Count; index++)
            foreach (var child in bodies[index].GetWeaponModels())
                if (child != null && retired.Add(child)) bodies.Add(child);
        perksStage.RequireFormReferencesTransferred(retired);

        // Invisibility is represented by the body's active state. Preserve it
        // rather than unconditionally showing every newly committed form.
        bool visible = expected.GetGameObject().activeSelf;
        bool preparedVisible = replacement.GetGameObject().activeSelf;
        try
        {
            replacement.GetGameObject().SetActive(visible);
            expected.GetGameObject().SetActive(false);
        }
        catch
        {
            replacement.GetGameObject().SetActive(preparedVisible);
            expected.GetGameObject().SetActive(visible);
            throw;
        }
        bindings.Commit();
        prepared.Take();
        _retiredFormBodies.Add(expected);
        if (IsEclipseActorModel(replacement))
            try { CancelEclipseActorProjectiles(expected); }
            catch (Exception exception) { UnityEngine.Debug.LogException(exception); }

        // Ownership has committed. Cleanup failures must not report the swap as
        // rejected or let disposing the preparation destroy the active fighter.
        pendingModels.RemoveAll(retired.Contains);
        modelsToRemove.RemoveAll(retired.Contains);
        ActiveModels.RemoveAll(retired.Contains);
        for (int index = bodies.Count - 1; index >= 0; index--)
        {
            var body = bodies[index];
            try
            {
                if (body != expected) RemoveModel(body);
                else
                {
                    // Camera/selector/perk registrations already belong to the
                    // replacement at this index. Only dispose the retired body.
                    body.DetachCurrentEffects();
                    body.DestroyModel();
                }
            }
            catch (Exception exception) { UnityEngine.Debug.LogException(exception); }
        }
    }

    private void StopModelListening(Model model)
    {
        model.RemoveEventListener(2, OnAnimationStart);
        model.RemoveEventListener(3, OnAnimationEnd);
        model.RemoveEventListener(0, OnIntervalStart);
        model.RemoveEventListener(1, OnIntervalEnd);
        model.RemoveEventListener(4, OnEveryFrame);
        model.RemoveEventListener(5, RequestModelRemoval);
        model.RemoveEventListener(6, OnModelSpawned);
        model.RemoveEventListener(12, OnActionButtonPercentage);
        model.RemoveEventListener(18, OnActionButtonBulletsCount);
        model.RemoveEventListener(13, OnUserComboIncrease);
        model.RemoveEventListener(15, OnShakeScreen);
        model.RemoveEventListener(16, OnModelPerkEvent);
        model.RemoveEventListener(17, OnZoomEffect);
    }

    internal Action BindFormPresentation(Model expected, Model replacement, bool player, bool actor = false)
    {
        var viewer = preFight == null ? null : preFight.get_ViewerFight();
        var panel = actor || viewer == null ? null : (player ? viewer.get_LeftModel() : viewer.get_RightModel());
        bool attached = false, detached = false, refreshed = false;
        Action restore = () =>
        {
            // The HUD can throw after assigning its parameters. Always attempt
            // its reverse refresh, and restore event ownership even if it fails.
            try
            {
                if (refreshed && panel != null)
                    panel.RefreshForm(replacement.Parameters, expected.Parameters);
            }
            finally
            {
                if (attached) { StopModelListening(replacement); attached = false; }
                if (detached) { SetModelOnListening(expected); detached = false; }
                refreshed = false;
            }
        };
        try
        {
            attached = true;
            SetModelOnListening(replacement);
            StopModelListening(expected); detached = true;
            if (panel != null)
            {
                refreshed = true;
                if (!panel.RefreshForm(expected.Parameters, replacement.Parameters))
                    throw new InvalidOperationException("Fight HUD no longer belongs to the original fighter.");
            }
        }
        catch (Exception original)
        {
            try { restore(); }
            catch (Exception rollback) { throw new AggregateException("Form presentation and restoration failed.", original, rollback); }
            throw;
        }
        return restore;
    }

	private void UpdateFightData(FightEvent KOJNCHKPLLN = FightEvent.NoneEvent)
	{
		fightData.PlayerData.FightEventType = KOJNCHKPLLN;
		fightData.EnemyData.FightEventType = KOJNCHKPLLN;
		fightData.PlayerData.CurrentAnimation = _playerModel.GetCurrentAnimation();
		fightData.EnemyData.CurrentAnimation = _enemyModel.GetCurrentAnimation();
		fightData.PlayerData.Style = (FightStatistics.FightStyle)_playerModel.StyleRank;
		fightData.EnemyData.Style = (FightStatistics.FightStyle)_enemyModel.StyleRank;
		fightData.PlayerData.IsUsingItem = _playerModel.HasDisarmedItem();
		fightData.PlayerData.IsOpponentUsingItem = _enemyModel.HasDisarmedItem();
		fightData.EnemyData.IsUsingItem = _enemyModel.HasDisarmedItem();
		fightData.EnemyData.IsOpponentUsingItem = _playerModel.HasDisarmedItem();
		fightData.PlayerData.IsShocked = _playerModel.IsInShock();
		fightData.PlayerData.IsOpponentShocked = _enemyModel.IsInShock();
		fightData.EnemyData.IsShocked = _enemyModel.IsInShock();
		fightData.EnemyData.IsOpponentShocked = _playerModel.IsInShock();
		fightData.SlowMode = GameUtils.GetSlowMode();
	}

	private void UpdateFightDataDamage(Model.StrikeResult PPIAOBPLGOK, RuleAppliance EJPOJJKKICO)
	{
		FightData hCPJJKMNMCE = null;
		FightData hCPJJKMNMCE2 = null;
		switch (EJPOJJKKICO)
		{
		case RuleAppliance.AppliancePlayer:
			hCPJJKMNMCE = fightData.PlayerData;
			hCPJJKMNMCE2 = fightData.EnemyData;
			break;
		case RuleAppliance.ApplianceOpponent:
			hCPJJKMNMCE = fightData.EnemyData;
			hCPJJKMNMCE2 = fightData.PlayerData;
			break;
		default:
			GameLog.Error("Fight::updateFightDataDamage ERROR - wrong RuleAppliance %i", EJPOJJKKICO);
			return;
		}
		hCPJJKMNMCE.DamageDealt = PPIAOBPLGOK.FinalDamage;
		hCPJJKMNMCE.DamageReceived = 0f;
		hCPJJKMNMCE.IsAttacker = true;
		hCPJJKMNMCE.IsBlocked = PPIAOBPLGOK.IsBlocked;
		hCPJJKMNMCE.IsCritical = PPIAOBPLGOK.IsCritical;
		hCPJJKMNMCE.IsHeadHit = PPIAOBPLGOK.IsHeadHit;
		hCPJJKMNMCE2.DamageReceived = PPIAOBPLGOK.FinalDamage;
		hCPJJKMNMCE2.DamageDealt = 0f;
		hCPJJKMNMCE2.IsAttacker = false;
		hCPJJKMNMCE2.IsBlocked = PPIAOBPLGOK.IsBlocked;
	}

	private void CheckCountersStopFight(ModelParameters ABKBEJBICOA, ModelParameters LEBLJJCFKOP)
	{
		if (IsLocalVersus) return;
		BattleType pJMEMGHKKBM = FightDefinition.get_Type();
		Battle cNAOMDMIGLJ = FightDefinition.Battle;
		int num = cNAOMDMIGLJ.GetFights().Count - 1;
		int gCAABNKEIBN = FightDefinition.Index;
		bool jEDBJFMHGCH = ABKBEJBICOA.IsPlayer;
		if (jEDBJFMHGCH)
		{
			if (pJMEMGHKKBM == BattleType.FightBosses || pJMEMGHKKBM == BattleType.FightBossesReplayable || pJMEMGHKKBM == BattleType.FightFinalTitan)
			{
				bool flag = gCAABNKEIBN == num;
				if (!flag)
				{
					counters.OnBodyguardsWin();
				}
				else if (flag)
				{
					counters.OnBossWin();
				}
				if (flag && hasNotLostRound)
				{
					counters.OnBossNoLose();
				}
			}
			else if (pJMEMGHKKBM == BattleType.FightTournament && gCAABNKEIBN == num)
			{
				counters.OnTournamentBeaten();
			}
			else if (pJMEMGHKKBM == BattleType.FightChallenge && gCAABNKEIBN == num)
			{
				counters.OnChallengeBeaten();
			}
			else if (pJMEMGHKKBM == BattleType.FightAscension && gCAABNKEIBN == num)
			{
				counters.OnChallenge2Beaten();
			}
			counters.OnFightBeaten(FightDefinition.FightId);
			counters.OnWinBattle(FightDefinition.FightId);
			counters.OnDifficultyWin();
		}
		else
		{
			counters.OnLoss();
		}
		CheckMaximumLevelCounter(jEDBJFMHGCH);
		if (pJMEMGHKKBM == BattleType.FightSurvival || pJMEMGHKKBM == BattleType.FightRaid)
		{
			counters.SetSurvivalRounds((!jEDBJFMHGCH) ? currentEnemyIndex : (currentEnemyIndex + 1));
		}
		counters.Complete(round.roundTotal);
	}

	private void CheckCountersEndRound(ModelParameters ABKBEJBICOA, ModelParameters LEBLJJCFKOP)
	{
		if (IsLocalVersus) return;
		Model nPPONCJECLA = _playerModel;
		if (ABKBEJBICOA.IsPlayer)
		{
			int bAINMLLIKOL = FightDefinition.EffectiveRoundTime - preFight.get_TimeLeft();
			ComboStatistic statistic = preFight.GetStatistic(0);
			counters.SetTime(bAINMLLIKOL);
			float bAINMLLIKOL2 = (ObscuredFloat)(ABKBEJBICOA.GetCurrentLife());
			counters.SetLife(bAINMLLIKOL2);
			if (ABKBEJBICOA.RewardsEnabled)
			{
				counters.OnPerfectRound();
			}
			if (wasPlayerShocked)
			{
				counters.OnShockWin();
			}
		}
		counters.ResetRound();
	}

	private GameUtils.HitEffect FindHitEffect(bool EDKDBAJCEHI, bool IFCOPPPDOCD, bool EPKEEMFHHFM)
	{
		foreach (GameUtils.HitEffect item in GameUtils.GetHitEffects().Effects)
		{
			if (EPKEEMFHHFM && item.Type == "Shock")
			{
				return item;
			}
			if (EDKDBAJCEHI && item.Type == "CriticalHit")
			{
				return item;
			}
			if (IFCOPPPDOCD && item.Type == "HeadHit")
			{
				return item;
			}
		}
		return null;
	}

	private void OnCounterIncrement(object data)
	{
		if (!FightDefinition.TrackFightProgress)
		{
			return;
		}
		CountersFight.CurrentCounter pEMLBKDIDHA = (CountersFight.CurrentCounter)data;
		if (pEMLBKDIDHA != null)
		{
			Achievement jNPIOKEKMII = GameUtils.FindReachedAchievement(pEMLBKDIDHA.Definition, pEMLBKDIDHA.Value);
			if (jNPIOKEKMII != null)
			{
				pendingAchievements.Add(jNPIOKEKMII);
				hasPendingAchievement = true;
			}
		}
	}

    private void DispatchEclipseActivity(Model model, ModCombatActivityEvent activity)
    {
        if (!_eclipseFightBeginDispatched) return;
        if (model == _playerModel) DispatchEclipseCombatEvent(activity.Type, null, null, activity);
        else if (model == _enemyModel) DispatchEclipseOpponent(activity.Type, null, null, activity);
    }

	private void OnStyleChanged(object data)
	{
		ModelStyleChange lONCJPNBHEA = (ModelStyleChange)data;
		int kNBKAELNFDD = lONCJPNBHEA.StyleIndex;
		string bPJNHNCOPOP = lONCJPNBHEA.StyleName;
		float hFKPJPBCIEK = lONCJPNBHEA.StyleGain;
		bool oJAHEEIFMBM = lONCJPNBHEA.IsHit;
		Model fGCODGKLHED = GetModelByScreenModel(lONCJPNBHEA.Side);
		if (kNBKAELNFDD != fGCODGKLHED.StyleRank)
		{
			if (lONCJPNBHEA.Side == ScreenModel.ScreenSide.TYPE_LEFT)
			{
				counters.SetStyle(kNBKAELNFDD);
			}
			perksStage.FireEvent(fGCODGKLHED, PerkEvent.PerkEventType.EVENT_STYLE);
			fGCODGKLHED.OnStyleChanged(kNBKAELNFDD, bPJNHNCOPOP, hFKPJPBCIEK, oJAHEEIFMBM);
            if (_eclipseFightBeginDispatched && ModRuntime.Scripts != null)
                DispatchEclipseActivity(fGCODGKLHED, ModCombatActivityEvent.StyleChange(
                kNBKAELNFDD, bPJNHNCOPOP, hFKPJPBCIEK, oJAHEEIFMBM));
			RuleAppliance eJPOJJKKICO = ((lONCJPNBHEA.Side == ScreenModel.ScreenSide.TYPE_LEFT) ? RuleAppliance.AppliancePlayer : RuleAppliance.ApplianceOpponent);
			CheckFightRules(FightEvent.CrazyEvent, eJPOJJKKICO);
		}
	}

	private void OnModExpired(PerksStage.PerkEventStruct data)
	{
		data.Model.EventData.Data = data.Data;
		_SelectAnimation.CheckEvent(EventAnimation.EventAnimationType.EVENT_MOD_EXPIRES, data.Model.EventData);
	}

	private int GetModelIndexByScreenModel(ScreenModel.ScreenSide LFLGCDNKNJI)
	{
		int result = 0;
		switch (LFLGCDNKNJI)
		{
		case ScreenModel.ScreenSide.TYPE_LEFT:
			result = 0;
			break;
		case ScreenModel.ScreenSide.TYPE_RIGHT:
			result = 1;
			break;
		default:
			GameLog.Error("Fight::getModelIndexByScreenModel - Unknown model: %i", LFLGCDNKNJI);
			break;
		}
		return result;
	}

	private Model GetModelByScreenModel(ScreenModel.ScreenSide LFLGCDNKNJI)
	{
		Model result = null;
		switch (LFLGCDNKNJI)
		{
		case ScreenModel.ScreenSide.TYPE_LEFT:
			result = _playerModel;
			break;
		case ScreenModel.ScreenSide.TYPE_RIGHT:
			result = _enemyModel;
			break;
		default:
			GameLog.Error("Fight::getModelByScreenModel - Unknown model: %i", LFLGCDNKNJI);
			break;
		}
		return result;
	}

	private void OnUserComboIncrease(object data)
	{
		Model fGCODGKLHED = (Model)data;
		if (fGCODGKLHED != _playerModel && fGCODGKLHED != _enemyModel)
		{
			GameLog.Error("Fight::onUserComboIncrease ERROR - Model is not player nor enemy");
			return;
		}
		int num = fGCODGKLHED.GetComboCount();
		FightData hCPJJKMNMCE = ((!fGCODGKLHED.IsPlayerModel()) ? fightData.EnemyData : fightData.PlayerData);
		hCPJJKMNMCE.currentComboLevel = num;
		CheckFightRules(FightEvent.ComboEvent, fGCODGKLHED.IsPlayerModel() ? RuleAppliance.AppliancePlayer : RuleAppliance.ApplianceOpponent);
		if (fGCODGKLHED.IsPlayerModel())
		{
			counters.SetComboCount(fGCODGKLHED.GetLastComboCount());
		}
		perksStage.FireEvent(fGCODGKLHED, PerkEvent.PerkEventType.EVENT_COMBO);
        if (_eclipseFightBeginDispatched && ModRuntime.Scripts != null)
            DispatchEclipseActivity(fGCODGKLHED, ModCombatActivityEvent.ComboChange(num, fGCODGKLHED.GetLastComboCount()));
		int hIGBAPPOOKJ = fGCODGKLHED.LastComboTime;
		if (preFight != null)
		{
			preFight.get_ViewerFight().UpdateCombo(fGCODGKLHED.IsPlayerModel(), num, hIGBAPPOOKJ);
		}
	}

	private void ShowNextAchievement()
	{
		if (!isShowingAchievement)
		{
			isShowingAchievement = true;
			SortAchievements();
			Achievement jNPIOKEKMII = pendingAchievements[0];
			pendingAchievements.Remove(jNPIOKEKMII);
			if (preFight != null)
			{
				preFight.ShowAchievementMessage(jNPIOKEKMII);
			}
			else
			{
				OnAchievementMessageHidden();
			}
		}
	}

	private void OnAchievementMessageHidden()
	{
		isShowingAchievement = false;
		if (pendingAchievements.Count == 0)
		{
			isAchievementBlocking = false;
		}
		else
		{
			ShowNextAchievement();
		}
	}

	private void OnShakeScreen(object data)
	{
		ActionShakeScreen oEAOHCOKDPF = (ActionShakeScreen)data;
		_Camera.ApplyHitEffect(oEAOHCOKDPF.GetEffect());
	}

	private void OnZoomEffect(object data)
	{
		ActionZoomEffect pHFCNOBALGE = (ActionZoomEffect)data;
		_Camera.ApplyZoomEffect(pHFCNOBALGE.GetEffect());
	}

	private void Surrender()
	{
		AbortFight(GameOverTypes.GAME_OVER_SURRENDER);
	}

	private void AbortFight(GameOverTypes MHNEKAEGNBO)
	{
        _eclipseRoundOutcomes.Cancel();
        CancelEclipseProjectiles();
        CancelEclipseActors("round_ended");
        CancelEclipseFighterMotion();
        CancelEclipseFighterPlayback();
		if (IsLocalVersus)
		{
			Eclipse.Multiplayer.LocalVersusSession.Complete(this, true);
			return;
		}
        _eclipsePlayerResult = MHNEKAEGNBO == GameOverTypes.GAME_OVER_SURRENDER ? "surrender" : "loss";
        FinishRound();
        if (_eclipseFightBeginDispatched && !_eclipseFightEndDispatched)
        {
            _eclipseFightEndDispatched = true;
            DispatchEclipseCombatEvent(ModEffectEvent.FightEnd);
            DispatchEclipseOpponent(ModEffectEvent.FightEnd);
            _eclipseShields.Clear();
			ClearEclipseStatusIcons();
			Controller?.ClearScriptControlBlocks();
        }
		Sound.StopLoopedSounds();
		counters.Complete(round.roundTotal, true);
		counters.SaveCompleteValues(true);
		ComboStatistic aIOMDIAFHGB = null;
		ComboStatistic mOJHPBGGNAH = null;
		int num = 0;
		if (preFight != null)
		{
			aIOMDIAFHGB = preFight.GetStatistic(0);
			mOJHPBGGNAH = preFight.GetStatistic(1);
			num = preFight.get_TimeLeft();
		}
		if (FightDefinition.get_Type() != BattleType.FightRaid || Eclipse.Modding.ModModeRuntime.IsRaid(FightDefinition))
		{
			GameUtils.EndFight(aIOMDIAFHGB, FightDefinition, null, null, MHNEKAEGNBO, mOJHPBGGNAH, averageFps);
		}
		if (FightDefinition.TrackFightProgress)
		{
			ListSF.GetRoster().GetAchievements().ApplyPendingCounters();
		}
	}

	private void OpenPauseScreen()
	{
		if (IsLocalVersus)
		{
			Eclipse.Multiplayer.LocalVersusSession.Pause("Paused");
			return;
		}
		if (preFight != null)
		{
			SetPaused(true);
			preFight.OpenPauseScreen();
		}
	}

	private void ClosePauseScreen()
	{
		if (preFight != null)
		{
			SetPaused(false);
			preFight.ClosePauseScreen();
		}
	}

	public void ShowEndFightScreen(FightResult DCJLKCFKCOM)
	{
		if (preFight != null)
		{
			SetPaused(true);
			preFight.OpenEndFightScreen(DCJLKCFKCOM);
		}
	}

	private void RegisterSpawnedModel(Model ACENLMONNPA)
	{
		if (ACENLMONNPA != null)
		{
			Eclipse.Multiplayer.Rollback.RollbackObjects.Created(ACENLMONNPA.GetGameObject());
			ACENLMONNPA.Index = _Camera.AddModel(ACENLMONNPA, false, false);
			pendingModels.Add(ACENLMONNPA);
			SetModelOnListening(ACENLMONNPA);
			perksStage.AddModel(ACENLMONNPA);
			ACENLMONNPA.SetCameraAttached(false);
		}
	}

	private Model SpawnWeaponModel(Model MDKDAHCNCMC, List<CopyItemInfo> HELFDCAIJNE = null, string JLHDJLHLGND = "")
	{
		if (HELFDCAIJNE == null)
		{
			HELFDCAIJNE = new List<CopyItemInfo>();
		}
		return MDKDAHCNCMC.SpawnWeaponModel(HELFDCAIJNE, JLHDJLHLGND);
	}

	private void OnModelSpawned(object data)
	{
		Model aCENLMONNPA = (Model)data;
		RegisterSpawnedModel(aCENLMONNPA);
	}

	private void ProcessRemovedModels()
	{
		while (modelsToRemove.Count != 0)
		{
            var item = modelsToRemove[0]; modelsToRemove.RemoveAt(0);
			RemoveActiveModel(item);
		}
	}

	private void RemoveModel(Model ACENLMONNPA)
	{
        if (ACENLMONNPA != null)
        {
            pendingModels.Remove(ACENLMONNPA);
            ForgetEclipseProjectile(ACENLMONNPA);
            ForgetEclipseActor(ACENLMONNPA);
        }
		if (ACENLMONNPA == null)
		{
			GameLog.Error("Fight::removeModel - cant find model");
			return;
		}
		Model fGCODGKLHED = ACENLMONNPA.GetParentModel();
		if (fGCODGKLHED != null)
		{
			fGCODGKLHED.RemoveWeaponModel((WeaponModel)ACENLMONNPA);
		}
		_Camera.RemoveObject(ACENLMONNPA);
		foreach (Model item in ActiveModels)
		{
			item.RemoveEnemy(ACENLMONNPA);
			item.SetNearestEnemy();
		}
		perksStage.RemoveModel(ACENLMONNPA);
		_SelectAnimation.RemoveModel(ACENLMONNPA);
		ACENLMONNPA.DetachCurrentEffects();
		ACENLMONNPA.DestroyModel();
		ACENLMONNPA.RemoveAllEventListener();
	}

	private void ApplyItemRules(ModelParameters JCICKLIMBEF)
	{
		itemRuleParameters = JCICKLIMBEF;
		int num = round.round;
		if (num < 1)
		{
			num = 1;
		}
		List<ItemRule> list = ((!JCICKLIMBEF.IsPlayer) ? _rulesInspector.GetEnemyItemRules() : _rulesInspector.GetPlayerItemRules());
		_rulesInspector.PrepareItemRules(list);
		if (_rulesInspector != null)
		{
			JCICKLIMBEF.SetItemsFromRules(list, false, num);
			itemRuleParameters.SetItemsFromRules(list, true, num);
		}
		JCICKLIMBEF.BuildModelDocuments();
		itemRuleParameters.CalculateAttributes();
		JCICKLIMBEF.FinalAttributes = itemRuleParameters.FinalAttributes;
	}

	/// <summary>Syncs the banner pause with the fight before a stepping source runs; false while paused.</summary>
	internal bool PrepareVersusStep()
	{
		if ((bool)preFight)
		{
			preFight.SetPause(IsPaused());
		}
		return !IsPaused();
	}

	/// <summary>
	/// True while a versus tick may run on a predicted opponent input. The steps of a
	/// transition that a rollback cannot undo (stage changes, banners, round end, the
	/// result) each call VersusTickDriver.Barrier, which discards a predicted tick and
	/// re-runs it on confirmed input, so intros and outros predict like the rest of the
	/// round. -rollback-strict-transitions keeps whole transitions on confirmed input.
	/// </summary>
	internal bool VersusCanSpeculate => !IsPaused() && (!Eclipse.Multiplayer.VersusTickDriver.StrictTransitions ||
		stageType == StageType.Stage.STAGE_FIGHT && round.processing && !isEndRound && !isGameOver && !isStopFight && !isRoundResultPending);

	/// <summary>Advances intro/round banners by one fixed step; see VersusTickDriver.</summary>
	internal void AdvanceVersusScreens(float step)
	{
		if (preFight != null)
		{
			preFight.AdvanceScreenSimulationStep(step);
		}
	}

	/// <summary>
	/// Eclipse training: puts both fighters back on the arena's start marks (or swapped)
	/// at full health, even mid-move or mid-air.
	/// </summary>
	internal void TrainingPlaceFighters(bool swapped)
	{
		var player = GetPlayerModel();
		var enemy = GetEnemyModel();
		if (_location == null || player == null || enemy == null) return;
		player.TrainingMoveToX((swapped ? _location.enemyStartPosition : _location.playerStartPosition).GetX());
		enemy.TrainingMoveToX((swapped ? _location.playerStartPosition : _location.enemyStartPosition).GetX());
		SetLife(player, player.Parameters.MaxLife);
		SetLife(enemy, enemy.Parameters.MaxLife);
	}

	/// <summary>Eclipse training: keeps the round clock from running out.</summary>
	internal void TrainingRefillTime()
	{
		if (preFight != null && preFight.get_ViewerFight() != null) preFight.get_ViewerFight().RefillTime(round.timeTotal > 0 ? round.timeTotal : 99);
	}

	/// <summary>Delivers one tick-aligned versus control event for <paramref name="side"/> (0 left, 1 right).</summary>
	internal void ApplyVersusControl(int side, bool press, FightCID control)
	{
		var data = new FightControlEventData { Index = side, Control = control };
		if (press) ControlPress(data);
		else ControlRelease(data);
	}

	private void ControlPress(object data)
	{
		// The versus tick driver is the only input path while it owns the fight.
		if (Eclipse.Multiplayer.VersusTickDriver.Owns(this) && !Eclipse.Multiplayer.VersusTickDriver.IsApplyingInput)
		{
			return;
		}
		if (!isInputEnabled || (IsLocalVersus && IsPaused()))
		{
			return;
		}
		FightControlEventData cBBEIGACPPD = (FightControlEventData)data;
		Model fGCODGKLHED = GetModelByIndex(cBBEIGACPPD.Index);
		FightCID eCHINOPKGGI = (FightCID)GetControlId(cBBEIGACPPD);
		if (stageType == StageType.Stage.STAGE_START_STANCE)
		{
			if (fGCODGKLHED.PendingKey == -1)
			{
				fGCODGKLHED.PendingKey = (int)eCHINOPKGGI;
			}
		}
		else if (stageType == StageType.Stage.STAGE_FIGHT && eCHINOPKGGI != (FightCID)(-1))
		{
			fGCODGKLHED.PressAnyKey(eCHINOPKGGI);
		}
	}

	private void ControlRelease(object data)
	{
		if (Eclipse.Multiplayer.VersusTickDriver.Owns(this) && !Eclipse.Multiplayer.VersusTickDriver.IsApplyingInput)
		{
			return;
		}
		if (isInputEnabled)
		{
			FightControlEventData cBBEIGACPPD = (FightControlEventData)data;
			Model fGCODGKLHED = GetModelByIndex(cBBEIGACPPD.Index);
			FightCID eCHINOPKGGI = (FightCID)GetControlId(cBBEIGACPPD);
			if (fGCODGKLHED.PendingKey == (int)eCHINOPKGGI)
			{
				fGCODGKLHED.PendingKey = -1;
			}
			if (eCHINOPKGGI != (FightCID)(-1))
			{
				fGCODGKLHED.ReleaseAnyKey(eCHINOPKGGI);
			}
			ReleaseAnyKey(cBBEIGACPPD.Control);
		}
	}

	private int GetControlId(FightControlEventData DFIBLGKFAHN)
	{
		int count = ActiveModels.Count;
		if (count > 0 && DFIBLGKFAHN.Index < count)
		{
			Model fGCODGKLHED = GetModelByIndex(DFIBLGKFAHN.Index);
			if (fGCODGKLHED.IsUserControlled())
			{
				return (int)MirrorControl(DFIBLGKFAHN.Control);
			}
		}
		return -1;
	}

	private void ClearRuleVisuals()
	{
		RemoveDarkness();
		SetHealthBarVisible(RuleAppliance.ApplianceAll, true);
		RemoveRingout();
		RemoveHotGround();
		RemovePointsTable();
		RemoveCombo();
		RemovePerkActivationArea();
	}

	private void ResetMagicButton()
	{
		UpdateMagicButtonVisibility();
		Controller.GetActionButtons().ResetMagicButton();
	}

	private void SaveEquippedItems()
	{
		playerParameters.CopyEquippedItemsTo(playerEquippedItems);
		enemyParameters.CopyEquippedItemsTo(enemyEquippedItems);
	}

	private void ResetRaidChargeButton()
	{
		UpdateRaidChargeButtonVisibility();
		Controller.GetActionButtons().ResetRaidChargeButton();
	}

	private void AlignCameraOnModels(List<Model> INNLAFHKJNI)
	{
		Vector3f eMAFACPEPDK = new Vector3f();
		float num = 0f;
		foreach (Model item in INNLAFHKJNI)
		{
			ModelObject oIEODIEHJMH = item.GetBodyObject();
			float num2 = oIEODIEHJMH.GetTotalWeight();
			Vector3f eMAFACPEPDK2 = new Vector3f(oIEODIEHJMH.GetCenterOfMassPosition());
			eMAFACPEPDK2.Multiply(num2);
			eMAFACPEPDK.Add(eMAFACPEPDK2);
			num += num2;
		}
		eMAFACPEPDK.Multiply(1f / num);
		Vector3f eMAFACPEPDK3 = new Vector3f(_Camera.GetCameraTarget());
		eMAFACPEPDK3.Subtract(eMAFACPEPDK);
		eMAFACPEPDK3.SetY(0f);
		eMAFACPEPDK3.SetZ(0f);
		foreach (Model item2 in INNLAFHKJNI)
		{
			item2.ShiftModelPosition(eMAFACPEPDK3, true);
		}
	}

	private void EndFight()
	{
		if (IsLocalVersus)
		{
			Eclipse.Multiplayer.LocalVersusSession.Complete(this);
			return;
		}
        _eclipsePlayerResult = gameOverParameters.GameOverType == GameOverTypes.GAME_OVER_WIN ? "win" : gameOverParameters.GameOverType == GameOverTypes.GAME_OVER_LOSS ? "loss" : "timeout";
		if (_eclipseFightBeginDispatched && !_eclipseFightEndDispatched)
		{
			_eclipseFightEndDispatched = true;
			DispatchEclipseCombatEvent(ModEffectEvent.FightEnd);
            DispatchEclipseOpponent(ModEffectEvent.FightEnd);
            _eclipseShields.Clear();
			ClearEclipseStatusIcons();
			Controller?.ClearScriptControlBlocks();
		}
		Sound.StopLoopedSounds();
		if (hasNotLostRound)
		{
			counters.OnNoLose();
		}
		_Camera.SetEnabled(false);
		_Camera.SetFightVisible(false);
		ResetModels(true);
		_enemyModel.SetTactic(enemyTactic);
		ComboStatistic aIOMDIAFHGB = null;
		ComboStatistic mOJHPBGGNAH = null;
		if (preFight != null)
		{
			aIOMDIAFHGB = preFight.GetStatistic(0);
			mOJHPBGGNAH = preFight.GetStatistic(1);
			preFight.gameObject.SetActive(false);
		}
		float pIFMOMMPFFM = (float)fightTimeInFrame / 60f;
		GameUtils.EndFight(aIOMDIAFHGB, FightDefinition, gameOverParameters.Winner, gameOverParameters.Loser, gameOverParameters.GameOverType, mOJHPBGGNAH, averageFps, _playerModel.GetRaidChargesUsed(), pIFMOMMPFFM, (int)_playerModel.GetStatistics().GetStyle());
		counters.SaveCompleteValues(false);
		GameUtils.CommitPendingAchievements();
		if (FightDefinition.TrackFightProgress)
		{
			ListSF.GetRoster().GetAchievements().ApplyPendingCounters();
		}
		if (FightDefinition.get_Type() != BattleType.FightRaid)
		{
		}
	}

	private void EndFightRaid()
	{
		if (Eclipse.Modding.ModModeRuntime.IsRaid(FightDefinition)) EndFight();
	}

	private Model GetModelByIndex(int index)
	{
		switch (index)
		{
		case 0:
			return _playerModel;
		case 1:
			return _enemyModel;
		default:
			if (index >= 0 && index < ActiveModels.Count)
			{
				return ActiveModels[index];
			}
			return null;
		}
	}

	private void SaveMagicBuffer(Model ACENLMONNPA)
	{
		if (ACENLMONNPA == null)
		{
			GameLog.Error("Fight::fillMagicAndMissilesBuffer ERROR - model is NULL");
			return;
		}
		magicBuffer.MagicCharges = ACENLMONNPA.GetMagicCharges();
		magicBuffer.MagicChargeFraction = ACENLMONNPA.GetMagicChargeFraction();
	}

	private void RestoreMagicBuffer(Model ACENLMONNPA)
	{
		if (ACENLMONNPA == null)
		{
			GameLog.Error("Fight::setMagicAndMissilesFromBuffer ERROR - model is NULL");
			return;
		}
		ACENLMONNPA.SetMagicCharges(magicBuffer.MagicCharges);
		ACENLMONNPA.SetMagicChargeFraction(magicBuffer.MagicChargeFraction);
	}

	private void OnFightRulesChanging(FightList KGKDKENMAOA)
	{
	}

	private void OnFightUnloading(FightList KGKDKENMAOA)
	{
	}

	private void LockLifeUpdate(bool value)
	{
		if (preFight != null && preFight.get_ViewerFight() != null)
		{
			preFight.get_ViewerFight().SetLockLifeUpdate(true, value);
			preFight.get_ViewerFight().SetLockLifeUpdate(false, value);
		}
	}

	private void InitRules()
	{
		_rulesInspector.ResetRules((round.round <= 0) ? 1 : round.round);
		if (FightDefinition != null)
		{
			FightDefinition.RefreshDescriptionRule();
		}
	}

	private void ApplyRules()
	{
		_rulesInspector.CheckPreDraws();
		RuleInitData oIFPCFEGFOB = new RuleInitData(_playerModel, _enemyModel, _location, fightData);
		oIFPCFEGFOB.PlayerParameters = playerParameters;
		oIFPCFEGFOB.OpponentParameters = enemyParameters;
		_rulesInspector.InitRules(oIFPCFEGFOB);
	}

	private void ResetFightData()
	{
		fightData.PlayerData.Reset();
		fightData.EnemyData.Reset();
	}

	private void OnReservedPrivateHookB()
	{
	}

	private void OnReservedPrivateHookC()
	{
	}

	private void StopAllRules()
	{
		_rulesInspector.RulesActive = false;
		_rulesInspector.StopRules();
		ClearRuleVisuals();
	}

	private void CheckChangeFightRules()
	{
		OnFightRulesChanging(FightDefinition);
		_rulesInspector.CheckChangeFightRules(FightDefinition);
	}

	private void ApplyHeldKeys()
	{
		foreach (Model item in ActiveModels)
		{
			if (item.PendingKey != -1)
			{
				item.PressAnyKey((FightCID)item.PendingKey);
				item.PendingKey = -1;
			}
		}
	}

	private void OnRenderFinished()
	{
	}

	private void LoadAiTactics()
	{
		List<string> list = new List<string>(4);
		List<string> list2 = new List<string>();
		ItemInfo jGMLKIPCFII = _playerModel.Parameters.Weapon;
		if (jGMLKIPCFII != null)
		{
			string mDPPNGIEJGD = jGMLKIPCFII.SubType;
			list.AddIfNotExist(mDPPNGIEJGD);
			list2.AddIfNotExist(mDPPNGIEJGD);
		}
		ItemInfo jGMLKIPCFII2 = _enemyModel.Parameters.Weapon;
		if (jGMLKIPCFII2 != null)
		{
			string mDPPNGIEJGD2 = jGMLKIPCFII2.SubType;
			list.AddIfNotExist(mDPPNGIEJGD2);
			if (list2.Contains(mDPPNGIEJGD2))
			{
				list2.Remove(mDPPNGIEJGD2);
			}
			else
			{
				list2.AddIfNotExist(mDPPNGIEJGD2);
			}
		}
		GameLog.Write("Loading tactics for next subtypes:");
		foreach (string item in list)
		{
			GameLog.Write(item);
		}
		AiData.ClearTables();
		AiData.Load(list, list2);
	}

	private void RenderEndRoundEffect(float GPEGBHMKKJL)
	{
		if (isFightInitialized)
		{
			_Camera.GetRender().FadePerkActivationArea(GPEGBHMKKJL);
		}
	}

	private void UpdateMagicButtonVisibility()
	{
		if (AssemblyController.GetShowController() || AssemblyController.GetGamepadEnabled())
		{
			bool hFIIEPMEMFF = playerParameters.Magic != null && playerParameters.Magic.Name != GameUtils.GetDefaultItem("Magic");
			Controller.GetActionButtons().ShowMagic(hFIIEPMEMFF);
		}
	}

	private void UpdateRangedButtonVisibility()
	{
		if (AssemblyController.GetShowController() || AssemblyController.GetMarket().GetIsAmazonMarket())
		{
			bool gKGKKCLPGBB = playerParameters.Ranged != null && playerParameters.Ranged.Name != GameUtils.GetDefaultItem("Ranged");
			Controller.GetActionButtons().ShowRanged(gKGKKCLPGBB);
		}
	}

	private void UpdateRaidChargeButtonVisibility()
	{
		if (AssemblyController.GetShowController() || AssemblyController.GetGamepadEnabled())
		{
			RaidModelParameters kAOPLEPILDH = playerParameters as RaidModelParameters;
			bool oPPBHOOBHOE = FightDefinition.get_Type() == BattleType.FightRaid && kAOPLEPILDH != null && kAOPLEPILDH.RaidChargeItem != null && kAOPLEPILDH.RaidChargeItem.Name != GameUtils.GetDefaultItem("RaidCharge") && _playerModel.GetRaidBullets() > 0;
			Controller.GetActionButtons().ShowRaidCharge(oPPBHOOBHOE);
		}
	}

	private void ResetModelsHitData()
	{
		foreach (Model item in ActiveModels)
		{
			item.ResetHitData();
		}
	}

	private string GetAttackLogName(IntervalAttack FLGCMOKINLI)
	{
		return string.Empty;
	}

	private void OnPauseRequested(object AOMLCBHAJJH)
	{
		OpenPauseScreen();
	}

	private void OnBackPressed(object AOMLCBHAJJH)
	{
		OpenPauseScreen();
	}

	private void CheckMaximumLevelCounter(bool MFDIOECHDOA)
	{
		uint num = ListSF.GetRoster().GetExperience();
		uint num2 = ListSF.GetRoster().GetExperienceToNextLevel();
		uint num3 = GameUtils.GetFightExpReward(FightDefinition, MFDIOECHDOA);
		int num4 = ListSF.GetRoster().GetLevel();
		int count = GameUtils.LevelThresholdTable.Thresholds.Count;
		global::Pair<int, uint> cCKLNOPEKHO = GameUtils.LevelThresholdTable.Thresholds[count - 1];
		int lLHEDBIEHAA = cCKLNOPEKHO.First;
		if (num + num3 >= num2 && num4 + 1 == lLHEDBIEHAA)
		{
			counters.OnMaximumLevel();
		}
	}
}
