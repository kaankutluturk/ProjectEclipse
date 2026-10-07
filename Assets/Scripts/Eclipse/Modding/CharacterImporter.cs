using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Eclipse.Modding
{
    /// <summary>
    /// Desktop front end for Tools/Animation/ImportCharacter.py. Runs the user's
    /// Blender in the background to turn a rigged humanoid (.glb/.gltf/.fbx/.blend)
    /// into a loose mod in the Mods folder. Blender is not bundled.
    /// </summary>
    public static class CharacterImporter
    {
        public static readonly string[] SourceExtensions = { "glb", "gltf", "fbx", "blend" };
        // The importer's generated character.lua registers this warrior.
        public const string WarriorLocalId = "warriors/authored_character";
        private const string BlenderPreference = "Eclipse.BlenderPath";

        public static bool IsSupported
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>User-chosen Blender executable; empty uses automatic discovery.</summary>
        public static string BlenderOverride
        {
            get { try { return PlayerPrefs.GetString(BlenderPreference, string.Empty); } catch { return string.Empty; } }
            set { PlayerPrefs.SetString(BlenderPreference, value ?? string.Empty); PlayerPrefs.Save(); }
        }

        public static string FindBlender()
        {
            string chosen = BlenderOverride;
            if (!string.IsNullOrEmpty(chosen) && File.Exists(chosen)) return chosen;
            var candidates = new List<string>();
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                foreach (string root in new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                                                 Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) })
                {
                    if (string.IsNullOrEmpty(root)) continue;
                    string foundation = Path.Combine(root, "Blender Foundation");
                    if (Directory.Exists(foundation))
                    {
                        var versions = new List<string>(Directory.GetDirectories(foundation));
                        versions.Sort((a, b) => VersionOf(b).CompareTo(VersionOf(a)));
                        foreach (string directory in versions) candidates.Add(Path.Combine(directory, "blender.exe"));
                    }
                    candidates.Add(Path.Combine(root, "Steam", "steamapps", "common", "Blender", "blender.exe"));
                }
            }
            else if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
                candidates.Add("/Applications/Blender.app/Contents/MacOS/Blender");
            else
                candidates.AddRange(new[] { "/usr/bin/blender", "/usr/local/bin/blender", "/snap/bin/blender" });
            string executable = Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.WindowsEditor ? "blender.exe" : "blender";
            foreach (string directory in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator))
                if (directory.Length != 0) candidates.Add(Path.Combine(directory.Trim('"'), executable));
            foreach (string candidate in candidates)
            {
                try { if (File.Exists(candidate)) return candidate; }
                catch (Exception) { }
            }
            return null;
        }

        private static Version VersionOf(string directory)
        {
            var match = Regex.Match(Path.GetFileName(directory), @"(\d+)\.(\d+)");
            return match.Success ? new Version(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value)) : new Version(0, 0);
        }

        /// <summary>Importer scripts: the repository in the editor, a copy beside player data in builds.</summary>
        public static string ScriptsDirectory()
        {
#if UNITY_EDITOR
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Tools", "Animation"));
#else
            return Path.Combine(Application.dataPath, "CharacterImport");
#endif
        }

        /// <summary>Packages an installed mod folder as an installable ZIP (one top-level
        /// folder, as Mods > Install ZIP expects) in "Eclipse characters" on the desktop.</summary>
        public static string ExportZip(string modRoot)
        {
            if (string.IsNullOrEmpty(modRoot) || !File.Exists(Path.Combine(modRoot, "mod.toml")))
                throw new DirectoryNotFoundException("This character's mod folder is missing.");
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string folder = Path.Combine(string.IsNullOrEmpty(desktop) ? Application.persistentDataPath : desktop, "Eclipse characters");
            Directory.CreateDirectory(folder);
            string name = Path.GetFileName(modRoot.TrimEnd('/', '\\'));
            string zip = Path.Combine(folder, name + ".zip");
            string temporary = zip + ".partial";
            if (File.Exists(temporary)) File.Delete(temporary);
            using (var stream = File.Create(temporary))
            using (var archive = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Create))
            {
                foreach (string file in Directory.GetFiles(modRoot, "*", SearchOption.AllDirectories))
                {
                    string relative = file.Substring(modRoot.TrimEnd('/', '\\').Length + 1).Replace('\\', '/');
                    var entry = archive.CreateEntry(name + "/" + relative, System.IO.Compression.CompressionLevel.Optimal);
                    using (var input = File.OpenRead(file))
                    using (var output = entry.Open())
                        input.CopyTo(output);
                }
            }
            if (File.Exists(zip)) File.Delete(zip);
            File.Move(temporary, zip);
            return zip;
        }

        /// <summary>Shows a file's folder in the desktop file browser.</summary>
        public static void Reveal(string path)
        {
            try { Application.OpenURL(new Uri(Path.GetDirectoryName(path) + Path.DirectorySeparatorChar).AbsoluteUri); }
            catch (Exception error) { Debug.LogWarning("[CharacterImport] " + error.Message); }
        }

        public static string DefaultTitle(string source)
        {
            string name = Regex.Replace(Path.GetFileNameWithoutExtension(source ?? string.Empty), @"[_\-.]+", " ").Trim();
            if (name.Length == 0) name = "Imported Fighter";
            return name.Length > 80 ? name.Substring(0, 80) : name;
        }

        /// <summary>A fresh "local.*" mod ID that is not installed yet.</summary>
        public static string SuggestModId(string title, string modsRoot)
        {
            string slug = Regex.Replace((title ?? string.Empty).ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
            if (slug.Length == 0) slug = "fighter";
            if (slug.Length > 60) slug = slug.Substring(0, 60).Trim('-');
            string id = "local." + slug;
            for (int i = 2; Directory.Exists(Path.Combine(modsRoot, id)); i++) id = "local." + slug + "-" + i;
            return id;
        }

        /// <summary>Writes the game's canonical combat skeleton for the importer. Main thread only.</summary>
        internal static void WriteCanonicalRig(string path)
        {
            var document = ModelLoader.DocumentCache.GetDocument(SF2Paths.GetModelsPath(), "mdl_skeleton.xml");
            if (document == null || document["Scene"] == null)
                throw new InvalidDataException("The game's combat skeleton (mdl_skeleton) could not be read.");
            using (var writer = new StreamWriter(path, false, new UTF8Encoding(false)))
                document.Save(writer);
        }

        public static readonly string[] Voices = { "Male", "Female" };

        /// <param name="voice">"Male" or "Female": the character's gendered combat sounds.</param>
        // Native clips drawn in preview.png after the source profile: idle stance,
        // a kick and a handspring (open hands, extended limbs).
        private static readonly (string File, int Frame)[] PreviewClips =
            { ("stance_idle.bytes", 3), ("front_kick.bytes", 14), ("back_handflip.bytes", 8) };

        /// <summary>Semantic roles the importer maps, in display order (CharacterPipeline/RigMapping roles).</summary>
        public static readonly string[] Roles =
        {
            "pelvis", "spine", "chest", "neck", "head",
            "left_upper_arm", "left_forearm", "left_hand", "right_upper_arm", "right_forearm", "right_hand",
            "left_thigh", "left_shin", "left_foot", "right_thigh", "right_shin", "right_foot"
        };
        public static bool IsOptionalRole(string role) => role == "spine" || role == "neck";
        public static readonly string[] ClipKeys = { "Punch", "Kick", "Ranged", "Magic", "Up", "Down", "Forward", "Back" };

        public static CharacterImportJob Start(string source, string title, string voice = "Male",
            IDictionary<string, string> mapping = null, IList<(string Name, string Action, string Key)> clips = null)
        {
            if (Array.IndexOf(Voices, voice) < 0) throw new ArgumentException("Voice must be Male or Female.", nameof(voice));
            if (!IsSupported) throw new PlatformNotSupportedException("Character import needs a desktop build and Blender.");
            if (string.IsNullOrEmpty(source) || !File.Exists(source)) throw new FileNotFoundException("Choose an existing model file.", source);
            string extension = Path.GetExtension(source).TrimStart('.').ToLowerInvariant();
            if (Array.IndexOf(SourceExtensions, extension) < 0)
                throw new InvalidDataException("Choose a .glb, .gltf, .fbx or .blend file.");
            title = (title ?? string.Empty).Trim();
            if (title.Length == 0 || title.Length > 80) throw new InvalidDataException("Give the character a name of 1 to 80 characters.");
            string blender = FindBlender() ?? throw new FileNotFoundException("Blender was not found. Install Blender 3.6 or newer, or locate blender manually.");
            string scripts = ScriptsDirectory();
            string importer = Path.Combine(scripts, "ImportCharacter.py");
            if (!File.Exists(importer)) throw new FileNotFoundException("The character importer scripts are missing: " + importer);
            string modsRoot = ModHost.GetDefaultModsRoot();
            Directory.CreateDirectory(modsRoot);
            string modId = SuggestModId(title, modsRoot);
            string work = Path.Combine(Application.temporaryCachePath, "CharacterImport-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(work);
            string rig = Path.Combine(work, "mdl_skeleton.xml");
            WriteCanonicalRig(rig);
            var job = new CharacterImportJob(title, modId, modId + ":" + WarriorLocalId, Path.Combine(modsRoot, modId), work);
            var arguments = new List<string> { "--background", "--factory-startup", "--python-exit-code", "1", "--python", importer, "--",
                "--source", source, "--rig", rig, "--mod-id", modId, "--output", job.OutputDirectory, "--title", title, "--voice", voice };
            foreach (var (file, frame) in PreviewClips)
            {
                // The game's own clips feed the preview; a missing one only drops that cell.
                byte[] data = ResourceManager.GetBinary(SF2Paths.GetBinaryAnimationsPath() + "/" + file);
                if (data == null || data.Length == 0) continue;
                string path = Path.Combine(work, file);
                File.WriteAllBytes(path, data);
                arguments.Add("--preview-animation"); arguments.Add(path + ":" + frame.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            if (mapping != null && mapping.Count != 0)
            {
                string path = Path.Combine(work, "mapping.json");
                File.WriteAllText(path, Json(mapping), new UTF8Encoding(false));
                arguments.Add("--mapping"); arguments.Add(path);
            }
            foreach (var clip in clips ?? Array.Empty<(string, string, string)>())
            {
                if (Array.IndexOf(ClipKeys, clip.Key) < 0) throw new ArgumentException("Unknown clip key: " + clip.Key);
                arguments.Add("--clip"); arguments.Add(clip.Name); arguments.Add(clip.Action); arguments.Add(clip.Key);
            }
            job.Begin(blender, Quote(arguments));
            return job;
        }

        /// <summary>Lists the source file's animations (actions) through Blender.</summary>
        public static CharacterImportJob ListActions(string source)
        {
            if (!IsSupported) throw new PlatformNotSupportedException("Character import needs a desktop build and Blender.");
            string blender = FindBlender() ?? throw new FileNotFoundException("Blender was not found.");
            string importer = Path.Combine(ScriptsDirectory(), "ImportCharacter.py");
            if (!File.Exists(importer)) throw new FileNotFoundException("The character importer scripts are missing: " + importer);
            string work = Path.Combine(Application.temporaryCachePath, "CharacterActions-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(work);
            var job = new CharacterImportJob(Path.GetFileNameWithoutExtension(source), null, null, null, work);
            job.Begin(blender, Quote(new[] { "--background", "--factory-startup", "--python-exit-code", "1", "--python", importer, "--",
                "--source", source, "--list-actions" }));
            return job;
        }

        private static string Json(IDictionary<string, string> values)
        {
            var builder = new StringBuilder("{");
            foreach (var pair in values)
            {
                if (builder.Length > 1) builder.Append(',');
                builder.Append(JsonString(pair.Key)).Append(':').Append(JsonString(pair.Value));
            }
            return builder.Append('}').ToString();
        }

        private static string JsonString(string value)
        {
            var builder = new StringBuilder("\"");
            foreach (char c in value)
            {
                if (c == '"' || c == '\\') builder.Append('\\').Append(c);
                else if (c < ' ') builder.Append("\\u").Append(((int)c).ToString("x4"));
                else builder.Append(c);
            }
            return builder.Append('"').ToString();
        }

        // Windows command-line quoting (CommandLineToArgvW rules); also valid for Mono on Unix.
        private static string Quote(IEnumerable<string> values)
        {
            var builder = new StringBuilder();
            foreach (string value in values)
            {
                if (builder.Length != 0) builder.Append(' ');
                if (value.Length != 0 && value.IndexOfAny(new[] { ' ', '\t', '"' }) < 0) { builder.Append(value); continue; }
                builder.Append('"');
                int slashes = 0;
                foreach (char c in value)
                {
                    if (c == '\\') { slashes++; continue; }
                    builder.Append('\\', c == '"' ? slashes * 2 + 1 : slashes); slashes = 0;
                    builder.Append(c);
                }
                builder.Append('\\', slashes * 2).Append('"');
            }
            return builder.ToString();
        }
    }

    public sealed class CharacterImportJob
    {
        public enum JobState { Running, Succeeded, Failed, Canceled }

        // Python exception lines ("ValueError: ...", "__main__.BudgetExceeded: ..."),
        // not Blender's own "Error: ..." banners around them.
        private static readonly Regex Failure = new Regex(@"^(?!Error: )[\w.]*?(\w*(?:Error|Exceeded|Exception)): (.+)$");
        private const int TimeoutMinutes = 20;
        private readonly object _gate = new object();
        private readonly StringBuilder _log = new StringBuilder();
        private Process _process;
        private string _status = "Starting Blender...";
        private string _error;
        private volatile JobState _state = JobState.Running;
        private int _version;
        private readonly DateTime _started = DateTime.UtcNow.AddSeconds(-1);

        public string Title { get; }
        /// <summary>Skeleton bones (depth, name) in hierarchy order when matching failed.</summary>
        public IReadOnlyList<(int Depth, string Name)> Bones { get { lock (_gate) return _bones.ToArray(); } }
        /// <summary>Roles the importer could guess by name when matching failed.</summary>
        public IReadOnlyDictionary<string, string> Suggested { get { lock (_gate) return new Dictionary<string, string>(_suggested); } }
        /// <summary>Animation (action) names reported by a ListActions job.</summary>
        public IReadOnlyList<string> Actions { get { lock (_gate) return _actions.ToArray(); } }
        public string PreviewPath => OutputDirectory == null ? null : Path.Combine(OutputDirectory, "preview.png");
        private readonly List<(int, string)> _bones = new List<(int, string)>();
        private readonly Dictionary<string, string> _suggested = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly List<string> _actions = new List<string>();
        private static readonly Regex ActionName = new Regex("\"name\": \"((?:[^\"\\\\]|\\\\.)*)\"");
        public string ModId { get; }
        public string WarriorId { get; }
        public string OutputDirectory { get; }
        private readonly string _workDirectory;

        internal CharacterImportJob(string title, string modId, string warriorId, string output, string work)
        {
            Title = title; ModId = modId; WarriorId = warriorId; OutputDirectory = output; _workDirectory = work;
        }

        public JobState State => _state;
        /// <summary>Changes whenever State or Status changes; poll from the UI thread.</summary>
        public int Version => Volatile.Read(ref _version);
        public string Status { get { lock (_gate) return _error ?? _status; } }
        public string Log { get { lock (_gate) return _log.ToString(); } }

        internal void Begin(string blender, string arguments)
        {
            var info = new ProcessStartInfo(blender, arguments)
            {
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true,
                WorkingDirectory = _workDirectory
            };
            Debug.Log("[CharacterImport] " + blender + " " + arguments);
            var thread = new System.Threading.Thread(() => Run(info)) { IsBackground = true, Name = "Eclipse character import" };
            thread.Start();
        }

        public void Cancel()
        {
            if (_state != JobState.Running) return;
            _state = JobState.Canceled;
            lock (_gate) _status = "Import canceled.";
            try { if (_process != null && !_process.HasExited) _process.Kill(); }
            catch (Exception) { }
            Interlocked.Increment(ref _version);
        }

        private void Run(ProcessStartInfo info)
        {
            try
            {
                using (var process = new Process { StartInfo = info })
                {
                    process.OutputDataReceived += (_, e) => Line(e.Data);
                    process.ErrorDataReceived += (_, e) => Line(e.Data);
                    lock (_gate) _process = process;
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    if (!process.WaitForExit(TimeoutMinutes * 60 * 1000))
                    {
                        try { process.Kill(); } catch (Exception) { }
                        Finish(JobState.Failed, "Blender did not finish within " + TimeoutMinutes + " minutes.");
                        return;
                    }
                    process.WaitForExit(); // flush redirected output
                    if (_state == JobState.Canceled) return;
                    bool published = ModId == null || Directory.Exists(OutputDirectory) && File.Exists(Path.Combine(OutputDirectory, "mod.toml"));
                    if (process.ExitCode == 0 && published) Finish(JobState.Succeeded, ModId == null ? "Read " + Actions.Count + " animations." : "Imported " + Title + ".");
                    else Finish(JobState.Failed, _error ?? "Blender exited with code " + process.ExitCode + ". See the log for details.");
                }
            }
            catch (Exception error)
            {
                Finish(JobState.Failed, "Could not run Blender: " + error.Message);
            }
            finally
            {
                try { Directory.Delete(_workDirectory, true); } catch (Exception) { }
                if (_state != JobState.Succeeded && OutputDirectory != null) RemoveStaging();
            }
        }

        // A killed Blender cannot run the importer's own cleanup of its staging folder.
        private void RemoveStaging()
        {
            try
            {
                foreach (string directory in Directory.GetDirectories(Path.GetDirectoryName(OutputDirectory), ".imported-character-*"))
                    if (Directory.GetCreationTimeUtc(directory) >= _started) Directory.Delete(directory, true);
            }
            catch (Exception error) { Debug.LogWarning("[CharacterImport] " + error.Message); }
        }

        private void Line(string line)
        {
            if (line == null) return;
            lock (_gate)
            {
                if (_log.Length < 256 * 1024) _log.AppendLine(line);
                const string step = "SF2 IMPORT STEP: ", bone = "SF2 IMPORT BONE: ", suggest = "SF2 IMPORT SUGGEST: ";
                if (line.StartsWith(step, StringComparison.Ordinal)) _status = line.Substring(step.Length);
                else if (line.StartsWith(bone, StringComparison.Ordinal))
                {
                    var parts = line.Substring(bone.Length).Split(new[] { '\t' }, 2);
                    if (parts.Length == 2 && int.TryParse(parts[0], out int depth)) _bones.Add((depth, parts[1]));
                }
                else if (line.StartsWith(suggest, StringComparison.Ordinal))
                {
                    var parts = line.Substring(suggest.Length).Split(new[] { '\t' }, 2);
                    if (parts.Length == 2) _suggested[parts[0]] = parts[1];
                }
                else if (ModId == null)
                {
                    var action = ActionName.Match(line);
                    if (action.Success) _actions.Add(Regex.Unescape(action.Groups[1].Value));
                }
                var failure = Failure.Match(line);
                if (failure.Success) _error = failure.Groups[2].Value.Trim();
            }
            Interlocked.Increment(ref _version);
        }

        private void Finish(JobState state, string message)
        {
            if (_state == JobState.Canceled) return;
            lock (_gate) { _status = message; if (state == JobState.Succeeded) _error = null; else _error = message; }
            _state = state;
            if (state == JobState.Failed) Debug.LogWarning("[CharacterImport] " + message + "\n" + Log);
            Interlocked.Increment(ref _version);
        }
    }
}
