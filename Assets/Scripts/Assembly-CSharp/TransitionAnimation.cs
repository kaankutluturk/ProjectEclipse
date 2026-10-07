using System.Collections.Generic;

public class TransitionAnimation
{
	private List<ConditionAnimation> _conditions = new List<ConditionAnimation>();

	public bool IsFrameShift;

	public int FrameShift;

	public List<ConditionAnimation> Conditions
	{
		set
		{
			SetConditions(value);
		}
	}

	public TransitionAnimation()
	{
		IsFrameShift = false;
		FrameShift = 0;
	}

	public void SetConditions(List<ConditionAnimation> value)
	{
		_conditions = value;
	}

	public void AddCondition(ConditionAnimation IOFGGOCEIAM)
	{
		_conditions.Add(IOFGGOCEIAM);
	}

	public bool AreConditionsMet(ModelConditions conditions)
	{
		for (int i = 0; i < _conditions.Count; i++)
		{
			if (!_conditions[i].IsEqual(conditions))
			{
				return false;
			}
		}
		return true;
	}
}
