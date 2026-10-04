#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Research fixture, not a supported actor API. Reflection deliberately enters
// recovered native seams without adding a public spawning contract.
[InitializeOnLoad]
public static class ExtraFighterUnity
{
    const string Active = "Eclipse.ExtraFighterUnity.Active";
    const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    static double started, report;
    static bool campaign, entered;
    static int phase, checks, births, phaseFrame, attackStarts, firstContactAttackStarts;
    static Model actor;
    static bool explicitAttack;
    static bool targetingAccepted;
    static float playerBefore, enemyBefore, actorBefore, firstContactHealth, secondAttackBeforeHealth;
    static string failure;
    static readonly System.Collections.Generic.HashSet<string> animations = new System.Collections.Generic.HashSet<string>();
    static string Root => Path.GetDirectoryName(Application.dataPath);
    static bool RequireAutonomous => Environment.GetCommandLineArgs().Contains("-requireAutonomousExtraFighter");
    static bool RequireTargeting => Environment.GetCommandLineArgs().Contains("-requireTargetingExtraFighter");

    static ExtraFighterUnity()
    {
        if (!SessionState.GetBool(Active, false)) return;
        started = EditorApplication.timeSinceStartup;
        EditorApplication.update += Update;
        Application.logMessageReceived += Log;
    }
    public static void Run()
    {
        if (!File.Exists(Path.Combine(Root, "extra-fighter-fixture.marker"))) throw new Exception("Requires isolated extra-fighter fixture.");
        Environment.SetEnvironmentVariable("ECLIPSE_MODS_ROOT", Path.Combine(Root, "extra-fighter-empty-mods"));
        var args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "-extraFighterAcceptanceProductName");
        if (index < 0 || index + 1 >= args.Length || !System.Text.RegularExpressions.Regex.IsMatch(args[index + 1], "^ExtraFighterUnity-[0-9a-f]{32}$"))
            throw new Exception("Pass the generated acceptance product through the runner.");
        PlayerSettings.companyName = "EclipseAcceptance";
        PlayerSettings.productName = args[index + 1];
        SessionState.SetBool(Active, true);
        EditorSceneManager.OpenScene("Assets/src/GUI/Scenes/GameLoaderScene/GameLoader.unity");
        var view = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        view.Show(); view.Focus(); EditorApplication.EnterPlaymode();
    }
    static object Field(object value, string name) => value.GetType().GetField(name, Hidden | BindingFlags.Public).GetValue(value);
    static object Invoke(object value, string name, params object[] args) => value.GetType().GetMethod(name, Hidden | BindingFlags.Public).Invoke(value, args);
    static void Check(bool value, string message) { checks++; if (!value) throw new Exception(message); }
    static void Next(Fight fight) { phase++; phaseFrame = fight.get_FightTimeInFrames(); }
    static void Update()
    {
        if (!EditorApplication.isPlaying) return;
        try
        {
            if (failure != null) throw new Exception(failure);
            if (EditorApplication.timeSinceStartup - started > 300) throw new Exception("Timed out phase " + phase + "; starts=" + births + "; animations=" + string.Join(",", animations));
            if (EditorApplication.timeSinceStartup - report > 20) { report = EditorApplication.timeSinceStartup; Debug.Log("[ExtraFighterUnity] Waiting entered=" + entered + " phase=" + phase + "; starts=" + births + "; animations=" + string.Join(",", animations)); }
            if (!campaign && Eclipse.UI.TitleScreen.IsOpen)
            {
                var title = UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.TitleScreen>();
                if ((bool)Field(title, "splashing") || (string)Field(title, "currentPage") != "Home") return;
                Invoke(title, "BeginCampaign");
                var directory = SF2Paths.GetUserDataDirectory();
                Check(Eclipse.Saves.CampaignSaveSession.PreviewDirectory == null && directory.StartsWith(Application.persistentDataPath, StringComparison.OrdinalIgnoreCase) && Application.persistentDataPath.Contains("ExtraFighterUnity-"), "Profile not isolated");
                var profile = XmlUtils.OpenXMLDocument(SF2Paths.KKIDGPBOBNI(), "usersDefault.xml", XmlUtils.EBLFEPIOMOL.Normal, true, XmlCryptoUtils.NNLGALNDJCL());
                ((System.Xml.XmlElement)profile.SelectSingleNode("/Root/Warriors/Warrior[@ID='1']")).SetAttribute("Tutorial", "END");
                Directory.CreateDirectory(directory); XmlUtils.ONLDJNLKKAL(profile, Path.Combine(directory, Constants.OJMIJINKBPJ).Replace('\\', '/'));
                campaign = true; return;
            }
            if (!entered)
            {
                if (Eclipse.Modding.ModRuntime.Scripts == null || Module.GetInstance() == null) return;
                var screen = Module.GetInstance().GetCurrentScreenType(); if (screen != ScreenType.ModuleDojo && screen != ScreenType.ModuleMap) return;
                Check(!Eclipse.Modding.ModRuntime.Host.HasErrors, Eclipse.Modding.ModRuntime.Host.FormatReport());
                Check(Eclipse.Modding.ModRuntime.Host.EnabledMods.All(m => m.Id.Value == "core"), "Probe must have no user mods");
                var encounter = ListSF.CHMCKGCDGCM(new FightIDS(Eclipse.Modding.ModRuntime.Scripts.Content.RuntimeFightId(Eclipse.Modding.DefinitionId.Parse("core:fights/zone_1/tournament/3"))));
                Check(encounter != null, "Core encounter missing"); entered = GameUtils.StartFight(encounter, false, null, true, false); return;
            }
            var fight = Fight.GetCurrentFight();
            if (fight == null || fight.get_FightTimeInFrames() < 100) return;
            var player = fight.GetPlayerModel(); var enemy = fight.GetEnemyModel();
            if (player == null || enemy == null) return;
            player.Parameters.UserControlled = false; player.Parameters.AiControlled = false;
            if (phase != 8) enemy.Parameters.AiControlled = false;
            int frame = fight.get_FightTimeInFrames();
            if (actor != null && fight.LNDLFINJHDB.Contains(actor) && actor.GetCurrentAnimation() != null) animations.Add(actor.GetCurrentAnimation().Name);
            switch (phase)
            {
                case 0:
                    var parameters = new ModelParameters(enemy.Parameters);
                    parameters.IsPlayer = false; parameters.UserControlled = false; parameters.AiControlled = true;
                    Check(parameters != enemy.Parameters && parameters.MaxLife > 0 && parameters.HBFMBOHLKPJ != null, "Clone lacks independent health/tactic");
                    actor = (Model)Invoke(fight, "AddModel", parameters);
                    actor.set_Name("extra-fighter-research");
                    actor.JMHJDHLBHLK = player.JMHJDHLBHLK;
                    actor.AHBNPODMIOD(true);
                    Invoke(Field(fight, "_SelectAnimation"), "PrepareFormAnimation", actor);
                    actor.AddEventListener(2, value =>
                    {
                        births++;
                        if (value is Model.EventModel notification && notification.Data is InfoAnimation animation)
                        {
                            animations.Add(animation.Name);
                            if (animation.Type == InfoAnimation.MGHNBEPCKIF.AnimationAttack)
                            {
                                attackStarts++;
                                if (firstContactAttackStarts > 0 && attackStarts == firstContactAttackStarts + 1)
                                    secondAttackBeforeHealth = Fight.GetCurrentFight().GetPlayerModel().KKMCHCNOHMB();
                            }
                        }
                    });
                    Check(!(actor is WeaponModel) && actor.NJDJHGDMCIJ() == null && actor.GetRootModel() == actor, "Probe is a weapon child");
                    Check(fight.LNDLFINJHDB.Count(m => !(m is WeaponModel)) == 3, "No third root");
                    Check(actor._Enemies.Contains(player) && actor._Enemies.Contains(enemy) && player._Enemies.Contains(actor) && enemy._Enemies.Contains(actor), "Native insertion did not make mutual enemies");
                    Check(actor.EGGEACCDAEK() == player && player.EGGEACCDAEK() == enemy, "Insertion-order target assumption changed");
                    playerBefore = player.KKMCHCNOHMB(); enemyBefore = enemy.KKMCHCNOHMB(); actorBefore = actor.KKMCHCNOHMB();
                    fight.UpdateLife(actor, -parameters.MaxLife * .2f);
                    Check(actor.KKMCHCNOHMB() < actorBefore && Math.Abs(player.KKMCHCNOHMB() - playerBefore) < .00001 && Math.Abs(enemy.KKMCHCNOHMB() - enemyBefore) < .00001, "Extra root health is shared");
                    fight.SetLife(actor, parameters.MaxLife);
                    Next(fight); break;
                case 1:
                    if (actor.GetCurrentAnimation() == null || frame - phaseFrame < 10) return;
                    animations.Add(actor.GetCurrentAnimation().Name);
                    Check(actor.GetRenderObject() != null && actor.GetRenderObject().activeInHierarchy, "Third root did not render");
                    Check(actor.Parameters.AiControlled && actor.JMHJDHLBHLK == 2, "Third root not in active AI stage");
                    CheckNodeBindings(player, enemy, actor);
                    Debug.Log("[ExtraFighterUnity] Third-root tactic=" + actor.Parameters.HBFMBOHLKPJ.get_Type() + "; decisionDelay=" + Field(actor, "APOHBENDEKO"));
                    var origin = player.PLBNCDCFPML();
                    enemy.ShiftModelPosition(new Vector3f(origin.GetX() + 700 - enemy.PLBNCDCFPML().GetX(), 0, 0), true);
                    actor.ShiftModelPosition(new Vector3f(origin.GetX() + 180 - actor.PLBNCDCFPML().GetX(), 0, 0), true);
                    Check(player.EGGEACCDAEK() == enemy, "Native targeting unexpectedly selected nearest root");
                    Debug.Log("[ExtraFighterUnity] Native target remains first registered enemy despite nearer third root; actor=" + actor.get_Name() + " life=" + actor.KKMCHCNOHMB());
                    playerBefore = player.KKMCHCNOHMB(); enemyBefore = enemy.KKMCHCNOHMB();
                    Next(fight); break;
                case 2:
                    int observationFrames = RequireAutonomous ? 600 : 120;
                    if (player.KKMCHCNOHMB() >= playerBefore && frame - phaseFrame < observationFrames) return;
                    Check(actor.GetCurrentAnimation() != null && animations.Count > 0, "Third root did not start native AI animation");
                    Debug.Log("[ExtraFighterUnity] Bounded AI observation: starts=" + births + "; animations=" + string.Join(",", animations) + "; contact=" + (player.KKMCHCNOHMB() < playerBefore) + "; decisionDelay=" + Field(actor, "APOHBENDEKO"));
                    if (player.KKMCHCNOHMB() >= playerBefore)
                    {
                        Check(!RequireAutonomous, "Third-root AI did not make native contact within 600 frames; explicit playback is forbidden in this acceptance run");
                        actor.Parameters.AiControlled = false;
                        var attacks = actor.GetAvailableAnimations().Where(m => m.Name.StartsWith("Knives", StringComparison.Ordinal) && m.MoveData.Intervals.OfType<IntervalAttack>().Any() && !m.Name.Contains("Throw")).ToArray();
                        Check(attacks.Length > 0, "No native knife attack for controlled contact proof");
                        var point = player.PLBNCDCFPML();
                        actor.ShiftModelPosition(new Vector3f(point.GetX() + 100 - actor.PLBNCDCFPML().GetX(), 0, 0), true);
                        Check(actor.PlayAnimation(attacks[0], -1), "Controlled native attack did not start");
                        explicitAttack = true;
                        Debug.Log("[ExtraFighterUnity] Explicit contact probe: " + attacks[0].Name);
                    }
                    Next(fight); break;
                case 3:
                    if (player.KKMCHCNOHMB() >= playerBefore) return;
                    Check(((Model.StrikeResult)Field(player, "GHHCDAFIKJE")).AttackerModel == actor, "Player health loss was not an extra-root native strike");
                    Check(Math.Abs(enemy.KKMCHCNOHMB() - enemyBefore) < .00001, "Third root strike damaged wrong target");
                    Debug.Log("[ExtraFighterUnity] Native third-root contact: player=" + playerBefore + " -> " + player.KKMCHCNOHMB() + "; explicit=" + explicitAttack);
                    if (RequireAutonomous)
                    {
                        Check(attackStarts >= 1, "First autonomous contact lacks a native attack start");
                        CheckAiObservations(actor);
                    }
                    firstContactHealth = player.KKMCHCNOHMB();
                    firstContactAttackStarts = attackStarts;
                    secondAttackBeforeHealth = float.NaN;
                    Next(fight); break;
                case 4:
                    if (RequireAutonomous && !(attackStarts > firstContactAttackStarts && player.KKMCHCNOHMB() < secondAttackBeforeHealth - .00001f)) return;
                    if (RequireAutonomous)
                    {
                        Check(!explicitAttack && actor.Parameters.AiControlled && attackStarts > firstContactAttackStarts, "Sustained autonomous attack required fixture playback or lost AI");
                        Check(((Model.StrikeResult)Field(player, "GHHCDAFIKJE")).AttackerModel == actor, "Second native contact came from another attacker");
                        Check(Math.Abs(enemy.KKMCHCNOHMB() - enemyBefore) < .00001, "Sustained AI contact damaged original opponent");
                        CheckAiObservations(actor);
                        Debug.Log("[ExtraFighterUnity] Sustained autonomous contact: " + firstContactHealth + " -> " + player.KKMCHCNOHMB() + "; animations=" + string.Join(",", animations));
                    }
                    if (RequireTargeting && !targetingAccepted)
                    {
                        actor.Parameters.AiControlled = false;
                        var previousTarget = enemy.EGGEACCDAEK();
                        var previousAnimation = enemy.OCPMJKIEPIG().OJKLPPNCONP();
                        var previousObservation = Field(Field(enemy, "HJOGNGDMAKJ"), "COKFBIJAFLH");
                        var rollback = (Action)Invoke(enemy, "ReplaceCombatEnemies", new Model[] { actor }, actor);
                        CheckTargetBindings(enemy, actor, player);
                        Check(Field(Field(enemy, "HJOGNGDMAKJ"), "COKFBIJAFLH") == null, "Retarget retained previous opponent observation");
                        rollback();
                        Check(enemy.EGGEACCDAEK() == previousTarget && enemy.OCPMJKIEPIG().OJKLPPNCONP() == previousAnimation &&
                            ReferenceEquals(Field(Field(enemy, "HJOGNGDMAKJ"), "COKFBIJAFLH"), previousObservation), "Synchronous native rollback lost target/animation/observation");
                        Invoke(enemy, "ReplaceCombatEnemies", new Model[] { actor }, actor);
                        Invoke(actor, "ReplaceCombatEnemies", new Model[] { enemy }, enemy);
                        Invoke(player, "ReplaceCombatEnemies", new Model[] { enemy }, enemy);
                        CheckTargetBindings(enemy, actor, player);
                        CheckTargetBindings(actor, enemy, player);
                        CheckTargetBindings(player, enemy, actor);
                        fight.SetLife(actor, actor.Parameters.MaxLife);
                        var actorPoint = actor.PLBNCDCFPML();
                        enemy.ShiftModelPosition(new Vector3f(actorPoint.GetX() + 180 - enemy.PLBNCDCFPML().GetX(), 0, 0), true);
                        actorBefore = actor.KKMCHCNOHMB(); playerBefore = player.KKMCHCNOHMB();
                        enemy.Parameters.AiControlled = true;
                        enemy.BHAFOEICJPE(0);
                        phase = 8; phaseFrame = frame;
                        Debug.Log("[ExtraFighterUnity] Atomic targeting: original enemy now targets extra root; original player excluded from its hostile registry.");
                        break;
                    }
                    actor.Parameters.AiControlled = false;
                    fight.UpdateLife(actor, -actor.Parameters.MaxLife);
                    Check(actor.KKMCHCNOHMB() == 0 && actor.Parameters.PCALDKCJGCK, "Extra root knockout not recorded");
                    Next(fight); break;
                case 5:
                    if (frame - phaseFrame < 10) return;
                    Check(fight.GetPlayerModel() == player && fight.GetEnemyModel() == enemy && fight.LNDLFINJHDB.Contains(actor), "Extra root knockout changed main identities");
                    Check(!(bool)Field(fight, "isGameOver") && !(bool)Field(fight, "isStopFight"), "Extra root knockout incorrectly ended duel");
                    Check(actor.GetRenderObject() != null && actor.GetRenderObject().activeInHierarchy, "Extra root unexpectedly retired itself");
                    Invoke(fight, "RequestModelRemoval", actor);
                    Next(fight); break;
                case 6:
                    if (fight.LNDLFINJHDB.Contains(actor)) return;
                    Check(!player._Enemies.Contains(actor) && !enemy._Enemies.Contains(actor), "Removal retained targeting references");
                    Check(actor.GetRenderObject() == null || !actor.GetRenderObject().activeInHierarchy, "Removal retained rendering");
                    Check(player.EGGEACCDAEK() == enemy && enemy.EGGEACCDAEK() == player, "Removal corrupted original duel targets");
                    Check(fight.LNDLFINJHDB.Count(m => !(m is WeaponModel)) == 2, "Removal lost original roots");
                    CheckNodeBindings(player, enemy);
                    Next(fight); break;
                case 7:
                    if (frame - phaseFrame < 10) return;
                    Check(player.GetCurrentAnimation() != null && enemy.GetCurrentAnimation() != null, "Original duel cannot continue after third root");
                    File.WriteAllText(Path.Combine(Root, "extra-fighter-result.txt"), "PASS: " + checks + " full-game native extra-root feasibility checks. Independent cloned health, native rig/rendering and shared point bindings, native contact attribution (explicit attack requested=" + explicitAttack + "; sustained autonomous acceptance required=" + RequireAutonomous + "; atomic targeting and incoming native contact accepted=" + targetingAccepted + "), initial insertion-order mutual enemies, main-duel-only round result and requested removal. Reflection/controlled spacing/inputs and fresh profile; not a public actor API, general teams, arbitrary rigs/loadouts, multiplayer or export acceptance.");
                    Debug.Log("[ExtraFighterUnity] PASS: " + checks + " checks"); Finish(0); break;
                case 8:
                    if (Math.Abs(player.KKMCHCNOHMB() - playerBefore) >= .00001)
                        throw new Exception("Retargeted enemy damaged excluded original player");
                    if (actor.KKMCHCNOHMB() >= actorBefore)
                    {
                        if (frame - phaseFrame >= 600) throw new Exception("Retargeted native AI did not strike extra root within 600 frames");
                        return;
                    }
                    Check(Math.Abs(player.KKMCHCNOHMB() - playerBefore) < .00001, "Excluded original player health changed");
                    Check(((Model.StrikeResult)Field(actor, "GHHCDAFIKJE")).AttackerModel == enemy, "Incoming extra-root damage has wrong native attacker identity");
                    CheckTargetBindings(enemy, actor, player);
                    CheckAiObservations(enemy);
                    CheckNodeBindings(player, enemy, actor);
                    Debug.Log("[ExtraFighterUnity] Incoming native contact after retarget: actor=" + actorBefore + " -> " + actor.KKMCHCNOHMB() + "; original player unchanged; attacker=" + enemy.get_Name());
                    enemy.Parameters.AiControlled = false;
                    Invoke(player, "ReplaceCombatEnemies", new Model[] { enemy, actor }, enemy);
                    Invoke(enemy, "ReplaceCombatEnemies", new Model[] { player, actor }, player);
                    Invoke(actor, "ReplaceCombatEnemies", new Model[] { player, enemy }, player);
                    CheckTargetBindings(player, enemy, null);
                    CheckTargetBindings(enemy, player, null);
                    CheckTargetBindings(actor, player, null);
                    targetingAccepted = true;
                    // Rejoin the ordinary knockout/removal path without crediting
                    // the incoming-contact scenario as another autonomous attack.
                    actor.Parameters.AiControlled = false;
                    fight.UpdateLife(actor, -actor.Parameters.MaxLife);
                    Check(actor.KKMCHCNOHMB() == 0 && actor.Parameters.PCALDKCJGCK, "Retargeted extra root knockout not recorded");
                    phase = 5; phaseFrame = frame;
                    break;
            }
        }
        catch (Exception error) { Debug.LogError("[ExtraFighterUnity] FAIL: " + error); Finish(1); }
    }
    static void Log(string message, string stack, LogType type)
    {
        if (entered && type == LogType.Exception && (stack.Contains("Fight.") || stack.Contains("Model.") || stack.Contains("SelectAnimation."))) failure = message + "\n" + stack;
    }
    static void CheckNodeBindings(params Model[] models)
    {
        int observations = 0;
        var lookup = typeof(DistancePoint).GetMethod("MHIDGNCKHON", Hidden);
        foreach (var model in models)
        {
            var conditions = model.EBABHGHPLFK();
            Check(conditions.IHJJBIDMEMB.CBAECAAKAIA == model.GetModelObject(), "Native conditions lost own model identity");
            foreach (var move in model.GetAvailableAnimations())
            foreach (var distance in move.SelectionConditions.OfType<ConditionDistance>())
            foreach (var member in typeof(ConditionDistance).GetFields(Hidden).Where(f => f.FieldType == typeof(DistancePoint)))
            {
                var point = (DistancePoint)member.GetValue(distance);
                if (point.HLGJJGHDEAP != DistancePoint.Object.OBJECT_NODES) continue;
                Model root;
                switch (point.OOFFOILONLO)
                {
                    case ModelType.KEIDBIOIFGA.MODEL_NULL:
                    case ModelType.KEIDBIOIFGA.MODEL_THIS: root = model; break;
                    case ModelType.KEIDBIOIFGA.MODEL_OTHER:
                    case ModelType.KEIDBIOIFGA.MODEL_OTHER_CHILD: root = model.EGGEACCDAEK(); break;
                    default: continue;
                }
                if (root == null) continue;
                var bound = (DistancePoint.PointNode)lookup.Invoke(point, new object[] { conditions });
                Check(bound.Node == root.GetModelObject().EGHIDHMENEF(point.Part), "Shared native point borrowed another body: " + model.get_Name() + "/" + move.Name + "/" + point.Part);
                observations++;
            }
        }
        Check(observations > 0, "No real shared native node conditions observed");
        Debug.Log("[ExtraFighterUnity] Verified " + observations + " shared native node bindings across " + models.Length + " roots");
    }
    static void CheckAiObservations(Model model)
    {
        var controller = Field(model, "HJOGNGDMAKJ");
        var own = model.GetCurrentAnimation();
        if (own != null && model.OCPMJKIEPIG().NMEEPBDJHMG())
            Check(ReferenceEquals(Field(controller, "CGPDPHJIDPA"), own.IMFGMAAEMIC() ?? own), "Third-root controller did not observe its own actual move");
        var target = model.EGGEACCDAEK();
        var other = target?.GetCurrentAnimation();
        if (other != null && target.OCPMJKIEPIG().NMEEPBDJHMG())
            Check(ReferenceEquals(Field(controller, "COKFBIJAFLH"), other.IMFGMAAEMIC() ?? other), "Third-root controller did not observe its actual target's move");
    }
    static void CheckTargetBindings(Model model, Model target, Model excluded)
    {
        Check(model.EGGEACCDAEK() == target && model._Enemies[0] == target, "Cached/insertion target disagrees with explicit selection");
        Check(model.OCPMJKIEPIG().OJKLPPNCONP() == target.OCPMJKIEPIG(), "Native animation retains old enemy body");
        Check(((Model.EventModel)Field(model, "KDAHHIMLJGG")).GAIBPAGPEGK == target, "Native event target disagrees with selection");
        if (excluded != null) Check(!model._Enemies.Contains(excluded), "Explicit hostile roots retained excluded ally");
        foreach (var child in model.KGGIDBLBMDJ()) CheckTargetBindings(child, target, excluded);
    }
    static void Finish(int code)
    {
        SessionState.SetBool(Active, false); EditorApplication.update -= Update; Application.logMessageReceived -= Log; EditorApplication.Exit(code);
    }
}
#endif
