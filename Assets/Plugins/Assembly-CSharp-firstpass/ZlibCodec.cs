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

	public ZlibCodec(ZlibCompressionMode NMMPBADCFHK)
	{
		switch (NMMPBADCFHK)
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

	public int InitializeInflate(bool EKEOIGPLABK)
	{
		return InitializeInflate(WindowBits, EKEOIGPLABK);
	}

	public int InitializeInflate(int KGFELFAKFIA)
	{
		WindowBits = KGFELFAKFIA;
		return InitializeInflate(KGFELFAKFIA, true);
	}

	public int InitializeInflate(int KGFELFAKFIA, bool EKEOIGPLABK)
	{
		WindowBits = KGFELFAKFIA;
		if (DeflateState != null)
		{
			throw new ZlibException("You may not call InitializeInflate() after calling InitializeDeflate().");
		}
		InflateState = new InflateManager(EKEOIGPLABK);
		return InflateState.Initialize(this, KGFELFAKFIA);
	}

	public int Inflate(FlushType NGBJDNFAPKC)
	{
		if (InflateState == null)
		{
			throw new ZlibException("No Inflate State!");
		}
		return InflateState.Inflate(NGBJDNFAPKC);
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

	public int InitializeDeflate(ZlibCompressionLevel GNLOCMLBNHF)
	{
		CompressLevel = GNLOCMLBNHF;
		return InitializeDeflateInternal(true);
	}

	public int InitializeDeflate(ZlibCompressionLevel GNLOCMLBNHF, bool JIHPEOOBCBG)
	{
		CompressLevel = GNLOCMLBNHF;
		return InitializeDeflateInternal(JIHPEOOBCBG);
	}

	public int InitializeDeflate(ZlibCompressionLevel GNLOCMLBNHF, int HLFOKLCKNEE)
	{
		CompressLevel = GNLOCMLBNHF;
		WindowBits = HLFOKLCKNEE;
		return InitializeDeflateInternal(true);
	}

	public int InitializeDeflate(ZlibCompressionLevel GNLOCMLBNHF, int HLFOKLCKNEE, bool JIHPEOOBCBG)
	{
		CompressLevel = GNLOCMLBNHF;
		WindowBits = HLFOKLCKNEE;
		return InitializeDeflateInternal(JIHPEOOBCBG);
	}

	private int InitializeDeflateInternal(bool JIHPEOOBCBG)
	{
		if (InflateState != null)
		{
			throw new ZlibException("You may not call InitializeDeflate() after calling InitializeInflate().");
		}
		DeflateState = new DeflateManager();
		DeflateState.SetWantRfc1950HeaderBytes(JIHPEOOBCBG);
		return DeflateState.Initialize(this, CompressLevel, WindowBits, Strategy);
	}

	public int Deflate(FlushType NGBJDNFAPKC)
	{
		if (DeflateState == null)
		{
			throw new ZlibException("No Deflate State!");
		}
		return DeflateState.Deflate(NGBJDNFAPKC);
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

	public int SetDeflateParams(ZlibCompressionLevel GNLOCMLBNHF, CompressionStrategy FNLGJNHJCPL)
	{
		if (DeflateState == null)
		{
			throw new ZlibException("No Deflate State!");
		}
		return DeflateState.SetParams(GNLOCMLBNHF, FNLGJNHJCPL);
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

	internal int read_buf(byte[] HLDLIFPJMOA, int ILENLCMAMBH, int PEEOEOMEBFG)
	{
		int num = AvailableBytesIn;
		if (num > PEEOEOMEBFG)
		{
			num = PEEOEOMEBFG;
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
		Array.Copy(InputBuffer, NextIn, HLDLIFPJMOA, ILENLCMAMBH, num);
		NextIn += num;
		TotalBytesIn += num;
		return num;
	}
}
