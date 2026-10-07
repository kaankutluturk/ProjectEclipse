using System;
using System.IO;

internal class ZlibDeflateStream : Stream
{
	internal ZlibBaseStream _baseStream;

	internal Stream _innerStream;

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

	public CompressionStrategy Strategy
	{
		get
		{
			return GetStrategy();
		}
		set
		{
			SetStrategy(value);
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

	public ZlibDeflateStream(Stream stream, ZlibCompressionMode compressionMode)
		: this(stream, compressionMode, ZlibCompressionLevel.Default, false)
	{
	}

	public ZlibDeflateStream(Stream stream, ZlibCompressionMode compressionMode, ZlibCompressionLevel compressionLevel)
		: this(stream, compressionMode, compressionLevel, false)
	{
	}

	public ZlibDeflateStream(Stream stream, ZlibCompressionMode compressionMode, bool leaveOpen)
		: this(stream, compressionMode, ZlibCompressionLevel.Default, leaveOpen)
	{
	}

	public ZlibDeflateStream(Stream stream, ZlibCompressionMode compressionMode, ZlibCompressionLevel compressionLevel, bool leaveOpen)
	{
		_innerStream = stream;
		_baseStream = new ZlibBaseStream(stream, compressionMode, compressionLevel, ZlibStreamFlavor.DEFLATE, leaveOpen);
	}

	public virtual FlushType GetFlushMode()
	{
		return _baseStream._flushMode;
	}

	public virtual void SetFlushMode(FlushType value)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("DeflateStream");
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
			throw new ObjectDisposedException("DeflateStream");
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

	public CompressionStrategy GetStrategy()
	{
		return _baseStream.Strategy;
	}

	public void SetStrategy(CompressionStrategy value)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("DeflateStream");
		}
		_baseStream.Strategy = value;
	}

	public virtual long GetTotalIn()
	{
		return _baseStream._z.TotalBytesIn;
	}

	public virtual long GetTotalOut()
	{
		return _baseStream._z.TotalBytesOut;
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!_disposed)
			{
				if (disposing && _baseStream != null)
				{
					_baseStream.Close();
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
				throw new ObjectDisposedException("DeflateStream");
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
				throw new ObjectDisposedException("DeflateStream");
			}
			return _baseStream._stream.CanWrite;
		}
	}
	public override void Flush()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("DeflateStream");
		}
		_baseStream.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("DeflateStream");
		}
		if (_baseStream._streamMode == ZlibBaseStream.StreamMode.Writer)
		{
			throw new InvalidOperationException("Cannot Read after Writing.");
		}
		return _baseStream.Read(buffer, offset, count);
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
				return _baseStream._z.TotalBytesOut;
			}
			if (_baseStream._streamMode == ZlibBaseStream.StreamMode.Reader)
			{
				return _baseStream._z.TotalBytesIn;
			}
			return 0L;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotImplementedException();
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("DeflateStream");
		}
		_baseStream.Write(buffer, offset, count);
	}

	public static byte[] CompressString(string text)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			Stream compressor = new ZlibDeflateStream(memoryStream, ZlibCompressionMode.Compress, ZlibCompressionLevel.BestCompression);
			ZlibBaseStream.CompressString(text, compressor);
			return memoryStream.ToArray();
		}
	}

	public static byte[] CompressBuffer(byte[] data)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			Stream compressor = new ZlibDeflateStream(memoryStream, ZlibCompressionMode.Compress, ZlibCompressionLevel.BestCompression);
			ZlibBaseStream.CompressBuffer(data, compressor);
			return memoryStream.ToArray();
		}
	}

	public static string UncompressString(byte[] compressedData)
	{
		using (MemoryStream memoryStream = new MemoryStream(compressedData))
		{
			Stream decompressor = new ZlibDeflateStream(memoryStream, ZlibCompressionMode.Decompress);
			return ZlibBaseStream.UncompressString(compressedData, decompressor);
		}
	}

	public static byte[] UncompressBuffer(byte[] compressedData)
	{
		using (MemoryStream memoryStream = new MemoryStream(compressedData))
		{
			Stream decompressor = new ZlibDeflateStream(memoryStream, ZlibCompressionMode.Decompress);
			return ZlibBaseStream.UncompressBuffer(compressedData, decompressor);
		}
	}
}
