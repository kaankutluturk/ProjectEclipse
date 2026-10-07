using System.Collections.Generic;
using System.Xml;

public class RandomRule : Rule
{
	public enum RefreshMode
	{
		REFRESH_NONE = 0,
		REFRESH_EACH_FIGHT = 1,
		REFRESH_EACH_ROUND = 2
	}

	public const string RefreshEachFightName = "EachFight";

	public const string RefreshEachRoundName = "EachRound";

	private Rule selectedRule;

	private List<Rule> _rules = new List<Rule>();

	private List<Rule> availableRules = new List<Rule>();

	private List<Rule> usedRules = new List<Rule>();

	private bool _noDoubles;

	private RefreshMode refreshMode;

	public RandomRule(XmlNode node)
		: base(RuleType.RuleRandom, node)
	{
		selectedRule = null;
		_noDoubles = false;
		refreshMode = RefreshMode.REFRESH_NONE;
		Parse(node);
		usedRules = new List<Rule>();
	}

	public void SelectRandomRule()
	{
		List<Rule> list = new List<Rule>();
		int num = ListSF.GetRoster().GetLevel();
		foreach (Rule item in availableRules)
		{
			if (item.IsPlayerLevelInRange())
			{
				list.Add(item);
			}
		}
		int count = list.Count;
		if (count > 0)
		{
			int index = NekkiMath.randomInt(count);
			selectedRule = list[index];
			if (_noDoubles)
			{
				availableRules.RemoveAt(index);
				usedRules.Add(selectedRule);
			}
		}
		else if (usedRules.Count > 0)
		{
			ResetAvailableRules();
			SelectRandomRule();
		}
		else
		{
			GameLog.Error("RandomRule::resetRandom ERROR - RandomRule is empty");
		}
	}

	public void ResetAvailableRules()
	{
		availableRules.Clear();
		availableRules.AddRange(_rules);
		usedRules.Clear();
		selectedRule = null;
	}

	public Rule GetSelectedRule()
	{
		return selectedRule;
	}

	public override void SetActive(bool value)
	{
		base.SetActive(value);
		foreach (Rule item in availableRules)
		{
			item.SetActive(value);
		}
	}

	public bool CheckReset(int round)
	{
		switch (refreshMode)
		{
		case RefreshMode.REFRESH_EACH_ROUND:
			return true;
		case RefreshMode.REFRESH_EACH_FIGHT:
			return round == 1;
		default:
			GameLog.Error("RandomRule::checkReset ERROR - invalid Refresh value");
			return false;
		}
	}

	public RefreshMode GetRefreshMode()
	{
		return refreshMode;
	}

	public List<Rule> GetRules()
	{
		return _rules;
	}

	protected new void Parse(XmlNode node)
	{
		RuleParser.ParseRules(node, _rules);
		availableRules.Clear();
		availableRules.AddRange(_rules);
		foreach (Rule item in _rules)
		{
			item.IsRandom = true;
		}
		_noDoubles = node.Attributes["NoDoubles"].ParseBool();
		string text = node.Attributes["Refresh"].GetStringOrDefault(string.Empty);
		if (text == "EachRound")
		{
			refreshMode = RefreshMode.REFRESH_EACH_ROUND;
		}
		else if (text == "EachFight")
		{
			refreshMode = RefreshMode.REFRESH_EACH_FIGHT;
		}
		else
		{
			refreshMode = RefreshMode.REFRESH_EACH_FIGHT;
		}
	}
}
