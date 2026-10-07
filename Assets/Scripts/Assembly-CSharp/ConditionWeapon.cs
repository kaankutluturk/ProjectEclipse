using System.Xml;

public class ConditionWeapon : ConditionAnimation
{
	private string _weaponType;

	private string _subType;

	private string _Name;

	public string SubType
	{
		get
		{
			return GetSubType();
		}
	}

	public ConditionWeapon(XmlNode node)
		: base(ConditionType.WEAPONS)
	{
		_weaponType = node.Attributes["Type"].GetStringOrDefault(string.Empty);
		_subType = node.Attributes["SubType"].GetStringOrDefault(string.Empty);
		_Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public string get_Type()
	{
		return _weaponType;
	}

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
		if (conditions.IsWeapon)
		{
			foreach (ItemInfo item in conditions.Items)
			{
				if ((string.IsNullOrEmpty(_weaponType) || _weaponType == item.Type) && (string.IsNullOrEmpty(_subType) || _subType == item.SubType) && (string.IsNullOrEmpty(_Name) || _Name == item.Name))
				{
					return !IsNot;
				}
			}
		}
		return IsNot;
	}
}
