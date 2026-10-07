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
        public ModContentCatalog Content; public IModScriptContext Context; public ModDescriptor Mod; public DefinitionId Behavior;
        public void Dispose()=>Context.Dispose();
    }
    static Loaded Load(string body,string caps="content.register,combat.projectiles",string id="fixture.projectiles",string prefix="")
    {
        string parent=Path.Combine(fixture,Guid.NewGuid().ToString("N")),dir=Path.Combine(parent,id);
        Directory.CreateDirectory(Path.Combine(dir,"scripts"));Directory.CreateDirectory(Path.Combine(dir,"assets/animations"));File.WriteAllBytes(Path.Combine(dir,"assets/animations/flight.bytes"),new byte[]{1,2,3});
        File.WriteAllText(Path.Combine(dir,"mod.toml"),"schema=1\nid=\""+id+"\"\nname=\"Projectiles\"\nversion=\"1.0.0\"\nauthors=[\"Fixture\"]\nentrypoint=\"scripts/main.lua\"\ncapabilities=["+string.Join(",",caps.Split(',').Select(c=>"\""+c+"\""))+"]\n");
        File.WriteAllText(Path.Combine(dir,"scripts/main.lua"),"local sf2=require('sf2');local saved,request;local calls=0;"+prefix+";local function callback(_,fighter,event) calls=calls+1; "+body+" end;sf2.behaviors.register{id='test',on_tick=callback,on_round_begin=callback,on_round_end=callback,on_fight_begin=callback,on_fight_end=callback,on_damage_dealt=callback,on_damage_received=callback,on_damage_dealing=callback,on_damage_resolving=callback,on_block=callback,on_critical=callback,on_hit_post_crit=callback,on_post_hit=callback,on_actor_spawn=callback,on_actor_end=callback}");
        var mod=ModDiscovery.DiscoverLoose(parent).Mods.Single();var content=new ModContentCatalog();
        using var tx=content.BeginRegistration(mod);
        var ctx=new MoonSharpScriptRuntime().CreateContext(mod,new ModApiFacade(mod,new AssetResolver(new IAssetProvider[]{new LooseModProvider(mod)}),tx,new ModStateRuntime(),null));ctx.ExecuteEntrypoint();tx.Commit();
        return new Loaded{Content=content,Context=ctx,Mod=mod,Behavior=DefinitionId.Parse(id+":behaviors/test")};
    }
    static Fight Native(params Loaded[] loaded){var fight=new Fight();if(loaded.Length>0)ModRuntime.Scripts.Content=loaded[0].Content;ModRuntime.Scripts.ActiveMods.AddRange(loaded.Select(x=>x.Mod));return fight;}
    static bool Invoke(Loaded loaded,Fight fight,out string error,ModEffectEvent kind=ModEffectEvent.Tick,IModFighterOperations operations=null){var doc=new XmlDocument();doc.LoadXml("<BehaviorInstance/>");return ((IModInteractiveBehaviorScriptContext)loaded.Context).TryInvokeBehavior(loaded.Behavior,kind,null,new Dictionary<string,string>{{"side","player"}},new ModInstanceFighter(operations??fight.Operations(),doc.DocumentElement),out error);}
    static void Event(Loaded l,Fight f){Check(Invoke(l,f,out var error),error);}
    const string Flight = "local move=sf2.moves.register{id='flight',animation='animations/flight'};";
    const string Prefab = "local prefab=sf2.projectiles.register{id='dart',name='fixture.dart',core_skeleton='SkeletonMissile',copy_parent_type='Ranged',start_move=move}";
    static void SpawnTests()
    {
        string prefix=Flight+Prefab;
        using(var l=Load("if calls==1 then request=fighter:spawn_projectile(prefab,12,-30,nil);assert(request.status=='queued' and not request.projectile_id);assert(#(fighter:projectiles())==0) else assert(request.status=='applied' and request.projectile_id=='1' and not request.error);local p=fighter:projectiles()[1];assert(p:snapshot().position.x==112 and p:snapshot().position.y==-25 and p:snapshot().age_frames==0) end",prefix:prefix))
        {
            var f=Native(l);Event(l,f);Check(f.pendingModels.Count==0,"Spawn ran during Lua");f.Player.X=100;f.Player.Y=5;
            f.Materialize();Check(f.pendingModels.Count==1,"Queued factory did not run");
            Check(f.Query(f.Player,l.Mod.Id,out var list,out _)&&list.Count==0,"Uninitialized child exposed");
            f.InitializeBirths();Event(l,f);Check(f.pendingModels.Single().X==112,"Offset used stale request-time position");
            f.ProjectileStep();Check(f.pendingModels.Count==1,"Spawn repeated");
        }
        foreach(string call in new[]{"","{} ,0,0","{id=prefab.id},0,0","move,0,0","prefab,0","prefab,'x',0","prefab,0,0,'z'","prefab,1001,0","prefab,0/0,0","prefab,1/0,0","prefab,0,0,0,0"})
        {using var l=Load("fighter:spawn_projectile("+call+")",prefix:prefix);var f=Native(l);Check(!Invoke(l,f,out _),"Invalid spawn accepted: "+call);f.ProjectileStep();Check(f.pendingModels.Count==0,"Invalid spawn reached native factory");}
        using(var l=Load("if calls==1 then saved=fighter else saved:spawn_projectile(prefab,0,0) end",prefix:prefix))
        {var f=Native(l);Event(l,f);Check(!Invoke(l,f,out var e)&&e.Contains("expired"),"Retained spawn closure did not expire");}
        using(var l=Load("fighter.opponent:spawn_projectile(prefab,0,0)",prefix:prefix))
        {var f=Native(l);Check(!Invoke(l,f,out _),"Opponent got a spawn method");}
        foreach(var kind in new[]{ModEffectEvent.FightBegin,ModEffectEvent.RoundBegin,ModEffectEvent.RoundEnd,ModEffectEvent.FightEnd})
        {using var l=Load("fighter:spawn_projectile(prefab,0,0)",prefix:prefix);Check(!Invoke(l,Native(l),out var e,kind)&&e.Contains("simulation"),"Lifecycle spawn accepted");}
        using(var l=Load("fighter:spawn_projectile(prefab,0,0)","content.register",prefix:prefix))
        {Check(!Invoke(l,Native(l),out var e)&&e.Contains("capability"),"Unprivileged spawn accepted");}
        using(var l=Load("if calls==1 then for i=1,16 do assert(fighter:spawn_projectile(prefab,0,0).status=='queued') end;assert(fighter:spawn_projectile(prefab,0,0).status=='failed') else assert(#(fighter:projectiles())==16) end",prefix:prefix))
        {var f=Native(l);Event(l,f);Check(f.Spawn(f.Player,l.Mod.Id.Value)==null,"Native timeline ignored queued capacity reservations");f.ProjectileStep();Check(f.pendingModels.Count==16,"Reserved factory batch rejected its own capacity");Event(l,f);}
        using(var l=Load("if calls==1 then request=fighter:spawn_projectile(prefab,0,0) else assert(request.status=='applied') end",prefix:prefix))
        {var f=Native(l);Event(l,f);f.Materialize();f.pendingModels.Single().DriftPasses=3;f.InitializeBirths();Event(l,f);Check(f.pendingModels.Single().Y==0,"Birth constraint residual not corrected");}
        using(var l=Load("if calls==1 then request=fighter:spawn_projectile(prefab,0,0) else assert(request.status=='failed' and request.error:find('constraints') and not request.projectile_id) end",prefix:prefix))
        {var f=Native(l);Event(l,f);f.Materialize();var child=f.pendingModels.Single();child.ClampPosition=true;f.InitializeBirths();Check(child.Translations==5,"Birth correction exceeded four-pass bound");f.ProjectileStep();Event(l,f);Check(f.pendingModels.Count==0,"Constraint-rejected child retained");}
        using(var l=Load("",prefix:prefix))
        {
            var peers=Enumerable.Range(0,4).Select(i=>Load("",id:"fixture.reserve"+i)).ToArray();
            try{
                var f=Native(new[]{l}.Concat(peers).ToArray());
                for(int i=0;i<15;i++)Check(f.Spawn(f.Player,l.Mod.Id.Value)!=null,"Early live reservation rejection");
                for(int i=0;i<3;i++)for(int j=0;j<16;j++)Check(f.Spawn(f.Player,peers[i].Mod.Id.Value)!=null,"Early global reservation rejection");
                var def=l.Content.Projectiles.Single().Id;
                Check(f.QueueSpawn(f.Enemy,l.Mod.Id,def,0,0,0,(_,__)=>{},out _),"Final global reservation failed");
                Check(f.Spawn(f.Player,peers[3].Mod.Id.Value)==null,"Global native spawn ignored queued reservation");
                Check(!f.QueueSpawn(f.Player,l.Mod.Id,def,0,0,0,(_,__)=>{},out _),"Per-mod capacity omitted opposite-root reservation");
                f.ProjectileStep();Check(f.pendingModels.Count==64,"Final reserved slot did not materialize");
                f.CancelProjectiles();f.ProjectileStep();Check(f.pendingModels.Count==0,"Global reservation cleanup failed");
            }finally{foreach(var peer in peers)peer.Dispose();}
        }
        foreach(string state in new[]{"pause","round","session","owner","death","cancel","factory","after-create","birth","position"})
        using(var l=Load("if calls==1 then request=fighter:spawn_projectile(prefab,0,0) else assert(request.status=='failed' and request.error and not request.projectile_id) end",prefix:prefix))
        {
            var f=Native(l);Event(l,f);
            switch(state){case "pause":f.Paused=true;f.ProjectileStep();Check(f.pendingModels.Count==0,"Pause materialized spawn");f.Paused=false;f.CancelProjectiles();break;case "round":f.round.round++;break;case "session":ModRuntime.Scripts=new ModScriptSession();break;case "owner":ModRuntime.Scripts.ActiveMods.Clear();break;case "death":f.Player.Health=0;break;case "cancel":f.CancelProjectiles();break;case "factory":ModRuntime.FailSpawn=true;break;case "after-create":ModRuntime.FailAfterCreate=true;break;case "birth":ModRuntime.FailBirth=true;break;case "position":f.Materialize();f.pendingModels.Single().Throw=true;break;}
            f.ProjectileStep();Event(l,f);Check(f.pendingModels.Count==0,"Failed spawn left child: "+state);
        }
        using(var l=Load("if calls==1 then request=fighter:spawn_projectile(prefab,0,0) else assert(request.status=='failed') end",prefix:prefix))
        {var f=Native(l);Event(l,f);f.Materialize();f.CancelProjectiles();f.InitializeBirths();f.ProjectileStep();Event(l,f);Check(f.pendingModels.Count==0,"Cancelled birth survived");}
        foreach(string spec in new[]{"id='dart',name='fixture.dart',core_skeleton='SkeletonMissile',copy_parent_type='Ranged'","id='dart',name='fixture.dart',core_skeleton='SkeletonMissile',copy_parent_type='Ranged',start_move=move,core_start_animation='Fly'","id='dart',name='fixture.dart',core_skeleton='SkeletonMissile',copy_parent_type='Ranged',start_move=move,lifetime_frames=601"})
        {bool failed=false;try{using var l=Load("",prefix:Flight+"sf2.projectiles.register{"+spec+"}");}catch(Exception){failed=true;}Check(failed,"Invalid registered prefab accepted");}
        using(var l=Load("",prefix:prefix))
        {
            var def=l.Content.Projectiles.Single();Check(def.Id.ToString()=="fixture.projectiles:projectiles/dart","Definition namespace");
            var hash=ModSaveData.ComputeContentSetFingerprint(new[]{l.Mod},l.Content);
            using var changed=Load("",prefix:prefix.Replace("start_move=move}","start_move=move,lifetime_frames=21}"));
            Check(hash!=ModSaveData.ComputeContentSetFingerprint(new[]{changed.Mod},changed.Content),"Prefab TTL missing from content fingerprint");
            using var tx=l.Content.BeginRegistration(l.Mod);
            tx.RegisterProjectile("missing",new ModMoveProjectile("fixture.missing","SkeletonMissile","Weapon",startMove:DefinitionId.Parse("fixture.projectiles:moves/missing")));
            bool failed=false;try{tx.Commit();}catch(ModContentException){failed=true;}Check(failed&&l.Content.Projectiles.Count==1,"Invalid reference committed partially");
        }
    }

    static void ActorRoots()
    {
        string prefix=Flight+Prefab;
        using(var l=Load("if calls==1 then request=fighter:spawn_projectile(prefab,12,-30);assert(request.status=='queued') else assert(request.status=='applied');local list=assert(fighter:projectiles());assert(#list==1 and list[1]:snapshot().position.x==112);assert(list[1]:move_by(8,0));saved=list[1] end",prefix:prefix))
        {
            var f=Native(l);var body=f.AddActor(l.Mod.Id);body.X=100;
            Check(Invoke(l,f,out var error,ModEffectEvent.ActorSpawn,f.Operations(body)),error);
            f.ProjectileStep();Check(f.pendingModels.Single().GetRootModel()==body,"Child rooted in main instead of actor");
            Check(f.Query(f.Player,l.Mod.Id,out var main,out _)&&main.Count==0,"Main query leaked actor children");
            var peer=f.AddActor(l.Mod.Id);Check(f.Query(peer,l.Mod.Id,out var empty,out _)&&empty.Count==0,"Sibling query leaked actor children");
            Check(Invoke(l,f,out error,operations:f.Operations(body)),error);f.ProjectileStep();Check(f.pendingModels.Single().X==120,"Actor child motion did not apply");
            var source=f.Attack(f.pendingModels.Single(),new Model.StrikeResult{Point=new Vector3f(0,0,0),AttackAnimation=new InfoAnimation{Name="flight"}});
            Check(source.Kind=="projectile"&&source.ActorId=="a1"&&source.ProjectileId=="1"&&source.ActorOwner==l.Mod.Id.Value,"Actor/projectile provenance missing");
            Check(!Invoke(l,f,out error,ModEffectEvent.ActorEnd,f.Operations(body))&&error.Contains("simulation"),"Terminal actor callback acquired projectile authority");
            f.RetireActor(body);f.ProjectileStep();Check(f.pendingModels.Count==0,"Actor retirement retained initialized child");
        }
        foreach(string state in new[]{"unborn","birth","unavailable","owner-death","actor-death","pause","round","retire-before-create","retire-before-init"})
        using(var l=Load("request=fighter:spawn_projectile(prefab,0,0)",prefix:prefix))
        {
            var f=Native(l);var body=f.AddActor(l.Mod.Id,state!="unborn",state=="birth");
            string receiptId=null,receiptError=null;bool completed=false;
            if(state=="unavailable")f.MakeActorUnavailable(body);
            if(state=="owner-death")f.Player.Health=0;
            if(state=="actor-death")body.Health=0;
            if(state=="pause")f.Paused=true;
            bool queued=f.QueueSpawn(body,l.Mod.Id,l.Content.Projectiles.Single().Id,0,0,0,(id,error)=>{receiptId=id;receiptError=error;completed=true;},out _);
            bool shouldQueue=state=="round"||state.StartsWith("retire-");Check(queued==shouldQueue,"Actor eligibility mismatch: "+state);
            if(!queued){Check(f.pendingModels.Count==0,"Rejected actor reached factory");continue;}
            if(state=="round")f.round.round++;
            if(state=="retire-before-init")f.Materialize();
            if(state.StartsWith("retire-"))f.RetireActor(body);
            f.ProjectileStep();Check(completed&&receiptId==null&&receiptError!=null&&f.pendingModels.Count==0,"Actor cancellation did not fail receipt/clean child: "+state);
        }
        using(var l=Load("fighter:projectiles()", "content.register",prefix:prefix))
        {var f=Native(l);var body=f.AddActor(l.Mod.Id);Check(!Invoke(l,f,out var error,operations:f.Operations(body))&&error.Contains("capability"),"Actor bypassed combat.projectiles");}
    }

    static void ShippedBurst(string repo)
    {
        var mod=ModDiscovery.DiscoverLoose(Path.Combine(repo,"Mods")).Mods.Single(m=>m.Id.Value=="example.scripted-burst");
        var content=new ModContentCatalog();var stages=new XmlDocument();stages.Load(Path.Combine(repo,"Assets/vanillaXml/stages.xml"));
        CoreContentImporter.ImportStages(content,stages.SelectSingleNode("Stages/Zones"));
        var items=new XmlDocument();items.Load(Path.Combine(repo,"Assets/vanillaXml/list.xml"));
        CoreContentImporter.ImportRanged(content,items.SelectNodes("//Item[@Name='RANGED_C2_Z2_MONK_SHURIKEN']").Cast<XmlNode>(),new Dictionary<string,XmlDocument>());
        var surfaces=new List<ModUiSurface>();using var tx=content.BeginRegistration(mod);
        var api=new ModApiFacade(mod,new AssetResolver(new IAssetProvider[]{new LooseModProvider(mod)}),tx,new ModStateRuntime(),null);
        using var context=new MoonSharpScriptRuntime(surfaces.Add).CreateContext(mod,api);context.ExecuteEntrypoint();tx.Commit();
        Check(content.Moves.Count==1&&content.Projectiles.Count==1,"Shipped burst registration lost its flight/prefab");
        var loaded=new Loaded{Mod=mod,Content=content};var f=Native(loaded);f.Enemy.X=500;
        var doc=new XmlDocument();doc.LoadXml("<BehaviorInstance/>");var rule=content.FightRules.Single();
        var ops=(Fight.EclipseFighterOperations)f.Operations();var fighter=new ModInstanceFighter(ops,doc.DocumentElement,rule);
        void Event(ModEffectEvent kind){Check(((IModInteractiveBehaviorScriptContext)context).TryInvokeBehavior(rule.Behavior,kind,null,
            new Dictionary<string,string>{{"side","player"},{"round",f.round.round.ToString()},{"source","rule"}},fighter,out var error),error);}
        Event(ModEffectEvent.RoundBegin);var hud=surfaces.Single();Check(hud.Read("fire").Enabled,"Shipped burst HUD disabled initially");
        Check(hud.TryClick("fire"),"Shipped burst click rejected");Event(ModEffectEvent.Tick);
        Check(f.pendingModels.Count==0&&!hud.Read("fire").Enabled&&f.Player.Plays==0,"Shipped burst created recursively or played a caster move");
        f.ProjectileStep();Check(f.pendingModels.Count==3,"Shipped burst did not materialize three children");
        var first=f.pendingModels[0];double x=first.X;f.Clock++;Event(ModEffectEvent.Tick);f.ProjectileStep();
        Check(first.X==x+10,"Shipped burst did not match applied receipt ID to live child motion");
        ops.DamageEvent=new ModDamageEvent(1,1,.9,false,false,f.Attack(first,new Model.StrikeResult{Point=new Vector3f(0,0,0),AttackAnimation=new InfoAnimation{Name=content.Moves.Single().RuntimeName}}));
        Event(ModEffectEvent.DamageDealt);Check(hud.Read("status").Text=="Native hits: 1","Shipped burst source filter/count");ops.DamageEvent=null;
        f.CancelProjectiles();f.ProjectileStep();
        for(int i=0;i<180;i++){f.Clock++;Event(ModEffectEvent.Tick);}Check(hud.Read("fire").Enabled,"Shipped burst cooldown did not recover");
        for(int i=0;i<15;i++)Check(f.Spawn(f.Player,mod.Id.Value)!=null,"Partial burst setup failed");
        Check(hud.TryClick("fire"),"Partial burst button rejected");Event(ModEffectEvent.Tick);f.ProjectileStep();f.Clock++;Event(ModEffectEvent.Tick);
        Check(f.pendingModels.Count==16&&!hud.Read("fire").Enabled,"Partial burst did not retain accepted child and cooldown");
        Event(ModEffectEvent.RoundEnd);Check(hud.IsClosed,"Shipped burst HUD leaked on round end");f.CancelProjectiles();f.ProjectileStep();f.round.round++;
        Event(ModEffectEvent.RoundBegin);Check(surfaces.Last().Read("fire").Enabled,"Shipped burst next-round state did not reset");Event(ModEffectEvent.FightEnd);
        Check(surfaces.Last().IsClosed,"Shipped burst HUD leaked on fight end");
    }

    static void Main(string[] args)
    {
        fixture=args[0];
        SpawnTests();
        ActorRoots();
        ShippedBurst(args[1]);
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
        {var f=Native(l,peer);f.Spawn(f.Player,l.Mod.Id.Value);f.Spawn(f.Enemy,l.Mod.Id.Value);f.Spawn(f.Player,peer.Mod.Id.Value);f.pendingModels.Add(new Model{Parent=f.Player});Event(l,f);Event(peer,f);}
        foreach(string call in new[]{"p:move_by()","p:move_by('1',0)","p:move_by(1/0,0)","p:move_by(0/0,0)","p:move_by(101,0)","p:move_by(1,0,0,0)","p:snapshot({})","p:remove({})","p.snapshot({})","p.move_by({},0)","fighter:projectiles(1)"})
        {using var l=Load("local p=fighter:projectiles()[1];"+call);var f=Native(l);var child=f.Spawn(f.Player,l.Mod.Id.Value);Check(!Invoke(l,f,out _),"Invalid arguments accepted: "+call);f.ProjectileStep();Check(child.Translations==0,"Invalid call moved child");}
        using(var l=Load("local p=fighter:projectiles()[1];assert(p:move_by(60,0));local ok,e=p:move_by(41,0);assert(not ok and e);for i=1,31 do assert(p:move_by(1,0)) end;assert(not p:move_by(1,0));assert(p:move_by(0,0))"))
        {var f=Native(l);var child=f.Spawn(f.Player,l.Mod.Id.Value);Event(l,f);f.ProjectileStep();Check(child.X==91,"Aggregate/request bound did not preserve accepted commands");}
        using(var l=Load("for i=1,32 do assert(fighter:projectiles()) end;fighter:projectiles()"))
        {var f=Native(l);Check(!Invoke(l,f,out var e)&&e.Contains("32"),"Query budget missing");}
        foreach(string action in new[]{"saved:snapshot()","saved:move_by(1,0)","saved:remove()"})
        {using var l=Load("if calls==1 then saved=fighter:projectiles()[1] else "+action+" end");var f=Native(l);f.Spawn(f.Player,l.Mod.Id.Value);Event(l,f);Check(!Invoke(l,f,out var e)&&e.Contains("expired"),"Retained reference stayed live");}
        using(var l=Load("local p=fighter:projectiles()[1];assert(p:move_by(10,0));assert(p:remove());assert(not p:remove());assert(not p:move_by(1,0));assert(p:snapshot()==nil);assert(#(fighter:projectiles())==0)"))
        {var f=Native(l);var child=f.Spawn(f.Player,l.Mod.Id.Value);Event(l,f);Check(f.pendingModels.Contains(child),"Delete synchronous");f.ProjectileStep();Check(child.X==0&&!f.pendingModels.Contains(child),"Removal failed/cancelled motion survived");}
        foreach(var kind in new[]{ModEffectEvent.FightBegin,ModEffectEvent.RoundBegin,ModEffectEvent.RoundEnd,ModEffectEvent.FightEnd})
        {using var l=Load("fighter:projectiles()");var f=Native(l);Check(!Invoke(l,f,out var e,kind)&&e.Contains("simulation"),"Forbidden lifecycle accepted");}
        using(var l=Load("fighter:projectiles()","content.register")){Check(!Invoke(l,Native(l),out var e)&&e.Contains("capability"),"Missing capability accepted");}
        foreach(string state in new[]{"pause","round","session","disposed","owner","root","death","cancel","native-delete"})
        {using var l=Load("local p=fighter:projectiles()[1];assert(p:move_by(10,0))");var f=Native(l);var child=f.Spawn(f.Player,l.Mod.Id.Value);Event(l,f);
         switch(state){case "pause":f.Paused=true;f.ProjectileStep();Check(child.X==0,"Pause consumed command");f.Paused=false;break;case "round":f.round.round++;break;case "session":ModRuntime.Scripts=new ModScriptSession();break;case "disposed":ModRuntime.Scripts.IsDisposed=true;break;case "owner":ModRuntime.Scripts.ActiveMods.Clear();break;case "root":child.Parent=f.Enemy;break;case "death":f.Player.Health=0;break;case "cancel":f.CancelProjectiles();break;case "native-delete":f.modelsToRemove.Add(child);break;}
         f.ProjectileStep();Check(child.X==(state=="pause"?10:0),"Stale command applied: "+state);Check(state=="pause"||!f.pendingModels.Contains(child),"Retired child survived: "+state);}
        using(var l=Load("assert(#(fighter:projectiles())==0)"))
        {var f=Native(l);var child=f.Spawn(f.Player,l.Mod.Id.Value,2);f.Age();Check(f.pendingModels.Contains(child),"Early expiry");f.Age();Event(l,f);f.ProjectileStep();Check(!f.pendingModels.Contains(child),"TTL retained native child");}
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
