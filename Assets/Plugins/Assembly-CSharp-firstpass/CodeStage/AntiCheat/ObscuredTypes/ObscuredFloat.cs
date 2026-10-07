using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	[Serializable]
	public struct ObscuredFloat : IEquatable<ObscuredFloat>, IFormattable
	{
		[StructLayout(LayoutKind.Explicit)]
		private struct FloatIntBytesUnion
		{
			[FieldOffset(0)]
			public float f;

			[FieldOffset(0)]
			public int i;

			[FieldOffset(0)]
			public byte b1;

			[FieldOffset(1)]
			public byte b2;

			[FieldOffset(2)]
			public byte b3;

			[FieldOffset(3)]
			public byte b4;
		}

		private static int cryptoKey = 230887;

		[SerializeField]
		private int currentCryptoKey;

		[SerializeField]
		private byte[] hiddenValue;

		[SerializeField]
		private float fakeValue;

		[SerializeField]
		private bool inited;

		private ObscuredFloat(byte[] value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = 0f;
			inited = true;
		}

		public static void SetNewCryptoKey(int CNOFJICCAHK)
		{
			cryptoKey = CNOFJICCAHK;
		}

		public static int Encrypt(float value)
		{
			return Encrypt(value, cryptoKey);
		}

		public static int Encrypt(float value, int KGBGENDIMBC)
		{
			FloatIntBytesUnion eHLBLJICLKD = new FloatIntBytesUnion
			{
				f = value
			};
			eHLBLJICLKD.i ^= KGBGENDIMBC;
			return eHLBLJICLKD.i;
		}

		private static byte[] InternalEncrypt(float value)
		{
			return InternalEncrypt(value, 0);
		}

		private static byte[] InternalEncrypt(float value, int KGBGENDIMBC)
		{
			int num = KGBGENDIMBC;
			if (num == 0)
			{
				num = cryptoKey;
			}
			FloatIntBytesUnion eHLBLJICLKD = new FloatIntBytesUnion
			{
				f = value
			};
			eHLBLJICLKD.i ^= num;
			return new byte[4] { eHLBLJICLKD.b1, eHLBLJICLKD.b2, eHLBLJICLKD.b3, eHLBLJICLKD.b4 };
		}

		public static float Decrypt(int value)
		{
			return Decrypt(value, cryptoKey);
		}

		public static float Decrypt(int value, int KGBGENDIMBC)
		{
			FloatIntBytesUnion eHLBLJICLKD = new FloatIntBytesUnion
			{
				i = (value ^ KGBGENDIMBC)
			};
			return eHLBLJICLKD.f;
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
			float bAINMLLIKOL = InternalDecrypt();
			currentCryptoKey = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			hiddenValue = InternalEncrypt(bAINMLLIKOL, currentCryptoKey);
		}

		public int GetEncrypted()
		{
			ApplyNewCryptoKey();
			FloatIntBytesUnion eHLBLJICLKD = new FloatIntBytesUnion
			{
				b1 = hiddenValue[0],
				b2 = hiddenValue[1],
				b3 = hiddenValue[2],
				b4 = hiddenValue[3]
			};
			return eHLBLJICLKD.i;
		}

		public void SetEncrypted(int ANGFOBEKKKD)
		{
			inited = true;
			FloatIntBytesUnion eHLBLJICLKD = new FloatIntBytesUnion
			{
				i = ANGFOBEKKKD
			};
			hiddenValue = new byte[4] { eHLBLJICLKD.b1, eHLBLJICLKD.b2, eHLBLJICLKD.b3, eHLBLJICLKD.b4 };
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				fakeValue = InternalDecrypt();
			}
		}

		private float InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = InternalEncrypt(0f);
				fakeValue = 0f;
				inited = true;
			}
			FloatIntBytesUnion eHLBLJICLKD = new FloatIntBytesUnion
			{
				b1 = hiddenValue[0],
				b2 = hiddenValue[1],
				b3 = hiddenValue[2],
				b4 = hiddenValue[3]
			};
			eHLBLJICLKD.i ^= currentCryptoKey;
			float jKBEIEPBHOD = eHLBLJICLKD.f;
			if (ObscuredCheatingDetector.GetIsRunning() && fakeValue != 0f && Math.Abs(jKBEIEPBHOD - fakeValue) > ObscuredCheatingDetector.get_Instance().floatEpsilon)
			{
				ObscuredCheatingDetector.get_Instance().OnCheatingDetected();
			}
			return jKBEIEPBHOD;
		}

		public static implicit operator ObscuredFloat(float value)
		{
			ObscuredFloat result = new ObscuredFloat(InternalEncrypt(value));
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				result.fakeValue = value;
			}
			return result;
		}

		public static implicit operator float(ObscuredFloat value)
		{
			return value.InternalDecrypt();
		}

		[SpecialName]
		public static ObscuredFloat op_Increment(ObscuredFloat NILNDHEKNLJ)
		{
			float bAINMLLIKOL = NILNDHEKNLJ.InternalDecrypt() + 1f;
			NILNDHEKNLJ.hiddenValue = InternalEncrypt(bAINMLLIKOL, NILNDHEKNLJ.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				NILNDHEKNLJ.fakeValue = bAINMLLIKOL;
			}
			return NILNDHEKNLJ;
		}

		[SpecialName]
		public static ObscuredFloat op_Decrement(ObscuredFloat NILNDHEKNLJ)
		{
			float bAINMLLIKOL = NILNDHEKNLJ.InternalDecrypt() - 1f;
			NILNDHEKNLJ.hiddenValue = InternalEncrypt(bAINMLLIKOL, NILNDHEKNLJ.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				NILNDHEKNLJ.fakeValue = bAINMLLIKOL;
			}
			return NILNDHEKNLJ;
		}

		public override bool Equals(object AOMLCBHAJJH)
		{
			if (!(AOMLCBHAJJH is ObscuredFloat))
			{
				return false;
			}
			return Equals((ObscuredFloat)AOMLCBHAJJH);
		}

		public bool Equals(ObscuredFloat AOMLCBHAJJH)
		{
			double num = AOMLCBHAJJH.InternalDecrypt();
			double obj = InternalDecrypt();
			return num.Equals(obj);
		}

		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
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
	}
}
