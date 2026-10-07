using System.Collections.Generic;

public class Attributes
{
	private Dictionary<string, int> values;

	public int Count
	{
		get
		{
			return GetCount();
		}
	}

	public Attributes()
	{
		values = new Dictionary<string, int>();
	}

	public Attributes(Attributes NOLFMPDGCOC)
	{
		values = new Dictionary<string, int>();
		foreach (KeyValuePair<string, int> item in NOLFMPDGCOC.values)
		{
			values.Add(item.Key, item.Value);
		}
	}

	public int GetCount()
	{
		return values.Count;
	}

	public void AddRange(Attributes NOLFMPDGCOC)
	{
		foreach (KeyValuePair<string, int> item in NOLFMPDGCOC.values)
		{
			values[item.Key] = item.Value;
		}
	}

	public bool Get(string KGBGENDIMBC, ref int OEMALIFPGPO, bool PIDPHPGMLOD = true, bool OMHMHCLDBFA = false)
	{
		if (OMHMHCLDBFA)
		{
			Aspect hOHAPDGFMHL = GameUtils.GetAspectByName(KGBGENDIMBC);
			if (hOHAPDGFMHL != null)
			{
				return Get(hOHAPDGFMHL.GetAttribute(), ref OEMALIFPGPO, PIDPHPGMLOD, OMHMHCLDBFA);
			}
		}
		if (KGBGENDIMBC != null && values.ContainsKey(KGBGENDIMBC))
		{
			int num = values[KGBGENDIMBC];
			if (PIDPHPGMLOD)
			{
				Aspect hOHAPDGFMHL2 = GameUtils.GetAspectByName(KGBGENDIMBC);
				if (hOHAPDGFMHL2 != null)
				{
					int gNLOCMLBNHF = ListSF.GetRoster().GetLevel();
					int bIJKNKAJBHH = GameUtils.GetAspectDoublingRange().GetLevelStep();
					float num2 = GameUtils.GetAspectDoublingRange().GetValue();
					OEMALIFPGPO = hOHAPDGFMHL2.GetValue(num, gNLOCMLBNHF, bIJKNKAJBHH, num2);
					if (values.ContainsKey(hOHAPDGFMHL2.GetAttribute()))
					{
						num = values[hOHAPDGFMHL2.GetAttribute()];
						OEMALIFPGPO += num;
					}
					return true;
				}
			}
			OEMALIFPGPO = num;
			return true;
		}
		return false;
	}

	public void Set(string KGBGENDIMBC, int value, bool OMHMHCLDBFA = false)
	{
		if (OMHMHCLDBFA)
		{
			Aspect hOHAPDGFMHL = GameUtils.GetAspectByName(KGBGENDIMBC);
			if (hOHAPDGFMHL != null)
			{
				string key = hOHAPDGFMHL.GetAttribute();
				values[key] = value;
				return;
			}
		}
		values[KGBGENDIMBC] = value;
	}

	public void Clear()
	{
		values.Clear();
	}
}
