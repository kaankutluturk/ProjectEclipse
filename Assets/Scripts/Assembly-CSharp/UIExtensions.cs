using UnityEngine;
using UnityEngine.UI;

public static class UIExtensions
{
	public static void SetAlpha(this Graphic AJDDOEIAJKB, float KGJALFLDIBG)
	{
		Color color = AJDDOEIAJKB.color;
		color.a = KGJALFLDIBG;
		AJDDOEIAJKB.color = color;
	}

	public static float GetAlpha(this Graphic AJDDOEIAJKB)
	{
		return AJDDOEIAJKB.color.a;
	}
}
