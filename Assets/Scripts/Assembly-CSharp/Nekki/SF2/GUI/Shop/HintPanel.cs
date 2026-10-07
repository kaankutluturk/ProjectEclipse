using System.Collections;
using Eclipse.Modding;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nekki.SF2.GUI.Shop
{
	public class HintPanel : MonoBehaviour
	{
		[SerializeField]
		private float hintBoxWidth = 650f;

		[SerializeField]
		private float hintBoxHeight = 200f;

		[SerializeField]
		private bool showingHint;

		[SerializeField]
		private float timeToHide = 5f;

		[SerializeField]
		private GameObject hintBoxPrefab;

		private GameObject hintSource;

		private IEnumerator hideCoroutine;

		private HintBox hintBox;

		private GameObject listAnchor;

		public void Init()
		{
			if (hintBoxPrefab != null)
			{
				GameObject gameObject = Object.Instantiate(hintBoxPrefab);
				hintBox = gameObject.GetComponent<HintBox>();
				RectTransform rectTransform = hintBox.transform as RectTransform;
				gameObject.transform.SetParent(base.transform, false);
				if (hintBox != null && rectTransform != null)
				{
					rectTransform.sizeDelta = new Vector2(hintBoxWidth, hintBoxHeight);
					hintBox.Init();
				}
				gameObject.SetActive(false);
				showingHint = false;
			}
		}

		public void ShowPerkHint(PerkInfoItem AEFFHJGMNFI, Vector2 MGMMDGFPBLP, Vector2 IPCOBJBKNAO, GameObject AOMLCBHAJJH)
		{
			if (AEFFHJGMNFI == null || false || hintBox == null)
			{
				return;
			}
			if (hintSource == AOMLCBHAJJH)
			{
				HideHintAndStopCorutine();
				return;
			}
			if (hintSource != null && hintSource != AOMLCBHAJJH && showingHint)
			{
				HideHintAndStopCorutine();
			}
				hintSource = AOMLCBHAJJH;
				hintBox.gameObject.SetActive(true);
				string title = AEFFHJGMNFI.Alias;
				string description = AEFFHJGMNFI.ResolveDescriptionText(AEFFHJGMNFI.DescriptionKey);
				// Public mod presentation belongs to the Eclipse definition, not to the
				// recovered PerkInfoItem compatibility projection. Saved/cloned enchantment
				// instances can carry legacy presentation metadata, so prefer the canonical
				// registry keys for qualified external perks/enchantments.
				string modTitle;
				string modDescription;
				if (ModRuntime.TryGetExternalEffectPresentation(AEFFHJGMNFI.Name, out modTitle, out modDescription))
				{
					title = modTitle;
					description = modDescription;
				}
				hintBox.SetText(title, description);
			if (AnchorToIcon(AOMLCBHAJJH))
			{
				showingHint = true;
				hideCoroutine = WaitAndHideHint();
				StartCoroutine(hideCoroutine);
				return;
			}
			bool flag = false;
			RectTransform component = base.transform.root.GetComponent<RectTransform>();
			if (component != null)
			{
				Vector2 vector = new Vector2(0f, (0f - component.sizeDelta.y) * 0.5f);
				Vector2 vector2 = MGMMDGFPBLP + IPCOBJBKNAO;
				vector2 = base.transform.InverseTransformPoint(vector2);
				flag = Mathf.Abs((vector - vector2).y) < hintBox.get_RectTransform().sizeDelta.y;
			}
			if (flag)
			{
				hintBox.transform.position = MGMMDGFPBLP - IPCOBJBKNAO;
				hintBox.Flip();
			}
			else
			{
				hintBox.transform.position = MGMMDGFPBLP + IPCOBJBKNAO;
				hintBox.ResetFlip();
			}
			showingHint = true;
			hideCoroutine = WaitAndHideHint();
			StartCoroutine(hideCoroutine);
		}

		public void ShowListHint(string titleAlias, string content, GameObject source, GameObject anchor)
		{
			if (hintBox == null || source == null || anchor == null) return;
			bool toggleOff = showingHint && hintSource == source;
			HideHintAndStopCorutine();
			if (toggleOff) return;
			hintSource = source;
			listAnchor = anchor;
			hintBox.ResetFlip();
			hintBox.gameObject.SetActive(true);
			hintBox.SetListContent(titleAlias, content);
			AnchorToIcon(anchor);
			showingHint = true;
			hideCoroutine = WaitAndHideHint();
			StartCoroutine(hideCoroutine);
		}

		// Called after the forge drawer docks during canvas layout.
		public void UpdateListHintPosition()
		{
			if (!showingHint || listAnchor == null) return;
			if (!listAnchor.activeInHierarchy) HideHintAndStopCorutine();
			else AnchorToIcon(listAnchor);
		}

		// Eclipse: the recovered placement added a fixed (0,-25) world-space offset to the icon
		// centre, which on the scaled canvas left the hint box covering the icon. Hang the box's
		// arrow off the icon's own edge instead: below it, or above it when there is no room,
		// kept inside the screen with the arrow still pointing at the icon.
		private const float ArrowReach = 12f;

		private bool AnchorToIcon(GameObject icon)
		{
			RectTransform iconRect = (icon != null) ? (icon.transform as RectTransform) : null;
			RectTransform root = base.transform.root as RectTransform;
			RectTransform box = hintBox.get_RectTransform();
			if (iconRect == null || root == null || box == null)
			{
				return false;
			}
			Rect iconBounds = LocalBounds(iconRect);
			Rect screen = LocalBounds(root);
			Vector2 size = box.rect.size;
			float below = iconBounds.yMin - ArrowReach;
			bool flip = below - size.y < screen.yMin && iconBounds.yMax + ArrowReach + size.y <= screen.yMax;
			float y = flip ? (iconBounds.yMax + ArrowReach) : below;
			float x = iconBounds.center.x;
			float half = size.x * 0.5f;
			if (screen.width > size.x)
			{
				x = Mathf.Clamp(x, screen.xMin + half, screen.xMax - half);
			}
			if (flip)
			{
				hintBox.Flip();
			}
			else
			{
				hintBox.ResetFlip();
			}
			box.localPosition = new Vector3(x, y, box.localPosition.z);
			float arrowShift = iconBounds.center.x - x;
			hintBox.SetArrowOffset(flip ? -arrowShift : arrowShift, half - 60f);
			return true;
		}

		private Rect LocalBounds(RectTransform target)
		{
			Vector3[] corners = new Vector3[4];
			target.GetWorldCorners(corners);
			Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 max = new Vector2(float.MinValue, float.MinValue);
			for (int i = 0; i < 4; i++)
			{
				Vector2 local = base.transform.InverseTransformPoint(corners[i]);
				min = Vector2.Min(min, local);
				max = Vector2.Max(max, local);
			}
			return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
		}

		public void HideHintAndStopCorutine()
		{
			if (hideCoroutine != null)
			{
				StopCoroutine(hideCoroutine);
			}
			HideHint();
		}

		public void HideHint()
		{
			showingHint = false;
			if (hintBox != null) hintBox.gameObject.SetActive(false);
			listAnchor = null;
			hintSource = null;
			hideCoroutine = null;
		}

		public IEnumerator WaitAndHideHint()
		{
			yield return new WaitForSeconds(timeToHide);
			HideHint();
		}

		public void Update()
		{
			if (showingHint && (Eclipse.Input.EclipseInput.touchCount > 0 || Eclipse.Input.EclipseInput.anyKeyDown) && (EventSystem.current == null || hintSource != EventSystem.current.currentSelectedGameObject))
			{
				HideHintAndStopCorutine();
			}
		}
	}
}
