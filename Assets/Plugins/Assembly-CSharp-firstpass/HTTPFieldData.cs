using System.Diagnostics;
using System.Text;

public class HTTPFieldData
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string name;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string fileName;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string mimeType;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Encoding encoding;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string text;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private byte[] binary;

	public string FieldName
	{
		get
		{
			return get_Name();
		}
		set
		{
			set_Name(value);
		}
	}

	public string FileName
	{
		get
		{
			return GetFileName();
		}
		set
		{
			SetFileName(value);
		}
	}

	public string MimeType
	{
		get
		{
			return GetMimeType();
		}
		set
		{
			SetMimeType(value);
		}
	}

	public Encoding TextEncoding
	{
		get
		{
			return GetEncoding();
		}
		set
		{
			set_Encoding(value);
		}
	}

	public string Text
	{
		get
		{
			return GetText();
		}
		set
		{
			SetText(value);
		}
	}

	public byte[] BinaryData
	{
		get
		{
			return GetBinary();
		}
		set
		{
			set_Binary(value);
		}
	}

	public byte[] Payload
	{
		get
		{
			return GetPayload();
		}
	}

	public string get_Name()
	{
		return name;
	}

	public void set_Name(string value)
	{
		name = value;
	}

	public string GetFileName()
	{
		return fileName;
	}

	public void SetFileName(string value)
	{
		fileName = value;
	}

	public string GetMimeType()
	{
		return mimeType;
	}

	public void SetMimeType(string value)
	{
		mimeType = value;
	}

	public Encoding GetEncoding()
	{
		return encoding;
	}

	public void set_Encoding(Encoding value)
	{
		encoding = value;
	}

	public string GetText()
	{
		return text;
	}

	public void SetText(string value)
	{
		text = value;
	}

	public byte[] GetBinary()
	{
		return binary;
	}

	public void set_Binary(byte[] value)
	{
		binary = value;
	}

	public byte[] GetPayload()
	{
		if (GetBinary() != null)
		{
			return GetBinary();
		}
		if (GetEncoding() == null)
		{
			set_Encoding(Encoding.UTF8);
		}
		byte[] bytes = GetEncoding().GetBytes(GetText());
		set_Binary(bytes);
		return bytes;
	}
}
