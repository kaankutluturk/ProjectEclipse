using System;
using System.Text;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public static class ObscuredPrefs
{
	public enum DataType : byte
	{
		Unknown = 0,
		Int = 5,
		UInt = 10,
		String = 15,
		Float = 20,
		Double = 25,
		Long = 30,
		Bool = 35,
		ByteArray = 40,
		Vector2 = 45,
		Vector3 = 50,
		Quaternion = 55,
		Color = 60,
		Rect = 65
	}

	public enum DeviceLockLevel : byte
	{
		None = 0,
		Soft = 1,
		Strict = 2
	}

	private const byte VERSION = 2;

	private const string RawNotFound = "{not_found}";

	private const string RawSeparator = "|";

	private static bool foreignSavesReported;

	private static string cryptoKey = "e806f6";

	private static string deviceId;

	private static uint deviceIdHash;

	public static Action OnAlterationDetected;

	public static bool PreservePlayerPrefs;

	public static Action OnPossibleForeignSavesDetected;

	public static DeviceLockLevel LockToDevice;

	public static bool ReadForeignSaves;

	public static bool EmergencyMode;

	private const char DEPRECATED_RAW_SEPARATOR = ':';

	private static string deprecatedDeviceId;

	public static string CryptoKey
	{
		get
		{
			return GetCryptoKey();
		}
		set
		{
			SetCryptoKey(value);
		}
	}

	public static string DeviceId
	{
		get
		{
			return GetDeviceId();
		}
		set
		{
			SetDeviceId(value);
		}
	}

	[Obsolete("This property is obsolete, please use DeviceId instead.")]
	public static string DeviceID
	{
		get
		{
			return GetDeviceIDObsolete();
		}
		set
		{
			SetDeviceIDObsolete(value);
		}
	}

	private static uint DeviceIdHash
	{
		get
		{
			return GetDeviceIdHash();
		}
	}

	private static string DeprecatedDeviceId
	{
		get
		{
			return GetDeprecatedDeviceId();
		}
	}

	public static void SetCryptoKey(string value)
	{
		cryptoKey = value;
	}

	public static string GetCryptoKey()
	{
		return cryptoKey;
	}

	public static string GetDeviceId()
	{
		if (string.IsNullOrEmpty(deviceId))
		{
			deviceId = GetDeviceIdInternal();
		}
		return deviceId;
	}

	public static void SetDeviceId(string value)
	{
		deviceId = value;
	}

	public static string GetDeviceIDObsolete()
	{
		return GetDeviceId();
	}

	public static void SetDeviceIDObsolete(string value)
	{
		SetDeviceId(value);
	}

	private static uint GetDeviceIdHash()
	{
		if (deviceIdHash == 0)
		{
			deviceIdHash = CalculateChecksum(GetDeviceId());
		}
		return deviceIdHash;
	}

	public static void ForceLockToDeviceInit()
	{
		if (string.IsNullOrEmpty(deviceId))
		{
			deviceId = GetDeviceIdInternal();
			deviceIdHash = CalculateChecksum(deviceId);
		}
		else
		{
			Debug.LogWarning("[ACTk] ObscuredPrefs.ForceLockToDeviceInit() is called, but device ID is already obtained!");
		}
	}

	[Obsolete("This method is obsolete, use property CryptoKey instead")]
	public static void SetNewCryptoKey(string CNOFJICCAHK)
	{
		SetCryptoKey(CNOFJICCAHK);
	}

	public static void SetInt(string KGBGENDIMBC, int value)
	{
		PlayerPrefs.SetString(EncryptKey(KGBGENDIMBC), EncryptIntValue(KGBGENDIMBC, value));
	}

	public static int GetInt(string KGBGENDIMBC)
	{
		return GetInt(KGBGENDIMBC, 0);
	}

	public static int GetInt(string KGBGENDIMBC, int OBPKDHBJKJL)
	{
		string text = EncryptKey(KGBGENDIMBC);
		if (!PlayerPrefs.HasKey(text) && PlayerPrefs.HasKey(KGBGENDIMBC))
		{
			int num = PlayerPrefs.GetInt(KGBGENDIMBC, OBPKDHBJKJL);
			if (!PreservePlayerPrefs)
			{
				SetInt(KGBGENDIMBC, num);
				PlayerPrefs.DeleteKey(KGBGENDIMBC);
			}
			return num;
		}
		string text2 = GetEncryptedPrefsString(KGBGENDIMBC, text);
		return (!(text2 == "{not_found}")) ? DecryptIntValue(KGBGENDIMBC, text2, OBPKDHBJKJL) : OBPKDHBJKJL;
	}

	public static string EncryptIntValue(string KGBGENDIMBC, int value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		return EncryptData(KGBGENDIMBC, bytes, DataType.Int);
	}

	public static int DecryptIntValue(string KGBGENDIMBC, string JKPOIFLKGEN, int OBPKDHBJKJL)
	{
		if (JKPOIFLKGEN.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(JKPOIFLKGEN);
			if (text == string.Empty)
			{
				return OBPKDHBJKJL;
			}
			int result;
			int.TryParse(text, out result);
			SetInt(KGBGENDIMBC, result);
			return result;
		}
		byte[] array = DecryptData(KGBGENDIMBC, JKPOIFLKGEN);
		if (array == null)
		{
			return OBPKDHBJKJL;
		}
		return BitConverter.ToInt32(array, 0);
	}

	public static void SetUInt(string KGBGENDIMBC, uint value)
	{
		PlayerPrefs.SetString(EncryptKey(KGBGENDIMBC), EncryptUIntValue(KGBGENDIMBC, value));
	}

	public static uint GetUInt(string KGBGENDIMBC)
	{
		return GetUInt(KGBGENDIMBC, 0u);
	}

	public static uint GetUInt(string KGBGENDIMBC, uint OBPKDHBJKJL)
	{
		string text = GetEncryptedPrefsString(KGBGENDIMBC, EncryptKey(KGBGENDIMBC));
		return (!(text == "{not_found}")) ? DecryptUIntValue(KGBGENDIMBC, text, OBPKDHBJKJL) : OBPKDHBJKJL;
	}

	private static string EncryptUIntValue(string KGBGENDIMBC, uint value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		return EncryptData(KGBGENDIMBC, bytes, DataType.UInt);
	}

	private static uint DecryptUIntValue(string KGBGENDIMBC, string JKPOIFLKGEN, uint OBPKDHBJKJL)
	{
		if (JKPOIFLKGEN.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(JKPOIFLKGEN);
			if (text == string.Empty)
			{
				return OBPKDHBJKJL;
			}
			uint result;
			uint.TryParse(text, out result);
			SetUInt(KGBGENDIMBC, result);
			return result;
		}
		byte[] array = DecryptData(KGBGENDIMBC, JKPOIFLKGEN);
		if (array == null)
		{
			return OBPKDHBJKJL;
		}
		return BitConverter.ToUInt32(array, 0);
	}

	public static void SetString(string KGBGENDIMBC, string value)
	{
		PlayerPrefs.SetString(EncryptKey(KGBGENDIMBC), EncryptStringValue(KGBGENDIMBC, value));
	}

	public static string GetString(string KGBGENDIMBC)
	{
		return GetString(KGBGENDIMBC, string.Empty);
	}

	public static string GetString(string KGBGENDIMBC, string OBPKDHBJKJL)
	{
		string text = EncryptKey(KGBGENDIMBC);
		if (!PlayerPrefs.HasKey(text) && PlayerPrefs.HasKey(KGBGENDIMBC))
		{
			string text2 = PlayerPrefs.GetString(KGBGENDIMBC, OBPKDHBJKJL);
			if (!PreservePlayerPrefs)
			{
				SetString(KGBGENDIMBC, text2);
				PlayerPrefs.DeleteKey(KGBGENDIMBC);
			}
			return text2;
		}
		string text3 = GetEncryptedPrefsString(KGBGENDIMBC, text);
		return (!(text3 == "{not_found}")) ? DecryptStringValue(KGBGENDIMBC, text3, OBPKDHBJKJL) : OBPKDHBJKJL;
	}

	public static string EncryptStringValue(string KGBGENDIMBC, string value)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		return EncryptData(KGBGENDIMBC, bytes, DataType.String);
	}

	public static string DecryptStringValue(string KGBGENDIMBC, string JKPOIFLKGEN, string OBPKDHBJKJL)
	{
		if (JKPOIFLKGEN.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(JKPOIFLKGEN);
			if (text == string.Empty)
			{
				return OBPKDHBJKJL;
			}
			SetString(KGBGENDIMBC, text);
			return text;
		}
		byte[] array = DecryptData(KGBGENDIMBC, JKPOIFLKGEN);
		if (array == null)
		{
			return OBPKDHBJKJL;
		}
		return Encoding.UTF8.GetString(array, 0, array.Length);
	}

	public static void SetFloat(string KGBGENDIMBC, float value)
	{
		PlayerPrefs.SetString(EncryptKey(KGBGENDIMBC), EncryptFloatValue(KGBGENDIMBC, value));
	}

	public static float GetFloat(string KGBGENDIMBC)
	{
		return GetFloat(KGBGENDIMBC, 0f);
	}

	public static float GetFloat(string KGBGENDIMBC, float OBPKDHBJKJL)
	{
		string text = EncryptKey(KGBGENDIMBC);
		if (!PlayerPrefs.HasKey(text) && PlayerPrefs.HasKey(KGBGENDIMBC))
		{
			float num = PlayerPrefs.GetFloat(KGBGENDIMBC, OBPKDHBJKJL);
			if (!PreservePlayerPrefs)
			{
				SetFloat(KGBGENDIMBC, num);
				PlayerPrefs.DeleteKey(KGBGENDIMBC);
			}
			return num;
		}
		string text2 = GetEncryptedPrefsString(KGBGENDIMBC, text);
		return (!(text2 == "{not_found}")) ? DecryptFloatValue(KGBGENDIMBC, text2, OBPKDHBJKJL) : OBPKDHBJKJL;
	}

	public static string EncryptFloatValue(string KGBGENDIMBC, float value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		return EncryptData(KGBGENDIMBC, bytes, DataType.Float);
	}

	public static float DecryptFloatValue(string KGBGENDIMBC, string JKPOIFLKGEN, float OBPKDHBJKJL)
	{
		if (JKPOIFLKGEN.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(JKPOIFLKGEN);
			if (text == string.Empty)
			{
				return OBPKDHBJKJL;
			}
			float result;
			float.TryParse(text, out result);
			SetFloat(KGBGENDIMBC, result);
			return result;
		}
		byte[] array = DecryptData(KGBGENDIMBC, JKPOIFLKGEN);
		if (array == null)
		{
			return OBPKDHBJKJL;
		}
		return BitConverter.ToSingle(array, 0);
	}

	public static void SetDouble(string KGBGENDIMBC, double value)
	{
		PlayerPrefs.SetString(EncryptKey(KGBGENDIMBC), EncryptDoubleValue(KGBGENDIMBC, value));
	}

	public static double GetDouble(string KGBGENDIMBC)
	{
		return GetDouble(KGBGENDIMBC, 0.0);
	}

	public static double GetDouble(string KGBGENDIMBC, double OBPKDHBJKJL)
	{
		string text = GetEncryptedPrefsString(KGBGENDIMBC, EncryptKey(KGBGENDIMBC));
		return (!(text == "{not_found}")) ? DecryptDoubleValue(KGBGENDIMBC, text, OBPKDHBJKJL) : OBPKDHBJKJL;
	}

	private static string EncryptDoubleValue(string KGBGENDIMBC, double value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		return EncryptData(KGBGENDIMBC, bytes, DataType.Double);
	}

	private static double DecryptDoubleValue(string KGBGENDIMBC, string JKPOIFLKGEN, double OBPKDHBJKJL)
	{
		if (JKPOIFLKGEN.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(JKPOIFLKGEN);
			if (text == string.Empty)
			{
				return OBPKDHBJKJL;
			}
			double result;
			double.TryParse(text, out result);
			SetDouble(KGBGENDIMBC, result);
			return result;
		}
		byte[] array = DecryptData(KGBGENDIMBC, JKPOIFLKGEN);
		if (array == null)
		{
			return OBPKDHBJKJL;
		}
		return BitConverter.ToDouble(array, 0);
	}

	public static void SetLong(string KGBGENDIMBC, long value)
	{
		PlayerPrefs.SetString(EncryptKey(KGBGENDIMBC), EncryptLongValue(KGBGENDIMBC, value));
	}

	public static long GetLong(string KGBGENDIMBC)
	{
		return GetLong(KGBGENDIMBC, 0L);
	}

	public static long GetLong(string KGBGENDIMBC, long OBPKDHBJKJL)
	{
		string text = GetEncryptedPrefsString(KGBGENDIMBC, EncryptKey(KGBGENDIMBC));
		return (!(text == "{not_found}")) ? DecryptLongValue(KGBGENDIMBC, text, OBPKDHBJKJL) : OBPKDHBJKJL;
	}

	private static string EncryptLongValue(string KGBGENDIMBC, long value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		return EncryptData(KGBGENDIMBC, bytes, DataType.Long);
	}

	private static long DecryptLongValue(string KGBGENDIMBC, string JKPOIFLKGEN, long OBPKDHBJKJL)
	{
		if (JKPOIFLKGEN.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(JKPOIFLKGEN);
			if (text == string.Empty)
			{
				return OBPKDHBJKJL;
			}
			long result;
			long.TryParse(text, out result);
			SetLong(KGBGENDIMBC, result);
			return result;
		}
		byte[] array = DecryptData(KGBGENDIMBC, JKPOIFLKGEN);
		if (array == null)
		{
			return OBPKDHBJKJL;
		}
		return BitConverter.ToInt64(array, 0);
	}

	public static void SetBool(string KGBGENDIMBC, bool value)
	{
		PlayerPrefs.SetString(EncryptKey(KGBGENDIMBC), EncryptBoolValue(KGBGENDIMBC, value));
	}

	public static bool GetBool(string KGBGENDIMBC)
	{
		return GetBool(KGBGENDIMBC, false);
	}

	public static bool GetBool(string KGBGENDIMBC, bool OBPKDHBJKJL)
	{
		string text = GetEncryptedPrefsString(KGBGENDIMBC, EncryptKey(KGBGENDIMBC));
		return (!(text == "{not_found}")) ? DecryptBoolValue(KGBGENDIMBC, text, OBPKDHBJKJL) : OBPKDHBJKJL;
	}

	private static string EncryptBoolValue(string KGBGENDIMBC, bool value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		return EncryptData(KGBGENDIMBC, bytes, DataType.Bool);
	}

	private static bool DecryptBoolValue(string KGBGENDIMBC, string JKPOIFLKGEN, bool OBPKDHBJKJL)
	{
		if (JKPOIFLKGEN.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(JKPOIFLKGEN);
			if (text == string.Empty)
			{
				return OBPKDHBJKJL;
			}
			int result;
			int.TryParse(text, out result);
			SetBool(KGBGENDIMBC, result == 1);
			return result == 1;
		}
		byte[] array = DecryptData(KGBGENDIMBC, JKPOIFLKGEN);
		if (array == null)
		{
			return OBPKDHBJKJL;
		}
		return BitConverter.ToBoolean(array, 0);
	}

	public static void SetByteArray(string KGBGENDIMBC, byte[] value)
	{
		PlayerPrefs.SetString(EncryptKey(KGBGENDIMBC), EncryptByteArrayValue(KGBGENDIMBC, value));
	}

	public static byte[] GetByteArray(string KGBGENDIMBC)
	{
		return GetByteArray(KGBGENDIMBC, 0, 0);
	}

	public static byte[] GetByteArray(string KGBGENDIMBC, byte OBPKDHBJKJL, int HLNIEGFECPK)
	{
		string text = GetEncryptedPrefsString(KGBGENDIMBC, EncryptKey(KGBGENDIMBC));
		if (text == "{not_found}")
		{
			return ConstructByteArray(OBPKDHBJKJL, HLNIEGFECPK);
		}
		return DecryptByteArrayValue(KGBGENDIMBC, text, OBPKDHBJKJL, HLNIEGFECPK);
	}

	private static string EncryptByteArrayValue(string KGBGENDIMBC, byte[] value)
	{
		return EncryptData(KGBGENDIMBC, value, DataType.ByteArray);
	}

	private static byte[] DecryptByteArrayValue(string KGBGENDIMBC, string JKPOIFLKGEN, byte OBPKDHBJKJL, int HLNIEGFECPK)
	{
		if (JKPOIFLKGEN.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(JKPOIFLKGEN);
			if (text == string.Empty)
			{
				return ConstructByteArray(OBPKDHBJKJL, HLNIEGFECPK);
			}
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			SetByteArray(KGBGENDIMBC, bytes);
			return bytes;
		}
		byte[] array = DecryptData(KGBGENDIMBC, JKPOIFLKGEN);
		if (array == null)
		{
			return ConstructByteArray(OBPKDHBJKJL, HLNIEGFECPK);
		}
		return array;
	}

	private static byte[] ConstructByteArray(byte value, int BDBOAEGELMC)
	{
		byte[] array = new byte[BDBOAEGELMC];
		for (int i = 0; i < BDBOAEGELMC; i++)
		{
			array[i] = value;
		}
		return array;
	}

	public static void SetVector2(string KGBGENDIMBC, Vector2 value)
	{
		PlayerPrefs.SetString(EncryptKey(KGBGENDIMBC), EncryptVector2Value(KGBGENDIMBC, value));
	}

	public static Vector2 GetVector2(string KGBGENDIMBC)
	{
		return GetVector2(KGBGENDIMBC, Vector2.zero);
	}

	public static Vector2 GetVector2(string KGBGENDIMBC, Vector2 OBPKDHBJKJL)
	{
		string text = GetEncryptedPrefsString(KGBGENDIMBC, EncryptKey(KGBGENDIMBC));
		return (!(text == "{not_found}")) ? DecryptVector2Value(KGBGENDIMBC, text, OBPKDHBJKJL) : OBPKDHBJKJL;
	}

	private static string EncryptVector2Value(string KGBGENDIMBC, Vector2 value)
	{
		byte[] array = new byte[8];
		Buffer.BlockCopy(BitConverter.GetBytes(value.x), 0, array, 0, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(value.y), 0, array, 4, 4);
		return EncryptData(KGBGENDIMBC, array, DataType.Vector2);
	}

	private static Vector2 DecryptVector2Value(string KGBGENDIMBC, string JKPOIFLKGEN, Vector2 OBPKDHBJKJL)
	{
		if (JKPOIFLKGEN.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(JKPOIFLKGEN);
			if (text == string.Empty)
			{
				return OBPKDHBJKJL;
			}
			string[] array = text.Split("|"[0]);
			float result;
			float.TryParse(array[0], out result);
			float result2;
			float.TryParse(array[1], out result2);
			Vector2 vector = new Vector2(result, result2);
			SetVector2(KGBGENDIMBC, vector);
			return vector;
		}
		byte[] array2 = DecryptData(KGBGENDIMBC, JKPOIFLKGEN);
		if (array2 == null)
		{
			return OBPKDHBJKJL;
		}
		Vector2 result3 = default(Vector2);
		result3.x = BitConverter.ToSingle(array2, 0);
		result3.y = BitConverter.ToSingle(array2, 4);
		return result3;
	}

	public static void SetVector3(string KGBGENDIMBC, Vector3 value)
	{
		PlayerPrefs.SetString(EncryptKey(KGBGENDIMBC), EncryptVector3Value(KGBGENDIMBC, value));
	}

	public static Vector3 GetVector3(string KGBGENDIMBC)
	{
		return GetVector3(KGBGENDIMBC, Vector3.zero);
	}

	public static Vector3 GetVector3(string KGBGENDIMBC, Vector3 OBPKDHBJKJL)
	{
		string text = GetEncryptedPrefsString(KGBGENDIMBC, EncryptKey(KGBGENDIMBC));
		return (!(text == "{not_found}")) ? DecryptVector3Value(KGBGENDIMBC, text, OBPKDHBJKJL) : OBPKDHBJKJL;
	}

	private static string EncryptVector3Value(string KGBGENDIMBC, Vector3 value)
	{
		byte[] array = new byte[12];
		Buffer.BlockCopy(BitConverter.GetBytes(value.x), 0, array, 0, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(value.y), 0, array, 4, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(value.z), 0, array, 8, 4);
		return EncryptData(KGBGENDIMBC, array, DataType.Vector3);
	}

	private static Vector3 DecryptVector3Value(string KGBGENDIMBC, string JKPOIFLKGEN, Vector3 OBPKDHBJKJL)
	{
		if (JKPOIFLKGEN.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(JKPOIFLKGEN);
			if (text == string.Empty)
			{
				return OBPKDHBJKJL;
			}
			string[] array = text.Split("|"[0]);
			float result;
			float.TryParse(array[0], out result);
			float result2;
			float.TryParse(array[1], out result2);
			float result3;
			float.TryParse(array[2], out result3);
			Vector3 vector = new Vector3(result, result2, result3);
			SetVector3(KGBGENDIMBC, vector);
			return vector;
		}
		byte[] array2 = DecryptData(KGBGENDIMBC, JKPOIFLKGEN);
		if (array2 == null)
		{
			return OBPKDHBJKJL;
		}
		Vector3 result4 = default(Vector3);
		result4.x = BitConverter.ToSingle(array2, 0);
		result4.y = BitConverter.ToSingle(array2, 4);
		result4.z = BitConverter.ToSingle(array2, 8);
		return result4;
	}

	public static void SetQuaternion(string KGBGENDIMBC, Quaternion value)
	{
		PlayerPrefs.SetString(EncryptKey(KGBGENDIMBC), EncryptQuaternionValue(KGBGENDIMBC, value));
	}

	public static Quaternion GetQuaternion(string KGBGENDIMBC)
	{
		return GetQuaternion(KGBGENDIMBC, Quaternion.identity);
	}

	public static Quaternion GetQuaternion(string KGBGENDIMBC, Quaternion OBPKDHBJKJL)
	{
		string text = GetEncryptedPrefsString(KGBGENDIMBC, EncryptKey(KGBGENDIMBC));
		return (!(text == "{not_found}")) ? DecryptQuaternionValue(KGBGENDIMBC, text, OBPKDHBJKJL) : OBPKDHBJKJL;
	}

	private static string EncryptQuaternionValue(string KGBGENDIMBC, Quaternion value)
	{
		byte[] array = new byte[16];
		Buffer.BlockCopy(BitConverter.GetBytes(value.x), 0, array, 0, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(value.y), 0, array, 4, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(value.z), 0, array, 8, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(value.w), 0, array, 12, 4);
		return EncryptData(KGBGENDIMBC, array, DataType.Quaternion);
	}

	private static Quaternion DecryptQuaternionValue(string KGBGENDIMBC, string JKPOIFLKGEN, Quaternion OBPKDHBJKJL)
	{
		if (JKPOIFLKGEN.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(JKPOIFLKGEN);
			if (text == string.Empty)
			{
				return OBPKDHBJKJL;
			}
			string[] array = text.Split("|"[0]);
			float result;
			float.TryParse(array[0], out result);
			float result2;
			float.TryParse(array[1], out result2);
			float result3;
			float.TryParse(array[2], out result3);
			float result4;
			float.TryParse(array[3], out result4);
			Quaternion quaternion = new Quaternion(result, result2, result3, result4);
			SetQuaternion(KGBGENDIMBC, quaternion);
			return quaternion;
		}
		byte[] array2 = DecryptData(KGBGENDIMBC, JKPOIFLKGEN);
		if (array2 == null)
		{
			return OBPKDHBJKJL;
		}
		Quaternion result5 = default(Quaternion);
		result5.x = BitConverter.ToSingle(array2, 0);
		result5.y = BitConverter.ToSingle(array2, 4);
		result5.z = BitConverter.ToSingle(array2, 8);
		result5.w = BitConverter.ToSingle(array2, 12);
		return result5;
	}

	public static void SetColor(string KGBGENDIMBC, Color32 value)
	{
		uint bAINMLLIKOL = (uint)((value.a << 24) | (value.r << 16) | (value.g << 8) | value.b);
		PlayerPrefs.SetString(EncryptKey(KGBGENDIMBC), EncryptColorValue(KGBGENDIMBC, bAINMLLIKOL));
	}

	public static Color32 GetColor(string KGBGENDIMBC)
	{
		return GetColor(KGBGENDIMBC, new Color32(0, 0, 0, 1));
	}

	public static Color32 GetColor(string KGBGENDIMBC, Color32 OBPKDHBJKJL)
	{
		string text = GetEncryptedPrefsString(KGBGENDIMBC, EncryptKey(KGBGENDIMBC));
		if (text == "{not_found}")
		{
			return OBPKDHBJKJL;
		}
		uint num = DecryptUIntValue(KGBGENDIMBC, text, 16777216u);
		byte a = (byte)(num >> 24);
		byte r = (byte)(num >> 16);
		byte g = (byte)(num >> 8);
		byte b = (byte)(num >> 0);
		return new Color32(r, g, b, a);
	}

	private static string EncryptColorValue(string KGBGENDIMBC, uint value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		return EncryptData(KGBGENDIMBC, bytes, DataType.Color);
	}

	public static void SetRect(string KGBGENDIMBC, Rect value)
	{
		PlayerPrefs.SetString(EncryptKey(KGBGENDIMBC), EncryptRectValue(KGBGENDIMBC, value));
	}

	public static Rect GetRect(string KGBGENDIMBC)
	{
		return GetRect(KGBGENDIMBC, new Rect(0f, 0f, 0f, 0f));
	}

	public static Rect GetRect(string KGBGENDIMBC, Rect OBPKDHBJKJL)
	{
		string text = GetEncryptedPrefsString(KGBGENDIMBC, EncryptKey(KGBGENDIMBC));
		return (!(text == "{not_found}")) ? DecryptRectValue(KGBGENDIMBC, text, OBPKDHBJKJL) : OBPKDHBJKJL;
	}

	private static string EncryptRectValue(string KGBGENDIMBC, Rect value)
	{
		byte[] array = new byte[16];
		Buffer.BlockCopy(BitConverter.GetBytes(value.x), 0, array, 0, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(value.y), 0, array, 4, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(value.width), 0, array, 8, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(value.height), 0, array, 12, 4);
		return EncryptData(KGBGENDIMBC, array, DataType.Rect);
	}

	private static Rect DecryptRectValue(string KGBGENDIMBC, string JKPOIFLKGEN, Rect OBPKDHBJKJL)
	{
		if (JKPOIFLKGEN.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(JKPOIFLKGEN);
			if (text == string.Empty)
			{
				return OBPKDHBJKJL;
			}
			string[] array = text.Split("|"[0]);
			float result;
			float.TryParse(array[0], out result);
			float result2;
			float.TryParse(array[1], out result2);
			float result3;
			float.TryParse(array[2], out result3);
			float result4;
			float.TryParse(array[3], out result4);
			Rect rect = new Rect(result, result2, result3, result4);
			SetRect(KGBGENDIMBC, rect);
			return rect;
		}
		byte[] array2 = DecryptData(KGBGENDIMBC, JKPOIFLKGEN);
		if (array2 == null)
		{
			return OBPKDHBJKJL;
		}
		return new Rect
		{
			x = BitConverter.ToSingle(array2, 0),
			y = BitConverter.ToSingle(array2, 4),
			width = BitConverter.ToSingle(array2, 8),
			height = BitConverter.ToSingle(array2, 12)
		};
	}

	public static void SetRawValue(string KGBGENDIMBC, string LJACLKKLMPA)
	{
		PlayerPrefs.SetString(EncryptKey(KGBGENDIMBC), LJACLKKLMPA);
	}

	public static string GetRawValue(string KGBGENDIMBC)
	{
		string key = EncryptKey(KGBGENDIMBC);
		return PlayerPrefs.GetString(key);
	}

	public static DataType GetRawValueType(string value)
	{
		DataType result = DataType.Unknown;
		byte[] array;
		try
		{
			array = Convert.FromBase64String(value);
		}
		catch (Exception)
		{
			return result;
		}
		if (array.Length < 7)
		{
			return result;
		}
		int num = array.Length;
		return (DataType)array[num - 7];
	}

	public static string EncryptKey(string KGBGENDIMBC)
	{
		KGBGENDIMBC = ObscuredString.EncryptDecrypt(KGBGENDIMBC, cryptoKey);
		KGBGENDIMBC = Convert.ToBase64String(Encoding.UTF8.GetBytes(KGBGENDIMBC));
		return KGBGENDIMBC;
	}

	public static bool HasKey(string KGBGENDIMBC)
	{
		return PlayerPrefs.HasKey(KGBGENDIMBC) || PlayerPrefs.HasKey(EncryptKey(KGBGENDIMBC));
	}

	public static void DeleteKey(string KGBGENDIMBC)
	{
		PlayerPrefs.DeleteKey(EncryptKey(KGBGENDIMBC));
		if (!PreservePlayerPrefs)
		{
			PlayerPrefs.DeleteKey(KGBGENDIMBC);
		}
	}

	public static void DeleteAll()
	{
		PlayerPrefs.DeleteAll();
	}

	public static void Save()
	{
		PlayerPrefs.Save();
	}

	private static string GetEncryptedPrefsString(string KGBGENDIMBC, string ILBAAOLECKP)
	{
		string text = PlayerPrefs.GetString(ILBAAOLECKP, "{not_found}");
		if (text == "{not_found}" && PlayerPrefs.HasKey(KGBGENDIMBC))
		{
			Debug.LogWarning("[ACTk] Are you trying to read regular PlayerPrefs data using ObscuredPrefs (key = " + KGBGENDIMBC + ")?");
		}
		return text;
	}

	private static string EncryptData(string KGBGENDIMBC, byte[] LLIKENLOBPI, DataType LFLGCDNKNJI)
	{
		int num = LLIKENLOBPI.Length;
		byte[] src = EncryptDecryptBytes(LLIKENLOBPI, num, KGBGENDIMBC + cryptoKey);
		uint num2 = xxHash.CalculateHash(LLIKENLOBPI, num, 0u);
		byte[] src2 = new byte[4]
		{
			(byte)(num2 & 0xFF),
			(byte)((num2 >> 8) & 0xFF),
			(byte)((num2 >> 16) & 0xFF),
			(byte)((num2 >> 24) & 0xFF)
		};
		byte[] array = null;
		int num3;
		if (LockToDevice != DeviceLockLevel.None)
		{
			num3 = num + 11;
			uint num4 = GetDeviceIdHash();
			array = new byte[4]
			{
				(byte)(num4 & 0xFF),
				(byte)((num4 >> 8) & 0xFF),
				(byte)((num4 >> 16) & 0xFF),
				(byte)((num4 >> 24) & 0xFF)
			};
		}
		else
		{
			num3 = num + 7;
		}
		byte[] array2 = new byte[num3];
		Buffer.BlockCopy(src, 0, array2, 0, num);
		if (array != null)
		{
			Buffer.BlockCopy(array, 0, array2, num, 4);
		}
		array2[num3 - 7] = (byte)LFLGCDNKNJI;
		array2[num3 - 6] = 2;
		array2[num3 - 5] = (byte)LockToDevice;
		Buffer.BlockCopy(src2, 0, array2, num3 - 4, 4);
		return Convert.ToBase64String(array2);
	}

	public static byte[] DecryptData(string KGBGENDIMBC, string JKPOIFLKGEN)
	{
		byte[] array;
		try
		{
			array = Convert.FromBase64String(JKPOIFLKGEN);
		}
		catch (Exception)
		{
			SavesTampered();
			return null;
		}
		if (array.Length <= 0)
		{
			SavesTampered();
			return null;
		}
		int num = array.Length;
		byte b = array[num - 6];
		if (b != 2)
		{
			SavesTampered();
			return null;
		}
		DeviceLockLevel eAONKJOAGJI = (DeviceLockLevel)array[num - 5];
		byte[] array2 = new byte[4];
		Buffer.BlockCopy(array, num - 4, array2, 0, 4);
		uint num2 = (uint)(array2[0] | (array2[1] << 8) | (array2[2] << 16) | (array2[3] << 24));
		uint num3 = 0u;
		int num4;
		if (eAONKJOAGJI != DeviceLockLevel.None)
		{
			num4 = num - 11;
			if (LockToDevice != DeviceLockLevel.None)
			{
				byte[] array3 = new byte[4];
				Buffer.BlockCopy(array, num4, array3, 0, 4);
				num3 = (uint)(array3[0] | (array3[1] << 8) | (array3[2] << 16) | (array3[3] << 24));
			}
		}
		else
		{
			num4 = num - 7;
		}
		byte[] array4 = new byte[num4];
		Buffer.BlockCopy(array, 0, array4, 0, num4);
		byte[] array5 = EncryptDecryptBytes(array4, num4, KGBGENDIMBC + cryptoKey);
		uint num5 = xxHash.CalculateHash(array5, num4, 0u);
		if (num5 != num2)
		{
			SavesTampered();
			return null;
		}
		if (LockToDevice == DeviceLockLevel.Strict && num3 == 0 && !EmergencyMode && !ReadForeignSaves)
		{
			return null;
		}
		if (num3 != 0 && !EmergencyMode)
		{
			uint num6 = GetDeviceIdHash();
			if (num3 != num6)
			{
				PossibleForeignSavesDetected();
				if (!ReadForeignSaves)
				{
					return null;
				}
			}
		}
		return array5;
	}

	private static uint CalculateChecksum(string NILNDHEKNLJ)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(NILNDHEKNLJ + cryptoKey);
		return xxHash.CalculateHash(bytes, bytes.Length, 0u);
	}

	private static void SavesTampered()
	{
		if (OnAlterationDetected != null)
		{
			OnAlterationDetected();
			OnAlterationDetected = null;
		}
	}

	private static void PossibleForeignSavesDetected()
	{
		if (OnPossibleForeignSavesDetected != null && !foreignSavesReported)
		{
			foreignSavesReported = true;
			OnPossibleForeignSavesDetected();
		}
	}

	private static string GetDeviceIdInternal()
	{
		string text = string.Empty;
		if (string.IsNullOrEmpty(text))
		{
			text = SystemInfo.deviceUniqueIdentifier;
		}
		return text;
	}

	private static byte[] EncryptDecryptBytes(byte[] KPAMPCLHCEN, int HIGBAHGOFIJ, string KGBGENDIMBC)
	{
		int length = KGBGENDIMBC.Length;
		byte[] array = new byte[HIGBAHGOFIJ];
		for (int i = 0; i < HIGBAHGOFIJ; i++)
		{
			array[i] = (byte)(KPAMPCLHCEN[i] ^ KGBGENDIMBC[i % length]);
		}
		return array;
	}

	private static string DeprecatedDecrypt(string value)
	{
		string[] array = value.Split(':');
		if (array.Length < 2)
		{
			SavesTampered();
			return string.Empty;
		}
		string text = array[0];
		string text2 = array[1];
		byte[] array2;
		try
		{
			array2 = Convert.FromBase64String(text);
		}
		catch
		{
			SavesTampered();
			return string.Empty;
		}
		string bAINMLLIKOL = Encoding.UTF8.GetString(array2, 0, array2.Length);
		string result = ObscuredString.EncryptDecrypt(bAINMLLIKOL, cryptoKey);
		if (array.Length == 3)
		{
			if (text2 != DeprecatedCalculateChecksum(text + GetDeprecatedDeviceId()))
			{
				SavesTampered();
			}
		}
		else if (array.Length == 2)
		{
			if (text2 != DeprecatedCalculateChecksum(text))
			{
				SavesTampered();
			}
		}
		else
		{
			SavesTampered();
		}
		if (LockToDevice != DeviceLockLevel.None && !EmergencyMode)
		{
			if (array.Length >= 3)
			{
				string text3 = array[2];
				if (text3 != GetDeprecatedDeviceId())
				{
					if (!ReadForeignSaves)
					{
						result = string.Empty;
					}
					PossibleForeignSavesDetected();
				}
			}
			else if (LockToDevice == DeviceLockLevel.Strict)
			{
				if (!ReadForeignSaves)
				{
					result = string.Empty;
				}
				PossibleForeignSavesDetected();
			}
			else if (text2 != DeprecatedCalculateChecksum(text))
			{
				if (!ReadForeignSaves)
				{
					result = string.Empty;
				}
				PossibleForeignSavesDetected();
			}
		}
		return result;
	}

	private static string DeprecatedCalculateChecksum(string NILNDHEKNLJ)
	{
		int num = 0;
		byte[] bytes = Encoding.UTF8.GetBytes(NILNDHEKNLJ + cryptoKey);
		int num2 = bytes.Length;
		int num3 = cryptoKey.Length ^ 0x40;
		for (int i = 0; i < num2; i++)
		{
			byte b = bytes[i];
			num += b + b * (i + num3) % 3;
		}
		return num.ToString("X2");
	}

	private static string GetDeprecatedDeviceId()
	{
		if (string.IsNullOrEmpty(deprecatedDeviceId))
		{
			deprecatedDeviceId = DeprecatedCalculateChecksum(GetDeviceId());
		}
		return deprecatedDeviceId;
	}
}
