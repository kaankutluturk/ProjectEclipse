#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
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
    static Model originalPlayer, authoredPlayer;
    static float formRatio;
    static int formFrame, playerStarts, playerHits, formApplied, inputReadyFrame=-1;
    static int ownerFormPhase, ownerFormApplied, ownerLeftStarts, ownerRightStarts, ownerLeftHits, ownerRightHits;
    static IModActor ownerLeftActor, ownerRightActor;
    static object ownerLeftBehavior, ownerRightBehavior;
    static int ownerLeftBorn, ownerRightBorn;
    static bool inputStrike, playerCaptured, enemyUsedAuthored, comparisonUsedAuthored, corePunchSeen;
    static bool initialPlayerChecked;
    static bool entryRequested,entryCanceled,choiceCaptureStarted,contentChecked;
    static float choiceOpenedAt=-1;
    static ModModeDefinition choiceMode;
    static bool PlayerEntry => Environment.GetCommandLineArgs().Contains("-authoredPlayerEntry");
    static bool ComparisonEntry => Environment.GetCommandLineArgs().Contains("-comparisonPlayerEntry");
    static XmlNode Profile => (XmlNode)typeof(ModModeRuntime).GetField("_warrior",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
    static ModelParameters EntryPlayer(FightList fight)=>(ModelParameters)typeof(ModRuntime).GetMethod("BuildFightPlayerParameters",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{fight});
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
    static ModUiSurface Surface(string id="authored")
    {
        foreach(var context in (IEnumerable)Field(ModRuntime.Scripts,"_contexts"))
        {
            var scope=context.GetType().GetProperty("UiScope").GetValue(context) as ModUiScope;
            if(scope==null||scope.Owner.Value!=Owner)continue;
            foreach(ModUiSurface value in ((IDictionary)Field(scope,"surfaces")).Values)if(value.Id==id)return value;
        }
        return null;
    }
    static void Click(string id,ModUiSurface selected=null)
    {
        var view=UnityEngine.Object.FindObjectsByType<Eclipse.UI.Modding.ModUiView>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(v=>ReferenceEquals(Field(v,"surface"),selected??surface));
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
                if(Eclipse.UI.TitleScreen.IsOpen||UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.EclipseLoadingOverlay>()!=null)return;
                if(!contentChecked)
                {
                    Check(!ModRuntime.Host.HasErrors,ModRuntime.Host.FormatReport());
                    Check(ModRuntime.Scripts.ActiveMods.Any(m=>m.Id.Value==Owner),"Authored mod did not finish registration");
                    Check(ModRuntime.Host.EnabledMods.All(m=>m.Id.Value=="core"||m.Id.Value==Owner),"Unexpected mods enabled");
                    if(PlayerEntry)Check(ModRuntime.Scripts.Content.TryGetMode(DefinitionId.Parse(Owner+":modes/playable"),out choiceMode),"Playable mode missing");
                    contentChecked=true;
                }
                if(PlayerEntry&&entryRequested)
                {
                    var choice=Surface("choose_player");if(choice==null)return;
                    if(choiceOpenedAt<0){choiceOpenedAt=Time.unscaledTime;return;}
                    if(Time.unscaledTime-choiceOpenedAt<.4f)return;
                    if(!entryCanceled)
                    {
                        Check((Fight.GetCurrentFight()==null||Fight.GetCurrentFight().GetFightDefinition().get_Type()==BattleType.FightNone)&&new ModModeProgress(Profile,choiceMode).ReadPlan()==null,"Pending choice launched or saved early");
                        Check(Eclipse.UI.Modding.ModUiGameBridge.TryHandleBack(),"Setup did not route Back");
                        Check(choice.IsClosed,"Back retained chooser");
                        entryCanceled=true;entryRequested=false;choiceOpenedAt=-1;return;
                    }
                    if(!choiceCaptureStarted){choiceCaptureStarted=true;captured=false;new GameObject("Character chooser capture").AddComponent<AuthoredChoiceCapture>();return;}
                    if(!captured)return;
                    Click(ComparisonEntry?"comparison":"authored",choice);
                    Check(choice.IsClosed&&(Fight.GetCurrentFight()==null||Fight.GetCurrentFight().GetFightDefinition().get_Type()==BattleType.FightNone),"Choice launched recursively or retained lobby");
                    entered=true;return;
                }
                var encounterId=PlayerEntry?Owner+":fights/playable":"core:fights/zone_1/tournament/3";
                var encounter=ListSF.CHMCKGCDGCM(new FightIDS(ModRuntime.Scripts.Content.RuntimeFightId(DefinitionId.Parse(encounterId))));
                Check(encounter!=null,"Core encounter missing");entered=GameUtils.StartFight(encounter,false,null,true,false);
                if(PlayerEntry)entryRequested=Surface("choose_player")!=null;
                return;
            }
            var fight=Fight.GetCurrentFight();var player=fight?.GetPlayerModel();var enemy=fight?.GetEnemyModel();if(player==null||enemy==null)return;
            // The pending UI resolves before the scene loader retires the dojo.
            // Inspect only the requested encounter, never its transient training fight.
            string requestedId=ModRuntime.Scripts.Content.RuntimeFightId(DefinitionId.Parse(PlayerEntry?Owner+":fights/playable":"core:fights/zone_1/tournament/3"));
            if(fight.GetFightDefinition().FightId.ToString()!=requestedId)return;
            if(PlayerEntry&&!initialPlayerChecked)
            {
                string expected=Owner+":warriors/"+(ComparisonEntry?"core_comparison":"sash_fighter");
                Check(player.Parameters.EclipseCharacterId==expected&&(ComparisonEntry?string.IsNullOrEmpty(player.Parameters.EclipseBodyModel):player.Parameters.EclipseBodyModel==Owner+":models/body"),"Encounter did not start as its chosen player character: expected="+expected+" actual="+player.Parameters.EclipseCharacterId+" body="+player.Parameters.EclipseBodyModel+" fight="+fight.GetFightDefinition().FightId+" selected="+EntryPlayer(fight.GetFightDefinition())?.EclipseCharacterId+" canonical="+((ModelParameters)Field(fight,"NMNCKBPFCCP")).EclipseCharacterId+" screen="+Module.GetInstance().GetCurrentScreenType()+" plan="+Profile?.SelectSingleNode("EclipseModes/Mode/Encounter")?.OuterXml+" canceled="+entryCanceled+" choice="+choiceCaptureStarted);
                Check(player.Parameters.IsPlayer&&player.Parameters.UserControlled&&!player.Parameters.AiControlled,"Declared player entered without native player/input ownership");
                Check(formApplied==0,"Declared player depended on a live form request");
                Check(ModRuntime.Scripts.Content.TryGetMode(DefinitionId.Parse(Owner+":modes/playable"),out var mode),"Playable mode missing after entry");
                var plan=new ModModeProgress(Profile,mode).ReadPlan();
                Check(plan?.PlayerCharacter?.ToString()==expected,"Chosen character not retained in saved plan");
                var blueprint=ListSF.CHMCKGCDGCM(new FightIDS(ModRuntime.Scripts.Content.RuntimeFightId(mode.Fights[0])));
                Check(EntryPlayer(blueprint).EclipseCharacterId==Owner+":warriors/sash_fighter","Prepared character leaked into shared blueprint");
                var alternate=ModModeRuntime.BuildEncounter(mode,0,new ModEncounterPlan(playerCharacter:DefinitionId.Parse(Owner+":warriors/"+(ComparisonEntry?"sash_fighter":"core_comparison"))));
                Check(EntryPlayer(alternate).EclipseCharacterId!=expected&&player.Parameters.EclipseCharacterId==expected,"Separate generated instance did not isolate player selection");
                var missing=new ModEncounterPlan(playerCharacter:DefinitionId.Parse(Owner+":warriors/missing"));
                bool rejected=false;string before=Profile.OuterXml;
                try{ModModeRuntime.BuildEncounter(mode,0,missing);}catch(ModContentException){rejected=true;}
                Check(rejected&&Profile.OuterXml==before,"Unavailable character changed saved preparation");
                initialPlayerChecked=true;
            }
            if(phase<8)player.Parameters.UserControlled=false;
            player.Parameters.AiControlled=false;enemy.Parameters.UserControlled=phase>=10;enemy.Parameters.AiControlled=false;
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
                    if(ownerFormPhase==0)
                    {
                        // The pause prevented the earlier stop-auto click from
                        // being consumed. Let its public callback finish first.
                        fight.SetPaused(false);ownerFormPhase=-1;phaseFrame=frame;return;
                    }
                    if(ownerFormPhase==-1)
                    {
                        if(frame-phaseFrame<3)return;
                        originalPlayer=player;ownerFormApplied=formApplied;
                        ownerLeftActor=Actor(fight,left);ownerRightActor=Actor(fight,right);
                        ownerLeftBehavior=Field(ownerLeftActor,"BehaviorInstance");ownerRightBehavior=Field(ownerRightActor,"BehaviorInstance");
                        ownerLeftBorn=(int)Field(ownerLeftActor,"Born");ownerRightBorn=(int)Field(ownerRightActor,"Born");
                        autoLeftLife=Life(left);autoRightLife=Life(right);
                        formRatio=Life(player)/player.Parameters.MaxLife;Click("player");
                        Check(fight.GetPlayerModel()==originalPlayer,"Owner form committed recursively in HUD callback");
                        ownerFormPhase=1;phaseFrame=frame;return;
                    }
                    if(ownerFormPhase==1)
                    {
                        if(frame-phaseFrame>240)throw new Exception("Owner form did not settle with companions");
                        if(player==originalPlayer||formApplied<=ownerFormApplied||Eclipse.UI.Modding.ModUiGameBridge.BlocksGameplayInput)return;
                        Check(Actors(fight).Length==2,"Owner form retired its live companions");
                        Check(ReferenceEquals(Actor(fight,left),ownerLeftActor)&&ReferenceEquals(Actor(fight,right),ownerRightActor),"Owner form replaced actor handles");
                        Check(Id(fight,left)==leftId&&Id(fight,right)==rightId,"Owner form changed actor IDs");
                        Check(Field(ownerLeftActor,"Root")==player&&Field(ownerRightActor,"Root")==player,"Companions still belong to retired owner body");
                        Check(ReferenceEquals(Field(ownerLeftActor,"BehaviorInstance"),ownerLeftBehavior)&&ReferenceEquals(Field(ownerRightActor,"BehaviorInstance"),ownerRightBehavior),"Owner form reset actor behavior state");
                        Check((int)Field(ownerLeftActor,"Born")==ownerLeftBorn&&(int)Field(ownerRightActor,"Born")==ownerRightBorn,"Owner form restarted companion lifetimes");
                        Check(ended.Count==0&&Life(left)<=autoLeftLife&&Life(right)<=autoRightLife,"Owner form ended or healed companions");
                        Check(left.GetCombatTarget()==right&&right.GetCombatTarget()==left,"Owner form lost companion targeting");
                        Check(Math.Abs(Life(player)/player.Parameters.MaxLife-formRatio)<.001,"Companion owner form changed main health percentage");
                        Check(Surface()==surface&&!surface.IsClosed,"Owner form replaced companion rule HUD");
                        autoLeftLife=Life(left);autoRightLife=Life(right);
                        ownerLeftStarts=Count(starts,leftId);ownerRightStarts=Count(starts,rightId);
                        ownerLeftHits=Count(dealt,leftId);ownerRightHits=Count(dealt,rightId);
                        new GameObject("Companion owner form capture").AddComponent<AuthoredOwnerFormCapture>();
                        Click("auto");ownerFormPhase=2;phaseFrame=frame;return;
                    }
                    if(frame-phaseFrame>720)throw new Exception("Retained companions stopped producing public Lua/native contact: "+surface.Read("status").Text+" left="+left.GetCurrentAnimation()?.Name+" right="+right.GetCurrentAnimation()?.Name);
                    if(Count(starts,leftId)<=ownerLeftStarts||Count(starts,rightId)<=ownerRightStarts||Count(dealt,leftId)<=ownerLeftHits||Count(dealt,rightId)<=ownerRightHits||Life(left)>=autoLeftLife||Life(right)>=autoRightLife)return;
                    CheckSource(left,right);CheckSource(right,left);
                    Check(ended.Count==0,"Retained companions ended during continued combat");
                    Click("dismiss");Next(fight);break;
                case 8:
                    if(Actors(fight).Length!=0)return;
                    Check(ended.Count==2&&surface!=null&&!surface.IsClosed,"Authored dismissal lifecycle incorrect");
                    Check(ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Count==0,"Lua callback failures");
                    originalPlayer=player;player.Parameters.UserControlled=true;
                    player.Parameters.SetCurrentLife(player.Parameters.MaxLife*.65f);
                    formRatio=Life(player)/player.Parameters.MaxLife;formFrame=frame;
                    Click("player");Check(fight.GetPlayerModel()==originalPlayer,"Form committed recursively inside HUD callback");Next(fight);break;
                case 9:
                    if(frame-phaseFrame>240)throw new Exception("Authored player form did not settle: "+surface.Read("form_status").Text);
                    if(player==originalPlayer||formApplied<1||Eclipse.UI.Modding.ModUiGameBridge.BlocksGameplayInput)return;
                    if(inputReadyFrame<0){inputReadyFrame=frame;return;}
                    // Let the replacement's first native stance initialize before input.
                    if(frame-inputReadyFrame<6)return;
                    inputReadyFrame=-1;
                    authoredPlayer=player;
                    Check(player.Parameters.EclipseCharacterId==Owner+":warriors/sash_fighter"&&player.Parameters.EclipseBodyModel==Owner+":models/body","Wrong authored player form");
                    Check(player.Parameters.IsPlayer&&player.Parameters.UserControlled&&!player.Parameters.AiControlled,"Player form lost input eligibility");
                    Check(Math.Abs(Life(player)/player.Parameters.MaxLife-formRatio)<.001&&frame>=formFrame,"Form changed health percentage or restarted clock");
                    Check(fight.GetEnemyModel()==enemy&&enemy.GetCombatTarget()==player&&fight.LNDLFINJHDB.Contains(player)&&!fight.LNDLFINJHDB.Contains(originalPlayer),"Canonical participant/target ownership did not rebind");
                    Check(Surface()==surface&&!surface.IsClosed&&Actors(fight).Length==0,"Form reset or duplicated the rule HUD/actor lifecycle");
                    Check(player.GetModelObject().NAMKCLGOPDD().Any(n=>n.GetName()=="AuthoredSashV5"),"Player form did not load authored skin");
                    Invoke(player,"TrainingMoveToX",450f);Invoke(enemy,"TrainingMoveToX",550f);beforeHit=Life(enemy);
                    Invoke(fight.Controller,"SendGamepadControlEvent",0,FightCID.Punch);Next(fight);break;
                case 10:
                    // Editor update callbacks can run twice before a game tick.
                    // Hold across native frames so release cannot erase a tap
                    // before the real input/move-selection pipeline consumes it.
                    if(frame-phaseFrame>=3)Invoke(fight.Controller,"SendGamepadControlEvent",1,FightCID.Punch);
                    inputStrike|=player.GetCurrentAnimation()?.Name==Move;
                    if(frame-phaseFrame>240)throw new Exception("Native Punch did not make authored player contact: "+player.GetCurrentAnimation()?.Name+" starts="+playerStarts+" hits="+playerHits+" playerControl="+Field(player,"HCPHOJKFIDM")+" fightInput="+Field(fight,"IOPJDMCBIMM")+" controller="+fight.Controller.IsQuadrantEnabled(FightCID.Punch)+" stage="+Field(fight,"stageType")+" char="+player.Parameters.EclipseCharacterId);
                    if(!inputStrike||Life(enemy)>=beforeHit||player.GetCurrentAnimation()?.Name==Move)return;
                    CheckSource(enemy,player);Check(playerStarts>0&&playerHits>0,"Player input lacked ordinary animation/damage callbacks");
                    Invoke(enemy,"TrainingMoveToX",950f);enemy.PressAnyKey(FightCID.Punch);
                    Next(fight);break;
                case 11:
                    if(frame-phaseFrame>=3)enemy.ReleaseAnyKey(FightCID.Punch);
                    enemyUsedAuthored|=enemy.GetCurrentAnimation()?.Name==Move;
                    // Let the deliberate opponent Punch and the player's hit/block
                    // reaction finish before testing input on the comparison form.
                    if(frame-phaseFrame>240)throw new Exception("Opponent comparison action did not settle: "+enemy.GetCurrentAnimation()?.Name+" player="+player.GetCurrentAnimation()?.Name);
                    if(frame-phaseFrame<60||enemy.NMEEPBDJHMG()&&enemy.GetCurrentAnimation()!=null&&(int)enemy.GetCurrentAnimation().Type==2)return;
                    if(!playerCaptured)Check(!enemyUsedAuthored,"Character-specific Punch leaked to the other fighter");
                    if(!playerCaptured){playerCaptured=true;captured=false;new GameObject("Authored player capture").AddComponent<AuthoredPlayerCapture>();return;}
                    if(!captured)return;
                    formRatio=Life(player)/player.Parameters.MaxLife;formFrame=frame;Click("core");Next(fight);break;
                case 12:
                    if(frame-phaseFrame>240)throw new Exception("Core comparison form did not settle: "+surface.Read("form_status").Text);
                    if(player==authoredPlayer||formApplied<2||Eclipse.UI.Modding.ModUiGameBridge.BlocksGameplayInput)return;
                    if(player.GetCurrentAnimation()?.Name!="StanceIdle")return;
                    if(inputReadyFrame<0){inputReadyFrame=frame;return;}
                    if(frame-inputReadyFrame<6)return;
                    inputReadyFrame=-1;
                    Check(player.Parameters.EclipseCharacterId==Owner+":warriors/core_comparison"&&string.IsNullOrEmpty(player.Parameters.EclipseBodyModel),"Comparison retained authored body identity");
                    Check(player.Parameters.IsPlayer&&player.Parameters.UserControlled&&Math.Abs(Life(player)/player.Parameters.MaxLife-formRatio)<.001&&frame>=formFrame,"Comparison form lost player state");
                    Check(!player.GetModelObject().NAMKCLGOPDD().Any(n=>n.GetName()=="AuthoredSashV5"),"Comparison retained authored sash geometry");
                    Invoke(player,"TrainingMoveToX",450f);Invoke(enemy,"TrainingMoveToX",550f);
                    Invoke(fight.Controller,"SendGamepadControlEvent",0,FightCID.Punch);Next(fight);break;
                case 13:
                    if(frame-phaseFrame>=3)Invoke(fight.Controller,"SendGamepadControlEvent",1,FightCID.Punch);
                    comparisonUsedAuthored|=player.GetCurrentAnimation()?.Name==Move;
                    // AnimationAttack is native category 2. Confirm a real core
                    // action, rather than passing just because no input worked.
                    corePunchSeen|=player.GetCurrentAnimation()!=null&&(int)player.GetCurrentAnimation().Type==2&&!comparisonUsedAuthored;
                    if(frame-phaseFrame>120)throw new Exception("Core comparison Punch selected no native attack: "+player.GetCurrentAnimation()?.Name);
                    if(frame-phaseFrame<20)return;
                    if(!corePunchSeen)return;
                    Check(!comparisonUsedAuthored,"Character-specific Punch leaked to core comparison form");
                    Check(corePunchSeen,"Comparison form lost ordinary Punch selection");
                    Check(Surface()==surface&&ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Count==0,"Player form callbacks/HUD failed");
                    Invoke(fight,"OBNEDPKCNKJ");Check(surface.IsClosed,"Round teardown retained authored HUD");
                    File.WriteAllText(Path.Combine(Root,"authored-fighter-result.txt"),"PASS: "+checks+" full-game authored fighter checks; original 61x67 native clip/skin, both actor facings and repeated Lua contact, pause/dismissal, owner form retaining companion handles/IDs/behavior/lifetimes/targets and continued bidirectional native contact, public player/comparison form swaps preserving health/input/timer/HUD/target identity, native controller Punch selects authored attack with contact/callbacks and stays character-specific. Controlled Campaign/standard rig/core equipment; not physical devices, arbitrary rigs, all exports/platforms.");
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
                if(tokens[1]=="form"&&tokens[2]=="applied")formApplied++;
            }
            if(tokens.Length>=2&&tokens[1]=="player-start")playerStarts++;
            if(tokens.Length>=2&&tokens[1]=="player-dealt")playerHits++;
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
public sealed class AuthoredOwnerFormCapture : MonoBehaviour
{
    IEnumerator Start(){yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Application.dataPath),"authored-owner-form-native.png"),texture.EncodeToPNG());UnityEngine.Object.Destroy(texture);}
}
public sealed class AuthoredPlayerCapture : MonoBehaviour
{
    IEnumerator Start(){yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Application.dataPath),"authored-player-native.png"),texture.EncodeToPNG());UnityEngine.Object.Destroy(texture);AuthoredFighterUnity.Captured();}
}
public sealed class AuthoredChoiceCapture : MonoBehaviour
{
    IEnumerator Start(){yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Application.dataPath),"authored-character-choice.png"),texture.EncodeToPNG());UnityEngine.Object.Destroy(texture);AuthoredFighterUnity.Captured();}
}
#endif
