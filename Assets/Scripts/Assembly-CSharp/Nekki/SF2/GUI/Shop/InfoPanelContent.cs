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
				ItemAction pCKPFBFHKJH = (ItemAction)data;
				TradeDialog.TradeAction iBODMPMJELJ = TradeDialog.TradeAction.A_BUY;
				GameValueType bAINMLLIKOL = GameValueType.Gold;
				long num = 0L;
				long cNIOCCCBDBJ = 0L;
				bool flag = false;
				ListSF.CheckItemType dDEDNPLHOJH = ListSF.CheckItemType.CHECK_ITEM_NONE;
				Action<object> oDDEOFKLIAG = null;
				UserItem dKCHDHMLKHN = ListSF.GetUserItem(currentItem.Name);
				switch (pCKPFBFHKJH)
				{
				case ItemAction.Item_Buy_Gold:
					iBODMPMJELJ = TradeDialog.TradeAction.A_BUY;
					bAINMLLIKOL = GameValueType.Gold;
					num = currentItem.GetCoinPrice();
					cNIOCCCBDBJ = 0L;
					oDDEOFKLIAG = OnBuyGoldConfirmed;
					flag = num > ListSF.GetRoster().GetMoney();
					dDEDNPLHOJH = ListSF.CheckItemType.CHECK_ITEM_MONEY;
					break;
				case ItemAction.Item_Buy_Ruby:
					iBODMPMJELJ = TradeDialog.TradeAction.A_BUY;
					bAINMLLIKOL = GameValueType.Gems;
					num = currentItem.GetGemPrice();
					cNIOCCCBDBJ = 0L;
					oDDEOFKLIAG = OnBuyRubyConfirmed;
					flag = num > ListSF.GetRoster().GetBonus();
					dDEDNPLHOJH = ListSF.CheckItemType.CHECK_ITEM_BONUS;
					break;
				case ItemAction.Item_Upgrade_Gold:
					iBODMPMJELJ = TradeDialog.TradeAction.A_UPGRADE;
					bAINMLLIKOL = GameValueType.Gold;
					num = dKCHDHMLKHN.GetNextUpgradeItem().GetCoinPrice();
					cNIOCCCBDBJ = 0L;
					oDDEOFKLIAG = OnUpgradeGoldConfirmed;
					flag = num > ListSF.GetRoster().GetMoney();
					dDEDNPLHOJH = ListSF.CheckItemType.CHECK_ITEM_MONEY;
					break;
				case ItemAction.Item_Upgrade_Ruby:
					iBODMPMJELJ = TradeDialog.TradeAction.A_UPGRADE;
					bAINMLLIKOL = GameValueType.Gems;
					num = dKCHDHMLKHN.GetNextUpgradeItem().GetGemPrice();
					cNIOCCCBDBJ = 0L;
					oDDEOFKLIAG = OnUpgradeRubyConfirmed;
					flag = num > ListSF.GetRoster().GetBonus();
					dDEDNPLHOJH = ListSF.CheckItemType.CHECK_ITEM_BONUS;
					break;
				case ItemAction.Item_Delivery_Ruby:
					BuyImmediateDelivery();
					return;
				case ItemAction.Item_Consumable:
					iBODMPMJELJ = TradeDialog.TradeAction.A_BUY;
					bAINMLLIKOL = GameValueType.Gems;
					num = currentItem.GetGemPrice();
					cNIOCCCBDBJ = 0L;
					oDDEOFKLIAG = OnConsumableConfirmed;
					flag = num > ListSF.GetRoster().GetBonus();
					dDEDNPLHOJH = ListSF.CheckItemType.CHECK_ITEM_BONUS;
					break;
				case ItemAction.Item_Buy_Real:
					ShopScene.get_Instance().get_PaymentUI().MakePurchase(currentItem);
					return;
				}
				pendingItem = currentItem;
				if (flag)
				{
					GameUtils.NotifyPurchaseUnsuccessful(currentItem, dDEDNPLHOJH);
				}
				else
				{
					DialogsOpener.OpenTradeDialog(iBODMPMJELJ, bAINMLLIKOL, num, oDDEOFKLIAG, cNIOCCCBDBJ);
				}
			}
		}

		private bool IsPurchaseBlockedByQuest()
		{
			bool result = false;
			QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
			FightIDS jLGLBLDPAAF = hHKLFIIBIFF.fightIds;
			hHKLFIIBIFF.fightIds = FightIDS.Empty();
			hHKLFIIBIFF.fightResult = string.Empty;
			hHKLFIIBIFF.raidResult = string.Empty;
			hHKLFIIBIFF.purchasedItem = currentItem;
			if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_PREPURCHASE))
			{
				ListSF.GetInstance().RunQuestActions();
				result = true;
			}
			hHKLFIIBIFF.fightIds = jLGLBLDPAAF;
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

		private void SetButton(IconLabelButton AMACDAACGCA, long GGPEGMLPBKA, Color OHJKNABLCMF)
		{
			if (GGPEGMLPBKA > 0)
			{
				SetButton(AMACDAACGCA, GGPEGMLPBKA.ToString(), OHJKNABLCMF);
			}
		}

		private void SetButton(IconLabelButton AMACDAACGCA, string NGEPNAJJHCD, Color OHJKNABLCMF)
		{
			if (AMACDAACGCA != null)
			{
				AMACDAACGCA.gameObject.SetActive(true);
				AMACDAACGCA.SetText(NGEPNAJJHCD);
				AMACDAACGCA.SetColor(OHJKNABLCMF);
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
			Color oHJKNABLCMF = Color.black;
			if (ListSF.GetRoster().GetBonus() < (ObscuredLong)(currentItem.GemPrice))
			{
				oHJKNABLCMF = Constants.NegativeValueColor;
			}
			SetButton(consumableButton, (ObscuredLong)(currentItem.GemPrice), oHJKNABLCMF);
		}

		public void ShowPaymentButton()
		{
			SetButton(realMoneyButton, currentItem.PriceAmountText + " " + currentItem.CurrencyCode, Color.black);
		}

		public void ShowDefaultButton()
		{
			if (!isOwned)
			{
				Color oHJKNABLCMF = Color.black;
				if (ListSF.GetRoster().GetMoney() < (ObscuredLong)(currentItem.CoinPrice))
				{
					oHJKNABLCMF = Constants.NegativeValueColor;
				}
				SetButton(buyGoldButton, (ObscuredLong)(currentItem.CoinPrice), oHJKNABLCMF);
				Color oHJKNABLCMF2 = Color.black;
				if (ListSF.GetRoster().GetBonus() < (ObscuredLong)(currentItem.GemPrice))
				{
					oHJKNABLCMF2 = Constants.NegativeValueColor;
				}
				SetButton(buyRubyButton, (ObscuredLong)(currentItem.GemPrice), oHJKNABLCMF2);
			}
			else if (isBeingMade)
			{
				ItemInfo dJKEECEOCJB = userItem.GetNextUpgradeItem();
				if (dJKEECEOCJB == null)
				{
					dJKEECEOCJB = userItem.GetCurrentUpgradeItem();
				}
				Color oHJKNABLCMF3 = Color.black;
				if (ListSF.GetRoster().GetBonus() < (ObscuredLong)(dJKEECEOCJB.DeliveryGemPrice))
				{
					oHJKNABLCMF3 = Constants.NegativeValueColor;
				}
				SetButton(deliveryRubyButton, (ObscuredLong)(dJKEECEOCJB.DeliveryGemPrice), oHJKNABLCMF3);
			}
			else if (isUpgradable && ListSF.GetRoster().GetShowUpgrades())
			{
				ItemInfo dJKEECEOCJB2 = userItem.GetNextUpgradeItem();
				Color oHJKNABLCMF4 = Color.black;
				if (ListSF.GetRoster().GetMoney() < (ObscuredLong)(dJKEECEOCJB2.CoinPrice))
				{
					oHJKNABLCMF4 = Constants.NegativeValueColor;
				}
				SetButton(upgradeGoldButton, (ObscuredLong)(dJKEECEOCJB2.CoinPrice), oHJKNABLCMF4);
				Color oHJKNABLCMF5 = Color.black;
				if (ListSF.GetRoster().GetBonus() < (ObscuredLong)(dJKEECEOCJB2.GemPrice))
				{
					oHJKNABLCMF5 = Constants.NegativeValueColor;
				}
				SetButton(upgradeRubyButton, (ObscuredLong)(dJKEECEOCJB2.GemPrice), oHJKNABLCMF5);
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
				ItemInfo dJKEECEOCJB = ((userItem == null) ? null : userItem.GetNextUpgradeItem());
				if (dJKEECEOCJB != null)
				{
					int oMHDLKNHNMJ = ListSF.GetRoster().GetLevel();
					int oBJDGBBFJOO = dJKEECEOCJB.UpgradeLevel;
					UpgradeIndexItem aACAFOBANOH = dJKEECEOCJB.GetUpgradeIndexItem(oMHDLKNHNMJ, oBJDGBBFJOO);
					int num = ((aACAFOBANOH != null) ? aACAFOBANOH.Index : 0);
					if (aACAFOBANOH.Type == UpgradeIndexItem.UpgradeIndexType.UPGRADE_INDEX_MILESTONE)
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
				ItemInfo dJKEECEOCJB = ((userItem == null) ? null : userItem.GetNextUpgradeItem());
				bool oGMLCLNEAIJ = ListSF.GetRoster().GetShowUpgrades() && isOwned && dJKEECEOCJB != null;
				_parametersPanel.SetParameters(currentItem, dJKEECEOCJB, oGMLCLNEAIJ);
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
