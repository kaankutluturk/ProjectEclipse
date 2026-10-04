using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eclipse.Modding;

static class Program
{
    static int checks; static string fixture;
    static void Check(bool yes,string message){checks++;if(!yes)throw new Exception(message);}
    sealed class Fighter : IModFighterOperations, IModFighterCamera, IModCombatSnapshotSource
    {
        public ModCameraSlot Slot=new ModCameraSlot(); public bool Alive=true; public int Acquisitions;
        public double Health=>1;
        public ModCombatSnapshot CaptureCombatSnapshot()=>new ModCombatSnapshot(new ModFighterSnapshot(1,1,1,0,0,0),null,1,true);
        public bool TryChangeHealth(double amount,out string error){error=null;return true;}
        public bool TryAddMagicCharge(double amount,out string error){error=null;return true;}
        public bool TryAcquireCamera(ModId owner,ModCameraSettings settings,out IModCameraControl camera,out string error)
        {Acquisitions++;return Slot.TryAcquire(owner,settings,()=>Alive,out camera,out error);}
    }
    sealed class Loaded : IDisposable
    {
        public IModScriptContext Context; public ModContentCatalog Content; public DefinitionId Behavior;
        public Fighter Fighter=new Fighter(); public List<ModUiSurface> Surfaces=new List<ModUiSurface>();
        public void Dispose()=>Context.Dispose();
    }
    static Loaded Load(string body,string caps="content.register,presentation.camera",bool freeze=true,string id="fixture.camera")
    {
        var parent=Path.Combine(fixture,Guid.NewGuid().ToString("N"));var folder=Path.Combine(parent,id);
        Directory.CreateDirectory(Path.Combine(folder,"scripts"));
        File.WriteAllText(Path.Combine(folder,"mod.toml"),"schema=1\nid=\""+id+"\"\nname=\"Camera\"\nversion=\"1.0.0\"\nauthors=[\"Fixture\"]\nentrypoint=\"scripts/main.lua\"\ncapabilities=["+string.Join(",",caps.Split(',').Select(c=>"\""+c+"\""))+ "]\n");
        File.WriteAllText(Path.Combine(folder,"scripts/main.lua"),"local sf2=require('sf2');local camera,saved;local calls=0;local function run(_,fighter) calls=calls+1;"+body+" end;sf2.behaviors.register{id='test',on_tick=run,on_round_begin=run,on_round_end=run,on_fight_begin=run,on_fight_end=run,on_actor_spawn=run,on_actor_end=run};");
        var discovery=ModDiscovery.DiscoverLoose(parent);Check(discovery.Diagnostics.Count==0,"Manifest");
        var mod=discovery.Mods.Single();var content=new ModContentCatalog();using var tx=content.BeginRegistration(mod);
        var loaded=new Loaded{Content=content,Behavior=DefinitionId.Parse(id+":behaviors/test")};
        var api=new ModApiFacade(mod,new AssetResolver(new IAssetProvider[]{new LooseModProvider(mod)}),tx,new ModStateRuntime(),null);
        loaded.Context=new MoonSharpScriptRuntime(loaded.Surfaces.Add).CreateContext(mod,api);
        loaded.Context.ExecuteEntrypoint();tx.Commit();if(freeze)content.Freeze();return loaded;
    }
    static bool Invoke(Loaded l,out string error,ModEffectEvent kind=ModEffectEvent.Tick)=>
        ((IModInteractiveBehaviorScriptContext)l.Context).TryInvokeBehavior(l.Behavior,kind,null,null,l.Fighter,out error);
    static void Event(Loaded l,ModEffectEvent kind=ModEffectEvent.Tick){Check(Invoke(l,out var error,kind),error);}
    static void Main(string[] args)
    {
        fixture=args[0];
        using(var l=Load("camera=assert(fighter:acquire_camera{center_x=123,offset_y=-30,zoom=1.2});assert(sf2.world.is_camera_active(camera));assert(sf2.world.set_camera(camera,{}))"))
        {Event(l);Check(l.Fighter.Slot.Settings.CenterX==null&&l.Fighter.Slot.Settings.Zoom==null&&l.Fighter.Slot.Settings.OffsetY==0,"Full settings replacement restores defaults");l.Dispose();Check(l.Fighter.Slot.Settings==null,"Script disposal releases native view");}
        using(var l=Load("camera=assert(fighter:acquire_camera());local no,e=fighter:acquire_camera();assert(no==nil and e:find('already owns'));assert(sf2.world.release_camera(camera));assert(not sf2.world.release_camera(camera));assert(not sf2.world.is_camera_active(camera));local ok,why=sf2.world.set_camera(camera,{zoom=2});assert(ok==false and why:find('expired'));assert(fighter:acquire_camera(nil))"))
        {Event(l);Check(l.Fighter.Acquisitions==2,"Same-script conflict precedes backend allocation");}
        foreach(var settings in new[]{"1","'bad'","{center_x='0'}","{center_x=10001}","{center_x=-10001}","{center_x=math.huge}","{center_x=0/0}","{offset_y=1001}","{offset_y=-1001}","{offset_y=0/0}","{zoom=0.249}","{zoom=4.001}","{zoom=math.huge}","{zoom=0/0}","{yaw=0}","{[1]=1}"})
        using(var l=Load("fighter:acquire_camera("+settings+")"))
        {Check(!Invoke(l,out var error)&&error.Length>0,"Bad settings "+settings);Check(l.Fighter.Acquisitions==0,"Invalid settings allocated camera");}
        foreach(var bad in new[]{"fighter:acquire_camera({},1)","sf2.world.release_camera({})","sf2.world.is_camera_active({})","sf2.world.set_camera({}, {})"})
        using(var l=Load(bad)){Check(!Invoke(l,out var error),"Malformed call "+bad);Check(l.Fighter.Acquisitions==0,"Malformed call allocated");}
        foreach(var bad in new[]{"sf2.world.set_camera(camera)","sf2.world.set_camera(camera,{zoom=0})","sf2.world.set_camera(camera,{},1)","sf2.world.release_camera(camera,1)","sf2.world.is_camera_active(camera,1)"})
        using(var l=Load("camera=assert(fighter:acquire_camera{zoom=1.25});"+bad))
        {Check(!Invoke(l,out var error),"Bad retained call "+bad);Check(l.Fighter.Slot.Settings.Zoom==1.25,"Invalid retained update preserved view");}
        foreach(var caps in new[]{"content.register","content.register,presentation.visuals"})
        using(var l=Load("fighter:acquire_camera()",caps))
        {Check(!Invoke(l,out var error)&&error.Contains("capability"),"Independent camera capability");Check(l.Fighter.Acquisitions==0,"Missing capability allocated");}
        foreach(var kind in new[]{ModEffectEvent.FightBegin,ModEffectEvent.RoundBegin,ModEffectEvent.RoundEnd,ModEffectEvent.FightEnd,ModEffectEvent.ActorEnd})
        using(var l=Load("fighter:acquire_camera()"))
        {bool invoked=Invoke(l,out var error,kind);Check(!invoked&&error.Contains("combat callback"),"Lifecycle acquisition "+kind+": "+error);Check(l.Fighter.Acquisitions==0,"Lifecycle allocated");}
        using(var l=Load("camera=assert(fighter:acquire_camera())"))Event(l,ModEffectEvent.ActorSpawn);
        using(var l=Load("if calls==1 then saved=fighter.acquire_camera else saved() end"))
        {Event(l);Check(!Invoke(l,out var error)&&error.Contains("expired"),"Escaped fighter callable");Check(l.Fighter.Acquisitions==0,"Expired callable allocated");}
        using(var l=Load("camera=assert(fighter:acquire_camera())",freeze:false))
        {Check(!Invoke(l,out var error)&&error.Contains("registration"),"Registration gate");Check(l.Fighter.Acquisitions==0,"Registration allocated");l.Content.Freeze();Event(l);}
        using(var l=Load("local hud=sf2.ui.open{id='owner',mount='hud',root={id='label',kind='text',text='Owner',width=100,height=40},on_close=function() fighter:acquire_camera() end};sf2.ui.close(hud)","content.register,presentation.camera,ui.create"))
        {Event(l);Check(l.Fighter.Acquisitions==0,"UI cleanup cannot create ownership");}
        using(var l=Load("if calls==1 then camera=assert(fighter:acquire_camera()) else assert(not sf2.world.is_camera_active(camera));assert(sf2.world.release_camera(camera));assert(not sf2.world.release_camera(camera)) end"))
        {Event(l);l.Fighter.Alive=false;Event(l);Check(l.Fighter.Slot.Settings==null,"Expired owner releases slot");}
        using(var l=Load("if calls==1 then camera=assert(fighter:acquire_camera()) else assert(sf2.world.release_camera(camera)) end"))
        {Event(l);Event(l,ModEffectEvent.RoundEnd);Check(l.Fighter.Slot.Settings==null,"Retained cleanup allowed at round end");}
        using(var a=Load("camera=assert(fighter:acquire_camera{zoom=2})"))
        using(var b=Load("local c,e=fighter:acquire_camera();assert(c==nil and e:find('fixture.camera'))",id:"fixture.other"))
        {b.Fighter.Slot=a.Fighter.Slot;Event(a);Event(b);Check(a.Fighter.Slot.Settings.Zoom==2,"Cross-mod conflict retains owner view");a.Dispose();Check(a.Fighter.Slot.Settings==null,"Owner disposal releases shared slot");}
        using(var a=Load("foreign=assert(fighter:acquire_camera())"))using(var b=Load("sf2.world.release_camera(foreign)"))
        {
            Event(a);var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            var sa=(MoonSharp.Interpreter.Script)a.Context.GetType().GetField("_script",flags).GetValue(a.Context);
            var sb=(MoonSharp.Interpreter.Script)b.Context.GetType().GetField("_script",flags).GetValue(b.Context);
            bool rejected=false;try{sb.Globals.Set("foreign",sa.Globals.Get("foreign"));}catch(MoonSharp.Interpreter.ScriptRuntimeException){rejected=true;}
            Check(rejected&&a.Fighter.Slot.Settings!=null,"Foreign context cannot transfer camera handle");
        }
        Scopes();Shipped(args[1]);Console.WriteLine("Camera runtime PASS: "+checks+" checks; actual Lua, ownership, capability, callback/registration/cleanup gates, shipped HUD and production scopes; native source controlled.");
    }
    static void Shipped(string repo)
    {
        var mod=ModDiscovery.DiscoverLoose(Path.Combine(repo,"Mods")).Mods.Single(m=>m.Id.Value=="example.camera-lab");
        var content=new ModContentCatalog();var stages=new System.Xml.XmlDocument();stages.Load(Path.Combine(repo,"Assets/vanillaXml/stages.xml"));
        CoreContentImporter.ImportStages(content,stages.SelectSingleNode("Stages/Zones"));
        var surfaces=new List<ModUiSurface>();using var tx=content.BeginRegistration(mod);
        var api=new ModApiFacade(mod,new AssetResolver(new IAssetProvider[]{new LooseModProvider(mod)}),tx,new ModStateRuntime(),null);
        using var context=new MoonSharpScriptRuntime(surfaces.Add).CreateContext(mod,api);context.ExecuteEntrypoint();tx.Commit();content.Freeze();
        var native=new Fighter();var doc=new System.Xml.XmlDocument();doc.LoadXml("<Instance/>");var rule=content.FightRules.Single();
        var fighter=new ModInstanceFighter(native,doc.DocumentElement,rule);
        void Event(ModEffectEvent kind){Check(((IModInteractiveBehaviorScriptContext)context).TryInvokeBehavior(rule.Behavior,kind,null,new Dictionary<string,string>{{"source","rule"},{"round","1"},{"fight_id","fixture"}},fighter,out var error),error);}
        Event(ModEffectEvent.RoundBegin);var hud=surfaces.Single();Event(ModEffectEvent.Tick);Check(native.Slot.Settings==null,"Shipped starts released");
        Check(hud.TryClick("focus"),"Focus button");Event(ModEffectEvent.Tick);Check(native.Slot.Settings.Zoom==1.3&&native.Slot.Settings.OffsetY==-40,"Shipped focus");
        Check(hud.TryClick("sweep"),"Sweep button");Event(ModEffectEvent.Tick);var first=native.Slot.Settings;
        for(int i=0;i<15;i++)Event(ModEffectEvent.Tick);Check(native.Slot.Settings.CenterX!=first.CenterX&&native.Slot.Settings.OffsetY!=first.OffsetY,"Shipped simulation sweep");
        Check(hud.TryClick("native"),"Native button");Event(ModEffectEvent.Tick);Check(native.Slot.Settings.Zoom==null&&native.Slot.Settings.CenterX==null&&native.Slot.Settings.OffsetY==0,"Shipped default replacement");
        Check(hud.TryClick("release")&&native.Slot.Settings==null,"Shipped release without simulation tick");
        Check(hud.TryClick("focus"),"Restart focus");Event(ModEffectEvent.Tick);hud.Close();Check(native.Slot.Settings==null,"HUD cleanup releases ownership");
        Event(ModEffectEvent.RoundBegin);hud=surfaces.Last();Check(hud.TryClick("focus"),"Next HUD focus");Event(ModEffectEvent.Tick);Event(ModEffectEvent.RoundEnd);Check(hud.IsClosed&&native.Slot.Settings==null,"Round cleanup");
        Event(ModEffectEvent.RoundBegin);hud=surfaces.Last();hud.TryClick("focus");Event(ModEffectEvent.Tick);Event(ModEffectEvent.FightEnd);Check(hud.IsClosed&&native.Slot.Settings==null,"Fight cleanup");
    }
    sealed class Partial : IModCameraControl
    {
        public int Disposals; public bool Active=true,ThrowDispose;
        public bool IsActive=>Active;
        public bool TrySet(ModCameraSettings settings,out string error){error=null;return Active;}
        public void Dispose(){Disposals++;Active=false;if(ThrowDispose)throw new Exception("dispose failed");}
    }
    sealed class Provider : IModFighterCamera
    {
        public Partial Native=new Partial(); public bool Throw,Success; public Action During;
        public bool TryAcquireCamera(ModId owner,ModCameraSettings settings,out IModCameraControl camera,out string error)
        {camera=Native;error="rejected";During?.Invoke();if(Throw)throw new Exception("partial failed");return Success;}
    }
    static void Scopes()
    {
        var owner=ModId.Parse("fixture.camera");var settings=new ModCameraSettings();
        foreach(var throws in new[]{false,true})foreach(var disposalThrows in new[]{false,true})
        {using var scope=new ModCameraScope();var p=new Provider{Throw=throws};p.Native.ThrowDispose=disposalThrows;Check(!scope.TryAcquire(p,owner,settings,out var c,out var e)&&c==null,"Partial rejected");Check(p.Native.Disposals==1,"Partial disposed once even when disposal throws");}
        using(var scope=new ModCameraScope())
        {var p=new Provider{Success=true,During=scope.Dispose};Check(!scope.TryAcquire(p,owner,settings,out var c,out var e)&&p.Native.Disposals==1,"Reentrant teardown cannot adopt native lease");}
        using(var scope=new ModCameraScope())
        {var p=new Provider{Success=true};p.Native.Active=false;Check(!scope.TryAcquire(p,owner,settings,out var c,out var e)&&p.Native.Disposals==1,"Inactive backend not adopted");}
        using(var scope=new ModCameraScope())Check(!scope.TryAcquire(null,owner,settings,out var c,out var e)&&e.Contains("unavailable"),"Unsupported optional backend");
        using(var slot=new ModCameraSlot())
        {
            bool alive=true;Check(slot.TryAcquire(owner,settings,()=>alive,out var old,out var e),"Initial slot");
            alive=false;Check(slot.Settings==null&&!old.IsActive,"Native liveness prunes settings");
            Check(slot.TryAcquire(owner,new ModCameraSettings(0,100,2),()=>true,out var next,out e),"Successor ownership");
            old.Dispose();Check(next.IsActive&&slot.Settings.Zoom==2,"Stale handle cannot release successor");
            slot.Clear();Check(!next.IsActive&&slot.Settings==null,"Round clear");
            Check(!slot.TryAcquire(owner,settings,()=>throw new Exception("liveness"),out var bad,out e)&&bad==null&&slot.Settings==null,"Throwing lifetime fails closed");
            slot.Dispose();Check(!slot.TryAcquire(owner,settings,()=>true,out bad,out e),"Closed slot");
        }
        var retained=ReleasedOwner();GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();
        Check(!retained.owner.IsAlive&&!retained.camera.IsActive,"Released handle does not retain native owner through lifetime delegate");
        GC.KeepAlive(retained.camera);
    }
    sealed class LifetimeOwner { public bool Active()=>true; }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    static (WeakReference owner,IModCameraControl camera) ReleasedOwner()
    {
        var owner=new LifetimeOwner();var weak=new WeakReference(owner);var slot=new ModCameraSlot();
        slot.TryAcquire(ModId.Parse("fixture.camera"),new ModCameraSettings(),owner.Active,out var camera,out _);
        camera.Dispose();return (weak,camera);
    }
}
