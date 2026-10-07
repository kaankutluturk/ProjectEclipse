using System;
using System.Collections.Generic;
using System.Diagnostics;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;
using Range = UnityEngine.SocialPlatforms.Range;
using UnityEngine.UI;

namespace Nekki.SF2.GUI
{
	public class TableView : MonoBehaviour, ITableView
	{
		[Serializable]
		public class SelectCellEvent : UnityEvent<TableViewCell>
		{
		}

		[SerializeField]
		private TableViewOrientation tableViewOrientation;

		[SerializeField]
		private float _spacing;

		private float _leadingPadding;

		private float _trailingPadding;

		[SerializeField]
		private RectOffset padding;

		[SerializeField]
		private bool inertia = true;

		[SerializeField]
		private float elasticity = 0.1f;

		[SerializeField]
		private float scrollSensitivity = 1f;

		[SerializeField]
		private float decelerationRate = 0.135f;

		[SerializeField]
		private bool scrollToHighlighted = true;

		private ITableViewDataSource _dataSource;

		private ITableViewDelegate _delegate;

		private GameObject _cellPrefab;

		private float _currentPosition;

		private float _lastRefreshPosition;

		private CellSizes _cellSizes;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private VisibleCells _visibleCells;

		private ReusableCellsContainer _reusableCells;

		private TableViewScroll tableViewScroll;

		private bool _isEmpty;

		private bool _needsReload;

		private bool _needsCellUpdate;

		private Tween _tween;

		private bool _isDragging;

		[SerializeField]
		public SelectCellEvent onSelectCell = new SelectCellEvent();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private TableViewCell _selectedCell;

		[SerializeField]
		private float _MinScrollVelocity;

		public float SpacingValue
		{
			get
			{
				return get_Spacing();
			}
			set
			{
				set_Spacing(value);
			}
		}

		public ITableViewDataSource TableDataSource
		{
			get
			{
				return get_DataSource();
			}
			set
			{
				set_DataSource(value);
			}
		}

		public GameObject RowCellPrefab
		{
			get
			{
				return get_CellPrefab();
			}
			set
			{
				set_CellPrefab(value);
			}
		}

		public Range VisibleRowRange
		{
			get
			{
				return get_VisibleRange();
			}
		}

		public float TotalContentSize
		{
			get
			{
				return get_ContentSize();
			}
		}

		public float ScrollPosition
		{
			get
			{
				return get_Position();
			}
		}

		public VisibleCells VisibleCellStore
		{
			get
			{
				return get_visibleCells();
			}
			private set
			{
				SetVisibleCells(value);
			}
		}

		public TableViewScroll ScrollComponent
		{
			get
			{
				return get_Scroll();
			}
		}

		private bool IsVerticalLayout
		{
			get
			{
				return CheckIsVertical();
			}
		}

		private float ViewportExtent
		{
			get
			{
				return GetViewportExtent();
			}
		}

		public TableViewCell CurrentCell
		{
			get
			{
				return get_SelectedCell();
			}
			protected set
			{
				set_SelectedCell(value);
			}
		}

		public float MinScrollVelocityValue
		{
			get
			{
				return get_MinScrollVelocity();
			}
			set
			{
				set_MinScrollVelocity(value);
			}
		}

		public float get_Spacing()
		{
			return _spacing;
		}

		public void set_Spacing(float value)
		{
			_spacing = value;
			if (_cellSizes != null)
			{
				_cellSizes.set_Spacing(_spacing);
			}
		}

		public ITableViewDataSource get_DataSource()
		{
			return _dataSource;
		}

		public void set_DataSource(ITableViewDataSource value)
		{
			_dataSource = value;
			_needsReload = true;
		}

		public ITableViewDelegate Delegate
		{
			get
			{
				return _delegate;
			}
			set
			{
				_delegate = value;
			}
		}

		public GameObject get_CellPrefab()
		{
			return _cellPrefab;
		}

		public void set_CellPrefab(GameObject value)
		{
			_cellPrefab = value;
		}

		public Range get_VisibleRange()
		{
			return get_visibleCells().IndexesRange;
		}

		public float get_ContentSize()
		{
			return tableViewScroll.get_Size() - GetViewportExtent();
		}

		public float get_Position()
		{
			return _currentPosition;
		}

		public VisibleCells get_visibleCells()
		{
			return _visibleCells;
		}

		private void SetVisibleCells(VisibleCells value)
		{
			_visibleCells = value;
		}

		public TableViewScroll get_Scroll()
		{
			return tableViewScroll;
		}

		private bool CheckIsVertical()
		{
			return tableViewOrientation == TableViewOrientation.Vertical;
		}

		private float GetViewportExtent()
		{
			Rect rect = (base.transform as RectTransform).rect;
			return (!CheckIsVertical()) ? rect.width : rect.height;
		}

		public void Init(ITableViewDataSource dataSource, ITableViewDelegate viewDelegate)
		{
			_dataSource = dataSource;
			_delegate = viewDelegate;
			_isEmpty = true;
			_cellSizes = new CellSizes();
			_cellSizes.set_Spacing(_spacing);
			SetVisibleCells(new VisibleCells());
			_reusableCells = new ReusableCellsContainer();
			_reusableCells.Init();
			tableViewScroll = base.gameObject.AddComponent<TableViewScroll>();
			tableViewScroll.Init();
			tableViewScroll.SetOrientation(tableViewOrientation);
			tableViewScroll.set_elasticity(elasticity);
			tableViewScroll.set_movementType(SFScrollRect.ScrollMovementType.SF2);
			tableViewScroll.set_inertia(inertia);
			tableViewScroll.set_decelerationRate(decelerationRate);
			tableViewScroll.set_scrollSensitivity(scrollSensitivity);
			tableViewScroll.get_onValueChanged().AddListener(ScrollViewValueChanged);
			tableViewScroll.onDragBegin.AddListener(HandleDragBegin);
            tableViewScroll.onWheel.AddListener(KillTween);
            Eclipse.UI.DesktopScrollbars.Attach(tableViewScroll, KillTween);
			tableViewScroll.onDragEnd.AddListener(HandleDragEnd);
			_leadingPadding = (int)(GetViewportExtent() / 2f);
			_trailingPadding = (int)(GetViewportExtent() / 2f);
			base.gameObject.AddComponent<RectMask2D>();
			base.gameObject.AddComponent<CanvasRenderer>();
			ReloadData();
		}

		private void Update()
		{
			if (_needsReload)
			{
				ReloadData();
			}
			UpdateSelectedCell();
		}

		private void LateUpdate()
		{
			if (_needsCellUpdate)
			{
				UpdateVisibleCells();
			}
		}

		public TableViewCell ReusableCellForRow(int row)
		{
			TableViewCell tableViewCell = _reusableCells.TakeCell();
			if (tableViewCell == null)
			{
				tableViewCell = InstantiateCell(row);
			}
			return tableViewCell;
		}

		public TableViewCell CellForRow(int row)
		{
			return get_visibleCells().GetCellAtIndex(row);
		}

		public float PositionForRow(int row)
		{
			if (row < 0 || row > NumberOfRows() - 1)
			{
				return 0f;
			}
			return _cellSizes.GetCumulativeSize(row) - _cellSizes.GetRowSize(row) / 2f + _leadingPadding;
		}

		public void ReloadData()
		{
			RecycleAllVisibleCells();
			set_SelectedCell(null);
			int num = NumberOfRows();
			_cellSizes.SetRowsCount(num);
			_isEmpty = num == 0;
			if (!_isEmpty)
			{
				for (int i = 0; i < num; i++)
				{
					float rowSize = _dataSource.SizeForRowInTableView(this, i);
					_cellSizes.SetRowSize(rowSize, i);
				}
				tableViewScroll.set_SizeDelta(_leadingPadding + _cellSizes.GetCumulativeSize(num - 1) + _trailingPadding);
				RebuildVisibleCells();
				_needsReload = false;
			}
		}

		public void ScrollToCell(int row, float time = 0f)
		{
			float rowPosition = PositionForRow(row);
			SetPosition(rowPosition, time);
		}

		public void SetPosition(float targetPosition, float time = 0f)
		{
			KillTween();
			if (_isDragging)
			{
				return;
			}
			if (!base.gameObject.activeSelf || time <= 0f)
			{
				SetPosition(targetPosition);
				return;
			}
			_tween = DOTween.To(() => _currentPosition, (float tweenedPosition) =>
			{
				SetPosition(tweenedPosition);
			}, targetPosition, time);
		}

		private void SetPosition(float scrollPosition)
		{
			if (!_isEmpty)
			{
				scrollPosition = Mathf.Clamp(scrollPosition, PositionForRow(0), PositionForRow(_cellSizes.GetRowCount() - 1));
				if (_currentPosition != scrollPosition)
				{
					_needsCellUpdate = true;
					_currentPosition = scrollPosition;
					float num = scrollPosition - GetViewportExtent() / 2f;
					float num2 = num / get_ContentSize();
					float num3 = 0f;
					num3 = ((!CheckIsVertical()) ? num2 : (1f - num2));
					tableViewScroll.SetNormalizedPosition(num3);
				}
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

		private TableViewCell InstantiateCell(int row)
		{
			if (get_CellPrefab() == null)
			{
				return null;
			}
			TableViewCell component = UnityEngine.Object.Instantiate(get_CellPrefab(), tableViewScroll.get_content(), false).GetComponent<TableViewCell>();
			component.set_RowNumber(row);
			return ConfigureCellWithRowAtEnd(component, row, true);
		}

		private void ScrollViewValueChanged(Vector2 scrollPosition)
		{
			float num = 0f;
			num = ((!CheckIsVertical()) ? scrollPosition.x : (1f - scrollPosition.y));
			_currentPosition = num * get_ContentSize() + GetViewportExtent() / 2f;
			_needsCellUpdate = true;
		}

		private void RebuildVisibleCells()
		{
			RecycleAllVisibleCells();
			CreateVisibleCells();
		}

		private void RecycleAllVisibleCells()
		{
			while (get_visibleCells().GetCount() > 0)
			{
				MoveCellToReusable(false);
			}
			get_visibleCells().IndexesRange = new Range(0, 0);
		}

		private void DestroyAllCells()
		{
			if (_reusableCells != null)
			{
				foreach (TableViewCell item in _reusableCells.cells)
				{
					UnityEngine.Object.Destroy(item.gameObject);
				}
				_reusableCells.cells.Clear();
			}
			if (get_visibleCells() == null)
			{
				return;
			}
			foreach (KeyValuePair<int, TableViewCell> item2 in get_visibleCells().GetCells())
			{
				UnityEngine.Object.Destroy(item2.Value.gameObject);
			}
			get_visibleCells().IndexesRange = new Range(0, 0);
			get_visibleCells().GetCells().Clear();
		}

		private Range CalculateVisibleRange()
		{
			float firstVisiblePosition = Math.Max(_currentPosition - GetViewportExtent() * 1.5f, PositionForRow(0));
			float lastVisiblePosition = Math.Min(_currentPosition + GetViewportExtent() * 1.5f, PositionForRow(_cellSizes.GetRowCount() - 1));
			int num = FindIndexOfRowAtPosition(firstVisiblePosition);
			int num2 = FindIndexOfRowAtPosition(lastVisiblePosition);
			int valueCount = num2 - num + 1;
			return new Range(num, valueCount);
		}

		public int FindIndexOfRowAtPosition(float searchPosition)
		{
			return FindIndexOfRowAtPosition(searchPosition, 0, _cellSizes.GetRowCount() - 1);
		}

		public int FindIndexOfRowAtPosition(float searchPosition, int startRow, int endRow)
		{
			if (startRow >= endRow)
			{
				return startRow;
			}
			if (endRow - startRow == 1)
			{
				float num = Mathf.Abs(searchPosition - PositionForRow(startRow));
				float num2 = Mathf.Abs(searchPosition - PositionForRow(endRow));
				if (num <= num2)
				{
					return startRow;
				}
				return endRow;
			}
			int num3 = (startRow + endRow) / 2;
			float num4 = PositionForRow(num3);
			if (num4 >= searchPosition)
			{
				return FindIndexOfRowAtPosition(searchPosition, startRow, num3);
			}
			return FindIndexOfRowAtPosition(searchPosition, num3, endRow);
		}

		private void CreateVisibleCells()
		{
			Range visibleRange = CalculateVisibleRange();
			for (int i = 0; i < visibleRange.count; i++)
			{
				CreateCell(visibleRange.from + i, true);
			}
			get_visibleCells().IndexesRange = visibleRange;
		}

		private void UpdateVisibleCells()
		{
			_needsCellUpdate = false;
			if (!_isEmpty && !(Mathf.Abs(_currentPosition - _lastRefreshPosition) < _cellSizes.GetRowSize(0) / 2f + _spacing / 2f))
			{
				_lastRefreshPosition = _currentPosition;
				Range previousRange = get_visibleCells().IndexesRange;
				Range range = CalculateVisibleRange();
				if (range.from > previousRange.GetLastIndex() || range.GetLastIndex() < previousRange.from)
				{
					RebuildVisibleCells();
				}
				else if (!previousRange.Equals(range))
				{
					RecycleCellsOutsideRange(previousRange, range);
					CreateCellsInNewRange(previousRange, range);
					get_visibleCells().IndexesRange = range;
				}
			}
		}

		private void RecycleCellsOutsideRange(Range oldRange, Range newRange)
		{
			for (int i = oldRange.from; i < newRange.from; i++)
			{
				MoveCellToReusable(false);
			}
			for (int j = newRange.GetLastIndex(); j < oldRange.GetLastIndex(); j++)
			{
				MoveCellToReusable(true);
			}
		}

		private void CreateCellsInNewRange(Range oldRange, Range newRange)
		{
			for (int num = oldRange.from - 1; num >= newRange.from; num--)
			{
				CreateCell(num, false);
			}
			for (int i = oldRange.GetLastIndex() + 1; i <= newRange.GetLastIndex(); i++)
			{
				CreateCell(i, true);
			}
		}

		private void CreateCell(int row, bool isAtEnd)
		{
			TableViewCell cell = _dataSource.CellForRowInTableView(this, row);
			cell = ConfigureCellWithRowAtEnd(cell, row, isAtEnd);
		}

		private TableViewCell ConfigureCellWithRowAtEnd(TableViewCell cell, int row, bool isAtEnd)
		{
			cell.set_RowNumber(row);
			cell.DidHighlightEvent.RemoveListener(OnCellHighlighted);
			cell.DidHighlightEvent.AddListener(OnCellHighlighted);
			cell.DidSelectEvent.RemoveListener(OnCellSelectedByUser);
			cell.DidSelectEvent.AddListener(OnCellSelectedByUser);
			get_visibleCells().SetCellAtIndex(row, cell);
			if (isAtEnd)
			{
				cell.transform.SetSiblingIndex(tableViewScroll.get_content().childCount - 1);
			}
			else
			{
				cell.transform.SetSiblingIndex(0);
			}
			if (!CheckIsVertical())
			{
				cell.transform.SetLocalX(0f - PositionForRow(row));
			}
			else
			{
				cell.transform.SetLocalY(0f - PositionForRow(row));
			}
			return cell;
		}

		private void MoveCellToReusable(bool fromEnd)
		{
			int num = ((!fromEnd) ? get_visibleCells().IndexesRange.from : get_visibleCells().IndexesRange.GetLastIndex());
			TableViewCell tableViewCell = get_visibleCells().GetCellAtIndex(num);
			tableViewCell.DidHighlightEvent.RemoveAllListeners();
			tableViewCell.DidSelectEvent.RemoveAllListeners();
			_reusableCells.RecycleCell(tableViewCell);
			get_visibleCells().RemoveCellAtIndex(num);
			get_visibleCells().IndexesRange.count--;
			if (!fromEnd)
			{
				get_visibleCells().IndexesRange.from++;
			}
			if (!CheckIsVertical())
			{
				tableViewCell.transform.SetLocalX(_cellSizes.GetRowSize(num));
			}
			else
			{
				tableViewCell.transform.SetLocalY(_cellSizes.GetRowSize(num));
			}
		}

		private void OnCellHighlighted(int row)
		{
			if (_delegate != null)
			{
				_delegate.TableViewDidHighlightCellForRow(this, row);
			}
			if (!scrollToHighlighted)
			{
			}
		}

		private void OnCellSelectedByUser(int row)
		{
			if (_delegate != null)
			{
				_delegate.TableViewDidSelectCellForRow(this, row);
			}
		}

		public TableViewCell get_SelectedCell()
		{
			return _selectedCell;
		}

		protected void set_SelectedCell(TableViewCell value)
		{
			_selectedCell = value;
		}

		public float get_MinScrollVelocity()
		{
			return _MinScrollVelocity;
		}

		public void set_MinScrollVelocity(float value)
		{
			_MinScrollVelocity = value;
		}

		private void UpdateSelectedCell()
		{
			if (get_SelectedCell() != null)
			{
				float num = Mathf.Abs(_currentPosition - PositionForRow(get_SelectedCell().get_RowNumber()));
				if (num <= _cellSizes.GetRowSize(get_SelectedCell().get_RowNumber()) / 2f + _spacing / 2f)
				{
					return;
				}
			}
			TableViewCell tableViewCell = FindNearestVisibleCell();
			if (tableViewCell != get_SelectedCell())
			{
				set_SelectedCell(tableViewCell);
				onSelectCell.Invoke(get_SelectedCell());
			}
		}

		public int GetCurrentCellRow()
		{
			if (get_SelectedCell() != null)
			{
				return get_SelectedCell().get_RowNumber();
			}
			return 0;
		}

		private TableViewCell FindNearestVisibleCell()
		{
			TableViewCell result = null;
			float num = float.MaxValue;
			foreach (KeyValuePair<int, TableViewCell> item in get_visibleCells().GetCells())
			{
				float num2 = Mathf.Abs(_currentPosition - PositionForRow(item.Key));
				if (num2 < num)
				{
					num = num2;
					result = item.Value;
				}
			}
			return result;
		}

		public int GetNearestCellRow(float offset)
		{
			TableViewCell selectedCell = get_SelectedCell();
			int rowNumber = selectedCell.get_RowNumber();
			if (offset == 0f)
			{
				return rowNumber;
			}
			float num = _currentPosition + offset;
			int result = rowNumber;
			float num2 = PositionForRow(rowNumber);
			if (offset > 0f)
			{
				if (rowNumber == NumberOfRows() - 1)
				{
					return rowNumber;
				}
				for (int i = rowNumber + 1; i < NumberOfRows(); i++)
				{
					float num3 = PositionForRow(i);
					if (Mathf.Abs(num - num3) < Mathf.Abs(num - num2))
					{
						num2 = num3;
						result = i;
						continue;
					}
					break;
				}
			}
			else
			{
				if (rowNumber == 0)
				{
					return rowNumber;
				}
				int num4 = rowNumber - 1;
				while (num4 >= 0)
				{
					float num5 = PositionForRow(num4);
					if (Mathf.Abs(num - num5) < Mathf.Abs(num - num2))
					{
						num2 = num5;
						result = num4;
						num4--;
						continue;
					}
					break;
				}
			}
			return result;
		}

		public int NumberOfRows()
		{
			return _dataSource.NumberOfRowsInTableView(this);
		}

		private void HandleDragBegin()
		{
			_isDragging = true;
			KillTween();
		}

		private void HandleDragEnd()
		{
			_isDragging = false;
			if (Mathf.Abs(tableViewScroll.get_velocity().magnitude) != 0f)
			{
				float num = 0f;
				num = ((!CheckIsVertical()) ? (tableViewScroll.get_velocity().x / 2f) : (tableViewScroll.get_velocity().y / 2f));
				int nearestRow = get_SelectedCell().get_RowNumber();
				if (Math.Abs(num) >= Math.Abs(get_MinScrollVelocity() / 2f))
				{
					nearestRow = GetNearestCellRow(num);
				}
				float speed = tableViewScroll.get_velocity().magnitude;
                tableViewScroll.set_velocity(default(Vector2));
				float scrollTime = Mathf.Clamp(Mathf.Abs(num) / speed, .1f, .5f);
				ScrollToCell(nearestRow, scrollTime);
			}
		}
	}
}
