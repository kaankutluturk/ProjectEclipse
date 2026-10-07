using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Eclipse.AssetPacker;

// Bulk workflow over every catalog group: unpack all archives into one editable workspace,
// regroup/delete logical assets by address, then repack changed groups and rebuild catalog.json.
internal static partial class Program
{
    private const string WorkspaceFile = "workspace.json";
    private const string DefaultCatalog = "Assets/Resources/SF2Content/Art/catalog.json";
    private const string DefaultBundles = "Assets/StreamingAssets/SF2Content/ArtBundles";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static readonly string[] RefExtensions = [".xml", ".cs", ".lua", ".json", ".txt", ".csv", ".ini"];

    private static int RunWorkspaceCommand(string[] args)
    {
        var options = new CommandOptions(args.Skip(1));
        string command = args[0].ToLowerInvariant();
        switch (command)
        {
            case "unpack-all" when options.Positional.Count == 1:
                UnpackAll(options.Positional[0], options);
                return 0;
            case "check" when options.Positional.Count == 1:
                return CheckWorkspace(LoadWorkspace(options.Positional[0]), verbose: true) ? 0 : 1;
            case "report" when options.Positional.Count == 2:
                Report(LoadWorkspace(options.Positional[0]), options.Positional[1], options);
                return 0;
            case "move" when options.Positional.Count == 3:
                MoveAssets(LoadWorkspace(options.Positional[0]), options.Positional[1], options.Positional[2], options);
                return 0;
            case "delete" when options.Positional.Count == 2:
                DeleteAssets(LoadWorkspace(options.Positional[0]), options.Positional[1], options);
                return 0;
            case "prune" when options.Positional.Count == 1:
                Prune(LoadWorkspace(options.Positional[0]), options.Has("dry-run"));
                return 0;
            case "repack-all" when options.Positional.Count == 1:
                return RepackAll(LoadWorkspace(options.Positional[0]), options);
            default:
                return Usage();
        }
    }

    // ---------------------------------------------------------------- unpack

    private static void UnpackAll(string workspacePath, CommandOptions options)
    {
        string root = Path.GetFullPath(workspacePath);
        string catalogPath = Path.GetFullPath(options.Value("catalog") ?? DefaultCatalog);
        string bundleDirectory = Path.GetFullPath(options.Value("bundles") ?? DefaultBundles);
        if (File.Exists(Path.Combine(root, WorkspaceFile)))
            throw new InvalidDataException("Workspace already exists (delete it first): " + root);
        if (Directory.Exists(root) && Directory.EnumerateFileSystemEntries(root).Any())
            throw new InvalidDataException("Workspace directory is not empty: " + root);

        JsonObject catalog = ReadJsonObject(catalogPath);
        HashSet<string>? only = options.NameSet("only");
        var workspace = new WorkspaceState(root)
        {
            CatalogPath = catalogPath,
            BundleDirectory = bundleDirectory,
        };

        foreach (JsonObject record in CatalogRecords(catalog))
        {
            string name = (string?)record["name"] ?? throw new InvalidDataException("Catalog group without name.");
            string file = (string?)record["file"] ?? string.Empty;
            bool unpack = file.Length > 0 && (only == null || only.Contains(name));
            var group = new WorkspaceGroup(name, (JsonObject)record.DeepClone(), unpack);
            workspace.Groups.Add(group);
            if (!unpack)
            {
                Console.WriteLine($"keep    {name}" + (file.Length == 0 ? " (no archive)" : " (not unpacked)"));
                continue;
            }
            string archive = Path.Combine(bundleDirectory, file.Replace('/', Path.DirectorySeparatorChar));
            Console.WriteLine($"unpack  {name}");
            Extract(archive, Path.Combine(root, name));
            group.Fingerprint = Fingerprint(Path.Combine(root, name));
        }

        // Remember which content currently wins for addresses shipped by several groups, so a
        // regroup that silently changes lookup priority can be reported by `check`.
        if (workspace.Groups.All(g => g.Unpacked || string.IsNullOrEmpty((string?)g.Record["file"])))
        {
            var groups = workspace.Groups.Where(g => g.Unpacked).Select(g => LoadGroup(workspace, g.Name)).ToList();
            foreach (var pair in AddressOwners(groups).Where(x => x.Value.Count > 1))
                workspace.Winners[pair.Key] = AddressHash(pair.Value[0], pair.Key);
        }
        else
        {
            Console.WriteLine("Partial unpack: lookup-priority tracking for duplicate addresses is disabled.");
        }

        SaveWorkspace(workspace);
        Console.WriteLine($"Workspace ready: {root}");
        Console.WriteLine("Next: edit, then `check`, then `repack-all`.");
    }

    // ---------------------------------------------------------------- check

    private static bool CheckWorkspace(WorkspaceState workspace, bool verbose)
    {
        int errors = 0, warnings = 0;
        void Error(string message) { errors++; Console.WriteLine("ERROR   " + message); }
        void Warn(string message) { warnings++; Console.WriteLine("WARN    " + message); }

        var loaded = new List<LoadedGroup>();
        foreach (string name in CurrentOrder(workspace))
        {
            WorkspaceGroup? known = workspace.Find(name);
            if (known != null && !known.Unpacked)
                continue;
            string directory = Path.Combine(workspace.Root, name);
            if (!SafeGroupName(name))
            {
                Error($"{name}: group folder names may only use letters, digits, '_' and '-'");
                continue;
            }
            LoadedGroup group;
            try
            {
                group = LoadGroup(workspace, name);
                if (group.Descriptors.Count > 0) ValidateBundle(ScanDirectory(directory));
            }
            catch (Exception exception)
            {
                Error($"{name}: {exception.Message}");
                continue;
            }
            loaded.Add(group);
            int orphans = group.OrphanPayloads().Count();
            if (orphans > 0) Warn($"{name}: {orphans} unreferenced payload file(s) (run `prune`)");
            if (known == null && verbose) Console.WriteLine($"new     {name} (appended after existing groups)");
            if (verbose && known != null && known.Fingerprint != Fingerprint(directory))
                Console.WriteLine($"changed {name}");
        }
        foreach (WorkspaceGroup group in workspace.Groups.Where(g => g.Unpacked && !Directory.Exists(Path.Combine(workspace.Root, g.Name))))
            Warn($"{group.Name}: folder deleted; the group and its archive will be removed");
        foreach (LoadedGroup group in loaded.Where(g => g.Descriptors.Count == 0))
            Warn($"{group.Name}: no asset descriptors left; it will be removed");

        // Lookup priority: the first group in catalog order that ships an address wins.
        Dictionary<string, List<LoadedGroup>> owners = AddressOwners(loaded);
        Dictionary<string, string> packedOwners = PackedOwners(workspace);
        foreach (var pair in workspace.Winners)
        {
            string address = pair.Key;
            string? current = FirstOwner(workspace, address, owners, packedOwners, out LoadedGroup? winner);
            if (current == null)
                Warn($"{address}: shipped by several groups originally, now by none");
            else if (winner != null && AddressHash(winner, address) != pair.Value)
                Warn($"{address}: lookup now resolves to different content (winner: {current}); check group order");
        }
        int duplicates = owners.Count(x => x.Value.Count > 1);
        if (verbose && duplicates > 0)
            Console.WriteLine($"info    {duplicates} address(es) are shipped by more than one unpacked group (first group in order wins)");

        Console.WriteLine(errors == 0
            ? $"OK: {loaded.Count} unpacked group(s), {warnings} warning(s)"
            : $"FAILED: {errors} error(s), {warnings} warning(s)");
        return errors == 0;
    }

    // ---------------------------------------------------------------- report

    private static void Report(WorkspaceState workspace, string outputPath, CommandOptions options)
    {
        HashSet<string>? tokens = null;
        List<string>? refRoots = options.Values("refs");
        if (refRoots.Count > 0)
            tokens = CollectReferenceTokens(refRoots);

        var output = new StringBuilder();
        output.AppendLine("group,type,address,name,descriptor,payload,payload_bytes,name_mentioned");
        int rows = 0;
        foreach (string name in CurrentOrder(workspace))
        {
            WorkspaceGroup? known = workspace.Find(name);
            if (known != null && !known.Unpacked)
            {
                foreach (string address in RecordAddresses(known.Record))
                {
                    output.AppendLine(Csv(name, "(packed)", address, string.Empty, string.Empty, string.Empty, string.Empty,
                        Mentioned(tokens, address, string.Empty)));
                    rows++;
                }
                continue;
            }
            LoadedGroup group = LoadGroup(workspace, name);
            foreach (Descriptor descriptor in group.Descriptors.OrderBy(d => d.Meta.Address, StringComparer.OrdinalIgnoreCase))
            {
                string payload = descriptor.Meta.References.FirstOrDefault() ?? string.Empty;
                string payloadPath = Path.Combine(group.Directory, payload);
                long bytes = File.Exists(payloadPath) ? new FileInfo(payloadPath).Length : -1;
                output.AppendLine(Csv(name, descriptor.Meta.Type, descriptor.Meta.Address, descriptor.Meta.Name,
                    descriptor.Path, payload, bytes.ToString(),
                    Mentioned(tokens, descriptor.Meta.Address, descriptor.Meta.Name)));
                rows++;
            }
        }
        File.WriteAllText(Path.GetFullPath(outputPath), output.ToString(), new UTF8Encoding(true));
        Console.WriteLine($"Wrote {rows} row(s) -> {Path.GetFullPath(outputPath)}");
        if (tokens != null)
            Console.WriteLine("name_mentioned is a text-search hint only; names built at runtime will show 'no'.");
    }

    private static string Mentioned(HashSet<string>? tokens, string address, string name)
    {
        if (tokens == null) return string.Empty;
        string last = address[(address.LastIndexOf('/') + 1)..];
        return tokens.Contains(address) || tokens.Contains(last) || (name.Length > 0 && tokens.Contains(name)) ? "yes" : "no";
    }

    private static HashSet<string> CollectReferenceTokens(IEnumerable<string> roots)
    {
        var tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var splitter = new Regex(@"[^A-Za-z0-9_\-./]+", RegexOptions.Compiled);
        foreach (string root in roots)
        {
            string full = Path.GetFullPath(root);
            if (!Directory.Exists(full)) throw new DirectoryNotFoundException(full);
            foreach (string path in Directory.EnumerateFiles(full, "*", SearchOption.AllDirectories))
            {
                if (!RefExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)) continue;
                if (Path.GetFileName(path).Equals("catalog.json", StringComparison.OrdinalIgnoreCase)) continue;
                if (path.Contains(Path.DirectorySeparatorChar + "node_modules" + Path.DirectorySeparatorChar)) continue;
                foreach (string token in splitter.Split(File.ReadAllText(path)))
                {
                    if (token.Length == 0) continue;
                    tokens.Add(token.Trim('.', '/'));
                    foreach (string part in token.Split('/', '.'))
                        if (part.Length > 0) tokens.Add(part);
                    int slash = token.LastIndexOf('/');
                    if (slash >= 0) tokens.Add(token[(slash + 1)..]);
                }
            }
        }
        return tokens;
    }

    private static string Csv(params string[] values)
    {
        return string.Join(",", values.Select(v =>
            v.IndexOfAny([',', '"', '\n', '\r']) >= 0 ? "\"" + v.Replace("\"", "\"\"") + "\"" : v));
    }

    // ---------------------------------------------------------------- move / delete / prune

    private static void MoveAssets(WorkspaceState workspace, string pattern, string targetName, CommandOptions options)
    {
        bool dryRun = options.Has("dry-run");
        if (!SafeGroupName(targetName))
            throw new InvalidDataException("Group names may only use letters, digits, '_' and '-': " + targetName);
        WorkspaceGroup? knownTarget = workspace.Find(targetName);
        if (knownTarget != null && !knownTarget.Unpacked)
            throw new InvalidDataException(targetName + " was not unpacked into this workspace.");
        Func<string, bool> matcher = AddressMatcher(pattern);
        string? from = options.Value("from");
        List<string> order = CurrentOrder(workspace);

        string targetDirectory = Path.Combine(workspace.Root, targetName);
        bool createdTarget = !Directory.Exists(targetDirectory);
        LoadedGroup target = Directory.Exists(targetDirectory)
            ? LoadGroup(workspace, targetName)
            : new LoadedGroup(targetName, targetDirectory, []);
        int targetRank = order.FindIndex(n => n.Equals(targetName, StringComparison.OrdinalIgnoreCase));
        if (targetRank < 0) targetRank = int.MaxValue;
        // Lookup priority of each asset now in the target: the rank of the group it came from.
        var ranks = target.Descriptors.ToDictionary(d => d.Key, _ => targetRank);
        var replacedPayloads = new List<string>();

        int moved = 0, dropped = 0, replaced = 0;
        foreach (string sourceName in order.Where(n => !n.Equals(targetName, StringComparison.OrdinalIgnoreCase)))
        {
            if (from != null && !sourceName.Equals(from, StringComparison.OrdinalIgnoreCase)) continue;
            WorkspaceGroup? known = workspace.Find(sourceName);
            if (known != null && !known.Unpacked) continue;
            LoadedGroup source = LoadGroup(workspace, sourceName);
            List<Descriptor> matches = source.Descriptors.Where(d => matcher(d.Meta.Address)).ToList();
            if (matches.Count == 0) continue;
            int sourceRank = order.FindIndex(n => n.Equals(sourceName, StringComparison.OrdinalIgnoreCase));
            var payloadMap = new Dictionary<string, string>(PathComparer);

            foreach (Descriptor descriptor in matches)
            {
                Descriptor? existing = target.Descriptors.FirstOrDefault(d => d.Key == descriptor.Key);
                if (existing != null && ranks[descriptor.Key] < sourceRank)
                {
                    // Target already ships this exact asset with higher lookup priority.
                    Console.WriteLine($"drop    {sourceName}:{descriptor.Meta.Address} ({descriptor.Meta.Name}) - higher-priority copy already in {targetName}");
                    if (!dryRun) source.Remove(descriptor);
                    dropped++;
                    continue;
                }
                if (existing != null)
                {
                    Console.WriteLine($"replace {targetName}:{descriptor.Meta.Address} ({descriptor.Meta.Name}) with higher-priority copy from {sourceName}");
                    if (!dryRun) { target.Remove(existing); replacedPayloads.AddRange(existing.Meta.References); }
                    replaced++;
                }
                Console.WriteLine($"move    {sourceName} -> {targetName}  {descriptor.Meta.Address} ({descriptor.Meta.Name})");
                moved++;
                ranks[descriptor.Key] = sourceRank;
                if (dryRun) continue;

                string text = descriptor.Text;
                foreach (string reference in descriptor.Meta.References)
                {
                    if (!payloadMap.TryGetValue(reference, out string? destination))
                    {
                        destination = target.PlaceFile(Path.Combine(source.Directory, reference), reference);
                        payloadMap[reference] = destination;
                    }
                    if (!destination.Equals(reference, StringComparison.Ordinal))
                        text = RewriteReference(text, descriptor.Meta.Type, destination);
                }
                string descriptorPath = target.UniquePath(descriptor.Path);
                target.WriteDescriptor(descriptorPath, text);
                source.Remove(descriptor);
            }
            if (!dryRun)
            {
                source.DeleteUnreferenced(matches.SelectMany(d => d.Meta.References));
                if (source.Descriptors.Count == 0)
                    Console.WriteLine($"note    {sourceName} has no assets left; `prune` removes the folder");
            }
        }
        if (!dryRun && createdTarget && target.Descriptors.Count > 0)
            Console.WriteLine($"note    created new group {targetName}; it is appended after existing groups (edit workspace.json order to change priority)");
        if (!dryRun && replacedPayloads.Count > 0)
            target.DeleteUnreferenced(replacedPayloads);
        Console.WriteLine($"{(dryRun ? "Would move" : "Moved")} {moved} descriptor(s); {dropped} duplicate(s) dropped, {replaced} replaced.");
    }

    private static void DeleteAssets(WorkspaceState workspace, string pattern, CommandOptions options)
    {
        bool dryRun = options.Has("dry-run");
        Func<string, bool> matcher = AddressMatcher(pattern);
        string? from = options.Value("from");
        int deleted = 0;
        foreach (string name in CurrentOrder(workspace))
        {
            if (from != null && !name.Equals(from, StringComparison.OrdinalIgnoreCase)) continue;
            WorkspaceGroup? known = workspace.Find(name);
            if (known != null && !known.Unpacked) continue;
            LoadedGroup group = LoadGroup(workspace, name);
            List<Descriptor> matches = group.Descriptors.Where(d => matcher(d.Meta.Address)).ToList();
            foreach (Descriptor descriptor in matches)
            {
                Console.WriteLine($"delete  {name}:{descriptor.Meta.Address} ({descriptor.Meta.Name})");
                if (!dryRun) group.Remove(descriptor);
                deleted++;
            }
            if (!dryRun && matches.Count > 0)
                group.DeleteUnreferenced(matches.SelectMany(d => d.Meta.References));
        }
        Console.WriteLine($"{(dryRun ? "Would delete" : "Deleted")} {deleted} descriptor(s).");
    }

    private static void Prune(WorkspaceState workspace, bool dryRun)
    {
        int files = 0, groups = 0;
        foreach (string name in CurrentOrder(workspace))
        {
            WorkspaceGroup? known = workspace.Find(name);
            if (known != null && !known.Unpacked) continue;
            LoadedGroup group = LoadGroup(workspace, name);
            if (group.Descriptors.Count == 0)
            {
                Console.WriteLine($"remove  {name}/ (no asset descriptors)");
                if (!dryRun) Directory.Delete(group.Directory, recursive: true);
                groups++;
                continue;
            }
            foreach (string orphan in group.OrphanPayloads().ToList())
            {
                Console.WriteLine($"remove  {name}/{orphan}");
                if (!dryRun) File.Delete(Path.Combine(group.Directory, orphan));
                files++;
            }
            if (!dryRun) RemoveEmptyDirectories(group.Directory);
        }
        Console.WriteLine($"{(dryRun ? "Would remove" : "Removed")} {files} unreferenced file(s) and {groups} empty group(s).");
    }

    // ---------------------------------------------------------------- repack

    private static int RepackAll(WorkspaceState workspace, CommandOptions options)
    {
        bool dryRun = options.Has("dry-run");
        string catalogPath = Path.GetFullPath(options.Value("catalog") ?? workspace.CatalogPath);
        string bundleDirectory = Path.GetFullPath(options.Value("bundles") ?? workspace.BundleDirectory);
        if (!CheckWorkspace(workspace, verbose: false))
        {
            Console.WriteLine("Fix the errors above before repacking.");
            return 1;
        }

        JsonObject catalog = ReadJsonObject(catalogPath);
        var oldArchives = CatalogRecords(catalog)
            .Select(r => (string?)r["file"] ?? string.Empty).Where(f => f.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        string staging = Path.Combine(workspace.Root, ".staging");
        if (!dryRun)
        {
            if (Directory.Exists(staging)) Directory.Delete(staging, recursive: true);
            Directory.CreateDirectory(staging);
        }

        var records = new JsonArray();
        var staged = new List<(string File, string StagedPath)>();
        var nextGroups = new List<WorkspaceGroup>();
        foreach (string name in CurrentOrder(workspace))
        {
            WorkspaceGroup? known = workspace.Find(name);
            if (known != null && !known.Unpacked)
            {
                records.Add(known.Record.DeepClone());
                nextGroups.Add(known);
                continue;
            }
            LoadedGroup group = LoadGroup(workspace, name);
            if (group.Descriptors.Count == 0)
            {
                Console.WriteLine($"remove  {name} (empty)");
                continue;
            }
            string fingerprint = Fingerprint(group.Directory);
            string file = name + ".tar.lz4";
            string archive = Path.Combine(bundleDirectory, file);
            JsonObject record;
            // Keep the previous address order so catalog diffs only show real changes.
            var present = group.Descriptors.Select(d => d.Meta.Address).ToHashSet(StringComparer.OrdinalIgnoreCase);
            List<string> previous = known == null ? [] : RecordAddresses(known.Record).Where(present.Contains).ToList();
            var previousSet = previous.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var assets = new JsonArray();
            foreach (string address in previous.Concat(present.Where(a => !previousSet.Contains(a)).OrderBy(a => a, StringComparer.Ordinal)))
                assets.Add(new JsonObject
                {
                    ["address"] = address, ["texture"] = "", ["sprites"] = "", ["audio"] = "", ["font"] = "",
                });
            if (known != null && known.Fingerprint == fingerprint && (string?)known.Record["file"] == file &&
                File.Exists(archive) && new FileInfo(archive).Length == (long?)known.Record["size"] &&
                Hash(archive) == (string?)known.Record["sha256"])
            {
                Console.WriteLine($"same    {name}");
                record = (JsonObject)known.Record.DeepClone();
            }
            else
            {
                Console.WriteLine($"pack    {name}");
                record = new JsonObject
                {
                    ["name"] = name,
                    ["namespaceId"] = (string?)known?.Record["namespaceId"] ?? group.Namespace(),
                    ["file"] = file,
                };
                if (!dryRun)
                {
                    string stagedPath = Path.Combine(staging, file);
                    Pack(group.Directory, stagedPath);
                    record["sha256"] = Hash(stagedPath);
                    record["size"] = new FileInfo(stagedPath).Length;
                    record["unpackedSize"] = MeasureDecodedLength(stagedPath);
                    staged.Add((file, stagedPath));
                }
            }
            record["assets"] = assets;
            records.Add(record);
            nextGroups.Add(new WorkspaceGroup(name, record, unpacked: true) { Fingerprint = fingerprint });
        }

        var newArchives = records.Select(r => (string?)r!["file"] ?? string.Empty).Where(f => f.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        List<string> stale = oldArchives.Where(f => !newArchives.Contains(f)).ToList();
        foreach (string file in stale)
            Console.WriteLine($"delete  {file} (group removed)");
        if (dryRun)
        {
            Console.WriteLine($"Dry run: {records.Count} group(s) would be written to {catalogPath}");
            return 0;
        }

        foreach ((string file, string stagedPath) in staged)
            File.Move(stagedPath, Path.Combine(bundleDirectory, file), overwrite: true);
        foreach (string file in stale)
        {
            string path = Path.Combine(bundleDirectory, file.Replace('/', Path.DirectorySeparatorChar));
            File.Delete(path);
            File.Delete(path + ".meta");
        }
        Directory.Delete(staging, recursive: true);

        catalog["bundles"] = records;
        File.WriteAllText(catalogPath, catalog.ToJsonString(JsonOptions) + Environment.NewLine, new UTF8Encoding(false));
        workspace.Groups.Clear();
        workspace.Groups.AddRange(nextGroups);
        workspace.CatalogPath = catalogPath;
        workspace.BundleDirectory = bundleDirectory;
        SaveWorkspace(workspace);
        Console.WriteLine($"Repacked {staged.Count} archive(s), removed {stale.Count}; catalog has {records.Count} group(s).");
        if (staged.Count > 0)
            Console.WriteLine("New archives get their Unity .meta file on the next editor import.");
        return 0;
    }

    // ---------------------------------------------------------------- workspace model

    private static List<string> CurrentOrder(WorkspaceState workspace)
    {
        var order = new List<string>();
        foreach (WorkspaceGroup group in workspace.Groups)
            if (!group.Unpacked || Directory.Exists(Path.Combine(workspace.Root, group.Name)))
                order.Add(group.Name);
        var known = new HashSet<string>(workspace.Groups.Select(g => g.Name), StringComparer.OrdinalIgnoreCase);
        foreach (string directory in Directory.GetDirectories(workspace.Root).Select(Path.GetFileName).OfType<string>()
                     .Where(n => !n.StartsWith('.') && !known.Contains(n)).OrderBy(n => n, StringComparer.Ordinal))
            order.Add(directory);
        return order;
    }

    private static Dictionary<string, List<LoadedGroup>> AddressOwners(IEnumerable<LoadedGroup> groups)
    {
        var owners = new Dictionary<string, List<LoadedGroup>>(StringComparer.OrdinalIgnoreCase);
        foreach (LoadedGroup group in groups)
            foreach (string address in group.Descriptors.Select(d => d.Meta.Address).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!owners.TryGetValue(address, out List<LoadedGroup>? list))
                    owners[address] = list = [];
                list.Add(group);
            }
        return owners;
    }

    private static Dictionary<string, string> PackedOwners(WorkspaceState workspace)
    {
        var owners = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (WorkspaceGroup group in workspace.Groups.Where(g => !g.Unpacked))
            foreach (string address in RecordAddresses(group.Record))
                owners.TryAdd(address, group.Name);
        return owners;
    }

    private static string? FirstOwner(WorkspaceState workspace, string address,
        Dictionary<string, List<LoadedGroup>> owners, Dictionary<string, string> packedOwners, out LoadedGroup? winner)
    {
        winner = null;
        owners.TryGetValue(address, out List<LoadedGroup>? unpacked);
        packedOwners.TryGetValue(address, out string? packed);
        foreach (string name in CurrentOrder(workspace))
        {
            LoadedGroup? match = unpacked?.FirstOrDefault(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (match != null) { winner = match; return name; }
            if (packed != null && packed.Equals(name, StringComparison.OrdinalIgnoreCase)) return name;
        }
        return null;
    }

    // Content identity of one address inside one group, independent of payload file names.
    private static string AddressHash(LoadedGroup group, string address)
    {
        var builder = new StringBuilder();
        foreach (Descriptor descriptor in group.Descriptors
                     .Where(d => d.Meta.Address.Equals(address, StringComparison.OrdinalIgnoreCase))
                     .OrderBy(d => d.Meta.Type, StringComparer.Ordinal).ThenBy(d => d.Meta.Name, StringComparer.Ordinal))
        {
            foreach (string line in descriptor.Text.Replace("\r", string.Empty).Split('\n'))
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith('#') ||
                    trimmed.StartsWith("texture=", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.StartsWith("file=", StringComparison.OrdinalIgnoreCase))
                    continue;
                builder.Append(trimmed).Append('\n');
            }
            foreach (string reference in descriptor.Meta.References)
            {
                string path = Path.Combine(group.Directory, reference);
                builder.Append(File.Exists(path) ? Hash(path) : "missing").Append('\n');
            }
        }
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()))).ToLowerInvariant();
    }

    private static LoadedGroup LoadGroup(WorkspaceState workspace, string name)
    {
        string directory = Path.Combine(workspace.Root, name);
        var descriptors = new List<Descriptor>();
        if (Directory.Exists(directory))
            foreach (string full in Directory.GetFiles(directory, "*.meta", SearchOption.AllDirectories))
            {
                string relative = NormalizePath(Path.GetRelativePath(directory, full));
                string text = File.ReadAllText(full, Encoding.UTF8);
                descriptors.Add(new Descriptor(relative, text, ParseMeta(relative, text)));
            }
        return new LoadedGroup(name, directory, descriptors);
    }

    private static string RewriteReference(string text, string type, string destination)
    {
        string key = type == "sprite" ? "texture" : "file";
        var lines = text.Replace("\r", string.Empty).Split('\n');
        for (int i = 0; i < lines.Length; i++)
            if (lines[i].TrimStart().StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
                lines[i] = key + "=" + destination;
        return string.Join("\n", lines);
    }

    private static string Fingerprint(string directory)
    {
        var builder = new StringBuilder();
        foreach (string file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal))
        {
            var info = new FileInfo(file);
            builder.Append(NormalizePath(Path.GetRelativePath(directory, file))).Append('|')
                .Append(info.Length).Append('|').Append(info.LastWriteTimeUtc.Ticks).Append('\n');
        }
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()))).ToLowerInvariant();
    }

    // "@file" selects the exact addresses listed in that file (one per line); anything else is a glob.
    private static Func<string, bool> AddressMatcher(string pattern)
    {
        if (pattern.StartsWith('@'))
        {
            var exact = File.ReadAllLines(Path.GetFullPath(pattern[1..]))
                .Select(l => l.Trim().Replace('\\', '/')).Where(l => l.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            return exact.Contains;
        }
        Regex glob = AddressGlob(pattern);
        return glob.IsMatch;
    }

    private static Regex AddressGlob(string pattern)
    {
        var regex = new StringBuilder("^");
        string normalized = pattern.Replace('\\', '/');
        for (int i = 0; i < normalized.Length; i++)
        {
            char c = normalized[i];
            if (c == '*' && i + 1 < normalized.Length && normalized[i + 1] == '*') { regex.Append(".*"); i++; }
            else if (c == '*') regex.Append("[^/]*");
            else if (c == '?') regex.Append("[^/]");
            else regex.Append(Regex.Escape(c.ToString()));
        }
        return new Regex(regex.Append('$').ToString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static bool SafeGroupName(string name)
    {
        return name.Length > 0 && name.All(c => char.IsAsciiLetterOrDigit(c) || c is '_' or '-');
    }

    private static IEnumerable<JsonObject> CatalogRecords(JsonObject catalog)
    {
        if ((int?)catalog["version"] != 3 || catalog["bundles"] is not JsonArray bundles)
            throw new InvalidDataException("Expected a version 3 TAR/LZ4 catalog.");
        return bundles.OfType<JsonObject>();
    }

    private static IEnumerable<string> RecordAddresses(JsonObject record)
    {
        return (record["assets"] as JsonArray ?? []).OfType<JsonObject>()
            .Select(a => (string?)a["address"] ?? string.Empty).Where(a => a.Length > 0);
    }

    private static JsonObject ReadJsonObject(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("File not found.", path);
        return JsonNode.Parse(File.ReadAllText(path)) as JsonObject ?? throw new InvalidDataException("Expected a JSON object: " + path);
    }

    private static WorkspaceState LoadWorkspace(string workspacePath)
    {
        string root = Path.GetFullPath(workspacePath);
        JsonObject json = ReadJsonObject(Path.Combine(root, WorkspaceFile));
        var workspace = new WorkspaceState(root)
        {
            CatalogPath = (string?)json["catalog"] ?? Path.GetFullPath(DefaultCatalog),
            BundleDirectory = (string?)json["bundles"] ?? Path.GetFullPath(DefaultBundles),
        };
        foreach (JsonObject group in (json["groups"] as JsonArray ?? []).OfType<JsonObject>())
            workspace.Groups.Add(new WorkspaceGroup(
                (string?)group["name"] ?? throw new InvalidDataException("workspace.json group without name"),
                (JsonObject)(group["record"] ?? new JsonObject()).DeepClone(),
                (bool?)group["unpacked"] ?? false)
            { Fingerprint = (string?)group["fingerprint"] ?? string.Empty });
        if (json["winners"] is JsonObject winners)
            foreach (var pair in winners)
                workspace.Winners[pair.Key] = (string?)pair.Value ?? string.Empty;
        return workspace;
    }

    private static void SaveWorkspace(WorkspaceState workspace)
    {
        Directory.CreateDirectory(workspace.Root);
        var groups = new JsonArray();
        foreach (WorkspaceGroup group in workspace.Groups)
        {
            JsonObject record = (JsonObject)group.Record.DeepClone();
            groups.Add(new JsonObject
            {
                ["name"] = group.Name,
                ["unpacked"] = group.Unpacked,
                ["fingerprint"] = group.Fingerprint,
                ["record"] = record,
            });
        }
        var winners = new JsonObject();
        foreach (var pair in workspace.Winners.OrderBy(p => p.Key, StringComparer.Ordinal))
            winners[pair.Key] = pair.Value;
        var json = new JsonObject
        {
            ["version"] = 1,
            ["catalog"] = workspace.CatalogPath,
            ["bundles"] = workspace.BundleDirectory,
            ["groups"] = groups,
            ["winners"] = winners,
        };
        File.WriteAllText(Path.Combine(workspace.Root, WorkspaceFile), json.ToJsonString(JsonOptions) + Environment.NewLine);
    }

    private static void RemoveEmptyDirectories(string directory)
    {
        foreach (string child in Directory.GetDirectories(directory))
        {
            RemoveEmptyDirectories(child);
            if (!Directory.EnumerateFileSystemEntries(child).Any()) Directory.Delete(child);
        }
    }

    private sealed class WorkspaceState(string root)
    {
        public string Root { get; } = root;
        public string CatalogPath { get; set; } = string.Empty;
        public string BundleDirectory { get; set; } = string.Empty;
        public List<WorkspaceGroup> Groups { get; } = [];
        public Dictionary<string, string> Winners { get; } = new(StringComparer.OrdinalIgnoreCase);

        public WorkspaceGroup? Find(string name) =>
            Groups.FirstOrDefault(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private sealed class WorkspaceGroup(string name, JsonObject record, bool unpacked)
    {
        public string Name { get; } = name;
        public JsonObject Record { get; } = record;
        public bool Unpacked { get; } = unpacked;
        public string Fingerprint { get; set; } = string.Empty;
    }

    private sealed record Descriptor(string Path, string Text, Meta Meta)
    {
        public string Key => $"{Meta.Namespace}:{Meta.Address}:{Meta.Name}:{Meta.Type}".ToLowerInvariant();
    }

    private sealed class LoadedGroup(string name, string directory, List<Descriptor> descriptors)
    {
        public string Name { get; } = name;
        public string Directory { get; } = directory;
        public List<Descriptor> Descriptors { get; } = descriptors;

        public IEnumerable<string> AllFiles() => System.IO.Directory.Exists(Directory)
            ? System.IO.Directory.GetFiles(Directory, "*", SearchOption.AllDirectories)
                .Select(f => NormalizePath(System.IO.Path.GetRelativePath(Directory, f)))
            : [];

        public IEnumerable<string> OrphanPayloads()
        {
            var referenced = new HashSet<string>(Descriptors.SelectMany(d => d.Meta.References), PathComparer);
            return AllFiles().Where(f => !f.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) && !referenced.Contains(f));
        }

        public string Namespace() => Descriptors.GroupBy(d => d.Meta.Namespace, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault() ?? "core";

        public void Remove(Descriptor descriptor)
        {
            Descriptors.Remove(descriptor);
            File.Delete(Full(descriptor.Path));
        }

        public void WriteDescriptor(string relative, string text)
        {
            string full = Full(relative);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(full)!);
            File.WriteAllText(full, text, new UTF8Encoding(false));
            Descriptors.Add(new Descriptor(relative, text, ParseMeta(relative, text)));
        }

        // Copies a payload in, reusing an identical file or choosing a free name on collision.
        public string PlaceFile(string sourceFull, string preferred)
        {
            string candidate = preferred;
            for (int n = 2; File.Exists(Full(candidate)); n++)
            {
                if (SameBytes(sourceFull, Full(candidate))) return candidate;
                candidate = WithSuffix(preferred, n);
            }
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Full(candidate))!);
            File.Copy(sourceFull, Full(candidate));
            return candidate;
        }

        public string UniquePath(string preferred)
        {
            string candidate = preferred;
            for (int n = 2; File.Exists(Full(candidate)); n++)
                candidate = WithSuffix(preferred, n);
            return candidate;
        }

        public void DeleteUnreferenced(IEnumerable<string> candidates)
        {
            var referenced = new HashSet<string>(Descriptors.SelectMany(d => d.Meta.References), PathComparer);
            foreach (string path in candidates.Distinct(PathComparer).Where(p => !referenced.Contains(p)))
                File.Delete(Full(path));
            if (System.IO.Directory.Exists(Directory))
                RemoveEmptyDirectories(Directory);
        }

        private string Full(string relative) => System.IO.Path.Combine(Directory, relative.Replace('/', System.IO.Path.DirectorySeparatorChar));

        private static string WithSuffix(string path, int n)
        {
            string extension = path.EndsWith(".tar.lz4", StringComparison.OrdinalIgnoreCase) ? ".tar.lz4" : System.IO.Path.GetExtension(path);
            return path[..^extension.Length] + "~" + n + extension;
        }

        private static bool SameBytes(string a, string b)
        {
            var left = new FileInfo(a);
            var right = new FileInfo(b);
            return left.Length == right.Length && File.ReadAllBytes(a).AsSpan().SequenceEqual(File.ReadAllBytes(b));
        }
    }

    private sealed class CommandOptions
    {
        private readonly Dictionary<string, List<string>> _values = new(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> Flags = new(StringComparer.OrdinalIgnoreCase) { "dry-run" };
        public List<string> Positional { get; } = [];

        public CommandOptions(IEnumerable<string> args)
        {
            using IEnumerator<string> e = args.GetEnumerator();
            while (e.MoveNext())
            {
                string arg = e.Current;
                if (!arg.StartsWith("--")) { Positional.Add(arg); continue; }
                string key = arg[2..];
                string value = string.Empty;
                if (!Flags.Contains(key))
                {
                    if (!e.MoveNext()) throw new ArgumentException("Missing value for " + arg);
                    value = e.Current;
                }
                if (!_values.TryGetValue(key, out List<string>? list))
                    _values[key] = list = [];
                list.Add(value);
            }
        }

        public bool Has(string key) => _values.ContainsKey(key);
        public string? Value(string key) => _values.TryGetValue(key, out List<string>? list) ? list[^1] : null;
        public List<string> Values(string key) => _values.TryGetValue(key, out List<string>? list) ? list : [];

        public HashSet<string>? NameSet(string key)
        {
            string? value = Value(key);
            return value == null ? null : new HashSet<string>(
                value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), StringComparer.OrdinalIgnoreCase);
        }
    }
}
