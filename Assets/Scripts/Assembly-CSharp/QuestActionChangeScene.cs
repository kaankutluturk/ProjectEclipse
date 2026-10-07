using System.Xml;

public class QuestActionChangeScene : QuestAction
{
	private string _Destination = string.Empty;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		_Destination = EPKLCPOEELO.Attributes["Destination"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		string empty = string.Empty;
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(GFIHPBCEEOB);
		kKDGLNECFHA.SetValue(_Destination, lNIDLHOIHIM);
		empty = lNIDLHOIHIM.ToString();
		Module.GetInstance().AddEventListener(1, OnModuleChanged);
		ScreenType kAHMHPNJBGI = Module.ParseScreenType(empty);
		ChangeToScreen(kAHMHPNJBGI);
	}

	private void OnModuleChanged(object data)
	{
		FinishAction();
		Module.GetInstance().RemoveEventListener(1, OnModuleChanged);
	}

	private void ChangeToScreen(ScreenType KAHMHPNJBGI)
	{
		ScreenType iPKNDMINFMJ = Module.GetInstance().GetCurrentScreenType();
		bool flag = false;
		bool flag2 = KAHMHPNJBGI == iPKNDMINFMJ;
		bool flag3 = KAHMHPNJBGI != ScreenType.ModuleFight;
		if (flag2)
		{
			flag = true;
		}
		else if (flag3)
		{
			flag = !Module.OpenScreen(KAHMHPNJBGI);
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
