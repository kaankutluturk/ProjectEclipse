using Eclipse.Modding;

public static class ShopAvailabilityPolicy
{
    public static bool IsAvailable(ItemInfo item, Roster roster)
    {
        if (item == null || roster == null) return false;
        bool groupLocked = !string.IsNullOrEmpty(item.GroupId) && !roster.HasShopLock(item.GroupId);
        bool available = item.IsShopVisible && !item.IsHidden() && !groupLocked;

        ModScriptSession scripts = ModRuntime.Scripts;
        if (scripts == null) return available;
        DefinitionId id;
        string runtimeXml = item.NodeXML == null ? null : item.NodeXML.OuterXml;
        if (!scripts.Content.TryResolveRuntimeItem(item.Name, runtimeXml, out id)) return available;
        ItemAvailabilityPolicyDefinition policy;
        if (!scripts.Content.TryGetItemAvailability(id, out policy)) return available;
        if (roster.Level < policy.MinimumLevel) return false;
        if (policy.Visibility == ModItemVisibility.ForceHidden) return false;
        if (policy.Visibility == ModItemVisibility.ForceVisible) available = true;
        if (!available) return false;
        return string.IsNullOrEmpty(policy.RequiredGroup) || roster.HasShopLock(policy.RequiredGroup);
    }
}
