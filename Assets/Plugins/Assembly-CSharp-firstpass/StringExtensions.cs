using System;
using System.Text.RegularExpressions;

internal static class StringExtensions
{
	private static string ToCamelOrPascalCase(string input, Func<char, char> firstCharTransform)
	{
		string text = Regex.Replace(input, "([_\\-])(?<char>[a-z])", (System.Text.RegularExpressions.Match match) => match.Groups["char"].Value.ToUpperInvariant(), RegexOptions.IgnoreCase);
		return firstCharTransform(text[0]) + text.Substring(1);
	}

	public static string ToCamelCase(this string input)
	{
		return ToCamelOrPascalCase(input, char.ToLowerInvariant);
	}

	public static string ToPascalCase(this string input)
	{
		return ToCamelOrPascalCase(input, char.ToUpperInvariant);
	}

	public static string FromCamelCase(this string input, string separator)
	{
		input = char.ToLower(input[0]) + input.Substring(1);
		input = Regex.Replace(input.ToCamelCase(), "(?<char>[A-Z])", (System.Text.RegularExpressions.Match match) => separator + match.Groups["char"].Value.ToLowerInvariant());
		return input;
	}
}
