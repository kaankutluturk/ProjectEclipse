using System;
using System.IO;
using System.Linq;
using System.Xml;
using Eclipse.Modding;

static class Program
{
    static int checks;
    static string fixture;
    static void Check(bool condition,string message){checks++;if(!condition)throw new Exception(message);}
    static (ModContentCatalog Content,string Hash) Load(string suffix,bool valid=true,string caps="content.register",
        Action<IModInteractiveBehaviorScriptContext,ActorDefinition> verify=null)
    {
        string parent=Path.Combine(fixture,Guid.NewGuid().ToString("N")),dir=Path.Combine(parent,"fixture.actors");
        Directory.CreateDirectory(Path.Combine(dir,"scripts"));
        File.WriteAllText(Path.Combine(dir,"mod.toml"),"schema=1\nid=\"fixture.actors\"\nname=\"Actors\"\nversion=\"1.0.0\"\nauthors=[\"Fixture\"]\nentrypoint=\"scripts/main.lua\"\ncapabilities=[\""+caps+"\"]\n[[dependencies]]\nid=\"core\"\nversion=\">=1.0.0 <2.0.0\"\n");
        const string start="local sf2=require('sf2');local warrior=sf2.warriors.register{id='unit',template=sf2.warriors.get_template('core:warrior-templates/default'),level=1};";
        File.WriteAllText(Path.Combine(dir,"scripts/main.lua"),start+suffix);
        var mod=ModDiscovery.DiscoverLoose(parent).Mods.Single();var content=new ModContentCatalog();
        var templates=new XmlDocument();templates.LoadXml("<Templates><Warrior Name='Default'/></Templates>");
        CoreContentImporter.ImportWarriorTemplates(content,templates.DocumentElement);
        bool accepted=false;
        using(var tx=content.BeginRegistration(mod))
        using(var ctx=new MoonSharpScriptRuntime().CreateContext(mod,new ModApiFacade(mod,new AssetResolver(new IAssetProvider[]{new LooseModProvider(mod)}),tx,new ModStateRuntime(),null)))
        {
            try{ctx.ExecuteEntrypoint();tx.Commit();accepted=true;if(verify!=null)verify((IModInteractiveBehaviorScriptContext)ctx,content.Actors.Single());}
            catch(Exception){if(valid)throw;}
        }
        Check(accepted==valid,"Unexpected registration result: "+suffix);
        if(!valid)Check(content.Actors.Count==0&&content.Warriors.Count==0,"Failed actor registration partially committed.");
        return (content,ModSaveData.ComputeContentSetFingerprint(new[]{mod},content));
    }
    static void Main(string[] args)
    {
        fixture=args[0];
        const string definition="sf2.actors.register{id='ally',character=warrior";
        var baseline=Load(definition+"}");
        var actor=baseline.Content.Actors.Single();
        Check(actor.Id.ToString()=="fixture.actors:actors/ally"&&!actor.OpposingTeam&&actor.AiControlled&&actor.LifetimeFrames==1800&&actor.MaxHealth==1,"Actor defaults/identity invalid.");
        Check(baseline.Hash==Load(definition+"}").Hash,"Actor fingerprint depends on loose path or registration object identity.");
        var empty=Load("");Check(empty.Hash!=baseline.Hash,"Actor definitions omitted from fingerprint.");
        foreach(string field in new[]{"team='opponent'","ai=false","lifetime_frames=60","max_health=2"})
            Check(baseline.Hash!=Load(definition+","+field+"}").Hash,"Actor fingerprint omitted "+field);
        foreach(string fields in new[]{"lifetime_frames=1,max_health=0.01","lifetime_frames=36000,max_health=100"})
            Check(Load(definition+","+fields+"}").Content.Actors.Count==1,"Actor boundary rejected.");
        foreach(string field in new[]{"team='ally'","ai=1","lifetime_frames=0","lifetime_frames=36001","lifetime_frames=1.5","max_health=0","max_health=100.01","max_health=0/0","max_health=1/0","max_health='1'","unknown=true"})
            Load(definition+","+field+"}",false);
        foreach(string call in new[]{"sf2.actors.register{}","sf2.actors.register{id='ally',character={}}","sf2.actors.register{id='ally',character=warrior},{}","sf2.actors.register{id='ally'}"})
            Load(call,false);
        Load(definition+"};"+definition+"}",false);
        const string host="local host=sf2.behaviors.register{id='host',parameters={power={type='number',default=0.1}},state={lifetime='round',fields={hits={type='integer',default=0}}},on_actor_spawn=function()end,on_actor_end=function()end};";
        var attached=Load(host+definition+",behavior=host}");
        var configured=attached.Content.Actors.Single();
        Check(configured.Behavior.Value.ToString()=="fixture.actors:behaviors/host"&&configured.InitialParameters["power"].Number==.1,"Actor behavior defaults/reference were not resolved");
        Check(attached.Hash==Load(host+definition+",behavior=host}").Hash,"Actor behavior fingerprint is unstable");
        Check(attached.Hash!=Load(host+definition+",behavior=host,parameters={power=0.2}}").Hash,"Actor parameters omitted from fingerprint");
        Check(attached.Hash!=Load(host+definition+"}").Hash,"Actor attachment omitted from fingerprint");
        foreach(string field in new[]{"behavior={}","parameters={}","behavior=host,parameters={unknown=1}","behavior=host,parameters={power='wrong'}"})
            Load(host+definition+","+field+"}",false);
        Load(host.Replace("lifetime='round'","lifetime='saved'")+definition+",behavior=host}",false);
        HostChecks(definition);
        FormChecks(definition);
        Console.WriteLine("PASS: "+checks+" production Lua actor definition/default/bound/strict-field/handle/duplicate/rollback/fingerprint checks. Native lifetime/combat is verified separately in Unity.");
    }
    class Actor : IModActor
    {
        public string Id;
        public bool TrySnapshot(out ModActorSnapshot snapshot,out string error)
        {
            error=null;snapshot=new ModActorSnapshot(Id,"fixture.actors:actors/ally","player","a2",
                new ModFighterSnapshot(1,1,1,0,0,0,actor:new ModActorIdentity(Id,"fixture.actors:actors/ally","fixture.actors","player")),0,1800);return true;
        }
        public bool TryMoveBy(double x,double y,double z,out string error){error=null;return true;}
        public bool TryChangeHealth(double amount,out string error){error=null;return true;}
        public bool TryPlayMove(DefinitionId move,Action<bool,string> complete,out string error){error=null;return true;}
        public bool TrySetTarget(string main,IModActor other,out string error){error=null;return true;}
        public bool TryRemove(out string error){error=null;return true;}
    }
    sealed class Body : IModFighterOperations,IModActorBehaviorSource,IModCombatSnapshotSource
    {
        public IModActor Actor {get;set;}
        public ModCombatSnapshot CaptureCombatSnapshot()
        {Actor.TrySnapshot(out var own,out _);return new ModCombatSnapshot(own.Fighter,null,240,true);}
        public bool TryChangeHealth(double amount,out string error){error=null;return true;}
        public bool TryAddMagicCharge(double amount,out string error){error=null;return true;}
    }
    static void HostChecks(string definition)
    {
        const string behavior=@"
local retained
local host=sf2.behaviors.register{id='host',state={lifetime='round',fields={id={type='string',default=''},calls={type='integer',default=0}}},
on_actor_spawn=function(self,fighter)
 assert(self.state.id=='' and self.state.calls==0)
 retained=fighter.actor
 self.state.id=fighter.actor_id
 local view=assert(fighter.actor:snapshot());assert(view.id==fighter.actor_id)
 view.actor.id='forged';assert(fighter.actor:snapshot().actor.id==fighter.actor_id)
end,
on_tick=function(self,fighter)
 assert(self.state.id==fighter.actor_id)
 self.state.calls=self.state.calls+1
 assert(self.state.calls==tonumber(fighter.expected))
 if fighter.fail=='true' then error('controlled callback failure') end
end,
on_damage_dealt=function() retained:snapshot() end,
on_damage_received=function() retained:remove() end,
on_actor_end=function(self,fighter)
 assert(self.state.id==fighter.actor_id and self.state.calls==tonumber(fighter.expected))
 assert(fighter.actor_end_reason=='removed')
end};";
        Load(behavior+definition+",behavior=host}",caps:"content.register\",\"combat.actors",verify:(context,actor)=>
        {
            ModInstanceFighter Instance(string id)
            {
                var document=new XmlDocument();document.LoadXml("<ActorBehaviorInstance/>");
                return new ModInstanceFighter(new Body{Actor=new Actor{Id=id}},document.DocumentElement);
            }
            var first=Instance("a1");var other=Instance("a3");var replacement=Instance("a4");
            void Invoke(ModInstanceFighter fighter,string id,ModEffectEvent kind,int expected=0,bool fail=false)
            {
                var contextValues=new System.Collections.Generic.Dictionary<string,string>{["source"]="actor",["fight_id"]="f1",["round"]="1",["actor_id"]=id,["actor_end_reason"]="removed",["expected"]=expected.ToString(),["fail"]=fail?"true":"false"};
                Check(context.TryInvokeBehavior(actor.Behavior.Value,kind,actor.InitialParameters,contextValues,fighter,out var error)==!fail,"Actor behavior state/scope check failed: "+error);
            }
            Invoke(first,"a1",ModEffectEvent.ActorSpawn);Invoke(other,"a3",ModEffectEvent.ActorSpawn);
            Invoke(first,"a1",ModEffectEvent.DamageDealt,fail:true);Invoke(other,"a3",ModEffectEvent.DamageReceived,fail:true);
            Invoke(first,"a1",ModEffectEvent.Tick,1);Invoke(other,"a3",ModEffectEvent.Tick,1);
            Invoke(first,"a1",ModEffectEvent.Tick,2,true);Invoke(first,"a1",ModEffectEvent.Tick,2);
            Invoke(first,"a1",ModEffectEvent.ActorEnd,2);Invoke(replacement,"a4",ModEffectEvent.ActorSpawn);
            Invoke(replacement,"a4",ModEffectEvent.Tick,1);Invoke(replacement,"a4",ModEffectEvent.ActorEnd,1);
        });
    }
    sealed class FormActor : Actor,IModActorForms
    {
        public int Calls;public bool Reject;public DefinitionId Character;public Action<bool,string> Complete;
        public bool TryChangeForm(DefinitionId character,Action<bool,string> complete,out string error)
        {Calls++;Character=character;error=Reject?"controlled rejection":null;Complete=complete;return !Reject;}
    }
    static void FormChecks(string definition)
    {
        const string capabilities="content.register\",\"combat.actors\",\"combat.transform";
        const string callback=@"
local saved,request
local host=sf2.behaviors.register{id='host',on_tick=function(_,fighter)
 if fighter.action=='poll' then assert(request.status==fighter.expected)
 elseif fighter.action=='escape' then saved:change_form(warrior)
 else saved=fighter.actor;request=saved:change_form(warrior);assert(request.status==fighter.expected) end
end,on_actor_end=function(_,fighter) fighter.actor:change_form(warrior) end};";
        void Run(IModInteractiveBehaviorScriptContext context,ActorDefinition actor,IModActor backend,Action<Action<string,string,ModEffectEvent,bool>> test)
        {
            var xml=new XmlDocument();xml.LoadXml("<Instance/>");var instance=new ModInstanceFighter(new Body{Actor=backend},xml.DocumentElement);
            test((action,expected,kind,valid)=>
            {
                bool ok=context.TryInvokeBehavior(actor.Behavior.Value,kind,actor.InitialParameters,
                    new System.Collections.Generic.Dictionary<string,string>{{"source","actor"},{"fight_id","f"},{"round","1"},{"actor_id","a1"},{"action",action},{"expected",expected}},instance,out var error);
                Check(ok==valid,"Actor form Lua scope/receipt failed: "+error);
            });
        }
        Load(callback+definition+",behavior=host}",caps:capabilities,verify:(context,actor)=>
        {
            var backend=new FormActor{Id="a1"};
            Run(context,actor,backend,invoke=>
            {
                invoke("queue","queued",ModEffectEvent.Tick,true);Check(backend.Calls==1&&backend.Character.ToString()=="fixture.actors:warriors/unit","Typed actor form did not reach backend");
                backend.Complete(true,null);invoke("poll","applied",ModEffectEvent.Tick,true);
                invoke("escape","",ModEffectEvent.Tick,false);Check(backend.Calls==1,"Expired actor form reached backend");
                backend.Reject=true;invoke("queue","failed",ModEffectEvent.Tick,true);
                backend.Reject=false;invoke("queue","queued",ModEffectEvent.Tick,true);backend.Complete(false,"late failure");invoke("poll","failed",ModEffectEvent.Tick,true);
                invoke("queue","",ModEffectEvent.ActorEnd,false);
            });
        });
        Load(callback+definition+",behavior=host}",caps:capabilities,verify:(context,actor)=>
            Run(context,actor,new Actor{Id="a1"},invoke=>invoke("queue","failed",ModEffectEvent.Tick,true)));
        foreach(string cap in new[]{"content.register\",\"combat.actors","content.register\",\"combat.transform"})
            Load(callback+definition+",behavior=host}",caps:cap,verify:(context,actor)=>
            {
                var backend=new FormActor{Id="a1"};Run(context,actor,backend,invoke=>invoke("queue","",ModEffectEvent.Tick,false));Check(backend.Calls==0,"Missing capability reached actor form backend");
            });
        foreach(string arguments in new[]{"{}","1","'warrior'","nil","warrior,warrior"})
            Load(callback.Replace("request=saved:change_form(warrior)","request=saved:change_form("+arguments+")")+definition+",behavior=host}",caps:capabilities,verify:(context,actor)=>
            {
                var backend=new FormActor{Id="a1"};Run(context,actor,backend,invoke=>invoke("queue","",ModEffectEvent.Tick,false));Check(backend.Calls==0,"Malformed form arguments reached backend");
            });
    }

}
