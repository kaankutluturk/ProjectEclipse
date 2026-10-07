using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class KeyData
{
	public enum PressType
	{
		BOTH = 0,
		SEQUENCE = 1
	}

	public class DistanceRange
	{
		public float Min;

		public float Max;
	}

	public class Distances
	{
		public DistanceRange NearRange;

		public DistanceRange MiddleRange;

		public DistanceRange FarRange;

		public Distances()
		{
		}

		public Distances(Distances other)
		{
			NearRange = other.NearRange;
			MiddleRange = other.MiddleRange;
			FarRange = other.FarRange;
		}
	}

	public List<int> StarterKeys = new List<int>();

	public List<int> AdditionalKeys = new List<int>();

	public List<int> ReleaseKeys = new List<int>();

	public Distances DistanceRanges = new Distances();

	public PressType PressTypeMode;

	public bool IsInverted;

	public KeyData()
	{
		IsInverted = false;
		PressTypeMode = PressType.BOTH;
	}

	public KeyData(KeyData other)
	{
		StarterKeys = new List<int>(other.StarterKeys);
		AdditionalKeys = new List<int>(other.AdditionalKeys);
		ReleaseKeys = new List<int>(other.ReleaseKeys);
		DistanceRanges = new Distances(other.DistanceRanges);
		PressTypeMode = other.PressTypeMode;
		IsInverted = other.IsInverted;
	}

	public void Set(KeyData other)
	{
		Clear();
		StarterKeys.Clear();
		StarterKeys.AddRange(other.StarterKeys);
		AdditionalKeys.AddRange(other.AdditionalKeys);
		ReleaseKeys.AddRange(other.ReleaseKeys);
		PressTypeMode = other.PressTypeMode;
	}

	public KeyData Copy()
	{
		return new KeyData(this);
	}

	public void Reverse(int direction)
	{
		if (direction < 0)
		{
			MirrorKeys(AdditionalKeys);
			MirrorKeys(StarterKeys);
			MirrorKeys(ReleaseKeys);
		}
	}

	public bool IsVariable(KeyData other)
	{
		return CompareKeys(StarterKeys, other.StarterKeys) && CompareKeys(AdditionalKeys, other.AdditionalKeys) && CompareKeys(ReleaseKeys, other.ReleaseKeys);
	}

	public void Clear()
	{
		AdditionalKeys.Clear();
		ReleaseKeys.Clear();
		PressTypeMode = PressType.BOTH;
	}

	public string ToDebugString()
	{
		if (StarterKeys.Count == 0 && AdditionalKeys.Count == 0)
		{
			return string.Empty;
		}
		int num = 0;
		string empty = string.Empty;
		string text = "KeyData: ";
		text += "starter:";
		foreach (int item in StarterKeys)
		{
			empty = ((num < StarterKeys.Count - 1) ? "," : string.Empty);
			text = text + item + empty;
			num++;
		}
		num = 0;
		text += " additional:";
		foreach (int item2 in AdditionalKeys)
		{
			empty = ((num < AdditionalKeys.Count - 1) ? "," : string.Empty);
			text = text + item2 + empty;
			num++;
		}
		text += " pressType:";
		switch (PressTypeMode)
		{
		case PressType.BOTH:
			return text + "BOTH";
		case PressType.SEQUENCE:
			return text + "SEQUENCE";
		default:
			return text + "? ";
		}
	}

	public string ToDisplayString()
	{
		string text = string.Empty;
		int i = 0;
		for (int count = StarterKeys.Count; i < count; i++)
		{
			int key = StarterKeys[i];
			text += KeyToString(key);
			if (i < count - 1)
			{
				text += " + ";
			}
		}
		text += " ADD: ";
		int j = 0;
		for (int count2 = AdditionalKeys.Count; j < count2; j++)
		{
			int keyCode = AdditionalKeys[j];
			text += KeyToString(keyCode);
			if (j < count2 - 1)
			{
				text += " + ";
			}
		}
		text += " PressType: ";
		switch (PressTypeMode)
		{
		case PressType.BOTH:
			text += "Both";
			break;
		case PressType.SEQUENCE:
			text += "Sequence";
			break;
		}
		return text;
	}

	public void ResetPressType()
	{
		if (StarterKeys.Count == 1)
		{
			PressTypeMode = PressType.BOTH;
			return;
		}
		foreach (int item in StarterKeys)
		{
			foreach (int item2 in AdditionalKeys)
			{
				if (item == item2)
				{
					PressTypeMode = PressType.BOTH;
					return;
				}
			}
		}
		PressTypeMode = PressType.SEQUENCE;
	}

	[SpecialName]
	public static bool AreEqual(KeyData left, KeyData right)
	{
		if (left.IsInverted == right.IsInverted && left.PressTypeMode == right.PressTypeMode && left.StarterKeys.Count == right.StarterKeys.Count && left.AdditionalKeys.Count == right.AdditionalKeys.Count && left.StarterKeys == right.StarterKeys && left.AdditionalKeys == right.AdditionalKeys)
		{
			return true;
		}
		return false;
	}

	[SpecialName]
	public static bool AreNotEqual(KeyData left, KeyData right)
	{
		return !AreEqual(left, right);
	}

	private static void MirrorKeys(List<int> keys)
	{
		for (int i = 0; i < keys.Count; i++)
		{
			switch ((FightCID)keys[i])
			{
			case FightCID.QuadrantUpForward:
				keys[i] = 8;
				break;
			case FightCID.QuadrantForward:
				keys[i] = 7;
				break;
			case FightCID.QuadrantDownForward:
				keys[i] = 6;
				break;
			case FightCID.QuadrantDownBack:
				keys[i] = 4;
				break;
			case FightCID.QuadrantBack:
				keys[i] = 3;
				break;
			case FightCID.QuadrantUpBack:
				keys[i] = 2;
				break;
			}
		}
	}

	private static bool CompareKeys(List<int> requiredKeys, List<int> availableKeys)
	{
		return availableKeys.ContainsAllItems(requiredKeys);
	}

	private static string KeyToString(int key)
	{
		switch ((FightCID)key)
		{
		case FightCID.QuadrantUp:
			return "U";
		case FightCID.QuadrantUpForward:
			return "UF";
		case FightCID.QuadrantForward:
			return "F";
		case FightCID.QuadrantDownForward:
			return "DF";
		case FightCID.QuadrantDown:
			return "D";
		case FightCID.QuadrantDownBack:
			return "DB";
		case FightCID.QuadrantBack:
			return "B";
		case FightCID.QuadrantUpBack:
			return "UB";
		case FightCID.Punch:
			return "Punch";
		case FightCID.Kick:
			return "Kick";
		case FightCID.MissileButton:
			return "Throw";
		case FightCID.MagicButton:
			return "Magic";
		case FightCID.RaidChargeButton:
			return "RaidCharge";
		default:
			return string.Empty;
		}
	}
}
