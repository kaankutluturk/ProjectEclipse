using System.Collections.Generic;
using System.Text.RegularExpressions;
using Nekki.SF2.GUI;
using UnityEngine;
using UnityEngine.UI;

namespace Eclipse.UI
{
    // A combo enchantment (a set ability such as Monk's Tempest Rage) is active once every
    // equipped gear piece carries it (GameUtils.ILFJCODGINO, used by ModelParameters when
    // building fight perks). The recovered game has no feedback when that happens, so
    // equipment and forge actions snapshot the active combos beforehand and announce any
    // that became active with a short banner.
    public sealed class SetBonusNotice : MonoBehaviour
    {
        private const float FadeSeconds = .25f, HoldSeconds = 3.5f;
        private static readonly Regex Markup = new Regex(@"\{[^}]*\}");
        private static SetBonusNotice instance;

        private readonly Queue<PerkInfoItem> _pending = new Queue<PerkInfoItem>();
        private CanvasGroup _group;
        private ResolutionImage _icon;
        private Text _title, _brief;
        private float _shownAt = -1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() { instance = null; }

        public static HashSet<string> ActiveCombos()
        {
            var result = new HashSet<string>();
            try
            {
                Roster roster = ListSF.CCDKHLAMKKO();
                if (roster == null || roster.KHCNHPCPFII() == null) return result;
                foreach (UserItem item in roster.KHCNHPCPFII().JCMOHPFKPBO())
                {
                    ItemInfo info = item.BHKHOJPANHE();
                    if (info == null || info.IgnoreInventoryEnchantments) continue;
                    foreach (PerkInfoItem perk in item.GetEnchantments())
                        if (perk != null && perk.LELHEEDNMBP == PerkInfoItem.DNPGIEGCGKH.COMBO && GameUtils.ILFJCODGINO(perk))
                            result.Add(perk.Name);
                }
            }
            catch (System.Exception error)
            {
                Debug.LogWarning("[SetBonus] Could not read equipped combos: " + error.Message);
            }
            return result;
        }

        // Shows a banner for every combo active now that was not in before.
        public static void AnnounceNew(HashSet<string> before)
        {
            if (before == null) return;
            HashSet<string> now = ActiveCombos();
            foreach (string name in now)
            {
                if (before.Contains(name)) continue;
                PerkInfoItem perk = GameUtils.FDEJIIDIPBI.ABAGJKMKCBA(name);
                if (perk != null) Instance().Enqueue(perk);
            }
        }

        private static SetBonusNotice Instance()
        {
            if (instance != null) return instance;
            var canvasObject = new GameObject("Eclipse Set Bonus Notice", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            DontDestroyOnLoad(canvasObject);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30500;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 1f;
            instance = canvasObject.AddComponent<SetBonusNotice>();
            instance.Build(canvasObject.transform);
            return instance;
        }

        private void Build(Transform root)
        {
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            panel.transform.SetParent(root, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f);
            rect.pivot = new Vector2(.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -24f);
            rect.sizeDelta = new Vector2(560f, 104f);
            var background = panel.GetComponent<Image>();
            background.color = new Color(.08f, .06f, .05f, .88f);
            background.raycastTarget = false;
            _group = panel.GetComponent<CanvasGroup>();
            _group.alpha = 0f;
            _group.interactable = _group.blocksRaycasts = false;

            var iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(ResolutionImage));
            iconObject.transform.SetParent(panel.transform, false);
            var iconRect = (RectTransform)iconObject.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, .5f);
            iconRect.pivot = new Vector2(0f, .5f);
            iconRect.anchoredPosition = new Vector2(14f, 0f);
            iconRect.sizeDelta = new Vector2(80f, 80f);
            _icon = iconObject.GetComponent<ResolutionImage>();
            _icon.raycastTarget = false;
            _icon.preserveAspect = true;
            _icon.set_TexturePath("UI/Atlases/");

            _title = Label(panel.transform, "Title", new Vector2(108f, -12f), new Vector2(-122f, 34f), 24, new Color32(240, 196, 98, 255));
            _brief = Label(panel.transform, "Brief", new Vector2(108f, -46f), new Vector2(-122f, 50f), 16, new Color32(223, 207, 177, 255));
            _brief.horizontalOverflow = HorizontalWrapMode.Wrap;
            _brief.verticalOverflow = VerticalWrapMode.Truncate;
            panel.SetActive(false);
        }

        private static Text Label(Transform parent, string name, Vector2 position, Vector2 size, int fontSize, Color color)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Shadow));
            textObject.transform.SetParent(parent, false);
            var rect = (RectTransform)textObject.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var label = textObject.GetComponent<Text>();
            label.font = Resources.Load<Font>("ui/fonts/AGOpusBold") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAnchor.UpperLeft;
            label.raycastTarget = false;
            textObject.GetComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, .75f);
            return label;
        }

        private void Enqueue(PerkInfoItem perk)
        {
            _pending.Enqueue(perk);
            if (_shownAt < 0f) ShowNext();
        }

        private void ShowNext()
        {
            if (_pending.Count == 0) { _shownAt = -1f; _group.gameObject.SetActive(false); return; }
            PerkInfoItem perk = _pending.Dequeue();
            ItemSet set = FindSet(perk);
            string name = set != null ? Localized(set.Title) : string.Empty;
            if (string.IsNullOrEmpty(name)) name = Localized(perk.HBCNKNFPAIM);
            if (string.IsNullOrEmpty(name)) name = perk.Name;
            _title.text = "SET BONUS ACTIVE: " + name;
            string brief = set != null ? Localized(set.LHOJGHFGLFD) : string.Empty;
            if (string.IsNullOrEmpty(brief)) brief = Localized(perk.MGNNJPBCOGD);
            _brief.text = Markup.Replace(brief ?? string.Empty, string.Empty).Trim();
            _icon.enabled = !string.IsNullOrEmpty(perk.NHKMCLPOMFK);
            if (_icon.enabled) _icon.set_SpriteName(perk.NHKMCLPOMFK);
            _group.gameObject.SetActive(true);
            _group.alpha = 0f;
            _shownAt = Time.unscaledTime;
        }

        private static ItemSet FindSet(PerkInfoItem perk)
        {
            ItemSets sets = ListSF.GetItems()?.DGKMILIPLLF();
            if (sets == null) return null;
            if (!string.IsNullOrEmpty(perk.DIJBDEJFKKF))
            {
                ItemSet named = sets.IGHHCHBEHOH(perk.DIJBDEJFKKF);
                if (named != null) return named;
            }
            foreach (ItemSet set in sets.Sets)
                if (set.DefaultComboPerk == perk.Name) return set;
            return null;
        }

        private static string Localized(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            string value = LocalizationManager.GetString(key);
            return string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        private void Update()
        {
            if (_shownAt < 0f) return;
            float elapsed = Time.unscaledTime - _shownAt;
            float total = FadeSeconds * 2f + HoldSeconds;
            if (elapsed >= total) { ShowNext(); return; }
            float alpha = elapsed < FadeSeconds ? elapsed / FadeSeconds
                : elapsed > total - FadeSeconds ? (total - elapsed) / FadeSeconds : 1f;
            _group.alpha = alpha;
        }
    }
}
