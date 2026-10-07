using System;
using System.Runtime.CompilerServices;
using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	[Serializable]
	public sealed class ObscuredString
	{
		private static string cryptoKey = "4441";

		[SerializeField]
		private string currentCryptoKey;

		[SerializeField]
		private byte[] hiddenValue;

		[SerializeField]
		private string fakeValue;

		[SerializeField]
		private bool inited;

		private ObscuredString()
		{
		}

		private ObscuredString(byte[] value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = null;
			inited = true;
		}

		public static void SetNewCryptoKey(string CNOFJICCAHK)
		{
			cryptoKey = CNOFJICCAHK;
		}

		public static string EncryptDecrypt(string value)
		{
			return EncryptDecrypt(value, string.Empty);
		}

		public static string EncryptDecrypt(string value, string KGBGENDIMBC)
		{
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}
			if (string.IsNullOrEmpty(KGBGENDIMBC))
			{
				KGBGENDIMBC = cryptoKey;
			}
			int length = KGBGENDIMBC.Length;
			int length2 = value.Length;
			char[] array = new char[length2];
			for (int i = 0; i < length2; i++)
			{
				array[i] = (char)(value[i] ^ KGBGENDIMBC[i % length]);
			}
			return new string(array);
		}

		public void ApplyNewCryptoKey()
		{
			if (currentCryptoKey != cryptoKey)
			{
				hiddenValue = InternalEncrypt(InternalDecrypt());
				currentCryptoKey = cryptoKey;
			}
		}

		public void RandomizeCryptoKey()
		{
			string bAINMLLIKOL = InternalDecrypt();
			currentCryptoKey = UnityEngine.Random.Range(int.MinValue, int.MaxValue).ToString();
			hiddenValue = InternalEncrypt(bAINMLLIKOL, currentCryptoKey);
		}

		public string GetEncrypted()
		{
			ApplyNewCryptoKey();
			return GetString(hiddenValue);
		}

		public void SetEncrypted(string ANGFOBEKKKD)
		{
			inited = true;
			hiddenValue = GetBytes(ANGFOBEKKKD);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				fakeValue = InternalDecrypt();
			}
		}

		private static byte[] InternalEncrypt(string value)
		{
			return InternalEncrypt(value, cryptoKey);
		}

		private static byte[] InternalEncrypt(string value, string KGBGENDIMBC)
		{
			return GetBytes(EncryptDecrypt(value, KGBGENDIMBC));
		}

		private string InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = InternalEncrypt(string.Empty);
				fakeValue = string.Empty;
				inited = true;
			}
			string text = currentCryptoKey;
			if (string.IsNullOrEmpty(text))
			{
				text = cryptoKey;
			}
			string text2 = EncryptDecrypt(GetString(hiddenValue), text);
			if (ObscuredCheatingDetector.GetIsRunning() && !string.IsNullOrEmpty(fakeValue) && text2 != fakeValue)
			{
				ObscuredCheatingDetector.get_Instance().OnCheatingDetected();
			}
			return text2;
		}

		public static implicit operator ObscuredString(string value)
		{
			if (value == null)
			{
				return null;
			}
			ObscuredString obscuredString = new ObscuredString(InternalEncrypt(value));
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				obscuredString.fakeValue = value;
			}
			return obscuredString;
		}

		public static implicit operator string(ObscuredString value)
		{
			if (op_Equality(value, null))
			{
				return null;
			}
			return value.InternalDecrypt();
		}

		public override string ToString()
		{
			return InternalDecrypt();
		}

		[SpecialName]
		public static bool op_Equality(ObscuredString LHBNIMGFKIB, ObscuredString AAOIAEJJINO)
		{
			if (object.ReferenceEquals(LHBNIMGFKIB, AAOIAEJJINO))
			{
				return true;
			}
			if (LHBNIMGFKIB == null || AAOIAEJJINO == null)
			{
				return false;
			}
			if (LHBNIMGFKIB.currentCryptoKey == AAOIAEJJINO.currentCryptoKey)
			{
				return ArraysEquals(LHBNIMGFKIB.hiddenValue, AAOIAEJJINO.hiddenValue);
			}
			return string.Equals(LHBNIMGFKIB.InternalDecrypt(), AAOIAEJJINO.InternalDecrypt());
		}

		[SpecialName]
		public static bool op_Inequality(ObscuredString LHBNIMGFKIB, ObscuredString AAOIAEJJINO)
		{
			return !op_Equality(LHBNIMGFKIB, AAOIAEJJINO);
		}

		public override bool Equals(object AOMLCBHAJJH)
		{
			if (!(AOMLCBHAJJH is ObscuredString))
			{
				return false;
			}
			return Equals((ObscuredString)AOMLCBHAJJH);
		}

		public bool Equals(ObscuredString value)
		{
			if (op_Equality(value, null))
			{
				return false;
			}
			if (currentCryptoKey == value.currentCryptoKey)
			{
				return ArraysEquals(hiddenValue, value.hiddenValue);
			}
			return string.Equals(InternalDecrypt(), value.InternalDecrypt());
		}

		public bool Equals(ObscuredString value, StringComparison HLEHPPPKBMD)
		{
			if (op_Equality(value, null))
			{
				return false;
			}
			return string.Equals(InternalDecrypt(), value.InternalDecrypt(), HLEHPPPKBMD);
		}

		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}

		private static byte[] GetBytes(string IGGFGLLIGCG)
		{
			byte[] array = new byte[IGGFGLLIGCG.Length * 2];
			Buffer.BlockCopy(IGGFGLLIGCG.ToCharArray(), 0, array, 0, array.Length);
			return array;
		}

		private static string GetString(byte[] KPAMPCLHCEN)
		{
			char[] array = new char[KPAMPCLHCEN.Length / 2];
			Buffer.BlockCopy(KPAMPCLHCEN, 0, array, 0, KPAMPCLHCEN.Length);
			return new string(array);
		}

		private static bool ArraysEquals(byte[] HICHONIJHKL, byte[] LNPFHLPCLOP)
		{
			if (HICHONIJHKL == LNPFHLPCLOP)
			{
				return true;
			}
			if (HICHONIJHKL != null && LNPFHLPCLOP != null)
			{
				if (HICHONIJHKL.Length != LNPFHLPCLOP.Length)
				{
					return false;
				}
				for (int i = 0; i < HICHONIJHKL.Length; i++)
				{
					if (HICHONIJHKL[i] != LNPFHLPCLOP[i])
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
	}
}
