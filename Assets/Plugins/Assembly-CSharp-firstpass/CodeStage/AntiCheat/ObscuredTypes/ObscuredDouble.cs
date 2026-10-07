using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	[Serializable]
	public struct ObscuredDouble : IEquatable<ObscuredDouble>, IFormattable
	{
		[StructLayout(LayoutKind.Explicit)]
		private struct DoubleLongBytesUnion
		{
			[FieldOffset(0)]
			public double d;

			[FieldOffset(0)]
			public long l;

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
		}

		private static long cryptoKey = 210987L;

		[SerializeField]
		private long currentCryptoKey;

		[SerializeField]
		private byte[] hiddenValue;

		[SerializeField]
		private double fakeValue;

		[SerializeField]
		private bool inited;

		private ObscuredDouble(byte[] value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = 0.0;
			inited = true;
		}

		public static void SetNewCryptoKey(long CNOFJICCAHK)
		{
			cryptoKey = CNOFJICCAHK;
		}

		public static long Encrypt(double value)
		{
			return Encrypt(value, cryptoKey);
		}

		public static long Encrypt(double value, long KGBGENDIMBC)
		{
			DoubleLongBytesUnion oGDAPDCFLOF = new DoubleLongBytesUnion
			{
				d = value
			};
			oGDAPDCFLOF.l ^= KGBGENDIMBC;
			return oGDAPDCFLOF.l;
		}

		private static byte[] InternalEncrypt(double value)
		{
			return InternalEncrypt(value, 0L);
		}

		private static byte[] InternalEncrypt(double value, long KGBGENDIMBC)
		{
			long num = KGBGENDIMBC;
			if (num == 0)
			{
				num = cryptoKey;
			}
			DoubleLongBytesUnion oGDAPDCFLOF = new DoubleLongBytesUnion
			{
				d = value
			};
			oGDAPDCFLOF.l ^= num;
			return new byte[8] { oGDAPDCFLOF.b1, oGDAPDCFLOF.b2, oGDAPDCFLOF.b3, oGDAPDCFLOF.b4, oGDAPDCFLOF.b5, oGDAPDCFLOF.b6, oGDAPDCFLOF.b7, oGDAPDCFLOF.b8 };
		}

		public static double Decrypt(long value)
		{
			return Decrypt(value, cryptoKey);
		}

		public static double Decrypt(long value, long KGBGENDIMBC)
		{
			DoubleLongBytesUnion oGDAPDCFLOF = new DoubleLongBytesUnion
			{
				l = (value ^ KGBGENDIMBC)
			};
			return oGDAPDCFLOF.d;
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
			double bAINMLLIKOL = InternalDecrypt();
			currentCryptoKey = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			hiddenValue = InternalEncrypt(bAINMLLIKOL, currentCryptoKey);
		}

		public long GetEncrypted()
		{
			ApplyNewCryptoKey();
			DoubleLongBytesUnion oGDAPDCFLOF = new DoubleLongBytesUnion
			{
				b1 = hiddenValue[0],
				b2 = hiddenValue[1],
				b3 = hiddenValue[2],
				b4 = hiddenValue[3],
				b5 = hiddenValue[4],
				b6 = hiddenValue[5],
				b7 = hiddenValue[6],
				b8 = hiddenValue[7]
			};
			return oGDAPDCFLOF.l;
		}

		public void SetEncrypted(long ANGFOBEKKKD)
		{
			inited = true;
			DoubleLongBytesUnion oGDAPDCFLOF = new DoubleLongBytesUnion
			{
				l = ANGFOBEKKKD
			};
			hiddenValue = new byte[8] { oGDAPDCFLOF.b1, oGDAPDCFLOF.b2, oGDAPDCFLOF.b3, oGDAPDCFLOF.b4, oGDAPDCFLOF.b5, oGDAPDCFLOF.b6, oGDAPDCFLOF.b7, oGDAPDCFLOF.b8 };
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				fakeValue = InternalDecrypt();
			}
		}

		private double InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = InternalEncrypt(0.0);
				fakeValue = 0.0;
				inited = true;
			}
			DoubleLongBytesUnion oGDAPDCFLOF = new DoubleLongBytesUnion
			{
				b1 = hiddenValue[0],
				b2 = hiddenValue[1],
				b3 = hiddenValue[2],
				b4 = hiddenValue[3],
				b5 = hiddenValue[4],
				b6 = hiddenValue[5],
				b7 = hiddenValue[6],
				b8 = hiddenValue[7]
			};
			oGDAPDCFLOF.l ^= currentCryptoKey;
			double oFMGDFKHPDO = oGDAPDCFLOF.d;
			if (ObscuredCheatingDetector.GetIsRunning() && fakeValue != 0.0 && Math.Abs(oFMGDFKHPDO - fakeValue) > 1E-06)
			{
				ObscuredCheatingDetector.get_Instance().OnCheatingDetected();
			}
			return oFMGDFKHPDO;
		}

		public static implicit operator ObscuredDouble(double value)
		{
			ObscuredDouble result = new ObscuredDouble(InternalEncrypt(value));
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				result.fakeValue = value;
			}
			return result;
		}

		public static implicit operator double(ObscuredDouble value)
		{
			return value.InternalDecrypt();
		}

		[SpecialName]
		public static ObscuredDouble op_Increment(ObscuredDouble NILNDHEKNLJ)
		{
			double bAINMLLIKOL = NILNDHEKNLJ.InternalDecrypt() + 1.0;
			NILNDHEKNLJ.hiddenValue = InternalEncrypt(bAINMLLIKOL, NILNDHEKNLJ.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				NILNDHEKNLJ.fakeValue = bAINMLLIKOL;
			}
			return NILNDHEKNLJ;
		}

		[SpecialName]
		public static ObscuredDouble op_Decrement(ObscuredDouble NILNDHEKNLJ)
		{
			double bAINMLLIKOL = NILNDHEKNLJ.InternalDecrypt() - 1.0;
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
			if (!(AOMLCBHAJJH is ObscuredDouble))
			{
				return false;
			}
			return Equals((ObscuredDouble)AOMLCBHAJJH);
		}

		public bool Equals(ObscuredDouble AOMLCBHAJJH)
		{
			return AOMLCBHAJJH.InternalDecrypt().Equals(InternalDecrypt());
		}

		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}
	}
}
