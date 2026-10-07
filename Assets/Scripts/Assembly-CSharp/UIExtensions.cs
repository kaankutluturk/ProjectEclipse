using UnityEngine;
using UnityEngine.UI;

public static class UIExtensions
{
	public static void SetAlpha(this Graphic graphic, float alpha)
	{
		Color color = graphic.color;
		color.a = alpha;
		graphic.color = color;
	}

	public static float GetAlpha(this Graphic graphic)
	{
		return graphic.color.a;
	}
}
