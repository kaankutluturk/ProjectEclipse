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
			int LCCLEFMKLPB = 0;
			string scheme = header.Read(ref LCCLEFMKLPB, (char ch) => !char.IsWhiteSpace(ch) && !char.IsControl(ch)).TrimAndLower();
			list.Add(new KeyValuePair(scheme));
			while (LCCLEFMKLPB < header.Length)
			{
				string kGBGENDIMBC2 = header.Read(ref LCCLEFMKLPB, '=').TrimAndLower();
				KeyValuePair keyValuePair = new KeyValuePair(kGBGENDIMBC2);
				header.SkipWhiteSpace(ref LCCLEFMKLPB);
				keyValuePair.set_Value(header.ReadQuotedText(ref LCCLEFMKLPB));
				list.Add(keyValuePair);
			}
		}
		return list;
	}
}
