using System;
using System.Collections;
using System.Reflection;

[DefaultMember("Item")]
public class Parser
{
	public ArrayList NonSwitchStrings = new ArrayList();

	private SwitchResult[] _switches;

	private const char kSwitchID1 = '-';

	private const char kSwitchID2 = '/';

	private const char kSwitchMinus = '-';

	private const string kStopSwitchParsing = "--";

	// C# has no syntax for parameterized property 'DLKPBAJDHBO'.
	public SwitchResult get_DLKPBAJDHBO(int index)
	{
		return get_Item(index);
	}

	public Parser(int DJLBIKHFOFA)
	{
		_switches = new SwitchResult[DJLBIKHFOFA];
		for (int i = 0; i < DJLBIKHFOFA; i++)
		{
			_switches[i] = new SwitchResult();
		}
	}

	private bool ParseString(string BGPCMGJPELK, SwitchForm[] OKHCFPOEPGG)
	{
		int length = BGPCMGJPELK.Length;
		if (length == 0)
		{
			return false;
		}
		int num = 0;
		if (!IsItSwitchChar(BGPCMGJPELK[num]))
		{
			return false;
		}
		while (num < length)
		{
			if (IsItSwitchChar(BGPCMGJPELK[num]))
			{
				num++;
			}
			int num2 = 0;
			int num3 = -1;
			for (int i = 0; i < _switches.Length; i++)
			{
				int length2 = OKHCFPOEPGG[i].IDString.Length;
				if (length2 > num3 && num + length2 <= length && string.Compare(OKHCFPOEPGG[i].IDString, 0, BGPCMGJPELK, num, length2, true) == 0)
				{
					num2 = i;
					num3 = length2;
				}
			}
			if (num3 == -1)
			{
				throw new Exception("maxLen == kNoLen");
			}
			SwitchResult oHAKJKDHPAO = _switches[num2];
			SwitchForm pMBOOOLGPJI = OKHCFPOEPGG[num2];
			if (!pMBOOOLGPJI.Multi && oHAKJKDHPAO.ThereIs)
			{
				throw new Exception("switch must be single");
			}
			oHAKJKDHPAO.ThereIs = true;
			num += num3;
			int num4 = length - num;
			SwitchType kKJHFNGGFCG = pMBOOOLGPJI.Type;
			switch (kKJHFNGGFCG)
			{
			case SwitchType.PostMinus:
				if (num4 == 0)
				{
					oHAKJKDHPAO.WithMinus = false;
					break;
				}
				oHAKJKDHPAO.WithMinus = BGPCMGJPELK[num] == '-';
				if (oHAKJKDHPAO.WithMinus)
				{
					num++;
				}
				break;
			case SwitchType.PostChar:
			{
				if (num4 < pMBOOOLGPJI.MinLen)
				{
					throw new Exception("switch is not full");
				}
				string hKJPFMBPOJO = pMBOOOLGPJI.PostCharSet;
				if (num4 == 0)
				{
					oHAKJKDHPAO.PostCharIndex = -1;
					break;
				}
				int num6 = hKJPFMBPOJO.IndexOf(BGPCMGJPELK[num]);
				if (num6 < 0)
				{
					oHAKJKDHPAO.PostCharIndex = -1;
					break;
				}
				oHAKJKDHPAO.PostCharIndex = num6;
				num++;
				break;
			}
			case SwitchType.LimitedPostString:
			case SwitchType.UnLimitedPostString:
			{
				int dJIJIHHBHHP = pMBOOOLGPJI.MinLen;
				if (num4 < dJIJIHHBHHP)
				{
					throw new Exception("switch is not full");
				}
				if (kKJHFNGGFCG == SwitchType.UnLimitedPostString)
				{
					oHAKJKDHPAO.PostStrings.Add(BGPCMGJPELK.Substring(num));
					return true;
				}
				string text = BGPCMGJPELK.Substring(num, dJIJIHHBHHP);
				num += dJIJIHHBHHP;
				int num5 = dJIJIHHBHHP;
				while (num5 < pMBOOOLGPJI.MaxLen && num < length)
				{
					char c = BGPCMGJPELK[num];
					if (IsItSwitchChar(c))
					{
						break;
					}
					text += c;
					num5++;
					num++;
				}
				oHAKJKDHPAO.PostStrings.Add(text);
				break;
			}
			}
		}
		return true;
	}

	public void ParseStrings(SwitchForm[] OKHCFPOEPGG, string[] LPJJGICENBE)
	{
		int num = LPJJGICENBE.Length;
		bool flag = false;
		for (int i = 0; i < num; i++)
		{
			string text = LPJJGICENBE[i];
			if (flag)
			{
				NonSwitchStrings.Add(text);
			}
			else if (text == "--")
			{
				flag = true;
			}
			else if (!ParseString(text, OKHCFPOEPGG))
			{
				NonSwitchStrings.Add(text);
			}
		}
	}

	public SwitchResult get_Item(int index)
	{
		return _switches[index];
	}

	public static int ParseCommand(CommandForm[] JHOPHFBKNAI, string FGNIBFLIOCO, out string KKKODGNDMKM)
	{
		for (int i = 0; i < JHOPHFBKNAI.Length; i++)
		{
			string aAEEJJMOFIL = JHOPHFBKNAI[i].IDString;
			if (JHOPHFBKNAI[i].PostStringMode)
			{
				if (FGNIBFLIOCO.IndexOf(aAEEJJMOFIL) == 0)
				{
					KKKODGNDMKM = FGNIBFLIOCO.Substring(aAEEJJMOFIL.Length);
					return i;
				}
			}
			else if (FGNIBFLIOCO == aAEEJJMOFIL)
			{
				KKKODGNDMKM = string.Empty;
				return i;
			}
		}
		KKKODGNDMKM = string.Empty;
		return -1;
	}

	private static bool ParseSubCharsCommand(int COKKBDEDJEO, CommandSubCharsSet[] FKGBLNPFCJC, string FGNIBFLIOCO, ArrayList PAHFPIAOPOG)
	{
		PAHFPIAOPOG.Clear();
		int num = 0;
		for (int i = 0; i < COKKBDEDJEO; i++)
		{
			CommandSubCharsSet aNEPNHNMPMJ = FKGBLNPFCJC[i];
			int num2 = -1;
			int length = aNEPNHNMPMJ.Chars.Length;
			for (int j = 0; j < length; j++)
			{
				char value = aNEPNHNMPMJ.Chars[j];
				int num3 = FGNIBFLIOCO.IndexOf(value);
				if (num3 >= 0)
				{
					if (num2 >= 0)
					{
						return false;
					}
					if (FGNIBFLIOCO.IndexOf(value, num3 + 1) >= 0)
					{
						return false;
					}
					num2 = j;
					num++;
				}
			}
			if (num2 == -1 && !aNEPNHNMPMJ.EmptyAllowed)
			{
				return false;
			}
			PAHFPIAOPOG.Add(num2);
		}
		return num == FGNIBFLIOCO.Length;
	}

	private static bool IsItSwitchChar(char ILHDJDNPFKH)
	{
		return ILHDJDNPFKH == '-' || ILHDJDNPFKH == '/';
	}
}
