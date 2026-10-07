using UnityEngine;
using UnityEngine.UI;

public static class ImageAlphaExtensions
{
	public static void SetImageAlpha(this Image KEFKMDAKBFF, float HJPDNAPBEMF)
	{
		if (KEFKMDAKBFF != null)
		{
			Color color = KEFKMDAKBFF.color;
			color.a = HJPDNAPBEMF;
			KEFKMDAKBFF.color = color;
		}
	}
}
