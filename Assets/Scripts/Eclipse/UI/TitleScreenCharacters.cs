using System;
using System.Collections.Generic;
using System.IO;
using Eclipse.Modding;
using UnityEngine;
using UnityEngine.UI;

namespace Eclipse.UI
{
    // Mods > Characters: choose which mod character's look Shadow uses, and
    // import a rigged humanoid through the user's Blender installation.
    public sealed partial class TitleScreen
    {
        private sealed class LookRow
        {
            internal string WarriorId, Name, Detail, RootPath;
            internal bool NeedsRestart, Imported;
            internal ModDescriptor DisabledMod;
        }

        private const int LooksPerPage = 4;
        private const int ActionsPerPage = 6;
        private const int BonesPerPage = 9;
        private int lookPage;
        private string lookMessage;
        private bool lookRestartPending;
        private string pendingImportSource;
        private string pendingImportVoice = "Male";
        private string pendingImportTitle;
        // Manual skeleton roles (role -> bone) and selected source animations for the next import.
        private Dictionary<string, string> importMapping;
        private readonly List<(string Name, string Action, string Key)> importClips = new List<(string, string, string)>();
        private List<string> importActions;
        private List<(int Depth, string Name)> mapperBones;
        private int actionPage, bonePage;
        private CharacterImportJob characterJob, actionsJob;
        private int characterJobVersion = -1, actionsJobVersion = -1;
        private CharacterImportJob lastImport;
        private Text characterStatusLabel;
        private Texture2D importPreview;

        private void OpenCharacters()
        {
            lookPage = 0;
            lookMessage = null;
            DrawCharacters();
        }

        private List<LookRow> CharacterLooks()
        {
            var rows = new List<LookRow> { new LookRow { WarriorId = string.Empty, Name = "Shadow", Detail = "Original look" } };
            var loaded = new HashSet<string>(StringComparer.Ordinal);
            var roots = new Dictionary<string, string>(StringComparer.Ordinal);
            try
            {
                foreach (var mod in ModDiscovery.DiscoverLoose(ModHost.GetDefaultModsRoot()).Mods) roots[mod.Id.Value] = mod.RootPath;
            }
            catch (Exception error) { Debug.LogWarning("[Appearance] " + error.Message); }
            foreach (var choice in PlayerAppearance.LoadedChoices())
            {
                loaded.Add(choice.WarriorId);
                roots.TryGetValue(choice.ModId, out string root);
                rows.Add(new LookRow
                {
                    WarriorId = choice.WarriorId, Name = choice.Name, Detail = choice.ModId, RootPath = root,
                    Imported = root != null && File.Exists(Path.Combine(root, "import.json"))
                });
            }
            // Imported characters installed since this session's mods were loaded.
            try
            {
                string root = ModHost.GetDefaultModsRoot();
                var discovery = ModDiscovery.DiscoverLoose(root);
                ModSelection selection;
                try { selection = ModSelection.Load(ModHost.GetSelectionPath(root)); }
                catch (Exception) { selection = new ModSelection(); }
                foreach (var mod in discovery.Mods)
                {
                    if (!File.Exists(Path.Combine(mod.RootPath, "import.json"))) continue;
                    string id = mod.Id.Value + ":" + CharacterImporter.WarriorLocalId;
                    if (loaded.Contains(id)) continue;
                    bool enabled = selection.IsEnabled(mod.Id);
                    // Before game content loads, an enabled mod simply has not been read yet.
                    bool contentLoaded = ModRuntime.Scripts != null && !ModRuntime.Scripts.IsDisposed;
                    rows.Add(new LookRow
                    {
                        WarriorId = id, Name = mod.Manifest.Name, NeedsRestart = !enabled || contentLoaded,
                        DisabledMod = enabled ? null : mod, RootPath = mod.RootPath, Imported = true,
                        Detail = mod.Id.Value + (!enabled ? "  ·  mod disabled; choosing it enables it"
                            : contentLoaded ? "  ·  loads after a restart" : "  ·  imported character")
                    });
                }
            }
            catch (Exception error) { Debug.LogWarning("[Appearance] " + error.Message); }
            return rows;
        }

        private void DrawCharacters(int focus = 0)
        {
            Clear("Characters");
            Label(page, "Characters", 76, 96, 700, 64, 46, Ink);
            Label(page, "Choose who fights as Shadow. Items, stats and moves stay the same.",
                76, 166, 1120, 40, 19, Ink);
            var rows = CharacterLooks();
            string current = PlayerAppearance.SelectedWarrior;
            int pages = Math.Max(1, (rows.Count + LooksPerPage - 1) / LooksPerPage);
            lookPage = Math.Min(lookPage, pages - 1);
            for (int i = 0; i < LooksPerPage && lookPage * LooksPerPage + i < rows.Count; i++)
            {
                var row = rows[lookPage * LooksPerPage + i];
                int index = i;
                float y = 226 + i * 62;
                var name = Label(page, row.Name, 76, y, 700, 30, 24, Ink);
                name.supportRichText = false;
                name.resizeTextForBestFit = true; name.resizeTextMinSize = 16; name.resizeTextMaxSize = 24;
                var detail = Label(page, row.Detail, 76, y + 30, 700, 24, 16, Ink);
                detail.supportRichText = false;
                if (row.Imported) Button(page, "Share", 790, y, 150, 48, () => ShareCharacter(row), UiSound.Confirm);
                bool chosen = string.Equals(current, row.WarriorId, StringComparison.Ordinal);
                var use = Button(page, chosen ? "In use" : "Use", 952, y, 240, 48, () => ChooseLook(row, index), UiSound.Toggle);
                use.GetComponent<EclipseUiButton>().SetColors(
                    chosen ? Red : new Color(Ink.r, Ink.g, Ink.b, .38f),
                    chosen ? RedBright : new Color(Ink.r, Ink.g, Ink.b, .72f), Paper, Paper);
            }
            var status = Label(page, lookMessage ?? (rows.Count == 1
                    ? "No mod characters yet. Import a rigged humanoid model to add one."
                    : "Looks apply in campaign fights, the shop and on your own fighter in versus."),
                76, 500, 1120, 40, 18, Ink);
            status.supportRichText = false;
            if (pages > 1)
            {
                Button(page, "Previous", 76, 552, 240, 40, () => { lookPage = (lookPage + pages - 1) % pages; DrawCharacters(); }, UiSound.Tab);
                Label(page, (lookPage + 1) + " / " + pages, 332, 552, 105, 40, 20, Ink);
                Button(page, "Next", 446, 552, 190, 40, () => { lookPage = (lookPage + 1) % pages; DrawCharacters(); }, UiSound.Tab);
            }
            Button(page, "Back", 76, 604, 200, 48, OpenMods, UiSound.Back);
            var import = Button(page, "Import model...", 290, 604, 300, 48, PickCharacterModel);
            import.interactable = CharacterImporter.IsSupported;
            // Armor and helmets are fitted to Shadow's body; drawing them over another
            // character is optional.
            Button(page, PlayerAppearance.ShowArmor ? "Armor: shown" : "Armor: hidden", 604, 604, 300, 48, () =>
            {
                PlayerAppearance.ShowArmor = !PlayerAppearance.ShowArmor;
                lookMessage = PlayerAppearance.ShowArmor
                    ? "Equipped armor and helmets are drawn over the chosen character (fitted to Shadow's body)."
                    : "Armor and helmet visuals are hidden on the chosen character; their stats still apply.";
                DrawCharacters(controls.Count - 1);
            }, UiSound.Toggle);
            if (lookRestartPending) Button(page, "Apply & Restart", 918, 604, 274, 48, RestartForCharacters);
            FocusFirst();
            if (focus < controls.Count) controls[focus].Select();
        }

        private void ChooseLook(LookRow row, int focus)
        {
            try
            {
                if (row.DisabledMod != null)
                {
                    string root = ModHost.GetDefaultModsRoot();
                    string path = ModHost.GetSelectionPath(root);
                    ModSelection selection;
                    try { selection = ModSelection.Load(path); }
                    catch (Exception) { selection = new ModSelection(); }
                    selection.SetEnabled(row.DisabledMod.Id, true, ModDiscovery.DiscoverLoose(root).Mods);
                    selection.Save(path);
                }
                PlayerAppearance.SelectedWarrior = row.WarriorId;
                lookRestartPending |= row.NeedsRestart;
                lookMessage = row.NeedsRestart
                    ? row.Name + " is chosen. Apply & Restart to load it."
                    : (row.WarriorId.Length == 0 ? "Shadow uses his original look." : "Shadow now looks like " + row.Name + ".");
            }
            catch (Exception error) { lookMessage = "Could not choose this look: " + error.Message; }
            DrawCharacters(focus);
        }

        private void ShareCharacter(LookRow row)
        {
            try
            {
                string zip = CharacterImporter.ExportZip(row.RootPath);
                lookMessage = "Saved " + Path.GetFileName(zip) + " to " + Path.GetDirectoryName(zip) +
                    ". Only share models you have the right to share.";
                CharacterImporter.Reveal(zip);
            }
            catch (Exception error) { lookMessage = "Could not package this character: " + error.Message; }
            DrawCharacters();
        }

        private void RestartForCharacters()
        {
            DiscardGameDataPreview();
            if (!GameSessionRestart.TryRestart(() => PlayerPrefs.Save(), out var error))
            {
                lookMessage = error ?? "Restart already in progress.";
                DrawCharacters();
                return;
            }
            Clear("Restarting");
            Label(page, "Restarting to load your characters...", 76, 270, 1120, 120, 36, Ink, TextAnchor.MiddleCenter);
        }

        private void PickCharacterModel()
        {
            try
            {
                string path = ModZipPicker.PickDesktop("Import a rigged character", "Rigged models",
                    CharacterImporter.SourceExtensions);
                if (string.IsNullOrEmpty(path)) return;
                pendingImportSource = path;
                pendingImportVoice = "Male";
                pendingImportTitle = CharacterImporter.DefaultTitle(path);
                importMapping = null;
                importClips.Clear();
                importActions = null;
                DrawCharacterImportSetup();
            }
            catch (Exception error)
            {
                lookMessage = "Could not open file picker: " + error.Message;
                DrawCharacters();
            }
        }

        private void DrawCharacterImportSetup(string error = null)
        {
            Clear("Import character");
            Label(page, "Import a character", 76, 96, 1120, 64, 42, Ink);
            var file = Label(page, "Model: " + Path.GetFileName(pendingImportSource), 76, 170, 1120, 36, 20, Ink);
            file.supportRichText = false;
            Label(page, "Name", 76, 222, 700, 40, 24, Ink);
            // Voice picks the gendered grunts and hit cries; it also follows the look onto Shadow.
            Label(page, "Voice", 852, 222, 340, 40, 24, Ink);
            var fieldRoot = Rect(page, "Character name", 76, 262, 740, 68);
            var hit = fieldRoot.gameObject.AddComponent<Image>(); hit.color = new Color(Ink.r, Ink.g, Ink.b, .045f);
            var underline = Stroke(fieldRoot, "Ink underline", 0, 60, 740, 7, "Character name");
            underline.color = Red;
            underline.raycastTarget = false;
            var value = Label(fieldRoot, "", 18, 4, 704, 52, 28, Ink);
            value.supportRichText = false;
            var input = fieldRoot.gameObject.AddComponent<InputField>();
            input.targetGraphic = hit; input.textComponent = value;
            input.lineType = InputField.LineType.SingleLine;
            input.characterLimit = 80;
            input.text = pendingImportTitle ?? CharacterImporter.DefaultTitle(pendingImportSource);
            input.selectionColor = new Color(Red.r, Red.g, Red.b, .25f);
            input.onValueChanged.AddListener(text => pendingImportTitle = text);
            campaignNameField = input;
            controls.Add(input);
            Button(page, pendingImportVoice, 852, 270, 340, 52, () =>
            {
                pendingImportVoice = pendingImportVoice == "Male" ? "Female" : "Male";
                DrawCharacterImportSetup();
                if (controls.Count > 1) controls[1].Select();
            }, UiSound.Toggle, Look.Field);
            Button(page, importClips.Count == 0 ? "Animations: none (core moves)" : "Animations: " + importClips.Count + " on controls",
                76, 346, 560, 48, OpenImportActions, UiSound.Open, Look.Field);
            if (importMapping != null)
                Button(page, "Skeleton: matched by hand (" + importMapping.Count + " roles)", 652, 346, 540, 48,
                    () => DrawSkeletonMapper(), UiSound.Open, Look.Field);
            string blender = CharacterImporter.FindBlender();
            var blenderLabel = Label(page, blender == null
                    ? "Blender was not found. Install Blender 3.6 or newer (free, blender.org), or locate it."
                    : "Blender: " + blender, 76, 410, 1120, 32, 17, blender == null ? Red : Ink);
            blenderLabel.supportRichText = false;
            Label(page, "Works with humanoid rigs from Mixamo, VRoid, Unreal, Blender, Character Creator and similar. " +
                "Fighters are drawn as silhouettes, so textures are not used. You get a preview before keeping it.",
                76, 444, 1120, 52, 17, Ink);
            var errorText = Label(page, error ?? "", 76, 500, 1120, 90, 18, Red);
            errorText.supportRichText = false;
            Action submit = () => BeginCharacterImport(input.text);
            campaignNameSubmit = submit;
            Button(page, "Import", 742, 604, 450, 48, submit, UiSound.Begin);
            Button(page, "Locate Blender...", 330, 604, 380, 48, LocateBlender);
            Button(page, "Cancel", 76, 604, 240, 48, () => DrawCharacters(), UiSound.Back);
            FocusFirst();
        }

        private void LocateBlender()
        {
            try
            {
                bool windows = Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor;
                string path = windows ? ModZipPicker.PickDesktop("Locate blender.exe", "Blender", "exe")
                    : ModZipPicker.PickDesktop("Locate Blender", "Blender", "*");
                if (!string.IsNullOrEmpty(path)) CharacterImporter.BlenderOverride = path;
                DrawCharacterImportSetup();
            }
            catch (Exception error) { DrawCharacterImportSetup("Could not open file picker: " + error.Message); }
        }

        private void BeginCharacterImport(string title)
        {
            if (characterJob != null && characterJob.State == CharacterImportJob.JobState.Running) return;
            try
            {
                pendingImportTitle = title;
                characterJob = CharacterImporter.Start(pendingImportSource, title, pendingImportVoice, importMapping, importClips);
                characterJobVersion = -1;
                DrawCharacterImporting();
            }
            catch (Exception error) { DrawCharacterImportSetup(error.Message); }
        }

        private void DrawCharacterImporting()
        {
            Clear("Importing character");
            var title = Label(page, "Importing " + characterJob.Title, 76, 96, 1120, 64, 42, Ink);
            title.supportRichText = false;
            characterStatusLabel = Label(page, characterJob.Status, 76, 240, 1120, 120, 26, Ink, TextAnchor.MiddleCenter);
            characterStatusLabel.supportRichText = false;
            Label(page, "Blender converts the rig and mesh into a fighter silhouette in the background.",
                76, 380, 1120, 40, 18, Ink, TextAnchor.MiddleCenter);
            Button(page, "Cancel import", 76, 604, 320, 48, () => { characterJob?.Cancel(); }, UiSound.Back);
            FocusFirst();
        }

        // Called every frame: imports and animation listings run on worker threads.
        private void PollCharacterImport()
        {
            var actions = actionsJob;
            if (actions != null && actions.Version != actionsJobVersion)
            {
                actionsJobVersion = actions.Version;
                if (actions.State != CharacterImportJob.JobState.Running)
                {
                    actionsJob = null;
                    importActions = actions.State == CharacterImportJob.JobState.Succeeded ? new List<string>(actions.Actions) : new List<string>();
                    if (currentPage == "Import animations")
                        DrawImportActions(actions.State == CharacterImportJob.JobState.Succeeded ? null : actions.Status);
                }
            }
            var job = characterJob;
            if (job == null || job.Version == characterJobVersion) return;
            characterJobVersion = job.Version;
            if (currentPage != "Importing character") return;
            if (job.State == CharacterImportJob.JobState.Running)
            {
                if (characterStatusLabel != null) characterStatusLabel.text = job.Status;
                return;
            }
            characterJob = null;
            lastImport = job;
            DrawCharacterImportResult(job);
        }

        private void DrawCharacterImportResult(CharacterImportJob job)
        {
            if (job.State == CharacterImportJob.JobState.Canceled)
            {
                lookMessage = "Import canceled.";
                DrawCharacters();
                return;
            }
            bool success = job.State == CharacterImportJob.JobState.Succeeded;
            Clear("Character imported");
            Label(page, success ? job.Title : "Import failed", 76, 86, 1120, 56, 38, Ink).supportRichText = false;
            if (success)
            {
                PlayerAppearance.SelectedWarrior = job.WarriorId;
                lookRestartPending = true;
                bool hasPreview = ShowImportPreview(job.PreviewPath, 76, 150, 1120, 280);
                var text = Label(page, (hasPreview ? "Left: your model from the side. Then the fighter in the idle stance, a kick and a handspring. " : "") +
                    "It is installed as " + job.ModId + " and chosen as Shadow's look. Keep it with Apply & Restart, or discard it.",
                    76, hasPreview ? 440 : 200, 1120, 100, 19, Ink);
                text.supportRichText = false;
                Button(page, "Discard", 76, 604, 220, 48, () => DiscardImport(job), UiSound.Back);
                Button(page, "Share as ZIP", 310, 604, 260, 48, () =>
                {
                    try { string zip = CharacterImporter.ExportZip(job.OutputDirectory); CharacterImporter.Reveal(zip); }
                    catch (Exception error) { Debug.LogWarning("[CharacterImport] " + error.Message); }
                });
                Button(page, "Characters", 584, 604, 220, 48, () => DrawCharacters());
                Button(page, "Apply & Restart", 818, 604, 374, 48, RestartForCharacters, UiSound.Begin);
            }
            else
            {
                var text = Label(page, job.Status, 76, 160, 1120, 200, 21, Red);
                text.supportRichText = false;
                bool canMap = job.Bones.Count != 0;
                Label(page, canMap
                        ? "The skeleton could not be matched automatically. Match the missing body parts by hand, then import again."
                        : "Full details are in the game log.", 76, 380, 1120, 70, 19, Ink);
                Button(page, "Back", 76, 604, 240, 48, () => DrawCharacters(), UiSound.Back);
                if (canMap)
                {
                    Button(page, "Match skeleton...", 330, 604, 380, 48, () =>
                    {
                        mapperBones = new List<(int, string)>(job.Bones);
                        importMapping = new Dictionary<string, string>(StringComparer.Ordinal);
                        foreach (var pair in job.Suggested) importMapping[pair.Key] = pair.Value;
                        DrawSkeletonMapper();
                    }, UiSound.Open);
                }
                Button(page, "Try again", 742, 604, 450, 48, () => DrawCharacterImportSetup());
            }
            FocusFirst();
        }

        private bool ShowImportPreview(string path, float x, float y, float w, float h)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) return false;
                if (importPreview != null) Destroy(importPreview);
                importPreview = new Texture2D(2, 2, TextureFormat.RGB24, false);
                if (!importPreview.LoadImage(File.ReadAllBytes(path))) return false;
                // Fit the strip inside the box, keeping its aspect.
                float scale = Mathf.Min(w / importPreview.width, h / importPreview.height);
                float pw = importPreview.width * scale, ph = importPreview.height * scale;
                var image = Rect(page, "Import preview", x + (w - pw) / 2, y + (h - ph) / 2, pw, ph).gameObject.AddComponent<RawImage>();
                image.texture = importPreview;
                image.raycastTarget = false;
                return true;
            }
            catch (Exception error)
            {
                Debug.LogWarning("[CharacterImport] Preview unavailable: " + error.Message);
                return false;
            }
        }

        private void DiscardImport(CharacterImportJob job)
        {
            try
            {
                if (Directory.Exists(job.OutputDirectory)) Directory.Delete(job.OutputDirectory, true);
                if (PlayerAppearance.SelectedWarrior == job.WarriorId) PlayerAppearance.SelectedWarrior = string.Empty;
                lookMessage = job.Title + " was discarded.";
            }
            catch (Exception error) { lookMessage = "Could not remove " + job.ModId + ": " + error.Message; }
            DrawCharacters();
        }

        // Source animations (actions) to bring along, each bound to a control.
        private void OpenImportActions()
        {
            actionPage = 0;
            if (importActions == null && actionsJob == null)
            {
                try
                {
                    actionsJob = CharacterImporter.ListActions(pendingImportSource);
                    actionsJobVersion = -1;
                }
                catch (Exception error) { DrawCharacterImportSetup(error.Message); return; }
            }
            DrawImportActions();
        }

        private void DrawImportActions(string error = null)
        {
            Clear("Import animations");
            Label(page, "Animations", 76, 96, 1120, 64, 42, Ink);
            Label(page, "Bind source animations to controls. They play with normal SF2 timing but deal no damage until you " +
                "add attack timing in the mod's scripts/character.lua. Unbound animations are not imported.",
                76, 160, 1120, 56, 17, Ink);
            if (importActions == null)
            {
                Label(page, "Reading animations with Blender...", 76, 300, 1120, 60, 24, Ink, TextAnchor.MiddleCenter);
                Button(page, "Back", 76, 604, 240, 48, () => DrawCharacterImportSetup(), UiSound.Back);
                FocusFirst();
                return;
            }
            if (error != null) Label(page, error, 76, 226, 1120, 40, 18, Red).supportRichText = false;
            if (importActions.Count == 0)
                Label(page, "This file has no animations.", 76, 300, 1120, 60, 24, Ink, TextAnchor.MiddleCenter);
            int pages = Math.Max(1, (importActions.Count + ActionsPerPage - 1) / ActionsPerPage);
            actionPage = Math.Min(actionPage, pages - 1);
            for (int i = 0; i < ActionsPerPage && actionPage * ActionsPerPage + i < importActions.Count; i++)
            {
                string action = importActions[actionPage * ActionsPerPage + i];
                float y = 266 + i * 50;
                var name = Label(page, action, 76, y, 800, 44, 22, Ink);
                name.supportRichText = false; name.resizeTextForBestFit = true; name.resizeTextMinSize = 14; name.resizeTextMaxSize = 22;
                int bound = importClips.FindIndex(c => c.Action == action);
                Button(page, bound < 0 ? "Not imported" : importClips[bound].Key, 900, y, 292, 44,
                    () => { CycleClipKey(action); DrawImportActions(); }, UiSound.Toggle, Look.Field);
            }
            if (pages > 1)
            {
                Button(page, "Previous", 76, 568, 200, 32, () => { actionPage = (actionPage + pages - 1) % pages; DrawImportActions(); }, UiSound.Tab);
                Label(page, (actionPage + 1) + " / " + pages, 290, 568, 105, 32, 18, Ink);
                Button(page, "Next", 400, 568, 160, 32, () => { actionPage = (actionPage + 1) % pages; DrawImportActions(); }, UiSound.Tab);
            }
            Button(page, "Done", 742, 604, 450, 48, () => DrawCharacterImportSetup(), UiSound.Confirm);
            Button(page, "Clear all", 76, 604, 240, 48, () => { importClips.Clear(); DrawImportActions(); }, UiSound.Back);
            FocusFirst();
        }

        // Not imported -> first free control -> next free control ... -> not imported.
        private void CycleClipKey(string action)
        {
            int bound = importClips.FindIndex(c => c.Action == action);
            int start = bound < 0 ? 0 : Array.IndexOf(CharacterImporter.ClipKeys, importClips[bound].Key) + 1;
            if (bound >= 0) importClips.RemoveAt(bound);
            for (int k = start; k < CharacterImporter.ClipKeys.Length; k++)
            {
                string key = CharacterImporter.ClipKeys[k];
                if (importClips.Exists(c => c.Key == key)) continue;
                importClips.Add((ClipName(action), action, key));
                return;
            }
        }

        private string ClipName(string action)
        {
            var builder = new System.Text.StringBuilder();
            foreach (char c in action.ToLowerInvariant())
                builder.Append(c >= 'a' && c <= 'z' || c >= '0' && c <= '9' ? c : '_');
            string name = builder.ToString().Trim('_');
            if (name.Length == 0 || !(name[0] >= 'a' && name[0] <= 'z')) name = "move_" + name;
            if (name.Length > 40) name = name.Substring(0, 40);
            string unique = name;
            for (int i = 2; importClips.Exists(c => c.Name == unique); i++) unique = name + "_" + i;
            return unique;
        }

        // Manual role -> bone matching after automatic matching failed.
        private void DrawSkeletonMapper()
        {
            Clear("Match skeleton");
            Label(page, "Match the skeleton", 76, 86, 1120, 56, 38, Ink);
            Label(page, "Pick the source bone for each body part. Parts marked optional can stay empty; " +
                "bones you do not assign (fingers, twist, cloth) follow their nearest assigned parent.",
                76, 140, 1120, 44, 16, Ink);
            var roles = CharacterImporter.Roles;
            for (int i = 0; i < roles.Length; i++)
            {
                string role = roles[i];
                float x = i < 9 ? 76 : 642, y = 192 + (i % 9) * 44;
                importMapping.TryGetValue(role, out string bone);
                string text = RoleLabel(role) + ": " + (bone ?? (CharacterImporter.IsOptionalRole(role) ? "(optional)" : "choose..."));
                var button = Button(page, text, x, y, 550, 40, () => { bonePage = PageOf(bone); DrawBonePicker(role); },
                    UiSound.Open, Look.Field);
                if (bone == null && !CharacterImporter.IsOptionalRole(role))
                    button.GetComponent<EclipseUiButton>().SetColors(new Color(Red.r, Red.g, Red.b, .5f), Red, Red, Red);
            }
            bool complete = Array.TrueForAll(roles, r => CharacterImporter.IsOptionalRole(r) || importMapping.ContainsKey(r));
            Button(page, "Cancel", 76, 604, 240, 48, () => { importMapping = null; DrawCharacterImportSetup(); }, UiSound.Back);
            var go = Button(page, complete ? "Import with this skeleton" : "Choose the red parts first", 642, 604, 550, 48, () =>
            {
                if (Array.TrueForAll(roles, r => CharacterImporter.IsOptionalRole(r) || importMapping.ContainsKey(r)))
                    BeginCharacterImport(pendingImportTitle ?? CharacterImporter.DefaultTitle(pendingImportSource));
            }, UiSound.Begin);
            go.interactable = complete;
            FocusFirst();
        }

        private int PageOf(string bone)
        {
            int index = bone == null ? -1 : mapperBones.FindIndex(b => b.Name == bone);
            return index < 0 ? 0 : index / BonesPerPage;
        }

        private void DrawBonePicker(string role)
        {
            Clear("Pick bone");
            Label(page, "Choose the " + RoleLabel(role).ToLowerInvariant(), 76, 86, 1120, 56, 38, Ink);
            Label(page, "Bones are listed in hierarchy order; children are indented under their parent.", 76, 140, 1120, 36, 16, Ink);
            int pages = Math.Max(1, (mapperBones.Count + BonesPerPage - 1) / BonesPerPage);
            bonePage = Math.Min(Math.Max(0, bonePage), pages - 1);
            importMapping.TryGetValue(role, out string current);
            for (int i = 0; i < BonesPerPage && bonePage * BonesPerPage + i < mapperBones.Count; i++)
            {
                var (depth, name) = mapperBones[bonePage * BonesPerPage + i];
                float indent = Mathf.Min(depth, 16) * 18;
                var button = Button(page, name, 76 + indent, 184 + i * 44, 1116 - indent, 40, () =>
                {
                    foreach (var other in new List<string>(importMapping.Keys))
                        if (importMapping[other] == name) importMapping.Remove(other); // one role per bone
                    importMapping[role] = name;
                    DrawSkeletonMapper();
                }, UiSound.Confirm, Look.Field);
                if (name == current) button.GetComponent<EclipseUiButton>().SetColors(Red, RedBright, Red, Red);
            }
            if (pages > 1)
            {
                Button(page, "Previous", 330, 604, 200, 48, () => { bonePage = (bonePage + pages - 1) % pages; DrawBonePicker(role); }, UiSound.Tab);
                Label(page, (bonePage + 1) + " / " + pages, 540, 604, 120, 48, 20, Ink, TextAnchor.MiddleCenter);
                Button(page, "Next", 670, 604, 200, 48, () => { bonePage = (bonePage + 1) % pages; DrawBonePicker(role); }, UiSound.Tab);
            }
            Button(page, "Back", 76, 604, 240, 48, () => DrawSkeletonMapper(), UiSound.Back);
            Button(page, "None", 942, 604, 250, 48, () => { importMapping.Remove(role); DrawSkeletonMapper(); }, UiSound.Back);
            FocusFirst();
        }

        private static string RoleLabel(string role)
        {
            string text = role.Replace("left_", "Left ").Replace("right_", "Right ").Replace("_", " ");
            return char.ToUpperInvariant(text[0]) + text.Substring(1);
        }

        private void CharactersBack()
        {
            if (currentPage == "Importing character") characterJob?.Cancel();
            else if (currentPage == "Characters") OpenMods();
            else if (currentPage == "Pick bone") DrawSkeletonMapper();
            else if (currentPage == "Import animations" || currentPage == "Match skeleton") DrawCharacterImportSetup();
            else DrawCharacters();
        }
    }
}
