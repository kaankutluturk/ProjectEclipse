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

		public void Init(ITableViewDataSource PHPCFCPCOAG, ITableViewDelegate CGPBNFFLLDK)
		{
			_dataSource = PHPCFCPCOAG;
			_delegate = CGPBNFFLLDK;
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

		public TableViewCell ReusableCellForRow(int IBAKGENOEPH)
		{
			TableViewCell tableViewCell = _reusableCells.TakeCell();
			if (tableViewCell == null)
			{
				tableViewCell = InstantiateCell(IBAKGENOEPH);
			}
			return tableViewCell;
		}

		public TableViewCell CellForRow(int IBAKGENOEPH)
		{
			return get_visibleCells().GetCellAtIndex(IBAKGENOEPH);
		}

		public float PositionForRow(int IBAKGENOEPH)
		{
			if (IBAKGENOEPH < 0 || IBAKGENOEPH > NumberOfRows() - 1)
			{
				return 0f;
			}
			return _cellSizes.GetCumulativeSize(IBAKGENOEPH) - _cellSizes.GetRowSize(IBAKGENOEPH) / 2f + _leadingPadding;
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
					float pEEOEOMEBFG = _dataSource.SizeForRowInTableView(this, i);
					_cellSizes.SetRowSize(pEEOEOMEBFG, i);
				}
				tableViewScroll.set_SizeDelta(_leadingPadding + _cellSizes.GetCumulativeSize(num - 1) + _trailingPadding);
				RebuildVisibleCells();
				_needsReload = false;
			}
		}

		public void ScrollToCell(int IBAKGENOEPH, float time = 0f)
		{
			float fFMJGKPCBNK = PositionForRow(IBAKGENOEPH);
			SetPosition(fFMJGKPCBNK, time);
		}

		public void SetPosition(float FFMJGKPCBNK, float time = 0f)
		{
			KillTween();
			if (_isDragging)
			{
				return;
			}
			if (!base.gameObject.activeSelf || time <= 0f)
			{
				SetPosition(FFMJGKPCBNK);
				return;
			}
			_tween = DOTween.To(() => _currentPosition, (float DHDMNHCIPEH) =>
			{
				SetPosition(DHDMNHCIPEH);
			}, FFMJGKPCBNK, time);
		}

		private void SetPosition(float FFMJGKPCBNK)
		{
			if (!_isEmpty)
			{
				FFMJGKPCBNK = Mathf.Clamp(FFMJGKPCBNK, PositionForRow(0), PositionForRow(_cellSizes.GetRowCount() - 1));
				if (_currentPosition != FFMJGKPCBNK)
				{
					_needsCellUpdate = true;
					_currentPosition = FFMJGKPCBNK;
					float num = FFMJGKPCBNK - GetViewportExtent() / 2f;
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

		private TableViewCell InstantiateCell(int IBAKGENOEPH)
		{
			if (get_CellPrefab() == null)
			{
				return null;
			}
			TableViewCell component = UnityEngine.Object.Instantiate(get_CellPrefab(), tableViewScroll.get_content(), false).GetComponent<TableViewCell>();
			component.set_RowNumber(IBAKGENOEPH);
			return ConfigureCellWithRowAtEnd(component, IBAKGENOEPH, true);
		}

		private void ScrollViewValueChanged(Vector2 PHONDPFNNGF)
		{
			float num = 0f;
			num = ((!CheckIsVertical()) ? PHONDPFNNGF.x : (1f - PHONDPFNNGF.y));
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
			float mGMMDGFPBLP = Math.Max(_currentPosition - GetViewportExtent() * 1.5f, PositionForRow(0));
			float mGMMDGFPBLP2 = Math.Min(_currentPosition + GetViewportExtent() * 1.5f, PositionForRow(_cellSizes.GetRowCount() - 1));
			int num = FindIndexOfRowAtPosition(mGMMDGFPBLP);
			int num2 = FindIndexOfRowAtPosition(mGMMDGFPBLP2);
			int valueCount = num2 - num + 1;
			return new Range(num, valueCount);
		}

		public int FindIndexOfRowAtPosition(float MGMMDGFPBLP)
		{
			return FindIndexOfRowAtPosition(MGMMDGFPBLP, 0, _cellSizes.GetRowCount() - 1);
		}

		public int FindIndexOfRowAtPosition(float MGMMDGFPBLP, int CAILGDNIKJD, int FBGEOOKNPCF)
		{
			if (CAILGDNIKJD >= FBGEOOKNPCF)
			{
				return CAILGDNIKJD;
			}
			if (FBGEOOKNPCF - CAILGDNIKJD == 1)
			{
				float num = Mathf.Abs(MGMMDGFPBLP - PositionForRow(CAILGDNIKJD));
				float num2 = Mathf.Abs(MGMMDGFPBLP - PositionForRow(FBGEOOKNPCF));
				if (num <= num2)
				{
					return CAILGDNIKJD;
				}
				return FBGEOOKNPCF;
			}
			int num3 = (CAILGDNIKJD + FBGEOOKNPCF) / 2;
			float num4 = PositionForRow(num3);
			if (num4 >= MGMMDGFPBLP)
			{
				return FindIndexOfRowAtPosition(MGMMDGFPBLP, CAILGDNIKJD, num3);
			}
			return FindIndexOfRowAtPosition(MGMMDGFPBLP, num3, FBGEOOKNPCF);
		}

		private void CreateVisibleCells()
		{
			Range bIGCGGHIPIK = CalculateVisibleRange();
			for (int i = 0; i < bIGCGGHIPIK.count; i++)
			{
				CreateCell(bIGCGGHIPIK.from + i, true);
			}
			get_visibleCells().IndexesRange = bIGCGGHIPIK;
		}

		private void UpdateVisibleCells()
		{
			_needsCellUpdate = false;
			if (!_isEmpty && !(Mathf.Abs(_currentPosition - _lastRefreshPosition) < _cellSizes.GetRowSize(0) / 2f + _spacing / 2f))
			{
				_lastRefreshPosition = _currentPosition;
				Range bIGCGGHIPIK = get_visibleCells().IndexesRange;
				Range range = CalculateVisibleRange();
				if (range.from > bIGCGGHIPIK.GetLastIndex() || range.GetLastIndex() < bIGCGGHIPIK.from)
				{
					RebuildVisibleCells();
				}
				else if (!bIGCGGHIPIK.Equals(range))
				{
					RecycleCellsOutsideRange(bIGCGGHIPIK, range);
					CreateCellsInNewRange(bIGCGGHIPIK, range);
					get_visibleCells().IndexesRange = range;
				}
			}
		}

		private void RecycleCellsOutsideRange(Range IKJKAMKCCMB, Range MHEKHCKHNLG)
		{
			for (int i = IKJKAMKCCMB.from; i < MHEKHCKHNLG.from; i++)
			{
				MoveCellToReusable(false);
			}
			for (int j = MHEKHCKHNLG.GetLastIndex(); j < IKJKAMKCCMB.GetLastIndex(); j++)
			{
				MoveCellToReusable(true);
			}
		}

		private void CreateCellsInNewRange(Range IKJKAMKCCMB, Range MHEKHCKHNLG)
		{
			for (int num = IKJKAMKCCMB.from - 1; num >= MHEKHCKHNLG.from; num--)
			{
				CreateCell(num, false);
			}
			for (int i = IKJKAMKCCMB.GetLastIndex() + 1; i <= MHEKHCKHNLG.GetLastIndex(); i++)
			{
				CreateCell(i, true);
			}
		}

		private void CreateCell(int IBAKGENOEPH, bool HJIIHCLNCGH)
		{
			TableViewCell hJCPCBLCJJN = _dataSource.CellForRowInTableView(this, IBAKGENOEPH);
			hJCPCBLCJJN = ConfigureCellWithRowAtEnd(hJCPCBLCJJN, IBAKGENOEPH, HJIIHCLNCGH);
		}

		private TableViewCell ConfigureCellWithRowAtEnd(TableViewCell HJCPCBLCJJN, int IBAKGENOEPH, bool HJIIHCLNCGH)
		{
			HJCPCBLCJJN.set_RowNumber(IBAKGENOEPH);
			HJCPCBLCJJN.DidHighlightEvent.RemoveListener(OnCellHighlighted);
			HJCPCBLCJJN.DidHighlightEvent.AddListener(OnCellHighlighted);
			HJCPCBLCJJN.DidSelectEvent.RemoveListener(OnCellSelectedByUser);
			HJCPCBLCJJN.DidSelectEvent.AddListener(OnCellSelectedByUser);
			get_visibleCells().SetCellAtIndex(IBAKGENOEPH, HJCPCBLCJJN);
			if (HJIIHCLNCGH)
			{
				HJCPCBLCJJN.transform.SetSiblingIndex(tableViewScroll.get_content().childCount - 1);
			}
			else
			{
				HJCPCBLCJJN.transform.SetSiblingIndex(0);
			}
			if (!CheckIsVertical())
			{
				HJCPCBLCJJN.transform.SetLocalX(0f - PositionForRow(IBAKGENOEPH));
			}
			else
			{
				HJCPCBLCJJN.transform.SetLocalY(0f - PositionForRow(IBAKGENOEPH));
			}
			return HJCPCBLCJJN;
		}

		private void MoveCellToReusable(bool IBMGAPMHMOB)
		{
			int num = ((!IBMGAPMHMOB) ? get_visibleCells().IndexesRange.from : get_visibleCells().IndexesRange.GetLastIndex());
			TableViewCell tableViewCell = get_visibleCells().GetCellAtIndex(num);
			tableViewCell.DidHighlightEvent.RemoveAllListeners();
			tableViewCell.DidSelectEvent.RemoveAllListeners();
			_reusableCells.RecycleCell(tableViewCell);
			get_visibleCells().RemoveCellAtIndex(num);
			get_visibleCells().IndexesRange.count--;
			if (!IBMGAPMHMOB)
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

		private void OnCellHighlighted(int IBAKGENOEPH)
		{
			if (_delegate != null)
			{
				_delegate.TableViewDidHighlightCellForRow(this, IBAKGENOEPH);
			}
			if (!scrollToHighlighted)
			{
			}
		}

		private void OnCellSelectedByUser(int IBAKGENOEPH)
		{
			if (_delegate != null)
			{
				_delegate.TableViewDidSelectCellForRow(this, IBAKGENOEPH);
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

		public int GetNearestCellRow(float GHGLPGGMDNP)
		{
			TableViewCell selectedCell = get_SelectedCell();
			int rowNumber = selectedCell.get_RowNumber();
			if (GHGLPGGMDNP == 0f)
			{
				return rowNumber;
			}
			float num = _currentPosition + GHGLPGGMDNP;
			int result = rowNumber;
			float num2 = PositionForRow(rowNumber);
			if (GHGLPGGMDNP > 0f)
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
				int iBAKGENOEPH = get_SelectedCell().get_RowNumber();
				if (Math.Abs(num) >= Math.Abs(get_MinScrollVelocity() / 2f))
				{
					iBAKGENOEPH = GetNearestCellRow(num);
				}
				float speed = tableViewScroll.get_velocity().magnitude;
                tableViewScroll.set_velocity(default(Vector2));
				float aFHNFJLOGIC = Mathf.Clamp(Mathf.Abs(num) / speed, .1f, .5f);
				ScrollToCell(iBAKGENOEPH, aFHNFJLOGIC);
			}
		}
	}
}
