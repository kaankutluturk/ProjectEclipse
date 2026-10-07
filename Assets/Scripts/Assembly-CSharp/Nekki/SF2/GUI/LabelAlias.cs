using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Nekki.SF2.GUI
{
	[AddComponentMenu("UI_Nekki/LabelAlias")]
	public class LabelAlias : TextPic
	{
		public enum LabelFontType
		{
			Content = 0,
			Title = 1,
			Button = 2
		}

		[Tooltip("If true target will use font from localization settings")]
		public bool UseLocalizationFont = true;

		private static readonly Regex QuadTagRegex = new Regex("<quad name=(.+?) size=(\\d*\\.?\\d+%?) width=(\\d*\\.?\\d+%?) />", RegexOptions.Singleline);

		[Tooltip("Type of font. Choose font from languages.yaml")]
		public LabelFontType FontType;

		[Tooltip("Flag for turn on/off using custom font size.")]
		public bool UseLabelFontSize = true;

		[Tooltip("Size who used for calculating result font size.")]
		[SerializeField]
		private int _labelFontSize;

		[Tooltip("Flag for turn on/off using custom line spacing.")]
		public bool UseLabelLineSpacing;

		[SerializeField]
		[Tooltip("Size who used for calculating result line spacing.")]
		private float _labelLineSpacing;

		[Tooltip("Text template with aliases")]
		[SerializeField]
		[TextArea(4, 10)]
		private string _Alias = string.Empty;

		[SerializeField]
		private bool _ToUpperCase;

		public int BaseFontSize
		{
			get
			{
				return get_LabelFontSize();
			}
			set
			{
				set_LabelFontSize(value);
			}
		}

		public float BaseLineSpacing
		{
			get
			{
				return get_LabelLineSpacing();
			}
			set
			{
				set_LabelLineSpacing(value);
			}
		}

		public string AliasKey
		{
			get
			{
				return get_Alias();
			}
			set
			{
				set_Alias(value);
			}
		}

		public bool UppercaseText
		{
			get
			{
				return get_ToUpperCase();
			}
			set
			{
				set_ToUpperCase(value);
			}
		}

		public string DisplayText
		{
			get
			{
				return get_text();
			}
			set
			{
				set_text(value);
			}
		}

		private string LocalizedText
		{
			get
			{
				return GetLocalizedAlias();
			}
		}

		private Font LocalizedFont
		{
			get
			{
				return GetLocalizationFont();
			}
		}

		public int get_LabelFontSize()
		{
			return _labelFontSize;
		}

		public void set_LabelFontSize(int value)
		{
			_labelFontSize = value;
			UpdateLabelFontSize();
		}

		public float get_LabelLineSpacing()
		{
			return _labelLineSpacing;
		}

		public void set_LabelLineSpacing(float value)
		{
			_labelLineSpacing = value;
			UpdateLabelLineSpacing();
		}

		public string get_Alias()
		{
			return _Alias;
		}

		public void set_Alias(string value)
		{
			SetAlias(value);
		}

		public bool get_ToUpperCase()
		{
			return _ToUpperCase;
		}

		public void set_ToUpperCase(bool value)
		{
			_ToUpperCase = value;
		}

		public string get_text()
		{
			return base.text;
		}

		public void set_text(string value)
		{
			string text = value ?? string.Empty;
			foreach (Match item in QuadTagRegex.Matches(text))
			{
				string valueText = item.Groups[1].Value;
				Sprite sprite = ResolutionImage.GetSprite(string.Empty, valueText);
                if (sprite == null) continue;
				string newValue = Regex.Replace(item.ToString(), "size=(\\d*\\.?\\d+%?)", "size=" + sprite.rect.width);
				text = text.Replace(item.ToString(), newValue);
			}
			base.text = text;
			if (UseLocalizationFont)
			{
				base.font = GetLocalizationFont();
			}
		}

		private string GetLocalizedAlias()
		{
			return LocalizationManager.GetString(_Alias);
		}

		private Font GetLocalizationFont()
		{
			switch (FontType)
			{
			case LabelFontType.Content:
				return LocalizationManager.GetContentFont();
			case LabelFontType.Title:
				return LocalizationManager.GetTitleFont();
			case LabelFontType.Button:
				return LocalizationManager.GetButtonFont();
			default:
				return LocalizationManager.GetContentFont();
			}
		}

		private List<string> ExtractCaretSegments()
		{
			List<string> list = new List<string>();
			string text = null;
			bool flag = false;
			if (_Alias != null)
			{
				string alias = _Alias;
				foreach (char c in alias)
				{
					if (flag)
					{
						if (c == '^')
						{
							list.Add(text);
							flag = false;
						}
						else
						{
							text += c;
						}
					}
					else if (c == '^')
					{
						text = string.Empty;
						flag = true;
					}
				}
				return list;
			}
			return null;
		}

		protected override void Awake()
		{
			base.Awake();
			// Counters have no alias and are often updated through Text.text, which
			// bypasses set_text. Apply their configured font independently of text.
			if (UseLocalizationFont)
			{
				Font localizedFont = GetLocalizationFont();
				if (localizedFont != null)
				{
					base.font = localizedFont;
				}
			}
			UpdateLabelFontSize();
			UpdateLabelLineSpacing();
			UpdateVerticalOverflow();
			LocalizationManager.AddLanguageChangedHandler(RefreshLocalizedText);
			if (LocalizationManager.GetHasCurrentLanguage() && _Alias != null && _Alias.Length != 0)
			{
				RefreshLocalizedText();
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			LocalizationManager.RemoveLanguageChangedHandler(RefreshLocalizedText);
		}

		private void RefreshLocalizedText()
		{
			string text = GetLocalizedAlias().TrimStart(' ');
			if (_ToUpperCase)
			{
				text = text.ToUpper();
			}
			if (get_Alias() != string.Empty)
			{
				set_text(text);
				if (get_text() == "%ERROR%")
				{
					color = Color.red;
				}
			}
			if (UseLocalizationFont)
			{
				base.font = GetLocalizationFont();
			}
			UpdateLabelFontSize();
			UpdateLabelLineSpacing();
			UpdateVerticalOverflow();
		}

		public void SetAlias(string HCPNFPMHFCM)
		{
			_Alias = HCPNFPMHFCM ?? string.Empty;
			// Explicit clears must discard text left by a previous tooltip or item.
			if (_Alias.Length == 0) set_text(string.Empty);
			RefreshLocalizedText();
		}

		public int CalculateLengthOfMessage()
		{
			int num = 0;
			CharacterInfo info = default(CharacterInfo);
			char[] array = get_text().ToCharArray();
			char[] array2 = array;
			foreach (char ch in array2)
			{
				base.font.GetCharacterInfo(ch, out info, base.fontSize);
				num += info.advance;
			}
			return num;
		}

		public void UpdateLabelFontSize()
		{
			if (UseLabelFontSize)
			{
				int num = (int)((float)_labelFontSize * LocalizationManager.GetFontSizeScale());
				if (base.fontSize != num)
				{
					base.fontSize = num;
					base.resizeTextMaxSize = num;
				}
			}
		}

		public void UpdateLabelLineSpacing()
		{
			float num = 0f;
			num = ((!UseLabelLineSpacing) ? LocalizationManager.GetLineSpacing() : (_labelLineSpacing * LocalizationManager.GetCustomLineSpacingScale()));
			if (base.lineSpacing != num)
			{
				base.lineSpacing = num;
			}
		}

		public void UpdateVerticalOverflow()
		{
			if (LocalizationManager.CurrentLanguage != null && LocalizationManager.CurrentLanguage.IsAsian)
			{
				base.verticalOverflow = VerticalWrapMode.Overflow;
			}
		}
	}
}
