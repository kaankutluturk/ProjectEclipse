using System.Xml;

public class ActionAddBullets : ActionAnimation
{
	private BulletType bulletType;

	private int _Value;

	public BulletType Kind
	{
		get
		{
			return GetBulletType();
		}
	}

	public int Value
	{
		get
		{
			return GetValue();
		}
	}

	public ActionAddBullets(XmlNode node)
		: base(ActionType.ADD_BULLETS)
	{
		Parse(node);
	}

	public BulletType GetBulletType()
	{
		return bulletType;
	}

	public int GetValue()
	{
		return _Value;
	}

	public override void Visit(Model model)
	{
		model.StartAction(this);
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		string text = node.Attributes["Type"].GetStringOrDefault(string.Empty);
		if (text == "MagicBullet")
		{
			bulletType = BulletType.MAGIC_BULLET;
		}
		else if (text == "RaidChargeBullet")
		{
			bulletType = BulletType.RAID_CHARGE_BULLET;
		}
		else
		{
			GameLog.Error("ERROR: Unknown bulletType");
		}
		_Value = node.Attributes["Value"].ParseInt();
		if ((bulletType == BulletType.MAGIC_BULLET || bulletType == BulletType.RAID_CHARGE_BULLET) && GameUtils.AlwaysMagicMode)
		{
			_Value = 0;
		}
	}
}
