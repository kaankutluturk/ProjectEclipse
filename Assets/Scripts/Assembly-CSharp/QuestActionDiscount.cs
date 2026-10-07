using System.Collections.Generic;
using System.Xml;

public class QuestActionDiscount : QuestAction
{
	private string itemExpression = string.Empty;

	private string percentExpression = "0";

	private string toggleExpression = "0";

	private string periodExpression = string.Empty;

	private string newAmountExpression = string.Empty;

	private string newPriceExpression = string.Empty;

	private string saleExpression = string.Empty;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		itemExpression = EPKLCPOEELO.Attributes["Item"].GetStringOrDefault(string.Empty);
		percentExpression = EPKLCPOEELO.Attributes["Percent"].GetStringOrDefault(string.Empty);
		toggleExpression = EPKLCPOEELO.Attributes["Toggle"].GetStringOrDefault(string.Empty);
		newAmountExpression = EPKLCPOEELO.Attributes["NewAmount"].GetStringOrDefault("0");
		newPriceExpression = EPKLCPOEELO.Attributes["NewPrice"].GetStringOrDefault(string.Empty);
		periodExpression = EPKLCPOEELO.Attributes["Period"].GetStringOrDefault("0");
		saleExpression = EPKLCPOEELO.Attributes["Sale"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		FinishAction();
	}

	private void EvaluateParameters(QuestParameters GFIHPBCEEOB, ItemInfo item, ref int upgradeLevel, ref float IFKAJHEOAEG, ref bool LPPNCLBEAFA, ref long AJKMNFGEHIJ, ref long GKIHFPFHKCI, ref string DDHOJFFGBKM, ref bool GEPBMEMMLEA)
	{
		string empty = string.Empty;
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(GFIHPBCEEOB);
		kKDGLNECFHA.SetValue(itemExpression, lNIDLHOIHIM);
		empty = lNIDLHOIHIM.ToString();
		string text = string.Empty;
		string text2 = string.Empty;
		List<string> list = new List<string>(empty.Split('|'));
		int count = list.Count;
		if (count > 0)
		{
			text = list[0];
		}
		if (count >= 2)
		{
			text2 = list[1];
		}
		if (text2 != string.Empty)
		{
			kKDGLNECFHA.SetValue(text2, lNIDLHOIHIM);
			upgradeLevel = (int)lNIDLHOIHIM.resultNumber;
		}
		else
		{
			upgradeLevel = -1;
		}
		lNIDLHOIHIM.Clear();
		kKDGLNECFHA.SetValue(percentExpression, lNIDLHOIHIM);
		IFKAJHEOAEG = (float)lNIDLHOIHIM.resultNumber;
		lNIDLHOIHIM.Clear();
		kKDGLNECFHA.SetValue(toggleExpression, lNIDLHOIHIM);
		LPPNCLBEAFA = lNIDLHOIHIM.resultNumber > 0.0;
		lNIDLHOIHIM.Clear();
		kKDGLNECFHA.SetValue(newAmountExpression, lNIDLHOIHIM);
		AJKMNFGEHIJ = (long)lNIDLHOIHIM.resultNumber;
		lNIDLHOIHIM.Clear();
		kKDGLNECFHA.SetValue(periodExpression, lNIDLHOIHIM);
		GKIHFPFHKCI = (long)lNIDLHOIHIM.resultNumber;
		lNIDLHOIHIM.Clear();
		kKDGLNECFHA.SetValue(newPriceExpression, lNIDLHOIHIM);
		if (newPriceExpression != string.Empty && lNIDLHOIHIM.IsNumber())
		{
			DDHOJFFGBKM = lNIDLHOIHIM.resultNumber.ToString();
		}
		else
		{
			DDHOJFFGBKM = lNIDLHOIHIM.resultSTR;
		}
		lNIDLHOIHIM.Clear();
		kKDGLNECFHA.SetValue(saleExpression, lNIDLHOIHIM);
		GEPBMEMMLEA = lNIDLHOIHIM.resultNumber > 0.0;
		item = ListSF.GetItems().GetItemByName(text);
		if (item == null)
		{
			GameLog.Error("QuestActionDiscount - cant find item \"%s\" from name \"%s\"", text, itemExpression);
		}
	}
}
