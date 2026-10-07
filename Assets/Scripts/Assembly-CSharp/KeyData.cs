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

		public Distances(Distances NBMGOEMJJAF)
		{
			NearRange = NBMGOEMJJAF.NearRange;
			MiddleRange = NBMGOEMJJAF.MiddleRange;
			FarRange = NBMGOEMJJAF.FarRange;
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

	public KeyData(KeyData NBMGOEMJJAF)
	{
		StarterKeys = new List<int>(NBMGOEMJJAF.StarterKeys);
		AdditionalKeys = new List<int>(NBMGOEMJJAF.AdditionalKeys);
		ReleaseKeys = new List<int>(NBMGOEMJJAF.ReleaseKeys);
		DistanceRanges = new Distances(NBMGOEMJJAF.DistanceRanges);
		PressTypeMode = NBMGOEMJJAF.PressTypeMode;
		IsInverted = NBMGOEMJJAF.IsInverted;
	}

	public void Set(KeyData NBMGOEMJJAF)
	{
		Clear();
		StarterKeys.Clear();
		StarterKeys.AddRange(NBMGOEMJJAF.StarterKeys);
		AdditionalKeys.AddRange(NBMGOEMJJAF.AdditionalKeys);
		ReleaseKeys.AddRange(NBMGOEMJJAF.ReleaseKeys);
		PressTypeMode = NBMGOEMJJAF.PressTypeMode;
	}

	public KeyData Copy()
	{
		return new KeyData(this);
	}

	public void Reverse(int AOJJBKLCHJO)
	{
		if (AOJJBKLCHJO < 0)
		{
			MirrorKeys(AdditionalKeys);
			MirrorKeys(StarterKeys);
			MirrorKeys(ReleaseKeys);
		}
	}

	public bool IsVariable(KeyData OKJABKNIBEO)
	{
		return CompareKeys(StarterKeys, OKJABKNIBEO.StarterKeys) && CompareKeys(AdditionalKeys, OKJABKNIBEO.AdditionalKeys) && CompareKeys(ReleaseKeys, OKJABKNIBEO.ReleaseKeys);
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
			int mJGKGLGJHHK = StarterKeys[i];
			text += KeyToString(mJGKGLGJHHK);
			if (i < count - 1)
			{
				text += " + ";
			}
		}
		text += " ADD: ";
		int j = 0;
		for (int count2 = AdditionalKeys.Count; j < count2; j++)
		{
			int mJGKGLGJHHK2 = AdditionalKeys[j];
			text += KeyToString(mJGKGLGJHHK2);
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
	public static bool AreEqual(KeyData LHBNIMGFKIB, KeyData AAOIAEJJINO)
	{
		if (LHBNIMGFKIB.IsInverted == AAOIAEJJINO.IsInverted && LHBNIMGFKIB.PressTypeMode == AAOIAEJJINO.PressTypeMode && LHBNIMGFKIB.StarterKeys.Count == AAOIAEJJINO.StarterKeys.Count && LHBNIMGFKIB.AdditionalKeys.Count == AAOIAEJJINO.AdditionalKeys.Count && LHBNIMGFKIB.StarterKeys == AAOIAEJJINO.StarterKeys && LHBNIMGFKIB.AdditionalKeys == AAOIAEJJINO.AdditionalKeys)
		{
			return true;
		}
		return false;
	}

	[SpecialName]
	public static bool AreNotEqual(KeyData LHBNIMGFKIB, KeyData AAOIAEJJINO)
	{
		return !AreEqual(LHBNIMGFKIB, AAOIAEJJINO);
	}

	private static void MirrorKeys(List<int> EGJHGBCEPHO)
	{
		for (int i = 0; i < EGJHGBCEPHO.Count; i++)
		{
			switch ((FightCID)EGJHGBCEPHO[i])
			{
			case FightCID.QuadrantUpForward:
				EGJHGBCEPHO[i] = 8;
				break;
			case FightCID.QuadrantForward:
				EGJHGBCEPHO[i] = 7;
				break;
			case FightCID.QuadrantDownForward:
				EGJHGBCEPHO[i] = 6;
				break;
			case FightCID.QuadrantDownBack:
				EGJHGBCEPHO[i] = 4;
				break;
			case FightCID.QuadrantBack:
				EGJHGBCEPHO[i] = 3;
				break;
			case FightCID.QuadrantUpBack:
				EGJHGBCEPHO[i] = 2;
				break;
			}
		}
	}

	private static bool CompareKeys(List<int> BMKNHNOGIHO, List<int> GDOOLJGKOMG)
	{
		return GDOOLJGKOMG.ContainsAllItems(BMKNHNOGIHO);
	}

	private static string KeyToString(int MJGKGLGJHHK)
	{
		switch ((FightCID)MJGKGLGJHHK)
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
