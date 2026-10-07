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

		public static void SetNewCryptoKey(ushort CNOFJICCAHK)
		{
			cryptoKey = CNOFJICCAHK;
		}

		public static ushort EncryptDecrypt(ushort value)
		{
			return EncryptDecrypt(value, 0);
		}

		public static ushort EncryptDecrypt(ushort value, ushort KGBGENDIMBC)
		{
			if (KGBGENDIMBC == 0)
			{
				return (ushort)(value ^ cryptoKey);
			}
			return (ushort)(value ^ KGBGENDIMBC);
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
			ushort bAINMLLIKOL = InternalDecrypt();
			currentCryptoKey = (ushort)UnityEngine.Random.Range(0, 32767);
			hiddenValue = EncryptDecrypt(bAINMLLIKOL, currentCryptoKey);
		}

		public ushort GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}

		public void SetEncrypted(ushort ANGFOBEKKKD)
		{
			inited = true;
			hiddenValue = ANGFOBEKKKD;
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
		public static ObscuredUShort op_Increment(ObscuredUShort NILNDHEKNLJ)
		{
			ushort bAINMLLIKOL = (ushort)(NILNDHEKNLJ.InternalDecrypt() + 1);
			NILNDHEKNLJ.hiddenValue = EncryptDecrypt(bAINMLLIKOL, NILNDHEKNLJ.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				NILNDHEKNLJ.fakeValue = bAINMLLIKOL;
			}
			return NILNDHEKNLJ;
		}

		[SpecialName]
		public static ObscuredUShort op_Decrement(ObscuredUShort NILNDHEKNLJ)
		{
			ushort bAINMLLIKOL = (ushort)(NILNDHEKNLJ.InternalDecrypt() - 1);
			NILNDHEKNLJ.hiddenValue = EncryptDecrypt(bAINMLLIKOL, NILNDHEKNLJ.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				NILNDHEKNLJ.fakeValue = bAINMLLIKOL;
			}
			return NILNDHEKNLJ;
		}

		public override bool Equals(object AOMLCBHAJJH)
		{
			if (!(AOMLCBHAJJH is ObscuredUShort))
			{
				return false;
			}
			return Equals((ObscuredUShort)AOMLCBHAJJH);
		}

		public bool Equals(ObscuredUShort AOMLCBHAJJH)
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
