using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Eclipse.Modding
{
    /// <summary>The game's mod.io project (https://mod.io/g/project-eclipse).</summary>
    public static class ModIoConfig
    {
        public const string ApiBase = "https://g-14375.modapi.io/v1";
        public const int GameId = 14375;
        public const string GameNameId = "project-eclipse";
        // A game API key is read-only: it lists public mods and their files, nothing else.
        public const string ApiKey = "e69461afb5e63f4fcd9208c8f1d05fe3";
        public const string ProfileUrl = "https://mod.io/g/project-eclipse";
    }

    public sealed class ModIoModfile
    {
        public long Id;
        public long FileSize;
        public string FileName = string.Empty;
        public string Version = string.Empty;
        public string Md5 = string.Empty;
        public string BinaryUrl = string.Empty;
    }

    /// <summary>One mod as the mod.io API lists it.</summary>
    public sealed class ModIoMod
    {
        public long Id;
        public string Name = string.Empty;
        public string NameId = string.Empty;
        public string Summary = string.Empty;
        public string Description = string.Empty;
        public string ProfileUrl = string.Empty;
        public string LogoUrl = string.Empty;
        public string Author = string.Empty;
        public long DateUpdated;
        public long Downloads;
        public string Rating = string.Empty;
        public List<string> Tags = new List<string>();
        /// <summary>The live file, or null when the mod has none.</summary>
        public ModIoModfile Modfile;

        public static ModIoMod Parse(ModJsonNode node)
        {
            var mod = new ModIoMod
            {
                Id = Long(node["id"]), Name = Text(node["name"]), NameId = Text(node["name_id"]), Summary = Text(node["summary"]),
                Description = Text(node["description_plaintext"]), ProfileUrl = Text(node["profile_url"]),
                LogoUrl = Text(node["logo"]?["thumb_320x180"]), Author = Text(node["submitted_by"]?["username"]),
                DateUpdated = Long(node["date_updated"]), Downloads = Long(node["stats"]?["downloads_total"]),
                Rating = Text(node["stats"]?["ratings_display_text"]),
            };
            var tags = node["tags"];
            if (tags != null && tags.Kind == ModJsonKind.Array)
                foreach (var tag in tags.Items) { string name = Text(tag["name"]); if (name.Length != 0) mod.Tags.Add(name); }
            var file = node["modfile"];
            if (file != null && file.Kind == ModJsonKind.Object && Long(file["id"]) > 0)
                mod.Modfile = new ModIoModfile
                {
                    Id = Long(file["id"]), FileSize = Long(file["filesize"]), FileName = Text(file["filename"]), Version = Text(file["version"]),
                    Md5 = Text(file["filehash"]?["md5"]), BinaryUrl = Text(file["download"]?["binary_url"]),
                };
            return mod;
        }

        internal static string Text(ModJsonNode node) => node != null && node.Kind == ModJsonKind.String ? node.String : string.Empty;
        internal static long Long(ModJsonNode node) => node != null && node.Kind == ModJsonKind.Number ? (long)node.Number : 0;
    }

    /// <summary>A page of a mod.io list request.</summary>
    public sealed class ModIoPage
    {
        public List<ModIoMod> Mods = new List<ModIoMod>();
        public int Total;
        public int Offset;

        public static ModIoPage Parse(string json)
        {
            var root = ModJsonNode.Parse(json, 32);
            var page = new ModIoPage { Total = (int)ModIoMod.Long(root["result_total"]), Offset = (int)ModIoMod.Long(root["result_offset"]) };
            var data = root["data"];
            if (data != null && data.Kind == ModJsonKind.Array)
                foreach (var item in data.Items) page.Mods.Add(ModIoMod.Parse(item));
            return page;
        }

        /// <summary>The API's error message, or null when <paramref name="json"/> is not an error body.</summary>
        public static string ErrorMessage(string json)
        {
            try
            {
                var error = ModJsonNode.Parse(json, 8)["error"];
                return error == null ? null : ModIoMod.Text(error["message"]);
            }
            catch (FormatException) { return null; }
        }
    }

    /// <summary>A mod installed from mod.io: which mod.io mod and file it came from.</summary>
    public sealed class ModIoInstall
    {
        public long ModIoId;
        public long ModfileId;
        public string ModId = string.Empty;
        public string Name = string.Empty;
        public string Version = string.Empty;
        /// <summary>The mod ships moveset files, which turn off online versus while it is enabled.</summary>
        public bool ChangesMovesets;
        /// <summary>The mod does not say which Eclipse version it is made for.</summary>
        public bool UndeclaredCompatibility;
        /// <summary>A downloaded update installed on the next launch, or empty.</summary>
        public string PendingZip = string.Empty;
        public long PendingModfileId;
    }

    /// <summary>
    /// Mods installed from mod.io, kept beside the Mods folder (never inside it, so discovery does
    /// not see it): &lt;parent of Mods&gt;/.eclipse-modio/installs.json, plus downloaded updates
    /// waiting for the next launch in pending/.
    /// </summary>
    public sealed class ModIoState
    {
        public List<ModIoInstall> Installs { get; } = new List<ModIoInstall>();
        public string ModsRoot { get; private set; }

        public static string Folder(string modsRoot)
        {
            string root = Path.GetFullPath(modsRoot);
            string parent = Directory.GetParent(root)?.FullName ?? root;
            return Path.Combine(parent, ".eclipse-modio");
        }

        public string PendingFolder => Path.Combine(Folder(ModsRoot), "pending");

        public ModIoInstall Find(long modIoId) => Installs.FirstOrDefault(i => i.ModIoId == modIoId);
        public ModIoInstall FindByModId(string modId) => Installs.FirstOrDefault(i => i.ModId == modId);

        public static ModIoState Load(string modsRoot)
        {
            var state = new ModIoState { ModsRoot = Path.GetFullPath(modsRoot) };
            string path = Path.Combine(Folder(modsRoot), "installs.json");
            if (!File.Exists(path)) return state;
            try
            {
                var root = ModJsonNode.Parse(File.ReadAllText(path, Encoding.UTF8), 8);
                var list = root["installs"];
                if (list != null && list.Kind == ModJsonKind.Array)
                    foreach (var item in list.Items)
                        state.Installs.Add(new ModIoInstall
                        {
                            ModIoId = ModIoMod.Long(item["modio_id"]), ModfileId = ModIoMod.Long(item["modfile_id"]),
                            ModId = ModIoMod.Text(item["mod_id"]), Name = ModIoMod.Text(item["name"]), Version = ModIoMod.Text(item["version"]),
                            ChangesMovesets = item["changes_movesets"]?.Kind == ModJsonKind.Boolean && item["changes_movesets"].Boolean,
                            UndeclaredCompatibility = item["undeclared_compatibility"]?.Kind == ModJsonKind.Boolean && item["undeclared_compatibility"].Boolean,
                            PendingZip = ModIoMod.Text(item["pending_zip"]), PendingModfileId = ModIoMod.Long(item["pending_modfile_id"]),
                        });
            }
            catch (Exception) { /* A damaged record only forgets update tracking; installed mods stay. */ }
            // A mod removed by hand is no longer tracked.
            state.Installs.RemoveAll(i => i.ModId.Length == 0 || !Directory.Exists(Path.Combine(state.ModsRoot, i.ModId)) && i.PendingZip.Length == 0);
            return state;
        }

        public void Save()
        {
            var list = ModJsonNode.NewArray();
            foreach (var install in Installs)
                list.Add(ModJsonNode.NewObject()
                    .Set("modio_id", ModJsonNode.Of((double)install.ModIoId)).Set("modfile_id", ModJsonNode.Of((double)install.ModfileId))
                    .Set("mod_id", ModJsonNode.Of(install.ModId)).Set("name", ModJsonNode.Of(install.Name)).Set("version", ModJsonNode.Of(install.Version))
                    .Set("changes_movesets", ModJsonNode.Of(install.ChangesMovesets)).Set("undeclared_compatibility", ModJsonNode.Of(install.UndeclaredCompatibility))
                    .Set("pending_zip", ModJsonNode.Of(install.PendingZip)).Set("pending_modfile_id", ModJsonNode.Of((double)install.PendingModfileId)));
            string folder = Folder(ModsRoot);
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, "installs.json"), temp = path + ".write";
            File.WriteAllText(temp, ModJsonNode.NewObject().Set("schema", ModJsonNode.Of(1)).Set("installs", list).ToJson(), new UTF8Encoding(false));
            if (File.Exists(path)) File.Replace(temp, path, null); else File.Move(temp, path);
        }
    }

    /// <summary>What a downloaded mod ZIP is, read before it is installed.</summary>
    public sealed class ModIoZipFacts
    {
        public ModZipPreview Preview;
        public bool ChangesMovesets;
        /// <summary>The manifest names no core version range.</summary>
        public bool UndeclaredCompatibility;
        /// <summary>Why this Eclipse cannot load it, or null.</summary>
        public string Incompatible;

        public static ModIoZipFacts Read(string zipPath, string modsRoot)
        {
            var facts = new ModIoZipFacts { Preview = ModZipInstaller.Inspect(zipPath, modsRoot) };
            using (var archive = ZipFile.OpenRead(zipPath))
                facts.ChangesMovesets = archive.Entries.Any(e => e.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
                    (e.FullName.StartsWith("movesets/", StringComparison.Ordinal) || e.FullName.Contains("/movesets/")));
            var core = facts.Preview.Manifest.Dependencies.FirstOrDefault(d => d.Id.Value == "core");
            if (core == null) facts.UndeclaredCompatibility = true;
            else if (!core.Version.Contains(ModPlatformVersions.Core))
                facts.Incompatible = "It needs Eclipse core " + core.Version + "; this game has core " + ModPlatformVersions.Core + ".";
            return facts;
        }
    }

    /// <summary>Installs updates downloaded during the last session, before mods are loaded.</summary>
    public static class ModIoPendingUpdates
    {
        /// <returns>Names of the mods updated.</returns>
        public static List<string> Apply(string modsRoot)
        {
            var updated = new List<string>();
            var state = ModIoState.Load(modsRoot);
            bool changed = false;
            foreach (var install in state.Installs)
            {
                if (install.PendingZip.Length == 0) continue;
                string zip = install.PendingZip;
                try
                {
                    if (File.Exists(zip))
                    {
                        var facts = ModIoZipFacts.Read(zip, modsRoot);
                        if (facts.Incompatible == null && facts.Preview.Manifest.Id.Value == install.ModId)
                        {
                            ModZipInstaller.Install(zip, modsRoot, true, facts.Preview.Manifest.Id);
                            install.ModfileId = install.PendingModfileId;
                            install.Version = facts.Preview.Manifest.Version.ToString();
                            install.ChangesMovesets = facts.ChangesMovesets;
                            install.UndeclaredCompatibility = facts.UndeclaredCompatibility;
                            updated.Add(install.Name);
                        }
                    }
                }
                catch (Exception) { /* The installed version stays; the next check downloads again. */ }
                finally
                {
                    try { if (File.Exists(zip)) File.Delete(zip); } catch (Exception) { }
                    install.PendingZip = string.Empty;
                    install.PendingModfileId = 0;
                    changed = true;
                }
            }
            if (changed) state.Save();
            return updated;
        }
    }

    public static class ModIoText
    {
        public static string Size(long bytes) =>
            bytes >= 1024 * 1024 ? (bytes / 1048576.0).ToString("0.0", CultureInfo.InvariantCulture) + " MB"
            : (Math.Max(1, bytes / 1024)).ToString(CultureInfo.InvariantCulture) + " KB";

        public static string Date(long unix) =>
            unix <= 0 ? "" : DateTimeOffset.FromUnixTimeSeconds(unix).ToLocalTime().ToString("d MMM yyyy", CultureInfo.InvariantCulture);
    }
}
