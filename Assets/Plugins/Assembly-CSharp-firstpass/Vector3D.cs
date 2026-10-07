using System.Globalization;
using UnityEngine;

public class Vector3D
{
	public static Vector3 Lerp(Vector3 from, Vector3 to, float ratio)
	{
		return new Vector3(from.x + (to.x - from.x) * ratio, from.y + (to.y - from.y) * ratio, from.z + (to.z - from.z) * ratio);
	}

	public static Vector3 Parse(string text)
	{
		string[] array = text.Split(' ');
		if (array.Length < 3)
		{
			AdvLog.LogError("Wrong Vector3 string!!! - " + text);
			return Vector3.zero;
		}
		return new Vector3(float.Parse(array[0], CultureInfo.InvariantCulture), float.Parse(array[1], CultureInfo.InvariantCulture), float.Parse(array[2], CultureInfo.InvariantCulture));
	}
}
