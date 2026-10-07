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
	public static void SetNewCryptoKey(string newCryptoKey)
	{
		SetCryptoKey(newCryptoKey);
	}

	public static void SetInt(string key, int value)
	{
		PlayerPrefs.SetString(EncryptKey(key), EncryptIntValue(key, value));
	}

	public static int GetInt(string key)
	{
		return GetInt(key, 0);
	}

	public static int GetInt(string key, int defaultValue)
	{
		string text = EncryptKey(key);
		if (!PlayerPrefs.HasKey(text) && PlayerPrefs.HasKey(key))
		{
			int num = PlayerPrefs.GetInt(key, defaultValue);
			if (!PreservePlayerPrefs)
			{
				SetInt(key, num);
				PlayerPrefs.DeleteKey(key);
			}
			return num;
		}
		string text2 = GetEncryptedPrefsString(key, text);
		return (!(text2 == "{not_found}")) ? DecryptIntValue(key, text2, defaultValue) : defaultValue;
	}

	public static string EncryptIntValue(string key, int value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		return EncryptData(key, bytes, DataType.Int);
	}

	public static int DecryptIntValue(string key, string encryptedValue, int defaultValue)
	{
		if (encryptedValue.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(encryptedValue);
			if (text == string.Empty)
			{
				return defaultValue;
			}
			int result;
			int.TryParse(text, out result);
			SetInt(key, result);
			return result;
		}
		byte[] array = DecryptData(key, encryptedValue);
		if (array == null)
		{
			return defaultValue;
		}
		return BitConverter.ToInt32(array, 0);
	}

	public static void SetUInt(string key, uint value)
	{
		PlayerPrefs.SetString(EncryptKey(key), EncryptUIntValue(key, value));
	}

	public static uint GetUInt(string key)
	{
		return GetUInt(key, 0u);
	}

	public static uint GetUInt(string key, uint defaultValue)
	{
		string text = GetEncryptedPrefsString(key, EncryptKey(key));
		return (!(text == "{not_found}")) ? DecryptUIntValue(key, text, defaultValue) : defaultValue;
	}

	private static string EncryptUIntValue(string key, uint value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		return EncryptData(key, bytes, DataType.UInt);
	}

	private static uint DecryptUIntValue(string key, string encryptedValue, uint defaultValue)
	{
		if (encryptedValue.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(encryptedValue);
			if (text == string.Empty)
			{
				return defaultValue;
			}
			uint result;
			uint.TryParse(text, out result);
			SetUInt(key, result);
			return result;
		}
		byte[] array = DecryptData(key, encryptedValue);
		if (array == null)
		{
			return defaultValue;
		}
		return BitConverter.ToUInt32(array, 0);
	}

	public static void SetString(string key, string value)
	{
		PlayerPrefs.SetString(EncryptKey(key), EncryptStringValue(key, value));
	}

	public static string GetString(string key)
	{
		return GetString(key, string.Empty);
	}

	public static string GetString(string key, string defaultValue)
	{
		string text = EncryptKey(key);
		if (!PlayerPrefs.HasKey(text) && PlayerPrefs.HasKey(key))
		{
			string text2 = PlayerPrefs.GetString(key, defaultValue);
			if (!PreservePlayerPrefs)
			{
				SetString(key, text2);
				PlayerPrefs.DeleteKey(key);
			}
			return text2;
		}
		string text3 = GetEncryptedPrefsString(key, text);
		return (!(text3 == "{not_found}")) ? DecryptStringValue(key, text3, defaultValue) : defaultValue;
	}

	public static string EncryptStringValue(string key, string value)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		return EncryptData(key, bytes, DataType.String);
	}

	public static string DecryptStringValue(string key, string encryptedValue, string defaultValue)
	{
		if (encryptedValue.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(encryptedValue);
			if (text == string.Empty)
			{
				return defaultValue;
			}
			SetString(key, text);
			return text;
		}
		byte[] array = DecryptData(key, encryptedValue);
		if (array == null)
		{
			return defaultValue;
		}
		return Encoding.UTF8.GetString(array, 0, array.Length);
	}

	public static void SetFloat(string key, float value)
	{
		PlayerPrefs.SetString(EncryptKey(key), EncryptFloatValue(key, value));
	}

	public static float GetFloat(string key)
	{
		return GetFloat(key, 0f);
	}

	public static float GetFloat(string key, float defaultValue)
	{
		string text = EncryptKey(key);
		if (!PlayerPrefs.HasKey(text) && PlayerPrefs.HasKey(key))
		{
			float num = PlayerPrefs.GetFloat(key, defaultValue);
			if (!PreservePlayerPrefs)
			{
				SetFloat(key, num);
				PlayerPrefs.DeleteKey(key);
			}
			return num;
		}
		string text2 = GetEncryptedPrefsString(key, text);
		return (!(text2 == "{not_found}")) ? DecryptFloatValue(key, text2, defaultValue) : defaultValue;
	}

	public static string EncryptFloatValue(string key, float value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		return EncryptData(key, bytes, DataType.Float);
	}

	public static float DecryptFloatValue(string key, string encryptedValue, float defaultValue)
	{
		if (encryptedValue.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(encryptedValue);
			if (text == string.Empty)
			{
				return defaultValue;
			}
			float result;
			float.TryParse(text, out result);
			SetFloat(key, result);
			return result;
		}
		byte[] array = DecryptData(key, encryptedValue);
		if (array == null)
		{
			return defaultValue;
		}
		return BitConverter.ToSingle(array, 0);
	}

	public static void SetDouble(string key, double value)
	{
		PlayerPrefs.SetString(EncryptKey(key), EncryptDoubleValue(key, value));
	}

	public static double GetDouble(string key)
	{
		return GetDouble(key, 0.0);
	}

	public static double GetDouble(string key, double defaultValue)
	{
		string text = GetEncryptedPrefsString(key, EncryptKey(key));
		return (!(text == "{not_found}")) ? DecryptDoubleValue(key, text, defaultValue) : defaultValue;
	}

	private static string EncryptDoubleValue(string key, double value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		return EncryptData(key, bytes, DataType.Double);
	}

	private static double DecryptDoubleValue(string key, string encryptedValue, double defaultValue)
	{
		if (encryptedValue.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(encryptedValue);
			if (text == string.Empty)
			{
				return defaultValue;
			}
			double result;
			double.TryParse(text, out result);
			SetDouble(key, result);
			return result;
		}
		byte[] array = DecryptData(key, encryptedValue);
		if (array == null)
		{
			return defaultValue;
		}
		return BitConverter.ToDouble(array, 0);
	}

	public static void SetLong(string key, long value)
	{
		PlayerPrefs.SetString(EncryptKey(key), EncryptLongValue(key, value));
	}

	public static long GetLong(string key)
	{
		return GetLong(key, 0L);
	}

	public static long GetLong(string key, long defaultValue)
	{
		string text = GetEncryptedPrefsString(key, EncryptKey(key));
		return (!(text == "{not_found}")) ? DecryptLongValue(key, text, defaultValue) : defaultValue;
	}

	private static string EncryptLongValue(string key, long value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		return EncryptData(key, bytes, DataType.Long);
	}

	private static long DecryptLongValue(string key, string encryptedValue, long defaultValue)
	{
		if (encryptedValue.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(encryptedValue);
			if (text == string.Empty)
			{
				return defaultValue;
			}
			long result;
			long.TryParse(text, out result);
			SetLong(key, result);
			return result;
		}
		byte[] array = DecryptData(key, encryptedValue);
		if (array == null)
		{
			return defaultValue;
		}
		return BitConverter.ToInt64(array, 0);
	}

	public static void SetBool(string key, bool value)
	{
		PlayerPrefs.SetString(EncryptKey(key), EncryptBoolValue(key, value));
	}

	public static bool GetBool(string key)
	{
		return GetBool(key, false);
	}

	public static bool GetBool(string key, bool defaultValue)
	{
		string text = GetEncryptedPrefsString(key, EncryptKey(key));
		return (!(text == "{not_found}")) ? DecryptBoolValue(key, text, defaultValue) : defaultValue;
	}

	private static string EncryptBoolValue(string key, bool value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		return EncryptData(key, bytes, DataType.Bool);
	}

	private static bool DecryptBoolValue(string key, string encryptedValue, bool defaultValue)
	{
		if (encryptedValue.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(encryptedValue);
			if (text == string.Empty)
			{
				return defaultValue;
			}
			int result;
			int.TryParse(text, out result);
			SetBool(key, result == 1);
			return result == 1;
		}
		byte[] array = DecryptData(key, encryptedValue);
		if (array == null)
		{
			return defaultValue;
		}
		return BitConverter.ToBoolean(array, 0);
	}

	public static void SetByteArray(string key, byte[] value)
	{
		PlayerPrefs.SetString(EncryptKey(key), EncryptByteArrayValue(key, value));
	}

	public static byte[] GetByteArray(string key)
	{
		return GetByteArray(key, 0, 0);
	}

	public static byte[] GetByteArray(string key, byte defaultValue, int defaultLength)
	{
		string text = GetEncryptedPrefsString(key, EncryptKey(key));
		if (text == "{not_found}")
		{
			return ConstructByteArray(defaultValue, defaultLength);
		}
		return DecryptByteArrayValue(key, text, defaultValue, defaultLength);
	}

	private static string EncryptByteArrayValue(string key, byte[] value)
	{
		return EncryptData(key, value, DataType.ByteArray);
	}

	private static byte[] DecryptByteArrayValue(string key, string encryptedValue, byte defaultValue, int defaultLength)
	{
		if (encryptedValue.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(encryptedValue);
			if (text == string.Empty)
			{
				return ConstructByteArray(defaultValue, defaultLength);
			}
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			SetByteArray(key, bytes);
			return bytes;
		}
		byte[] array = DecryptData(key, encryptedValue);
		if (array == null)
		{
			return ConstructByteArray(defaultValue, defaultLength);
		}
		return array;
	}

	private static byte[] ConstructByteArray(byte value, int arrayLength)
	{
		byte[] array = new byte[arrayLength];
		for (int i = 0; i < arrayLength; i++)
		{
			array[i] = value;
		}
		return array;
	}

	public static void SetVector2(string key, Vector2 value)
	{
		PlayerPrefs.SetString(EncryptKey(key), EncryptVector2Value(key, value));
	}

	public static Vector2 GetVector2(string key)
	{
		return GetVector2(key, Vector2.zero);
	}

	public static Vector2 GetVector2(string key, Vector2 defaultValue)
	{
		string text = GetEncryptedPrefsString(key, EncryptKey(key));
		return (!(text == "{not_found}")) ? DecryptVector2Value(key, text, defaultValue) : defaultValue;
	}

	private static string EncryptVector2Value(string key, Vector2 value)
	{
		byte[] array = new byte[8];
		Buffer.BlockCopy(BitConverter.GetBytes(value.x), 0, array, 0, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(value.y), 0, array, 4, 4);
		return EncryptData(key, array, DataType.Vector2);
	}

	private static Vector2 DecryptVector2Value(string key, string encryptedValue, Vector2 defaultValue)
	{
		if (encryptedValue.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(encryptedValue);
			if (text == string.Empty)
			{
				return defaultValue;
			}
			string[] array = text.Split("|"[0]);
			float result;
			float.TryParse(array[0], out result);
			float result2;
			float.TryParse(array[1], out result2);
			Vector2 vector = new Vector2(result, result2);
			SetVector2(key, vector);
			return vector;
		}
		byte[] array2 = DecryptData(key, encryptedValue);
		if (array2 == null)
		{
			return defaultValue;
		}
		Vector2 result3 = default(Vector2);
		result3.x = BitConverter.ToSingle(array2, 0);
		result3.y = BitConverter.ToSingle(array2, 4);
		return result3;
	}

	public static void SetVector3(string key, Vector3 value)
	{
		PlayerPrefs.SetString(EncryptKey(key), EncryptVector3Value(key, value));
	}

	public static Vector3 GetVector3(string key)
	{
		return GetVector3(key, Vector3.zero);
	}

	public static Vector3 GetVector3(string key, Vector3 defaultValue)
	{
		string text = GetEncryptedPrefsString(key, EncryptKey(key));
		return (!(text == "{not_found}")) ? DecryptVector3Value(key, text, defaultValue) : defaultValue;
	}

	private static string EncryptVector3Value(string key, Vector3 value)
	{
		byte[] array = new byte[12];
		Buffer.BlockCopy(BitConverter.GetBytes(value.x), 0, array, 0, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(value.y), 0, array, 4, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(value.z), 0, array, 8, 4);
		return EncryptData(key, array, DataType.Vector3);
	}

	private static Vector3 DecryptVector3Value(string key, string encryptedValue, Vector3 defaultValue)
	{
		if (encryptedValue.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(encryptedValue);
			if (text == string.Empty)
			{
				return defaultValue;
			}
			string[] array = text.Split("|"[0]);
			float result;
			float.TryParse(array[0], out result);
			float result2;
			float.TryParse(array[1], out result2);
			float result3;
			float.TryParse(array[2], out result3);
			Vector3 vector = new Vector3(result, result2, result3);
			SetVector3(key, vector);
			return vector;
		}
		byte[] array2 = DecryptData(key, encryptedValue);
		if (array2 == null)
		{
			return defaultValue;
		}
		Vector3 result4 = default(Vector3);
		result4.x = BitConverter.ToSingle(array2, 0);
		result4.y = BitConverter.ToSingle(array2, 4);
		result4.z = BitConverter.ToSingle(array2, 8);
		return result4;
	}

	public static void SetQuaternion(string key, Quaternion value)
	{
		PlayerPrefs.SetString(EncryptKey(key), EncryptQuaternionValue(key, value));
	}

	public static Quaternion GetQuaternion(string key)
	{
		return GetQuaternion(key, Quaternion.identity);
	}

	public static Quaternion GetQuaternion(string key, Quaternion defaultValue)
	{
		string text = GetEncryptedPrefsString(key, EncryptKey(key));
		return (!(text == "{not_found}")) ? DecryptQuaternionValue(key, text, defaultValue) : defaultValue;
	}

	private static string EncryptQuaternionValue(string key, Quaternion value)
	{
		byte[] array = new byte[16];
		Buffer.BlockCopy(BitConverter.GetBytes(value.x), 0, array, 0, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(value.y), 0, array, 4, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(value.z), 0, array, 8, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(value.w), 0, array, 12, 4);
		return EncryptData(key, array, DataType.Quaternion);
	}

	private static Quaternion DecryptQuaternionValue(string key, string encryptedValue, Quaternion defaultValue)
	{
		if (encryptedValue.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(encryptedValue);
			if (text == string.Empty)
			{
				return defaultValue;
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
			SetQuaternion(key, quaternion);
			return quaternion;
		}
		byte[] array2 = DecryptData(key, encryptedValue);
		if (array2 == null)
		{
			return defaultValue;
		}
		Quaternion result5 = default(Quaternion);
		result5.x = BitConverter.ToSingle(array2, 0);
		result5.y = BitConverter.ToSingle(array2, 4);
		result5.z = BitConverter.ToSingle(array2, 8);
		result5.w = BitConverter.ToSingle(array2, 12);
		return result5;
	}

	public static void SetColor(string key, Color32 value)
	{
		uint encodedColor = (uint)((value.a << 24) | (value.r << 16) | (value.g << 8) | value.b);
		PlayerPrefs.SetString(EncryptKey(key), EncryptColorValue(key, encodedColor));
	}

	public static Color32 GetColor(string key)
	{
		return GetColor(key, new Color32(0, 0, 0, 1));
	}

	public static Color32 GetColor(string key, Color32 defaultValue)
	{
		string text = GetEncryptedPrefsString(key, EncryptKey(key));
		if (text == "{not_found}")
		{
			return defaultValue;
		}
		uint num = DecryptUIntValue(key, text, 16777216u);
		byte a = (byte)(num >> 24);
		byte r = (byte)(num >> 16);
		byte g = (byte)(num >> 8);
		byte b = (byte)(num >> 0);
		return new Color32(r, g, b, a);
	}

	private static string EncryptColorValue(string key, uint value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		return EncryptData(key, bytes, DataType.Color);
	}

	public static void SetRect(string key, Rect value)
	{
		PlayerPrefs.SetString(EncryptKey(key), EncryptRectValue(key, value));
	}

	public static Rect GetRect(string key)
	{
		return GetRect(key, new Rect(0f, 0f, 0f, 0f));
	}

	public static Rect GetRect(string key, Rect defaultValue)
	{
		string text = GetEncryptedPrefsString(key, EncryptKey(key));
		return (!(text == "{not_found}")) ? DecryptRectValue(key, text, defaultValue) : defaultValue;
	}

	private static string EncryptRectValue(string key, Rect value)
	{
		byte[] array = new byte[16];
		Buffer.BlockCopy(BitConverter.GetBytes(value.x), 0, array, 0, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(value.y), 0, array, 4, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(value.width), 0, array, 8, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(value.height), 0, array, 12, 4);
		return EncryptData(key, array, DataType.Rect);
	}

	private static Rect DecryptRectValue(string key, string encryptedValue, Rect defaultValue)
	{
		if (encryptedValue.IndexOf(':') > -1)
		{
			string text = DeprecatedDecrypt(encryptedValue);
			if (text == string.Empty)
			{
				return defaultValue;
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
			SetRect(key, rect);
			return rect;
		}
		byte[] array2 = DecryptData(key, encryptedValue);
		if (array2 == null)
		{
			return defaultValue;
		}
		return new Rect
		{
			x = BitConverter.ToSingle(array2, 0),
			y = BitConverter.ToSingle(array2, 4),
			width = BitConverter.ToSingle(array2, 8),
			height = BitConverter.ToSingle(array2, 12)
		};
	}

	public static void SetRawValue(string key, string rawValue)
	{
		PlayerPrefs.SetString(EncryptKey(key), rawValue);
	}

	public static string GetRawValue(string prefKey)
	{
		string key = EncryptKey(prefKey);
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

	public static string EncryptKey(string key)
	{
		key = ObscuredString.EncryptDecrypt(key, cryptoKey);
		key = Convert.ToBase64String(Encoding.UTF8.GetBytes(key));
		return key;
	}

	public static bool HasKey(string key)
	{
		return PlayerPrefs.HasKey(key) || PlayerPrefs.HasKey(EncryptKey(key));
	}

	public static void DeleteKey(string key)
	{
		PlayerPrefs.DeleteKey(EncryptKey(key));
		if (!PreservePlayerPrefs)
		{
			PlayerPrefs.DeleteKey(key);
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

	private static string GetEncryptedPrefsString(string key, string encryptedKey)
	{
		string text = PlayerPrefs.GetString(encryptedKey, "{not_found}");
		if (text == "{not_found}" && PlayerPrefs.HasKey(key))
		{
			Debug.LogWarning("[ACTk] Are you trying to read regular PlayerPrefs data using ObscuredPrefs (key = " + key + ")?");
		}
		return text;
	}

	private static string EncryptData(string key, byte[] data, DataType dataType)
	{
		int num = data.Length;
		byte[] src = EncryptDecryptBytes(data, num, key + cryptoKey);
		uint num2 = xxHash.CalculateHash(data, num, 0u);
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
		array2[num3 - 7] = (byte)dataType;
		array2[num3 - 6] = 2;
		array2[num3 - 5] = (byte)LockToDevice;
		Buffer.BlockCopy(src2, 0, array2, num3 - 4, 4);
		return Convert.ToBase64String(array2);
	}

	public static byte[] DecryptData(string key, string encryptedValue)
	{
		byte[] array;
		try
		{
			array = Convert.FromBase64String(encryptedValue);
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
		DeviceLockLevel lockLevel = (DeviceLockLevel)array[num - 5];
		byte[] array2 = new byte[4];
		Buffer.BlockCopy(array, num - 4, array2, 0, 4);
		uint num2 = (uint)(array2[0] | (array2[1] << 8) | (array2[2] << 16) | (array2[3] << 24));
		uint num3 = 0u;
		int num4;
		if (lockLevel != DeviceLockLevel.None)
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
		byte[] array5 = EncryptDecryptBytes(array4, num4, key + cryptoKey);
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

	private static uint CalculateChecksum(string input)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(input + cryptoKey);
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

	private static byte[] EncryptDecryptBytes(byte[] data, int dataLength, string key)
	{
		int length = key.Length;
		byte[] array = new byte[dataLength];
		for (int i = 0; i < dataLength; i++)
		{
			array[i] = (byte)(data[i] ^ key[i % length]);
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
		string decodedValue = Encoding.UTF8.GetString(array2, 0, array2.Length);
		string result = ObscuredString.EncryptDecrypt(decodedValue, cryptoKey);
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

	private static string DeprecatedCalculateChecksum(string input)
	{
		int num = 0;
		byte[] bytes = Encoding.UTF8.GetBytes(input + cryptoKey);
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
