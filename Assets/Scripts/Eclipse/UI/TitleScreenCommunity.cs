using System;
using System.Collections.Generic;
using System.Linq;
using Eclipse.Modding;
using UnityEngine;
using UnityEngine.UI;

namespace Eclipse.UI
{
    /// <summary>Opt-in for the in-game mod.io browser (Options > Mod settings). Off by default.</summary>
    public static class CommunityModsSetting
    {
        private const string Key = "Eclipse.CommunityMods";
        public static bool Enabled => PlayerPrefs.GetInt(Key, 0) == 1;
        public static void Toggle() { PlayerPrefs.SetInt(Key, Enabled ? 0 : 1); PlayerPrefs.Save(); }
    }

    // Community mods: Project Eclipse's mods on mod.io, browsed and installed in game. Shown only
    // when the player opts in. Definitive Edition ships with the game and is never listed here.
    public sealed partial class TitleScreen
    {
        private const int CommunityPerPage = 4;
        private string communitySearch = string.Empty;
        private ModIoClient.Sort communitySort = ModIoClient.Sort.Popular;
        private int communityOffset;
        private ModIoPage communityPage;
        private string communityMessage;
        private bool communityBusy;
        private int communityRequest;
        private readonly Dictionary<string, Texture2D> communityThumbnails = new Dictionary<string, Texture2D>();
        private static bool communityUpdatesStarted;

        private static ISet<string> LoadedModIds()
        {
            try { return new HashSet<string>(ModRuntime.Host.EnabledMods.Select(m => m.Id.Value), StringComparer.Ordinal); }
            catch (Exception) { return new HashSet<string>(StringComparer.Ordinal); }
        }

        /// <summary>Once per session, with the opt-in on: update installed community mods.</summary>
        private async void StartCommunityUpdates()
        {
            if (communityUpdatesStarted || !CommunityModsSetting.Enabled) return;
            communityUpdatesStarted = true;
            try
            {
                var report = await ModIoClient.UpdateInstalledOnce(ModHost.GetDefaultModsRoot(), LoadedModIds());
                var parts = new List<string>();
                if (report.Installed.Count > 0) parts.Add("Updated " + string.Join(", ", report.Installed) + ".");
                if (report.NextLaunch.Count > 0) parts.Add(string.Join(", ", report.NextLaunch) + " will update when the game next starts.");
                if (report.Failed.Count > 0) parts.Add("Could not update " + string.Join(", ", report.Failed) + ".");
                if (parts.Count > 0)
                {
                    modMessage = "Community mods: " + string.Join(" ", parts);
                    Debug.Log("[mod.io] " + modMessage);
                }
            }
            catch (Exception error) { Debug.LogWarning("[mod.io] Update check failed: " + error.Message); }
        }

        private void OpenCommunity()
        {
            communityOffset = 0;
            communityPage = null;
            communityMessage = null;
            DrawCommunity();
            LoadCommunity();
        }

        private async void LoadCommunity()
        {
            int request = ++communityRequest;
            communityBusy = true;
            communityMessage = "Loading mods from mod.io...";
            DrawCommunity();
            try
            {
                var result = await ModIoClient.List(communitySearch, communitySort, communityOffset, CommunityPerPage);
                if (request != communityRequest) return;
                communityPage = result;
                communityMessage = result.Total == 0
                    ? (communitySearch.Length > 0 ? "No mod matches \"" + communitySearch + "\"." : "No community mods yet.")
                    : null;
            }
            catch (Exception error)
            {
                if (request != communityRequest) return;
                communityMessage = "Could not reach mod.io: " + error.Message;
            }
            communityBusy = false;
            if (currentPage == "Community mods") DrawCommunity();
        }

        private void DrawCommunity()
        {
            Clear("Community mods");
            Label(page, "Community mods", 76, 96, 700, 64, 46, Ink);
            Label(page, "Made by players, not by the Definitive Edition team.  Powered by mod.io", 640, 108, 552, 40, 17, Ink, TextAnchor.MiddleRight);

            // Search and sort.
            var fieldRoot = Rect(page, "Search", 76, 168, 520, 52);
            var hit = fieldRoot.gameObject.AddComponent<Image>(); hit.color = new Color(Ink.r, Ink.g, Ink.b, .045f);
            var underline = Stroke(fieldRoot, "Ink underline", 0, 44, 520, 7, "Search");
            underline.color = Red; underline.raycastTarget = false;
            var value = Label(fieldRoot, "", 14, 4, 492, 40, 22, Ink); value.supportRichText = false;
            var hint = Label(fieldRoot, "Search mods", 14, 4, 492, 40, 22, new Color(Ink.r, Ink.g, Ink.b, .4f)); hint.fontStyle = FontStyle.Italic;
            var input = fieldRoot.gameObject.AddComponent<InputField>();
            input.targetGraphic = hit; input.textComponent = value; input.placeholder = hint;
            input.lineType = InputField.LineType.SingleLine; input.characterLimit = 64;
            input.text = communitySearch;
            input.selectionColor = new Color(Red.r, Red.g, Red.b, .25f);
            input.onEndEdit.AddListener(text =>
            {
                if (text == communitySearch) return;
                communitySearch = text.Trim();
                communityOffset = 0;
                LoadCommunity();
            });
            controls.Add(input);
            Button(page, "Search", 612, 172, 170, 44, () => { communitySearch = input.text.Trim(); communityOffset = 0; LoadCommunity(); });
            Button(page, "Sort: " + ModIoClient.SortName(communitySort), 798, 172, 394, 44, () =>
            {
                communitySort = (ModIoClient.Sort)(((int)communitySort + 1) % 4);
                communityOffset = 0;
                LoadCommunity();
            }, UiSound.Toggle);

            // Rows.
            var state = ModIoState.Load(ModHost.GetDefaultModsRoot());
            var mods = communityPage?.Mods ?? new List<ModIoMod>();
            for (int i = 0; i < mods.Count && i < CommunityPerPage; i++)
            {
                var mod = mods[i];
                float y = 236 + i * 76;
                var thumb = Rect(page, "Thumbnail", 76, y, 112, 63).gameObject.AddComponent<RawImage>();
                thumb.color = new Color(Ink.r, Ink.g, Ink.b, .12f);
                thumb.raycastTarget = false;
                ShowThumbnail(thumb, mod.LogoUrl);
                var name = Label(page, mod.Name, 204, y - 2, 640, 32, 24, Ink); name.supportRichText = false;
                name.resizeTextForBestFit = true; name.resizeTextMinSize = 16; name.resizeTextMaxSize = 24;
                var install = state.Find(mod.Id);
                string badge = install == null ? "" : install.PendingModfileId == mod.Modfile.Id ? "   ·   update on next start"
                    : install.ModfileId == mod.Modfile.Id ? "   ·   installed" : "   ·   update available";
                if (install != null && install.ChangesMovesets) badge += "   ·   turns off online play";
                var line = Label(page, "by " + mod.Author + (mod.Modfile.Version.Length > 0 ? "   ·   " + mod.Modfile.Version : "") + "   ·   " +
                    ModIoText.Size(mod.Modfile.FileSize) + badge, 204, y + 28, 640, 22, 15, Ink);
                line.supportRichText = false;
                var summary = Label(page, Shorten(mod.Summary, 110), 204, y + 48, 640, 22, 15, new Color(Ink.r, Ink.g, Ink.b, .75f));
                summary.supportRichText = false;
                var captured = mod;
                Button(page, "Details", 860, y + 6, 150, 44, () => DrawCommunityMod(captured), UiSound.Open);
                string action = install == null ? "Install" : install.ModfileId == mod.Modfile.Id || install.PendingModfileId == mod.Modfile.Id ? "Installed" : "Update";
                var button = Button(page, action, 1022, y + 6, 170, 44, () => InstallCommunityMod(captured, () => DrawCommunity()));
                button.interactable = action != "Installed" && !communityBusy;
            }

            // Paging, status and navigation.
            int total = communityPage?.Total ?? 0;
            int pages = Math.Max(1, (total + CommunityPerPage - 1) / CommunityPerPage);
            int current = communityOffset / CommunityPerPage;
            if (pages > 1)
            {
                Button(page, "Previous", 76, 552, 200, 40, () => { communityOffset = Math.Max(0, communityOffset - CommunityPerPage); LoadCommunity(); }, UiSound.Tab)
                    .interactable = current > 0 && !communityBusy;
                Label(page, (current + 1) + " / " + pages, 290, 552, 110, 40, 20, Ink);
                Button(page, "Next", 406, 552, 200, 40, () => { communityOffset += CommunityPerPage; LoadCommunity(); }, UiSound.Tab)
                    .interactable = current < pages - 1 && !communityBusy;
            }
            var status = Label(page, communityMessage ?? "Installed mods appear on the Mods screen. Apply & Restart there to load them.",
                640, 552, 552, 40, 16, communityMessage != null && communityMessage.StartsWith("Could not", StringComparison.Ordinal) ? Red : Ink, TextAnchor.MiddleRight);
            status.supportRichText = false;
            Button(page, "Back to Mods", 76, 604, 320, 48, OpenMods, UiSound.Back);
            Button(page, "Open mod.io", 410, 604, 290, 48, () => Application.OpenURL(ModIoConfig.ProfileUrl));
            FocusFirst();
        }

        private void DrawCommunityMod(ModIoMod mod)
        {
            Clear("Community mod");
            var title = Label(page, mod.Name, 76, 96, 1120, 64, 42, Ink); title.supportRichText = false;
            title.resizeTextForBestFit = true; title.resizeTextMinSize = 24; title.resizeTextMaxSize = 42;
            var thumb = Rect(page, "Logo", 76, 180, 320, 180).gameObject.AddComponent<RawImage>();
            thumb.color = new Color(Ink.r, Ink.g, Ink.b, .12f); thumb.raycastTarget = false;
            ShowThumbnail(thumb, mod.LogoUrl);
            var install = ModIoState.Load(ModHost.GetDefaultModsRoot()).Find(mod.Id);
            var facts = new List<string>
            {
                "by " + mod.Author,
                "Version " + (mod.Modfile.Version.Length > 0 ? mod.Modfile.Version : "?") + "   ·   " + ModIoText.Size(mod.Modfile.FileSize),
                "Updated " + ModIoText.Date(mod.DateUpdated) + "   ·   " + mod.Downloads + " downloads" + (mod.Rating.Length > 0 ? "   ·   " + mod.Rating : ""),
            };
            if (mod.Tags.Count > 0) facts.Add("Tags: " + string.Join(", ", mod.Tags));
            if (install != null)
            {
                facts.Add(install.PendingModfileId == mod.Modfile.Id ? "Installed; the new version installs when the game next starts."
                    : install.ModfileId == mod.Modfile.Id ? "Installed (" + install.ModId + ")." : "Installed; an update is available.");
                if (install.ChangesMovesets) facts.Add("Changes movesets: online versus is off while it is enabled.");
                if (install.UndeclaredCompatibility) facts.Add("Does not say which Eclipse version it is made for; it may not work.");
            }
            var info = Label(page, string.Join("\n", facts), 420, 180, 776, 180, 18, Ink); info.supportRichText = false;
            info.alignment = TextAnchor.UpperLeft;
            string text = mod.Description.Length > 0 ? mod.Description : mod.Summary;
            var description = Label(page, Shorten(text, 700), 76, 380, 1120, 160, 17, Ink); description.supportRichText = false;
            description.alignment = TextAnchor.UpperLeft;
            var status = Label(page, communityMessage ?? "Report or rate this mod on its mod.io page.", 76, 552, 1120, 40, 16, Ink); status.supportRichText = false;
            Button(page, "Back", 76, 604, 220, 48, () => { communityMessage = null; DrawCommunity(); }, UiSound.Back);
            Button(page, "View on mod.io", 310, 604, 300, 48, () => Application.OpenURL(mod.ProfileUrl.Length > 0 ? mod.ProfileUrl : ModIoConfig.ProfileUrl));
            if (install != null)
                Button(page, "Remove", 624, 604, 220, 48, () => RemoveCommunityMod(mod), UiSound.Back);
            string action = install == null ? "Install" : install.ModfileId == mod.Modfile.Id || install.PendingModfileId == mod.Modfile.Id ? "Installed" : "Update";
            var button = Button(page, action, 858, 604, 334, 48, () => InstallCommunityMod(mod, () => DrawCommunityMod(mod)));
            button.interactable = action != "Installed" && !communityBusy;
            FocusFirst();
        }

        private async void InstallCommunityMod(ModIoMod mod, Action redraw)
        {
            if (communityBusy) return;
            communityBusy = true;
            communityMessage = "Downloading " + mod.Name + "...";
            redraw();
            try
            {
                var result = await ModIoClient.Install(mod, ModHost.GetDefaultModsRoot(), LoadedModIds());
                communityMessage = (result.Deferred ? mod.Name + " will update when the game next starts."
                    : (result.Updated ? "Updated " : "Installed ") + mod.Name + ". Apply & Restart on the Mods screen to load it.") +
                    (result.Notice.Length > 0 ? " " + result.Notice : "");
                modDiscovery = ModDiscovery.DiscoverLoose(ModHost.GetDefaultModsRoot());
            }
            catch (Exception error)
            {
                communityMessage = "Could not install " + mod.Name + ": " + error.Message;
            }
            communityBusy = false;
            if (currentPage == "Community mods" || currentPage == "Community mod") redraw();
        }

        private void RemoveCommunityMod(ModIoMod mod)
        {
            try
            {
                ModIoClient.Remove(mod.Id, ModHost.GetDefaultModsRoot());
                communityMessage = "Removed " + mod.Name + ". Apply & Restart on the Mods screen to unload it.";
            }
            catch (Exception error) { communityMessage = "Could not remove " + mod.Name + ": " + error.Message; }
            DrawCommunityMod(mod);
        }

        private async void ShowThumbnail(RawImage image, string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            if (!communityThumbnails.TryGetValue(url, out var texture))
            {
                try { texture = await ModIoClient.Thumbnail(url); }
                catch (Exception) { texture = null; }
                communityThumbnails[url] = texture;
            }
            if (image == null || texture == null) return;
            image.texture = texture;
            image.color = Color.white;
        }

        private static string Shorten(string text, int length)
        {
            text = (text ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Trim();
            return text.Length <= length ? text : text.Substring(0, length - 1).TrimEnd() + "…";
        }
    }
}
