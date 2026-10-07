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

	public void SetRowSize(float PEEOEOMEBFG, int IBAKGENOEPH)
	{
		if (!(PEEOEOMEBFG <= 0f) && IBAKGENOEPH < GetRowCount())
		{
			_rowSizes[IBAKGENOEPH] = PEEOEOMEBFG;
		}
	}

	public float GetRowSize(int IBAKGENOEPH)
	{
		if (IBAKGENOEPH < 0)
		{
			return 0f;
		}
		return _rowSizes[IBAKGENOEPH];
	}

	public float SumWithRange(Range JMPCNIOBPAI)
	{
		if (JMPCNIOBPAI.count == 0)
		{
			return 0f;
		}
		return GetCumulativeSize(JMPCNIOBPAI.from + JMPCNIOBPAI.count - 1) - GetCumulativeSize(JMPCNIOBPAI.from - 1);
	}

	public float GetCumulativeSize(int IBAKGENOEPH)
	{
		if (IBAKGENOEPH < 0)
		{
			return 0f;
		}
		while (CumulativeIndex < IBAKGENOEPH)
		{
			CumulativeIndex++;
			_cumulativeSizes[CumulativeIndex] = _rowSizes[CumulativeIndex];
			if (CumulativeIndex > 0)
			{
				_cumulativeSizes[CumulativeIndex] += _spacing;
				_cumulativeSizes[CumulativeIndex] += _cumulativeSizes[CumulativeIndex - 1];
			}
		}
		return _cumulativeSizes[IBAKGENOEPH];
	}
}
