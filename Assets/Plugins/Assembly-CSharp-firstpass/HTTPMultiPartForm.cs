using System.IO;

public sealed class HTTPMultiPartForm : HTTPFormBase
{
	private string Boundary;

	private byte[] CachedData;

	public HTTPMultiPartForm()
	{
		Boundary = GetHashCode().ToString("X");
	}

	public override void PrepareRequest(HTTPRequest ONOCIELLAPL)
	{
		ONOCIELLAPL.SetHeader("Content-Type", "multipart/form-data; boundary=\"" + Boundary + "\"");
	}

	public override byte[] GetData()
	{
		if (CachedData != null)
		{
			return CachedData;
		}
		using (MemoryStream memoryStream = new MemoryStream())
		{
			for (int i = 0; i < GetFields().Count; i++)
			{
				HTTPFieldData iIMHHCDGJOL = GetFields()[i];
				memoryStream.WriteLine("--" + Boundary);
				memoryStream.WriteLine("Content-Disposition: form-data; name=\"" + iIMHHCDGJOL.get_Name() + "\"" + (string.IsNullOrEmpty(iIMHHCDGJOL.GetFileName()) ? string.Empty : ("; filename=\"" + iIMHHCDGJOL.GetFileName() + "\"")));
				if (!string.IsNullOrEmpty(iIMHHCDGJOL.GetMimeType()))
				{
					memoryStream.WriteLine("Content-Type: " + iIMHHCDGJOL.GetMimeType());
				}
				memoryStream.WriteLine("Content-Length: " + iIMHHCDGJOL.GetPayload().Length);
				memoryStream.WriteLine();
				memoryStream.Write(iIMHHCDGJOL.GetPayload(), 0, iIMHHCDGJOL.GetPayload().Length);
				memoryStream.Write(HTTPRequest.EOL, 0, HTTPRequest.EOL.Length);
			}
			memoryStream.WriteLine("--" + Boundary + "--");
			SetIsChanged(false);
			return CachedData = memoryStream.ToArray();
		}
	}
}
