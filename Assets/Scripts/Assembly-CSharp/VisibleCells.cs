using System.Collections.Generic;
using System.Diagnostics;
using Nekki.SF2.GUI;
using UnityEngine.SocialPlatforms;

public class VisibleCells
{
	public Range IndexesRange;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Dictionary<int, TableViewCell> cells;

	public Dictionary<int, TableViewCell> Cells
	{
		get
		{
			return GetCells();
		}
		private set
		{
			set_cells(value);
		}
	}

	public int Count
	{
		get
		{
			return GetCount();
		}
	}

	public VisibleCells()
	{
		IndexesRange = new Range(0, 0);
		set_cells(new Dictionary<int, TableViewCell>());
	}

	public Dictionary<int, TableViewCell> GetCells()
	{
		return cells;
	}

	private void set_cells(Dictionary<int, TableViewCell> value)
	{
		cells = value;
	}

	public int GetCount()
	{
		return GetCells().Count;
	}

	public TableViewCell GetCellAtIndex(int index)
	{
		TableViewCell value = null;
		GetCells().TryGetValue(index, out value);
		return value;
	}

	public void SetCellAtIndex(int index, TableViewCell cell)
	{
		GetCells()[index] = cell;
	}

	public void RemoveCellAtIndex(int index)
	{
		GetCells().Remove(index);
	}
}
