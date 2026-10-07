using UnityEngine;

public class RenderContainer
{
	public enum RenderContainerZLayer
	{
		Z_EFFECTS_CONTAINER_BACKGROUND = 0,
		Z_VIEWER_MODEL = 1,
		Z_EFFECTS_CONTAINER_FRONT = 2
	}

	private GameObject _UnityObject;

	private ViewerModel viewerModel;

	private EffectsContainer backgroundEffects;

	private EffectsContainer foregroundEffects;

	public GameObject RootObject
	{
		get
		{
			return GetRootObject();
		}
	}

	public ViewerModel ModelViewer
	{
		get
		{
			return GetViewerModel();
		}
	}

	public EffectsContainer BackgroundEffects
	{
		get
		{
			return GetBackgroundEffects();
		}
	}

	public EffectsContainer ForegroundEffects
	{
		get
		{
			return GetForegroundEffects();
		}
	}

	public RenderContainer()
	{
		_UnityObject = new GameObject("RenderContainer");
		viewerModel = new ViewerModel();
	}

	public GameObject GetRootObject()
	{
		return _UnityObject;
	}

	public ViewerModel GetViewerModel()
	{
		return viewerModel;
	}

	public EffectsContainer GetBackgroundEffects()
	{
		return backgroundEffects;
	}

	public EffectsContainer GetForegroundEffects()
	{
		return foregroundEffects;
	}

	private void OnEffectStarted(object data)
	{
		ActionEffect jFJGGMEJDPG = (ActionEffect)data;
		if (jFJGGMEJDPG.GetIsOnBackground())
		{
			backgroundEffects.StartEffect(jFJGGMEJDPG);
		}
		else
		{
			foregroundEffects.StartEffect(jFJGGMEJDPG);
		}
	}

	private void OnEffectStopped(object data)
	{
		ActionStopEffect iBODMPMJELJ = (ActionStopEffect)data;
		backgroundEffects.StopEffect(iBODMPMJELJ);
		foregroundEffects.StopEffect(iBODMPMJELJ);
	}

	private void OnEffectFollowStopped(object data)
	{
		ActionStopFollowEffect iBODMPMJELJ = (ActionStopFollowEffect)data;
		backgroundEffects.StopFollowEffect(iBODMPMJELJ);
		foregroundEffects.StopFollowEffect(iBODMPMJELJ);
	}

	public void Init(Location LPJNEDFCBOI)
	{
		CreateViewerModel(LPJNEDFCBOI.floorHeight);
		CreateEffectContainers(LPJNEDFCBOI.floorHeight);
		_UnityObject.transform.localPosition = new Vector3((0f - LPJNEDFCBOI.width) / 2f, (0f - LPJNEDFCBOI.height) / 2f + LPJNEDFCBOI.floorHeight, 0f);
		_UnityObject.transform.SetParent(LPJNEDFCBOI.gameLayer.GetLayerObject().transform, false);
		Vector3 localScale = _UnityObject.transform.localScale;
		_UnityObject.transform.localScale = new Vector3(localScale.x, localScale.y * -1f, localScale.z);
	}

	public void CreateViewerModel(float GBNPHCHGKDO)
	{
		viewerModel.Init(GBNPHCHGKDO);
		viewerModel.GetRootObject().transform.SetParent(_UnityObject.transform, false);
		viewerModel.GetRootObject().transform.localPosition = new Vector3(0f, 0f, 0f);
	}

	public void CreateEffectContainers(float GBNPHCHGKDO)
	{
		backgroundEffects = new EffectsContainer();
		backgroundEffects.init(GBNPHCHGKDO);
		backgroundEffects.GetUnityObject().transform.SetParent(_UnityObject.transform, false);
		backgroundEffects.GetUnityObject().transform.localPosition = new Vector3(0f, 0f, 0.01f);
		foregroundEffects = new EffectsContainer();
		foregroundEffects.init(GBNPHCHGKDO);
		foregroundEffects.GetUnityObject().transform.SetParent(_UnityObject.transform, false);
		foregroundEffects.GetUnityObject().transform.localPosition = new Vector3(0f, 0f, -0.01f);
	}

	public void Clear()
	{
		viewerModel.Clear();
		backgroundEffects.ClearModels();
		foregroundEffects.ClearModels();
	}

	public void AttachModelEffects(Model ACENLMONNPA)
	{
		ACENLMONNPA.AddEventListener(7, OnEffectStarted);
		ACENLMONNPA.AddEventListener(8, OnEffectStopped);
		ACENLMONNPA.AddEventListener(9, OnEffectFollowStopped);
	}

	public void DetachModelEffects(Model ACENLMONNPA)
	{
		ACENLMONNPA.RemoveEventListener(7, OnEffectStarted);
		ACENLMONNPA.RemoveEventListener(8, OnEffectStopped);
		ACENLMONNPA.RemoveEventListener(9, OnEffectFollowStopped);
	}

	public void UpdateEffects()
	{
		backgroundEffects.RemoveAllEffects();
		foregroundEffects.RemoveAllEffects();
	}
}
