using System.Xml;

public class QuestActionGiveCurrency : QuestAction
{
	private string Type;

	private string Value;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		Type = EPKLCPOEELO.Attributes["Type"].GetStringOrDefault(string.Empty);
		Value = EPKLCPOEELO.Attributes["Value"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		string LFLGCDNKNJI = string.Empty;
		long value = 0L;
		GetValues(ref LFLGCDNKNJI, ref value);
		if (LFLGCDNKNJI == "Gold")
		{
			nKGLHEGIKKP.SetMoney(nKGLHEGIKKP.GetMoney() + value);
		}
		else if (LFLGCDNKNJI == "Bonus")
		{
			nKGLHEGIKKP.SetBonus(nKGLHEGIKKP.GetBonus() + value, Roster.BalanceChangeType.CHANGE_QUEST);
		}
		else if (LFLGCDNKNJI != string.Empty)
		{
			nKGLHEGIKKP.AddCurrencyCount(LFLGCDNKNJI, (int)value);
		}
		MenuController.RecreateMoney();
		FinishAction();
	}

	private void GetValues(ref string LFLGCDNKNJI, ref long value)
	{
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(Parameters);
		kKDGLNECFHA.SetValue(Type, lNIDLHOIHIM);
		LFLGCDNKNJI = lNIDLHOIHIM.ToString();
		lNIDLHOIHIM.Clear();
		kKDGLNECFHA.SetValue(Value, lNIDLHOIHIM);
		value = (long)lNIDLHOIHIM.resultNumber;
	}
}
