using System.Xml;

public class QuestActionChangeScene : QuestAction
{
	private string _Destination = string.Empty;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_Destination = node.Attributes["Destination"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		string empty = string.Empty;
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(_Destination, result);
		empty = result.ToString();
		Module.GetInstance().AddEventListener(1, OnModuleChanged);
		ScreenType screenType = Module.ParseScreenType(empty);
		ChangeToScreen(screenType);
	}

	private void OnModuleChanged(object data)
	{
		FinishAction();
		Module.GetInstance().RemoveEventListener(1, OnModuleChanged);
	}

	private void ChangeToScreen(ScreenType targetScreen)
	{
		ScreenType currentScreen = Module.GetInstance().GetCurrentScreenType();
		bool flag = false;
		bool flag2 = targetScreen == currentScreen;
		bool flag3 = targetScreen != ScreenType.ModuleFight;
		if (flag2)
		{
			flag = true;
		}
		else if (flag3)
		{
			flag = !Module.OpenScreen(targetScreen);
		}
		else
		{
			Module.OpenScreen(ScreenType.ModuleDojo);
		}
		if (flag)
		{
			OnModuleChanged(0);
		}
	}
}
