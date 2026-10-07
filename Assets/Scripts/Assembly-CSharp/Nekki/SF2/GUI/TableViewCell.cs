using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nekki.SF2.GUI
{
	public abstract class TableViewCell : SFMonoBehaviour<object>, IPointerClickHandler, IEventSystemHandler, ISelectHandler, ISubmitHandler
	{
		public TableViewCellDidSelectEvent DidSelectEvent;

		public TableViewCellDidHighlightEvent DidHighlightEvent;

		private int _rowNumber;

		public int RowIndex
		{
			get
			{
				return get_RowNumber();
			}
			set
			{
				set_RowNumber(value);
			}
		}

		public int get_RowNumber()
		{
			return _rowNumber;
		}

		public void set_RowNumber(int value)
		{
			_rowNumber = value;
			Display();
		}

		private void Awake()
		{
			base.gameObject.AddComponent<Selectable>();
		}

		public abstract void SetHighlighted();

		public abstract void SetSelected();

		public abstract void Display();

		public void OnSelect(BaseEventData BHOLFGOGPCP)
		{
			SetHighlighted();
			if (DidHighlightEvent != null)
			{
				DidHighlightEvent.Invoke(_rowNumber);
			}
		}

		public void OnSubmit(BaseEventData BHOLFGOGPCP)
		{
			SetSelected();
			if (DidSelectEvent != null)
			{
				DidSelectEvent.Invoke(_rowNumber);
			}
		}

		public void OnPointerClick(PointerEventData BHOLFGOGPCP)
		{
			SetSelected();
			if (DidSelectEvent != null)
			{
				DidSelectEvent.Invoke(_rowNumber);
			}
		}

		private void OnDestroy()
		{
			DidSelectEvent.RemoveAllListeners();
			DidHighlightEvent.RemoveAllListeners();
		}
	}
}
