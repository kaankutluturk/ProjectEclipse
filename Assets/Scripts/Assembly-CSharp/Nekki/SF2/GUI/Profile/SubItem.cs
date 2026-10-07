using UnityEngine;
using UnityEngine.EventSystems;

namespace Nekki.SF2.GUI.Profile
{
	public class SubItem : SFButton
	{
		public enum SubItemEvent
		{
			onChoose = 10
		}

		[SerializeField]
		protected ResolutionImage _icon;

		[SerializeField]
		protected ResolutionImage _lockPicture;

		[SerializeField]
		protected ResolutionImage _inactivePicture;

		[SerializeField]
		protected ResolutionImage _selectedPicture;

		[SerializeField]
		protected ResolutionImage _selectWhiteSquare;

		[SerializeField]
		protected ResolutionImage _backPicture;

		public ProfileCell ParentCell;

		protected bool isLocked;

		protected bool _active;

		protected bool isSelected;

		public object Data;

		protected int animationFrame;

		protected int animationFrameCount;

		protected bool isFadingIn = true;

		protected float iconMaxOpacity = 1f;

		protected float iconMinOpacity = 1f;

		protected string _texturePath = SF2Paths.GetSkillsUiPath();

		protected string spriteName = string.Empty;

		private float selectedMaxOpacity;

		private float selectedMinOpacity;

		private static bool animationEnabled = true;

		private bool isSelectFlashing;

		private float selectFlashMinOpacity;

		private float selectFlashMaxOpacity = 1f;

		public void Init(int buttonId)
		{
			selectedMaxOpacity = ProfileGUI.SelectOpacity.Max / 255f;
			selectedMinOpacity = ProfileGUI.SelectOpacity.Min / 255f;
			animationFrameCount = ProfileGUI.AnimationSpeed;
			ButtonId = buttonId;
			_backPicture.gameObject.SetActive(false);
		}

		public virtual void SetLock(bool locked)
		{
			isLocked = locked;
			UpdateLockPicture();
		}

		public virtual bool GetLock()
		{
			return isLocked;
		}

		public virtual void SetActive(bool isActive)
		{
			_active = isActive;
			UpdateInactivePicture();
		}

		public virtual bool GetActive()
		{
			return _active;
		}

		public virtual void SetSelected(bool selected)
		{
			isSelected = selected;
			if ((bool)_selectedPicture)
			{
				_selectedPicture.gameObject.SetActive(isSelected);
			}
			if ((bool)_selectWhiteSquare && AssemblyController.GetGamepadEnabled())
			{
				_selectWhiteSquare.gameObject.SetActive(isSelected);
			}
		}

		public virtual bool GetSelected()
		{
			return isSelected;
		}

		public virtual void Choose()
		{
			int buttonId = ButtonId;
			CallEvent(10, buttonId);
		}

		public virtual void UpdateState()
		{
		}

		public static void EnableAnimation(bool value)
		{
			animationEnabled = value;
		}

		public void SetSelectFlashing(bool flashing)
		{
			if (_selectWhiteSquare != null)
			{
			}
			isSelectFlashing = flashing;
		}

		public void SetSelectFlashingMinOpacity(float minOpacity)
		{
			if (_selectWhiteSquare != null)
			{
			}
			selectFlashMinOpacity = minOpacity;
			if (selectFlashMinOpacity < 0f)
			{
				selectFlashMinOpacity = 0f;
			}
			if (selectFlashMinOpacity > 1f)
			{
				selectFlashMinOpacity = 1f;
			}
		}

		public void SetSelectFlashingMaxOpacity(int maxOpacity)
		{
			if (_selectWhiteSquare != null)
			{
			}
			selectFlashMaxOpacity = maxOpacity;
			if (selectFlashMaxOpacity < 0f)
			{
				selectFlashMaxOpacity = 0f;
			}
			if (selectFlashMaxOpacity > 1f)
			{
				selectFlashMaxOpacity = 1f;
			}
		}

		protected virtual void UpdateSelectedFlash()
		{
			if (isSelected)
			{
				float num = selectedMaxOpacity - selectedMinOpacity;
				if (num > 0f && animationFrameCount > 0)
				{
					float num2 = num / (float)animationFrameCount * (float)animationFrame;
					float alpha = ((!isFadingIn) ? (selectedMaxOpacity - num2) : (selectedMinOpacity + num2));
					UIExtensions.SetAlpha(_selectedPicture, alpha);
				}
			}
		}

		protected virtual void UpdateIconFlash()
		{
			float num = iconMaxOpacity - iconMinOpacity;
			if (num > 0f && animationFrameCount > 0)
			{
				float num2 = num / (float)animationFrameCount * (float)animationFrame;
				float alpha = ((!isFadingIn) ? (iconMaxOpacity - num2) : (iconMinOpacity + num2));
				if (_icon != null)
				{
					UIExtensions.SetAlpha(_icon, alpha);
				}
			}
		}

		public virtual void UpdateIcon()
		{
			_icon.set_TexturePath(_texturePath);
			_icon.set_SpriteName(spriteName);
			_selectedPicture.gameObject.SetActive(false);
			_lockPicture.gameObject.SetActive(false);
			_inactivePicture.gameObject.SetActive(false);
			UpdateInactivePicture();
		}

		protected virtual void UpdateInactivePicture()
		{
			if (!_inactivePicture.IsDestroyed())
			{
				_inactivePicture.gameObject.SetActive(!_active);
			}
		}

		protected virtual void UpdateLockPicture()
		{
			if (_lockPicture != null)
			{
				_lockPicture.gameObject.SetActive(isLocked);
			}
			if (_icon != null)
			{
				_icon.gameObject.SetActive(!isLocked);
			}
		}

		protected virtual void OnPressTypeChanged(ButtonStateExtensions.ButtonPressType pressType)
		{
			UpdateInactivePicture();
		}

		private void Update()
		{
			if (animationEnabled)
			{
				UpdateSelectedFlash();
			}
			animationFrame++;
			if (animationFrame > animationFrameCount)
			{
				animationFrame = 0;
				isFadingIn = !isFadingIn;
			}
		}

		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			if (!GetLock())
			{
				Choose();
			}
		}

		protected virtual void SetBackPictureVisible(bool isVisible)
		{
			_backPicture.gameObject.SetActive(isVisible);
		}
	}
}
