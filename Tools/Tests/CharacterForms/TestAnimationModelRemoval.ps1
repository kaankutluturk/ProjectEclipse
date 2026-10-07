$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$source=Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/SelectAnimation.cs')
$remove=[regex]::Match($source,'(?ms)^\tpublic void RemoveModel\(.*?^\t\}').Value
$filter=[regex]::Match($source,'(?ms)^\tprivate void RemoveEventsForModel\(.*?^\t\}').Value
$replace=[regex]::Match($source,'(?ms)^    internal bool ReplaceModel\(.*?^    \}').Value
$capture=[regex]::Match($source,'(?ms)^    internal System.Action CapturePendingEvents\(.*?^    \}').Value
if(!$remove -or !$filter -or !$replace -or !$capture){throw 'Animation removal extraction failed.'}
$fixture=Join-Path $root ('Temp/AnimationRemoval-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
$code=@'
using System;
using System.Collections.Generic;
using System.Linq;
class ModelConditions{}
class Model{public ModelConditions Conditions=new ModelConditions();public static Model Bound;public bool Fail;public List<Model> BindingModels;public ModelConditions GetConditions()=>Conditions;public void UpdateAnimationParameters(List<Model> models){Bound=this;BindingModels=new List<Model>(models);if(Fail)throw new InvalidOperationException("binding");}public void AddEventListener(int id,Action<object> handler){Listeners[id]=handler;} public Dictionary<int,Action<object>> Listeners=new Dictionary<int,Action<object>>(); public void RemoveEventListener(int id,Action<object> handler){if(Listeners.TryGetValue(id,out var existing)&&existing==handler)Listeners.Remove(id);}}
class EventModelDelayed{public Model Owner,Target;}
class SelectAnimation{
 List<Model> _Models=new List<Model>(),_NewlyCreatedModels=new List<Model>(),_ExplicitBirthModels=new List<Model>();
 List<ModelConditions> _ModelsConditions=new List<ModelConditions>();
 List<EventModelDelayed> _PendingEvents=new List<EventModelDelayed>(),_PendingIntervalEndEvents=new List<EventModelDelayed>();
 class TriggerStruct{public Model OwnerModel;}
 List<TriggerStruct> _PendingTriggers=new List<TriggerStruct>();
 void OnAnimationStart(object o){} void OnAnimationEnd(object o){} void OnIntervalStart(object o){} void OnIntervalEnd(object o){} void OnEveryFrame(object o){} void OnModelCreate(object o){} void OnKeyPress(object o){} void OnKeyRelease(object o){}
 REMOVE
 FILTER
 REPLACE
 CAPTURE
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 public static void Main(){
  var s=new SelectAnimation();var old=new Model();var live=new Model();var other=new Model();
  old.Listeners=new Dictionary<int,Action<object>>{{2,s.OnAnimationStart},{3,s.OnAnimationEnd},{0,s.OnIntervalStart},{1,s.OnIntervalEnd},{4,s.OnEveryFrame},{6,s.OnModelCreate},{10,s.OnKeyPress},{11,s.OnKeyRelease},{99,o=>{}}};
  s._Models.AddRange(new[]{old,live});var condition=live.Conditions;s._ModelsConditions.AddRange(new[]{old.Conditions,condition});
  var unrelated=new EventModelDelayed{Owner=live,Target=other};
  foreach(var list in new[]{s._PendingEvents,s._PendingIntervalEndEvents})list.AddRange(new[]{new EventModelDelayed{Owner=old},new EventModelDelayed{Owner=live,Target=old},unrelated});
  s._ExplicitBirthModels.AddRange(new[]{old,live});s._NewlyCreatedModels.AddRange(new[]{old,live});
  s._PendingTriggers.AddRange(new[]{new TriggerStruct{OwnerModel=old},new TriggerStruct{OwnerModel=live}});
  s.RemoveModel(old);s.RemoveModel(old);s.RemoveModel(null);
  Check(old.Listeners.Count==1&&old.Listeners.ContainsKey(99),"only selector listeners removed");
  Check(s._Models.Count==1&&s._Models[0]==live,"active model removed");
  Check(s._ModelsConditions.Count==1&&s._ModelsConditions[0]==condition,"condition alignment");
  foreach(var list in new[]{s._PendingEvents,s._PendingIntervalEndEvents})Check(list.Count==1&&list[0]==unrelated,"self/target events purged, unrelated retained");
  Check(s._ExplicitBirthModels.Count==1&&s._ExplicitBirthModels[0]==live,"birth cleanup");
  Check(s._NewlyCreatedModels.Count==1&&s._NewlyCreatedModels[0]==live,"created-model cleanup");
  Check(s._PendingTriggers.Count==1&&s._PendingTriggers[0].OwnerModel==live,"trigger cleanup");
  var next=new Model{Fail=true};bool failed=false;try{s.ReplaceModel(live,next);}catch(InvalidOperationException){failed=true;}
  Check(failed&&Model.Bound==live&&s._Models[0]==live&&next.Listeners.Count==0,"binding failure restores active binding and membership");
  next.Fail=false;Check(s.ReplaceModel(live,next),"replacement accepted");
  Check(s._Models[0]==next&&s._ModelsConditions[0]==next.Conditions&&next.BindingModels[0]==next,"replacement order/condition/binding snapshot");
  Check(next.Listeners.Count==8&&live.Listeners.Count==0,"replacement listeners");
  Check(!s.ReplaceModel(live,new Model())&&!s.ReplaceModel(next,next),"stale or duplicate replacement");
  var replacement=new Model();
  var ownerEvent=new EventModelDelayed{Owner=next};
  var targetEvent=new EventModelDelayed{Owner=other,Target=next};
  s._PendingEvents.AddRange(new[]{unrelated,ownerEvent,targetEvent});
  s._PendingIntervalEndEvents.AddRange(new[]{targetEvent,unrelated,ownerEvent});
  s._ExplicitBirthModels.AddRange(new[]{other,next,other});
  s._NewlyCreatedModels.AddRange(new[]{next,other,next});
  s._PendingTriggers.AddRange(new[]{new TriggerStruct{OwnerModel=next},new TriggerStruct{OwnerModel=other}});
  var events=s._PendingEvents.ToArray();var ends=s._PendingIntervalEndEvents.ToArray();
  var births=s._ExplicitBirthModels.ToArray();var created=s._NewlyCreatedModels.ToArray();var triggers=s._PendingTriggers.ToArray();
  var restore=s.CapturePendingEvents();
  Check(s.ReplaceModel(next,replacement),"stage replacement for rollback");
  s._PendingEvents.Add(new EventModelDelayed{Owner=replacement});
  s._PendingIntervalEndEvents.Clear();s._ExplicitBirthModels.Clear();s._NewlyCreatedModels.Clear();s._PendingTriggers.Clear();
  Check(s.ReplaceModel(replacement,next),"reverse registration before event restoration");restore();
  Check(s._PendingEvents.SequenceEqual(events)&&s._PendingIntervalEndEvents.SequenceEqual(ends),"both event queues restore exact identity and order");
  Check(s._ExplicitBirthModels.SequenceEqual(births)&&s._NewlyCreatedModels.SequenceEqual(created)&&s._PendingTriggers.SequenceEqual(triggers),"birth, created and trigger queues restore identity, duplicates and order");
  Console.WriteLine("PASS: production animation model removal; listeners, owner/target events, birth/candidate/trigger references, condition alignment and repeat removal; dispatcher/model services controlled.");
 }
}
'@
$code=$code.Replace('REMOVE',$remove).Replace('FILTER',$filter).Replace('REPLACE',$replace).Replace('CAPTURE',$capture)
[IO.File]::WriteAllText((Join-Path $fixture 'Program.cs'),$code)
[IO.File]::WriteAllText((Join-Path $fixture 'Test.csproj'),'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>')
dotnet run --project (Join-Path $fixture 'Test.csproj') --verbosity quiet
if($LASTEXITCODE -ne 0){throw 'Animation removal checks failed.'}
