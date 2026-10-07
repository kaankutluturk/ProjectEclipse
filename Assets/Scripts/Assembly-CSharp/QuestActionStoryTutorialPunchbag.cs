using System.Collections;
using Nekki.SF2.GUI.Menu;
using UnityEngine;

public class QuestActionStoryTutorialPunchbag : QuestAction
{
	private int attackCount;

	private int requiredAttackCount = 3;

	private IEnumerator _WaitTimeCoroutine;

	private bool _LastAnimationIsKick;

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		MainMenu.get_Instance().SetEnabled(false);
		Fight gDBOMJODDEA = Fight.GetCurrentFight();
		SFButton buttonPunch = gDBOMJODDEA.Controller.GetButtonPunch();
		buttonPunch.AddFlashImage("FightButtons.Kick_Highlight");
		buttonPunch.FlashingImage.rectTransform.localScale = new Vector3(1.33f, 1.33f);
		buttonPunch.set_IsFlashing(true);
		SFButton buttonKick = gDBOMJODDEA.Controller.GetButtonKick();
		buttonKick.AddFlashImage("FightButtons.Kick_Highlight");
		buttonKick.FlashingImage.rectTransform.localScale = new Vector3(1.33f, 1.33f);
		buttonKick.set_IsFlashing(true);
		Model fGCODGKLHED = gDBOMJODDEA.ActiveModels[0];
		fGCODGKLHED.AddEventListener(2, OnAnimationStart);
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
		Fight gDBOMJODDEA = Fight.GetCurrentFight();
		Model fGCODGKLHED = gDBOMJODDEA.ActiveModels[0];
		InfoAnimation.AnimationKind dFLPNNBIFFN = fGCODGKLHED.LastAnimationType;
		if (dFLPNNBIFFN == InfoAnimation.AnimationKind.AnimationAttack)
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
		Fight gDBOMJODDEA = Fight.GetCurrentFight();
		SFButton buttonPunch = gDBOMJODDEA.Controller.GetButtonPunch();
		buttonPunch.set_IsFlashing(false);
		SFButton buttonKick = gDBOMJODDEA.Controller.GetButtonKick();
		buttonKick.set_IsFlashing(false);
		Model fGCODGKLHED = gDBOMJODDEA.ActiveModels[0];
		fGCODGKLHED.RemoveEventListener(2, OnAnimationStart);
		FinishAction();
	}
}
