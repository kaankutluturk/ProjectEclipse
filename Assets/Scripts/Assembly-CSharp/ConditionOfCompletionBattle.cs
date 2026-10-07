using System.Collections.Generic;
using System.Xml;

public class ConditionOfCompletionBattle : ConditionOfCompletion
{
	private string _name;

	public ConditionOfCompletionBattle(XmlNode node)
	{
		_name = node.Attributes["Name"].GetStringOrDefault();
	}

	public ConditionOfCompletionBattle(string name, int OGOLNFLBLBD)
	{
		_name = name;
	}

	bool ConditionOfCompletion.IsComplete(FightIDS DIAIIPCBMFL)
	{
		if (IsFightInRoster() || DIAIIPCBMFL.Equals(_name))
		{
			return true;
		}
		return false;
	}

	private bool IsFightInRoster()
	{
		List<RosterFight> list = ListSF.GetRoster().GetSavedFights();
		bool result = false;
		FightIDS mOCEDDJOAEB = new FightIDS();
		mOCEDDJOAEB.SetFightIDSByString(_name);
		for (int i = 0; i < list.Count; i++)
		{
			RosterFight pIGKOIFBOME = list[i];
			if (mOCEDDJOAEB.Equals(pIGKOIFBOME.GetFightIdString()))
			{
				result = true;
			}
		}
		return result;
	}
}
