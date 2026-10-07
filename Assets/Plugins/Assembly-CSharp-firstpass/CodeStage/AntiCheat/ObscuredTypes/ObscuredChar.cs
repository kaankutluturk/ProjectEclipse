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

		public static void SetNewCryptoKey(char CNOFJICCAHK)
		{
			cryptoKey = CNOFJICCAHK;
		}

		public static char EncryptDecrypt(char value)
		{
			return EncryptDecrypt(value, '\0');
		}

		public static char EncryptDecrypt(char value, char KGBGENDIMBC)
		{
			if (KGBGENDIMBC == '\0')
			{
				return (char)(value ^ cryptoKey);
			}
			return (char)(value ^ KGBGENDIMBC);
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
			char bAINMLLIKOL = InternalDecrypt();
			currentCryptoKey = (char)UnityEngine.Random.Range(0, 65535);
			hiddenValue = EncryptDecrypt(bAINMLLIKOL, currentCryptoKey);
		}

		public char GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}

		public void SetEncrypted(char ANGFOBEKKKD)
		{
			inited = true;
			hiddenValue = ANGFOBEKKKD;
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
		public static ObscuredChar op_Increment(ObscuredChar NILNDHEKNLJ)
		{
			char bAINMLLIKOL = (char)(NILNDHEKNLJ.InternalDecrypt() + 1);
			NILNDHEKNLJ.hiddenValue = EncryptDecrypt(bAINMLLIKOL, NILNDHEKNLJ.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				NILNDHEKNLJ.fakeValue = bAINMLLIKOL;
			}
			return NILNDHEKNLJ;
		}

		[SpecialName]
		public static ObscuredChar op_Decrement(ObscuredChar NILNDHEKNLJ)
		{
			char bAINMLLIKOL = (char)(NILNDHEKNLJ.InternalDecrypt() - 1);
			NILNDHEKNLJ.hiddenValue = EncryptDecrypt(bAINMLLIKOL, NILNDHEKNLJ.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				NILNDHEKNLJ.fakeValue = bAINMLLIKOL;
			}
			return NILNDHEKNLJ;
		}

		public override bool Equals(object AOMLCBHAJJH)
		{
			if (!(AOMLCBHAJJH is ObscuredChar))
			{
				return false;
			}
			return Equals((ObscuredChar)AOMLCBHAJJH);
		}

		public bool Equals(ObscuredChar AOMLCBHAJJH)
		{
			if (currentCryptoKey == AOMLCBHAJJH.currentCryptoKey)
			{
				return hiddenValue == AOMLCBHAJJH.hiddenValue;
			}
			return EncryptDecrypt(hiddenValue, currentCryptoKey) == EncryptDecrypt(AOMLCBHAJJH.hiddenValue, AOMLCBHAJJH.currentCryptoKey);
		}

		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}

		public string ToString(IFormatProvider EEGMFLOPLLH)
		{
			return InternalDecrypt().ToString(EEGMFLOPLLH);
		}

		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}
	}
}
