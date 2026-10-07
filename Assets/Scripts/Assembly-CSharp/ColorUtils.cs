using System;
using UnityEngine;

public static class ColorUtils
{
	public static Color ParseHexColor(string hex, float alpha = 1f)
	{
		hex = hex.Replace("#", string.Empty);
		hex = hex.Replace("0x", string.Empty);
		if (hex.Length != 6 && hex.Length != 8)
		{
			return new Color(0f, 0f, 0f, 1f);
		}
		int num = hex.Length / 2;
		byte[] array = new byte[4]
		{
			0,
			0,
			0,
			(byte)(255f * alpha)
		};
		for (int i = 0; i < num; i++)
		{
			array[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
		}
		return new Color((float)(int)array[0] / 255f, (float)(int)array[1] / 255f, (float)(int)array[2] / 255f, (float)(int)array[3] / 255f);
	}
}
