using System.Collections.Generic;
using System.Xml;

public class QuestActionResumeQuests : QuestAction
{
	private QuestActionsSequence successSequence = new QuestActionsSequence();

	private QuestActionsSequence errorSequence = new QuestActionsSequence();

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		XmlNode ePKLCPOEELO = EPKLCPOEELO["Success"];
		XmlNode ePKLCPOEELO2 = EPKLCPOEELO["Error"];
		ParseSequenceWithUnlock(ePKLCPOEELO, successSequence, OnSuccessComplete);
		ParseSequenceWithUnlock(ePKLCPOEELO2, errorSequence, OnActionComplete);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		ResetSequences();
		base.Execute(GFIHPBCEEOB);
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		int num = 0;
		List<RosterQuest> list = nKGLHEGIKKP.GetQuests();
		foreach (RosterQuest item in list)
		{
			if (ListSF.GetInstance().IsEclipseQuestSuppressed(item.Name, item.FileName)) continue;
			if (QuestName != item.Name && item.get_Parameters() != null)
			{
				QuestStage mLLKDGBEGJI = ListSF.GetInstance().FindEclipseSavedQuest(item.Name, item.FileName);
				if (mLLKDGBEGJI != null && !mLLKDGBEGJI.IsUnresumable())
				{
					num++;
				}
			}
		}
		ScreenType iPKNDMINFMJ = Module.GetInstance().GetCurrentScreenType();
		bool flag = false;
		if (num == 0 && (iPKNDMINFMJ == ScreenType.ModulePreloader || iPKNDMINFMJ == ScreenType.ModuleNone || flag))
		{
			errorSequence.Run(GFIHPBCEEOB);
		}
		else
		{
			successSequence.Run(GFIHPBCEEOB);
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
