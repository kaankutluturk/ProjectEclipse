using System.Xml;

public class PerkRule : InFightRule
{
	private PerkInfoItem perk;

	public PerkRule(XmlNode node, RuleAppliance appliance)
		: base(RuleType.RulePerk, appliance, node)
	{
		perk = null;
		Parse(node);
	}

	public PerkInfoItem GetPerk()
	{
		return perk;
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		string perkName = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		perk = GameUtils.PerkItemList.FindBasePerk(perkName);
		if (perk != null)
		{
			perk = perk.Clone(node["Set"], node["RatingEvaluation"]);
		}
	}

	public override InFightRule Copy()
	{
		InFightRule copy = null;
		RuleAppliance appliance = GetAppliance();
		XmlNode ruleNode = GetXmlSource().GetNode();
		copy = new PerkRule(ruleNode, appliance);
		copy.IsRandom = IsRandom;
		return copy;
	}
}
