using UnityEngine;

public class MapButtonInfo
{
	public enum MapButtonType
	{
		IMAGE = 0,
		SEQUENCE = 1
	}

	public enum MapButtonShowType
	{
		Both = 0,
		Story = 1,
		Raid = 2
	}

	public string Name = string.Empty;

	public string ImageName = string.Empty;

	public string Timer = string.Empty;

	public string AtlasName = string.Empty;

	public string TypeName = string.Empty;

	public Vector2 Position = default(Vector2);

	// Newer gamedata positions some buttons relative to a canvas edge.  The
	// decompiled runtime had lost these XML fields and treated every position as
	// canvas-centred, which moved the Eclipse switch far to the left.
	public float AnchorMinX = 0.5f;

	public float AnchorMaxX = 0.5f;

	public bool AutoPosition;

	public float Speed;

	public float Pause;

	public string ShowTypeName = string.Empty;

	public MapButtonInfo()
	{
	}

	public MapButtonInfo(string _name, string imageName, string timer, Vector2 buttonPosition, bool _AutoPosition = false, string Atlas = "", string typeName = "IMAGE", float speed = 0f, float pause = 0f, string showTypeName = "Story", float anchorMinX = 0.5f, float anchorMaxX = 0.5f)
	{
		Name = _name;
		ImageName = imageName;
		Timer = timer;
		AtlasName = Atlas;
		TypeName = typeName;
		Position = buttonPosition;
		AutoPosition = _AutoPosition;
		Speed = speed;
		Pause = pause;
		ShowTypeName = showTypeName;
		AnchorMinX = anchorMinX;
		AnchorMaxX = anchorMaxX;
	}

	public MapButtonType GetButtonType()
	{
		MapButtonType result = MapButtonType.IMAGE;
		if (TypeName == "Sequence")
		{
			result = MapButtonType.SEQUENCE;
		}
		return result;
	}

	public MapButtonShowType GetShowType()
	{
		MapButtonShowType result = MapButtonShowType.Both;
		if (ShowTypeName == "Story")
		{
			result = MapButtonShowType.Story;
		}
		if (ShowTypeName == "Raid")
		{
			result = MapButtonShowType.Raid;
		}
		return result;
	}
}
