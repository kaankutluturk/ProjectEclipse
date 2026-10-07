using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class CommonExtensions
{
	public static bool IsNullOrEmpty(this string value)
	{
		return string.IsNullOrEmpty(value);
	}

	public static string EvaluateFormula(this string value)
	{
		return new RpnParser.Formula(value).Calculate().ToString();
	}

	public static IEnumerable<string> SplitKeepDelimiters(this string source, char[] delims)
	{
		int num = 0;
		while (true)
		{
			int num3;
			int num2 = (num3 = source.IndexOfAny(delims, num));
			if (num3 == -1)
			{
				break;
			}
			if (num2 - num > 0)
			{
				yield return source.Substring(num, num2 - num);
			}
			yield return source.Substring(num2, 1);
			num = num2 + 1;
		}
		if (num < source.Length)
		{
			yield return source.Substring(num);
		}
	}

	public static void LogColored(this object message, string color = "green")
	{
		Debug.Log(string.Concat("$<color=", color, "><b>", message, "</b></color>"));
	}

	public static void LogRed(this object message)
	{
		message.LogColored("red");
	}

	public static void LogBlue(this object message)
	{
		message.LogColored("blue");
	}

	public static string Colorize(this string text, string colorHex)
	{
		return "<color=#" + colorHex + ">" + text + "</color>";
	}

	public static string Colorize(this int number, string colorHex)
	{
		return number.ToString().Colorize(colorHex);
	}

	public static string Colorize(this float number, string colorHex)
	{
		return number.ToString(CultureInfo.InvariantCulture).Colorize(colorHex);
	}
}
