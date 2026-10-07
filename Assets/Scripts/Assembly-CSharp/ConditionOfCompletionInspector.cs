using System.Collections.Generic;

public class ConditionOfCompletionInspector
{
	private List<ConditionOfCompletion> _conditions = new List<ConditionOfCompletion>();

	public bool AreAllComplete(FightIDS fightId)
	{
		for (int i = 0; i < _conditions.Count; i++)
		{
			if (!_conditions[i].IsComplete(fightId))
			{
				return false;
			}
		}
		return true;
	}

	public void AddCondition(ConditionOfCompletion condition)
	{
		_conditions.Add(condition);
	}

	public void AddConditions(List<ConditionOfCompletion> conditions)
	{
		_conditions.AddRange(conditions);
	}
}
