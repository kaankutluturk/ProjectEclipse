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

		public static void SetNewCryptoKey(short CNOFJICCAHK)
		{
			cryptoKey = CNOFJICCAHK;
		}

		public static short EncryptDecrypt(short value)
		{
			return EncryptDecrypt(value, 0);
		}

		public static short EncryptDecrypt(short value, short KGBGENDIMBC)
		{
			if (KGBGENDIMBC == 0)
			{
				return (short)(value ^ cryptoKey);
			}
			return (short)(value ^ KGBGENDIMBC);
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
			short bAINMLLIKOL = InternalDecrypt();
			currentCryptoKey = (short)UnityEngine.Random.Range(-32768, 32767);
			hiddenValue = EncryptDecrypt(bAINMLLIKOL, currentCryptoKey);
		}

		public short GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}

		public void SetEncrypted(short ANGFOBEKKKD)
		{
			inited = true;
			hiddenValue = ANGFOBEKKKD;
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
		public static ObscuredShort op_Increment(ObscuredShort NILNDHEKNLJ)
		{
			short bAINMLLIKOL = (short)(NILNDHEKNLJ.InternalDecrypt() + 1);
			NILNDHEKNLJ.hiddenValue = EncryptDecrypt(bAINMLLIKOL);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				NILNDHEKNLJ.fakeValue = bAINMLLIKOL;
			}
			return NILNDHEKNLJ;
		}

		[SpecialName]
		public static ObscuredShort op_Decrement(ObscuredShort NILNDHEKNLJ)
		{
			short bAINMLLIKOL = (short)(NILNDHEKNLJ.InternalDecrypt() - 1);
			NILNDHEKNLJ.hiddenValue = EncryptDecrypt(bAINMLLIKOL);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				NILNDHEKNLJ.fakeValue = bAINMLLIKOL;
			}
			return NILNDHEKNLJ;
		}

		public override bool Equals(object AOMLCBHAJJH)
		{
			if (!(AOMLCBHAJJH is ObscuredShort))
			{
				return false;
			}
			return Equals((ObscuredShort)AOMLCBHAJJH);
		}

		public bool Equals(ObscuredShort AOMLCBHAJJH)
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

		public string ToString(string LBOHOKIBHOH)
		{
			return InternalDecrypt().ToString(LBOHOKIBHOH);
		}

		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
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
