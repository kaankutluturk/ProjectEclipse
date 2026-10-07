$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$selectorSource=Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/SelectAnimation.cs')
$modelSource=Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/Model.cs')
$aiSource=Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/ModelAi.cs')
$selection=[regex]::Match($selectorSource,'(?ms)^\tpublic class SelectInfo.*?^\t\}').Value
$selection+=[regex]::Match($selectorSource,'(?ms)^    internal void PrepareFormAnimation.*?^    \}').Value
$selection+=[regex]::Match($selectorSource,'(?ms)^\tprivate void CheckAnimations\(.*?^\t\}').Value
$selection+=[regex]::Match($selectorSource,'(?ms)^\tprivate void PlayAnimation\(Model.*?^\t\}').Value
$selection+=[regex]::Match($selectorSource,'(?ms)^\tprivate static void SetTransitions\(.*?^\t\}').Value
$selection+=[regex]::Match($selectorSource,'(?ms)^\tprivate static bool ContainsAnimation\(.*?^\t\}').Value
$selection+=[regex]::Match($selectorSource,'(?ms)^\tprivate static bool IsModelTypeMatch\(.*?^\t\}').Value
$selection+=[regex]::Match($selectorSource,'(?ms)^\tprivate static bool IsEventMatch\(.*?^\t\}').Value
$selection+=[regex]::Match($selectorSource,'(?ms)^\tprivate static bool IsAnimationEndMatch\(.*?^\t\}').Value
$selection+=[regex]::Match($selectorSource,'(?ms)^\tprivate static bool IsAnimationStartMatch\(.*?^\t\}').Value
$selection+=[regex]::Match($selectorSource,'(?ms)^\tprivate static bool IsNames\(.*?^\t\}').Value
$delay=[regex]::Match($modelSource,'(?ms)^\tpublic void PlayAnimationDelay\(.*?^\t\}').Value
$delay+=[regex]::Match($modelSource,'(?ms)^\tpublic bool RenderAnimationDelay\(.*?^\t\}').Value
$delay+=[regex]::Match($modelSource,'(?ms)^\tpublic bool HasPendingAnimation\(.*?^\t\}').Value
$delay+=[regex]::Match($modelSource,'(?ms)^\tpublic InfoAnimation GetPendingAnimation\(.*?^\t\}').Value
# Execute the exact readiness prefix through SetFactors. The remainder of native
# AI decision selection is outside this fixture and is not copied or simulated.
$readiness=[regex]::Match($aiSource,'(?s)\tpublic InfoAnimation Render\(Model.*?TacticFactors fJCBLOKOBBD = SetFactors\(FNKFIMEDNLP\);').Value
$observation=[regex]::Match($aiSource,'(?ms)^\tpublic void StartAnimationEnemy\(.*?^\t\}').Value
$observation+=[regex]::Match($aiSource,'(?ms)^\tpublic void StartAnimationBot\(.*?^\t\}').Value
$observation+=[regex]::Match($aiSource,'(?ms)^\tprivate bool get_IsEnabled\(.*?^\t\}').Value
if(!$selection -or !$delay -or !$readiness -or !$observation){throw 'Form animation entry extraction failed.'}
$fixture=Join-Path $root ('Temp/FormAnimationEntry-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
$code=@'
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Random = UnityEngine.Random;
using ObscuredFloat = System.Single;

namespace UnityEngine {
    static class Random {
        public static int Choice;
        public static int Range(int minimum, int maximum) {
            if (maximum <= minimum) throw new Exception("empty native priority choice");
            return minimum + Choice % (maximum - minimum);
        }
    }
}
namespace Eclipse.Multiplayer {
    static class VersusDeterminism {
        public static int Range(int minimum,int maximum)=>UnityEngine.Random.Range(minimum,maximum);
    }
}
class Fight {
    public static Fight Current;
    public bool IsLocalVersus,IsTitleSparring;
    public static Fight GetCurrentFight()=>Current;
}
static class StageType { public enum Stage { STAGE_NONE, STAGE_START_STANCE, STAGE_FIGHT, STAGE_END_STANCE } }
static class ModelType { public enum ModelTargetType { MODEL_NULL, MODEL_THIS, MODEL_OTHER, MODEL_PARENT, MODEL_CHILD, MODEL_BOTH } }
class EventAnimation {
    public enum EventAnimationType { EVENT_NONE, EVENT_ROUND_STAGE, EVENT_KEY_PRESSED, EVENT_KEY_RELEASED,
        EVENT_ANIMATION_START, EVENT_ANIMATION_END, EVENT_INTERVAL_START, EVENT_INTERVAL_END,
        EVENT_HIT, EVENT_STRIKE, EVENT_EVERY_FRAME, EVENT_BIRTH, EVENT_MOD_EXPIRES }
    public EventAnimationType Type;
    public ModelType.ModelTargetType TargetModel=ModelType.ModelTargetType.MODEL_THIS;
    public string AnimationName=string.Empty,HitType=string.Empty;
    public bool IsNot;
}
class EventModelDelayed { public EventAnimation.EventAnimationType Type; public object Data; public Model Owner,Target; public bool IsRandom; }
class IntervalAnimation { }
class ItemInfo { public string SubType; }
class ModelParameters {
    public int RemainingHealthBars=1;
    public bool IsPlayer,AiControlled=true,BeginnerCheat;
    public float MaxLife=100;
    public float GetCurrentLife()=>100;
    public ItemInfo GetItemByType(string type)=>null;
}
class Vector3f {
    public float GetX()=>X;
    public void SetX(float value){X=value;}
    public float X,Y,Z;
    public Vector3f(float x=0,float y=0,float z=0){X=x;Y=y;Z=z;}
    public Vector3f(Vector3f value){X=value.X;Y=value.Y;Z=value.Z;}
    public bool IsEqual(float x,float y,float z)=>X==x&&Y==y&&Z==z;
    public float GILCBJJPKBK()=>X;
    public void JPFALPBDBAP(float value){X=value;}
}
class ModelConditions {
    public int Epoch,RoundStage=2,AnimationSign,PivotPairSelector;
    public bool IsKeyCheckEnabled,IsPlayer;
    public List<string> CandidateMoveNames=new List<string>(),SelfAnimationNames=new List<string>(),OtherAnimationNames=new List<string>(),
        ParentAnimationNames=new List<string>(),ChildAnimationNames=new List<string>();
}
class TransitionAnimation {
    public bool Matches,IsFrameShift; public int FrameShift;
    public bool AreConditionsMet(ModelConditions conditions)=>Matches;
}
class Orientation { public int PivotSideKind; }
class AnimationDefinition {
    public List<EventAnimation> Events=new List<EventAnimation>();
    public List<TransitionAnimation> Transitions=new List<TransitionAnimation>();
    public Orientation AlignData=new Orientation();
}
class InfoAnimation {
    public enum AnimationKind { AnimationNone,AnimationAttack,AnimationIdle }
    public string Name; public int Priority,Sign=1; public bool Eligible=true,HasPhysics;
    public AnimationKind Type=AnimationKind.AnimationIdle;
    public AnimationDefinition MoveData=new AnimationDefinition();
    public InfoAnimation Equivalent; public Vector3f Velocity=new Vector3f();
    public int ConditionCalls;
    public InfoAnimation GetTacticEquivalent()=>Equivalent;
    public List<string> GetTemplateNames()=>new List<string>{Name};
    public int GetDirection(ModelConditions conditions,int sign)=>Sign;
    public bool IsItemRequired(string type,string subtype)=>false;
    public Vector3f GetVelocity()=>Velocity;
    public bool AreConditionsMet(Model model,object unused,EventAnimation cause){
        if(model.Conditions.Epoch==0)throw new Exception("conditions were not refreshed before entry selection");
        ConditionCalls++;return Eligible;
    }
}
class AnimationCandidates {
    public List<InfoAnimation> Values=new List<InfoAnimation>();
    public EventAnimation.EventAnimationType Requested;
    // Supply already loadout-filtered candidates. Production event/side matching
    // still runs over their actual EventAnimation records in CheckAnimations.
    public List<InfoAnimation> GetListForEvent(EventAnimation.EventAnimationType kind){Requested=kind;return Values;}
}
class PendingAnimation {
    public InfoAnimation Animation; public int Direction,FrameShift; public bool IsFrameShift;
    public bool IsEmpty()=>Animation==null;
    public void Clear(){Animation=null;}
}
class Physics { public bool Running;public int Stops;public bool IsPhysics()=>Running;public bool EGNOOKHNFLK()=>Running;public void Stop(){Stops++;Running=false;} }
class ModelAnimation {
    public InfoAnimation Current; public bool Playing=true;public Vector3f Moved;
    public InfoAnimation GetCurrentInfo()=>Current;
    public bool GetIsPlaying()=>Playing&&Current!=null;
    public int GetSign()=>1;
    public int HILLKPNMCIP()=>17;
    public int GetFrameInMove()=>17;
    public void MoveByVelocity(Vector3f velocity){Moved=velocity;}
}
class Statistics {
    readonly Dictionary<InfoAnimation,int> observations=new Dictionary<InfoAnimation,int>();
    public int Reads;
    public void GetCountAndDamage(bool incoming,InfoAnimation animation,ref float count,ref float damage,ref float value){
        // Dictionary null-key behavior is real; native statistics/accounting
        // beyond this dependency is controlled in the readiness fixture.
        if(!observations.ContainsKey(animation))observations.Add(animation,0);
        Reads++;count=observations[animation];
    }
}
class Model {
    public bool RenderStrikeDelay()=>false;
    DELAY_METHODS
    public readonly ModelParameters Parameters=new ModelParameters();
    public readonly ModelConditions Conditions=new ModelConditions();
    public readonly AnimationCandidates AnimationEvents=new AnimationCandidates();
    public readonly ModelAnimation _Animation=new ModelAnimation();
    public readonly Physics _Physics=new Physics();
    public readonly PendingAnimation pendingPlayRequest=new PendingAnimation();
    public readonly Statistics Stats=new Statistics();
    public Model Enemy,Owner;public int FirstFrameActions,PlayedSign,PlayedShift;public bool PlayedFrameShift,RejectPlay;
    public bool ReceivedCritical;public StrikeResult LastStrike=new StrikeResult();
    public class StrikeResult { public bool IsBlocked; }
    public InfoAnimation.AnimationKind LastAnimationType;public EventAnimation.EventAnimationType LastEventType;
    public ModelConditions GetConditions()=>Conditions;
    public ModelAnimation GetAnimationModule()=>_Animation;
    public InfoAnimation GetCurrentAnimation()=>_Animation.Current;
    public Model GetCombatTarget()=>Enemy;
    public Model GetParentModel()=>Owner;
    public Model GetModelByType(ModelType.ModelTargetType kind)=>null;
    public bool IsInShock()=>false;
    public bool IsAiControlled()=>Parameters.AiControlled;
    public Statistics GetModelStats()=>Stats;
    public int GetNoRangedFlag()=>0;
    public int GetMagicCharges()=>0;
    public void SetDelayedStrike(InfoAnimation animation,bool hit){}
    public bool PlayAnimation(InfoAnimation animation,int sign,bool shifted,int shift){
        if(RejectPlay)return false;
        FirstFrameActions++;_Animation.Current=animation;PlayedSign=sign;PlayedFrameShift=shifted;PlayedShift=shift;return true;
    }
}
class WeaponModel:Model { }
static class GameUtils { public static Balance RandomTactics=new Balance();public class Balance{public float BeginnerCheat;} }
static class AiData { public static bool Enabled=true,Both;public static bool get_BothBotEnabled()=>Both; }
class TacticFactors {
    public float FactorsCount,Damage,Hits,Health,EnemyHealth;
    public int AnimationFrames,ChildFrames;public InfoAnimation Selected;
    public TacticFactors(Statistics stats,int ranged,int magic){}
}
class ModelAi {
    AI_READINESS_PREFIX
        return fJCBLOKOBBD.Selected;
    }
    AI_OBSERVATION_METHODS
    readonly Model _Model;readonly ModelAnimation _ModelAnimation;readonly ModelParameters parameters;
    public InfoAnimation enemyAnimation;
    InfoAnimation botAnimation;
    public InfoAnimation OwnObserved=>botAnimation;
    public int decisionWait;
    public int Randomizations,FactorCalls,ResponseChecks,responseDelay;
    public bool Ignore;public readonly InfoAnimation Result=new InfoAnimation{Name="fixture decision"};
    public ModelAi(Model model){_Model=model;_ModelAnimation=model._Animation;parameters=model.Parameters;}
    static bool get_AiOn()=>AiData.Enabled;
    bool IsFitStartAnimation(InfoAnimation animation)=>animation!=null;
    void RandomizeBehavior(Model enemy){Randomizations++;}
    bool IsIgnoredEnemyAnimation(InfoAnimation animation)=>Ignore;
    int ChildMaxModelFrame(Model model)=>0;
    int GetResponseDelay(TacticFactors factors){ResponseChecks++;return 9;}
    TacticFactors SetFactors(Model enemy){
        FactorCalls++;var factors=new TacticFactors(_Model.Stats,0,0){Selected=Result};
        _Model.Stats.GetCountAndDamage(true,enemyAnimation,ref factors.FactorsCount,ref factors.Damage,ref factors.Hits);
        return factors;
    }
}
class SelectAnimation {
    SELECTOR_METHODS
    public readonly List<Model> _Models=new List<Model>();
    readonly List<ModelConditions> _ModelsConditions=new List<ModelConditions>();
    public readonly List<object> _PendingEvents=new List<object>(),_PendingIntervalEndEvents=new List<object>(),_PendingTriggers=new List<object>();
    public readonly List<Model> _ExplicitBirthModels=new List<Model>(),_NewlyCreatedModels=new List<Model>();
    public int Updates;
    void UpdateConditions(){
        Updates++;_ModelsConditions.Clear();
        foreach(var model in _Models){model.Conditions.Epoch=Updates;model.Conditions.IsPlayer=model.Parameters.IsPlayer;_ModelsConditions.Add(model.Conditions);}
    }
    void PlayAnimationRandom(Model model,List<SelectInfo> selections){throw new Exception("entry unexpectedly used AI random choice");}
    static bool IsRoundStageMatch(EventAnimation value,StageType.Stage stage)=>false;
    static bool IsKeyPressedMatch(EventAnimation value)=>true;
    static bool IsKeyReleasedMatch(EventAnimation value)=>true;
    static bool IsIntervalStartMatch(EventAnimation value,IntervalAnimation interval)=>false;
    static bool IsIntervalEndMatch(EventAnimation value,IntervalAnimation interval)=>false;
    static bool IsHit(EventAnimation value,EventModelDelayed delayed,bool critical=false,bool shock=false)=>false;
    static bool IsEveryFrameMatch(EventAnimation value)=>true;
    static bool IsBirthMatch(EventAnimation value)=>true;
    static bool IsModExpiresMatch(EventAnimation value,string name)=>false;
}
static class ValidateFormAnimationEntry {
    static int checks;
    static void Check(bool value,string why){checks++;if(!value)throw new Exception(why);}
    static void Reject(Action operation,string why){bool failed=false;try{operation();}catch(InvalidOperationException){failed=true;}Check(failed,why);}
    static InfoAnimation Move(string name,int priority,EventAnimation.EventAnimationType kind=EventAnimation.EventAnimationType.EVENT_ANIMATION_END){
        var move=new InfoAnimation{Name=name,Priority=priority};move.MoveData.Events.Add(new EventAnimation{Type=kind});return move;
    }
    static void Selection(){
        foreach(bool player in new[]{false,true})foreach(int sign in new[]{-1,1}){
            var form=new Model();form.Parameters.IsPlayer=player;
            var other=new Model();var previous=Move("ongoing opponent",0);other._Animation.Current=previous;
            form.Enemy=other;other.Enemy=form;
            var selector=new SelectAnimation();selector._Models.AddRange(player?new[]{form,other}:new[]{other,form});
            var marker=new object();selector._PendingEvents.Add(marker);selector._PendingIntervalEndEvents.Add(marker);selector._PendingTriggers.Add(marker);
            selector._ExplicitBirthModels.Add(other);selector._NewlyCreatedModels.Add(other);
            var idle=Move("generic idle",0);var transition=Move("arbitrary mod entry",7);transition.Sign=sign;
            transition.Velocity=new Vector3f(2,3,0);
            transition.MoveData.Transitions.Add(new TransitionAnimation{Matches=false,FrameShift=91});
            transition.MoveData.Transitions.Add(new TransitionAnimation{Matches=true,IsFrameShift=true,FrameShift=6});
            var denied=Move("ineligible high priority",99);denied.Eligible=false;
            var otherEvent=Move("opponent only",100);otherEvent.MoveData.Events[0].TargetModel=ModelType.ModelTargetType.MODEL_OTHER;
            var namedEnd=Move("unrelated animation end",101);namedEnd.MoveData.Events[0].AnimationName="missing old animation";
            var birth=Move("helper birth",102,EventAnimation.EventAnimationType.EVENT_BIRTH);
            form.AnimationEvents.Values.AddRange(new[]{idle,transition,transition,denied,otherEvent,namedEnd,birth});
            selector.PrepareFormAnimation(form);
            Check(form.GetPendingAnimation()==transition,"native priority chooses eligible entry without a hardcoded animation name");
            Check(transition.ConditionCalls==1,"duplicate candidate references are deduplicated by native selection");
            Check(form.GetCurrentAnimation()==null&&form.FirstFrameActions==0,"preparation schedules but does not execute animation actions");
            Check(other.GetCurrentAnimation()==previous&&!other.HasPendingAnimation()&&other.FirstFrameActions==0,"opponent animation stays untouched");
            Check(form.Conditions.RoundStage==2,"selection preserves current fight stage");
            Check(form.AnimationEvents.Requested==EventAnimation.EventAnimationType.EVENT_ANIMATION_END,"requests native animation-end candidates");
            Check(selector._PendingEvents.SequenceEqual(new[]{marker})&&selector._PendingIntervalEndEvents.SequenceEqual(new[]{marker})&&selector._PendingTriggers.SequenceEqual(new[]{marker}),"pending animation and trigger records are unchanged");
            Check(selector._ExplicitBirthModels.SequenceEqual(new[]{other})&&selector._NewlyCreatedModels.SequenceEqual(new[]{other}),"helper creation queues are unchanged");
            Check(form._Animation.Moved.X==2*sign&&form._Animation.Moved.Y==3&&transition.Velocity.X==2,"native scheduling applies facing to an independent velocity vector");
            Check(form.RenderAnimationDelay()&&form.GetCurrentAnimation()==transition,"first native delayed-render step starts selected animation");
            Check(form.PlayedSign==sign&&form.PlayedFrameShift&&form.PlayedShift==6,"native matching transition and facing survive scheduling");
            Check(!form.RenderAnimationDelay()&&form.FirstFrameActions==1,"first animation actions execute exactly once");
        }
    }
    static void Rejections(){
        var selector=new SelectAnimation();var form=new Model();selector._Models.Add(form);
        Reject(()=>selector.PrepareFormAnimation(null),"null body rejects");
        Reject(()=>selector.PrepareFormAnimation(new Model()),"unregistered body rejects");
        Check(selector.Updates==0,"identity rejection occurs before condition updates");
        form._Animation.Current=Move("started",0);Reject(()=>selector.PrepareFormAnimation(form),"started body rejects");
        form._Animation.Current=null;form.pendingPlayRequest.Animation=Move("pending",0);
        Reject(()=>selector.PrepareFormAnimation(form),"already scheduled body rejects");form.pendingPlayRequest.Clear();
        Reject(()=>selector.PrepareFormAnimation(form),"no eligible move rejects before commit");
        Check(!form.HasPendingAnimation()&&form.FirstFrameActions==0,"empty candidate failure does not start body");
        var denied=Move("denied",1);denied.Eligible=false;form.AnimationEvents.Values.Add(denied);
        Reject(()=>selector.PrepareFormAnimation(form),"native condition rejection cannot report entry success");
        form.AnimationEvents.Values.Clear();var hit=Move("strike branch",1);hit.HasPhysics=true;form.AnimationEvents.Values.Add(hit);
        Reject(()=>selector.PrepareFormAnimation(form),"selection that does not schedule an animation is rejected");
        form.AnimationEvents.Values.Clear();var one=Move("tie one",2);var two=Move("tie two",2);
        form.AnimationEvents.Values.AddRange(new[]{one,two});UnityEngine.Random.Choice=1;selector.PrepareFormAnimation(form);
        Check(form.GetPendingAnimation()==two,"equal highest priorities use native selection pool");UnityEngine.Random.Choice=0;
    }
    static void Readiness(){
        var owner=new Model();var enemy=new Model();var ai=new ModelAi(owner);
        enemy._Animation.Current=Move("already running opponent",0);
        Check(ai.Render(enemy,180)==null&&ai.FactorCalls==0&&ai.Randomizations==0,"AI defers before own initial animation");
        owner._Animation.Current=Move("newly started form",0);
        Check(ai.Render(null,181)==null&&ai.FactorCalls==0,"missing opponent defers AI");
        enemy._Animation.Current=null;
        Check(ai.Render(enemy,182)==null&&ai.FactorCalls==0&&ai.enemyAnimation==null,"unstarted opponent cannot populate observation or factors");
        var original=Move("opponent motion",0);var equivalent=Move("native tactic equivalent",0);original.Equivalent=equivalent;
        enemy._Animation.Current=original;enemy._Animation.Playing=false;
        Check(ai.Render(enemy,183)==null&&ai.Randomizations==0,"nonplaying opponent does not fake an animation observation");
        enemy._Animation.Playing=true;
        Check(ai.Render(enemy,184)==ai.Result&&ai.enemyAnimation==equivalent,"native enemy observation seeds tactic equivalent before factors");
        Check(ai.Randomizations==1&&ai.ResponseChecks==1&&ai.responseDelay==9&&ai.FactorCalls==1,"native observation initialization happens once");
        Check(ai.Render(enemy,185)==ai.Result&&ai.Randomizations==1&&ai.FactorCalls==2,"ready controller does not repeat observation initialization");
        var next=Move("next opponent motion",0);enemy._Animation.Current=next;ai.StartAnimationEnemy(enemy);
        Check(ai.enemyAnimation==next&&ai.Randomizations==2,"ordinary later native observation still updates cache");
        foreach(bool global in new[]{false,true})foreach(bool bot in new[]{false,true}){
            AiData.Enabled=global;owner=new Model();owner.Parameters.AiControlled=bot;owner._Animation.Current=Move("own",0);ai=new ModelAi(owner);
            var result=ai.Render(enemy,186);
            Check((result!=null)==(global&&bot),"original global and per-fighter AI eligibility remains effective");
        }
        AiData.Enabled=true;AiData.Both=true;owner=new Model();owner.Parameters.AiControlled=false;owner._Animation.Current=Move("player",0);ai=new ModelAi(owner);
        Check(ai.Render(enemy,187)==ai.Result,"both-bot native override still initializes observation");AiData.Both=false;
        foreach(bool versus in new[]{false,true}){
            Fight.Current=new Fight{IsLocalVersus=versus,IsTitleSparring=!versus};
            owner=new Model();var own=Move("already running own motion",0);var ownEquivalent=Move("own tactic equivalent",0);own.Equivalent=ownEquivalent;
            owner._Animation.Current=own;ai=new ModelAi(owner);
            Check(ai.Render(enemy,188)==ai.Result&&ai.OwnObserved==ownEquivalent&&ai.decisionWait==1,"versus/title controller observes its running own move through native StartAnimationBot");
            Check(ai.Render(enemy,189)==ai.Result&&ai.OwnObserved==ownEquivalent,"ready controller retains its own observation");
            owner=new Model();owner._Animation.Current=own;owner._Animation.Playing=false;ai=new ModelAi(owner);
            Check(ai.Render(enemy,190)==ai.Result&&ai.OwnObserved==null,"nonplaying own move does not seed observation");
        }
        Fight.Current=new Fight();owner=new Model();owner._Animation.Current=Move("campaign own",0);ai=new ModelAi(owner);
        Check(ai.Render(enemy,191)==ai.Result&&ai.OwnObserved==owner._Animation.Current,"campaign controller joining a running move observes itself through the native hook");
        var remembered=ai.OwnObserved;owner._Animation.Current=Move("next own motion",0);
        Check(ai.Render(enemy,192)==ai.Result&&ai.OwnObserved==remembered,"readiness does not replace an existing own observation or replay its start");
        Fight.Current=null;
        owner=new Model();owner._Animation.Current=Move("outside a fight",0);ai=new ModelAi(owner);
        Check(ai.Render(enemy,193)==ai.Result&&ai.OwnObserved==null,"no active fight does not seed own observation");
    }
    public static void Main(){
        Selection();Rejections();Readiness();
        Console.WriteLine("PASS: "+checks+" production form animation entry/readiness checks. Actual candidate/event/side matching, priority, transition and delayed playback orchestration; exact AI Render readiness prefix and native StartAnimationEnemy/StartAnimationBot. Rig/condition evaluation, playback, statistics, start-animation fitness and downstream AI decisions controlled; no Unity gameplay.");
    }
}
'@
[IO.File]::WriteAllText((Join-Path $fixture 'Program.cs'),$code.Replace('SELECTOR_METHODS',$selection).Replace('DELAY_METHODS',$delay).Replace('AI_READINESS_PREFIX',$readiness).Replace('AI_OBSERVATION_METHODS',$observation))
[IO.File]::WriteAllText((Join-Path $fixture 'Test.csproj'),'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>')
dotnet run --project (Join-Path $fixture 'Test.csproj') --verbosity quiet
if($LASTEXITCODE -ne 0){throw 'Form animation entry/readiness checks failed.'}
