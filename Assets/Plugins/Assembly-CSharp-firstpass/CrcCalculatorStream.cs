using System;
using System.IO;

internal class CrcCalculatorStream : Stream, IDisposable
{
	private static readonly long UnsetLengthLimit = -99L;

	internal Stream _innerStream;

	private CRC32 _crc32;

	private long _lengthLimit = -99L;

	private bool _leaveOpen;

	public long TotalBytesSlurped
	{
		get
		{
			return GetTotalBytesSlurped();
		}
	}

	public int Crc
	{
		get
		{
			return GetCrc();
		}
	}

	public bool LeaveInnerStreamOpen
	{
		get
		{
			return GetLeaveOpen();
		}
		set
		{
			set_LeaveOpen(value);
		}
	}

	public CrcCalculatorStream(Stream stream)
		: this(true, UnsetLengthLimit, stream, null)
	{
	}

	public CrcCalculatorStream(Stream stream, bool leaveOpen)
		: this(leaveOpen, UnsetLengthLimit, stream, null)
	{
	}

	public CrcCalculatorStream(Stream stream, long length)
		: this(true, length, stream, null)
	{
		if (length < 0)
		{
			throw new ArgumentException("length");
		}
	}

	public CrcCalculatorStream(Stream stream, long length, bool leaveOpen)
		: this(leaveOpen, length, stream, null)
	{
		if (length < 0)
		{
			throw new ArgumentException("length");
		}
	}

	public CrcCalculatorStream(Stream stream, long length, bool leaveOpen, CRC32 crc32)
		: this(leaveOpen, length, stream, crc32)
	{
		if (length < 0)
		{
			throw new ArgumentException("length");
		}
	}

	private CrcCalculatorStream(bool leaveOpen, long length, Stream stream, CRC32 crc32)
	{
		_innerStream = stream;
		_crc32 = crc32 ?? new CRC32();
		_lengthLimit = length;
		_leaveOpen = leaveOpen;
	}

	public long GetTotalBytesSlurped()
	{
		return _crc32.GetTotalBytesRead();
	}

	public int GetCrc()
	{
		return _crc32.GetCrc32Result();
	}

	public bool GetLeaveOpen()
	{
		return _leaveOpen;
	}

	public void set_LeaveOpen(bool value)
	{
		_leaveOpen = value;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (_lengthLimit != UnsetLengthLimit)
		{
			if (_crc32.GetTotalBytesRead() >= _lengthLimit)
			{
				return 0;
			}
			long num = _lengthLimit - _crc32.GetTotalBytesRead();
			if (num < count)
			{
				count = (int)num;
			}
		}
		int num2 = _innerStream.Read(buffer, offset, count);
		if (num2 > 0)
		{
			_crc32.SlurpBlock(buffer, offset, num2);
		}
		return num2;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (count > 0)
		{
			_crc32.SlurpBlock(buffer, offset, count);
		}
		_innerStream.Write(buffer, offset, count);
	}

	public override bool CanRead
	{
		get
		{
			return _innerStream.CanRead;
		}
	}
	public override bool CanSeek
	{
		get
		{
			return false;
		}
	}
	public override bool CanWrite
	{
		get
		{
			return _innerStream.CanWrite;
		}
	}
	public override void Flush()
	{
		_innerStream.Flush();
	}

	public override long Length
	{
		get
		{
			if (_lengthLimit == UnsetLengthLimit)
			{
				return _innerStream.Length;
			}
			return _lengthLimit;
		}
	}
	public override long Position
	{
		get
		{
			return _crc32.GetTotalBytesRead();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	void IDisposable.Dispose()
	{
		Close();
	}

	public override void Close()
	{
		Dispose();
		if (!_leaveOpen)
		{
			_innerStream.Dispose();
		}
	}
}
