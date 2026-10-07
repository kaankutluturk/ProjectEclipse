using System;
using System.Runtime.CompilerServices;
using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	[Serializable]
	public struct ObscuredVector3
	{
		[Serializable]
		public struct RawEncryptedVector3
		{
			public int x;

			public int y;

			public int z;
		}

		private static int cryptoKey = 120207;

		private static readonly Vector3 initialFakeValue = Vector3.zero;

		[SerializeField]
		private int currentCryptoKey;

		[SerializeField]
		private RawEncryptedVector3 hiddenValue;

		[SerializeField]
		private Vector3 fakeValue;

		[SerializeField]
		private bool inited;

		public float x
		{
			get
			{
				float num = InternalDecryptField(hiddenValue.x);
				if (ObscuredCheatingDetector.GetIsRunning() && !fakeValue.Equals(initialFakeValue) && Math.Abs(num - fakeValue.x) > ObscuredCheatingDetector.get_Instance().vector3Epsilon)
				{
					ObscuredCheatingDetector.get_Instance().OnCheatingDetected();
				}
				return num;
			}
			set
			{
				hiddenValue.x = InternalEncryptField(value);
				if (ObscuredCheatingDetector.GetIsRunning())
				{
					fakeValue.x = value;
				}
			}
		}

		public float y
		{
			get
			{
				float num = InternalDecryptField(hiddenValue.y);
				if (ObscuredCheatingDetector.GetIsRunning() && !fakeValue.Equals(initialFakeValue) && Math.Abs(num - fakeValue.y) > ObscuredCheatingDetector.get_Instance().vector3Epsilon)
				{
					ObscuredCheatingDetector.get_Instance().OnCheatingDetected();
				}
				return num;
			}
			set
			{
				hiddenValue.y = InternalEncryptField(value);
				if (ObscuredCheatingDetector.GetIsRunning())
				{
					fakeValue.y = value;
				}
			}
		}

		public float z
		{
			get
			{
				float num = InternalDecryptField(hiddenValue.z);
				if (ObscuredCheatingDetector.GetIsRunning() && !fakeValue.Equals(initialFakeValue) && Math.Abs(num - fakeValue.z) > ObscuredCheatingDetector.get_Instance().vector3Epsilon)
				{
					ObscuredCheatingDetector.get_Instance().OnCheatingDetected();
				}
				return num;
			}
			set
			{
				hiddenValue.z = InternalEncryptField(value);
				if (ObscuredCheatingDetector.GetIsRunning())
				{
					fakeValue.z = value;
				}
			}
		}

		public float this[int index]
		{
			get
			{
				switch (index)
				{
				case 0:
					return x;
				case 1:
					return y;
				case 2:
					return z;
				default:
					throw new IndexOutOfRangeException("Invalid ObscuredVector3 index!");
				}
			}
			set
			{
				switch (index)
				{
				case 0:
					x = value;
					break;
				case 1:
					y = value;
					break;
				case 2:
					z = value;
					break;
				default:
					throw new IndexOutOfRangeException("Invalid ObscuredVector3 index!");
				}
			}
		}

		private ObscuredVector3(RawEncryptedVector3 encrypted)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = encrypted;
			fakeValue = initialFakeValue;
			inited = true;
		}

		public static void SetNewCryptoKey(int newKey)
		{
			cryptoKey = newKey;
		}

		public static RawEncryptedVector3 Encrypt(Vector3 value)
		{
			return Encrypt(value, 0);
		}

		public static RawEncryptedVector3 Encrypt(Vector3 value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}
			RawEncryptedVector3 result = default(RawEncryptedVector3);
			result.x = ObscuredFloat.Encrypt(value.x, key);
			result.y = ObscuredFloat.Encrypt(value.y, key);
			result.z = ObscuredFloat.Encrypt(value.z, key);
			return result;
		}

		public static Vector3 Decrypt(RawEncryptedVector3 value)
		{
			return Decrypt(value, 0);
		}

		public static Vector3 Decrypt(RawEncryptedVector3 value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}
			Vector3 result = default(Vector3);
			result.x = ObscuredFloat.Decrypt(value.x, key);
			result.y = ObscuredFloat.Decrypt(value.y, key);
			result.z = ObscuredFloat.Decrypt(value.z, key);
			return result;
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
			Vector3 decrypted = InternalDecrypt();
			currentCryptoKey = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		public RawEncryptedVector3 GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}

		public void SetEncrypted(RawEncryptedVector3 encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				fakeValue = InternalDecrypt();
			}
		}

		private Vector3 InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = Encrypt(initialFakeValue, cryptoKey);
				fakeValue = initialFakeValue;
				inited = true;
			}
			Vector3 vector = default(Vector3);
			vector.x = ObscuredFloat.Decrypt(hiddenValue.x, currentCryptoKey);
			vector.y = ObscuredFloat.Decrypt(hiddenValue.y, currentCryptoKey);
			vector.z = ObscuredFloat.Decrypt(hiddenValue.z, currentCryptoKey);
			if (ObscuredCheatingDetector.GetIsRunning() && !fakeValue.Equals(Vector3.zero) && !CompareVectorsWithTolerance(vector, fakeValue))
			{
				ObscuredCheatingDetector.get_Instance().OnCheatingDetected();
			}
			return vector;
		}

		private bool CompareVectorsWithTolerance(Vector3 first, Vector3 second)
		{
			float vector3Epsilon = ObscuredCheatingDetector.get_Instance().vector3Epsilon;
			return Math.Abs(first.x - second.x) < vector3Epsilon && Math.Abs(first.y - second.y) < vector3Epsilon && Math.Abs(first.z - second.z) < vector3Epsilon;
		}

		private float InternalDecryptField(int encrypted)
		{
			int key = cryptoKey;
			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}
			return ObscuredFloat.Decrypt(encrypted, key);
		}

		private int InternalEncryptField(float plain)
		{
			return ObscuredFloat.Encrypt(plain, cryptoKey);
		}

		public static implicit operator ObscuredVector3(Vector3 value)
		{
			ObscuredVector3 result = new ObscuredVector3(Encrypt(value, cryptoKey));
			if (ObscuredCheatingDetector.GetIsRunning())
			{
				result.fakeValue = value;
			}
			return result;
		}

		public static implicit operator Vector3(ObscuredVector3 value)
		{
			return value.InternalDecrypt();
		}

		[SpecialName]
		public static ObscuredVector3 op_Addition(ObscuredVector3 left, ObscuredVector3 right)
		{
			return (ObscuredVector3)(left.InternalDecrypt() + right.InternalDecrypt());
		}

		[SpecialName]
		public static ObscuredVector3 op_Addition(Vector3 left, ObscuredVector3 right)
		{
			return (ObscuredVector3)(left + right.InternalDecrypt());
		}

		[SpecialName]
		public static ObscuredVector3 op_Addition(ObscuredVector3 left, Vector3 right)
		{
			return (ObscuredVector3)(left.InternalDecrypt() + right);
		}

		[SpecialName]
		public static ObscuredVector3 op_Subtraction(ObscuredVector3 left, ObscuredVector3 right)
		{
			return (ObscuredVector3)(left.InternalDecrypt() - right.InternalDecrypt());
		}

		[SpecialName]
		public static ObscuredVector3 op_Subtraction(Vector3 left, ObscuredVector3 right)
		{
			return (ObscuredVector3)(left - right.InternalDecrypt());
		}

		[SpecialName]
		public static ObscuredVector3 op_Subtraction(ObscuredVector3 left, Vector3 right)
		{
			return (ObscuredVector3)(left.InternalDecrypt() - right);
		}

		[SpecialName]
		public static ObscuredVector3 op_UnaryNegation(ObscuredVector3 input)
		{
			return (ObscuredVector3)(-input.InternalDecrypt());
		}

		[SpecialName]
		public static ObscuredVector3 op_Multiply(ObscuredVector3 input, float d)
		{
			return (ObscuredVector3)(input.InternalDecrypt() * d);
		}

		[SpecialName]
		public static ObscuredVector3 op_Multiply(float d, ObscuredVector3 input)
		{
			return (ObscuredVector3)(d * input.InternalDecrypt());
		}

		[SpecialName]
		public static ObscuredVector3 op_Division(ObscuredVector3 input, float d)
		{
			return (ObscuredVector3)(input.InternalDecrypt() / d);
		}

		[SpecialName]
		public static bool op_Equality(ObscuredVector3 left, ObscuredVector3 right)
		{
			return left.InternalDecrypt() == right.InternalDecrypt();
		}

		[SpecialName]
		public static bool op_Equality(Vector3 left, ObscuredVector3 right)
		{
			return left == right.InternalDecrypt();
		}

		[SpecialName]
		public static bool op_Equality(ObscuredVector3 left, Vector3 right)
		{
			return left.InternalDecrypt() == right;
		}

		[SpecialName]
		public static bool op_Inequality(ObscuredVector3 left, ObscuredVector3 right)
		{
			return left.InternalDecrypt() != right.InternalDecrypt();
		}

		[SpecialName]
		public static bool op_Inequality(Vector3 left, ObscuredVector3 right)
		{
			return left != right.InternalDecrypt();
		}

		[SpecialName]
		public static bool op_Inequality(ObscuredVector3 left, Vector3 right)
		{
			return left.InternalDecrypt() != right;
		}

		public override bool Equals(object obj)
		{
			return InternalDecrypt().Equals(obj);
		}

		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}

		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}

		public string ToString(string format)
		{
			return InternalDecrypt().ToString(format);
		}
	}
}
