using System.Xml;

public class IntervalAnimation
{
	public enum IntervalType
	{
		INTERVAL_NONE = 0,
		INTERVAL_UNSTABLE = 1,
		INTERVAL_UNINTERRUPT = 2,
		INTERVAL_SELF_UNINTERRUPT = 3,
		INTERVAL_ATTACK = 4,
		INTERVAL_BLOCK = 5,
		INTERVAL_INVULNERABLE = 6,
		INTERVAL_INVISIBLE = 7
	}

	private int _Id;

	private int animationFinishFrame;

	public int Start;

	public int EndFrameValue;

	// best guess for name
	public int EndFrame { get => EndFrameValue; set => EndFrameValue = value; }


	public string Name;

	public IntervalType Type;

	public XmlNode NodeInterval;

	// Eclipse: authored identity kept after lazy parsing, so guarded mod patches can
	// select an already-parsed interval exactly ("Throwable" types and open ends
	// are otherwise indistinguishable once parsed).
	public string AuthoredType = string.Empty;

	public bool HasAuthoredEnd = true;

	public int AnimationId
	{
		get
		{
			return GetAnimationId();
		}
	}

	public int FinishFrame
	{
		set
		{
			set_AnimationFinishFrame(value);
		}
	}

	public IntervalAnimation(IntervalType intervalType)
	{
		_Id = -1;
		animationFinishFrame = int.MaxValue;
		Type = intervalType;
	}

	public int GetAnimationId()
	{
		return _Id;
	}

	public void set_AnimationFinishFrame(int value)
	{
		animationFinishFrame = value;
	}

	public virtual void Parse(XmlNode node)
	{
		NodeInterval = node;
		_Id = XmlUtils.ParseInt(node.Attributes["ID"], -1);
		AuthoredType = node.Attributes["Type"]?.Value ?? string.Empty;
	}

	public virtual void Init()
	{
		Name = XmlUtils.ParseString(NodeInterval.Attributes["Name"]);
		if (Name == "Unstable")
		{
			Type = IntervalType.INTERVAL_UNSTABLE;
		}
		else if (Name == "Uninterrupt")
		{
			Type = IntervalType.INTERVAL_UNINTERRUPT;
		}
		else if (Name == "SelfUninterrupt")
		{
			Type = IntervalType.INTERVAL_SELF_UNINTERRUPT;
		}
		Start = XmlUtils.ParseInt(NodeInterval.Attributes["Start"]);
		bool flag = NodeInterval.Attributes["End"] != null;
		HasAuthoredEnd = flag;
		EndFrameValue = ((!flag) ? (animationFinishFrame + 2) : XmlUtils.ParseInt(NodeInterval.Attributes["End"], int.MaxValue));
		if (Start > EndFrameValue)
		{
			// Newer templates can override Start while inheriting an older End.
			// The modern merger treats that as an open-ended interval; this legacy
			// merger leaves the stale End behind. Preserve a valid one-frame window.
			EndFrameValue = Start;
		}
		ParseInside();
		NodeInterval = null;
	}

	public static IntervalType ParseIntervalType(string typeName)
	{
		switch (typeName)
		{
		case "Attack":
			return IntervalType.INTERVAL_ATTACK;
		case "Block":
			return IntervalType.INTERVAL_BLOCK;
		case "Invulnerable":
			return IntervalType.INTERVAL_INVULNERABLE;
		case "Invisible":
			return IntervalType.INTERVAL_INVISIBLE;
		default:
			return IntervalType.INTERVAL_NONE;
		}
	}

	private static string IntervalTypeToString(IntervalType intervalType)
	{
		switch (intervalType)
		{
		case IntervalType.INTERVAL_NONE:
			return string.Empty;
		case IntervalType.INTERVAL_UNSTABLE:
			return "Unstable";
		case IntervalType.INTERVAL_UNINTERRUPT:
			return "Uninterrupt";
		case IntervalType.INTERVAL_SELF_UNINTERRUPT:
			return "SelfUninterrupt";
		case IntervalType.INTERVAL_ATTACK:
			return "Attack";
		case IntervalType.INTERVAL_BLOCK:
			return "Block";
		case IntervalType.INTERVAL_INVULNERABLE:
			return "Invulnerable";
		case IntervalType.INTERVAL_INVISIBLE:
			return "Invisible";
		default:
			return string.Empty;
		}
	}

	protected virtual void ParseInside()
	{
	}
}
