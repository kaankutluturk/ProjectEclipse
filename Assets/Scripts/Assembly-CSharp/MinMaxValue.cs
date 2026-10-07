using System.Xml;

public class MinMaxValue
{
	public float Min;

	public float Max;

	public MinMaxValue(float NOFALOKFBEM = 0f, float MFODOCNLNPH = 0f)
	{
		Min = NOFALOKFBEM;
		Max = MFODOCNLNPH;
	}

	public void Parse(XmlNode node, float PCEKHCGCHFH = 0f, float JCKIAGACDMA = 0f)
	{
		Min = node.Attributes["Min"].ParseFloat(PCEKHCGCHFH);
		Max = node.Attributes["Max"].ParseFloat(JCKIAGACDMA);
	}
}
