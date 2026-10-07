using System.Xml;

public class ConditionItemInfo : ConditionAnimation
{
	private string _itemType;

	private string _subType;

	private string _Name;

	public string SubType
	{
		get
		{
			return GetSubType();
		}
	}

	public ConditionItemInfo(XmlNode node)
		: base(ConditionType.ITEM)
	{
		_itemType = node.Attributes["Type"].GetStringOrDefault(string.Empty);
		_subType = node.Attributes["SubType"].GetStringOrDefault(string.Empty);
		_Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public string get_Type()
	{
		return _itemType;
	}

	// best guess for name
	public string GetSubType()
	{
		return _subType;
	}

	public string get_Name()
	{
		return _Name;
	}

	public override bool IsEqual(ModelConditions conditions)
	{
		if (conditions == null || conditions.Items == null)
			return IsNot;
		foreach (ItemInfo item in conditions.Items)
		{
			if ((string.IsNullOrEmpty(_itemType) || _itemType == item.Type) && (string.IsNullOrEmpty(_subType) || _subType == item.SubType) && (string.IsNullOrEmpty(_Name) || _Name == item.Name))
			{
				return !IsNot;
			}
		}
		return IsNot;
	}
}
