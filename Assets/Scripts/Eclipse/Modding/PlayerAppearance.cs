using System;
using System.Collections.Generic;
using UnityEngine;

namespace Eclipse.Modding
{
    /// <summary>
    /// Local, cosmetic choice of a mod warrior's skin models for the campaign
    /// player (Shadow). Shadow keeps his body, equipment, stats and moves; only
    /// armor and helmet geometry is replaced by the chosen warrior's skins.
    /// Stored in local preferences, never in a save, and ignored in versus play
    /// so both peers simulate identical fighter models.
    /// </summary>
    public static class PlayerAppearance
    {
        public sealed class Choice
        {
            public string WarriorId { get; internal set; }
            public string Name { get; internal set; }
            public string ModId { get; internal set; }
        }

        private const string PreferenceKey = "Eclipse.PlayerAppearance";
        private const string ArmorKey = "Eclipse.PlayerAppearance.ShowArmor";
        private static int _showArmor = -1;

        /// <summary>Draw equipped armor and helmets (fitted to Shadow) over the chosen look.</summary>
        public static bool ShowArmor
        {
            get
            {
                if (_showArmor < 0)
                {
                    try { _showArmor = PlayerPrefs.GetInt(ArmorKey, 0) != 0 ? 1 : 0; }
                    catch (Exception) { _showArmor = 0; }
                }
                return _showArmor == 1;
            }
            set
            {
                _showArmor = value ? 1 : 0;
                PlayerPrefs.SetInt(ArmorKey, _showArmor);
                PlayerPrefs.Save();
            }
        }
        private static string _selected;
        private static bool _loaded;
        private static object _resolvedFor;
        private static string _resolvedId;
        private static string[] _resolvedSkins = Array.Empty<string>();
        private static string _resolvedVoice;

        // Preferences are main-thread only; read them before model preparation can ask.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Preload() { _ = SelectedWarrior; _ = ShowArmor; }

        /// <summary>Qualified warrior ID ("mod:warriors/id"), or empty for Shadow's own look.</summary>
        public static string SelectedWarrior
        {
            get
            {
                if (!_loaded)
                {
                    try { _selected = PlayerPrefs.GetString(PreferenceKey, string.Empty); }
                    catch (Exception error) { Debug.LogWarning("[Appearance] " + error.Message); _selected = string.Empty; }
                    _loaded = true;
                }
                return _selected ?? string.Empty;
            }
            set
            {
                _selected = value ?? string.Empty;
                _loaded = true;
                _resolvedFor = null;
                PlayerPrefs.SetString(PreferenceKey, _selected);
                PlayerPrefs.Save();
            }
        }

        /// <summary>Skin model references and voice for the player look, or false for
        /// Shadow's own look. Voice is the warrior's own (for example "Female"), or
        /// null when it declares none so Shadow's voice is kept.</summary>
        internal static bool TryGetLook(out string[] skins, out string voice, bool versusFighter = false)
        {
            skins = Array.Empty<string>();
            voice = null;
            string selected = SelectedWarrior;
            // In versus only this device's own fighter takes the look; the campaign
            // roster fighter is never used there.
            bool versus = Eclipse.Multiplayer.LocalVersusSession.IsActive || Eclipse.Multiplayer.LocalVersusSession.IsStarting;
            if (selected.Length == 0 || versus && !versusFighter) return false;
            var scripts = ModRuntime.Scripts;
            if (scripts == null || scripts.IsDisposed) return false;
            if (!ReferenceEquals(_resolvedFor, scripts) || _resolvedId != selected)
            {
                _resolvedFor = scripts; _resolvedId = selected;
                _resolvedSkins = Array.Empty<string>();
                _resolvedVoice = null;
                if (DefinitionId.TryParse(selected, out var id) && scripts.Content.TryGetWarrior(id, out var warrior) &&
                    warrior.SkinModels.Count > 0)
                {
                    var references = new string[warrior.SkinModels.Count];
                    for (int i = 0; i < references.Length; i++) references[i] = warrior.SkinModels[i].ToString();
                    _resolvedSkins = references;
                    _resolvedVoice = string.IsNullOrEmpty(warrior.Voice) ? null : warrior.Voice;
                }
                else Debug.LogWarning("[Appearance] Selected look is not loaded; using Shadow: " + selected);
            }
            skins = _resolvedSkins;
            voice = _resolvedVoice;
            return skins.Length != 0;
        }

        /// <summary>Loaded mod warriors that declare skin models, in mod then registration order.</summary>
        public static List<Choice> LoadedChoices()
        {
            var result = new List<Choice>();
            var scripts = ModRuntime.Scripts;
            if (scripts == null || scripts.IsDisposed) return result;
            var names = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var mod in scripts.ActiveMods) names[mod.Id.Value] = mod.Manifest.Name;
            var perMod = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var warrior in scripts.Content.Warriors)
                if (warrior.SkinModels.Count > 0)
                    perMod[warrior.Id.Namespace.Value] = perMod.TryGetValue(warrior.Id.Namespace.Value, out int n) ? n + 1 : 1;
            foreach (var warrior in scripts.Content.Warriors)
            {
                if (warrior.SkinModels.Count == 0) continue;
                string mod = warrior.Id.Namespace.Value;
                string name = names.TryGetValue(mod, out string title) ? title : mod;
                if (perMod[mod] > 1) name += " (" + warrior.Id.LocalId + ")";
                result.Add(new Choice { WarriorId = warrior.Id.ToString(), Name = name, ModId = mod });
            }
            return result;
        }
    }
}
