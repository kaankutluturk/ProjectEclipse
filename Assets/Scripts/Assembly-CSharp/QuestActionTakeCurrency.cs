using System.Xml;

public class QuestActionTakeCurrency : QuestAction
{
	private string _type = string.Empty;

	private string _name = string.Empty;

	private string _value = string.Empty;

	private QuestActionsSequence successSequence = new QuestActionsSequence();

	private QuestActionsSequence errorSequence = new QuestActionsSequence();

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		_type = EPKLCPOEELO.Attributes["Type"].GetStringOrDefault(string.Empty);
		_name = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
		_value = EPKLCPOEELO.Attributes["Value"].GetStringOrDefault(string.Empty);
		XmlNode ePKLCPOEELO = EPKLCPOEELO["Success"];
		XmlNode ePKLCPOEELO2 = EPKLCPOEELO["Error"];
		ParseSequenceWithUnlock(ePKLCPOEELO, successSequence, OnActionComplete);
		ParseSequenceWithUnlock(ePKLCPOEELO2, errorSequence, OnActionComplete);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		ResetSequences();
		base.Execute(GFIHPBCEEOB);
		string LFLGCDNKNJI = string.Empty;
		string name = string.Empty;
		long value = 0L;
		GetValues(ref LFLGCDNKNJI, ref name, ref value);
		bool flag = GetIsCurrencyExist(LFLGCDNKNJI, name);
		bool flag2 = GetCurrencyCount(LFLGCDNKNJI, name) >= value;
		if (flag && flag2)
		{
			AddCurrencyCount(LFLGCDNKNJI, name, value);
			MenuController.RefreshMoney();
			successSequence.Run(GFIHPBCEEOB);
		}
		else
		{
			errorSequence.Run(GFIHPBCEEOB);
		}
	}

	private bool GetIsCurrencyExist(string LFLGCDNKNJI, string name)
	{
		bool result = false;
		switch (LFLGCDNKNJI)
		{
		case "Gold":
		case "Bonus":
			result = true;
			break;
		case "Currency":
			result = ListSF.GetRoster().GetIsCurrencyExist(name);
			break;
		default:
			if (LFLGCDNKNJI != string.Empty)
			{
				result = ListSF.GetRoster().GetIsCurrencyExist(LFLGCDNKNJI);
			}
			break;
		}
		return result;
	}

	private long GetCurrencyCount(string LFLGCDNKNJI, string name)
	{
		long num = 0L;
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		switch (LFLGCDNKNJI)
		{
		case "Gold":
			return nKGLHEGIKKP.GetMoney();
		case "Bonus":
			return nKGLHEGIKKP.GetBonus();
		case "Currency":
			return nKGLHEGIKKP.GetCurrencyCount(name);
		default:
			return nKGLHEGIKKP.GetCurrencyCount(LFLGCDNKNJI);
		}
	}

	private void AddCurrencyCount(string LFLGCDNKNJI, string name, long value)
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		switch (LFLGCDNKNJI)
		{
		case "Gold":
			nKGLHEGIKKP.SetMoney(nKGLHEGIKKP.GetMoney() - value);
			return;
		case "Bonus":
			nKGLHEGIKKP.SetBonus(nKGLHEGIKKP.GetBonus() - value, Roster.BalanceChangeType.CHANGE_QUEST);
			return;
		case "Currency":
			nKGLHEGIKKP.AddCurrencyCount(name, (int)(-value));
			return;
		}
		if (LFLGCDNKNJI != string.Empty)
		{
			nKGLHEGIKKP.AddCurrencyCount(LFLGCDNKNJI, (int)(-value));
		}
	}

	private void GetValues(ref string LFLGCDNKNJI, ref string name, ref long value)
	{
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(Parameters);
		kKDGLNECFHA.SetValue(_type, lNIDLHOIHIM);
		LFLGCDNKNJI = lNIDLHOIHIM.ToString();
		lNIDLHOIHIM.Clear();
		kKDGLNECFHA.SetValue(_name, lNIDLHOIHIM);
		name = lNIDLHOIHIM.ToString();
		lNIDLHOIHIM.Clear();
		kKDGLNECFHA.SetValue(_value, lNIDLHOIHIM);
		value = lNIDLHOIHIM.ToString().ToLong(0L);
	}

	private void OnActionComplete(object data)
	{
		FinishAction();
	}

	public override void ResetSequences()
	{
		successSequence.Reset();
		errorSequence.Reset();
	}
}
