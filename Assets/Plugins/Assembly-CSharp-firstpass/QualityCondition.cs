using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

public class QualityCondition
{
	private readonly List<ComparisonExpression> expressions = new List<ComparisonExpression>();

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string name;

	public string ConditionName
	{
		get
		{
			return get_Name();
		}
		private set
		{
			set_Name(value);
		}
	}

	public QualityCondition(XmlNode node)
	{
		if (node.Attributes != null)
		{
			set_Name(node.Attributes["Name"].Value);
		}
		for (int i = 0; i < node.ChildNodes.Count; i++)
		{
			expressions.Add(new ComparisonExpression(node.ChildNodes[i]));
		}
	}

	public string get_Name()
	{
		return name;
	}

	private void set_Name(string value)
	{
		name = value;
	}

	public bool IsSatisfied()
	{
		for (int i = 0; i < expressions.Count; i++)
		{
			if (!expressions[i].Compare())
			{
				return false;
			}
		}
		return true;
	}
}
