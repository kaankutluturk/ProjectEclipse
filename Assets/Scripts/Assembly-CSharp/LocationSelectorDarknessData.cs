using UnityEngine;

public class LocationSelectorDarknessData
{
	public Color color;

	public int darkeningEndFrame;

	public int darkEndFrame;

	public int lightingEndFrame;

	public int lightEndFrame;

	public LocationSelectorDarknessData(int darkeningEnd = 0, int darkEnd = 0, int lightingEnd = 0, int lightEnd = 0)
	{
		darkeningEndFrame = darkeningEnd;
		darkEndFrame = darkEnd;
		lightingEndFrame = lightingEnd;
		lightEndFrame = lightEnd;
	}
}
