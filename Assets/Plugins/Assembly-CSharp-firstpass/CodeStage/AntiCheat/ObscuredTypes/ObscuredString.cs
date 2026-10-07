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

		public static void SetNewCryptoKey(string newKey)
		{
			cryptoKey = newKey;
		}

		public static string EncryptDecrypt(string value)
		{
			return EncryptDecrypt(value, string.Empty);
		}

		public static string EncryptDecrypt(string value, string key)
		{
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}
			if (string.IsNullOrEmpty(key))
			{
				key = cryptoKey;
			}
			int length = key.Length;
			int length2 = value.Length;
			char[] array = new char[length2];
			for (int i = 0; i < length2; i++)
			{
				array[i] = (char)(value[i] ^ key[i % length]);
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
			string decrypted = InternalDecrypt();
			currentCryptoKey = UnityEngine.Random.Range(int.MinValue, int.MaxValue).ToString();
			hiddenValue = InternalEncrypt(decrypted, currentCryptoKey);
		}

		public string GetEncrypted()
		{
			ApplyNewCryptoKey();
			return GetString(hiddenValue);
		}

		public void SetEncrypted(string encrypted)
		{
			inited = true;
			hiddenValue = GetBytes(encrypted);
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				fakeValue = InternalDecrypt();
			}
		}

		private static byte[] InternalEncrypt(string value)
		{
			return InternalEncrypt(value, cryptoKey);
		}

		private static byte[] InternalEncrypt(string value, string key)
		{
			return GetBytes(EncryptDecrypt(value, key));
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
		public static bool op_Equality(ObscuredString left, ObscuredString right)
		{
			if (object.ReferenceEquals(left, right))
			{
				return true;
			}
			if (left == null || right == null)
			{
				return false;
			}
			if (left.currentCryptoKey == right.currentCryptoKey)
			{
				return ArraysEquals(left.hiddenValue, right.hiddenValue);
			}
			return string.Equals(left.InternalDecrypt(), right.InternalDecrypt());
		}

		[SpecialName]
		public static bool op_Inequality(ObscuredString left, ObscuredString right)
		{
			return !op_Equality(left, right);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ObscuredString))
			{
				return false;
			}
			return Equals((ObscuredString)obj);
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

		public bool Equals(ObscuredString value, StringComparison comparisonType)
		{
			if (op_Equality(value, null))
			{
				return false;
			}
			return string.Equals(InternalDecrypt(), value.InternalDecrypt(), comparisonType);
		}

		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}

		private static byte[] GetBytes(string text)
		{
			byte[] array = new byte[text.Length * 2];
			Buffer.BlockCopy(text.ToCharArray(), 0, array, 0, array.Length);
			return array;
		}

		private static string GetString(byte[] bytes)
		{
			char[] array = new char[bytes.Length / 2];
			Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
			return new string(array);
		}

		private static bool ArraysEquals(byte[] first, byte[] second)
		{
			if (first == second)
			{
				return true;
			}
			if (first != null && second != null)
			{
				if (first.Length != second.Length)
				{
					return false;
				}
				for (int i = 0; i < first.Length; i++)
				{
					if (first[i] != second[i])
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
