using UnityEngine;
using UnityEngine.UI;

public static class ImageAlphaExtensions
{
	public static void SetImageAlpha(this Image image, float alpha)
	{
		if (image != null)
		{
			Color color = image.color;
			color.a = alpha;
			image.color = color;
		}
	}
}
