using System.Xml;

public class PointsRule : InFightRule
{
	public enum StrikeZone
	{
		STRIKE_ZONE_HEAD = 0,
		STRIKE_ZONE_BODY = 1,
		STRIKE_ZONE_ALL = 2
	}

	private PointsTableType tableType;

	private int pointsPerHit;

	private int playerPoints;

	private int opponentPoints;

	private int maxPoints;

	private bool isFinished;

	private bool requiredBlock;

	private bool requiredCritical;

	private bool requiredShock;

	private bool hasBlockFilter;

	private bool hasCriticalFilter;

	private bool hasShockFilter;

	private StrikeZone strikeZone;

	public PointsRule(XmlNode node, RuleAppliance appliance)
		: base(RuleType.RulePoints, appliance, node)
	{
		opponentPoints = 0;
		playerPoints = 0;
		isFinished = false;
		strikeZone = StrikeZone.STRIKE_ZONE_ALL;
		tableType = PointsTableType.POINTS_TABLE_CONTEST;
		requiredBlock = false;
		requiredCritical = false;
		requiredShock = false;
		applianceLosesOnTrigger = false;
		SubscribeEvent(FightEvent.StrikeEvent);
		SubscribeEvent(FightEvent.TimeoutEvent);
		Parse(node);
		Reset();
	}

	public override void Reset()
	{
		opponentPoints = 0;
		playerPoints = 0;
		isFinished = false;
	}

	public override bool Compare(object data)
	{
		PrepareCompare(data);
		PlayersFightData fightData = (PlayersFightData)data;
		bool result = false || TryCountStrike(fightData.PlayerData, fightData.EnemyData, true) || TryCountStrike(fightData.EnemyData, fightData.PlayerData, false);
		if (fightData.PlayerData.FightEventType == FightEvent.TimeoutEvent || fightData.EnemyData.FightEventType == FightEvent.TimeoutEvent)
		{
			isFinished = true;
			result = true;
		}
		return result;
	}

	public int GetPlayerPoints()
	{
		return playerPoints;
	}

	public int GetOpponentPoints()
	{
		return opponentPoints;
	}

	public int GetMaxPoints()
	{
		return maxPoints;
	}

	public bool GetIsFinished()
	{
		return isFinished;
	}

	public override void InitRule(object data)
	{
		Reset();
	}

	public override RuleAppliance GetWinnerAppliance()
	{
		switch (tableType)
		{
		case PointsTableType.POINTS_TABLE_CONTEST:
			return (playerPoints > opponentPoints) ? RuleAppliance.AppliancePlayer : RuleAppliance.ApplianceOpponent;
		case PointsTableType.POINTS_TABLE_SCORE:
			if (playerPoints >= maxPoints)
			{
				return RuleAppliance.AppliancePlayer;
			}
			return RuleAppliance.ApplianceOpponent;
		default:
			return RuleAppliance.ApplianceNone;
		}
	}

	public PointsTableType GetTableType()
	{
		return tableType;
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		string text = node.Attributes["Type"].GetStringOrDefault("Contest");
		if (text == "Contest")
		{
			tableType = PointsTableType.POINTS_TABLE_CONTEST;
		}
		else if (text == "Score")
		{
			tableType = PointsTableType.POINTS_TABLE_SCORE;
		}
		maxPoints = node.Attributes["Max"].ParseInt();
		pointsPerHit = node.Attributes["PointsPerHit"].ParseInt();
		hasBlockFilter = !node.Attributes["Block"].Empty();
		requiredBlock = node.Attributes["Block"].ParseBool();
		hasCriticalFilter = !node.Attributes["Critical"].Empty();
		requiredCritical = node.Attributes["Critical"].ParseBool();
		hasShockFilter = !node.Attributes["Shock"].Empty();
		requiredShock = node.Attributes["Shock"].ParseBool();
		ParseStrikeZone(node);
	}

	protected void ParseStrikeZone(XmlNode node)
	{
		string text = node.Attributes["Defense"].GetStringOrDefault(string.Empty);
		if (text == string.Empty)
		{
			strikeZone = StrikeZone.STRIKE_ZONE_ALL;
		}
		else if (text == "BodyDefense")
		{
			strikeZone = StrikeZone.STRIKE_ZONE_BODY;
		}
		else if (text == "HeadDefense")
		{
			strikeZone = StrikeZone.STRIKE_ZONE_HEAD;
		}
	}

	protected bool CheckStrikeZone(bool isHeadHit)
	{
		return strikeZone == StrikeZone.STRIKE_ZONE_ALL || (strikeZone == StrikeZone.STRIKE_ZONE_HEAD && isHeadHit) || (strikeZone == StrikeZone.STRIKE_ZONE_BODY && !isHeadHit);
	}

	protected bool TryCountStrike(FightData strikerData, FightData targetData, bool isPlayer)
	{
		bool flag = !hasBlockFilter || strikerData.IsBlocked == requiredBlock;
		bool flag2 = !hasCriticalFilter || strikerData.IsCritical == requiredCritical;
		bool flag3 = !hasShockFilter || targetData.IsShocked == requiredShock;
		if (strikerData.FightEventType == FightEvent.StrikeEvent && strikerData.IsAttacker && flag && CheckStrikeZone(strikerData.IsHeadHit) && flag2 && flag3)
		{
			if (isPlayer)
			{
				playerPoints++;
			}
			else
			{
				opponentPoints++;
			}
			if (tableType == PointsTableType.POINTS_TABLE_SCORE && ((isPlayer && playerPoints >= maxPoints) || (!isPlayer && opponentPoints >= maxPoints)))
			{
				isFinished = true;
			}
			return true;
		}
		return false;
	}

	public override InFightRule Copy()
	{
		InFightRule copy = null;
		RuleAppliance appliance = GetAppliance();
		XmlNode node = GetXmlSource().GetNode();
		copy = new PointsRule(node, appliance);
		copy.IsRandom = IsRandom;
		return copy;
	}
}
