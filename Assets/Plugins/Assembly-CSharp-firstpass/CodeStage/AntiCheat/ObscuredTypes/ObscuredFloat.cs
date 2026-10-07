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

		public static void SetNewCryptoKey(int newKey)
		{
			cryptoKey = newKey;
		}

		public static int Encrypt(float value)
		{
			return Encrypt(value, cryptoKey);
		}

		public static int Encrypt(float value, int key)
		{
			FloatIntBytesUnion union = new FloatIntBytesUnion
			{
				f = value
			};
			union.i ^= key;
			return union.i;
		}

		private static byte[] InternalEncrypt(float value)
		{
			return InternalEncrypt(value, 0);
		}

		private static byte[] InternalEncrypt(float value, int key)
		{
			int num = key;
			if (num == 0)
			{
				num = cryptoKey;
			}
			FloatIntBytesUnion union = new FloatIntBytesUnion
			{
				f = value
			};
			union.i ^= num;
			return new byte[4] { union.b1, union.b2, union.b3, union.b4 };
		}

		public static float Decrypt(int value)
		{
			return Decrypt(value, cryptoKey);
		}

		public static float Decrypt(int value, int key)
		{
			FloatIntBytesUnion union = new FloatIntBytesUnion
			{
				i = (value ^ key)
			};
			return union.f;
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
			float decrypted = InternalDecrypt();
			currentCryptoKey = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			hiddenValue = InternalEncrypt(decrypted, currentCryptoKey);
		}

		public int GetEncrypted()
		{
			ApplyNewCryptoKey();
			FloatIntBytesUnion union = new FloatIntBytesUnion
			{
				b1 = hiddenValue[0],
				b2 = hiddenValue[1],
				b3 = hiddenValue[2],
				b4 = hiddenValue[3]
			};
			return union.i;
		}

		public void SetEncrypted(int encrypted)
		{
			inited = true;
			FloatIntBytesUnion union = new FloatIntBytesUnion
			{
				i = encrypted
			};
			hiddenValue = new byte[4] { union.b1, union.b2, union.b3, union.b4 };
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
			FloatIntBytesUnion union = new FloatIntBytesUnion
			{
				b1 = hiddenValue[0],
				b2 = hiddenValue[1],
				b3 = hiddenValue[2],
				b4 = hiddenValue[3]
			};
			union.i ^= currentCryptoKey;
			float decrypted = union.f;
			if (ObscuredCheatingDetector.GetIsRunning() && fakeValue != 0f && Math.Abs(decrypted - fakeValue) > ObscuredCheatingDetector.get_Instance().floatEpsilon)
			{
				ObscuredCheatingDetector.get_Instance().OnCheatingDetected();
			}
			return decrypted;
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
		public static ObscuredFloat op_Increment(ObscuredFloat input)
		{
			float newValue = input.InternalDecrypt() + 1f;
			input.hiddenValue = InternalEncrypt(newValue, input.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				input.fakeValue = newValue;
			}
			return input;
		}

		[SpecialName]
		public static ObscuredFloat op_Decrement(ObscuredFloat input)
		{
			float newValue = input.InternalDecrypt() - 1f;
			input.hiddenValue = InternalEncrypt(newValue, input.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				input.fakeValue = newValue;
			}
			return input;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ObscuredFloat))
			{
				return false;
			}
			return Equals((ObscuredFloat)obj);
		}

		public bool Equals(ObscuredFloat other)
		{
			double num = other.InternalDecrypt();
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

		public string ToString(string format)
		{
			return InternalDecrypt().ToString(format);
		}

		public string ToString(IFormatProvider provider)
		{
			return InternalDecrypt().ToString(provider);
		}

		public string ToString(string format, IFormatProvider provider)
		{
			return InternalDecrypt().ToString(format, provider);
		}
	}
}
