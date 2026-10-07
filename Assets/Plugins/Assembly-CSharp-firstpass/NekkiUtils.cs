using System.Globalization;
using UnityEngine;

public class NekkiUtils
{
	public enum DeviceKind
	{
		NONE = 0,
		UNKNOWN = 1,
		PHONE = 2,
		TABLET = 3,
		CONSOLE = 4,
		DESKTOP = 5
	}

	private static DeviceKind _deviceKind;

	public static Vector2 GetVector2FromString(string text, char separator)
	{
		text = text.Trim();
		string[] array = text.Split(separator);
		return new Vector2(float.Parse(array[0]), float.Parse(array[1]));
	}

	public static Vector3 GetVector3FromString(string text, char separator)
	{
		text = text.Trim();
		string[] array = text.Split(separator);
		return new Vector3(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]));
	}

	public static Vector4 GetVector4FromString(string text, char separator)
	{
		text = text.Trim();
		string[] array = text.Split(separator);
		return new Vector4(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
	}

	public static Matrix4x4 GetMatrixFromStringColumnMajor(string text, char separator)
	{
		text = text.Trim();
		string[] array = text.Split(separator);
		Matrix4x4 result = default(Matrix4x4);
		short num = 0;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				result[j, i] = float.Parse(array[num]);
				num++;
			}
		}
		return result;
	}

	public static Matrix4x4 GetMatrixFromStringRowMajor(string text, char separator)
	{
		text = text.Trim();
		string[] array = text.Split(separator);
		Matrix4x4 result = default(Matrix4x4);
		short num = 0;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				result[i, j] = float.Parse(array[num]);
				num++;
			}
		}
		return result;
	}

	public static string ReadSeparatedTokens(string text, char separator, int startIndex, int count, out int endIndex)
	{
		text = text.Trim();
		int num = 0;
		int num2 = startIndex;
		while (num < count && num2 < text.Length)
		{
			if (text[num2] == separator)
			{
				num++;
			}
			num2++;
		}
		endIndex = num2;
		if (num == count)
		{
			return text.Substring(startIndex, num2 - startIndex);
		}
		return text.Substring(startIndex);
	}

	public static Quaternion GetQuaternionFromMatrix(Matrix4x4 matrix)
	{
		return Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
	}

	public static string ColorToHex(Color32 color)
	{
		return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
	}

	public static Color HexToColor(string hex)
	{
		byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
		return new Color32(r, g, b, byte.MaxValue);
	}

	public static Color IntToColor(int color)
	{
		return new Color
		{
			r = (float)((color & 0xFF0000) >> 16) / 255f,
			g = (float)((color & 0xFF00) >> 8) / 255f,
			b = (float)(color & 0xFF) / 255f,
			a = 1f
		};
	}

	public static int ColorToInt(Color color)
	{
		return ((int)(color.r * 255f) << 16) + ((int)(color.g * 255f) << 8) + (int)(color.b * 255f);
	}

	public static void DetectDeviceKind()
	{
		if (SystemInfo.deviceType != DeviceType.Handheld)
		{
			switch (SystemInfo.deviceType)
			{
			case DeviceType.Unknown:
				_deviceKind = DeviceKind.UNKNOWN;
				break;
			case DeviceType.Console:
				_deviceKind = DeviceKind.CONSOLE;
				break;
			case DeviceType.Desktop:
				_deviceKind = DeviceKind.DESKTOP;
				break;
			}
			return;
		}
		float num = ((Screen.width <= Screen.height) ? ((float)Screen.height) : ((float)Screen.width));
		if (num < 800f)
		{
			_deviceKind = DeviceKind.PHONE;
		}
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			float f = (float)Screen.width / Screen.dpi;
			float f2 = (float)Screen.height / Screen.dpi;
			float num2 = Mathf.Sqrt(Mathf.Pow(f, 2f) + Mathf.Pow(f2, 2f));
			if (num2 >= 6.5f)
			{
				_deviceKind = DeviceKind.TABLET;
			}
		}
		_deviceKind = DeviceKind.PHONE;
	}

	public static DeviceKind GetDeviceKind()
	{
		if (_deviceKind == DeviceKind.NONE)
		{
			DetectDeviceKind();
		}
		return _deviceKind;
	}

	public static bool IsTablet()
	{
		return GetDeviceKind() == DeviceKind.TABLET;
	}

	public static bool IsPhone()
	{
		return GetDeviceKind() == DeviceKind.PHONE;
	}

	public static bool IsEditorNotPlaying()
	{
		return Application.isEditor && !Application.isPlaying;
	}

	public static string GetPlatformName()
	{
		switch (Application.platform)
		{
		case RuntimePlatform.OSXEditor:
			return "standaloneMacOSX";
		case RuntimePlatform.OSXPlayer:
			return "standaloneMacOSX";
		case RuntimePlatform.WindowsPlayer:
			return "standaloneWindows";
		case RuntimePlatform.WindowsEditor:
			return "standaloneWindows";
		case RuntimePlatform.IPhonePlayer:
			return "ios";
		case RuntimePlatform.Android:
			return "android";
		default:
			return string.Empty;
		}
	}
}
