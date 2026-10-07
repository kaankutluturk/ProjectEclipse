using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Map
{
	public class BattleButton : ResolutionButton
	{
		[SerializeField]
		private string _battleName = string.Empty;

		private Battle battleData;

		public bool Locked;

		private bool isHidden;

		[SerializeField]
		private LabelAlias _lblName;

		private float _CurrentAlpha = 1f;

		private Tween _tween;

		public Battle BattleRef
		{
			get
			{
				return get_Battle();
			}
			set
			{
				set_Battle(value);
			}
		}

		public bool IsHiddenOnMap
		{
			get
			{
				return get_Hidden();
			}
			set
			{
				set_Hidden(value);
			}
		}

		public Battle get_Battle()
		{
			return battleData;
		}

		public void set_Battle(Battle value)
		{
			battleData = value;
			_battleName = battleData.get_Name();
		}

		public bool get_Hidden()
		{
			return isHidden;
		}

		public void set_Hidden(bool value)
		{
			isHidden = value;
			base.gameObject.SetActive(get_Battle().IsMapVisible && !isHidden);
		}

		public void Init(string baseSprite, string activeSprite, string lockedSprite, string lockedActiveSprite, bool locked, string iconAtlas = "")
		{
			// Zones 1-6 still tag their original Tournament/Lynx map entries with
			// the legacy BattleBtnStart atlas. The modern bundle no longer ships that
			// atlas, so those normal states fall back to recovered BattleBtnBase art
			// while their Eclipse replacements come from the bundle. ResolutionImage
			// restores the fallback sprites' stripped padding without changing the
			// 300x300 map-button hit area or label layout.
			if (iconAtlas == "BattleBtnStart")
			{
				iconAtlas = string.Empty;
			}
			bool raidDefault = iconAtlas == "BattleBtn_raid";
			string baseAtlas = string.IsNullOrEmpty(iconAtlas) ? "BattleBtnBase" :
				(raidDefault ? "BattleBtnBase_raid" : iconAtlas + "Base");
			string activeAtlas = string.IsNullOrEmpty(iconAtlas) ? "BattleBtnActive" :
				(raidDefault ? "BattleBtnActive_raid" : iconAtlas + "Active");
			string lockAtlas = string.IsNullOrEmpty(iconAtlas) ? "BattleBtnLock" :
				(raidDefault ? "BattleBtnLock_raid" : iconAtlas + "Lock");
			string lockActiveAtlas = string.IsNullOrEmpty(iconAtlas) ? "BattleBtnLockActive" :
				(raidDefault ? "BattleBtnLockActive_raid" : iconAtlas + "LockActive");
			// Some raid pages intentionally reuse classic icons. Prefer the raid or
			// event atlas, but gracefully retain those classic entries.
			if (ResolutionImage.GetSprite("UI/Atlases/", baseAtlas + "." + baseSprite) == null)
			{
				baseAtlas = "BattleBtnBase";
			}
			if (ResolutionImage.GetSprite("UI/Atlases/", activeAtlas + "." + activeSprite) == null)
			{
				activeAtlas = "BattleBtnActive";
			}
			if (ResolutionImage.GetSprite("UI/Atlases/", lockAtlas + "." + lockedSprite) == null)
			{
				lockAtlas = "BattleBtnLock";
			}
			if (ResolutionImage.GetSprite("UI/Atlases/", lockActiveAtlas + "." + lockedActiveSprite) == null)
			{
				lockActiveAtlas = "BattleBtnLockActive";
			}
			if (!locked)
			{
				SetNormalSprite("UI/Atlases/", baseAtlas + "." + baseSprite);
				SetDisabledSprite("UI/Atlases/", activeAtlas + "." + activeSprite);
			}
			else
			{
				SetNormalSprite("UI/Atlases/", lockAtlas + "." + lockedSprite);
				SetDisabledSprite("UI/Atlases/", lockActiveAtlas + "." + lockedActiveSprite);
			}
		}

		// Eclipse modding seam: mod-supplied map-button sprites (qualified IDs) replace
		// the atlas lookup. A locked button keeps the native lock art unless the mod
		// supplied its own locked sprites.
		public void ApplyModIcons(string baseSprite, string activeSprite, string lockedSprite, string lockedActiveSprite, bool locked)
		{
			if (!locked)
			{
				SetNormalSprite(string.Empty, baseSprite);
				SetDisabledSprite(string.Empty, activeSprite);
			}
			else if (!string.IsNullOrEmpty(lockedSprite))
			{
				SetNormalSprite(string.Empty, lockedSprite);
				SetDisabledSprite(string.Empty, lockedActiveSprite);
			}
		}

		public void CorrectLabel()
		{
		}

		public void SetText(string text)
		{
			_lblName.set_text(text);
		}

		public void SetAlias(string alias)
		{
			_lblName.SetAlias(alias);
		}

		public void SetActiveBattle(bool isActive)
		{
			base.interactable = !isActive;
			base.transition = (base.interactable ? Transition.ColorTint : Transition.SpriteSwap);
		}

		public void SetAlpha(float alpha, float time = 0f)
		{
			KillTween();
			if (!base.gameObject.activeSelf || time <= 0f)
			{
				ApplyAlpha(alpha);
				return;
			}
			_tween = DOTween.To(() => _CurrentAlpha, (float alpha) =>
			{
				ApplyAlpha(alpha);
			}, alpha, time);
		}

		private void ApplyAlpha(float alpha)
		{
			if (_CurrentAlpha != alpha)
			{
				_CurrentAlpha = Mathf.Clamp(alpha, 0f, 1f);
				GetComponent<CanvasGroup>().alpha = _CurrentAlpha;
			}
		}

		private void KillTween()
		{
			if (_tween != null)
			{
				_tween.Kill();
				_tween = null;
			}
		}
	}
}
