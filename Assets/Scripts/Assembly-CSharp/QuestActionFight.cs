using System.Xml;

public class QuestActionFight : QuestAction
{
	private string _name = string.Empty;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ListSF.GetInstance().ClearQuestsStack();
		if (string.IsNullOrEmpty(_name))
		{
			// The newer quest graph deliberately uses <Fight /> in the error branch of
			// ScriptsResumeOnStart.  It means "there was no interrupted fight to
			// resume, return to the normal game screen", rather than a malformed fight.
			Module.OpenScreen(ScreenType.ModuleDojo);
			FinishAction();
			return;
		}
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(_name, result);
		string fightIdsText = result.ToString();
		FightIDS fightIds = new FightIDS();
		fightIds.SetFightIDSByString(fightIdsText);
		FightList fight = ListSF.GetFightById(fightIds);
		if (fight != null)
		{
			GameUtils.StartFight(fight);
		}
		else
		{
			Module.OpenScreen(ScreenType.ModuleDojo);
		}
		FinishAction();
	}
}
