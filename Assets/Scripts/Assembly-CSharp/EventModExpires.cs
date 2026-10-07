using System.Collections.Generic;
using System.Xml;

public class EventModExpires : EventAnimation
{
	private string modName;

	public string ModName
	{
		get
		{
			return GetModName();
		}
	}

	public EventModExpires()
		: base(EventAnimationType.EVENT_MOD_EXPIRES)
	{
	}

	public string GetModName()
	{
		return modName;
	}

	protected override bool Compare(EventAnimation other)
	{
		bool flag = false;
		List<PerksStage.ActionPerk> expiredPerks = other.Conditions.SelfExpiredPerks;
		for (int i = 0; i < expiredPerks.Count; i++)
		{
			if (expiredPerks[i].GetModName() == modName)
			{
				flag = true;
			}
		}
		return (!IsNot) ? flag : (!flag);
	}

	protected override void Parse(XmlNode node)
	{
		modName = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}
}
