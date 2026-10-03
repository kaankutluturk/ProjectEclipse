#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Eclipse.Modding;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class ArcDartUnity
{
    const string Active = "Eclipse.ArcDartUnity.Active";
    const string Actor = "example.arc-dart.dart", Flight = "example.arc-dart:moves/flight";
    const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    static double started, report, phaseAt;
    static int phase, checks, castFrame, pauseFrame;
    static bool campaign, entered, captured;
    static Model dart;
    static float before, launchX, pauseX, travel;
    static ModUiSurface surface;
    static string combatException;

    static ArcDartUnity()
    {
        if (!SessionState.GetBool(Active, false)) return;
        started = EditorApplication.timeSinceStartup;
        EditorApplication.update += Update;
        Application.logMessageReceived += Log;
    }
    public static void Run()
    {
        var root = Path.GetDirectoryName(Application.dataPath);
        if (!File.Exists(Path.Combine(root, "arc-dart-fixture.marker"))) throw new Exception("Requires isolated projectile fixture.");
        Environment.SetEnvironmentVariable("ECLIPSE_MODS_ROOT", Path.Combine(root, "projectile-mods"));
        var args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "-arcDartAcceptanceProductName");
        if (index < 0 || index + 1 >= args.Length || !System.Text.RegularExpressions.Regex.IsMatch(args[index + 1], "^ArcDartUnity-[0-9a-f]{32}$"))
            throw new Exception("Pass the generated acceptance product through the runner.");
        PlayerSettings.companyName = "EclipseAcceptance"; PlayerSettings.productName = args[index + 1];
        SessionState.SetBool(Active, true);
        EditorSceneManager.OpenScene("Assets/src/GUI/Scenes/GameLoaderScene/GameLoader.unity");
        var view = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        view.Show(); view.Focus(); EditorApplication.EnterPlaymode();
    }
    static object Field(object value, string name) => value.GetType().GetField(name, Hidden | BindingFlags.Public).GetValue(value);
    static void Check(bool value, string message) { checks++; if (!value) throw new Exception(message); }
    static Model[] Darts(Fight fight) => ((IEnumerable)Field(fight, "LNDLFINJHDB")).Cast<Model>().Concat(((IEnumerable)Field(fight, "HCPGFOCGDAA")).Cast<Model>()).Where(m => m.get_Name() == Actor).ToArray();
    static void Next() { phase++; phaseAt = EditorApplication.timeSinceStartup; }
    static void Click()
    {
        var view = UnityEngine.Object.FindObjectsByType<Eclipse.UI.Modding.ModUiView>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single(v => ReferenceEquals(Field(v, "surface"), surface));
        var button = (Button)Field(((IDictionary)Field(view, "widgets"))["cast"], "Button");
        Check(button.gameObject.activeInHierarchy && button.interactable, "Native cast button blocked");
        button.onClick.Invoke();
    }
    static void Update()
    {
        if (!EditorApplication.isPlaying) return;
        try
        {
            if (combatException != null) throw new Exception(combatException);
            if (EditorApplication.timeSinceStartup - started > 300) throw new Exception("Timed out phase " + phase);
            if (EditorApplication.timeSinceStartup - report > 20) { report = EditorApplication.timeSinceStartup; Debug.Log("[ArcDartUnity] Waiting entered=" + entered + " phase=" + phase); }
            if (!campaign && Eclipse.UI.TitleScreen.IsOpen)
            {
                var title = UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.TitleScreen>();
                if ((bool)Field(title, "splashing") || (string)Field(title, "currentPage") != "Home") return;
                typeof(Eclipse.UI.TitleScreen).GetMethod("BeginCampaign", Hidden).Invoke(title, null);
                var directory = SF2Paths.GetUserDataDirectory();
                Check(Eclipse.Saves.CampaignSaveSession.PreviewDirectory == null && directory.StartsWith(Application.persistentDataPath, StringComparison.OrdinalIgnoreCase) && Application.persistentDataPath.Contains("ArcDartUnity-"), "Profile not isolated");
                var profile = XmlUtils.OpenXMLDocument(SF2Paths.KKIDGPBOBNI(), "usersDefault.xml", XmlUtils.EBLFEPIOMOL.Normal, true, XmlCryptoUtils.NNLGALNDJCL());
                ((System.Xml.XmlElement)profile.SelectSingleNode("/Root/Warriors/Warrior[@ID='1']")).SetAttribute("Tutorial", "END");
                Directory.CreateDirectory(directory); XmlUtils.ONLDJNLKKAL(profile, Path.Combine(directory, Constants.OJMIJINKBPJ).Replace('\\', '/'));
                campaign = true; return;
            }
            if (!entered)
            {
                if (ModRuntime.Scripts == null || Module.GetInstance() == null) return;
                var screen = Module.GetInstance().GetCurrentScreenType(); if (screen != ScreenType.ModuleDojo && screen != ScreenType.ModuleMap) return;
                Check(!ModRuntime.Host.HasErrors, ModRuntime.Host.FormatReport()); Check(!ModRuntime.Scripts.HasErrors, "Startup script errors");
                Check(ModRuntime.Host.EnabledMods.Count(m => m.Id.Value != "core") == 1 && ModRuntime.Host.EnabledMods.Any(m => m.Id.Value == "example.arc-dart"), "Fixture must enable only Arc Dart");
                var encounter = ListSF.CHMCKGCDGCM(new FightIDS(ModRuntime.Scripts.Content.RuntimeFightId(DefinitionId.Parse("core:fights/zone_1/tournament/3"))));
                Check(encounter != null, "Core encounter missing"); entered = GameUtils.StartFight(encounter, false, null, true, false); return;
            }
            var fight = Fight.GetCurrentFight(); if (fight == null || fight.get_FightTimeInFrames() < 100) return;
            var player = fight.GetPlayerModel(); var enemy = fight.GetEnemyModel(); if (player == null || enemy == null) return;
            player.Parameters.UserControlled = false; enemy.Parameters.AiControlled = false;
            var frame = fight.get_FightTimeInFrames(); var elapsed = EditorApplication.timeSinceStartup - phaseAt;
            if (phase == 3 && Darts(fight).Contains(dart)) travel = Math.Max(travel, Math.Abs(dart.PLBNCDCFPML().GetX() - launchX));
            switch (phase)
            {
                case 0:
                    ModRuntime.Scripts.CallbackDiagnostics.Recording = true;
                    surface = Surface(); if (surface == null) return;
                    Check(surface.Read("status").Text == "Arc Dart ready", "Initial shipped HUD");
                    Check(player.Parameters.Ranged?.Name == "NoRanged", "Scenario accidentally relies on equipped ranged item: " + player.Parameters.Ranged?.Name);
                    Check(player.GetAvailableAnimations().Any(m => m.Name == "example.arc-dart:moves/cast"), "Owned cast not available");
                    var pos = player.PLBNCDCFPML(); var other = enemy.PLBNCDCFPML();
                    enemy.ShiftModelPosition(new Vector3f(pos.GetX() + 500 - other.GetX(), 0, 0), true);
                    before = enemy.KKMCHCNOHMB(); castFrame = frame; Click();
                    Check(Darts(fight).Length == 0, "Button spawned recursively"); Next(); break;
                case 1:
                    var children = Darts(fight); if (children.Length == 0 || children[0].GetCurrentAnimation()?.Name != Flight) return;
                    dart = children.Single(); launchX = dart.PLBNCDCFPML().GetX();
                    Check(dart is WeaponModel && dart.Parameters.Skeleton.SubType == "SkeletonMissile" && dart.Parameters.Weapon.SubType == "Shuriken", "Wrong native child/rig/item");
                    Check(dart.Parameters.Weapon.Name == "RANGED_C2_Z2_MONK_SHURIKEN", "Typed item not projected into child Weapon slot");
                    Check(dart.GetRenderObject().activeInHierarchy, "Projectile render object inactive");
                    Check(surface.Read("counts").Text == "Flights: 1 | Hits: 0", "Flight notification missing");
                    Check(surface.Read("status").Text.Contains("started") && !surface.Read("cast").Enabled, "Applied receipt/cooldown missing");
                    Check(Math.Abs(enemy.KKMCHCNOHMB() - before) < .00001, "Damage occurred before projectile travel");
                    fight.SetPaused(true); pauseFrame = frame; pauseX = launchX;
                    new GameObject("Arc Dart capture").AddComponent<ArcDartCapture>(); Next(); break;
                case 2:
                    if (elapsed < .5 || !captured) return;
                    Check(fight.get_FightTimeInFrames() == pauseFrame && Math.Abs(dart.PLBNCDCFPML().GetX() - pauseX) < .00001, "Paused projectile moved");
                    Check(Math.Abs(enemy.KKMCHCNOHMB() - before) < .00001 && Darts(fight).Length == 1, "Pause damaged enemy or removed child");
                    fight.SetPaused(false); Next(); break;
                case 3:
                    if (Darts(fight).Length > 0) return;
                    Check(enemy.KKMCHCNOHMB() < before, "Native flight made no contact damage");
                    Check(surface.Read("counts").Text == "Flights: 1 | Hits: 1", "Child damage not attributed to main fighter: " + surface.Read("counts").Text);
                    Check(travel > 100, "Projectile never travelled");
                    Debug.Log("[ArcDartUnity] Native hit: " + before + " -> " + enemy.KKMCHCNOHMB() + "; sampled travel=" + travel); Next(); break;
                case 4:
                    if (!surface.Read("cast").Enabled) return;
                    Check(frame >= castFrame + 181 && surface.Read("status").Text == "Arc Dart ready", "Cooldown elapsed too soon");
                    // Miss intentionally: the opponent is behind the cast-facing path
                    // after launch. Expiry must remove the child without contact.
                    pos = player.PLBNCDCFPML(); other = enemy.PLBNCDCFPML();
                    enemy.ShiftModelPosition(new Vector3f(pos.GetX() + 500 - other.GetX(), 0, 0), true);
                    Click(); Next(); break;
                case 5:
                    children = Darts(fight); if (children.Length == 0 || children[0].GetCurrentAnimation()?.Name != Flight) return;
                    dart = children.Single(); before = enemy.KKMCHCNOHMB();
                    enemy.ShiftModelPosition(new Vector3f(-1500, 0, 0), true);
                    castFrame = frame; Next(); break;
                case 6:
                    if (Darts(fight).Length > 0) return;
                    Check(frame > castFrame && frame - castFrame < 90, "Missed projectile expiry unbounded");
                    Check(Math.Abs(enemy.KKMCHCNOHMB() - before) < .00001, "Miss caused health damage");
                    Check(surface.Read("counts").Text == "Flights: 2 | Hits: 1", "Miss counted as hit");
                    typeof(Fight).GetMethod("HCNDAFDHACI", Hidden).Invoke(fight, new object[] { GameOverTypes.GAME_OVER_SURRENDER });
                    Check(surface.IsClosed && Darts(fight).Length == 0, "Surrender retained HUD/child"); Next(); break;
                case 7:
                    if (elapsed < .2) return;
                    Check(ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Count == 0, "Lua callback failures: " + string.Join("; ", ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Select(f => f.Error)));
                    File.WriteAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath), "arc-dart-result.txt"), "PASS: " + checks + " full-game native Arc Dart checks; typed owned cast/child flight, item/rig/rendering, native travel/contact damage/attribution, pause, hit deletion, miss expiry, cooldown and surrender HUD cleanup. Controls/AI/spacing and fresh post-tutorial profile controlled.");
                    Debug.Log("[ArcDartUnity] PASS: " + checks + " full-game checks"); Finish(0); break;
            }
        }
        catch (Exception error) { Debug.LogError("[ArcDartUnity] FAIL: " + error); Finish(1); }
    }
    static ModUiSurface Surface()
    {
        foreach (var context in (IEnumerable)Field(ModRuntime.Scripts, "_contexts"))
        {
            var scope = context.GetType().GetProperty("UiScope").GetValue(context) as ModUiScope;
            if (scope == null || scope.Owner.Value != "example.arc-dart") continue;
            foreach (ModUiSurface s in ((IDictionary)Field(scope, "surfaces")).Values) if (s.Id == "ability") return s;
        }
        return null;
    }
    public static void Captured() { captured = true; }
    static void Log(string message, string stack, LogType type) { if (entered && type == LogType.Exception && (stack.Contains("Fight.") || stack.Contains("Model.") || stack.Contains("Modding"))) combatException = message + "\n" + stack; }
    static void Finish(int code) { SessionState.SetBool(Active, false); EditorApplication.update -= Update; Application.logMessageReceived -= Log; EditorApplication.Exit(code); }
}
public sealed class ArcDartCapture : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame(); var texture = ScreenCapture.CaptureScreenshotAsTexture();
        File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Application.dataPath), "arc-dart-native.png"), texture.EncodeToPNG());
        UnityEngine.Object.Destroy(texture); UnityEngine.Object.Destroy(gameObject); ArcDartUnity.Captured();
    }
}
#endif
