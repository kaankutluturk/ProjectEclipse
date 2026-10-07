using System;
using System.Runtime.CompilerServices;
using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	[Serializable]
	public struct ObscuredULong : IEquatable<ObscuredULong>, IFormattable
	{
		private static ulong cryptoKey = 444443uL;

		private ulong currentCryptoKey;

		private ulong hiddenValue;

		private ulong fakeValue;

		private bool inited;

		private ObscuredULong(ulong value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = 0uL;
			inited = true;
		}

		public static void SetNewCryptoKey(ulong newKey)
		{
			cryptoKey = newKey;
		}

		public static ulong Encrypt(ulong value)
		{
			return Encrypt(value, 0uL);
		}

		public static ulong Decrypt(ulong value)
		{
			return Decrypt(value, 0uL);
		}

		public static ulong Encrypt(ulong value, ulong key)
		{
			if (key == 0)
			{
				return value ^ cryptoKey;
			}
			return value ^ key;
		}

		public static ulong Decrypt(ulong value, ulong key)
		{
			if (key == 0)
			{
				return value ^ cryptoKey;
			}
			return value ^ key;
		}

		public void ApplyNewCryptoKey()
		{
			if (currentCryptoKey != cryptoKey)
			{
				hiddenValue = Encrypt(InternalDecrypt(), cryptoKey);
				currentCryptoKey = cryptoKey;
			}
		}

		public void RandomizeCryptoKey()
		{
			ulong decrypted = InternalDecrypt();
			currentCryptoKey = (ulong)UnityEngine.Random.Range(0, int.MaxValue);
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		public ulong GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}

		public void SetEncrypted(ulong encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				fakeValue = InternalDecrypt();
			}
		}

		private ulong InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = Encrypt(0uL);
				fakeValue = 0uL;
				inited = true;
			}
			ulong num = Decrypt(hiddenValue, currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning() && fakeValue != 0 && num != fakeValue)
			{
				ObscuredCheatingDetector.get_Instance().OnCheatingDetected();
			}
			return num;
		}

		public static implicit operator ObscuredULong(ulong value)
		{
			ObscuredULong result = new ObscuredULong(Encrypt(value));
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				result.fakeValue = value;
			}
			return result;
		}

		public static implicit operator ulong(ObscuredULong value)
		{
			return value.InternalDecrypt();
		}

		[SpecialName]
		public static ObscuredULong op_Increment(ObscuredULong input)
		{
			ulong newValue = input.InternalDecrypt() + 1;
			input.hiddenValue = Encrypt(newValue, input.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				input.fakeValue = newValue;
			}
			return input;
		}

		[SpecialName]
		public static ObscuredULong op_Decrement(ObscuredULong input)
		{
			ulong newValue = input.InternalDecrypt() - 1;
			input.hiddenValue = Encrypt(newValue, input.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				input.fakeValue = newValue;
			}
			return input;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ObscuredULong))
			{
				return false;
			}
			return Equals((ObscuredULong)obj);
		}

		public bool Equals(ObscuredULong other)
		{
			if (currentCryptoKey == other.currentCryptoKey)
			{
				return hiddenValue == other.hiddenValue;
			}
			return Decrypt(hiddenValue, currentCryptoKey) == Decrypt(other.hiddenValue, other.currentCryptoKey);
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
