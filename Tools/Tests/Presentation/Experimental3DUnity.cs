#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Eclipse.Modding;
using Eclipse.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class Experimental3DUnity
{
    const string Active = "Eclipse.Experimental3DUnity.Active";
    const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    static double started, report, phaseAt;
    static int phase, checks, captured, frame;
    static bool campaign, entered, optionsShown, spacingSet;
    static float[] pose;
    static float health;
    static int mask;
    static Button toggle;
    static string renderException;
    static Experimental3DUnity()
    {
        if (!SessionState.GetBool(Active, false)) return;
        started = EditorApplication.timeSinceStartup;
        EditorApplication.update += Update;
        Application.logMessageReceived += Log;
    }
    public static void Run()
    {
        var root = Path.GetDirectoryName(Application.dataPath);
        if (!File.Exists(Path.Combine(root, "experimental-3d-fixture.marker"))) throw new Exception("Requires isolated rendering fixture.");
        Environment.SetEnvironmentVariable("ECLIPSE_MODS_ROOT", Path.Combine(root, "render-mods"));
        var args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, "-experimental3DAcceptanceProductName");
        if (index < 0 || index + 1 >= args.Length || !System.Text.RegularExpressions.Regex.IsMatch(args[index + 1], "^Experimental3DUnity-[0-9a-f]{32}$")) throw new Exception("Use the acceptance runner.");
        PlayerSettings.companyName = "EclipseAcceptance"; PlayerSettings.productName = args[index + 1];
        SessionState.SetBool(Active, true);
        EditorSceneManager.OpenScene("Assets/src/GUI/Scenes/GameLoaderScene/GameLoader.unity");
        var view = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        view.Show(); view.Focus(); EditorApplication.EnterPlaymode();
    }
    static object Field(object value, string name) => value.GetType().GetField(name, Hidden | BindingFlags.Public).GetValue(value);
    static void Check(bool value, string message) { checks++; if (!value) throw new Exception(message); }
    static void Next() { phase++; phaseAt = EditorApplication.timeSinceStartup; }
    static float[] Pose(Model model) => model.GetModelObject().NAMKCLGOPDD().SelectMany(n => new[] { n.GetStart().GetX(), n.GetStart().GetY(), n.GetStart().GetZ(), n.GetEnd().GetX(), n.GetEnd().GetY(), n.GetEnd().GetZ() }).ToArray();
    static void Capture(string filename)
    { var capture = new GameObject("3D acceptance capture").AddComponent<Experimental3DCapture>(); capture.Filename = filename; }
    static void Update()
    {
        if (!EditorApplication.isPlaying) return;
        try
        {
            if (renderException != null) throw new Exception(renderException);
            if (EditorApplication.timeSinceStartup - started > 300) throw new Exception("Timeout phase " + phase);
            if (EditorApplication.timeSinceStartup - report > 20) { report = EditorApplication.timeSinceStartup; Debug.Log("[Experimental3DUnity] Waiting entered=" + entered + " phase=" + phase); }
            if (!campaign && Eclipse.UI.TitleScreen.IsOpen)
            {
                var title = UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.TitleScreen>();
                if ((bool)Field(title, "splashing") || (string)Field(title, "currentPage") != "Home") return;
                Check(!SF2DisplayFrameRate.Experimental3DEnabled, "Default must be off");
                typeof(Eclipse.UI.TitleScreen).GetMethod("BeginCampaign", Hidden).Invoke(title, null);
                var directory = SF2Paths.GetUserDataDirectory();
                Check(directory.StartsWith(Application.persistentDataPath, StringComparison.OrdinalIgnoreCase) && Application.persistentDataPath.Contains("Experimental3DUnity-"), "Profile not isolated");
                var profile = XmlUtils.OpenXMLDocument(SF2Paths.KKIDGPBOBNI(), "usersDefault.xml", XmlUtils.EBLFEPIOMOL.Normal, true, XmlCryptoUtils.NNLGALNDJCL());
                ((System.Xml.XmlElement)profile.SelectSingleNode("/Root/Warriors/Warrior[@ID='1']")).SetAttribute("Tutorial", "END");
                Directory.CreateDirectory(directory); XmlUtils.ONLDJNLKKAL(profile, Path.Combine(directory, Constants.OJMIJINKBPJ).Replace('\\', '/'));
                campaign = true; return;
            }
            if (!entered)
            {
                if (ModRuntime.Scripts == null || Module.GetInstance() == null) return;
                var screen = Module.GetInstance().GetCurrentScreenType(); if (screen != ScreenType.ModuleDojo && screen != ScreenType.ModuleMap) return;
                Check(!ModRuntime.Host.HasErrors, ModRuntime.Host.FormatReport());
                var encounter = ListSF.CHMCKGCDGCM(new FightIDS(ModRuntime.Scripts.Content.RuntimeFightId(DefinitionId.Parse("core:fights/zone_1/tournament/3"))));
                Check(encounter != null, "Core encounter missing"); entered = GameUtils.StartFight(encounter, false, null, true, false); return;
            }
            var fight = Fight.GetCurrentFight(); if (fight == null || fight.get_FightTimeInFrames() < 100) return;
            var player = fight.GetPlayerModel(); var enemy = fight.GetEnemyModel(); if (player == null || enemy == null) return;
            player.Parameters.UserControlled = false; enemy.Parameters.AiControlled = false;
            double elapsed = EditorApplication.timeSinceStartup - phaseAt;
            var volumes = UnityEngine.Object.FindObjectsByType<FighterVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var perspective = UnityEngine.Object.FindObjectsByType<UnityEngine.Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(c => c.name == "Eclipse experimental perspective camera");
            switch (phase)
            {
                case 0:
                    if (!spacingSet)
                    {
                        var position = player.PLBNCDCFPML(); var other = enemy.PLBNCDCFPML();
                        enemy.ShiftModelPosition(new Vector3f(position.GetX() + 500 - other.GetX(), 0, 0), true);
                        spacingSet = true; return;
                    }
                    if (fight.get_FightTimeInFrames() < 180) return;
                    fight.SetPaused(true); frame = fight.get_FightTimeInFrames(); pose = Pose(player); health = enemy.KKMCHCNOHMB(); mask = UnityEngine.Camera.main.cullingMask;
                    Capture("fighters-orthographic.png"); Next(); break;
                case 1:
                    if (captured < 1) return;
                    if (!optionsShown) { Eclipse.UI.TitleScreen.ShowOptions(null); optionsShown = true; phaseAt = EditorApplication.timeSinceStartup; return; }
                    if (elapsed < 1) return;
                    var title = UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.TitleScreen>();
                    var label = title.GetComponentsInChildren<Text>().Single(t => t.text == "3D fighters (experimental)");
                    Check(label.gameObject.activeInHierarchy, "Settings caption missing");
                    toggle = title.GetComponentsInChildren<Button>().Single(b => b.name == "Experimental 3D fighters");
                    Check(toggle.GetComponentInChildren<Text>().text.StartsWith("Off"), "Settings toggle not off");
                    toggle.onClick.Invoke();
                    Check(SF2DisplayFrameRate.Experimental3DEnabled && PlayerPrefs.GetInt("Eclipse.ExperimentalFighter3D") == 1, "Settings toggle did not persist");
                    Check(toggle.GetComponentInChildren<Text>().text.StartsWith("On"), "Settings label not refreshed"); Next(); break;
                case 2:
                    if (elapsed < .7 || perspective == null) return;
                    Check(Quaternion.Angle(perspective.transform.rotation,UnityEngine.Camera.main.transform.rotation)<.01f,"Camera changed original composition");
                    Check(!perspective.orthographic && perspective.enabled && perspective.cullingMask == 1 << ExperimentalFighterCamera.Layer, "Perspective camera missing");
                    Check(UnityEngine.Object.FindObjectsByType<ExperimentalFighterCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 1, "Menu preview acquired a fight perspective pass");
                    Check(volumes.Count(v => v.gameObject.activeInHierarchy) >= 2, "Solid renderers missing");
                    Check(ProceduralFighterBody.IsBodyOutline("BODY_WOMAN-Capsule-1_EArm_1") && !ProceduralFighterBody.IsBodyOutline("WEAPON_KNIVES-Capsule-Edge17_1"), "Body provenance includes equipment or misses female rig");
                    Check(!volumes.Any(v => v.gameObject.activeInHierarchy && ProceduralFighterBody.IsBodyOutline(v.transform.parent.name)), "Recovered body capsules still cover lofted skin");
                    Check(volumes.All(v => v.transform.IsChildOf(player.GetRenderObject().transform.parent)), "Menu preview leaked 3D volumes into fight");
                    Check(volumes.All(v => v.GetComponent<MeshRenderer>().sharedMaterial.shader.isSupported), "Volume shader unsupported");
                    Check(volumes.Any(v => v.GetComponent<MeshFilter>().sharedMesh.bounds.size.z > 10), "No actual geometry depth");
                    Check(player._MeshRender.get_Base().Vertices.Any(v => Math.Abs(v.z) > .01), "Native body depth discarded");
                    var nativeVertices = player._MeshRender.get_Base().Vertices;
                    var low = player._MeshRender.transform.TransformPoint(nativeVertices.OrderBy(v => v.y).First());
                    var high = player._MeshRender.transform.TransformPoint(nativeVertices.OrderBy(v => v.y).Last());
                    float originalOrientation = UnityEngine.Camera.main.WorldToViewportPoint(high).y - UnityEngine.Camera.main.WorldToViewportPoint(low).y;
                    float depthOrientation = perspective.WorldToViewportPoint(high).y - perspective.WorldToViewportPoint(low).y;
                    Check(originalOrientation * depthOrientation > 0, "Perspective projection inverted fighter orientation");
                    Check(volumes.All(v => v.GetComponent<MeshFilter>().sharedMesh.normals.Length == v.GetComponent<MeshFilter>().sharedMesh.vertexCount), "Missing lighting normals");
                    Check(Pose(player).SequenceEqual(pose) && enemy.KKMCHCNOHMB() == health && fight.get_FightTimeInFrames() == frame, "Renderer mutated native simulation");
                    Debug.Log("[Experimental3DUnity] Volumes=" + volumes.Length + "; native depth=" + player._MeshRender.get_Base().Vertices.Min(v => v.z) + ".." + player._MeshRender.get_Base().Vertices.Max(v => v.z) + "; perspective=" + perspective.transform.position + "; source=" + UnityEngine.Camera.main.transform.position + " size=" + UnityEngine.Camera.main.orthographicSize);
                    File.WriteAllLines(Path.Combine(Path.GetDirectoryName(Application.dataPath),"procedural3d-parts.txt"),
                        player._MeshRender.get_Base().FigureNames.Distinct().Concat(enemy._MeshRender.get_Base().FigureNames.Distinct()).Concat(new[]{"ACTIVE VOLUME PARENTS"}).Concat(volumes.Where(v=>v.gameObject.activeInHierarchy).Select(v=>v.transform.parent.name)));
                    Capture("experimental-3d-settings.png"); Next(); break;
                case 3:
                    if (captured < 2) return;
                    title = UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.TitleScreen>();
                    title.GetComponentsInChildren<Button>().Single(b => b.name == "Back").onClick.Invoke(); Next(); break;
                case 4:
                    if (elapsed < .5 || Eclipse.UI.TitleScreen.IsOpen) return;
                    Capture("fighters-perspective-3d.png"); Next(); break;
                case 5:
                    if (captured < 3) return;
                    if(FighterVolume.ReviewExposure == 1) { FighterVolume.ReviewExposure = 4; Capture("fighters-3d-bright-inspection.png"); return; }
                    if(captured < 4)return;
                    FighterVolume.ReviewExposure = 1;
                    SF2DisplayFrameRate.ToggleExperimental3D(); Next(); break;
                case 6:
                    if (elapsed < .4) return;
                    Check(!perspective.enabled && volumes.All(v => !v.gameObject.activeSelf), "Off did not disable volumes/camera");
                    Check(UnityEngine.Camera.main.cullingMask == mask && player._MeshRender.get_Base().Vertices.All(v => v.z == 0), "Original rendering not restored");
                    Check(player._MeshRender.GetComponent<MeshRenderer>().enabled, "Original body mesh hidden");
                    Check(Pose(player).SequenceEqual(pose) && fight.get_FightTimeInFrames() == frame, "Toggle changed native pose/time");
                    SF2DisplayFrameRate.ToggleExperimental3D(); fight.SetPaused(false); Next(); break;
                case 7:
                    if (fight.get_FightTimeInFrames() < frame + 90) return;
                    Check(perspective.enabled && !Pose(player).SequenceEqual(pose), "Animation did not resume in 3D");
                    fight.SetPaused(true); Capture("fighters-3d-animation.png"); Next(); break;
                case 8:
                    if (captured < 5) return;
                    var bodies=UnityEngine.Object.FindObjectsByType<ProceduralFighterBody>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(b=>b.Ready).ToArray();
                    Check(bodies.Length == 2,"Standard fighter bodies were not reconstructed");
                    Check(bodies.All(b=>b.GetComponentsInChildren<FighterVolume>().Any(v=>v.GetComponent<MeshFilter>().sharedMesh.vertexCount>3000)),"Dedicated continuous body skins missing");
                    var panelTest=FighterVolume.Create(player.GetRenderObject().transform);
                    var quad=new[]{new Vector3(0,0,0),new Vector3(50,0,0),new Vector3(50,50,0),new Vector3(0,50,0)};
                    var quadFaces=new[]{0,1,2,0,2,3};
                    panelTest.Surface(quad,quadFaces,Color.black,new[]{"Cloth-Triangle1","Cloth-Triangle2"});
                    var panelMesh=panelTest.GetComponent<MeshFilter>().sharedMesh;
                    Check(panelMesh.vertexCount==50&&panelMesh.triangles.Length==288,"Cloth did not share subdivision edges or emitted internal walls");
                    Check(panelMesh.vertices.Max(v=>v.z)-panelMesh.vertices.Min(v=>v.z)>10,"Cloth interior stayed a flat slab");
                    panelTest.Surface(quad,quadFaces,Color.black,new[]{"WEAPON-Triangle1","WEAPON-Triangle2"},true);
                    // The cache key includes native topology/body selection; use a new
                    // immutable topology array when the owning native mesh changes.
                    panelTest.Surface(quad,(int[])quadFaces.Clone(),Color.black,new[]{"WEAPON-Triangle1","WEAPON-Triangle2"},true);
                    Check(panelMesh.vertexCount==24&&panelMesh.triangles.Length==36,"Rigid panel lost sharp boundary normals or emitted internal walls");
                    UnityEngine.Object.Destroy(panelTest.gameObject);
                    // Exercise geometry at coincident endpoints as well as live native poses.
                    var test = FighterVolume.Create(player.GetRenderObject().transform);
                    test.Capsule(Vector3.zero, Vector3.zero, 25f, Color.black);
                    var mesh = test.GetComponent<MeshFilter>().sharedMesh;
                    Check(mesh.bounds.size.x > 24 && mesh.bounds.size.z > 17 && mesh.vertices.All(v => !float.IsNaN(v.x)), "Degenerate fallback is not a finite ellipsoid");
                    UnityEngine.Object.Destroy(test.gameObject);
                    SF2DisplayFrameRate.ResetRenderSettings(); Check(!SF2DisplayFrameRate.Experimental3DEnabled && !PlayerPrefs.HasKey("Eclipse.ExperimentalFighter3D"), "Reset defaults did not clear 3D");
                    var driver = UnityEngine.Object.FindFirstObjectByType<ExperimentalFighterCamera>(); UnityEngine.Object.Destroy(driver.gameObject); Next(); break;
                case 9:
                    if (elapsed < .3) return;
                    Check(perspective == null && UnityEngine.Camera.main.cullingMask == mask, "Viewer teardown leaked camera/mask");
                    File.WriteAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath), "experimental-3d-result.txt"), "PASS: " + checks + " full-game experimental 3D checks; native depth, solid meshes/normals, perspective pass, real Settings toggle, persistence/defaults, live restore, pause/pose isolation, resumed animation and camera teardown. Controlled Campaign Tournament 3; not all arenas, equipment, physical inputs or exported players.");
                    Debug.Log("[Experimental3DUnity] PASS: " + checks); Finish(0); break;
            }
        }
        catch (Exception error) { Debug.LogError("[Experimental3DUnity] FAIL: " + error); Finish(1); }
    }
    public static void Captured() { captured++; }
    static void Log(string message, string stack, LogType type)
    { if (entered && type == LogType.Exception && (stack.Contains("Fight.") || stack.Contains("Model.") || stack.Contains("Rendering"))) renderException = message + "\n" + stack; }
    static void Finish(int code) { SessionState.SetBool(Active, false); EditorApplication.update -= Update; Application.logMessageReceived -= Log; EditorApplication.Exit(code); }
}
public sealed class Experimental3DCapture : MonoBehaviour
{
    public string Filename;
    System.Collections.IEnumerator Start()
    {
        yield return new WaitForEndOfFrame(); var texture = ScreenCapture.CaptureScreenshotAsTexture();
        File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Application.dataPath), Filename), texture.EncodeToPNG());
        UnityEngine.Object.Destroy(texture); UnityEngine.Object.Destroy(gameObject); Experimental3DUnity.Captured();
    }
}
#endif
