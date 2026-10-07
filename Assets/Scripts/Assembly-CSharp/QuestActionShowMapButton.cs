using System.Xml;
using UnityEngine;

public class QuestActionShowMapButton : QuestAction
{
	private string _name = string.Empty;

	private string imageExpression = string.Empty;

	private string timerExpression = string.Empty;

	private string xExpression = string.Empty;

	private string yExpression = string.Empty;

	private string _type = string.Empty;

	private string Atlas = string.Empty;

	private string speedExpression = string.Empty;

	private string pauseExpression = string.Empty;

	private bool _AutoPosition;

	private string showTypeExpression = string.Empty;

	private float _anchorMinX = 0.5f;

	private float _anchorMaxX = 0.5f;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		imageExpression = node.Attributes["Image"].GetStringOrDefault(string.Empty);
		timerExpression = node.Attributes["Timer"].GetStringOrDefault(string.Empty);
		_AutoPosition = node.Attributes["X"] == null || node.Attributes["Y"] == null;
		xExpression = node.Attributes["X"].GetStringOrDefault(string.Empty);
		yExpression = node.Attributes["Y"].GetStringOrDefault(string.Empty);
		_type = node.Attributes["Type"].GetStringOrDefault(string.Empty);
		Atlas = node.Attributes["Atlas"].GetStringOrDefault(string.Empty);
		speedExpression = node.Attributes["Speed"].GetStringOrDefault(string.Empty);
		pauseExpression = node.Attributes["Pause"].GetStringOrDefault(string.Empty);
		showTypeExpression = node.Attributes["ShowType"].GetStringOrDefault("Both");
		_anchorMinX = node.Attributes["AnchorMinX"].ParseFloat(0.5f);
		_anchorMaxX = node.Attributes["AnchorMaxX"].ParseFloat(_anchorMinX);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		string name = string.Empty;
		string imageName = string.Empty;
		string timer = string.Empty;
		string buttonType = string.Empty;
		string atlasName = string.Empty;
		string showType = string.Empty;
		float speed = 0f;
		float pause = 0f;
		Vector3 buttonPosition = default(Vector3);
		GetValues(ref name, ref imageName, ref timer, ref buttonType, ref atlasName, ref speed, ref pause, ref buttonPosition, ref showType);
		MapButtonInfo buttonInfo = new MapButtonInfo(name, imageName, timer, buttonPosition, _AutoPosition, atlasName, buttonType, speed, pause, showType, _anchorMinX, _anchorMaxX);
		MapButtonController.GetInstance().AddButton(buttonInfo);
		FinishAction();
	}

	private void GetValues(ref string name, ref string imageName, ref string timer, ref string buttonType, ref string atlasName, ref float speed, ref float pause, ref Vector3 buttonPosition, ref string showType)
	{
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(Parameters);
		condition.SetValue(_name, result);
		name = result.ToString();
		result.Clear();
		condition.SetValue(imageExpression, result);
		imageName = result.ToString();
		result.Clear();
		if (!string.IsNullOrEmpty(timerExpression))
		{
			condition.SetValue(timerExpression, result);
			timer = result.ToString();
		}
		result.Clear();
		if (!string.IsNullOrEmpty(_type))
		{
			condition.SetValue(_type, result);
			buttonType = result.ToString();
		}
		result.Clear();
		if (!string.IsNullOrEmpty(Atlas))
		{
			condition.SetValue(Atlas, result);
			atlasName = result.ToString();
		}
		result.Clear();
		if (!string.IsNullOrEmpty(speedExpression))
		{
			condition.SetValue(speedExpression, result);
			speed = ((!result.IsNumber()) ? 0f : ((float)result.resultNumber));
		}
		result.Clear();
		if (!string.IsNullOrEmpty(pauseExpression))
		{
			condition.SetValue(pauseExpression, result);
			pause = ((!result.IsNumber()) ? 0f : ((float)result.resultNumber));
		}
		result.Clear();
		if (!string.IsNullOrEmpty(xExpression))
		{
			condition.SetValue(xExpression, result);
			buttonPosition.x = ((!result.IsNumber()) ? 0f : ((float)result.resultNumber));
		}
		result.Clear();
		if (!string.IsNullOrEmpty(yExpression))
		{
			condition.SetValue(yExpression, result);
			buttonPosition.y = ((!result.IsNumber()) ? 0f : ((float)result.resultNumber));
		}
		result.Clear();
		condition.SetValue(showTypeExpression, result);
		showType = result.ToString();
	}
}
