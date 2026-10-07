using UnityEngine.SocialPlatforms;

public class CellSizes
{
	private float[] _rowSizes;

	private float[] _cumulativeSizes;

	public int CumulativeIndex = -1;

	private float _spacing;

	public int RowCount
	{
		get
		{
			return GetRowCount();
		}
	}

	public int CumulativeCount
	{
		get
		{
			return GetCumulativeCount();
		}
	}

	public float CellSpacing
	{
		get
		{
			return GetSpacing();
		}
		set
		{
			set_Spacing(value);
		}
	}

	public int GetRowCount()
	{
		return _rowSizes.Length;
	}

	public int GetCumulativeCount()
	{
		return _cumulativeSizes.Length;
	}

	public float GetSpacing()
	{
		return _spacing;
	}

	public void set_Spacing(float value)
	{
		_spacing = value;
	}

	public void SetRowsCount(int count)
	{
		CumulativeIndex = -1;
		_rowSizes = new float[count];
		_cumulativeSizes = new float[count];
	}

	public void SetRowSize(float size, int rowIndex)
	{
		if (!(size <= 0f) && rowIndex < GetRowCount())
		{
			_rowSizes[rowIndex] = size;
		}
	}

	public float GetRowSize(int rowIndex)
	{
		if (rowIndex < 0)
		{
			return 0f;
		}
		return _rowSizes[rowIndex];
	}

	public float SumWithRange(Range range)
	{
		if (range.count == 0)
		{
			return 0f;
		}
		return GetCumulativeSize(range.from + range.count - 1) - GetCumulativeSize(range.from - 1);
	}

	public float GetCumulativeSize(int rowIndex)
	{
		if (rowIndex < 0)
		{
			return 0f;
		}
		while (CumulativeIndex < rowIndex)
		{
			CumulativeIndex++;
			_cumulativeSizes[CumulativeIndex] = _rowSizes[CumulativeIndex];
			if (CumulativeIndex > 0)
			{
				_cumulativeSizes[CumulativeIndex] += _spacing;
				_cumulativeSizes[CumulativeIndex] += _cumulativeSizes[CumulativeIndex - 1];
			}
		}
		return _cumulativeSizes[rowIndex];
	}
}
