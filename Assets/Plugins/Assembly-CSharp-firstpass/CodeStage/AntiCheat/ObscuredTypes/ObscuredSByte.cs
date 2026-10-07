using System;
using System.Runtime.CompilerServices;
using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	[Serializable]
	public struct ObscuredSByte : IEquatable<ObscuredSByte>, IFormattable
	{
		private static sbyte cryptoKey = 112;

		private sbyte currentCryptoKey;

		private sbyte hiddenValue;

		private sbyte fakeValue;

		private bool inited;

		private ObscuredSByte(sbyte value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = 0;
			inited = true;
		}

		public static void SetNewCryptoKey(sbyte CNOFJICCAHK)
		{
			cryptoKey = CNOFJICCAHK;
		}

		public static sbyte EncryptDecrypt(sbyte value)
		{
			return EncryptDecrypt(value, 0);
		}

		public static sbyte EncryptDecrypt(sbyte value, sbyte KGBGENDIMBC)
		{
			if (KGBGENDIMBC == 0)
			{
				return (sbyte)(value ^ cryptoKey);
			}
			return (sbyte)(value ^ KGBGENDIMBC);
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
			sbyte bAINMLLIKOL = InternalDecrypt();
			currentCryptoKey = (sbyte)UnityEngine.Random.Range(-128, 127);
			hiddenValue = EncryptDecrypt(bAINMLLIKOL, currentCryptoKey);
		}

		public sbyte GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}

		public void SetEncrypted(sbyte ANGFOBEKKKD)
		{
			inited = true;
			hiddenValue = ANGFOBEKKKD;
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				fakeValue = InternalDecrypt();
			}
		}

		private sbyte InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = EncryptDecrypt(0);
				fakeValue = 0;
				inited = true;
			}
			sbyte b = EncryptDecrypt(hiddenValue, currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning() && fakeValue != 0 && b != fakeValue)
			{
				ObscuredCheatingDetector.get_Instance().OnCheatingDetected();
			}
			return b;
		}

		public static implicit operator ObscuredSByte(sbyte value)
		{
			ObscuredSByte result = new ObscuredSByte(EncryptDecrypt(value));
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				result.fakeValue = value;
			}
			return result;
		}

		public static implicit operator sbyte(ObscuredSByte value)
		{
			return value.InternalDecrypt();
		}

		[SpecialName]
		public static ObscuredSByte op_Increment(ObscuredSByte NILNDHEKNLJ)
		{
			sbyte bAINMLLIKOL = (sbyte)(NILNDHEKNLJ.InternalDecrypt() + 1);
			NILNDHEKNLJ.hiddenValue = EncryptDecrypt(bAINMLLIKOL, NILNDHEKNLJ.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				NILNDHEKNLJ.fakeValue = bAINMLLIKOL;
			}
			return NILNDHEKNLJ;
		}

		[SpecialName]
		public static ObscuredSByte op_Decrement(ObscuredSByte NILNDHEKNLJ)
		{
			sbyte bAINMLLIKOL = (sbyte)(NILNDHEKNLJ.InternalDecrypt() - 1);
			NILNDHEKNLJ.hiddenValue = EncryptDecrypt(bAINMLLIKOL, NILNDHEKNLJ.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				NILNDHEKNLJ.fakeValue = bAINMLLIKOL;
			}
			return NILNDHEKNLJ;
		}

		public override bool Equals(object AOMLCBHAJJH)
		{
			if (!(AOMLCBHAJJH is ObscuredSByte))
			{
				return false;
			}
			return Equals((ObscuredSByte)AOMLCBHAJJH);
		}

		public bool Equals(ObscuredSByte AOMLCBHAJJH)
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
