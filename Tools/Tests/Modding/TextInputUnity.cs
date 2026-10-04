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
public static class TextInputUnity
{
    const string Active="Eclipse.TextInputUnity.Active";
    const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    static double started,lastReport,phaseAt; static bool campaign,entered;
    static int checks,phase; static ModUiSurface surface,form; static string combatException;
    static InputField name,notes; static Eclipse.UI.Modding.ModUiCoordinator coordinator;
    static TextInputUnity(){if(SessionState.GetBool(Active,false)){started=EditorApplication.timeSinceStartup;EditorApplication.update+=Update;Application.logMessageReceived+=Log;}}
    public static void Run()
    {
        var root=Path.GetDirectoryName(Application.dataPath);
        if(!File.Exists(Path.Combine(root,"text-input-fixture.marker")))throw new Exception("Requires isolated text input fixture.");
        Environment.SetEnvironmentVariable("ECLIPSE_MODS_ROOT",Path.Combine(root,"text-input-mods"));
        var arguments=Environment.GetCommandLineArgs();int productIndex=Array.IndexOf(arguments,"-text-inputAcceptanceProductName");
        if(productIndex<0||productIndex+1>=arguments.Length||!System.Text.RegularExpressions.Regex.IsMatch(arguments[productIndex+1],"^TextInputUnity-[0-9a-f]{32}$"))throw new Exception("Pass the generated acceptance product through the runner.");
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
            if(EditorApplication.timeSinceStartup-lastReport>20){lastReport=EditorApplication.timeSinceStartup;Debug.Log("[TextInputUnity] Waiting entered="+entered+" phase="+phase);}
            if(!campaign&&Eclipse.UI.TitleScreen.IsOpen){
                var title=UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.TitleScreen>();
                if((bool)Field(title,"splashing")||(string)Field(title,"currentPage")!="Home")return;
                typeof(Eclipse.UI.TitleScreen).GetMethod("BeginCampaign",Hidden).Invoke(title,null);
                var directory=SF2Paths.GetUserDataDirectory();
                Check(Eclipse.Saves.CampaignSaveSession.PreviewDirectory==null&&directory.StartsWith(Application.persistentDataPath,StringComparison.OrdinalIgnoreCase)&&Application.persistentDataPath.Contains("TextInputUnity-"),"Profile not isolated");
                var profile=XmlUtils.OpenXMLDocument(SF2Paths.KKIDGPBOBNI(),"usersDefault.xml",XmlUtils.EBLFEPIOMOL.Normal,true,XmlCryptoUtils.NNLGALNDJCL());
                ((System.Xml.XmlElement)profile.SelectSingleNode("/Root/Warriors/Warrior[@ID='1']")).SetAttribute("Tutorial","END");
                Directory.CreateDirectory(directory);XmlUtils.ONLDJNLKKAL(profile,Path.Combine(directory,Constants.OJMIJINKBPJ).Replace('\\','/'));
                campaign=true;return;
            }
            if(!entered){
                if(ModRuntime.Scripts==null||Module.GetInstance()==null||Eclipse.UI.TitleScreen.IsOpen||UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.EclipseLoadingOverlay>()!=null)return;
                var screen=Module.GetInstance().GetCurrentScreenType();if(screen!=ScreenType.ModuleDojo&&screen!=ScreenType.ModuleMap)return;
                Check(!ModRuntime.Host.HasErrors,ModRuntime.Host.FormatReport());Check(ModRuntime.Host.EnabledMods.Any(m=>m.Id.Value=="example.text-input-lab"),"Text Input Lab not enabled");Check(!ModRuntime.Scripts.HasErrors,"Startup mod errors");
                var encounter=ListSF.CHMCKGCDGCM(new FightIDS(ModRuntime.Scripts.Content.RuntimeFightId(DefinitionId.Parse("core:fights/zone_1/tournament/3"))));
                Check(encounter!=null,"Core encounter missing");entered=GameUtils.StartFight(encounter,false,null,true,false);return;
            }
            var fight=Fight.GetCurrentFight();if(fight==null||fight.get_FightTimeInFrames()<10)return;
            if(ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Count!=0)
                throw new Exception("Text input callback failed: "+string.Join(";",ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Select(f=>f.Error)));
            if(fight.GetFightDefinition()?.FightId.ToString()!=ModRuntime.Scripts.Content.RuntimeFightId(DefinitionId.Parse("core:fights/zone_1/tournament/3")))return;
            var player=fight.GetPlayerModel();var enemy=fight.GetEnemyModel();if(player==null||enemy==null)return;
            player.Parameters.UserControlled=false;enemy.Parameters.AiControlled=false;player.Parameters.set_IsImmortalityEnabled(true);
            double elapsed=EditorApplication.timeSinceStartup-phaseAt;

            switch(phase){
            case 0:
                if(UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.EclipseLoadingOverlay>()!=null)return;
                surface=Surface("text_input");if(surface==null)return;
                coordinator=UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.Modding.ModUiCoordinator>();
                name=Input("name");Check(name!=null&&coordinator!=null,"Native HUD field mounted");
                Check(name.characterLimit==24&&name.lineType==InputField.LineType.SingleLine&&!name.textComponent.supportRichText,"Native limits, single line, plaintext");
                Check(((Text)name.placeholder).text=="Fighter name","Native placeholder");
                Check(!coordinator.CapturesInput,"HUD idle leaves gameplay input");
                name.Select();name.ActivateInputField();Next();break;
            case 1:
                if(!name.isFocused||elapsed<.1)return;
                Check(coordinator.IsEditingText&&coordinator.CapturesInput&&Eclipse.UI.Modding.ModUiGameBridge.BlocksGameplayInput,"HUD editing captures native fighter controls");
                Check(!UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents,"Navigation suspended during typing");
                Type(name,"Shadow");
                Check(surface.GetText("name")=="Shadow"&&surface.GetText("status")=="Name: Shadow","Native keyboard events reach actual Lua string callback/getter");
                Check(name.isFocused,"Callback/model refresh preserves editing focus");
                Eclipse.UI.Modding.ModUiGameBridge.Route(-1,false,false,sequential:false);
                Check(name.isFocused&&UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject==name.gameObject,"Arrow navigation remains inside field");
                surface.SetText("name","Script");
                Check(name.text=="Script"&&surface.GetText("status")=="Name: Shadow","Script setter updates native field without callback echo");
                name.onValueChanged.Invoke("bad\tinput");Check(name.text=="Script"&&surface.GetText("name")=="Script","Invalid native control character restores owned value");
                Eclipse.UI.Modding.ModUiGameBridge.Route(1,false,false);
                Check(!name.isFocused&&!coordinator.CapturesInput,"Tab leaves field and releases HUD capture");Next();break;
            case 2:
                if(elapsed<.1)return;
                Check(!Eclipse.UI.Modding.ModUiGameBridge.BlocksGameplayInput,"HUD release clears frame guard");
                Check(surface.TryClick("edit"),"Public Lua opens multiline modal");form=Surface("notes");notes=Input("notes");
                Check(form!=null&&notes!=null&&notes.lineType==InputField.LineType.MultiLineNewline&&notes.characterLimit==128,"Multiline modal configured");
                notes.Select();notes.ActivateInputField();Next();break;
            case 3:
                if(!notes.isFocused||notes.GetComponentInParent<Eclipse.UI.Modding.ModUiFade>().GetComponent<CanvasGroup>().alpha<.99f)return;
                Type(notes,"First\nSecond");Check(form.GetText("notes")=="First\nSecond"&&form.GetText("count")=="Edited introduction","Native multiline edits notify actual Lua");
                Check(notes.isFocused&&coordinator.IsEditingText,"Multiline callback retains focus");
                new GameObject("Text input capture").AddComponent<TextInputCapture>();
                Check(Eclipse.UI.Modding.ModUiGameBridge.Route(0,false,true),"Back consumed during typing");
                Check(!notes.isFocused&&!form.IsClosed&&coordinator.CapturesInput,"First Back leaves editing while retaining modal");Next();break;
            case 4:
                if(!TextInputCapture.Done)return;
                Check(form.TryClick("apply")&&form.IsClosed&&surface.GetText("status")=="Introduction applied","Apply reads current text through public getter and closes modal");
                name.Select();name.ActivateInputField();Next();break;
            case 5:
                if(!name.isFocused)return;
                surface.SetEnabled("root",false);Check(!name.isFocused&&!coordinator.IsEditingText,"Disabled ancestor stops native editing");
                surface.SetEnabled("root",true);name.Select();name.ActivateInputField();Next();break;
            case 6:
                if(!name.isFocused)return;
                Eclipse.UI.Modding.ModUiGameBridge.SetNativeBlocked(true);
                Check(!name.isFocused&&!coordinator.CapturesInput,"Native dialog blocking releases text focus");
                Eclipse.UI.Modding.ModUiGameBridge.SetNativeBlocked(false);
                surface.SetVisible("root",false);Check(!coordinator.CapturesInput,"Hidden HUD is not interactive");
                surface.SetVisible("root",true);
                Check(surface.TryClick("edit"),"Modal can reopen after cleanup");form=Surface("notes");Next();break;
            case 7:
                if(elapsed<.2)return;
                Check(form.GetText("notes")=="First\nSecond","Example retains applied draft within context");
                Check(Eclipse.UI.Modding.ModUiGameBridge.Route(0,false,true),"Back stops auto-selected modal field");Next();break;
            case 8:
                if(elapsed<.1)return;
                Eclipse.UI.Modding.ModUiGameBridge.Route(0,false,true);
                Check(form.IsClosed&&!surface.IsClosed,"Second Back dismisses modal only");
                surface.Close();Check(surface.IsClosed&&!coordinator.IsEditingText,"Owner close removes field input authority");
                typeof(Fight).GetMethod("HCNDAFDHACI",Hidden).Invoke(fight,new object[]{GameOverTypes.GAME_OVER_SURRENDER});Next();break;
            case 9:
                if(elapsed<.2)return;
                Check(ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Count==0,"Text Input Lab callback errors");
                Check(File.Exists(Path.Combine(Path.GetDirectoryName(Application.dataPath),"text-input-native.png")),"Native screenshot missing");
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath),"text-input-result.txt"),"PASS: "+checks+" full-game text input checks; installed Lua HUD and multiline modal, native InputField keyboard Event processing, string callbacks/getter, silent updates, validation rollback, focus retention, HUD capture/release, arrows/Tab/Back, disabled ancestor, native dialog blocking, draft retention and close cleanup. Synthetic native key Events; physical keyboard/IME, touch keyboards, glyph coverage, platforms and pack performance are outside this acceptance.");
                Debug.Log("[TextInputUnity] PASS: "+checks+" full-game checks");Finish(0);break;
            }
        }catch(Exception error){Debug.LogError("[TextInputUnity] FAIL: "+error);Finish(1);}
    }
    static void Type(InputField field,string text){foreach(char c in text)field.ProcessEvent(new Event{type=EventType.KeyDown,character=c,keyCode=c=='\n'?KeyCode.Return:KeyCode.None});field.ForceLabelUpdate();}
    static InputField Input(string id)=>UnityEngine.Object.FindObjectsByType<InputField>(FindObjectsSortMode.None).SingleOrDefault(f=>f.name==id);
    static ModUiSurface Surface(string id){foreach(var context in (IEnumerable)Field(ModRuntime.Scripts,"_contexts")){var scope=context.GetType().GetProperty("UiScope").GetValue(context) as ModUiScope;if(scope==null||scope.Owner.Value!="example.text-input-lab")continue;foreach(ModUiSurface s in ((IDictionary)Field(scope,"surfaces")).Values)if(s.Id==id)return s;}return null;}
    static void Log(string message,string stack,LogType type){if(entered&&type==LogType.Exception&&(stack.Contains("Fight.")||stack.Contains("Model.")||stack.Contains("Modding")))combatException=message+"\n"+stack;}
    static void Finish(int code){SessionState.SetBool(Active,false);EditorApplication.update-=Update;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
public sealed class TextInputCapture : MonoBehaviour
{
    internal static bool Done;
    IEnumerator Start(){yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Application.dataPath),"text-input-native.png"),texture.EncodeToPNG());Done=true;UnityEngine.Object.Destroy(texture);UnityEngine.Object.Destroy(gameObject);}
}
#endif
