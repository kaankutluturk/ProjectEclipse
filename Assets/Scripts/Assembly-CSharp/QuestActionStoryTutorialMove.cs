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

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		MainMenu.get_Instance().SetEnabled(false);
		Fight fight = Fight.GetCurrentFight();
		Stick joystick = fight.Controller.GetJoystick();
		joystick.SetIsFlashing(true);
		Model playerModel = fight.ActiveModels[0];
		playerModel.AddEventListener(2, OnAnimationStart);
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
		Fight fight = Fight.GetCurrentFight();
		Model playerModel = fight.ActiveModels[0];
		InfoAnimation.AnimationKind animationKind = playerModel.LastAnimationType;
		if (animationKind == InfoAnimation.AnimationKind.AnimationMove)
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
		Fight fight = Fight.GetCurrentFight();
		Stick joystick = fight.Controller.GetJoystick();
		joystick.SetIsFlashing(false);
		Model playerModel = fight.ActiveModels[0];
		playerModel.RemoveEventListener(2, OnAnimationStart);
		FinishAction();
	}
}
