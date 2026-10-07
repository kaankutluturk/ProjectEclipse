$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$model = Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/Model.cs')
$ai = Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/ModelAi.cs')
$methods = ''
foreach ($name in @('ReplaceCombatEnemies','AppendCombatEnemy','ReplaceCombatEnemyBindings')) {
    $method = [regex]::Match($model, "(?ms)^    (?:internal|private)[^\r\n]* $name\(.*?^    \}").Value
    if (!$method) { throw "Missing production target method: $name" }
    $methods += $method + "`n"
}
$reset = [regex]::Match($ai, '(?ms)^    internal System.Action ResetCombatTarget\(.*?^    \}').Value
if (!$reset) { throw 'Missing production AI target invalidation.' }
$fixture = Join-Path $root ('Temp/CombatTargetBindings-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
$code = @'
using System;
using System.Collections.Generic;
using System.Linq;
class InfoAnimation { public string Name; }
static class AiData {
 public static string GetItemEquivalent(string input) {
  if(input=="fail")throw new Exception("controlled item mapping failure");
  return input==null?null:"equivalent/"+input;
 }
}
class ModelAi {
 enum WaitMode {SetWaitNone,OldWait}
 string enemyWeaponSubtype="old weapon",_modDecisionTactic="keep tactic";
 InfoAnimation enemyAnimation=new InfoAnimation{Name="old observed"},botAnimation=new InfoAnimation{Name="own"};
 int responseDelay=77,enemyFrame=44,decisionWait=18,_modDecisionFrame=123;
 bool waitRequested=true,_modDecisionOwned=true;
 WaitMode waitMode=WaitMode.OldWait;
 public string State()=>string.Join("|",enemyWeaponSubtype,enemyAnimation?.Name,botAnimation?.Name,
  responseDelay,enemyFrame,decisionWait,waitRequested,waitMode,_modDecisionFrame,_modDecisionOwned,_modDecisionTactic);
 public bool Invalidated(string weapon)=>enemyWeaponSubtype==weapon&&enemyAnimation==null&&botAnimation.Name=="own"&&
  responseDelay==0&&enemyFrame==-1&&decisionWait==1&&!waitRequested&&waitMode==WaitMode.SetWaitNone&&
  _modDecisionFrame==-1&&!_modDecisionOwned&&_modDecisionTactic=="keep tactic";
 RESET
}
class ModelAnimation {
 public ModelAnimation Other; public bool Fail;
 public ModelAnimation GetOtherAnimation()=>Other;
 public void SetOtherAnimation(ModelAnimation value){if(Fail){Fail=false;throw new Exception("controlled binding failure");}Other=value;}
}
class Parameters { public Item Weapon=new Item(); }
class Item { public string EffectiveTacticSubtype="knife"; }
class Model {
 public class EventModel { public Model Opponent; }
 public List<Model> _Enemies=new List<Model>(); public ModelAnimation _Animation=new ModelAnimation();
 public ModelAi ai=new ModelAi(); public Model combatTarget;
 public EventModel EventData=new EventModel(); public int decisionDelay=27;
 public Parameters Parameters=new Parameters(); public Model Parent;
 public List<WeaponModel> Children=new List<WeaponModel>();
 public bool IsWeapon()=>this is WeaponModel;
 public Model GetParentModel()=>Parent;
 public List<WeaponModel> GetWeaponModels()=>Children;
 public void SetNearestEnemy(){combatTarget=_Enemies.FirstOrDefault();}
 METHODS
}
class WeaponModel:Model {}
static class Program {
 static int checks;
 static void Check(bool value,string why){checks++;if(!value)throw new Exception(why);}
 static void Reject(Action action,string why){bool rejected=false;try{action();}catch{rejected=true;}Check(rejected,why);}
 static void Main(){
  var root=new Model();var old=new Model();var next=new Model();var friend=new Model();
  next.Parameters.Weapon.EffectiveTacticSubtype="sword";
  var ownChild=new WeaponModel{Parent=root};root.Children.Add(ownChild);
  var oldChild=new WeaponModel{Parent=old};old.Children.Add(oldChild);
  var nextChild=new WeaponModel{Parent=next};next.Children.Add(nextChild);
  foreach(var source in new Model[]{root,ownChild}){
   source._Enemies.AddRange(new Model[]{old,oldChild,friend});source.combatTarget=old;
   source.EventData.Opponent=old;source._Animation.Other=old._Animation;
  }
  string aiBefore=root.ai.State(),childBefore=ownChild.ai.State();
  var restore=root.ReplaceCombatEnemies(new[]{friend,next},next);
  foreach(var source in new Model[]{root,ownChild}){
   Check(source._Enemies.SequenceEqual(new Model[]{next,nextChild,friend}),"selected root and its children first; remaining roots retained");
   Check(source.combatTarget==next,"cached target is selected root");
   Check(source._Animation.Other==next._Animation,"animation target is selected root");
   Check(source.EventData.Opponent==next,"event target is selected root");
   Check(source.ai.Invalidated("equivalent/sword"),"old AI observation, waits and handler throttle invalidated; own move and tactic retained");
   Check(source.decisionDelay==0,"native decision delay awakened");
   source.SetNearestEnemy();Check(source.combatTarget==next,"insertion fallback retains explicit root target");
  }
  restore();
  foreach(var source in new Model[]{root,ownChild}){
   Check(source._Enemies.SequenceEqual(new Model[]{old,oldChild,friend}),"rollback restores exact enemy registry including child order");
   Check(source.combatTarget==old&&source._Animation.Other==old._Animation&&source.EventData.Opponent==old,"rollback restores cached, animation and event targets");
   Check(source.decisionDelay==27,"rollback restores native decision delay");
  }
  Check(root.ai.State()==aiBefore&&ownChild.ai.State()==childBefore,"rollback restores all touched AI state");
  Reject(()=>root.ReplaceCombatEnemies(null,null),"null root list rejected");
  Reject(()=>root.ReplaceCombatEnemies(new Model[]{null},null),"null root rejected");
  Reject(()=>root.ReplaceCombatEnemies(new[]{root},root),"self hostility rejected");
  Reject(()=>root.ReplaceCombatEnemies(new Model[]{nextChild},nextChild),"child cannot be selected as root");
  Reject(()=>root.ReplaceCombatEnemies(new[]{next,next},next),"duplicate hostile roots rejected");
  Reject(()=>root.ReplaceCombatEnemies(new[]{old},next),"foreign selected root rejected");
  Reject(()=>root.ReplaceCombatEnemies(new[]{old},null),"nonempty roots require selection");
  Reject(()=>root.ReplaceCombatEnemies(Array.Empty<Model>(),next),"empty roots require null selection");
  Reject(()=>ownChild.ReplaceCombatEnemies(new[]{next},next),"weapon cannot own root targeting transaction");
  var parented=new Model{Parent=old};Reject(()=>root.ReplaceCombatEnemies(new[]{parented},parented),"parented fighter rejected");
  var retired=new Model{_Animation=null};Reject(()=>root.ReplaceCombatEnemies(new[]{retired},retired),"retired hostile root rejected");
  var retiredSource=new Model{_Animation=null};Reject(()=>retiredSource.ReplaceCombatEnemies(new[]{next},next),"retired source rejected");
  Check(root.ai.State()==aiBefore&&root.combatTarget==old,"validation failures leave original state");
  next.Parameters.Weapon.EffectiveTacticSubtype="fail";
  Reject(()=>root.ReplaceCombatEnemies(new[]{next},next),"item mapping failure rejected before mutation");
  Check(root.ai.State()==aiBefore&&root._Enemies[0]==old,"mapping failure leaves bindings intact");
  next.Parameters.Weapon=null;
  ownChild._Animation.Fail=true;
  Reject(()=>root.ReplaceCombatEnemies(new[]{next},next),"child binding failure propagates");
  Check(root.ai.State()==aiBefore&&ownChild.ai.State()==childBefore,"child failure rolls back root and child AI");
  Check(root.combatTarget==old&&ownChild.combatTarget==old&&root._Enemies.Contains(oldChild),"child failure restores target and registry");
  var empty=root.ReplaceCombatEnemies(Array.Empty<Model>(),null);
  Check(root._Enemies.Count==0&&ownChild._Enemies.Count==0&&root.combatTarget==null&&ownChild.combatTarget==null,"empty hostility clears root and children");
  Check(root._Animation.Other==null&&root.EventData.Opponent==null&&root.ai.Invalidated(null),"empty hostility clears animation/event/AI target");
  empty();
  var unarmed=root.ReplaceCombatEnemies(new[]{next},next);
  Check(root.ai.Invalidated(null),"unarmed target clears previous weapon subtype");
  unarmed();root.EventData=null;
  var noEvent=root.ReplaceCombatEnemies(new[]{next},next);noEvent();
  Check(root.combatTarget==old&&root.EventData==null,"missing event object is supported and restored");
  Console.WriteLine("PASS: "+checks+" production atomic combat-target binding checks. AI/animation services controlled; native contacts require separate Unity acceptance.");
 }
}
'@
[IO.File]::WriteAllText((Join-Path $fixture 'Program.cs'),$code.Replace('METHODS',$methods).Replace('RESET',$reset))
[IO.File]::WriteAllText((Join-Path $fixture 'Test.csproj'),'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><NoWarn>CS0649</NoWarn></PropertyGroup></Project>')
dotnet run --project (Join-Path $fixture 'Test.csproj') --verbosity quiet
if ($LASTEXITCODE -ne 0) { throw 'Combat target bindings regression failed.' }
