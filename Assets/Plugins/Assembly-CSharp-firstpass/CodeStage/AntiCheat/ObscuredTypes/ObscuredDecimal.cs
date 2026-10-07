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

		public static void SetNewCryptoKey(long newKey)
		{
			cryptoKey = newKey;
		}

		public static decimal Encrypt(decimal value)
		{
			return Encrypt(value, cryptoKey);
		}

		public static decimal Encrypt(decimal value, long key)
		{
			DecimalLongBytesUnion union = new DecimalLongBytesUnion
			{
				d = value
			};
			union.l1 ^= key;
			union.l2 ^= key;
			return union.d;
		}

		private static byte[] InternalEncrypt(decimal value)
		{
			return InternalEncrypt(value, 0L);
		}

		private static byte[] InternalEncrypt(decimal value, long key)
		{
			long num = key;
			if (num == 0)
			{
				num = cryptoKey;
			}
			DecimalLongBytesUnion union = new DecimalLongBytesUnion
			{
				d = value
			};
			union.l1 ^= num;
			union.l2 ^= num;
			return new byte[16]
			{
				union.b1, union.b2, union.b3, union.b4, union.b5, union.b6, union.b7, union.b8, union.b9, union.b10,
				union.b11, union.b12, union.b13, union.b14, union.b15, union.b16
			};
		}

		public static decimal Decrypt(decimal value)
		{
			return Decrypt(value, cryptoKey);
		}

		public static decimal Decrypt(decimal value, long key)
		{
			DecimalLongBytesUnion union = new DecimalLongBytesUnion
			{
				d = value
			};
			union.l1 ^= key;
			union.l2 ^= key;
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
			decimal decrypted = InternalDecrypt();
			currentCryptoKey = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			hiddenValue = InternalEncrypt(decrypted, currentCryptoKey);
		}

		public decimal GetEncrypted()
		{
			ApplyNewCryptoKey();
			DecimalLongBytesUnion union = new DecimalLongBytesUnion
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
			return union.d;
		}

		public void SetEncrypted(decimal encrypted)
		{
			inited = true;
			DecimalLongBytesUnion union = new DecimalLongBytesUnion
			{
				d = encrypted
			};
			hiddenValue = new byte[16]
			{
				union.b1, union.b2, union.b3, union.b4, union.b5, union.b6, union.b7, union.b8, union.b9, union.b10,
				union.b11, union.b12, union.b13, union.b14, union.b15, union.b16
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
			DecimalLongBytesUnion union = new DecimalLongBytesUnion
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
			union.l1 ^= currentCryptoKey;
			union.l2 ^= currentCryptoKey;
			decimal decrypted = union.d;
			if (ObscuredCheatingDetector.GetIsRunning() && fakeValue != 0m && decrypted != fakeValue)
			{
				ObscuredCheatingDetector.get_Instance().OnCheatingDetected();
			}
			return decrypted;
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
		public static ObscuredDecimal op_Increment(ObscuredDecimal input)
		{
			decimal newValue = input.InternalDecrypt() + 1m;
			input.hiddenValue = InternalEncrypt(newValue, input.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				input.fakeValue = newValue;
			}
			return input;
		}

		[SpecialName]
		public static ObscuredDecimal op_Decrement(ObscuredDecimal input)
		{
			decimal newValue = input.InternalDecrypt() - 1m;
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
			if (!(obj is ObscuredDecimal))
			{
				return false;
			}
			return Equals((ObscuredDecimal)obj);
		}

		public bool Equals(ObscuredDecimal other)
		{
			return other.InternalDecrypt().Equals(InternalDecrypt());
		}

		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}
	}
}
