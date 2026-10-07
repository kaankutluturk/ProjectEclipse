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

	public void AddAction(QuestAction action)
	{
		actions.Add(action);
	}

	public void Run(QuestParameters questParameters)
	{
		CallEvent(0, questParameters);
		this.parameters = questParameters;
		int count = actions.Count;
		if (count > 0)
		{
			if (currentIndex < count)
			{
				QuestAction action = actions[currentIndex];
				action.AddEventListener(1, actionCompleteHandler);
				action.Execute(questParameters);
			}
		}
		else
		{
			CallEvent(1, questParameters);
		}
	}

	public void OnActionComplete(object data)
	{
		if (data != null)
		{
			parameters = (QuestParameters)data;
		}
		QuestAction completedAction = actions[currentIndex];
		completedAction.RemoveEventListener(1, actionCompleteHandler);
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
