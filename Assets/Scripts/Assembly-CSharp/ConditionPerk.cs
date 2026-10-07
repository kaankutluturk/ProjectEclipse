using System.Xml;

public class ConditionPerk : ConditionAnimation
{
	private string _Name;

	public ConditionPerk(XmlNode node)
		: base(ConditionType.PERK)
	{
		_Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public string get_Name()
	{
		return _Name;
	}

	public override bool IsEqual(ModelConditions conditions)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (conditions.SelfPerks != null)
		{
			foreach (PerkInfoItem item in conditions.SelfPerks)
			{
				if (item != null && _Name == item.Name)
				{
					flag2 = true;
					break;
				}
			}
		}
		if (conditions.OtherPerks != null)
		{
			foreach (PerkInfoItem item2 in conditions.OtherPerks)
			{
				if (item2 != null && _Name == item2.Name)
				{
					flag3 = true;
					break;
				}
			}
		}
		switch (_targetModelType)
		{
		case ModelType.ModelTargetType.MODEL_THIS:
			flag = flag2;
			break;
		case ModelType.ModelTargetType.MODEL_OTHER:
			flag = flag3;
			break;
		case ModelType.ModelTargetType.MODEL_BOTH:
			flag = flag2 && flag3;
			break;
		default:
			flag = flag2;
			break;
		}
		if (!IsNot)
		{
			return flag;
		}
		return !flag;
	}
}
