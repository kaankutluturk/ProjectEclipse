using UnityEngine.UI;

public static class ButtonStateExtensions
{
	public enum ButtonPressType
	{
		PressNormal = 0,
		PressSelected = 1,
		PressInactive = 2,
		PressPressed = 3
	}

	public static void SetPressType(this Button button, ButtonPressType pressType, bool isEnabled = true)
	{
		switch (pressType)
		{
		case ButtonPressType.PressNormal:
			button.interactable = true;
			break;
		case ButtonPressType.PressSelected:
			break;
		case ButtonPressType.PressInactive:
			button.interactable = false;
			break;
		case ButtonPressType.PressPressed:
			button.Select();
			break;
		}
	}

	public static ButtonPressType GetPressType(this Button button)
	{
		if (button.interactable)
		{
			return ButtonPressType.PressNormal;
		}
		return ButtonPressType.PressInactive;
	}
}
