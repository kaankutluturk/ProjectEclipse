using System.Xml;
using UnityEngine;

public class ComparisonExpression
{
	public enum ComparisonType
	{
		COMPARISON_NONE = 0,
		COMPARISON_EQUAL = 1,
		COMPARISON_GREATER = 2,
		COMPARISON_GREATER_EQUAL = 3,
		COMPARISON_LESS = 4,
		COMPARISON_LESS_EQUAL = 5
	}

	private const float FLT_VALUE_DELTA = 1E-05f;

	protected bool _isTrue;

	protected ComparisonType _comparisonType;

	protected float _actualValue;

	protected float _thanValue;

	public ComparisonExpression(XmlNode node)
	{
		_comparisonType = ParseComparisonType(node.Name);
		_isTrue = node.Attributes["Not"] == null || (!(node.Attributes["Not"].Value == "True") && !(node.Attributes["Not"].Value == "1"));
	}

	public bool Compare()
	{
		bool flag = true;
		switch (_comparisonType)
		{
		case ComparisonType.COMPARISON_EQUAL:
			flag = Mathf.Abs(_actualValue - _thanValue) < 1E-05f;
			break;
		case ComparisonType.COMPARISON_GREATER:
			flag = _actualValue - _thanValue > 1E-05f;
			break;
		case ComparisonType.COMPARISON_GREATER_EQUAL:
			flag = _actualValue - _thanValue > -1E-05f;
			break;
		case ComparisonType.COMPARISON_LESS:
			flag = _actualValue - _thanValue < -1E-05f;
			break;
		case ComparisonType.COMPARISON_LESS_EQUAL:
			flag = _actualValue - _thanValue < 1E-05f;
			break;
		}
		return (!_isTrue) ? (!flag) : flag;
	}

	public static ComparisonType ParseComparisonType(string name)
	{
		switch (name)
		{
		case "Equal":
			return ComparisonType.COMPARISON_EQUAL;
		case "Greater":
			return ComparisonType.COMPARISON_GREATER;
		case "GreaterEqual":
			return ComparisonType.COMPARISON_GREATER_EQUAL;
		case "Less":
			return ComparisonType.COMPARISON_LESS;
		case "LessEqual":
			return ComparisonType.COMPARISON_LESS_EQUAL;
		default:
			return ComparisonType.COMPARISON_NONE;
		}
	}
}
