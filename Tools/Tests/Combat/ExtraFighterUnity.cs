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
    static int phase, checks, births, phaseFrame;
    static Model actor;
    static bool explicitAttack;
    static float playerBefore, enemyBefore, actorBefore;
    static string failure;
    static readonly System.Collections.Generic.HashSet<string> animations = new System.Collections.Generic.HashSet<string>();
    static string Root => Path.GetDirectoryName(Application.dataPath);

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
            player.Parameters.UserControlled = false; player.Parameters.AiControlled = false; enemy.Parameters.AiControlled = false;
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
                        if (value is Model.EventModel notification && notification.Data is InfoAnimation animation) animations.Add(animation.Name);
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
                    var origin = player.PLBNCDCFPML();
                    enemy.ShiftModelPosition(new Vector3f(origin.GetX() + 700 - enemy.PLBNCDCFPML().GetX(), 0, 0), true);
                    actor.ShiftModelPosition(new Vector3f(origin.GetX() + 180 - actor.PLBNCDCFPML().GetX(), 0, 0), true);
                    Check(player.EGGEACCDAEK() == enemy, "Native targeting unexpectedly selected nearest root");
                    Debug.Log("[ExtraFighterUnity] Native target remains first registered enemy despite nearer third root; actor=" + actor.get_Name() + " life=" + actor.KKMCHCNOHMB());
                    playerBefore = player.KKMCHCNOHMB(); enemyBefore = enemy.KKMCHCNOHMB();
                    Next(fight); break;
                case 2:
                    if (player.KKMCHCNOHMB() >= playerBefore && frame - phaseFrame < 120) return;
                    Check(actor.GetCurrentAnimation() != null && animations.Count > 0, "Third root did not start native AI animation");
                    Debug.Log("[ExtraFighterUnity] Bounded AI observation: starts=" + births + "; animations=" + string.Join(",", animations) + "; contact=" + (player.KKMCHCNOHMB() < playerBefore));
                    if (player.KKMCHCNOHMB() >= playerBefore)
                    {
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
                    actor.Parameters.AiControlled = false;
                    fight.UpdateLife(actor, -actor.Parameters.MaxLife);
                    Check(actor.KKMCHCNOHMB() == 0 && actor.Parameters.PCALDKCJGCK, "Extra root knockout not recorded");
                    Next(fight); break;
                case 4:
                    if (frame - phaseFrame < 10) return;
                    Check(fight.GetPlayerModel() == player && fight.GetEnemyModel() == enemy && fight.LNDLFINJHDB.Contains(actor), "Extra root knockout changed main identities");
                    Check(!(bool)Field(fight, "isGameOver") && !(bool)Field(fight, "isStopFight"), "Extra root knockout incorrectly ended duel");
                    Check(actor.GetRenderObject() != null && actor.GetRenderObject().activeInHierarchy, "Extra root unexpectedly retired itself");
                    Invoke(fight, "RequestModelRemoval", actor);
                    Next(fight); break;
                case 5:
                    if (fight.LNDLFINJHDB.Contains(actor)) return;
                    Check(!player._Enemies.Contains(actor) && !enemy._Enemies.Contains(actor), "Removal retained targeting references");
                    Check(actor.GetRenderObject() == null || !actor.GetRenderObject().activeInHierarchy, "Removal retained rendering");
                    Check(player.EGGEACCDAEK() == enemy && enemy.EGGEACCDAEK() == player, "Removal corrupted original duel targets");
                    Check(fight.LNDLFINJHDB.Count(m => !(m is WeaponModel)) == 2, "Removal lost original roots");
                    Next(fight); break;
                case 6:
                    if (frame - phaseFrame < 10) return;
                    Check(player.GetCurrentAnimation() != null && enemy.GetCurrentAnimation() != null, "Original duel cannot continue after third root");
                    File.WriteAllText(Path.Combine(Root, "extra-fighter-result.txt"), "PASS: " + checks + " full-game native extra-root feasibility checks. Independent cloned health, native rig/rendering, bounded AI observation, native contact attribution (explicit attack requested=" + explicitAttack + "), insertion-order mutual enemies, main-duel-only round result and requested removal. Reflection/controlled spacing/inputs and fresh profile; not a public actor API, reliable autonomous attack selection, teams, custom rig or multiplayer acceptance.");
                    Debug.Log("[ExtraFighterUnity] PASS: " + checks + " checks"); Finish(0); break;
            }
        }
        catch (Exception error) { Debug.LogError("[ExtraFighterUnity] FAIL: " + error); Finish(1); }
    }
    static void Log(string message, string stack, LogType type)
    {
        if (entered && type == LogType.Exception && (stack.Contains("Fight.") || stack.Contains("Model.") || stack.Contains("SelectAnimation."))) failure = message + "\n" + stack;
    }
    static void Finish(int code)
    {
        SessionState.SetBool(Active, false); EditorApplication.update -= Update; Application.logMessageReceived -= Log; EditorApplication.Exit(code);
    }
}
#endif
