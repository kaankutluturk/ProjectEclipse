using System.Collections.Generic;
using Nekki.SF2.GUI;

namespace Nekki.SF2.Core.Tutorials
{
	public class TutorialRaid : SFMonoBehaviour<object>
	{
		public enum TutorialRaidEvent
		{
			ON_TUTORIAL_RAID_COMPLETE = 0
		}

		private List<TutorialAction> actions = new List<TutorialAction>();

		private int currentActionIndex = -1;

		private bool advancePending;

		public TutorialRaid()
		{
			Init();
		}

		public void Run()
		{
			advancePending = true;
		}

		public virtual bool Init()
		{
			return true;
		}

		public void Clear()
		{
			foreach (TutorialAction item in actions)
			{
			}
			actions.Clear();
		}

		public virtual void Draw()
		{
			if (advancePending)
			{
				advancePending = false;
				currentActionIndex++;
				if (actions.Count - 1 >= currentActionIndex)
				{
					actions[currentActionIndex].Run();
				}
				else
				{
					CompleteTutorial();
				}
			}
		}

		private void AddAction(TutorialAction action)
		{
			action.AddEventListener(0, OnActionComplete);
			actions.Add(action);
		}

		private void OnActionComplete(object data)
		{
			if (data == null)
			{
				advancePending = true;
			}
			else
			{
				CompleteTutorial();
			}
		}

		private void CompleteTutorial()
		{
			CallEvent(0, null);
		}
	}
}
