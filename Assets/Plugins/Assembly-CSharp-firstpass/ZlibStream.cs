using System;
using System.IO;

internal class ZlibStream : Stream
{
	internal ZlibBaseStream baseStream;

	private bool _disposed;

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

	public ZlibStream(Stream ABJIEFMMIEK, ZlibCompressionMode NMMPBADCFHK)
		: this(ABJIEFMMIEK, NMMPBADCFHK, ZlibCompressionLevel.Default, false)
	{
	}

	public ZlibStream(Stream ABJIEFMMIEK, ZlibCompressionMode NMMPBADCFHK, ZlibCompressionLevel GNLOCMLBNHF)
		: this(ABJIEFMMIEK, NMMPBADCFHK, GNLOCMLBNHF, false)
	{
	}

	public ZlibStream(Stream ABJIEFMMIEK, ZlibCompressionMode NMMPBADCFHK, bool LOLBAGJKKPH)
		: this(ABJIEFMMIEK, NMMPBADCFHK, ZlibCompressionLevel.Default, LOLBAGJKKPH)
	{
	}

	public ZlibStream(Stream ABJIEFMMIEK, ZlibCompressionMode NMMPBADCFHK, ZlibCompressionLevel GNLOCMLBNHF, bool LOLBAGJKKPH)
	{
		baseStream = new ZlibBaseStream(ABJIEFMMIEK, NMMPBADCFHK, GNLOCMLBNHF, ZlibStreamFlavor.ZLIB, LOLBAGJKKPH);
	}

	public virtual FlushType GetFlushMode()
	{
		return baseStream._flushMode;
	}

	public virtual void SetFlushMode(FlushType value)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("ZlibStream");
		}
		baseStream._flushMode = value;
	}

	public int GetBufferSize()
	{
		return baseStream._bufferSize;
	}

	public void set_BufferSize(int value)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("ZlibStream");
		}
		if (baseStream._workingBuffer != null)
		{
			throw new ZlibException("The working buffer is already set.");
		}
		if (value < 1024)
		{
			throw new ZlibException(string.Format("Don't be silly. {0} bytes?? Use a bigger buffer, at least {1}.", value, 1024));
		}
		baseStream._bufferSize = value;
	}

	public virtual long GetTotalIn()
	{
		return baseStream._z.TotalBytesIn;
	}

	public virtual long GetTotalOut()
	{
		return baseStream._z.TotalBytesOut;
	}

	protected override void Dispose(bool KLCPNDHEBGP)
	{
		try
		{
			if (!_disposed)
			{
				if (KLCPNDHEBGP && baseStream != null)
				{
					baseStream.Close();
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
				throw new ObjectDisposedException("ZlibStream");
			}
			return baseStream._stream.CanRead;
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
				throw new ObjectDisposedException("ZlibStream");
			}
			return baseStream._stream.CanWrite;
		}
	}
	public override void Flush()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("ZlibStream");
		}
		baseStream.Flush();
	}

	public override int Read(byte[] buffer, int IPCOBJBKNAO, int count)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("ZlibStream");
		}
		if (baseStream._streamMode == ZlibBaseStream.StreamMode.Writer)
		{
			throw new InvalidOperationException("Cannot Read after Writing.");
		}
		return baseStream.Read(buffer, IPCOBJBKNAO, count);
	}

	public override long Length
	{
		get
		{
			throw new NotSupportedException();
		}
	}
	public override long Position
	{
		get
		{
			if (baseStream._streamMode == ZlibBaseStream.StreamMode.Writer)
			{
				return baseStream._z.TotalBytesOut;
			}
			if (baseStream._streamMode == ZlibBaseStream.StreamMode.Reader)
			{
				return baseStream._z.TotalBytesIn;
			}
			return 0L;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public override long Seek(long IPCOBJBKNAO, SeekOrigin IKOOJMAOFOD)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int IPCOBJBKNAO, int count)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("ZlibStream");
		}
		baseStream.Write(buffer, IPCOBJBKNAO, count);
	}

	public static byte[] CompressString(string JDCCBCNFENK)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			Stream aBKOBELCOIK = new ZlibStream(memoryStream, ZlibCompressionMode.Compress, ZlibCompressionLevel.BestCompression);
			ZlibBaseStream.CompressString(JDCCBCNFENK, aBKOBELCOIK);
			return memoryStream.ToArray();
		}
	}

	public static byte[] CompressBuffer(byte[] AAOIAEJJINO)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			Stream aBKOBELCOIK = new ZlibStream(memoryStream, ZlibCompressionMode.Compress, ZlibCompressionLevel.BestCompression);
			ZlibBaseStream.CompressBuffer(AAOIAEJJINO, aBKOBELCOIK);
			return memoryStream.ToArray();
		}
	}

	public static string UncompressString(byte[] FCPABLANKDN)
	{
		using (MemoryStream aBJIEFMMIEK = new MemoryStream(FCPABLANKDN))
		{
			Stream iNIMCIOFFCJ = new ZlibStream(aBJIEFMMIEK, ZlibCompressionMode.Decompress);
			return ZlibBaseStream.UncompressString(FCPABLANKDN, iNIMCIOFFCJ);
		}
	}

	public static byte[] UncompressBuffer(byte[] FCPABLANKDN)
	{
		using (MemoryStream aBJIEFMMIEK = new MemoryStream(FCPABLANKDN))
		{
			Stream iNIMCIOFFCJ = new ZlibStream(aBJIEFMMIEK, ZlibCompressionMode.Decompress);
			return ZlibBaseStream.UncompressBuffer(FCPABLANKDN, iNIMCIOFFCJ);
		}
	}
}
