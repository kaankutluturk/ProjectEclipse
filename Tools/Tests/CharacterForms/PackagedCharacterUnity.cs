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
public static class PackagedCharacterUnity
{
    const string Active="Eclipse.PackagedCharacterUnity.Active";
    const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    static double started,lastReport; static bool campaign,entered;
    static int checks,phase,phaseFrame; static string combatException;
    static string owner=>Argument("-characterPackageModId");
    static bool sawMove, captured; static float minWrist=float.PositiveInfinity,maxWrist=float.NegativeInfinity;
    static string Move=>owner+":moves/authored_move";
    static string Character=>owner+":warriors/authored_character";
    static bool OpponentOnly=>Environment.GetCommandLineArgs().Contains("-packagedOpponentOnly");
    static string Argument(string key){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,key);if(i<0||i+1>=args.Length)throw new Exception("Missing "+key);return args[i+1];}
    static PackagedCharacterUnity(){if(SessionState.GetBool(Active,false)){started=EditorApplication.timeSinceStartup;EditorApplication.update+=Update;Application.logMessageReceived+=Log;}}
    public static void Run()
    {
        var root=Path.GetDirectoryName(Application.dataPath);
        if(!File.Exists(Path.Combine(root,"packaged-character-fixture.marker")))throw new Exception("Requires isolated packaged character fixture.");
        Environment.SetEnvironmentVariable("ECLIPSE_MODS_ROOT",Argument("-characterPackageModsRoot"));
        var arguments=Environment.GetCommandLineArgs();int productIndex=Array.IndexOf(arguments,"-packaged-characterAcceptanceProductName");
        if(productIndex<0||productIndex+1>=arguments.Length||!System.Text.RegularExpressions.Regex.IsMatch(arguments[productIndex+1],"^PackagedCharacterUnity-[0-9a-f]{32}$"))throw new Exception("Pass the generated acceptance product through the runner.");
        PlayerSettings.companyName="EclipseAcceptance";PlayerSettings.productName=arguments[productIndex+1];
        SessionState.SetBool(Active,true);EditorSceneManager.OpenScene("Assets/src/GUI/Scenes/GameLoaderScene/GameLoader.unity");
        var view=EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));view.Show();view.Focus();EditorApplication.EnterPlaymode();
    }
    static object Field(object value,string name)=>value.GetType().GetField(name,Hidden|BindingFlags.Public).GetValue(value);
    static void Check(bool value,string message){checks++;if(!value)throw new Exception(message);}
    static void Update()
    {
        if(!EditorApplication.isPlaying)return;
        try{
            if(ModRuntime.Host!=null&&ModRuntime.Host.HasErrors)throw new Exception(ModRuntime.Host.FormatReport());
            if(ModRuntime.Scripts!=null&&ModRuntime.Scripts.HasErrors)throw new Exception("Generated package script registration failed");
            if(combatException!=null)throw new Exception(combatException);
            if(EditorApplication.timeSinceStartup-started>300)throw new Exception("Timed out phase "+phase);
            if(EditorApplication.timeSinceStartup-lastReport>20){lastReport=EditorApplication.timeSinceStartup;Debug.Log("[PackagedCharacterUnity] Waiting entered="+entered+" phase="+phase);}
            if(!campaign&&Eclipse.UI.TitleScreen.IsOpen){
                var title=UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.TitleScreen>();
                if((bool)Field(title,"splashing")||(string)Field(title,"currentPage")!="Home")return;
                typeof(Eclipse.UI.TitleScreen).GetMethod("BeginCampaign",Hidden).Invoke(title,null);
                var directory=SF2Paths.GetUserDataDirectory();
                Check(Eclipse.Saves.CampaignSaveSession.PreviewDirectory==null&&directory.StartsWith(Application.persistentDataPath,StringComparison.OrdinalIgnoreCase)&&Application.persistentDataPath.Contains("PackagedCharacterUnity-"),"Profile not isolated");
                var profile=XmlUtils.OpenXMLDocument(SF2Paths.KKIDGPBOBNI(),"usersDefault.xml",XmlUtils.EBLFEPIOMOL.Normal,true,XmlCryptoUtils.NNLGALNDJCL());
                ((System.Xml.XmlElement)profile.SelectSingleNode("/Root/Warriors/Warrior[@ID='1']")).SetAttribute("Tutorial","END");
                Directory.CreateDirectory(directory);XmlUtils.ONLDJNLKKAL(profile,Path.Combine(directory,Constants.OJMIJINKBPJ).Replace('\\','/'));
                campaign=true;return;
            }
            if(!entered){
                if(ModRuntime.Scripts==null||Module.GetInstance()==null||Eclipse.UI.TitleScreen.IsOpen||UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.EclipseLoadingOverlay>()!=null)return;
                var screen=Module.GetInstance().GetCurrentScreenType();if(screen!=ScreenType.ModuleDojo&&screen!=ScreenType.ModuleMap)return;
                Check(!ModRuntime.Host.HasErrors,ModRuntime.Host.FormatReport());Check(ModRuntime.Host.EnabledMods.Any(m=>m.Id.Value==owner),"Generated character package not enabled");Check(!ModRuntime.Scripts.HasErrors,"Startup mod errors");
                var encounter=ListSF.CHMCKGCDGCM(new FightIDS(ModRuntime.Scripts.Content.RuntimeFightId(DefinitionId.Parse(owner+":fights/preview"))));
                Check(encounter!=null,"Core encounter missing");entered=GameUtils.StartFight(encounter,false,null,true,false);return;
            }
            var fight=Fight.GetCurrentFight();if(fight==null||fight.get_FightTimeInFrames()<10)return;
            if(ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Count!=0)
                throw new Exception("Packaged character callback failed: "+string.Join(";",ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Select(f=>f.Error)));
            if(fight.GetFightDefinition()?.FightId.ToString()!=ModRuntime.Scripts.Content.RuntimeFightId(DefinitionId.Parse(owner+":fights/preview")))return;
            var player=fight.GetPlayerModel();var enemy=fight.GetEnemyModel();if(player==null||enemy==null)return;
            player.Parameters.set_IsImmortalityEnabled(true);enemy.Parameters.set_IsImmortalityEnabled(true);

            int frame=fight.get_FightTimeInFrames();
            switch(phase){
            case 0:
                if(UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.EclipseLoadingOverlay>()!=null||!fight.Controller.IsQuadrantEnabled(FightCID.Punch))return;
                var definition=ModRuntime.Scripts.Content.Fights.Single(f=>f.Id.Namespace.Value==owner);
                Check(definition.PlayerCharacter==(OpponentOnly?(DefinitionId?)null:DefinitionId.Parse(Character)),"Generated public fight player selection");
                Check(enemy.Parameters.EclipseCharacterId==Character,"Native generated opponent identity");
                Check(player.Parameters.UserControlled&&!player.Parameters.AiControlled,"Canonical player retains native controls");
                Check(OpponentOnly?player.Parameters.EclipseCharacterId!=Character:player.Parameters.EclipseCharacterId==Character,"Native player role matches package option");
                ValidateSkin(enemy);if(!OpponentOnly)ValidateSkin(player);
                enemy.Parameters.AiControlled=OpponentOnly;
                if(OpponentOnly){phase=4;phaseFrame=frame;break;}
                var clip=player.GetAvailableAnimations().Single(m=>m.Name==Move);
                Check(clip.AnimationEndFrame==59&&ModRuntime.Scripts.Content.Moves.Single(m=>m.Id.ToString()==Move).MidFrames==0,"Native clip sample bounds and timing");
                Position(player,450);Position(enemy,1000);
                Send(fight,0);phase=1;phaseFrame=frame;break;
            case 1:
            case 3:
                if(frame-phaseFrame>=3)Send(fight,1);
                if(player.GetCurrentAnimation()?.Name==Move){
                    if(!sawMove&&phase==3){var animation=Field(player,"_Animation");Check((int)Field(animation,"JMKAHNADIOI")==-1,"Native exported Punch faces opposite direction");}
                    if(!sawMove){var data=(Vector3[][])Field(player.GetCurrentAnimation(),"_AnimationContainer");Check(data.Length==60&&data.All(f=>f.Length==67),"Unmodified exported clip loads native point count");}
                    sawMove=true;var wrist=player.GetModelObject().FindNodeOrParent("NWrist_1").GetEnd();var pivot=player.GetModelObject().FindNodeOrParent("NPivot").GetEnd();
                    float value=wrist.GetY()-pivot.GetY();minWrist=Mathf.Min(minWrist,value);maxWrist=Mathf.Max(maxWrist,value);
                    if(!captured){captured=true;new GameObject("Packaged player capture").AddComponent<PackagedCharacterCapture>();}
                }
                if(frame-phaseFrame>240)throw new Exception("Generated Punch export did not play/finish: "+player.GetCurrentAnimation()?.Name);
                if(!sawMove||player.GetCurrentAnimation()?.Name==Move)return;
                Check(maxWrist-minWrist>5,"Authored IK wrist motion reaches native model");
                Check(!Eclipse.UI.Modding.ModUiGameBridge.BlocksGameplayInput,"Generated player has no orphan mod input capture");
                if(phase==1){
                    ValidateSkin(player);Position(player,1000);Position(enemy,450);phase=2;phaseFrame=frame;
                }else{enemy.Parameters.AiControlled=true;phase=4;phaseFrame=frame;sawMove=false;}
                break;
            case 2:
                if(frame-phaseFrame<30)return;
                sawMove=false;minWrist=float.PositiveInfinity;maxWrist=float.NegativeInfinity;Send(fight,0);phase=3;phaseFrame=frame;break;
            case 4:
                sawMove|=enemy.GetCurrentAnimation()?.Name==Move;
                if(frame-phaseFrame>480)throw new Exception("Generated native AI did not choose authored export: "+enemy.GetCurrentAnimation()?.Name);
                if(!sawMove||enemy.GetCurrentAnimation()?.Name==Move)return;
                Check(sawMove,"Generated Lua tactic reaches native opponent animation");ValidateSkin(enemy);
                if(!captured){captured=true;new GameObject("Packaged opponent capture").AddComponent<PackagedCharacterCapture>();}
                typeof(Fight).GetMethod("HCNDAFDHACI",Hidden).Invoke(fight,new object[]{GameOverTypes.GAME_OVER_SURRENDER});phase=5;phaseFrame=frame;break;
            case 5:
                if(!PackagedCharacterCapture.Done)return;
                Check(ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Count==0,"Generated package callback errors");
                Check(!Eclipse.UI.Modding.ModUiGameBridge.BlocksGameplayInput,"End cleanup leaves no mod input capture");
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath),"packaged-character-result.txt"),"PASS: "+checks+" full-game generated character checks; actual exported models/skin, public Lua fight selection, "+(OpponentOnly?"canonical player with exported opponent":"60x67 player clip, native Punch in both facing directions and IK deformation")+", weighted skin helpers, generated AI selection and surrender cleanup. Controlled Gymnast scene/rig/events; attack contacts, physical devices, arbitrary rigs and exported platforms remain outside this acceptance.");
                Debug.Log("[PackagedCharacterUnity] PASS: "+checks+" full-game checks");Finish(0);break;
            }
        }catch(Exception error){Debug.LogError("[PackagedCharacterUnity] FAIL: "+error);Finish(1);}
    }
    static void Position(Model model,float x)=>model.GetType().GetMethod("TrainingMoveToX",Hidden|BindingFlags.Public).Invoke(model,new object[]{x});
    static void Send(Fight fight,int action)=>fight.Controller.GetType().GetMethod("SendGamepadControlEvent",Hidden|BindingFlags.Public).Invoke(fight.Controller,new object[]{action,FightCID.Punch});
    static void ValidateSkin(Model model){
        var body=model.GetModelObject();var node=body.FindNodeOrParent("EclipseFixtureNode-3");Check(node!=null,"Exported Gymnast skin helper mounted");
        // ModelMacroNode computes current Start XY from child Starts through
        // Vector3f.GLGNIMKANCA, which deliberately excludes Z. End retains the
        // previous helper pose. This proves planar native skin binding only.
        var actual=node.GetStart();var expected=new Vector3();
        // Coefficients come from the actual upstream HEAD_GEAR export, not a
        // reconstructed proxy. Read the installed XML used by this model.
        var document=new System.Xml.XmlDocument();document.Load(Path.Combine(Argument("-characterPackageModsRoot"),owner,"assets/models/skin1.xml"));
        var helper=document.SelectSingleNode("/Scene/Nodes/EclipseFixtureNode-3");
        for(int i=1;i<=int.Parse(helper.Attributes["NodesCount"].Value);i++){
            var point=body.FindNodeOrParent(helper.Attributes["ChildNode"+i].Value).GetStart();
            var weight=float.Parse(helper.Attributes["LCC"+i].Value,System.Globalization.CultureInfo.InvariantCulture);expected+=new Vector3(point.GetX(),point.GetY(),0)*weight;
        }
        var actualVector=new Vector3(actual.GetX(),actual.GetY(),actual.GetZ());
        Check(Vector3.Distance(actualVector,expected)<.01,"Native exported skin follows weighted head landmarks in XY: "+model.Parameters.EclipseCharacterId+" actual="+actualVector+" expected="+expected);
    }
    static void Log(string message,string stack,LogType type){if(entered&&type==LogType.Exception&&(stack.Contains("Fight.")||stack.Contains("Model.")||stack.Contains("Modding")))combatException=message+"\n"+stack;}
    static void Finish(int code){SessionState.SetBool(Active,false);EditorApplication.update-=Update;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
public sealed class PackagedCharacterCapture : MonoBehaviour
{
    internal static bool Done;
    IEnumerator Start(){yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Application.dataPath),"packaged-character-native.png"),texture.EncodeToPNG());Done=true;UnityEngine.Object.Destroy(texture);UnityEngine.Object.Destroy(gameObject);}
}
#endif
