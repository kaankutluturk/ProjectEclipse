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

	public static IEnumerable<string> SplitKeepDelimiters(this string JDCCBCNFENK, char[] delims)
	{
		int num = 0;
		while (true)
		{
			int num3;
			int num2 = (num3 = JDCCBCNFENK.IndexOfAny(delims, num));
			if (num3 == -1)
			{
				break;
			}
			if (num2 - num > 0)
			{
				yield return JDCCBCNFENK.Substring(num, num2 - num);
			}
			yield return JDCCBCNFENK.Substring(num2, 1);
			num = num2 + 1;
		}
		if (num < JDCCBCNFENK.Length)
		{
			yield return JDCCBCNFENK.Substring(num);
		}
	}

	public static void LogColored(this object HCPNFPMHFCM, string color = "green")
	{
		Debug.Log(string.Concat("$<color=", color, "><b>", HCPNFPMHFCM, "</b></color>"));
	}

	public static void LogRed(this object HCPNFPMHFCM)
	{
		HCPNFPMHFCM.LogColored("red");
	}

	public static void LogBlue(this object HCPNFPMHFCM)
	{
		HCPNFPMHFCM.LogColored("blue");
	}

	public static string Colorize(this string HCPNFPMHFCM, string ABHINJEKNLG)
	{
		return "<color=#" + ABHINJEKNLG + ">" + HCPNFPMHFCM + "</color>";
	}

	public static string Colorize(this int HCPNFPMHFCM, string ABHINJEKNLG)
	{
		return HCPNFPMHFCM.ToString().Colorize(ABHINJEKNLG);
	}

	public static string Colorize(this float HCPNFPMHFCM, string ABHINJEKNLG)
	{
		return HCPNFPMHFCM.ToString(CultureInfo.InvariantCulture).Colorize(ABHINJEKNLG);
	}
}
