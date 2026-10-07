using UnityEngine;

public static class TransformExtensions
{
	public static void SetLocalX(this Transform transform, float value)
	{
		transform.localPosition = new Vector3(value, transform.localPosition.y, transform.localPosition.z);
	}

	public static void SetLocalY(this Transform transform, float value)
	{
		transform.localPosition = new Vector3(transform.localPosition.x, value, transform.localPosition.z);
	}

	public static void SetLocalZ(this Transform transform, float value)
	{
		transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, value);
	}
}
