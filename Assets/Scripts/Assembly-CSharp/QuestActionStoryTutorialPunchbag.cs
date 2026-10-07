using System.Collections;
using Nekki.SF2.GUI.Menu;
using UnityEngine;

public class QuestActionStoryTutorialPunchbag : QuestAction
{
	private int attackCount;

	private int requiredAttackCount = 3;

	private IEnumerator _WaitTimeCoroutine;

	private bool _LastAnimationIsKick;

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		MainMenu.get_Instance().SetEnabled(false);
		Fight fight = Fight.GetCurrentFight();
		SFButton buttonPunch = fight.Controller.GetButtonPunch();
		buttonPunch.AddFlashImage("FightButtons.Kick_Highlight");
		buttonPunch.FlashingImage.rectTransform.localScale = new Vector3(1.33f, 1.33f);
		buttonPunch.set_IsFlashing(true);
		SFButton buttonKick = fight.Controller.GetButtonKick();
		buttonKick.AddFlashImage("FightButtons.Kick_Highlight");
		buttonKick.FlashingImage.rectTransform.localScale = new Vector3(1.33f, 1.33f);
		buttonKick.set_IsFlashing(true);
		Model playerModel = fight.ActiveModels[0];
		playerModel.AddEventListener(2, OnAnimationStart);
		_WaitTimeCoroutine = WaitForTimeout();
		CoroutineManager.get_Current().StartRoutine(_WaitTimeCoroutine);
	}

	private void OnAnimationStart(object data)
	{
		if (_LastAnimationIsKick)
		{
			_LastAnimationIsKick = false;
			attackCount++;
			if (attackCount >= requiredAttackCount)
			{
				CompleteStep();
			}
		}
		Fight fight = Fight.GetCurrentFight();
		Model playerModel = fight.ActiveModels[0];
		InfoAnimation.AnimationKind animationKind = playerModel.LastAnimationType;
		if (animationKind == InfoAnimation.AnimationKind.AnimationAttack)
		{
			_LastAnimationIsKick = true;
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
		SFButton buttonPunch = fight.Controller.GetButtonPunch();
		buttonPunch.set_IsFlashing(false);
		SFButton buttonKick = fight.Controller.GetButtonKick();
		buttonKick.set_IsFlashing(false);
		Model playerModel = fight.ActiveModels[0];
		playerModel.RemoveEventListener(2, OnAnimationStart);
		FinishAction();
	}
}
