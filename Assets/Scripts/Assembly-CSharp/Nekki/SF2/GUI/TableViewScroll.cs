using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Nekki.SF2.GUI
{
	public class TableViewScroll : SFScrollRect
	{
		private TableViewOrientation _orientation;

		[SerializeField]
		public UnityEvent onDragBegin = new UnityEvent();

		[SerializeField]
		public UnityEvent onDragEnd = new UnityEvent();

		public float ContentSizeDelta
		{
			get
			{
				return get_SizeDelta();
			}
			set
			{
				set_SizeDelta(value);
			}
		}

		public float get_Size()
		{
			if (_orientation == TableViewOrientation.Horizontal)
			{
				return get_content().rect.width;
			}
			return get_content().rect.height;
		}

		public float get_SizeDelta()
		{
			if (_orientation == TableViewOrientation.Horizontal)
			{
				return get_content().sizeDelta.x;
			}
			return get_content().sizeDelta.y;
		}

		public void set_SizeDelta(float value)
		{
			if (_orientation == TableViewOrientation.Horizontal)
			{
				get_content().sizeDelta = new Vector2(value, get_content().sizeDelta.y);
			}
			else
			{
				get_content().sizeDelta = new Vector2(get_content().sizeDelta.x, value);
			}
		}

		public void Init()
		{
			CreateContent();
		}

		public void SetOrientation(TableViewOrientation orientation)
		{
			_orientation = orientation;
			if (_orientation == TableViewOrientation.Horizontal)
			{
				get_content().anchorMin = new Vector2(0f, 0f);
				get_content().anchorMax = new Vector2(0f, 1f);
				get_content().pivot = new Vector2(0f, 0.5f);
			}
			else
			{
				get_content().anchorMin = new Vector2(0f, 1f);
				get_content().anchorMax = new Vector2(1f, 1f);
				get_content().pivot = new Vector2(0.5f, 1f);
			}
			set_horizontal(_orientation == TableViewOrientation.Horizontal);
			set_vertical(!get_horizontal());
		}

		public void SetNormalizedPosition(float normalizedValue)
		{
			if (_orientation == TableViewOrientation.Horizontal)
			{
				set_horizontalNormalizedPosition(normalizedValue);
			}
			else
			{
				set_verticalNormalizedPosition(normalizedValue);
			}
		}

		private void CreateContent()
		{
			set_content(new GameObject("Table View Content", typeof(RectTransform), typeof(CanvasRenderer)).GetComponent<RectTransform>());
			get_content().SetParent(base.gameObject.GetComponent<RectTransform>(), false);
			get_content().offsetMin = Vector2.zero;
			get_content().offsetMax = Vector2.zero;
			get_content().gameObject.AddComponent<NonDrawingGraphic>();
		}

        public UnityEvent onWheel = new UnityEvent();
        public override void OnScroll(PointerEventData data)
        {
            onWheel.Invoke();
            base.OnScroll(data);
        }

        public override void OnBeginDrag(PointerEventData eventData)
		{
			base.OnBeginDrag(eventData);
			onDragBegin.Invoke();
		}

		public override void OnEndDrag(PointerEventData eventData)
		{
			base.OnEndDrag(eventData);
			onDragEnd.Invoke();
		}
	}
}
