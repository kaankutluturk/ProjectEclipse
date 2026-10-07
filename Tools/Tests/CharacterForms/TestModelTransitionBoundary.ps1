$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$source=Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/Fight.cs')
$queue=[regex]::Match($source,'(?s)    private sealed class PendingModelTransition.*?(?=    // Participant identity)').Value
$queue=[regex]::Replace($queue,'(?s)    // A reversible registration stage.*?(?=    private readonly Dictionary<Model, PendingModelTransition>)','')
$render=[regex]::Match($source,'(?ms)^\tpublic void Render\(\).*?^\t\}').Value
if (!$queue -or !$render) { throw 'Production boundary extraction failed.' }
if ($source -notmatch 'public void Unload\(\)\s*\{\s*CloseModelTransitions\(\);') { throw 'Unload cancellation hook missing.' }
$fixture=Join-Path $root ('Temp/ModelTransition-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
$code=@'
using System;
using System.Collections.Generic;
namespace UnityEngine { static class Debug { public static void LogException(Exception e) {} } }
namespace Eclipse.Multiplayer {
    static class VersusTickDriver { public static bool Speculative; public static bool Barrier()=>Speculative; }
}
namespace Eclipse.Rendering.Interpolation {
    static class FightInterpolation {
        public static Action FightStep=()=>{},CameraStep=()=>{};
        public static void MarkFightStep()=>FightStep();
        public static void MarkCameraStep()=>CameraStep();
    }
}
class Model { public float Health=100; public float GetLife()=>Health; }
class Round { public bool processing=true; public int round=1; }
class Fight {
QUEUE
RENDER
    sealed class CameraSlot { public int Clears; public void Clear(){Clears++;} }
    readonly CameraSlot _eclipseCamera=new CameraSlot();
    // This fixture controls motion; the full production motion queue has its own tests.
    int motionCancels; void CancelEclipseFighterMotion(){motionCancels++;}
    int playbackCancels; void CancelEclipseFighterPlayback() { playbackCancels++; }
    int projectileCancels,actorCancels; string actorEndReason;
    void CancelEclipseProjectiles(){projectileCancels++;}
    void CancelEclipseActors(string reason){actorCancels++;actorEndReason=reason;}
    HashSet<Model> actors=new HashSet<Model>();
    bool IsEclipseFormParticipant(Model model)=>model==_playerModel||model==_enemyModel||actors.Contains(model);
    Round round=new Round(); Model _playerModel=new Model(),_enemyModel=new Model();
    bool _eclipseFightEndDispatched=false,isRenderFight=true,isRenderCamera=true,IsLocalVersus; int frame;
    Action Step=()=>{}; List<string> order=new List<string>();
    void RenderFight(){order.Add("step"); Step();}
    void RenderCamera(){order.Add("camera");}
    static void Check(bool yes,string message){if(!yes)throw new Exception(message);}
    public static void Main(){
        var f=new Fight(); int applied=0,completed=0;
        f.Step=()=>{Check(f.QueueModelTransition(f._playerModel,()=>{applied++;f.order.Add("apply");},e=>{Check(e==null,"successful completion");completed++;}),"queue in step"); Check(applied==0,"not inline");};
        Eclipse.Rendering.Interpolation.FightInterpolation.FightStep=()=>f.order.Add("fight interpolation");
        Eclipse.Rendering.Interpolation.FightInterpolation.CameraStep=()=>f.order.Add("camera interpolation");
        f.Render(); Check(string.Join(",",f.order)=="step,apply,fight interpolation,camera,camera interpolation" && completed==1,"boundary order");
        Eclipse.Rendering.Interpolation.FightInterpolation.FightStep=()=>{};
        Eclipse.Rendering.Interpolation.FightInterpolation.CameraStep=()=>{};
        f.Step=()=>{}; f.order.Clear();
        Check(f.QueueModelTransition(f._playerModel,()=>applied++,e=>completed++),"queue");
        Check(!f.QueueModelTransition(f._playerModel,()=>{},e=>{}),"duplicate");
        f.isRenderFight=false;f.Render();Check(applied==1,"pause retains request");
        f.isRenderFight=true;f.Render();Check(applied==2,"resume applies");
        int canceled=0;
        f.QueueModelTransition(f._playerModel,()=>throw new Exception("must not apply"),e=>{Check(e is OperationCanceledException,"round cancellation");canceled++;});
        f.round.round++; f.Render(); Check(canceled==1,"stale round");
        f.QueueModelTransition(f._playerModel,()=>throw new Exception("must not apply"),e=>{Check(e is OperationCanceledException,"death cancellation");canceled++;});
        f._playerModel.Health=0;f.Render();Check(canceled==2,"dead fighter");f._playerModel.Health=100;
        f.QueueModelTransition(f._playerModel,()=>{},e=>{Check(e is OperationCanceledException,"stale model cancellation");canceled++;});
        f._playerModel=new Model();f.Render();Check(canceled==3,"model identity");
        int nested=0;
        f.QueueModelTransition(f._playerModel,()=>{},e=>{f.QueueModelTransition(f._playerModel,()=>nested++,x=>{});f.DrainModelTransitions();});
        f.Render();Check(nested==0,"reentrant drain postponed");f.Render();Check(nested==1,"next frame");
        bool failed=false;f.QueueModelTransition(f._playerModel,()=>throw new InvalidOperationException(),e=>failed=e is InvalidOperationException);f.Render();Check(failed,"failure completion");
        f.QueueModelTransition(f._playerModel,()=>{},e=>throw new Exception("completion error"));f.Render();
        f.QueueModelTransition(f._playerModel,()=>{},e=>{Check(e is OperationCanceledException,"unload cancellation");canceled++;});
        Check(f.actorCancels==0&&f.projectileCancels==0,"ordinary successful/rejected model transitions do not close actor/projectile lifetime");
        f.CloseModelTransitions();f.CloseModelTransitions();Check(f.actorCancels==2&&f.projectileCancels==2&&f.actorEndReason=="round_ended","each close cancels companions and projectiles with round reason");Check(canceled==4,"unload exactly once"); Check(f.motionCancels==2,"each close clears pending motion"); Check(f.playbackCancels==2,"each close cancels pending playback");
        Check(f._eclipseCamera.Clears==2,"each close clears camera ownership");
        Check(!f.QueueModelTransition(f._playerModel,()=>{},e=>{}),"closed queue");
        var g=new Fight();int other=0,done=0;
        g.QueueModelTransition(g._playerModel,()=>g.CloseModelTransitions(),e=>done++);
        g.QueueModelTransition(g._enemyModel,()=>other++,e=>done++);
        g.Render();Check(other==0&&done==2,"unload during drain cancels remaining exactly once");
        var local=new Fight{IsLocalVersus=true};int confirmed=0,finished=0;
        local.QueueModelTransition(local._playerModel,()=>confirmed++,e=>{Check(e==null,"confirmed completion");finished++;});
        Eclipse.Multiplayer.VersusTickDriver.Speculative=true;local.Render();
        Check(confirmed==0&&finished==0,"speculative versus step retains transition");
        Eclipse.Multiplayer.VersusTickDriver.Speculative=false;local.Render();local.Render();
        Check(confirmed==1&&finished==1,"confirmed versus step applies exactly once");
        var actorFight=new Fight();var actorBody=new Model();actorFight.actors.Add(actorBody);int actorApplied=0,actorFailed=0;
        Check(actorFight.QueueModelTransition(actorBody,()=>actorApplied++,e=>Check(e==null,"actor completion")),"owned actor queues");actorFight.Render();Check(actorApplied==1,"actor transition applied at boundary");
        actorFight.QueueModelTransition(actorBody,()=>actorApplied++,e=>{Check(e is OperationCanceledException,"removed actor cancellation");actorFailed++;});
        actorFight.actors.Remove(actorBody);actorFight.Render();Check(actorApplied==1&&actorFailed==1,"actor removed before boundary cannot transform");
        Check(!actorFight.QueueModelTransition(new Model(),()=>{},e=>{}),"unmanaged root rejected");
        Console.WriteLine("PASS: production transition queue/Render boundary; deferred apply, interpolation order, pause, duplicate, stale round/model, death, reentrancy, failure, unload and speculative-versus barrier. Model/simulation/camera/interpolation services controlled.");
    }
}
'@
$code=$code.Replace('QUEUE',$queue).Replace('RENDER',$render)
[IO.File]::WriteAllText((Join-Path $fixture 'Program.cs'),$code)
[IO.File]::WriteAllText((Join-Path $fixture 'Test.csproj'),'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><EnableDefaultCompileItems>true</EnableDefaultCompileItems></PropertyGroup></Project>')
dotnet run --project (Join-Path $fixture 'Test.csproj') --verbosity quiet
if ($LASTEXITCODE -ne 0) { throw 'Transition boundary checks failed.' }
