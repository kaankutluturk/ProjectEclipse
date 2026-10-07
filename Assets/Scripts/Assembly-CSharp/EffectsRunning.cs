using System.Collections.Generic;
using UnityEngine;

public class EffectsRunning
{
	private const string _Path = "Textures/Effects/Magic/";

	private GameObject _UnityObject;

	private List<CurrentEffect> runningEffects = new List<CurrentEffect>();

	public GameObject UnityObject
	{
		get
		{
			return GetUnityObject();
		}
	}

	public EffectsRunning()
	{
		_UnityObject = new GameObject("EffectsRunning");
	}

	public GameObject GetUnityObject()
	{
		return _UnityObject;
	}

	private void StopEffect(string name, Model ACENLMONNPA)
	{
		int i = 0;
		for (int count = runningEffects.Count; i < count; i++)
		{
			CurrentEffect bNGLFPIBAIM = runningEffects[i];
			if (ACENLMONNPA == bNGLFPIBAIM.Owner && (name == bNGLFPIBAIM.Effect.get_Name() || name == string.Empty))
			{
				RemoveEffect(bNGLFPIBAIM, i);
				break;
			}
		}
	}

	private void RemoveEffect(CurrentEffect LLOLBKJMKNC, int index)
	{
		Object.Destroy(LLOLBKJMKNC.EffectObject);
		if (LLOLBKJMKNC.Owner != null)
		{
			LLOLBKJMKNC.Owner.RemoveCurrentEffect(LLOLBKJMKNC);
		}
		runningEffects.RemoveAt(index);
	}

	private void stopFollowEffect(string name, Model ACENLMONNPA)
	{
		int i = 0;
		for (int count = runningEffects.Count; i < count; i++)
		{
			CurrentEffect bNGLFPIBAIM = runningEffects[i];
			if (ACENLMONNPA == bNGLFPIBAIM.Owner && name == bNGLFPIBAIM.Effect.get_Name())
			{
				bNGLFPIBAIM.stopFollowEffect = true;
				break;
			}
		}
	}

	public void StartEffect(ActionEffect IBODMPMJELJ, Model ACENLMONNPA)
	{
		ModelConditions dGJJDPIAEAO = ACENLMONNPA.GetConditions();
		dGJJDPIAEAO.AnimationSign = ACENLMONNPA.GetAnimationModule().GetSign();
		Vector3f eMAFACPEPDK = Vector3f.op_Implicit(IBODMPMJELJ.GetPosition().GetPosition(dGJJDPIAEAO));
		// Effects are presentation; a rollback re-simulation does not spawn them twice.
		if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
		{
			return;
		}
		GameObject gameObject = new GameObject(IBODMPMJELJ.get_Name());
		gameObject.transform.localPosition = new Vector3(eMAFACPEPDK.GetX(), eMAFACPEPDK.GetY(), eMAFACPEPDK.GetZ());
		Quaternion attachmentRotation = Quaternion.identity;
		if (IBODMPMJELJ.Attachment != null)
		{
			Vector3 attachmentPosition;
			if (!IBODMPMJELJ.Attachment.TryGetTransform(ACENLMONNPA, out attachmentPosition, out attachmentRotation))
			{
				Debug.LogWarning("[EffectAttach] Missing live anchor for " + IBODMPMJELJ.get_Name() + " on " + ACENLMONNPA.get_Name());
				Object.Destroy(gameObject);
				return;
			}
			gameObject.transform.localPosition = attachmentPosition;
		}
		if (IBODMPMJELJ.GetIsOnBackground())
			gameObject.transform.localPosition += new Vector3(0f, 0f, 0.1f);
		Vector3 localScale = new Vector3((float)dGJJDPIAEAO.AnimationSign * IBODMPMJELJ.GetScaleX(), 0f - IBODMPMJELJ.GetScaleY(), 1f);
		gameObject.transform.localScale = localScale;
		gameObject.transform.localRotation = IBODMPMJELJ.Attachment == null
			? Quaternion.Euler(0f, 0f, IBODMPMJELJ.GetStartRotation()) : attachmentRotation;
		gameObject.transform.SetParent(_UnityObject.transform, false);
		float changeSpriteTime = IBODMPMJELJ.GetTimeScale() / 60f;
		string oNNKJLOGHGH = "Textures/Effects/Magic/" + IBODMPMJELJ.GetSequence();
		CocosAnimation cocosAnimation = gameObject.AddComponent<CocosAnimation>();
		bool effectLoaded = cocosAnimation.Init(oNNKJLOGHGH, true);
		// The recovered -10 background order puts effects behind every location
		// sprite in Unity. Keep their order with the arena and use model depth;
		// an authored positive Priority (vanilla OrbOfHungerEffect1) still applies.
		cocosAnimation.SetSortingOrder(IBODMPMJELJ.GetIsOnBackground() ? Mathf.Max(0, IBODMPMJELJ.GetPriority()) : IBODMPMJELJ.GetPriority());
		if (!effectLoaded)
		{
			GameLog.Write("Effect NO " + oNNKJLOGHGH);
		}
		if (IBODMPMJELJ.GetIsLooped())
		{
			cocosAnimation.set_Iterations(-1);
		}
		else
		{
			cocosAnimation.set_Iterations(1);
		}
		cocosAnimation.SetFirstFrame();
		cocosAnimation.set_ChangeSpriteTime(changeSpriteTime);
		// A rollback that undoes this tick destroys the effect.
		Eclipse.Multiplayer.Rollback.RollbackObjects.Created(gameObject);
		CurrentEffect bNGLFPIBAIM = new CurrentEffect(ACENLMONNPA, IBODMPMJELJ, gameObject, cocosAnimation);
		runningEffects.Add(bNGLFPIBAIM);
		ACENLMONNPA.AddCurrentEffect(bNGLFPIBAIM);
	}

	public void UpdateEffects()
	{
		// Effect animations advance once per displayed tick, not per re-simulated one.
		if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
		{
			return;
		}
		float num = 1f / (float)GameUtils.GetSlowMode();
		int i = 0;
		for (int num2 = runningEffects.Count; i < num2; i++)
		{
			CurrentEffect bNGLFPIBAIM = runningEffects[i];
			if (bNGLFPIBAIM.EffectObject == null)
			{
				// Destroyed by a rollback: drop it from the running list.
				RemoveEffect(bNGLFPIBAIM, i);
				num2--;
				i--;
				continue;
			}
			if (!bNGLFPIBAIM.Animation.get_IsWork() || (bNGLFPIBAIM.Effect.GetIsFollowObject() && bNGLFPIBAIM.Owner == null && !bNGLFPIBAIM.stopFollowEffect))
			{
				StopEffect(bNGLFPIBAIM.Effect.get_Name(), bNGLFPIBAIM.Owner);
				num2--;
				i--;
				continue;
			}
			if (bNGLFPIBAIM.Effect.GetIsFollowObject() && !bNGLFPIBAIM.stopFollowEffect)
			{
				bNGLFPIBAIM.UpdateFollow();
			}
			bNGLFPIBAIM.Animation.Render(1f / 60f * num);
		}
	}

	public void StopEffect(ActionStopEffect IBODMPMJELJ, Model ACENLMONNPA)
	{
		StopEffect(IBODMPMJELJ.get_Name(), ACENLMONNPA);
	}

	public void stopFollowEffect(ActionStopFollowEffect IBODMPMJELJ, Model ACENLMONNPA)
	{
		stopFollowEffect(IBODMPMJELJ.get_Name(), ACENLMONNPA);
	}

	public void RemoveAllEffects()
	{
		while (runningEffects.Count > 0)
		{
			RemoveEffect(runningEffects[0], 0);
		}
	}
}
