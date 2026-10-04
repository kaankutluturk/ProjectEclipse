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
 enum CBGBLIPAMGA {SetWaitNone,OldWait}
 string HCJOIHLKOKJ="old weapon",_modDecisionTactic="keep tactic";
 InfoAnimation COKFBIJAFLH=new InfoAnimation{Name="old observed"},CGPDPHJIDPA=new InfoAnimation{Name="own"};
 int EBEHPENMJLK=77,MEHOEEIGCEP=44,NHIPFEIIPKG=18,_modDecisionFrame=123;
 bool BEEPJNOFDCK=true,_modDecisionOwned=true;
 CBGBLIPAMGA PLDABIGHHFG=CBGBLIPAMGA.OldWait;
 public string State()=>string.Join("|",HCJOIHLKOKJ,COKFBIJAFLH?.Name,CGPDPHJIDPA?.Name,
  EBEHPENMJLK,MEHOEEIGCEP,NHIPFEIIPKG,BEEPJNOFDCK,PLDABIGHHFG,_modDecisionFrame,_modDecisionOwned,_modDecisionTactic);
 public bool Invalidated(string weapon)=>HCJOIHLKOKJ==weapon&&COKFBIJAFLH==null&&CGPDPHJIDPA.Name=="own"&&
  EBEHPENMJLK==0&&MEHOEEIGCEP==-1&&NHIPFEIIPKG==1&&!BEEPJNOFDCK&&PLDABIGHHFG==CBGBLIPAMGA.SetWaitNone&&
  _modDecisionFrame==-1&&!_modDecisionOwned&&_modDecisionTactic=="keep tactic";
 RESET
}
class ModelAnimation {
 public ModelAnimation Other; public bool Fail;
 public ModelAnimation OJKLPPNCONP()=>Other;
 public void NFEGCGJIICB(ModelAnimation value){if(Fail){Fail=false;throw new Exception("controlled binding failure");}Other=value;}
}
class Parameters { public Item Weapon=new Item(); }
class Item { public string EffectiveTacticSubtype="knife"; }
class Model {
 public class EventModel { public Model GAIBPAGPEGK; }
 public List<Model> _Enemies=new List<Model>(); public ModelAnimation _Animation=new ModelAnimation();
 public ModelAi HJOGNGDMAKJ=new ModelAi(); public Model PNNMOKIBOPP;
 public EventModel KDAHHIMLJGG=new EventModel(); public int APOHBENDEKO=27;
 public Parameters Parameters=new Parameters(); public Model Parent;
 public List<WeaponModel> Children=new List<WeaponModel>();
 public bool KIAFPPHPEEK()=>this is WeaponModel;
 public Model NJDJHGDMCIJ()=>Parent;
 public List<WeaponModel> GetWeaponModels()=>Children;
 public void SetNearestEnemy(){PNNMOKIBOPP=_Enemies.FirstOrDefault();}
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
   source._Enemies.AddRange(new Model[]{old,oldChild,friend});source.PNNMOKIBOPP=old;
   source.KDAHHIMLJGG.GAIBPAGPEGK=old;source._Animation.Other=old._Animation;
  }
  string aiBefore=root.HJOGNGDMAKJ.State(),childBefore=ownChild.HJOGNGDMAKJ.State();
  var restore=root.ReplaceCombatEnemies(new[]{friend,next},next);
  foreach(var source in new Model[]{root,ownChild}){
   Check(source._Enemies.SequenceEqual(new Model[]{next,nextChild,friend}),"selected root and its children first; remaining roots retained");
   Check(source.PNNMOKIBOPP==next,"cached target is selected root");
   Check(source._Animation.Other==next._Animation,"animation target is selected root");
   Check(source.KDAHHIMLJGG.GAIBPAGPEGK==next,"event target is selected root");
   Check(source.HJOGNGDMAKJ.Invalidated("equivalent/sword"),"old AI observation, waits and handler throttle invalidated; own move and tactic retained");
   Check(source.APOHBENDEKO==0,"native decision delay awakened");
   source.SetNearestEnemy();Check(source.PNNMOKIBOPP==next,"insertion fallback retains explicit root target");
  }
  restore();
  foreach(var source in new Model[]{root,ownChild}){
   Check(source._Enemies.SequenceEqual(new Model[]{old,oldChild,friend}),"rollback restores exact enemy registry including child order");
   Check(source.PNNMOKIBOPP==old&&source._Animation.Other==old._Animation&&source.KDAHHIMLJGG.GAIBPAGPEGK==old,"rollback restores cached, animation and event targets");
   Check(source.APOHBENDEKO==27,"rollback restores native decision delay");
  }
  Check(root.HJOGNGDMAKJ.State()==aiBefore&&ownChild.HJOGNGDMAKJ.State()==childBefore,"rollback restores all touched AI state");
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
  Check(root.HJOGNGDMAKJ.State()==aiBefore&&root.PNNMOKIBOPP==old,"validation failures leave original state");
  next.Parameters.Weapon.EffectiveTacticSubtype="fail";
  Reject(()=>root.ReplaceCombatEnemies(new[]{next},next),"item mapping failure rejected before mutation");
  Check(root.HJOGNGDMAKJ.State()==aiBefore&&root._Enemies[0]==old,"mapping failure leaves bindings intact");
  next.Parameters.Weapon=null;
  ownChild._Animation.Fail=true;
  Reject(()=>root.ReplaceCombatEnemies(new[]{next},next),"child binding failure propagates");
  Check(root.HJOGNGDMAKJ.State()==aiBefore&&ownChild.HJOGNGDMAKJ.State()==childBefore,"child failure rolls back root and child AI");
  Check(root.PNNMOKIBOPP==old&&ownChild.PNNMOKIBOPP==old&&root._Enemies.Contains(oldChild),"child failure restores target and registry");
  var empty=root.ReplaceCombatEnemies(Array.Empty<Model>(),null);
  Check(root._Enemies.Count==0&&ownChild._Enemies.Count==0&&root.PNNMOKIBOPP==null&&ownChild.PNNMOKIBOPP==null,"empty hostility clears root and children");
  Check(root._Animation.Other==null&&root.KDAHHIMLJGG.GAIBPAGPEGK==null&&root.HJOGNGDMAKJ.Invalidated(null),"empty hostility clears animation/event/AI target");
  empty();
  var unarmed=root.ReplaceCombatEnemies(new[]{next},next);
  Check(root.HJOGNGDMAKJ.Invalidated(null),"unarmed target clears previous weapon subtype");
  unarmed();root.KDAHHIMLJGG=null;
  var noEvent=root.ReplaceCombatEnemies(new[]{next},next);noEvent();
  Check(root.PNNMOKIBOPP==old&&root.KDAHHIMLJGG==null,"missing event object is supported and restored");
  Console.WriteLine("PASS: "+checks+" production atomic combat-target binding checks. AI/animation services controlled; native contacts require separate Unity acceptance.");
 }
}
'@
[IO.File]::WriteAllText((Join-Path $fixture 'Program.cs'),$code.Replace('METHODS',$methods).Replace('RESET',$reset))
[IO.File]::WriteAllText((Join-Path $fixture 'Test.csproj'),'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><NoWarn>CS0649</NoWarn></PropertyGroup></Project>')
dotnet run --project (Join-Path $fixture 'Test.csproj') --verbosity quiet
if ($LASTEXITCODE -ne 0) { throw 'Combat target bindings regression failed.' }
