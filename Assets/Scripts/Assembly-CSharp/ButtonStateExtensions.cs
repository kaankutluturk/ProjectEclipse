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

	public static void SetPressType(this Button GAMILDJHFDB, ButtonPressType BGFHMJMEGEE, bool GHJGPAEDIHG = true)
	{
		switch (BGFHMJMEGEE)
		{
		case ButtonPressType.PressNormal:
			GAMILDJHFDB.interactable = true;
			break;
		case ButtonPressType.PressSelected:
			break;
		case ButtonPressType.PressInactive:
			GAMILDJHFDB.interactable = false;
			break;
		case ButtonPressType.PressPressed:
			GAMILDJHFDB.Select();
			break;
		}
	}

	public static ButtonPressType GetPressType(this Button GAMILDJHFDB)
	{
		if (GAMILDJHFDB.interactable)
		{
			return ButtonPressType.PressNormal;
		}
		return ButtonPressType.PressInactive;
	}
}
