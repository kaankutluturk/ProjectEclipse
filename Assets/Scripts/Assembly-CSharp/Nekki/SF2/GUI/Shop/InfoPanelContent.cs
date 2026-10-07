using System;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using Nekki.SF2.GUI.Dialogs;
using UnityEngine;
using UnityEngine.Events;

namespace Nekki.SF2.GUI.Shop
{
	public class InfoPanelContent : SidePanelContent
	{
		public class ContentUpdatedEvent : UnityEvent
		{
		}

		public ContentUpdatedEvent updateEvent = new ContentUpdatedEvent();

		[SerializeField]
		private LabelAlias _header;

		[SerializeField]
		private LabelAlias _description;

		[SerializeField]
		private LabelAlias _descriptionDonate;

		[SerializeField]
		private ParametersPanel _parametersPanel;

		[SerializeField]
		private GameObject _buttonPanel;

		[SerializeField]
		private GameObject _buttonPrefab;

		private IconLabelButton buyGoldButton;

		private IconLabelButton buyRubyButton;

		private IconLabelButton upgradeGoldButton;

		private IconLabelButton upgradeRubyButton;

		private IconLabelButton deliveryRubyButton;

		private IconLabelButton consumableButton;

		private IconLabelButton realMoneyButton;

		private List<IconLabelButton> _buttons = new List<IconLabelButton>();

		private ItemInfo currentItem;

		private ItemInfo pendingItem;

		private UserItem userItem;

		private bool isOwned;

		private bool isOrdered;

		private bool isBeingMade;

		private bool isUpgradable;

		public override void Init()
		{
			if (_buttonPrefab != null && _buttonPanel != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(_buttonPrefab);
				gameObject.transform.SetParent(_buttonPanel.transform, false);
				buyGoldButton = gameObject.GetComponent<IconLabelButton>();
				_buttons.Add(buyGoldButton);
				gameObject = UnityEngine.Object.Instantiate(_buttonPrefab);
				gameObject.transform.SetParent(_buttonPanel.transform, false);
				buyRubyButton = gameObject.GetComponent<IconLabelButton>();
				_buttons.Add(buyRubyButton);
				gameObject = UnityEngine.Object.Instantiate(_buttonPrefab);
				gameObject.transform.SetParent(_buttonPanel.transform, false);
				upgradeGoldButton = gameObject.GetComponent<IconLabelButton>();
				_buttons.Add(upgradeGoldButton);
				gameObject = UnityEngine.Object.Instantiate(_buttonPrefab);
				gameObject.transform.SetParent(_buttonPanel.transform, false);
				upgradeRubyButton = gameObject.GetComponent<IconLabelButton>();
				_buttons.Add(upgradeRubyButton);
				gameObject = UnityEngine.Object.Instantiate(_buttonPrefab);
				gameObject.transform.SetParent(_buttonPanel.transform, false);
				deliveryRubyButton = gameObject.GetComponent<IconLabelButton>();
				_buttons.Add(deliveryRubyButton);
				gameObject = UnityEngine.Object.Instantiate(_buttonPrefab);
				gameObject.transform.SetParent(_buttonPanel.transform, false);
				consumableButton = gameObject.GetComponent<IconLabelButton>();
				_buttons.Add(consumableButton);
				gameObject = UnityEngine.Object.Instantiate(_buttonPrefab);
				gameObject.transform.SetParent(_buttonPanel.transform, false);
				realMoneyButton = gameObject.GetComponent<IconLabelButton>();
				_buttons.Add(realMoneyButton);
			}
			if (buyGoldButton != null)
			{
				buyGoldButton.get_Icon().set_SpriteName(ListSF.GetRoster().GetCoinIcon());
				buyGoldButton.SetColor(LabelButton.ButtonColor.BUTTON_GREEN);
				buyGoldButton.SetText("0");
				buyGoldButton.ButtonId = 1;
				buyGoldButton.AddEventListener(2, OnItemActionClicked);
			}
			if (buyRubyButton != null)
			{
				buyRubyButton.get_Icon().set_SpriteName("TopPanel.ruby");
				buyRubyButton.SetColor(LabelButton.ButtonColor.BUTTON_GREEN);
				buyRubyButton.SetText("0");
				buyRubyButton.ButtonId = 2;
				buyRubyButton.AddEventListener(2, OnItemActionClicked);
			}
			if (upgradeGoldButton != null)
			{
				upgradeGoldButton.get_Icon().set_SpriteName(ListSF.GetRoster().GetCoinIcon());
				upgradeGoldButton.SetColor(LabelButton.ButtonColor.BUTTON_YELLOW);
				upgradeGoldButton.SetText("0");
				upgradeGoldButton.ButtonId = 7;
				upgradeGoldButton.AddEventListener(2, OnItemActionClicked);
			}
			if (upgradeRubyButton != null)
			{
				upgradeRubyButton.get_Icon().set_SpriteName("TopPanel.ruby");
				upgradeRubyButton.SetColor(LabelButton.ButtonColor.BUTTON_YELLOW);
				upgradeRubyButton.SetText("0");
				upgradeRubyButton.ButtonId = 8;
				upgradeRubyButton.AddEventListener(2, OnItemActionClicked);
			}
			if (deliveryRubyButton != null)
			{
				deliveryRubyButton.get_Icon().set_SpriteName("TopPanel.ruby");
				deliveryRubyButton.SetColor(LabelButton.ButtonColor.BUTTON_GREEN);
				deliveryRubyButton.SetText("0");
				deliveryRubyButton.ButtonId = 10;
				deliveryRubyButton.AddEventListener(2, OnItemActionClicked);
			}
			if (consumableButton != null)
			{
				consumableButton.get_Icon().set_SpriteName("TopPanel.ruby");
				consumableButton.SetColor(LabelButton.ButtonColor.BUTTON_GREEN);
				consumableButton.SetText("0");
				consumableButton.ButtonId = 17;
				consumableButton.AddEventListener(2, OnItemActionClicked);
			}
			if (realMoneyButton != null)
			{
				realMoneyButton.get_Icon().gameObject.SetActive(false);
				realMoneyButton.SetColor(LabelButton.ButtonColor.BUTTON_GREEN);
				realMoneyButton.SetText("0.00 USD");
				realMoneyButton.ButtonId = 3;
				realMoneyButton.AddEventListener(2, OnItemActionClicked);
			}
			HideAllButtons();
		}

		private void OnItemActionClicked(object data)
		{
			if (!IsPurchaseBlockedByQuest())
			{
				ItemAction itemAction = (ItemAction)data;
				TradeDialog.TradeAction tradeAction = TradeDialog.TradeAction.A_BUY;
				GameValueType valueType = GameValueType.Gold;
				long num = 0L;
				long price = 0L;
				bool flag = false;
				ListSF.CheckItemType checkType = ListSF.CheckItemType.CHECK_ITEM_NONE;
				Action<object> confirmCallback = null;
				UserItem userItem = ListSF.GetUserItem(currentItem.Name);
				switch (itemAction)
				{
				case ItemAction.Item_Buy_Gold:
					tradeAction = TradeDialog.TradeAction.A_BUY;
					valueType = GameValueType.Gold;
					num = currentItem.GetCoinPrice();
					price = 0L;
					confirmCallback = OnBuyGoldConfirmed;
					flag = num > ListSF.GetRoster().GetMoney();
					checkType = ListSF.CheckItemType.CHECK_ITEM_MONEY;
					break;
				case ItemAction.Item_Buy_Ruby:
					tradeAction = TradeDialog.TradeAction.A_BUY;
					valueType = GameValueType.Gems;
					num = currentItem.GetGemPrice();
					price = 0L;
					confirmCallback = OnBuyRubyConfirmed;
					flag = num > ListSF.GetRoster().GetBonus();
					checkType = ListSF.CheckItemType.CHECK_ITEM_BONUS;
					break;
				case ItemAction.Item_Upgrade_Gold:
					tradeAction = TradeDialog.TradeAction.A_UPGRADE;
					valueType = GameValueType.Gold;
					num = userItem.GetNextUpgradeItem().GetCoinPrice();
					price = 0L;
					confirmCallback = OnUpgradeGoldConfirmed;
					flag = num > ListSF.GetRoster().GetMoney();
					checkType = ListSF.CheckItemType.CHECK_ITEM_MONEY;
					break;
				case ItemAction.Item_Upgrade_Ruby:
					tradeAction = TradeDialog.TradeAction.A_UPGRADE;
					valueType = GameValueType.Gems;
					num = userItem.GetNextUpgradeItem().GetGemPrice();
					price = 0L;
					confirmCallback = OnUpgradeRubyConfirmed;
					flag = num > ListSF.GetRoster().GetBonus();
					checkType = ListSF.CheckItemType.CHECK_ITEM_BONUS;
					break;
				case ItemAction.Item_Delivery_Ruby:
					BuyImmediateDelivery();
					return;
				case ItemAction.Item_Consumable:
					tradeAction = TradeDialog.TradeAction.A_BUY;
					valueType = GameValueType.Gems;
					num = currentItem.GetGemPrice();
					price = 0L;
					confirmCallback = OnConsumableConfirmed;
					flag = num > ListSF.GetRoster().GetBonus();
					checkType = ListSF.CheckItemType.CHECK_ITEM_BONUS;
					break;
				case ItemAction.Item_Buy_Real:
					ShopScene.get_Instance().get_PaymentUI().MakePurchase(currentItem);
					return;
				}
				pendingItem = currentItem;
				if (flag)
				{
					GameUtils.NotifyPurchaseUnsuccessful(currentItem, checkType);
				}
				else
				{
					DialogsOpener.OpenTradeDialog(tradeAction, valueType, num, confirmCallback, price);
				}
			}
		}

		private bool IsPurchaseBlockedByQuest()
		{
			bool result = false;
			QuestParameters questParameters = ListSF.GetInstance().GetQuestParameters();
			FightIDS savedFightIds = questParameters.fightIds;
			questParameters.fightIds = FightIDS.Empty();
			questParameters.fightResult = string.Empty;
			questParameters.raidResult = string.Empty;
			questParameters.purchasedItem = currentItem;
			if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_PREPURCHASE))
			{
				ListSF.GetInstance().RunQuestActions();
				result = true;
			}
			questParameters.fightIds = savedFightIds;
			return result;
		}

		private void OnBuyGoldConfirmed(object data)
		{
			int num = ((data != null) ? ((int)data) : 0);
			if (num > 0 && pendingItem != null)
			{
				if (ItemBuyHelper.BuyItemWithCoins(pendingItem))
				{
					ListSF.GetRoster().GetInventory().EquipItem(pendingItem, true);
				}
				UpdateContent();
			}
		}

		private void OnBuyRubyConfirmed(object data)
		{
			int num = ((data != null) ? ((int)data) : 0);
			if (num > 0 && pendingItem != null)
			{
				if (ItemBuyHelper.BuyItemWithGems(pendingItem))
				{
					ListSF.GetRoster().GetInventory().EquipItem(pendingItem, true);
				}
				UpdateContent();
			}
		}

		private void OnUpgradeGoldConfirmed(object data)
		{
			int num = ((data != null) ? ((int)data) : 0);
			if (num > 0 && pendingItem != null)
			{
				if (ItemBuyHelper.UpgradeItemWithCoins(pendingItem))
				{
					ListSF.GetRoster().GetInventory().EquipItem(pendingItem, true);
				}
				UpdateContent();
			}
		}

		private void OnUpgradeRubyConfirmed(object data)
		{
			int num = ((data != null) ? ((int)data) : 0);
			if (num > 0 && pendingItem != null)
			{
				if (ItemBuyHelper.UpgradeItemWithGems(pendingItem))
				{
					ListSF.GetRoster().GetInventory().EquipItem(pendingItem, true);
				}
				UpdateContent();
			}
		}

		private void BuyImmediateDelivery()
		{
			if (pendingItem != null)
			{
				if (ItemBuyHelper.BuyImmediatelyDelivery(pendingItem))
				{
					ListSF.GetRoster().GetInventory().EquipItem(pendingItem, true);
				}
				UpdateContent();
			}
		}

		private void OnConsumableConfirmed(object data)
		{
			int num = ((data != null) ? ((int)data) : 0);
			if (num > 0 && pendingItem != null)
			{
				bool flag = ItemBuyHelper.BuyConsumableWithGems(pendingItem);
				UpdateContent();
			}
		}

		private void DisableButtons()
		{
			float opacity = 0.7f;
			foreach (IconLabelButton item in _buttons)
			{
				if (item != null)
				{
					item.SetOpacity(opacity);
					item.interactable = false;
				}
			}
		}

		private void EnableButtons()
		{
			float opacity = 1f;
			foreach (IconLabelButton item in _buttons)
			{
				if (item != null)
				{
					item.SetOpacity(opacity);
					item.interactable = true;
				}
			}
		}

		private void HideAllButtons()
		{
			foreach (IconLabelButton item in _buttons)
			{
				if (item != null)
				{
					item.gameObject.SetActive(false);
				}
			}
		}

		private void SetButton(IconLabelButton button, long price, Color color)
		{
			if (price > 0)
			{
				SetButton(button, price.ToString(), color);
			}
		}

		private void SetButton(IconLabelButton button, string text, Color color)
		{
			if (button != null)
			{
				button.gameObject.SetActive(true);
				button.SetText(text);
				button.SetColor(color);
			}
		}

		public void ShowButton()
		{
			if (currentItem != null)
			{
				switch (currentItem.Type)
				{
				case "Consumable":
					ShowConsumableButton();
					break;
				case "RealMoneyItem":
					ShowPaymentButton();
					break;
				default:
					ShowDefaultButton();
					break;
				}
			}
		}

		public void ShowConsumableButton()
		{
			Color priceColor = Color.black;
			if (ListSF.GetRoster().GetBonus() < (ObscuredLong)(currentItem.GemPrice))
			{
				priceColor = Constants.NegativeValueColor;
			}
			SetButton(consumableButton, (ObscuredLong)(currentItem.GemPrice), priceColor);
		}

		public void ShowPaymentButton()
		{
			SetButton(realMoneyButton, currentItem.PriceAmountText + " " + currentItem.CurrencyCode, Color.black);
		}

		public void ShowDefaultButton()
		{
			if (!isOwned)
			{
				Color priceColor = Color.black;
				if (ListSF.GetRoster().GetMoney() < (ObscuredLong)(currentItem.CoinPrice))
				{
					priceColor = Constants.NegativeValueColor;
				}
				SetButton(buyGoldButton, (ObscuredLong)(currentItem.CoinPrice), priceColor);
				Color buyRubyColor = Color.black;
				if (ListSF.GetRoster().GetBonus() < (ObscuredLong)(currentItem.GemPrice))
				{
					buyRubyColor = Constants.NegativeValueColor;
				}
				SetButton(buyRubyButton, (ObscuredLong)(currentItem.GemPrice), buyRubyColor);
			}
			else if (isBeingMade)
			{
				ItemInfo itemInfo = userItem.GetNextUpgradeItem();
				if (itemInfo == null)
				{
					itemInfo = userItem.GetCurrentUpgradeItem();
				}
				Color deliveryRubyColor = Color.black;
				if (ListSF.GetRoster().GetBonus() < (ObscuredLong)(itemInfo.DeliveryGemPrice))
				{
					deliveryRubyColor = Constants.NegativeValueColor;
				}
				SetButton(deliveryRubyButton, (ObscuredLong)(itemInfo.DeliveryGemPrice), deliveryRubyColor);
			}
			else if (isUpgradable && ListSF.GetRoster().GetShowUpgrades())
			{
				ItemInfo upgradeItemInfo = userItem.GetNextUpgradeItem();
				Color upgradeGoldColor = Color.black;
				if (ListSF.GetRoster().GetMoney() < (ObscuredLong)(upgradeItemInfo.CoinPrice))
				{
					upgradeGoldColor = Constants.NegativeValueColor;
				}
				SetButton(upgradeGoldButton, (ObscuredLong)(upgradeItemInfo.CoinPrice), upgradeGoldColor);
				Color upgradeRubyColor = Color.black;
				if (ListSF.GetRoster().GetBonus() < (ObscuredLong)(upgradeItemInfo.GemPrice))
				{
					upgradeRubyColor = Constants.NegativeValueColor;
				}
				SetButton(upgradeRubyButton, (ObscuredLong)(upgradeItemInfo.GemPrice), upgradeRubyColor);
			}
		}

		public void SetDescription()
		{
			if (_description != null)
			{
				_description.gameObject.SetActive(false);
			}
			if (_descriptionDonate != null)
			{
				_descriptionDonate.gameObject.SetActive(false);
			}
			if (currentItem.Type.Equals("Consumable"))
			{
				SetConsumableDescription();
			}
			else if (currentItem.Type.Equals("RealMoneyItem"))
			{
				SetRealMoneyItemDescription();
			}
			else
			{
				SetDefaultDescription();
			}
		}

		public void SetConsumableDescription()
		{
			if (!(_descriptionDonate == null))
			{
				_descriptionDonate.gameObject.SetActive(true);
				_descriptionDonate.SetAlias(currentItem.DescriptionAlias);
			}
		}

		public void SetRealMoneyItemDescription()
		{
			if (!(_descriptionDonate == null))
			{
				string text = string.Empty;
				if ((ObscuredLong)(currentItem.ReceiveBonus) > 0)
				{
					text = string.Format("<quad name={0} size=106 width=1 /> {1}", "TopPanel.ruby", currentItem.ReceiveBonus);
				}
				if ((ObscuredLong)(currentItem.ReceiveGold) > 0)
				{
					text = string.Format("<quad name={0} size=106 width=1 /> {1}", ListSF.GetRoster().GetCoinIcon(), currentItem.ReceiveGold);
				}
				if (!string.IsNullOrEmpty(text))
				{
					_descriptionDonate.gameObject.SetActive(true);
					_descriptionDonate.set_text(text);
				}
			}
		}

		public void SetDefaultDescription()
		{
			if (_description == null)
			{
				return;
			}
			_description.gameObject.SetActive(true);
			if (isBeingMade)
			{
				_description.SetAlias("shopMaking");
			}
			else if (isOrdered)
			{
				_description.SetAlias("shopOrder");
			}
			else if (isUpgradable && ListSF.GetRoster().GetShowUpgrades())
			{
				ItemInfo upgradeItem = ((userItem == null) ? null : userItem.GetNextUpgradeItem());
				if (upgradeItem != null)
				{
					int playerLevel = ListSF.GetRoster().GetLevel();
					int upgradeLevel = upgradeItem.UpgradeLevel;
					UpgradeIndexItem upgradeIndexItem = upgradeItem.GetUpgradeIndexItem(playerLevel, upgradeLevel);
					int num = ((upgradeIndexItem != null) ? upgradeIndexItem.Index : 0);
					if (upgradeIndexItem.Type == UpgradeIndexItem.UpgradeIndexType.UPGRADE_INDEX_MILESTONE)
					{
						string alias = "shopUpgrade{img::MiscSprites.star}{" + num + "}";
						_description.SetAlias(alias);
					}
					else
					{
						_description.SetAlias("shopUpgrade{}{" + num + "}");
					}
				}
			}
			else
			{
				_description.set_text(string.Empty);
			}
		}

		public void SetItemInfo(ItemInfo item)
		{
			userItem = ListSF.GetRoster().GetInventory().FindItem(item);
			isOwned = userItem != null;
			currentItem = ((!isOwned) ? item : userItem.GetCurrentUpgradeItem());
			isOrdered = InputDeviceExtension.CanOrderDelivery(item, userItem);
			isBeingMade = InputDeviceExtension.IsDeliveryInProgress(item, userItem);
			isUpgradable = InputDeviceExtension.CanUpgradeItem(item, userItem);
			_header.SetAlias(item.Name);
			SetDescription();
			HideAllButtons();
			ShowButton();
			if (item.ItemLevel > ListSF.GetRoster().GetLevel())
			{
				DisableButtons();
			}
			else
			{
				EnableButtons();
			}
			if (_parametersPanel != null)
			{
				ItemInfo upgradeItem = ((userItem == null) ? null : userItem.GetNextUpgradeItem());
				bool showUpgrades = ListSF.GetRoster().GetShowUpgrades() && isOwned && upgradeItem != null;
				_parametersPanel.SetParameters(currentItem, upgradeItem, showUpgrades);
			}
		}

		public void UpdateContent()
		{
			ShopScene.get_Instance().RememberFocus();
			SetItemInfo(currentItem);
			updateEvent.Invoke();
			ShopScene.get_Instance().FocusOnLastFocus();
		}

		public IconLabelButton GetGoldButton()
		{
			return buyGoldButton;
		}
	}
}
