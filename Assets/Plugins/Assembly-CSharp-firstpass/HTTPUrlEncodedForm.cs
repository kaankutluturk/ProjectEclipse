using System;
using System.Text;

public sealed class HTTPUrlEncodedForm : HTTPFormBase
{
	private byte[] CachedData;

	public override void PrepareRequest(HTTPRequest request)
	{
		request.SetHeader("Content-Type", "application/x-www-form-urlencoded");
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
			HTTPFieldData field = GetFields()[i];
			if (i > 0)
			{
				stringBuilder.Append("&");
			}
			stringBuilder.Append(Uri.EscapeDataString(field.get_Name()));
			stringBuilder.Append("=");
			if (!string.IsNullOrEmpty(field.GetText()) || field.GetBinary() == null)
			{
				stringBuilder.Append(Uri.EscapeDataString(field.GetText()));
			}
			else
			{
				stringBuilder.Append(Uri.EscapeDataString(Encoding.UTF8.GetString(field.GetBinary(), 0, field.GetBinary().Length)));
			}
		}
		SetIsChanged(false);
		return CachedData = Encoding.UTF8.GetBytes(stringBuilder.ToString());
	}
}
