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

	public AttributesAlign(AttributesAlign NBMGOEMJJAF)
	{
		Factor = NBMGOEMJJAF.Factor;
		Shift = NBMGOEMJJAF.Shift;
		Priority = NBMGOEMJJAF.Priority;
		DifficultyFilter = NBMGOEMJJAF.DifficultyFilter;
	}

	public static int GetMaxPriority(List<AttributesAlign> JPJIIDGEODE)
	{
		int num = int.MinValue;
		for (int i = 0; i < JPJIIDGEODE.Count; i++)
		{
			if (num < JPJIIDGEODE[i].Priority)
			{
				num = JPJIIDGEODE[i].Priority;
			}
		}
		return num;
	}

	public static int AppendHighestPriority(List<AttributesAlign> MHDPIEJEKIP, List<AttributesAlign> PNKJPOHEOJB)
	{
		int count = PNKJPOHEOJB.Count;
		int num = GetMaxPriority(MHDPIEJEKIP);
		for (int i = 0; i < MHDPIEJEKIP.Count; i++)
		{
			if (MHDPIEJEKIP[i].Priority == num)
			{
				PNKJPOHEOJB.Add(MHDPIEJEKIP[i]);
			}
		}
		return PNKJPOHEOJB.Count - count;
	}
}
