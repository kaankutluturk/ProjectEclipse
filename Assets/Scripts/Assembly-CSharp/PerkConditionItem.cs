using System.Collections.Generic;
using System.Xml;

public class PerkConditionItem : PerkCondition
{
	private string Name;

	private string ItemType;

	private string SubType;

	public PerkConditionItem()
	{
		set_Type(PerkConditionType.CONDITION_ITEM);
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		ItemType = node.Attributes["Type"].GetStringOrDefault(string.Empty);
		SubType = node.Attributes["Subtype"].GetStringOrDefault(string.Empty);
	}

	public override bool IsEqual(Model ACENLMONNPA, List<string> NIKHAICFGNM)
	{
		Model fGCODGKLHED = ResolveTargetModel(ACENLMONNPA);
		if (ACENLMONNPA == null)
		{
			return false;
		}
		List<ItemInfo> list = fGCODGKLHED.Parameters.GetEquippedItemsByType();
		foreach (ItemInfo item in list)
		{
			if ((ItemType.Equals(string.Empty) || ItemType.Equals(item.Type)) && (SubType.Equals(string.Empty) || SubType.Equals(item.SubType)) && (Name.Equals(string.Empty) || Name.Equals(item.Name)))
			{
				return true;
			}
		}
		return false;
	}
}
