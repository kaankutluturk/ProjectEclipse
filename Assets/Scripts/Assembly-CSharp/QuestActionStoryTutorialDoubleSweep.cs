using System.Collections;
using Nekki.SF2.Core.Fights.Controller;
using Nekki.SF2.GUI.Menu;
using UnityEngine;

public class QuestActionStoryTutorialDoubleSweep : QuestAction
{
	private IEnumerator _WaitTimeCoroutine;

	private bool _LastAnimationIsDoubleSweep;

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		MainMenu.get_Instance().SetEnabled(false);
		Fight gDBOMJODDEA = Fight.GetCurrentFight();
		Model fGCODGKLHED = gDBOMJODDEA.ActiveModels[0];
		fGCODGKLHED.AddEventListener(2, OnAnimationStart);
		Stick joystick = gDBOMJODDEA.Controller.GetJoystick();
		joystick.SetIsFlashing(true);
		SFButton buttonKick = gDBOMJODDEA.Controller.GetButtonKick();
		buttonKick.AddFlashImage("FightButtons.Kick_Highlight");
		buttonKick.FlashingImage.rectTransform.localScale = new Vector3(1.33f, 1.33f);
		buttonKick.set_IsFlashing(true);
		_WaitTimeCoroutine = WaitForTimeout();
		CoroutineManager.get_Current().StartRoutine(_WaitTimeCoroutine);
	}

	private void OnAnimationStart(object data)
	{
		if (_LastAnimationIsDoubleSweep)
		{
			_LastAnimationIsDoubleSweep = false;
			CompleteStep();
		}
		Model.EventModel oJDOHGBGPFK = (Model.EventModel)data;
		InfoAnimation pJAHIOELGGD = (InfoAnimation)oJDOHGBGPFK.Data;
		if ("DoubleSweep" == pJAHIOELGGD.Name)
		{
			_LastAnimationIsDoubleSweep = true;
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
		Model fGCODGKLHED = gDBOMJODDEA.ActiveModels[0];
		fGCODGKLHED.RemoveEventListener(2, OnAnimationStart);
		Stick joystick = gDBOMJODDEA.Controller.GetJoystick();
		joystick.SetIsFlashing(false);
		SFButton buttonKick = gDBOMJODDEA.Controller.GetButtonKick();
		buttonKick.set_IsFlashing(false);
		FinishAction();
	}
}
