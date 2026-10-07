using System.Collections.Generic;
using UnityEngine;

public class EffectsContainer
{
	private GameObject _UnityObject;

	private List<Model> _models = new List<Model>();

	private EffectsRunning effectsRunning;

	public GameObject UnityObject
	{
		get
		{
			return GetUnityObject();
		}
	}

	public EffectsRunning RunningEffects
	{
		get
		{
			return GetEffectsRunning();
		}
	}

	public EffectsContainer()
	{
		effectsRunning = null;
		_UnityObject = new GameObject("EffectsContainer");
	}

	public GameObject GetUnityObject()
	{
		return _UnityObject;
	}

	public EffectsRunning GetEffectsRunning()
	{
		return effectsRunning;
	}

	public void init(float GBNPHCHGKDO)
	{
		EnsureRunning();
	}

	public void EnsureRunning()
	{
		if (effectsRunning == null)
		{
			effectsRunning = new EffectsRunning();
			effectsRunning.GetUnityObject().transform.SetParent(_UnityObject.transform, false);
		}
	}

	public void ClearModels()
	{
		_models.Clear();
	}

	public void UpdateEffects()
	{
		effectsRunning.UpdateEffects();
	}

	public void StartEffect(ActionEffect IBODMPMJELJ)
	{
		effectsRunning.StartEffect(IBODMPMJELJ, IBODMPMJELJ.get_Model());
	}

	public void StopEffect(ActionStopEffect IBODMPMJELJ)
	{
		effectsRunning.StopEffect(IBODMPMJELJ, IBODMPMJELJ.get_Model());
	}

	public void StopFollowEffect(ActionStopFollowEffect IBODMPMJELJ)
	{
		effectsRunning.stopFollowEffect(IBODMPMJELJ, IBODMPMJELJ.get_Model());
	}

	public void RemoveAllEffects()
	{
		effectsRunning.RemoveAllEffects();
	}
}
