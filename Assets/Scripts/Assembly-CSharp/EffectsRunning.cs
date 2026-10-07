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

	private void StopEffect(string name, Model owner)
	{
		int i = 0;
		for (int count = runningEffects.Count; i < count; i++)
		{
			CurrentEffect currentEffect = runningEffects[i];
			if (owner == currentEffect.Owner && (name == currentEffect.Effect.get_Name() || name == string.Empty))
			{
				RemoveEffect(currentEffect, i);
				break;
			}
		}
	}

	private void RemoveEffect(CurrentEffect currentEffect, int index)
	{
		Object.Destroy(currentEffect.EffectObject);
		if (currentEffect.Owner != null)
		{
			currentEffect.Owner.RemoveCurrentEffect(currentEffect);
		}
		runningEffects.RemoveAt(index);
	}

	private void stopFollowEffect(string name, Model owner)
	{
		int i = 0;
		for (int count = runningEffects.Count; i < count; i++)
		{
			CurrentEffect currentEffect = runningEffects[i];
			if (owner == currentEffect.Owner && name == currentEffect.Effect.get_Name())
			{
				currentEffect.stopFollowEffect = true;
				break;
			}
		}
	}

	public void StartEffect(ActionEffect action, Model owner)
	{
		ModelConditions conditions = owner.GetConditions();
		conditions.AnimationSign = owner.GetAnimationModule().GetSign();
		Vector3f effectPosition = Vector3f.op_Implicit(action.GetPosition().GetPosition(conditions));
		// Effects are presentation; a rollback re-simulation does not spawn them twice.
		if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
		{
			return;
		}
		GameObject gameObject = new GameObject(action.get_Name());
		gameObject.transform.localPosition = new Vector3(effectPosition.GetX(), effectPosition.GetY(), effectPosition.GetZ());
		Quaternion attachmentRotation = Quaternion.identity;
		if (action.Attachment != null)
		{
			Vector3 attachmentPosition;
			if (!action.Attachment.TryGetTransform(owner, out attachmentPosition, out attachmentRotation))
			{
				Debug.LogWarning("[EffectAttach] Missing live anchor for " + action.get_Name() + " on " + owner.get_Name());
				Object.Destroy(gameObject);
				return;
			}
			gameObject.transform.localPosition = attachmentPosition;
		}
		if (action.GetIsOnBackground())
			gameObject.transform.localPosition += new Vector3(0f, 0f, 0.1f);
		Vector3 localScale = new Vector3((float)conditions.AnimationSign * action.GetScaleX(), 0f - action.GetScaleY(), 1f);
		gameObject.transform.localScale = localScale;
		gameObject.transform.localRotation = action.Attachment == null
			? Quaternion.Euler(0f, 0f, action.GetStartRotation()) : attachmentRotation;
		gameObject.transform.SetParent(_UnityObject.transform, false);
		float changeSpriteTime = action.GetTimeScale() / 60f;
		string texturePath = "Textures/Effects/Magic/" + action.GetSequence();
		CocosAnimation cocosAnimation = gameObject.AddComponent<CocosAnimation>();
		bool effectLoaded = cocosAnimation.Init(texturePath, true);
		// The recovered -10 background order puts effects behind every location
		// sprite in Unity. Keep their order with the arena and use model depth;
		// an authored positive Priority (vanilla OrbOfHungerEffect1) still applies.
		cocosAnimation.SetSortingOrder(action.GetIsOnBackground() ? Mathf.Max(0, action.GetPriority()) : action.GetPriority());
		if (!effectLoaded)
		{
			GameLog.Write("Effect NO " + texturePath);
		}
		if (action.GetIsLooped())
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
		CurrentEffect currentEffect = new CurrentEffect(owner, action, gameObject, cocosAnimation);
		runningEffects.Add(currentEffect);
		owner.AddCurrentEffect(currentEffect);
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
			CurrentEffect currentEffect = runningEffects[i];
			if (currentEffect.EffectObject == null)
			{
				// Destroyed by a rollback: drop it from the running list.
				RemoveEffect(currentEffect, i);
				num2--;
				i--;
				continue;
			}
			if (!currentEffect.Animation.get_IsWork() || (currentEffect.Effect.GetIsFollowObject() && currentEffect.Owner == null && !currentEffect.stopFollowEffect))
			{
				StopEffect(currentEffect.Effect.get_Name(), currentEffect.Owner);
				num2--;
				i--;
				continue;
			}
			if (currentEffect.Effect.GetIsFollowObject() && !currentEffect.stopFollowEffect)
			{
				currentEffect.UpdateFollow();
			}
			currentEffect.Animation.Render(1f / 60f * num);
		}
	}

	public void StopEffect(ActionStopEffect action, Model owner)
	{
		StopEffect(action.get_Name(), owner);
	}

	public void stopFollowEffect(ActionStopFollowEffect action, Model owner)
	{
		stopFollowEffect(action.get_Name(), owner);
	}

	public void RemoveAllEffects()
	{
		while (runningEffects.Count > 0)
		{
			RemoveEffect(runningEffects[0], 0);
		}
	}
}
