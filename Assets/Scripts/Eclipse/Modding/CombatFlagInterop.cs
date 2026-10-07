using System;
using System.Collections.Generic;

// Typed entry points into recovered modifier lifetime/expiry handling. These
// containers have no native triggers; Lua owns all decisions to set/clear flags.
public partial class PerksStage
{
    internal InfoPerk GetScriptFlagContainer(Model model, object owner, string behavior, bool create)
    {
        if (model == null || owner == null) throw new ArgumentException("Flag owner is unavailable.");
        var registration = modelRegistrations.Find(value => value.get_Model() == model);
        if (registration == null) throw new InvalidOperationException("Fighter has no active perk registration.");
        var containers = registration.ActivePerkEffects;
        int owned = 0;
        foreach (var container in containers)
        {
            if (container.ScriptFlagOwner == null) continue;
            owned++;
            if (Equals(container.ScriptFlagOwner, owner)) return container;
        }
        if (!create) return null;
        if (owned >= 64) throw new InvalidOperationException("Fighter flag-owner limit reached (64).");
        var result = new InfoPerk {
            ScriptFlagOwner = owner,
            Data = new PerkData(new PerkInfoItem { Name = behavior })
        };
        containers.Add(result);
        return result;
    }
}

public partial class InfoPerk
{
    internal object ScriptFlagOwner { get; set; }

    internal bool HasScriptFlag(string name)
    {
        return GetActiveActionNames().Contains(name);
    }

    internal void SetScriptFlag(Model model, string name)
    {
        if (ScriptFlagOwner == null) throw new InvalidOperationException("Not a script flag container.");
        if (HasScriptFlag(name)) return;
        if (GetActiveActionNames().Count >= 64) throw new InvalidOperationException("Flag limit reached (64 per owner).");
        var action = new PerksStage.ActionPerk {
            TargetModel = model, SourceModel = model,
            Action = new PerkActionFlag(name, Data.PerkInfo)
        };
        // The native path copies modifiers into active actions/name lists and
        // handles removal/expiry through ClearActions. No synthetic event DSL.
        ExecuteActions(new List<PerksStage.ActionPerk> { action });
    }

    internal void ClearScriptFlag(string name)
    {
        if (ScriptFlagOwner == null) throw new InvalidOperationException("Not a script flag container.");
        foreach (var action in GetActiveActions())
            if (action.Action.get_Name() == name) action.IsExpired = true;
        ClearActions();
    }
}

public partial class PerkActionFlag
{
    internal PerkActionFlag(string name, PerkInfoItem owner)
    {
        set_Name(name);
        set_Type(ActionType.ACTION_FLAG);
        set_Namespace(string.Empty);
        SetPerk(owner);
        SetTrigger(new PerkTrigger { PerkInfo = owner });
        SetElementName("ModFlag");
    }
}
