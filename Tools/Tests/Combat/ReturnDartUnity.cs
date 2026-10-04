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
public static class ReturnDartUnity
{
    const string Active = "Eclipse.ReturnDartUnity.Active";
    const string Actor = "example.return-dart.dart", Flight = "example.return-dart:moves/flight";
    const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    static double started, report, phaseAt;
    static int phase, checks, castFrame, pauseFrame;
    static bool campaign, entered, captured;
    static Model dart;
    static Fight acceptedFight;
    static float before, launchX, pauseX, travel, peak, targetX;
    static bool reversed;
    static ModUiSurface surface;
    static string combatException;
    static readonly System.Collections.Generic.HashSet<string> sourceEvents = new System.Collections.Generic.HashSet<string>();

    static ReturnDartUnity()
    {
        if (!SessionState.GetBool(Active, false)) return;
        started = EditorApplication.timeSinceStartup;
        EditorApplication.update += Update;
        Application.logMessageReceived += Log;
    }
    public static void Run()
    {
        var root = Path.GetDirectoryName(Application.dataPath);
        if (!File.Exists(Path.Combine(root, "return-dart-fixture.marker"))) throw new Exception("Requires isolated projectile fixture.");
        Environment.SetEnvironmentVariable("ECLIPSE_MODS_ROOT", Path.Combine(root, "return-projectile-mods"));
        var probeScript = Path.Combine(root,"return-projectile-mods/example.return-dart/scripts/main.lua");
        string shippedScript = File.ReadAllText(probeScript);
        int patchAt = shippedScript.IndexOf("for _, fight in ipairs", StringComparison.Ordinal);
        if (patchAt < 0) throw new Exception("Shipped example patch block missing.");
        File.WriteAllText(probeScript, shippedScript.Substring(0,patchAt) + @"
-- Native acceptance-only provenance probe; the runner copies the shipped mod afresh.
local known = {}
local function inspect(_, fighter, event)
    local attack = event.attack
    if not attack or attack.kind ~= 'projectile' then return end
    assert(attack.projectile_owner == sf2.mod.id and attack.model_name == sf2.mod.id .. '.dart')
    assert(attack.animation_name == sf2.mod.id .. ':moves/flight')
    assert(known[attack.projectile_id], 'Contact ID does not match a previously observed live child')
    assert(attack.point and attack.point.x == attack.point.x and attack.point.y == attack.point.y and attack.point.z == attack.point.z)
    sf2.log.info('SOURCE-PROBE:' .. event.type .. ':' .. fighter.side)
    -- A later callback/side must still receive the original copied values.
    attack.model_name = 'mutated'; attack.point.x = 0/0
end
local probe = sf2.behaviors.register {
    id = 'source_probe', on_tick = function(_, fighter)
        local list = fighter:projectiles()
        if list then for _, child in ipairs(list) do local view = child:snapshot(); if view then known[view.id] = true end end end
    end,
    on_hit_post_crit = inspect, on_post_hit = inspect,
    on_damage_dealing = inspect, on_damage_resolving = inspect,
    on_damage_dealt = inspect, on_damage_received = inspect,
}
local probe_rule = sf2.rules.behavior { id = 'source_probe', behavior = probe, target = sf2.rules.ALL }
for _, fight in ipairs { 'core:fights/zone_1/tournament/3', 'core:fights/zone_1/tournament_eclipsemode/3' } do
    sf2.fights.patch { target = fight, append_rules = {rule, probe_rule} }
end
");
        var args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "-returnDartAcceptanceProductName");
        if (index < 0 || index + 1 >= args.Length || !System.Text.RegularExpressions.Regex.IsMatch(args[index + 1], "^ReturnDartUnity-[0-9a-f]{32}$"))
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
            if (EditorApplication.timeSinceStartup - report > 20) { report = EditorApplication.timeSinceStartup; Debug.Log("[ReturnDartUnity] Waiting entered=" + entered + " phase=" + phase); }
            if (!campaign && Eclipse.UI.TitleScreen.IsOpen)
            {
                var title = UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.TitleScreen>();
                if ((bool)Field(title, "splashing") || (string)Field(title, "currentPage") != "Home") return;
                typeof(Eclipse.UI.TitleScreen).GetMethod("BeginCampaign", Hidden).Invoke(title, null);
                var directory = SF2Paths.GetUserDataDirectory();
                Check(Eclipse.Saves.CampaignSaveSession.PreviewDirectory == null && directory.StartsWith(Application.persistentDataPath, StringComparison.OrdinalIgnoreCase) && Application.persistentDataPath.Contains("ReturnDartUnity-"), "Profile not isolated");
                var profile = XmlUtils.OpenXMLDocument(SF2Paths.KKIDGPBOBNI(), "usersDefault.xml", XmlUtils.EBLFEPIOMOL.Normal, true, XmlCryptoUtils.NNLGALNDJCL());
                ((System.Xml.XmlElement)profile.SelectSingleNode("/Root/Warriors/Warrior[@ID='1']")).SetAttribute("Tutorial", "END");
                Directory.CreateDirectory(directory); XmlUtils.ONLDJNLKKAL(profile, Path.Combine(directory, Constants.OJMIJINKBPJ).Replace('\\', '/'));
                campaign = true; return;
            }
            if (!entered)
            {
                if (ModRuntime.Scripts == null || Module.GetInstance() == null) return;
                var screen = Module.GetInstance().GetCurrentScreenType(); if (screen != ScreenType.ModuleDojo && screen != ScreenType.ModuleMap) return;
                Check(!ModRuntime.Host.HasErrors, ModRuntime.Host.FormatReport()); Check(!ModRuntime.Scripts.HasErrors, ModRuntime.Scripts.FormatReport());
                Check(ModRuntime.Host.EnabledMods.Count(m => m.Id.Value != "core") == 1 && ModRuntime.Host.EnabledMods.Any(m => m.Id.Value == "example.return-dart"), "Fixture must enable only Return Dart");
                var encounter = ListSF.CHMCKGCDGCM(new FightIDS(ModRuntime.Scripts.Content.RuntimeFightId(DefinitionId.Parse("core:fights/zone_1/tournament/3"))));
                Check(encounter != null, "Core encounter missing"); entered = GameUtils.StartFight(encounter, false, null, true, false); return;
            }
            if (phase == 9)
            {
                if (EditorApplication.timeSinceStartup-phaseAt < .5) return;
                Check(((IDictionary)Field(acceptedFight,"_eclipseProjectiles")).Count == 0, "Surrender retained owned references");
                Check(dart.GetRenderObject() == null || !dart.GetRenderObject().activeInHierarchy, "Surrender retained live child rendering");
                Check(ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Count == 0, "Lua callback failures");
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath), "return-dart-result.txt"), "PASS: " + checks + " full-game native Return Dart checks; typed owned cast/child flight, item/rig/rendering, native travel/contact damage/attribution, pause, hit deletion, Lua out-and-return trajectory, cooldown and live-child surrender cleanup. Controls/AI/spacing and fresh post-tutorial profile controlled.");
                Debug.Log("[ReturnDartUnity] PASS: " + checks + " full-game checks"); Finish(0); return;
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
                    Check(surface.Read("status").Text == "Return Dart ready", "Initial shipped HUD");
                    Check(player.Parameters.Ranged?.Name == "NoRanged", "Scenario accidentally relies on equipped ranged item: " + player.Parameters.Ranged?.Name);
                    Check(player.GetAvailableAnimations().Any(m => m.Name == "example.return-dart:moves/cast"), "Owned cast not available");
                    var pos = player.PLBNCDCFPML(); var other = enemy.PLBNCDCFPML();
                    enemy.ShiftModelPosition(new Vector3f(pos.GetX() + 900 - other.GetX(), 0, 0), true);
                    before = enemy.KKMCHCNOHMB(); castFrame = frame; Click();
                    Check(Darts(fight).Length == 0, "Button spawned recursively"); Next(); break;
                case 1:
                    var children = Darts(fight); if (children.Length == 0 || children[0].GetCurrentAnimation()?.Name != Flight) return;
                    dart = children.Single(); launchX = dart.PLBNCDCFPML().GetX();
                    targetX = launchX + 220;
                    enemy.ShiftModelPosition(new Vector3f(targetX-enemy.PLBNCDCFPML().GetX(),0,0),true);
                    Check(dart is WeaponModel && dart.Parameters.Skeleton.SubType == "SkeletonMissile" && dart.Parameters.Weapon.SubType == "Shuriken", "Wrong native child/rig/item");
                    Check(dart.Parameters.Weapon.Name == "RANGED_C2_Z2_MONK_SHURIKEN", "Typed item not projected into child Weapon slot");
                    Check(dart.GetRenderObject().activeInHierarchy, "Projectile render object inactive");
                    Check(surface.Read("counts").Text == "Flights: 1 | Hits: 0", "Flight notification missing");
                    Check(surface.Read("status").Text.Contains("started") && !surface.Read("cast").Enabled, "Applied receipt/cooldown missing");
                    Check(Math.Abs(enemy.KKMCHCNOHMB() - before) < .00001, "Damage occurred before projectile travel");
                    fight.SetPaused(true); pauseFrame = frame; pauseX = launchX;
                    new GameObject("Return Dart capture").AddComponent<ReturnDartCapture>(); Next(); break;
                case 2:
                    if (elapsed < .5 || !captured) return;
                    Check(fight.get_FightTimeInFrames() == pauseFrame && Math.Abs(dart.PLBNCDCFPML().GetX() - pauseX) < .00001, "Paused projectile moved");
                    Check(Math.Abs(enemy.KKMCHCNOHMB() - before) < .00001 && Darts(fight).Length == 1, "Pause damaged enemy or removed child");
                    fight.SetPaused(false); Next(); break;
                case 3:
                    if (Darts(fight).Length > 0) { enemy.ShiftModelPosition(new Vector3f(targetX-enemy.PLBNCDCFPML().GetX(),0,0),true); return; }
                    Debug.Log("[ReturnDartUnity] First flight finished: travel="+travel+"; launchX="+launchX+"; frame="+frame+"; cast="+castFrame+"; enemyX="+enemy.PLBNCDCFPML().GetX()+"; failures="+string.Join("; ",ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Select(f=>f.Error)));
                    Check(enemy.KKMCHCNOHMB() < before, "Native flight made no contact damage");
                    Check(surface.Read("counts").Text == "Flights: 1 | Hits: 1", "Child damage not attributed to main fighter: " + surface.Read("counts").Text);
                    Check(travel > 30, "Projectile never travelled");
                    foreach (string notification in new[]{"HitPostCrit:player","HitPostCrit:opponent","PostHit:player","PostHit:opponent","DamageDealing:player","DamageResolving:opponent","DamageDealt:player","DamageReceived:opponent"})
                        Check(sourceEvents.Contains(notification),"Missing copied native attack source: "+notification);
                    Debug.Log("[ReturnDartUnity] Native hit: " + before + " -> " + enemy.KKMCHCNOHMB() + "; sampled travel=" + travel); Next(); break;
                case 4:
                    if (!surface.Read("cast").Enabled) return;
                    Check(frame >= castFrame + 181 && surface.Read("status").Text == "Return Dart ready", "Cooldown elapsed too soon");
                    // Miss intentionally: the opponent is behind the cast-facing path
                    // after launch. Expiry must remove the child without contact.
                    pos = player.PLBNCDCFPML(); other = enemy.PLBNCDCFPML();
                    enemy.ShiftModelPosition(new Vector3f(pos.GetX() + 900 - other.GetX(), 0, 0), true);
                    Click(); Next(); break;
                case 5:
                    children = Darts(fight); if (children.Length == 0 || children[0].GetCurrentAnimation()?.Name != Flight) return;
                    dart = children.Single(); before = enemy.KKMCHCNOHMB();
                    enemy.ShiftModelPosition(new Vector3f(-1500, 0, 0), true);
                    castFrame = frame; launchX = dart.PLBNCDCFPML().GetX(); peak = 0; reversed = false; Next(); break;
                case 6:
                    if (Darts(fight).Length > 0) { float x = dart.PLBNCDCFPML().GetX(); float distance = Math.Abs(x-launchX); peak = Math.Max(peak, distance); if (peak - distance > 30) reversed = true; return; }
                    Check(reversed && peak > 150, "Lua trajectory did not travel out and return");
                    Check(frame > castFrame && frame - castFrame < 120, "Returned projectile cleanup unbounded");
                    Check(Math.Abs(enemy.KKMCHCNOHMB() - before) < .00001, "Miss caused health damage");
                    Check(surface.Read("counts").Text == "Flights: 2 | Hits: 1", "Miss counted as hit");
                    Debug.Log("[ReturnDartUnity] Miss turned and returned: outward=" + peak);
                    Next(); break;
                case 7:
                    if (!surface.Read("cast").Enabled) return;
                    pos = player.PLBNCDCFPML(); other = enemy.PLBNCDCFPML();
                    enemy.ShiftModelPosition(new Vector3f(pos.GetX()+500-other.GetX(),0,0),true);
                    Click(); Next(); break;
                case 8:
                    if (Darts(fight).Length == 0) return;
                    dart = Darts(fight).Single(); acceptedFight = fight;
                    typeof(Fight).GetMethod("HCNDAFDHACI", Hidden).Invoke(fight, new object[] { GameOverTypes.GAME_OVER_SURRENDER });
                    Check(surface.IsClosed, "Surrender retained HUD"); Next(); break;

            }
        }
        catch (Exception error) { Debug.LogError("[ReturnDartUnity] FAIL: " + error); Finish(1); }
    }
    static ModUiSurface Surface()
    {
        foreach (var context in (IEnumerable)Field(ModRuntime.Scripts, "_contexts"))
        {
            var scope = context.GetType().GetProperty("UiScope").GetValue(context) as ModUiScope;
            if (scope == null || scope.Owner.Value != "example.return-dart") continue;
            foreach (ModUiSurface s in ((IDictionary)Field(scope, "surfaces")).Values) if (s.Id == "ability") return s;
        }
        return null;
    }
    public static void Captured() { captured = true; }
    static void Log(string message, string stack, LogType type) { int probe = message.IndexOf("SOURCE-PROBE:",StringComparison.Ordinal); if(probe>=0)sourceEvents.Add(message.Substring(probe+13).Trim()); if (entered && type == LogType.Exception && (stack.Contains("Fight.") || stack.Contains("Model.") || stack.Contains("Modding"))) combatException = message + "\n" + stack; }
    static void Finish(int code) { SessionState.SetBool(Active, false); EditorApplication.update -= Update; Application.logMessageReceived -= Log; EditorApplication.Exit(code); }
}
public sealed class ReturnDartCapture : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame(); var texture = ScreenCapture.CaptureScreenshotAsTexture();
        File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Application.dataPath), "return-dart-native.png"), texture.EncodeToPNG());
        UnityEngine.Object.Destroy(texture); UnityEngine.Object.Destroy(gameObject); ReturnDartUnity.Captured();
    }
}
#endif
