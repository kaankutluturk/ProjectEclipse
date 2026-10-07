using System.Collections.Generic;

public class Intervals
{
	public List<IntervalNew> Items = new List<IntervalNew>();

	public int AddAnimationFromDistance(float OIOMNNFMDOO, List<InfoAnimation> OEMALIFPGPO)
	{
		int num = 0;
		for (int i = 0; i < Items.Count; i++)
		{
			int num2 = Items[i].GetInterframeByDistance(OIOMNNFMDOO);
			if (0 < num2)
			{
				if (num2 != 1)
				{
				}
				OEMALIFPGPO.Add(Items[i].Animation);
				num++;
			}
		}
		return num;
	}
}
