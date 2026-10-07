$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$parameterSource=Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/ModelParameters.cs')
$animationSource=Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/AnimationData.cs')
$rulesSource=Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/RulesInspector.cs')
$fields=[regex]::Match($parameterSource,'(?ms)^\tpublic List<GroupModel> GroupModels.*?^\tpublic int WarriorPower;').Value
$fields += [regex]::Match($parameterSource,'(?ms)^\tpublic int ShieldTotal;.*?^\tpublic bool HasShieldTotalOverride;').Value
$fields += [regex]::Match($parameterSource,'(?ms)^\tpublic int RatingCorrection;.*?^\tprivate bool isImmortalityEnabled;').Value
$copy=[regex]::Match($parameterSource,'(?ms)^\tpublic ModelParameters\(ModelParameters NBMGOEMJJAF\).*?^\t\}').Value
$parameters=[regex]::Match($parameterSource,'(?ms)^\tpublic ObscuredInt GetLevel\(.*?^\t\}').Value
$parameters += [regex]::Match($parameterSource,'(?ms)^\tpublic void SetLevel\(.*?^\t\}').Value
$parameters += [regex]::Match($parameterSource,'(?ms)^\tpublic List<ItemInfo> GetEquippedItems\(.*?^\t\}').Value
$parameters += [regex]::Match($parameterSource,'(?ms)^\tpublic List<PerkInfoItem> GetAllPerks\(.*?^\t\}').Value
$parameters += [regex]::Match($parameterSource,'(?ms)^\tpublic void RemovePerksByNames\(.*?^\t\}').Value
$parameters += [regex]::Match($parameterSource,'(?ms)^\tprivate bool IsPerkInNames\(.*?^\t\}').Value
$animations=[regex]::Match($animationSource,'(?ms)^\tpublic static void CollectAvailableAnimations\(.*?^\t\}').Value
$rules=[regex]::Match($rulesSource,'(?ms)^\tpublic void ApplyNoPerksRules\(.*?^\t\}').Value
$rules += [regex]::Match($rulesSource,'(?ms)^\tpublic void ApplyNoAnimationRules\(.*?^\t\}').Value
if(!$fields -or !$copy -or !$parameters -or !$animations -or !$rules){throw 'Form parameter copy/filter extraction failed.'}
$fixture=Join-Path $root ('Temp/FormParameterCopy-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
$code=@'
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using ObscuredInt = System.Int32;
using ObscuredFloat = System.Single;

// Execute the production copy constructor, rule application, perk aggregation,
// and animation filtering. Item/perk identities and animation conditions are
// controlled; this does not load Unity, XML catalogs, or native fighter bodies.
enum SceneTypes { SceneNone, SceneFight }
enum EndRoundType { EndRoundTypeNone }
class GroupModel { }
class AttributesAlign { }
class Tactic { }
class Vector3f
{
    public Vector3f() { }
    public Vector3f(Vector3f source)
    { if (source == null) throw new ArgumentNullException(nameof(source)); }
}
class Attributes
{
    public readonly Dictionary<string, int> Values = new Dictionary<string, int>();
    public Attributes() { }
    public Attributes(Attributes source)
    { foreach (var pair in source.Values) Values.Add(pair.Key, pair.Value); }
}
class PerkInfoItem
{
    public enum PerkKind { SINGLE, COMBO }
    public string Name;
    public PerkKind Kind;
    public bool IsPerkByNames(string name) => name == Name;
    public void SetIsWeaponPerk(bool weapon) { }
}
class ItemInfo
{
    public string Type;
    public bool IgnoreInventoryEnchantments = true;
    public readonly List<PerkInfoItem> InnatePerks = new List<PerkInfoItem>();
}
class UserItem { }
class Inventory { public UserItem FindItem(ItemInfo item) => throw new Exception("unexpected save lookup"); }
class Roster { public Inventory GetInventory() => throw new Exception("unexpected save lookup"); }
class ListSF
{
    public static Roster GetRoster() => throw new Exception("unexpected save lookup");
    public static List<PerkInfoItem> GetItemEnchantmentsForSide(ItemInfo item, bool player) => throw new Exception("unexpected save lookup");
}
class GameUtils { public static bool IsPerkCompatibleWithEquipment(PerkInfoItem perk) => true; }
class GameLog { public static void Error(string message) => throw new Exception(message); }
class ModelParameters
{
    PARAMETER_FIELDS
    public ModelParameters()
    {
        FinalAttributes = new Attributes(); BaseAttributes = new Attributes();
        SpawnPosition = new Vector3f();
    }
    COPY_CONSTRUCTOR
    PARAMETER_METHODS
}
class ConditionAnimation { }
class ConditionTable { public List<ConditionAnimation> Locks = new List<ConditionAnimation>(); }
class InfoAnimation
{
    public string Name;
    public ConditionTable MoveData = new ConditionTable();
    public bool AreConditionsMet(ModelConditions conditions, List<ConditionAnimation> required) => true;
    public bool CheckAnimationName(List<string> names) => names.Contains(Name);
}
class ModelConditions
{
    public List<ItemInfo> Items;
    public bool IsWeapon;
    public SceneTypes SceneType;
    public List<PerkInfoItem> SelfPerks, OtherPerks;
}
class AnimationData
{
    public static readonly List<InfoAnimation> _Animations = new List<InfoAnimation> {
        new InfoAnimation { Name = "Stance" }, new InfoAnimation { Name = "RestrictedKick" } };
    ANIMATION_METHODS
}
class NoPerksRule
{
    public string Name;
    public string GetPerkName() => Name;
}
class NoAnimationRule
{
    public string Name;
    public bool Active;
    public bool GetActive() => Active;
    public string GetAnimationName() => Name;
}
class RulesInspector
{
    public List<NoAnimationRule> _noAnimationRules = new List<NoAnimationRule>();
    RULE_METHODS
}
class ValidateFormParameterCopy
{
    static int checks;
    static readonly List<string> failures = new List<string>();
    static void Check(bool value, string message)
    { checks++; if (!value) failures.Add(message); }
    static string[] Moves(ModelParameters parameters)
    {
        var selected = new List<InfoAnimation>();
        AnimationData.CollectAvailableAnimations(selected, parameters.GetEquippedItems(), false,
            parameters.ExcludedMoveNames, SceneTypes.SceneFight, parameters.GetAllPerks());
        return selected.Select(move => move.Name).ToArray();
    }
    static void Restrictions(bool player)
    {
        var blocked = new PerkInfoItem { Name = "BlockedPerk" };
        var allowed = new PerkInfoItem { Name = "AllowedPerk" };
        var source = new ModelParameters {
            IsPlayer = player, UserControlled = player, AiControlled = !player,
            SceneType = SceneTypes.SceneFight, MaxLife = 40,
            EclipseCharacterId = "sample:restricted-form",
            EclipseSkinModels = new[] { "sample:skin.xml" },
            Weapon = new ItemInfo { Type = "Weapon" } };
        source.Perks.Add(blocked);
        source.Weapon.InnatePerks.AddRange(new[] { blocked, allowed });
        source.FinalAttributes.Values.Add("WeaponDamage", 23);
        source.ModelDocuments.Add("sample:body.xml");
        var rules = new RulesInspector();
        rules._noAnimationRules.Add(new NoAnimationRule { Name = "RestrictedKick", Active = true });
        rules._noAnimationRules.Add(new NoAnimationRule { Name = "Stance", Active = false });
        rules.ApplyNoAnimationRules(source);
        rules.ApplyNoPerksRules(source, new List<NoPerksRule> { new NoPerksRule { Name = "BlockedPerk" } });
        Check(Moves(source).SequenceEqual(new[] { "Stance" }), "native active animation restriction applies before copying");
        Check(source.GetAllPerks().SequenceEqual(new[] { allowed }), "native perk exclusion covers equipment after innate filtering");
        var copy = new ModelParameters(source);
        Check(copy.IsPlayer == player && copy.UserControlled == player && copy.AiControlled == !player &&
            copy.MaxLife == 40 && copy.EclipseCharacterId == source.EclipseCharacterId,
            "copy preserves participant role, prepared health pool and destination character");
        Check(Moves(copy).SequenceEqual(Moves(source)), "copy must not re-enable RestrictedKick in the native animation filter");
        Check(copy.GetAllPerks().SequenceEqual(source.GetAllPerks()), "copy must not re-enable equipment BlockedPerk in native perk aggregation");
        Check(copy.ExcludedMoveNames != source.ExcludedMoveNames && copy.ExcludedPerkNames != source.ExcludedPerkNames,
            "copied restrictions must have independent collection ownership");
        var repeated = new ModelParameters(copy);
        Check(Moves(repeated).SequenceEqual(Moves(source)) && repeated.GetAllPerks().SequenceEqual(source.GetAllPerks()),
            "repeated copies preserve original restrictions through native consumers");
        copy.ExcludedMoveNames.Clear(); copy.ExcludedPerkNames.Clear();
        Check(Moves(source).SequenceEqual(new[] { "Stance" }) && source.GetAllPerks().SequenceEqual(new[] { allowed }),
            "clearing a detached copy cannot weaken original restrictions");
        copy.FinalAttributes.Values["WeaponDamage"] = 99;
        copy.ModelDocuments.Clear(); copy.EclipseSkinModels[0] = "changed";
        Check(source.FinalAttributes.Values["WeaponDamage"] == 23 && source.ModelDocuments.Count == 1 &&
            source.EclipseSkinModels[0] == "sample:skin.xml", "existing attribute/model/skin copy isolation remains intact");
    }
    public static int Main()
    {
        Restrictions(false); Restrictions(true);
        foreach (string failure in failures) Console.Error.WriteLine("FAIL: " + failure);
        if (failures.Count != 0)
        { Console.Error.WriteLine(failures.Count + " of " + checks + " copy/filter checks failed."); return 1; }
        Console.WriteLine("PASS: " + checks + " production parameter-copy/rule/filter checks. Actual constructor, active rule application, equipment perk aggregation and animation exclusion execute. Catalogs, native body loading, animation conditions and perk name matching are controlled; no Unity gameplay.");
        return 0;
    }
}
'@
$code=$code.Replace('PARAMETER_FIELDS',$fields).Replace('COPY_CONSTRUCTOR',$copy).Replace('PARAMETER_METHODS',$parameters).Replace('ANIMATION_METHODS',$animations).Replace('RULE_METHODS',$rules)
[IO.File]::WriteAllText((Join-Path $fixture 'Program.cs'),$code)
[IO.File]::WriteAllText((Join-Path $fixture 'Test.csproj'),'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><NoWarn>CS0649;CS0414</NoWarn></PropertyGroup></Project>')
dotnet run --project (Join-Path $fixture 'Test.csproj') --verbosity quiet
if($LASTEXITCODE -ne 0){throw 'Form parameter copy/filter checks failed.'}
