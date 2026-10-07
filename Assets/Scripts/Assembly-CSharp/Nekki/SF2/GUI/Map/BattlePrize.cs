using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Map
{
	public class BattlePrize : SFMonoBehaviour<object>
	{
		public const float BATTLE_GOLD_SIZE = 40f;

		private List<BattlePrizeElement> prizeElements = new List<BattlePrizeElement>();

		private float maxWidth = -1f;

		private float itemIconHeight = -1f;

		private float unusedWidth;

		private float unusedHeight;

		private bool needsScroll;

		[SerializeField]
		private GameObject _prizeElemPrefab;

		[SerializeField]
		private HorizontalLayoutGroup _layoutGroup;

		[SerializeField]
		private ResolutionImage _itemIcon;

		private float totalContentWidth;

		private float _spacing = 20f;

		private static int scrollFrameCounter = 360;

		public void Init(long money, long rubies, RewardPrize prize, float width, float iconHeight, int fontSize)
		{
			float moneyScale = 1f;
			float jPDGMJHNKPK2 = 1f;
			foreach (BattlePrizeElement item in prizeElements)
			{
				Object.Destroy(item.gameObject);
			}
			prizeElements.Clear();
			_itemIcon.gameObject.SetActive(false);
			maxWidth = width;
			itemIconHeight = iconHeight;
			string rubyIcon = "MiscSprites.ruby";
			string aDONPNOBBDE2 = ListSF.GetRoster().GetCoinIcon();
			totalContentWidth = 0f;
			if (money > 0)
			{
				BattlePrizeElement component = Object.Instantiate(_prizeElemPrefab).GetComponent<BattlePrizeElement>();
				component.Init(aDONPNOBBDE2, money, fontSize, moneyScale);
				AddPrizeElement(component);
			}
			if (rubies > 0)
			{
				BattlePrizeElement component2 = Object.Instantiate(_prizeElemPrefab).GetComponent<BattlePrizeElement>();
				component2.Init(rubyIcon, rubies, fontSize, jPDGMJHNKPK2);
				AddPrizeElement(component2);
			}
			foreach (RewardCurrency item2 in prize.currencyRewards)
			{
				if (item2.ShowReward)
				{
					GameCurrency currency = GameUtils.GameCurrencies.GetCurrencyByName(item2.Name);
					if (currency != null)
					{
						string currencyIcon = currency.Icon;
						long amount = item2.GetMinimumAmount();
						BattlePrizeElement component3 = Object.Instantiate(_prizeElemPrefab).GetComponent<BattlePrizeElement>();
						component3.Init(currencyIcon, amount, fontSize);
						AddPrizeElement(component3);
					}
				}
			}
			foreach (RewardResistance item3 in prize.resistanceRewards)
			{
				if (item3.ShowReward)
				{
					GameResistance resistance = GameUtils.GameResistances.GetResistanceByName(item3.Name);
					if (resistance != null)
					{
						string aDONPNOBBDE3 = resistance.GetIcon();
						long bAINMLLIKOL2 = item3.Value;
						BattlePrizeElement component4 = Object.Instantiate(_prizeElemPrefab).GetComponent<BattlePrizeElement>();
						component4.Init(aDONPNOBBDE3, bAINMLLIKOL2, fontSize);
						AddPrizeElement(component4);
					}
				}
			}
			_layoutGroup.spacing = _spacing;
			_layoutGroup.GetComponent<RectTransform>().sizeDelta = new Vector2(totalContentWidth, _layoutGroup.GetComponent<RectTransform>().sizeDelta.y);
			UpdateScrollState();
			foreach (RewardItem item4 in prize.items)
			{
				if (item4.ShowReward)
				{
					ItemInfo itemInfo = ListSF.GetItems().GetItemByName(item4.Name);
					UserItem userItem = ListSF.GetRoster().GetInventory().FindItem(itemInfo);
					if (userItem == null)
					{
						AddItem(itemInfo);
						break;
					}
				}
			}
		}

		public void AddItem(ItemInfo itemInfo)
		{
			if (itemInfo.Type == "Seal")
			{
				_itemIcon.set_TexturePath(SF2Paths.GetUsersUiPath());
			}
			else
			{
				_itemIcon.set_TexturePath(SF2Paths.GetItemsUiPath());
			}
			_itemIcon.set_SpriteName(itemInfo.FileName);
			_itemIcon.gameObject.SetActive(true);
			_itemIcon.preserveAspect = true;
			foreach (BattlePrizeElement item in prizeElements)
			{
				item.gameObject.SetActive(false);
			}
			UpdateScrollState();
		}

		private void Update()
		{
			if (needsScroll)
			{
				ScrollPrizes();
			}
		}

		private void AddPrizeElement(BattlePrizeElement prizeElement)
		{
			if (prizeElement != null)
			{
				totalContentWidth += prizeElement.GetComponent<LayoutElement>().preferredWidth;
				totalContentWidth += _spacing;
				prizeElement.transform.SetParent(_layoutGroup.transform, false);
				prizeElements.Add(prizeElement);
			}
		}

		private void UpdateScrollState()
		{
			needsScroll = _layoutGroup.GetComponent<RectTransform>().rect.width > GetComponent<RectTransform>().rect.width;
			if (null != _itemIcon)
			{
				_itemIcon.rectTransform.sizeDelta = new Vector2(500f, itemIconHeight);
			}
		}

		private void ScrollPrizes()
		{
			scrollFrameCounter++;
			float period = MapGUI.RewardLineOscillation.OscillationPeriod;
			float factor = MapGUI.RewardLineOscillation.OscillationFactor;
			float num = _layoutGroup.GetComponent<RectTransform>().rect.width - GetComponent<RectTransform>().rect.width;
			float num2 = num / (2f * period);
			float offsetX = num2 * period * Mathf.Cos(factor * (float)scrollFrameCounter);
			_layoutGroup.transform.SetLocalX(offsetX);
		}
	}
}
