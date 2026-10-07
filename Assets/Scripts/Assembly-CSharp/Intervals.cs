using System.Collections.Generic;

public class Intervals
{
	public List<IntervalNew> Items = new List<IntervalNew>();

	public int AddAnimationFromDistance(float distance, List<InfoAnimation> animations)
	{
		int num = 0;
		for (int i = 0; i < Items.Count; i++)
		{
			int num2 = Items[i].GetInterframeByDistance(distance);
			if (0 < num2)
			{
				if (num2 != 1)
				{
				}
				animations.Add(Items[i].Animation);
				num++;
			}
		}
		return num;
	}
}
