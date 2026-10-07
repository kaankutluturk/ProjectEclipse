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

	public Parser(int switchCount)
	{
		_switches = new SwitchResult[switchCount];
		for (int i = 0; i < switchCount; i++)
		{
			_switches[i] = new SwitchResult();
		}
	}

	private bool ParseString(string commandString, SwitchForm[] switchForms)
	{
		int length = commandString.Length;
		if (length == 0)
		{
			return false;
		}
		int num = 0;
		if (!IsItSwitchChar(commandString[num]))
		{
			return false;
		}
		while (num < length)
		{
			if (IsItSwitchChar(commandString[num]))
			{
				num++;
			}
			int num2 = 0;
			int num3 = -1;
			for (int i = 0; i < _switches.Length; i++)
			{
				int length2 = switchForms[i].IDString.Length;
				if (length2 > num3 && num + length2 <= length && string.Compare(switchForms[i].IDString, 0, commandString, num, length2, true) == 0)
				{
					num2 = i;
					num3 = length2;
				}
			}
			if (num3 == -1)
			{
				throw new Exception("maxLen == kNoLen");
			}
			SwitchResult switchResult = _switches[num2];
			SwitchForm switchForm = switchForms[num2];
			if (!switchForm.Multi && switchResult.ThereIs)
			{
				throw new Exception("switch must be single");
			}
			switchResult.ThereIs = true;
			num += num3;
			int num4 = length - num;
			SwitchType switchType = switchForm.Type;
			switch (switchType)
			{
			case SwitchType.PostMinus:
				if (num4 == 0)
				{
					switchResult.WithMinus = false;
					break;
				}
				switchResult.WithMinus = commandString[num] == '-';
				if (switchResult.WithMinus)
				{
					num++;
				}
				break;
			case SwitchType.PostChar:
			{
				if (num4 < switchForm.MinLen)
				{
					throw new Exception("switch is not full");
				}
				string postCharSet = switchForm.PostCharSet;
				if (num4 == 0)
				{
					switchResult.PostCharIndex = -1;
					break;
				}
				int num6 = postCharSet.IndexOf(commandString[num]);
				if (num6 < 0)
				{
					switchResult.PostCharIndex = -1;
					break;
				}
				switchResult.PostCharIndex = num6;
				num++;
				break;
			}
			case SwitchType.LimitedPostString:
			case SwitchType.UnLimitedPostString:
			{
				int minLength = switchForm.MinLen;
				if (num4 < minLength)
				{
					throw new Exception("switch is not full");
				}
				if (switchType == SwitchType.UnLimitedPostString)
				{
					switchResult.PostStrings.Add(commandString.Substring(num));
					return true;
				}
				string text = commandString.Substring(num, minLength);
				num += minLength;
				int num5 = minLength;
				while (num5 < switchForm.MaxLen && num < length)
				{
					char c = commandString[num];
					if (IsItSwitchChar(c))
					{
						break;
					}
					text += c;
					num5++;
					num++;
				}
				switchResult.PostStrings.Add(text);
				break;
			}
			}
		}
		return true;
	}

	public void ParseStrings(SwitchForm[] switchForms, string[] commandStrings)
	{
		int num = commandStrings.Length;
		bool flag = false;
		for (int i = 0; i < num; i++)
		{
			string text = commandStrings[i];
			if (flag)
			{
				NonSwitchStrings.Add(text);
			}
			else if (text == "--")
			{
				flag = true;
			}
			else if (!ParseString(text, switchForms))
			{
				NonSwitchStrings.Add(text);
			}
		}
	}

	public SwitchResult get_Item(int index)
	{
		return _switches[index];
	}

	public static int ParseCommand(CommandForm[] commandForms, string commandString, out string postString)
	{
		for (int i = 0; i < commandForms.Length; i++)
		{
			string idString = commandForms[i].IDString;
			if (commandForms[i].PostStringMode)
			{
				if (commandString.IndexOf(idString) == 0)
				{
					postString = commandString.Substring(idString.Length);
					return i;
				}
			}
			else if (commandString == idString)
			{
				postString = string.Empty;
				return i;
			}
		}
		postString = string.Empty;
		return -1;
	}

	private static bool ParseSubCharsCommand(int charSetCount, CommandSubCharsSet[] charSets, string commandString, ArrayList indices)
	{
		indices.Clear();
		int num = 0;
		for (int i = 0; i < charSetCount; i++)
		{
			CommandSubCharsSet charSet = charSets[i];
			int num2 = -1;
			int length = charSet.Chars.Length;
			for (int j = 0; j < length; j++)
			{
				char value = charSet.Chars[j];
				int num3 = commandString.IndexOf(value);
				if (num3 >= 0)
				{
					if (num2 >= 0)
					{
						return false;
					}
					if (commandString.IndexOf(value, num3 + 1) >= 0)
					{
						return false;
					}
					num2 = j;
					num++;
				}
			}
			if (num2 == -1 && !charSet.EmptyAllowed)
			{
				return false;
			}
			indices.Add(num2);
		}
		return num == commandString.Length;
	}

	private static bool IsItSwitchChar(char ch)
	{
		return ch == '-' || ch == '/';
	}
}
