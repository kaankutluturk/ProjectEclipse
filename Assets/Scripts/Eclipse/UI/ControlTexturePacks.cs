using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Eclipse.UI
{
    // On-screen fight control texture packs.
    //
    // A pack is a folder of layer PNGs with fixed names (see Recipes; the shipped DE128
    // pack is the reference). Each FightButtons atlas member is composited from its
    // layers: centred at their native pixel size on a canvas the size of the first
    // layer, later layers drawn over earlier ones, or cut out of them on pressed
    // buttons so the icon reads on the solid background. A PNG named after a member
    // (e.g. Joystick_norm.png) is used as that member's finished image instead of its
    // recipe. Members whose layers are not all present, and members without a recipe or
    // an image, keep the built-in art.
    // Pack folders are looked up, first match by name wins, in:
    //   <Application.persistentDataPath>/ControlPacks/<name>/
    //   <game folder next to the data folder>/ControlPacks/<name>/
    //   <Application.streamingAssetsPath>/ControlPacks/<name>/
    // "Default" is the built-in art. Sprites are created at runtime with
    // the original sprite's pivot, border and on-screen size (pixels-per-unit is scaled by
    // the PNG width over the original width), so higher resolution art keeps the layout.
    // The selection is stored in PlayerPrefs "Eclipse.ControlPack" and applies to controls
    // created after the change (the next fight or dojo scene load).
    public static class ControlTexturePacks
    {
        public const string DefaultName = "Default";
        public const string AtlasPath = "UI/Atlases/FightButtons";
        private const string PrefKey = "Eclipse.ControlPack";
        private const string MemberPrefix = "FightButtons.";

        private enum Blend { Over, Cut }

        private struct Layer
        {
            public readonly string File;
            public readonly Blend Blend;
            public Layer(string file, Blend blend = Blend.Over) { File = file; Blend = blend; }
        }

        private static Layer[] Button(string icon, bool pressed) => new[]
        {
            new Layer(pressed ? "fight_bg_active" : "fight_bg_normal"),
            new Layer("fight_bg_frame"),
            new Layer(icon, pressed ? Blend.Cut : Blend.Over),
        };

        // FightButtons member -> layer files (without .png). The pause button and the
        // ranged ammo ring have no layers and keep the built-in art.
        private static readonly Dictionary<string, Layer[]> Recipes = new Dictionary<string, Layer[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["btn_punch_normal"] = Button("icon_Punch", false),
            ["btn_punch_action"] = Button("icon_Punch", true),
            ["btn_kick_normal"] = Button("icon_Kick", false),
            ["btn_kick_action"] = Button("icon_Kick", true),
            ["btn_magic_normal"] = Button("icon_Magic", false),
            ["btn_magic_action"] = Button("icon_Magic", true),
            ["btn_throw_normal"] = Button("ranged_attack", false),
            ["btn_throw_action"] = Button("ranged_attack", true),
            ["btn_charge_normal"] = Button("icon_Charge", false),
            ["btn_charge_action"] = Button("icon_Charge", true),
            ["JoystickContainer_norm"] = new[] { new Layer("fight_bg_frame_big"), new Layer("stick_arrows") },
            ["JoystickContainer_action"] = new[] { new Layer("fight_bg_frame_big"), new Layer("stick_arrows") },
            ["Joystick_norm"] = new[] { new Layer("fight_bg_normal"), new Layer("fight_bg_frame") },
            ["Joystick_action"] = new[] { new Layer("fight_bg_active"), new Layer("fight_bg_frame") },
            ["Highlight_Stick"] = new[] { new Layer("icon_HighlightStick") },
            ["Kick_Highlight"] = new[] { new Layer("icon_KickHighlight") },
            ["Charge_Highlight"] = new[] { new Layer("icon_ChargeHighlight") },
            ["magic_progress"] = new[] { new Layer("icon_Stroke") },
            ["magic_full"] = new[] { new Layer("icon_Magic_Full") },
            ["ranged_full"] = new[] { new Layer("icon_Range_Full") },
        };

        private static readonly List<string> names = new List<string>();
        private static readonly Dictionary<string, string> folders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
        private static bool scanned;
        private static string current;

        public static IList<string> Available
        {
            get { EnsureScanned(); return names.AsReadOnly(); }
        }

        // Selected pack name; "Default" when none is chosen or the saved pack is missing.
        public static string Current
        {
            get
            {
                EnsureScanned();
                if (current == null)
                {
                    string saved = PlayerPrefs.GetString(PrefKey, DefaultName);
                    current = folders.ContainsKey(saved) ? CanonicalName(saved) : DefaultName;
                }
                return current;
            }
            set
            {
                EnsureScanned();
                string next = !string.IsNullOrEmpty(value) && folders.ContainsKey(value) ? CanonicalName(value) : DefaultName;
                if (next == Current) return;
                current = next;
                PlayerPrefs.SetString(PrefKey, next);
                PlayerPrefs.Save();
            }
        }

        public static string Label
        {
            get { return Current == DefaultName ? "DEFAULT" : Current.ToUpperInvariant(); }
        }

        public static void Cycle()
        {
            Refresh();
            int index = names.IndexOf(Current);
            Current = names[(index + 1) % names.Count];
        }

        // Rescans the pack folders (call before showing the option). Drops a saved
        // pack whose folder was removed back to Default without forgetting the pref.
        public static void Refresh()
        {
            scanned = false;
            current = null;
            EnsureScanned();
        }

        // Called from AtlasCache after mod sprite replacement. Returns false for any
        // other atlas, for the Default pack and for members the pack does not provide.
        public static bool TryGetSprite(string atlas, string member, Func<string, string, Sprite> original, out Sprite sprite)
        {
            sprite = null;
            if (string.IsNullOrEmpty(member) || !string.Equals(Normalize(atlas), AtlasPath, StringComparison.OrdinalIgnoreCase))
                return false;
            string pack = Current;
            if (pack == DefaultName) return false;
            string key = pack + "|" + member;
            if (sprites.TryGetValue(key, out sprite)) return sprite != null;
            sprite = Load(folders[pack], atlas, member, original);
            sprites[key] = sprite;
            return sprite != null;
        }

        // Sprite-swap states (the fight buttons' pressed art) are serialized sprite
        // references, not atlas lookups; replace the FightButtons ones with the pack's.
        public static void ApplyToSpriteSwap(UnityEngine.UI.Selectable selectable)
        {
            if (selectable == null || selectable.transition != UnityEngine.UI.Selectable.Transition.SpriteSwap) return;
            var state = selectable.spriteState;
            bool changed = false;
            state.highlightedSprite = PackVersion(state.highlightedSprite, ref changed);
            state.pressedSprite = PackVersion(state.pressedSprite, ref changed);
            state.selectedSprite = PackVersion(state.selectedSprite, ref changed);
            state.disabledSprite = PackVersion(state.disabledSprite, ref changed);
            if (changed) selectable.spriteState = state;
        }

        private static Sprite PackVersion(Sprite sprite, ref bool changed)
        {
            if (sprite == null || !sprite.name.StartsWith(MemberPrefix, StringComparison.Ordinal)) return sprite;
            Sprite builtIn = sprite;
            if (!TryGetSprite(AtlasPath, sprite.name, (atlas, member) => builtIn, out Sprite packed)) return sprite;
            changed = true;
            return packed;
        }

        private static Sprite Load(string folder, string atlas, string member, Func<string, string, Sprite> original)
        {
            string shortName = member.StartsWith(MemberPrefix, StringComparison.OrdinalIgnoreCase) ? member.Substring(MemberPrefix.Length) : member;
            // A finished image named after the member (e.g. Joystick_norm.png) wins over its recipe.
            Layer[] layers = File.Exists(LayerPath(folder, shortName)) ? new[] { new Layer(shortName) }
                : Recipes.TryGetValue(shortName, out Layer[] recipe) ? recipe : null;
            if (layers == null) return null;
            foreach (Layer layer in layers)
                if (!File.Exists(LayerPath(folder, layer.File))) return null;
            Sprite source = original != null ? original(atlas, member) : null;
            Texture2D texture = null;
            try
            {
                texture = Composite(folder, layers);
                if (texture == null) return null;
                texture.name = member;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.filterMode = FilterMode.Bilinear;
                Vector2 pivot = new Vector2(.5f, .5f);
                float pixelsPerUnit = 100f;
                Vector4 border = Vector4.zero;
                if (source != null && source.rect.width > 0f && source.rect.height > 0f)
                {
                    float scale = texture.width / source.rect.width;
                    pivot = new Vector2(source.pivot.x / source.rect.width, source.pivot.y / source.rect.height);
                    pixelsPerUnit = source.pixelsPerUnit * scale;
                    border = source.border * scale;
                }
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot, pixelsPerUnit,
                    0u, SpriteMeshType.FullRect, border);
                sprite.name = member;
                return sprite;
            }
            catch (Exception exception)
            {
                if (texture != null) UnityEngine.Object.Destroy(texture);
                Debug.LogWarning("[ControlPacks] Could not build " + member + " from " + folder + ": " + exception.Message);
                return null;
            }
        }

        private static string LayerPath(string folder, string layer) => Path.Combine(folder, layer + ".png");

        private static Texture2D Decode(string file)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (ImageConversion.LoadImage(texture, File.ReadAllBytes(file), false)) return texture;
            UnityEngine.Object.Destroy(texture);
            Debug.LogWarning("[ControlPacks] Could not decode " + file);
            return null;
        }

        // A single layer is used as is; otherwise the later layers are blended, centred,
        // onto a canvas the size of the first layer.
        private static Texture2D Composite(string folder, Layer[] layers)
        {
            Texture2D canvas = Decode(LayerPath(folder, layers[0].File));
            if (canvas == null || layers.Length == 1) return canvas;
            int width = canvas.width, height = canvas.height;
            Color32[] target = canvas.GetPixels32();
            for (int i = 1; i < layers.Length; i++)
            {
                Texture2D layer = Decode(LayerPath(folder, layers[i].File));
                if (layer == null) { UnityEngine.Object.Destroy(canvas); return null; }
                Color32[] pixels = layer.GetPixels32();
                int lw = layer.width, lh = layer.height;
                int ox = (width - lw) / 2, oy = (height - lh) / 2;
                UnityEngine.Object.Destroy(layer);
                bool cut = layers[i].Blend == Blend.Cut;
                for (int y = 0; y < lh; y++)
                {
                    int ty = y + oy;
                    if (ty < 0 || ty >= height) continue;
                    for (int x = 0; x < lw; x++)
                    {
                        int tx = x + ox;
                        if (tx < 0 || tx >= width) continue;
                        Color32 src = pixels[y * lw + x];
                        if (src.a == 0) continue;
                        int index = ty * width + tx;
                        target[index] = cut ? Cut(target[index], src) : Over(target[index], src);
                    }
                }
            }
            canvas.SetPixels32(target);
            canvas.Apply(false, false);
            return canvas;
        }

        private static Color32 Over(Color32 dst, Color32 src)
        {
            float sa = src.a / 255f, da = dst.a / 255f;
            float a = sa + da * (1f - sa);
            if (a <= 0f) return new Color32(0, 0, 0, 0);
            float k = da * (1f - sa);
            return new Color32(
                (byte)Mathf.RoundToInt((src.r * sa + dst.r * k) / a),
                (byte)Mathf.RoundToInt((src.g * sa + dst.g * k) / a),
                (byte)Mathf.RoundToInt((src.b * sa + dst.b * k) / a),
                (byte)Mathf.RoundToInt(a * 255f));
        }

        // The layer's coverage removes the canvas: a see-through icon on a solid button.
        private static Color32 Cut(Color32 dst, Color32 src)
        {
            dst.a = (byte)Mathf.RoundToInt(dst.a * (1f - src.a / 255f));
            return dst;
        }

        private static void EnsureScanned()
        {
            if (scanned) return;
            scanned = true;
            names.Clear();
            folders.Clear();
            names.Add(DefaultName);
            foreach (string root in Roots())
            {
                string[] directories;
                try
                {
                    if (!Directory.Exists(root)) continue;
                    directories = Directory.GetDirectories(root);
                }
                catch (Exception) { continue; }
                Array.Sort(directories, StringComparer.OrdinalIgnoreCase);
                foreach (string directory in directories)
                {
                    string name = Path.GetFileName(directory);
                    if (string.IsNullOrEmpty(name) || folders.ContainsKey(name) ||
                        string.Equals(name, DefaultName, StringComparison.OrdinalIgnoreCase)) continue;
                    bool hasPng;
                    try { hasPng = Directory.GetFiles(directory, "*.png").Length > 0; }
                    catch (Exception) { hasPng = false; }
                    if (!hasPng) continue;
                    folders[name] = directory;
                    names.Add(name);
                }
            }
            // Default resolves through the regular atlas path.
            folders[DefaultName] = null;
        }

        private static IEnumerable<string> Roots()
        {
            yield return Path.Combine(Application.persistentDataPath, "ControlPacks");
            string gameFolder = null;
            try { gameFolder = Path.GetDirectoryName(Application.dataPath); } catch (Exception) { }
            if (!string.IsNullOrEmpty(gameFolder)) yield return Path.Combine(gameFolder, "ControlPacks");
            yield return Path.Combine(Application.streamingAssetsPath, "ControlPacks");
        }

        private static string CanonicalName(string name)
        {
            foreach (string known in names)
                if (string.Equals(known, name, StringComparison.OrdinalIgnoreCase)) return known;
            return DefaultName;
        }

        private static string Normalize(string atlas)
        {
            return string.IsNullOrEmpty(atlas) ? string.Empty : atlas.Replace('\\', '/').Trim('/');
        }

        // Keep cached sprites alive while existing controls reference them. Refreshing
        // option labels must not discard ownership and leak a new copy on every visit.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            foreach (Sprite sprite in sprites.Values)
            {
                if (sprite == null) continue;
                if (sprite.texture != null) UnityEngine.Object.Destroy(sprite.texture);
                UnityEngine.Object.Destroy(sprite);
            }
            sprites.Clear();
            scanned = false;
            current = null;
        }
    }
}
