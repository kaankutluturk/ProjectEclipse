using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Shop
{
	public class TouchHandler : Selectable
	{
		private UnityEvent _onTouch = new UnityEvent();

		public UnityEvent OnTouchEvent
		{
			get
			{
				return get_OnTouch();
			}
		}

		public UnityEvent get_OnTouch()
		{
			return _onTouch;
		}

		public override void OnPointerDown(PointerEventData BHOLFGOGPCP)
		{
			base.OnPointerDown(BHOLFGOGPCP);
			_onTouch.Invoke();
		}

		private new void OnDestroy()
		{
			get_OnTouch().RemoveAllListeners();
			base.OnDestroy();
		}
	}
}
