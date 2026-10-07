using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using Eclipse.Content.TarAssets;
using UnityEngine;

namespace Eclipse.Modding
{
    public interface IRuntimeAssetProvider
    {
        bool TryLoadUnityAsset<T>(AssetId id, out T asset) where T : UnityEngine.Object;
        bool TryLoadUnityAssets<T>(AssetId id, out T[] assets) where T : UnityEngine.Object;
        bool TryLoadModelText(AssetId id, out string text);
    }

    public sealed class ModAssetLoader : IDisposable
    {
        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);
        private const int MaxTextBytes = 64 * 1024 * 1024;

        private readonly AssetResolver _resolver;
        private readonly Dictionary<AssetId, Sprite> _sprites = new Dictionary<AssetId, Sprite>();
        // Import settings belong to the texture instance. Cache variants so descriptors with
        // different settings cannot mutate each other's textures or depend on load order.
        private readonly Dictionary<(AssetId, FilterMode, TextureWrapMode, bool), Texture2D> _textures =
            new Dictionary<(AssetId, FilterMode, TextureWrapMode, bool), Texture2D>();
        private readonly Dictionary<AssetId, AudioClip> _audio = new Dictionary<AssetId, AudioClip>();

        public ModAssetLoader(AssetResolver resolver)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        private readonly Dictionary<string, Sprite> _replacementMembers = new Dictionary<string, Sprite>();
        public Sprite LoadReplacementMember(AssetId id, string memberName)
        {
            string key = id + "|" + memberName;
            if (_replacementMembers.TryGetValue(key, out var cached) && cached != null) return cached;
            var source = LoadSprite(id);
            if (source == null) throw new InvalidDataException("Replacement sprite is missing: " + id);
            var member = UnityEngine.Object.Instantiate(source);
            member.name = memberName;
            _replacementMembers.Add(key, member);
            return member;
        }

        public T LoadUnityAsset<T>(AssetId id) where T : UnityEngine.Object
        {
            id = _resolver.Resolve(id);
            AssetMetadata metadata;
            if (!_resolver.TryDescribe(id, out metadata)) return null;
            IRuntimeAssetProvider runtimeProvider;
            if (TryGetRuntimeProvider(id, out runtimeProvider))
            {
                T runtimeAsset;
                return runtimeProvider.TryLoadUnityAsset(id, out runtimeAsset) ? runtimeAsset : null;
            }
            if (typeof(T) == typeof(Sprite)) return LoadSprite(id) as T;
            if (typeof(T) == typeof(Texture2D))
                return LoadTexture(id) as T;
            if (typeof(T) == typeof(AudioClip)) return LoadAudio(id) as T;
            return null;
        }

        public T[] LoadUnityAssets<T>(AssetId id) where T : UnityEngine.Object
        {
            id = _resolver.Resolve(id);
            AssetMetadata metadata;
            if (!_resolver.TryDescribe(id, out metadata)) return null;
            IRuntimeAssetProvider runtimeProvider;
            if (TryGetRuntimeProvider(id, out runtimeProvider))
            {
                T[] runtimeAssets;
                return runtimeProvider.TryLoadUnityAssets(id, out runtimeAssets) ? runtimeAssets : null;
            }
            T asset = LoadUnityAsset<T>(id);
            return asset == null ? null : new[] { asset };
        }

        public Sprite LoadSprite(AssetId id)
        {
            id = _resolver.Resolve(id);
            AssetMetadata metadata;
            if (!_resolver.TryDescribe(id, out metadata)) return null;
            IRuntimeAssetProvider runtimeProvider;
            if (TryGetRuntimeProvider(id, out runtimeProvider))
            {
                Sprite runtimeAsset;
                return runtimeProvider.TryLoadUnityAsset(id, out runtimeAsset) ? runtimeAsset : null;
            }

            Sprite cached;
            if (_sprites.TryGetValue(id, out cached) && cached != null) return cached;

            AssetBytes bytes;
            if (!_resolver.TryRead(id, out bytes)) return null;
            if (bytes.Metadata.Kind != AssetKind.Sprite)
                throw new InvalidDataException("Asset is not a sprite: " + id);

            SpriteAssetDescriptor descriptor;
            Texture2D texture;
            if (bytes.Metadata.Format == ".asset")
            {
                descriptor = SpriteAssetDescriptor.Parse(DecodeText(bytes, id), id.ToString(), standalone: true);
                AssetId textureId = descriptor.GetTextureId(id);
                AssetMetadata textureMetadata;
                if (!_resolver.TryDescribe(textureId, out textureMetadata) ||
                    textureMetadata.Kind != AssetKind.Texture || textureMetadata.Format != ".png")
                    throw new InvalidDataException("Sprite '" + id + "' requires a PNG texture: " + textureId);
                texture = LoadTextureData(textureId, descriptor);
                if (texture == null) throw new InvalidDataException("Sprite texture is missing: " + textureId);
            }
            else if (bytes.Metadata.Format == ".png")
            {
                descriptor = LoadSpriteDescriptor(id);
                texture = LoadTextureData(id, descriptor, bytes);
            }
            else throw new InvalidDataException("Unsupported sprite format: " + id);

            Rect rect = descriptor.HasRect ? descriptor.Rect : new Rect(0f, 0f, texture.width, texture.height);
            ValidateSpriteRect(rect, texture, id);
            ValidateBorder(descriptor.Border, rect, id);
            Sprite sprite = Sprite.Create(texture, rect, descriptor.Pivot, descriptor.PixelsPerUnit, 0,
                SpriteMeshType.FullRect, descriptor.Border);
            if (sprite == null) throw new InvalidDataException("Unity cannot create loose mod sprite: " + id);
            sprite.name = GetLastSegment(id.Path);
            _sprites[id] = sprite;
            return sprite;
        }

        public Texture2D LoadTexture(AssetId id)
        {
            id = _resolver.Resolve(id);
            AssetMetadata metadata;
            if (!_resolver.TryDescribe(id, out metadata)) return null;
            IRuntimeAssetProvider runtimeProvider;
            if (TryGetRuntimeProvider(id, out runtimeProvider))
            {
                Texture2D runtimeAsset;
                return runtimeProvider.TryLoadUnityAsset(id, out runtimeAsset) ? runtimeAsset : null;
            }
            // Preserve early mods and legacy callers that request a sprite's backing texture.
            if (metadata.Kind == AssetKind.Sprite)
            {
                Sprite sprite = LoadSprite(id);
                return sprite == null ? null : sprite.texture;
            }
            if (metadata.Kind != AssetKind.Texture || metadata.Format != ".png")
                throw new InvalidDataException("Asset is not a PNG texture: " + id);
            return LoadTextureData(id, SpriteAssetDescriptor.Parse(string.Empty, id.ToString()));
        }

        private Texture2D LoadTextureData(AssetId id, SpriteAssetDescriptor descriptor, AssetBytes bytes = null)
        {
            var key = (id, descriptor.Filter, descriptor.Wrap, descriptor.Mipmaps);
            Texture2D cached;
            if (_textures.TryGetValue(key, out cached) && cached != null) return cached;
            if (bytes == null && !_resolver.TryRead(id, out bytes)) return null;
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, descriptor.Mipmaps);
            try
            {
                if (!ImageConversion.LoadImage(texture, bytes.Data, false))
                    throw new InvalidDataException("Unity cannot decode loose mod PNG: " + id);
                texture.name = GetLastSegment(id.Path);
                texture.filterMode = descriptor.Filter;
                texture.wrapMode = descriptor.Wrap;
                _textures[key] = texture;
                return texture;
            }
            catch { Destroy(texture); throw; }
        }

        public string LoadModelText(AssetId id)
        {
            id = _resolver.Resolve(id);
            AssetMetadata metadata;
            if (!_resolver.TryDescribe(id, out metadata)) return null;
            IRuntimeAssetProvider runtimeProvider;
            if (TryGetRuntimeProvider(id, out runtimeProvider))
            {
                string text;
                return runtimeProvider.TryLoadModelText(id, out text) ? text : null;
            }
            AssetBytes bytes;
            if (!_resolver.TryRead(id, out bytes)) return null;
            if (bytes.Metadata.Kind != AssetKind.Model)
                throw new InvalidDataException("Asset is not model geometry: " + id);
            return DecodeModelText(bytes, id);
        }

        public string LoadText(AssetId id)
        {
            AssetBytes bytes;
            if (!_resolver.TryRead(id, out bytes)) return null;
            if (bytes.Metadata.Kind != AssetKind.Text && bytes.Metadata.Kind != AssetKind.Model)
                throw new InvalidDataException("Asset is not text: " + id);
            return bytes.Metadata.Kind == AssetKind.Model ? DecodeModelText(bytes, id) : DecodeText(bytes, id);
        }

        public byte[] LoadBinary(AssetId id)
        {
            id = _resolver.Resolve(id);
            AssetBytes bytes;
            if (!_resolver.TryRead(id, out bytes)) return null;
            if (bytes.Metadata.Kind != AssetKind.Binary)
                throw new InvalidDataException("Asset is not binary data: " + id);
            return bytes.Data;
        }

        public AudioClip LoadAudio(AssetId id)
        {
            id = _resolver.Resolve(id);
            AssetMetadata metadata;
            if (!_resolver.TryDescribe(id, out metadata)) return null;
            IRuntimeAssetProvider runtimeProvider;
            if (TryGetRuntimeProvider(id, out runtimeProvider))
            {
                AudioClip runtimeAsset;
                return runtimeProvider.TryLoadUnityAsset(id, out runtimeAsset) ? runtimeAsset : null;
            }
            AudioClip cached;
            if (_audio.TryGetValue(id, out cached) && cached != null) return cached;

            AssetBytes bytes;
            if (!_resolver.TryRead(id, out bytes)) return null;
            if (bytes.Metadata.Kind != AssetKind.Audio)
                throw new InvalidDataException("Asset is not audio: " + id);
            if (bytes.Metadata.Format != ".wav")
                throw new NotSupportedException("Loose mod audio currently supports PCM16 WAV only: " + id);

            AudioClip clip = WaveDecoder.Decode(bytes.Data, GetLastSegment(id.Path));
            _audio.Add(id, clip);
            return clip;
        }

        public void Dispose()
        {
            foreach (var member in _replacementMembers.Values) if (member != null) Destroy(member);
            _replacementMembers.Clear();
            foreach (Sprite sprite in _sprites.Values)
                if (sprite != null) Destroy(sprite);
            foreach (Texture2D texture in _textures.Values)
                if (texture != null) Destroy(texture);
            foreach (AudioClip clip in _audio.Values)
                if (clip != null) Destroy(clip);
            _sprites.Clear();
            _textures.Clear();
            _audio.Clear();
        }

        private SpriteAssetDescriptor LoadSpriteDescriptor(AssetId id)
        {
            AssetId descriptorId = AssetId.Parse(id.Namespace.Value + ":" + id.Path + ".sprite");
            AssetBytes bytes;
            if (!_resolver.TryRead(descriptorId, out bytes)) return SpriteAssetDescriptor.Parse(string.Empty, descriptorId.ToString());
            if (bytes.Metadata.Kind != AssetKind.Text || bytes.Metadata.Format != ".toml")
                throw new InvalidDataException("Sprite sidecar must be TOML text: " + descriptorId);
            return SpriteAssetDescriptor.Parse(DecodeText(bytes, descriptorId), descriptorId.ToString());
        }

        private static string DecodeText(AssetBytes bytes, AssetId id)
        {
            if (bytes.Data.Length > MaxTextBytes) throw new InvalidDataException("Text asset exceeds size limit: " + id);
            try { return StrictUtf8.GetString(bytes.Data); }
            catch (DecoderFallbackException exception)
            {
                throw new InvalidDataException("Text asset is not valid UTF-8: " + id, exception);
            }
        }

        private static string DecodeModelText(AssetBytes bytes, AssetId id)
        {
            if (bytes.Metadata.Format != ".modelz") return DecodeText(bytes, id);
            try
            {
                using (var compressed = new MemoryStream(bytes.Data, false))
                using (var gzip = new GZipStream(compressed, CompressionMode.Decompress))
                using (var decoded = new MemoryStream())
                {
                    byte[] buffer = new byte[8192];
                    int count;
                    while ((count = gzip.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        if (decoded.Length + count > MaxTextBytes)
                            throw new InvalidDataException("Model asset exceeds decoded size limit: " + id);
                        decoded.Write(buffer, 0, count);
                    }
                    return StrictUtf8.GetString(decoded.ToArray());
                }
            }
            catch (DecoderFallbackException error)
            {
                throw new InvalidDataException("Compressed model is not valid UTF-8: " + id, error);
            }
            catch (InvalidDataException) { throw; }
            catch (IOException error)
            {
                throw new InvalidDataException("Compressed model could not be decoded: " + id, error);
            }
        }

        private static void ValidateSpriteRect(Rect rect, Texture2D texture, AssetId id)
        {
            if (rect.width <= 0f || rect.height <= 0f || rect.x < 0f || rect.y < 0f ||
                rect.xMax > texture.width || rect.yMax > texture.height)
                throw new InvalidDataException("Sprite rect is outside PNG bounds: " + id + "; rect=" + rect +
                    "; texture=" + texture.width + "x" + texture.height);
        }

        private static void ValidateBorder(Vector4 border, Rect rect, AssetId id)
        {
            if (border.x < 0f || border.y < 0f || border.z < 0f || border.w < 0f ||
                border.x + border.z > rect.width || border.y + border.w > rect.height)
                throw new InvalidDataException("Sprite border is outside rect: " + id);
        }

        private static string GetLastSegment(string path)
        {
            int slash = path.LastIndexOf('/');
            return slash < 0 ? path : path.Substring(slash + 1);
        }

        private bool TryGetRuntimeProvider(AssetId id, out IRuntimeAssetProvider provider)
        {
            provider = null;
            IAssetProvider raw;
            if (!_resolver.TryGetProvider(id.Namespace, out raw)) return false;
            provider = raw as IRuntimeAssetProvider;
            return provider != null;
        }

        private static void Destroy(UnityEngine.Object value)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEngine.Object.DestroyImmediate(value);
            else UnityEngine.Object.Destroy(value);
#else
            UnityEngine.Object.Destroy(value);
#endif
        }
    }

    // Runtime-only bridge used by recovered host systems. Public scripting receives typed
    // handles instead of this loader or raw Unity objects.
    internal static class ModAssetBinding
    {
        internal static bool TryLoadAudio(string reference, out AudioClip clip)
        {
            clip = null;
            AssetId id;
            if (!TryParseExternal(reference, out id) || !ModRuntime.IsInitialized) return false;
            clip = ModRuntime.Host.TypedAssets.LoadAudio(id);
            return clip != null;
        }

        internal static bool TryLoadBinary(string reference, out byte[] data)
        {
            data = null;
            AssetId id;
            if (!TryParseExternal(reference, out id) || !ModRuntime.IsInitialized) return false;
            // The Moveset Lab's own clips first: they may be new or changed since startup.
            data = ModRuntime.TryReadLabBinary(id);
            if (data == null)
            {
                try { data = ModRuntime.Host.TypedAssets.LoadBinary(id); }
                catch (Exception) when (ModRuntime.IsLabMod(id.Namespace.Value)) { data = null; }
            }
            return data != null;
        }

        internal static bool TryLoadSprite(string reference, out Sprite sprite)
        {
            sprite = null;
            AssetId id;
            if (!TryParseExternal(reference, out id) || !ModRuntime.IsInitialized) return false;
            sprite = ModRuntime.Host.TypedAssets.LoadSprite(id);
            return sprite != null;
        }

        internal static bool IsQualified(string reference)
        {
            AssetId id;
            return TryParseExternal(reference, out id);
        }

        private static bool TryParseExternal(string reference, out AssetId id)
        {
            if (!AssetId.TryParse(reference, out id)) return false;
            return id.Namespace.Value != "core";
        }
    }

    internal static class ExternalLocationRuntime
    {
        internal sealed class Entry
        {
            internal XmlDocument Params;
            internal string MusicAsset;
        }

        private static readonly Dictionary<string, Entry> Entries =
            new Dictionary<string, Entry>(StringComparer.Ordinal);

        internal static void Set(string runtimeName, XmlDocument parameters, string musicAsset)
        {
            if (string.IsNullOrWhiteSpace(runtimeName))
                throw new ArgumentException("External location runtime name must not be empty.", "runtimeName");
            if (parameters == null || parameters["Root"] == null)
                throw new ArgumentException("External location parameters require a Root element.", "parameters");
            if (!string.IsNullOrEmpty(musicAsset) && !ModAssetBinding.IsQualified(musicAsset))
                throw new ArgumentException("External location music must be a mod-owned qualified asset.", "musicAsset");
            Entries[runtimeName] = new Entry
            {
                Params = (XmlDocument)parameters.CloneNode(true),
                MusicAsset = musicAsset ?? string.Empty
            };
        }

        internal static bool TryGet(string runtimeName, out Entry entry)
        {
            return Entries.TryGetValue(runtimeName ?? string.Empty, out entry);
        }

        internal static void Remove(string runtimeName)
        {
            if (!string.IsNullOrEmpty(runtimeName)) Entries.Remove(runtimeName);
        }

        internal static void Clear()
        {
            Entries.Clear();
        }
    }

    internal sealed class ExternalLocaleMetadata
    {
        internal string Name;
        internal string Locale;
        internal string Alias;
        internal string FileIcon;
        internal string FileIconSelected;
        internal string LoaderImage;
        internal string PreloaderImage;
        internal bool IsAsian;
        internal string ContentFont;
        internal string TitleFont;
        internal string ButtonFont;
        internal float FontSizeScale = 1f;
        internal float LineSpacing = 1f;
        internal float CustomLineSpacingScale = 1f;
    }

    internal static class ExternalLocaleRuntime
    {
        private static readonly List<ExternalLocaleMetadata> Entries = new List<ExternalLocaleMetadata>();
        private static readonly List<LocalizationManager.Language> Applied = new List<LocalizationManager.Language>();
        private static XmlNode BaseLanguage;

        internal static void ValidateBaseLanguages(XmlNode languages)
        {
            if (Entries.Count == 0) return;
            if (languages == null) throw new InvalidOperationException("Recovered locale metadata is unavailable.");
            string defaultName = languages.Attributes?["Default"]?.Value;
            XmlNode fallback = null;
            foreach (XmlNode language in languages.ChildNodes)
            {
                if (language.NodeType != XmlNodeType.Element) continue;
                string name = language.Attributes?["Name"]?.Value;
                string locale = language.Attributes?["Locale"]?.Value;
                if (name == defaultName) fallback = language;
                foreach (ExternalLocaleMetadata metadata in Entries)
                    ValidateNoCollision(metadata, name, locale);
            }
            if (fallback == null) throw new InvalidOperationException("Recovered default locale metadata is unavailable.");
            BaseLanguage = fallback.CloneNode(true);
        }

        private static void ValidateNoCollision(ExternalLocaleMetadata metadata, string name, string locale)
        {
            if (string.Equals(metadata.Name, name, StringComparison.Ordinal) ||
                string.Equals(metadata.Locale, locale, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("External locale collides with recovered locale metadata: " + metadata.Name);
        }

        internal static void Add(ExternalLocaleMetadata metadata)
        {
            if (metadata == null) throw new ArgumentNullException("metadata");
            if (string.IsNullOrWhiteSpace(metadata.Name) || string.IsNullOrWhiteSpace(metadata.Locale))
                throw new ArgumentException("External locale requires both Name and Locale.", "metadata");
            bool anyFont = !string.IsNullOrEmpty(metadata.ContentFont) || !string.IsNullOrEmpty(metadata.TitleFont) ||
                !string.IsNullOrEmpty(metadata.ButtonFont);
            if (anyFont && (string.IsNullOrEmpty(metadata.ContentFont) || string.IsNullOrEmpty(metadata.TitleFont) ||
                string.IsNullOrEmpty(metadata.ButtonFont)))
                throw new ArgumentException("External locale font metadata must provide content, title, and button fonts together.", "metadata");
            for (int i = 0; i < Entries.Count; i++)
            {
                if (string.Equals(Entries[i].Name, metadata.Name, StringComparison.Ordinal) ||
                    string.Equals(Entries[i].Locale, metadata.Locale, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("External locale metadata collides with an already registered locale: " + metadata.Name);
            }
            Entries.Add(metadata);
        }

        internal static void Apply()
        {
            if (LocalizationManager.Languages == null || Entries.Count == 0) return;
            // Validate the complete overlay before adding anything. A collision must not
            // escape halfway through ParseModule and cause all game data to be parsed again.
            foreach (ExternalLocaleMetadata metadata in Entries)
                foreach (LocalizationManager.Language language in LocalizationManager.Languages)
                    if (!Applied.Contains(language)) ValidateNoCollision(metadata, language.name, language.Locale);

            var pending = new List<LocalizationManager.Language>();
            for (int i = 0; i < Entries.Count; i++)
            {
                ExternalLocaleMetadata metadata = Entries[i];
                if (Applied.Exists(language => language.name == metadata.Name &&
                    LocalizationManager.Languages.Contains(language))) continue;

                XmlDocument document = new XmlDocument();
                XmlElement node = BaseLanguage == null ? document.CreateElement("Language") :
                    (XmlElement)document.ImportNode(BaseLanguage, true);
                document.AppendChild(node);
                Set(node, "Name", metadata.Name);
                Set(node, "Locale", metadata.Locale);
                Set(node, "Alias", metadata.Alias);
                Set(node, "FileIcon", metadata.FileIcon);
                Set(node, "FileIconSelected", metadata.FileIconSelected);
                Set(node, "LoaderImage", metadata.LoaderImage);
                Set(node, "PreloaderImage", metadata.PreloaderImage);
                Set(node, "IsAsian", metadata.IsAsian ? "1" : "0");
                if (!string.IsNullOrEmpty(metadata.ContentFont))
                {
                    if (node["Fonts"] != null) node.RemoveChild(node["Fonts"]);
                    XmlElement fonts = document.CreateElement("Fonts");
                    Set(fonts, "ContentFont", metadata.ContentFont);
                    Set(fonts, "TitleFont", metadata.TitleFont);
                    Set(fonts, "ButtonFont", metadata.ButtonFont);
                    Set(fonts, "FontSizeScale", metadata.FontSizeScale.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    Set(fonts, "LineSpacing", metadata.LineSpacing.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    Set(fonts, "CustomLineSpacingScale", metadata.CustomLineSpacingScale.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    node.AppendChild(fonts);
                }
                var language = new LocalizationManager.Language(node, LocalizationManager.Languages.Count + pending.Count);
                // Loose locales supply mod strings through TOML, not a replacement base
                // localization XML. Keep ordinary game text available through the base locale.
                LocalizationManager.Language fallback = LocalizationManager.FindLanguageByName(LocalizationManager.DefaultLanguageName);
                if (fallback != null) language.FilePath = fallback.FilePath;
                pending.Add(language);
            }
            LocalizationManager.Languages.AddRange(pending);
            Applied.AddRange(pending);
        }

        internal static void Clear()
        {
            if (LocalizationManager.Languages != null)
            {
                bool selected = Applied.Contains(LocalizationManager.CurrentLanguage);
                foreach (LocalizationManager.Language language in Applied)
                    LocalizationManager.Languages.Remove(language);
                for (int i = 0; i < LocalizationManager.Languages.Count; i++)
                    LocalizationManager.Languages[i].index = i;
                if (selected) LocalizationManager.CurrentLanguage = LocalizationManager.FindLanguageByName(LocalizationManager.DefaultLanguageName);
            }
            Applied.Clear();
            Entries.Clear();
            BaseLanguage = null;
        }

        private static void Set(XmlElement node, string name, string value)
        {
            node.SetAttribute(name, value ?? string.Empty);
        }
    }

    internal static class ExternalCombatContentRuntime
    {
        internal sealed class MovePerkLockRollback
        {
            internal sealed class Snapshot
            {
                internal readonly InfoAnimation Move;
                internal readonly ConditionAnimation[] Locks;
                internal Snapshot(InfoAnimation move, ConditionAnimation[] locks) { Move = move; Locks = locks; }
            }

            internal readonly List<Snapshot> Snapshots = new List<Snapshot>();
        }

        internal sealed class MoveItemLockRollback : IDisposable
        {
            internal sealed class Replacement
            {
                internal List<ConditionAnimation> Locks;
                internal int Index;
                internal ConditionAnimation Original;
                internal ConditionAnimation Applied;
            }
            internal readonly List<Replacement> Replacements = new List<Replacement>();
            public void Dispose()
            {
                for (int i=Replacements.Count-1; i>=0; i--)
                {
                    var entry=Replacements[i];
                    int index=entry.Locks.IndexOf(entry.Applied);
                    if (index>=0) entry.Locks[index]=entry.Original;
                }
                Replacements.Clear();
            }
        }

        internal static MoveItemLockRollback ApplyItemLockExtensions(IReadOnlyList<InfoAnimation> moves,
            IReadOnlyList<MoveItemLockExtension> extensions)
        {
            if (extensions == null || extensions.Count == 0) return null;
            var byName=new Dictionary<string,InfoAnimation>(StringComparer.Ordinal);
            foreach (var move in moves)
                if (move != null && !string.IsNullOrEmpty(move.Name))
                {
                    if (byName.ContainsKey(move.Name)) throw new InvalidOperationException("Ambiguous live move name: " + move.Name);
                    byName.Add(move.Name,move);
                }
            var rollback=new MoveItemLockRollback();
            var pending=new Dictionary<List<ConditionAnimation>,List<ConditionAnimation>>();
            var document=new XmlDocument { XmlResolver=null };
            foreach (var extension in extensions)
            {
                if (!byName.TryGetValue(extension.MoveName,out var move) || move.MoveData == null)
                    throw new InvalidOperationException("Item lock extension references unavailable move '"+extension.MoveName+"'.");
                var live=move.MoveData.Locks;
                if (!pending.TryGetValue(live,out var locks)) pending.Add(live,locks=new List<ConditionAnimation>(live));
                int match=-1;
                for(int i=0;i<live.Count;i++)
                {
                    // Select against original clauses, never another pending addition.
                    // This keeps independent mod registration order from changing targets.
                    var candidate=live[i];
                    bool found=MatchesItemLock(candidate,extension.ItemType,extension.SourceSubtype);
                    if (candidate is ConditionList group && !group.IsNot && group.get_Type()==ConditionList.OperatorType.OR)
                        foreach (var child in group.GetConditions()) found |= MatchesItemLock(child,extension.ItemType,extension.SourceSubtype);
                    if (!found) continue;
                    if (match>=0) throw new InvalidOperationException("Ambiguous item lock clause for '"+extension.MoveName+"'.");
                    match=i;
                }
                if (match<0) throw new InvalidOperationException("No matching positive item lock clause for '"+extension.MoveName+"'.");
                var original=locks[match];
                var children=original is ConditionList existing ? new List<ConditionAnimation>(existing.GetConditions()) : new List<ConditionAnimation>{original};
                foreach (var child in children)
                    if (MatchesItemLock(child,extension.ItemType,extension.Subtype))
                        throw new InvalidOperationException("Item lock subtype already exists for '"+extension.MoveName+"'.");
                var item=document.CreateElement("Item"); item.SetAttribute("Type",extension.ItemType); item.SetAttribute("SubType",extension.Subtype);
                var added=new ConditionItemInfo(item); added.Parse(item); children.Add(added);
                var op=document.CreateElement("Operator"); op.SetAttribute("Type","Or");
                var replacement=new ConditionList(op,children); replacement.Parse(op);
                locks[match]=replacement;
            }
            // Validate the entire batch before changing any live condition list.
            foreach (var pair in pending)
                for(int i=0;i<pair.Key.Count;i++)
                    if (!ReferenceEquals(pair.Key[i],pair.Value[i]))
                        rollback.Replacements.Add(new MoveItemLockRollback.Replacement {
                            Locks=pair.Key,Index=i,Original=pair.Key[i],Applied=pair.Value[i] });
            foreach(var entry in rollback.Replacements) entry.Locks[entry.Index]=entry.Applied;
            return rollback;
        }

        // Same batch/rollback contract as ApplyItemLockExtensions, for perk alternatives.
        internal static MoveItemLockRollback ApplyPerkLockExtensions(IReadOnlyList<InfoAnimation> moves,
            IReadOnlyList<MovePerkLockExtension> extensions)
        {
            if (extensions == null || extensions.Count == 0) return null;
            var byName=new Dictionary<string,InfoAnimation>(StringComparer.Ordinal);
            foreach (var move in moves)
                if (move != null && !string.IsNullOrEmpty(move.Name))
                {
                    if (byName.ContainsKey(move.Name)) throw new InvalidOperationException("Ambiguous live move name: " + move.Name);
                    byName.Add(move.Name,move);
                }
            var rollback=new MoveItemLockRollback();
            var pending=new Dictionary<List<ConditionAnimation>,List<ConditionAnimation>>();
            var document=new XmlDocument { XmlResolver=null };
            foreach (var extension in extensions)
            {
                if (!byName.TryGetValue(extension.MoveName,out var move) || move.MoveData == null)
                    throw new InvalidOperationException("Perk lock extension references unavailable move '"+extension.MoveName+"'.");
                var live=move.MoveData.Locks;
                if (!pending.TryGetValue(live,out var locks)) pending.Add(live,locks=new List<ConditionAnimation>(live));
                int match=-1;
                for(int i=0;i<live.Count;i++)
                {
                    var candidate=live[i];
                    bool found=MatchesPerkLock(candidate,extension.SourceRuntimePerkName);
                    if (candidate is ConditionList group && !group.IsNot && group.get_Type()==ConditionList.OperatorType.OR)
                        foreach (var child in group.GetConditions()) found |= MatchesPerkLock(child,extension.SourceRuntimePerkName);
                    if (!found) continue;
                    if (match>=0) throw new InvalidOperationException("Ambiguous perk lock clause for '"+extension.MoveName+"'.");
                    match=i;
                }
                if (match<0) throw new InvalidOperationException("No positive perk lock '"+extension.SourceRuntimePerkName+"' on '"+extension.MoveName+"'.");
                var original=locks[match];
                var children=original is ConditionList existing ? new List<ConditionAnimation>(existing.GetConditions()) : new List<ConditionAnimation>{original};
                foreach (var child in children)
                    if (MatchesPerkLock(child,extension.RuntimePerkName))
                        throw new InvalidOperationException("Perk lock alternative already exists for '"+extension.MoveName+"'.");
                var perk=document.CreateElement("Perk"); perk.SetAttribute("Name",extension.RuntimePerkName);
                var added=new ConditionPerk(perk); added.Parse(perk); children.Add(added);
                var op=document.CreateElement("Operator"); op.SetAttribute("Type","Or");
                var replacement=new ConditionList(op,children); replacement.Parse(op);
                locks[match]=replacement;
            }
            foreach (var pair in pending)
                for(int i=0;i<pair.Key.Count;i++)
                    if (!ReferenceEquals(pair.Key[i],pair.Value[i]))
                        rollback.Replacements.Add(new MoveItemLockRollback.Replacement {
                            Locks=pair.Key,Index=i,Original=pair.Key[i],Applied=pair.Value[i] });
            foreach(var entry in rollback.Replacements) entry.Locks[entry.Index]=entry.Applied;
            return rollback;
        }

        // Eclipse move forks: narrow a positive item lock group (subtype scope) or add a
        // negated item-name lock (item scope). Validated as a batch; disposing restores.
        internal sealed class LockEditRollback : IDisposable
        {
            internal readonly List<Action> Undo = new List<Action>();
            public void Dispose() { for (int i = Undo.Count - 1; i >= 0; i--) Undo[i](); Undo.Clear(); }
        }

        private static Dictionary<string,InfoAnimation> MovesByName(IReadOnlyList<InfoAnimation> moves)
        {
            var byName=new Dictionary<string,InfoAnimation>(StringComparer.Ordinal);
            foreach (var move in moves)
                if (move != null && !string.IsNullOrEmpty(move.Name))
                {
                    if (byName.ContainsKey(move.Name)) throw new InvalidOperationException("Ambiguous live move name: " + move.Name);
                    byName.Add(move.Name,move);
                }
            return byName;
        }

        internal static LockEditRollback ApplyItemLockEdits(IReadOnlyList<InfoAnimation> moves,
            IReadOnlyList<MoveItemLockRemoval> removals, IReadOnlyList<MoveItemExclusion> exclusions)
        {
            if ((removals == null || removals.Count == 0) && (exclusions == null || exclusions.Count == 0)) return null;
            var byName=MovesByName(moves);
            var rollback=new LockEditRollback();
            var apply=new List<Action>();
            var document=new XmlDocument { XmlResolver=null };
            foreach (var removal in removals ?? Array.Empty<MoveItemLockRemoval>())
            {
                if (!byName.TryGetValue(removal.MoveName,out var move) || move.MoveData == null)
                    throw new InvalidOperationException("Item lock removal references unavailable move '"+removal.MoveName+"'.");
                var locks=move.MoveData.Locks;
                int match=-1;
                for(int i=0;i<locks.Count;i++)
                {
                    var candidate=locks[i];
                    bool found=MatchesItemLock(candidate,removal.ItemType,removal.Subtype);
                    if (candidate is ConditionList group && !group.IsNot && group.get_Type()==ConditionList.OperatorType.OR)
                        foreach (var child in group.GetConditions()) found |= MatchesItemLock(child,removal.ItemType,removal.Subtype);
                    if (!found) continue;
                    if (match>=0) throw new InvalidOperationException("Ambiguous item lock clause for '"+removal.MoveName+"'.");
                    match=i;
                }
                if (match<0) throw new InvalidOperationException("'"+removal.MoveName+"' is not locked to "+removal.ItemType+" subtype "+removal.Subtype+".");
                var original=locks[match];
                var kept=new List<ConditionAnimation>();
                if (original is ConditionList existing)
                    foreach (var child in existing.GetConditions()) if (!MatchesItemLock(child,removal.ItemType,removal.Subtype)) kept.Add(child);
                if (kept.Count==0)
                    throw new InvalidOperationException("Removing "+removal.Subtype+" would unlock '"+removal.MoveName+"' for every fighter; use a fork of another subtype instead.");
                var op=document.CreateElement("Operator"); op.SetAttribute("Type","Or");
                var replacement=new ConditionList(op,kept); replacement.Parse(op);
                var target=locks; var selected=original;
                apply.Add(() => { int index=target.IndexOf(selected); if (index>=0) target[index]=replacement; });
                rollback.Undo.Add(() => { int index=target.IndexOf(replacement); if (index>=0) target[index]=selected; });
            }
            foreach (var exclusion in exclusions ?? Array.Empty<MoveItemExclusion>())
            {
                if (!byName.TryGetValue(exclusion.MoveName,out var move) || move.MoveData == null)
                    throw new InvalidOperationException("Item exclusion references unavailable move '"+exclusion.MoveName+"'.");
                var item=document.CreateElement("Item"); item.SetAttribute("Type",exclusion.ItemType); item.SetAttribute("Name",exclusion.RuntimeItemName); item.SetAttribute("Not","1");
                var added=new ConditionItemInfo(item); added.Parse(item);
                var locks=move.MoveData.Locks;
                apply.Add(() => locks.Add(added));
                rollback.Undo.Add(() => locks.Remove(added));
            }
            foreach (var action in apply) action();
            return rollback;
        }

        /// <summary>Builds the XML of each fork: a copy of its source with narrowed locks.</summary>
        internal static XmlDocument BuildForkDocument(IReadOnlyList<MoveForkDefinition> forks)
        {
            var document=new XmlDocument { XmlResolver=null };
            var root=document.CreateElement("Movesxml"); document.AppendChild(root);
            var movesNode=document.CreateElement("Moves"); root.AppendChild(movesNode);
            var built=new Dictionary<string,XmlNode>(StringComparer.Ordinal);
            foreach (var fork in forks)
            {
                XmlNode source;
                if (built.TryGetValue(fork.Source,out var earlier)) source=earlier;
                else if (!MovesParser.TryReadBaseMoveSource(fork.Source,out source))
                    throw new InvalidOperationException("Move fork source is not a native move or earlier fork: '"+fork.Source+"'.");
                var copy=(XmlElement)document.ImportNode(source,true);
                copy.SetAttribute("Name",fork.RuntimeName);
                // The source keeps its profile/trick entry; a copy must not list a second one.
                var profile=copy["Profile"]; if (profile!=null) copy.RemoveChild(profile);
                var locks=copy["Locks"];
                if (locks==null) { locks=document.CreateElement("Locks"); copy.AppendChild(locks); }
                if (fork.Subtype != null)
                {
                    XmlNode match=null;
                    // A new move limited to a subtype its source is not locked to (a shared
                    // move, say) simply gains that lock.
                    bool adds=fork.Adds;
                    foreach (XmlNode clause in locks.ChildNodes)
                    {
                        if (clause.NodeType!=XmlNodeType.Element) continue;
                        bool found=MatchesItemNode(clause,fork.ItemType,fork.Subtype);
                        if (clause.Name=="Operator" && clause.Attributes["Type"]?.Value=="Or" && clause.Attributes["Not"]?.Value!="1")
                            foreach (XmlNode child in clause.ChildNodes) found |= MatchesItemNode(child,fork.ItemType,fork.Subtype);
                        if (!found) continue;
                        if (match!=null) throw new InvalidOperationException("Ambiguous item lock clause for fork source '"+fork.Source+"'.");
                        match=clause;
                    }
                    if (match==null && !adds) throw new InvalidOperationException("Fork source '"+fork.Source+"' is not locked to "+fork.ItemType+" subtype "+fork.Subtype+".");
                    var narrowed=document.CreateElement("Item"); narrowed.SetAttribute("Type",fork.ItemType); narrowed.SetAttribute("SubType",fork.Subtype);
                    if (match!=null) locks.ReplaceChild(narrowed,match); else locks.AppendChild(narrowed);
                }
                else if (fork.RuntimeItemName != null)
                {
                    var only=document.CreateElement("Item"); only.SetAttribute("Type",fork.ItemType); only.SetAttribute("Name",fork.RuntimeItemName);
                    locks.AppendChild(only);
                }
                movesNode.AppendChild(copy);
                built[fork.RuntimeName]=copy;
            }
            return document;
        }

        private static bool MatchesItemNode(XmlNode node,string itemType,string subtype) =>
            node.NodeType==XmlNodeType.Element && node.Name=="Item" && node.Attributes["Type"]?.Value==itemType &&
            node.Attributes["SubType"]?.Value==subtype && node.Attributes["Name"]==null && node.Attributes["Not"]?.Value!="1";

        private static bool MatchesPerkLock(ConditionAnimation condition,string perkName)
            => condition is ConditionPerk perk && !perk.IsNot && string.Equals(perk.get_Name(),perkName,StringComparison.Ordinal);

        private static bool MatchesItemLock(ConditionAnimation condition,string itemType,string subtype)
            => condition is ConditionItemInfo item && !item.IsNot && item.get_Type()==itemType &&
                item.GetSubType()==subtype && string.IsNullOrEmpty(item.get_Name());

        internal static int ApplyMoves(XmlDocument document)
        {
            if (document == null || document["Movesxml"] == null)
                throw new InvalidOperationException("External moves overlay requires a Movesxml root.");
            return AnimationData.AddExternalMoves(document);
        }

        internal static MovePerkLockRollback ApplyMovePerkLocks(IReadOnlyList<MovePerkLockRemoval> removals)
        {
            if (removals == null || removals.Count == 0) return null;
            var wanted = new HashSet<string>(StringComparer.Ordinal);
            foreach (MovePerkLockRemoval removal in removals) wanted.Add(removal.MoveName);
            Dictionary<string, XmlNode> sourceByName = ReadRecoveredMoves(wanted);

            var liveByName = new Dictionary<string, InfoAnimation>(StringComparer.Ordinal);
            foreach (InfoAnimation move in AnimationData.Animations)
                if (move != null && !string.IsNullOrEmpty(move.Name)) liveByName[move.Name] = move;

            var removalsByMove = new Dictionary<InfoAnimation, HashSet<int>>();
            var rollback = new MovePerkLockRollback();
            foreach (MovePerkLockRemoval removal in removals)
            {
                if (!sourceByName.TryGetValue(removal.MoveName, out XmlNode sourceMove))
                    throw new InvalidOperationException("Move perk-lock removal references missing base move '" + removal.MoveName + "'.");
                if (!liveByName.TryGetValue(removal.MoveName, out InfoAnimation liveMove) || liveMove.MoveData == null)
                    throw new InvalidOperationException("Move perk-lock removal references unavailable live move '" + removal.MoveName + "'.");

                XmlNode locksNode = sourceMove["Locks"];
                if (locksNode == null)
                    throw new InvalidOperationException("Move '" + removal.MoveName + "' has no direct Locks block.");
                string perkName = removal.RuntimePerkName;
                int parsedIndex = 0, targetIndex = -1;
                foreach (XmlNode lockNode in locksNode.ChildNodes)
                {
                    if (lockNode.NodeType != XmlNodeType.Element) continue;
                    ConditionAnimation parsed = ConditionsParser.Create(lockNode);
                    if (parsed == null) continue;
                    if (lockNode.Name == "Perk" && string.Equals(lockNode.Attributes?["Name"]?.Value, perkName, StringComparison.Ordinal))
                    {
                        if (targetIndex >= 0)
                            throw new InvalidOperationException("Move '" + removal.MoveName + "' contains duplicate direct perk lock '" + perkName + "'.");
                        targetIndex = parsedIndex;
                    }
                    parsedIndex++;
                }
                if (targetIndex < 0)
                    throw new InvalidOperationException("Move '" + removal.MoveName + "' has no direct perk lock '" + perkName + "'.");

                List<ConditionAnimation> liveLocks = liveMove.MoveData.Locks;
                if (targetIndex >= liveLocks.Count || !(liveLocks[targetIndex] is ConditionPerk livePerk) ||
                    !string.Equals(livePerk.get_Name(), perkName, StringComparison.Ordinal))
                    throw new InvalidOperationException("Live move lock layout does not match recovered XML for '" + removal.MoveName + "'.");
                if (!removalsByMove.TryGetValue(liveMove, out HashSet<int> indices))
                {
                    indices = new HashSet<int>();
                    removalsByMove.Add(liveMove, indices);
                    rollback.Snapshots.Add(new MovePerkLockRollback.Snapshot(liveMove, liveLocks.ToArray()));
                }
                if (!indices.Add(targetIndex))
                    throw new InvalidOperationException("Duplicate live move perk-lock removal for '" + removal.MoveName + "' and '" + perkName + "'.");
            }

            foreach (MovePerkLockRollback.Snapshot snapshot in rollback.Snapshots)
            {
                HashSet<int> indices = removalsByMove[snapshot.Move];
                List<ConditionAnimation> liveLocks = snapshot.Move.MoveData.Locks;
                liveLocks.Clear();
                for (int i = 0; i < snapshot.Locks.Length; i++) if (!indices.Contains(i)) liveLocks.Add(snapshot.Locks[i]);
            }
            return rollback;
        }

        // The recovered /Movesxml/Moves/Move elements named in `wanted`, as XmlUtils.OpenXMLDocument
        // would read them (a later duplicate replaces an earlier one). Streams the file and
        // builds only those moves: the whole document is megabytes, a removal needs a few.
        private static Dictionary<string, XmlNode> ReadRecoveredMoves(HashSet<string> wanted)
        {
            // Animation startup already read this source. Reuse its pre-expansion
            // direct locks instead of loading/adapting/scanning moves.xml a second time.
            if (MovesParser.TryReadBaseMoveLockSources(wanted, out var sources)) return sources;
            string path = SF2Paths.GetAnimationsPath() + "/moves.xml";
            string text = path.StartsWith(SF2Paths.UserDataRoot) ? ResourceManager.GetFileOrDevText(path) : ResourceManager.GetText(path);
            var result = new Dictionary<string, XmlNode>(StringComparer.Ordinal);
            bool found = false;
            if (!string.IsNullOrEmpty(text))
            {
                // IgnoreWhitespace matches a loaded XmlDocument, which drops insignificant whitespace.
                var settings = new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true, DtdProcessing = DtdProcessing.Ignore, XmlResolver = null };
                var document = new XmlDocument();
                using (var reader = XmlReader.Create(new System.IO.StringReader(text), settings))
                {
                    int depth = -1;
                    bool inRoot = false, inMoves = false;
                    reader.MoveToContent();
                    while (!reader.EOF)
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            depth = reader.Depth;
                            if (depth == 0) inRoot = reader.Name == "Movesxml";
                            else if (depth == 1 && inRoot) { inMoves = reader.Name == "Moves"; found |= inMoves; }
                            else if (depth == 2 && inMoves && reader.Name == "Move")
                            {
                                string name = reader.GetAttribute("Name");
                                if (!string.IsNullOrEmpty(name) && wanted.Contains(name))
                                {
                                    result[name] = document.ReadNode(reader); // leaves the reader after the move
                                    continue;
                                }
                                reader.Skip();
                                continue;
                            }
                        }
                        else if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == 1)
                            inMoves = false;
                        reader.Read();
                    }
                }
            }
            if (!found) throw new InvalidOperationException("Recovered animations/moves.xml is unavailable.");
            return result;
        }

        internal static void RemoveMovePerkLocks(MovePerkLockRollback rollback)
        {
            if (rollback == null) return;
            for (int i = rollback.Snapshots.Count - 1; i >= 0; i--)
            {
                MovePerkLockRollback.Snapshot snapshot = rollback.Snapshots[i];
                if (snapshot.Move?.MoveData == null) continue;
                List<ConditionAnimation> locks = snapshot.Move.MoveData.Locks;
                locks.Clear();
                locks.AddRange(snapshot.Locks);
            }
            rollback.Snapshots.Clear();
        }

        internal static int ApplyTactics(XmlDocument overlay)
        {
            if (overlay == null)
                throw new ArgumentNullException("overlay");
            XmlNode overlayRoot = overlay["TacticsSettings"];
            if (overlayRoot == null)
                throw new InvalidOperationException("External tactics overlay requires a TacticsSettings root.");
            if (overlayRoot["ConditionalDecisions"] != null)
                throw new NotSupportedException("ConditionalDecisions have no recovered parser/evaluator and cannot be registered by mods.");
            XmlNode overlayTactics = overlayRoot["Tactics"];
            if (overlayTactics == null) return 0;

            XmlDocument combined = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), "tacticSettings.xml");
            if (combined == null || combined["TacticsSettings"] == null || combined["TacticsSettings"]["Tactics"] == null)
                throw new InvalidOperationException("Recovered tacticSettings.xml is unavailable.");
            XmlNode combinedTactics = combined["TacticsSettings"]["Tactics"];
            List<string> names = new List<string>();
            HashSet<string> pendingNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (XmlNode node in overlayTactics.ChildNodes)
            {
                if (node.Name != "Tactic") continue;
                string name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
                if (string.IsNullOrEmpty(name))
                    throw new InvalidOperationException("External tactic requires a Name.");
                if (!pendingNames.Add(name))
                    throw new InvalidOperationException("External tactics asset contains duplicate tactic '" + name + "'.");
                Tactic existing = AiData.GetTacticByName(name);
                if (existing != null && existing.get_Name() == name)
                    throw new InvalidOperationException("External tactic collides with existing tactic '" + name + "'.");
                names.Add(name);
                combinedTactics.AppendChild(combined.ImportNode(node, true));
            }

            TacticsCompiler.CompileTacticsSettings(combined);
            List<XmlNode> compiledNodes = new List<XmlNode>();
            for (int i = 0; i < names.Count; i++)
            {
                XmlNode node = FindTactic(combinedTactics, names[i]);
                if (node == null)
                    throw new InvalidOperationException("Compiled external tactic disappeared: " + names[i]);
                // Construct before mutating AiData so malformed tactic content cannot leave
                // a partially applied overlay.
                new Tactic(node);
                compiledNodes.Add(node);
            }
            for (int i = 0; i < compiledNodes.Count; i++) AiData.AddExternalTactic(compiledNodes[i]);
            return compiledNodes.Count;
        }

        internal static bool RemoveTactic(string runtimeName)
        {
            return AiData.RemoveExternalTactic(runtimeName);
        }

        private static XmlNode FindTactic(XmlNode tactics, string name)
        {
            foreach (XmlNode node in tactics.ChildNodes)
                if (node.Name == "Tactic" && node.Attributes["Name"].GetStringOrDefault(string.Empty) == name)
                    return node;
            return null;
        }

    }
}

namespace Eclipse.Modding
{
    // Validate the complete batch first. Animation loading can consume NodeInterval
    // between application and removal, so rollback handles both representations.
    internal static class MoveCombatPatchRuntime
    {
        private sealed class DisabledMoveCondition : ConditionAnimation
        {
            internal DisabledMoveCondition() : base(ConditionType.NONE) { }
            public override bool IsEqual(ModelConditions conditions) => false;
            public override bool IsEqual(Model model, InfoAnimation animation) => false;
        }

        internal sealed class Lifetime : IDisposable
        {
            internal readonly List<Action> Apply = new List<Action>();
            internal readonly List<Action> Undo = new List<Action>();
            private bool _disposed;
            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                for (int i = Undo.Count - 1; i >= 0; i--) Undo[i]();
            }
        }

        internal static Lifetime Apply(IReadOnlyList<InfoAnimation> moves, IReadOnlyList<MoveCombatPatch> patches,
            Func<ModMoveCondition, ConditionAnimation> parse, Action rebuildPriorityConflicts = null, bool dryRun = false)
        {
            var lifetime = new Lifetime();
            // Native priority-conflict tables are derived from each move's key
            // condition and Priority. Rebuild them after key/priority changes are
            // applied and again after they are undone (Undo runs in reverse).
            bool changesConflicts = false;
            foreach (var patch in patches)
                if (patch.Input != null || patch.Priority != null) changesConflicts = true;
            if (changesConflicts && rebuildPriorityConflicts != null) lifetime.Undo.Add(rebuildPriorityConflicts);
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var patch in patches)
            {
                if (!names.Add(patch.MoveName)) throw new InvalidOperationException("Duplicate runtime move patch: " + patch.MoveName);
                InfoAnimation move = null;
                foreach (var candidate in moves)
                    if (candidate != null && candidate.Name == patch.MoveName)
                    {
                        if (move != null) throw new InvalidOperationException("Ambiguous native move: " + patch.MoveName);
                        move = candidate;
                    }
                if (move == null || move.MoveData == null) throw new InvalidOperationException("Missing native move: " + patch.MoveName);
                var target = move;
                if (patch.Disable)
                {
                    var condition = new DisabledMoveCondition();
                    var conditions = target.SelectionConditions;
                    lifetime.Apply.Add(() => conditions.Add(condition));
                    lifetime.Undo.Add(() => conditions.Remove(condition));
                }
                foreach (var condition in patch.Conditions)
                {
                    var parsed = parse(condition);
                    if (parsed == null) throw new InvalidOperationException("Unsupported parsed patch condition: " + patch.MoveName);
                    var conditions = target.SelectionConditions;
                    lifetime.Apply.Add(() => conditions.Add(parsed));
                    lifetime.Undo.Add(() => conditions.Remove(parsed));
                }
                if (patch.IntervalEnd != null && patch.IntervalStart != null &&
                    patch.IntervalEnd.Name != patch.IntervalStart.Name)
                {
                    PrepareBounds(target, null, patch.IntervalEnd, lifetime);
                    PrepareBounds(target, patch.IntervalStart, null, lifetime);
                }
                else if (patch.IntervalEnd != null || patch.IntervalStart != null)
                    PrepareBounds(target, patch.IntervalStart, patch.IntervalEnd, lifetime);
                if (patch.Hit != null) PrepareHit(target, patch.Hit, lifetime);
                if (patch.SoundFrame != null) PrepareSound(target, patch.SoundFrame, lifetime);
                if (patch.Input != null) PrepareInput(target, patch.Input, parse, lifetime);
                if (patch.Priority != null) PreparePriority(target, patch.Priority, lifetime);
                if (patch.Animation != null) PrepareAnimation(target, patch.Animation, lifetime);
                if (patch.RemoveInterval != null) PrepareRemoveInterval(target, patch.RemoveInterval, lifetime);
                if (patch.AddInterval != null) PrepareAddInterval(target, patch.AddInterval, lifetime);
                PrepareExtras(target, moves, patch.Extras, lifetime);
            }
            // A dry run validates every guard and selector without touching the moves.
            if (dryRun) return lifetime;
            if (changesConflicts && rebuildPriorityConflicts != null) lifetime.Apply.Add(rebuildPriorityConflicts);
            try { foreach (var apply in lifetime.Apply) apply(); }
            catch { lifetime.Dispose(); throw; }
            return lifetime;
        }

        private static string Attribute(XmlNode node, string name) => node?.Attributes?[name]?.Value;
        private static int Integer(XmlNode node, string name, int fallback)
        {
            string raw = Attribute(node, name);
            if (raw == null) return fallback;
            if (!int.TryParse(raw, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int value))
                throw new InvalidOperationException("Native move field is not an integer: " + name);
            return value;
        }
        private static void PrepareBounds(InfoAnimation move, ModMoveFramePatch startPatch,
            ModMoveFramePatch endPatch, Lifetime lifetime)
        {
            string name = (startPatch ?? endPatch).Name;
            IntervalAnimation interval = null;
            foreach (var candidate in move.MoveData.Intervals)
                if ((candidate.NodeInterval != null ? Attribute(candidate.NodeInterval,"Name") : candidate.Name) == name)
                {
                    if (interval != null) throw new InvalidOperationException("Ambiguous interval: " + move.Name + "/" + name);
                    interval = candidate;
                }
            if (interval == null) throw new InvalidOperationException("Missing interval: " + move.Name + "/" + name);
            var target = interval; var originalNode = target.NodeInterval;
            int start = originalNode == null ? target.Start : Integer(originalNode,"Start",0);
            int end = originalNode == null ? target.EndFrame : Integer(originalNode,"End",-1);
            if ((startPatch != null && start != startPatch.Expected) ||
                (endPatch != null && end != endPatch.Expected) ||
                (startPatch?.Value ?? start) > (endPatch?.Value ?? end))
                throw new InvalidOperationException("Interval expected bounds mismatch: " + move.Name);
            XmlNode replacement = originalNode?.CloneNode(true);
            if (replacement != null)
            {
                if (startPatch != null) ((XmlElement)replacement).SetAttribute("Start",startPatch.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                if (endPatch != null) ((XmlElement)replacement).SetAttribute("End",endPatch.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            lifetime.Apply.Add(() =>
            {
                if (replacement != null) target.NodeInterval = replacement;
                else
                {
                    if (startPatch != null) target.Start = startPatch.Value;
                    if (endPatch != null) target.EndFrame = endPatch.Value;
                }
            });
            lifetime.Undo.Add(() =>
            {
                if (replacement != null && ReferenceEquals(target.NodeInterval,replacement)) target.NodeInterval = originalNode;
                else if (target.NodeInterval == null)
                {
                    if (startPatch != null && target.Start == startPatch.Value) target.Start = startPatch.Expected;
                    if (endPatch != null && target.EndFrame == endPatch.Value) target.EndFrame = endPatch.Expected;
                }
            });
        }
        private static void PrepareHit(InfoAnimation move, ModMoveHitPatch patch, Lifetime lifetime)
        {
            IntervalAttack attack = null;
            foreach (var candidate in move.MoveData.Intervals)
                if (candidate is IntervalAttack found)
                {
                    if (attack != null) throw new InvalidOperationException("Hit patch requires one attack interval: " + move.Name);
                    attack = found;
                }
            if (attack == null) throw new InvalidOperationException("Hit patch has no attack interval: " + move.Name);
            var target = attack; var originalNode = target.NodeInterval;
            XmlNode replacement = null;
            if (originalNode != null)
            {
                var hits = originalNode.SelectNodes("Hit");
                if (hits.Count != 1 || Attribute(hits[0],"Name") != patch.Expected ||
                    Attribute(hits[0],"Start") != null || Attribute(hits[0],"End") != null)
                    throw new InvalidOperationException("Hit patch requires one matching full-interval hit: " + move.Name);
                replacement = originalNode.CloneNode(true);
                ((XmlElement)replacement["Hit"]).SetAttribute("Name",patch.Value);
            }
            else if (target.HitReactions.Count != 1 || target.HitReactions[0].Name != patch.Expected ||
                target.HitReactions[0].Start != target.Start || target.HitReactions[0].EndFrame != target.EndFrame)
                throw new InvalidOperationException("Hit patch requires one matching full-interval reaction: " + move.Name);
            lifetime.Apply.Add(() => { if (replacement != null) target.NodeInterval = replacement; else target.HitReactions[0].Name = patch.Value; });
            lifetime.Undo.Add(() =>
            {
                if (replacement != null && ReferenceEquals(target.NodeInterval,replacement)) target.NodeInterval = originalNode;
                else if (target.NodeInterval == null && target.HitReactions.Count == 1 && target.HitReactions[0].Name == patch.Value)
                    target.HitReactions[0].Name = patch.Expected;
            });
        }
        private static void PrepareSound(InfoAnimation move, ModMoveFramePatch patch, Lifetime lifetime)
        {
            ActionSound sound = null;
            foreach (var candidate in move.ScheduledActions)
                if (candidate is ActionSound found && found.get_Name() == patch.Name)
                {
                    if (sound != null) throw new InvalidOperationException("Ambiguous sound action: " + move.Name + "/" + patch.Name);
                    sound = found;
                }
            if (sound == null || sound.ScheduledFrame != patch.Expected)
                throw new InvalidOperationException("Sound expected frame mismatch: " + move.Name + "/" + patch.Name);
            var target = sound;
            lifetime.Apply.Add(() => target.SetScheduledFrame(patch.Value));
            lifetime.Undo.Add(() => { if (target.ScheduledFrame == patch.Value) target.SetScheduledFrame(patch.Expected); });
        }
        /// <summary>
        /// The selection condition holding a move's key input: one top-level key chord, or one
        /// top-level "Or" whose members are all key chords (alternatives). -1 when the move has
        /// no key input. Throws when keys appear in any other shape (not editable as an input).
        /// </summary>
        internal static int FindInputSlot(IList<ConditionAnimation> conditions, out List<ConditionKeys> chords)
        {
            chords = new List<ConditionKeys>();
            int slot = -1;
            for (int i = 0; i < conditions.Count; i++)
            {
                var condition = conditions[i];
                List<ConditionKeys> found = null;
                if (condition is ConditionKeys keys && !keys.IsNot) found = new List<ConditionKeys> { keys };
                else if (condition is ConditionList list && !list.IsNot && list.get_Type() == ConditionList.OperatorType.OR &&
                    list.Conditions.Count != 0 && list.Conditions.TrueForAll(c => c is ConditionKeys member && !member.IsNot))
                    found = list.Conditions.ConvertAll(c => (ConditionKeys)c);
                else if (ContainsKeys(condition))
                    throw new InvalidOperationException("its keys are combined with other conditions");
                if (found == null) continue;
                if (slot >= 0) throw new InvalidOperationException("it has more than one key condition");
                slot = i;
                chords = found;
            }
            return slot;
        }

        private static bool ContainsKeys(ConditionAnimation condition) =>
            condition is ConditionKeys || condition is ConditionList list && list.Conditions.Exists(ContainsKeys);

        private static void PrepareInput(InfoAnimation move, ModMoveInputPatch patch,
            Func<ModMoveCondition, ConditionAnimation> parse, Lifetime lifetime)
        {
            ConditionKeys Chord(IReadOnlyList<ModMoveKey> keys) =>
                parse(new ModMoveCondition(ModMoveConditionKind.Keys, keys: keys.ToArray())) as ConditionKeys
                ?? throw new InvalidOperationException("Move input patch could not parse native keys: " + move.Name);
            var conditions = move.SelectionConditions;
            int index;
            List<ConditionKeys> current;
            try { index = FindInputSlot(conditions, out current); }
            catch (InvalidOperationException reason) { throw new InvalidOperationException("Move input of " + move.Name + " is not editable: " + reason.Message); }
            var expected = patch.Expected.Chords.Select(Chord).ToList();
            bool matches = expected.Count == current.Count;
            for (int i = 0; matches && i < expected.Count; i++) matches = current[i].HasSameKeyRequirementAs(expected[i]);
            if (!matches) throw new InvalidOperationException("Move input expected key mismatch: " + move.Name);
            ConditionAnimation replacement = patch.Value.IsNone ? null
                : patch.Value.Chords.Count == 1 ? Chord(patch.Value.Chords[0])
                : parse(new ModMoveCondition(ModMoveConditionKind.Any, children: patch.Value.Chords
                    .Select(chord => new ModMoveCondition(ModMoveConditionKind.Keys, keys: chord.ToArray())).ToArray()));
            if (replacement == null && !patch.Value.IsNone)
                throw new InvalidOperationException("Move input patch could not parse native keys: " + move.Name);
            var original = index >= 0 ? conditions[index] : null;
            lifetime.Apply.Add(() =>
            {
                if (index >= 0) { if (replacement == null) conditions.RemoveAt(index); else conditions[index] = replacement; }
                else if (replacement != null) conditions.Insert(0, replacement);
            });
            lifetime.Undo.Add(() =>
            {
                if (replacement != null)
                {
                    int at = conditions.IndexOf(replacement);
                    if (at < 0) return;
                    if (original != null) conditions[at] = original; else conditions.RemoveAt(at);
                }
                else if (original != null) conditions.Insert(Math.Min(index, conditions.Count), original);
            });
        }

        private static void PreparePriority(InfoAnimation move, ModMovePriorityPatch patch, Lifetime lifetime)
        {
            if (move.Priority != patch.Expected)
                throw new InvalidOperationException("Move priority expected value mismatch: " + move.Name);
            lifetime.Apply.Add(() => move.Priority = patch.Value);
            lifetime.Undo.Add(() => { if (move.Priority == patch.Value) move.Priority = patch.Expected; });
        }

        private static void PrepareAnimation(InfoAnimation move, ModMoveAnimationPatch patch, Lifetime lifetime)
        {
            if (move.FileName != patch.Expected)
                throw new InvalidOperationException("Move animation expected filename mismatch: " + move.Name);
            string original = move.FileName;
            int originalEndFrame = move.AnimationEndFrame;
            string replacement = patch.ClipName;
            lifetime.Apply.Add(() => move.ReplaceClip(replacement, 0));
            lifetime.Undo.Add(() =>
            {
                if (move.FileName == replacement) move.ReplaceClip(original, originalEndFrame);
            });
        }

        private static void PrepareAddInterval(InfoAnimation move, ModMoveIntervalAddition patch, Lifetime lifetime)
        {
            var intervals = move.MoveData.Intervals;
            if (intervals.Count == 0)
                throw new InvalidOperationException("add_interval requires a move with native intervals: " + move.Name);
            // Native intervals parse lazily on the move's first use. Match the
            // existing list so the added one is parsed with it, or parse it now.
            bool deferred = false;
            foreach (var candidate in intervals)
            {
                if (candidate.NodeInterval != null) deferred = true;
                string name = candidate.NodeInterval != null ? Attribute(candidate.NodeInterval, "Name") : candidate.Name;
                if (name == patch.Name) throw new InvalidOperationException("Move already has interval: " + move.Name + "/" + patch.Name);
            }
            var node = new XmlDocument().CreateElement("Interval");
            node.SetAttribute("Name", patch.Name);
            node.SetAttribute("Start", patch.Start.ToString(System.Globalization.CultureInfo.InvariantCulture));
            node.SetAttribute("End", patch.End.ToString(System.Globalization.CultureInfo.InvariantCulture));
            var added = new IntervalAnimation(default) { NodeInterval = node };
            if (!deferred) added.Init();
            lifetime.Apply.Add(() => intervals.Add(added));
            lifetime.Undo.Add(() => intervals.Remove(added));
        }

        private static void PrepareRemoveInterval(InfoAnimation move, ModMoveIntervalRemoval patch, Lifetime lifetime)
        {
            var intervals = move.MoveData.Intervals;
            IntervalAnimation target = null;
            int originalIndex = -1;
            string expectedType = "INTERVAL_" + patch.Type.Replace("SelfUninterrupt", "SELF_UNINTERRUPT").ToUpperInvariant();
            for (int i = 0; i < intervals.Count; i++)
            {
                var candidate = intervals[i];
                var node = candidate.NodeInterval;
                string name = node != null ? Attribute(node, "Name") : candidate.Name;
                string type = node != null ? Attribute(node, "Type") : null;
                if (node != null && string.IsNullOrEmpty(type)) type = name;
                if (name != patch.Name ||
                    (node != null ? type != patch.Type : candidate.Type.ToString() != expectedType)) continue;
                int start = node != null ? Integer(node, "Start", 0) : candidate.Start;
                int end = node != null ? Integer(node, "End", -1) : candidate.EndFrame;
                if (start != patch.Start || end != patch.End || target != null)
                    throw new InvalidOperationException("Move interval removal expected selector mismatch: " + move.Name + "/" + patch.Name);
                target = candidate;
                originalIndex = i;
            }
            if (target == null) throw new InvalidOperationException("Missing move interval for removal: " + move.Name + "/" + patch.Name);
            var matched = target;
            int index = originalIndex;
            lifetime.Apply.Add(() => intervals.Remove(matched));
            lifetime.Undo.Add(() =>
            {
                if (!intervals.Contains(matched)) intervals.Insert(Math.Min(index, intervals.Count), matched);
            });
        }

        // ---- List-based edits: playback rate, interval list and attacks by ID. ----

        private static void PrepareExtras(InfoAnimation move, IReadOnlyList<InfoAnimation> moves, ModMoveCombatExtras extras, Lifetime lifetime)
        {
            if (extras == null || extras.IsEmpty) return;
            if (extras.PlaybackRate != null) PreparePlaybackRate(move, extras.PlaybackRate, lifetime);
            var claimed = new HashSet<IntervalAnimation>();
            foreach (var edit in extras.Intervals) PrepareIntervalEdit(move, edit, claimed, lifetime);
            foreach (var edit in extras.Attacks) PrepareAttackEdit(move, edit, claimed, lifetime);
            foreach (var addition in extras.NewAttacks) PrepareAttackAddition(move, addition, lifetime);
            if (extras.ClipRange != null) PrepareClipRange(move, extras.ClipRange, lifetime);
            if (extras.Chains.Count != 0) PrepareChains(move, moves, extras.Chains, lifetime);
        }

        /// <summary>
        /// Plays keyframes First..Last of the move's clip. Runs after a clip swap (applied
        /// earlier in the same patch), so the range is checked against the clip actually loaded.
        /// </summary>
        private static void PrepareClipRange(InfoAnimation move, ModMoveGuard<ModMoveClipRange> patch, Lifetime lifetime)
        {
            // A move without an authored EndFrame learns its last keyframe when its clip loads.
            if (move.AnimationEndFrame == 0) move.LoadAnimationClip();
            if (move.FirstFrame != patch.Expected.First || move.AnimationEndFrame != patch.Expected.Last)
                throw new InvalidOperationException("Move clip_range expected " + patch.Expected + " but it is [" + move.FirstFrame + ", " + move.AnimationEndFrame + "]: " + move.Name);
            if (move.HasPhysics || move.GetIsLooped())
                throw new InvalidOperationException("clip_range is unavailable for physics or looped moves: " + move.Name);
            var value = patch.Value;
            lifetime.Apply.Add(() =>
            {
                var frames = move.GetAnimationFrames();
                if (frames != null && value.Last >= frames.Length)
                    throw new InvalidOperationException("clip_range " + value + " ends after the clip's last keyframe " + (frames.Length - 1) + ": " + move.Name);
                move.FirstFrame = value.First;
                move.AnimationEndFrame = value.Last;
            });
            lifetime.Undo.Add(() =>
            {
                if (move.FirstFrame == value.First && move.AnimationEndFrame == value.Last)
                {
                    move.FirstFrame = patch.Expected.First;
                    move.AnimationEndFrame = patch.Expected.Last;
                }
            });
        }

        /// <summary>
        /// A combo link. The source move gets a window interval; the follow-up's checks of the
        /// current move and interval (its "cannot interrupt" gates) also pass inside that window.
        /// Its input, locks, round and distance conditions are unchanged.
        /// </summary>
        private static void PrepareChains(InfoAnimation move, IReadOnlyList<InfoAnimation> moves, IReadOnlyList<ModMoveChain> chains, Lifetime lifetime)
        {
            string window = ModMoveChain.WindowName(move.Name);
            foreach (var chain in chains)
            {
                InfoAnimation source = null;
                foreach (var candidate in moves)
                    if (candidate != null && candidate.Name == chain.From)
                    {
                        if (source != null) throw new InvalidOperationException("Ambiguous chain source: " + chain.From);
                        source = candidate;
                    }
                if (source == null || source.MoveData == null) throw new InvalidOperationException("Chain source move does not exist: " + chain.From + " (for " + move.Name + ")");
                var intervals = source.MoveData.Intervals;
                bool deferred = false;
                foreach (var candidate in intervals)
                {
                    if (candidate.NodeInterval != null) deferred = true;
                    if (IntervalName(candidate) == window) throw new InvalidOperationException("Move " + chain.From + " already has a chain window for " + move.Name);
                }
                var node = new XmlDocument().CreateElement("Interval");
                node.SetAttribute("Name", window);
                node.SetAttribute("Start", chain.Start.ToString(System.Globalization.CultureInfo.InvariantCulture));
                node.SetAttribute("End", chain.End.ToString(System.Globalization.CultureInfo.InvariantCulture));
                var added = new IntervalAnimation(default) { NodeInterval = node };
                if (!deferred) added.Init();
                lifetime.Apply.Add(() => intervals.Add(added));
                lifetime.Undo.Add(() => intervals.Remove(added));
            }
            var conditions = move.SelectionConditions;
            var gates = new List<ConditionAnimation>();
            foreach (var condition in conditions) if (ChainCondition.IsGate(condition)) gates.Add(condition);
            if (gates.Count == 0) return;
            var wrapped = new List<ChainCondition>();
            lifetime.Apply.Add(() =>
            {
                wrapped.Clear();
                foreach (var gate in gates)
                {
                    int index = conditions.IndexOf(gate);
                    if (index < 0) continue;
                    var wrapper = new ChainCondition(gate, window);
                    conditions[index] = wrapper;
                    wrapped.Add(wrapper);
                }
            });
            lifetime.Undo.Add(() =>
            {
                foreach (var wrapper in wrapped)
                {
                    int index = conditions.IndexOf(wrapper);
                    if (index >= 0) conditions[index] = wrapper.Inner;
                }
                wrapped.Clear();
            });
        }

        /// <summary>A follow-up's current-move or current-interval gate, also true inside its chain window.</summary>
        private sealed class ChainCondition : ConditionAnimation
        {
            internal ConditionAnimation Inner { get; }
            private readonly string _window;

            internal ChainCondition(ConditionAnimation inner, string window) : base(ConditionType.NONE)
            {
                Inner = inner;
                _window = window;
                SetTargetModelType(ModelType.ModelTargetType.MODEL_THIS);
            }

            /// <summary>Checks of this fighter's own current move or interval, alone or combined with Or/And.</summary>
            internal static bool IsGate(ConditionAnimation condition)
            {
                if (condition == null || condition.GetTargetModelType() != ModelType.ModelTargetType.MODEL_THIS && condition.Type != ConditionType.LIST) return false;
                if (condition.Type == ConditionType.CURRENT_INTERVAL || condition.Type == ConditionType.CURRENT_ANIMATION) return true;
                if (condition.Type != ConditionType.LIST || !(condition is ConditionList list)) return false;
                var nested = list.GetConditions();
                if (nested == null || nested.Count == 0) return false;
                foreach (var item in nested) if (!IsGate(item)) return false;
                return true;
            }

            private bool InWindow(ModelConditions conditions)
            {
                var intervals = conditions?.Intervals;
                if (intervals == null) return false;
                foreach (var interval in intervals) if (interval != null && interval.Name == _window) return true;
                return false;
            }

            public override bool IsEqual(ModelConditions conditions) => InWindow(conditions) || Inner.IsEqual(conditions);

            public override bool IsEqual(Model model, InfoAnimation animationInfo)
            {
                if (InWindow(model.GetConditions())) return true;
                if (Inner is ConditionList list) return list.EvaluateWithModel(model.GetConditions(), model);
                return Inner.IsEqual(model, animationInfo);
            }
        }

        /// <summary>
        /// Adds an attack interval, built from the same XML a registered move's attack uses
        /// (LegacyContentAdapter.BuildMoveNode), so it parses and initializes like a native one.
        /// </summary>
        private static void PrepareAttackAddition(InfoAnimation move, ModMoveAttackAddition addition, Lifetime lifetime)
        {
            var intervals = move.MoveData.Intervals;
            bool deferred = false;
            foreach (var candidate in intervals)
            {
                if (candidate.NodeInterval != null) deferred = true;
                if (candidate is IntervalAttack existing &&
                    (existing.NodeInterval != null ? Integer(existing.NodeInterval, "ID", -1) : existing.GetAnimationId()) == addition.Id)
                    throw new InvalidOperationException("Move already has an attack with id " + addition.Id + ": " + move.Name);
            }
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            var document = new XmlDocument();
            var node = document.CreateElement("Interval");
            node.SetAttribute("Type", "Attack");
            node.SetAttribute("ID", addition.Id.ToString(culture));
            node.SetAttribute("Start", addition.Start.ToString(culture));
            node.SetAttribute("End", addition.End.ToString(culture));
            var parts = document.CreateElement("AttackingParts");
            foreach (string edge in addition.Edges) { var part = document.CreateElement("Edge"); part.SetAttribute("Name", edge); parts.AppendChild(part); }
            node.AppendChild(parts);
            var damage = document.CreateElement("Damage");
            damage.SetAttribute("Value", addition.Damage.ToString("R", culture));
            foreach (string type in ModMoveCombatTermOrder.Ordered(addition.Terms.Keys))
            {
                var term = document.CreateElement("Damage");
                term.SetAttribute("Type", type);
                if (addition.Terms[type] != 0) term.SetAttribute("Shift", addition.Terms[type].ToString("R", culture));
                damage.AppendChild(term);
            }
            node.AppendChild(damage);
            var impulse = document.CreateElement("Impulse");
            impulse.SetAttribute("X", addition.Impulse[0].ToString("R", culture));
            impulse.SetAttribute("Y", addition.Impulse[1].ToString("R", culture));
            impulse.SetAttribute("Z", addition.Impulse[2].ToString("R", culture));
            node.AppendChild(impulse);
            var hit = document.CreateElement("Hit"); hit.SetAttribute("Name", addition.Hit); node.AppendChild(hit);
            var added = new IntervalAttack();
            added.Parse(node);
            added.FinishFrame = move.AnimationEndFrame;
            if (!deferred) added.Init();
            lifetime.Apply.Add(() => intervals.Add(added));
            lifetime.Undo.Add(() => intervals.Remove(added));
        }

        private static void PreparePlaybackRate(InfoAnimation move, ModMoveGuard<int> patch, Lifetime lifetime)
        {
            if (move.PlaybackRatePermille != patch.Expected)
                throw new InvalidOperationException("Move playback_rate expected value mismatch: " + move.Name);
            if (move.HasPhysics || move.GetIsLooped())
                throw new InvalidOperationException("playback_rate is unavailable for physics or looped moves: " + move.Name);
            if (!Eclipse.Runtime.PlaybackTiming.IsPlayable(move.MidFrames, patch.Value))
                throw new InvalidOperationException("playback_rate " + patch.Value + " permille would skip keyframes of " + move.Name +
                    " (MidFrames " + move.MidFrames + " allows at most " + (move.MidFrames + 1) * 1000 + ").");
            lifetime.Apply.Add(() => move.PlaybackRatePermille = patch.Value);
            lifetime.Undo.Add(() => { if (move.PlaybackRatePermille == patch.Value) move.PlaybackRatePermille = patch.Expected; });
        }

        private static string IntervalName(IntervalAnimation interval) =>
            interval.NodeInterval != null ? Attribute(interval.NodeInterval, "Name") ?? string.Empty : interval.Name ?? string.Empty;
        private static string IntervalType(IntervalAnimation interval) =>
            interval.NodeInterval != null ? Attribute(interval.NodeInterval, "Type") ?? string.Empty : interval.AuthoredType ?? string.Empty;
        private static int IntervalStart(IntervalAnimation interval) =>
            interval.NodeInterval != null ? Integer(interval.NodeInterval, "Start", 0) : interval.Start;
        private static int? IntervalEnd(IntervalAnimation interval)
        {
            if (interval.NodeInterval != null)
                return Attribute(interval.NodeInterval, "End") == null ? (int?)null : Integer(interval.NodeInterval, "End", -1);
            return interval.HasAuthoredEnd ? interval.EndFrame : (int?)null;
        }

        private static IntervalAnimation SelectInterval(InfoAnimation move, ModMoveIntervalSelector select, HashSet<IntervalAnimation> claimed)
        {
            IntervalAnimation found = null;
            foreach (var candidate in move.MoveData.Intervals)
            {
                if (candidate is IntervalAttack || IntervalType(candidate) != select.Type || IntervalName(candidate) != select.Name ||
                    IntervalStart(candidate) != select.Start || IntervalEnd(candidate) != select.End) continue;
                if (found != null) throw new InvalidOperationException("Ambiguous interval selector: " + move.Name + "/" + select);
                found = candidate;
            }
            if (found == null) throw new InvalidOperationException("Interval selector matches nothing: " + move.Name + "/" + select);
            if (!claimed.Add(found)) throw new InvalidOperationException("Interval edited twice: " + move.Name + "/" + select);
            return found;
        }

        private static void PrepareIntervalEdit(InfoAnimation move, ModMoveIntervalEdit edit, HashSet<IntervalAnimation> claimed, Lifetime lifetime)
        {
            var intervals = move.MoveData.Intervals;
            if (edit.Kind == ModMoveIntervalEditKind.Add)
            {
                if (intervals.Count == 0) throw new InvalidOperationException("intervals.add requires a move with native intervals: " + move.Name);
                bool deferred = false;
                foreach (var candidate in intervals)
                {
                    if (candidate.NodeInterval != null) deferred = true;
                    if (edit.AddName.Length != 0 && IntervalName(candidate) == edit.AddName)
                        throw new InvalidOperationException("Move already has interval: " + move.Name + "/" + edit.AddName);
                }
                var node = new XmlDocument().CreateElement("Interval");
                if (edit.AddType.Length != 0) node.SetAttribute("Type", edit.AddType);
                if (edit.AddName.Length != 0) node.SetAttribute("Name", edit.AddName);
                node.SetAttribute("Start", edit.Start.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                if (edit.End.HasValue) node.SetAttribute("End", edit.End.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                var added = new IntervalAnimation(IntervalAnimation.ParseIntervalType(edit.AddType));
                added.Parse(node);
                added.FinishFrame = move.AnimationEndFrame;
                if (!deferred) added.Init();
                lifetime.Apply.Add(() => intervals.Add(added));
                lifetime.Undo.Add(() => intervals.Remove(added));
                return;
            }
            var target = SelectInterval(move, edit.Select, claimed);
            if (edit.Kind == ModMoveIntervalEditKind.Remove)
            {
                int index = intervals.IndexOf(target);
                lifetime.Apply.Add(() => intervals.Remove(target));
                lifetime.Undo.Add(() => { if (!intervals.Contains(target)) intervals.Insert(Math.Min(index, intervals.Count), target); });
                return;
            }
            int start = edit.Start ?? edit.Select.Start;
            if (target.NodeInterval != null)
            {
                var original = target.NodeInterval;
                var replacement = (XmlElement)original.CloneNode(true);
                replacement.SetAttribute("Start", start.ToString(System.Globalization.CultureInfo.InvariantCulture));
                if (edit.End.HasValue) replacement.SetAttribute("End", edit.End.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                int? authoredEnd = edit.Select.End;
                lifetime.Apply.Add(() => target.NodeInterval = replacement);
                lifetime.Undo.Add(() =>
                {
                    if (ReferenceEquals(target.NodeInterval, replacement)) { target.NodeInterval = original; return; }
                    // The move parsed the patched node meanwhile: restore its parsed bounds.
                    if (target.NodeInterval != null || target.Start != start || (edit.End.HasValue && target.EndFrame != edit.End.Value)) return;
                    target.Start = edit.Select.Start;
                    if (edit.End.HasValue)
                    {
                        if (authoredEnd.HasValue) target.EndFrame = authoredEnd.Value;
                        else { target.HasAuthoredEnd = false; target.EndFrame = move.AnimationEndFrame + 2; }
                    }
                });
                return;
            }
            int originalStart = target.Start, originalEnd = target.EndFrame; bool originalAuthored = target.HasAuthoredEnd;
            int end = edit.End ?? originalEnd;
            if (start > end) throw new InvalidOperationException("Interval edit would start after it ends: " + move.Name + "/" + edit.Select);
            lifetime.Apply.Add(() => { target.Start = start; target.EndFrame = end; if (edit.End.HasValue) target.HasAuthoredEnd = true; });
            lifetime.Undo.Add(() =>
            {
                if (target.NodeInterval == null && target.Start == start && target.EndFrame == end)
                { target.Start = originalStart; target.EndFrame = originalEnd; target.HasAuthoredEnd = originalAuthored; }
            });
        }

        private static float ParseFloat(string raw) =>
            raw == null ? 0f : float.Parse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
        private static string Number(double value) => ((float)value).ToString("R", System.Globalization.CultureInfo.InvariantCulture);

        private static bool SameTerms(IEnumerable<KeyValuePair<string, float>> actual, IReadOnlyDictionary<string, double> expected)
        {
            int count = 0;
            foreach (var pair in actual)
            {
                count++;
                if (!expected.TryGetValue(pair.Key, out double shift) || (float)shift != pair.Value) return false;
            }
            return count == expected.Count;
        }

        private static void PrepareAttackEdit(InfoAnimation move, ModMoveAttackEdit edit, HashSet<IntervalAnimation> claimed, Lifetime lifetime)
        {
            IntervalAttack attack = null;
            foreach (var candidate in move.MoveData.Intervals)
                if (candidate is IntervalAttack found && (found.NodeInterval != null ? Integer(found.NodeInterval, "ID", -1) : found.GetAnimationId()) == edit.Id)
                {
                    if (attack != null) throw new InvalidOperationException("Ambiguous attack id: " + move.Name + "/" + edit.Id);
                    attack = found;
                }
            if (attack == null) throw new InvalidOperationException("Move has no attack with id " + edit.Id + ": " + move.Name);
            if (!claimed.Add(attack)) throw new InvalidOperationException("Attack edited twice: " + move.Name + "/" + edit.Id);
            string where = move.Name + "/attack " + edit.Id;
            if (attack.NodeInterval != null) { PrepareDeferredAttack(attack, edit, where, lifetime); return; }

            int start = attack.Start, end = attack.EndFrame;
            if ((edit.Start != null && start != edit.Start.Expected) || (edit.End != null && end != edit.End.Expected))
                throw new InvalidOperationException("Attack frame expected value mismatch: " + where);
            int newStart = edit.Start?.Value ?? start, newEnd = edit.End?.Value ?? end;
            if (newStart > newEnd) throw new InvalidOperationException("Attack edit would start after it ends: " + where);
            var reactions = attack.HitReactions;
            if (edit.ChangesBounds)
                foreach (var reaction in reactions)
                    if (reaction.Start != start || reaction.EndFrameValue != end)
                        throw new InvalidOperationException("Attack bounds edits require full-interval hit reactions: " + where);
            if (edit.Hit != null && (reactions.Count != 1 || reactions[0].Name != edit.Hit.Expected || reactions[0].Start != start || reactions[0].EndFrameValue != end))
                throw new InvalidOperationException("Attack hit edit requires one matching full-interval reaction: " + where);
            if (edit.Damage != null && attack.GetDamage() != (float)edit.Damage.Expected)
                throw new InvalidOperationException("Attack damage expected value mismatch: " + where);
            var terms = attack.GetDamageAttributes();
            if (edit.DamageTerms != null && !SameTerms(terms.Select(t => new KeyValuePair<string, float>(t.First, t.Second)), edit.DamageTerms.Expected))
                throw new InvalidOperationException("Attack damage_terms expected value mismatch: " + where);
            var parts = attack.GetAttackingParts();
            if (edit.Edges != null && !parts.SequenceEqual(edit.Edges.Expected))
                throw new InvalidOperationException("Attack edges expected value mismatch: " + where);
            var impulse = attack.GetImpulse();
            if (edit.Impulse != null && (impulse.GetX() != (float)edit.Impulse.Expected[0] || impulse.GetY() != (float)edit.Impulse.Expected[1] || impulse.GetZ() != (float)edit.Impulse.Expected[2]))
                throw new InvalidOperationException("Attack impulse expected value mismatch: " + where);

            float damage = attack.GetDamage();
            var originalTerms = terms.Select(t => new global::Pair<string, float>(t.First, t.Second)).ToList();
            var originalParts = parts.ToList();
            float ix = impulse.GetX(), iy = impulse.GetY(), iz = impulse.GetZ();
            string hitName = reactions.Count == 1 ? reactions[0].Name : null;
            lifetime.Apply.Add(() =>
            {
                attack.Start = newStart; attack.EndFrame = newEnd;
                foreach (var reaction in attack.HitReactions)
                    if (edit.ChangesBounds) { reaction.Start = newStart; reaction.EndFrameValue = newEnd; }
                if (edit.Hit != null) attack.HitReactions[0].Name = edit.Hit.Value;
                if (edit.Damage != null) attack.EclipseSetDamage((float)edit.Damage.Value);
                if (edit.DamageTerms != null)
                {
                    var list = attack.GetDamageAttributes(); list.Clear();
                    foreach (string type in ModMoveCombatTermOrder.Ordered(edit.DamageTerms.Value.Keys))
                        list.Add(new global::Pair<string, float>(type, (float)edit.DamageTerms.Value[type]));
                }
                if (edit.Edges != null) attack.EclipseSetAttackingParts(edit.Edges.Value);
                if (edit.Impulse != null) { var v = attack.GetImpulse(); v.SetX((float)edit.Impulse.Value[0]); v.SetY((float)edit.Impulse.Value[1]); v.SetZ((float)edit.Impulse.Value[2]); }
            });
            lifetime.Undo.Add(() =>
            {
                if (attack.NodeInterval != null) return;
                attack.Start = start; attack.EndFrame = end;
                foreach (var reaction in attack.HitReactions)
                    if (edit.ChangesBounds) { reaction.Start = start; reaction.EndFrameValue = end; }
                if (edit.Hit != null && attack.HitReactions.Count == 1) attack.HitReactions[0].Name = hitName;
                if (edit.Damage != null) attack.EclipseSetDamage(damage);
                if (edit.DamageTerms != null) { var list = attack.GetDamageAttributes(); list.Clear(); list.AddRange(originalTerms); }
                if (edit.Edges != null) attack.EclipseSetAttackingParts(originalParts);
                if (edit.Impulse != null) { var v = attack.GetImpulse(); v.SetX(ix); v.SetY(iy); v.SetZ(iz); }
            });
        }

        private static void PrepareDeferredAttack(IntervalAttack attack, ModMoveAttackEdit edit, string where, Lifetime lifetime)
        {
            var original = attack.NodeInterval;
            int start = Integer(original, "Start", 0);
            string rawEnd = Attribute(original, "End");
            if (edit.Start != null && start != edit.Start.Expected)
                throw new InvalidOperationException("Attack start expected value mismatch: " + where);
            if (edit.End != null && (rawEnd == null || Integer(original, "End", -1) != edit.End.Expected))
                throw new InvalidOperationException("Attack end expected value mismatch: " + where);
            var hits = original.SelectNodes("Hit");
            if (edit.ChangesBounds)
                foreach (XmlNode hit in hits)
                    if (Attribute(hit, "Start") != null || Attribute(hit, "End") != null)
                        throw new InvalidOperationException("Attack bounds edits require full-interval hit reactions: " + where);
            if (edit.Hit != null && (hits.Count != 1 || Attribute(hits[0], "Name") != edit.Hit.Expected ||
                Attribute(hits[0], "Start") != null || Attribute(hits[0], "End") != null))
                throw new InvalidOperationException("Attack hit edit requires one matching full-interval reaction: " + where);
            var damage = original["Damage"];
            if ((edit.Damage != null || edit.DamageTerms != null) && damage == null)
                throw new InvalidOperationException("Attack has no Damage node: " + where);
            if (edit.Damage != null && ParseFloat(Attribute(damage, "Value")) != (float)edit.Damage.Expected)
                throw new InvalidOperationException("Attack damage expected value mismatch: " + where);
            if (edit.DamageTerms != null)
            {
                var actual = new List<KeyValuePair<string, float>>();
                foreach (XmlNode child in damage.ChildNodes)
                    if (child.Name == "Damage") actual.Add(new KeyValuePair<string, float>(Attribute(child, "Type") ?? string.Empty, ParseFloat(Attribute(child, "Shift"))));
                if (!SameTerms(actual, edit.DamageTerms.Expected))
                    throw new InvalidOperationException("Attack damage_terms expected value mismatch: " + where);
            }
            if (edit.Edges != null)
            {
                var parts = new List<string>();
                var partsNode = original["AttackingParts"];
                if (partsNode != null) foreach (XmlNode edge in partsNode.ChildNodes) parts.Add(Attribute(edge, "Name") ?? string.Empty);
                if (!parts.SequenceEqual(edit.Edges.Expected))
                    throw new InvalidOperationException("Attack edges expected value mismatch: " + where);
            }
            if (edit.Impulse != null)
            {
                var impulse = original["Impulse"];
                float[] actual = { ParseFloat(Attribute(impulse, "X")), ParseFloat(Attribute(impulse, "Y")), ParseFloat(Attribute(impulse, "Z")) };
                for (int i = 0; i < 3; i++)
                    if (actual[i] != (float)edit.Impulse.Expected[i]) throw new InvalidOperationException("Attack impulse expected value mismatch: " + where);
            }

            var replacement = (XmlElement)original.CloneNode(true);
            var document = replacement.OwnerDocument;
            if (edit.Start != null) replacement.SetAttribute("Start", edit.Start.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            if (edit.End != null) replacement.SetAttribute("End", edit.End.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            if (edit.Start != null && edit.End == null && rawEnd != null && edit.Start.Value > Integer(original, "End", -1))
                throw new InvalidOperationException("Attack edit would start after it ends: " + where);
            if (edit.Hit != null) ((XmlElement)replacement.SelectSingleNode("Hit")).SetAttribute("Name", edit.Hit.Value);
            if (edit.Damage != null) ((XmlElement)replacement["Damage"]).SetAttribute("Value", Number(edit.Damage.Value));
            if (edit.DamageTerms != null)
            {
                var damageNode = (XmlElement)replacement["Damage"];
                var stale = new List<XmlNode>();
                foreach (XmlNode child in damageNode.ChildNodes) if (child.Name == "Damage") stale.Add(child);
                foreach (var child in stale) damageNode.RemoveChild(child);
                XmlNode insertBefore = damageNode.FirstChild;
                foreach (string type in ModMoveCombatTermOrder.Ordered(edit.DamageTerms.Value.Keys))
                {
                    var term = document.CreateElement("Damage");
                    term.SetAttribute("Type", type);
                    double shift = edit.DamageTerms.Value[type];
                    if (shift != 0) term.SetAttribute("Shift", Number(shift));
                    damageNode.InsertBefore(term, insertBefore);
                }
            }
            if (edit.Edges != null)
            {
                var partsNode = replacement["AttackingParts"];
                if (partsNode == null) { partsNode = document.CreateElement("AttackingParts"); replacement.PrependChild(partsNode); }
                partsNode.RemoveAll();
                foreach (string edge in edit.Edges.Value)
                {
                    var node = document.CreateElement("Edge"); node.SetAttribute("Name", edge); partsNode.AppendChild(node);
                }
            }
            if (edit.Impulse != null)
            {
                var impulse = (XmlElement)replacement["Impulse"];
                if (impulse == null) { impulse = document.CreateElement("Impulse"); replacement.AppendChild(impulse); }
                impulse.SetAttribute("X", Number(edit.Impulse.Value[0]));
                impulse.SetAttribute("Y", Number(edit.Impulse.Value[1]));
                impulse.SetAttribute("Z", Number(edit.Impulse.Value[2]));
            }
            // Parsed originals, restored if the move parses the patched node before teardown.
            int originalEnd = rawEnd != null ? Integer(original, "End", -1) : -1;
            float originalDamage = damage != null ? ParseFloat(Attribute(damage, "Value")) : 0f;
            var originalTerms = new List<global::Pair<string, float>>();
            if (damage != null)
                foreach (XmlNode child in damage.ChildNodes)
                    if (child.Name == "Damage") originalTerms.Add(new global::Pair<string, float>(Attribute(child, "Type") ?? string.Empty, ParseFloat(Attribute(child, "Shift"))));
            var originalParts = new List<string>();
            if (original["AttackingParts"] != null) foreach (XmlNode edge in original["AttackingParts"].ChildNodes) originalParts.Add(Attribute(edge, "Name") ?? string.Empty);
            var originalImpulse = original["Impulse"];
            float ix = ParseFloat(Attribute(originalImpulse, "X")), iy = ParseFloat(Attribute(originalImpulse, "Y")), iz = ParseFloat(Attribute(originalImpulse, "Z"));
            string originalHit = hits.Count == 1 ? Attribute(hits[0], "Name") : null;
            lifetime.Apply.Add(() => attack.NodeInterval = replacement);
            lifetime.Undo.Add(() =>
            {
                if (ReferenceEquals(attack.NodeInterval, replacement)) { attack.NodeInterval = original; return; }
                if (attack.NodeInterval != null) return;
                if (edit.Start != null && attack.Start == edit.Start.Value) attack.Start = start;
                if (edit.End != null && attack.EndFrame == edit.End.Value) attack.EndFrame = originalEnd;
                if (edit.ChangesBounds)
                    foreach (var reaction in attack.HitReactions) { reaction.Start = attack.Start; reaction.EndFrameValue = attack.EndFrame; }
                if (edit.Hit != null && attack.HitReactions.Count == 1 && attack.HitReactions[0].Name == edit.Hit.Value) attack.HitReactions[0].Name = originalHit;
                if (edit.Damage != null) attack.EclipseSetDamage(originalDamage);
                if (edit.DamageTerms != null) { var list = attack.GetDamageAttributes(); list.Clear(); list.AddRange(originalTerms); }
                if (edit.Edges != null) attack.EclipseSetAttackingParts(originalParts);
                if (edit.Impulse != null) { var v = attack.GetImpulse(); v.SetX(ix); v.SetY(iy); v.SetZ(iz); }
            });
        }
    }
}
