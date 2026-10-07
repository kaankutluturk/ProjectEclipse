using System;
using System.Runtime.CompilerServices;
using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	[Serializable]
	public struct ObscuredByte : IEquatable<ObscuredByte>, IFormattable
	{
		private static byte cryptoKey = 244;

		private byte currentCryptoKey;

		private byte hiddenValue;

		private byte fakeValue;

		private bool inited;

		private ObscuredByte(byte value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = 0;
			inited = true;
		}

		public static void SetNewCryptoKey(byte CNOFJICCAHK)
		{
			cryptoKey = CNOFJICCAHK;
		}

		public static byte EncryptDecrypt(byte value)
		{
			return EncryptDecrypt(value, 0);
		}

		public static byte EncryptDecrypt(byte value, byte KGBGENDIMBC)
		{
			if (KGBGENDIMBC == 0)
			{
				return (byte)(value ^ cryptoKey);
			}
			return (byte)(value ^ KGBGENDIMBC);
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
			byte bAINMLLIKOL = InternalDecrypt();
			currentCryptoKey = (byte)UnityEngine.Random.Range(0, 255);
			hiddenValue = EncryptDecrypt(bAINMLLIKOL, currentCryptoKey);
		}

		public byte GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}

		public void SetEncrypted(byte ANGFOBEKKKD)
		{
			inited = true;
			hiddenValue = ANGFOBEKKKD;
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				fakeValue = InternalDecrypt();
			}
		}

		private byte InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = EncryptDecrypt(0);
				fakeValue = 0;
				inited = true;
			}
			byte b = EncryptDecrypt(hiddenValue, currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning() && fakeValue != 0 && b != fakeValue)
			{
				ObscuredCheatingDetector.get_Instance().OnCheatingDetected();
			}
			return b;
		}

		public static implicit operator ObscuredByte(byte value)
		{
			ObscuredByte result = new ObscuredByte(EncryptDecrypt(value));
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				result.fakeValue = value;
			}
			return result;
		}

		public static implicit operator byte(ObscuredByte value)
		{
			return value.InternalDecrypt();
		}

		[SpecialName]
		public static ObscuredByte Increment(ObscuredByte NILNDHEKNLJ)
		{
			byte bAINMLLIKOL = (byte)(NILNDHEKNLJ.InternalDecrypt() + 1);
			NILNDHEKNLJ.hiddenValue = EncryptDecrypt(bAINMLLIKOL, NILNDHEKNLJ.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				NILNDHEKNLJ.fakeValue = bAINMLLIKOL;
			}
			return NILNDHEKNLJ;
		}

		[SpecialName]
		public static ObscuredByte Decrement(ObscuredByte NILNDHEKNLJ)
		{
			byte bAINMLLIKOL = (byte)(NILNDHEKNLJ.InternalDecrypt() - 1);
			NILNDHEKNLJ.hiddenValue = EncryptDecrypt(bAINMLLIKOL, NILNDHEKNLJ.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				NILNDHEKNLJ.fakeValue = bAINMLLIKOL;
			}
			return NILNDHEKNLJ;
		}

		public override bool Equals(object AOMLCBHAJJH)
		{
			if (!(AOMLCBHAJJH is ObscuredByte))
			{
				return false;
			}
			return Equals((ObscuredByte)AOMLCBHAJJH);
		}

		public bool Equals(ObscuredByte AOMLCBHAJJH)
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
