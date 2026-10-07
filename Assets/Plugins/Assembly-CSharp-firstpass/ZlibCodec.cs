using System;

internal sealed class ZlibCodec
{
	public byte[] InputBuffer;

	public int NextIn;

	public int AvailableBytesIn;

	public long TotalBytesIn;

	public byte[] OutputBuffer;

	public int NextOut;

	public int AvailableBytesOut;

	public long TotalBytesOut;

	public string Message;

	internal DeflateManager DeflateState;

	internal InflateManager InflateState;

	internal uint _Adler32;

	public ZlibCompressionLevel CompressLevel = ZlibCompressionLevel.Default;

	public int WindowBits = 15;

	public CompressionStrategy Strategy;

	public int Adler32
	{
		get
		{
			return GetAdler32();
		}
	}

	public ZlibCodec()
	{
	}

	public ZlibCodec(ZlibCompressionMode compressionMode)
	{
		switch (compressionMode)
		{
		case ZlibCompressionMode.Compress:
			if (InitializeDeflate() != 0)
			{
				throw new ZlibException("Cannot initialize for deflate.");
			}
			break;
		case ZlibCompressionMode.Decompress:
			if (InitializeInflate() != 0)
			{
				throw new ZlibException("Cannot initialize for inflate.");
			}
			break;
		default:
			throw new ZlibException("Invalid ZlibStreamFlavor.");
		}
	}

	public int GetAdler32()
	{
		return (int)_Adler32;
	}

	public int InitializeInflate()
	{
		return InitializeInflate(WindowBits);
	}

	public int InitializeInflate(bool expectRfc1950Header)
	{
		return InitializeInflate(WindowBits, expectRfc1950Header);
	}

	public int InitializeInflate(int windowBits)
	{
		WindowBits = windowBits;
		return InitializeInflate(windowBits, true);
	}

	public int InitializeInflate(int windowBits, bool expectRfc1950Header)
	{
		WindowBits = windowBits;
		if (DeflateState != null)
		{
			throw new ZlibException("You may not call InitializeInflate() after calling InitializeDeflate().");
		}
		InflateState = new InflateManager(expectRfc1950Header);
		return InflateState.Initialize(this, windowBits);
	}

	public int Inflate(FlushType flushType)
	{
		if (InflateState == null)
		{
			throw new ZlibException("No Inflate State!");
		}
		return InflateState.Inflate(flushType);
	}

	public int EndInflate()
	{
		if (InflateState == null)
		{
			throw new ZlibException("No Inflate State!");
		}
		int result = InflateState.End();
		InflateState = null;
		return result;
	}

	public int SyncInflate()
	{
		if (InflateState == null)
		{
			throw new ZlibException("No Inflate State!");
		}
		return InflateState.Sync();
	}

	public int InitializeDeflate()
	{
		return InitializeDeflateInternal(true);
	}

	public int InitializeDeflate(ZlibCompressionLevel compressionLevel)
	{
		CompressLevel = compressionLevel;
		return InitializeDeflateInternal(true);
	}

	public int InitializeDeflate(ZlibCompressionLevel compressionLevel, bool wantRfc1950Header)
	{
		CompressLevel = compressionLevel;
		return InitializeDeflateInternal(wantRfc1950Header);
	}

	public int InitializeDeflate(ZlibCompressionLevel compressionLevel, int windowBits)
	{
		CompressLevel = compressionLevel;
		WindowBits = windowBits;
		return InitializeDeflateInternal(true);
	}

	public int InitializeDeflate(ZlibCompressionLevel compressionLevel, int windowBits, bool wantRfc1950Header)
	{
		CompressLevel = compressionLevel;
		WindowBits = windowBits;
		return InitializeDeflateInternal(wantRfc1950Header);
	}

	private int InitializeDeflateInternal(bool wantRfc1950Header)
	{
		if (InflateState != null)
		{
			throw new ZlibException("You may not call InitializeDeflate() after calling InitializeInflate().");
		}
		DeflateState = new DeflateManager();
		DeflateState.SetWantRfc1950HeaderBytes(wantRfc1950Header);
		return DeflateState.Initialize(this, CompressLevel, WindowBits, Strategy);
	}

	public int Deflate(FlushType flushType)
	{
		if (DeflateState == null)
		{
			throw new ZlibException("No Deflate State!");
		}
		return DeflateState.Deflate(flushType);
	}

	public int EndDeflate()
	{
		if (DeflateState == null)
		{
			throw new ZlibException("No Deflate State!");
		}
		DeflateState = null;
		return 0;
	}

	public void ResetDeflate()
	{
		if (DeflateState == null)
		{
			throw new ZlibException("No Deflate State!");
		}
		DeflateState.Reset();
	}

	public int SetDeflateParams(ZlibCompressionLevel compressionLevel, CompressionStrategy strategy)
	{
		if (DeflateState == null)
		{
			throw new ZlibException("No Deflate State!");
		}
		return DeflateState.SetParams(compressionLevel, strategy);
	}

	public int SetDictionary(byte[] dictionary)
	{
		if (InflateState != null)
		{
			return InflateState.SetDictionary(dictionary);
		}
		if (DeflateState != null)
		{
			return DeflateState.SetDictionary(dictionary);
		}
		throw new ZlibException("No Inflate or Deflate state!");
	}

	internal void FlushPending()
	{
		int num = DeflateState.pendingCount;
		if (num > AvailableBytesOut)
		{
			num = AvailableBytesOut;
		}
		if (num != 0)
		{
			if (DeflateState.pending.Length <= DeflateState.nextPending || OutputBuffer.Length <= NextOut || DeflateState.pending.Length < DeflateState.nextPending + num || OutputBuffer.Length < NextOut + num)
			{
				throw new ZlibException(string.Format("Invalid State. (pending.Length={0}, pendingCount={1})", DeflateState.pending.Length, DeflateState.pendingCount));
			}
			Array.Copy(DeflateState.pending, DeflateState.nextPending, OutputBuffer, NextOut, num);
			NextOut += num;
			DeflateState.nextPending += num;
			TotalBytesOut += num;
			AvailableBytesOut -= num;
			DeflateState.pendingCount -= num;
			if (DeflateState.pendingCount == 0)
			{
				DeflateState.nextPending = 0;
			}
		}
	}

	internal int read_buf(byte[] buffer, int offset, int length)
	{
		int num = AvailableBytesIn;
		if (num > length)
		{
			num = length;
		}
		if (num == 0)
		{
			return 0;
		}
		AvailableBytesIn -= num;
		if (DeflateState.GetWantRfc1950HeaderBytes())
		{
			_Adler32 = Adler.Adler32(_Adler32, InputBuffer, NextIn, num);
		}
		Array.Copy(InputBuffer, NextIn, buffer, offset, num);
		NextIn += num;
		TotalBytesIn += num;
		return num;
	}
}
