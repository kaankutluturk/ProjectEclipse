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

	public Attributes(Attributes other)
	{
		values = new Dictionary<string, int>();
		foreach (KeyValuePair<string, int> item in other.values)
		{
			values.Add(item.Key, item.Value);
		}
	}

	public int GetCount()
	{
		return values.Count;
	}

	public void AddRange(Attributes other)
	{
		foreach (KeyValuePair<string, int> item in other.values)
		{
			values[item.Key] = item.Value;
		}
	}

	public bool Get(string name, ref int result, bool applyAspectBonus = true, bool isAspectName = false)
	{
		if (isAspectName)
		{
			Aspect aspect = GameUtils.GetAspectByName(name);
			if (aspect != null)
			{
				return Get(aspect.GetAttribute(), ref result, applyAspectBonus, isAspectName);
			}
		}
		if (name != null && values.ContainsKey(name))
		{
			int num = values[name];
			if (applyAspectBonus)
			{
				Aspect hOHAPDGFMHL2 = GameUtils.GetAspectByName(name);
				if (hOHAPDGFMHL2 != null)
				{
					int level = ListSF.GetRoster().GetLevel();
					int levelStep = GameUtils.GetAspectDoublingRange().GetLevelStep();
					float num2 = GameUtils.GetAspectDoublingRange().GetValue();
					result = hOHAPDGFMHL2.GetValue(num, level, levelStep, num2);
					if (values.ContainsKey(hOHAPDGFMHL2.GetAttribute()))
					{
						num = values[hOHAPDGFMHL2.GetAttribute()];
						result += num;
					}
					return true;
				}
			}
			result = num;
			return true;
		}
		return false;
	}

	public void Set(string name, int value, bool isAspectName = false)
	{
		if (isAspectName)
		{
			Aspect aspect = GameUtils.GetAspectByName(name);
			if (aspect != null)
			{
				string key = aspect.GetAttribute();
				values[key] = value;
				return;
			}
		}
		values[name] = value;
	}

	public void Clear()
	{
		values.Clear();
	}
}
