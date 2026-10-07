using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Map
{
	public class LampsPanel : SFMonoBehaviour<object>
	{
		public enum LampsPanelEvent
		{
			OnClickLamp = 0
		}

		public const float LAMP_PADDING_BOT = -55f;

		public const float LAMP_DISTANCE = 22f;

		public const float LAMP_WIDTH = 68f;

		private List<Button> lampButtons = new List<Button>();

		private List<ResolutionImage> lampIndicators = new List<ResolutionImage>();

		private List<Button> flashingLampButtons = new List<Button>();

		private List<ResolutionImage> flashingLampIndicators = new List<ResolutionImage>();

		[SerializeField]
		private ResolutionImage _lampOn;

		private int currentLampIndex;

		private int lampOpacity = 255;

		private int indicatorOpacity;

		private int flashPauseFrames;

		private bool isIndicatorFlashing;

		private bool isIndicatorFadingOut = true;

		private bool isLampFlashing = true;

		private bool isLampFadingOut;

		[SerializeField]
		private LabelAlias _locationName;

		[SerializeField]
		private GameObject _lampButtonPrefab;

		[SerializeField]
		private GameObject _lampIndicatorPrefab;

		[SerializeField]
		private GameObject _lampsContainer;

		[SerializeField]
		private GameObject _indicatorsContainer;

		public void Init()
		{
			flashPauseFrames = MapGUI.ZoneSwitchFade.DelayBeforeFade;
			_locationName.set_text("???");
		}

		public void ClearLamps()
		{
			foreach (Button item in lampButtons)
			{
				Object.Destroy(item.gameObject);
			}
			lampButtons.Clear();
			foreach (ResolutionImage item2 in lampIndicators)
			{
				Object.Destroy(item2.gameObject);
			}
			lampIndicators.Clear();
		}

		public void AddLamps(int lampCount)
		{
			float num = 0f;
			float num2 = 68f * (float)lampCount + 22f * (float)(lampCount - 1);
			for (int i = 0; i < lampCount; i++)
			{
				GameObject gameObject = Object.Instantiate(_lampButtonPrefab);
				SFButton component = gameObject.GetComponent<SFButton>();
				component.gameObject.transform.SetParent(_lampsContainer.transform, false);
				component.ButtonId = i;
				component.AddEventListener(2, OnLampClicked);
				float x = num - num2 / 2f + (float)i * 90f + 34f;
				float y = -55f;
				component.transform.localPosition = new Vector3(x, y);
				lampButtons.Add(component);
				GameObject gameObject2 = Object.Instantiate(_lampIndicatorPrefab);
				ResolutionImage component2 = gameObject2.GetComponent<ResolutionImage>();
				component2.gameObject.transform.SetParent(_indicatorsContainer.transform, false);
				component2.transform.localPosition = new Vector3(x, y);
				UIExtensions.SetAlpha(component2, 0f);
				component2.raycastTarget = false;
				lampIndicators.Add(component2);
			}
			currentLampIndex = 0;
			if (lampButtons.Count > 0)
			{
				_lampOn.transform.localPosition = lampButtons[0].transform.localPosition;
			}
		}

		public List<Button> GetLamps()
		{
			return lampButtons;
		}

		public void SetCurrentZone(int index, string zoneAlias)
		{
			if (lampButtons.Count != 0)
			{
				_lampOn.transform.localPosition = lampButtons[index].transform.localPosition;
				currentLampIndex = index;
				_locationName.SetAlias(zoneAlias);
			}
		}

		public int GetCurrentLamp()
		{
			return currentLampIndex;
		}

		public void Flashing()
		{
			if (flashPauseFrames > 0)
			{
				flashPauseFrames--;
				return;
			}
			int minOpacity = MapGUI.ZoneSwitchFade.MinOpacity;
			int fadeFrames = MapGUI.ZoneSwitchFade.FadeSpeed;
			if (fadeFrames <= 0)
			{
				return;
			}
			if (isLampFlashing)
			{
				ChangeLampOpacity(flashingLampButtons);
				if (lampOpacity <= minOpacity && isLampFadingOut)
				{
					isIndicatorFlashing = true;
				}
				if (lampOpacity <= 0)
				{
					isLampFlashing = false;
				}
			}
			if (isIndicatorFlashing)
			{
				ChangeLampIndicatorOpacity(flashingLampIndicators);
				if (indicatorOpacity <= minOpacity && isIndicatorFadingOut)
				{
					isLampFlashing = true;
				}
				if (indicatorOpacity <= 0)
				{
					isIndicatorFlashing = false;
				}
			}
		}

		public void CheckOpenZones(List<ZoneScrollItem> zoneItems)
		{
			flashingLampButtons.Clear();
			flashingLampIndicators.Clear();
			for (int i = 0; i < zoneItems.Count; i++)
			{
				Zone zone = zoneItems[i].get_Zone();
				if (!zone.GetIsStart())
				{
					if (!MapScene.IsZoneOpen(zone))
					{
						break;
					}
					if (MapScene.IsZoneHaveDontCompleteBattle(zone))
					{
						flashingLampButtons.Add(lampButtons[i]);
						flashingLampIndicators.Add(lampIndicators[i]);
					}
				}
			}
		}

		public virtual void SetTouchEnabled(bool isTouchEnabled)
		{
			foreach (Button item in lampButtons)
			{
				item.interactable = isTouchEnabled;
			}
		}

		public void SetLampsVisible(bool value)
		{
			foreach (Button item in lampButtons)
			{
				if (item != null)
				{
					item.gameObject.SetActive(value);
				}
			}
			foreach (ResolutionImage item2 in lampIndicators)
			{
				if (item2 != null)
				{
					item2.gameObject.SetActive(value);
				}
			}
			foreach (Button item3 in flashingLampButtons)
			{
				if (item3 != null)
				{
					item3.gameObject.SetActive(value);
				}
			}
			foreach (ResolutionImage item4 in flashingLampIndicators)
			{
				if (item4 != null)
				{
					item4.gameObject.SetActive(value);
				}
			}
			if (_lampOn != null)
			{
				_lampOn.gameObject.SetActive(value);
			}
		}

		private void OnLampClicked(object data)
		{
			CallEvent(0, (int)data);
		}

		private void ChangeLampOpacity(List<Button> lampButtons)
		{
			int fadeFrames = MapGUI.ZoneSwitchFade.FadeSpeed;
			if (fadeFrames <= 0)
			{
				return;
			}
			int num = 255 / fadeFrames;
			if (isLampFadingOut)
			{
				lampOpacity -= num;
				if (lampOpacity <= 0)
				{
					lampOpacity = 0;
					isLampFadingOut = false;
				}
			}
			else
			{
				lampOpacity += num;
				if (lampOpacity >= 255)
				{
					lampOpacity = 255;
					isLampFadingOut = true;
					flashPauseFrames = MapGUI.ZoneSwitchFade.DelayBeforeFade;
				}
			}
			for (int i = 0; i < lampButtons.Count; i++)
			{
				lampButtons[i].targetGraphic.SetAlpha(lampOpacity / 255);
			}
		}

		private void ChangeLampIndicatorOpacity(List<ResolutionImage> indicatorImages)
		{
			int fadeFrames = MapGUI.ZoneSwitchFade.FadeSpeed;
			if (fadeFrames <= 0)
			{
				return;
			}
			int num = 255 / fadeFrames;
			if (isIndicatorFadingOut)
			{
				indicatorOpacity -= num;
				if (indicatorOpacity <= 0)
				{
					indicatorOpacity = 0;
					isIndicatorFadingOut = false;
				}
			}
			else
			{
				indicatorOpacity += num;
				if (indicatorOpacity >= 255)
				{
					indicatorOpacity = 255;
					isIndicatorFadingOut = true;
					flashPauseFrames = MapGUI.ZoneSwitchFade.DelayBeforeFade;
				}
			}
			for (int i = 0; i < indicatorImages.Count; i++)
			{
				indicatorImages[i].gameObject.SetActive(true);
				UIExtensions.SetAlpha(indicatorImages[i], indicatorOpacity / 255);
			}
		}
	}
}
