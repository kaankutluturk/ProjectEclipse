using System.Collections.Generic;

public sealed class WWWAuthenticateHeaderParser : KeyValuePairList
{
	public WWWAuthenticateHeaderParser(string PNJNBBFLCAH)
	{
		SetValues(ParseQuotedHeader(PNJNBBFLCAH));
	}

	private List<KeyValuePair> ParseQuotedHeader(string IGGFGLLIGCG)
	{
		List<KeyValuePair> list = new List<KeyValuePair>();
		if (IGGFGLLIGCG != null)
		{
			int LCCLEFMKLPB = 0;
			string kGBGENDIMBC = IGGFGLLIGCG.Read(ref LCCLEFMKLPB, (char KDFCGMMKAME) => !char.IsWhiteSpace(KDFCGMMKAME) && !char.IsControl(KDFCGMMKAME)).TrimAndLower();
			list.Add(new KeyValuePair(kGBGENDIMBC));
			while (LCCLEFMKLPB < IGGFGLLIGCG.Length)
			{
				string kGBGENDIMBC2 = IGGFGLLIGCG.Read(ref LCCLEFMKLPB, '=').TrimAndLower();
				KeyValuePair gGCJLGPPHKP = new KeyValuePair(kGBGENDIMBC2);
				IGGFGLLIGCG.SkipWhiteSpace(ref LCCLEFMKLPB);
				gGCJLGPPHKP.set_Value(IGGFGLLIGCG.ReadQuotedText(ref LCCLEFMKLPB));
				list.Add(gGCJLGPPHKP);
			}
		}
		return list;
	}
}
