using System;
using System.Text;

public sealed class HTTPUrlEncodedForm : HTTPFormBase
{
	private byte[] CachedData;

	public override void PrepareRequest(HTTPRequest ONOCIELLAPL)
	{
		ONOCIELLAPL.SetHeader("Content-Type", "application/x-www-form-urlencoded");
	}

	public override byte[] GetData()
	{
		if (CachedData != null && !GetIsChanged())
		{
			return CachedData;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < GetFields().Count; i++)
		{
			HTTPFieldData iIMHHCDGJOL = GetFields()[i];
			if (i > 0)
			{
				stringBuilder.Append("&");
			}
			stringBuilder.Append(Uri.EscapeDataString(iIMHHCDGJOL.get_Name()));
			stringBuilder.Append("=");
			if (!string.IsNullOrEmpty(iIMHHCDGJOL.GetText()) || iIMHHCDGJOL.GetBinary() == null)
			{
				stringBuilder.Append(Uri.EscapeDataString(iIMHHCDGJOL.GetText()));
			}
			else
			{
				stringBuilder.Append(Uri.EscapeDataString(Encoding.UTF8.GetString(iIMHHCDGJOL.GetBinary(), 0, iIMHHCDGJOL.GetBinary().Length)));
			}
		}
		SetIsChanged(false);
		return CachedData = Encoding.UTF8.GetBytes(stringBuilder.ToString());
	}
}
