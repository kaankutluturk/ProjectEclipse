using System.Xml;

public class ComboRule : DamageRule
{
	private int _comboLevel;

	public ComboRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(node, EJPOJJKKICO, RuleType.RuleCombo)
	{
		_comboLevel = 0;
		applianceLosesOnTrigger = false;
		Parse(node);
		SubscribeEvent(FightEvent.ComboEvent);
	}

	protected override bool CompareSingle(object data)
	{
		FightData hCPJJKMNMCE = (FightData)data;
		if (hCPJJKMNMCE.FightEventType == FightEvent.DamageCheckEvent)
		{
			return false;
		}
		return CheckIsNoDamageChange(hCPJJKMNMCE.currentComboLevel < _comboLevel);
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		_comboLevel = node.Attributes["Value"].ParseInt();
	}
}
