using Nekki.SF2.GUI;
using UnityEngine;

[RequireComponent(typeof(ResolutionImage))]
public class LabelButton : ResolutionButton
{
	public enum ButtonColor
	{
		BUTTON_DARK = 0,
		BUTTON_WHITE = 1,
		BUTTON_GREEN = 2,
		BUTTON_SILVER = 3,
		BUTTON_YELLOW = 4,
		BUTTON_BEIGE = 5
	}

	public const string FILE_BUTTON_DARK = "CommonButtons.BtnDark";

	public const string FILE_BUTTON_WHITE = "CommonButtons.BtnWhite";

	public const string FILE_BUTTON_GREEN = "CommonButtons.BtnGreen";

	public const string FILE_BUTTON_SILVER = "CommonButtons.btnSilver";

	public const string FILE_BUTTON_YELLOW = "CommonButtons.BtnYellow";

	public const string FILE_BUTTON_BEIGE = "CommonButtons.btnBeige";

	private string[] buttonSpriteNames = new string[6] { "CommonButtons.BtnDark", "CommonButtons.BtnWhite", "CommonButtons.BtnGreen", "CommonButtons.btnSilver", "CommonButtons.BtnYellow", "CommonButtons.btnBeige" };

	public LabelAlias Label;

	private float opacity;

	public Rect ButtonRect
	{
		get
		{
			return get_rect();
		}
	}

	public void SetAlias(string alias)
	{
		if (alias != null)
		{
			Label.SetAlias(alias);
		}
	}

	public string GetAlias()
	{
		if (Label != null)
		{
			return Label.get_Alias();
		}
		return string.Empty;
	}

	public void SetText(string text)
	{
		if (Label != null)
		{
			Label.set_text(text);
		}
	}

	public void SetColor(Color color)
	{
		if (Label != null)
		{
			Label.color = color;
		}
	}

	public string GetText()
	{
		if (Label != null)
		{
			return Label.get_text();
		}
		return string.Empty;
	}

	public void SetColor(ButtonColor color)
	{
		ResolutionImage resolutionImage = base.targetGraphic as ResolutionImage;
		if (resolutionImage != null)
		{
			resolutionImage.set_TexturePath("UI/Atlases/");
			resolutionImage.set_SpriteName(buttonSpriteNames[(int)color]);
		}
	}

	public ButtonColor GetColor()
	{
		ResolutionImage resolutionImage = base.targetGraphic as ResolutionImage;
		if (resolutionImage != null)
		{
			for (int i = 0; i < buttonSpriteNames.Length; i++)
			{
				if (resolutionImage.get_SpriteName() == buttonSpriteNames[i])
				{
					return (ButtonColor)i;
				}
			}
		}
		return ButtonColor.BUTTON_WHITE;
	}

	public virtual void SetOpacity(float newOpacity)
	{
		opacity = newOpacity;
		ResolutionImage resolutionImage = base.targetGraphic as ResolutionImage;
		Color color = resolutionImage.color;
		color.a = newOpacity;
		resolutionImage.color = color;
		if (Label != null)
		{
			Color color2 = Label.color;
			color2.a = newOpacity;
			Label.color = color2;
		}
	}

	public virtual float GetOpacity()
	{
		return opacity;
	}

	public static ButtonColor GetBtnColor(string colorName)
	{
		ButtonColor result = ButtonColor.BUTTON_WHITE;
		switch (colorName)
		{
		case "Red":
			result = ButtonColor.BUTTON_DARK;
			break;
		case "Green":
			result = ButtonColor.BUTTON_GREEN;
			break;
		case "White":
		case "Beige":
			result = ButtonColor.BUTTON_WHITE;
			break;
		}
		return result;
	}

	public Rect get_rect()
	{
		return GetComponent<RectTransform>().rect;
	}
}
