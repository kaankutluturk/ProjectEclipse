using System;
using System.Globalization;

internal static class YamlFormatter
{
	private static readonly NumberFormatInfo numberFormat = new NumberFormatInfo
	{
		CurrencyDecimalSeparator = ".",
		CurrencyGroupSeparator = "_",
		CurrencyGroupSizes = new int[1] { 3 },
		CurrencySymbol = string.Empty,
		CurrencyDecimalDigits = 99,
		NumberDecimalSeparator = ".",
		NumberGroupSeparator = "_",
		NumberGroupSizes = new int[1] { 3 },
		NumberDecimalDigits = 99
	};

	public static string FormatNumber(object number)
	{
		return Convert.ToString(number, numberFormat);
	}

	public static string FormatBool(object value)
	{
		return (!value.Equals(true)) ? "false" : "true";
	}

	public static string FormatDateTime(object value)
	{
		return ((DateTime)value).ToString("o", CultureInfo.InvariantCulture);
	}

	public static string FormatTimeSpan(object value)
	{
		return ((TimeSpan)value/*cast due to constrained. prefix*/).ToString();
	}
}
