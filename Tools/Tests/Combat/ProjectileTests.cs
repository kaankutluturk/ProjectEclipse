using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Eclipse.Modding;

static class Program
{
    static int checks; static string fixture;
    static void Check(bool value,string message){checks++;if(!value)throw new Exception(message);}
    sealed class Loaded : IDisposable
    {
        public IModScriptContext Context; public ModDescriptor Mod; public DefinitionId Behavior;
        public void Dispose()=>Context.Dispose();
    }
    static Loaded Load(string body,string caps="content.register,combat.projectiles",string id="fixture.projectiles")
    {
        string parent=Path.Combine(fixture,Guid.NewGuid().ToString("N")),dir=Path.Combine(parent,id);
        Directory.CreateDirectory(Path.Combine(dir,"scripts"));
        File.WriteAllText(Path.Combine(dir,"mod.toml"),"schema=1\nid=\""+id+"\"\nname=\"Projectiles\"\nversion=\"1.0.0\"\nauthors=[\"Fixture\"]\nentrypoint=\"scripts/main.lua\"\ncapabilities=["+string.Join(",",caps.Split(',').Select(c=>"\""+c+"\""))+"]\n");
        File.WriteAllText(Path.Combine(dir,"scripts/main.lua"),"local sf2=require('sf2');local saved;local calls=0;local function callback(_,fighter,event) calls=calls+1; "+body+" end;sf2.behaviors.register{id='test',on_tick=callback,on_round_begin=callback,on_round_end=callback,on_fight_begin=callback,on_fight_end=callback,on_damage_dealt=callback,on_damage_received=callback,on_damage_dealing=callback,on_damage_resolving=callback,on_block=callback,on_critical=callback,on_hit_post_crit=callback,on_post_hit=callback}");
        var mod=ModDiscovery.DiscoverLoose(parent).Mods.Single();var content=new ModContentCatalog();
        using var tx=content.BeginRegistration(mod);
        var ctx=new MoonSharpScriptRuntime().CreateContext(mod,new ModApiFacade(mod,new AssetResolver(new IAssetProvider[]{new LooseModProvider(mod)}),tx,new ModStateRuntime(),null));ctx.ExecuteEntrypoint();tx.Commit();
        return new Loaded{Context=ctx,Mod=mod,Behavior=DefinitionId.Parse(id+":behaviors/test")};
    }
    static Fight Native(params Loaded[] loaded){var fight=new Fight();ModRuntime.Scripts.ActiveMods.AddRange(loaded.Select(x=>x.Mod));return fight;}
    static bool Invoke(Loaded loaded,Fight fight,out string error,ModEffectEvent kind=ModEffectEvent.Tick,IModFighterOperations operations=null){var doc=new XmlDocument();doc.LoadXml("<BehaviorInstance/>");return ((IModInteractiveBehaviorScriptContext)loaded.Context).TryInvokeBehavior(loaded.Behavior,kind,null,new Dictionary<string,string>{{"side","player"}},new ModInstanceFighter(operations??fight.Operations(),doc.DocumentElement),out error);}
    static void Event(Loaded l,Fight f){Check(Invoke(l,f,out var error),error);}
    static void Main(string[] args)
    {
        fixture=args[0];
        using(var l=Load("local a=event.attack;assert(a.kind=='projectile' and a.projectile_owner==sf2.mod.id and a.projectile_id=='1');assert(a.model_name=='fixture.dart' and a.animation_name=='contact' and a.point.x==12 and a.point.y==-4 and a.point.z==3);if saved then assert(saved.point.x==999) end;saved=a;a.point.x=999;a.projectile_id='fake'", "content.register"))
        {
            var f=Native(l);var child=f.Spawn(f.Player,l.Mod.Id.Value);
            var strike=new Model.StrikeResult{Point=new Vector3f(12,-4,3),AttackAnimation=new InfoAnimation{Name="contact"}};
            var attack=f.Attack(child,strike);Check(attack.Kind=="projectile"&&attack.ProjectileId=="1","Owned native contact attribution");
            var ops=new Fight.EclipseFighterOperations(f,f.Player){DamageEvent=new ModDamageEvent(1,1,.9,false,true,attack)};
            foreach(var kind in new[]{ModEffectEvent.DamageDealt,ModEffectEvent.DamageReceived,ModEffectEvent.Block,ModEffectEvent.Critical})
                Check(Invoke(l,f,out var error,kind,ops),error);
            ops.DamageEvent=null;
            ops.IncomingHit=new ModIncomingHit(()=>.1,_=>{},false,true,new ModHitEvent(false,false,false,true,false),attack);
            foreach(var kind in new[]{ModEffectEvent.DamageDealing,ModEffectEvent.DamageResolving,ModEffectEvent.HitPostCrit,ModEffectEvent.PostHit})
                Check(Invoke(l,f,out var error,kind,ops),error);
            Check(attack.ProjectileId=="1"&&attack.X==12,"Lua mutation escaped copied attack data");
            f.CancelProjectiles();f.ProjectileStep();
            ops.DamageEvent=new ModDamageEvent(1,1,.9,false,true,attack);ops.IncomingHit=null;
            Check(Invoke(l,f,out var finalError,ModEffectEvent.DamageDealt,ops),finalError);
            Check(f.Attack(child,strike).Kind=="native_child","Retired child still claims owned capability provenance");
            Check(f.Attack(f.Player,strike).Kind=="fighter","Root contact classification");
            Check(f.Attack(child,new Model.StrikeResult{Point=new Vector3f(float.NaN,0,0)})==null,"Invalid contact point leaked");
            Check(f.Attack(child,new Model.StrikeResult())==null,"Missing contact point invented");
        }
        foreach(string kind in new[]{"fighter","native_child"})
        using(var l=Load("assert(event.attack.kind=='"+kind+"' and not event.attack.projectile_id and not event.attack.projectile_owner)","content.register"))
        {
            var f=Native(l);var actor=kind=="fighter"?f.Player:new Model{Parent=f.Player};
            var attack=f.Attack(actor,new Model.StrikeResult{Point=new Vector3f(0,0,0)});
            var ops=new Fight.EclipseFighterOperations(f,f.Player){DamageEvent=new ModDamageEvent(1,1,.9,false,false,attack)};
            Check(Invoke(l,f,out var error,ModEffectEvent.DamageDealt,ops),error);
        }
        using(var l=Load("assert(event.attack==nil)","content.register"))
        {var f=Native(l);var ops=new Fight.EclipseFighterOperations(f,f.Player){DamageEvent=new ModDamageEvent(1,1,.9,false,false)};Check(Invoke(l,f,out var error,ModEffectEvent.DamageDealt,ops),error);}

        using(var l=Load("local list,e=fighter:projectiles();assert(#list==1 and not e);local p=list[1];local v=p:snapshot();assert(v.name=='fixture.dart' and v.age_frames==0 and v.lifetime_frames==180);v.position.x=999;assert(p:snapshot().position.x==0);assert(p:move_by(10,2));assert(p.move_by(3,0,nil));assert(p:snapshot().position.x==0)"))
        {var f=Native(l);var child=f.Spawn(f.Player,l.Mod.Id.Value);Event(l,f);Check(child.Translations==0,"Lua moved synchronously");f.ProjectileStep();Check(child.X==13&&child.Y==2&&child.AnimationShifted,"Queued additive displacement failed");f.ProjectileStep();Check(child.Translations==1,"Command repeated");}
        using(var l=Load("local list=fighter:projectiles();assert(#list==1);assert(#(fighter:projectiles())==1)"))
        using(var peer=Load("assert(#(fighter:projectiles())==1)",id:"fixture.peer"))
        {var f=Native(l,peer);f.Spawn(f.Player,l.Mod.Id.Value);f.Spawn(f.Enemy,l.Mod.Id.Value);f.Spawn(f.Player,peer.Mod.Id.Value);f.HCPGFOCGDAA.Add(new Model{Parent=f.Player});Event(l,f);Event(peer,f);}
        foreach(string call in new[]{"p:move_by()","p:move_by('1',0)","p:move_by(1/0,0)","p:move_by(0/0,0)","p:move_by(101,0)","p:move_by(1,0,0,0)","p:snapshot({})","p:remove({})","p.snapshot({})","p.move_by({},0)","fighter:projectiles(1)"})
        {using var l=Load("local p=fighter:projectiles()[1];"+call);var f=Native(l);var child=f.Spawn(f.Player,l.Mod.Id.Value);Check(!Invoke(l,f,out _),"Invalid arguments accepted: "+call);f.ProjectileStep();Check(child.Translations==0,"Invalid call moved child");}
        using(var l=Load("local p=fighter:projectiles()[1];assert(p:move_by(60,0));local ok,e=p:move_by(41,0);assert(not ok and e);for i=1,31 do assert(p:move_by(1,0)) end;assert(not p:move_by(1,0));assert(p:move_by(0,0))"))
        {var f=Native(l);var child=f.Spawn(f.Player,l.Mod.Id.Value);Event(l,f);f.ProjectileStep();Check(child.X==91,"Aggregate/request bound did not preserve accepted commands");}
        using(var l=Load("for i=1,32 do assert(fighter:projectiles()) end;fighter:projectiles()"))
        {var f=Native(l);Check(!Invoke(l,f,out var e)&&e.Contains("32"),"Query budget missing");}
        foreach(string action in new[]{"saved:snapshot()","saved:move_by(1,0)","saved:remove()"})
        {using var l=Load("if calls==1 then saved=fighter:projectiles()[1] else "+action+" end");var f=Native(l);f.Spawn(f.Player,l.Mod.Id.Value);Event(l,f);Check(!Invoke(l,f,out var e)&&e.Contains("expired"),"Retained reference stayed live");}
        using(var l=Load("local p=fighter:projectiles()[1];assert(p:move_by(10,0));assert(p:remove());assert(not p:remove());assert(not p:move_by(1,0));assert(p:snapshot()==nil);assert(#(fighter:projectiles())==0)"))
        {var f=Native(l);var child=f.Spawn(f.Player,l.Mod.Id.Value);Event(l,f);Check(f.HCPGFOCGDAA.Contains(child),"Delete synchronous");f.ProjectileStep();Check(child.X==0&&!f.HCPGFOCGDAA.Contains(child),"Removal failed/cancelled motion survived");}
        foreach(var kind in new[]{ModEffectEvent.FightBegin,ModEffectEvent.RoundBegin,ModEffectEvent.RoundEnd,ModEffectEvent.FightEnd})
        {using var l=Load("fighter:projectiles()");var f=Native(l);Check(!Invoke(l,f,out var e,kind)&&e.Contains("simulation"),"Forbidden lifecycle accepted");}
        using(var l=Load("fighter:projectiles()","content.register")){Check(!Invoke(l,Native(l),out var e)&&e.Contains("capability"),"Missing capability accepted");}
        foreach(string state in new[]{"pause","round","session","disposed","owner","root","death","cancel","native-delete"})
        {using var l=Load("local p=fighter:projectiles()[1];assert(p:move_by(10,0))");var f=Native(l);var child=f.Spawn(f.Player,l.Mod.Id.Value);Event(l,f);
         switch(state){case "pause":f.Paused=true;f.ProjectileStep();Check(child.X==0,"Pause consumed command");f.Paused=false;break;case "round":f.round.round++;break;case "session":ModRuntime.Scripts=new ModScriptSession();break;case "disposed":ModRuntime.Scripts.IsDisposed=true;break;case "owner":ModRuntime.Scripts.ActiveMods.Clear();break;case "root":child.Parent=f.Enemy;break;case "death":f.Player.Health=0;break;case "cancel":f.CancelProjectiles();break;case "native-delete":f.JLEFIKJODGG.Add(child);break;}
         f.ProjectileStep();Check(child.X==(state=="pause"?10:0),"Stale command applied: "+state);Check(state=="pause"||!f.HCPGFOCGDAA.Contains(child),"Retired child survived: "+state);}
        using(var l=Load("assert(#(fighter:projectiles())==0)"))
        {var f=Native(l);var child=f.Spawn(f.Player,l.Mod.Id.Value,2);f.Age();Check(f.HCPGFOCGDAA.Contains(child),"Early expiry");f.Age();Event(l,f);f.ProjectileStep();Check(!f.HCPGFOCGDAA.Contains(child),"TTL retained native child");}
        using(var l=Load(""))
        {var f=Native(l);for(int i=0;i<16;i++)Check(f.Spawn(i%2==0?f.Player:f.Enemy,l.Mod.Id.Value)!=null,"Capacity rejected early");Check(f.Spawn(f.Player,l.Mod.Id.Value)==null,"Per-mod bound missing");f.CancelProjectiles();f.ProjectileStep();Check(f.Spawn(f.Player,l.Mod.Id.Value)!=null,"Capacity not released");}
        using(var l=Load(""))
        {foreach(var guard in new Action<Fight>[]{f=>f.Paused=true,f=>f.IsLocalVersus=true,f=>f.FightDefinition.Type=BattleType.FightPVP,f=>f.FightDefinition.Type=BattleType.FightRaid,f=>f.isEndRound=true,f=>f.Player.Health=0}){var f=Native(l);guard(f);Check(f.Spawn(f.Player,l.Mod.Id.Value)==null,"Excluded host spawned child");}}
        var owners=Enumerable.Range(0,5).Select(i=>Load("",id:"fixture.owner"+i)).ToArray();
        try {var f=Native(owners);for(int i=0;i<4;i++)for(int j=0;j<16;j++)Check(f.Spawn(f.Player,owners[i].Mod.Id.Value)!=null,"Global capacity rejected early");Check(f.Spawn(f.Player,owners[4].Mod.Id.Value)==null,"Global 64 bound missing");f.CancelProjectiles();f.ProjectileStep();Check(f.Spawn(f.Player,owners[4].Mod.Id.Value)!=null,"Global capacity not released");}
        finally {foreach(var owner in owners)owner.Dispose();}
        Console.WriteLine("PASS: "+checks+" production Lua/projectile command checks; controlled model translation, ownership, arguments, lifetime, pause, deletion, capacity, round/session retirement. No native geometry/contact claim.");
    }
}
