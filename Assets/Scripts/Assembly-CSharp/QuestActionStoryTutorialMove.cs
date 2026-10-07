using System.Collections;
using Nekki.SF2.Core.Fights.Controller;
using Nekki.SF2.GUI.Menu;
using UnityEngine;

public class QuestActionStoryTutorialMove : QuestAction
{
	private int moveCount;

	private int requiredMoveCount = 3;

	private IEnumerator _WaitTimeCoroutine;

	private bool _LastAnimationIsMove;

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		MainMenu.get_Instance().SetEnabled(false);
		Fight gDBOMJODDEA = Fight.GetCurrentFight();
		Stick joystick = gDBOMJODDEA.Controller.GetJoystick();
		joystick.SetIsFlashing(true);
		Model fGCODGKLHED = gDBOMJODDEA.ActiveModels[0];
		fGCODGKLHED.AddEventListener(2, OnAnimationStart);
		_WaitTimeCoroutine = WaitForTimeout();
		CoroutineManager.get_Current().StartRoutine(_WaitTimeCoroutine);
	}

	private void OnAnimationStart(object data)
	{
		if (_LastAnimationIsMove)
		{
			_LastAnimationIsMove = false;
			moveCount++;
			if (moveCount >= requiredMoveCount)
			{
				CompleteStep();
			}
		}
		Fight gDBOMJODDEA = Fight.GetCurrentFight();
		Model fGCODGKLHED = gDBOMJODDEA.ActiveModels[0];
		InfoAnimation.AnimationKind dFLPNNBIFFN = fGCODGKLHED.LastAnimationType;
		if (dFLPNNBIFFN == InfoAnimation.AnimationKind.AnimationMove)
		{
			_LastAnimationIsMove = true;
		}
	}

	private IEnumerator WaitForTimeout()
	{
		yield return new WaitForSeconds(GameUtils.TutorialSettings.DefaultTutorialStepTimeout);
		CompleteStep();
	}

	private void CompleteStep()
	{
		if (_WaitTimeCoroutine != null)
		{
			CoroutineManager.get_Current().StopRoutine(_WaitTimeCoroutine);
		}
		MainMenu.get_Instance().SetEnabled(true);
		Fight gDBOMJODDEA = Fight.GetCurrentFight();
		Stick joystick = gDBOMJODDEA.Controller.GetJoystick();
		joystick.SetIsFlashing(false);
		Model fGCODGKLHED = gDBOMJODDEA.ActiveModels[0];
		fGCODGKLHED.RemoveEventListener(2, OnAnimationStart);
		FinishAction();
	}
}
