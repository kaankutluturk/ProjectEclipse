using System;
using System.Diagnostics;
using UnityEngine;

namespace Nekki.SF2.GUI.Fight
{
	public class ActivePerkItem : MonoBehaviour, IComparable<ActivePerkItem>
	{
		[SerializeField]
		private ResolutionImage _icon;

		[SerializeField]
		private ResolutionImage _expiration;

		private global::Nekki.SF2.GUI.LabelAlias _eclipseStackCount;

		private const float MaxOpacity = 1f;

		private const float MinOpacity = 0f;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string perkName;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private PerksStage.ActionPerk currentAction;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool needsDelete;

		private bool isShown;

		private bool showsExpiration;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool isOpacityChanging;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float currentIconOpacity;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float expirationOpacity;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool isPulseGrowing;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int pulseFrame;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int pulseCount;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool deleteRequested;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool isHidden;

		public PerksStage.ActionPerk PerkAction
		{
			get
			{
				return get_Action();
			}
			private set
			{
				SetAction(value);
			}
		}

		public bool IsMarkedForDeletion
		{
			get
			{
				return get_NeedDelete();
			}
			private set
			{
				set_NeedDelete(value);
			}
		}

		public int ElapsedFrames
		{
			get
			{
				return get_CurrentFrames();
			}
		}

		public int DurationFrames
		{
			get
			{
				return get_TotalFrames();
			}
		}

		public bool IsShown
		{
			get
			{
				return get_Show();
			}
			set
			{
				set_Show(value);
			}
		}

		public bool IsExpirationShown
		{
			get
			{
				return get_ShowExpiration();
			}
			set
			{
				set_ShowExpiration(value);
			}
		}

		public bool IsOpacityChanging
		{
			get
			{
				return get_ChangeOpacity();
			}
			private set
			{
				SetOpacityChanging(value);
			}
		}

		public float IconOpacity
		{
			get
			{
				return get_CurrentIconOpacity();
			}
			private set
			{
				SetIconOpacity(value);
			}
		}

		private float ExpirationOpacity
		{
			get
			{
				return GetExpirationOpacity();
			}
			set
			{
				SetExpirationOpacity(value);
			}
		}

		private bool IsPulseGrowing
		{
			get
			{
				return GetPulseGrowing();
			}
			set
			{
				SetPulseGrowing(value);
			}
		}

		private int PulseFrame
		{
			get
			{
				return GetPulseFrame();
			}
			set
			{
				SetPulseFrame(value);
			}
		}

		public int PulseRepeats
		{
			get
			{
				return get_PulseCount();
			}
			set
			{
				set_PulseCount(value);
			}
		}

		public bool IsDeleteRequested
		{
			get
			{
				return get_DeleteRequested();
			}
			private set
			{
				SetDeleteRequested(value);
			}
		}

		private bool IsHidden
		{
			get
			{
				return GetHidden();
			}
			set
			{
				SetHidden(value);
			}
		}

		public int RemainingFrames
		{
			get
			{
				return get_FramesToEnd();
			}
		}

		public string get_Name()
		{
			return perkName;
		}

		private void set_Name(string value)
		{
			perkName = value;
		}

		public PerksStage.ActionPerk get_Action()
		{
			return currentAction;
		}

		private void SetAction(PerksStage.ActionPerk value)
		{
			currentAction = value;
		}

		public bool get_NeedDelete()
		{
			return needsDelete;
		}

		private void set_NeedDelete(bool value)
		{
			needsDelete = value;
		}

		public int get_CurrentFrames()
		{
			return (get_Action() != null) ? get_Action().ElapsedFrames : 0;
		}

		public int get_TotalFrames()
		{
			return (get_Action() != null) ? get_Action().DurationFrames : 0;
		}

		public bool get_Show()
		{
			return isShown;
		}

		public void set_Show(bool value)
		{
			isShown = value;
			SetOpacityChanging(true);
		}

		public bool get_ShowExpiration()
		{
			return showsExpiration;
		}

		public void set_ShowExpiration(bool value)
		{
			showsExpiration = value;
			if (_expiration != null)
			{
				_expiration.gameObject.SetActive(showsExpiration);
			}
		}

		public bool get_ChangeOpacity()
		{
			return isOpacityChanging;
		}

		private void SetOpacityChanging(bool value)
		{
			isOpacityChanging = value;
		}

		public float get_CurrentIconOpacity()
		{
			return currentIconOpacity;
		}

		private void SetIconOpacity(float value)
		{
			currentIconOpacity = value;
		}

		private float GetExpirationOpacity()
		{
			return expirationOpacity;
		}

		private void SetExpirationOpacity(float value)
		{
			expirationOpacity = value;
		}

		private bool GetPulseGrowing()
		{
			return isPulseGrowing;
		}

		private void SetPulseGrowing(bool value)
		{
			isPulseGrowing = value;
		}

		private int GetPulseFrame()
		{
			return pulseFrame;
		}

		private void SetPulseFrame(int value)
		{
			pulseFrame = value;
		}

		public int get_PulseCount()
		{
			return pulseCount;
		}

		public void set_PulseCount(int value)
		{
			pulseCount = value;
		}

		public bool get_DeleteRequested()
		{
			return deleteRequested;
		}

		private void SetDeleteRequested(bool value)
		{
			deleteRequested = value;
		}

		private bool GetHidden()
		{
			return isHidden;
		}

		private void SetHidden(bool value)
		{
			isHidden = value;
		}

		public int get_FramesToEnd()
		{
			return get_TotalFrames() - get_CurrentFrames();
		}

		public void Init(PerksStage.ActionPerk IBODMPMJELJ)
		{
			SetAction(IBODMPMJELJ);
			set_Name(IBODMPMJELJ.IconPath);
			set_Show(true);
			SetOpacityChanging(true);
			SetHidden(false);
			set_NeedDelete(false);
			SetIconOpacity(0f);
			SetExpirationOpacity(PerkGUI.GetExpirationOpacity());
			set_ShowExpiration(IBODMPMJELJ.ShowExpiration);
			SetEclipseStackCount(IBODMPMJELJ.EclipseStackCount);
			RectTransform rectTransform = base.transform as RectTransform;
			if (rectTransform == null)
			{
				rectTransform = base.gameObject.AddComponent<RectTransform>();
			}
			if (_icon != null)
			{
				_icon.set_SpriteName(IBODMPMJELJ.IconPath);
				_icon.SetNativeSize();
				_icon.set_Alpha(0f);
				rectTransform.sizeDelta = _icon.rectTransform.sizeDelta;
			}
			if (_expiration != null)
			{
				_expiration.set_Alpha(GetExpirationOpacity());
				_expiration.fillAmount = 0f;
			}
		}

		public void SetEclipseStackCount(int count)
		{
			if (count <= 0)
			{
				if (_eclipseStackCount != null) _eclipseStackCount.gameObject.SetActive(false);
				return;
			}
			if (_eclipseStackCount == null)
			{
				var badge = new GameObject("EclipseStackCount", typeof(RectTransform), typeof(CanvasRenderer),
					typeof(global::Nekki.SF2.GUI.LabelAlias));
				badge.layer = gameObject.layer;
				var rect = badge.GetComponent<RectTransform>();
				rect.SetParent(transform, false);
				rect.anchorMin = Vector2.zero;
				rect.anchorMax = Vector2.one;
				rect.offsetMin = new Vector2(4f, 2f);
				rect.offsetMax = new Vector2(-4f, -2f);
				_eclipseStackCount = badge.GetComponent<global::Nekki.SF2.GUI.LabelAlias>();
				_eclipseStackCount.FontType = global::Nekki.SF2.GUI.LabelAlias.LabelFontType.Content;
				_eclipseStackCount.UseLocalizationFont = true;
				_eclipseStackCount.UseLabelFontSize = false;
				_eclipseStackCount.fontSize = 26;
				_eclipseStackCount.alignment = TextAnchor.LowerRight;
				_eclipseStackCount.horizontalOverflow = HorizontalWrapMode.Overflow;
				_eclipseStackCount.verticalOverflow = VerticalWrapMode.Overflow;
				_eclipseStackCount.raycastTarget = false;
				var outline = badge.AddComponent<UnityEngine.UI.Outline>();
				outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
				outline.effectDistance = new Vector2(1f, -1f);
			}
			_eclipseStackCount.gameObject.SetActive(true);
			_eclipseStackCount.set_text(count.ToString(System.Globalization.CultureInfo.InvariantCulture));
		}

		private void ClearAction()
		{
			SetAction(null);
		}

		private void ForceShow()
		{
			if (!get_Show() && _icon != null)
			{
				_icon.set_Alpha(1f);
				SetIconOpacity(1f);
				set_Show(true);
			}
		}

		private void FadeIn()
		{
			if (get_CurrentIconOpacity() < 1f)
			{
				float num = 1f / PerkGUI.GetFadeFrames().x;
				SetIconOpacity(get_CurrentIconOpacity() + num);
				if (get_CurrentIconOpacity() >= 1f)
				{
					SetIconOpacity(1f);
					SetOpacityChanging(false);
				}
				if (_icon != null)
				{
					_icon.set_Alpha(get_CurrentIconOpacity());
				}
			}
		}

		private void FadeOut()
		{
			if (get_CurrentIconOpacity() > 0f)
			{
				float num = 1f / PerkGUI.GetFadeFrames().y;
				SetIconOpacity(get_CurrentIconOpacity() - num);
				if (get_CurrentIconOpacity() <= 0f)
				{
					SetIconOpacity(0f);
					SetOpacityChanging(false);
				}
				if (_icon != null)
				{
					_icon.set_Alpha(get_CurrentIconOpacity());
				}
			}
		}

		public void Render()
		{
			UpdatePulse();
			if (showsExpiration && _expiration != null)
			{
				float fillAmount = ((get_TotalFrames() == 0) ? 0f : ((float)get_CurrentFrames() / (float)get_TotalFrames()));
				_expiration.fillAmount = fillAmount;
			}
			if (get_ChangeOpacity())
			{
				if (get_Show())
				{
					FadeIn();
				}
				else
				{
					FadeOut();
				}
				if (get_ShowExpiration() && _expiration != null)
				{
					_expiration.set_Alpha(get_CurrentIconOpacity() * (GetExpirationOpacity() / 1f));
				}
				if (get_CurrentIconOpacity() == 0f)
				{
					Destroy();
				}
			}
		}

		private void UpdatePulse()
		{
			if (get_PulseCount() <= 0)
			{
				return;
			}
			float num = PerkGUI.GetPulseAmplitude();
			float x = PerkGUI.GetPulseAccel().x;
			float y = PerkGUI.GetPulseAccel().y;
			float x2 = PerkGUI.GetPulseFrames().x;
			float y2 = PerkGUI.GetPulseFrames().y;
			int num2 = (int)((!GetPulseGrowing()) ? y2 : x2);
			float num3 = (GetPulseGrowing() ? 1f : num);
			if (GetPulseFrame() > num2)
			{
				SetPulseGrowing(!GetPulseGrowing());
				SetPulseFrame(0);
			}
			if (GetPulseFrame() <= num2)
			{
				if (GetPulseGrowing())
				{
					num3 = 1f + (num - 1f) / x2 * (x * Mathf.Pow(GetPulseFrame(), 2f) / x2 + (1f - x) * (float)GetPulseFrame());
				}
				else
				{
					num3 = num - (num - 1f) / y2 * (y * Mathf.Pow(GetPulseFrame(), 2f) / y2 + (1f - y) * (float)GetPulseFrame());
					if (num3 <= 1f)
					{
						set_PulseCount(get_PulseCount() - 1);
					}
				}
			}
			Vector2 vector = base.transform.localScale;
			vector.x = num3;
			vector.y = num3;
			base.transform.localScale = vector;
			SetPulseFrame(GetPulseFrame() + 1);
		}

		public void Destroy()
		{
			set_NeedDelete(true);
			SetOpacityChanging(false);
			base.gameObject.SetActive(false);
			UnityEngine.Object.Destroy(base.gameObject);
		}

		public int CompareTo(ActivePerkItem NOLFMPDGCOC)
		{
			if (NOLFMPDGCOC == null)
			{
				return 1;
			}
			return get_FramesToEnd().CompareTo(NOLFMPDGCOC.get_FramesToEnd());
		}
	}
}
