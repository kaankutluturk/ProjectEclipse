using System.Xml;

public class QuestActionSetParameter : QuestAction
{
	private class SetParameterOperands
	{
		public ConditionExtension.CompareResult itemNameResult;

		public ConditionExtension.CompareResult parameterNameResult;

		public ConditionExtension.CompareResult valueResult;
	}

	private string name;

	private string parameterExpression;

	private string value;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		parameterExpression = node.Attributes["Parameter"].GetStringOrDefault(string.Empty);
		value = node.Attributes["Value"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		SetParameterOperands operands = new SetParameterOperands();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(name, operands.itemNameResult);
		condition.SetValue(parameterExpression, operands.parameterNameResult);
		condition.SetValue(value, operands.valueResult);
		ApplyParameter(operands);
		FinishAction();
	}

	private void ApplyParameter(SetParameterOperands operands)
	{
		Roster roster = ListSF.GetRoster();
		UserItem userItem = roster.GetInventory().FindItem(operands.itemNameResult.resultSTR);
		if (userItem != null)
		{
			string parameterName = operands.parameterNameResult.resultSTR;
			if (parameterName.Equals("UpgradeLevel"))
			{
				userItem.SetUpgradeLevel((int)operands.valueResult.resultNumber);
				userItem.RefreshUpgradeState(roster.GetLevel());
			}
		}
	}
}
