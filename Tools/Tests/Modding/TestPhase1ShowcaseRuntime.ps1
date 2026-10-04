$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$testRoot = Join-Path $root 'Temp/Phase1ShowcaseRuntime'
$modsRoot = Join-Path $testRoot 'Mods'
$modRoot = Join-Path $modsRoot 'example.phase1'
if (Test-Path -LiteralPath $testRoot) {
    $resolvedFixture = (Resolve-Path -LiteralPath $testRoot).Path
    $resolvedTemp = (Resolve-Path -LiteralPath (Join-Path $root 'Temp')).Path
    if (!$resolvedFixture.StartsWith($resolvedTemp + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to delete fixture outside Temp: $resolvedFixture"
    }
    Remove-Item -LiteralPath $resolvedFixture -Recurse -Force
}
New-Item -ItemType Directory -Force $modsRoot | Out-Null
$example = Join-Path $root 'Mods/example.phase1'
if (!(Test-Path -LiteralPath $example)) { $example = Join-Path $root 'ArchivedMods/example.phase1' }
Copy-Item -Recurse -Force $example $modRoot

$sources = @(
    'Assets/Scripts/Eclipse/Runtime/Modding/ModId.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/AssetId.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/DefinitionId.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/SemanticVersion.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/VersionRange.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModManifest.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModManifestReader.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModDiagnostics.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModCallbackDiagnostics.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModDiscovery.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/DependencyResolver.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/AssetProvider.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/AssetResolver.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/LooseModProvider.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModScripting.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModExtensions.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModRoundOutcomes.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModUiRuntime.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModProjectiles.cs','Assets/Scripts/Eclipse/Runtime/Modding/ModActors.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModArenaRuntime.cs','Assets/Scripts/Eclipse/Runtime/Modding/ModCameraRuntime.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModAudioRuntime.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModStoryEvents.cs','Assets/Scripts/Eclipse/Runtime/Modding/ModFightEntry.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModScriptingP1C.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModScriptingP1D.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModWarriorTemplates.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModSaveData.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/CoreContentImporter.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/CoreContentImporterP1C.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModContent.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModContentP1B.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModContentP1C.cs','Assets/Scripts/Eclipse/Runtime/Modding/ModContentShopPrices.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModContentP1D.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModContentP2.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModContentP3.cs','Assets/Scripts/Eclipse/Runtime/Modding/ModDojoButtons.cs','Assets/Scripts/Eclipse/Runtime/Modding/ModVisuals.cs','Assets/Scripts/Eclipse/Runtime/Modding/ModFx.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModSelection.cs',
    'Assets/Scripts/Eclipse/Runtime/Modding/ModLocalizationLoader.cs',
    'Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntime.cs',
    'Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntimeExtensions.cs',
    'Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntimeUi.cs',
    'Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntimeProjectiles.cs','Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntimeActors.cs',
    'Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntimeArena.cs',
    'Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntimeAudio.cs','Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntimeVisuals.cs',
    'Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntimeP1D.cs',
    'Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntimeShortForm.cs',
    'Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntimeSequence.cs',
    'Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntimeP2.cs'
    'Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntimeP3.cs','Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntimeFightEntry.cs'
    'Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntimeTrialRules.cs'
    'Assets/Scripts/Eclipse/Runtime/Modding/ModTrialRules.cs'
    'Assets/Scripts/Eclipse/Runtime/Modding/ModMovePerkLocks.cs'
    'Assets/Scripts/Eclipse/Runtime/Modding/ModRuleGroups.cs'
    'Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntimeRuleGroups.cs'
    'Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntimeUnderworld.cs'
) | ForEach-Object { Join-Path $root $_ }

$program = Join-Path $testRoot 'Program.cs'
$exe = Join-Path $testRoot 'Phase1ShowcaseRuntime.dll'
$adapterSource = Get-Content -Raw (Join-Path $root 'Assets/Scripts/Eclipse/Modding/LegacyContentAdapterP1D.cs')
$adapterBase = Get-Content -Raw (Join-Path $root 'Assets/Scripts/Eclipse/Modding/LegacyContentAdapter.cs')
$projectionMethods = foreach ($method in @('BuildLocationDocument', 'AppendLocationCurve', 'LocationAssetDirectory', 'LocationAssetLeaf', 'BuildMoveCondition', 'BuildMovePoint', 'AppendMovePresentation', 'AppendProjectileSpecification', 'BuildMoveNode', 'MoveTemplateNames', 'AppendEvents', 'AppendConditions', 'MoveEventElement', 'MoveRuntimeName')) {
    $match = [regex]::Match($adapterSource, '(?ms)^        private [^\r\n]*\b' + $method + '\(.*?^        \}')
    if (!$match.Success) { throw "Cannot extract production projection: $method" }
    $match.Value
}
$projectionMethods += [regex]::Match($adapterSource, '(?m)^        private static string F\(.*$').Value
$projectionMethods += [regex]::Match($adapterBase, '(?ms)^        private static void Set\(.*?^        \}').Value
foreach ($method in @('BuildRewardItemNode', 'LegacyItemName', 'LegacyPerkName', 'SetIfNotEmpty', 'RuleTargetName', 'BuildRuleNode', 'BuildRewardNode', 'BuildWarriorNode', 'BuildFightNode', 'AppendFightRules')) {
    $projectionMethods += [regex]::Match($adapterBase, '(?ms)^        private [^\r\n]*\b' + $method + '\(.*?^        \}').Value
}
$encounter = [regex]::Match($adapterBase, '(?ms)^        public XmlElement BuildEncounterNode\(.*?^        \}')
if (!$encounter.Success) { throw 'Cannot extract production encounter projection.' }
$projectionMethods += $encounter.Value
$projection = Join-Path $testRoot 'Projection.cs'
Set-Content -Encoding UTF8 $projection ('using System; using System.Collections.Generic; using System.Globalization; using System.Xml; using Eclipse.Modding; internal sealed class Projection {' +
    'private ModContentCatalog _content; public XmlElement Reward(ModContentCatalog catalog, RewardItemGrant grant) { _content = catalog; var reward = new RewardDefinition(DefinitionId.Parse("core:rewards/fixture"), new[] { grant }, Array.Empty<RewardChoiceDefinition>()); return BuildRewardItemNode(new XmlDocument(), reward, grant, null, 0); }' +
    'public XmlDocument Location(LocationDefinition value) => BuildLocationDocument(value);' +
    'public XmlElement Condition(ModMoveCondition value) => BuildMoveCondition(new XmlDocument(), value);' +
    'public XmlElement Move(MoveDefinition value) => BuildMoveNode(new XmlDocument(), "Move", value, value.Animation);' +
    'public XmlElement Warrior(ModContentCatalog catalog, WarriorDefinition value) { _content = catalog; return BuildWarriorNode(new XmlDocument(), value); }' +
    'public XmlElement Encounter(ModContentCatalog catalog, FightDefinition fight, ModEncounterPlan plan) { _content = catalog; return BuildEncounterNode(fight, plan); }' +
    ($projectionMethods -join "`n") + '}')
@'
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Eclipse.Modding;

internal sealed class EmptyCoreProvider : IAssetProvider
{
    public ModId Namespace => ModId.Parse("core");
    public bool TryDescribe(AssetId id, out AssetMetadata metadata)
    {
        metadata = null;
        if (id.Namespace != Namespace) return false;
        if (id.Path == "textures/locations/battlefield/battlefield_bg1.back_1")
        {
            metadata = new AssetMetadata(id, AssetKind.Sprite, AssetSourceKind.Core, string.Empty, -1, "core-background-fixture");
            return true;
        }
        if (id.Path != "gamedata/models/mdl_weapon_katana_ritual") return false;
        metadata = new AssetMetadata(id, AssetKind.Model, AssetSourceKind.Core, string.Empty, -1, "core-model-fixture");
        return true;
    }
}

internal static class Program
{
    private sealed class Fighter : IModFighterOperations
    {
        public double Charge;
        public bool TryChangeHealth(double amount, out string error) { error = "Unexpected health change"; return false; }
        public bool TryAddMagicCharge(double amount, out string error) { Charge += amount; error = string.Empty; return true; }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static void ImportMinimumCore(ModContentCatalog catalog)
    {
        CoreContentImporter.ImportForgeEconomicProfiles(catalog, new[] { "Simple" });
        var templates = new XmlDocument();
        templates.LoadXml("<Templates><Warrior Name='Default'/></Templates>");
        CoreContentImporter.ImportWarriorTemplates(catalog, templates.DocumentElement);
    }

    private static ModDescriptor Discover(string modsRoot)
    {
        ModDiscoveryResult discovery = ModDiscovery.DiscoverLoose(modsRoot);
        Assert(!discovery.HasErrors, "Discovery failed: " + string.Join(" | ", discovery.Diagnostics));
        DependencyResolutionResult resolution = DependencyResolver.Resolve(discovery.Mods,
            ModPlatformVersions.Core);
        Assert(!resolution.HasErrors, "Dependency resolution failed: " + string.Join(" | ", resolution.Diagnostics));
        Assert(resolution.OrderedMods.Count == 1 && resolution.OrderedMods[0].Id.Value == "example.phase1",
            "Did not resolve exactly the showcase mod.");
        return resolution.OrderedMods[0];
    }

    private static void Execute(ModDescriptor mod, AssetResolver assets, ModContentCatalog catalog,
        ModStateRuntime state, bool expectCommit)
    {
        IModScriptContext context = null;
        ModRegistrationTransaction registration = null;
        try
        {
            registration = catalog.BeginRegistration(mod);
            ModLocalizationLoader.Load(mod, assets, registration);
            var api = new ModApiFacade(mod, assets, registration, state, null);
            context = new MoonSharpScriptRuntime().CreateContext(mod, api);
            context.ExecuteEntrypoint();
            registration.Commit();
            if (!expectCommit) throw new Exception("Duplicate showcase transaction unexpectedly committed.");
            var save = new XmlDocument(); save.LoadXml("<Warrior/>");
            Assert(ModSaveData.RecordContext(save.DocumentElement, new[] { mod }, catalog, state), "Cannot bind fixture save.");
            state.Bind(save.DocumentElement, new[] { context });
            var perk = catalog.Perks.Single();
            var behavior = catalog.Behaviors.Single();
            var fighter = new Fighter();
            string error;
            Assert(((IModInteractiveBehaviorScriptContext)context).TryInvokeBehavior(perk.Behavior,
                ModEffectEvent.FightBegin, behavior.Parameters.ResolveValues(perk.InitialParameters),
                new Dictionary<string, string> { { "side", "player" }, { "source", "enchantment" } },
                fighter, out error), "Opening Focus callback failed: " + error);
            Assert(fighter.Charge == 0.25, "Opening Focus did not grant 25% magic charge.");
            Assert(behavior.Parameters.ResolveValues(new Dictionary<string, ModParameterValue>()).Count == 1,
                "Old empty perk parameters do not receive the new optional default.");
        }
        catch (ModContentException)
        {
            if (expectCommit) throw;
            state.RemoveDefinition(mod.Id);
        }
        finally
        {
            registration?.Dispose();
            context?.Dispose();
        }
    }

    private static void PlayerCharacterContracts(ModDescriptor mod)
    {
        var entrypoint=Path.Combine(mod.RootPath,mod.Manifest.Entrypoint);
        var original=File.ReadAllText(entrypoint);
        try
        {
            ModEncounterPlan preparedPlan=null;
            ModContentCatalog Run(string field, bool reject=false, string preparation=null, bool rejectPreparation=false)
            {
                File.WriteAllText(entrypoint,@"local sf2=require('sf2')
local hero=sf2.warriors.register{id='hero',level=3}
local foe=sf2.warriors.register{id='foe',level=2}
local zone=sf2.zones.register{id='test',file='Map1.1'}
local battle=sf2.battles.register{id='test',zone=zone,type=sf2.battles.STORY,x=0,y=0}
local fight=sf2.fights.register{id='test',battle=battle,warriors={foe},"+field+"}"+
                    (preparation==null?"":"\nsf2.modes.register{id='test',fights={fight},on_prepare=function()return {"+preparation+"} end}"));
                var content=new ModContentCatalog();
                var assets=new AssetResolver(new IAssetProvider[]{new EmptyCoreProvider(),new LooseModProvider(mod)});
                using(var registration=content.BeginRegistration(mod))
                using(var context=new MoonSharpScriptRuntime().CreateContext(mod,new ModApiFacade(mod,assets,registration,new ModStateRuntime(),null)))
                {
                    try { context.ExecuteEntrypoint(); registration.Commit(); Assert(!reject,"Invalid player character committed"); }
                    catch(ModScriptException) { Assert(reject,"Valid player character rejected"); }
                    if(preparation!=null)
                    {
                        var request=new ModModeRequest();
                        bool valid=((IModModePrepareScriptContext)context).TryPrepareMode(content.Modes.Single(),0,0,request,out var error);
                        Assert(valid!=rejectPreparation,"Unexpected player choice preparation result: "+error);
                        Assert(!request.IsPending&&(valid?request.Plan!=null:request.Plan==null),"Preparation retained an invalid request");
                        preparedPlan=request.Plan;
                    }
                }
                Assert(content.Fights.Count==(reject?0:1)&&content.Warriors.Count==(reject?0:2),"Player character registration was not atomic");
                return content;
            }
            var owned=Run("player_character=hero");var fight=owned.Fights.Single();
            var heroId=DefinitionId.Parse(mod.Id.Value+":warriors/hero");
            Assert(fight.PlayerCharacter==heroId,"Public warrior handle was not retained as the player");
            Assert(Run("").Fights.Single().PlayerCharacter==null&&Run("player_character=nil").Fights.Single().PlayerCharacter==null,"Omitted/nil player character changed the default contract");
            Run("player_character=battle",true);Run("player_character='hero'",true);Run("player_character=false",true);
            string Fingerprint(ModContentCatalog value)=>ModSaveData.ComputeContentSetFingerprint(new[]{mod},value);
            Assert(Fingerprint(owned)==Fingerprint(Run("player_character=hero")),"Declared player fingerprint is not deterministic");
            Assert(Fingerprint(owned)!=Fingerprint(Run("player_character=foe"))&&Fingerprint(owned)!=Fingerprint(Run("")),"Player choice is absent from content identity");
            Assert(Fingerprint(Run(""))==Fingerprint(Run("player_character=nil")),"Unset player changed previous content fingerprints");
            foreach(var copy in new[]{fight.WithDescription("changed"),fight.WithRounds(2),fight.WithRoundTime(30),fight.WithPresentation("arena","track"),fight.WithRules(Array.Empty<DefinitionId>(),false),fight.WithWarriors(fight.Warriors.ToArray())})
                Assert(copy.PlayerCharacter==heroId,"Fight copy discarded its declared player");
            Console.WriteLine("PASS: owned player character public Lua registration, wrong-handle/scalar rejection, transaction rollback, defaults, fingerprint and fight-copy contracts.");
            var modeCatalog=Run("player_character=foe",preparation:"player_character=hero,rules={},description='choice'");
            var chosen=preparedPlan;Assert(chosen.PlayerCharacter==heroId,"Prepared player choice lost its typed handle");
            var mode=modeCatalog.Modes.Single();var save=new XmlDocument();save.LoadXml("<Warrior/>");
            new ModModeProgress(save.DocumentElement,mode).SavePlan(chosen);
            Assert(save.SelectSingleNode("Warrior/EclipseModes/Mode/Encounter").Attributes["Version"].Value=="3","Player choice did not use the new save version");
            var clone=new XmlDocument();clone.LoadXml(save.OuterXml);
            var restored=new ModModeProgress(clone.DocumentElement,mode).ReadPlan();
            Assert(restored.PlayerCharacter==heroId&&restored.Rules.Count==0&&restored.Description=="choice","Player/rules/description did not survive plan reload");
            clone.SelectSingleNode("Warrior/EclipseModes/Mode/Encounter").Attributes["Version"].Value="2";
            string preserved=clone.OuterXml;bool rejectedOld=false;
            try{new ModModeProgress(clone.DocumentElement,mode).ReadPlan();}catch(ModContentException){rejectedOld=true;}
            Assert(rejectedOld&&clone.OuterXml==preserved,"Older save version silently interpreted a new player field");
            clone.LoadXml(save.OuterXml);var progress=new ModModeProgress(clone.DocumentElement,mode);
            progress.Enter();progress.Complete(mode,false);Assert(progress.ReadPlan()==null,"Resolved loss retained the previous chosen character plan");
            foreach(var input in new[]{"","player_character=nil","description='legacy'"})
            {
                var c=Run("",preparation:input);var old=new XmlDocument();old.LoadXml("<Warrior/>");
                var p=new ModModeProgress(old.DocumentElement,c.Modes.Single());p.SavePlan(preparedPlan);
                Assert(p.ReadPlan().PlayerCharacter==null,"Omitted/nil prepared player changed inheritance");
                Assert(old.SelectSingleNode("Warrior/EclipseModes/Mode/Encounter").Attributes["Version"].Value==(input.StartsWith("description")?"2":"1"),"Unchanged plan format was needlessly upgraded");
            }
            foreach(var input in new[]{"player_character=battle","player_character='hero'","player_character=false"})Run("",preparation:input,rejectPreparation:true);
            Console.WriteLine("PASS: prepared player choice public Lua validation, defaults, version-3 save/reload, version-1/2 compatibility, preserved rejection and loss consumption.");
        }
        finally { File.WriteAllText(entrypoint,original); }
    }

    public static int Main(string[] args)
    {
        string modsRoot = Path.GetFullPath(args[0]);
        ModDescriptor mod = Discover(modsRoot);
        var loose = new LooseModProvider(mod);
        var assets = new AssetResolver(new IAssetProvider[] { new EmptyCoreProvider(), loose });

        var catalog = new ModContentCatalog();
        ImportMinimumCore(catalog);
        int coreProfilesBefore = catalog.ForgeEconomicProfiles.Count;
        int coreTemplatesBefore = catalog.WarriorTemplates.Count;
        var state = new ModStateRuntime();

        Execute(mod, assets, catalog, state, true);

        Assert(catalog.TryGetItem(DefinitionId.Parse("example.phase1:items/consumable/phase_token"), out ItemDefinition item),
            "P1C item was not committed.");
        Assert(catalog.TryGetItemSet(DefinitionId.Parse("example.phase1:itemsets/phase_relics"), out ItemSetDefinition set),
            "P1C item set was not committed.");
        Assert(catalog.TryGetForgeRecipeFamily(DefinitionId.Parse("example.phase1:forge-recipes/showcase_simple"), out ForgeRecipeFamilyDefinition recipe),
            "P1C forge family was not committed.");
        Assert(catalog.Zones.Count == 1 && catalog.Battles.Count == 1 && catalog.Fights.Count == 1 &&
            catalog.Warriors.Count == 1 && catalog.FightRules.Count == 1 && catalog.Rewards.Count == 2,
            "P1A graph did not commit as one coherent slice.");
        Assert(catalog.Quests.Count == 2, "P1B quests did not commit.");
        Assert(catalog.Locations.Count == 1 && catalog.MoveTemplates.Count == 1 && catalog.Moves.Count == 1 &&
            catalog.MoveTriggers.Count == 1 && catalog.Tactics.Count == 1 && catalog.LocaleMetadata.Count == 1,
            "P1D definitions did not commit.");
        Assert(catalog.Behaviors.Count == 1 && catalog.Perks.Count == 1,
            "Behavior-backed perk did not commit.");
        var localeDocument = new XmlDocument(); localeDocument.Load(args[1]);
        var locale = catalog.LocaleMetadata.Single();
        foreach (XmlNode language in localeDocument.SelectNodes("/Localization/Languages/Language"))
            Assert(locale.Name != language.Attributes["Name"].Value &&
                !string.Equals(locale.Locale, language.Attributes["Locale"].Value, StringComparison.OrdinalIgnoreCase),
                "Showcase locale collides with a shipped language.");
        Assert(catalog.Zones.Single().FileName == "Map1.1", "Showcase zone has no recovered map art.");
        var gameplay = catalog.Locations.Single().Layers.Single(layer => layer.Type == 2);
        Assert(gameplay.Fighters != null && gameplay.Fighters.PlayerX < gameplay.Fighters.EnemyX,
            "Arena has no distinct fighter spawn positions on its gameplay layer.");
        var move = catalog.Moves.Single();
        Assert(catalog.Tactics.Single().AnimationWeights.Count == 0 && catalog.Tactics.Single().SafeAttack == null,
            "Showcase tactic replaces the inherited Standard decision weights.");
        Assert(!move.Intervals.Any(i => i.Name == "SelfUninterrupt"), "Opening move has an unbounded self-interruption lock.");
        Assert(move.Events.Any(e => e.Kind == ModMoveEventKind.RoundStageStart && e.Name == "Fight"),
            "Showcase move has no deterministic activation event.");
        Assert(move.Conditions.Any(c => c.Kind == ModMoveConditionKind.Perk && c.Name == catalog.Perks.Single().Id.ToString()),
            "Showcase opening move is not scoped to its perk owner.");
        var projection = new Projection();
        var fightRewards = catalog.Fights.Single().Rewards;
        Assert(fightRewards.Count == 2 && catalog.TryGetReward(fightRewards[0], out var lossReward) && lossReward.Items.Count == 0,
            "One-round showcase must provide the recovered zero-win reward slot.");
        Assert(catalog.TryGetReward(fightRewards[1], out var winReward) && winReward.Items.Count == 1,
            "Winning the one-round showcase cannot resolve reward slot 1.");
        var rewardXml = projection.Reward(catalog, winReward.Items.Single());
        Assert(rewardXml.GetAttribute("Drop") == "1" && rewardXml.GetAttribute("Name") == item.Id.ToString(),
            "Token grant is not marked for end-of-fight item presentation.");
        var arenaXml = projection.Location(catalog.Locations.Single());
        var background = arenaXml.SelectSingleNode("/Root/Layer[@Path='core:textures/locations/battlefield']/Image");
        Assert(background != null && background.Attributes["X"].Value == "0" && background.Attributes["Y"].Value == "0",
            "Arena backdrop is not centered in the location artwork coordinate system.");
        var spawns = arenaXml.SelectSingleNode("/Root/Layer[@Type='2']/ModelsViewer");
        Assert(spawns != null && spawns.Attributes["PlayerPositionX"].Value == "868" &&
            spawns.Attributes["EnemyPositionX"].Value == "1068", "Location projection lost recovered spawn attributes.");
        var perkXml = projection.Condition(move.Conditions.Single(c => c.Kind == ModMoveConditionKind.Perk));
        Assert(perkXml.Name == "Perk" && perkXml.GetAttribute("Name") == catalog.Perks.Single().Id.ToString(),
            "Move projection does not match the recovered ConditionPerk predicate.");
        string SpawnFingerprint(float enemyX)
        {
            var spawnCatalog = new ModContentCatalog();
            using (var registration = spawnCatalog.BeginRegistration(mod))
            {
                registration.RegisterLocation("spawn_hash", "0x000000", 200, 80, 0, 1936, 512, 1936, 0, 0,
                    default(AssetId), new[] { new LocationLayerDefinition(2, 1, false, null,
                        new LocationFighterPositions(868, -94, enemyX, -94)) });
                registration.Commit();
            }
            return ModSaveData.ComputeContentSetFingerprint(new[] { mod }, spawnCatalog);
        }
        Assert(SpawnFingerprint(1068) == SpawnFingerprint(1068) && SpawnFingerprint(1068) != SpawnFingerprint(1168),
            "Content identity does not deterministically include fighter spawn positions.");
        Assert(catalog.ForgeEconomicProfiles.Count == coreProfilesBefore && catalog.WarriorTemplates.Count == coreTemplatesBefore,
            "Showcase mutated imported core registries.");

        int zones = catalog.Zones.Count, battles = catalog.Battles.Count, fights = catalog.Fights.Count;
        int quests = catalog.Quests.Count, items = catalog.NonEquipmentItems.Count, locations = catalog.Locations.Count;
        var duplicateState = new ModStateRuntime();
        Execute(mod, assets, catalog, duplicateState, false);
        Assert(catalog.Zones.Count == zones && catalog.Battles.Count == battles && catalog.Fights.Count == fights &&
            catalog.Quests.Count == quests && catalog.NonEquipmentItems.Count == items && catalog.Locations.Count == locations,
            "Failed duplicate transaction partially mutated committed content.");

        Assert(catalog.ForgeEconomicProfiles.Count == coreProfilesBefore && catalog.WarriorTemplates.Count == coreTemplatesBefore,
            "Rollback/teardown changed base-owned core fixtures.");

        PlayerCharacterContracts(mod);
        Console.WriteLine("Phase 1 showcase MoonSharp runtime PASS: discovery, localization load, public Lua execution, transactional commit, duplicate rollback, context teardown, core fixtures unchanged.");
        return 0;
    }
}
'@ | Set-Content -Encoding UTF8 $program

$moon = Join-Path $root 'Library/ScriptAssemblies/MoonSharp.Interpreter.dll'
$project = Join-Path $testRoot 'Phase1ShowcaseRuntime.csproj'
$compileItems = @($program, $projection) + $sources
$compileXml = ($compileItems | ForEach-Object { '    <Compile Include="' + [Security.SecurityElement]::Escape($_) + '" />' }) -join "`n"
$moonXml = [Security.SecurityElement]::Escape($moon)
@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <Nullable>disable</Nullable>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>
  <ItemGroup>
$compileXml
    <Reference Include="MoonSharp.Interpreter">
      <HintPath>$moonXml</HintPath>
      <Private>true</Private>
    </Reference>
  </ItemGroup>
</Project>
"@ | Set-Content -Encoding UTF8 $project

dotnet build $project -nologo --verbosity quiet
if ($LASTEXITCODE -ne 0) { throw "Showcase runtime fixture compile failed: $LASTEXITCODE" }
$exe = Join-Path $testRoot 'bin/Debug/net10.0/Phase1ShowcaseRuntime.dll'
dotnet $exe $modsRoot (Join-Path $root 'Assets/vanillaXml/localization.xml')
if ($LASTEXITCODE -ne 0) { throw "Showcase runtime fixture failed: $LASTEXITCODE" }
