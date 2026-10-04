using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml;
using Eclipse.Modding;

sealed class Core : IAssetProvider
{
    public ModId Namespace => ModId.Parse("core");
    public bool TryDescribe(AssetId id,out AssetMetadata metadata)
    { metadata=new AssetMetadata(id,AssetKind.Sprite,AssetSourceKind.Core,"",-1,"fixture"); return true; }
}

static class Program
{
    static int checks;
    static void Check(bool value, string message) { checks++; if (!value) throw new Exception(message); }
    static void Main(string[] args)
    {
        string mods = args[0], entry = Path.Combine(mods,"example.charge-ui/scripts/main.lua");
        foreach (string body in new[] {
            "memory.calls=(memory.calls or 0)+1; assert(event.self.health==40 and event.opponent.health==30); if memory.calls==1 then return event.actions[2] end; return 'wait'",
            "local identity=event.self.actor; assert(identity.id=='a1' and identity.definition=='fixture:actors/ally' and identity.owner=='fixture' and identity.team=='player'); assert(event.opponent.actor.id=='a2' and event.opponent.actor.team=='opponent'); identity.id='forged'; identity.owner='forged'; event.opponent.actor.team='player'; return event.actions[1]",
            "assert(event.self.actor==nil and event.opponent.actor==nil); return event.actions[1]",
            "return nil", "return {}", "while true do end",
            "return event.opponent.animation",
            "local observed=event.opponent.animation; assert(event.self.animation==nil and observed.name=='opponent kick' and observed.type=='attack' and observed.facing==-1); for _,interval in ipairs(observed.intervals) do if interval.type=='attack' then assert(interval.name=='contact'); interval.type='none'; observed.name='edited'; return event.actions[1] end end; error('missing attack observation')",
            "local timed=event.actions[2]; assert(timed.timing.first_sample==3 and timed.timing.last_sample==12 and timed.timing.mid_frames==2 and timed.timing.nominal_frames==30 and timed.timing.nominal_seconds==0.5 and not timed.timing.looped); assert(timed.inputs[1].control=='Kick' and timed.inputs[1].press=='tap' and timed.inputs[2].control=='Back' and timed.inputs[2].press=='hold'); assert(event.actions[1].timing==nil and #event.actions[1].inputs==0); timed.timing.nominal_frames=999; timed.inputs[1].control='Magic'; return timed",
            "local best; for _,a in ipairs(event.actions) do if a.type=='attack' and (not best or a.priority>best.priority) then best=a end end; assert(best and best.name=='kick' and best.priority==7); best.name='changed'; best.type='move'; best.priority=-100; return best",
            "assert(event.back_wall_distance==250); return event.actions[1]",
            "if memory.saved then return memory.saved end; memory.saved=event.actions[1]; return memory.saved"
        })
        {
            File.WriteAllText(entry,"local sf2=require('sf2')\nsf2.tactics.register{id='brain',template='Standard',on_decide=function(memory,event) "+body+" end}");
            var mod = ModDiscovery.DiscoverLoose(mods).Mods.Single();
            var catalog = new ModContentCatalog();
            using (var tx = catalog.BeginRegistration(mod))
            using (var context = new MoonSharpScriptRuntime().CreateContext(mod,new ModApiFacade(mod,new AssetResolver(new IAssetProvider[]{new LooseModProvider(mod)}),tx,new ModStateRuntime(),null)))
            {
                context.ExecuteEntrypoint(); tx.Commit();
                var ai = (IModAiScriptContext)context;
                string tactic = "example.charge-ui:tactics/brain";
                Check(ai.HasAiHandler(tactic) && !ai.HasAiHandler("other.mod:tactics/brain"),"AI owner isolation failed");
                var observed = new ModAnimationSnapshot("opponent kick","attack",-1,new[]{new ModAnimationIntervalSnapshot("contact","attack")});
                var snapshot = new ModCombatSnapshot(new ModFighterSnapshot(40,50,1,10,0,0,actor:body.StartsWith("local identity")?new ModActorIdentity("a1","fixture:actors/ally","fixture","player"):null),new ModFighterSnapshot(30,50,1,60,0,0,observed,body.StartsWith("local identity")?new ModActorIdentity("a2","fixture:actors/rival","fixture","opponent"):null),60,true,250);
                var instance = new object();
                var candidates = new[] { new ModAiActionSnapshot("punch","move",3), new ModAiActionSnapshot("kick","attack",7,
                    new ModAiActionTiming(3,12,2,false),new[]{new ModAiActionInput("Kick","tap"),new ModAiActionInput("Back","hold")}) };
                bool valid = ai.TryDecideAi(tactic,instance,snapshot,candidates,out var first,out var error);
                if (body.StartsWith("memory.calls"))
                {
                    Check(valid && first==1,error);
                    Check(ai.TryDecideAi(tactic,instance,snapshot,new[]{"punch"},out var next,out error) && next==-1,"AI memory/wait failed");
                    Check(ai.TryDecideAi(tactic,new object(),snapshot,new[]{"punch","kick"},out var other,out error) && other==1,"Fighter memories leaked");
                }
                else if (body.StartsWith("local observed"))
                {
                    Check(valid && first==0,"Reactive animation decision failed: "+error);
                    Check(observed.Name=="opponent kick"&&observed.Intervals[0].Type=="attack","Lua changed native animation observation");
                    Check(ai.TryDecideAi(tactic,instance,snapshot,candidates,out var again,out error)&&again==0,"Animation observation edits leaked into next decision: "+error);
                }
                else if (body.StartsWith("local timed"))
                {
                    Check(valid && first==1,"Timing/input selection failed: "+error);
                    Check(candidates[1].Timing.NominalFrames==30 && candidates[1].Inputs[0].Control=="Kick","Lua changed nested host metadata");
                    Check(ai.TryDecideAi(tactic,instance,snapshot,candidates,out var again,out error) && again==1,"Nested edits leaked into next decision: "+error);
                }
                else if (body.StartsWith("local best"))
                {
                    Check(valid && first==1,"Metadata-based selection failed: "+error);
                    Check(candidates[1].Name=="kick" && candidates[1].Type=="attack" && candidates[1].Priority==7,"Lua mutated the host candidate");
                    Check(ai.TryDecideAi(tactic,instance,snapshot,candidates,out var again,out error) && again==1,"Candidate edits leaked into the next decision: "+error);
                }
                else if (body.StartsWith("assert(event.back_wall_distance"))
                    Check(valid && first==0,"Back-wall observation failed: "+error);
                else if(body.StartsWith("local identity"))
                {
                    Check(valid&&first==0,"Actor identity observation failed: "+error);
                    Check(snapshot.Self.Actor.Id=="a1"&&snapshot.Self.Actor.Owner=="fixture"&&snapshot.Opponent.Actor.Team=="opponent","Lua mutated native actor provenance");
                    Check(ai.TryDecideAi(tactic,instance,snapshot,candidates,out var again,out error)&&again==0,"Identity edits leaked across decisions: "+error);
                }
                else if(body.StartsWith("assert(event.self.actor"))Check(valid&&first==0,"Main fighter actor metadata was not nil: "+error);
                else if (body=="return nil") Check(valid && first==null,"Native fallback failed");
                else if (body.StartsWith("if memory.saved"))
                {
                    Check(valid && first==0,error);
                    Check(!ai.TryDecideAi(tactic,instance,snapshot,new[]{"punch"},out _,out error),"Stale action accepted");
                }
                else
                {
                    Check(!valid && error != null,"Forged/unbounded AI accepted");
                    Check(ai.TryDecideAi(tactic,instance,snapshot,new[]{"punch"},out var fallback,out error) && fallback==null,"Broken AI was retried instead of falling back");
                }
                context.Dispose(); Check(!ai.HasAiHandler(tactic),"Disposed context retained AI");
            }
        }
        ScriptedActorChecks(mods,entry,args[1]);
        var examples=Directory.Exists(Path.Combine(args[1],"Mods/example.programmable-ai"))?"Mods":"ArchivedMods";
        var shipped=ModDiscovery.DiscoverLoose(Path.Combine(args[1],examples)).Mods.Single(m=>m.Id.Value=="example.programmable-ai");
        var content=new ModContentCatalog(); var stages=new XmlDocument(); stages.Load(Path.Combine(args[1],"Assets/vanillaXml/stages.xml"));
        CoreContentImporter.ImportWarriorTemplates(content,stages.SelectSingleNode("Stages/Warriors/Templates"));
        var resolver=new AssetResolver(new IAssetProvider[]{new Core(),new LooseModProvider(shipped)});
        using(var tx=content.BeginRegistration(shipped))
        using(var context=new MoonSharpScriptRuntime().CreateContext(shipped,new ModApiFacade(shipped,resolver,tx,new ModStateRuntime(),null)))
        {
            ModLocalizationLoader.Load(shipped,resolver,tx); context.ExecuteEntrypoint(); tx.Commit();
            Check(content.Warriors.Count==4 && content.Modes.Count==1,"AI Dojo failed to register its distinct fighters and map mode");
            Check(content.Modes.Single().Fights.Count==4 && content.Modes.Single().Fights[3].ToString()==shipped.Id+":fights/encounter_4","Reactive encounter is not fourth in the dojo");
            var reactiveFighter=content.Warriors.Single(w=>w.Id.ToString()==shipped.Id+":warriors/fighter_4");
            Check(reactiveFighter.Template.ToString()=="core:warrior-templates/man_staff" && reactiveFighter.Tactic==shipped.Id+":tactics/reactive","Reactive fighter appearance/tactic wiring failed");
            Check(content.Fights.Single(f=>f.Id==content.Modes.Single().Fights[3]).Warriors.Single()==reactiveFighter.Id,"Reactive encounter uses the wrong warrior");
            var ai=(IModAiScriptContext)context;
            var close=new ModCombatSnapshot(new ModFighterSnapshot(40,50,1,10,0,0),new ModFighterSnapshot(30,50,1,60,0,0),60,true);
            var later=new ModCombatSnapshot(close.Self,close.Opponent,180,true);
            var names=new[]{"HighKick","LowKick","KatanaStepBack","KatanaStepForward"};
            foreach(var brain in new[]{"patient","footwork","alternating"})
            {
                var instance=new object(); string tactic=shipped.Id+":tactics/"+brain;
                Check(ai.TryDecideAi(tactic,instance,close,names,out var selected,out var error) && selected==(brain=="footwork"?2:0),"Shipped "+brain+" decision failed: "+error);
                Check(ai.TryDecideAi(tactic,instance,close,names,out selected,out error) && selected==-1,"Shipped "+brain+" cooldown failed");
                Check(ai.TryDecideAi(tactic,instance,later,names,out selected,out error) && selected==(brain=="alternating"?1:brain=="footwork"?2:0),"Shipped "+brain+" follow-up failed: "+error);
            }
            ReactiveChecks(ai,shipped.Id.ToString(),ReadNativeCandidates(args[2]));
        }
        Console.WriteLine("PASS: "+checks+" actual Lua AI decision, shipped AI Dojo/scripted actor policy, state isolation, action lifetime, fallback and instruction-budget checks. Native combat playtest remains separate.");
    }

    static void ScriptedActorChecks(string mods,string entry,string root)
    {
        // Load the public policy unchanged; the remaining example registers its
        // character, HUD and fight patch and is covered by the native playtest.
        string source=File.ReadAllText(Path.Combine(root,"Mods/example.scripted-actors/scripts/main.lua"));
        int end=source.IndexOf("local character = sf2.warriors.register",StringComparison.Ordinal);
        Check(end>0,"Public scripted-actor policy boundary is missing");
        File.WriteAllText(entry,source.Substring(0,end));
        var mod=ModDiscovery.DiscoverLoose(mods).Mods.Single();
        var catalog=new ModContentCatalog();
        using(var tx=catalog.BeginRegistration(mod))
        using(var context=new MoonSharpScriptRuntime().CreateContext(mod,new ModApiFacade(mod,new AssetResolver(new IAssetProvider[]{new LooseModProvider(mod)}),tx,new ModStateRuntime(),null)))
        {
            context.ExecuteEntrypoint(); tx.Commit();
            var ai=(IModAiScriptContext)context;
            string tactic=mod.Id+":tactics/sparring";
            ModAiActionSnapshot Action(string name,string type,int duration,bool looped,params string[] controls)=>
                new ModAiActionSnapshot(name,type,1,new ModAiActionTiming(0,duration-1,0,looped),controls.Select(c=>new ModAiActionInput(c,"tap")).ToArray());
            var actions=new[]{
                Action("short forward","move",40,false,"Forward"),
                Action("short backward","move",30,false,"Back"),
                Action("long double step","move",80,false,"Forward"),
                Action("quick punch","attack",12,false,"Punch"),
                Action("slower kick","attack",18,false,"Kick"),
                Action("looping punch","attack",1,true,"Punch"),
                Action("forward jump","move",5,false,"Forward","Up"),
            };
            ModFighterSnapshot Self(int facing,string id="a1",bool actor=true)=>new ModFighterSnapshot(10,10,1,0,0,0,
                new ModAnimationSnapshot("idle","none",facing,Array.Empty<ModAnimationIntervalSnapshot>()),
                actor?new ModActorIdentity(id,"fixture:actors/ally","fixture","player"):null);
            ModFighterSnapshot Target(float x,string team="opponent",bool actor=true)=>new ModFighterSnapshot(10,10,1,x,0,0,
                actor:actor?new ModActorIdentity("a2","fixture:actors/rival","fixture",team):null);
            void Decide(object controller,ModFighterSnapshot self,ModFighterSnapshot target,int frame,ModAiActionSnapshot[] available,int expected,string message)
            {
                Check(ai.TryDecideAi(tactic,controller,new ModCombatSnapshot(self,target,frame,true),available,out var choice,out var error)&&choice==expected,message+": "+error);
            }
            var controller=new object();
            Decide(controller,Self(1),Target(200),60,actions,0,"Public actor policy did not choose the shortest forward approach");
            Decide(controller,Self(1),Target(-200),66,actions,1,"Crossing a right-facing actor did not choose backward approach");
            Decide(controller,Self(-1),Target(-200),72,actions,0,"Left-facing forward approach was reversed");
            Decide(controller,Self(-1),Target(200),78,actions,1,"Crossing a left-facing actor did not choose backward approach");
            Decide(controller,Self(1),Target(95),84,actions,3,"Public actor policy did not choose the shortest non-looping contact attack");
            Decide(controller,Self(1),Target(50),153,actions,-1,"Actor voluntary cooldown ended early");
            Decide(controller,Self(1),Target(50),154,actions,3,"Actor did not resume after its voluntary cooldown");
            Decide(new object(),Self(1,"a3"),Target(50),90,actions,3,"Replacement actor inherited another controller's identity or cooldown");
            Decide(new object(),Self(1),Target(200,"player"),60,actions,-1,"Public actor policy attacked its own team");
            Decide(new object(),Self(1),Target(50,actor:false),60,actions,-1,"Public actor policy attacked a main fighter");
            Decide(new object(),Self(1,actor:false),Target(50),60,actions,-1,"Main fighter ran the actor policy");
            Decide(new object(),Self(1),null,60,actions,-1,"Missing target did not request wait");
            Decide(new object(),Self(1),Target(50),60,Array.Empty<ModAiActionSnapshot>(),-1,"Empty actor shortlist did not request wait");
            Decide(new object(),Self(1),Target(50),60,new[]{actions[5]},-1,"Looping attack was selected by the public actor policy");
        }
    }

    static ModAiActionSnapshot[] ReadNativeCandidates(string path)
    {
        using var document=JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.EnumerateArray().Select(value=>{
            var timing=value.GetProperty("Timing");
            var inputs=value.GetProperty("Inputs").EnumerateArray().Select(input=>
                new ModAiActionInput(input.GetProperty("Control").GetString(),input.GetProperty("Press").GetString())).ToArray();
            return new ModAiActionSnapshot(value.GetProperty("Name").GetString(),value.GetProperty("Type").GetString(),value.GetProperty("Priority").GetInt32(),
                new ModAiActionTiming(timing.GetProperty("FirstSample").GetInt32(),timing.GetProperty("LastSample").GetInt32(),timing.GetProperty("MidFrames").GetInt32(),timing.GetProperty("Looped").GetBoolean()),inputs);
        }).ToArray();
    }

    static void ReactiveChecks(IModAiScriptContext ai,string mod,ModAiActionSnapshot[] native)
    {
        string tactic=mod+":tactics/reactive";
        var self=new ModFighterSnapshot(40,50,1,0,0,0);
        var idle=new ModFighterSnapshot(40,50,1,50,0,0);
        var attacking=new ModFighterSnapshot(40,50,1,50,0,0,
            new ModAnimationSnapshot("unfamiliar move","none",-1,new[]{new ModAnimationIntervalSnapshot("custom contact","attack")}));
        var block=new ModFighterSnapshot(40,50,1,50,0,0,
            new ModAnimationSnapshot("attack in name only","attack",1,new[]{new ModAnimationIntervalSnapshot("guard","block")}));
        var kickInput=new[]{new ModAiActionInput("Kick","tap")};
        var slow=new ModAiActionSnapshot("mod:slow","attack",5,new ModAiActionTiming(0,59,0,false),kickInput);
        var quick=new ModAiActionSnapshot("mod:quick","attack",1,new ModAiActionTiming(0,11,0,false),kickInput);
        var looped=new ModAiActionSnapshot("mod:loop","attack",99,new ModAiActionTiming(0,0,0,true),kickInput);
        var back=new ModAiActionSnapshot("unfamiliar retreat","move",1,inputs:new[]{new ModAiActionInput("Back","tap")});
        var release=new ModAiActionSnapshot("back release","move",1,inputs:new[]{new ModAiActionInput("Back","release")});
        var candidates=new[]{slow,quick,looped,back,release};
        void Decide(object actor,ModFighterSnapshot opponent,int frame,ModAiActionSnapshot[] actions,int? expected,string message)
        {
            Check(ai.TryDecideAi(tactic,actor,new ModCombatSnapshot(self,opponent,frame,true),actions,out var choice,out var error)&&choice==expected,message+": "+error);
        }
        var first=new object();
        Decide(first,idle,60,candidates,1,"Reactive AI did not choose shortest non-looping kick");
        Decide(first,idle,83,candidates,-1,"Reactive AI ignored timing-based voluntary pause");
        Decide(first,idle,84,candidates,1,"Reactive AI failed to resume after voluntary pause");
        Decide(first,attacking,85,candidates,3,"Active attack did not override voluntary pause with a retreat");
        Decide(new object(),block,60,candidates,1,"Animation label caused retreat without active attack interval");
        Decide(new object(),attacking,60,new[]{release,quick},1,"Release-only direction was treated as backward movement");
        var held=new ModAiActionSnapshot("held back","move",1,inputs:new[]{new ModAiActionInput("Back","hold")});
        Decide(new object(),attacking,60,new[]{held,quick},0,"Held backward control was ignored");
        Decide(new object(),idle,60,new[]{looped},null,"Looping-only kick prevented native fallback");
        Decide(new object(),idle,60,new[]{new ModAiActionSnapshot("missing timing","attack",inputs:kickInput)},null,"Missing timing prevented native fallback");
        Decide(new object(),null,60,candidates,null,"Missing opponent prevented native fallback");
        Decide(new object(),new ModFighterSnapshot(40,50,1,500,0,0,attacking.Animation),60,candidates,null,"Far-away attack incorrectly triggered retreat");
        Decide(new object(),idle,60,Array.Empty<ModAiActionSnapshot>(),null,"Empty shortlist prevented native fallback");
        Decide(new object(),idle,60,candidates,1,"Reactive fighter memory leaked between controllers");
        // Metadata comes from the compiled native parser/reader and shipped clip
        // bytes. Eligibility remains controlled: provide the staff retreat here.
        var staff=native.Single(a=>a.Name=="StaffStepBack");
        var high=native.Single(a=>a.Name=="HighKick");
        var low=native.Single(a=>a.Name=="LowKick");
        Check(staff.Timing.NominalFrames==39&&high.Timing.NominalFrames==54&&low.Timing.NominalFrames==48,"Shipped native clip timing changed; review reactive fixture expectations");
        Decide(new object(),attacking,60,new[]{staff,high,low},0,"Real staff retreat metadata did not drive the shipped reactive brain");
        Decide(new object(),idle,60,new[]{staff,high,low},2,"Real kick timing/input metadata did not drive shortest-clip choice");
        Decide(new object(),attacking,60,new[]{high,low},1,"Real native kick fallback failed without retreat eligibility");
    }
}
