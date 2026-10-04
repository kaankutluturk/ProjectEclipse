using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Eclipse.Modding
{
    public sealed partial class MoonSharpScriptRuntime
    {
        private sealed partial class MoonSharpScriptContext
        {
            private readonly Dictionary<Table,DefinitionId> _actorDefinitions=new Dictionary<Table,DefinitionId>();
            private sealed class ActorQueryBudget
            {public int Used;public readonly Dictionary<Table,IModActor> Handles=new Dictionary<Table,IModActor>();}
            private void AddActorModule(Table root)
            {
                var actors=new Table(_script);
                actors.Set("register",DynValue.NewCallback((ctx,args)=>ApiCall("sf2.actors.register",()=>
                {
                    const string fn="sf2.actors.register";
                    if(args.Count!=1)throw new ModContentException("Actor registration expects exactly one table.");
                    var spec=args.AsType(0,fn,DataType.Table,false).Table;
                    ValidateFields(spec,fn,"id","character","team","ai","lifetime_frames","max_health","behavior","parameters");
                    string team=spec.Get("team").IsNil()?"owner":RequiredString(spec,"team",fn);
                    if(team!="owner"&&team!="opponent")throw new ModContentException("Actor team must be owner or opponent, relative to the spawning fighter.");
                    if(!spec.Get("ai").IsNil()&&spec.Get("ai").Type!=DataType.Boolean)throw new ModContentException("Actor ai must be boolean.");
                    var health=spec.Get("max_health");
                    if(!health.IsNil()&&health.Type!=DataType.Number)throw new ModContentException("Actor max_health must be numeric.");
                    ModBehaviorDefinition behavior=null;
                    if(!spec.Get("behavior").IsNil())behavior=RequiredHandle(spec,"behavior",_behaviorHandles,"behavior",fn);
                    if(behavior==null&&!spec.Get("parameters").IsNil())throw new ModContentException("Actor parameters require a behavior.");
                    var definition=_api.RegisterActor(RequiredString(spec,"id",fn),RequiredHandle(spec,"character",_warriorHandles,"warrior",fn),
                        team=="opponent",spec.Get("ai").IsNil()||spec.Get("ai").Boolean,
                        spec.Get("lifetime_frames").IsNil()?ModActorLimits.DefaultLifetimeFrames:RequiredInt(spec,"lifetime_frames",fn),
                        health.IsNil()?1:health.Number,behavior?.Id,
                        behavior==null?null:OptionalTypedParameterMap(spec,"parameters",behavior.Parameters,fn));
                    return NewHandle(_actorDefinitions,definition.Id);
                })));
                root.Set("actors",DynValue.NewTable(actors));
            }
            private void CheckActorCallback(ModEffectEvent kind,Func<bool> active,bool events=false)
            {
                if(!active())throw new ScriptRuntimeException("Actor references have expired; reacquire inside the current callback.");
                _api.RequireCapability("combat.actors");
                if(!events&&(kind==ModEffectEvent.FightBegin||kind==ModEffectEvent.RoundBegin||kind==ModEffectEvent.RoundEnd||kind==ModEffectEvent.FightEnd))
                    throw new ScriptRuntimeException("Live actor operations require an active simulation callback.");
            }
            private DynValue SpawnFighterActor(CallbackArguments args,Table fighterTable,IModFighterOperations fighter,ModEffectEvent kind,Func<bool> active)
            {
                CheckActorCallback(kind,active);
                int offset=args[0].Type==DataType.Table&&args[0].Table==fighterTable?1:0,count=args.Count-offset;
                if(count<3||count>4||args[offset].Type!=DataType.Table||!_actorDefinitions.TryGetValue(args[offset].Table,out var definition))
                    throw new ScriptRuntimeException("spawn_actor expects an owned actor definition and numeric x, y and optional z offsets.");
                ReadActorOffsets(args,offset+1,count-1,out var x,out var y,out var z);
                if(!ModFighterMotionLimits.IsValid(x,y,z))throw new ScriptRuntimeException("Actor spawn offsets must be finite in -1000..1000 per axis.");
                var receipt=new Table(_script);receipt.Set("status",DynValue.NewString("queued"));
                void Complete(string id,string error)
                {
                    receipt.Set("status",DynValue.NewString(id!=null?"applied":"failed"));
                    receipt.Set("actor_id",id==null?DynValue.Nil:DynValue.NewString(id));
                    receipt.Set("error",id!=null?DynValue.Nil:DynValue.NewString(error??"Actor creation failed."));
                }
                if(!(fighter is IModFighterActors provider))Complete(null,"Actor spawning is unavailable.");
                else if(!provider.TrySpawnActor(_api.Mod.Id,definition,x,y,z,Complete,out var error))Complete(null,error);
                return DynValue.NewTable(receipt);
            }
            private DynValue GetFighterActors(CallbackArguments args,Table fighterTable,IModFighterOperations fighter,ModEffectEvent kind,Func<bool> active,ActorQueryBudget budget,bool events)
            {
                CheckActorCallback(kind,active,events);
                int offset=args[0].Type==DataType.Table&&args[0].Table==fighterTable?1:0;
                if(args.Count!=offset)throw new ScriptRuntimeException("Actor queries expect no arguments.");
                if(++budget.Used>ModActorLimits.MaximumQueriesPerCallback)throw new ScriptRuntimeException("At most 32 combined actor/event queries are allowed per callback.");
                if(!(fighter is IModFighterActors provider))return DynValue.NewTuple(DynValue.Nil,DynValue.NewString("Actor observations are unavailable."));
                var result=new Table(_script);
                if(events)
                {
                    if(!provider.TryGetActorEvents(_api.Mod.Id,out var observations,out var error))return DynValue.NewTuple(DynValue.Nil,DynValue.NewString(error));
                    for(int i=0;i<observations.Count;i++)
                    {
                        var item=observations[i];var table=new Table(_script);
                        table.Set("sequence",DynValue.NewNumber(item.Sequence));table.Set("actor_id",DynValue.NewString(item.Id));
                        table.Set("kind",DynValue.NewString(item.Kind));table.Set("frame",DynValue.NewNumber(item.Frame));result.Set(i+1,DynValue.NewTable(table));
                    }
                }
                else
                {
                    if(!provider.TryGetActors(_api.Mod.Id,out var actors,out var error))return DynValue.NewTuple(DynValue.Nil,DynValue.NewString(error));
                    for(int i=0;i<actors.Count;i++)result.Set(i+1,ActorTable(actors[i],kind,active,budget));
                }
                return DynValue.NewTuple(DynValue.NewTable(result),DynValue.Nil);
            }
            private static void ReadActorOffsets(CallbackArguments args,int offset,int count,out double x,out double y,out double z)
            {
                if(count<2||count>3||args[offset].Type!=DataType.Number||args[offset+1].Type!=DataType.Number||
                    count==3&&!args[offset+2].IsNil()&&args[offset+2].Type!=DataType.Number)throw new ScriptRuntimeException("Actor offsets require numeric x, y and optional z.");
                x=args[offset].Number;y=args[offset+1].Number;z=count==3&&!args[offset+2].IsNil()?args[offset+2].Number:0;
            }
            private DynValue ActorTable(IModActor actor,ModEffectEvent kind,Func<bool> active,ActorQueryBudget budget)
            {
                var actorTable=new Table(_script);budget.Handles.Add(actorTable,actor);
                int Offset(CallbackArguments args)
                {CheckActorCallback(kind,active);return args[0].Type==DataType.Table&&args[0].Table==actorTable?1:0;}
                DynValue Result(bool ok,string error)=>DynValue.NewTuple(DynValue.NewBoolean(ok),ok?DynValue.Nil:DynValue.NewString(error??"Actor command rejected."));
                actorTable.Set("snapshot",DynValue.NewCallback((ctx,args)=>
                {
                    if(args.Count!=Offset(args))throw new ScriptRuntimeException("Actor snapshot expects no arguments.");
                    if(!actor.TrySnapshot(out var snapshot,out var error))return DynValue.NewTuple(DynValue.Nil,DynValue.NewString(error));
                    var copy=FighterSnapshotTable(snapshot.Fighter).Table;
                    copy.Set("id",DynValue.NewString(snapshot.Id));copy.Set("definition",DynValue.NewString(snapshot.Definition));
                    copy.Set("team",DynValue.NewString(snapshot.Team));copy.Set("target_id",snapshot.Target==null?DynValue.Nil:DynValue.NewString(snapshot.Target));
                    copy.Set("age_frames",DynValue.NewNumber(snapshot.AgeFrames));copy.Set("lifetime_frames",DynValue.NewNumber(snapshot.LifetimeFrames));
                    return DynValue.NewTuple(DynValue.NewTable(copy),DynValue.Nil);
                }));
                actorTable.Set("move_by",DynValue.NewCallback((ctx,args)=>
                {
                    int offset=Offset(args);ReadActorOffsets(args,offset,args.Count-offset,out var x,out var y,out var z);
                    if(!ModProjectileLimits.ValidDisplacement(x,y,z))throw new ScriptRuntimeException("Actor displacement must be finite in -100..100 per axis.");
                    return Result(actor.TryMoveBy(x,y,z,out var error),error);
                }));
                actorTable.Set("set_target",DynValue.NewCallback((ctx,args)=>
                {
                    int offset=Offset(args);if(args.Count!=offset+1)throw new ScriptRuntimeException("set_target expects player, opponent, nearest, or a live actor reference.");
                    IModActor other=null;string main=null;
                    if(args[offset].Type==DataType.String)main=args[offset].String;
                    else if(args[offset].Type!=DataType.Table||!budget.Handles.TryGetValue(args[offset].Table,out other))throw new ScriptRuntimeException("Target actor must come from this callback's actor query.");
                    return Result(actor.TrySetTarget(main,other,out var error),error);
                }));
                actorTable.Set("change_health",DynValue.NewCallback((ctx,args)=>
                {
                    int offset=Offset(args);
                    if(args.Count!=offset+1||args[offset].Type!=DataType.Number)throw new ScriptRuntimeException("Actor change_health expects one numeric amount.");
                    return Result(actor.TryChangeHealth(args[offset].Number,out var error),error);
                }));
                actorTable.Set("play_move",DynValue.NewCallback((ctx,args)=>
                {
                    int offset=Offset(args);
                    if(args.Count!=offset+1||args[offset].Type!=DataType.Table||!_moveHandles.TryGetValue(args[offset].Table,out var move))
                        throw new ScriptRuntimeException("Actor play_move expects a registered move handle.");
                    var receipt=new Table(_script);receipt.Set("status",DynValue.NewString("queued"));
                    void Complete(bool success,string error)
                    {receipt.Set("status",DynValue.NewString(success?"applied":"failed"));receipt.Set("error",success?DynValue.Nil:DynValue.NewString(error??"Actor move rejected."));}
                    if(!actor.TryPlayMove(move,Complete,out var failure))Complete(false,failure);
                    return DynValue.NewTable(receipt);
                }));
                actorTable.Set("remove",DynValue.NewCallback((ctx,args)=>
                {if(args.Count!=Offset(args))throw new ScriptRuntimeException("Actor remove expects no arguments.");return Result(actor.TryRemove(out var error),error);}));
                return DynValue.NewTable(actorTable);
            }
        }
    }
}
