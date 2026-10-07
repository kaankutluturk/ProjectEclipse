using System;
using System.Runtime.CompilerServices;
using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	[Serializable]
	public struct ObscuredShort : IEquatable<ObscuredShort>, IFormattable
	{
		private static short cryptoKey = 214;

		private short currentCryptoKey;

		private short hiddenValue;

		private short fakeValue;

		private bool inited;

		private ObscuredShort(short value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = 0;
			inited = true;
		}

		public static void SetNewCryptoKey(short newKey)
		{
			cryptoKey = newKey;
		}

		public static short EncryptDecrypt(short value)
		{
			return EncryptDecrypt(value, 0);
		}

		public static short EncryptDecrypt(short value, short key)
		{
			if (key == 0)
			{
				return (short)(value ^ cryptoKey);
			}
			return (short)(value ^ key);
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
			short decrypted = InternalDecrypt();
			currentCryptoKey = (short)UnityEngine.Random.Range(-32768, 32767);
			hiddenValue = EncryptDecrypt(decrypted, currentCryptoKey);
		}

		public short GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}

		public void SetEncrypted(short encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				fakeValue = InternalDecrypt();
			}
		}

		private short InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = EncryptDecrypt(0);
				fakeValue = 0;
				inited = true;
			}
			short num = EncryptDecrypt(hiddenValue, currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning() && fakeValue != 0 && num != fakeValue)
			{
				ObscuredCheatingDetector.get_Instance().OnCheatingDetected();
			}
			return num;
		}

		public static implicit operator ObscuredShort(short value)
		{
			ObscuredShort result = new ObscuredShort(EncryptDecrypt(value));
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				result.fakeValue = value;
			}
			return result;
		}

		public static implicit operator short(ObscuredShort value)
		{
			return value.InternalDecrypt();
		}

		[SpecialName]
		public static ObscuredShort op_Increment(ObscuredShort input)
		{
			short newValue = (short)(input.InternalDecrypt() + 1);
			input.hiddenValue = EncryptDecrypt(newValue);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				input.fakeValue = newValue;
			}
			return input;
		}

		[SpecialName]
		public static ObscuredShort op_Decrement(ObscuredShort input)
		{
			short newValue = (short)(input.InternalDecrypt() - 1);
			input.hiddenValue = EncryptDecrypt(newValue);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				input.fakeValue = newValue;
			}
			return input;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ObscuredShort))
			{
				return false;
			}
			return Equals((ObscuredShort)obj);
		}

		public bool Equals(ObscuredShort other)
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
