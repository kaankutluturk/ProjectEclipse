using System.Xml;

public class Counter
{
	public enum CounterSpan
	{
		SPAN_NONE = 0,
		SPAN_ROUND = 1,
		SPAN_FIGHT = 2
	}

	public enum CounterMode
	{
		ECLIPSE_MODE = 0,
		NORMAL_MODE = 1,
		RAID_MODE = 2,
		NONE_MODE = 3
	}

	public string Name;

	public string Type;

	public string FightType;

	public string AnimationName;

	public string WeaponName;

	public string FightName;

	public string SecondFightName;

	public string MaxDifficulty;

	public string MinDifficulty;

	public CounterSpan Span;

	public CounterMode Mode;

	public float Value;

	public int CompleteValue;

	public bool IsFightEnd;

	public ConditionOperator Conditions = new ConditionOperator();

	public ConditionOfCompletionInspector CompletionInspector = new ConditionOfCompletionInspector();

	public Counter(XmlNode node)
	{
		Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		Value = node.Attributes["Value"].ParseFloat();
		AnimationName = node.Attributes["Animation"].GetStringOrDefault(string.Empty);
		WeaponName = node.Attributes["Weapon"].GetStringOrDefault(string.Empty);
		FightName = node.Attributes["Fight"].GetStringOrDefault(string.Empty);
		SecondFightName = node.Attributes["Fight2"].GetStringOrDefault(string.Empty);
		Type = node.Attributes["Type"].GetStringOrDefault(string.Empty);
		FightType = node.Attributes["FightType"].GetStringOrDefault(string.Empty);
		MaxDifficulty = node.Attributes["MaxDifficulty"].GetStringOrDefault(string.Empty);
		MinDifficulty = node.Attributes["MinDifficulty"].GetStringOrDefault(string.Empty);
		IsFightEnd = node.Attributes["OnFightEnd"].ParseBool();
		CompleteValue = 0;
		SetSpan(node.Attributes["CounterSpan"].GetStringOrDefault(string.Empty));
		ParseMode(node);
		Conditions.Type = ConditionOperator.OperatorKind.TYPE_AND;
		CounterConditionsParser.ParseCompletionConditions(node, Conditions, CompletionInspector);
	}

	public void SetSpan(string name)
	{
		if (name == "Round")
		{
			Span = CounterSpan.SPAN_ROUND;
		}
		else if (name == "Fight")
		{
			Span = CounterSpan.SPAN_FIGHT;
		}
		else
		{
			Span = CounterSpan.SPAN_NONE;
		}
	}

	public void ParseMode(XmlNode node)
	{
		if (node.Attributes["RaidMode"].ParseBool())
		{
			Mode = CounterMode.RAID_MODE;
			return;
		}
		string text = node.Attributes["EclipseMode"].GetStringOrDefault();
		if (text == "1")
		{
			Mode = CounterMode.ECLIPSE_MODE;
		}
		else if (text == "0")
		{
			Mode = CounterMode.NORMAL_MODE;
		}
		else
		{
			Mode = CounterMode.NONE_MODE;
		}
	}

	public void ParseConditions(XmlNode node)
	{
		Conditions.Type = ConditionOperator.OperatorKind.TYPE_AND;
		Conditions.ParseConditions(node);
	}

	public bool AreConditionsMet(CounterConditions conditions)
	{
		return Conditions.IsEqual(conditions);
	}

	public bool IsFightComplete(FightIDS DIAIIPCBMFL)
	{
		return CompletionInspector.AreAllComplete(DIAIIPCBMFL);
	}

	public void Initialize()
	{
		Conditions.Initialize();
	}

	public void ResetCompleteValue()
	{
		CompleteValue = 0;
	}
}
