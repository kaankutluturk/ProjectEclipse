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
		ActionEffect effect = (ActionEffect)data;
		if (effect.GetIsOnBackground())
		{
			backgroundEffects.StartEffect(effect);
		}
		else
		{
			foregroundEffects.StartEffect(effect);
		}
	}

	private void OnEffectStopped(object data)
	{
		ActionStopEffect stopEffect = (ActionStopEffect)data;
		backgroundEffects.StopEffect(stopEffect);
		foregroundEffects.StopEffect(stopEffect);
	}

	private void OnEffectFollowStopped(object data)
	{
		ActionStopFollowEffect stopFollowEffect = (ActionStopFollowEffect)data;
		backgroundEffects.StopFollowEffect(stopFollowEffect);
		foregroundEffects.StopFollowEffect(stopFollowEffect);
	}

	public void Init(Location location)
	{
		CreateViewerModel(location.floorHeight);
		CreateEffectContainers(location.floorHeight);
		_UnityObject.transform.localPosition = new Vector3((0f - location.width) / 2f, (0f - location.height) / 2f + location.floorHeight, 0f);
		_UnityObject.transform.SetParent(location.gameLayer.GetLayerObject().transform, false);
		Vector3 localScale = _UnityObject.transform.localScale;
		_UnityObject.transform.localScale = new Vector3(localScale.x, localScale.y * -1f, localScale.z);
	}

	public void CreateViewerModel(float floorHeight)
	{
		viewerModel.Init(floorHeight);
		viewerModel.GetRootObject().transform.SetParent(_UnityObject.transform, false);
		viewerModel.GetRootObject().transform.localPosition = new Vector3(0f, 0f, 0f);
	}

	public void CreateEffectContainers(float floorHeight)
	{
		backgroundEffects = new EffectsContainer();
		backgroundEffects.init(floorHeight);
		backgroundEffects.GetUnityObject().transform.SetParent(_UnityObject.transform, false);
		backgroundEffects.GetUnityObject().transform.localPosition = new Vector3(0f, 0f, 0.01f);
		foregroundEffects = new EffectsContainer();
		foregroundEffects.init(floorHeight);
		foregroundEffects.GetUnityObject().transform.SetParent(_UnityObject.transform, false);
		foregroundEffects.GetUnityObject().transform.localPosition = new Vector3(0f, 0f, -0.01f);
	}

	public void Clear()
	{
		viewerModel.Clear();
		backgroundEffects.ClearModels();
		foregroundEffects.ClearModels();
	}

	public void AttachModelEffects(Model model)
	{
		model.AddEventListener(7, OnEffectStarted);
		model.AddEventListener(8, OnEffectStopped);
		model.AddEventListener(9, OnEffectFollowStopped);
	}

	public void DetachModelEffects(Model model)
	{
		model.RemoveEventListener(7, OnEffectStarted);
		model.RemoveEventListener(8, OnEffectStopped);
		model.RemoveEventListener(9, OnEffectFollowStopped);
	}

	public void UpdateEffects()
	{
		backgroundEffects.RemoveAllEffects();
		foregroundEffects.RemoveAllEffects();
	}
}
