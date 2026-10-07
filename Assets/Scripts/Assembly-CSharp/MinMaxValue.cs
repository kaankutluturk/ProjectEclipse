using System.Xml;

public class MinMaxValue
{
	public float Min;

	public float Max;

	public MinMaxValue(float min = 0f, float max = 0f)
	{
		Min = min;
		Max = max;
	}

	public void Parse(XmlNode node, float defaultMin = 0f, float defaultMax = 0f)
	{
		Min = node.Attributes["Min"].ParseFloat(defaultMin);
		Max = node.Attributes["Max"].ParseFloat(defaultMax);
	}
}
