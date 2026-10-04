using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Eclipse.Modding;

static class Program
{
    static int checks; static string fixture,repo;
    static void Check(bool value,string message){checks++;if(!value)throw new Exception(message);}
    sealed class Marker : IModArenaArtwork
    {
        public bool Active=true,RejectUpdate;public int Removed,SpriteChanges,RectChanges; public ModUiColor Color;public ModArenaRect Rect;public AssetId Sprite;
        public bool IsActive=>Active;
        public void SetColor(ModUiColor color){Color=color;}
        public void SetRect(ModArenaRect rect){if(RejectUpdate)throw new Exception("update failed");Rect=rect;RectChanges++;}
        public void SetSprite(AssetId sprite){if(RejectUpdate)throw new Exception("update failed");Sprite=sprite;SpriteChanges++;}
        public void Dispose(){Active=false;Removed++;}
    }
    sealed class Fighter : IModFighterOperations, IModFighterRegions, IModFighterArtwork, IModCombatSnapshotSource, IModFighterTargets
    {
        public readonly List<Marker> Markers=new List<Marker>();public bool Inside=true,Reject,Throws,PartialThrows;public int Queries,Changes;
        public double Health{get;private set;}=1;public Fighter Peer;public IModFighterOperations Opponent=>Peer;
        public ModCombatSnapshot CaptureCombatSnapshot()=>new ModCombatSnapshot(new ModFighterSnapshot(Health,1,1,0,0,0),null,1,true);
        public bool TryChangeHealth(double amount,out string error){Changes++;Health+=amount;error=null;return true;}
        public bool TryAddMagicCharge(double amount,out string error){error=null;return true;}
        public bool TryOverlapRect(ModArenaRect rect,out bool hit,out string error){Queries++;hit=Inside;error=Reject?"unavailable":null;return !Reject;}
        public bool TryMarkSprite(AssetId sprite,ModArenaRect rect,ModUiColor color,out IModArenaMarker result,out string error)
        {if(!TryMarkRect(rect,color,out result,out error))return false;((Marker)result).SetSprite(sprite);return true;}
        public bool TryMarkRect(ModArenaRect rect,ModUiColor color,out IModArenaMarker result,out string error)
        {
            result=null;error=null;if(Throws)throw new Exception("render failed");if(Reject){error="inactive round";return false;}
            var marker=new Marker{Color=color,Rect=rect};Markers.Add(marker);result=marker;if(PartialThrows)throw new Exception("partial allocation failed");return true;
        }
    }
    sealed class Loaded : IDisposable
    {
        public IModScriptContext Context;public ModContentCatalog Content;public DefinitionId Behavior;public Fighter Fighter=new Fighter{Peer=new Fighter()};
        public void Dispose()=>Context.Dispose();
    }
    static Loaded Load(string body,string caps="content.register,presentation.visuals,combat.target",bool freeze=true)
    {
        var parent=Path.Combine(fixture,Guid.NewGuid().ToString("N"));var folder=Path.Combine(parent,"fixture.arena");Directory.CreateDirectory(Path.Combine(folder,"scripts"));
        Directory.CreateDirectory(Path.Combine(folder,"assets/sprites"));Directory.CreateDirectory(Path.Combine(folder,"assets/textures"));Directory.CreateDirectory(Path.Combine(folder,"assets/models"));
        File.WriteAllBytes(Path.Combine(folder,"assets/textures/one.png"),Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9Wl2nVsAAAAASUVORK5CYII="));
        File.Copy(Path.Combine(folder,"assets/textures/one.png"),Path.Combine(folder,"assets/textures/two.png"));
        foreach(var name in new[]{"one","two"})File.WriteAllText(Path.Combine(folder,"assets/sprites/"+name+".asset"),"type=sprite\ntexture=textures/"+name+".png\n");File.WriteAllText(Path.Combine(folder,"assets/models/wrong.xml"),"<Model/>");
        File.WriteAllText(Path.Combine(folder,"mod.toml"),"schema=1\nid=\"fixture.arena\"\nname=\"Arena\"\nversion=\"1.0.0\"\nauthors=[\"Fixture\"]\nentrypoint=\"scripts/main.lua\"\ncapabilities=["+string.Join(",",caps.Split(',').Select(c=>"\""+c+"\""))+"]\n");
        File.WriteAllText(Path.Combine(folder,"scripts/main.lua"),"local sf2=require('sf2');local rect={x=0,y=0,width=20,height=40};local saved,marker;local one=sf2.assets.sprite('sprites/one');local two=sf2.assets.sprite('sprites/two');local wrong=sf2.assets.model('models/wrong');local calls=0;local function run(_,fighter) calls=calls+1;"+body+" end;sf2.behaviors.register{id='test',on_tick=run,on_round_begin=run,on_round_end=run,on_fight_begin=run,on_fight_end=run};");
        var discovery=ModDiscovery.DiscoverLoose(parent);Check(discovery.Diagnostics.Count==0,"Bad manifest");var mod=discovery.Mods.Single();var content=new ModContentCatalog();using var tx=content.BeginRegistration(mod);
        var api=new ModApiFacade(mod,new AssetResolver(new IAssetProvider[]{new LooseModProvider(mod)}),tx,new ModStateRuntime(),null);
        var context=new MoonSharpScriptRuntime(surface=>{}).CreateContext(mod,api);context.ExecuteEntrypoint();tx.Commit();if(freeze)content.Freeze();
        return new Loaded{Context=context,Content=content,Behavior=DefinitionId.Parse("fixture.arena:behaviors/test")};
    }
    static bool Invoke(Loaded loaded,out string error,ModEffectEvent kind=ModEffectEvent.Tick)=>
        ((IModInteractiveBehaviorScriptContext)loaded.Context).TryInvokeBehavior(loaded.Behavior,kind,null,null,loaded.Fighter,out error);
    static void Event(Loaded loaded){Check(Invoke(loaded,out var error),error);}
    static void Main(string[] args)
    {
        fixture=args[0];repo=args[1];Geometry();
        using(var l=Load("local yes,e=fighter:overlaps_rect(rect);assert(yes and e==nil);local no,f=fighter.opponent:overlaps_rect(rect);assert(no==false and f==nil);marker=assert(fighter:mark_rect(rect));assert(sf2.world.is_marker_active(marker));assert(sf2.world.set_marker_color(marker,'#12345678'));assert(sf2.world.remove_marker(marker));assert(not sf2.world.is_marker_active(marker));assert(not sf2.world.remove_marker(marker));assert(not sf2.world.set_marker_color(marker,'#ffffff'))"))
        {l.Fighter.Peer.Inside=false;Event(l);Check(l.Fighter.Markers[0].Color.A==0x78&&l.Fighter.Markers[0].Removed==1,"Color/remove idempotency");}
        using(var l=Load("marker=assert(fighter:mark_rect(rect))")){Event(l);l.Dispose();Check(!l.Fighter.Markers[0].Active,"Script teardown");}
        using(var l=Load("for i=1,16 do assert(fighter:mark_rect(rect)) end;local m,e=fighter:mark_rect(rect);assert(m==nil and e:find('16'))")){Event(l);Check(l.Fighter.Markers.Count==16,"Per-context marker budget");}
        using(var l=Load("if calls==1 then marker=assert(fighter:mark_rect(rect)) else assert(not sf2.world.is_marker_active(marker));assert(fighter:mark_rect(rect)) end")){Event(l);l.Fighter.Markers[0].Active=false;Event(l);Check(l.Fighter.Markers[0].Removed==1&&l.Fighter.Markers.Count==2,"Completed slot release");}
        using(var l=Load("local value,e=fighter:overlaps_rect(rect);assert(value==nil and e=='unavailable');local m,f=fighter:mark_rect(rect);assert(m==nil and f=='inactive round')")){l.Fighter.Reject=true;Event(l);}
        using(var l=Load("local m,e=fighter:mark_rect(rect);assert(m==nil and e:find('render failed'))")){l.Fighter.Throws=true;Event(l);}
        foreach(var rect in new[]{"{}","{x=0,y=0,width=0,height=1}","{x=0,y=0,width=1,height=-1}","{x=math.huge,y=0,width=1,height=1}","{x=0/0,y=0,width=1,height=1}","{x=10001,y=0,width=1,height=1}","{x=0,y=0,width=4001,height=1}","{x='0',y=0,width=1,height=1}","{x=0,y=0,width=1,height=1,angle=0}","nil","1"})
        foreach(var method in new[]{"overlaps_rect","mark_rect"})
        using(var l=Load("fighter:"+method+"("+rect+")")){Check(!Invoke(l,out var error)&&error.Length>0,"Invalid rectangle: "+method+rect);Check(l.Fighter.Queries==0&&l.Fighter.Markers.Count==0,"Invalid input reached backend");}
        foreach(var bad in new[]{"fighter:overlaps_rect()","fighter:overlaps_rect(rect,2)","fighter:mark_rect(rect,1)","fighter:mark_rect(rect,'bad')","fighter:mark_rect(rect,nil,2)","sf2.world.is_marker_active({})","sf2.world.remove_marker({})","sf2.world.set_marker_color({},'#ffffff')"})
        using(var l=Load(bad)){Check(!Invoke(l,out var error)&&error.Length>0,"Invalid arena call: "+bad);Check(l.Fighter.Markers.Count==0,"Invalid call allocated");}
        foreach(var bad in new[]{"sf2.world.remove_marker(marker,1)","sf2.world.is_marker_active(marker,1)","sf2.world.set_marker_color(marker)","sf2.world.set_marker_color(marker,1)","sf2.world.set_marker_color(marker,'bad')"})
        using(var l=Load("marker=assert(fighter:mark_rect(rect));"+bad)){Check(!Invoke(l,out var error),"Bad retained handle call accepted");Check(l.Fighter.Markers[0].Active,"Bad call removed marker");}
        foreach(var body in new[]{"fighter:mark_rect(rect)","fighter.opponent:overlaps_rect(rect)"})
        using(var l=Load(body,"content.register")){Check(!Invoke(l,out var error)&&error.Contains("capability"),"Capability missing");}
        using(var l=Load("assert(fighter:overlaps_rect(rect))","content.register"))Event(l);
        foreach(var kind in new[]{ModEffectEvent.FightBegin,ModEffectEvent.RoundBegin,ModEffectEvent.RoundEnd,ModEffectEvent.FightEnd})
        using(var l=Load("fighter:mark_rect(rect)")){Check(!Invoke(l,out var error,kind)&&error.Contains("simulation"),"Marker forbidden lifecycle");}
        foreach(var body in new[]{"if calls==1 then saved=fighter.overlaps_rect else saved(rect) end","if calls==1 then saved=fighter.mark_rect else saved(rect) end","if calls==1 then saved=fighter.opponent.overlaps_rect else saved(rect) end"})
        using(var l=Load(body)){Event(l);Check(!Invoke(l,out var error)&&error.Contains("expired"),"Escaped fighter remained active");}
        using(var l=Load("for i=1,16 do assert(fighter:overlaps_rect(rect));assert(fighter.opponent:overlaps_rect(rect)) end;fighter:overlaps_rect(rect)"))
        {Check(!Invoke(l,out var error)&&error.Contains("32"),"Shared query budget");Check(l.Fighter.Queries+l.Fighter.Peer.Queries==32,"Budget queried native source");}
        // Foreign handle identity: copying an actual table from another context
        // cannot make the recipient its owner, even when the shape is unchanged.
        using(var a=Load("foreign=assert(fighter:mark_rect(rect))"))using(var b=Load("sf2.world.remove_marker(foreign)"))
        {Event(a);var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;var sa=(MoonSharp.Interpreter.Script)a.Context.GetType().GetField("_script",flags).GetValue(a.Context);var sb=(MoonSharp.Interpreter.Script)b.Context.GetType().GetField("_script",flags).GetValue(b.Context);bool blocked=false;try{sb.Globals.Set("foreign",sa.Globals.Get("foreign"));}catch(MoonSharp.Interpreter.ScriptRuntimeException e){blocked=e.Message.Contains("different scripts");}Check(blocked&&a.Fighter.Markers[0].Active,"MoonSharp foreign marker ownership");}
        using(var l=Load("marker=assert(fighter:mark_rect(rect))",freeze:false)){Check(!Invoke(l,out var error)&&error.Contains("registration")&&l.Fighter.Markers.Count==0,"Marker created while dependent registration open");l.Content.Freeze();Event(l);}
        using(var l=Load("local hud=sf2.ui.open{id='owner',mount='hud',root={id='text',kind='text',text='Owner',width=100,height=40},on_close=function() fighter:mark_rect(rect) end};sf2.ui.close(hud)","content.register,presentation.visuals,ui.create")){Event(l);Check(l.Fighter.Markers.Count==0,"UI cleanup created a marker");}
        Artwork();Shipped();Console.WriteLine("Arena runtime PASS: "+checks+" checks; production geometry/Lua/scopes and shipped hazard with controlled native rig/markers/clock.");
    }
    static void Artwork()
    {
        using(var l=Load("marker=assert(fighter:mark_sprite(one,rect));assert(sf2.world.set_marker_rect(marker,{x=10,y=-30,width=50,height=60}));assert(sf2.world.set_marker_sprite(marker,two));assert(sf2.world.set_marker_color(marker,'#ffcc3388'));assert(sf2.world.remove_marker(marker));local a,e=sf2.world.set_marker_rect(marker,rect);assert(a==false and e==nil);assert(sf2.world.set_marker_sprite(marker,one)==false)"))
        {Event(l);var m=l.Fighter.Markers.Single();Check(m.Sprite.Path=="sprites/two"&&m.Rect.X==10&&m.Rect.Width==50&&m.SpriteChanges==2&&m.RectChanges==1&&m.Removed==1,"Typed sprite/update/remove contract");}
        using(var l=Load("for i=1,8 do assert(fighter:mark_sprite(one,rect));assert(fighter:mark_rect(rect)) end;local m,e=fighter:mark_sprite(two,rect);assert(m==nil and e:find('16'))")){Event(l);Check(l.Fighter.Markers.Count==16,"Sprites and rectangles share scope capacity");}
        using(var l=Load("if calls==1 then marker=assert(fighter:mark_sprite(one,rect)) else local ok,e=sf2.world.set_marker_sprite(marker,two);assert(not ok and e=='update failed');ok,e=sf2.world.set_marker_rect(marker,{x=1,y=2,width=3,height=4});assert(not ok and e=='update failed') end"))
        {Event(l);var m=l.Fighter.Markers[0];m.RejectUpdate=true;Event(l);Check(m.Active&&m.Sprite.Path=="sprites/one"&&m.Rect.X==0&&m.Rect.Width==20,"Failed updates retain live prior artwork");}
        using(var l=Load("local m,e=fighter:mark_sprite(one,rect);assert(m==nil and e:find('partial allocation'))")){l.Fighter.PartialThrows=true;Event(l);Check(l.Fighter.Markers.Single().Removed==1,"Partial native allocation is disposed after throwing");}
        using(var l=Load("marker=assert(fighter:mark_sprite(one,rect))")){Event(l);l.Dispose();Check(l.Fighter.Markers[0].Removed==1,"Sprite script teardown");}
        foreach(var bad in new[]{"fighter:mark_sprite()","fighter:mark_sprite(one)","fighter:mark_sprite({},rect)","fighter:mark_sprite('sprites/one',rect)","fighter:mark_sprite(wrong,rect)","fighter:mark_sprite(one,rect,'bad')","fighter:mark_sprite(one,rect,nil,1)","fighter:mark_sprite(one,{x=0,y=0,width=-1,height=1})","sf2.world.set_marker_rect({},rect)","sf2.world.set_marker_sprite({},one)"})
        using(var l=Load(bad)){Check(!Invoke(l,out var error)&&error.Length>0,"Invalid artwork arguments: "+bad);Check(l.Fighter.Markers.Count==0,"Invalid artwork allocated");}
        foreach(var bad in new[]{"sf2.world.set_marker_rect(marker)","sf2.world.set_marker_rect(marker,{},1)","sf2.world.set_marker_rect(marker,{x=0,y=0,width=0,height=1})","sf2.world.set_marker_sprite(marker,wrong)","sf2.world.set_marker_sprite(marker,'sprites/one')","sf2.world.set_marker_sprite(marker,one,2)"})
        using(var l=Load("marker=assert(fighter:mark_sprite(one,rect));"+bad)){Check(!Invoke(l,out var error),"Invalid retained artwork update accepted");Check(l.Fighter.Markers[0].Active&&l.Fighter.Markers[0].SpriteChanges==1,"Invalid artwork update mutated existing art");}
        using(var l=Load("fighter:mark_sprite(one,rect)","content.register")){Check(!Invoke(l,out var error)&&error.Contains("capability"),"Sprite presentation permission");}
        using(var l=Load("if calls==1 then saved=fighter.mark_sprite else saved(one,rect) end")){Event(l);Check(!Invoke(l,out var error)&&error.Contains("expired"),"Sprite creation callable escaped callback");}
        foreach(var kind in new[]{ModEffectEvent.FightBegin,ModEffectEvent.RoundBegin,ModEffectEvent.RoundEnd,ModEffectEvent.FightEnd})
        using(var l=Load("fighter:mark_sprite(one,rect)")){Check(!Invoke(l,out var error,kind)&&error.Contains("simulation"),"Sprite creation forbidden lifecycle");}
        using(var l=Load("local m,e=fighter:mark_sprite(one,rect);assert(m==nil and e=='inactive round')")){l.Fighter.Reject=true;Event(l);}
        using(var l=Load("marker=assert(fighter:mark_sprite(one,rect))",freeze:false)){Check(!Invoke(l,out var error)&&error.Contains("registration")&&l.Fighter.Markers.Count==0,"Sprite registration gate");}
        using(var l=Load("local hud=sf2.ui.open{id='owner',mount='hud',root={id='text',kind='text',text='Owner',width=100,height=40},on_close=function() fighter:mark_sprite(one,rect) end};sf2.ui.close(hud)","content.register,presentation.visuals,ui.create")){Event(l);Check(l.Fighter.Markers.Count==0,"Sprite UI cleanup gate");}
    }
    static void Geometry()
    {
        var r=new ModArenaRect(0,0,10,10);
        foreach(var c in new[]{(-5d,5d,15d,5d,0d),(5d,-5d,5d,15d,0d),(-1d,-1d,11d,11d,0d),(0d,0d,0d,0d,0d),(5d,5d,5d,5d,0d),(-2d,5d,-2d,9d,2d),(-3d,-4d,-3d,-4d,5d)})
            Check(r.OverlapsCapsule(c.Item1,c.Item2,c.Item3,c.Item4,c.Item5),"Capsule contact/tangent");
        foreach(var c in new[]{(-2d,-2d,-2d,-2d,2d),(-2d,5d,-2d,9d,1.99d),(-3d,-4d,-3d,-4d,4.99d),(11d,-4d,11d,-2d,1d),(-5d,-1d,-1d,-5d,1d)})
            Check(!r.OverlapsCapsule(c.Item1,c.Item2,c.Item3,c.Item4,c.Item5),"Rounded corner/distant capsule false positive");
        Check(!r.OverlapsCapsule(double.NaN,0,0,0,1)&&!r.OverlapsCapsule(0,0,0,0,-1),"Invalid native capsule");
    }
    static void Shipped()
    {
        var mod=ModDiscovery.DiscoverLoose(Path.Combine(repo,"Mods")).Mods.Single(m=>m.Id.Value=="example.pulse-arena");var content=new ModContentCatalog();var stages=new XmlDocument();stages.Load(Path.Combine(repo,"Assets/vanillaXml/stages.xml"));CoreContentImporter.ImportStages(content,stages.SelectSingleNode("Stages/Zones"));
        var surfaces=new List<ModUiSurface>();using var tx=content.BeginRegistration(mod);var api=new ModApiFacade(mod,new AssetResolver(new IAssetProvider[]{new LooseModProvider(mod)}),tx,new ModStateRuntime(),null);using var context=new MoonSharpScriptRuntime(surfaces.Add).CreateContext(mod,api);context.ExecuteEntrypoint();tx.Commit();content.Freeze();
        var native=new Fighter{Peer=new Fighter{Inside=false}};var doc=new XmlDocument();doc.LoadXml("<Instance/>");var rule=content.FightRules.Single();var fighter=new ModInstanceFighter(native,doc.DocumentElement,rule);int round=1;
        void Event(ModEffectEvent kind){Check(((IModInteractiveBehaviorScriptContext)context).TryInvokeBehavior(rule.Behavior,kind,null,new Dictionary<string,string>{{"source","rule"},{"round",round.ToString()},{"fight_id","fixture"}},fighter,out var error),error);}
        Event(ModEffectEvent.RoundBegin);var hud=surfaces.Single();
        for(int i=0;i<120;i++)Event(ModEffectEvent.Tick);Check(native.Changes==0&&hud.Read("status").Text.Contains("Warning")&&native.Markers.Single().Active,"Warning phase");
        for(int i=0;i<120;i++)Event(ModEffectEvent.Tick);Check(native.Changes==4&&native.Peer.Changes==0&&Math.Abs(native.Health-.9)<.0001,"Active contact schedule");Check(native.Markers[0].Color.R==255&&native.Markers[0].Color.G==0x33,"Recolor");
        for(int i=0;i<120;i++)Event(ModEffectEvent.Tick);Check(!native.Markers[0].Active&&native.Changes==4&&hud.Read("status").Text.Contains("Safe"),"Safe phase");
        Event(ModEffectEvent.Tick);Check(native.Markers.Count==2,"Recurring phase");Event(ModEffectEvent.RoundEnd);Check(hud.IsClosed&&!native.Markers[1].Active,"Round cleanup");
        round++;Event(ModEffectEvent.RoundBegin);Event(ModEffectEvent.Tick);Check(native.Markers.Count==3&&surfaces.Last().Read("contacts").Text=="Contacts: 0","New round state");Event(ModEffectEvent.FightEnd);Check(surfaces.Last().IsClosed&&!native.Markers.Last().Active,"Fight cleanup");
    }
}
