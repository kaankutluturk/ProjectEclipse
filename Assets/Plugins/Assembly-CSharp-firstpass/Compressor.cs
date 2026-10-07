using System;
using System.Diagnostics;
using System.IO;

public class Compressor
{
	public enum CompressionAlgorithm
	{
		LZMA = 0
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static CompressionAlgorithm algorithm;

	private static int dictionary;

	private static bool eos;

	private static CoderPropID[] propIDs;

	private static object[] properties;

	public static CompressionAlgorithm Algorithm
	{
		get
		{
			return GetAlgorithm();
		}
		set
		{
			SetAlgorithm(value);
		}
	}

	static Compressor()
	{
		dictionary = 8388608;
		eos = false;
		propIDs = new CoderPropID[8]
		{
			CoderPropID.DictionarySize,
			CoderPropID.PosStateBits,
			CoderPropID.LitContextBits,
			CoderPropID.LitPosBits,
			CoderPropID.Algorithm,
			CoderPropID.NumFastBytes,
			CoderPropID.MatchFinder,
			CoderPropID.EndMarker
		};
		properties = new object[8] { dictionary, 2, 3, 0, 2, 128, "bt4", eos };
		SetAlgorithm(CompressionAlgorithm.LZMA);
	}

	public static CompressionAlgorithm GetAlgorithm()
	{
		return algorithm;
	}

	public static void SetAlgorithm(CompressionAlgorithm value)
	{
		algorithm = value;
	}

	public static void CompressFile(string OBAMLJHHPPE, string POBFCEMGIGO, CompressionAlgorithm ABKOBELCOIK = CompressionAlgorithm.LZMA)
	{
		File.WriteAllBytes(OBAMLJHHPPE, Compress(File.ReadAllBytes(POBFCEMGIGO), ABKOBELCOIK));
	}

	public static void DecompressFile(string OBAMLJHHPPE, string OOPMIPCMFPC, CompressionAlgorithm ABKOBELCOIK = CompressionAlgorithm.LZMA)
	{
		File.WriteAllBytes(OOPMIPCMFPC, Decompress(File.ReadAllBytes(OBAMLJHHPPE), ABKOBELCOIK));
	}

	public static byte[] Compress(byte[] APACFLKJCKF, CompressionAlgorithm ABKOBELCOIK = CompressionAlgorithm.LZMA)
	{
		if (ABKOBELCOIK == CompressionAlgorithm.LZMA)
		{
			MemoryStream memoryStream = new MemoryStream(APACFLKJCKF);
			MemoryStream memoryStream2 = new MemoryStream();
			LzmaEncoder mNPBDHNFEBB = new LzmaEncoder();
			mNPBDHNFEBB.SetCoderProperties(propIDs, properties);
			mNPBDHNFEBB.WriteCoderProperties(memoryStream2);
			long length = memoryStream.Length;
			for (int i = 0; i < 8; i++)
			{
				memoryStream2.WriteByte((byte)(length >> 8 * i));
			}
			mNPBDHNFEBB.Code(memoryStream, memoryStream2, -1L, -1L, null);
			return memoryStream2.ToArray();
		}
		return new byte[0];
	}

	public static byte[] Decompress(byte[] APACFLKJCKF, CompressionAlgorithm ABKOBELCOIK = CompressionAlgorithm.LZMA)
	{
		if (ABKOBELCOIK == CompressionAlgorithm.LZMA)
		{
			using (MemoryStream memoryStream = new MemoryStream(APACFLKJCKF))
			{
				LzmaDecoder gDEMLIAGBCB = new LzmaDecoder();
				memoryStream.Seek(0L, SeekOrigin.Begin);
				using (MemoryStream memoryStream2 = new MemoryStream())
				{
					byte[] array = new byte[5];
					if (memoryStream.Read(array, 0, 5) != 5)
					{
						throw new Exception("input .lzma is too short");
					}
					long num = 0L;
					for (int i = 0; i < 8; i++)
					{
						int num2 = memoryStream.ReadByte();
						if (num2 < 0)
						{
							throw new Exception("Can't Read 1");
						}
						num |= (long)(int)(byte)num2 << 8 * i;
					}
					gDEMLIAGBCB.SetDecoderProperties(array);
					long nCKELGLBGJN = memoryStream.Length - memoryStream.Position;
					gDEMLIAGBCB.Code(memoryStream, memoryStream2, nCKELGLBGJN, num, null);
					return memoryStream2.ToArray();
				}
			}
		}
		return new byte[0];
	}

	public static void DecodeStream(FileStream FBGBPGIDKHM, MemoryStream BBBGGJLOCPB)
	{
		LzmaDecoder gDEMLIAGBCB = new LzmaDecoder();
		FBGBPGIDKHM.Seek(0L, SeekOrigin.Begin);
		byte[] array = new byte[5];
		if (FBGBPGIDKHM.Read(array, 0, 5) != 5)
		{
			throw new Exception("input .lzma is too short");
		}
		long num = 0L;
		for (int i = 0; i < 8; i++)
		{
			int num2 = FBGBPGIDKHM.ReadByte();
			if (num2 < 0)
			{
				throw new Exception("Can't Read 1");
			}
			num |= (long)(int)(byte)num2 << 8 * i;
		}
		gDEMLIAGBCB.SetDecoderProperties(array);
		long nCKELGLBGJN = FBGBPGIDKHM.Length - FBGBPGIDKHM.Position;
		gDEMLIAGBCB.Code(FBGBPGIDKHM, BBBGGJLOCPB, nCKELGLBGJN, num, null);
	}
}
