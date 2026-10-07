using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nekki.SF2.GUI
{
	public class BaseScrollItem : Button
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string itemName;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ButtonClickedEvent doubleClickEvent;

		protected float maxOpacity = 1f;

		protected float minOpacity;

		protected float currentOpacity = 1f;

		public ButtonClickedEvent DoubleClickEvent
		{
			get
			{
				return get_onDoubleClick();
			}
			set
			{
				set_onDoubleClick(value);
			}
		}

		public virtual Vector3 CenterWorldPosition
		{
			get
			{
				return get_CenterPosition();
			}
		}

		public float MaximumOpacity
		{
			get
			{
				return get_MaxOpacity();
			}
			set
			{
				set_MaxOpacity(value);
			}
		}

		public float MinimumOpacity
		{
			get
			{
				return get_MinOpacity();
			}
			set
			{
				set_MinOpacity(value);
			}
		}

		public virtual float CurrentOpacity
		{
			get
			{
				return get_Opacity();
			}
			set
			{
				set_Opacity(value);
			}
		}

		public BaseScrollItem()
		{
			set_Name(string.Empty);
			set_onDoubleClick(new ButtonClickedEvent());
		}

		public string get_Name()
		{
			return itemName;
		}

		public void set_Name(string value)
		{
			itemName = value;
		}

		public ButtonClickedEvent get_onDoubleClick()
		{
			return doubleClickEvent;
		}

		public void set_onDoubleClick(ButtonClickedEvent value)
		{
			doubleClickEvent = value;
		}

		public virtual Vector2 get_Size()
		{
			RectTransform rectTransform = (RectTransform)base.transform;
			return rectTransform.sizeDelta;
		}

		public virtual void set_Size(Vector2 value)
		{
			RectTransform rectTransform = (RectTransform)base.transform;
			rectTransform.sizeDelta = value;
		}

		public virtual Vector3 get_CenterPosition()
		{
			return base.transform.position;
		}

		public float get_MaxOpacity()
		{
			return maxOpacity;
		}

		public void set_MaxOpacity(float value)
		{
			maxOpacity = value;
		}

		public float get_MinOpacity()
		{
			return minOpacity;
		}

		public void set_MinOpacity(float value)
		{
			minOpacity = value;
		}

		public virtual float get_Opacity()
		{
			return currentOpacity;
		}

		public virtual void set_Opacity(float value)
		{
			currentOpacity = value;
		}

		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			if (eventData.clickCount > 1)
			{
				get_onDoubleClick().Invoke();
			}
		}
	}
}
