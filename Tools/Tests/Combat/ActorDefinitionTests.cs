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
        Console.WriteLine("PASS: "+checks+" production Lua actor definition/default/bound/strict-field/handle/duplicate/rollback/fingerprint checks. Native lifetime/combat is verified separately in Unity.");
    }
    sealed class Actor : IModActor
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
}
