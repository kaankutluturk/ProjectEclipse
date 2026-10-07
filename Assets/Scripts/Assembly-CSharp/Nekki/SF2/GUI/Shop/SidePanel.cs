using DG.Tweening;
using DG.Tweening.Core.Surrogates;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Shop
{
	public class SidePanel : SFMonoBehaviour<object>
	{
		[SerializeField]
		private Vector3 _openBtnPos = new Vector2(0f, 0f);

		[SerializeField]
		private Vector3 _closeBtnPos = new Vector2(0f, 0f);

		[SerializeField]
		private Button _moveButton;

		[SerializeField]
		private ScrollRect _scrollRect;

		[SerializeField]
		private RectTransform _contentParent;

		[SerializeField]
		private CanvasGroup _canvasGroup;

		private SidePanelContent _content;

		private const float DefaultAnimationDuration = 1f;

		private bool _isOpen = true;

		private bool _shouldOpenOnRelease = true;

		private bool _isMovable = true;

		private string _openImage;

		private string _closeImage;

		private Vector2 _normalizedPosition;

		private Vector3 _closeButtonOffset = new Vector3(0f, 0f);

		private Vector3 _openButtonOffset = new Vector3(0f, 0f);

		private Tween _tween;

		public string OpenImageName
		{
			get
			{
				return get_OpenImage();
			}
			set
			{
				set_OpenImage(value);
			}
		}

		public string CloseImageName
		{
			get
			{
				return get_CloseImage();
			}
			set
			{
				set_CloseImage(value);
			}
		}

		public string get_OpenImage()
		{
			return _openImage;
		}

		public void set_OpenImage(string value)
		{
			_openImage = value;
			UpdateMoveButton();
		}

		public string get_CloseImage()
		{
			return _closeImage;
		}

		public void set_CloseImage(string value)
		{
			_closeImage = value;
			UpdateMoveButton();
		}

		public void Init(SidePanelContent content, bool isMovable, float buttonOffsetY = 0f, bool isOpen = true, string openImage = null, string closeImage = null)
		{
			_isMovable = isMovable;
			_closeButtonOffset.y = buttonOffsetY;
			_openButtonOffset.y = buttonOffsetY;
			_isOpen = isOpen;
			_openImage = openImage;
			_closeImage = closeImage;
			_content = content;
			if (_scrollRect != null)
			{
				_normalizedPosition = _scrollRect.normalizedPosition;
			}
			if (_moveButton != null)
			{
				_moveButton.gameObject.SetActive(_isMovable);
			}
			if (_scrollRect != null)
			{
				_scrollRect.horizontal = _isMovable;
			}
			if (_content != null && _contentParent != null)
			{
				_content.transform.SetParent(_contentParent, false);
			}
			SetOpen(_isOpen, 0f);
		}

		public void OnClick()
		{
			if (!_isMovable) return;
			SetOpen(!_isOpen);
		}

		public void OnValueChanged(Vector2 normalizedPosition)
		{
			if (!_isMovable) return;
			if (_normalizedPosition.x > normalizedPosition.x || normalizedPosition.x == 0f)
			{
				_shouldOpenOnRelease = true;
			}
			if (_normalizedPosition.x < normalizedPosition.x || normalizedPosition.x == 1f)
			{
				_shouldOpenOnRelease = false;
			}
			_normalizedPosition = normalizedPosition;
		}

		public void OnScrollDragBegin(PointerEventData data)
		{
			if (!_isMovable) return;
			KillTween();
			base.gameObject.transform.SetSiblingIndex(1);
		}

		public void OnScrollDragEnd(PointerEventData data)
		{
			if (!_isMovable) return;
			SetOpen(_shouldOpenOnRelease);
		}

		private void SetPosition(Vector2 normalizedPosition)
		{
			_normalizedPosition = normalizedPosition;
			if (_scrollRect != null)
			{
				_scrollRect.normalizedPosition = _normalizedPosition;
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

		private void TweenPosition(Vector2 targetPosition, float _Duration)
		{
			KillTween();
			_tween = DOTween.To(() => _normalizedPosition, (Vector2Wrapper positionWrapper) =>
			{
				SetPosition(positionWrapper);
			}, targetPosition, _Duration);
			_tween.OnComplete(OnTweenComplete);
		}

		private void OnTweenComplete()
		{
			if (!_isOpen)
			{
				base.gameObject.transform.SetSiblingIndex(0);
				if (_canvasGroup != null)
				{
					_canvasGroup.blocksRaycasts = false;
				}
			}
			else if (_canvasGroup != null)
			{
				_canvasGroup.blocksRaycasts = true;
			}
		}

		private void SetButtonSprite(string spriteName)
		{
			ResolutionImage resolutionImage = _moveButton.image as ResolutionImage;
			if (resolutionImage != null)
			{
				resolutionImage.set_SpriteName(spriteName);
			}
		}

		private void UpdateMoveButton()
		{
			if (_moveButton != null)
			{
				if (_isOpen && _openImage != null)
				{
					SetButtonSprite(_openImage);
					_moveButton.transform.localPosition = _openBtnPos + _openButtonOffset;
					_moveButton.transform.SetSiblingIndex(1);
				}
				else if (!_isOpen && _closeImage != null)
				{
					SetButtonSprite(_closeImage);
					_moveButton.transform.localPosition = _closeBtnPos + _closeButtonOffset;
					_moveButton.transform.SetSiblingIndex(0);
				}
			}
		}

		public void SetOpen(bool isOpen, float _Duration = 1f)
		{
			_isOpen = isOpen;
			if (isOpen)
			{
				TweenPosition(new Vector2(0f, 0f), _Duration);
				base.gameObject.transform.SetSiblingIndex(1);
				UpdateMoveButton();
			}
			else if (!isOpen)
			{
				TweenPosition(new Vector2(1f, 0f), _Duration);
				UpdateMoveButton();
				if (_canvasGroup != null)
				{
					_canvasGroup.blocksRaycasts = false;
				}
			}
		}
	}
}
