using System.Collections.Generic;
using Nekki.SF2.GUI.Shop;
using UnityEngine;

namespace Nekki.SF2.GUI.Profile
{
	public class UserPerksSprite : SFMonoBehaviour<object>
	{
		private ProfileSliderItem currentSliderItem;

		[SerializeField]
		private LabelAlias _label;

		private List<ProfilePerk> perks = new List<ProfilePerk>();

		[SerializeField]
		protected Scroll _slider;

		[SerializeField]
		private BaseScrollContent _sliderContent;

		[SerializeField]
		private GameObject _perkItemPrefab;

		[SerializeField]
		private GameObject _profileSliderItemPrefab;

		public void Init()
		{
			_label.set_text(string.Empty);
			_label.set_LabelFontSize(83);
			_label.color = Constants.DialogTextColor;
			_label.alignment = TextAnchor.MiddleCenter;
			InitSlider();
			LoadUserPerks();
			_label.rectTransform.sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width - 120f, _label.rectTransform.rect.height);
			_label.set_Alias("ProfileNoPerks");
		}

		public void AddItem(PerkInfoItem perkInfo)
		{
			if (perkInfo == null)
			{
				return;
			}
			ProfilePerk profilePerk = FindPerkByName(perkInfo.Name);
			if (profilePerk != null)
			{
				profilePerk.SetPerkInfo(perkInfo);
				profilePerk.set_Description(perkInfo.DescriptionKey);
				return;
			}
			ProfilePerk pLKCIINIFMJ2 = new ProfilePerk(perkInfo, 0, ProfilePerk.ProfilePerkState.PERK_SELECTED, ProfilePerk.ProfilePerkType.TYPE_PERK_SELETED);
			perks.Add(pLKCIINIFMJ2);
			if (!(pLKCIINIFMJ2.GetMoveName() != string.Empty))
			{
				if (NeedsNewSliderItem())
				{
					GameObject gameObject = Object.Instantiate(_profileSliderItemPrefab);
					currentSliderItem = gameObject.GetComponent<ProfileSliderItem>();
					currentSliderItem.Init(15f);
					_slider.AddItem(currentSliderItem);
				}
				GameObject gameObject2 = Object.Instantiate(_perkItemPrefab, currentSliderItem.transform, false);
				PerkSubItem component = gameObject2.GetComponent<PerkSubItem>();
				component.Init(pLKCIINIFMJ2, 0);
				ProfileScene current = Scene<ProfileScene>.get_Current();
				if (current != null)
				{
					current.AddSubItem(component);
				}
				currentSliderItem.AddIcons(component);
				ScrollToLastItems();
				if (_label != null)
				{
					_label.gameObject.SetActive(false);
				}
			}
		}

		public void Clear()
		{
			_slider.ClearItems();
			perks.ForEach((ProfilePerk perk) =>
			{
				perk.RemoveAllEventListener();
			});
			perks.Clear();
		}

		private void InitSlider()
		{
			_slider.Init(_sliderContent);
			_slider.get_ItemsScroll().AutoscrollIsOn = false;
		}

		private void LoadUserPerks()
		{
			List<RosterPerk> list = ListSF.GetRoster().GetPerks().GetPerks();
			for (int i = 0; i < list.Count; i++)
			{
				AddItem(list[i].GetPerkInfo());
			}
		}

		private void ScrollToLastItems()
		{
			if (!(_slider == null))
			{
				int count = _slider.GetItems().Count;
				if (count > 3)
				{
					_slider.ScrollToItem(count - 2);
				}
			}
		}

		private bool NeedsNewSliderItem()
		{
			if (currentSliderItem == null)
			{
				return true;
			}
			if (currentSliderItem.GetIcons().Count >= 2)
			{
				return true;
			}
			return false;
		}

		private ProfilePerk FindPerkByName(string name)
		{
			ProfilePerk result = null;
			foreach (ProfilePerk item in perks)
			{
				if (item.GetPerkName() == name)
				{
					result = item;
					break;
				}
			}
			return result;
		}
	}
}
