using System.Xml;

public class RaidCurrencyCostRule : CurrencyCostRule
{
	protected string packName;

	public RaidCurrencyCostRule(XmlNode node)
		: base(node)
	{
		_type = RuleType.RuleRaidCurrencyCost;
		Parse(node);
	}

	public RaidCurrencyCostRule(RaidCurrencyCostRule HNBFMAKFJAM)
		: base(HNBFMAKFJAM)
	{
		packName = HNBFMAKFJAM.GetPackName();
	}

	public string GetPackName()
	{
		return packName;
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		packName = node.Attributes["PackName"].GetStringOrDefault(string.Empty);
	}
}
