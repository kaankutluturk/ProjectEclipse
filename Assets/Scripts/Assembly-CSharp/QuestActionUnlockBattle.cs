using System.Collections.Generic;
using System.Xml;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Map;

public class QuestActionUnlockBattle : QuestAction
{
	private bool _toggle;

	private bool isLocked;

	private bool isInstant;

	private bool isHidden;

	private string _name = string.Empty;

	private string replayCountExpression = string.Empty;

	private string _hidden = string.Empty;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		string name = EPKLCPOEELO.Name;
		_toggle = name == "ShowBattle";
		isLocked = EPKLCPOEELO.Attributes["Locked"].ParseBool();
		_name = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
		isInstant = EPKLCPOEELO.Attributes["Instant"].ParseBool();
		_hidden = EPKLCPOEELO.Attributes["Hidden"].GetStringOrDefault(string.Empty);
		replayCountExpression = EPKLCPOEELO.Attributes["ReplayCount"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters JCICKLIMBEF)
	{
		base.Execute(JCICKLIMBEF);
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(JCICKLIMBEF);
		kKDGLNECFHA.SetValue(_name, lNIDLHOIHIM);
		int oAHPBDFKJOK = 0;
		if (!string.IsNullOrEmpty(replayCountExpression))
		{
			ConditionExtension.CompareResult lNIDLHOIHIM2 = new ConditionExtension.CompareResult();
			kKDGLNECFHA.SetValue(replayCountExpression, lNIDLHOIHIM2);
			oAHPBDFKJOK = (int)lNIDLHOIHIM2.resultNumber;
		}
		if (!string.IsNullOrEmpty(_hidden))
		{
			// 2.41.x intermission data uses this compact infix form.  The legacy
			// quest evaluator only understands ?Sub(...), so it used to parse as
			// false and exposed Eclipse battles while the mode was disabled.
			if (_hidden.Trim() == "1 - _$InEclipseMode")
			{
				Roster roster = ListSF.GetRoster();
				isHidden = roster == null || !roster.IsEclipseMode();
			}
			else
			{
				ConditionExtension.CompareResult lNIDLHOIHIM3 = new ConditionExtension.CompareResult();
				kKDGLNECFHA.SetValue(_hidden, lNIDLHOIHIM3);
				isHidden = lNIDLHOIHIM3.resultNumber > 0.0;
			}
		}
		FightIDS mOCEDDJOAEB = new FightIDS();
		mOCEDDJOAEB.SetFightIDSByString(lNIDLHOIHIM.resultSTR);
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		nKGLHEGIKKP.AddBattle(mOCEDDJOAEB, true, _toggle, isLocked, isHidden, oAHPBDFKJOK);
		ListSF.GetInstance().RequestSave();
		ListSF.RefreshConditionStatuses();
		Zone pKCPOJKLMOK = ListSF.GetZoneByName(mOCEDDJOAEB.GetZone());
		bool flag = HasVisibleBattle(pKCPOJKLMOK);
		Battle cGJCGEBPCAF = ((pKCPOJKLMOK == null) ? null : pKCPOJKLMOK.FindBattle(mOCEDDJOAEB.GetBattle()));
		if (cGJCGEBPCAF != null)
		{
			cGJCGEBPCAF.IsMapVisible = _toggle;
		}
		bool flag2 = HasVisibleBattle(pKCPOJKLMOK);
		MapScene current = Scene<MapScene>.get_Current();
		if (current != null)
		{
			if ((!flag && flag2) || (flag && !flag2))
			{
				current.ReloadZones();
			}
			current.ActiveBattleByFightIDS(mOCEDDJOAEB, _toggle, false, isInstant);
		}
		FinishAction();
	}

	private bool HasVisibleBattle(Zone HLJKOKMKMLM)
	{
		if (HLJKOKMKMLM == null)
		{
			return false;
		}
		List<Battle> lGIIBNJFADA = HLJKOKMKMLM.Battles;
		for (int i = 0; i < lGIIBNJFADA.Count; i++)
		{
			if (lGIIBNJFADA[i].IsMapVisible)
			{
				return true;
			}
		}
		return false;
	}
}
