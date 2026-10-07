using System;
using Eclipse.Rendering.Interpolation;
using UnityEngine;

internal class BloodEffect
{
	private GameObject _UnityObject;

	public int index;

	public int LifetimeFrames;

	private Vector3f velocity = new Vector3f();

	private int frames;

	private FightTransformInterpolation _Interpolation;

	public BloodEffect(Vector3f impulse)
	{
		_UnityObject = new GameObject("BloodEffect");
		_Interpolation = _UnityObject.AddComponent<FightTransformInterpolation>();
		_Interpolation.Snap(Vector3.zero, Quaternion.identity);
		frames = 0;
		index = 0;
		LifetimeFrames = 0;
		int min = -40;
		int max = 40;
		int min2 = -60;
		int max2 = 20;
		float num = 200f;
		velocity.SetX(impulse.GetX() / num + (float)UnityEngine.Random.Range(min, max) / 10f);
		velocity.SetY(impulse.GetY() / num + (float)UnityEngine.Random.Range(min2, max2) / 10f);
	}

	public void CreateSprite(string spritePath, Color color)
	{
		SpriteRenderer spriteRenderer = _UnityObject.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = ResourcesAndBundles.Load<Sprite>(spritePath);
		spriteRenderer.color = color;
	}

	public void SetParent(GameObject parent)
	{
		_UnityObject.transform.SetParent(parent.transform, false);
	}

	public void Render()
	{
		Vector3 localPosition = _Interpolation.CurrentPosition;
		localPosition.x += velocity.GetX();
		localPosition.y += velocity.GetY();
		Vector3f currentVelocity = velocity;
		currentVelocity.SetY(currentVelocity.GetY() + 0.2f);
		int num = ((!(velocity.GetX() < 0f)) ? 1 : (-1));
		float z = Mathf.Atan((0f - velocity.GetY()) / velocity.GetX()) / (float)Math.PI * 180f - 90f * (float)num + 180f;
		Quaternion worldRotation = Quaternion.Euler(0f, 0f, z);
		Quaternion localRotation = (_UnityObject.transform.parent == null) ? worldRotation : Quaternion.Inverse(_UnityObject.transform.parent.rotation) * worldRotation;
		_Interpolation.Push(localPosition, localRotation);
	}

	public void SetPosition(Vector3f NAAPALOFBCI)
	{
		_Interpolation.Snap(new Vector3(NAAPALOFBCI.GetX(), NAAPALOFBCI.GetY(), 0f), _Interpolation.CurrentRotation);
	}

	public void SetScale(float scale)
	{
		_UnityObject.transform.localScale = new Vector3(scale, scale, scale);
	}

	public void Destroy()
	{
		UnityEngine.Object.Destroy(_UnityObject);
	}
}
