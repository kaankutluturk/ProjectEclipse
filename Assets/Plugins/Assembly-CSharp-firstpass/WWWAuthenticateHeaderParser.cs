using System.Collections.Generic;

public sealed class WWWAuthenticateHeaderParser : KeyValuePairList
{
	public WWWAuthenticateHeaderParser(string header)
	{
		SetValues(ParseQuotedHeader(header));
	}

	private List<KeyValuePair> ParseQuotedHeader(string header)
	{
		List<KeyValuePair> list = new List<KeyValuePair>();
		if (header != null)
		{
			int cursor = 0;
			string scheme = header.Read(ref cursor, (char ch) => !char.IsWhiteSpace(ch) && !char.IsControl(ch)).TrimAndLower();
			list.Add(new KeyValuePair(scheme));
			while (cursor < header.Length)
			{
				string headerKey = header.Read(ref cursor, '=').TrimAndLower();
				KeyValuePair keyValuePair = new KeyValuePair(headerKey);
				header.SkipWhiteSpace(ref cursor);
				keyValuePair.set_Value(header.ReadQuotedText(ref cursor));
				list.Add(keyValuePair);
			}
		}
		return list;
	}
}
