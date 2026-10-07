using System.Collections.Generic;
using System;
using UnityEngine;

namespace Eclipse.Modding
{
    public sealed class ModCharacterCondition : ConditionAnimation
    {
        private readonly string character;
        public ModCharacterCondition(string character) : base(ConditionType.ECLIPSE_CHARACTER) { this.character=character; }
        public override bool IsEqual(ModelConditions conditions)
        {
            bool matches=!string.IsNullOrEmpty(character) && conditions != null && conditions.EclipseCharacterId==character;
            return IsNot ? !matches : matches;
        }
    }

    // Runs a completed mode request after Lua has returned. Scene destruction
    // cancels it, and no coroutine or continuation is serialized into the save.
    public sealed class ModPendingEncounter : MonoBehaviour
    {
        private ModModeRequest request;
        private Action ready, cancel;
        public void Configure(ModModeRequest request, Action ready, Action cancel)
        { this.request=request; this.ready=ready; this.cancel=cancel; }
        private void Update()
        {
            if (request == null || request.IsPending) return;
            var pending=request; request=null;
            var action=pending.Plan == null ? cancel : ready;
            try { action?.Invoke(); }
            catch (Exception error) { pending.Invalidate(); cancel?.Invoke(); Debug.LogWarning("[ModMode] " + error.Message); }
            finally { ready=cancel=null; Destroy(gameObject); }
        }
        private void OnDestroy()
        {
            if (request == null) return;
            request.Invalidate(); request=null; cancel?.Invoke(); ready=cancel=null;
        }
    }

    public static partial class ModRuntime
    {
        public static bool HasAiHandler(string tactic) => _scripts != null && _scripts.HasAiHandler(tactic);

        public static int? DecideAi(string tactic, object instance, Model self, Model opponent, int frame,
            System.Collections.Generic.IReadOnlyList<InfoAnimation> actions)
        {
            try
            {
                if (_scripts == null) return null;
                var candidates = new ModAiActionSnapshot[actions.Count];
                for (int i = 0; i < candidates.Length; i++)
                    candidates[i] = AiActionSnapshot(actions[i]);
                var selfSnapshot = AiSnapshot(self);
                double backWallDistance = Math.Abs(selfSnapshot.X - self.GetBackWallX());
                return _scripts.DecideAi(tactic, instance,
                    new ModCombatSnapshot(selfSnapshot, AiSnapshot(opponent), Math.Max(0, frame), true, backWallDistance), candidates);
            }
            catch (Exception error) { Debug.LogWarning("[ModAI] Using native tactics after decision failure. " + error.Message); return null; }
        }

        private static ModAiActionSnapshot AiActionSnapshot(InfoAnimation action)
        {
            string type = action.Type == InfoAnimation.AnimationKind.AnimationAttack ? "attack" :
                action.Type == InfoAnimation.AnimationKind.AnimationMove ? "move" : "none";
            var timing = new ModAiActionTiming(action.FirstFrame, action.AnimationEndFrame,
                action.MidFrames, action.GetIsLooped());
            var inputs = new System.Collections.Generic.List<ModAiActionInput>();
            var keys = action.GetFirstKeysCondition()?.RequiredKeys;
            if (keys != null)
            {
                AppendAiInputs(inputs, keys.StarterKeys, "tap");
                AppendAiInputs(inputs, keys.AdditionalKeys, "hold");
                AppendAiInputs(inputs, keys.ReleaseKeys, "release");
            }
            return new ModAiActionSnapshot(action.Name, type, action.Priority, timing, inputs);
        }

        private static void AppendAiInputs(System.Collections.Generic.List<ModAiActionInput> target,
            System.Collections.Generic.List<int> controls, string press)
        {
            if (controls.Count > 64 - target.Count) throw new ModContentException("AI action has too many input entries.");
            foreach (int control in controls)
            {
                string name;
                switch ((FightCID)control)
                {
                    case FightCID.QuadrantUp: name = "Up"; break;
                    case FightCID.QuadrantUpForward: name = "Up-Forward"; break;
                    case FightCID.QuadrantForward: name = "Forward"; break;
                    case FightCID.QuadrantDownForward: name = "Down-Forward"; break;
                    case FightCID.QuadrantDown: name = "Down"; break;
                    case FightCID.QuadrantDownBack: name = "Down-Back"; break;
                    case FightCID.QuadrantBack: name = "Back"; break;
                    case FightCID.QuadrantUpBack: name = "Up-Back"; break;
                    case FightCID.Punch: name = "Punch"; break;
                    case FightCID.Kick: name = "Kick"; break;
                    case FightCID.MissileButton: name = "Ranged"; break;
                    case FightCID.MagicButton: name = "Magic"; break;
                    case FightCID.RaidChargeButton: name = "RaidCharge"; break;
                    case FightCID.Super: name = "Super"; break;
                    default: name = "Unknown"; break;
                }
                target.Add(new ModAiActionInput(name, press));
            }
        }

        private static ModFighterSnapshot AiSnapshot(Model model)
        {
            if (model == null || model.Parameters == null || model.GetPosition() == null) return null;
            var position = model.GetPosition();
            return new ModFighterSnapshot(model.GetLife(), model.Parameters.MaxLife,
                model.Parameters.HealthBarCount, position.GetX(),position.GetY(),position.GetZ(),
                CaptureAnimationSnapshot(model),Fight.GetCurrentFight()?.CaptureEclipseActorIdentity(model));
        }

        public static ModAnimationSnapshot CaptureAnimationSnapshot(Model model)
        {
            var controller = model?.GetAnimationModule();
            if (controller == null || !controller.GetIsPlaying()) return null;
            var animation = controller.GetCurrentInfo();
            var active = controller.GetActiveIntervals();
            if (animation == null || animation.Name == null || active == null || active.Count > 256) return null;
            int facing = controller.GetSign();
            if (facing != -1 && facing != 1) return null;
            var intervals = new ModAnimationIntervalSnapshot[active.Count];
            for (int i = 0; i < intervals.Length; i++)
            {
                var interval = active[i];
                if (interval == null) return null;
                string kind;
                switch (interval.Type)
                {
                    case IntervalAnimation.IntervalType.INTERVAL_UNSTABLE: kind = "unstable"; break;
                    case IntervalAnimation.IntervalType.INTERVAL_UNINTERRUPT: kind = "uninterrupt"; break;
                    case IntervalAnimation.IntervalType.INTERVAL_SELF_UNINTERRUPT: kind = "self_uninterrupt"; break;
                    case IntervalAnimation.IntervalType.INTERVAL_ATTACK: kind = "attack"; break;
                    case IntervalAnimation.IntervalType.INTERVAL_BLOCK: kind = "block"; break;
                    case IntervalAnimation.IntervalType.INTERVAL_INVULNERABLE: kind = "invulnerable"; break;
                    case IntervalAnimation.IntervalType.INTERVAL_INVISIBLE: kind = "invisible"; break;
                    default: kind = "none"; break;
                }
                intervals[i] = new ModAnimationIntervalSnapshot(interval.Name ?? string.Empty, kind);
            }
            string type = animation.Type == InfoAnimation.AnimationKind.AnimationAttack ? "attack" :
                animation.Type == InfoAnimation.AnimationKind.AnimationMove ? "move" : "none";
            return new ModAnimationSnapshot(animation.Name, type, facing, intervals);
        }

        public static void ApplyLocaleMetadata()
        {
            if (_legacyContent == null) return;
            try
            {
                ExternalLocaleRuntime.Apply();
            }
            catch (Exception exception)
            {
                Debug.LogError("[ModContent] Failed to bind mod locales; disabling external mods without restarting the game parser. " + exception);
                Shutdown();
            }
        }

        public static void ApplyP1DContent()
        {
            if (_legacyContent == null) return;
            try
            {
                _legacyContent.ApplyP1DContent();
                Debug.Log("[ModContent] Applied P1D content: " + Scripts.Content.LocaleMetadata.Count +
                    " locales, " + Scripts.Content.Locations.Count + " locations, " +
                    Scripts.Content.MoveTemplates.Count + " move templates, " + Scripts.Content.Moves.Count +
                    " moves, " + Scripts.Content.MoveTriggers.Count + " move triggers, " +
                    Scripts.Content.Tactics.Count + " tactics.");
            }
            catch (Exception exception)
            {
                Debug.LogError("[ModContent] Failed to apply P1D content; mod startup is invalid. " + exception);
                throw;
            }
        }

        // Developer live tuning for move edits (sf2.moves.patch, forks, item lock edits and
        // movesets/*.json). Re-runs every enabled mod's registration in a scratch host and
        // session (loose files are re-indexed, so edited Lua and moveset files are read) and
        // swaps only the move overlay. All other content, running Lua behaviors and the save
        // fingerprint keep the startup session until the game restarts.
        public static bool TryReloadMovePatches(out string report) => TryApplyMoveOverlay(null, out report);

        /// <summary>True while a reload or the Moveset Lab has replaced the startup move edits.</summary>
        public static bool IsMoveOverlayActive => _legacyContent != null && _legacyContent.HasReplacedOverlay;

        /// <summary>
        /// Applies the move edits of every enabled mod as currently saved on disk.
        /// <paramref name="labModId"/> names the Moveset Lab's own mod, which may be new since
        /// startup; every other active mod must be unchanged.
        /// </summary>
        public static bool TryApplyMoveOverlay(string labModId, out string report)
        {
            if (labModId != null) _labMods.Add(labModId);
            if (_host == null || _scripts == null || _legacyContent == null)
            {
                report = "Mod content is not active.";
                return false;
            }
            if (Eclipse.Multiplayer.OnlineVersusSession.IsActive || Eclipse.Multiplayer.RoomSession.IsActive ||
                Eclipse.Multiplayer.LocalVersusSession.IsOnline)
            {
                report = "Move edits cannot be reloaded during an online session.";
                return false;
            }
            ModHost scratchHost = null;
            ModScriptSession scratch = null;
            try
            {
                scratchHost = ModHost.Build(_host.ModsRoot);
                scratch = scratchHost.StartScripts(new MoonSharpScriptRuntime(null,
                    () => LocalizationManager.CurrentLanguage == null ? LocalizationManager.DefaultLanguageName : LocalizationManager.CurrentLanguage.name,
                    new ModDojoSelection(), new ModStoryEvents(), () => new ModAudioBackend()), LogScript, ImportCoreContent);
                string mismatch = DescribeReloadMismatch(scratch, labModId);
                if (mismatch != null)
                {
                    report = "Move edits were not applied: " + mismatch;
                    Debug.LogWarning("[ModContent] " + report);
                    return false;
                }
                var content = scratch.Content;
                _legacyContent.ReplaceMoveOverlay(LegacyContentAdapter.MoveOverlaySet.From(content));
                report = "Applied " + content.MoveCombatPatches.Count + " move patch(es) and " + content.MoveForks.Count +
                    " fork(s) from " + scratch.ActiveMods.Count + " mod(s).";
                Debug.Log("[ModContent] " + report);
                return true;
            }
            catch (Exception exception)
            {
                report = "Move edits were not applied; the previous edits remain: " + exception.Message;
                Debug.LogWarning("[ModContent] " + report + "\n" + exception);
                return false;
            }
            finally
            {
                scratch?.Dispose();
                scratchHost?.Dispose();
                LoadTimings.Take();
            }
        }

        /// <summary>Runs <paramref name="read"/> against base-game moves (no mod move edits applied).</summary>
        internal static void WithoutMoveOverlay(Action read)
        {
            if (_legacyContent == null) read();
            else _legacyContent.WithoutMoveOverlay(read);
        }

        /// <summary>Restores the move edits the game started with (closing the Moveset Lab).</summary>
        public static void ClearMoveOverlay()
        {
            try { _legacyContent?.RestoreStartupMoveOverlay(); }
            catch (Exception exception) { Debug.LogError("[ModContent] Could not restore startup move edits: " + exception); }
        }

        /// <summary>
        /// Why online play is unavailable, or null. Moveset files and the Moveset Lab change
        /// combat without changing a mod's version, which online play cannot verify.
        /// </summary>
        public static string OnlineBlockReason()
        {
            if (IsMoveOverlayActive) return "Move edits are being tested. Leave the Moveset Lab (or restart after a move reload) to play online.";
            var files = _scripts?.Content.MovesetFiles;
            if (files == null || files.Count == 0) return null;
            var owners = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var file in files) owners.Add(file.Owner.Value);
            return "Moveset mods are offline only. Disable " + string.Join(", ", owners) + " in Mods to play online.";
        }

        public static void RequireOnlineAllowed()
        {
            string reason = OnlineBlockReason();
            if (reason != null) throw new InvalidOperationException(reason);
        }

        // The scratch session must activate the same mods as the running one (except the
        // Moveset Lab's own mod); otherwise its overlay would silently drop or add whole mods.
        // Mods the Moveset Lab has saved this session; they may be new since startup.
        private static readonly HashSet<string> _labMods = new HashSet<string>(StringComparer.Ordinal);

        private static string DescribeReloadMismatch(ModScriptSession scratch, string labModId)
        {
            foreach (ModDiagnostic diagnostic in scratch.Diagnostics)
                if (diagnostic.Severity == ModDiagnosticSeverity.Error) return diagnostic.ToString();
            var live = new Dictionary<string, SemanticVersion>(StringComparer.Ordinal);
            foreach (var mod in _scripts.ActiveMods) live[mod.Id.Value] = mod.Version;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var mod in scratch.ActiveMods)
            {
                seen.Add(mod.Id.Value);
                if (mod.Id.Value == labModId) continue;
                if (!live.ContainsKey(mod.Id.Value) && _labMods.Contains(mod.Id.Value)) continue;
                if (!live.TryGetValue(mod.Id.Value, out var version))
                    return "mod '" + mod.Id + "' was enabled since startup; restart the game to apply it.";
                if (!version.Equals(mod.Version))
                    return "mod '" + mod.Id + "' changed version; restart the game to apply it.";
            }
            foreach (var id in live.Keys)
                if (!seen.Contains(id)) return "mod '" + id + "' is no longer active; restart the game to apply the change.";
            if (labModId != null && !seen.Contains(labModId))
                return "the Moveset Lab mod '" + labModId + "' is disabled; enable it in Mods.";
            return null;
        }
    }
}
