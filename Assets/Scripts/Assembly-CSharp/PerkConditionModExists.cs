using System.Collections.Generic;
using System.Xml;

public class PerkConditionModExists : PerkCondition
{
	private string Name;

	private string Namespace;

	public PerkConditionModExists()
	{
		set_Type(PerkConditionType.CONDITION_MOD_EXISTS);
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		Namespace = node.Attributes["Namespace"].GetStringOrDefault(string.Empty);
	}

	public override bool IsEqual(Model model, List<string> modNames)
	{
		if (Namespace != string.Empty && PerksStage.CheckModNameInNamespace(Name, Namespace))
		{
			return true;
		}
		Model targetModel = ResolveTargetModel(model);
		if (targetModel == null)
		{
			return false;
		}
		if (targetModel.HasTransientPerkFlag(Name))
		{
			return true;
		}
		if (modNames == null)
		{
			return false;
		}
		foreach (string item in modNames)
		{
			string value = item;
			if (Name.Equals(value))
			{
				return true;
			}
		}
		return false;
	}
}
