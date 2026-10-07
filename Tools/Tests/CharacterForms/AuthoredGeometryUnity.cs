#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using Eclipse.Modding;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class AuthoredGeometryUnity
{
    const string Active = "Eclipse.AuthoredGeometryUnity.Active";
    const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    static double started, report;
    static bool campaign, entered;
    static int checks;
    static string failure;
    static string Root => Path.GetDirectoryName(Application.dataPath);
    static AuthoredGeometryUnity()
    {
        if (!SessionState.GetBool(Active, false)) return;
        started = EditorApplication.timeSinceStartup; EditorApplication.update += Update; Application.logMessageReceived += Log;
    }
    public static void Run()
    {
        if (!File.Exists(Path.Combine(Root, "authored-geometry-fixture.marker"))) throw new Exception("Requires isolated authored geometry fixture.");
        Environment.SetEnvironmentVariable("ECLIPSE_MODS_ROOT", Path.Combine(Root, "geometry-empty-mods"));
        var args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, "-geometryAcceptanceProductName");
        if (index < 0 || index + 1 >= args.Length || !System.Text.RegularExpressions.Regex.IsMatch(args[index + 1], "^AuthoredGeometryUnity-[0-9a-f]{32}$")) throw new Exception("Requires isolated runner product.");
        PlayerSettings.companyName = "EclipseAcceptance"; PlayerSettings.productName = args[index + 1];
        SessionState.SetBool(Active, true); EditorSceneManager.OpenScene("Assets/src/GUI/Scenes/GameLoaderScene/GameLoader.unity");
        var view = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView")); view.Show(); view.Focus(); EditorApplication.EnterPlaymode();
    }
    static void Check(bool value, string why) { checks++; if (!value) throw new Exception(why); }
    static void Update()
    {
        try
        {
            if (failure != null) throw new Exception(failure);
            if (EditorApplication.timeSinceStartup - started > 300) throw new Exception("Timed out authored geometry acceptance.");
            if (EditorApplication.timeSinceStartup - report > 20) { report = EditorApplication.timeSinceStartup; Debug.Log("[AuthoredGeometryUnity] Waiting campaign=" + campaign + " entered=" + entered); }
            if (!EditorApplication.isPlaying) return;
            if (!campaign)
            {
                var title = UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.TitleScreen>(); if (!title) return;
                if ((bool)title.GetType().GetField("splashing", Hidden).GetValue(title) || (string)title.GetType().GetField("currentPage", Hidden).GetValue(title) != "Home") return;
                title.GetType().GetMethod("BeginCampaign", Hidden).Invoke(title, null);
                var directory = SF2Paths.GetUserDataDirectory();
                Check(directory.StartsWith(Application.persistentDataPath, StringComparison.OrdinalIgnoreCase) && Application.persistentDataPath.Contains("AuthoredGeometryUnity-"), "Profile not isolated");
                var profile = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), "usersDefault.xml", XmlUtils.XmlSourceMode.Normal, true, XmlCryptoUtils.GetIsEncryptionEnabled());
                ((XmlElement)profile.SelectSingleNode("/Root/Warriors/Warrior[@ID='1']")).SetAttribute("Tutorial", "END");
                Directory.CreateDirectory(directory); XmlUtils.SaveDocumentWithHash(profile, Path.Combine(directory, Constants.UsersFileName).Replace('\\', '/')); campaign = true; return;
            }
            if (!entered)
            {
                if (ModRuntime.Scripts == null || Module.GetInstance() == null) return;
                var screen = Module.GetInstance().GetCurrentScreenType(); if (screen != ScreenType.ModuleDojo && screen != ScreenType.ModuleMap) return;
                Check(!ModRuntime.Host.HasErrors, ModRuntime.Host.FormatReport());
                var encounter = ListSF.GetFightById(new FightIDS(ModRuntime.Scripts.Content.RuntimeFightId(DefinitionId.Parse("core:fights/zone_1/tournament/3"))));
                Check(encounter != null, "Core encounter missing"); entered = GameUtils.StartFight(encounter, false, null, true, false); return;
            }
            var fight = Fight.GetCurrentFight(); var player = fight?.GetPlayerModel();
            if (player == null || fight.get_FightTimeInFrames() < 100) return;
            fight.SetPaused(true); ValidateNative(player);
            File.WriteAllText(Path.Combine(Root, "authored-geometry-result.txt"), "PASS: " + checks + " full-game native preparation checks; cached authored body/skin, real model bindings, hidden ownership/disposal, field-specific rejection before native parsing and unchanged main fighter. Cache injection is a controlled loader fixture; no public mod, authored animation/contact or visual acceptance implied.");
            Debug.Log("[AuthoredGeometryUnity] PASS: " + checks); Finish(0);
        }
        catch (Exception e) { Debug.LogError("[AuthoredGeometryUnity] FAIL: " + e); Finish(1); }
    }
    static void ValidateNative(Model player)
    {
        const string body = "acceptance.character:models/body.xml", skin = "acceptance.character:models/skin.xml";
        var documents = (Dictionary<string, XmlDocument>)typeof(ModelLoader.CacheModelDocuments).GetField("documents", Hidden).GetValue(ModelLoader.DocumentCache);
        var original = ModelLoader.DocumentCache.GetDocument(SF2Paths.GetModelsPath(), player.Parameters.ModelDocuments[0]);
        documents[body] = (XmlDocument)original.CloneNode(true);
        var overlay = new XmlDocument(); overlay.LoadXml("<Scene><Nodes><AuthoredTip Type='MacroNode' NodesCount='1' ChildNode1='NNeck' LCC1='1'/></Nodes><Edges/><Figures><AuthoredTriangle Type='Triangle' Node1='NChest' Node2='NShoulder_1' Node3='AuthoredTip'/></Figures></Scene>");
        documents[skin] = overlay;
        var parameters = new ModelParameters(player.Parameters);
        parameters.EclipseBodyModel = body; parameters.EclipseSkinModels = new[] { skin };
        parameters.ModelDocuments[0] = body; parameters.ModelDocuments.Add(skin);
        var preparedType = typeof(Fight).GetNestedType("PreparedFormModel", BindingFlags.NonPublic);
        var ctor = preparedType.GetConstructor(Hidden, null, new[] { typeof(ModelParameters) }, null);
        var modelProperty = preparedType.GetProperty("Model", Hidden);
        var liveObject = player.GetModelObject(); int nodes = liveObject.GetAllNodes().Count;
        var prepared = (IDisposable)ctor.Invoke(new object[] { parameters });
        var model = (Model)modelProperty.GetValue(prepared);
        Check(model != null && !model.GetGameObject().activeSelf, "Prepared authored model was not hidden");
        Check(model.GetModelObject().GetAllNodes().Count == nodes + 1, "Authored helper not added to native composition");
        Check(model.GetModelObject().FindNodeOrParent("AuthoredTip") != null, "Authored point binding absent");
        Check(!ReferenceEquals(model.GetModelObject(), liveObject), "Preparation reused live native body");
        prepared.Dispose(); prepared.Dispose(); Check(modelProperty.GetValue(prepared) == null, "Prepared ownership not released");
        var tip = (XmlElement)overlay.SelectSingleNode("/Scene/Nodes/AuthoredTip"); tip.SetAttribute("ChildNode1", "MissingPoint");
        try { var unexpected = (IDisposable)ctor.Invoke(new object[] { parameters }); unexpected.Dispose(); throw new Exception("Invalid authored binding accepted"); }
        catch (TargetInvocationException e)
        {
            Check(e.InnerException is InvalidDataException && e.InnerException.Message.Contains(skin) && e.InnerException.Message.Contains("@ChildNode1"), "Missing actionable native authored diagnostic: " + e.InnerException);
        }
        Check(ReferenceEquals(player.GetModelObject(), liveObject) && liveObject.GetAllNodes().Count == nodes, "Failed preparation altered main native model");
        Check(player.Parameters.EclipseBodyModel == null && player.Parameters.EclipseSkinModels.Length == 0, "Authored preparation mutated main parameters");
        documents.Remove(body); documents.Remove(skin);
    }
    static void Log(string message, string stack, LogType type)
    { if (entered && type == LogType.Exception && (stack.Contains("Fight.") || stack.Contains("Model."))) failure = message + "\n" + stack; }
    static void Finish(int code)
    {
        SessionState.SetBool(Active, false); EditorApplication.update -= Update; Application.logMessageReceived -= Log;
        EditorApplication.Exit(code);
    }
}
#endif
