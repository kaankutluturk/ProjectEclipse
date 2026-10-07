using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

internal class ZlibBaseStream : Stream
{
	internal enum StreamMode
	{
		Writer = 0,
		Reader = 1,
		Undefined = 2
	}

	protected internal ZlibCodec _z;

	protected internal StreamMode _streamMode = StreamMode.Undefined;

	protected internal FlushType _flushMode;

	protected internal ZlibStreamFlavor _flavor;

	protected internal ZlibCompressionMode _compressionMode;

	protected internal ZlibCompressionLevel _level;

	protected internal bool _leaveOpen;

	protected internal byte[] _workingBuffer;

	protected internal int _bufferSize = 16384;

	protected internal byte[] _buf1 = new byte[1];

	protected internal Stream _stream;

	protected internal CompressionStrategy Strategy;

	private CRC32 _crc;

	protected internal string _gzipFileName;

	protected internal string _gzipComment;

	protected internal DateTime _GzipMtime;

	protected internal int _gzipHeaderByteCount;

	private bool _noMoreInput;

	internal int Crc32
	{
		get
		{
			return GetCrc32();
		}
	}

	protected internal bool WantCompress
	{
		get
		{
			return GetWantCompress();
		}
	}

	private ZlibCodec Codec
	{
		get
		{
			return GetCodec();
		}
	}

	private byte[] WorkingBuffer
	{
		get
		{
			return GetWorkingBuffer();
		}
	}

	public ZlibBaseStream(Stream ABJIEFMMIEK, ZlibCompressionMode HCCDFEPLGBA, ZlibCompressionLevel GNLOCMLBNHF, ZlibStreamFlavor CENOEIJNIAG, bool LOLBAGJKKPH)
	{
		_flushMode = FlushType.None;
		_stream = ABJIEFMMIEK;
		_leaveOpen = LOLBAGJKKPH;
		_compressionMode = HCCDFEPLGBA;
		_flavor = CENOEIJNIAG;
		_level = GNLOCMLBNHF;
		if (CENOEIJNIAG == ZlibStreamFlavor.GZIP)
		{
			_crc = new CRC32();
		}
	}

	internal int GetCrc32()
	{
		if (_crc == null)
		{
			return 0;
		}
		return _crc.GetCrc32Result();
	}

	protected internal bool GetWantCompress()
	{
		return _compressionMode == ZlibCompressionMode.Compress;
	}

	private ZlibCodec GetCodec()
	{
		if (_z == null)
		{
			bool flag = _flavor == ZlibStreamFlavor.ZLIB;
			_z = new ZlibCodec();
			if (_compressionMode == ZlibCompressionMode.Decompress)
			{
				_z.InitializeInflate(flag);
			}
			else
			{
				_z.Strategy = Strategy;
				_z.InitializeDeflate(_level, flag);
			}
		}
		return _z;
	}

	private byte[] GetWorkingBuffer()
	{
		if (_workingBuffer == null)
		{
			_workingBuffer = new byte[_bufferSize];
		}
		return _workingBuffer;
	}

	public override void Write(byte[] buffer, int IPCOBJBKNAO, int count)
	{
		if (_crc != null)
		{
			_crc.SlurpBlock(buffer, IPCOBJBKNAO, count);
		}
		if (_streamMode == StreamMode.Undefined)
		{
			_streamMode = StreamMode.Writer;
		}
		else if (_streamMode != StreamMode.Writer)
		{
			throw new ZlibException("Cannot Write after Reading.");
		}
		if (count == 0)
		{
			return;
		}
		GetCodec().InputBuffer = buffer;
		_z.NextIn = IPCOBJBKNAO;
		_z.AvailableBytesIn = count;
		bool flag = false;
		do
		{
			_z.OutputBuffer = GetWorkingBuffer();
			_z.NextOut = 0;
			_z.AvailableBytesOut = _workingBuffer.Length;
			int num = ((!GetWantCompress()) ? _z.Inflate(_flushMode) : _z.Deflate(_flushMode));
			if (num != 0 && num != 1)
			{
				throw new ZlibException(((!GetWantCompress()) ? "in" : "de") + "flating: " + _z.Message);
			}
			_stream.Write(_workingBuffer, 0, _workingBuffer.Length - _z.AvailableBytesOut);
			flag = _z.AvailableBytesIn == 0 && _z.AvailableBytesOut != 0;
			if (_flavor == ZlibStreamFlavor.GZIP && !GetWantCompress())
			{
				flag = _z.AvailableBytesIn == 8 && _z.AvailableBytesOut != 0;
			}
		}
		while (!flag);
	}

	private void Finish()
	{
		if (_z == null)
		{
			return;
		}
		if (_streamMode == StreamMode.Writer)
		{
			bool flag = false;
			do
			{
				_z.OutputBuffer = GetWorkingBuffer();
				_z.NextOut = 0;
				_z.AvailableBytesOut = _workingBuffer.Length;
				int num = ((!GetWantCompress()) ? _z.Inflate(FlushType.Finish) : _z.Deflate(FlushType.Finish));
				if (num != 1 && num != 0)
				{
					string text = ((!GetWantCompress()) ? "in" : "de") + "flating";
					if (_z.Message == null)
					{
						throw new ZlibException(string.Format("{0}: (rc = {1})", text, num));
					}
					throw new ZlibException(text + ": " + _z.Message);
				}
				if (_workingBuffer.Length - _z.AvailableBytesOut > 0)
				{
					_stream.Write(_workingBuffer, 0, _workingBuffer.Length - _z.AvailableBytesOut);
				}
				flag = _z.AvailableBytesIn == 0 && _z.AvailableBytesOut != 0;
				if (_flavor == ZlibStreamFlavor.GZIP && !GetWantCompress())
				{
					flag = _z.AvailableBytesIn == 8 && _z.AvailableBytesOut != 0;
				}
			}
			while (!flag);
			Flush();
			if (_flavor == ZlibStreamFlavor.GZIP)
			{
				if (!GetWantCompress())
				{
					throw new ZlibException("Writing with decompression is not supported.");
				}
				int value = _crc.GetCrc32Result();
				_stream.Write(BitConverter.GetBytes(value), 0, 4);
				int value2 = (int)(_crc.GetTotalBytesRead() & 0xFFFFFFFFu);
				_stream.Write(BitConverter.GetBytes(value2), 0, 4);
			}
		}
		else
		{
			if (_streamMode != StreamMode.Reader || _flavor != ZlibStreamFlavor.GZIP)
			{
				return;
			}
			if (GetWantCompress())
			{
				throw new ZlibException("Reading with compression is not supported.");
			}
			if (_z.TotalBytesOut == 0)
			{
				return;
			}
			byte[] array = new byte[8];
			if (_z.AvailableBytesIn < 8)
			{
				Array.Copy(_z.InputBuffer, _z.NextIn, array, 0, _z.AvailableBytesIn);
				int num2 = 8 - _z.AvailableBytesIn;
				int num3 = _stream.Read(array, _z.AvailableBytesIn, num2);
				if (num2 != num3)
				{
					throw new ZlibException(string.Format("Missing or incomplete GZIP trailer. Expected 8 bytes, got {0}.", _z.AvailableBytesIn + num3));
				}
			}
			else
			{
				Array.Copy(_z.InputBuffer, _z.NextIn, array, 0, array.Length);
			}
			int num4 = BitConverter.ToInt32(array, 0);
			int num5 = _crc.GetCrc32Result();
			int num6 = BitConverter.ToInt32(array, 4);
			int num7 = (int)(_z.TotalBytesOut & 0xFFFFFFFFu);
			if (num5 != num4)
			{
				throw new ZlibException(string.Format("Bad CRC32 in GZIP trailer. (actual({0:X8})!=expected({1:X8}))", num5, num4));
			}
			if (num7 != num6)
			{
				throw new ZlibException(string.Format("Bad size in GZIP trailer. (actual({0})!=expected({1}))", num7, num6));
			}
		}
	}

	private void End()
	{
		if (GetCodec() != null)
		{
			if (GetWantCompress())
			{
				_z.EndDeflate();
			}
			else
			{
				_z.EndInflate();
			}
			_z = null;
		}
	}

	public override void Close()
	{
		if (_stream == null)
		{
			return;
		}
		try
		{
			Finish();
		}
		finally
		{
			End();
			if (!_leaveOpen)
			{
				_stream.Dispose();
			}
			_stream = null;
		}
	}

	public override void Flush()
	{
		_stream.Flush();
	}

	public override long Position
	{
		get
		{
			throw new NotImplementedException();
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
		_stream.SetLength(value);
	}

	private string ReadZeroTerminatedString()
	{
		List<byte> list = new List<byte>();
		bool flag = false;
		do
		{
			int num = _stream.Read(_buf1, 0, 1);
			if (num != 1)
			{
				throw new ZlibException("Unexpected EOF reading GZIP header.");
			}
			if (_buf1[0] == 0)
			{
				flag = true;
			}
			else
			{
				list.Add(_buf1[0]);
			}
		}
		while (!flag);
		byte[] array = list.ToArray();
		return GZipCompressionStream.iso8859dash1.GetString(array, 0, array.Length);
	}

	private int ReadAndValidateGzipHeader()
	{
		int num = 0;
		byte[] array = new byte[10];
		int num2 = _stream.Read(array, 0, array.Length);
		switch (num2)
		{
		case 0:
			return 0;
		default:
			throw new ZlibException("Not a valid GZIP stream.");
		case 10:
		{
			if (array[0] != 31 || array[1] != 139 || array[2] != 8)
			{
				throw new ZlibException("Bad GZIP header.");
			}
			int num3 = BitConverter.ToInt32(array, 4);
			_GzipMtime = GZipCompressionStream._unixEpoch.AddSeconds(num3);
			num += num2;
			if ((array[3] & 4) == 4)
			{
				num2 = _stream.Read(array, 0, 2);
				num += num2;
				short num4 = (short)(array[0] + array[1] * 256);
				byte[] array2 = new byte[num4];
				num2 = _stream.Read(array2, 0, array2.Length);
				if (num2 != num4)
				{
					throw new ZlibException("Unexpected end-of-file reading GZIP header.");
				}
				num += num2;
			}
			if ((array[3] & 8) == 8)
			{
				_gzipFileName = ReadZeroTerminatedString();
			}
			if ((array[3] & 0x10) == 16)
			{
				_gzipComment = ReadZeroTerminatedString();
			}
			if ((array[3] & 2) == 2)
			{
				Read(_buf1, 0, 1);
			}
			return num;
		}
		}
	}

	public override int Read(byte[] buffer, int IPCOBJBKNAO, int count)
	{
		if (_streamMode == StreamMode.Undefined)
		{
			if (!_stream.CanRead)
			{
				throw new ZlibException("The stream is not readable.");
			}
			_streamMode = StreamMode.Reader;
			GetCodec().AvailableBytesIn = 0;
			if (_flavor == ZlibStreamFlavor.GZIP)
			{
				_gzipHeaderByteCount = ReadAndValidateGzipHeader();
				if (_gzipHeaderByteCount == 0)
				{
					return 0;
				}
			}
		}
		if (_streamMode != StreamMode.Reader)
		{
			throw new ZlibException("Cannot Read after Writing.");
		}
		if (count == 0)
		{
			return 0;
		}
		if (_noMoreInput && GetWantCompress())
		{
			return 0;
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (IPCOBJBKNAO < buffer.GetLowerBound(0))
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (IPCOBJBKNAO + count > buffer.GetLength(0))
		{
			throw new ArgumentOutOfRangeException("count");
		}
		int num = 0;
		_z.OutputBuffer = buffer;
		_z.NextOut = IPCOBJBKNAO;
		_z.AvailableBytesOut = count;
		_z.InputBuffer = GetWorkingBuffer();
		do
		{
			if (_z.AvailableBytesIn == 0 && !_noMoreInput)
			{
				_z.NextIn = 0;
				_z.AvailableBytesIn = _stream.Read(_workingBuffer, 0, _workingBuffer.Length);
				if (_z.AvailableBytesIn == 0)
				{
					_noMoreInput = true;
				}
			}
			num = ((!GetWantCompress()) ? _z.Inflate(_flushMode) : _z.Deflate(_flushMode));
			if (_noMoreInput && num == -5)
			{
				return 0;
			}
			if (num != 0 && num != 1)
			{
				throw new ZlibException(string.Format("{0}flating:  rc={1}  msg={2}", (!GetWantCompress()) ? "in" : "de", num, _z.Message));
			}
		}
		while (((!_noMoreInput && num != 1) || _z.AvailableBytesOut != count) && _z.AvailableBytesOut > 0 && !_noMoreInput && num == 0);
		if (_z.AvailableBytesOut > 0)
		{
			if (num != 0 || _z.AvailableBytesIn == 0)
			{
			}
			if (_noMoreInput && GetWantCompress())
			{
				num = _z.Deflate(FlushType.Finish);
				if (num != 0 && num != 1)
				{
					throw new ZlibException(string.Format("Deflating:  rc={0}  msg={1}", num, _z.Message));
				}
			}
		}
		num = count - _z.AvailableBytesOut;
		if (_crc != null)
		{
			_crc.SlurpBlock(buffer, IPCOBJBKNAO, num);
		}
		return num;
	}

	public override bool CanRead
	{
		get
		{
			return _stream.CanRead;
		}
	}
	public override bool CanSeek
	{
		get
		{
			return _stream.CanSeek;
		}
	}
	public override bool CanWrite
	{
		get
		{
			return _stream.CanWrite;
		}
	}
	public override long Length
	{
		get
		{
			return _stream.Length;
		}
	}
	public static void CompressString(string JDCCBCNFENK, Stream ABKOBELCOIK)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(JDCCBCNFENK);
		using (ABKOBELCOIK)
		{
			ABKOBELCOIK.Write(bytes, 0, bytes.Length);
		}
	}

	public static void CompressBuffer(byte[] AAOIAEJJINO, Stream ABKOBELCOIK)
	{
		using (ABKOBELCOIK)
		{
			ABKOBELCOIK.Write(AAOIAEJJINO, 0, AAOIAEJJINO.Length);
		}
	}

	public static string UncompressString(byte[] FCPABLANKDN, Stream INIMCIOFFCJ)
	{
		byte[] array = new byte[1024];
		Encoding uTF = Encoding.UTF8;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (INIMCIOFFCJ)
			{
				int count;
				while ((count = INIMCIOFFCJ.Read(array, 0, array.Length)) != 0)
				{
					memoryStream.Write(array, 0, count);
				}
			}
			memoryStream.Seek(0L, SeekOrigin.Begin);
			StreamReader streamReader = new StreamReader(memoryStream, uTF);
			return streamReader.ReadToEnd();
		}
	}

	public static byte[] UncompressBuffer(byte[] FCPABLANKDN, Stream INIMCIOFFCJ)
	{
		byte[] array = new byte[1024];
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (INIMCIOFFCJ)
			{
				int count;
				while ((count = INIMCIOFFCJ.Read(array, 0, array.Length)) != 0)
				{
					memoryStream.Write(array, 0, count);
				}
			}
			return memoryStream.ToArray();
		}
	}
}
