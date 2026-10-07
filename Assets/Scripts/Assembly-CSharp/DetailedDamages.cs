using System.Collections.Generic;

public class DetailedDamages
{
	public Dictionary<string, Dictionary<string, float>> DamagesByType = new Dictionary<string, Dictionary<string, float>>();

	public void Add(float amount, string damageType, string target)
	{
		if (!DamagesByType.ContainsKey(damageType))
		{
			DamagesByType[damageType] = new Dictionary<string, float>();
		}
		Dictionary<string, float> dictionary = DamagesByType[damageType];
		bool flag = dictionary.ContainsKey(target);
		dictionary[target] = ((!flag) ? amount : (dictionary[target] + amount));
	}

	public void Merge(DetailedDamages other)
	{
		foreach (KeyValuePair<string, Dictionary<string, float>> item in other.DamagesByType)
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
