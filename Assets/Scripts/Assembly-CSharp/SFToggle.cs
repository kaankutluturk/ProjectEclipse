using System;
using Nekki.SF2.GUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SFToggle : Toggle, global::IEventDispatcher<object>
{
	public enum ToggleEvent
	{
		OnPress = 0,
		OnRelease = 1,
		OnClick = 2,
		OnDoubleClick = 3,
		OnActiveOpacity = 4,
		OnTouchBegin = 5
	}

	[SerializeField]
	public ResolutionImage FlashingImage;

	private bool flashFadingIn;

	private int flashAlpha;

	private int flashAlphaStep = 10;

	private bool isFlashing;

	private global::EventDispatcher<object> eventDispatcher = new global::EventDispatcher<object>();

	public int ButtonId = -1;

	public bool Flashing
	{
		get
		{
			return get_IsFlashing();
		}
		set
		{
			set_IsFlashing(value);
		}
	}

	public bool get_IsFlashing()
	{
		return isFlashing;
	}

	public void set_IsFlashing(bool value)
	{
		isFlashing = value;
		if (FlashingImage != null)
		{
			FlashingImage.gameObject.SetActive(isFlashing);
		}
	}

	public int AddEventListener(int name, Action<object> ODDEOFKLIAG)
	{
		return eventDispatcher.AddEventListener(name, ODDEOFKLIAG);
	}

	public int CallEvent(int name, object EHCLMBADLKH)
	{
		return eventDispatcher.CallEvent(name, EHCLMBADLKH);
	}

	public int RemoveAllEventListener()
	{
		return eventDispatcher.RemoveAllEventListener();
	}

	public int RemoveEvent(int name)
	{
		return eventDispatcher.RemoveEvent(name);
	}

	public int RemoveEventListener(int name, Action<object> ODDEOFKLIAG)
	{
		return eventDispatcher.RemoveEventListener(name, ODDEOFKLIAG);
	}

	public override void OnPointerDown(PointerEventData BHOLFGOGPCP)
	{
		base.OnPointerDown(BHOLFGOGPCP);
		CallEvent(0, ButtonId);
	}

	public override void OnPointerUp(PointerEventData BHOLFGOGPCP)
	{
		base.OnPointerUp(BHOLFGOGPCP);
		CallEvent(1, ButtonId);
	}

	public override void OnPointerClick(PointerEventData BHOLFGOGPCP)
	{
		base.OnPointerClick(BHOLFGOGPCP);
		CallEvent(2, ButtonId);
	}

	private void Update()
	{
		if (!isFlashing || !(FlashingImage != null))
		{
			return;
		}
		FlashingImage.color = new Color(FlashingImage.color.r, FlashingImage.color.g, FlashingImage.color.b, (float)flashAlpha / 255f);
		if (flashFadingIn)
		{
			if (flashAlpha < 250)
			{
				flashAlpha += flashAlphaStep;
				return;
			}
			flashFadingIn = false;
			if (flashAlpha > 250)
			{
				flashAlpha = 250;
			}
		}
		else if (flashAlpha > 0)
		{
			flashAlpha -= flashAlphaStep;
		}
		else
		{
			flashFadingIn = true;
			if (flashAlpha < 0)
			{
				flashAlpha = 0;
			}
		}
	}

	public void AddFlashImage(string JGIGOMLGLPN)
	{
		GameObject gameObject = new GameObject();
		gameObject.name = "FlashingImage";
		gameObject.transform.SetParent(base.transform, false);
		FlashingImage = gameObject.AddComponent<ResolutionImage>();
		FlashingImage.set_SpriteName(JGIGOMLGLPN);

	}
}
