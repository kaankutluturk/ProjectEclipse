using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	[Serializable]
	public struct ObscuredDecimal : IEquatable<ObscuredDecimal>, IFormattable
	{
		[StructLayout(LayoutKind.Explicit)]
		private struct DecimalLongBytesUnion
		{
			[FieldOffset(0)]
			public decimal d;

			[FieldOffset(0)]
			public long l1;

			[FieldOffset(8)]
			public long l2;

			[FieldOffset(0)]
			public byte b1;

			[FieldOffset(1)]
			public byte b2;

			[FieldOffset(2)]
			public byte b3;

			[FieldOffset(3)]
			public byte b4;

			[FieldOffset(4)]
			public byte b5;

			[FieldOffset(5)]
			public byte b6;

			[FieldOffset(6)]
			public byte b7;

			[FieldOffset(7)]
			public byte b8;

			[FieldOffset(8)]
			public byte b9;

			[FieldOffset(9)]
			public byte b10;

			[FieldOffset(10)]
			public byte b11;

			[FieldOffset(11)]
			public byte b12;

			[FieldOffset(12)]
			public byte b13;

			[FieldOffset(13)]
			public byte b14;

			[FieldOffset(14)]
			public byte b15;

			[FieldOffset(15)]
			public byte b16;
		}

		private static long cryptoKey = 209208L;

		private long currentCryptoKey;

		private byte[] hiddenValue;

		private decimal fakeValue;

		private bool inited;

		private ObscuredDecimal(byte[] value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = 0m;
			inited = true;
		}

		public static void SetNewCryptoKey(long CNOFJICCAHK)
		{
			cryptoKey = CNOFJICCAHK;
		}

		public static decimal Encrypt(decimal value)
		{
			return Encrypt(value, cryptoKey);
		}

		public static decimal Encrypt(decimal value, long KGBGENDIMBC)
		{
			DecimalLongBytesUnion fFMMPKOPPGG = new DecimalLongBytesUnion
			{
				d = value
			};
			fFMMPKOPPGG.l1 ^= KGBGENDIMBC;
			fFMMPKOPPGG.l2 ^= KGBGENDIMBC;
			return fFMMPKOPPGG.d;
		}

		private static byte[] InternalEncrypt(decimal value)
		{
			return InternalEncrypt(value, 0L);
		}

		private static byte[] InternalEncrypt(decimal value, long KGBGENDIMBC)
		{
			long num = KGBGENDIMBC;
			if (num == 0)
			{
				num = cryptoKey;
			}
			DecimalLongBytesUnion fFMMPKOPPGG = new DecimalLongBytesUnion
			{
				d = value
			};
			fFMMPKOPPGG.l1 ^= num;
			fFMMPKOPPGG.l2 ^= num;
			return new byte[16]
			{
				fFMMPKOPPGG.b1, fFMMPKOPPGG.b2, fFMMPKOPPGG.b3, fFMMPKOPPGG.b4, fFMMPKOPPGG.b5, fFMMPKOPPGG.b6, fFMMPKOPPGG.b7, fFMMPKOPPGG.b8, fFMMPKOPPGG.b9, fFMMPKOPPGG.b10,
				fFMMPKOPPGG.b11, fFMMPKOPPGG.b12, fFMMPKOPPGG.b13, fFMMPKOPPGG.b14, fFMMPKOPPGG.b15, fFMMPKOPPGG.b16
			};
		}

		public static decimal Decrypt(decimal value)
		{
			return Decrypt(value, cryptoKey);
		}

		public static decimal Decrypt(decimal value, long KGBGENDIMBC)
		{
			DecimalLongBytesUnion fFMMPKOPPGG = new DecimalLongBytesUnion
			{
				d = value
			};
			fFMMPKOPPGG.l1 ^= KGBGENDIMBC;
			fFMMPKOPPGG.l2 ^= KGBGENDIMBC;
			return fFMMPKOPPGG.d;
		}

		public void ApplyNewCryptoKey()
		{
			if (currentCryptoKey != cryptoKey)
			{
				hiddenValue = InternalEncrypt(InternalDecrypt(), cryptoKey);
				currentCryptoKey = cryptoKey;
			}
		}

		public void RandomizeCryptoKey()
		{
			decimal bAINMLLIKOL = InternalDecrypt();
			currentCryptoKey = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			hiddenValue = InternalEncrypt(bAINMLLIKOL, currentCryptoKey);
		}

		public decimal GetEncrypted()
		{
			ApplyNewCryptoKey();
			DecimalLongBytesUnion fFMMPKOPPGG = new DecimalLongBytesUnion
			{
				b1 = hiddenValue[0],
				b2 = hiddenValue[1],
				b3 = hiddenValue[2],
				b4 = hiddenValue[3],
				b5 = hiddenValue[4],
				b6 = hiddenValue[5],
				b7 = hiddenValue[6],
				b8 = hiddenValue[7],
				b9 = hiddenValue[8],
				b10 = hiddenValue[9],
				b11 = hiddenValue[10],
				b12 = hiddenValue[11],
				b13 = hiddenValue[12],
				b14 = hiddenValue[13],
				b15 = hiddenValue[14],
				b16 = hiddenValue[15]
			};
			return fFMMPKOPPGG.d;
		}

		public void SetEncrypted(decimal ANGFOBEKKKD)
		{
			inited = true;
			DecimalLongBytesUnion fFMMPKOPPGG = new DecimalLongBytesUnion
			{
				d = ANGFOBEKKKD
			};
			hiddenValue = new byte[16]
			{
				fFMMPKOPPGG.b1, fFMMPKOPPGG.b2, fFMMPKOPPGG.b3, fFMMPKOPPGG.b4, fFMMPKOPPGG.b5, fFMMPKOPPGG.b6, fFMMPKOPPGG.b7, fFMMPKOPPGG.b8, fFMMPKOPPGG.b9, fFMMPKOPPGG.b10,
				fFMMPKOPPGG.b11, fFMMPKOPPGG.b12, fFMMPKOPPGG.b13, fFMMPKOPPGG.b14, fFMMPKOPPGG.b15, fFMMPKOPPGG.b16
			};
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				fakeValue = InternalDecrypt();
			}
		}

		private decimal InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = InternalEncrypt(0m);
				fakeValue = 0m;
				inited = true;
			}
			DecimalLongBytesUnion fFMMPKOPPGG = new DecimalLongBytesUnion
			{
				b1 = hiddenValue[0],
				b2 = hiddenValue[1],
				b3 = hiddenValue[2],
				b4 = hiddenValue[3],
				b5 = hiddenValue[4],
				b6 = hiddenValue[5],
				b7 = hiddenValue[6],
				b8 = hiddenValue[7],
				b9 = hiddenValue[8],
				b10 = hiddenValue[9],
				b11 = hiddenValue[10],
				b12 = hiddenValue[11],
				b13 = hiddenValue[12],
				b14 = hiddenValue[13],
				b15 = hiddenValue[14],
				b16 = hiddenValue[15]
			};
			fFMMPKOPPGG.l1 ^= currentCryptoKey;
			fFMMPKOPPGG.l2 ^= currentCryptoKey;
			decimal oFMGDFKHPDO = fFMMPKOPPGG.d;
			if (ObscuredCheatingDetector.GetIsRunning() && fakeValue != 0m && oFMGDFKHPDO != fakeValue)
			{
				ObscuredCheatingDetector.get_Instance().OnCheatingDetected();
			}
			return oFMGDFKHPDO;
		}

		public static implicit operator ObscuredDecimal(decimal value)
		{
			ObscuredDecimal result = new ObscuredDecimal(InternalEncrypt(value));
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				result.fakeValue = value;
			}
			return result;
		}

		public static implicit operator decimal(ObscuredDecimal value)
		{
			return value.InternalDecrypt();
		}

		public static explicit operator ObscuredDecimal(ObscuredFloat f)
		{
			return (ObscuredDecimal)((decimal)(float)(f));
		}

		[SpecialName]
		public static ObscuredDecimal op_Increment(ObscuredDecimal NILNDHEKNLJ)
		{
			decimal bAINMLLIKOL = NILNDHEKNLJ.InternalDecrypt() + 1m;
			NILNDHEKNLJ.hiddenValue = InternalEncrypt(bAINMLLIKOL, NILNDHEKNLJ.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				NILNDHEKNLJ.fakeValue = bAINMLLIKOL;
			}
			return NILNDHEKNLJ;
		}

		[SpecialName]
		public static ObscuredDecimal op_Decrement(ObscuredDecimal NILNDHEKNLJ)
		{
			decimal bAINMLLIKOL = NILNDHEKNLJ.InternalDecrypt() - 1m;
			NILNDHEKNLJ.hiddenValue = InternalEncrypt(bAINMLLIKOL, NILNDHEKNLJ.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				NILNDHEKNLJ.fakeValue = bAINMLLIKOL;
			}
			return NILNDHEKNLJ;
		}

		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}

		public string ToString(string LBOHOKIBHOH)
		{
			return InternalDecrypt().ToString(LBOHOKIBHOH);
		}

		public string ToString(IFormatProvider EEGMFLOPLLH)
		{
			return InternalDecrypt().ToString(EEGMFLOPLLH);
		}

		public string ToString(string LBOHOKIBHOH, IFormatProvider EEGMFLOPLLH)
		{
			return InternalDecrypt().ToString(LBOHOKIBHOH, EEGMFLOPLLH);
		}

		public override bool Equals(object AOMLCBHAJJH)
		{
			if (!(AOMLCBHAJJH is ObscuredDecimal))
			{
				return false;
			}
			return Equals((ObscuredDecimal)AOMLCBHAJJH);
		}

		public bool Equals(ObscuredDecimal AOMLCBHAJJH)
		{
			return AOMLCBHAJJH.InternalDecrypt().Equals(InternalDecrypt());
		}

		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}
	}
}
