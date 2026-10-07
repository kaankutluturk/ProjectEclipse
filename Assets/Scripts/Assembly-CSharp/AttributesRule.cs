using System.Collections.Generic;
using System.Xml;

public class AttributesRule : InFightRule
{
	private Dictionary<string, float> attributeValues = new Dictionary<string, float>();

	public AttributesRule(XmlNode node, RuleAppliance ruleAppliance)
		: base(RuleType.RuleAttributes, ruleAppliance, node)
	{
		foreach (GameUtils.AlignTargetAttribute item in GameUtils.AlignTargetAttributes)
		{
			attributeValues[item.Name] = 0f;
		}
		Parse(node);
	}

	public override void InitRule(object data)
	{
		RuleInitData initData = (RuleInitData)data;
		foreach (KeyValuePair<string, float> item in attributeValues)
		{
			float value = item.Value;
			switch (appliance)
			{
			case RuleAppliance.AppliancePlayer:
			{
				int attributeValue = 0;
				initData.PlayerParameters.FinalAttributes.Get(item.Key, ref attributeValue);
				initData.PlayerParameters.FinalAttributes.Set(item.Key, attributeValue + (int)value);
				break;
			}
			case RuleAppliance.ApplianceOpponent:
			{
				int currentValue = 0;
				initData.OpponentParameters.FinalAttributes.Get(item.Key, ref currentValue);
				initData.OpponentParameters.FinalAttributes.Set(item.Key, currentValue + (int)value);
				break;
			}
			default:
				GameLog.Error("AttributesRule::initRule - wrong appliance - %i", appliance);
				break;
			}
		}
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		foreach (XmlAttribute attribute in node.Attributes)
		{
			if (attribute.Name != "Round" && attribute.Name != "ApplyTo" && attribute.Name != "Eclipse" && attribute.Name != "WarriorPower")
			{
				if (!attributeValues.ContainsKey(attribute.Name))
				{
					attributeValues.Add(attribute.Name, attribute.ParseFloat());
				}
				else
				{
					attributeValues[attribute.Name] += attribute.ParseFloat();
				}
			}
			if (!(attribute.Name == "WarriorPower"))
			{
				continue;
			}
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, float> item in attributeValues)
			{
				list.Add(item.Key);
			}
			foreach (string item2 in list)
			{
				attributeValues[item2] += attribute.ParseFloat();
			}
		}
	}

	public Dictionary<string, float> GetAttributeValues()
	{
		return attributeValues;
	}

	public override InFightRule Copy()
	{
		InFightRule copy = null;
		RuleAppliance ruleAppliance = GetAppliance();
		XmlNode sourceNode = GetXmlSource().GetNode();
		copy = new AttributesRule(sourceNode, ruleAppliance);
		copy.IsRandom = IsRandom;
		return copy;
	}
}
