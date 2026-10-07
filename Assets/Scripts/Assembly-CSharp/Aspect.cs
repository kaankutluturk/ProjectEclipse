using System;
using System.Xml;

public class Aspect
{
	private string _name = string.Empty;

	private string _attribute = string.Empty;

	private float _base;

	public string Attribute
	{
		get
		{
			return GetAttribute();
		}
	}

	public string get_Name()
	{
		return _name;
	}

	public string GetAttribute()
	{
		return _attribute;
	}

	public void Parse(XmlNode node)
	{
		_name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		_attribute = node.Attributes["Attribute"].GetStringOrDefault(string.Empty);
		_base = node.Attributes["Base"].ParseFloat();
	}

	public int GetValue(double value, int level, int levelFactor, double scale)
	{
		double num = (value - (double)(level * levelFactor)) / scale;
		double num2 = ((!(num < 0.0)) ? ((double)_base * (2.0 - Math.Pow(2.0, 0.0 - num))) : ((double)_base * Math.Pow(2.0, num)));
		return (int)num2;
	}
}
