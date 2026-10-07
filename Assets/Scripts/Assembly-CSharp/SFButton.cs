using System;
using Nekki.SF2.GUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SFButton : Button, global::IEventDispatcher<object>
{
	public enum ButtonEvent
	{
		OnPress = 0,
		OnRelease = 1,
		OnClick = 2,
		OnDoubleClick = 3,
		OnActiveOpacity = 4,
		OnTouchBegin = 5
	}

	private global::EventDispatcher<object> eventDispatcher = new global::EventDispatcher<object>();

	[SerializeField]
	public ResolutionImage FlashingImage;

	private bool flashFadingIn;

	private int flashAlpha;

	private int flashAlphaStep = 10;

	private bool isFlashing;

	public bool IsOneShot;

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

	protected override void Awake()
	{
		base.Awake();
		Eclipse.UI.ControlTexturePacks.ApplyToSpriteSwap(this);
		base.onClick.AddListener(() =>
		{
			if (IsOneShot)
			{
				base.interactable = false;
			}
		});
	}

	private new void OnDestroy()
	{
		if (base.onClick != null)
		{
			base.onClick.RemoveAllListeners();
		}
	}

	public int AddEventListener(int name, Action<object> ODDEOFKLIAG)
	{
		return eventDispatcher.AddEventListener(name, ODDEOFKLIAG);
	}

	public int CallEvent(int name, object EHCLMBADLKH)
	{
		return (!base.interactable) ? 1 : eventDispatcher.CallEvent(name, EHCLMBADLKH);
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

    private SelectionState _inputVisualState;
    private bool _hasInputVisualState;

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);
        _inputVisualState = state;
        _hasInputVisualState = true;
    }

    public void SetInputPressedVisual(bool pressed)
    {
        var state = !interactable ? SelectionState.Disabled :
            pressed ? SelectionState.Pressed : SelectionState.Normal;
        // Input is sampled on both rendering and simulation frames. Repeating a
        // transition restarts UI tweens/animators even when the button is unchanged.
        // Track pointer-driven transitions too, so releasing touch while a key is
        // held still reapplies the keyboard's pressed visual.
        if (!_hasInputVisualState || state != _inputVisualState) DoStateTransition(state, true);
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
		FlashingImage.SetNativeSize();
	}
}
