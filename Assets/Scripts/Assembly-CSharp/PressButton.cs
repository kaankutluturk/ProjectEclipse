using System;
using Nekki.SF2.GUI;
using UnityEngine;
using UnityEngine.UI;

public class PressButton : Button
{
	[Serializable]
	public enum TransitionType
	{
		SHOW_ONE_IMAGE = 0,
		SHOW_BOTH_IMAGE = 1
	}

	[SerializeField]
	private TransitionType transitionType;

	[SerializeField]
	private ResolutionImage normalImage;

	[SerializeField]
	private ResolutionImage pressImage;

	[SerializeField]
	private LayoutElement layoutElement;

	private RectTransform rectTransform;

	private RectTransform CachedRectTransform
	{
		get
		{
			return GetRectTransform();
		}
	}

	private RectTransform GetRectTransform()
	{
		if (rectTransform == null)
		{
			rectTransform = (RectTransform)base.transform;
		}
		return rectTransform;
	}

	public void SetImages(string normalImageName, string pressImageName, string texturePath = "")
	{
		if (normalImageName == null)
		{
			GameLog.Error("PressButton.Init normalImage is null");
			return;
		}
		if (pressImageName == null)
		{
			GameLog.Error("PressButton.Init pressImage is null");
			return;
		}
		if (layoutElement == null)
		{
			GameLog.Error("PressButton.Init layoutElement is null");
			return;
		}
		if (texturePath != string.Empty)
		{
			normalImage.set_TexturePath(texturePath);
			pressImage.set_TexturePath(texturePath);
		}
		normalImage.set_SpriteName(normalImageName);
		pressImage.set_SpriteName(pressImageName);
		normalImage.SetNativeSize();
		pressImage.SetNativeSize();
		Vector2 size = normalImage.rectTransform.rect.size;
		GetRectTransform().sizeDelta = size;
		layoutElement.minWidth = size.x;
		layoutElement.minHeight = size.y;
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		base.DoStateTransition(state, instant);
		if (state == SelectionState.Pressed)
		{
			ShowPressedState();
		}
		else
		{
			ShowNormalState();
		}
	}

	private void ShowPressedState()
	{
		switch (transitionType)
		{
		case TransitionType.SHOW_BOTH_IMAGE:
			if (pressImage != null)
			{
				pressImage.gameObject.SetActive(true);
			}
			return;
		}
		if (pressImage != null)
		{
			pressImage.gameObject.SetActive(true);
		}
		if (normalImage != null)
		{
			normalImage.gameObject.SetActive(false);
		}
	}

	private void ShowNormalState()
	{
		if (pressImage != null)
		{
			pressImage.gameObject.SetActive(false);
		}
		if (normalImage != null)
		{
			normalImage.gameObject.SetActive(true);
		}
	}
}
