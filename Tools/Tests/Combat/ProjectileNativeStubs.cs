// The complete production FightProjectiles and FightFighterMotion partials is compiled by the runner.
// This fixture controls model translation and fight/session inputs. It does not
// claim native rig, keyframe, collision or rendering acceptance.
using System;
using System.Collections.Generic;
using System.Linq;
using Eclipse.Modding;
namespace UnityEngine { public static class Debug { public static void LogWarning(object value) { } } }
namespace Eclipse.Modding
{
    public sealed class ModScriptSession { public ModContentCatalog Content = new ModContentCatalog(); public bool IsDisposed; public List<ModDescriptor> ActiveMods = new List<ModDescriptor>(); }
    public static class ModRuntime
    {
        public static ModScriptSession Scripts = new ModScriptSession();
        public static bool FailSpawn, FailBirth, FailAfterCreate;
        public static Model SpawnProjectile(Model root, ProjectileDefinition definition)
        {
            if(FailSpawn)throw new InvalidOperationException("Controlled factory failure");
            var child=Fight.Current.Spawn(root,definition.Id.Namespace.Value,definition.Specification.LifetimeFrames);
            if(child!=null){child.Name=definition.Specification.Name;child.Playing=Scripts.Content.Moves.Single(m=>m.Id==definition.Specification.StartMove.Value).RuntimeName;child.ExplicitBirthAnimationStarted=!FailBirth;}
            if(FailAfterCreate)throw new InvalidOperationException("Controlled native create listener failure");
            return child;
        }
    }
    public static class ModModeRuntime { public static bool OfflineRaid; public static bool IsRaid(FightList fight)=>OfflineRaid; }
}
public static class StageType { public enum FDBBPEGEGMK { STAGE_FIGHT, STAGE_END_STANCE } }
public sealed class FightList { public BattleType Type=BattleType.FightTournament; public BattleType get_Type()=>Type; }
public sealed class RoundData { public int round=1; public bool processing=true; }
public sealed class Vector3f { public float X,Y,Z; public float GetX()=>X; public float GetY()=>Y; public float GetZ()=>Z; public Vector3f(float x,float y,float z){X=x;Y=y;Z=z;} }
public sealed class Model
{
    public sealed class StrikeResult { public Vector3f Point; public InfoAnimation AttackAnimation; }
    public bool ExplicitBirthAnimationStarted=true;
    public Model Parent; public string Name = "fixture.dart";
    public Model GetRootModel()=>Parent?.GetRootModel()??this;
    public string get_Name()=>Name;
    public InfoAnimation GetCurrentAnimation()=>new InfoAnimation{Name=Playing??"flight"};
    public readonly List<InfoAnimation> Animations=new List<InfoAnimation>();
    public string Playing; public int Plays; public bool RejectPlayback; public Action OnPlay;
    public List<InfoAnimation> GetAvailableAnimations()=>Animations;
    public bool PlayAnimation(string name){if(Throw)throw new InvalidOperationException("Controlled playback failure");if(RejectPlayback)return false;Playing=name;Plays++;OnPlay?.Invoke();return true;}
    public int DriftPasses; public bool ClampPosition;
    public double Health=1,X,Y,Z; public int Translations; public bool AnimationShifted,Throw;
    public void ShiftModelPosition(Vector3f offset,bool animation)
    {
        if(Throw)throw new InvalidOperationException("Controlled native failure");
        X+=offset.X;Y+=offset.Y;Z+=offset.Z;if(ClampPosition||DriftPasses>0){Y+=1;DriftPasses--;}Translations++;AnimationShifted=animation;
    }
}
public sealed class InfoAnimation { public string Name; }
public partial class Fight
{
    private sealed class OwnedActor { public string Id; public ModId Owner; }
    private readonly Dictionary<Model,OwnedActor> _eclipseActors=new Dictionary<Model,OwnedActor>();
    public static Fight Current;
    public readonly List<Model> LNDLFINJHDB=new List<Model>(),HCPGFOCGDAA=new List<Model>(),JLEFIKJODGG=new List<Model>();
    private int fightTimeInFrame=>Clock;
    private void RequestModelRemoval(object value){if(!JLEFIKJODGG.Contains((Model)value))JLEFIKJODGG.Add((Model)value);}
    public void ProjectileStep(){ApplyEclipseProjectileSpawns();InitializeEclipseProjectileBirths();ApplyEclipseProjectiles();foreach(var child in JLEFIKJODGG){LNDLFINJHDB.Remove(child);HCPGFOCGDAA.Remove(child);ForgetEclipseProjectile(child);}JLEFIKJODGG.Clear();}
    public void Age(){Clock++;UpdateEclipseProjectiles();}
    public void CancelProjectiles()=>CancelEclipseProjectiles();
    public ModAttackSource Attack(Model actor, Model.StrikeResult strike)=>CaptureEclipseAttackSource(actor,strike);
    public bool Query(Model body,ModId owner,out IReadOnlyList<IModProjectile> values,out string error)=>TryGetEclipseProjectiles(body,owner,out values,out error);
    public Model Spawn(Model body,string owner,int life=180){if(!CanSpawnEclipseProjectile(body,owner,life))return null;var child=new Model{Parent=body};RegisterEclipseProjectile(body,child,owner,life);HCPGFOCGDAA.Add(child);return child;}
    public Model Player=new Model(),Enemy=new Model();
    public readonly RoundData round=new RoundData();
    public FightList FightDefinition=new FightList();
    public bool IsLocalVersus,IsTitleSparring,_modelTransitionsClosed,_eclipseFightEndDispatched,Paused;
    public bool isEndRound,isGameOver,isStopFight;
    public int _eclipseEndedRound=-1,Clock=1;
    public StageType.FDBBPEGEGMK stageType=StageType.FDBBPEGEGMK.STAGE_FIGHT;
    public Fight(){ModRuntime.FailSpawn=ModRuntime.FailBirth=ModRuntime.FailAfterCreate=false;Current=this;ModRuntime.Scripts=new ModScriptSession();ModModeRuntime.OfflineRaid=false;}
    public static Fight GetCurrentFight()=>Current;
    public Model GetPlayerModel()=>Player;
    public Model GetEnemyModel()=>Enemy;
    public bool get_IsRaidFight()=>FightDefinition.Type==BattleType.FightRaid;
    public bool IsPaused()=>Paused;
    public bool Queue(Model body,double x,double y,double z,out string reason)=>TryQueueEclipseFighterMotion(body,x,y,z,out reason);
    public void Step()=>ApplyEclipseFighterMotion();
    public void PlaybackStep()=>ApplyEclipseFighterPlayback();
    public void CancelPlayback()=>CancelEclipseFighterPlayback();
    public bool QueuePlayback(Model body,DefinitionId move,Action<bool,string> complete,out string reason)=>TryQueueEclipseFighterPlayback(body,move,complete,out reason);
    public void Cancel()=>CancelEclipseFighterMotion();
    public void Materialize(){ApplyEclipseProjectileSpawns();}
    public void InitializeBirths(){InitializeEclipseProjectileBirths();}
    public bool QueueSpawn(Model root,ModId owner,DefinitionId definition,double x,double y,double z,Action<string,string> done,out string error)=>TryQueueEclipseProjectileSpawn(root,owner,definition,x,y,z,done,out error);
    public IModFighterOperations Operations()=>new EclipseFighterOperations(this,Player);
    public sealed class EclipseFighterOperations : IModFighterOperations, IModFighterMotion, IModFighterPlayback, IModFighterTargets, IModCombatSnapshotSource, IModAnimationLifecycleSource, IModDamageEventSource, IModIncomingHitSource, IModFighterProjectiles, IModFighterProjectileSpawning
    {
        readonly Fight fight; readonly Model body;
        public EclipseFighterOperations(Fight fight,Model body){this.fight=fight;this.body=body;}
        public bool TrySpawnProjectile(ModId owner,DefinitionId definition,double x,double y,double z,Action<string,string> done,out string error)=>fight.QueueSpawn(body,owner,definition,x,y,z,done,out error);
        public bool TryGetProjectiles(ModId owner,out IReadOnlyList<IModProjectile> values,out string error)=>fight.Query(body,owner,out values,out error);
        internal static ModFighterSnapshot Capture(Model model)=>new ModFighterSnapshot(model.Health,1,1,model.X,model.Y,model.Z);
        public double Health=>body.Health;
        public ModAnimationLifecycleEvent AnimationEvent { get; set; }
        public ModDamageEvent DamageEvent { get; set; }
        public ModIncomingHit IncomingHit { get; set; }
        public ModCombatSnapshot CaptureCombatSnapshot(){var other=body==fight.Player?fight.Enemy:fight.Player;return new ModCombatSnapshot(new ModFighterSnapshot(body.Health,1,1,body.X,body.Y,body.Z),new ModFighterSnapshot(other.Health,1,1,other.X,other.Y,other.Z),fight.Clock,fight.round.processing);}
        public IModFighterOperations Opponent=>new EclipseFighterOperations(fight,body==fight.Player?fight.Enemy:fight.Player);
        public bool TryMoveBy(double x,double y,double z,out string error)=>fight.Queue(body,x,y,z,out error);
        public bool TryPlayMove(DefinitionId move,Action<bool,string> complete,out string error)=>fight.QueuePlayback(body,move,complete,out error);
        public bool TryChangeHealth(double amount,out string error){error=null;body.Health+=amount;return true;}
        public bool TryAddMagicCharge(double amount,out string error){error="Controlled unavailable operation";return false;}
    }
}
