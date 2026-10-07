using System.Collections.Generic;
using System.Xml;

public class AttributesRule : InFightRule
{
	private Dictionary<string, float> attributeValues = new Dictionary<string, float>();

	public AttributesRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(RuleType.RuleAttributes, EJPOJJKKICO, node)
	{
		foreach (GameUtils.AlignTargetAttribute item in GameUtils.AlignTargetAttributes)
		{
			attributeValues[item.Name] = 0f;
		}
		Parse(node);
	}

	public override void InitRule(object data)
	{
		RuleInitData oIFPCFEGFOB = (RuleInitData)data;
		foreach (KeyValuePair<string, float> item in attributeValues)
		{
			float value = item.Value;
			switch (appliance)
			{
			case RuleAppliance.AppliancePlayer:
			{
				int OEMALIFPGPO2 = 0;
				oIFPCFEGFOB.PlayerParameters.FinalAttributes.Get(item.Key, ref OEMALIFPGPO2);
				oIFPCFEGFOB.PlayerParameters.FinalAttributes.Set(item.Key, OEMALIFPGPO2 + (int)value);
				break;
			}
			case RuleAppliance.ApplianceOpponent:
			{
				int OEMALIFPGPO = 0;
				oIFPCFEGFOB.OpponentParameters.FinalAttributes.Get(item.Key, ref OEMALIFPGPO);
				oIFPCFEGFOB.OpponentParameters.FinalAttributes.Set(item.Key, OEMALIFPGPO + (int)value);
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
		InFightRule aAJIFBJLJOA = null;
		RuleAppliance eJPOJJKKICO = GetAppliance();
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new AttributesRule(hKPPBKPJOEO, eJPOJJKKICO);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
