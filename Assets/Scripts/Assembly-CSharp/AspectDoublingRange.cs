using System.Xml;

public class AspectDoublingRange
{
	private float _value;

	private int _levelStep;

	public float Value
	{
		get
		{
			return GetValue();
		}
	}

	public int LevelStep
	{
		get
		{
			return GetLevelStep();
		}
	}

	public float GetValue()
	{
		return _value;
	}

	public int GetLevelStep()
	{
		return _levelStep;
	}

	public void Parse(XmlNode node)
	{
		_value = node.Attributes["Value"].ParseFloat();
		_levelStep = node.Attributes["LevelStep"].ParseInt();
	}
}
