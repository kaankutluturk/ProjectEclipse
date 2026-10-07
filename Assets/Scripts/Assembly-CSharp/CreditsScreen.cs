using System;
using System.Xml;
using Nekki.SF2.GUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CreditsScreen : SFMonoBehaviour<object>, IPointerClickHandler, IEventSystemHandler, BackKeyController
{
	public Transform content;

	public CreditsScreenElement elementPrefab;

	public ScrollRect scrollRect;

	public float autoScrollSpeed = 0.01f;

	private Action onClosed;

	public static CreditsScreen Create(Action OCLNBMKHLMH = null)
	{
		CreditsScreen original = Resources.Load<CreditsScreen>("Prefabs/Credits/CreditsScreen");
		original = UnityEngine.Object.Instantiate(original);
		original.onClosed = OCLNBMKHLMH;
		return original;
	}

	public void OnPointerClick(PointerEventData BHOLFGOGPCP)
	{
		Hide();
	}

	private void LoadCredits()
	{
		string text = SF2Paths.GetGameDataPath() + "/credits/";
		text += ((!(LocalizationManager.CurrentLanguage.name == "rus")) ? "eng.xml" : "rus.xml");
		XmlDocument xmlDocument = XmlUtils.OpenXMLDocument(text, string.Empty);
		XmlNode xmlNode = xmlDocument["Credits"];
		foreach (XmlNode item in xmlNode)
		{
			string kNNEDNHONBJ = item.Attributes["Name"].GetStringOrDefault();
			string innerText = item.InnerText;
			CreditsScreenElement creditsScreenElement = UnityEngine.Object.Instantiate(elementPrefab, content, false);
			creditsScreenElement.Init(kNNEDNHONBJ, innerText);
		}
	}

	public void Hide()
	{
		if (onClosed != null)
		{
			onClosed();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void Awake()
	{
		LoadCredits();
		BackKeyManager.get_Instance().AddBackKeyController(this);
	}

	private void OnDestroy()
	{
		BackKeyManager.get_Instance().RemoveBackKeyController(this);
	}

	private void Update()
	{
		scrollRect.verticalNormalizedPosition += autoScrollSpeed * Time.deltaTime;
		if (scrollRect.verticalNormalizedPosition <= 0f)
		{
			Hide();
		}
	}

	public void OnBackKeyClicked(object GHDPPHAAPCA)
	{
		Hide();
	}
}
