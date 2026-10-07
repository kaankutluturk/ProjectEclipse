using System.IO;

public sealed class HTTPMultiPartForm : HTTPFormBase
{
	private string Boundary;

	private byte[] CachedData;

	public HTTPMultiPartForm()
	{
		Boundary = GetHashCode().ToString("X");
	}

	public override void PrepareRequest(HTTPRequest request)
	{
		request.SetHeader("Content-Type", "multipart/form-data; boundary=\"" + Boundary + "\"");
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
				HTTPFieldData field = GetFields()[i];
				memoryStream.WriteLine("--" + Boundary);
				memoryStream.WriteLine("Content-Disposition: form-data; name=\"" + field.get_Name() + "\"" + (string.IsNullOrEmpty(field.GetFileName()) ? string.Empty : ("; filename=\"" + field.GetFileName() + "\"")));
				if (!string.IsNullOrEmpty(field.GetMimeType()))
				{
					memoryStream.WriteLine("Content-Type: " + field.GetMimeType());
				}
				memoryStream.WriteLine("Content-Length: " + field.GetPayload().Length);
				memoryStream.WriteLine();
				memoryStream.Write(field.GetPayload(), 0, field.GetPayload().Length);
				memoryStream.Write(HTTPRequest.EOL, 0, HTTPRequest.EOL.Length);
			}
			memoryStream.WriteLine("--" + Boundary + "--");
			SetIsChanged(false);
			return CachedData = memoryStream.ToArray();
		}
	}
}
