using System.Xml;
using UnityEngine;

public class QuestActionOpenUrl : QuestAction
{
	private string urlExpression = string.Empty;

	private string altUrlExpression = string.Empty;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		urlExpression = node.Attributes["URL"].GetStringOrDefault(string.Empty);
		altUrlExpression = node.Attributes["ALT_URL"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		bool flag = false;
		if (urlExpression != string.Empty)
		{
			ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
			QuestCondition condition = new QuestCondition();
			condition.SetParameters(parameters);
			condition.SetValue(urlExpression, result);
			string text = result.ToString();
			if (text != string.Empty)
			{
				OfflineServices.OpenExternalUrl(text);
			}
		}
		if (flag && altUrlExpression != string.Empty)
		{
			ConditionExtension.CompareResult lNIDLHOIHIM2 = new ConditionExtension.CompareResult();
			QuestCondition kKDGLNECFHA2 = new QuestCondition();
			kKDGLNECFHA2.SetParameters(parameters);
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
