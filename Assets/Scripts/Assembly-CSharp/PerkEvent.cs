using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

public class PerkEvent : PerkObject
{
	public enum PerkEventType
	{
		EVENT_NONE = 0,
		EVENT_ROUND_STAGE_START = 1,
		EVENT_EVERY_FRAME = 2,
		EVENT_STYLE = 3,
		EVENT_COMBO = 4,
		EVENT_HIT_PRECRIT = 5,
		EVENT_HIT_POSTCRIT = 6,
		EVENT_POST_HIT = 7,
		EVENT_MAGIC_CHARGED = 8,
		EVENT_ANIMATION_START = 9,
		EVENT_ANIMATION_END = 10,
		EVENT_MOD_EXPIRES = 11,
		EVENT_AREA_ENTER = 12,
		EVENT_AREA_EXIT = 13,
		EVENT_INTERVAL_END = 14
	}

	public class EventStruct
	{
		public PerkEventType Type;

		public object Info;

		public Model PerkOwnerModel;

		public Model EventModel;

		public string Namespace = string.Empty;
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private PerkEventType _type;

	public PerkEvent()
	{
	}

	public PerkEvent(PerkEvent source)
		: base(source)
	{
		set_Type(source.get_Type());
	}

	public PerkEventType get_Type()
	{
		return _type;
	}

	protected void set_Type(PerkEventType value)
	{
		_type = value;
	}

	public static List<PerkEvent> Create(XmlNode node, PerkInfoItem perk)
	{
		List<PerkEvent> list = new List<PerkEvent>();
		if (node != null)
		{
			foreach (XmlNode childNode in node.ChildNodes)
			{
				string name = childNode.Name;
				PerkEvent perkEvent = null;
				switch (name)
				{
				case "RoundStageStart":
					perkEvent = new PerkEventRoundStage();
					break;
				case "HitPreCrit":
					perkEvent = new PerkEventPostHit();
					break;
				case "HitPostCrit":
					perkEvent = new PerkEventPostHit();
					break;
				case "PostHit":
					perkEvent = new PerkEventPostHit();
					break;
				case "AnimationStart":
					perkEvent = new PerkEventAnimationStart();
					break;
				case "AnimationEnd":
					perkEvent = new PerkEventAnimationStart();
					break;
				case "ModExpires":
					perkEvent = new PerkEventModExpires();
					break;
				case "EveryFrame":
					perkEvent = new PerkEventEveryFrame();
					break;
				case "AreaEnter":
					perkEvent = new PerkEventAreaEnter();
					break;
				case "AreaExit":
					perkEvent = new PerkEventAreaEnter();
					break;
				case "MagicCharged":
					perkEvent = new PerkEventAreaEnter();
					break;
				case "IntervalEnd":
					perkEvent = new PerkEventIntervalEnd();
					break;
				default:
					perkEvent = new PerkEvent();
					break;
				}
				perkEvent.SetPerk(perk);
				perkEvent.Parse(childNode);
				if (perkEvent.get_Type() != PerkEventType.EVENT_NONE)
				{
					list.Add(perkEvent);
				}
			}
		}
		return list;
	}

	public static PerkEvent Clone(PerkEvent source, PerkInfoItem perk)
	{
		PerkEvent perkEvent = null;
		if (source != null)
		{
			switch (source.get_Type())
			{
			case PerkEventType.EVENT_ROUND_STAGE_START:
				perkEvent = new PerkEventRoundStage((PerkEventRoundStage)source);
				break;
			case PerkEventType.EVENT_HIT_PRECRIT:
				perkEvent = new PerkEventPostHit((PerkEventPostHit)source);
				break;
			case PerkEventType.EVENT_HIT_POSTCRIT:
				perkEvent = new PerkEventPostHit((PerkEventPostHit)source);
				break;
			case PerkEventType.EVENT_POST_HIT:
				perkEvent = new PerkEventPostHit((PerkEventPostHit)source);
				break;
			case PerkEventType.EVENT_ANIMATION_START:
				perkEvent = new PerkEventAnimationStart((PerkEventAnimationStart)source);
				break;
			case PerkEventType.EVENT_ANIMATION_END:
				perkEvent = new PerkEventAnimationStart((PerkEventAnimationStart)source);
				break;
			case PerkEventType.EVENT_MOD_EXPIRES:
				perkEvent = new PerkEventModExpires((PerkEventModExpires)source);
				break;
			case PerkEventType.EVENT_EVERY_FRAME:
				perkEvent = new PerkEventEveryFrame((PerkEventEveryFrame)source);
				break;
			case PerkEventType.EVENT_AREA_ENTER:
				perkEvent = new PerkEventAreaEnter((PerkEventAreaEnter)source);
				break;
			case PerkEventType.EVENT_AREA_EXIT:
				perkEvent = new PerkEventAreaEnter((PerkEventAreaEnter)source);
				break;
			case PerkEventType.EVENT_MAGIC_CHARGED:
				perkEvent = new PerkEventAreaEnter((PerkEventAreaEnter)source);
				break;
			case PerkEventType.EVENT_INTERVAL_END:
				perkEvent = new PerkEventIntervalEnd((PerkEventIntervalEnd)source);
				break;
			default:
				perkEvent = new PerkEvent(source);
				GameLog.Error("PerkEvent.Clone PerkEvent type is EventType.EVENT_NONE");
				break;
			}
			perkEvent.SetPerk(perk);
		}
		return perkEvent;
	}

	public virtual bool IsEqual(EventStruct eventData)
	{
		if (TargetPlayer == PlayerType.PLAYER_ME && eventData.PerkOwnerModel != eventData.EventModel)
		{
			return false;
		}
		if (TargetPlayer == PlayerType.PLAYER_ENEMY && eventData.PerkOwnerModel == eventData.EventModel)
		{
			return false;
		}
		return eventData.Type == get_Type();
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		switch (node.Name)
		{
		case "RoundStageStart":
			set_Type(PerkEventType.EVENT_ROUND_STAGE_START);
			break;
		case "EveryFrame":
			set_Type(PerkEventType.EVENT_EVERY_FRAME);
			break;
		case "Style":
			set_Type(PerkEventType.EVENT_STYLE);
			break;
		case "Combo":
			set_Type(PerkEventType.EVENT_COMBO);
			break;
		case "HitPreCrit":
			set_Type(PerkEventType.EVENT_HIT_PRECRIT);
			break;
		case "HitPostCrit":
			set_Type(PerkEventType.EVENT_HIT_POSTCRIT);
			break;
		case "PostHit":
			set_Type(PerkEventType.EVENT_POST_HIT);
			break;
		case "MagicCharged":
			set_Type(PerkEventType.EVENT_MAGIC_CHARGED);
			break;
		case "AnimationStart":
			set_Type(PerkEventType.EVENT_ANIMATION_START);
			break;
		case "AnimationEnd":
			set_Type(PerkEventType.EVENT_ANIMATION_END);
			break;
		case "ModExpires":
			set_Type(PerkEventType.EVENT_MOD_EXPIRES);
			break;
		case "AreaEnter":
			set_Type(PerkEventType.EVENT_AREA_ENTER);
			break;
		case "AreaExit":
			set_Type(PerkEventType.EVENT_AREA_EXIT);
			break;
		case "IntervalEnd":
			set_Type(PerkEventType.EVENT_INTERVAL_END);
			break;
		default:
			set_Type(PerkEventType.EVENT_NONE);
			break;
		}
	}
}

// Newer enchantments can proc when a named/type animation interval ends.
// The old fight runtime already emits this information, but its perk layer did
// not expose a corresponding event.
public class PerkEventIntervalEnd : PerkEvent
{
	private string _name = string.Empty;
	private IntervalAnimation.IntervalType _intervalType = IntervalAnimation.IntervalType.INTERVAL_NONE;

	public PerkEventIntervalEnd()
	{
	}

	public PerkEventIntervalEnd(PerkEventIntervalEnd other) : base(other)
	{
		_name = other._name;
		_intervalType = other._intervalType;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		_intervalType = IntervalAnimation.ParseIntervalType(node.Attributes["Type"].GetStringOrDefault(string.Empty));
	}

	public override bool IsEqual(EventStruct eventInfo)
	{
		if (!base.IsEqual(eventInfo) || eventInfo == null || eventInfo.Info == null)
			return false;
		Dictionary<string, object> info = (Dictionary<string, object>)eventInfo.Info;
		IntervalAnimation interval = info.ContainsKey("Interval") ? info["Interval"] as IntervalAnimation : null;
		if (interval == null)
			return false;
		return (_name == string.Empty || interval.Name == _name) &&
			(_intervalType == IntervalAnimation.IntervalType.INTERVAL_NONE || interval.Type == _intervalType);
	}
}
