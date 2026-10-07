using System.Collections.Generic;
using System.Xml;

public class QuestActionResumeQuests : QuestAction
{
	private QuestActionsSequence successSequence = new QuestActionsSequence();

	private QuestActionsSequence errorSequence = new QuestActionsSequence();

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		XmlNode successNode = node["Success"];
		XmlNode sequenceNode = node["Error"];
		ParseSequenceWithUnlock(successNode, successSequence, OnSuccessComplete);
		ParseSequenceWithUnlock(sequenceNode, errorSequence, OnActionComplete);
	}

	public override void Execute(QuestParameters parameters)
	{
		ResetSequences();
		base.Execute(parameters);
		Roster roster = ListSF.GetRoster();
		int num = 0;
		List<RosterQuest> list = roster.GetQuests();
		foreach (RosterQuest item in list)
		{
			if (ListSF.GetInstance().IsEclipseQuestSuppressed(item.Name, item.FileName)) continue;
			if (QuestName != item.Name && item.get_Parameters() != null)
			{
				QuestStage stage = ListSF.GetInstance().FindEclipseSavedQuest(item.Name, item.FileName);
				if (stage != null && !stage.IsUnresumable())
				{
					num++;
				}
			}
		}
		ScreenType currentScreen = Module.GetInstance().GetCurrentScreenType();
		bool flag = false;
		if (num == 0 && (currentScreen == ScreenType.ModulePreloader || currentScreen == ScreenType.ModuleNone || flag))
		{
			errorSequence.Run(parameters);
		}
		else
		{
			successSequence.Run(parameters);
		}
	}

	private void OnActionComplete(object data)
	{
		FinishAction();
	}

	private void OnSuccessComplete(object data)
	{
		FinishAction();
		ListSF.GetRoster().StartPendingQuests();
	}

	public override void ResetSequences()
	{
		successSequence.Reset();
		errorSequence.Reset();
	}
}
