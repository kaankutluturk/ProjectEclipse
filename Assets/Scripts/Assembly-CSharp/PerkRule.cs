using System.Xml;

public class PerkRule : InFightRule
{
	private PerkInfoItem perk;

	public PerkRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(RuleType.RulePerk, EJPOJJKKICO, node)
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
		string gOHIIMFFFJI = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		perk = GameUtils.PerkItemList.FindBasePerk(gOHIIMFFFJI);
		if (perk != null)
		{
			perk = perk.Clone(node["Set"], node["RatingEvaluation"]);
		}
	}

	public override InFightRule Copy()
	{
		InFightRule aAJIFBJLJOA = null;
		RuleAppliance eJPOJJKKICO = GetAppliance();
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new PerkRule(hKPPBKPJOEO, eJPOJJKKICO);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
