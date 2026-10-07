using System;
using System.Runtime.CompilerServices;
using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	[Serializable]
	public struct ObscuredChar : IEquatable<ObscuredChar>
	{
		private static char cryptoKey = '—';

		private char currentCryptoKey;

		private char hiddenValue;

		private char fakeValue;

		private bool inited;

		private ObscuredChar(char value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = '\0';
			inited = true;
		}

		public static void SetNewCryptoKey(char newKey)
		{
			cryptoKey = newKey;
		}

		public static char EncryptDecrypt(char value)
		{
			return EncryptDecrypt(value, '\0');
		}

		public static char EncryptDecrypt(char value, char key)
		{
			if (key == '\0')
			{
				return (char)(value ^ cryptoKey);
			}
			return (char)(value ^ key);
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
			char decrypted = InternalDecrypt();
			currentCryptoKey = (char)UnityEngine.Random.Range(0, 65535);
			hiddenValue = EncryptDecrypt(decrypted, currentCryptoKey);
		}

		public char GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}

		public void SetEncrypted(char encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				fakeValue = InternalDecrypt();
			}
		}

		private char InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = EncryptDecrypt('\0');
				fakeValue = '\0';
				inited = true;
			}
			char c = EncryptDecrypt(hiddenValue, currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning() && fakeValue != 0 && c != fakeValue)
			{
				ObscuredCheatingDetector.get_Instance().OnCheatingDetected();
			}
			return c;
		}

		public static implicit operator ObscuredChar(char value)
		{
			ObscuredChar result = new ObscuredChar(EncryptDecrypt(value));
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				result.fakeValue = value;
			}
			return result;
		}

		public static implicit operator char(ObscuredChar value)
		{
			return value.InternalDecrypt();
		}

		[SpecialName]
		public static ObscuredChar op_Increment(ObscuredChar input)
		{
			char newValue = (char)(input.InternalDecrypt() + 1);
			input.hiddenValue = EncryptDecrypt(newValue, input.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				input.fakeValue = newValue;
			}
			return input;
		}

		[SpecialName]
		public static ObscuredChar op_Decrement(ObscuredChar input)
		{
			char newValue = (char)(input.InternalDecrypt() - 1);
			input.hiddenValue = EncryptDecrypt(newValue, input.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				input.fakeValue = newValue;
			}
			return input;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ObscuredChar))
			{
				return false;
			}
			return Equals((ObscuredChar)obj);
		}

		public bool Equals(ObscuredChar other)
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

		public string ToString(IFormatProvider provider)
		{
			return InternalDecrypt().ToString(provider);
		}

		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}
	}
}
