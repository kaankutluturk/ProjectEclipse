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
public static class RangedActorsUnity
{
    const string Active="Eclipse.RangedActorsUnity.Active";
    const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    static double started,report,pauseAt;
    static bool campaign,entered;
    static int phase,checks,phaseFrame,pauseFrame;
    static double playerHealth,enemyHealth;
    static Model[] actors;
    static WeaponModel[] frozenChildren;
    static ModUiSurface surface;
    static string failure;
    static Fight accepted;
    static Model retiring;
    static bool cancelledReceipt;
    static string cancelledId,cancelledError;
    static readonly HashSet<string> born=new HashSet<string>(),applied=new HashSet<string>(),contacts=new HashSet<string>();
    static readonly Dictionary<string,int> hits=new Dictionary<string,int>(),ended=new Dictionary<string,int>();
    static readonly Dictionary<string,string> childOwners=new Dictionary<string,string>();
    static readonly Dictionary<string,int> firstHits=new Dictionary<string,int>();
    static readonly Dictionary<WeaponModel,ModFighterSnapshot> frozen=new Dictionary<WeaponModel,ModFighterSnapshot>();
    static string Root=>Path.GetDirectoryName(Application.dataPath);
    static RangedActorsUnity(){if(!SessionState.GetBool(Active,false))return;started=EditorApplication.timeSinceStartup;EditorApplication.update+=Update;Application.logMessageReceived+=Log;}
    public static void Run()
    {
        if(!File.Exists(Path.Combine(Root,"ranged-actors-fixture.marker")))throw new Exception("Requires isolated ranged actors fixture.");
        Environment.SetEnvironmentVariable("ECLIPSE_MODS_ROOT",Path.Combine(Root,"ranged-actor-mods"));
        var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-actorAcceptanceProductName");
        if(index<0||index+1>=args.Length||!System.Text.RegularExpressions.Regex.IsMatch(args[index+1],"^RangedActorsUnity-[0-9a-f]{32}$"))throw new Exception("Pass runner-generated isolated product.");
        PlayerSettings.companyName="EclipseAcceptance";PlayerSettings.productName=args[index+1];
        SessionState.SetBool(Active,true);EditorSceneManager.OpenScene("Assets/src/GUI/Scenes/GameLoaderScene/GameLoader.unity");
        var view=EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));view.Show();view.Focus();EditorApplication.EnterPlaymode();
    }
    static object Field(object target,string name)=>target.GetType().GetField(name,Hidden|BindingFlags.Public).GetValue(target);
    static object Invoke(object target,string name,params object[] args)=>target.GetType().GetMethod(name,Hidden|BindingFlags.Public).Invoke(target,args);
    static void Check(bool condition,string message){checks++;if(!condition)throw new Exception(message);}
    static Model[] Models(Fight fight)=>fight.LNDLFINJHDB.Where(m=>m!=null&&!(m is WeaponModel)&&m!=fight.GetPlayerModel()&&m!=fight.GetEnemyModel()).ToArray();
    static WeaponModel[] Children(Fight fight)=>fight.LNDLFINJHDB.OfType<WeaponModel>().Where(m=>m.get_Name()=="example.ranged-actors.dart").ToArray();
    static IModActor Actor(Fight fight,Model model)=>(IModActor)((IDictionary)Field(fight,"_eclipseActors"))[model];
    static ModFighterSnapshot Capture(Model model)=>(ModFighterSnapshot)typeof(Fight).GetNestedType("EclipseFighterOperations",BindingFlags.NonPublic).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{model});
    static ModUiSurface Surface()
    {
        foreach(var context in (IEnumerable)Field(ModRuntime.Scripts,"_contexts"))
        {
            var scope=context.GetType().GetProperty("UiScope").GetValue(context) as ModUiScope;
            if(scope==null||scope.Owner.Value!="example.ranged-actors")continue;
            foreach(ModUiSurface value in ((IDictionary)Field(scope,"surfaces")).Values)if(value.Id=="ranged")return value;
        }
        return null;
    }
    static void Click(string id)
    {
        var view=UnityEngine.Object.FindObjectsByType<Eclipse.UI.Modding.ModUiView>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(v=>ReferenceEquals(Field(v,"surface"),surface));
        var button=(Button)Field(((IDictionary)Field(view,"widgets"))[id],"Button");
        Check(button.gameObject.activeInHierarchy&&button.interactable,"Ranged UI control unavailable");button.onClick.Invoke();
    }
    static void Next(Fight fight){phase++;phaseFrame=fight.get_FightTimeInFrames();}
    static string Id(Fight fight,Model model){if(!Actor(fight,model).TrySnapshot(out var snap,out var error))throw new Exception(error);return snap.Id;}
    static void Update()
    {
        try
        {
            if(failure!=null)throw new Exception(failure);
            if(EditorApplication.timeSinceStartup-started>300)throw new Exception("Timed out phase "+phase);
            if(EditorApplication.timeSinceStartup-report>15){report=EditorApplication.timeSinceStartup;Debug.Log("[RangedActorsUnity] Waiting entered="+entered+" phase="+phase+" born="+born.Count+" applied="+applied.Count);}
            if(!EditorApplication.isPlaying)return;
            if(!campaign)
            {
                var title=UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.TitleScreen>();if(!title)return;
                if((bool)Field(title,"splashing")||(string)Field(title,"currentPage")!="Home")return;
                Invoke(title,"BeginCampaign");var directory=SF2Paths.GetUserDataDirectory();
                Check(directory.StartsWith(Application.persistentDataPath,StringComparison.OrdinalIgnoreCase)&&Application.persistentDataPath.Contains("RangedActorsUnity-"),"Profile not isolated");
                var profile=XmlUtils.OpenXMLDocument(SF2Paths.KKIDGPBOBNI(),"usersDefault.xml",XmlUtils.EBLFEPIOMOL.Normal,true,XmlCryptoUtils.NNLGALNDJCL());
                ((System.Xml.XmlElement)profile.SelectSingleNode("/Root/Warriors/Warrior[@ID='1']")).SetAttribute("Tutorial","END");
                Directory.CreateDirectory(directory);XmlUtils.ONLDJNLKKAL(profile,Path.Combine(directory,Constants.OJMIJINKBPJ).Replace('\\','/'));campaign=true;return;
            }
            if(!entered)
            {
                if(ModRuntime.Scripts==null||Module.GetInstance()==null)return;
                var screen=Module.GetInstance().GetCurrentScreenType();if(screen!=ScreenType.ModuleDojo&&screen!=ScreenType.ModuleMap)return;
                Check(!ModRuntime.Host.HasErrors,ModRuntime.Host.FormatReport());
                Check(ModRuntime.Scripts.ActiveMods.Any(m=>m.Id.Value=="example.ranged-actors"),"Ranged example did not finish registration");
                Check(ModRuntime.Host.EnabledMods.All(m=>m.Id.Value=="core"||m.Id.Value=="example.ranged-actors"),"Unexpected user mods");
                var encounter=ListSF.CHMCKGCDGCM(new FightIDS(ModRuntime.Scripts.Content.RuntimeFightId(DefinitionId.Parse("core:fights/zone_1/tournament/3"))));
                Check(encounter!=null,"Core encounter missing");entered=GameUtils.StartFight(encounter,false,null,true,false);return;
            }
            var fight=Fight.GetCurrentFight();if(fight==null)return;accepted=fight;
            var player=fight.GetPlayerModel();var enemy=fight.GetEnemyModel();if(player==null||enemy==null)return;
            player.Parameters.UserControlled=false;player.Parameters.AiControlled=false;enemy.Parameters.UserControlled=false;enemy.Parameters.AiControlled=false;
            int frame=fight.get_FightTimeInFrames();if(frame<100)return;
            switch(phase)
            {
                case 0:
                    surface=Surface();if(surface==null)return;
                    var origin=Capture(player);var other=Capture(enemy);
                    enemy.ShiftModelPosition(new Vector3f((float)(origin.X+900-other.X),0,0),true);
                    playerHealth=Capture(player).Health;enemyHealth=Capture(enemy).Health;
                    ModRuntime.Scripts.CallbackDiagnostics.Recording=true;Click("summon");Next(fight);break;
                case 1:
                    actors=Models(fight);if(actors.Length!=2||actors.Any(m=>!Actor(fight,m).TrySnapshot(out _,out _)))return;
                    Check(actors.All(m=>!m.Parameters.AiControlled&&!m.Parameters.UserControlled),"Scenario must use actor Lua abilities without fixture/native AI attacks");
                    foreach(var model in actors)Check(born.Contains(Id(fight,model)),"Actor spawn state not initialized");
                    Next(fight);break;
                case 2:
                    if(frame-phaseFrame>600)throw new Exception("No bidirectional actor-authored projectile contact");
                    if(!actors.All(m=>hits.TryGetValue(Id(fight,m),out var count)&&count>0))return;
                    foreach(var model in actors)
                    {
                        var id=Id(fight,model);Check(applied.Contains(id),"No applied actor child receipt");firstHits[id]=hits[id];
                        foreach(var kind in new[]{"HitPostCrit","PostHit","DamageDealing","DamageResolving","DamageDealt","DamageReceived"})Check(contacts.Contains(kind+":"+id),"Missing projectile hit callback "+kind+":"+id);
                    }
                    Check(Capture(player).Health==playerHealth&&Capture(enemy).Health==enemyHealth,"Actor children changed main health");
                    Debug.Log("[RangedActorsUnity] First projectile contact pools="+string.Join(",",actors.Select(m=>Capture(m).Health)));
                    Next(fight);break;
                case 3:
                    if(frame-phaseFrame>600)throw new Exception("No later projectile volley contact");
                    if(!actors.All(m=>hits[Id(fight,m)]>firstHits[Id(fight,m)]))return;
                    Check(actors.All(m=>Capture(m).Health<10),"Native actor health did not change");
                    Check(childOwners.Values.Distinct().Count()==2,"Projectile IDs were not independently scoped to both actors");
                    Click("hold");Next(fight);break;
                case 4:
                    if(!surface.Read("status").Text.Contains("holding"))return;
                    if(Children(fight).Length!=0)return;
                    Click("hold");Next(fight);break;
                case 5:
                    frozenChildren=Children(fight);if(frozenChildren.Length<2)return;
                    foreach(var child in frozenChildren){Check(actors.Contains(child.GetRootModel()),"Child linked to main instead of actual actor caster");Check(child.GetRenderObject()!=null&&child.GetRenderObject().activeInHierarchy,"Actor child not rendered");frozen[child]=Capture(child);}
                    fight.SetPaused(true);pauseFrame=frame;pauseAt=EditorApplication.timeSinceStartup;Next(fight);break;
                case 6:
                    if(EditorApplication.timeSinceStartup-pauseAt<1)return;
                    Check(fight.get_FightTimeInFrames()==pauseFrame,"Pause advanced projectile clock");
                    foreach(var child in frozenChildren)Check(Capture(child).X==frozen[child].X&&Capture(child).Y==frozen[child].Y,"Pause moved actor child");
                    fight.SetPaused(false);Click("dismiss");Next(fight);break;
                case 7:
                    if(Models(fight).Length!=0||Children(fight).Length!=0)return;
                    Check(((IDictionary)Field(fight,"_eclipseProjectiles")).Count==0&&((IList)Field(fight,"_eclipseProjectileSpawns")).Count==0,"Actor retirement retained projectile references/reservations");
                    Check(ended.Count==2&&ended.Values.All(count=>count==1),"Dismissal did not retire each actor once");
                    Click("summon");Next(fight);break;
                case 8:
                    if(Models(fight).Length!=2||Children(fight).Length<2)return;
                    Check(born.Count==4,"Replacement actor state/identity not fresh");
                    // Controlled boundary race through the production queue and
                    // actor reference, after public Lua has supplied live children.
                    retiring=Models(fight)[0];
                    Action<string,string> receipt=(id,error)=>{cancelledReceipt=true;cancelledId=id;cancelledError=error;};
                    var request=new object[]{retiring,ModId.Parse("example.ranged-actors"),DefinitionId.Parse("example.ranged-actors:projectiles/dart"),0d,0d,0d,receipt,null};
                    Check((bool)typeof(Fight).GetMethod("TryQueueEclipseProjectileSpawn",Hidden).Invoke(fight,request),"Actor boundary spawn rejected: "+request[7]);
                    Check(Actor(fight,retiring).TryRemove(out var removeError),removeError);
                    Next(fight);break;
                case 9:
                    if(Models(fight).Length!=1||Children(fight).Any(child=>child.GetRootModel()==retiring))return;
                    Check(cancelledReceipt&&cancelledId==null&&cancelledError!=null,"Queued birth survived actor retirement or did not settle failed receipt");
                    Check(((IList)Field(fight,"_eclipseProjectileSpawns")).Count==0,"Retirement left reserved spawn capacity");
                    Check(ended.Count==3&&ended.Values.All(count=>count==1),"Single actor retirement ended siblings or duplicated callbacks");
                    Check(Children(fight).Any(child=>child.GetRootModel()==Models(fight)[0]),"Retiring one actor removed its sibling's live children");
                    Invoke(fight,"OBNEDPKCNKJ");Next(fight);break;
                case 10:
                    if(Models(fight).Length!=0||Children(fight).Length!=0)return;
                    Check(((IDictionary)Field(fight,"_eclipseProjectiles")).Count==0&&((IList)Field(fight,"_eclipseProjectileSpawns")).Count==0&&surface.IsClosed,"Surrender retained projectile ownership, births or HUD");
                    Check(ended.Count==4&&ended.Values.All(count=>count==1),"Surrender did not end each replacement once");
                    Check(ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Count==0,"Lua failures recorded");
                    File.WriteAllText(Path.Combine(Root,"ranged-actors-result.txt"),"PASS: "+checks+" native ranged actor checks: actual public Lua/HUD spawning, actor-root receipts/query isolation, independent state/IDs, native child linkage/rendering/flight/contact, both hit-phase perspectives, repeated bidirectional damage, unchanged main life, hold/resume, pause, live-child dismissal/replacement, controlled queued-birth retirement/failed receipt with sibling children preserved, and surrender cleanup. Core rig/weapon, controlled input/spacing/profile and final boundary race only; arbitrary content, death/TTL races, exports, raids and multiplayer remain open.");
                    Debug.Log("[RangedActorsUnity] PASS: "+checks);Finish(0);break;
            }
        }
        catch(Exception error){Debug.LogError("[RangedActorsUnity] FAIL: "+error);Finish(1);}
    }
    static void Log(string message,string stack,LogType type)
    {
        int index=message.IndexOf("RANGED-ACTOR:",StringComparison.Ordinal);
        if(index>=0&&!message.StartsWith("[RangedActorsUnity]",StringComparison.Ordinal))
        {
            var tokens=message.Substring(index).Split(':');
            if(tokens[1]=="born")born.Add(tokens[2]);
            if(tokens[1]=="applied")
            {
                applied.Add(tokens[2]);
                if(childOwners.TryGetValue(tokens[3],out var owner)&&owner!=tokens[2])failure="Child ID leaked between actor query scopes";
                childOwners[tokens[3]]=tokens[2];
            }
            if(tokens[1]=="contact")contacts.Add(tokens[2]+":"+tokens[3]);
            if(tokens[1]=="dealt")hits[tokens[2]]=int.Parse(tokens[3]);
            if(tokens[1]=="received")
            {
                double Number(int n)=>double.Parse(tokens[n],System.Globalization.CultureInfo.InvariantCulture);
                if(Math.Abs(Number(4)-Number(5)-Number(3))>.000005)failure="Resolved projectile damage did not match native health loss";
            }
            if(tokens[1]=="ended"){ended.TryGetValue(tokens[2],out int previous);ended[tokens[2]]=previous+1;}
            Debug.Log("[RangedActorsUnity] "+message.Substring(index));
        }
        if(entered&&(type==LogType.Exception&&(stack.Contains("Fight.")||stack.Contains("Model."))||message.Contains("[ModProjectiles]")||message.Contains("[ModActors]")||message.Contains("[ModCombat]")))failure=message;
    }
    static void Finish(int code){SessionState.SetBool(Active,false);EditorApplication.update-=Update;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
#endif
