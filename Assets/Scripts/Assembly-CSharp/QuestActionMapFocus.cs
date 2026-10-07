using System.Xml;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Map;

public class QuestActionMapFocus : QuestAction
{
	private string _BattleName = string.Empty;

	private float _Duration;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		_BattleName = EPKLCPOEELO.Attributes["Battle"].GetStringOrDefault(string.Empty);
		_Duration = EPKLCPOEELO.Attributes["Frames"].ParseFloat() / 60f;
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(Parameters);
		kKDGLNECFHA.SetValue(_BattleName, lNIDLHOIHIM);
		ListSF.GetRoster().SetMapFocus(lNIDLHOIHIM.ToString());
		FightIDS dIAIIPCBMFL = ListSF.GetRoster().GetMapFocus();
		MapScene current = Scene<MapScene>.get_Current();
		if (current != null)
		{
			FightList jDIPBIHBGPF = ListSF.GetFightById(dIAIIPCBMFL);
			if (jDIPBIHBGPF != null)
			{
				current.SelectFight(jDIPBIHBGPF, _Duration);
			}
			else
			{
				Battle dPOOIONCEOA = ListSF.GetBattleById(dIAIIPCBMFL);
				current.SelectBattle(dPOOIONCEOA, _Duration);
			}
		}
		FinishAction();
	}
}
