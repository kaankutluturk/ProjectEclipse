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

	public ZlibStream(Stream stream, ZlibCompressionMode compressionMode)
		: this(stream, compressionMode, ZlibCompressionLevel.Default, false)
	{
	}

	public ZlibStream(Stream stream, ZlibCompressionMode compressionMode, ZlibCompressionLevel compressionLevel)
		: this(stream, compressionMode, compressionLevel, false)
	{
	}

	public ZlibStream(Stream stream, ZlibCompressionMode compressionMode, bool leaveOpen)
		: this(stream, compressionMode, ZlibCompressionLevel.Default, leaveOpen)
	{
	}

	public ZlibStream(Stream stream, ZlibCompressionMode compressionMode, ZlibCompressionLevel compressionLevel, bool leaveOpen)
	{
		baseStream = new ZlibBaseStream(stream, compressionMode, compressionLevel, ZlibStreamFlavor.ZLIB, leaveOpen);
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

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!_disposed)
			{
				if (disposing && baseStream != null)
				{
					baseStream.Close();
				}
				_disposed = true;
			}
		}
		finally
		{
			base.Dispose(disposing);
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

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("ZlibStream");
		}
		if (baseStream._streamMode == ZlibBaseStream.StreamMode.Writer)
		{
			throw new InvalidOperationException("Cannot Read after Writing.");
		}
		return baseStream.Read(buffer, offset, count);
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

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("ZlibStream");
		}
		baseStream.Write(buffer, offset, count);
	}

	public static byte[] CompressString(string text)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			Stream compressor = new ZlibStream(memoryStream, ZlibCompressionMode.Compress, ZlibCompressionLevel.BestCompression);
			ZlibBaseStream.CompressString(text, compressor);
			return memoryStream.ToArray();
		}
	}

	public static byte[] CompressBuffer(byte[] data)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			Stream compressor = new ZlibStream(memoryStream, ZlibCompressionMode.Compress, ZlibCompressionLevel.BestCompression);
			ZlibBaseStream.CompressBuffer(data, compressor);
			return memoryStream.ToArray();
		}
	}

	public static string UncompressString(byte[] compressedData)
	{
		using (MemoryStream memoryStream = new MemoryStream(compressedData))
		{
			Stream decompressor = new ZlibStream(memoryStream, ZlibCompressionMode.Decompress);
			return ZlibBaseStream.UncompressString(compressedData, decompressor);
		}
	}

	public static byte[] UncompressBuffer(byte[] compressedData)
	{
		using (MemoryStream memoryStream = new MemoryStream(compressedData))
		{
			Stream decompressor = new ZlibStream(memoryStream, ZlibCompressionMode.Decompress);
			return ZlibBaseStream.UncompressBuffer(compressedData, decompressor);
		}
	}
}
