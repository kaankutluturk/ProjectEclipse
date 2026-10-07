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
public static class ScriptedActorsUnity
{
    const string Active="Eclipse.ScriptedActorsUnity.Active";
    const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    static double started,report,pauseAt;
    static bool campaign,entered;
    static int phase,checks,phaseFrame,pauseFrame;
    static float playerHealth,enemyHealth;
    static Model[] actors;
    static ModUiSurface surface;
    static ModActorSnapshot paused;
    static string failure;
    static string Root=>Path.GetDirectoryName(Application.dataPath);
    static ScriptedActorsUnity(){if(!SessionState.GetBool(Active,false))return;started=EditorApplication.timeSinceStartup;EditorApplication.update+=Update;Application.logMessageReceived+=Log;}
    public static void Run()
    {
        if(!File.Exists(Path.Combine(Root,"scripted-actors-fixture.marker")))throw new Exception("Requires isolated actors fixture.");
        Environment.SetEnvironmentVariable("ECLIPSE_MODS_ROOT",Path.Combine(Root,"scripted-actor-mods"));
        var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-actorAcceptanceProductName");
        if(index<0||index+1>=args.Length||!System.Text.RegularExpressions.Regex.IsMatch(args[index+1],"^ScriptedActorsUnity-[0-9a-f]{32}$"))throw new Exception("Pass runner-generated isolated product.");
        PlayerSettings.companyName="EclipseAcceptance";PlayerSettings.productName=args[index+1];
        SessionState.SetBool(Active,true);EditorSceneManager.OpenScene("Assets/src/GUI/Scenes/GameLoaderScene/GameLoader.unity");
        var view=EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));view.Show();view.Focus();EditorApplication.EnterPlaymode();
    }
    static object Field(object target,string name)=>target.GetType().GetField(name,Hidden|BindingFlags.Public).GetValue(target);
    static object Invoke(object target,string name,params object[] args)=>target.GetType().GetMethod(name,Hidden|BindingFlags.Public).Invoke(target,args);
    static void Check(bool condition,string message){checks++;if(!condition)throw new Exception(message);}
    static Model[] Models(Fight fight)=>fight.ActiveModels.Where(m=>m!=null&&!(m is WeaponModel)&&(m.get_Name()??"").StartsWith("example.scripted-actors:actors/",StringComparison.Ordinal)).ToArray();
    static IModActor Actor(Fight fight,Model model)=>(IModActor)((IDictionary)Field(fight,"_eclipseActors"))[model];
    static void Next(Fight fight){phase++;phaseFrame=fight.get_FightTimeInFrames();}
    static ModUiSurface Surface()
    {
        foreach(var context in (IEnumerable)Field(ModRuntime.Scripts,"_contexts"))
        {
            var scope=context.GetType().GetProperty("UiScope").GetValue(context) as ModUiScope;
            if(scope==null||scope.Owner.Value!="example.scripted-actors")continue;
            foreach(ModUiSurface value in ((IDictionary)Field(scope,"surfaces")).Values)if(value.Id=="sparring")return value;
        }
        return null;
    }
    static void Click(string id)
    {
        var view=UnityEngine.Object.FindObjectsByType<Eclipse.UI.Modding.ModUiView>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(v=>ReferenceEquals(Field(v,"surface"),surface));
        var button=(Button)Field(((IDictionary)Field(view,"widgets"))[id],"Button");
        Check(button.gameObject.activeInHierarchy&&button.interactable,"Actor UI control unavailable");button.onClick.Invoke();
    }
    static readonly System.Collections.Generic.Dictionary<Model,int> starts=new System.Collections.Generic.Dictionary<Model,int>();
    static readonly System.Collections.Generic.Dictionary<Model,int> firstStarts=new System.Collections.Generic.Dictionary<Model,int>();
    static readonly System.Collections.Generic.Dictionary<Model,float> nextStartHealth=new System.Collections.Generic.Dictionary<Model,float>();
    static readonly System.Collections.Generic.Dictionary<string,System.Collections.Generic.HashSet<string>> choices=new System.Collections.Generic.Dictionary<string,System.Collections.Generic.HashSet<string>>();
    static readonly System.Collections.Generic.HashSet<string> born=new System.Collections.Generic.HashSet<string>();
    static readonly System.Collections.Generic.HashSet<string> hostEvents=new System.Collections.Generic.HashSet<string>();
    static readonly System.Collections.Generic.Dictionary<string,double> outgoing=new System.Collections.Generic.Dictionary<string,double>();
    static readonly System.Collections.Generic.Dictionary<string,double> expectedDamage=new System.Collections.Generic.Dictionary<string,double>();
    static readonly System.Collections.Generic.Dictionary<string,int> modifiedContacts=new System.Collections.Generic.Dictionary<string,int>();
    static readonly System.Collections.Generic.Dictionary<string,int> ended=new System.Collections.Generic.Dictionary<string,int>();
    static string[] originalIds;
    static ModFighterSnapshot Capture(Model model)
    {
        var type=typeof(Fight).GetNestedType("EclipseFighterOperations",Hidden);
        return (ModFighterSnapshot)type.GetMethod("Capture",Hidden|BindingFlags.Static).Invoke(null,new object[]{model});
    }
    static float Life(Model model)=>model.Parameters.RemainingHealthInDamageUnits;
    static Model Peer(Model model)=>actors.Single(other=>other!=model);
    static void CheckSource(Model target)
    {
        var field=typeof(Model).GetFields(Hidden|BindingFlags.Public).Single(f=>f.FieldType==typeof(Model.StrikeResult));
        Check(((Model.StrikeResult)field.GetValue(target)).AttackerModel.GetRootModel()==Peer(target),"Damage did not originate from the hostile scripted actor");
    }
    static void Observe(Fight fight,Model model)
    {
        starts[model]=0;
        Actor(fight,model).TrySnapshot(out var snapshot,out var error);Check(snapshot!=null,error);
        string id=snapshot.Id;
        Check(snapshot.Fighter.Actor!=null && snapshot.Fighter.Actor.Id==id && snapshot.Fighter.Actor.Owner=="example.scripted-actors","Actor provenance missing from native snapshot");
        model.AddEventListener(2,value=>
        {
            if(!(value is Model.EventModel notification)||!(notification.Data is InfoAnimation move)||move.Type!=InfoAnimation.AnimationKind.AnimationAttack)return;
            if(!choices.TryGetValue(id,out var names)||!names.Contains(move.Name)){failure="Native actor attack was not selected by Lua: "+id+" "+move.Name;return;}
            starts[model]++;
            if(firstStarts.TryGetValue(model,out var first)&&starts[model]==first+1)nextStartHealth[model]=Life(Peer(model));
        });
    }
    static void Update()
    {
        if(!EditorApplication.isPlaying)return;
        try
        {
            if(failure!=null)throw new Exception(failure);
            if(EditorApplication.timeSinceStartup-started>300)throw new Exception("Timed out scripted actor phase "+phase);
            if(EditorApplication.timeSinceStartup-report>20){report=EditorApplication.timeSinceStartup;Debug.Log("[ScriptedActorsUnity] Waiting entered="+entered+" phase="+phase+" choices="+choices.Count+" born="+born.Count);}
            if(!campaign&&Eclipse.UI.TitleScreen.IsOpen)
            {
                var title=UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.TitleScreen>();
                if((bool)Field(title,"splashing")||(string)Field(title,"currentPage")!="Home")return;
                Invoke(title,"BeginCampaign");var directory=SF2Paths.GetUserDataDirectory();
                Check(Eclipse.Saves.CampaignSaveSession.PreviewDirectory==null&&directory.StartsWith(Application.persistentDataPath,StringComparison.OrdinalIgnoreCase)&&Application.persistentDataPath.Contains("ScriptedActorsUnity-"),"Profile not isolated");
                var profile=XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(),"usersDefault.xml",XmlUtils.XmlSourceMode.Normal,true,XmlCryptoUtils.GetIsEncryptionEnabled());
                ((System.Xml.XmlElement)profile.SelectSingleNode("/Root/Warriors/Warrior[@ID='1']")).SetAttribute("Tutorial","END");
                Directory.CreateDirectory(directory);XmlUtils.SaveDocumentWithHash(profile,Path.Combine(directory,Constants.UsersFileName).Replace('\\','/'));campaign=true;return;
            }
            if(!entered)
            {
                if(ModRuntime.Scripts==null||Module.GetInstance()==null)return;
                var screen=Module.GetInstance().GetCurrentScreenType();if(screen!=ScreenType.ModuleDojo&&screen!=ScreenType.ModuleMap)return;
                Check(!ModRuntime.Host.HasErrors,ModRuntime.Host.FormatReport());
                Check(ModRuntime.Scripts.ActiveMods.Any(m=>m.Id.Value=="example.scripted-actors"),"Actor example did not finish registration");
                Check(ModRuntime.Host.EnabledMods.All(m=>m.Id.Value=="core"||m.Id.Value=="example.scripted-actors"),"Unexpected user mods");
                var encounter=ListSF.GetFightById(new FightIDS(ModRuntime.Scripts.Content.RuntimeFightId(DefinitionId.Parse("core:fights/zone_1/tournament/3"))));
                Check(encounter!=null,"Core encounter missing");entered=GameUtils.StartFight(encounter,false,null,true,false);return;
            }

            var fight=Fight.GetCurrentFight();if(fight==null)return;
            var player=fight.GetPlayerModel();var enemy=fight.GetEnemyModel();if(player==null||enemy==null)return;
            player.Parameters.UserControlled=false;player.Parameters.AiControlled=false;enemy.Parameters.UserControlled=false;enemy.Parameters.AiControlled=false;
            int frame=fight.get_FightTimeInFrames();if(frame<100)return;
            switch(phase)
            {
                case 0:
                    surface=Surface();if(surface==null)return;
                    var origin=Capture(player);var other=Capture(enemy);
                    enemy.ShiftModelPosition(new Vector3f((float)(origin.X+900-other.X),0,0),true);
                    playerHealth=Life(player);enemyHealth=Life(enemy);Click("summon");Next(fight);break;
                case 1:
                    actors=Models(fight);if(actors.Length!=2||!surface.Read("status").Text.StartsWith("Scripted actors: 2",StringComparison.Ordinal))return;
                    if(actors.Any(m=>!Actor(fight,m).TrySnapshot(out _,out _)))return;
                    Check(actors.All(m=>m.Parameters.AiControlled&&!m.Parameters.UserControlled&&m.Parameters.MaxLife==10),"Scripted roots are not independent AI fighters");
                    foreach(var model in actors){Observe(fight,model);Check(model.GetCombatTarget()==Peer(model),"Scripted actor did not select its hostile peer");}
                    originalIds=actors.Select(m=>{Actor(fight,m).TrySnapshot(out var snap,out _);return snap.Id;}).ToArray();
                    Next(fight);break;
                case 2:
                    if(frame-phaseFrame>900)throw new Exception("Scripted actors did not make autonomous contact: "+string.Join(",",actors.Select(m=>Life(m)+" "+m.GetCurrentAnimation()?.Name)));
                    if(!actors.All(m=>Life(m)<9.999f&&starts[m]>0))return;
                    foreach(var model in actors){CheckSource(model);firstStarts[model]=starts[model];}
                    Check(originalIds.All(id=>born.Contains(id)&&choices.ContainsKey(id)),"Independent Lua memory/identity observations missing");
                    Check(Math.Abs(Life(player)-playerHealth)<.00001&&Math.Abs(Life(enemy)-enemyHealth)<.00001,"Actor sparring changed main fighter life");
                    Debug.Log("[ScriptedActorsUnity] First contact health="+string.Join(",",actors.Select(m=>Life(m)))+" starts="+string.Join(",",actors.Select(m=>starts[m])));
                    Next(fight);break;
                case 3:
                    if(frame-phaseFrame>900)throw new Exception("No second contact after a subsequent Lua-selected attack start");
                    if(!actors.All(m=>nextStartHealth.ContainsKey(m)&&Life(Peer(m))<nextStartHealth[m]-.00001f))return;
                    foreach(var model in actors){CheckSource(model);Check(starts[model]>firstStarts[model],"Repeated contact lacked a later native start");}
                    foreach(var id in originalIds)
                    {
                        foreach(var kind in new[]{"host_spawn","host_tick","host_outgoing","host_resolving","host_received","host_dealt","host_animation"})
                            Check(hostEvents.Contains(kind+":"+id),"Actor behavior callback missing: "+kind+":"+id);
                        foreach(var kind in new[]{"HitPostCrit","PostHit"})foreach(var side in new[]{"self","opponent"})
                            Check(hostEvents.Contains("host_contact:"+id+":"+kind+":"+side),"Actor hit-phase perspective missing");
                        Check(modifiedContacts.TryGetValue(id,out var count)&&count>=2,"Outgoing bonus/incoming scaling did not match repeated native health loss");
                    }
                    Check(Math.Abs(Life(player)-playerHealth)<.00001&&Math.Abs(Life(enemy)-enemyHealth)<.00001,"Sustained sparring changed main life");
                    Debug.Log("[ScriptedActorsUnity] Repeated contact health="+string.Join(",",actors.Select(m=>Life(m)))+" starts="+string.Join(",",actors.Select(m=>starts[m])));
                    Actor(fight,actors[0]).TrySnapshot(out paused,out _);fight.SetPaused(true);pauseFrame=frame;pauseAt=EditorApplication.timeSinceStartup;Next(fight);break;
                case 4:
                    if(EditorApplication.timeSinceStartup-pauseAt<1)return;
                    Check(fight.get_FightTimeInFrames()==pauseFrame&&Actor(fight,actors[0]).TrySnapshot(out var frozen,out _)&&frozen.AgeFrames==paused.AgeFrames,"Pause advanced actor simulation");
                    fight.SetPaused(false);Click("dismiss");Next(fight);break;
                case 5:
                    if(Models(fight).Length!=0)return;
                    Check(((IDictionary)Field(fight,"_eclipseActors")).Count==0,"Dismissal retained native actors");
                    Check(originalIds.All(id=>hostEvents.Contains("host_end:"+id+":removed")),"Dismissal did not end each actor behavior");
                    Check(player.GetCombatTarget()==enemy&&enemy.GetCombatTarget()==player,"Dismissal retained actor target bindings");
                    Click("summon");Next(fight);break;
                case 6:
                    var replacements=Models(fight);if(replacements.Length!=2)return;
                    var ids=replacements.Select(m=>{Actor(fight,m).TrySnapshot(out var snap,out _);return snap?.Id;}).ToArray();
                    if(ids.Any(id=>id==null||!born.Contains(id)))return;
                    Check(ids.All(id=>!originalIds.Contains(id)),"Replacement actors reused runtime identities");
                    Check(born.Count==4,"Replacement Lua controller memory was not fresh");
                    Check(ids.All(id=>hostEvents.Contains("host_spawn:"+id)),"Replacement behavior instances did not start with fresh state");
                    Invoke(fight,"Surrender");Next(fight);break;
                case 7:
                    if(Models(fight).Length!=0)return;
                    Check(((IDictionary)Field(fight,"_eclipseActors")).Count==0&&surface.IsClosed,"Surrender retained actors or HUD");
                    Check(ended.Count==4&&ended.Values.All(count=>count==1),"Surrender did not end replacement actor behaviors exactly once");
                    File.WriteAllText(Path.Combine(Root,"scripted-actors-result.txt"),"PASS: "+checks+" full-game scripted actor checks: actual Lua/HUD registration and spawning, independent AI and behavior state/identity, Lua-selected native attacks, hit-phase perspectives, outgoing bonus/incoming mitigation matched to native health loss, repeated contact after subsequent starts, unchanged main health, pause, dismissal, fresh replacement instances and exactly-once end/surrender cleanup. Core Skeleton/knives and controlled spacing/input/profile only; arbitrary rigs, unsupported actor-host operations, exports, raids and multiplayer remain open.");
                    Debug.Log("[ScriptedActorsUnity] PASS: "+checks);Finish(0);break;
            }
        }
        catch(Exception error){Debug.LogError("[ScriptedActorsUnity] FAIL: "+error);Finish(1);}
    }
    static void Log(string message,string stack,LogType type)
    {
        int index=message.IndexOf("SCRIPTED-ACTOR:",StringComparison.Ordinal);
        if(index>=0&&!message.StartsWith("[ScriptedActorsUnity]",StringComparison.Ordinal))
        {
            var tokens=message.Substring(index).Split(':');
            if(tokens.Length>=4&&tokens[1]=="born")born.Add(tokens[2]);
            if(tokens.Length>=6&&tokens[1]=="choose")
            {if(!choices.TryGetValue(tokens[2],out var names))choices.Add(tokens[2],names=new System.Collections.Generic.HashSet<string>());names.Add(string.Join(":",tokens.Skip(5)));}
            if(tokens.Length>=3&&tokens[1].StartsWith("host_",StringComparison.Ordinal))
            {
                hostEvents.Add(string.Join(":",tokens.Skip(1)));
                hostEvents.Add(tokens[1]+":"+tokens[2]);
                if(tokens[1]=="host_end"){ended.TryGetValue(tokens[2],out var prior);ended[tokens[2]]=prior+1;}
                double Number(int position)=>double.Parse(tokens[position],System.Globalization.CultureInfo.InvariantCulture);
                if(tokens[1]=="host_outgoing")outgoing[tokens[2]]=Number(3)+Number(4);
                if(tokens[1]=="host_resolving")
                {
                    if(!outgoing.TryGetValue(tokens[3],out var value)||Math.Abs(value-Number(4))>.000005)failure="Actor incoming callback did not observe the outgoing bonus";
                    expectedDamage[tokens[2]]=Number(4)*.5;
                }
                if(tokens[1]=="host_received")
                {
                    double damage=Number(3),before=Number(4),after=Number(5);
                    if(!expectedDamage.TryGetValue(tokens[2],out var expected)||Math.Abs(expected-damage)>.000005||Math.Abs(before-after-damage)>.000005)
                        failure="Actor Lua damage modifiers did not match native applied health loss";
                    modifiedContacts.TryGetValue(tokens[2],out var count);modifiedContacts[tokens[2]]=count+1;
                }
            }
            Debug.Log("[ScriptedActorsUnity] "+message.Substring(index));
        }
        if(entered&&(type==LogType.Exception&&(stack.Contains("Fight.")||stack.Contains("Model."))||message.Contains("[ModAI]")||message.Contains("[ModActors]")||message.Contains("[ModCombat]")||message.Contains("Native actor initialization failed:")||message.Contains("Native actor creation failed:")))failure=message;
    }
    static void Finish(int code){SessionState.SetBool(Active,false);EditorApplication.update-=Update;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
#endif
