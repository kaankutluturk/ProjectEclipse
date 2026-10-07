using System.Xml;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Map;

public class QuestActionToggleBattle : QuestAction
{
	private bool _toggle;

	private string _name;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		string text = EPKLCPOEELO.Attributes["Toggle"].GetStringOrDefault(string.Empty);
		if (text.Equals("on"))
		{
			_toggle = true;
		}
		else if (text.Equals("off"))
		{
			_toggle = false;
		}
		_name = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(GFIHPBCEEOB);
		kKDGLNECFHA.SetValue(_name, lNIDLHOIHIM);
		FightIDS mOCEDDJOAEB = new FightIDS();
		mOCEDDJOAEB.SetFightIDSByString(lNIDLHOIHIM.resultSTR);
		Battle cGJCGEBPCAF = ListSF.GetBattleById(mOCEDDJOAEB);
		RosterBattle dDNLCGOPAGC = ((cGJCGEBPCAF == null) ? null : cGJCGEBPCAF.GetRosterBattle());
		if (dDNLCGOPAGC != null)
		{
			dDNLCGOPAGC.SetHidden(!_toggle);
		}
		MapScene current = Scene<MapScene>.get_Current();
		if (current != null && cGJCGEBPCAF != null)
		{
			current.UpdateBattleButtonHidden(cGJCGEBPCAF);
		}
		ListSF.GetInstance().RequestSave();
		FinishAction();
	}
}
