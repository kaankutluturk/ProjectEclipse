using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eclipse.Modding;

// Moveset data files: strict parsing, lossless canonical round-trips, and compilation
// into the same registration transaction that Lua patches and forks use.
internal static class MovesetDataTests
{
    private static int count;
    private static void Check(bool value, string why) { count++; if (!value) throw new Exception(why); }
    private static void Rejects(string json, string fragment)
    {
        try { ModMovesetJson.Parse(json, "fixture.json"); }
        catch (ModContentException exception) { Check(exception.Message.Contains(fragment), "Wrong rejection for " + fragment + ": " + exception.Message); return; }
        throw new Exception("Accepted invalid moveset: " + fragment);
    }

    private const string Full = @"{
  ""schema"": 1,
  ""kind"": ""eclipse.moveset"",
  ""name"": ""Fixture"",
  ""moves"": [
    {
      ""move"": ""HighKick"",
      ""note"": ""faster"",
      ""priority"": { ""expected"": 110, ""value"": 100 },
      ""playback_rate"": { ""expected"": 1.0, ""value"": 1.25 },
      ""animation"": { ""expected"": ""high_kick.bytes"", ""value"": ""front_kick.bytes"" },
      ""input"": { ""expected"": ""Kick"", ""value"": ""Punch"" },
      ""sound_frame"": { ""name"": ""snd_swish5"", ""expected"": 4, ""value"": 3 },
      ""intervals"": [
        { ""select"": { ""name"": ""Uninterrupt"", ""start"": 0, ""end"": 15 }, ""end"": 12 },
        { ""select"": { ""type"": ""Block"", ""start"": 16 }, ""remove"": true },
        { ""add"": { ""type"": ""Invulnerable"", ""name"": ""Dodge"", ""start"": 0, ""end"": 3 } }
      ],
      ""attacks"": [
        { ""id"": 0, ""start"": { ""expected"": 6, ""value"": 5 }, ""damage"": { ""expected"": 0.12, ""value"": 0.2 },
          ""damage_terms"": { ""expected"": { ""UnarmedDamage"": 0 }, ""value"": { ""UnarmedDamage"": -5, ""WeaponDamage"": 0 } },
          ""edges"": { ""expected"": [ ""EThigh_2"", ""ECalf_2"" ], ""value"": [ ""EFoot_2"" ] },
          ""impulse"": { ""expected"": [ 245, 0, 350 ], ""value"": [ 300, 0, 350 ] },
          ""hit"": { ""expected"": ""High"", ""value"": ""Middle"" } }
      ]
    },
    { ""move"": ""WaspFly_150"", ""disable"": true },
    { ""move"": ""FrontKick"", ""animation"": { ""expected"": ""front_kick.bytes"", ""value"": { ""asset"": ""animations/kick"" } } }
  ],
  ""forks"": [
    { ""id"": ""Slash_Ninja"", ""move"": ""KatanaHeavySlash"", ""subtype"": ""NinjaSword"", ""priority"": { ""expected"": 120, ""value"": 121 } },
    { ""id"": ""Slash_Golden"", ""move"": ""KatanaHeavySlash"", ""item"": ""core:items/weapon/weapon_golden_katana"" }
  ]
}";

    private sealed class MemoryProvider : IAssetEnumerableProvider, IAssetByteProvider
    {
        private readonly Dictionary<AssetId, (AssetMetadata Metadata, byte[] Data)> files = new Dictionary<AssetId, (AssetMetadata, byte[])>();
        public ModId Namespace { get; }
        public IReadOnlyList<AssetMetadata> Assets => files.Values.Select(f => f.Metadata).ToList();
        public MemoryProvider(ModId id) { Namespace = id; }
        public void Add(string path, string format, AssetKind kind, string text)
        {
            var id = AssetId.Parse(Namespace + ":" + path);
            var data = Encoding.UTF8.GetBytes(text);
            files[id] = (new AssetMetadata(id, kind, AssetSourceKind.LooseMod, format, data.Length, path + format), data);
        }
        public bool TryDescribe(AssetId id, out AssetMetadata metadata) { metadata = files.TryGetValue(id, out var f) ? f.Metadata : null; return metadata != null; }
        public bool TryRead(AssetId id, out AssetBytes bytes) { bytes = files.TryGetValue(id, out var f) ? new AssetBytes(f.Metadata, f.Data) : null; return bytes != null; }
    }

    private static ModDescriptor Mod(string id, string extra = "") => new ModDescriptor(ModManifestReader.ParseExternal(
        "schema = 1\nid = \"" + id + "\"\nname = \"Fixture\"\nversion = \"1.0.0\"\nauthors = [\"Test\"]\ncapabilities = [\"content.patch\"]\n" + extra), "/fixture/" + id, ModSourceKind.Loose);

    private static (ModContentCatalog, ModDescriptor) Compile(string json, ModContentCatalog catalog = null, string id = "fixture.moves")
    {
        catalog = catalog ?? new ModContentCatalog();
        var mod = Mod(id);
        var provider = new MemoryProvider(mod.Id);
        provider.Add("movesets/moveset", ".json", AssetKind.Text, json);
        provider.Add("animations/kick", ".bytes", AssetKind.Binary, "x");
        var assets = new AssetResolver(new IAssetProvider[] { provider });
        using (var registration = catalog.BeginRegistration(mod))
        {
            ModMovesetLoader.Load(mod, assets, registration);
            registration.Commit();
        }
        return (catalog, mod);
    }

    private static void WorkingCopyChecks()
    {
        var kick = new MovesetBaselineMove { Name = "HighKick", File = "high_kick.bytes", Priority = 110, MidFrames = 2 };
        var uninterrupt = new MovesetBaselineInterval { Name = "Uninterrupt", Start = 0, End = 15 };
        var block = new MovesetBaselineInterval { Type = "Block", Start = 16 };
        var attack = new MovesetBaselineAttack { Id = 0, Damage = 0.12, Hit = "High", Impulse = new double[] { 245, 0, 350 } };
        attack.Terms["UnarmedDamage"] = 0; attack.Edges.AddRange(new[] { "EThigh_2", "EFoot_2" });
        kick.Intervals.AddRange(new[] { uninterrupt, block, new MovesetBaselineInterval { Type = "Attack", Start = 6, End = 8, Attack = attack } });
        var copy = new MovesetWorkingCopy("lab.mod");
        int changes = 0; copy.Changed += () => changes++;
        copy.Edit(c => c.SetPlaybackRate("HighKick", 1.25));
        copy.Edit(c => c.SetPriority("HighKick", 110, 100));
        copy.Edit(c => c.SetIntervalBounds("HighKick", uninterrupt, 0, 12));
        copy.Edit(c => c.SetIntervalRemoved("HighKick", block, true));
        copy.Edit(c => c.SetAttack("HighKick", attack, 6, 8, 6, 8, 0.2, null, new[] { "EFoot_2" }, null, "Middle"));
        var entry = copy.Document.Moves.Single();
        Check(entry.PlaybackRate.Value == 1.25 && entry.Priority.Value == 100 && entry.Intervals.Count == 2 && entry.Attacks.Single().Damage.Value == 0.2 &&
            entry.Attacks.Single().Start == null && entry.Attacks.Single().Hit.Value == "Middle" && changes == 5 && copy.IsDirty, "Working copy edits were not recorded minimally.");
        copy.Edit(c => c.SetPriority("HighKick", 110, 110));
        Check(copy.Document.Moves.Single().Priority == null, "Setting a value back to its baseline kept the edit.");
        copy.Undo();
        Check(copy.Document.Moves.Single().Priority.Value == 100, "Undo did not restore the previous edit.");
        copy.Redo();
        Check(copy.Document.Moves.Single().Priority == null && copy.CanUndo, "Redo did not reapply the edit.");
        copy.Edit(c => { c.SetPlaybackRate("HighKick", 1.0); c.SetIntervalBounds("HighKick", uninterrupt, 0, 15); c.SetIntervalRemoved("HighKick", block, false);
            c.SetAttack("HighKick", attack, 6, 8, 6, 8, 0.12, null, new[] { "EThigh_2", "EFoot_2" }, null, "High"); });
        Check(copy.Document.Moves.Count == 0, "A move with every edit reverted was not pruned.");
        string fork = null;
        copy.Edit(c => fork = c.ForkName(c.CreateSubtypeFork("KatanaHeavySlash", "NinjaSword")));
        Check(fork == "lab.mod.KatanaHeavySlash_NinjaSword" && copy.NativeSource(fork) == "KatanaHeavySlash", "Fork naming or source lookup is wrong.");
        string golden = null;
        copy.Edit(c => golden = c.ForkName(c.CreateItemFork(fork, "core:items/weapon/weapon_golden_katana")));
        copy.Edit(c => c.SetPlaybackRate(golden, 0.75));
        Check(copy.NativeSource(golden) == "KatanaHeavySlash" && copy.Entry(golden, false).PlaybackRate.Value == 0.75, "Stacked fork edits were not stored on the fork.");
        copy.Edit(c => c.RemoveFork(fork));
        Check(copy.Document.Forks.Count == 0, "Removing a fork kept forks that copy it.");
        // Writing a data-only mod folder.
        string root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "eclipse-lab-" + Guid.NewGuid().ToString("N"));
        try
        {
            copy.Edit(c => c.SetPriority("HighKick", 110, 120));
            string folder = MovesetModWriter.Save(root, "lab.mod", "My Lab", copy.Document, false);
            var manifest = ModManifestReader.ReadExternalFile(System.IO.Path.Combine(folder, "mod.toml"));
            Check(!manifest.HasEntrypoint && manifest.Capabilities.Contains("content.patch") && manifest.Name == "My Lab", "Saved manifest is not a data-only content.patch mod.");
            Check(ModMovesetJson.Write(MovesetModWriter.Load(root, "lab.mod")) == copy.Json, "Saved moveset does not reload identically.");
            Check(!System.IO.Directory.GetFiles(folder, "*.eclipse-write", System.IO.SearchOption.AllDirectories).Any(), "Atomic write left a temporary file.");
            // A fork of another mod's weapon adds that mod as a dependency; the author's text stays.
            string manifestPath = System.IO.Path.Combine(folder, "mod.toml");
            System.IO.File.WriteAllText(manifestPath, System.IO.File.ReadAllText(manifestPath).Replace("version = \"1.0.0\"\n", "version = \"1.0.0\"\n# keep this comment\n"));
            MovesetModWriter.Save(root, "lab.mod", "Ignored", copy.Document, true, new Dictionary<string, string> { ["author.blades"] = "1.2.0" });
            manifest = ModManifestReader.ReadExternalFile(manifestPath);
            Check(manifest.Name == "My Lab" && manifest.Dependencies.Any(d => d.Id.Value == "core") &&
                manifest.Dependencies.Any(d => d.Id.Value == "author.blades" && d.Version.ToString() == ">=1.2.0"), "Item owner dependency was not added.");
            Check(System.IO.File.ReadAllText(manifestPath).Contains("# keep this comment"), "Saving rewrote the author's manifest text.");
            // A data-only mod without content.patch gains it in place.
            string bare = System.IO.Path.Combine(root, "bare.mod");
            System.IO.Directory.CreateDirectory(bare);
            System.IO.File.WriteAllText(System.IO.Path.Combine(bare, "mod.toml"),
                "schema = 1\nid = \"bare.mod\"\nname = \"Bare\"\nversion = \"2.0.0\"\nauthors = [\"Someone\"]\ncapabilities = []\n");
            MovesetModWriter.Save(root, "bare.mod", "Ignored", copy.Document, false);
            manifest = ModManifestReader.ReadExternalFile(System.IO.Path.Combine(bare, "mod.toml"));
            Check(manifest.Capabilities.Contains("content.patch") && manifest.Version.ToString() == "2.0.0" && manifest.Name == "Bare", "content.patch was not added in place.");
        }
        finally { if (System.IO.Directory.Exists(root)) System.IO.Directory.Delete(root, true); }
    }

    public static void Main()
    {
        var document = ModMovesetJson.Parse(Full, "fixture.json");
        Check(document.Moves.Count == 3 && document.Forks.Count == 2, "Document lists were not read.");
        var kick = document.Moves[0];
        Check(kick.PlaybackRate.Value == 1.25 && kick.Intervals.Count == 3 && kick.Attacks.Single().Edges.Value.Single() == "EFoot_2" &&
            kick.Intervals[1].Kind == ModMoveIntervalEditKind.Remove && kick.Intervals[1].Select.End == null, "Move fields were not read.");
        Check(document.Moves[2].Animation.AssetValue == "animations/kick" && document.Forks[1].Item == "core:items/weapon/weapon_golden_katana", "Asset or fork fields were not read.");
        string canonical = ModMovesetJson.Write(document);
        Check(ModMovesetJson.Write(ModMovesetJson.Parse(canonical, "canonical.json")) == canonical, "Canonical writing is not lossless.");
        Check(canonical.Contains("\"value\": 1.25") && canonical.Contains("\"expected\": 110"), "Numbers were not written invariantly.");
        Check(canonical.IndexOf("\"WeaponDamage\"", StringComparison.Ordinal) < canonical.IndexOf("\"UnarmedDamage\": -5", StringComparison.Ordinal), "Damage terms were not written in canonical order.");
        Check(!canonical.Contains("\r") && canonical.EndsWith("\n"), "Canonical text must use LF and end with a newline.");

        const string head = "{\"schema\":1,\"kind\":\"eclipse.moveset\",";
        Rejects(head + "\"moves\":[{\"move\":\"A\",\"move\":\"B\",\"disable\":true}]}", "not valid JSON");
        Rejects(head + "\"moves\":[{\"move\":\"A\",\"colour\":1}]}", "unknown field 'colour'");
        Rejects(head + "\"moves\":[{\"move\":\"A\",\"priority\":{\"expected\":\"1\",\"value\":2}}]}", "must be an integer");
        Rejects(head + "\"moves\":[{\"move\":\"A\"}]}", "must change at least one field");
        Rejects("{\"schema\":2,\"kind\":\"eclipse.moveset\"}", "schema must be 1");
        Rejects("{\"schema\":1,\"kind\":\"other\"}", "kind must be");
        Rejects(head + "\"forks\":[{\"id\":\"F\",\"move\":\"A\",\"subtype\":\"Katana\",\"item\":\"core:items/weapon/weapon_katana\"}]}", "exactly one of subtype or item");
        Rejects(head + "\"moves\":[{\"move\":\"A\",\"attacks\":[{\"id\":0,\"damage_terms\":{\"expected\":{\"UnarmedDamage\":0},\"value\":{\"MagicDamage\":0}}}]}]}", "MagicDamage");
        Rejects(head + "\"moves\":[{\"move\":\"A\",\"intervals\":[{\"select\":{\"type\":\"Attack\",\"start\":0},\"remove\":true}]}]}", "attacks by id");
        Rejects(head + "\"moves\":[{\"move\":\"core:moves/a\",\"disable\":true}]}", "exact native name");
        Rejects(head + "\"moves\":[{\"move\":\"A\",\"playback_rate\":{\"expected\":1.0,\"value\":3.0}}]}", "playback_rate must be");

        // A data-only manifest has no entrypoint.
        var dataOnly = Mod("fixture.data");
        Check(!dataOnly.Manifest.HasEntrypoint, "Manifest without entrypoint was not accepted as data-only.");

        // Compile a document without forks (forks need core items) into the transaction path.
        string movesOnly = ModMovesetJson.Write(new Func<ModMovesetDocument>(() => { var d = ModMovesetJson.Parse(Full); d.Forks.Clear(); return d; })());
        var (catalog, mod) = Compile(movesOnly);
        var patches = catalog.MoveCombatPatches;
        Check(patches.Count == 3 && patches[0].Owner == mod.Id, "Moves were not registered as patches.");
        var highKick = patches.Single(p => p.MoveName == "HighKick");
        Check(highKick.Extras.PlaybackRate.Value == 1250 && highKick.Priority.Value == 100 && highKick.Animation.NativeValue == "front_kick.bytes" &&
            highKick.Input.Value.Key == "Punch" && highKick.SoundFrame.Value == 3 && highKick.Extras.Attacks.Single().Damage.Value == 0.2, "Patch fields were not compiled.");
        Check(patches.Single(p => p.MoveName == "FrontKick").Animation.Value.ToString() == "fixture.moves:animations/kick", "Mod animation asset was not qualified.");
        Check(patches.Single(p => p.MoveName == "WaspFly_150").Disable, "Disable was not compiled.");
        string fingerprint = ModSaveData.ComputeContentSetFingerprint(new[] { mod }, catalog);
        Check(fingerprint == ModSaveData.ComputeContentSetFingerprint(new[] { mod }, Compile(movesOnly).Item1), "Fingerprint is not deterministic.");
        Check(fingerprint != ModSaveData.ComputeContentSetFingerprint(new[] { mod }, Compile(movesOnly.Replace("\"value\": 1.25", "\"value\": 1.5")).Item1),
            "Fingerprint ignores the playback rate.");
        // A second mod cannot edit the same move.
        bool conflict = false;
        try { Compile(movesOnly, catalog, "fixture.other"); } catch (ModContentException exception) { conflict = exception.Message.Contains("already owned"); }
        Check(conflict, "Two mods patched one move.");

        // Subtype forks register the copy and remove the subtype from the source lock group.
        var forkCatalog = Compile(head + "\"forks\":[{\"id\":\"Slash_Ninja\",\"move\":\"KatanaHeavySlash\",\"subtype\":\"NinjaSword\",\"priority\":{\"expected\":120,\"value\":121}}]}").Item1;
        var fork = forkCatalog.MoveForks.Single();
        Check(fork.RuntimeName == "fixture.moves.Slash_Ninja" && fork.Source == "KatanaHeavySlash" && fork.Subtype == "NinjaSword", "Fork was not registered.");
        Check(forkCatalog.MoveItemLockRemovals.Single().MoveName == "KatanaHeavySlash" && forkCatalog.MoveItemLockRemovals.Single().Subtype == "NinjaSword",
            "Fork did not remove its subtype from the source.");
        Check(forkCatalog.MoveCombatPatches.Single().MoveName == fork.RuntimeName, "Fork edits did not target the fork.");
        conflict = false;
        try { Compile(head + "\"forks\":[{\"id\":\"Other\",\"move\":\"KatanaHeavySlash\",\"subtype\":\"NinjaSword\"}]}", forkCatalog, "fixture.rival"); }
        catch (ModContentException exception) { conflict = exception.Message.Contains("already owned"); }
        Check(conflict, "Two mods forked one move for the same subtype.");
        WorkingCopyChecks();
        Console.WriteLine("PASS: " + count + " moveset data checks.");
    }
}
