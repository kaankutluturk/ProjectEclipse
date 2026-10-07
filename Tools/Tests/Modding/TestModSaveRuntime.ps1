$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$source = Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/UserItems.cs')
$userItemSource = Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/UserItem.cs')
# Execute the recovered inventory parse/add methods, with unrelated Unity/timer services stubbed.
$parse = [regex]::Match($source, '(?ms)^\tpublic void Parse\(XmlNode.*?^\t\}').Value
$add = [regex]::Match($source, '(?ms)^\tpublic UserItem AddItem\(.*?^\t\}').Value
$find = [regex]::Match($source, '(?ms)^\tpublic UserItem FindItem\(string name\).*?^\t\}').Value
$setEquipped = [regex]::Match($userItemSource, '(?ms)^\tpublic void SetIsEquipped\(bool value\).*?^\t\}').Value
$setCount = [regex]::Match($userItemSource, '(?ms)^\tpublic void SetCount\(int value\).*?^\t\}').Value
$setDelivery = [regex]::Match($userItemSource, '(?ms)^\tpublic void set_DeliveryTime\(long value\).*?^\t\}').Value
$setDeliveryUpgrade = [regex]::Match($userItemSource, '(?ms)^\tpublic void SetDeliveryUpgradeLevel\(int value\).*?^\t\}').Value
$setUpgrade = [regex]::Match($userItemSource, '(?ms)^\tpublic void SetUpgradeLevel\(int value\).*?^\t\}').Value
$setAcquire = [regex]::Match($userItemSource, '(?ms)^\tpublic void SetAcquireType\(string value\).*?^\t\}').Value
if (!$parse -or !$add -or !$find -or !$setEquipped -or !$setCount -or !$setDelivery -or !$setDeliveryUpgrade -or !$setUpgrade -or !$setAcquire) {
    throw 'Could not extract recovered inventory/item serialization methods.'
}
$testRoot = Join-Path $root 'Temp/ModSaveRuntime'
New-Item -ItemType Directory -Force -Path $testRoot | Out-Null
$code = @'
using System;
using System.Collections.Generic;
using System.Xml;
using Eclipse.Modding;

namespace UnityEngine { public static class Debug { public static void LogWarning(object value) {} } }
public static class XmlCompat
{
    public static XmlAttribute AppendAttribute(this XmlNode node, string name)
    {
        XmlAttribute attribute = node.OwnerDocument.CreateAttribute(name);
        node.Attributes.Append(attribute);
        return attribute;
    }
}
public sealed class ItemInfo { public string Type = "Weapon"; public void SetIsNew(bool value) {} }
public sealed class Items
{
    public readonly Dictionary<string, ItemInfo> Definitions = new Dictionary<string, ItemInfo>();
    public readonly Dictionary<string, string> Aliases = new Dictionary<string, string>();
    public ItemInfo GetItemByName(string name)
    {
        ItemInfo item;
        if (Definitions.TryGetValue(name, out item)) return item;
        string target;
        return Aliases.TryGetValue(name, out target) && Definitions.TryGetValue(target, out item) ? item : null;
    }
}
public static class ListSF { public static readonly Items Items = new Items(); public static Items GetItems() { return Items; } }
public sealed class UserItem
{
    public static int Constructions;
    private readonly XmlElement _Node;
    private bool writesToNode = true;
    private bool isEquipped;
    private int count;
    private long _DeliveryTime;
    private int deliveryUpgradeLevel;
    private int upgradeLevel;
    private string acquireType;
    public UserItem(XmlNode node)
    {
        Constructions++;
        _Node = (XmlElement)node;
        isEquipped = _Node.GetAttribute("Equipped") == "1";
        int.TryParse(_Node.GetAttribute("Count"), out count);
        long.TryParse(_Node.GetAttribute("DeliveryTime"), out _DeliveryTime);
        if (!int.TryParse(_Node.GetAttribute("DeliveryUpgradeLevel"), out deliveryUpgradeLevel)) deliveryUpgradeLevel = -1;
        if (!int.TryParse(_Node.GetAttribute("UpgradeLevel"), out upgradeLevel)) upgradeLevel = -1;
        acquireType = _Node.GetAttribute("AcquireType");
    }
    public string get_Name() { return _Node.GetAttribute("Name"); }
    public ItemInfo GetInfo() { return null; } // Binding occurs later in ApplyItemInfos.
    public long GetDeliveryTimestamp() { return _DeliveryTime; }
    public int GetCount() { return count; }
    public int GetDeliveryUpgradeLevel() { return deliveryUpgradeLevel; }
    public int GetUpgradeLevel() { return upgradeLevel; }
    public string GetAcquireType() { return acquireType; }
    __SET_EQUIPPED__
    __SET_COUNT__
    __SET_DELIVERY__
    __SET_DELIVERY_UPGRADE__
    __SET_UPGRADE__
    __SET_ACQUIRE__
}
public sealed class UserItems
{
    private readonly List<string> _missingModItemIds = new List<string>();
    private readonly List<UserItem> _items = new List<UserItem>();
    private readonly List<UserItem> pendingDeliveries = new List<UserItem>();
    public int Visible => _items.Count;
    public int Deliveries => pendingDeliveries.Count;
    public int Missing => _missingModItemIds.Count;
    __FIND__
    __PARSE__
    __ADD__
}
public static class Program
{
    private static void Assert(bool value, string message) { if (!value) throw new Exception(message); }
    public static int Main()
    {
        const string id = "example.weapon:items/weapon/example_blade";
        var save = new XmlDocument();
        save.LoadXml("<Warrior Weapon='" + id + "'><Items><Item Name='Fists' Count='1'/>" +
            "<Item Name='" + id + "' Count='1' UpgradeLevel='1201' Equipped='1' DeliveryTime='1234' Future='opaque'>" +
            "<Enchantments><Unrecognized data='preserve'/></Enchantments></Item></Items></Warrior>");
        XmlElement item = (XmlElement)save.DocumentElement["Items"].LastChild;
        string original = item.OuterXml;
        ListSF.Items.Definitions.Add("Fists", new ItemInfo());
        var missing = new UserItems();
        missing.Parse(save.DocumentElement["Items"]);
        Assert(missing.Visible == 1 && missing.Deliveries == 0 && missing.Missing == 1 && UserItem.Constructions == 1,
            "Missing mod item entered the constructor, active inventory, or delivery queue.");
        Assert(item.OuterXml == original, "Missing mod ownership was normalized or lost opaque children.");
        var reloaded = new XmlDocument();
        reloaded.LoadXml(save.OuterXml);
        var stillMissing = new UserItems();
        stillMissing.Parse(reloaded.DocumentElement["Items"]);
        Assert(stillMissing.Missing == 1 && reloaded.DocumentElement["Items"].LastChild.OuterXml == original,
            "Missing item changed across another save/load.");
        ListSF.Items.Definitions.Add(id, new ItemInfo());
        var restored = new UserItems();
        restored.Parse(reloaded.DocumentElement["Items"]);
        Assert(restored.Visible == 2 && restored.Missing == 0 && restored.Deliveries == 1,
            "Restored mod item did not return to inventory and pending deliveries.");
        Assert(reloaded.DocumentElement["Items"].LastChild.OuterXml == original,
            "Restoring the original equipped mod changed saved ownership or upgrades.");

        // Exercise the real recovered UserItem XML mutation methods. These are the exact
        // methods used by purchase, upgrade, equip and delivery code in the game runtime.
        UserItem live = restored.FindItem(id);
        Assert(live != null, "Restored mod item was not addressable through active inventory.");
        live.SetCount(2);
        live.SetUpgradeLevel(1300);
        live.set_DeliveryTime(7777);
        live.SetDeliveryUpgradeLevel(1400);
        live.SetAcquireType("Upgrade");
        live.SetIsEquipped(true);
        item = (XmlElement)reloaded.DocumentElement["Items"].LastChild;
        Assert(item.GetAttribute("Count") == "2" && item.GetAttribute("UpgradeLevel") == "1300" &&
            item.GetAttribute("DeliveryTime") == "7777" && item.GetAttribute("DeliveryUpgradeLevel") == "1400" &&
            item.GetAttribute("AcquireType") == "Upgrade" && item.GetAttribute("Equipped") == "1",
            "Recovered purchase/upgrade/equip state did not serialize to the owned save node.");
        var lifecycleReload = new UserItems();
        lifecycleReload.Parse(reloaded.DocumentElement["Items"]);
        UserItem lifecycleItem = lifecycleReload.FindItem(id);
        Assert(lifecycleItem != null && lifecycleItem.GetCount() == 2 && lifecycleItem.GetUpgradeLevel() == 1300 &&
            lifecycleItem.GetDeliveryTimestamp() == 7777 && lifecycleItem.GetDeliveryUpgradeLevel() == 1400 &&
            lifecycleItem.GetAcquireType() == "Upgrade",
            "Recovered purchase/upgrade/equip state did not survive a save DOM reload.");

        // The player chose a different weapon while this mod was absent. Do not re-equip it.
        reloaded.DocumentElement.SetAttribute("Weapon", "Fists");
        new UserItems().Parse(reloaded.DocumentElement["Items"]);
        item = (XmlElement)reloaded.DocumentElement["Items"].LastChild;
        Assert(item.GetAttribute("Equipped") == "0" && item.GetAttribute("Count") == "2" &&
            item.GetAttribute("UpgradeLevel") == "1300" && item["Enchantments"].FirstChild.Name == "Unrecognized",
            "Restoration overwrote a newer equipment choice or lost ownership state.");

        const string oldId = "example.weapon:items/weapon/example_blade_legacy";
        ListSF.Items.Aliases.Add(oldId, id);
        var aliasSave = new XmlDocument();
        aliasSave.LoadXml("<Warrior Weapon='" + oldId + "'><Items><Item Name='" + oldId +
            "' Count='1' UpgradeLevel='1200' Equipped='1'/></Items></Warrior>");
        var aliased = new UserItems();
        aliased.Parse(aliasSave.DocumentElement["Items"]);
        UserItem historical = aliased.FindItem(id);
        Assert(aliased.Visible == 1 && aliased.Missing == 0 && historical != null && historical.get_Name() == oldId,
            "Historical item alias did not restore ownership under the current definition.");
        historical.SetCount(2);
        Assert(aliasSave.DocumentElement["Items"].FirstChild.Attributes["Name"].Value == oldId &&
            aliasSave.DocumentElement["Items"].FirstChild.Attributes["Count"].Value == "2",
            "Alias resolution rewrote the historical save ID instead of preserving it non-destructively.");
        Console.WriteLine("Recovered inventory lifecycle: PASS (missing, restore, purchase/upgrade/equip serialization, delivery exclusion, equipment choice).");
        return 0;
    }
}
'@
$code = $code.Replace('__PARSE__', $parse).Replace('__ADD__', $add).Replace('__FIND__', $find)
$code = $code.Replace('__SET_EQUIPPED__', $setEquipped).Replace('__SET_COUNT__', $setCount)
$code = $code.Replace('__SET_DELIVERY__', $setDelivery).Replace('__SET_DELIVERY_UPGRADE__', $setDeliveryUpgrade)
$code = $code.Replace('__SET_UPGRADE__', $setUpgrade).Replace('__SET_ACQUIRE__', $setAcquire)
$harness = Join-Path $testRoot 'Program.cs'
[IO.File]::WriteAllText($harness, $code, [Text.UTF8Encoding]::new($false))
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$csc = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
if (!$csc) { throw 'Could not locate the Visual Studio Roslyn compiler.' }
$sources = @(Get-ChildItem -LiteralPath (Join-Path $root 'Assets/Scripts/Eclipse/Runtime/Modding') -Filter '*.cs' | Select-Object -ExpandProperty FullName)
$exe = Join-Path $testRoot 'ModSaveRuntime.exe'
& $csc /nologo /langversion:9.0 /target:exe "/out:$exe" @sources $harness
if ($LASTEXITCODE -ne 0) { throw 'Inventory regression compilation failed.' }
& $exe
if ($LASTEXITCODE -ne 0) { throw 'Inventory regression failed.' }
