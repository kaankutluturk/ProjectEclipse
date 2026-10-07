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

		public static void SetNewCryptoKey(ulong CNOFJICCAHK)
		{
			cryptoKey = CNOFJICCAHK;
		}

		public static ulong Encrypt(ulong value)
		{
			return Encrypt(value, 0uL);
		}

		public static ulong Decrypt(ulong value)
		{
			return Decrypt(value, 0uL);
		}

		public static ulong Encrypt(ulong value, ulong KGBGENDIMBC)
		{
			if (KGBGENDIMBC == 0)
			{
				return value ^ cryptoKey;
			}
			return value ^ KGBGENDIMBC;
		}

		public static ulong Decrypt(ulong value, ulong KGBGENDIMBC)
		{
			if (KGBGENDIMBC == 0)
			{
				return value ^ cryptoKey;
			}
			return value ^ KGBGENDIMBC;
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
			ulong bAINMLLIKOL = InternalDecrypt();
			currentCryptoKey = (ulong)UnityEngine.Random.Range(0, int.MaxValue);
			hiddenValue = Encrypt(bAINMLLIKOL, currentCryptoKey);
		}

		public ulong GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}

		public void SetEncrypted(ulong ANGFOBEKKKD)
		{
			inited = true;
			hiddenValue = ANGFOBEKKKD;
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
		public static ObscuredULong op_Increment(ObscuredULong NILNDHEKNLJ)
		{
			ulong bAINMLLIKOL = NILNDHEKNLJ.InternalDecrypt() + 1;
			NILNDHEKNLJ.hiddenValue = Encrypt(bAINMLLIKOL, NILNDHEKNLJ.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				NILNDHEKNLJ.fakeValue = bAINMLLIKOL;
			}
			return NILNDHEKNLJ;
		}

		[SpecialName]
		public static ObscuredULong op_Decrement(ObscuredULong NILNDHEKNLJ)
		{
			ulong bAINMLLIKOL = NILNDHEKNLJ.InternalDecrypt() - 1;
			NILNDHEKNLJ.hiddenValue = Encrypt(bAINMLLIKOL, NILNDHEKNLJ.currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				NILNDHEKNLJ.fakeValue = bAINMLLIKOL;
			}
			return NILNDHEKNLJ;
		}

		public override bool Equals(object AOMLCBHAJJH)
		{
			if (!(AOMLCBHAJJH is ObscuredULong))
			{
				return false;
			}
			return Equals((ObscuredULong)AOMLCBHAJJH);
		}

		public bool Equals(ObscuredULong AOMLCBHAJJH)
		{
			if (currentCryptoKey == AOMLCBHAJJH.currentCryptoKey)
			{
				return hiddenValue == AOMLCBHAJJH.hiddenValue;
			}
			return Decrypt(hiddenValue, currentCryptoKey) == Decrypt(AOMLCBHAJJH.hiddenValue, AOMLCBHAJJH.currentCryptoKey);
		}

		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}

		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}

		public string ToString(string LBOHOKIBHOH)
		{
			return InternalDecrypt().ToString(LBOHOKIBHOH);
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
