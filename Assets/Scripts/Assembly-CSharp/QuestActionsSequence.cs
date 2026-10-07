using System;
using System.Collections.Generic;

public class QuestActionsSequence : global::EventDispatcher<object>
{
	public enum SequenceEvent
	{
		onRun = 0,
		onComplete = 1
	}

	private Action<object> actionCompleteHandler;

	public int currentIndex;

	public QuestParameters parameters;

	public List<QuestAction> actions;

	public QuestActionsSequence()
	{
		actionCompleteHandler = OnActionComplete;
		currentIndex = 0;
		parameters = null;
		actions = new List<QuestAction>();
	}

	public void AddAction(QuestAction IBODMPMJELJ)
	{
		actions.Add(IBODMPMJELJ);
	}

	public void Run(QuestParameters GFIHPBCEEOB)
	{
		CallEvent(0, GFIHPBCEEOB);
		this.parameters = GFIHPBCEEOB;
		int count = actions.Count;
		if (count > 0)
		{
			if (currentIndex < count)
			{
				QuestAction mBAAKHELFKL = actions[currentIndex];
				mBAAKHELFKL.AddEventListener(1, actionCompleteHandler);
				mBAAKHELFKL.Execute(GFIHPBCEEOB);
			}
		}
		else
		{
			CallEvent(1, GFIHPBCEEOB);
		}
	}

	public void OnActionComplete(object data)
	{
		if (data != null)
		{
			parameters = (QuestParameters)data;
		}
		QuestAction mBAAKHELFKL = actions[currentIndex];
		mBAAKHELFKL.RemoveEventListener(1, actionCompleteHandler);
		currentIndex++;
		if (currentIndex < actions.Count)
		{
			Run(parameters);
		}
		else
		{
			CallEvent(1, parameters);
		}
	}

	public void Reset()
	{
		currentIndex = 0;
		foreach (QuestAction item in actions)
		{
			item.ResetSequences();
		}
	}
}
