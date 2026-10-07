using System.Xml;
using UnityEngine;

public class QuestActionOpenUrl : QuestAction
{
	private string urlExpression = string.Empty;

	private string altUrlExpression = string.Empty;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		urlExpression = EPKLCPOEELO.Attributes["URL"].GetStringOrDefault(string.Empty);
		altUrlExpression = EPKLCPOEELO.Attributes["ALT_URL"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		bool flag = false;
		if (urlExpression != string.Empty)
		{
			ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
			QuestCondition kKDGLNECFHA = new QuestCondition();
			kKDGLNECFHA.SetParameters(GFIHPBCEEOB);
			kKDGLNECFHA.SetValue(urlExpression, lNIDLHOIHIM);
			string text = lNIDLHOIHIM.ToString();
			if (text != string.Empty)
			{
				OfflineServices.OpenExternalUrl(text);
			}
		}
		if (flag && altUrlExpression != string.Empty)
		{
			ConditionExtension.CompareResult lNIDLHOIHIM2 = new ConditionExtension.CompareResult();
			QuestCondition kKDGLNECFHA2 = new QuestCondition();
			kKDGLNECFHA2.SetParameters(GFIHPBCEEOB);
			kKDGLNECFHA2.SetValue(altUrlExpression, lNIDLHOIHIM2);
			string text2 = lNIDLHOIHIM2.ToString();
			if (text2 != string.Empty)
			{
				OfflineServices.OpenExternalUrl(text2);
			}
		}
		FinishAction();
	}
}
