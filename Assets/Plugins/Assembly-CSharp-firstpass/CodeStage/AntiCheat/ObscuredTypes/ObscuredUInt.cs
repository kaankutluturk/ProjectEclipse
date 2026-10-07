using System;
using System.Runtime.CompilerServices;
using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	[Serializable]
	public struct ObscuredUInt : IEquatable<ObscuredUInt>, IFormattable
	{
		private static uint cryptoKey = 240513u;

		private uint currentCryptoKey;

		private uint hiddenValue;

		private uint fakeValue;

		private bool inited;

		private ObscuredUInt(uint value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = 0u;
			inited = true;
		}

		public static void SetNewCryptoKey(uint newKey)
		{
			cryptoKey = newKey;
		}

		public static uint Encrypt(uint value)
		{
			return Encrypt(value, 0u);
		}

		public static uint Decrypt(uint value)
		{
			return Decrypt(value, 0u);
		}

		public static uint Encrypt(uint value, uint key)
		{
			if (key == 0)
			{
				return value ^ cryptoKey;
			}
			return value ^ key;
		}

		public static uint Decrypt(uint value, uint key)
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
			uint decrypted = InternalDecrypt();
			currentCryptoKey = (uint)UnityEngine.Random.Range(0, int.MaxValue);
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		public uint GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}

		public void SetEncrypted(uint encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				fakeValue = InternalDecrypt();
			}
		}

		private uint InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = Encrypt(0u);
				fakeValue = 0u;
				inited = true;
			}
			uint num = Decrypt(hiddenValue, currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning() && fakeValue != 0 && num != fakeValue)
			{
				ObscuredCheatingDetector.get_Instance().OnCheatingDetected();
			}
			return num;
		}

		public static implicit operator ObscuredUInt(uint value)
		{
			ObscuredUInt result = new ObscuredUInt(Encrypt(value));
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				result.fakeValue = value;
			}
			return result;
		}

		public static implicit operator uint(ObscuredUInt value)
		{
			return value.InternalDecrypt();
		}

		public static explicit operator ObscuredInt(ObscuredUInt value)
		{
			return (ObscuredInt)((int)value.InternalDecrypt());
		}

		[SpecialName]
		public static ObscuredUInt op_Increment(ObscuredUInt input)
		{
			uint newValue = input.InternalDecrypt() + 1;
			input.hiddenValue = Encrypt(newValue, input.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				input.fakeValue = newValue;
			}
			return input;
		}

		[SpecialName]
		public static ObscuredUInt op_Decrement(ObscuredUInt input)
		{
			uint newValue = input.InternalDecrypt() - 1;
			input.hiddenValue = Encrypt(newValue, input.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				input.fakeValue = newValue;
			}
			return input;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ObscuredUInt))
			{
				return false;
			}
			return Equals((ObscuredUInt)obj);
		}

		public bool Equals(ObscuredUInt other)
		{
			if (currentCryptoKey == other.currentCryptoKey)
			{
				return hiddenValue == other.hiddenValue;
			}
			return Decrypt(hiddenValue, currentCryptoKey) == Decrypt(other.hiddenValue, other.currentCryptoKey);
		}

		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}

		public string ToString(string format)
		{
			return InternalDecrypt().ToString(format);
		}

		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
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
