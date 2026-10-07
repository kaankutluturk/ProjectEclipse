using System.Collections.Generic;

public class IntervalNew
{
	public InfoAnimation Animation;

	public List<float> Distances = new List<float>();

	public List<int> Interframes = new List<int>();

	public bool IsDistanceWithin(float distance)
	{
		int num = Distances.Count - 1;
		if (0 < num)
		{
			return Distances[0] <= distance && distance < Distances[num];
		}
		return false;
	}

	public int GetInterframeByDistance(float distance)
	{
		if (IsDistanceWithin(distance))
		{
			int i = 1;
			for (int count = Distances.Count; i < count; i++)
			{
				if (distance < Distances[i])
				{
					return Interframes[i - 1];
				}
			}
		}
		return -1;
	}
}
