using System.Collections.Generic;
using Nekki.SF2.Core.Fights.Controller;
using UnityEngine;
using UnityEngine.UI;

namespace Eclipse.UI
{
    // With the battle touch controls hidden, the magic, ranged and raid charge buttons are
    // the only place their cooldown fill and charge count are drawn. This shows a small,
    // non-interactive copy of those buttons in the bottom-right corner instead. Each copy
    // mirrors its source's graphics every frame (sprite, colour, radial fill, text, active
    // state), so it follows texture packs, pressed/ready states and rule-hidden buttons.
    public sealed class BattleCooldownHud : MonoBehaviour
    {
        private const float Scale = .5f;      // of each button's authored size
        private const float Margin = 48f;     // canvas units from the bottom-right corner
        private const float Spacing = 20f;
        private const float Alpha = .9f;

        private sealed class Mirror
        {
            public ProgressButton Source;
            public RectTransform Root;
            public readonly List<KeyValuePair<Graphic, Graphic>> Graphics = new List<KeyValuePair<Graphic, Graphic>>();
            public readonly List<KeyValuePair<GameObject, GameObject>> Objects = new List<KeyValuePair<GameObject, GameObject>>();
        }

        private readonly List<Mirror> _mirrors = new List<Mirror>();
        private CanvasGroup _group;

        public static void Attach(RectTransform parent, ActionButtons buttons)
        {
            if (parent == null || buttons == null || parent.GetComponentInChildren<BattleCooldownHud>(true) != null) return;
            var go = new GameObject("EclipseCooldownHud", typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var hud = go.AddComponent<BattleCooldownHud>();
            hud._group = go.AddComponent<CanvasGroup>();
            hud._group.interactable = false;
            hud._group.blocksRaycasts = false;
            hud._group.alpha = Alpha;
            // Right to left: magic sits in the corner, as on the touch layout.
            hud.Add(buttons.GetButtonMagic());
            hud.Add(buttons.GetButtonMissile());
            hud.Add(buttons.GetButtonRaidCharge());
            hud.LateUpdate();
        }

        private void Add(ProgressButton source)
        {
            if (source == null) return;
            var mirror = new Mirror { Source = source };
            var sourceRect = (RectTransform)source.transform;
            mirror.Root = Copy(sourceRect, transform, mirror);
            mirror.Root.anchorMin = mirror.Root.anchorMax = new Vector2(1f, 0f);
            mirror.Root.pivot = new Vector2(1f, 0f);
            mirror.Root.sizeDelta = sourceRect.rect.size;
            mirror.Root.localScale = Vector3.one * Scale;
            mirror.Root.localRotation = Quaternion.identity;
            _mirrors.Add(mirror);
        }

        // Rebuilds the source's Image/Text tree; other components (buttons, scripts) are skipped.
        private static RectTransform Copy(RectTransform source, Transform parent, Mirror mirror)
        {
            var go = new GameObject("Cooldown" + source.name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = source.anchorMin;
            rect.anchorMax = source.anchorMax;
            rect.pivot = source.pivot;
            rect.anchoredPosition = source.anchoredPosition;
            rect.sizeDelta = source.sizeDelta;
            rect.localScale = source.localScale;
            rect.localRotation = source.localRotation;

            var image = source.GetComponent<Image>();
            var text = source.GetComponent<Text>();
            if (image != null)
            {
                var copy = go.AddComponent<Image>();
                copy.raycastTarget = false;
                copy.material = image.material;
                copy.type = image.type;
                copy.preserveAspect = image.preserveAspect;
                copy.fillMethod = image.fillMethod;
                copy.fillOrigin = image.fillOrigin;
                copy.fillClockwise = image.fillClockwise;
                copy.fillCenter = image.fillCenter;
                mirror.Graphics.Add(new KeyValuePair<Graphic, Graphic>(image, copy));
            }
            else if (text != null)
            {
                var copy = go.AddComponent<Text>();
                copy.raycastTarget = false;
                copy.font = text.font;
                copy.fontSize = text.fontSize;
                copy.fontStyle = text.fontStyle;
                copy.alignment = text.alignment;
                copy.horizontalOverflow = text.horizontalOverflow;
                copy.verticalOverflow = text.verticalOverflow;
                copy.resizeTextForBestFit = text.resizeTextForBestFit;
                copy.resizeTextMinSize = text.resizeTextMinSize;
                copy.resizeTextMaxSize = text.resizeTextMaxSize;
                mirror.Graphics.Add(new KeyValuePair<Graphic, Graphic>(text, copy));
            }
            mirror.Objects.Add(new KeyValuePair<GameObject, GameObject>(source.gameObject, go));

            foreach (Transform child in source)
            {
                var childRect = child as RectTransform;
                if (childRect != null) Copy(childRect, rect, mirror);
            }
            return rect;
        }

        private void LateUpdate()
        {
            bool touchHidden = !BattleTouchControls.Visible;
            float right = -Margin;
            foreach (var mirror in _mirrors)
            {
                bool show = touchHidden && mirror.Source != null && mirror.Source.gameObject.activeInHierarchy;
                if (mirror.Root.gameObject.activeSelf != show) mirror.Root.gameObject.SetActive(show);
                if (!show) continue;

                mirror.Root.anchoredPosition = new Vector2(right, Margin);
                right -= mirror.Root.sizeDelta.x * Scale + Spacing;

                // Root's active state is the visibility rule above; children follow their sources.
                for (int i = 1; i < mirror.Objects.Count; i++)
                {
                    var pair = mirror.Objects[i];
                    bool active = pair.Key != null && pair.Key.activeSelf;
                    if (pair.Value.activeSelf != active) pair.Value.SetActive(active);
                }
                foreach (var pair in mirror.Graphics) Sync(pair.Key, pair.Value);
            }
        }

        private static void Sync(Graphic source, Graphic copy)
        {
            if (source == null) return;
            copy.enabled = source.enabled;
            copy.color = source.color;
            var image = source as Image;
            if (image != null)
            {
                var target = (Image)copy;
                if (target.sprite != image.overrideSprite) target.sprite = image.overrideSprite;
                if (target.fillAmount != image.fillAmount) target.fillAmount = image.fillAmount;
                return;
            }
            var text = source as Text;
            var label = (Text)copy;
            if (label.text != text.text) label.text = text.text;
        }
    }
}
