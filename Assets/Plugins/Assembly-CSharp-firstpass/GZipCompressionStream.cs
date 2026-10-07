using System;
using System.IO;
using System.Text;

internal class GZipCompressionStream : Stream
{
	public DateTime? LastModified;

	private int _headerByteCount;

	internal ZlibBaseStream _baseStream;

	private bool _disposed;

	private bool _firstReadDone;

	private string _fileName;

	private string _comment;

	private int _crc32;

	internal static readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	internal static readonly Encoding iso8859dash1 = Encoding.GetEncoding("iso-8859-1");

	public string GZipComment
	{
		get
		{
			return GetComment();
		}
		set
		{
			SetComment(value);
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

	public int Crc32
	{
		get
		{
			return GetCrc32();
		}
	}

	public virtual FlushType FlushMode
	{
		get
		{
			return GetFlushMode();
		}
		set
		{
			SetFlushMode(value);
		}
	}

	public int WorkingBufferSize
	{
		get
		{
			return GetBufferSize();
		}
		set
		{
			set_BufferSize(value);
		}
	}

	public virtual long TotalIn
	{
		get
		{
			return GetTotalIn();
		}
	}

	public virtual long TotalOut
	{
		get
		{
			return GetTotalOut();
		}
	}

	public GZipCompressionStream(Stream ABJIEFMMIEK, ZlibCompressionMode NMMPBADCFHK)
		: this(ABJIEFMMIEK, NMMPBADCFHK, ZlibCompressionLevel.Default, false)
	{
	}

	public GZipCompressionStream(Stream ABJIEFMMIEK, ZlibCompressionMode NMMPBADCFHK, ZlibCompressionLevel GNLOCMLBNHF)
		: this(ABJIEFMMIEK, NMMPBADCFHK, GNLOCMLBNHF, false)
	{
	}

	public GZipCompressionStream(Stream ABJIEFMMIEK, ZlibCompressionMode NMMPBADCFHK, bool LOLBAGJKKPH)
		: this(ABJIEFMMIEK, NMMPBADCFHK, ZlibCompressionLevel.Default, LOLBAGJKKPH)
	{
	}

	public GZipCompressionStream(Stream ABJIEFMMIEK, ZlibCompressionMode NMMPBADCFHK, ZlibCompressionLevel GNLOCMLBNHF, bool LOLBAGJKKPH)
	{
		_baseStream = new ZlibBaseStream(ABJIEFMMIEK, NMMPBADCFHK, GNLOCMLBNHF, ZlibStreamFlavor.GZIP, LOLBAGJKKPH);
	}

	public string GetComment()
	{
		return _comment;
	}

	public void SetComment(string value)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("GZipStream");
		}
		_comment = value;
	}

	public string GetFileName()
	{
		return _fileName;
	}

	public void SetFileName(string value)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("GZipStream");
		}
		_fileName = value;
		if (_fileName != null)
		{
			if (_fileName.IndexOf("/") != -1)
			{
				_fileName = _fileName.Replace("/", "\\");
			}
			if (_fileName.EndsWith("\\"))
			{
				throw new Exception("Illegal filename");
			}
			if (_fileName.IndexOf("\\") != -1)
			{
				_fileName = Path.GetFileName(_fileName);
			}
		}
	}

	public int GetCrc32()
	{
		return _crc32;
	}

	public virtual FlushType GetFlushMode()
	{
		return _baseStream._flushMode;
	}

	public virtual void SetFlushMode(FlushType value)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("GZipStream");
		}
		_baseStream._flushMode = value;
	}

	public int GetBufferSize()
	{
		return _baseStream._bufferSize;
	}

	public void set_BufferSize(int value)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("GZipStream");
		}
		if (_baseStream._workingBuffer != null)
		{
			throw new ZlibException("The working buffer is already set.");
		}
		if (value < 1024)
		{
			throw new ZlibException(string.Format("Don't be silly. {0} bytes?? Use a bigger buffer, at least {1}.", value, 1024));
		}
		_baseStream._bufferSize = value;
	}

	public virtual long GetTotalIn()
	{
		return _baseStream._z.TotalBytesIn;
	}

	public virtual long GetTotalOut()
	{
		return _baseStream._z.TotalBytesOut;
	}

	protected override void Dispose(bool KLCPNDHEBGP)
	{
		try
		{
			if (!_disposed)
			{
				if (KLCPNDHEBGP && _baseStream != null)
				{
					_baseStream.Close();
					_crc32 = _baseStream.GetCrc32();
				}
				_disposed = true;
			}
		}
		finally
		{
			base.Dispose(KLCPNDHEBGP);
		}
	}

	public override bool CanRead
	{
		get
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("GZipStream");
			}
			return _baseStream._stream.CanRead;
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
			if (_disposed)
			{
				throw new ObjectDisposedException("GZipStream");
			}
			return _baseStream._stream.CanWrite;
		}
	}
	public override void Flush()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("GZipStream");
		}
		_baseStream.Flush();
	}
	public override int Read(byte[] buffer, int IPCOBJBKNAO, int count)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("GZipStream");
		}
		if (_baseStream._streamMode == ZlibBaseStream.StreamMode.Writer)
		{
			throw new InvalidOperationException("Cannot Read after Writing.");
		}
		return _baseStream.Read(buffer, IPCOBJBKNAO, count);
	}


	public override long Length
	{
		get
		{
			throw new NotImplementedException();
		}
	}
	public override long Position
	{
		get
		{
			if (_baseStream._streamMode == ZlibBaseStream.StreamMode.Writer)
			{
				return _baseStream._z.TotalBytesOut + _headerByteCount;
			}
			if (_baseStream._streamMode == ZlibBaseStream.StreamMode.Reader)
			{
				return _baseStream._z.TotalBytesIn + _baseStream._gzipHeaderByteCount;
			}
			return 0L;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public override long Seek(long IPCOBJBKNAO, SeekOrigin IKOOJMAOFOD)
	{
		throw new NotImplementedException();
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public override void Write(byte[] buffer, int IPCOBJBKNAO, int count)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("GZipStream");
		}
		if (_baseStream._streamMode == ZlibBaseStream.StreamMode.Undefined)
		{
			if (!_baseStream.GetWantCompress())
			{
				throw new InvalidOperationException();
			}
			_headerByteCount = EmitHeader();
		}
		_baseStream.Write(buffer, IPCOBJBKNAO, count);
	}

	private int EmitHeader()
	{
		byte[] array = ((GetComment() != null) ? iso8859dash1.GetBytes(GetComment()) : null);
		byte[] array2 = ((GetFileName() != null) ? iso8859dash1.GetBytes(GetFileName()) : null);
		int num = ((GetComment() != null) ? (array.Length + 1) : 0);
		int num2 = ((GetFileName() != null) ? (array2.Length + 1) : 0);
		int num3 = 10 + num + num2;
		byte[] array3 = new byte[num3];
		int num4 = 0;
		array3[num4++] = 31;
		array3[num4++] = 139;
		array3[num4++] = 8;
		byte b = 0;
		if (GetComment() != null)
		{
			b ^= 0x10;
		}
		if (GetFileName() != null)
		{
			b ^= 8;
		}
		array3[num4++] = b;
		if (!LastModified.HasValue)
		{
			LastModified = DateTime.Now;
		}
		int value = (int)(LastModified.Value - _unixEpoch).TotalSeconds;
		Array.Copy(BitConverter.GetBytes(value), 0, array3, num4, 4);
		num4 += 4;
		array3[num4++] = 0;
		array3[num4++] = byte.MaxValue;
		if (num2 != 0)
		{
			Array.Copy(array2, 0, array3, num4, num2 - 1);
			num4 += num2 - 1;
			array3[num4++] = 0;
		}
		if (num != 0)
		{
			Array.Copy(array, 0, array3, num4, num - 1);
			num4 += num - 1;
			array3[num4++] = 0;
		}
		_baseStream._stream.Write(array3, 0, array3.Length);
		return array3.Length;
	}

	public static byte[] CompressString(string JDCCBCNFENK)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			Stream aBKOBELCOIK = new GZipCompressionStream(memoryStream, ZlibCompressionMode.Compress, ZlibCompressionLevel.BestCompression);
			ZlibBaseStream.CompressString(JDCCBCNFENK, aBKOBELCOIK);
			return memoryStream.ToArray();
		}
	}

	public static byte[] CompressBuffer(byte[] AAOIAEJJINO)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			Stream aBKOBELCOIK = new GZipCompressionStream(memoryStream, ZlibCompressionMode.Compress, ZlibCompressionLevel.BestCompression);
			ZlibBaseStream.CompressBuffer(AAOIAEJJINO, aBKOBELCOIK);
			return memoryStream.ToArray();
		}
	}

	public static string UncompressString(byte[] FCPABLANKDN)
	{
		using (MemoryStream aBJIEFMMIEK = new MemoryStream(FCPABLANKDN))
		{
			Stream iNIMCIOFFCJ = new GZipCompressionStream(aBJIEFMMIEK, ZlibCompressionMode.Decompress);
			return ZlibBaseStream.UncompressString(FCPABLANKDN, iNIMCIOFFCJ);
		}
	}

	public static byte[] UncompressBuffer(byte[] FCPABLANKDN)
	{
		using (MemoryStream aBJIEFMMIEK = new MemoryStream(FCPABLANKDN))
		{
			Stream iNIMCIOFFCJ = new GZipCompressionStream(aBJIEFMMIEK, ZlibCompressionMode.Decompress);
			return ZlibBaseStream.UncompressBuffer(FCPABLANKDN, iNIMCIOFFCJ);
		}
	}
}
