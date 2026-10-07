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

		public static void SetNewCryptoKey(long newKey)
		{
			cryptoKey = newKey;
		}

		public static long Encrypt(double value)
		{
			return Encrypt(value, cryptoKey);
		}

		public static long Encrypt(double value, long key)
		{
			DoubleLongBytesUnion union = new DoubleLongBytesUnion
			{
				d = value
			};
			union.l ^= key;
			return union.l;
		}

		private static byte[] InternalEncrypt(double value)
		{
			return InternalEncrypt(value, 0L);
		}

		private static byte[] InternalEncrypt(double value, long key)
		{
			long num = key;
			if (num == 0)
			{
				num = cryptoKey;
			}
			DoubleLongBytesUnion union = new DoubleLongBytesUnion
			{
				d = value
			};
			union.l ^= num;
			return new byte[8] { union.b1, union.b2, union.b3, union.b4, union.b5, union.b6, union.b7, union.b8 };
		}

		public static double Decrypt(long value)
		{
			return Decrypt(value, cryptoKey);
		}

		public static double Decrypt(long value, long key)
		{
			DoubleLongBytesUnion union = new DoubleLongBytesUnion
			{
				l = (value ^ key)
			};
			return union.d;
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
			double decrypted = InternalDecrypt();
			currentCryptoKey = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			hiddenValue = InternalEncrypt(decrypted, currentCryptoKey);
		}

		public long GetEncrypted()
		{
			ApplyNewCryptoKey();
			DoubleLongBytesUnion union = new DoubleLongBytesUnion
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
			return union.l;
		}

		public void SetEncrypted(long encrypted)
		{
			inited = true;
			DoubleLongBytesUnion union = new DoubleLongBytesUnion
			{
				l = encrypted
			};
			hiddenValue = new byte[8] { union.b1, union.b2, union.b3, union.b4, union.b5, union.b6, union.b7, union.b8 };
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
			DoubleLongBytesUnion union = new DoubleLongBytesUnion
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
			union.l ^= currentCryptoKey;
			double decrypted = union.d;
			if (ObscuredCheatingDetector.GetIsRunning() && fakeValue != 0.0 && Math.Abs(decrypted - fakeValue) > 1E-06)
			{
				ObscuredCheatingDetector.get_Instance().OnCheatingDetected();
			}
			return decrypted;
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
		public static ObscuredDouble op_Increment(ObscuredDouble input)
		{
			double newValue = input.InternalDecrypt() + 1.0;
			input.hiddenValue = InternalEncrypt(newValue, input.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				input.fakeValue = newValue;
			}
			return input;
		}

		[SpecialName]
		public static ObscuredDouble op_Decrement(ObscuredDouble input)
		{
			double newValue = input.InternalDecrypt() - 1.0;
			input.hiddenValue = InternalEncrypt(newValue, input.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				input.fakeValue = newValue;
			}
			return input;
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

		public override bool Equals(object obj)
		{
			if (!(obj is ObscuredDouble))
			{
				return false;
			}
			return Equals((ObscuredDouble)obj);
		}

		public bool Equals(ObscuredDouble other)
		{
			return other.InternalDecrypt().Equals(InternalDecrypt());
		}

		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}
	}
}
