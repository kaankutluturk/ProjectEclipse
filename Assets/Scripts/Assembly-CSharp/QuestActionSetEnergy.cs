using System;
using System.Xml;

public class QuestActionSetEnergy : QuestAction
{
	private int _value;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_value = node.Attributes["Value"].ParseInt();
		if (_value < 0)
		{
			GameLog.Error("QuestActionSetEnergy::parse - wrong value: %i, setting to 0", _value);
			_value = 0;
		}
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		Roster roster = ListSF.GetRoster();
		int maxPower = roster.PowerMax;
		roster.SetPower(Math.Min(_value, maxPower));
		FinishAction();
	}
}
