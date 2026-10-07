// Template populated with production methods by TestPerkFlagTransfer.ps1.
// Model services, expression evaluation and final event routing are controlled.
// The production action lifecycle, namespace registry and form stages execute here.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

public class FunctionResult
{
    public string Value;
    public int ToInt() => int.Parse(Value, CultureInfo.InvariantCulture);
}

public class FunctionExtension
{
    public class CallbackResult { }
    string value;
    public int Reads;
    public void Parse(string text) { value = text; }
    public FunctionResult Calculate() { Reads++; return new FunctionResult { Value = value }; }
    public void SetFunctionCallback(Action<CallbackResult> callback) { }
    public void SetVariableCallback(Action<CallbackResult> callback) { }
    public void set_Target(object target) { }
}

// The native flag/variable classes and their modifier base are compiled separately.
// Only the common XML/expression services below are controlled by the fixture.
public class PerkAction
{
    string name, scope;
    ActionType type;
    bool modifier;
    FunctionExtension frames;
    public PerkInfoItem Definition;
    public PerkAction() { }
    public PerkAction(PerkAction source)
    {
        name = source.name; scope = source.scope; type = source.type;
        modifier = source.modifier; frames = source.frames; Definition = source.Definition;
    }
    public virtual void Parse(XmlNode node)
    {
        name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
        scope = node.Attributes["Namespace"].GetStringOrDefault(string.Empty);
        if (node.Attributes["Frames"] != null)
        {
            frames = new FunctionExtension();
            frames.Parse(node.Attributes["Frames"].Value);
        }
    }
    public string get_Name() => name;
    public string GetNamespace() => scope;
    public ActionType get_Type() => type;
    protected void set_Type(ActionType value) { type = value; }
    public bool GetModificator() => modifier;
    protected void set_Modificator(bool value) { modifier = value; }
    public FunctionExtension GetFrames() => frames;
    public PerkInfoItem GetPerk() => Definition;
}

class PerkActionSetAttributes : PerkActionModificator
{
    public PerkActionSetAttributes() { set_Type(ActionType.ACTION_SET_ATTRIBUTES); }
}
class ModHealthChange : PerkActionModificator { }
class UnsupportedModifier : PerkActionModificator
{
    public UnsupportedModifier(ActionType type) { set_Type(type); }
}
class ItemInfo { }
public class PerkInfoItem
{
    public string Name = "fixture:flag-lifetime";
    public void EvaluateFunctionCallback(FunctionExtension.CallbackResult result) { }
    public void OnFunctionPreCallback(FunctionExtension.CallbackResult result) { }
}
class PerkData
{
    public PerkInfoItem PerkInfo;
    public bool Enabled = true;
    public PerkData(PerkInfoItem definition) { PerkInfo = definition; }
}
public class ModelParameters { public List<PerkInfoItem> Perks = new List<PerkInfoItem>(); }
public class Conditions
{
    public Dictionary<string, float> PerkVariables = new Dictionary<string, float>();
    public Dictionary<string, string> PerkStringVariables = new Dictionary<string, string>();
}
public class Model
{
    public readonly string Name;
    public ModelParameters Parameters = new ModelParameters();
    public Conditions Conditions = new Conditions();
    public int Modifier;
    public Model(string name) { Name = name; }
    public Conditions GetConditions() => Conditions;
    public bool HasTransientPerkFlag(string name) => false;
    public Action CopyFormModifiersFrom(Model source)
    {
        int previous = Modifier;
        Modifier = source.Modifier;
        return () => Modifier = previous;
    }
}
class PerkModelStruct
{
    Model model;
    readonly List<InfoPerk> effects = new List<InfoPerk>();
    readonly List<PerkData> data = new List<PerkData>();
    public Model get_Model() => model;
    public void set_Model(Model value) { model = value; }
    public List<InfoPerk> GetInfoPerks() => effects;
    public List<PerkData> GetPerkDataList() => data;
}
class PerkEvent { public enum PerkEventType { EVENT_MOD_EXPIRES } }
class PerkTrigger { }
public abstract class PerkCondition
{
    protected enum PerkConditionType { CONDITION_MOD_EXISTS }
    protected void set_Type(PerkConditionType type) { }
    public virtual void Parse(XmlNode node) { }
    protected Model ResolveTargetModel(Model model) => model;
    public abstract bool IsEqual(Model model, List<string> names);
}
static class FixtureExtensions
{
    public static string GetStringOrDefault(this XmlAttribute value, string fallback) => value?.Value ?? fallback;
    public static void AddIfNotExist<T>(this List<T> list, T value)
    {
        if (!list.Contains(value)) list.Add(value);
    }
}

class PerksStage
{
    /* STAGE_METHODS */

    public readonly List<PerkModelStruct> modelRegistrations = new List<PerkModelStruct>();
    public readonly List<ActionPerk> expiredActions = new List<ActionPerk>();
    public static readonly Dictionary<string, List<ActionPerk>> actionsByNamespace = new Dictionary<string, List<ActionPerk>>();
    readonly Dictionary<string, object> map = new Dictionary<string, object> { ["ModExpires"] = null };
    public readonly List<(Model Target, string Name, string Scope, object Parent)> Events = new List<(Model, string, string, object)>();
    public Action<Model> OnExpiry;
    public Dictionary<string, object> GetPerkMap() => map;
    public void RegisterPerkTriggers(PerkModelStruct registration, PerkInfoItem definition)
    {
        registration.GetPerkDataList().Add(new PerkData(definition));
    }
    public bool FireEvent(Model target, PerkEvent.PerkEventType type, bool include)
    {
        if (type != PerkEvent.PerkEventType.EVENT_MOD_EXPIRES) throw new Exception("Unexpected event.");
        Events.Add((target, (string)map["ModExpires"], (string)map["Namespace"], map["ParentPerk"]));
        OnExpiry?.Invoke(target);
        return true;
    }
    public static void IncrementPerkUse(string name) { throw new Exception("Unexpected perk-use mutation."); }
}

class Fight
{
    public static Fight Current;
    readonly PerksStage stage;
    public Fight(PerksStage value) { stage = value; }
    public static Fight GetCurrentFight() => Current;
    public PerksStage GetPerksStage() => stage;
}

class InfoPerk
{
    public PerkData Data;
    readonly List<PerksStage.ActionPerk> pendingActions = new List<PerksStage.ActionPerk>();
    readonly List<PerksStage.ActionPerk> activeActions = new List<PerksStage.ActionPerk>();
    readonly List<string> activeActionNames = new List<string>();
    readonly List<string> expiredModNames = new List<string>();
    public int Starts, Ends;

    /* INFO_METHODS */

    void LogModEvent(PerksStage.ActionPerk action, bool start) { if (start) Starts++; else Ends++; }
    public Action TransferAttributeEffect(PerksStage.ActionPerk action, Model old, Model next)
    {
        throw new InvalidOperationException("Injected later attribute-transfer failure.");
    }
    public Action TransferHealthEffect(PerksStage.ActionPerk action, Model old, Model next) => throw Unexpected();
    static Exception Unexpected() => new Exception("An unrelated native effect handler executed.");
    void ApplyShowIcon(PerksStage.ActionPerk action, bool remove) { throw Unexpected(); }
    void ApplyHealthChangeStart(PerksStage.ActionPerk action, bool remove) { throw Unexpected(); }
    void ApplySetAttributes(PerksStage.ActionPerk action, bool remove) { throw Unexpected(); }
    void ApplyInvisibility(PerksStage.ActionPerk action, bool remove) { throw Unexpected(); }
    void ApplyChangeImpulse(PerksStage.ActionPerk action, bool remove) { throw Unexpected(); }
    void ApplyChangeHitEffectScale(PerksStage.ActionPerk action, bool remove) { throw Unexpected(); }
    void ApplyChangeAdditionalDamage(PerksStage.ActionPerk action, bool remove) { throw Unexpected(); }
    void ApplyChangeModelColor(PerksStage.ActionPerk action, bool remove) { throw Unexpected(); }
    void ApplySlowModel(PerksStage.ActionPerk action, bool remove) { throw Unexpected(); }
    void ApplyTurnOffCollision(PerksStage.ActionPerk action, bool remove) { throw Unexpected(); }
    void ApplyPerkArea(PerksStage.ActionPerk action, bool remove) { throw Unexpected(); }
    void ApplyStealMagic(PerksStage.ActionPerk action, bool remove) { throw Unexpected(); }
    void ApplyDisableInterval(PerksStage.ActionPerk action) { throw Unexpected(); }
    void ApplySetHit(PerksStage.ActionPerk action) { throw Unexpected(); }
    void ApplyLifeSteal(PerksStage.ActionPerk action) { throw Unexpected(); }
    void ApplyAddBullets(PerksStage.ActionPerk action) { throw Unexpected(); }
    void ApplyAddMagicCharge(PerksStage.ActionPerk action) { throw Unexpected(); }
    void ApplySetModFrames(PerksStage.ActionPerk action) { throw Unexpected(); }
    void ApplySetModEffect(PerksStage.ActionPerk action) { throw Unexpected(); }
    void ApplyProvoke(PerksStage.ActionPerk action) { throw Unexpected(); }
    void ApplySetTactics(PerksStage.ActionPerk action) { throw Unexpected(); }
    void ApplyClearAction(PerksStage.ActionPerk action) { throw Unexpected(); }
    void ApplySetVariable(PerksStage.ActionPerk action) { throw Unexpected(); }
    void ApplySetCooldown(PerksStage.ActionPerk action) { throw Unexpected(); }
    void ApplySwitch(PerksStage.ActionPerk action) { throw Unexpected(); }
    void ApplyMoveModel(PerksStage.ActionPerk action) { throw Unexpected(); }
    void ApplySetMovesVariable(PerksStage.ActionPerk action) { throw Unexpected(); }
    void ApplyHealthChange(PerksStage.ActionPerk action) { throw Unexpected(); }
}

static class ValidatePerkFlagTransfer
{
    static int checks;
    static void Check(bool condition, string description)
    {
        checks++;
        if (!condition) throw new Exception(description);
    }
    static XmlNode Node(string xml)
    {
        var document = new XmlDocument(); document.LoadXml(xml); return document.DocumentElement;
    }
    static PerksStage Stage()
    {
        PerksStage.ClearNamespaceActions();
        var stage = new PerksStage();
        Fight.Current = new Fight(stage);
        return stage;
    }
    static InfoPerk Register(PerksStage stage, Model owner)
    {
        var definition = new PerkInfoItem();
        owner.Parameters.Perks.Add(definition);
        var registration = stage.PrepareModelRegistration(owner);
        var perk = new InfoPerk { Data = new PerkData(definition) };
        registration.GetInfoPerks().Add(perk);
        stage.modelRegistrations.Add(registration);
        return perk;
    }
    static PerksStage.ActionPerk Start(InfoPerk perk, Model target, Model source,
        string name = "BleedingCycle", int frames = 6, bool variable = false, string value = "42")
    {
        PerkAction definition = variable ? new PerkActionVariable() : new PerkActionFlag();
        definition.Definition = perk.Data.PerkInfo;
        string element = variable ? "SetModVariable" : "ModFlag";
        definition.Parse(Node("<" + element + " Name='" + name + "' Namespace='fixture' Frames='" + frames + "' Value='" + value + "'/>"));
        var pending = new PerksStage.ActionPerk {
            TargetModel = target, SourceModel = source, Action = definition,
            DurationFrames = definition.GetFrames().Calculate().ToInt()
        };
        perk.GetPendingActions().Add(pending);
        perk.Run();
        var action = perk.GetActiveActions().Last();
        Check(action != pending && action.Action == definition, "Native modifier start makes one action copy.");
        Check(perk.GetPendingActions().Count == 0 && action.ElapsedFrames == 0, "Native start drains queued work without advancing time.");
        Check(PerksStage.FindNamespaceAction(name, "fixture") == action, "Native namespace registration indexes the active copy.");
        return action;
    }
    static bool Exists(Model model, InfoPerk perk, string name, bool namespaced)
    {
        var condition = new PerkConditionModExists();
        condition.Parse(Node("<ModExists Name='" + name + "'" + (namespaced ? " Namespace='fixture'" : "") + "/>"));
        return condition.IsEqual(model, perk.GetActiveActionNames());
    }
    static int Lifetime(bool transfer, bool variable = false, string value = "42")
    {
        var stage = Stage();
        var old = new Model("old"); var next = new Model("next"); var final = new Model("final");
        var perk = Register(stage, old);
        var action = Start(perk, old, old, variable ? "EnemyMagic" : "BleedingCycle", 6, variable, value);
        string name = action.Action.get_Name();
        if (variable)
        {
            Check(value == "42" ? old.Conditions.PerkVariables[name] == 42 : old.Conditions.PerkStringVariables[name] == value,
                "Native variable handler stores the evaluated numeric/string value.");
        }
        Check(Exists(old, perk, name, false) && Exists(old, perk, name, true), "Active modifier is visible to native ModExists lookups.");
        perk.Render(); perk.Render();
        int elapsed = action.ElapsedFrames, reads = action.Action.GetFrames().Reads;
        var names = perk.GetActiveActionNames(); var active = perk.GetActiveActions(); var scope = PerksStage.GetNamespaceActions("fixture");
        Model current = old;
        if (transfer)
        {
            next.Parameters.Perks.Add(perk.Data.PerkInfo);
            stage.TransferFormEffects(old, next);
            stage.ReplaceFormRegistration(old, next);
            stage.RequireFormReferencesTransferred(new HashSet<Model> { old });
            Check(stage.modelRegistrations[0].get_Model() == next && stage.modelRegistrations[0].GetInfoPerks().Single() == perk,
                "Replacement retains the same effect container.");
            Check(active == perk.GetActiveActions() && names == perk.GetActiveActionNames() && scope == PerksStage.GetNamespaceActions("fixture"),
                "Action, name and namespace collections retain their identity.");
            Check(active.Single() == action && scope.Single() == action, "Form transfer preserves the action identity in every live index.");
            Check(action.TargetModel == next && action.SourceModel == next, "Both self references follow the new body.");
            Check(action.ElapsedFrames == elapsed && action.DurationFrames == 6 && action.Action.GetFrames().Reads == reads,
                "Transfer preserves the exact timer without evaluating its expression.");
            Check(perk.Starts == 1 && perk.Ends == 0 && stage.Events.Count == 0, "Transfer never replays a start or sends early expiry.");
            if (variable) Check(((PerkActionVariable)action.Action).GetValue().Reads == 1, "Variable expression is not evaluated again.");
            Check(Exists(next, perk, name, false) && Exists(next, perk, name, true), "Transferred state remains discoverable.");
            // Repeated changes are still one logical effect with one expiry.
            final.Parameters.Perks.Add(perk.Data.PerkInfo);
            stage.TransferFormEffects(next, final);
            stage.ReplaceFormRegistration(next, final);
            stage.RequireFormReferencesTransferred(new HashSet<Model> { old, next });
            Check(action.ElapsedFrames == elapsed && action.TargetModel == final, "Repeated replacement does not restart lifetime.");
            current = final;
        }
        stage.OnExpiry = actor => Check(actor == current && !Exists(actor, perk, name, false) && !Exists(actor, perk, name, true),
            "Native expiry removes name and namespace before delivering its event.");
        int remaining = 0;
        while (perk.GetActiveActions().Count > 0 && remaining < 20) { perk.Render(); remaining++; }
        Check(remaining < 20 && perk.Ends == 1 && stage.Events.Count == 1, "Native Render expires exactly once.");
        var observed = stage.Events.Single();
        Check(observed.Target == current && observed.Name == name && observed.Scope == "fixture" && observed.Parent == perk.Data.PerkInfo,
            "Expiry reports the current target and original native name, namespace and parent.");
        Check(stage.expiredActions.Single() == action && PerksStage.FindNamespaceAction(name, "fixture") == null,
            "Expired action keeps history identity and leaves the live namespace.");
        perk.Render(); perk.ClearActions(true);
        Check(perk.Ends == 1 && stage.Events.Count == 1, "Later rendering and teardown do not expire the action again.");
        return remaining;
    }
    static void ReferencesAndRollback(bool variable)
    {
        var stage = Stage(); var old = new Model("old") { Modifier = 7 }; var next = new Model("next"); var other = new Model("other");
        var owned = Register(stage, old); var external = Register(stage, other);
        var both = Start(owned, old, old, "both", variable: variable);
        var source = Start(owned, other, old, "source", variable: variable);
        var target = Start(external, old, other, "target", variable: variable);
        var untouched = Start(external, other, other, "unrelated", variable: variable);
        both.ElapsedFrames = 3; both.IsExpired = true;
        // Alias the same record across active, queued, history and namespace indexes.
        owned.GetActiveActions().Add(both); owned.GetPendingActions().Add(both); stage.expiredActions.Add(both);
        var undoQueued = stage.RebindQueuedFormActions(old, next);
        Check(both.TargetModel == old, "Queued transfer leaves active aliases for the effect-specific path.");
        var undo = stage.TransferFormEffects(old, next);
        stage.RequireFormReferencesTransferred(new HashSet<Model> { old });
        Check(both.TargetModel == next && both.SourceModel == next && source.TargetModel == other && source.SourceModel == next,
            "Both and source-only references move independently.");
        Check(target.TargetModel == next && target.SourceModel == other && untouched.TargetModel == other && untouched.SourceModel == other,
            "Opponent-owned target effects move while unrelated effects remain untouched.");
        Check(both.ElapsedFrames == 3 && both.IsExpired && owned.GetActiveActions().Count == 3 && owned.GetPendingActions().Single() == both,
            "Duplicate aliases, queued membership and pending removal are preserved without replay.");
        var originalRegistration = stage.modelRegistrations[0];
        next.Parameters.Perks.Add(owned.Data.PerkInfo);
        var undoRegistration = stage.ReplaceFormRegistration(old, next);
        undoRegistration(); undo(); undoQueued();
        Check(stage.modelRegistrations[0] == originalRegistration && both.TargetModel == old && both.SourceModel == old && next.Modifier == 0,
            "Rollback restores participant registration, original effect references and earlier model changes.");
        Check(source.SourceModel == old && target.TargetModel == old && PerksStage.FindNamespaceAction("both", "fixture") == both,
            "Rollback restores all attribution while retaining namespace aliases.");
        owned.GetActiveActions().RemoveAt(owned.GetActiveActions().Count - 1);
        owned.GetPendingActions().Clear(); stage.expiredActions.Clear();
        var failing = new PerksStage.ActionPerk { TargetModel = old, Action = new PerkActionSetAttributes() };
        external.GetActiveActions().Add(failing);
        bool failed = false;
        try { stage.TransferFormEffects(old, next); }
        catch (InvalidOperationException error) { failed = error.Message.Contains("later attribute-transfer"); }
        Check(failed && next.Modifier == 0 && both.TargetModel == old && source.SourceModel == old && target.TargetModel == old,
            "A later effect failure reverses already-transferred records and model state.");
        Check(both.ElapsedFrames == 3 && both.DurationFrames == 6 && both.IsExpired && stage.Events.Count == 0,
            "Failed transfer leaves timer, pending removal and event history unchanged.");
        if (variable)
        {
            Check(((PerkActionVariable)both.Action).GetValue().Reads == 1 &&
                ((PerkActionVariable)target.Action).GetValue().Reads == 1 && old.Conditions.PerkVariables["both"] == 42,
                "Reference transfer and rollback do not replay variable expressions or overwrite their stored value.");
        }
        external.GetActiveActions().Remove(failing);
        owned.ClearActions();
        Check(stage.Events.Single().Target == old && stage.Events.Single().Name == "both",
            "After rollback native clearing expires the flagged record on the original fighter.");
    }
    static void IndefiniteAndPendingRemoval(bool variable)
    {
        foreach (int frames in new[] { 0, 20 })
        {
            var stage = Stage(); var old = new Model("old"); var next = new Model("next"); var perk = Register(stage, old);
            var action = Start(perk, old, old, "held", frames, variable);
            for (int i = 0; i < 3; i++) perk.Render();
            int elapsed = action.ElapsedFrames;
            action.IsExpired = true;
            stage.TransferFormEffects(old, next);
            stage.RequireFormReferencesTransferred(new HashSet<Model> { old });
            Check(action.IsExpired && action.ElapsedFrames == elapsed && stage.Events.Count == 0,
                "Pending removal survives transfer for indefinite and timed modifiers.");
            perk.ClearActions(); perk.ClearActions(true);
            Check(perk.GetActiveActions().Count == 0 && stage.Events.Count == 1 && stage.Events[0].Target == next,
                "Native clearing ends indefinite/timed modifiers exactly once on the replacement.");
        }
    }
    static void UnresolvedReferencesStayGuarded()
    {
        foreach (ActionType kind in new[] { ActionType.ACTION_STEAL_MAGIC, ActionType.ACTION_NONE })
        {
            var stage = Stage(); var old = new Model("old"); var next = new Model("next"); var perk = Register(stage, old);
            var unresolved = new PerksStage.ActionPerk { TargetModel = old, Action = new UnsupportedModifier(kind) };
            perk.GetActiveActions().Add(unresolved);
            var undo = stage.TransferFormEffects(old, next);
            bool rejected = false;
            try { stage.RequireFormReferencesTransferred(new HashSet<Model> { old }); }
            catch (InvalidOperationException error) { rejected = error.Message.Contains(kind.ToString()); }
            Check(rejected && unresolved.TargetModel == old, "Unimplemented applied effects still reject retirement.");
            undo();
        }
        foreach (bool variable in new[] { false, true })
        {
            var stage = Stage(); var old = new Model("old"); var next = new Model("next"); var perk = Register(stage, old);
            var orphan = Start(perk, old, old, "namespace-only", variable: variable);
            perk.GetActiveActions().Remove(orphan);
            var undo = stage.TransferFormEffects(old, next);
            bool rejected = false;
            try { stage.RequireFormReferencesTransferred(new HashSet<Model> { old }); }
            catch (InvalidOperationException) { rejected = true; }
            Check(rejected && orphan.TargetModel == old, "Namespace-only state is not assumed safely expired or transferable.");
            undo();
        }
    }
    public static void Main()
    {
        int baseline = Lifetime(false);
        Check(Lifetime(true) == baseline, "Transferred flags expire on the same native step as uninterrupted flags.");
        foreach (bool variable in new[] { false, true })
        {
            ReferencesAndRollback(variable);
            IndefiniteAndPendingRemoval(variable);
        }
        foreach (string value in new[] { "42", "MAGIC_FIREBALL" })
        {
            int variableBaseline = Lifetime(false, true, value);
            Check(Lifetime(true, true, value) == variableBaseline, "Variable modifiers preserve native expiry timing.");
        }
        UnresolvedReferencesStayGuarded();
        Console.WriteLine("PASS: " + checks + " production flag/variable lifecycle and form-transfer checks. " +
            "Actual action copies, Run/start/Render/clear/expiry, ModExists, namespace indexes and form registration/transfer/gate; " +
            "models, expression evaluation and final event routing controlled. Variable dictionary handover is covered separately; no native fight playtest.");
    }
}
