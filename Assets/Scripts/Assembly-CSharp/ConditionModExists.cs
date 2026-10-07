using System.Collections.Generic;
using System.Xml;

public class ConditionModExists : ConditionAnimation
{
	private string _Name;

	private string _perk;

	public string PerkId
	{
		get
		{
			return GetPerk();
		}
	}

	public ConditionModExists(XmlNode node)
		: base(ConditionType.MOD_EXISTS)
	{
		_Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		_perk = node.Attributes["Perk"].GetStringOrDefault(string.Empty);
	}

	public string get_Name()
	{
		return _Name;
	}

	public string GetPerk()
	{
		return _perk;
	}

	public override bool IsEqual(ModelConditions conditions)
	{
		List<PerksStage.ActionPerk> list = null;
		switch (_targetModelType)
		{
		case ModelType.ModelTargetType.MODEL_THIS:
			list = conditions.SelfActionPerks;
			break;
		case ModelType.ModelTargetType.MODEL_OTHER:
			list = conditions.OtherActionPerks;
			break;
		case ModelType.ModelTargetType.MODEL_BOTH:
			list = new List<PerksStage.ActionPerk>(conditions.SelfActionPerks);
			list.AddRange(conditions.OtherActionPerks);
			break;
		}
		bool flag = false;
		PerksStage.ActionPerk oAJGINIDKJD = null;
		for (int i = 0; i < list.Count; i++)
		{
			oAJGINIDKJD = list[i];
			if ((string.IsNullOrEmpty(_perk) || _perk.Equals(oAJGINIDKJD.GetPerkName())) && _Name.Equals(oAJGINIDKJD.GetModName()))
			{
				flag = true;
				break;
			}
		}
		return (!IsNot) ? flag : (!flag);
	}
}
