# Execute the production native-to-safe AI adapter and native nominal timing formula.
# Animation storage and model services are controlled; this is not a combat playtest.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$bridge = Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Eclipse/Modding/ModRuntimeP1D.cs')
$contracts = Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Eclipse/Runtime/Modding/ModScripting.cs')
$animation = Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/InfoAnimation.cs')
function Extract([string]$source, [string]$pattern) {
    $found = [regex]::Match($source, $pattern)
    if (!$found.Success) { throw "AI snapshot source not found: $pattern" }
    return $found.Value
}
$dto = @('ModAiActionTiming','ModAiActionInput','ModAiActionSnapshot','ModAnimationIntervalSnapshot','ModAnimationSnapshot') | ForEach-Object {
    Extract $contracts "(?ms)^    public sealed class $_\b.*?^    \}"
}
$adapter = Extract $bridge '(?ms)^        private static ModAiActionSnapshot AiActionSnapshot\(.*?^        \}'
$inputs = Extract $bridge '(?ms)^        private static void AppendAiInputs\(.*?^        \}'
$count = Extract $animation '(?ms)^\tpublic int GetFrameCount\(.*?^\t\}'
$duration = Extract $animation '(?ms)^\tpublic int GetTotalFrames\(.*?^\t\}'
$loop = Extract $animation '(?ms)^\tpublic bool GetIsLooped\(.*?^\t\}'
$enum = Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/FightCID.cs')
$playback = Extract $bridge '(?ms)^        public static ModAnimationSnapshot CaptureAnimationSnapshot\(.*?^        \}'
$controllerSource = Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/ModelAnimation.cs')
$getters = @('bool GetIsPlaying','int GetSign','InfoAnimation GetCurrentInfo','List<IntervalAnimation> GetActiveIntervals') | ForEach-Object {
    Extract $controllerSource "(?ms)^\tpublic $_\(.*?^\t\}"
}
$intervalSource = Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/IntervalAnimation.cs')
$intervalEnum = Extract $intervalSource '(?ms)^\tpublic enum IntervalType.*?^\t\}'
$fixture = @'
using System;
using System.Collections.Generic;
namespace AiSnapshotTestScope {
using Eclipse.Modding;
/* ENUM */
public class KeyData {
 public List<int> StarterKeys=new List<int>(), AdditionalKeys=new List<int>(), ReleaseKeys=new List<int>();
}
public class ConditionKeys { public KeyData RequiredKeys=new KeyData(); }
public class IntervalAnimation {
 /* INTERVAL_ENUM */
 public string Name; public IntervalType Type;
}
public class Model {
 public ModelAnimation Controller=new ModelAnimation();
 public ModelAnimation GetAnimationModule(){return Controller;}
}
public class ModelAnimation {
 public bool isPlaying=true; public int sign=1;
 public InfoAnimation currentInfo=new InfoAnimation();
 public List<IntervalAnimation> activeIntervals=new List<IntervalAnimation>();
 /* GETTERS */
}
public class InfoAnimation {
 public enum AnimationKind { AnimationNone, AnimationMove, AnimationAttack }
 public string Name="custom"; public AnimationKind Type=AnimationKind.AnimationAttack;
 public int Priority=7, FirstFrame=3, AnimationEndFrame=12, MidFrames=2;
 public bool isLooped;
 public ConditionKeys Keys=new ConditionKeys();
 public ConditionKeys GetFirstKeysCondition(){return Keys;}
 /* COUNT */
 /* DURATION */
 /* LOOP */
}
namespace Eclipse.Modding {
 public class ModContentException : Exception { public ModContentException(string message):base(message){} }
 /* DTO */
 public static class AiSnapshotFixture {
  public static ModAiActionSnapshot Snapshot(InfoAnimation action){return AiActionSnapshot(action);}
  /* ADAPTER */
  /* INPUTS */
  /* PLAYBACK */
 }
}
public static class AiSnapshotTests {
 static int checks;
 static void Check(bool condition,string message){checks++;if(!condition)throw new Exception(message);}
 static void Reject(Action action,string message){bool rejected=false;try{action();}catch(ArgumentException){rejected=true;}Check(rejected,message);}
 public static void Run(){
  var native=new InfoAnimation();native.Keys.RequiredKeys.StarterKeys.Add((int)FightCID.Kick);
  native.Keys.RequiredKeys.AdditionalKeys.Add((int)FightCID.QuadrantBack);
  native.Keys.RequiredKeys.ReleaseKeys.Add((int)FightCID.Punch);
  var result=AiSnapshotFixture.Snapshot(native);
  Check(result.Name=="custom"&&result.Type=="attack"&&result.Priority==7,"Native identity metadata mismatch");
  Check(result.Timing.FirstSample==3&&result.Timing.LastSample==12&&result.Timing.MidFrames==2,"Sample bounds lost");
  Check(result.Timing.NominalFrames==native.GetTotalFrames()&&result.Timing.NominalFrames==30&&result.Timing.NominalSeconds==0.5,"Native nominal timing mismatch");
  Check(!result.Timing.Looped,"Non-looping clip marked looping");
  Check(result.Inputs.Count==3&&result.Inputs[0].Control=="Kick"&&result.Inputs[0].Press=="tap"&&result.Inputs[1].Control=="Back"&&result.Inputs[1].Press=="hold"&&result.Inputs[2].Control=="Punch"&&result.Inputs[2].Press=="release","Native key combination mismatch");
  native.Keys.RequiredKeys.StarterKeys.Clear();native.isLooped=true;native.AnimationEndFrame=22;
  Check(result.Inputs.Count==3&&result.Timing.LastSample==12&&!result.Timing.Looped,"Native mutation changed an existing snapshot");
  Check(AiSnapshotFixture.Snapshot(native).Timing.Looped,"Native looping flag missing");
  foreach(var type in new[]{InfoAnimation.AnimationKind.AnimationNone,InfoAnimation.AnimationKind.AnimationMove}){
   native.Type=type;Check(AiSnapshotFixture.Snapshot(native).Type==(type==InfoAnimation.AnimationKind.AnimationNone?"none":"move"),"Native type mapping mismatch");
  }
  string[] names={"Up","Up-Forward","Forward","Down-Forward","Down","Down-Back","Back","Up-Back","Punch","Kick","Ranged","Magic","RaidCharge","Super"};
  for(int i=0;i<names.Length;i++){
   native.Keys.RequiredKeys.StarterKeys.Clear();native.Keys.RequiredKeys.StarterKeys.Add(i+1);
   Check(AiSnapshotFixture.Snapshot(native).Inputs[0].Control==names[i],"Control mapping mismatch: "+names[i]);
  }
  native.Keys.RequiredKeys.StarterKeys.Clear();native.Keys.RequiredKeys.StarterKeys.Add(999);
  Check(AiSnapshotFixture.Snapshot(native).Inputs[0].Control=="Unknown","Unknown control leaked an unrelated enum label");
  native.Keys=null;Check(AiSnapshotFixture.Snapshot(native).Inputs.Count==0,"Missing keys did not yield empty metadata");
  foreach(int spacing in new[]{0,1,2,8}){
   native.FirstFrame=0;native.AnimationEndFrame=59;native.MidFrames=spacing;
   Check(AiSnapshotFixture.Snapshot(native).Timing.NominalFrames==native.GetTotalFrames(),"Nominal timing spacing mismatch");
  }
  Reject(()=>new ModAiActionTiming(-1,4,0,false),"Negative sample accepted");
  Reject(()=>new ModAiActionTiming(5,4,0,false),"Reversed sample range accepted");
  Reject(()=>new ModAiActionTiming(0,4,-1,false),"Negative spacing accepted");
  Reject(()=>new ModAiActionTiming(0,int.MaxValue,int.MaxValue,false),"Overflowing duration accepted");
  var source=new[]{new ModAiActionInput("Punch","tap")};var copied=new ModAiActionSnapshot("copied",inputs:source);
  source[0]=new ModAiActionInput("Kick","release");Check(copied.Inputs[0].Control=="Punch","Host array mutation leaked");
  bool immutable=false;try{((IList<ModAiActionInput>)copied.Inputs)[0]=source[0];}catch(NotSupportedException){immutable=true;}
  Check(immutable,"Snapshot input collection is writable");
  Reject(()=>new ModAiActionSnapshot("invalid",inputs:new ModAiActionInput[65]),"Unbounded input list accepted");
  native.Keys=new ConditionKeys();for(int i=0;i<65;i++)native.Keys.RequiredKeys.StarterKeys.Add(9);
  bool bounded=false;try{AiSnapshotFixture.Snapshot(native);}catch(ModContentException){bounded=true;}
  Check(bounded,"Adapter input list allocation was unbounded");
  var model=new Model();var active=model.Controller.activeIntervals;
  Check(AiSnapshotFixture.CaptureAnimationSnapshot(null)==null,"Null model exposed animation");
  var attack=new IntervalAnimation{Name="blade contact",Type=IntervalAnimation.IntervalType.INTERVAL_ATTACK};active.Add(attack);
  var observed=AiSnapshotFixture.CaptureAnimationSnapshot(model);
  Check(observed.Name=="custom"&&observed.Type=="attack"&&observed.Facing==1&&observed.Intervals[0].Type=="attack"&&observed.Intervals[0].Name=="blade contact","Active animation metadata mismatch");
  attack.Name="changed";active.Clear();model.Controller.sign=-1;
  Check(observed.Intervals.Count==1&&observed.Intervals[0].Name=="blade contact"&&observed.Facing==1,"Native changes leaked into retained animation");
  Check(AiSnapshotFixture.CaptureAnimationSnapshot(model).Facing==-1&&AiSnapshotFixture.CaptureAnimationSnapshot(model).Intervals.Count==0,"Fresh facing/interval snapshot stale");
  string[] kinds={"none","unstable","uninterrupt","self_uninterrupt","attack","block","invulnerable","invisible"};
  for(int i=0;i<kinds.Length;i++){
   active.Clear();active.Add(new IntervalAnimation{Type=(IntervalAnimation.IntervalType)i});
   var mapped=AiSnapshotFixture.CaptureAnimationSnapshot(model);
   Check(mapped.Intervals[0].Type==kinds[i]&&mapped.Intervals[0].Name=="","Interval mapping mismatch: "+kinds[i]);
  }
  model.Controller.isPlaying=false;
  Check(AiSnapshotFixture.CaptureAnimationSnapshot(model)==null,"Stopped playback exposed stale intervals");
  model.Controller.isPlaying=true;model.Controller.currentInfo=null;
  Check(AiSnapshotFixture.CaptureAnimationSnapshot(model)==null,"Missing native animation fabricated observation");
  model.Controller.currentInfo=new InfoAnimation();active.Clear();active.Add(null);
  Check(AiSnapshotFixture.CaptureAnimationSnapshot(model)==null,"Malformed interval fabricated observation");
  active.Clear();for(int i=0;i<257;i++)active.Add(attack);
  Check(AiSnapshotFixture.CaptureAnimationSnapshot(model)==null,"Unbounded interval observation");
  active.Clear();model.Controller.sign=0;
  Check(AiSnapshotFixture.CaptureAnimationSnapshot(model)==null,"Invalid facing fabricated observation");
  var intervalCopy=new[]{new ModAnimationIntervalSnapshot("original","block")};
  var detached=new ModAnimationSnapshot("block","move",1,intervalCopy);intervalCopy[0]=new ModAnimationIntervalSnapshot("changed","attack");
  Check(detached.Intervals[0].Name=="original","Host interval array mutation leaked");
  immutable=false;try{((IList<ModAnimationIntervalSnapshot>)detached.Intervals)[0]=intervalCopy[0];}catch(NotSupportedException){immutable=true;}
  Check(immutable,"Animation interval snapshot is writable");
  Console.WriteLine("PASS: "+checks+" production AI snapshot adapter, native nominal timing, control mapping, immutable copy and invalid-data checks; animation/model services controlled.");
 }
}
}
'@
$fixture = $fixture.Replace('/* ENUM */',$enum).Replace('/* DTO */',($dto -join "`n")).Replace('/* ADAPTER */',$adapter).Replace('/* INPUTS */',$inputs).Replace('/* COUNT */',$count).Replace('/* DURATION */',$duration).Replace('/* LOOP */',$loop)
$fixture = $fixture.Replace('/* PLAYBACK */',$playback).Replace('/* GETTERS */',($getters -join "`n")).Replace('/* INTERVAL_ENUM */',$intervalEnum)
Add-Type -TypeDefinition $fixture
[AiSnapshotTestScope.AiSnapshotTests]::Run()
