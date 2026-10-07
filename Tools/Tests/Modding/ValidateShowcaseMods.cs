using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Eclipse.Modding;

// Loads the showcase mods (Final Blow, Shadow Clones, Umbra) through the real Lua
// runtime and drives their combat behaviors with controlled fighter hosts. Native
// rendering, camera framing, AI and contact are not exercised here.
public static class ValidateShowcaseMods
{
    sealed class Core : IAssetProvider
    {
        public ModId Namespace => ModId.Parse("core");
        public bool TryDescribe(AssetId id, out AssetMetadata metadata)
        {
            AssetKind kind = id.Path.StartsWith("gamedata/models/") ? AssetKind.Model
                : id.Path.StartsWith("gamedata/animations/") ? AssetKind.Binary : AssetKind.Sprite;
            metadata = new AssetMetadata(id, kind, AssetSourceKind.Core, "", -1, "fixture");
            return true;
        }
    }

    sealed class Marker : IModArenaMarker
    {
        public bool IsActive { get; private set; } = true;
        public ModUiColor Color;
        public void SetColor(ModUiColor color) { Color = color; }
        public void Dispose() { IsActive = false; }
    }

    sealed class Actor : IModActor
    {
        public string Id; public double X; public bool Removed;
        public bool TrySnapshot(out ModActorSnapshot snapshot, out string error)
        {
            error = "";
            snapshot = new ModActorSnapshot(Id, "fixture", "player", "opponent", new ModFighterSnapshot(0.4, 0.4, 1, X, 0, 0), 0, 600);
            return true;
        }
        public bool TryMoveBy(double x, double y, double z, out string error) { error = ""; return true; }
        public bool TryChangeHealth(double amount, out string error) { error = ""; return true; }
        public bool TryPlayMove(DefinitionId move, Action<bool, string> complete, out string error) { error = ""; return true; }
        public bool TrySetTarget(string mainTarget, IModActor actorTarget, out string error) { error = ""; return true; }
        public bool TryRemove(out string error) { error = ""; Removed = true; return true; }
    }

    sealed class Camera : IModCameraControl
    {
        public bool IsActive { get; private set; } = true;
        public ModCameraSettings Last;
        public bool TrySet(ModCameraSettings settings, out string error) { error = ""; Last = settings; return true; }
        public void Dispose() { IsActive = false; }
    }

    // A controlled host implementing the fighter surfaces these mods use.
    sealed class Fighter : IModFighterOperations, IModCombatSnapshotSource, IModFighterFlags, IModFighterButtons,
        IModAnimationLifecycleSource, IModFighterActors, IModFighterCamera, IModFighterTargets, IModFighterEffects,
        IModFighterForms, IModFighterRegions, IModBehaviorInstanceSource, IModIncomingHitSource, IModFighterMotion, IModCombatActivitySource
    {
        public int Bars = 3; public double Moved;
        public ModIncomingHit IncomingHit { get; set; }
        public bool TryMoveBy(double x, double y, double z, out string error) { error = ""; Moved += x; return true; }
        // Applies a hit of `damage` single-bar units through on_damage_resolving, as the native health path does.
        public double Hit(Action resolve, double damage)
        {
            double pending = damage; IncomingHit = new ModIncomingHit(() => pending, v => pending = v);
            resolve(); IncomingHit = null; Health = Math.Max(0, Health - pending / Bars); return pending;
        }
        public XmlNode SavedInstance { get; } = NewInstance();
        static XmlNode NewInstance() { var document = new XmlDocument(); var node = document.CreateElement("BattleRuleInstance"); document.AppendChild(node); return node; }
        public int Frame; public double Health = 1, X, OpponentX = 300; public int Facing = 1;
        public readonly HashSet<string> Flags = new HashSet<string>();
        public readonly List<(string control, int frames)> Cooldowns = new List<(string, int)>();
        public readonly List<string> Visible = new List<string>();
        public readonly List<(DefinitionId definition, double x, double y)> Spawns = new List<(DefinitionId, double, double)>();
        public readonly List<Actor> Live = new List<Actor>();
        public readonly List<DefinitionId> Forms = new List<DefinitionId>();
        public readonly List<string> Shields = new List<string>();
        public readonly List<Marker> Markers = new List<Marker>();
        public Camera Camera;
        public ModAnimationLifecycleEvent AnimationEvent { get; set; }
        public ModCombatActivityEvent ActivityEvent { get; set; }
        public Fighter OpponentHost;
        public double LostHealth;
        public bool Inside = true;

        public double Health_ => Health;
        double IModFighterTargets.Health => Health;
        public IModFighterOperations Opponent => OpponentHost;
        public bool TryChangeHealth(double amount, out string error) { error = ""; LostHealth -= amount; Health += amount; return true; }
        public bool TryAddMagicCharge(double amount, out string error) { error = ""; return true; }
        public ModCombatSnapshot CaptureCombatSnapshot() => new ModCombatSnapshot(
            new ModFighterSnapshot(Health, 1, Bars, X, 0, 0, new ModAnimationSnapshot("Stance", "none", Facing, Array.Empty<ModAnimationIntervalSnapshot>())),
            new ModFighterSnapshot(1, 1, 1, OpponentX, 0, 0, OpponentAnimation), Frame, true);
        public ModAnimationSnapshot OpponentAnimation;
        public bool TrySetFlag(object owner, string behavior, string name, out string error) { error = ""; Flags.Add(name); return true; }
        public bool TryClearFlag(object owner, string behavior, string name, out string error) { error = ""; Flags.Remove(name); return true; }
        public bool TryHasFlag(object owner, string behavior, string name, out bool exists, out string error)
        { error = ""; exists = Flags.Contains(name); return true; }
        public bool TrySetButtonCooldown(string control, int frames, out string error) { error = ""; Cooldowns.Add((control, frames)); return true; }
        public bool TrySetControlVisible(string control, bool visible, out string error) { error = ""; if (visible) Visible.Add(control); return true; }
        public bool TrySpawnActor(ModId owner, DefinitionId definition, double x, double y, double z, Action<string, string> complete, out string error)
        {
            error = ""; Spawns.Add((definition, x, y));
            var actor = new Actor { Id = "a" + Spawns.Count, X = X + x }; Live.Add(actor);
            complete?.Invoke(actor.Id, null); return true;
        }
        public bool TryGetActors(ModId owner, out IReadOnlyList<IModActor> actors, out string error)
        { error = ""; actors = Live.Where(a => !a.Removed).Cast<IModActor>().ToList(); return true; }
        public bool TryGetActorEvents(ModId owner, out IReadOnlyList<ModActorEvent> events, out string error)
        { error = ""; events = Array.Empty<ModActorEvent>(); return true; }
        public bool TryAcquireCamera(ModId owner, ModCameraSettings settings, out IModCameraControl camera, out string error)
        { error = ""; Camera = new Camera { Last = settings }; camera = Camera; return true; }
        public bool TrySetDamageShield(object key, double fraction, int frames, out string error) { error = ""; Shields.Add(key + "=" + fraction); return true; }
        public bool TryRemoveDamageShield(object key, out string error) { error = ""; return true; }
        public bool TryChangeForm(DefinitionId character, Action<bool, string> complete, out string error) { error = ""; Forms.Add(character); return true; }
        public bool TryOverlapRect(ModArenaRect rect, out bool overlaps, out string error) { error = ""; overlaps = Inside; return true; }
        public bool TryMarkRect(ModArenaRect rect, ModUiColor color, out IModArenaMarker marker, out string error)
        { error = ""; var created = new Marker { Color = color }; Markers.Add(created); marker = created; return true; }
    }

    static int checks;
    static void Check(bool value, string message) { checks++; if (!value) throw new Exception(message); }

    public static void Main(string[] args)
    {
        string modsRoot = args[0], stagesPath = args[1];
        var fired = new List<(string name, float? x)>();
        ModFxScriptTriggers.Fire = (definition, x) => { fired.Add((definition.Name, x)); return true; };
        var items = new XmlDocument(); items.Load(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(stagesPath), "list.xml"));
        foreach (string name in new[] { "final-blow", "shadow-clones", "umbra", "home-run", "mirror", "combo-hype", "gauntlet" })
        {
            var mod = ModDiscovery.DiscoverLoose(modsRoot).Mods.Single(m => m.Id.Value == name);
            var catalog = new ModContentCatalog(); var state = new ModStateRuntime();
            var stages = new XmlDocument(); stages.Load(stagesPath);
            CoreContentImporter.ImportWarriorTemplates(catalog, stages.SelectSingleNode("Stages/Warriors/Templates"));
            CoreContentImporter.ImportWeapons(catalog, items.SelectNodes("/List/Items/Item").Cast<XmlNode>(), null);
            var assets = new AssetResolver(new IAssetProvider[] { new Core(), new LooseModProvider(mod) });
            var views = new List<ModUiSurface>(); var logs = new List<ModLogEntry>();
            var story = new ModStoryEvents((owner, message) => throw new Exception(owner + ": " + message));
            using (var tx = catalog.BeginRegistration(mod))
            using (var script = new MoonSharpScriptRuntime(views.Add, null, null, story).CreateContext(mod, new ModApiFacade(mod, assets, tx, state, logs.Add)))
            {
                ModLocalizationLoader.Load(mod, assets, tx); script.ExecuteEntrypoint(); tx.Commit(); catalog.Freeze();
                var interactive = (IModInteractiveBehaviorScriptContext)script;
                var context = new Dictionary<string, string> { { "round", "1" }, { "source", "rule" }, { "fight_id", name } };
                Action<DefinitionId, ModEffectEvent, Fighter> invoke = (behavior, kind, fighter) =>
                    Check(interactive.TryInvokeBehavior(behavior, kind, null, context, fighter, out var error), name + " " + kind + ": " + error);

                if (name == "final-blow")
                {
                    Check(catalog.Effects.Count == 3 && catalog.SettingToggles.Count == 3, "Final Blow effects/switches changed");
                    var ko = catalog.Effects.Single(e => e.Trigger == ModFxTrigger.Ko);
                    Check(ko.Number("zoom") > 1.5f && ko.Number("time_scale") < 0.2f && ko.Setting != null, "Knockout camera lost its zoom or slow motion");
                    Check(catalog.Effects.All(e => e.Kind == ModFxKind.Screen && e.Number("zoom") > 1f), "Every Final Blow grade should push the camera");
                }
                else if (name == "shadow-clones")
                {
                    Check(catalog.Actors.Count == 3 && catalog.Actors.All(a => !a.OpposingTeam), "Three allied clones expected");
                    var mask = DefinitionId.Parse("shadow-clones:behaviors/mask");
                    var player = new Fighter { X = 100 };
                    invoke(mask, ModEffectEvent.RoundBegin, player);
                    player.Frame = 1; invoke(mask, ModEffectEvent.Tick, player);
                    Check(player.Visible.Contains("raid_charge"), "RaidCharge button not shown");
                    Check(player.Flags.Contains("shadow-clones:behaviors/mask:ready"), "Ready flag not set: " + string.Join(",", player.Flags));
                    var move = catalog.Moves.Single(m => m.Id.ToString() == "shadow-clones:moves/summon");
                    player.Facing = -1;
                    player.AnimationEvent = new ModAnimationLifecycleEvent(ModEffectEvent.AnimationStart, move.Id.ToString(), "self", 2);
                    invoke(mask, ModEffectEvent.AnimationStart, player);
                    Check(!player.Flags.Contains("shadow-clones:behaviors/mask:ready"), "Cast did not consume the ready flag");
                    Check(player.Cooldowns.Single() == ("raid_charge", 1200), "Cooldown ring not started");
                    Check(fired.Count == 1 && fired[0].name == "shadow-clones.shadow_rise" && fired[0].x == 100f, "Summon flash not fired on the caster");
                    for (int frame = 2; frame <= 20; frame++) { player.Frame = frame; invoke(mask, ModEffectEvent.Tick, player); }
                    Check(player.Spawns.Count == 3 && player.Spawns.All(s => s.x >= 0), "Clones should rise behind a left-facing caster: " +
                        string.Join(";", player.Spawns.Select(s => s.x)));
                    Check(player.Camera != null && player.Camera.IsActive && player.Camera.Last.CenterX.HasValue, "Clone framing did not take the camera");
                    foreach (var actor in player.Live) actor.Removed = true;
                    player.Frame++; invoke(mask, ModEffectEvent.Tick, player);
                    Check(!player.Camera.IsActive, "Camera kept after the clones left");
                    for (int frame = 0; frame < 1200; frame++) { player.Frame++; invoke(mask, ModEffectEvent.Tick, player); }
                    Check(player.Flags.Contains("shadow-clones:behaviors/mask:ready"), "Ready flag not restored after cooldown");
                    invoke(mask, ModEffectEvent.RoundEnd, player);
                }
                else if (name == "home-run")
                {
                    var swing = catalog.Moves.Single(m => m.Id.ToString() == "home-run:moves/home_run");
                    var hammer = DefinitionId.Parse("home-run:behaviors/hammer");
                    var player = new Fighter { X = 100, OpponentX = 300 };
                    invoke(hammer, ModEffectEvent.RoundBegin, player);
                    player.Frame = 1; invoke(hammer, ModEffectEvent.Tick, player);
                    Check(player.Visible.Contains("raid_charge") && player.Flags.Contains("home-run:behaviors/hammer:ready"), "Home run not armed");
                    fired.Clear();
                    player.AnimationEvent = new ModAnimationLifecycleEvent(ModEffectEvent.AnimationStart, swing.Id.ToString(), "self", 2);
                    invoke(hammer, ModEffectEvent.AnimationStart, player);
                    Check(!player.Flags.Contains("home-run:behaviors/hammer:ready") && player.Cooldowns.Single().control == "raid_charge" && player.Cooldowns.Single().frames > 0 &&
                        fired.Single().name == "home-run.wind_up", "Swing did not start its cooldown and wind-up: " + string.Join(",", fired.Select(f => f.name)) + " cd=" + player.Cooldowns.Count + " flags=" + string.Join(",", player.Flags));
                    // Contact: an outgoing hit whose source is our swing.
                    player.IncomingHit = new ModIncomingHit(() => 0.1, v => { }, false, false, new ModHitEvent(false, true, false, false, false),
                        new ModAttackSource("fighter", "Shadow", swing.Id.ToString(), 300, -150, 0));
                    invoke(hammer, ModEffectEvent.PostHit, player);
                    player.IncomingHit = null;
                    Check(fired.Last().name == "home-run.contact" && fired.Last().x == 300f && views.Last().Read("title").Text == "HOME RUN!",
                        "Contact did not stop time or raise the banner");
                    // They fly 1650 units to the right, then lie still.
                    for (int frame = 0; frame < 60; frame++) { player.Frame++; player.OpponentX = Math.Min(1950, player.OpponentX + 60); invoke(hammer, ModEffectEvent.Tick, player); }
                    Check(views.Last().Read("distance").Text == "10.0 m", "Flight distance wrong: " + views.Last().Read("distance").Text);
                    for (int frame = 0; frame < 600; frame++) { player.Frame++; invoke(hammer, ModEffectEvent.Tick, player); }
                    Check(views.Last().IsClosed && player.Flags.Contains("home-run:behaviors/hammer:ready"), "Banner or cooldown did not clear");
                    invoke(hammer, ModEffectEvent.RoundEnd, player);
                }
                else if (name == "combo-hype")
                {
                    var hype = DefinitionId.Parse("combo-hype:behaviors/hype");
                    var player = new Fighter { X = 100, OpponentX = 260 };
                    invoke(hype, ModEffectEvent.RoundBegin, player);
                    fired.Clear();
                    Action<int, int> combo = (count, last) =>
                    {
                        player.ActivityEvent = ModCombatActivityEvent.ComboChange(count, last);
                        invoke(hype, ModEffectEvent.ComboChanged, player);
                    };
                    combo(2, 0);
                    Check(fired.Count == 0, "A two-hit combo should not hype");
                    for (int count = 3; count <= 12; count++) combo(count, 0);
                    var tiers = fired.Where(f => f.name.StartsWith("combo-hype.tier_")).Select(f => f.name).Distinct().ToList();
                    var stings = fired.Where(f => f.name.StartsWith("combo-hype.sting_")).Select(f => f.name).ToList();
                    Check(tiers.SequenceEqual(new[] { "combo-hype.tier_1", "combo-hype.tier_2", "combo-hype.tier_3", "combo-hype.tier_4" }),
                        "Tiers out of order: " + string.Join(",", tiers));
                    Check(stings.SequenceEqual(new[] { "combo-hype.sting_2", "combo-hype.sting_3", "combo-hype.sting_4" }), "Each tier-up should sting once");
                    Check(fired.Where(f => f.name.StartsWith("combo-hype.tier_")).All(f => f.x == 260f), "Hype should frame the opponent");
                    var hud = views.Last();
                    Check(hud.Read("count").Text == "12 HITS" && hud.Read("word").Text == "UNSTOPPABLE", "Counter wrong: " + hud.Read("count").Text);
                    combo(0, 12);
                    Check(fired.Last().name == "combo-hype.drain" && hud.Read("word").Text == "UNSTOPPABLE!", "Combo break did not drain");
                    for (int frame = 0; frame < 120; frame++) { player.Frame++; invoke(hype, ModEffectEvent.Tick, player); }
                    Check(hud.IsClosed, "Final count stayed up");
                    combo(4, 0);
                    Check(fired.Last().name == "combo-hype.tier_1" && !views.Last().IsClosed, "A new combo did not restart the hype");
                    invoke(hype, ModEffectEvent.RoundEnd, player);
                    Check(views.Last().IsClosed, "Hype counter leaked past the round");
                }
                else if (name == "gauntlet")
                {
                    var gauntlet = DefinitionId.Parse("gauntlet:behaviors/gauntlet");
                    var player = new Fighter { X = 300, Health = 0.5 };
                    var foe = new Fighter { X = 600, OpponentX = 300, OpponentHost = player, Bars = 5 };
                    Action tick = () => { foe.Frame++; invoke(gauntlet, ModEffectEvent.Tick, foe); };
                    Action resolve = () => invoke(gauntlet, ModEffectEvent.DamageResolving, foe);
                    fired.Clear();
                    invoke(gauntlet, ModEffectEvent.RoundBegin, foe);
                    tick();
                    Check(fired.Single().name == "gauntlet.opening" && views.Last().Read("title").Text == "THE GAUNTLET", "Opening missing");
                    for (int frame = 0; frame < 160; frame++) tick();
                    for (int wave = 2; wave <= 5; wave++)
                    {
                        foe.Hit(resolve, 5);
                        Check(Math.Abs(foe.Health - (6 - wave) / 5.0) < 1e-6, "Wave " + wave + " was skipped: " + foe.Health);
                        tick();
                        Check(foe.Forms.Count == wave - 1 && foe.Forms.Last().ToString() == "gauntlet:warriors/wave_" + wave &&
                            views.Last().Read("title").Text == "WAVE " + wave + " / 5" && fired.Last().name == "gauntlet.fallen",
                            "Wave " + wave + " did not enter");
                        Check(foe.Hit(resolve, 1) == 0, "Hit landed while wave " + wave + " entered");
                        for (int frame = 0; frame < 100; frame++) tick();
                    }
                    Check(Math.Abs(player.Health - (0.5 + 4 * 0.12)) < 1e-9, "Breather heal wrong: " + player.Health);
                    Check(Math.Abs(foe.Moved - 4 * 16 * 18) < 1e-9, "Newcomers did not land back at distance: " + foe.Moved);
                    foe.Hit(resolve, 5);
                    Check(foe.Health == 0, "The last wave should be beatable");
                    invoke(gauntlet, ModEffectEvent.RoundEnd, foe);
                    Check(views.All(v => v.IsClosed), "Gauntlet banner leaked past the round");
                }
                else if (name == "mirror")
                {
                    Check(catalog.Modes.Count == 1 && catalog.Fights.Count == 1, "Mirror fight/mode missing");
                    var watcher = DefinitionId.Parse("mirror:behaviors/watcher");
                    var ai = (IModAiScriptContext)script;
                    var mirror = new Fighter { X = 600, OpponentX = 300 };
                    invoke(watcher, ModEffectEvent.FightBegin, mirror);
                    invoke(watcher, ModEffectEvent.RoundBegin, mirror);
                    fired.Clear();
                    mirror.Frame = 1; invoke(watcher, ModEffectEvent.Tick, mirror);
                    var hud = views.Last();
                    Check(fired.Single().name == "mirror.awaken" && hud.Read("learned").Text == "IT IS WATCHING YOU", "Mirror did not wake");
                    var none = Array.Empty<ModAnimationIntervalSnapshot>();
                    var swinging = new[] { new ModAnimationIntervalSnapshot("", "attack") };
                    var slash = new ModAiActionSnapshot("KatanaHighSlash", "attack", 110);
                    var back = new ModAiActionSnapshot("KatanaStepBack", "move", 100, null, new[] { new ModAiActionInput("Back", "tap") });
                    var actions = new[] { back, slash };
                    object brain = new object();
                    Func<int, ModAnimationSnapshot, int?> decide = (frame, theirs) =>
                    {
                        var snapshot = new ModCombatSnapshot(new ModFighterSnapshot(1, 1, 1, 600, 0, 0), new ModFighterSnapshot(1, 1, 1, 300, 0, 0, theirs), frame, true);
                        Check(ai.TryDecideAi("mirror:tactics/mirror", brain, snapshot, actions, out var selected, out var error), error);
                        return selected;
                    };
                    // Walking is not learned; an attack is, and is echoed after a beat.
                    mirror.OpponentAnimation = new ModAnimationSnapshot("WalkForward", "move", 1, none);
                    mirror.AnimationEvent = new ModAnimationLifecycleEvent(ModEffectEvent.AnimationStart, "WalkForward", "opponent", 60);
                    invoke(watcher, ModEffectEvent.AnimationStart, mirror);
                    for (int use = 0; use < 3; use++)
                    {
                        int frame = 120 + use * 300;
                        mirror.OpponentAnimation = new ModAnimationSnapshot("KatanaHighSlash", "attack", 1, swinging);
                        mirror.AnimationEvent = new ModAnimationLifecycleEvent(ModEffectEvent.AnimationStart, "KatanaHighSlash", "opponent", frame);
                        invoke(watcher, ModEffectEvent.AnimationStart, mirror);
                        if (use < 2)
                        {
                            Check(decide(frame + 6, null) == null, "Echoed before its reaction delay");
                            Check(decide(frame + 36, null) == 1, "Did not echo the player's attack");
                            mirror.Frame++; invoke(watcher, ModEffectEvent.Tick, mirror);
                            Check(hud.Read("flash").Text == "ECHO: KATANA HIGH SLASH" && fired.Last().name == "mirror.echo", "Echo not shown: " + hud.Read("flash").Text);
                            Check(decide(frame + 60, null) == null, "Echoed the same attack twice");
                        }
                    }
                    mirror.Frame++; invoke(watcher, ModEffectEvent.Tick, mirror);
                    Check(hud.Read("learned").Text == "IT HAS LEARNED 1 MOVE", "Learned count wrong: " + hud.Read("learned").Text);
                    Check(decide(800, new ModAnimationSnapshot("KatanaHighSlash", "attack", 1, swinging)) == 0, "Did not read a thrice-used attack");
                    mirror.Frame++; invoke(watcher, ModEffectEvent.Tick, mirror);
                    Check(hud.Read("flash").Text == "READ YOU: KATANA HIGH SLASH" && fired.Last().name == "mirror.read", "Read not shown");
                    invoke(watcher, ModEffectEvent.RoundEnd, mirror);
                    Check(hud.IsClosed, "Mirror HUD leaked past the round");
                }
                else
                {
                    Check(catalog.Fights.Count == 1 && catalog.Actors.Count == 2, "Umbra fight/minions missing");
                    Check(story.FightEntries.Contains(DefinitionId.Parse("umbra:fights/umbra")), "Umbra prologue is not bound to its fight entry");
                    var boss = DefinitionId.Parse("umbra:behaviors/boss");
                    var player = new Fighter { X = 300 };
                    var umbra = new Fighter { X = 600, OpponentX = 300, OpponentHost = player, Bars = 6 };
                    Action tick = () => { umbra.Frame++; invoke(boss, ModEffectEvent.Tick, umbra); };
                    Action resolve = () => invoke(boss, ModEffectEvent.DamageResolving, umbra);
                    fired.Clear();
                    invoke(boss, ModEffectEvent.RoundBegin, umbra);
                    tick();
                    Check(fired.Any(f => f.name == "umbra.intro") && views.Count == 1, "Intro flash/banner missing on the opening frame");
                    for (int frame = 0; frame < 40; frame++) tick();
                    // One enormous hit stops exactly at the phase II threshold instead of skipping it.
                    umbra.Hit(resolve, 6);
                    Check(Math.Abs(umbra.Health - 2.0 / 3) < 1e-6, "Phase gate let a hit pass two thirds: " + umbra.Health);
                    tick();
                    Check(umbra.Forms.Single().ToString() == "umbra:warriors/umbra_greatsword" && umbra.Spawns.Count == 2 &&
                        umbra.Shields.Count == 1 && fired.Last().name == "umbra.phase_shift", "Phase II transition incomplete");
                    // Untouchable while transforming; the shockwave shoves the player away.
                    Check(umbra.Hit(resolve, 1) == 0 && Math.Abs(umbra.Health - 2.0 / 3) < 1e-6, "Hit landed during the transformation");
                    for (int frame = 0; frame < 150; frame++) tick();
                    Check(Math.Abs(player.Moved + 24 * 14) < 1e-9, "Shockwave did not push the player away from Umbra: " + player.Moved);
                    Check(umbra.Forms.Count == 1, "Phase II repeated");
                    umbra.Hit(resolve, 6);
                    Check(Math.Abs(umbra.Health - 1.0 / 3) < 1e-6, "Phase gate let a hit pass one third: " + umbra.Health);
                    tick();
                    Check(umbra.Forms.Count == 2 && umbra.Forms[1].ToString() == "umbra:warriors/umbra_storm" && fired.Any(f => f.name == "umbra.eclipse"),
                        "Phase III transition incomplete");
                    for (int frame = 0; frame < 150; frame++) tick();
                    Check(!fired.Any(f => f.name == "umbra.lightning"), "Lightning struck during the transformation");
                    for (int frame = 0; frame < 400; frame++) tick();
                    var bolts = fired.Where(f => f.name == "umbra.lightning").ToList();
                    Check(bolts.Count >= 2 && bolts.All(b => b.x == 300f), "Lightning did not strike the player's column");
                    Check(Math.Abs(player.LostHealth - 0.06 * bolts.Count) < 1e-9, "Lightning damage mismatch: " + player.LostHealth);
                    Check(umbra.Markers.Count == bolts.Count + (umbra.Markers.Last().IsActive ? 1 : 0), "Warning columns and strikes out of step");
                    // In the last phase damage is no longer capped.
                    umbra.Hit(resolve, 6);
                    Check(umbra.Health == 0, "Final phase damage was capped");
                    invoke(boss, ModEffectEvent.RoundEnd, umbra);
                    Check(umbra.Markers.All(m => !m.IsActive) && views.All(v => v.IsClosed), "Boss art or banner leaked past the round");
                }
                Check(logs.All(l => l.Level != ModLogLevel.Error && l.Level != ModLogLevel.Warning),
                    name + " logged: " + string.Join(" | ", logs.Where(l => l.Level != ModLogLevel.Info).Select(l => l.Message)));
            }
        }
        Console.WriteLine("PASS: " + checks + " showcase mod checks (Final Blow, Shadow Clones, Umbra, Home Run, The Mirror, Combo Hype, The Gauntlet). Native rendering, camera, AI and contact not exercised.");
    }
}
