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

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		_name = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
		imageExpression = EPKLCPOEELO.Attributes["Image"].GetStringOrDefault(string.Empty);
		timerExpression = EPKLCPOEELO.Attributes["Timer"].GetStringOrDefault(string.Empty);
		_AutoPosition = EPKLCPOEELO.Attributes["X"] == null || EPKLCPOEELO.Attributes["Y"] == null;
		xExpression = EPKLCPOEELO.Attributes["X"].GetStringOrDefault(string.Empty);
		yExpression = EPKLCPOEELO.Attributes["Y"].GetStringOrDefault(string.Empty);
		_type = EPKLCPOEELO.Attributes["Type"].GetStringOrDefault(string.Empty);
		Atlas = EPKLCPOEELO.Attributes["Atlas"].GetStringOrDefault(string.Empty);
		speedExpression = EPKLCPOEELO.Attributes["Speed"].GetStringOrDefault(string.Empty);
		pauseExpression = EPKLCPOEELO.Attributes["Pause"].GetStringOrDefault(string.Empty);
		showTypeExpression = EPKLCPOEELO.Attributes["ShowType"].GetStringOrDefault("Both");
		_anchorMinX = EPKLCPOEELO.Attributes["AnchorMinX"].ParseFloat(0.5f);
		_anchorMaxX = EPKLCPOEELO.Attributes["AnchorMaxX"].ParseFloat(_anchorMinX);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		string name = string.Empty;
		string KHPKDMGDMAB = string.Empty;
		string timer = string.Empty;
		string LFLGCDNKNJI = string.Empty;
		string NBHBEJFPFBN = string.Empty;
		string CACHHLONJII = string.Empty;
		float ALCFJHNPDGL = 0f;
		float KCANPMPILKI = 0f;
		Vector3 AJMBPDGKMAF = default(Vector3);
		GetValues(ref name, ref KHPKDMGDMAB, ref timer, ref LFLGCDNKNJI, ref NBHBEJFPFBN, ref ALCFJHNPDGL, ref KCANPMPILKI, ref AJMBPDGKMAF, ref CACHHLONJII);
		MapButtonInfo dJDNMAOEFBD = new MapButtonInfo(name, KHPKDMGDMAB, timer, AJMBPDGKMAF, _AutoPosition, NBHBEJFPFBN, LFLGCDNKNJI, ALCFJHNPDGL, KCANPMPILKI, CACHHLONJII, _anchorMinX, _anchorMaxX);
		MapButtonController.GetInstance().AddButton(dJDNMAOEFBD);
		FinishAction();
	}

	private void GetValues(ref string name, ref string KHPKDMGDMAB, ref string timer, ref string LFLGCDNKNJI, ref string NBHBEJFPFBN, ref float ALCFJHNPDGL, ref float KCANPMPILKI, ref Vector3 AJMBPDGKMAF, ref string CACHHLONJII)
	{
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(Parameters);
		kKDGLNECFHA.SetValue(_name, lNIDLHOIHIM);
		name = lNIDLHOIHIM.ToString();
		lNIDLHOIHIM.Clear();
		kKDGLNECFHA.SetValue(imageExpression, lNIDLHOIHIM);
		KHPKDMGDMAB = lNIDLHOIHIM.ToString();
		lNIDLHOIHIM.Clear();
		if (!string.IsNullOrEmpty(timerExpression))
		{
			kKDGLNECFHA.SetValue(timerExpression, lNIDLHOIHIM);
			timer = lNIDLHOIHIM.ToString();
		}
		lNIDLHOIHIM.Clear();
		if (!string.IsNullOrEmpty(_type))
		{
			kKDGLNECFHA.SetValue(_type, lNIDLHOIHIM);
			LFLGCDNKNJI = lNIDLHOIHIM.ToString();
		}
		lNIDLHOIHIM.Clear();
		if (!string.IsNullOrEmpty(Atlas))
		{
			kKDGLNECFHA.SetValue(Atlas, lNIDLHOIHIM);
			NBHBEJFPFBN = lNIDLHOIHIM.ToString();
		}
		lNIDLHOIHIM.Clear();
		if (!string.IsNullOrEmpty(speedExpression))
		{
			kKDGLNECFHA.SetValue(speedExpression, lNIDLHOIHIM);
			ALCFJHNPDGL = ((!lNIDLHOIHIM.IsNumber()) ? 0f : ((float)lNIDLHOIHIM.resultNumber));
		}
		lNIDLHOIHIM.Clear();
		if (!string.IsNullOrEmpty(pauseExpression))
		{
			kKDGLNECFHA.SetValue(pauseExpression, lNIDLHOIHIM);
			KCANPMPILKI = ((!lNIDLHOIHIM.IsNumber()) ? 0f : ((float)lNIDLHOIHIM.resultNumber));
		}
		lNIDLHOIHIM.Clear();
		if (!string.IsNullOrEmpty(xExpression))
		{
			kKDGLNECFHA.SetValue(xExpression, lNIDLHOIHIM);
			AJMBPDGKMAF.x = ((!lNIDLHOIHIM.IsNumber()) ? 0f : ((float)lNIDLHOIHIM.resultNumber));
		}
		lNIDLHOIHIM.Clear();
		if (!string.IsNullOrEmpty(yExpression))
		{
			kKDGLNECFHA.SetValue(yExpression, lNIDLHOIHIM);
			AJMBPDGKMAF.y = ((!lNIDLHOIHIM.IsNumber()) ? 0f : ((float)lNIDLHOIHIM.resultNumber));
		}
		lNIDLHOIHIM.Clear();
		kKDGLNECFHA.SetValue(showTypeExpression, lNIDLHOIHIM);
		CACHHLONJII = lNIDLHOIHIM.ToString();
	}
}
