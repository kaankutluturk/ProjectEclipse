using System.Xml;

public class QuestActionChangePlayerAvatar : QuestAction
{
	private string avatarExpression;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		avatarExpression = node.Attributes["Avatar"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		string avatarName = string.Empty;
		GetValues(ref avatarName);
		if (GameUtils.AvatarExists(avatarName))
		{
			ListSF.GetRoster().SetAvatar(avatarName);
		}
		FinishAction();
	}

	private void GetValues(ref string avatarName)
	{
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(Parameters);
		condition.SetValue(avatarExpression, result);
		avatarName = result.ToString();
	}
}
