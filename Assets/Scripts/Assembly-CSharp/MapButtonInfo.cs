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

	public MapButtonInfo(string _name, string NCKCDCODNHA, string KMFDBBKMLOO, Vector2 LGDMCAAHPOC, bool _AutoPosition = false, string Atlas = "", string IOKOBBFCIGE = "IMAGE", float AMEGCDJDGPB = 0f, float JDDJEAGMNMP = 0f, string BFBFKHHANJG = "Story", float anchorMinX = 0.5f, float anchorMaxX = 0.5f)
	{
		Name = _name;
		ImageName = NCKCDCODNHA;
		Timer = KMFDBBKMLOO;
		AtlasName = Atlas;
		TypeName = IOKOBBFCIGE;
		Position = LGDMCAAHPOC;
		AutoPosition = _AutoPosition;
		Speed = AMEGCDJDGPB;
		Pause = JDDJEAGMNMP;
		ShowTypeName = BFBFKHHANJG;
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
