using System;
using System.Xml;
using Eclipse.Modding;

public class QuestActionDialogLottery : QuestAction
{
	private string fightName;
	private string spinNumber;
	private ModQuestLotteryAction presentation;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		fightName = node.Attributes["FightName"].GetStringOrDefault(string.Empty);
		spinNumber = node.Attributes["SpinNumber"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		// SpinNumber is an archived paid-spin continuation, not a free draw count.
		if (!string.IsNullOrEmpty(spinNumber)) throw new NotSupportedException("Paid lottery spin continuation is not implemented.");
		if (GetLockMode() != InputLockMode.LOCK_NONE) throw new NotSupportedException("Lottery dialogs own their input; omit the native Lock attribute.");
		presentation?.Dispose();
		base.Execute(parameters);
		var result = new ConditionExtension.CompareResult();
		var condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(fightName, result);
		presentation = new ModQuestLotteryAction(this, result.ToString(), FinishAction);
		presentation.Show();
	}

	public override void ResetSequences()
	{
		presentation?.Dispose();
		presentation = null;
	}
}
