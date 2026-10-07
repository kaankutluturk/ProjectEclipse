using UnityEngine;

public static class TransformExtensions
{
	public static void SetLocalX(this Transform KGOIHPPNFGC, float value)
	{
		KGOIHPPNFGC.localPosition = new Vector3(value, KGOIHPPNFGC.localPosition.y, KGOIHPPNFGC.localPosition.z);
	}

	public static void SetLocalY(this Transform KGOIHPPNFGC, float value)
	{
		KGOIHPPNFGC.localPosition = new Vector3(KGOIHPPNFGC.localPosition.x, value, KGOIHPPNFGC.localPosition.z);
	}

	public static void SetLocalZ(this Transform KGOIHPPNFGC, float value)
	{
		KGOIHPPNFGC.localPosition = new Vector3(KGOIHPPNFGC.localPosition.x, KGOIHPPNFGC.localPosition.y, value);
	}
}
