#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Eclipse.Modding;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class AuthoredFighterUnity
{
    const string Active = "Eclipse.AuthoredFighterUnity.Active", Owner = "example.authored-fighter", Move = Owner + ":moves/sash_strike";
    const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    static double started, report, pauseAt;
    static bool campaign, entered, captured;
    static int phase, checks, phaseFrame, pauseFrame;
    static float playerLife, enemyLife, beforeHit, autoLeftLife, autoRightLife;
    static Model left, right;
    static string leftId, rightId;
    static ModUiSurface surface;
    static string failure;
    static float[] pausedPose, sashRest;
    static bool sashDeformed, readerChecked, mirroredRight;
    static readonly Dictionary<string,int> starts = new Dictionary<string,int>(), dealt = new Dictionary<string,int>();
    static readonly HashSet<string> receipts = new HashSet<string>(), ended = new HashSet<string>();
    static string Root => Path.GetDirectoryName(Application.dataPath);
    static AuthoredFighterUnity() { if (!SessionState.GetBool(Active,false)) return; started=EditorApplication.timeSinceStartup; EditorApplication.update+=Update; Application.logMessageReceived+=Log; }
    public static void Run()
    {
        if (!File.Exists(Path.Combine(Root,"authored-fighter-fixture.marker"))) throw new Exception("Requires isolated authored fighter fixture.");
        Environment.SetEnvironmentVariable("ECLIPSE_MODS_ROOT",Path.Combine(Root,"authored-fighter-mods"));
        var args=Environment.GetCommandLineArgs(); int index=Array.IndexOf(args,"-actorAcceptanceProductName");
        if(index<0||index+1>=args.Length||!System.Text.RegularExpressions.Regex.IsMatch(args[index+1],"^AuthoredFighterUnity-[0-9a-f]{32}$")) throw new Exception("Requires runner-generated isolated product.");
        PlayerSettings.companyName="EclipseAcceptance"; PlayerSettings.productName=args[index+1];
        SessionState.SetBool(Active,true); EditorSceneManager.OpenScene("Assets/src/GUI/Scenes/GameLoaderScene/GameLoader.unity");
        var view=EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView")); view.Show(); view.Focus(); EditorApplication.EnterPlaymode();
    }
    static object Field(object target,string name)=>target.GetType().GetField(name,Hidden|BindingFlags.Public).GetValue(target);
    static object Invoke(object target,string name,params object[] args)=>target.GetType().GetMethod(name,Hidden|BindingFlags.Public).Invoke(target,args);
    static void Check(bool value,string message) { checks++; if(!value) throw new Exception(message); }
    static float Life(Model model)=>model.Parameters.RemainingHealthInDamageUnits;
    static Model[] Actors(Fight fight)=>fight.LNDLFINJHDB.Where(m=>m!=null&&!(m is WeaponModel)&&(m.get_Name()??"").StartsWith(Owner+":actors/",StringComparison.Ordinal)).ToArray();
    static IModActor Actor(Fight fight,Model model)=>(IModActor)((IDictionary)Field(fight,"_eclipseActors"))[model];
    static string Id(Fight fight,Model model) { Check(Actor(fight,model).TrySnapshot(out var snap,out var error),error); return snap.Id; }
    static float[] Pose(Model model)=>model.GetModelObject().NAMKCLGOPDD().SelectMany(n=>new[]{n.GetEnd().GetX(),n.GetEnd().GetY(),n.GetEnd().GetZ()}).ToArray();
    static float[] Sash(Model model)
    {
        var body=model.GetModelObject(); var root=body.FindNodeOrParent("NPivot").GetEnd();
        return new[]{0,1,4,5}.SelectMany(i=>{var point=body.FindNodeOrParent("AuthoredSashV"+i).GetEnd();return new[]{point.GetX()-root.GetX(),point.GetY()-root.GetY(),point.GetZ()-root.GetZ()};}).ToArray();
    }
    static ModUiSurface Surface()
    {
        foreach(var context in (IEnumerable)Field(ModRuntime.Scripts,"_contexts"))
        {
            var scope=context.GetType().GetProperty("UiScope").GetValue(context) as ModUiScope;
            if(scope==null||scope.Owner.Value!=Owner)continue;
            foreach(ModUiSurface value in ((IDictionary)Field(scope,"surfaces")).Values)if(value.Id=="authored")return value;
        }
        return null;
    }
    static void Click(string id)
    {
        var view=UnityEngine.Object.FindObjectsByType<Eclipse.UI.Modding.ModUiView>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(v=>ReferenceEquals(Field(v,"surface"),surface));
        var button=(Button)Field(((IDictionary)Field(view,"widgets"))[id],"Button");
        Check(button.gameObject.activeInHierarchy&&button.interactable,"Authored lab button unavailable"); button.onClick.Invoke();
    }
    static void Next(Fight fight) { phase++; phaseFrame=fight.get_FightTimeInFrames(); }
    static int Count(Dictionary<string,int> values,string id)=>values.TryGetValue(id,out var count)?count:0;
    static void CheckSource(Model victim,Model caster)
    {
        var field=typeof(Model).GetFields(Hidden|BindingFlags.Public).Single(f=>f.FieldType==typeof(Model.StrikeResult));
        Check(((Model.StrikeResult)field.GetValue(victim)).AttackerModel.GetRootModel()==caster,"Contact did not originate from authored actor");
    }
    static void Update()
    {
        try
        {
            if(failure!=null)throw new Exception(failure);
            if(EditorApplication.timeSinceStartup-started>300)throw new Exception("Authored fighter timed out in phase "+phase);
            if(EditorApplication.timeSinceStartup-report>20){report=EditorApplication.timeSinceStartup;Debug.Log("[AuthoredFighterUnity] Waiting entered="+entered+" phase="+phase);}
            if(!EditorApplication.isPlaying)return;
            if(!campaign)
            {
                var title=UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.TitleScreen>(); if(!title)return;
                if((bool)Field(title,"splashing")||(string)Field(title,"currentPage")!="Home")return;
                Invoke(title,"BeginCampaign"); var directory=SF2Paths.GetUserDataDirectory();
                Check(directory.StartsWith(Application.persistentDataPath,StringComparison.OrdinalIgnoreCase)&&Application.persistentDataPath.Contains("AuthoredFighterUnity-"),"Profile not isolated");
                var profile=XmlUtils.OpenXMLDocument(SF2Paths.KKIDGPBOBNI(),"usersDefault.xml",XmlUtils.EBLFEPIOMOL.Normal,true,XmlCryptoUtils.NNLGALNDJCL());
                ((System.Xml.XmlElement)profile.SelectSingleNode("/Root/Warriors/Warrior[@ID='1']")).SetAttribute("Tutorial","END");
                Directory.CreateDirectory(directory);XmlUtils.ONLDJNLKKAL(profile,Path.Combine(directory,Constants.OJMIJINKBPJ).Replace('\\','/'));campaign=true;return;
            }
            if(!entered)
            {
                if(ModRuntime.Scripts==null||Module.GetInstance()==null)return;
                var screen=Module.GetInstance().GetCurrentScreenType();if(screen!=ScreenType.ModuleDojo&&screen!=ScreenType.ModuleMap)return;
                Check(!ModRuntime.Host.HasErrors,ModRuntime.Host.FormatReport());
                Check(ModRuntime.Scripts.ActiveMods.Any(m=>m.Id.Value==Owner),"Authored mod did not finish registration");
                Check(ModRuntime.Host.EnabledMods.All(m=>m.Id.Value=="core"||m.Id.Value==Owner),"Unexpected mods enabled");
                var encounter=ListSF.CHMCKGCDGCM(new FightIDS(ModRuntime.Scripts.Content.RuntimeFightId(DefinitionId.Parse("core:fights/zone_1/tournament/3"))));
                Check(encounter!=null,"Core encounter missing");entered=GameUtils.StartFight(encounter,false,null,true,false);return;
            }
            var fight=Fight.GetCurrentFight();var player=fight?.GetPlayerModel();var enemy=fight?.GetEnemyModel();if(player==null||enemy==null)return;
            player.Parameters.UserControlled=false;player.Parameters.AiControlled=false;enemy.Parameters.UserControlled=false;enemy.Parameters.AiControlled=false;
            int frame=fight.get_FightTimeInFrames();if(frame<100)return;
            switch(phase)
            {
                case 0:
                    surface=Surface();if(surface==null)return;
                    var origin=player.PLBNCDCFPML();var other=enemy.PLBNCDCFPML();enemy.ShiftModelPosition(new Vector3f(origin.GetX()+900-other.GetX(),0,0),true);
                    playerLife=Life(player);enemyLife=Life(enemy);Click("summon");Next(fight);break;
                case 1:
                    var pair=Actors(fight);if(pair.Length!=2)return;
                    left=pair.Single(m=>m.get_Name()==Owner+":actors/left");right=pair.Single(m=>m.get_Name()==Owner+":actors/right");
                    if(left.GetCombatTarget()!=right||right.GetCombatTarget()!=left)return;
                    if(frame-phaseFrame<30||surface.Read("status").Text.Contains("spacing"))return;
                    foreach(var model in pair)
                    {
                        Check(model.Parameters.EclipseBodyModel==Owner+":models/body","Authored body binding absent");
                        Check(model.Parameters.EclipseSkinModels.Single()==Owner+":models/sash","Authored skin binding absent");
                        Check((int)Field(model.GetModelObject(),"_NodesCount")==67,"Skin altered animation point count");
                        Check(Enumerable.Range(0,6).All(i=>model.GetModelObject().FindNodeOrParent("AuthoredSashV"+i)!=null),"Authored helper bindings missing");
                        Debug.Log("[AuthoredFighterUnity] Native composed nodes="+model.GetModelObject().NAMKCLGOPDD().Count+" animation nodes="+Field(model.GetModelObject(),"_NodesCount"));
                        Check(model.GetModelObject().Triangles.Count(t=>t.get_Name().StartsWith("AuthoredSash-",StringComparison.Ordinal))==4,"Connected authored triangles missing");
                        Check(!model.Parameters.AiControlled&&!model.Parameters.UserControlled,"Fixture did not use public manual actor controls");
                        Check(model.GetAvailableAnimations().Any(m=>m.Name==Move&&m.FileName.Contains("animations/strike")),"Owned native clip unavailable");
                    }
                    Check(left.PLBNCDCFPML().GetX()<right.PLBNCDCFPML().GetX(),"Left/right setup crossed before authored playback: "+left.PLBNCDCFPML().GetX()+" / "+right.PLBNCDCFPML().GetX());
                    leftId=Id(fight,left);rightId=Id(fight,right);
                    sashRest=Sash(left);beforeHit=Life(right);Click("left");Check(left.GetCurrentAnimation()?.Name!=Move,"HUD invoked playback recursively");Next(fight);break;
                case 2:
                    if(left.GetCurrentAnimation()?.Name==Move)
                    {
                        sashDeformed |= Sash(left).Zip(sashRest,(a,b)=>Math.Abs(a-b)).Max()>1;
                        var move=left.GetCurrentAnimation();var data=(Vector3[][])Field(move,"_AnimationContainer");
                        if(data!=null&&!readerChecked){Check(data.Length==61&&data.All(f=>f.Length==67),"Native reader changed authored clip sample/point counts");readerChecked=true;}
                    }
                    if(frame-phaseFrame>240)throw new Exception("Original strike made no contact: "+left.GetCurrentAnimation()?.Name+" health="+Life(right));
                    if(Life(right)>=beforeHit||left.GetCurrentAnimation()?.Name==Move)return;
                    CheckSource(right,left);Check(sashDeformed&&readerChecked,"Authored skin deformation or native clip reader evidence missing");
                    Check(Count(starts,leftId)==1&&Count(dealt,leftId)>0&&receipts.Contains(leftId),"Public playback callbacks/receipt missing");
                    Click("reform");Next(fight);break;
                case 3:
                    if(frame-phaseFrame<30||surface.Read("status").Text.Contains("spacing"))return;
                    Check(left.PLBNCDCFPML().GetX()<right.PLBNCDCFPML().GetX(),"Public reset did not form the mirrored contact setup");
                    beforeHit=Life(left);Click("right");Next(fight);break;
                case 4:
                    if(right.GetCurrentAnimation()?.Name==Move)
                    {
                        var animation=Field(right,"_Animation");
                        mirroredRight |= (int)Field(animation,"JMKAHNADIOI")==-1&&(bool)Field(animation,"IPGAAGOANDE");
                    }
                    if(frame-phaseFrame>240)throw new Exception("Mirrored authored strike made no contact: "+right.GetCurrentAnimation()?.Name+" health="+Life(left));
                    if(Life(left)>=beforeHit||right.GetCurrentAnimation()?.Name==Move)return;
                    CheckSource(left,right);Check(mirroredRight&&Count(starts,rightId)==1&&Count(dealt,rightId)>0&&receipts.Contains(rightId),"Mirrored callbacks/receipt or native facing missing");
                    Check(Life(player)==playerLife&&Life(enemy)==enemyLife,"Authored pair changed main fighter health");
                    autoLeftLife=Life(left);autoRightLife=Life(right);Click("auto");Next(fight);break;
                case 5:
                    if(frame-phaseFrame>720)throw new Exception("Repeated public Lua approach/playback did not make bidirectional contact");
                    if(Count(starts,leftId)<2||Count(starts,rightId)<2||Count(dealt,leftId)<2||Count(dealt,rightId)<2||Life(left)>=autoLeftLife||Life(right)>=autoRightLife)return;
                    CheckSource(left,right);CheckSource(right,left);
                    Check(Life(player)==playerLife&&Life(enemy)==enemyLife,"Repeated authored attacks changed main health");
                    Click("auto");fight.SetPaused(true);pausedPose=Pose(left);pauseFrame=frame;pauseAt=EditorApplication.timeSinceStartup;Next(fight);break;
                case 6:
                    if(EditorApplication.timeSinceStartup-pauseAt<1)return;
                    Check(fight.get_FightTimeInFrames()==pauseFrame&&Pose(left).SequenceEqual(pausedPose),"Paused authored fighter advanced");
                    new GameObject("Authored fighter acceptance capture").AddComponent<AuthoredFighterCapture>();Next(fight);break;
                case 7:
                    if(!captured)return;
                    fight.SetPaused(false);Click("dismiss");Next(fight);break;
                case 8:
                    if(Actors(fight).Length!=0)return;
                    Check(ended.Count==2&&surface!=null&&!surface.IsClosed,"Authored dismissal lifecycle incorrect");
                    Check(ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Count==0,"Lua callback failures");
                    Invoke(fight,"OBNEDPKCNKJ");Check(surface.IsClosed,"Round teardown retained authored HUD");
                    File.WriteAllText(Path.Combine(Root,"authored-fighter-result.txt"),"PASS: "+checks+" full-game authored fighter checks; actual public mod/HUD, owned body and connected weighted skin, original 61x67 clip through native reader, root-relative skin deformation, left and mirrored-right playback/receipts/contact attribution, repeated public Lua approach/playback with new health loss in both directions, unchanged main health, pause and dismissal/teardown. Standard rig/core idle/equipment and controlled spacing/profile; not arbitrary rigs, Blender/Gymnast exports or all platforms.");
                    Debug.Log("[AuthoredFighterUnity] PASS: "+checks);Finish(0);break;
            }
        }
        catch(Exception error){Debug.LogError("[AuthoredFighterUnity] FAIL: "+error);Finish(1);}
    }
    public static void Captured()=>captured=true;
    static void Log(string message,string stack,LogType type)
    {
        int index=message.IndexOf("AUTHORED-FIGHTER:",StringComparison.Ordinal);
        if(index>=0&&!message.StartsWith("[AuthoredFighterUnity]",StringComparison.Ordinal))
        {
            var tokens=message.Substring(index).Split(':');
            if(tokens.Length>=3)
            {
                if(tokens[1]=="start"){starts.TryGetValue(tokens[2],out var n);starts[tokens[2]]=n+1;}
                if(tokens[1]=="dealt"){dealt.TryGetValue(tokens[2],out var n);dealt[tokens[2]]=n+1;}
                if(tokens[1]=="receipt"&&tokens.Length>=4&&tokens[3]=="applied")receipts.Add(tokens[2]);
                if(tokens[1]=="ended")ended.Add(tokens[2]);
            }
            Debug.Log("[AuthoredFighterUnity] "+message.Substring(index));
        }
        if(entered&&(type==LogType.Exception&&(stack.Contains("Fight.")||stack.Contains("Model.")||stack.Contains("Modding"))||message.Contains("[ModActors]")||message.Contains("[ModCombat]")))failure=message;
    }
    static void Finish(int code){SessionState.SetBool(Active,false);EditorApplication.update-=Update;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
public sealed class AuthoredFighterCapture : MonoBehaviour
{
    IEnumerator Start(){yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Application.dataPath),"authored-fighter-native.png"),texture.EncodeToPNG());UnityEngine.Object.Destroy(texture);AuthoredFighterUnity.Captured();}
}
#endif
