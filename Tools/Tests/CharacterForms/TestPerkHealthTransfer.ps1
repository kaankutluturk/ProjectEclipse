$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$source=Get-Content -Raw (Join-Path $root 'Assets/Scripts/Assembly-CSharp/InfoPerk.cs')
$patterns=@('(?ms)^    internal System.Action TransferHealthEffect\(.*?^    \}', '(?ms)^\tpublic void Render\(.*?^\t\}', '(?ms)^\tprivate void ApplyHealthChangeTick\(.*?^\t\}', '(?ms)^\tprivate void ApplyHealthChange\(.*?^\t\}')
$methods=$patterns | ForEach-Object {$value=[regex]::Match($source,$_).Value;if(!$value){throw 'Health transfer extraction failed.'};$value}
$fixture=Join-Path $root ('Temp/PerkHealth-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $fixture | Out-Null
$code=@'
using System;
using System.Collections.Generic;
enum ActionType{ACTION_MOD_HEALTH_CHANGE}
class Model{public double Health=100;}
class ModHealthChange{public double Amount;public ActionType get_Type()=>ActionType.ACTION_MOD_HEALTH_CHANGE;public double GetPerFrameValue()=>Amount;}
class PerksStage{public class ActionPerk{public ModHealthChange Action;public Model TargetModel,SourceModel;public int ElapsedFrames,DurationFrames;public bool IsExpired;}}
class Fight{public static Fight Current=new Fight();public static Fight GetCurrentFight()=>Current;public void UpdateLife(Model model,double delta){model.Health+=delta;}}
class InfoPerk{
 List<PerksStage.ActionPerk> activeActions=new List<PerksStage.ActionPerk>();int Expired;
 void ExpireAction(PerksStage.ActionPerk action){Expired++;activeActions.Remove(action);}
 METHODS
 static void Check(bool x,string message){if(!x)throw new Exception(message);}
 static void Main(){
 foreach(double amount in new[]{-3d,4d}){
 var old=new Model();var next=new Model();var perk=new InfoPerk();var action=new PerksStage.ActionPerk{Action=new ModHealthChange{Amount=amount},TargetModel=old,SourceModel=old,ElapsedFrames=2,DurationFrames=3};perk.activeActions.Add(action);
 perk.Render();Check(old.Health==100+amount&&action.ElapsedFrames==3,"initial tick advances existing effect");
 var restore=perk.TransferHealthEffect(action,old,next);Check(action.ElapsedFrames==3&&action.DurationFrames==3&&next.Health==100,"transfer neither resets time nor applies an extra tick");
 restore();Check(action.TargetModel==old&&action.SourceModel==old,"rollback restores source and target");
 perk.TransferHealthEffect(action,old,next);perk.Render();perk.Render();
 Check(old.Health==100+amount&&next.Health==100+amount&&perk.Expired==1&&perk.activeActions.Count==0,"next native tick reaches new body and expires at original time");
 bool rejected=false;try{perk.TransferHealthEffect(action,next,old);}catch(InvalidOperationException){rejected=true;}Check(rejected,"expired effect cannot migrate");
 }
 var source=new Model();var replacement=new Model();var target=new Model();var other=new InfoPerk();var health=new PerksStage.ActionPerk{Action=new ModHealthChange{Amount=-2},TargetModel=target,SourceModel=source,DurationFrames=10};other.activeActions.Add(health);
 var undo=other.TransferHealthEffect(health,source,replacement);other.Render();Check(target.Health==98&&source.Health==100&&replacement.Health==100&&health.SourceModel==replacement,"source-only form change leaves affected opponent unchanged");undo();Check(health.SourceModel==source&&health.TargetModel==target,"source-only rollback");
 Console.WriteLine("PASS: production health-effect transfer, Render and health tick; damage/healing, no extra tick, unchanged lifetime, rollback and source-only migration. Native health application and expiration dispatch controlled.");
 }
}
'@
$code=$code.Replace('METHODS',($methods -join "`n"))
[IO.File]::WriteAllText((Join-Path $fixture 'Program.cs'),$code)
[IO.File]::WriteAllText((Join-Path $fixture 'Test.csproj'),'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>')
dotnet run --project (Join-Path $fixture 'Test.csproj') --verbosity quiet
if($LASTEXITCODE -ne 0){throw 'Health transfer checks failed.'}
