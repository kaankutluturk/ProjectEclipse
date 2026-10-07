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
public static class CameraUnity
{
    const string Active="Eclipse.CameraUnity.Active";
    const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    static double started,lastReport,phaseAt; static bool campaign,entered;
    static int checks,phase,pausedFrame; static ModUiSurface surface; static string combatException;
    static ModCameraSlot slot; static global::Camera camera; static LocationSelector gameLayer;
    static Vector3 initialLayer, pausedLayer, playerPosition, enemyPosition; static float initialScale, playerHealth, enemyHealth;
    static CameraUnity(){if(SessionState.GetBool(Active,false)){started=EditorApplication.timeSinceStartup;EditorApplication.update+=Update;Application.logMessageReceived+=Log;}}
    public static void Run()
    {
        var root=Path.GetDirectoryName(Application.dataPath);
        if(!File.Exists(Path.Combine(root,"camera-fixture.marker")))throw new Exception("Requires isolated audio fixture.");
        Environment.SetEnvironmentVariable("ECLIPSE_MODS_ROOT",Path.Combine(root,"camera-mods"));
        var arguments=Environment.GetCommandLineArgs();int productIndex=Array.IndexOf(arguments,"-cameraAcceptanceProductName");
        if(productIndex<0||productIndex+1>=arguments.Length||!System.Text.RegularExpressions.Regex.IsMatch(arguments[productIndex+1],"^CameraUnity-[0-9a-f]{32}$"))throw new Exception("Pass the generated acceptance product through the runner.");
        PlayerSettings.companyName="EclipseAcceptance";PlayerSettings.productName=arguments[productIndex+1];
        SessionState.SetBool(Active,true);EditorSceneManager.OpenScene("Assets/src/GUI/Scenes/GameLoaderScene/GameLoader.unity");
        var view=EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));view.Show();view.Focus();EditorApplication.EnterPlaymode();
    }
    static object Field(object value,string name)=>value.GetType().GetField(name,Hidden|BindingFlags.Public).GetValue(value);
    static void Check(bool value,string message){checks++;if(!value)throw new Exception(message);}
    static void Next(){phase++;phaseAt=EditorApplication.timeSinceStartup;}
    static void Update()
    {
        if(!EditorApplication.isPlaying)return;
        try{
            if(combatException!=null)throw new Exception(combatException);
            if(EditorApplication.timeSinceStartup-started>300)throw new Exception("Timed out phase "+phase);
            if(EditorApplication.timeSinceStartup-lastReport>20){lastReport=EditorApplication.timeSinceStartup;Debug.Log("[CameraUnity] Waiting entered="+entered+" phase="+phase);}
            if(!campaign&&Eclipse.UI.TitleScreen.IsOpen){
                var title=UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.TitleScreen>();
                if((bool)Field(title,"splashing")||(string)Field(title,"currentPage")!="Home")return;
                typeof(Eclipse.UI.TitleScreen).GetMethod("BeginCampaign",Hidden).Invoke(title,null);
                var directory=SF2Paths.GetUserDataDirectory();
                Check(Eclipse.Saves.CampaignSaveSession.PreviewDirectory==null&&directory.StartsWith(Application.persistentDataPath,StringComparison.OrdinalIgnoreCase)&&Application.persistentDataPath.Contains("CameraUnity-"),"Profile not isolated");
                var profile=XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(),"usersDefault.xml",XmlUtils.XmlSourceMode.Normal,true,XmlCryptoUtils.GetIsEncryptionEnabled());
                ((System.Xml.XmlElement)profile.SelectSingleNode("/Root/Warriors/Warrior[@ID='1']")).SetAttribute("Tutorial","END");
                Directory.CreateDirectory(directory);XmlUtils.SaveDocumentWithHash(profile,Path.Combine(directory,Constants.UsersFileName).Replace('\\','/'));
                campaign=true;return;
            }
            if(!entered){
                if(ModRuntime.Scripts==null||Module.GetInstance()==null||Eclipse.UI.TitleScreen.IsOpen||UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.EclipseLoadingOverlay>()!=null)return;
                var screen=Module.GetInstance().GetCurrentScreenType();if(screen!=ScreenType.ModuleDojo&&screen!=ScreenType.ModuleMap)return;
                Check(!ModRuntime.Host.HasErrors,ModRuntime.Host.FormatReport());Check(ModRuntime.Host.EnabledMods.Any(m=>m.Id.Value=="example.camera-lab"),"Camera Lab not enabled");Check(!ModRuntime.Scripts.HasErrors,"Startup mod errors");
                var encounter=ListSF.GetFightById(new FightIDS(ModRuntime.Scripts.Content.RuntimeFightId(DefinitionId.Parse("core:fights/zone_1/tournament/3"))));
                Check(encounter!=null,"Core encounter missing");entered=GameUtils.StartFight(encounter,false,null,true,false);return;
            }
            var fight=Fight.GetCurrentFight();if(fight==null||fight.get_FightTimeInFrames()<10)return;
            if(ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Count!=0)
                throw new Exception("Camera callback failed: "+string.Join(";",ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Select(f=>f.Error)));
            if(fight.GetFightDefinition()?.FightId.ToString()!=ModRuntime.Scripts.Content.RuntimeFightId(DefinitionId.Parse("core:fights/zone_1/tournament/3")))return;
            var player=fight.GetPlayerModel();var enemy=fight.GetEnemyModel();if(player==null||enemy==null)return;
            player.Parameters.UserControlled=false;enemy.Parameters.AiControlled=false;player.Parameters.set_IsImmortalityEnabled(true);
            double elapsed=EditorApplication.timeSinceStartup-phaseAt;

            switch(phase){
            case 0:
                if(UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.EclipseLoadingOverlay>()!=null)return;
                surface=Surface();if(surface==null)return;
                ModRuntime.Scripts.CallbackDiagnostics.Recording=true;
                slot=(ModCameraSlot)Field(fight,"_eclipseCamera");camera=(global::Camera)Field(fight,"_Camera");
                var render=camera.GetRender();if(render==null)return;
                gameLayer=(LocationSelector)Field(Field(render,"_location"),"gameLayer");
                initialLayer=gameLayer.GetLayerObject().transform.localPosition;initialScale=gameLayer.GetLayerObject().transform.localScale.x;
                Check(slot.Settings==null&&surface.Read("status").Text=="Camera: released","Camera starts released");
                Check(surface.TryClick("focus"),"Public Lua focus button");Next();break;
            case 1:
                if(surface.Read("status").Text!="Camera: focus"||slot.Settings==null)return;
                Check(slot.Settings.Zoom==1.3&&slot.Settings.OffsetY==-40,"Public Lua requested focus settings");
                if(Math.Abs(gameLayer.GetLayerObject().transform.localScale.x-initialScale)<.01)return;
                Check(gameLayer.GetLayerObject().transform.localPosition.y>initialLayer.y+20,"Native vertical pan applied");
                new GameObject("Camera focus capture").AddComponent<CameraFocusCapture>();
                Check(surface.TryClick("sweep"),"Public Lua sweep button");Next();break;
            case 2:
                if(surface.Read("status").Text!="Camera: sweep"||elapsed<.4)return;
                Check(slot.Settings.Zoom==1.2&&slot.Settings.CenterX.HasValue,"Sweep replaced focus settings");
                fight.SetPaused(true);pausedFrame=fight.get_FightTimeInFrames();
                pausedLayer=gameLayer.GetLayerObject().transform.localPosition;
                playerPosition=player.GetRenderObject().transform.localPosition;enemyPosition=enemy.GetRenderObject().transform.localPosition;
                playerHealth=player.GetLife();enemyHealth=enemy.GetLife();Next();break;
            case 3:
                if(elapsed<.5)return;
                Check(fight.get_FightTimeInFrames()==pausedFrame,"Simulation clock paused");
                Check(Vector3.Distance(gameLayer.GetLayerObject().transform.localPosition,pausedLayer)<.02,"Lua sweep paused");
                for(int i=0;i<200;i++)camera.RenderInterpolatedPresentation();
                Check(Vector3.Distance(gameLayer.GetLayerObject().transform.localPosition,pausedLayer)<.02,"Repeated native presentations do not accumulate pan");
                Check(player.GetLife()==playerHealth&&enemy.GetLife()==enemyHealth,"Presentation redraw leaves health unchanged");
                Check(Vector3.Distance(player.GetRenderObject().transform.localPosition,playerPosition)<.001&&Vector3.Distance(enemy.GetRenderObject().transform.localPosition,enemyPosition)<.001,"Presentation redraw leaves local fighter poses unchanged");
                Check(surface.TryClick("release")&&slot.Settings==null,"Release button works while paused");
                camera.RenderInterpolatedPresentation();
                Check(Math.Abs(gameLayer.GetLayerObject().transform.localPosition.y-initialLayer.y)<.02,"Release removes vertical contribution");
                Check(Math.Abs(gameLayer.GetLayerObject().transform.localScale.x-initialScale)<.02,"Release restores current native zoom");
                Check(surface.TryClick("native"),"Native view button queues next tick");Check(slot.Settings==null,"Pause does not acquire new ownership");
                fight.SetPaused(false);Next();break;
            case 4:
                if(surface.Read("status").Text!="Camera: native"||slot.Settings==null)return;
                Check(slot.Settings.CenterX==null&&slot.Settings.Zoom==null&&slot.Settings.OffsetY==0,"Owned native view uses defaults");
                Check(Math.Abs(gameLayer.GetLayerObject().transform.localPosition.y-initialLayer.y)<.02,"Owned native view has no pan");
                Check(surface.TryClick("focus"),"Reacquire focus through retained handle");Next();break;
            case 5:
                if(surface.Read("status").Text!="Camera: focus"||slot.Settings?.Zoom!=1.3)return;
                var type=typeof(Fight).GetNestedType("EclipseFighterOperations",BindingFlags.NonPublic);
                var provider=(IModFighterCamera)Activator.CreateInstance(type,new object[]{fight,enemy,null,null,null,null,false});
                Check(!provider.TryAcquireCamera(ModId.Parse("example.camera-lab"),new ModCameraSettings(0,0,2),out _,out var error)&&error.Contains("already owned"),"Native second fighter host cannot evict Lua owner");
                // Dispose the script's lease through HUD close, then acquire an
                // unclaimed native control to prove boundary cleanup independently.
                surface.Close();Check(slot.Settings==null,"HUD close releases retained Lua ownership");
                Check(provider.TryAcquireCamera(ModId.Parse("example.camera-lab"),new ModCameraSettings(600,-40,1.3),out orphan,out error),error);
                Check(orphan.IsActive,"Unclaimed native control active");
                typeof(Fight).GetMethod("AbortFight",Hidden).Invoke(fight,new object[]{GameOverTypes.GAME_OVER_SURRENDER});
                Check(!orphan.IsActive&&slot.Settings==null,"Surrender releases unclaimed ownership");
                long before=ModRuntime.Scripts.CallbackDiagnostics.TimedCalls;
                typeof(Fight).GetMethod("DispatchEclipseCombatEvent",Hidden).Invoke(fight,new object[]{ModEffectEvent.Tick,null,null,null,null});
                Check(ModRuntime.Scripts.CallbackDiagnostics.TimedCalls==before,"Ended round suppresses ticks");Next();break;
            case 6:
                if(elapsed<.2)return;
                Check(ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Count==0,"Camera Lab callbacks failed: "+string.Join(";",ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Select(f=>f.Error)));
                Check(File.Exists(Path.Combine(Path.GetDirectoryName(Application.dataPath),"camera-focus-native.png")),"Native focus screenshot missing");
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath),"camera-result.txt"),"PASS: "+checks+" full-game camera checks; installed public Lua HUD focus/sweep/native/release, native pan/zoom, pause, repeated redraw drift, unchanged fighter pose/health, owned defaults, shared native owner conflict, HUD and unclaimed surrender cleanup, post-end tick suppression. Core arena/rig/profile controlled; actor forms, arbitrary arenas, platform export and pack performance remain outside native acceptance.");
                Debug.Log("[CameraUnity] PASS: "+checks+" full-game checks");Finish(0);break;
            }
        }catch(Exception error){Debug.LogError("[CameraUnity] FAIL: "+error);Finish(1);}
    }
    static IModCameraControl orphan;
    static ModUiSurface Surface(){foreach(var context in (IEnumerable)Field(ModRuntime.Scripts,"_contexts")){var scope=context.GetType().GetProperty("UiScope").GetValue(context) as ModUiScope;if(scope==null||scope.Owner.Value!="example.camera-lab")continue;foreach(ModUiSurface s in ((IDictionary)Field(scope,"surfaces")).Values)if(s.Id=="camera")return s;}return null;}
    static void Log(string message,string stack,LogType type){if(entered&&type==LogType.Exception&&(stack.Contains("Fight.")||stack.Contains("Model.")||stack.Contains("Modding")))combatException=message+"\n"+stack;}
    static void Finish(int code){SessionState.SetBool(Active,false);EditorApplication.update-=Update;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
public sealed class CameraFocusCapture : MonoBehaviour
{
    IEnumerator Start(){yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Application.dataPath),"camera-focus-native.png"),texture.EncodeToPNG());UnityEngine.Object.Destroy(texture);UnityEngine.Object.Destroy(gameObject);}
}
#endif
