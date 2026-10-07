using System.Collections.Generic;

public class DetailedDamages
{
	public Dictionary<string, Dictionary<string, float>> DamagesByType = new Dictionary<string, Dictionary<string, float>>();

	public void Add(float CKKFKEIELCP, string BBNKIBKPBLO, string target)
	{
		if (!DamagesByType.ContainsKey(BBNKIBKPBLO))
		{
			DamagesByType[BBNKIBKPBLO] = new Dictionary<string, float>();
		}
		Dictionary<string, float> dictionary = DamagesByType[BBNKIBKPBLO];
		bool flag = dictionary.ContainsKey(target);
		dictionary[target] = ((!flag) ? CKKFKEIELCP : (dictionary[target] + CKKFKEIELCP));
	}

	public void Merge(DetailedDamages NOLFMPDGCOC)
	{
		foreach (KeyValuePair<string, Dictionary<string, float>> item in NOLFMPDGCOC.DamagesByType)
		{
			foreach (KeyValuePair<string, float> item2 in item.Value)
			{
				Add(item2.Value, item.Key, item2.Key);
			}
		}
	}

	public float GetTotalDamage()
	{
		float num = 0f;
		foreach (KeyValuePair<string, Dictionary<string, float>> item in DamagesByType)
		{
			foreach (KeyValuePair<string, float> item2 in item.Value)
			{
				num += item2.Value;
			}
		}
		return num;
	}

	public float GetRaidChargeDamage()
	{
		float num = 0f;
		foreach (KeyValuePair<string, Dictionary<string, float>> item in DamagesByType)
		{
			if (!item.Key.Equals("RaidChargeDamage"))
			{
				continue;
			}
			foreach (KeyValuePair<string, float> item2 in item.Value)
			{
				num += item2.Value;
			}
		}
		return num;
	}
}
