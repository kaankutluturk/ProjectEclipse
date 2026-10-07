using System.Collections.Generic;

public class TacticalTable
{
	public List<Intervals> IntervalList = new List<Intervals>();

	public string Label = string.Empty;

	public int FirstFrameIndex;

	public int LastFrameIndex
	{
		get
		{
			return get_MaxFrame();
		}
	}

	public int get_MaxFrame()
	{
		return FirstFrameIndex + IntervalList.Count - 1;
	}

	private static void Load(string PMFEIPCHENB)
	{
	}

	public Intervals GetFrameByFrameIndex(int FMNGLKIGFNA)
	{
		int num = GetArrayIndexByFrameIndex(FMNGLKIGFNA);
		if (-1 < num)
		{
			return IntervalList[num];
		}
		return null;
	}

	public int GetArrayIndexByFrameIndex(int FMNGLKIGFNA)
	{
		int num = FMNGLKIGFNA - FirstFrameIndex;
		if (num < IntervalList.Count)
		{
			return num;
		}
		return -1;
	}
}
