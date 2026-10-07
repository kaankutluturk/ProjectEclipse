using System;
using System.Collections.Generic;
using UnityEngine;

public class LocationSelector
{
	public enum LayerType
	{
		LayerBG = 1,
		LayerGame = 2,
		LayerStatic = 3
	}

	private List<ChangingSprite> effects = new List<ChangingSprite>();

	private float unusedFloatA;

	private float unusedFloatB;

	private bool unusedFlag;

	private const float DepthStep = -0.01f;

	private float nextChildDepth;

	private GameObject _UnityObject;

	private bool Scaling;

	private int _type;

	private float factor;

	private string Atlas;

	public int layerDepth;

	private CocosAnimationData animationData;

	private List<GameObject> unusedObjects = new List<GameObject>();

	public GameObject LayerObject
	{
		get
		{
			return GetLayerObject();
		}
	}

	public bool ScalingEnabled
	{
		get
		{
			return GetScalingEnabled();
		}
		set
		{
			SetScalingEnabled(value);
		}
	}

	public float Factor
	{
		get
		{
			return GetFactor();
		}
		set
		{
			SetFactor(value);
		}
	}

	public string AtlasName
	{
		get
		{
			return GetAtlasName();
		}
		set
		{
			SetAtlasName(value);
		}
	}

	public CocosAnimationData CocosAnimationData
	{
		get
		{
			return GetCocosAnimationData();
		}
		set
		{
			SetCocosAnimationData(value);
		}
	}

	public bool IsGameLayer
	{
		get
		{
			return GetIsGameLayer();
		}
	}

	public LocationSelector(int depth)
	{
		layerDepth = depth;
		_UnityObject = new GameObject("Layer");
		_UnityObject.transform.localPosition = new Vector3(0f, 0f, depth);
		_UnityObject.transform.localScale = new Vector3(1f, 1f, 1f);
	}

	public GameObject GetLayerObject()
	{
		return _UnityObject;
	}

	public bool GetScalingEnabled()
	{
		return Scaling;
	}

	public void SetScalingEnabled(bool value)
	{
		Scaling = value;
	}

	public int get_Type()
	{
		return _type;
	}

	public void set_Type(int value)
	{
		_type = value;
	}

	public float GetFactor()
	{
		return factor;
	}

	public void SetFactor(float value)
	{
		factor = value;
	}

	public string GetAtlasName()
	{
		return Atlas;
	}

	public void SetAtlasName(string value)
	{
		Atlas = value;
	}

	public CocosAnimationData GetCocosAnimationData()
	{
		return animationData;
	}

	public void SetCocosAnimationData(CocosAnimationData value)
	{
		animationData = value;
	}

	public bool GetIsGameLayer()
	{
		return _type == 2;
	}

	public void AddImage(GameObject image, int index)
	{
		image.transform.SetParent(_UnityObject.transform, false);
		Vector3 localPosition = image.transform.localPosition;
		localPosition.z = nextChildDepth;
		image.transform.localPosition = localPosition;
		nextChildDepth += -0.01f;
	}

	public void AddSimpleEffect(ChangingSprite effect, int index)
	{
		if (!(effect.SpriteObject == null))
		{
			effect.SpriteObject.transform.SetParent(_UnityObject.transform, false);
			effects.Add(effect);
			Vector3 localPosition = effect.SpriteObject.transform.localPosition;
			localPosition.z = nextChildDepth;
			effect.SpriteObject.transform.localPosition = localPosition;
			nextChildDepth += -0.01f;
		}
	}

	public void AddParticleEffect(ChangingSprite effect, int index)
	{
		effect.Particles.transform.SetParent(_UnityObject.transform, false);
		effects.Add(effect);
		Vector3 localPosition = effect.Particles.transform.localPosition;
		localPosition.z = nextChildDepth;
		effect.Particles.transform.localPosition = localPosition;
		nextChildDepth += -0.099999994f;
	}

	public void AddChangingSprite(ChangingSprite effect, int index)
	{
		effect.SpriteObject.transform.SetParent(_UnityObject.transform, false);
		effects.Add(effect);
		Vector3 localPosition = effect.SpriteObject.transform.localPosition;
		localPosition.z = nextChildDepth;
		effect.SpriteObject.transform.localPosition = localPosition;
		nextChildDepth += -0.01f;
	}

	public void RemoveChangingSprite(ChangingSprite effect)
	{
		if (effect != null)
		{
			effects.Remove(effect);
			UnityEngine.Object.Destroy(effect.SpriteObject.gameObject);
		}
	}

	public void Render()
	{
		int i = 0;
		for (int count = effects.Count; i < count; i++)
		{
			effects[i].Render(1f / (float)GameUtils.GetSlowMode());
		}
	}

	public void SetScale(float scale)
	{
		_UnityObject.transform.localScale = new Vector3(scale, scale, 1f);
	}

	public void SetPositionX(float positionX)
	{
		Vector3 localPosition = _UnityObject.transform.localPosition;
		localPosition.x = (float)Math.Round(positionX, 2, MidpointRounding.AwayFromZero);
		_UnityObject.transform.localPosition = localPosition;
	}

	public void SetPositionY(float positionY)
	{
		Vector3 localPosition = _UnityObject.transform.localPosition;
		localPosition.y = (float)Math.Round(positionY, 2, MidpointRounding.AwayFromZero);
		_UnityObject.transform.localPosition = localPosition;
	}
}
