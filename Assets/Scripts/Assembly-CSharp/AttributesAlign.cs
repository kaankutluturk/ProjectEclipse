using System.Collections.Generic;

public class AttributesAlign
{
	public float Factor;

	public float Shift;

	public int Priority;

	public ModelParameters.DifficultyFilter DifficultyFilter;

	public AttributesAlign()
	{
		Factor = 0f;
		Shift = 0f;
		Priority = 0;
		DifficultyFilter = ModelParameters.DifficultyFilter.DFBoth;
	}

	public AttributesAlign(AttributesAlign other)
	{
		Factor = other.Factor;
		Shift = other.Shift;
		Priority = other.Priority;
		DifficultyFilter = other.DifficultyFilter;
	}

	public static int GetMaxPriority(List<AttributesAlign> alignments)
	{
		int num = int.MinValue;
		for (int i = 0; i < alignments.Count; i++)
		{
			if (num < alignments[i].Priority)
			{
				num = alignments[i].Priority;
			}
		}
		return num;
	}

	public static int AppendHighestPriority(List<AttributesAlign> source, List<AttributesAlign> destination)
	{
		int count = destination.Count;
		int num = GetMaxPriority(source);
		for (int i = 0; i < source.Count; i++)
		{
			if (source[i].Priority == num)
			{
				destination.Add(source[i]);
			}
		}
		return destination.Count - count;
	}
}
