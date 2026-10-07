using System.Collections.Generic;
using System.Xml;

public class ConditionOfCompletionBattle : ConditionOfCompletion
{
	private string _name;

	public ConditionOfCompletionBattle(XmlNode node)
	{
		_name = node.Attributes["Name"].GetStringOrDefault();
	}

	public ConditionOfCompletionBattle(string name, int count)
	{
		_name = name;
	}

	bool ConditionOfCompletion.IsComplete(FightIDS fightId)
	{
		if (IsFightInRoster() || fightId.Equals(_name))
		{
			return true;
		}
		return false;
	}

	private bool IsFightInRoster()
	{
		List<RosterFight> list = ListSF.GetRoster().GetSavedFights();
		bool result = false;
		FightIDS targetFightId = new FightIDS();
		targetFightId.SetFightIDSByString(_name);
		for (int i = 0; i < list.Count; i++)
		{
			RosterFight rosterFight = list[i];
			if (targetFightId.Equals(rosterFight.GetFightIdString()))
			{
				result = true;
			}
		}
		return result;
	}
}
