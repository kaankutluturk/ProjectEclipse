using System.Diagnostics;

public sealed class HTTPRange
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int firstBytePos;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int lastBytePos;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int contentLength;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isValid;

	public int FirstBytePos
	{
		get
		{
			return GetFirstBytePos();
		}
		private set
		{
			SetFirstBytePos(value);
		}
	}

	public int LastBytePos
	{
		get
		{
			return GetLastBytePos();
		}
		private set
		{
			SetLastBytePos(value);
		}
	}

	public int ContentLength
	{
		get
		{
			return GetContentLength();
		}
		private set
		{
			SetContentLength(value);
		}
	}

	public bool IsValidRange
	{
		get
		{
			return GetIsValid();
		}
		private set
		{
			set_IsValid(value);
		}
	}

	internal HTTPRange()
	{
		SetContentLength(-1);
		set_IsValid(false);
	}

	internal HTTPRange(int contentLength)
	{
		SetContentLength(contentLength);
		set_IsValid(false);
	}

	internal HTTPRange(int firstBytePos, int lastBytePos, int contentLength)
	{
		SetFirstBytePos(firstBytePos);
		SetLastBytePos(lastBytePos);
		SetContentLength(contentLength);
		set_IsValid(GetFirstBytePos() <= GetLastBytePos() && GetContentLength() > GetLastBytePos());
	}

	public int GetFirstBytePos()
	{
		return firstBytePos;
	}

	private void SetFirstBytePos(int value)
	{
		firstBytePos = value;
	}

	public int GetLastBytePos()
	{
		return lastBytePos;
	}

	private void SetLastBytePos(int value)
	{
		lastBytePos = value;
	}

	public int GetContentLength()
	{
		return contentLength;
	}

	private void SetContentLength(int value)
	{
		contentLength = value;
	}

	public bool GetIsValid()
	{
		return isValid;
	}

	private void set_IsValid(bool value)
	{
		isValid = value;
	}

	public override string ToString()
	{
		return string.Format("{0}-{1}/{2} (valid: {3})", GetFirstBytePos(), GetLastBytePos(), GetContentLength(), GetIsValid());
	}
}
