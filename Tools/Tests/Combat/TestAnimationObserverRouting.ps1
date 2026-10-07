$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$source = Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/Model.cs')
$methods = [regex]::Match($source, '(?ms)^\tpublic void OnAnimationStarted\(.*?^\t\}').Value
$methods += [regex]::Match($source, '(?ms)^\tprotected void ObserveEnemyAnimationStarted\(.*?^\t\}').Value
if (!$methods.Contains('ObserveEnemyAnimationStarted') -or !$methods.Contains('OnAnimationStarted')) { throw 'Native animation observer methods missing.' }
$fixture = Join-Path $root ('Temp/AnimationObserverRouting-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
$code = @'
using System;
using System.Collections.Generic;
class InfoAnimation { public string Name; }
static class AiData { public static bool Both; public static bool get_BothBotEnabled()=>Both; }
class ModelAi {
 public List<Model> Enemies=new List<Model>(); public List<InfoAnimation> Own=new List<InfoAnimation>();
 public void StartAnimationEnemy(Model source)=>Enemies.Add(source);
 public void StartAnimationBot(InfoAnimation move)=>Own.Add(move);
}
class ModelAnimation { public InfoAnimation Current=new InfoAnimation(); public InfoAnimation GetCurrentInfo()=>Current; }
class Collision { public int Resets; public void ResetInterval()=>Resets++; }
class Model {
 public class EventModel { public object Data; }
 public EventModel EventData=new EventModel(); public List<Model> _Enemies=new List<Model>();
 public ModelAi ai=new ModelAi(); public ModelAnimation _Animation=new ModelAnimation();
 public Collision _Collision=new Collision(); public Model Target; public bool Ai=true,Weapon;
 public int Wait=9,Events; public object EventData;
 public Model GetCombatTarget()=>Target; public bool IsWeapon()=>Weapon; public bool IsAiControlled()=>Ai;
 public void SetDecisionDelay(int value)=>Wait=value;
 public void CallEvent(int kind,EventModel data){if(kind!=2)throw new Exception("Wrong event");Events++;EventData=data.Data;}
 METHODS
}
static class Program {
 static int checks;
 static void Check(bool value,string why){checks++;if(!value)throw new Exception(why);}
 static void Main(){
  var left=new Model();var right=new Model();left.Target=right;right.Target=left;
  left._Enemies.Add(right);right._Enemies.Add(left);var move=new InfoAnimation{Name="left first"};left._Animation.Current=move;
  left.OnAnimationStarted(move);
  Check(left._Collision.Resets==1&&left.Events==1&&ReferenceEquals(left.EventData,move),"native collision/event dispatch stays intact");
  Check(left.ai.Own.Count==1&&left.ai.Own[0]==move,"pair source observes its own move once");
  Check(right.ai.Enemies.Count==1&&right.ai.Enemies[0]==left&&right.Wait==0,"pair observer sees source and wakes decision delay");
  Check(right.ai.Own.Count==0&&left.ai.Enemies.Count==0,"pair does not swap own and enemy observations");
  var third=new Model{Target=left};var child=new Model{Target=left,Weapon=true};var unrelated=new Model{Target=right};
  left._Enemies.AddRange(new[]{third,child,unrelated,third,null,left});
  var next=new InfoAnimation{Name="left next"};left._Animation.Current=next;left.OnAnimationStarted(next);
  Check(right.ai.Enemies.Count==2&&third.ai.Enemies.Count==1,"every actual root observer is notified once");
  Check(third.ai.Enemies[0]==left&&third.Wait==0,"independent observer sees actual source");
  Check(child.ai.Enemies.Count==0&&child.Wait==9,"weapon observer stays excluded");
  Check(unrelated.ai.Enemies.Count==0&&unrelated.Wait==9,"root targeting another source is not notified");
  Check(left.ai.Own.Count==2&&left.ai.Own[1]==next,"source own observation is not multiplied by observer count");
  third._Enemies.AddRange(new[]{left,right});var attack=new InfoAnimation{Name="third attack"};third._Animation.Current=attack;
  third.OnAnimationStarted(attack);
  Check(third.ai.Own.Count==1&&third.ai.Own[0]==attack,"third source updates itself even when its target watches another root");
  Check(left.ai.Enemies.Count==0&&right.ai.Own.Count==0,"third source cannot corrupt original pair observations");
  left.Target=third;third.OnAnimationStarted(attack);
  Check(left.ai.Enemies.Count==1&&left.ai.Enemies[0]==third,"retargeted observer sees its new source");
  third.Ai=false;left.Ai=false;left.Wait=17;third.OnAnimationStarted(attack);
  Check(third.ai.Own.Count==2&&left.ai.Enemies.Count==1&&left.Wait==0,"per-model AI flags remain effective while native observer wake remains");
  AiData.Both=true;third.OnAnimationStarted(attack);
  Check(third.ai.Own.Count==3&&left.ai.Enemies.Count==2,"native both-bot override remains effective");
  child._Enemies.Add(left);left.Target=child;child.OnAnimationStarted(attack);
  Check(child.Events==1&&child.ai.Own.Count==0&&left.ai.Enemies.Count==2,"weapon source preserves events without waking root controllers");
  Console.WriteLine("PASS: "+checks+" production animation-start routing checks; pair/third-root/source/target identity, own-observation once, retargeting, duplicate/null/self/weapon exclusions and AI flags. Animation/controller services controlled; native gameplay acceptance separate.");
 }
}
'@
[IO.File]::WriteAllText((Join-Path $fixture 'Program.cs'), $code.Replace('METHODS',$methods))
[IO.File]::WriteAllText((Join-Path $fixture 'Test.csproj'), '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><NoWarn>CS0649</NoWarn></PropertyGroup></Project>')
dotnet run --project (Join-Path $fixture 'Test.csproj') --verbosity quiet
if ($LASTEXITCODE -ne 0) { throw 'Native animation observer routing checks failed.' }
