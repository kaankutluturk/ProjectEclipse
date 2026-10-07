using System.Xml;

public class ConditionBullets : ConditionAnimation
{
	private BulletType _bulletType;

	private int _min;

	private int _max;

	public ConditionBullets(XmlNode node)
		: base(ConditionType.BULLETS)
	{
		string text = node.Attributes["Type"].GetStringOrDefault(string.Empty);
		if (text == "MagicBullet")
		{
			_bulletType = BulletType.MAGIC_BULLET;
		}
		else if (text == "RaidChargeBullet")
		{
			_bulletType = BulletType.RAID_CHARGE_BULLET;
		}
		else
		{
			GameLog.Error("ERROR: Unknown bulletType");
		}
		_min = node.Attributes["Min"].ParseInt();
		_max = node.Attributes["Max"].ParseInt(int.MaxValue);
		if ((_bulletType == BulletType.MAGIC_BULLET || _bulletType == BulletType.RAID_CHARGE_BULLET) && GameUtils.AlwaysMagicMode)
		{
			_min = 0;
		}
	}

	public override bool IsEqual(ModelConditions conditions)
	{
		int num = 0;
		switch (_bulletType)
		{
		case BulletType.MAGIC_BULLET:
			num = conditions.MagicCharges;
			break;
		case BulletType.RAID_CHARGE_BULLET:
			num = conditions.RaidCharges;
			break;
		default:
			GameLog.Error("Strange type condition bullet");
			break;
		}
		bool flag = _min <= num && num <= _max;
		return (!IsNot) ? flag : (!flag);
	}
}
