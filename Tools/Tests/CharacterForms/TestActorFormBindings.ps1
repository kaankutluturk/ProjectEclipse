$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$actors=Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Eclipse/Modding/FightActors.cs')
$fight=Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/Fight.cs')
$methods=[regex]::Match($actors,'(?ms)^    internal Action BindEclipseActorFormParticipant.*?^    \}').Value
$methods += [regex]::Match($actors,'(?ms)^    private bool TryQueueEclipseActorForm.*?^    \}').Value
$methods += [regex]::Match($fight,'(?ms)^    private static List<DefinitionId> CaptureFormBehaviorKeys.*?^    \}').Value
$methods += [regex]::Match($fight,'(?ms)^    private static void MoveFormBehaviorKeys.*?^    \}').Value
if(!$methods.Contains('MoveFormBehaviorKeys') -or !$methods.Contains('TryQueueEclipseActorForm')){throw 'Actor form extraction failed.'}
$fixture=Join-Path $root ('Temp/ActorFormBindings-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
$code=@'
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
struct DefinitionId{public string Namespace;public DefinitionId(string owner){Namespace=owner;}public override string ToString()=>Namespace+":actors/ally";}
class Parameters{public bool IsPlayer,UserControlled,AiControlled;public float MaxLife;}
class Model{public Parameters Parameters=new Parameters();public string Name;public void set_Name(string name){Name=name;}}
class Definition{public DefinitionId Id=new DefinitionId("owner.mod");public bool AiControlled=true;public float MaxHealth=4;}
class OwnedActor{public Model Model,Root,TargetRequest;public object Birth,BehaviorInstance=new object();public string Owner="owner.mod";public bool PlayerTeam,Removing;public Definition Definition=new Definition();public int Born=17;}
class GameUtils{public static void InitializeActorParameters(Parameters p,bool player,bool ai,float max){p.IsPlayer=player;p.AiControlled=ai;p.UserControlled=false;p.MaxLife=max;}}
class ModRuntime{public static bool Fail;public static bool Player;public static Parameters BuildFormParameters(DefinitionId id,bool player){if(Fail)throw new Exception("invalid character");Player=player;return new Parameters();}}
class Fight{
METHODS
class PreparedFormModel:IDisposable{public Model Model;public static int Disposals;public PreparedFormModel(Parameters p){Model=new Model{Parameters=p};}public void Dispose(){Disposals++;}}
Dictionary<Model,OwnedActor> _eclipseActors=new Dictionary<Model,OwnedActor>();
List<Model> LNDLFINJHDB=new List<Model>();
Dictionary<Model,object> _eclipseShields=new Dictionary<Model,object>();
Dictionary<(Model,object),object> _eclipseStatusIcons=new Dictionary<(Model,object),object>();
Dictionary<(Model,DefinitionId),XmlNode> _eclipseOpponentInstances=new Dictionary<(Model,DefinitionId),XmlNode>(),_eclipseInnateInstances=new Dictionary<(Model,DefinitionId),XmlNode>();
Dictionary<Model,object> _modelTransitions=new Dictionary<Model,object>();
bool _applyingEclipseActors,_modelTransitionsClosed,AcceptQueue=true;int Queued;PreparedFormModel Preparation;
bool ActorValid(OwnedActor actor,bool mutation,out string error){error=actor.Removing?"removing":null;return !actor.Removing;}
bool QueuePreparedFighterForm(Model expected,PreparedFormModel prepared,Action<Exception> complete){Queued++;Preparation=prepared;return AcceptQueue;}
static int checks;static void Check(bool value,string why){checks++;if(!value)throw new Exception(why);}
static void Reject(Action action,string why){bool failed=false;try{action();}catch(InvalidOperationException){failed=true;}Check(failed,why);}
static void Main(){
 foreach(bool player in new[]{true,false}){
  var f=new Fight();var main=new Model();var old=new Model();var next=new Model{Parameters=new Parameters{IsPlayer=player,AiControlled=true,MaxLife=4}};
  var record=new OwnedActor{Model=old,Root=main,PlayerTeam=player,TargetRequest=new Model()};f._eclipseActors.Add(old,record);f.LNDLFINJHDB.AddRange(new[]{main,old});
  var state=new XmlDocument();state.LoadXml("<State/>");var key=new DefinitionId("behavior");var shield=new object();var icon=new object();var iconKey=new object();
  f._eclipseOpponentInstances.Add((old,key),state.DocumentElement);f._eclipseInnateInstances.Add((old,key),state.DocumentElement);f._eclipseShields.Add(old,shield);f._eclipseStatusIcons.Add((old,iconKey),icon);
  var behavior=record.BehaviorInstance;var target=record.TargetRequest;
  var undo=f.BindEclipseActorFormParticipant(old,next);
  Check(f._eclipseActors[next]==record&&!f._eclipseActors.ContainsKey(old)&&record.Model==next&&f.LNDLFINJHDB[1]==next,"native body and actor registry move together");
  Check(record.Root==main&&record.TargetRequest==target&&record.BehaviorInstance==behavior&&record.Born==17&&record.PlayerTeam==player,"owner, state, birth, target and team unchanged");
  Check(f.LNDLFINJHDB[0]==main&&f._eclipseShields[next]==shield&&f._eclipseStatusIcons[(next,iconKey)]==icon,"main slot unchanged; shields/icons transfer");
  Check(f._eclipseOpponentInstances[(next,key)]==state.DocumentElement&&f._eclipseInnateInstances[(next,key)]==state.DocumentElement,"behavior XML retained");
  undo();undo();Check(record.Model==old&&f._eclipseActors[old]==record&&f.LNDLFINJHDB[1]==old&&!f._eclipseActors.ContainsKey(next),"idempotent identity rollback");
  Check(f._eclipseShields[old]==shield&&f._eclipseStatusIcons[(old,iconKey)]==icon&&f._eclipseInnateInstances[(old,key)]==state.DocumentElement,"combat state rollback");
  next.Parameters.UserControlled=true;Reject(()=>f.BindEclipseActorFormParticipant(old,next),"player-controlled actor rejected");next.Parameters.UserControlled=false;
  next.Parameters.MaxLife=8;Reject(()=>f.BindEclipseActorFormParticipant(old,next),"max-health change rejected");next.Parameters.MaxLife=4;
  next.Parameters.IsPlayer=!player;Reject(()=>f.BindEclipseActorFormParticipant(old,next),"team change rejected");next.Parameters.IsPlayer=player;
  next.Parameters.AiControlled=false;Reject(()=>f.BindEclipseActorFormParticipant(old,next),"AI role change rejected");next.Parameters.AiControlled=true;
  record.Birth=new object();Reject(()=>f.BindEclipseActorFormParticipant(old,next),"initializing actor rejected");record.Birth=null;
  record.Removing=true;Reject(()=>f.BindEclipseActorFormParticipant(old,next),"removing actor rejected");record.Removing=false;
  f._eclipseShields.Add(next,new object());Reject(()=>f.BindEclipseActorFormParticipant(old,next),"replacement combat-state collision rejected");f._eclipseShields.Remove(next);
  f._eclipseInnateInstances.Add((next,key),state.DocumentElement);Reject(()=>f.BindEclipseActorFormParticipant(old,next),"replacement behavior collision rejected");f._eclipseInnateInstances.Remove((next,key));
  Check(f._eclipseActors[old]==record&&record.Model==old&&f._eclipseShields[old]==shield,"preflight failures leave original ownership intact");
  string error;Check(f.TryQueueEclipseActorForm(old,new DefinitionId("owner.mod"),e=>{},out error),error);
  Check(f.Queued==1&&ModRuntime.Player==player&&f.Preparation.Model.Name==record.Definition.Id.ToString()&&f.Preparation.Model.Parameters.MaxLife==4&&!f.Preparation.Model.Parameters.UserControlled,"preparation uses actor team, health, name and independent control");
  Check(!f.TryQueueEclipseActorForm(old,new DefinitionId("foreign.mod"),e=>{},out error)&&error.Contains("owning mod"),"foreign character rejected before preparation");
  f._modelTransitions.Add(old,new object());Check(!f.TryQueueEclipseActorForm(old,new DefinitionId("owner.mod"),e=>{},out error),"duplicate actor form rejected");f._modelTransitions.Clear();
  f.AcceptQueue=false;int disposed=PreparedFormModel.Disposals;Check(!f.TryQueueEclipseActorForm(old,new DefinitionId("owner.mod"),e=>{},out error)&&PreparedFormModel.Disposals==disposed+1,"queue rejection disposes preparation");
  ModRuntime.Fail=true;Check(!f.TryQueueEclipseActorForm(old,new DefinitionId("owner.mod"),e=>{},out error)&&error=="invalid character","construction error reported");ModRuntime.Fail=false;
 }
 Console.WriteLine("PASS: "+checks+" production actor form binding/preparation checks; both teams, identity/state/lifetime, combat state, collision preflight, role guards, own character, queued preparation and failure disposal. Native actor liveness/body/queue services controlled.");
}
}
'@
[IO.File]::WriteAllText((Join-Path $fixture 'Program.cs'),$code.Replace('METHODS',$methods))
[IO.File]::WriteAllText((Join-Path $fixture 'Test.csproj'),'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><NoWarn>CS0649</NoWarn></PropertyGroup></Project>')
dotnet run --project (Join-Path $fixture 'Test.csproj') --verbosity quiet
if($LASTEXITCODE -ne 0){throw 'Actor form checks failed.'}
