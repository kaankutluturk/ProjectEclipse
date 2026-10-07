using System.Collections.Generic;
using System.Xml;

public class PerkConditionBullets : PerkConditionMatchMinMax
{
	private string _bulletType;

	public PerkConditionBullets()
	{
		set_Type(PerkConditionType.CONDITION_BULLETS);
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		minMax.Parse(node, this, GetPerk());
		_bulletType = node.Attributes["Type"].GetStringOrDefault(string.Empty);
	}

	public override bool IsEqual(Model model, List<string> args)
	{
		Model targetModel = ResolveTargetModel(model);
		if (model == null)
		{
			return false;
		}
		int num = 0;
		if (_bulletType.Equals("MagicBullet"))
		{
			num = targetModel.GetMagicCharges();
		}
		else
		{
			if (!_bulletType.Equals("RaidChargeBullet"))
			{
				return false;
			}
			num = targetModel.GetRaidBullets();
		}
		minMax.EvaluateFunctions();
		if (!minMax.GetMinUnbounded() && (int)minMax.GetMinValue() > num)
		{
			return false;
		}
		if (!minMax.GetMaxUnbounded() && (int)minMax.GetMaxValue() < num)
		{
			return false;
		}
		return true;
	}
}
