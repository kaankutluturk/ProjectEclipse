using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml;
using UnityEngine;

public static class LocalizationManager
{
public class Language
	{
		public string name;

		public string FilePath;

		public string IconSprite;

		public string SelectedIconSprite;

		public string Locale;

		public string Alias;

		public int index;

		public string LoaderImage;

		public string PreloaderImage;

		public bool IsAsian;

		public Font ContentFont;

		public Font TitleFont;

		public Font ButtonFont;

		public string ContentFontName;

		public string TitleFontName;

		public string ButtonFontName;

		public float FontSizeScale = 1f;

		public float LineSpacing = 1f;

		public float CustomLineSpacingScale = 1f;

		public Language(XmlNode node, int languageIndex)
		{
			name = node.Attributes["Name"].GetStringOrDefault("Name");
			Locale = node.Attributes["Locale"].GetStringOrDefault("Locale");
			FilePath = SF2Paths.GetLocalizationsPath() + "/" + name + ".xml";
			IconSprite = "SettingsButtons." + node.Attributes["FileIcon"].GetStringOrDefault("FileIcon");
			if (!node.Attributes["FileIconSelected"].Empty())
			{
				SelectedIconSprite = "SettingsButtons." + node.Attributes["FileIconSelected"].GetStringOrDefault(string.Empty);
			}
			index = languageIndex;
			Alias = node.Attributes["Alias"].GetStringOrDefault("Alias");
			LoaderImage = node.Attributes["LoaderImage"].GetStringOrDefault("logo");
			PreloaderImage = node.Attributes["PreloaderImage"].GetStringOrDefault();
			IsAsian = node.Attributes["IsAsian"].ParseBool();
			if (node["Fonts"] != null)
			{
				LoadFonts(node["Fonts"], ref ContentFontName, ref TitleFontName, ref ButtonFontName, ref ContentFont, ref TitleFont, ref ButtonFont);
				FontSizeScale = node["Fonts"].Attributes["FontSizeScale"].ParseFloat(1f);
				LineSpacing = node["Fonts"].Attributes["LineSpacing"].ParseFloat(1f);
				CustomLineSpacingScale = node["Fonts"].Attributes["CustomLineSpacingScale"].ParseFloat(1f);
			}
		}

		public void LoadMissingFonts()
		{
			if (ContentFont == null || TitleFontName == null || ButtonFontName == null)
			{
				LoadFonts(ContentFontName, TitleFontName, ButtonFontName, ref ContentFont, ref TitleFont, ref ButtonFont);
			}
		}
	}

		private static Dictionary<string, string> words;

		private static readonly Dictionary<string, string> EclipseExternalStrings = new Dictionary<string, string>(StringComparer.Ordinal);

	private const string FontsPath = "UI/Fonts/";

	public static bool IsLoaded;

	public static string DefaultLanguageName;

	public static List<Language> Languages;

	public static Language CurrentLanguage;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[CompilerGenerated]
	private static Action OnLanguageChanged;

	private static string defaultContentFontName;

	private static Font defaultContentFont;

	private static string defaultTitleFontName;

	private static Font defaultTitleFont;

	private static string defaultButtonFontName;

	private static Font defaultButtonFont;

	public static Font ContentFont
	{
		get
		{
			return GetContentFont();
		}
	}

	public static Font TitleFont
	{
		get
		{
			return GetTitleFont();
		}
	}

	public static Font ButtonFont
	{
		get
		{
			return GetButtonFont();
		}
	}

	public static float FontSizeScale
	{
		get
		{
			return GetFontSizeScale();
		}
	}

	public static float LineSpacing
	{
		get
		{
			return GetLineSpacing();
		}
	}

	public static float CustomLineSpacingScale
	{
		get
		{
			return GetCustomLineSpacingScale();
		}
	}

	public static bool HasCurrentLanguage
	{
		get
		{
			return GetHasCurrentLanguage();
		}
	}

	public static event Action LanguageChanged
	{
		add
		{
			AddLanguageChangedHandler(value);
		}
		remove
		{
			RemoveLanguageChangedHandler(value);
		}
	}

	static LocalizationManager()
	{
		IsLoaded = false;
	}

	public static void AddLanguageChangedHandler(Action value)
	{
		Action action = OnLanguageChanged;
		Action action2;
		do
		{
			action2 = action;
			action = Interlocked.CompareExchange(ref OnLanguageChanged, (Action)Delegate.Combine(action2, value), action);
		}
		while ((object)action != action2);
	}

	public static void RemoveLanguageChangedHandler(Action value)
	{
		Action action = OnLanguageChanged;
		Action action2;
		do
		{
			action2 = action;
			action = Interlocked.CompareExchange(ref OnLanguageChanged, (Action)Delegate.Remove(action2, value), action);
		}
		while ((object)action != action2);
	}

	public static Font GetContentFont()
	{
		if (CurrentLanguage != null)
		{
			if (CurrentLanguage.ContentFont != null)
			{
				return CurrentLanguage.ContentFont;
			}
		}
		if (defaultContentFont == null)
		{
			defaultContentFont = ResourcesAndBundles.Load<Font>("UI/Fonts/majallab");
		}
		return defaultContentFont;
	}

	// Thin Eclipse modding seam. External strings are an overlay rather than destructive writes
	// into the recovered language dictionary. Removing an override therefore reveals the
	// canonical base value again, including for controlled core localization patches.
	public static void SetExternalString(string key, string value)
	{
		if (words == null)
		{
			throw new InvalidOperationException("LocalizationManager is not initialized.");
		}
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentException("External localization key must not be empty.", "key");
		}
		EclipseExternalStrings[key] = value ?? string.Empty;
	}

	public static void RemoveExternalString(string key)
	{
		if (!string.IsNullOrEmpty(key))
		{
			EclipseExternalStrings.Remove(key);
		}
	}

	public static Font GetTitleFont()
	{
		if (CurrentLanguage != null)
		{
			if (CurrentLanguage.TitleFont != null)
			{
				return CurrentLanguage.TitleFont;
			}
		}
		if (defaultTitleFont == null)
		{
			defaultTitleFont = ResourcesAndBundles.Load<Font>("UI/Fonts/majallab");
		}
		return defaultTitleFont;
	}

	public static Font GetButtonFont()
	{
		if (CurrentLanguage != null)
		{
			if (CurrentLanguage.ButtonFont != null)
			{
				return CurrentLanguage.ButtonFont;
			}
		}
		if (defaultButtonFont == null)
		{
			defaultButtonFont = ResourcesAndBundles.Load<Font>("UI/Fonts/majallab");
		}
		return defaultButtonFont;
	}

	public static float GetFontSizeScale()
	{
		if (CurrentLanguage != null)
		{
			return CurrentLanguage.FontSizeScale;
		}
		return 1f;
	}

	public static float GetLineSpacing()
	{
		if (CurrentLanguage != null)
		{
			return CurrentLanguage.LineSpacing;
		}
		return 1f;
	}

	public static float GetCustomLineSpacingScale()
	{
		if (CurrentLanguage != null)
		{
			return CurrentLanguage.CustomLineSpacingScale;
		}
		return 1f;
	}

	public static bool GetHasCurrentLanguage()
	{
		return CurrentLanguage != null;
	}

		public static void Init()
		{
			Languages = new List<Language>();
			words = new Dictionary<string, string>();
			EclipseExternalStrings.Clear();
			LoadLocalizationConfig();
		foreach (Language item in Languages)
		{
			if (item.name == DefaultLanguageName)
			{
				CurrentLanguage = item;
				break;
			}
		}
		ApplyLanguage(null, false);
	}

	public static void LoadLocalizationConfig()
	{
		string text = "/localization.xml";
		string text2 = SF2Paths.GetGameDataPath();
		XmlDocument xmlDocument = XmlUtils.OpenXMLDocument(text2 + text, string.Empty);
		if (xmlDocument != null)
		{
			XmlNode xmlNode = xmlDocument["Localization"]["DefaultFonts"];
			if (xmlNode != null)
			{
				LoadFonts(xmlNode, ref defaultContentFontName, ref defaultTitleFontName, ref defaultButtonFontName, ref defaultContentFont, ref defaultTitleFont, ref defaultButtonFont);
				if (defaultContentFont == null || defaultTitleFont == null || defaultButtonFont == null)
				{
					GameLog.Error(string.Format("ERROR: LoadFonts: one or more defalut font is missing"));
				}
			}
			else
			{
				GameLog.Error("ERROR: LocalizationManager.LoadLocalization - wrong file");
			}
			XmlNode xmlNode2 = xmlDocument["Localization"]["Languages"];
			if (xmlNode2 != null)
			{
				ParseLanguages(xmlNode2);
			}
			else
			{
				GameLog.Error("ERROR: LocalizationManager.LoadLocalization - wrong file");
			}
		}
		else
		{
			GameLog.Error(string.Format("ERROR: LoadLocalization - file \"{0}\" doesn't exist", text2 + text));
		}
	}

	private static void LoadFonts(XmlNode node, ref string contentFontName, ref string titleFontName, ref string buttonFontName, ref Font contentFont, ref Font titleFont, ref Font buttonFont)
	{
		contentFontName = node.Attributes["ContentFont"].GetStringOrDefault(string.Empty);
		titleFontName = node.Attributes["TitleFont"].GetStringOrDefault(string.Empty);
		buttonFontName = node.Attributes["ButtonFont"].GetStringOrDefault(string.Empty);
		LoadFonts(contentFontName, titleFontName, buttonFontName, ref contentFont, ref titleFont, ref buttonFont);
	}

	private static void LoadFonts(string contentFontName, string titleFontName, string buttonFontName, ref Font contentFont, ref Font titleFont, ref Font buttonFont)
	{
		contentFont = ResourcesAndBundles.Load<Font>("UI/Fonts/" + contentFontName);
		titleFont = ResourcesAndBundles.Load<Font>("UI/Fonts/" + titleFontName);
		buttonFont = ResourcesAndBundles.Load<Font>("UI/Fonts/" + buttonFontName);
	}

	private static void ParseLanguages(XmlNode node)
	{
		DefaultLanguageName = node.Attributes["Default"].GetStringOrDefault(string.Empty);
		Languages.Clear();
		int num = 0;
		foreach (XmlNode childNode in node.ChildNodes)
		{
			Language item = new Language(childNode, num);
			Languages.Add(item);
			num++;
		}
	}

	// Host UI can supply readable defaults for newly reconstructed labels without
	// logging a missing-key error for every frame or displaying %%ERROR%%.
	internal static string GetStringOrDefault(string key, string fallback, params string[] arguments)
	{
		if (string.IsNullOrEmpty(key) || CurrentLanguage == null ||
			(!EclipseExternalStrings.ContainsKey(key) && (words == null || !words.ContainsKey(key)))) return fallback;
		return GetString(key, arguments);
	}

	public static string GetString(string stringKey, params string[] args)
	{
		if (stringKey == null || CurrentLanguage == null)
		{
			return string.Empty;
		}
		List<string> list = new List<string>(args);
		string key = stringKey;
		bool flag = false;
		int num = stringKey.IndexOf("{");
		if (num != -1)
		{
			flag = true;
			// Several original aliases are authored as "title {value}".  The
			// separating space is presentation syntax, not part of the localization
			// key.  Keeping it made valid entries such as "replays {999}" look up
			// "replays " and spam a false missing-localization error.
			key = stringKey.Substring(0, num).TrimEnd();
		}
			string text;
			if (!EclipseExternalStrings.TryGetValue(key, out text) && !words.TryGetValue(key, out text))
			{
				if (stringKey != string.Empty)
				{
					GameLog.Error(string.Format("ERROR: localization does not contain title \"{0}\"", stringKey));
				}
				return "%%ERROR%%";
			}
			// Newer localization files use {br} as an explicit line break.  The
		// original formatter treated every brace token as a numeric argument;
		// int.TryParse("br") therefore became argument zero and produced strings
		// such as "EXPERIENCE6060/190".
		text = text.Replace("{br}", "\n").Replace("{BR}", "\n");
		if (flag)
		{
			if (list.Count != 0)
			{
				GameLog.Error(string.Format("ERROR: GetString - parameters passed both through arguments and title in \"{0}\"", stringKey));
			}
			for (int num2 = num; num2 != -1; num2 = stringKey.IndexOf('{', num2 + 1))
			{
				int num3 = stringKey.IndexOf('}', num2 + 1);
				if ((num3 > stringKey.IndexOf('{', num2 + 1) && stringKey.IndexOf('{', num2 + 1) != -1) || num3 == -1)
				{
					GameLog.Error(string.Format("ERROR: GetString - parameters brackets broken in title \"{0}\"", stringKey));
					break;
				}
				string argument = stringKey.Substring(num2 + 1, num3 - num2 - 1);
				argument = ResolveEmbeddedKeys(argument);
				list.Add(argument);
			}
		}
		if (list.Count != 0)
		{
			for (int num4 = text.IndexOf("{"); num4 != -1; num4 = text.IndexOf('{', num4 + 1))
			{
				int num5 = text.IndexOf('}', num4 + 1);
				if ((num5 > text.IndexOf('{', num4 + 1) && text.IndexOf('{', num4 + 1) != -1) || num5 == -1)
				{
					GameLog.Error(string.Format("ERROR: GetString - parameters brackets broken in content of title \"{0}\"", stringKey));
					break;
				}
				string text2 = text.Substring(num4 + 1, num5 - num4 - 1);
				int result;
				if (!int.TryParse(text2, out result) || result < 0 || result >= list.Count)
				{
					continue;
				}
				string oldValue = "{" + text2 + "}";
				string text3 = list[result];
				if (text3.StartsWith("img::"))
				{
					text3 = text3.Replace("img::", "<quad name=") + " size=25 width=1 />";
				}
				text = text.Replace(oldValue, text3);
			}
		}
		return ResolveEmbeddedKeys(text);
	}

	public static string DateString(long unixSeconds)
	{
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixSeconds);
		return string.Format("{0}.{1}.{2}", dateTime.Day, dateTime.Month, dateTime.Year);
	}

	private static string ResolveEmbeddedKeys(string source)
	{
		string text = source;
		for (int num = text.IndexOf('%'); num != -1; num = text.IndexOf('%', num + 1))
		{
			if (num + 1 >= text.Length || char.IsWhiteSpace(text[num + 1])) continue;
			if (text[num + 1] == '%')
			{
				text = text.Remove(num, 1);
			}
			else
			{
				int num2 = GetWordEndSymbol(text, num);
				string text2 = text.Substring(num + 1, num2 - num - 1);
				string newValue = GetString(text2);
				text = text.Remove(num, num2 - num).Insert(num, newValue);
				num += newValue.Length - 1;
			}
		}
		return text;
	}

	private static int GetWordEndSymbol(string source, int startIndex)
	{
		int num = source.IndexOf(' ', startIndex);
		int num2 = source.IndexOf('\n', startIndex);
		if ((num2 < num && num2 != -1) || num == -1)
		{
			num = num2;
		}
		if (num == -1)
		{
			num = source.Length;
		}
		return num;
	}

	public static Language GetNextLanguage(Language language = null)
	{
		if (language == null)
		{
			language = CurrentLanguage;
		}
		int nextIndex = language.index;
		int count = Languages.Count;
		nextIndex = (nextIndex + 1) % count;
		return Languages[nextIndex];
	}

	public static Language FindLanguageByName(string languageName)
	{
		return Languages.Find((Language candidate) => candidate.name.Equals(languageName));
	}

	public static Language FindLanguageByLocale(string locale)
	{
		return Languages.Find((Language candidate) => candidate.Locale.Equals(locale));
	}

	private static void Load(string fileName)
	{
		Clear();
		XmlDocument xmlDocument = XmlUtils.OpenXMLDocument(fileName, string.Empty);
		if (xmlDocument != null)
		{
			ParseWords(xmlDocument["Localization"]["Words"]);
			IsLoaded = true;
		}
		else
		{
			GameLog.Error(string.Format("ERROR: load - file \"{0}\" doesn't exist", fileName));
		}
	}

	private static void ParseWords(XmlNode node)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.NodeType == XmlNodeType.Element)
			{
				words[childNode.Attributes["Title"].GetStringOrDefault(string.Empty)] = childNode.InnerText;
			}
		}
	}

	private static void ApplyLanguage(Language language = null, bool saveToRoster = true)
	{
		if (language == null)
		{
			language = CurrentLanguage;
		}
		if (!HasAllFonts(language))
		{
			GameLog.Error(string.Format("ERROR: Language \"{0}\" doesn't have fonts", language.name));
		}
		Load(language.FilePath);
		CurrentLanguage = language;
		if (CurrentLanguage != null)
		{
			CurrentLanguage.LoadMissingFonts();
		}
		if (saveToRoster)
		{
			ListSF.GetRoster().SetLanguage(language.name);
			ListSF.GetRoster().RequestSave();
		}
	}

	private static void Clear()
	{
		words.Clear();
		EclipseExternalStrings.Clear();
		IsLoaded = false;
	}

	public static bool HasAllFonts(Language language)
	{
		if (language != null)
		{
			if (language.ButtonFont == null)
			{
				return false;
			}
			if (language.ContentFont == null)
			{
				return false;
			}
			if (language.TitleFont == null)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static void ChangeLanguage(Language language = null)
	{
		if (language != null)
		{
			ApplyLanguage(language);
		}
		else
		{
			ApplyLanguage(GetNextLanguage());
		}
		if (OnLanguageChanged != null)
		{
			OnLanguageChanged();
		}
	}
}
