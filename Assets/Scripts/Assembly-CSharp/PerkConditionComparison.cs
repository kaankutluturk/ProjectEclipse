using System.Collections.Generic;
using System.Text;
using System.Xml;

public class PerkConditionComparison : PerkConditionFunctionExtension
{
	public PerkConditionComparison()
	{
		set_Type(PerkConditionType.CONDITION_PERK_COMPARISON);
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("?Compare[");
		stringBuilder.Append(node.Attributes["Value1"].GetStringOrDefault(string.Empty));
		stringBuilder.Append(",");
		stringBuilder.Append(node.Attributes["Value2"].GetStringOrDefault(string.Empty));
		stringBuilder.Append(",");
		stringBuilder.Append(node.Name);
		stringBuilder.Append("]");
		functionExtension.Parse(stringBuilder.ToString());
		functionExtension.set_Target(this);
	}

	public override bool IsEqual(Model ACENLMONNPA, List<string> NIKHAICFGNM)
	{
		base.IsEqual(ACENLMONNPA, NIKHAICFGNM);
		FunctionResult dEIHAOLOPLC = functionExtension.Calculate();
		int num = dEIHAOLOPLC.ToInt();
		return num > 0;
	}
}
