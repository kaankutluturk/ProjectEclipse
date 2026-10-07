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

	protected override bool Compare(EventAnimation FOPOKALJIIJ)
	{
		bool flag = false;
		List<PerksStage.ActionPerk> fPFKABHOEHP = FOPOKALJIIJ.Conditions.SelfExpiredPerks;
		for (int i = 0; i < fPFKABHOEHP.Count; i++)
		{
			if (fPFKABHOEHP[i].GetModName() == modName)
			{
				flag = true;
			}
		}
		return (!IsNot) ? flag : (!flag);
	}

	protected override void Parse(XmlNode MEEAKLDGLDF)
	{
		modName = MEEAKLDGLDF.Attributes["Name"].GetStringOrDefault(string.Empty);
	}
}
