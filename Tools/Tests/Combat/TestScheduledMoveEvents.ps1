$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$source = Get-Content -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/SelectAnimation.cs') -Raw
$start = $source.IndexOf('var actionEvents = _PendingEvents.ToArray();')
$end = $source.IndexOf('foreach (TriggerStruct item3', $start)
if ($start -lt 0 -or $end -le $start) { throw 'Scheduled event drain not found.' }
$batch = $source.Substring($start, $end - $start)
$fixture = Join-Path $root ('Temp/ScheduledMoveEvents-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
$program = @'
using System;
using System.Collections.Generic;
sealed class Model {
    readonly Action<int> action; public Model(Action<int> action){this.action=action;}
    public Model GetAnimationModule()=>this; public void TriggerActionsForEvent(int type)=>action(type);
}
sealed class EventModelDelayed { public Model Owner; public int Type; }
sealed class Program {
    readonly List<EventModelDelayed> _PendingEvents=new List<EventModelDelayed>();
    void Step(){ __BATCH__ }
    static int checks; static void Check(bool value,string message){checks++;if(!value)throw new Exception(message);}
    static void Main(){
        var h=new Program();var seen=new List<int>(); Model model=null;
        model=new Model(type=>{seen.Add(type);if(type==1){
            // Playing another move raises lifecycle events synchronously.
            h._PendingEvents.Add(new EventModelDelayed{Owner=model,Type=3});
            h._PendingEvents.Add(new EventModelDelayed{Owner=model,Type=4});
        }});
        h._PendingEvents.Add(new EventModelDelayed{Owner=model,Type=1});
        h._PendingEvents.Add(new EventModelDelayed{Owner=model,Type=2});
        h._PendingEvents.Add(new EventModelDelayed{Type=99});
        h.Step();Check(string.Join(",",seen)=="1,2","Reentrant lifecycle events ran recursively or interrupted the old batch");
        Check(h._PendingEvents.Count==2,"New lifecycle events were discarded");
        h.Step();Check(string.Join(",",seen)=="1,2,3,4","New events did not retain native order");
        Check(h._PendingEvents.Count==0,"Completed queue was not consumed");
        h.Step();Check(seen.Count==4,"Scheduled actions replayed after queue drained");
        Console.WriteLine("PASS: "+checks+" production scheduled-event batching checks; native actor/event sources controlled.");
    }
}
'@
[IO.File]::WriteAllText((Join-Path $fixture 'Program.cs'), $program.Replace('__BATCH__', $batch))
[IO.File]::WriteAllText((Join-Path $fixture 'Test.csproj'), '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>')
dotnet run --project (Join-Path $fixture 'Test.csproj') --verbosity quiet
if ($LASTEXITCODE -ne 0) { throw 'Scheduled move event regression failed.' }
