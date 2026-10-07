using System;
using System.Runtime.CompilerServices;
using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	[Serializable]
	public struct ObscuredUShort : IEquatable<ObscuredUShort>, IFormattable
	{
		private static ushort cryptoKey = 224;

		private ushort currentCryptoKey;

		private ushort hiddenValue;

		private ushort fakeValue;

		private bool inited;

		private ObscuredUShort(ushort value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = 0;
			inited = true;
		}

		public static void SetNewCryptoKey(ushort newKey)
		{
			cryptoKey = newKey;
		}

		public static ushort EncryptDecrypt(ushort value)
		{
			return EncryptDecrypt(value, 0);
		}

		public static ushort EncryptDecrypt(ushort value, ushort key)
		{
			if (key == 0)
			{
				return (ushort)(value ^ cryptoKey);
			}
			return (ushort)(value ^ key);
		}

		public void ApplyNewCryptoKey()
		{
			if (currentCryptoKey != cryptoKey)
			{
				hiddenValue = EncryptDecrypt(InternalDecrypt(), cryptoKey);
				currentCryptoKey = cryptoKey;
			}
		}

		public void RandomizeCryptoKey()
		{
			ushort decrypted = InternalDecrypt();
			currentCryptoKey = (ushort)UnityEngine.Random.Range(0, 32767);
			hiddenValue = EncryptDecrypt(decrypted, currentCryptoKey);
		}

		public ushort GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}

		public void SetEncrypted(ushort encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				fakeValue = InternalDecrypt();
			}
		}

		private ushort InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = EncryptDecrypt(0);
				fakeValue = 0;
				inited = true;
			}
			ushort num = EncryptDecrypt(hiddenValue, currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning() && fakeValue != 0 && num != fakeValue)
			{
				ObscuredCheatingDetector.get_Instance().OnCheatingDetected();
			}
			return num;
		}

		public static implicit operator ObscuredUShort(ushort value)
		{
			ObscuredUShort result = new ObscuredUShort(EncryptDecrypt(value));
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				result.fakeValue = value;
			}
			return result;
		}

		public static implicit operator ushort(ObscuredUShort value)
		{
			return value.InternalDecrypt();
		}

		[SpecialName]
		public static ObscuredUShort op_Increment(ObscuredUShort input)
		{
			ushort newValue = (ushort)(input.InternalDecrypt() + 1);
			input.hiddenValue = EncryptDecrypt(newValue, input.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				input.fakeValue = newValue;
			}
			return input;
		}

		[SpecialName]
		public static ObscuredUShort op_Decrement(ObscuredUShort input)
		{
			ushort newValue = (ushort)(input.InternalDecrypt() - 1);
			input.hiddenValue = EncryptDecrypt(newValue, input.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				input.fakeValue = newValue;
			}
			return input;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ObscuredUShort))
			{
				return false;
			}
			return Equals((ObscuredUShort)obj);
		}

		public bool Equals(ObscuredUShort other)
		{
			if (currentCryptoKey == other.currentCryptoKey)
			{
				return hiddenValue == other.hiddenValue;
			}
			return EncryptDecrypt(hiddenValue, currentCryptoKey) == EncryptDecrypt(other.hiddenValue, other.currentCryptoKey);
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
