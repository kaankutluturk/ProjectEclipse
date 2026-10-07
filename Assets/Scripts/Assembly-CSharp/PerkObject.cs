using System.Diagnostics;
using System.Xml;

public class PerkObject
{
	public bool IsNot;

	public PlayerType TargetPlayer = PlayerType.PLAYER_ME;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private PerkInfoItem perk;

	public PerkInfoItem PerkInfo
	{
		get
		{
			return GetPerk();
		}
		protected set
		{
			SetPerk(value);
		}
	}

	public PerkObject()
	{
	}

	public PerkObject(PerkObject source)
	{
		IsNot = source.IsNot;
		TargetPlayer = source.TargetPlayer;
		SetPerk(source.GetPerk());
	}

	public PerkInfoItem GetPerk()
	{
		return perk;
	}

	protected void SetPerk(PerkInfoItem value)
	{
		perk = value;
	}

	public virtual void Parse(XmlNode node)
	{
		string text = node.Attributes["Player"].GetStringOrDefault(string.Empty);
		if (text == null || text.Equals(string.Empty) || text.Equals("Me"))
		{
			TargetPlayer = PlayerType.PLAYER_ME;
		}
		else if (text.Equals("Enemy"))
		{
			TargetPlayer = PlayerType.PLAYER_ENEMY;
		}
		else
		{
			TargetPlayer = PlayerType.PLAYER_NONE;
		}
		IsNot = node.Attributes["Not"].ParseInt() > 0;
	}

	public int GetRoundStage(string name)
	{
		switch (name)
		{
		case "StartStance":
			return 1;
		case "Fight":
			return 2;
		case "EndStance":
			return 3;
		default:
			return 0;
		}
	}
}
