using System.Xml;

public class EventAnimation
{
	public enum EventAnimationType
	{
		EVENT_NONE = 0,
		EVENT_ROUND_STAGE = 1,
		EVENT_KEY_PRESSED = 2,
		EVENT_KEY_RELEASED = 3,
		EVENT_ROUND_START = 4,
		EVENT_ROUND_END = 5,
		EVENT_HIT = 6,
		EVENT_STRIKE = 7,
		EVENT_WALL_HIT = 8,
		EVENT_ANIMATION_START = 9,
		EVENT_ANIMATION_END = 10,
		EVENT_INTERVAL_START = 11,
		EVENT_INTERVAL_END = 12,
		EVENT_EVERY_FRAME = 13,
		EVENT_BIRTH = 14,
		EVENT_MOD_EXPIRES = 15
	}

	public ModelConditions Conditions;

	public EventAnimationType Type;

	public string AnimationName;

	public string HitType;

	public string StageName;

	public bool IsNot;

	public ModelType.ModelTargetType TargetModel;

	public EventAnimation(EventAnimationType LFLGCDNKNJI = EventAnimationType.EVENT_NONE)
	{
		Type = LFLGCDNKNJI;
		Conditions = null;
	}

	public static string GetTypeName(EventAnimationType LFLGCDNKNJI)
	{
		switch (LFLGCDNKNJI)
		{
		case EventAnimationType.EVENT_NONE:
			return "None";
		case EventAnimationType.EVENT_ROUND_STAGE:
			return "RoundStage";
		case EventAnimationType.EVENT_KEY_PRESSED:
			return "KeyPressed";
		case EventAnimationType.EVENT_KEY_RELEASED:
			return "KeyReleased";
		case EventAnimationType.EVENT_ROUND_START:
			return "RoundStart";
		case EventAnimationType.EVENT_ROUND_END:
			return "RoundEnd";
		case EventAnimationType.EVENT_HIT:
			return "Hit";
		case EventAnimationType.EVENT_STRIKE:
			return "Strike";
		case EventAnimationType.EVENT_WALL_HIT:
			return "WallHit";
		case EventAnimationType.EVENT_ANIMATION_START:
			return "AnimationStart";
		case EventAnimationType.EVENT_ANIMATION_END:
			return "AnimationEnd";
		case EventAnimationType.EVENT_INTERVAL_START:
			return "IntervalStart";
		case EventAnimationType.EVENT_INTERVAL_END:
			return "IntervalEnd";
		case EventAnimationType.EVENT_EVERY_FRAME:
			return "EveryFrame";
		case EventAnimationType.EVENT_BIRTH:
			return "Birth";
		case EventAnimationType.EVENT_MOD_EXPIRES:
			return "ModExpires";
		default:
			return "None";
		}
	}

	public static EventAnimationType GetTypeByName(string name)
	{
		switch (name)
		{
		case "None":
		case "":
		case null:
			return EventAnimationType.EVENT_NONE;
		case "RoundStage":
			return EventAnimationType.EVENT_ROUND_STAGE;
		case "KeyPressed":
			return EventAnimationType.EVENT_KEY_PRESSED;
		case "RoundStart":
			return EventAnimationType.EVENT_ROUND_START;
		case "RoundEnd":
			return EventAnimationType.EVENT_ROUND_END;
		case "Hit":
			return EventAnimationType.EVENT_HIT;
		case "Strike":
			return EventAnimationType.EVENT_STRIKE;
		case "WallHit":
			return EventAnimationType.EVENT_WALL_HIT;
		case "AnimationStart":
			return EventAnimationType.EVENT_ANIMATION_START;
		case "AnimationEnd":
			return EventAnimationType.EVENT_ANIMATION_END;
		case "IntervalStart":
			return EventAnimationType.EVENT_INTERVAL_START;
		case "IntervalEnd":
			return EventAnimationType.EVENT_INTERVAL_END;
		case "EveryFrame":
			return EventAnimationType.EVENT_EVERY_FRAME;
		case "Birth":
			return EventAnimationType.EVENT_BIRTH;
		case "KeyReleased":
			return EventAnimationType.EVENT_KEY_RELEASED;
		case "ModExpires":
			return EventAnimationType.EVENT_MOD_EXPIRES;
		default:
			GameLog.Error("getEventTypeByName - unknown type: \"{0}\"", name);
			return EventAnimationType.EVENT_NONE;
		}
	}

	public bool IsEqual(EventAnimation JHJEPJJOCAE)
	{
		if (Type == JHJEPJJOCAE.Type)
		{
			return Compare(JHJEPJJOCAE);
		}
		return false;
	}

	public void Init(XmlNode EIGDDPDGIAN)
	{
		AnimationName = EIGDDPDGIAN.Attributes["Name"].GetStringOrDefault(string.Empty);
		HitType = EIGDDPDGIAN.Attributes["Type"].GetStringOrDefault(string.Empty);
		StageName = EIGDDPDGIAN.Attributes["Stage"].GetStringOrDefault(string.Empty);
		IsNot = EIGDDPDGIAN.Attributes["Not"].ParseBool();
		TargetModel = ModelType.ParseTargetType(EIGDDPDGIAN.Attributes["Player"].GetStringOrDefault("Me"));
		Parse(EIGDDPDGIAN);
	}

	protected virtual bool Compare(EventAnimation FOPOKALJIIJ)
	{
		if (Type == EventAnimationType.EVENT_HIT)
		{
			bool flag = false;
			if (!string.IsNullOrEmpty(AnimationName) && (!(AnimationName == FOPOKALJIIJ.AnimationName) || 1 == 0))
			{
				return false;
			}
			if (string.IsNullOrEmpty(HitType))
			{
				return true;
			}
			string[] array = HitType.Split('|');
			for (int i = 0; i < array.Length; i++)
			{
				if (HitType == array[i])
				{
					return true;
				}
			}
		}
		return false;
	}

	protected virtual void Parse(XmlNode MEEAKLDGLDF)
	{
	}
}
