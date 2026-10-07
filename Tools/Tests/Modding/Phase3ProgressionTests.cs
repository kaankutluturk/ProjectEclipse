// Host-boundary stubs only. Counter/achievement parsing, save mutation and the P3
// adapter are compiled from production source by TestPhase3Progression.ps1.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Eclipse.Modding;

public static class XmlExtensions
{
    public static string GetStringOrDefault(this XmlAttribute a, string fallback = "") => a?.Value ?? fallback;
    public static int ParseInt(this XmlAttribute a) => a == null ? 0 : int.Parse(a.Value);
    public static bool ParseBool(this XmlAttribute a) => a != null && (a.Value == "1" || a.Value == "true");
    public static XmlNode AppendElement(this XmlNode parent, string name) { var e = parent.OwnerDocument.CreateElement(name); parent.AppendChild(e); return e; }
    public static void AppendAttribute(this XmlNode n, string name) => n.Attributes.Append(n.OwnerDocument.CreateAttribute(name));
}
public static class SystemProperties { public static bool IsAndroidPlatform() => false; }
public sealed class ArgsDict : Dictionary<string, object> { }
public static class StatisticsEvent { public enum EventType { Achievement } }
public static class StatisticsCollector { public static void LogEvent(StatisticsEvent.EventType e, ArgsDict a) { } }
public class Pair<T, U> { }
public class Counter { public int CompleteValue; public string Name, Type; public void ResetCompleteValue() { } }
public static class GameUtils
{
    public sealed class AchievementCounters { public Dictionary<string, Counter> AllCounters = new Dictionary<string, Counter>(); }
    public sealed class AchievCounters
    {
        public List<AchievCounter> Counters = new List<AchievCounter>();
        public AchievCounter GetCounterByName(string name) => Counters.FirstOrDefault(c => c.Name == name);
        public Achievement GetAchievementByName(string name) => Counters.SelectMany(c => c.Achievements).FirstOrDefault(a => a.Name == name);
        public List<Pair<Achievement, int>> GetUnlockableAchievements(List<string> names) => new List<Pair<Achievement, int>>();
    }
    public static AchievCounters AchievementDefinitions = new AchievCounters();
    public static AchievementCounters ModeCounters = new AchievementCounters();
    public static void UnlockAchievements(List<Pair<Achievement, int>> a) { }
}
public sealed class ListSF
{
    public static readonly ListSF Instance = new ListSF();
    public UserAchievements User; public int Saves;
    public static ListSF GetRoster() => Instance;
    public UserAchievements GetAchievements() => User;
    public void RequestSave() => Saves++;
}
public static class LocalizationManager
{
    public static readonly Dictionary<string, string> Values = new Dictionary<string, string>();
    public static void SetExternalString(string key, string value) => Values[key] = value;
}
namespace Eclipse.Modding
{
    public sealed partial class LegacyContentAdapter : IDisposable
    {
        private readonly ModContentCatalog _content;
        private readonly HashSet<string> _localizationKeys = new HashSet<string>();
        public LegacyContentAdapter(ModContentCatalog content) { _content = content; }
        public void Localize() => ApplyP3Localization("eng");
        public void Dispose() => RemoveP3Content();
    }
}
public static class Phase3ProgressionTests
{
    private static int checks;
    private static void Check(bool value, string message) { checks++; if (!value) throw new Exception(message); }
    public static void Main(string[] args)
    {
        var mod = ModDiscovery.DiscoverLoose(args[0]).Mods.Single();
        var catalog = new ModContentCatalog();
        var assets = new AssetResolver(new IAssetProvider[] { new LooseModProvider(mod) });
        using (var tx = catalog.BeginRegistration(mod))
        {
            ModLocalizationLoader.Load(mod, assets, tx);
            var counter = tx.RegisterCounter("wins", 3);
            foreach (var row in new[] { ("first", 1), ("third", 3) })
                tx.RegisterAchievement(row.Item1, counter.Id, tx.GetLocalization(row.Item1), tx.GetLocalization(row.Item1 + ".description"),
                    AssetId.Parse("example.phase3:sprites/archivist"), row.Item2, false);
            tx.Commit();
        }
        using var adapter = new LegacyContentAdapter(catalog);
        adapter.ApplyP3Content(); adapter.ApplyP3Content(); adapter.Localize();
        Check(GameUtils.AchievementDefinitions.Counters.Count == 1, "Repeated apply duplicated native counters.");
        Check(LocalizationManager.Values["example.phase3:achievements/first"] == "P3 First Record", "Native achievement title alias missing.");
        var doc = new XmlDocument(); doc.LoadXml("<Warrior/>");
        void Load()
        {
            ListSF.Instance.User = new UserAchievements();
            ListSF.Instance.User.Parse(doc.DocumentElement);
        }
        Load();
        var id = DefinitionId.Parse("example.phase3:counters/wins");
        Check(ModProgressionAccess.Read(id) == 0, "New profile did not start at zero.");
        Check(ModProgressionAccess.Advance(id, 1) == 1 && ListSF.Instance.User.Achievements.Count == 1, "First threshold did not unlock.");
        Check(doc.SelectSingleNode("/Warrior/Counters/Counter").Attributes["CurrentValue"].Value == "1", "Counter was not written to native save XML.");
        Check(doc.SelectSingleNode("/Warrior/Achievements/Achievement").Attributes["ObtainedReward"].Value == "true", "Rewardless achievement has a spurious claim.");
        string saved = doc.OuterXml;
        adapter.Dispose();
        Check(ModProgressionAccess.Read == null && GameUtils.AchievementDefinitions.Counters.Count == 0, "Unmount retained native definitions.");
        Load();
        Check(doc.OuterXml == saved && ListSF.Instance.User.Achievements.Count == 1, "Missing mod destroyed orphan progress.");
        adapter.ApplyP3Content(); Load();
        Check(GameUtils.AchievementDefinitions.GetAchievementByName("example.phase3:achievements/first").IsUnlocked, "Reinstall/reload lost completion flag.");
        Check(ModProgressionAccess.Advance(id, 1) == 2 && ListSF.Instance.User.Achievements.Count == 1, "Counter duplicated first achievement.");
        Check(ModProgressionAccess.Advance(id, 1) == 3 && ListSF.Instance.User.Achievements.Count == 2, "Third victory did not unlock second achievement.");
        Check(ModProgressionAccess.Advance(id, 1000000000) == 3 && ListSF.Instance.User.Achievements.Count == 2, "Maximum overflowed or repeated unlock.");
        Check(ListSF.Instance.User.AdvanceExternalCounter(id.ToString(), 1, 1) == 3, "Lowered maximum destroyed prior progress.");
        Check(ListSF.Instance.Saves >= 3, "Counter mutations never requested save.");
        Console.WriteLine("PASS: production native achievement parser, P3 adapter, thresholds, save/reload, uninstall/reinstall, idempotency and lower-bound preservation (" + checks + " checks).");
    }
}
