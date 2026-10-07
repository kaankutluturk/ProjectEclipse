$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$source=Get-Content -Raw (Join-Path $root 'Assets/Scripts/Assembly-CSharp/Model.cs')
$method=[regex]::Match($source,'(?ms)^    internal System.Action ReplaceEnemyForm\(.*?^    \}').Value
$ai=Get-Content -Raw (Join-Path $root 'Assets/Scripts/Assembly-CSharp/ModelAi.cs')
$capture=[regex]::Match($ai,'(?ms)^    internal System.Action CaptureEnemyWeapon\(.*?^    \}').Value
if(!$method -or !$capture){throw 'Enemy form method extraction failed.'}
$fixture=Join-Path $root ('Temp/EnemyForm-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $fixture | Out-Null
$code=@"
using System;
using System.Collections.Generic;
using System.Linq;
class WeaponInfo{public string EffectiveTacticSubtype;}
class Parameters{public WeaponInfo Weapon;}
class Event{public Model Opponent;}
class Animation{public Animation Enemy;public Animation GetOtherAnimation()=>Enemy;public void SetOtherAnimation(Animation value){Enemy=value;}}
class Ai{public string enemyWeaponSubtype;public bool Fail;public void SetWeaponEnemy(string weapon){enemyWeaponSubtype=weapon;if(Fail)throw new Exception("AI binding");}
$capture
}
class WeaponModel:Model{}
class Model{
 public List<Model> _Enemies=new List<Model>();public List<WeaponModel> Weapons=new List<WeaponModel>();
 public List<WeaponModel> GetWeaponModels()=>Weapons;
 public Animation _Animation=new Animation();public Model combatTarget;public Event EventData=new Event();public Ai ai=new Ai();public Parameters Parameters=new Parameters();
$method
 static void Check(bool x,string why){if(!x)throw new Exception(why);}
 static void Main(){
 var observer=new Model();var old=new Model();var next=new Model();var other=new Model();
 var oldWeapon=new WeaponModel();var newWeapon=new WeaponModel();old.Weapons.Add(oldWeapon);next.Weapons.Add(newWeapon);
 next.Parameters.Weapon=new WeaponInfo{EffectiveTacticSubtype="Spear"};
 observer._Enemies.AddRange(new Model[]{old,oldWeapon,other,oldWeapon});
 observer.combatTarget=old;observer._Animation.Enemy=old._Animation;observer.EventData.Opponent=old;observer.ai.enemyWeaponSubtype="Claws";
 var original=observer._Enemies.ToArray();var restore=observer.ReplaceEnemyForm(old,next);
 Check(observer._Enemies.SequenceEqual(new Model[]{next,newWeapon,other}),"old weapon references removed, replacement children installed, unrelated retained");
 Check(observer.combatTarget==next&&observer._Animation.Enemy==next._Animation&&observer.EventData.Opponent==next&&observer.ai.enemyWeaponSubtype=="Spear","all direct targeting references exchanged");
 restore();Check(observer._Enemies.SequenceEqual(original)&&observer.combatTarget==old&&observer._Animation.Enemy==old._Animation&&observer.EventData.Opponent==old&&observer.ai.enemyWeaponSubtype=="Claws","exact original targeting state restored");
 observer.ai.Fail=true;bool failed=false;try{observer.ReplaceEnemyForm(old,next);}catch(Exception){failed=true;}
 Check(failed&&observer._Enemies.SequenceEqual(original)&&observer._Animation.Enemy==old._Animation&&observer.combatTarget==old&&observer.ai.enemyWeaponSubtype=="Claws","partial failure rolls back category and targeting");observer.ai.Fail=false;
 next.Parameters.Weapon=null;restore=observer.ReplaceEnemyForm(old,next);Check(observer.ai.enemyWeaponSubtype==null,"unarmed form clears old category");restore();
 observer.combatTarget=other;observer._Animation.Enemy=other._Animation;observer.EventData.Opponent=other;
 restore=observer.ReplaceEnemyForm(old,next);Check(observer.combatTarget==other&&observer._Animation.Enemy==other._Animation&&observer.EventData.Opponent==other&&observer.ai.enemyWeaponSubtype=="Claws","unrelated current target preserved");restore();
 failed=false;try{observer.ReplaceEnemyForm(next,old);}catch(InvalidOperationException){failed=true;}Check(failed&&observer._Enemies.SequenceEqual(original),"stale identity rejects before mutation");
 Console.WriteLine("PASS: production enemy form exchange and AI snapshot; weapon children, direct targets, exact restoration, unarmed category, unrelated targets and injected failure. Animation and AI resolution services controlled.");
 }
}
"@
[IO.File]::WriteAllText((Join-Path $fixture 'Program.cs'),$code)
[IO.File]::WriteAllText((Join-Path $fixture 'Test.csproj'),'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>')
dotnet run --project (Join-Path $fixture 'Test.csproj') --verbosity quiet
if($LASTEXITCODE -ne 0){throw 'Enemy form checks failed.'}
