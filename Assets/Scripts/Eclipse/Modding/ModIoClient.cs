using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Eclipse.Modding
{
    /// <summary>
    /// Reads Project Eclipse's mods from mod.io (anonymous, with the game's read-only API key)
    /// and installs their files through <see cref="ModZipInstaller"/>. Main thread only.
    /// </summary>
    public static class ModIoClient
    {
        public enum Sort { Popular, Newest, Updated, TopRated }
        private const int TimeoutSeconds = 30;

        private static string SortField(Sort sort) => sort switch
        {
            Sort.Newest => "-date_live",
            Sort.Updated => "-date_updated",
            Sort.TopRated => "-ratings",
            _ => "-popular",
        };

        public static string SortName(Sort sort) => sort switch
        {
            Sort.Newest => "Newest",
            Sort.Updated => "Recently updated",
            Sort.TopRated => "Top rated",
            _ => "Popular",
        };

        private static string ModsUrl(string query) =>
            ModIoConfig.ApiBase + "/games/" + ModIoConfig.GameId + "/mods?api_key=" + ModIoConfig.ApiKey + query;

        /// <summary>A page of mods that have a downloadable file.</summary>
        public static async Task<ModIoPage> List(string search, Sort sort, int offset, int limit)
        {
            string query = "&_limit=" + limit + "&_offset=" + offset + "&_sort=" + SortField(sort);
            if (!string.IsNullOrWhiteSpace(search)) query += "&_q=" + UnityWebRequest.EscapeURL(search.Trim());
            var page = ModIoPage.Parse(await GetText(ModsUrl(query)));
            page.Mods.RemoveAll(m => m.Modfile == null);
            return page;
        }

        /// <summary>The listed mods with the given mod.io ids (installed mods, for update checks).</summary>
        public static async Task<List<ModIoMod>> Get(IEnumerable<long> ids)
        {
            var list = ids.Distinct().ToList();
            var result = new List<ModIoMod>();
            for (int i = 0; i < list.Count; i += 50)
            {
                string query = "&_limit=100&id-in=" + string.Join(",", list.Skip(i).Take(50));
                result.AddRange(ModIoPage.Parse(await GetText(ModsUrl(query))).Mods);
            }
            return result;
        }

        public static async Task<Texture2D> Thumbnail(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            using (var request = UnityWebRequestTexture.GetTexture(url))
            {
                request.timeout = TimeoutSeconds;
                await Send(request);
                return request.result == UnityWebRequest.Result.Success ? DownloadHandlerTexture.GetContent(request) : null;
            }
        }

        private static async Task<string> GetText(string url)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = TimeoutSeconds;
                request.SetRequestHeader("Accept", "application/json");
                await Send(request);
                string body = request.downloadHandler?.text ?? string.Empty;
                if (request.result != UnityWebRequest.Result.Success)
                    throw new IOException(ModIoPage.ErrorMessage(body) ?? (request.error ?? "mod.io did not answer."));
                return body;
            }
        }

        private static Task Send(UnityWebRequest request)
        {
            var done = new TaskCompletionSource<bool>();
            request.SendWebRequest().completed += _ => done.TrySetResult(true);
            return done.Task;
        }

        // ---- Install ----

        public sealed class InstallResult
        {
            public ModIoInstall Install;
            public bool Updated;
            /// <summary>Downloaded but waiting for the next launch (the mod is loaded now).</summary>
            public bool Deferred;
            public string Notice = string.Empty;
        }

        /// <summary>
        /// Downloads <paramref name="mod"/>'s file, checks its size and MD5, and installs it. A mod
        /// that is loaded in this session is staged and installed on the next launch instead.
        /// </summary>
        public static async Task<InstallResult> Install(ModIoMod mod, string modsRoot, ISet<string> loadedModIds, Action<float> progress = null)
        {
            if (mod.Modfile == null || string.IsNullOrEmpty(mod.Modfile.BinaryUrl)) throw new InvalidDataException(mod.Name + " has no downloadable file.");
            var state = ModIoState.Load(modsRoot);
            var existing = state.Find(mod.Id);
            Directory.CreateDirectory(state.PendingFolder);
            string zip = Path.Combine(state.PendingFolder, mod.Id + "-" + mod.Modfile.Id + ".zip");
            bool keep = false;
            try
            {
                await Download(mod.Modfile, zip, progress);
                var facts = ModIoZipFacts.Read(zip, modsRoot);
                if (facts.Incompatible != null) throw new InvalidDataException(mod.Name + " cannot be used: " + facts.Incompatible);
                string modId = facts.Preview.Manifest.Id.Value;
                if (existing != null && existing.ModId != modId)
                    throw new InvalidDataException(mod.Name + "'s new file is a different mod (" + modId + ", was " + existing.ModId + ").");
                var owner = state.FindByModId(modId);
                if (owner != null && owner.ModIoId != mod.Id)
                    throw new InvalidDataException("Another mod.io mod already installed " + modId + ".");
                if (existing == null && facts.Preview.IsUpdate)
                    throw new InvalidDataException("A mod with the ID " + modId + " is already installed (not from mod.io). Remove it first to install this one.");
                var install = existing ?? new ModIoInstall { ModIoId = mod.Id, ModId = modId };
                install.Name = mod.Name;
                var result = new InstallResult { Install = install, Updated = existing != null };
                if (existing != null && loadedModIds != null && loadedModIds.Contains(modId))
                {
                    install.PendingZip = zip;
                    install.PendingModfileId = mod.Modfile.Id;
                    result.Deferred = true;
                    keep = true;
                }
                else
                {
                    ModZipInstaller.Install(zip, modsRoot, facts.Preview.IsUpdate, facts.Preview.Manifest.Id);
                    install.ModfileId = mod.Modfile.Id;
                    install.Version = facts.Preview.Manifest.Version.ToString();
                    install.ChangesMovesets = facts.ChangesMovesets;
                    install.UndeclaredCompatibility = facts.UndeclaredCompatibility;
                    install.PendingZip = string.Empty;
                    install.PendingModfileId = 0;
                }
                if (existing == null) state.Installs.Add(install);
                state.Save();
                var notes = new List<string>();
                if (facts.ChangesMovesets) notes.Add("It changes movesets: online versus is off while it is enabled.");
                if (facts.UndeclaredCompatibility) notes.Add("It does not say which Eclipse version it is made for, so it may not work.");
                result.Notice = string.Join(" ", notes);
                return result;
            }
            finally
            {
                if (!keep) { try { if (File.Exists(zip)) File.Delete(zip); } catch (Exception) { } }
            }
        }

        private static async Task Download(ModIoModfile file, string path, Action<float> progress)
        {
            string url = file.BinaryUrl;
            using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
            {
                request.downloadHandler = new DownloadHandlerFile(path) { removeFileOnAbort = true };
                request.timeout = 0;
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    progress?.Invoke(request.downloadProgress);
                    await Task.Yield();
                }
                if (request.result != UnityWebRequest.Result.Success)
                    throw new IOException("Download failed: " + (request.error ?? "no answer") + ".");
            }
            var info = new FileInfo(path);
            if (file.FileSize > 0 && info.Length != file.FileSize)
                throw new InvalidDataException("The download is incomplete (" + info.Length + " of " + file.FileSize + " bytes).");
            if (!string.IsNullOrEmpty(file.Md5))
            {
                string actual;
                using (var md5 = MD5.Create())
                using (var stream = File.OpenRead(path))
                    actual = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
                if (!string.Equals(actual, file.Md5, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("The download is damaged (checksum mismatch).");
            }
        }

        /// <summary>Removes a mod installed from mod.io (its folder and record).</summary>
        public static void Remove(long modIoId, string modsRoot)
        {
            var state = ModIoState.Load(modsRoot);
            var install = state.Find(modIoId);
            if (install == null) return;
            string folder = Path.Combine(Path.GetFullPath(modsRoot), install.ModId);
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            if (install.PendingZip.Length != 0 && File.Exists(install.PendingZip)) File.Delete(install.PendingZip);
            state.Installs.Remove(install);
            state.Save();
        }

        // ---- Automatic updates ----

        public sealed class UpdateReport
        {
            public List<string> Installed = new List<string>();
            public List<string> NextLaunch = new List<string>();
            public List<string> Failed = new List<string>();
        }

        private static Task<UpdateReport> _updates;

        /// <summary>Checks installed mod.io mods once per session and updates the ones with a newer file.</summary>
        public static Task<UpdateReport> UpdateInstalledOnce(string modsRoot, ISet<string> loadedModIds) =>
            _updates ??= UpdateInstalled(modsRoot, loadedModIds);

        private static async Task<UpdateReport> UpdateInstalled(string modsRoot, ISet<string> loadedModIds)
        {
            var report = new UpdateReport();
            var state = ModIoState.Load(modsRoot);
            if (state.Installs.Count == 0) return report;
            List<ModIoMod> listed;
            try { listed = await Get(state.Installs.Select(i => i.ModIoId)); }
            catch (Exception error) { Debug.LogWarning("[mod.io] Update check failed: " + error.Message); return report; }
            foreach (var mod in listed)
            {
                var install = state.Find(mod.Id);
                if (install == null || mod.Modfile == null || mod.Modfile.Id == install.ModfileId || mod.Modfile.Id == install.PendingModfileId) continue;
                try
                {
                    var result = await Install(mod, modsRoot, loadedModIds);
                    (result.Deferred ? report.NextLaunch : report.Installed).Add(mod.Name);
                }
                catch (Exception error)
                {
                    report.Failed.Add(mod.Name);
                    Debug.LogWarning("[mod.io] Could not update " + mod.Name + ": " + error.Message);
                }
            }
            return report;
        }
    }
}
