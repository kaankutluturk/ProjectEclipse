using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Menu
{
	public class MenuScroll : SFMonoBehaviour<object>, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public enum MenuScrollEvent
		{
			OnOpen = 0,
			OnClose = 1,
			OnChanging = 2,
			OnRolling = 3,
			OnTouch = 4
		}

		public enum ScrollOrientation
		{
			Horizontal = 0,
			Vertical = 1
		}

		public enum ScrollState
		{
			ScrollNone = 0,
			ScrollOpen = 1,
			ScrollClose = 2,
			ScrollOpeninig = 3,
			ScrollClosinig = 4
		}

		private enum MenuScrollLayer
		{
			ZBackground = 0,
			ZPaper = 1,
			ZContent = 2,
			ZWheel = 3
		}

		public Color SCROLL_BORDER_COLOR = new Color(255f, 100f, 100f);

		public const float SCROLL_EXPAND_TIME = 0.3f;

		public const int SCROLL_SAFE_DISTANCE = 15;

		public bool IsOpen;

		public ScrollState CurScrollState;

		private float unusedFloat;

		private float expandedLength;

		private float dragThresholdPercent = 10f;

		private ScrollOrientation _type;

		[SerializeField]
		private Button _wheel;

		[SerializeField]
		private Text _label;

		[SerializeField]
		private Image _background;

		private float backgroundAlphaScale = 1f;

		private Vector2 _touchPoint;

		private bool isExpanded;

		private bool isDragging;

		private bool allowRolling;

		private bool closeOnBackgroundClick;

		private Tween _tween;

		public void Init(ScrollOrientation LFLGCDNKNJI = ScrollOrientation.Vertical)
		{
			_type = LFLGCDNKNJI;
			expandedLength = GetCurrentLength();
			allowRolling = true;
			IsOpen = false;
			closeOnBackgroundClick = true;
			isDragging = false;
			CurScrollState = ScrollState.ScrollNone;
			float a = _background.color.a;
			backgroundAlphaScale = 255f / a;
			Collapse(0f);
		}

		public void Expand(float _Duration)
		{
			isExpanded = true;
			AnimateToLength(expandedLength, _Duration);
		}

		public void Collapse(float _Duration)
		{
			isExpanded = false;
			AnimateToLength(0f, _Duration);
		}

		private void AnimateToLength(float GGAIEIDOEAD, float _Duration)
		{
			KillTween();
			if (_Duration <= 0f)
			{
				SetLength(GGAIEIDOEAD);
				return;
			}
			_tween = DOTween.To(() => GetCurrentLength(), (float ECHIHNECKFK) =>
			{
				SetLength(ECHIHNECKFK);
			}, GGAIEIDOEAD, _Duration);
		}

		public bool IsExpanded()
		{
			return isExpanded;
		}

		public void SetOutsideTouchProperties(bool NEHLEJGGCIE)
		{
			closeOnBackgroundClick = NEHLEJGGCIE;
		}

		public float GetCurrentLength()
		{
			if (_type == ScrollOrientation.Vertical)
			{
				return base.gameObject.GetComponent<RectTransform>().rect.height;
			}
			return base.gameObject.GetComponent<RectTransform>().rect.width;
		}

		public void SetAllowRolling(bool value)
		{
			allowRolling = value;
		}

		public bool GetAllowRolling()
		{
			return allowRolling;
		}

		public Button GetButton()
		{
			return _wheel;
		}

		private void Update()
		{
		}

		private void SetLength(float BDBOAEGELMC)
		{
			if (BDBOAEGELMC < 0f)
			{
				BDBOAEGELMC = 0f;
			}
			bool flag = BDBOAEGELMC == 0f;
			bool flag2 = Mathf.Abs(BDBOAEGELMC) == Mathf.Abs(expandedLength);
			if (_type == ScrollOrientation.Vertical)
			{
				base.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(base.gameObject.GetComponent<RectTransform>().rect.width, BDBOAEGELMC);
			}
			else
			{
				base.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(BDBOAEGELMC, base.gameObject.GetComponent<RectTransform>().rect.height);
			}
			UpdateBackgroundAlpha();
			if (flag && CurScrollState != ScrollState.ScrollClose)
			{
				CurScrollState = ScrollState.ScrollClose;
				CallEvent(2, false);
				CallEvent(1, 0);
			}
			else if (flag2 && CurScrollState != ScrollState.ScrollOpen)
			{
				CurScrollState = ScrollState.ScrollOpen;
				CallEvent(2, true);
				CallEvent(0, 0);
			}
			else if (!flag && !flag2)
			{
				CurScrollState = ScrollState.ScrollNone;
			}
			CallEvent(3, CurScrollState);
		}

		public void OnMenuBtnClick()
		{
			if (isExpanded)
			{
				Collapse(0.3f);
			}
			else
			{
				Expand(0.3f);
			}
		}

		public void OnBackgroundClick()
		{
			if (closeOnBackgroundClick)
			{
				Collapse(0.3f);
			}
		}

		private void UpdateBackgroundAlpha()
		{
			float num = Mathf.Abs(GetCurrentLength() / expandedLength) * 255f;
			float num2 = num / backgroundAlphaScale;
			_background.color = new Color(_background.color.r, _background.color.g, _background.color.b, num2);
			if (num2 == 0f && _background.raycastTarget)
			{
				_background.raycastTarget = false;
			}
			else if (num2 > 0f && !_background.raycastTarget)
			{
				_background.raycastTarget = true;
			}
		}

		private bool IsTouchOnWheel(Vector2 DGEJJGMMODA)
		{
			return true;
		}

		public void OnBeginDrag(PointerEventData BHOLFGOGPCP)
		{
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), BHOLFGOGPCP.position, BHOLFGOGPCP.pressEventCamera, out localPoint);
			if (allowRolling || IsTouchOnWheel(localPoint))
			{
				_touchPoint = localPoint;
				isDragging = true;
				KillTween();
				CallEvent(4, 0);
			}
		}

		public void OnDrag(PointerEventData BHOLFGOGPCP)
		{
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), BHOLFGOGPCP.position, BHOLFGOGPCP.pressEventCamera, out localPoint);
			if (isDragging)
			{
				float num = _touchPoint.x - localPoint.x;
				float num2 = _touchPoint.y - localPoint.y;
				float a = GetCurrentLength() + ((_type != ScrollOrientation.Vertical) ? num : num2);
				SetLength(Mathf.Min(a, expandedLength));
				_touchPoint = localPoint;
			}
		}

		public void OnEndDrag(PointerEventData BHOLFGOGPCP)
		{
			isDragging = false;
			float num = ((!isExpanded) ? GetCurrentLength() : (expandedLength - GetCurrentLength()));
			bool flag = num > expandedLength * dragThresholdPercent / 100f;
			bool flag2 = isExpanded != flag;
			float dFNBHOEGAHO = ((!flag2) ? GetCurrentLength() : (expandedLength - GetCurrentLength())) / expandedLength * 0.3f;
			if (flag2)
			{
				Expand(dFNBHOEGAHO);
			}
			else
			{
				Collapse(dFNBHOEGAHO);
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
