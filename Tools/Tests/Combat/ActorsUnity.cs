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
public static class ActorsUnity
{
    const string Active="Eclipse.ActorsUnity.Active";
    const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    static double started,report,pauseAt;
    static bool campaign,entered,contract;
    static int phase,checks,phaseFrame,pauseFrame,actionStep,actionFrame;
    static bool applied,healthCommand,motionCommand;
    static bool expiredReference,forgedTarget,nonfiniteMotion,queryLimit;
    static double actorX;
    static float playerHealth,enemyHealth;
    static Model[] actors;
    static ModUiSurface surface;
    static ModActorSnapshot paused;
    static string failure;
    static readonly System.Collections.Generic.HashSet<string> events=new System.Collections.Generic.HashSet<string>();
    static string Root=>Path.GetDirectoryName(Application.dataPath);
    static ActorsUnity(){if(!SessionState.GetBool(Active,false))return;started=EditorApplication.timeSinceStartup;EditorApplication.update+=Update;Application.logMessageReceived+=Log;}
    public static void Run()
    {
        if(!File.Exists(Path.Combine(Root,"actors-fixture.marker")))throw new Exception("Requires isolated actors fixture.");
        Environment.SetEnvironmentVariable("ECLIPSE_MODS_ROOT",Path.Combine(Root,"actor-mods"));
        var path=Path.Combine(Root,"actor-mods/example.actor-companions/scripts/main.lua");
        string shipped=File.ReadAllText(path);int patchAt=shipped.IndexOf("for _, fight in ipairs",StringComparison.Ordinal);
        if(patchAt<0)throw new Exception("Shipped actor example patch block missing.");
        File.WriteAllText(path,shipped.Substring(0,patchAt)+@"
local retained, inspected
local probe = sf2.behaviors.register { id='contract_probe', on_tick=function(_, fighter)
    if retained then retained:snapshot(); error('Retained actor handle escaped its callback') end
    local list = fighter:actors()
    if not list or #list==0 then return end
    local first = list[1]
    local original = first:snapshot()
    if not original then return end
    original.health = -999; original.position.x = 0/0
    local fresh = first:snapshot()
    assert(fresh.health >= 0 and fresh.position.x == fresh.position.x, 'Actor observation was not copied')
    assert(not first:set_target(first), 'Actor can target itself')
    assert(not first:set_target(fresh.team), 'Actor can target a friendly main fighter')
    retained = first
    if not inspected then
        for i=1,31 do fighter:actors() end
        inspected=true; sf2.log.info('ACTOR-CONTRACT:passed')
        fighter:actors(); error('Actor callback query limit escaped')
    end
end }
local probe_rule = sf2.rules.behavior {id='contract_probe', behavior=probe, target=sf2.rules.PLAYER}
local forged = sf2.behaviors.register { id='forged_probe', on_tick=function(_, fighter)
    local actors=fighter:actors(); if actors and actors[1] then actors[1]:set_target({}); error('Forged target accepted') end
end }
local nonfinite = sf2.behaviors.register { id='nonfinite_probe', on_tick=function(_, fighter)
    local actors=fighter:actors(); if actors and actors[1] then actors[1]:move_by(0/0,0); error('Nonfinite actor motion accepted') end
end }
local forged_rule=sf2.rules.behavior {id='forged_probe',behavior=forged,target=sf2.rules.PLAYER}
local nonfinite_rule=sf2.rules.behavior {id='nonfinite_probe',behavior=nonfinite,target=sf2.rules.PLAYER}
"+shipped.Substring(patchAt).Replace("append_rules = { rule }","append_rules = { rule, probe_rule, forged_rule, nonfinite_rule }"));
        var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-actorAcceptanceProductName");
        if(index<0||index+1>=args.Length||!System.Text.RegularExpressions.Regex.IsMatch(args[index+1],"^ActorsUnity-[0-9a-f]{32}$"))throw new Exception("Pass runner-generated isolated product.");
        PlayerSettings.companyName="EclipseAcceptance";PlayerSettings.productName=args[index+1];
        SessionState.SetBool(Active,true);EditorSceneManager.OpenScene("Assets/src/GUI/Scenes/GameLoaderScene/GameLoader.unity");
        var view=EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));view.Show();view.Focus();EditorApplication.EnterPlaymode();
    }
    static object Field(object target,string name)=>target.GetType().GetField(name,Hidden|BindingFlags.Public).GetValue(target);
    static object Invoke(object target,string name,params object[] args)=>target.GetType().GetMethod(name,Hidden|BindingFlags.Public).Invoke(target,args);
    static void Check(bool condition,string message){checks++;if(!condition)throw new Exception(message);}
    static Model[] Models(Fight fight)=>fight.LNDLFINJHDB.Where(m=>m!=null&&!(m is WeaponModel)&&(m.get_Name()??"").StartsWith("example.actor-companions:actors/",StringComparison.Ordinal)).ToArray();
    static IModActor Actor(Fight fight,Model model)=>(IModActor)((IDictionary)Field(fight,"_eclipseActors"))[model];
    static void Next(Fight fight){phase++;phaseFrame=fight.get_FightTimeInFrames();}
    static ModUiSurface Surface()
    {
        foreach(var context in (IEnumerable)Field(ModRuntime.Scripts,"_contexts"))
        {
            var scope=context.GetType().GetProperty("UiScope").GetValue(context) as ModUiScope;
            if(scope==null||scope.Owner.Value!="example.actor-companions")continue;
            foreach(ModUiSurface value in ((IDictionary)Field(scope,"surfaces")).Values)if(value.Id=="companions")return value;
        }
        return null;
    }
    static void Click(string id)
    {
        var view=UnityEngine.Object.FindObjectsByType<Eclipse.UI.Modding.ModUiView>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(v=>ReferenceEquals(Field(v,"surface"),surface));
        var button=(Button)Field(((IDictionary)Field(view,"widgets"))[id],"Button");
        Check(button.gameObject.activeInHierarchy&&button.interactable,"Actor UI control unavailable");button.onClick.Invoke();
    }
    static void Update()
    {
        if(!EditorApplication.isPlaying)return;
        try
        {
            if(failure!=null)throw new Exception(failure);
            if(EditorApplication.timeSinceStartup-started>300)throw new Exception("Timed out actor phase "+phase);
            if(EditorApplication.timeSinceStartup-report>20){report=EditorApplication.timeSinceStartup;Debug.Log("[ActorsUnity] Waiting entered="+entered+" phase="+phase);}
            if(!campaign&&Eclipse.UI.TitleScreen.IsOpen)
            {
                var title=UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.TitleScreen>();
                if((bool)Field(title,"splashing")||(string)Field(title,"currentPage")!="Home")return;
                Invoke(title,"BeginCampaign");var directory=SF2Paths.GetUserDataDirectory();
                Check(Eclipse.Saves.CampaignSaveSession.PreviewDirectory==null&&directory.StartsWith(Application.persistentDataPath,StringComparison.OrdinalIgnoreCase)&&Application.persistentDataPath.Contains("ActorsUnity-"),"Profile not isolated");
                var profile=XmlUtils.OpenXMLDocument(SF2Paths.KKIDGPBOBNI(),"usersDefault.xml",XmlUtils.EBLFEPIOMOL.Normal,true,XmlCryptoUtils.NNLGALNDJCL());
                ((System.Xml.XmlElement)profile.SelectSingleNode("/Root/Warriors/Warrior[@ID='1']")).SetAttribute("Tutorial","END");
                Directory.CreateDirectory(directory);XmlUtils.ONLDJNLKKAL(profile,Path.Combine(directory,Constants.OJMIJINKBPJ).Replace('\\','/'));campaign=true;return;
            }
            if(!entered)
            {
                if(ModRuntime.Scripts==null||Module.GetInstance()==null)return;
                var screen=Module.GetInstance().GetCurrentScreenType();if(screen!=ScreenType.ModuleDojo&&screen!=ScreenType.ModuleMap)return;
                Check(!ModRuntime.Host.HasErrors,ModRuntime.Host.FormatReport());
                Check(ModRuntime.Scripts.ActiveMods.Any(m=>m.Id.Value=="example.actor-companions"),"Actor example did not finish registration");
                Check(ModRuntime.Host.EnabledMods.All(m=>m.Id.Value=="core"||m.Id.Value=="example.actor-companions"),"Unexpected user mods");
                var encounter=ListSF.CHMCKGCDGCM(new FightIDS(ModRuntime.Scripts.Content.RuntimeFightId(DefinitionId.Parse("core:fights/zone_1/tournament/3"))));
                Check(encounter!=null,"Core encounter missing");entered=GameUtils.StartFight(encounter,false,null,true,false);return;
            }
            var fight=Fight.GetCurrentFight();if(fight==null||fight.get_FightTimeInFrames()<100)return;
            var player=fight.GetPlayerModel();var enemy=fight.GetEnemyModel();if(player==null||enemy==null)return;
            player.Parameters.UserControlled=false;player.Parameters.AiControlled=false;enemy.Parameters.AiControlled=false;
            int frame=fight.get_FightTimeInFrames();
            switch(phase)
            {
                case 0:
                    surface=Surface();if(surface==null)return;
                    var origin=player.PLBNCDCFPML();enemy.ShiftModelPosition(new Vector3f(origin.GetX()+700-enemy.PLBNCDCFPML().GetX(),0,0),true);
                    playerHealth=player.KKMCHCNOHMB();enemyHealth=enemy.KKMCHCNOHMB();Click("summon");Next(fight);break;
                case 1:
                    actors=Models(fight);if(actors.Length!=2||!surface.Read("status").Text.StartsWith("Actors: 2",StringComparison.Ordinal))return;
                    Check(actors.All(m=>m.GetRootModel()==m&&m.Parameters.MaxLife==1&&m.Parameters.AiControlled&&!m.Parameters.UserControlled),"Actors lack independent health/AI root state");
                    Check(actors[0].Parameters!=actors[1].Parameters&&actors[0].Parameters!=player.Parameters,"Actor parameters are shared");
                    Check(actors.Count(m=>m.Parameters.IsPlayer)==1,"Relative actor teams missing");
                    foreach(var actor in actors)
                    {
                        Check(actor.GetRenderObject()!=null&&actor.GetRenderObject().activeInHierarchy,"Actor not rendered");
                        Check(Actor(fight,actor).TrySnapshot(out var snapshot,out var error),error);
                        Check(snapshot.Id.StartsWith("a")&&snapshot.Fighter.Health==1&&snapshot.AgeFrames<10,"Birth snapshot/receipt state invalid");
                        Debug.Log("[ActorsUnity] Actor "+snapshot.Id+" team="+snapshot.Team+" target="+snapshot.Target+" x="+snapshot.Fighter.X+" max="+snapshot.Fighter.MaxHealth);
                    }
                    Check(player.GetCombatTarget()!=actors.Single(m=>m.Parameters.IsPlayer)&&enemy.GetCombatTarget()!=actors.Single(m=>!m.Parameters.IsPlayer),"Main fighter targets friendly actor");
                    playerHealth=player.KKMCHCNOHMB();enemyHealth=enemy.KKMCHCNOHMB();
                    Next(fight);break;
                case 2:
                    if(actionStep==0)
                    {
                        if(frame-phaseFrame<15)return;
                        foreach(var actor in actors)
                        {
                            var ai=Field(actor,"HJOGNGDMAKJ");
                            Check(Field(ai,"CGPDPHJIDPA")!=null&&Field(ai,"COKFBIJAFLH")!=null,"Actor AI lacks own/target animation observations");
                            Check(!actor._Enemies.Contains(actor.Parameters.IsPlayer?player:enemy),"Actor collision list contains friendly main fighter");
                            actor.Parameters.AiControlled=false;
                        }
                        playerHealth=player.KKMCHCNOHMB();enemyHealth=enemy.KKMCHCNOHMB();
                        Actor(fight,actors[0]).TrySnapshot(out var beforeMotion,out _);actorX=beforeMotion.Fighter.X;
                        motionCommand=false;Click("guide");actionStep=1;actionFrame=frame;return;
                    }
                    if(actionStep==1)
                    {
                        if(!motionCommand||frame-actionFrame<2)return;
                        Check(Actor(fight,actors[0]).TrySnapshot(out var moved,out _)&&Math.Abs(moved.Fighter.X-actorX)>1,"Actual Lua motion did not displace actor");
                        healthCommand=false;Click("damage");actionStep=2;actionFrame=frame;return;
                    }
                    if(actionStep==2)
                    {
                        if(!healthCommand||frame-actionFrame<2)return;
                        Check(actors[0].KKMCHCNOHMB()<1&&actors[0].KKMCHCNOHMB()>0,"Actual Lua health command did not apply");
                        Check(Math.Abs(player.KKMCHCNOHMB()-playerHealth)<.00001&&Math.Abs(enemy.KKMCHCNOHMB()-enemyHealth)<.00001,"Actor health command mutated a main fighter");
                        applied=false;Click("strike");actionStep=3;actionFrame=frame;return;
                    }
                    if(actionStep==3)
                    {
                        if(!applied)return;
                        Check(actors[0].GetCurrentAnimation().Name=="example.actor-companions:moves/companion_strike","Actual Lua actor playback did not start owned move");
                        Check(actors[0].GetCombatTarget()==actors[1],"Actual Lua target command did not retain hostile actor");
                        Click("defeat");actionStep=4;actionFrame=frame;return;
                    }
                    if(!events.Contains("died"))return;
                    Check(Models(fight).Length==1&&((IDictionary)Field(fight,"_eclipseActors")).Count==1,"Death retained owned native root");
                    Click("dismiss");Next(fight);break;
                case 3:
                    if(Models(fight).Length!=0)return;
                    Check(actors.All(a=>a.GetRenderObject()==null||!a.GetRenderObject().activeInHierarchy),"Dismissal retained actor rendering");
                    Check(player.GetCombatTarget()==enemy&&enemy.GetCombatTarget()==player,"Dismissal failed to restore canonical targets");
                    Click("brief");Next(fight);break;
                case 4:
                    actors=Models(fight);if(actors.Length!=1||!surface.Read("status").Text.StartsWith("Actors: 1",StringComparison.Ordinal))return;
                    Check(Actor(fight,actors[0]).TrySnapshot(out paused,out var pauseError),pauseError);
                    fight.SetPaused(true);pauseFrame=frame;pauseAt=EditorApplication.timeSinceStartup;Next(fight);break;
                case 5:
                    if(EditorApplication.timeSinceStartup-pauseAt<2)return;
                    Check(fight.get_FightTimeInFrames()==pauseFrame&&Actor(fight,actors[0]).TrySnapshot(out var frozen,out _)&&frozen.AgeFrames==paused.AgeFrames,"Pause advanced actor lifetime");
                    fight.SetPaused(false);Next(fight);break;
                case 6:
                    if(Models(fight).Length!=0)return;
                    Check(frame-phaseFrame>=45,"Actor TTL expired before simulation lifetime");
                    Check(((IDictionary)Field(fight,"_eclipseActors")).Count==0,"TTL retained ownership registry");
                    Click("summon");Next(fight);break;
                case 7:
                    actors=Models(fight);if(actors.Length!=2||!surface.Read("status").Text.StartsWith("Actors: 2",StringComparison.Ordinal))return;
                    Invoke(fight,"OBNEDPKCNKJ");Next(fight);break;
                case 8:
                    if(Models(fight).Length!=0)return;
                    Check(((IDictionary)Field(fight,"_eclipseActors")).Count==0&&surface.IsClosed,"Surrender retained actors or owned HUD");
                    Check(contract&&events.Contains("spawned")&&events.Contains("removed")&&events.Contains("expired"),"Actual Lua contract/lifecycle observations missing");
                    Check(expiredReference&&forgedTarget&&nonfiniteMotion&&queryLimit,"Actual Lua negative contract errors missing");
                    File.WriteAllText(Path.Combine(Root,"actors-result.txt"),"PASS: "+checks+" full-game typed actor checks: actual Lua registration/spawn/query/copy/scope/target/motion/health/playback, native independent roots/render/health/AI observations/teams, queued death/removal, pause/TTL and surrender cleanup. Controlled core rig/tactic/spacing/input and unique profile; autonomous contact, arbitrary rigs, actor behavior hosts, forms/owner-disable, exports, raids, multiplayer and rollback remain separate.");
                    Debug.Log("[ActorsUnity] PASS: "+checks+" checks");Finish(0);break;
            }
        }
        catch(Exception error){Debug.LogError("[ActorsUnity] FAIL: "+error);Finish(1);}
    }
    static void Log(string message,string stack,LogType type)
    {
        if(message.StartsWith("[ModCombat]",StringComparison.Ordinal))
        {
            if(message.Contains("contract_probe")&&message.Contains("Actor references have expired"))expiredReference=true;
            if(message.Contains("contract_probe")&&message.Contains("At most 32 combined actor/event queries"))queryLimit=true;
            if(message.Contains("forged_probe")&&message.Contains("Target actor must come from this callback"))forgedTarget=true;
            if(message.Contains("nonfinite_probe")&&message.Contains("Actor displacement must be finite"))nonfiniteMotion=true;
        }
        if(!message.StartsWith("[ActorsUnity]",StringComparison.Ordinal)&&(message.Contains("ACTOR-CONTRACT:")||message.Contains("ACTOR-EVENT:")||message.Contains("ACTOR-COMMAND:")))
        {
            if(message.Contains("ACTOR-CONTRACT:passed"))contract=true;
            if(message.Contains("ACTOR-COMMAND:applied"))applied=true;
            if(message.Contains("ACTOR-COMMAND:health"))healthCommand=true;
            if(message.Contains("ACTOR-COMMAND:move"))motionCommand=true;
            foreach(var kind in new[]{"spawned","removed","expired","died"})if(message.Contains("ACTOR-EVENT:"+kind+":"))events.Add(kind);
            Debug.Log("[ActorsUnity] "+message);
        }
        if(entered&&(type==LogType.Exception&&(stack.Contains("Fight.")||stack.Contains("Model."))||message.Contains("[ModActors]")||message.Contains("Native actor initialization failed:")||message.Contains("Native actor creation failed:")))failure=message;
    }
    static void Finish(int code){SessionState.SetBool(Active,false);EditorApplication.update-=Update;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
#endif
