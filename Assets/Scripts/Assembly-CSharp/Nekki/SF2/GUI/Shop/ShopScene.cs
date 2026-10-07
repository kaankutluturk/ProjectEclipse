using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using CodeStage.AntiCheat.ObscuredTypes;
using DG.Tweening;
using Nekki.SF2.Core.Fights;
using Nekki.SF2.GUI.Common;
using Nekki.SF2.GUI.Menu;
using UnityEngine;

namespace Nekki.SF2.GUI.Shop
{
	public class ShopScene : Scene<ShopScene>, ITableViewDataSource, ITableViewDelegate
	{
		[SerializeField]
		private Vector2 _weaponImageSize = new Vector2(778f, 302f);

		[SerializeField]
		private Vector2 _armorImageSize = new Vector2(778f, 704f);

		[SerializeField]
		private Vector2 _helmetImageSize = new Vector2(778f, 706f);

		[SerializeField]
		private Vector2 _rangedImageSize = new Vector2(778f, 302f);

		[SerializeField]
		private Vector2 _magicImageSize = new Vector2(778f, 682f);

		[SerializeField]
		private Vector2 _paymentImageSize = new Vector2(670f, 500f);

		[SerializeField]
		private Vector2 _freeImageSize = new Vector2(670f, 500f);

		[SerializeField]
		private SidePanel _itemInfo;

		[SerializeField]
		private SidePanel _itemParam;

		[SerializeField]
		private SidePanel _itemProperties;

		[SerializeField]
		private ButtonPanel _buttonPanel;

		[SerializeField]
		private LabelAlias _noItemsMessage;

		[SerializeField]
		private MainMenu _mainMenu;

		[SerializeField]
		private LabelButton _tryItemButton;

		[SerializeField]
		private HintPanel _hintPanel;

		[SerializeField]
		private ModelContainer _modelContainer;

		[SerializeField]
		private CanvasGroup _shopUIGroup;

		private Eclipse.Forge.ShopForgeController _forgeController;

		[SerializeField]
		private GameObject _infoPanelContentPrefab;

		[SerializeField]
		private GameObject _ParametersPanelContentPrefab;

		[SerializeField]
		private GameObject _propertiesPanelContentPrefab;

		[SerializeField]
		private GameObject _cheatsPanelPrefab;

		[SerializeField]
		private TableView _shopTableView;

		[SerializeField]
		private GameObject _cellPrefab;

		[SerializeField]
		private SpriteRenderer _backgroundLeft;

		[SerializeField]
		private SpriteRenderer _backgroundRight;

		private PaymentUI _paymentUi;

		private List<ItemInfo> _weaponItems = new List<ItemInfo>();

		private List<ItemInfo> _armorItems = new List<ItemInfo>();

		private List<ItemInfo> _helmetItems = new List<ItemInfo>();

		private List<ItemInfo> _rangedItems = new List<ItemInfo>();

		private List<ItemInfo> _magicItems = new List<ItemInfo>();

		private List<ItemInfo> _paymentItems = new List<ItemInfo>();

		private List<ItemInfo> _freeItems = new List<ItemInfo>();

		private InfoPanelContent _infoPanelContent;

		private ParametersPanelContent _parametersPanelContent;

		private PropertiesPanelContent _propertiesPanelContent;

		private CheatsPanel _CheatsPanel;

		private DelayedStrike _delayedStrike;

		private string _leftFlagImage = "ShopPieces.Left_flag";

		private string _rightFlagImage = "ShopPieces.Right_flag";

		private string _leftFlagPropertiesImage = "ShopPieces.Left_flag_properties";

		private string _rightFlagPropertiesImage = "ShopPieces.Right_flag_properties";

		private string _leftFlagPropertiesNoneImage = "ShopPieces.Left_flag_properties_none";

		private string _rightFlagPropertiesNoneImage = "ShopPieces.Right_flag_properties_none";

		private float _unusedFullAlpha = 1f;

		private float _unusedHalfAlpha = 0.5f;

		private int _unusedSize120 = 120;

		private float _shownOpacity = 1f;

		private float _hiddenOpacity;

		private float _fadeDuration = 0.7f;

		private int _newItemsCount;

		private ShopSection _currentSection = ShopSection.Unknown;

		private static Dictionary<ShopSection, ItemInfo> _lastFocusedItems = new Dictionary<ShopSection, ItemInfo>();

		private List<ItemInfo> _currentItems = new List<ItemInfo>();

		private bool _iconPanelActive = true;

		private Vector2 _cellBaseSize = new Vector2(0f, 0f);

		private ShopTableViewCell _selectedCell;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static ShopScene _instance;

		public PaymentUI PaymentPanel
		{
			get
			{
				return get_PaymentUI();
			}
		}

		public override ScreenType SceneType
		{
			get
			{
				return get_SceneId();
			}
		}

		public static ShopScene SceneInstance
		{
			get
			{
				return get_Instance();
			}
			set
			{
				set_Instance(value);
			}
		}

		public PaymentUI get_PaymentUI()
		{
			return _paymentUi;
		}

		public override ScreenType get_SceneId()
		{
			return ScreenType.ModuleShop;
		}

		public static ShopScene get_Instance()
		{
			return _instance;
		}

		public static void set_Instance(ShopScene value)
		{
			_instance = value;
		}

		protected override void Init(object data)
		{
			base.Init(data);
			_delayedStrike = data as DelayedStrike;
			set_Instance(this);
			Eclipse.Rendering.ShopDojoBackdrop.Attach(gameObject, _backgroundLeft, _backgroundRight);
			_paymentUi = GetModule<PaymentUI>();
			_paymentUi.get_OnProductsUpdateEvent().AddListener(OnProductsUpdated);
			if (ListSF.GetRoster() != null && ListSF.GetRoster().GetInventory() != null)
			{
				ListSF.GetRoster().GetInventory().ItemsDelivered.AddListener(OnUserItemsChanged);
			}
			if (_mainMenu != null)
			{
				_mainMenu.Init();
			}
			if (_hintPanel != null)
			{
				_hintPanel.Init();
			}
			if (_tryItemButton != null)
			{
				_tryItemButton.onClick.AddListener(OnTryButtonClicked);
			}
			if (_infoPanelContentPrefab != null)
			{
				GameObject gameObject = Object.Instantiate(_infoPanelContentPrefab);
				_infoPanelContent = gameObject.GetComponent<InfoPanelContent>();
				_infoPanelContent.Init();
				_infoPanelContent.updateEvent.AddListener(OnItemInfoUpdated);
			}
			if (_ParametersPanelContentPrefab != null)
			{
				GameObject gameObject2 = Object.Instantiate(_ParametersPanelContentPrefab);
				_parametersPanelContent = gameObject2.GetComponent<ParametersPanelContent>();
				_parametersPanelContent.Init();
			}
			if (_propertiesPanelContentPrefab != null)
			{
				GameObject gameObject3 = Object.Instantiate(_propertiesPanelContentPrefab);
				_propertiesPanelContent = gameObject3.GetComponent<PropertiesPanelContent>();
				_propertiesPanelContent.Init();
				if (_propertiesPanelContent.get_PropertiesPanel() != null && _hintPanel != null)
				{
					_propertiesPanelContent.get_PropertiesPanel().onPerksClick.AddListener(_hintPanel.ShowPerkHint);
				}
			}
			if (_itemInfo != null)
			{
				bool isPanelMovable = false;
				_itemInfo.Init(_infoPanelContent, isPanelMovable);
			}
			if (_itemParam != null)
			{
				bool jOJGKNGGAHB2 = true;
				bool isPanelOpen = false;
				float buttonOffsetY = -40f;
				_itemParam.Init(_parametersPanelContent, jOJGKNGGAHB2, buttonOffsetY, isPanelOpen, _rightFlagImage, _leftFlagImage);
			}
			if (_itemProperties != null)
			{
				bool jOJGKNGGAHB3 = true;
				bool nKGDKKNNJOF2 = false;
				float mDPGKEDBHNO2 = 40f;
				_itemProperties.Init(_propertiesPanelContent, jOJGKNGGAHB3, mDPGKEDBHNO2, nKGDKKNNJOF2, _rightFlagPropertiesNoneImage, _leftFlagPropertiesNoneImage);
			}
			if (_buttonPanel != null)
			{
				_buttonPanel.Init();
				if (ListSF.GetItems().GetFreeItems().Count == 0)
				{
					_buttonPanel.HideButton(6);
				}
			}
			LoadSectionItems();
			_shopTableView.set_CellPrefab(_cellPrefab);
			_shopTableView.Init(this, this);
			_shopTableView.onSelectCell.AddListener(OnCellSelected);
			_shopTableView.set_MinScrollVelocity(100f);
			_shopTableView.get_Scroll().onDragBegin.AddListener(_hintPanel.HideHintAndStopCorutine);
			if (_tryItemButton != null)
			{
				Transform forgeParent = _shopUIGroup != null ? _shopUIGroup.transform : transform;
				_forgeController = new Eclipse.Forge.ShopForgeController(this, _mainMenu, _tryItemButton, forgeParent, _propertiesPanelContent, _shopTableView != null ? _shopTableView.transform.parent as RectTransform : null, _itemParam != null ? _itemParam.transform as RectTransform : null, _itemProperties != null ? _itemProperties.transform as RectTransform : null, _hintPanel);
			}
			if (_modelContainer != null)
			{
				_modelContainer.Init();
			}
			if (SystemProperties.IsDebug() && _cheatsPanelPrefab != null)
			{
				_CheatsPanel = Object.Instantiate(_cheatsPanelPrefab).GetComponent<CheatsPanel>();
				Transform parent = ((!(_shopUIGroup != null)) ? base.transform : _shopUIGroup.transform);
				_CheatsPanel.transform.SetParent(parent, false);
				_CheatsPanel.OnShowCheats.AddListener(OnShowCheats);
				_CheatsPanel.OnAddLevel.AddListener(OnCheatLevelAdded);
				_CheatsPanel.HideCheats();
			}
			SetFocusOnStart();
			UpdateModel(ListSF.GetRoster().get_Parameters().Weapon);
			ShowUI();
		}

		protected override void OnSceneClosed()
		{
			_forgeController?.Shutdown();
			_forgeController = null;
			RememberFocus();
			UpdateNewItemsCounters();
			_paymentUi.get_OnProductsUpdateEvent().RemoveListener(OnProductsUpdated);
			set_Instance(null);
			if (ListSF.GetRoster() != null && ListSF.GetRoster().GetInventory() != null)
			{
				ListSF.GetRoster().GetInventory().ItemsDelivered.RemoveListener(OnUserItemsChanged);
			}
			base.OnSceneClosed();
		}

		protected void OnCellSelected(TableViewCell cell)
		{
			ShopTableViewCell shopTableViewCell = cell as ShopTableViewCell;
			if (!(shopTableViewCell == null))
			{
				if (_selectedCell != null)
				{
				}
				_selectedCell = shopTableViewCell;
				UpdatePropertiesPanel(shopTableViewCell);
				UpdateParametersForCell(shopTableViewCell);
				if (_infoPanelContent != null)
				{
					_infoPanelContent.SetItemInfo(shopTableViewCell.get_ItemInfo());
				}
				if (_tryItemButton != null)
				{
					UpdateTryButton(shopTableViewCell.get_ItemInfo());
				}
				_forgeController?.OnItemSelected(shopTableViewCell.get_ItemInfo());
			}
		}

		private void OnProductsUpdated()
		{
			if (_selectedCell != null && _infoPanelContent != null)
			{
				_infoPanelContent.SetItemInfo(_selectedCell.get_ItemInfo());
			}
		}

		private void UpdatePropertiesPanel(ShopTableViewCell cell)
		{
			if (!(cell == null) && !(_itemProperties == null) && !(_propertiesPanelContent == null))
			{
				ItemInfo itemInfo = cell.get_ItemInfo();
				if (itemInfo != null && itemInfo.DefaultEnchantmentPreviews.Count > 0)
				{
					_itemProperties.set_OpenImage(_rightFlagPropertiesImage);
					_itemProperties.set_CloseImage(_leftFlagPropertiesImage);
				}
				else
				{
					_itemProperties.set_OpenImage(_rightFlagPropertiesNoneImage);
					_itemProperties.set_CloseImage(_leftFlagPropertiesNoneImage);
				}
				_propertiesPanelContent.SetItemInfo(itemInfo);
			}
		}

		private void UpdateParametersForCell(ShopTableViewCell cell)
		{
			if (cell != null && cell.get_ItemInfo() != null && _parametersPanelContent != null)
			{
				_parametersPanelContent.UpdateParameters(cell.get_ItemInfo());
			}
		}

		private void UpdateParametersAnimated()
		{
			if (_parametersPanelContent != null && _selectedCell != null && _selectedCell.get_ItemInfo() != null)
			{
				_parametersPanelContent.UpdateParametersWithDuration(_selectedCell.get_ItemInfo());
			}
		}

		public void DisableButton(ShopSection section)
		{
			if (_buttonPanel != null)
			{
				_buttonPanel.DisableButton((int)section);
			}
		}

		private void OnShowCheats()
		{
			if (_buttonPanel != null)
			{
				_buttonPanel.EnableAllButtons();
			}
			if (_tryItemButton != null)
			{
				_tryItemButton.gameObject.SetActive(false);
			}
			ShowSidePanels(false);
		}

		public void RefreshItems()
		{
			ReloadItems();
			UpdateNewItemsCounters();
		}

		private void OnCheatLevelAdded()
		{
			ReloadItems();
			UpdateNewItemsCounters();
		}

		public void SetFocusOnStart()
		{
			if (_delayedStrike != null)
			{
				GoToSlider(_delayedStrike.SliderType);
				if (_delayedStrike.Item != null)
				{
					ScrollToItem(_delayedStrike.Item);
				}
			}
			else
			{
				GoToSlider(SliderType.SliderWeapon);
			}
		}

		public void RememberFocus()
		{
			if (_selectedCell != null && _selectedCell.get_ItemInfo() != null)
			{
				_lastFocusedItems[_currentSection] = _selectedCell.get_ItemInfo();
			}
		}

		public void SetFocus()
		{
			if (!FocusOnNewItem() && !FocusOnLastFocus() && !FocusOnEquipedItem() && _shopTableView != null)
			{
				_shopTableView.ScrollToCell(0);
			}
		}

		public bool FocusOnNewItem()
		{
			ItemInfo newestItem = null;
			_currentItems.ForEach((ItemInfo entry) =>
			{
				if (entry.GetIsNew() && (newestItem == null || (ObscuredLong)(newestItem.GemPrice) < (ObscuredLong)(entry.GemPrice) || ((ObscuredLong)(newestItem.GemPrice) == (ObscuredLong)(entry.GemPrice) && (ObscuredLong)(newestItem.CoinPrice) < (ObscuredLong)(entry.CoinPrice))))
				{
					newestItem = entry;
				}
			});
			if (newestItem != null)
			{
				ScrollToItem(newestItem);
				return true;
			}
			return false;
		}

		public bool FocusOnLastFocus()
		{
			ItemInfo foundItem = null;
			if (_lastFocusedItems.ContainsKey(_currentSection))
			{
				ItemInfo lastFocusedItem = _lastFocusedItems[_currentSection];
				foundItem = _currentItems.Find((ItemInfo entry) => entry.Name.Equals(lastFocusedItem.Name));
			}
			if (foundItem != null)
			{
				ScrollToItem(foundItem);
				return true;
			}
			return false;
		}

		public bool FocusOnEquipedItem()
		{
			List<UserItem> list = ListSF.GetRoster().GetInventory().GetItems()
				.FindAll((UserItem userItemEntry) => userItemEntry.GetIsEquipped());
			ItemInfo itemInfo = null;
			foreach (ItemInfo item in _currentItems)
			{
				UserItem equippedItem = list.Find((UserItem userItemEntry) => userItemEntry.GetInfo() != null && userItemEntry.GetInfo().Name.Equals(item.Name));
				if (equippedItem != null)
				{
					itemInfo = equippedItem.GetInfo();
					break;
				}
			}
			if (itemInfo != null)
			{
				ScrollToItem(itemInfo);
				return true;
			}
			return false;
		}

		public void ScrollToItem(ItemInfo item)
		{
			ItemInfo itemInfo = _currentItems.Find((ItemInfo entry) => entry.Name.Equals(item.Name));
			if (itemInfo != null && _shopTableView != null)
			{
				int cellIndex = _currentItems.IndexOf(itemInfo);
				_shopTableView.ScrollToCell(cellIndex);
			}
		}

		public void ScrollToItemByName(SliderType sliderType, string itemName)
		{
			GoToSlider(sliderType);
			ItemInfo itemInfo = _currentItems.Find((ItemInfo entry) => entry.Name == itemName);
			if (itemInfo != null && _shopTableView != null)
			{
				int cellIndex = _currentItems.IndexOf(itemInfo);
				_shopTableView.ScrollToCell(cellIndex);
			}
		}

		public void ScrollToItemByName(ShopSection section, string itemName)
		{
			SetShopSection(section);
			ItemInfo itemInfo = _currentItems.Find((ItemInfo entry) => entry.Name.Equals(itemName));
			if (itemInfo != null && _shopTableView != null)
			{
				int cellIndex = _currentItems.IndexOf(itemInfo);
				_shopTableView.ScrollToCell(cellIndex);
			}
		}

		public void GoToSlider(SliderType sliderType)
		{
			switch (sliderType)
			{
			case SliderType.SliderWeapon:
				SetShopSection(ShopSection.Weapon);
				break;
			case SliderType.SliderArmor:
				SetShopSection(ShopSection.Armor);
				break;
			case SliderType.SliderHelmet:
				SetShopSection(ShopSection.Helmet);
				break;
			case SliderType.SliderMissile:
				SetShopSection(ShopSection.Ranged);
				break;
			case SliderType.SliderMagic:
				SetShopSection(ShopSection.Magic);
				break;
			case SliderType.SliderRuby:
				SetShopSection(ShopSection.Payment);
				break;
			case SliderType.SliderFree:
				SetShopSection(ShopSection.Free);
				break;
			default:
				SetShopSection(ShopSection.Weapon);
				break;
			}
		}

		private void LoadSectionItems()
		{
			AddAvailableItems(ListSF.GetItems().GetWeapons(), _weaponItems);
			AddAvailableItems(ListSF.GetItems().GetArmors(), _armorItems);
			AddAvailableItems(ListSF.GetItems().GetHelms(), _helmetItems);
			AddAvailableItems(ListSF.GetItems().GetRangedWeapons(), _rangedItems);
			AddAvailableItems(ListSF.GetItems().GetMagicItems(), _magicItems);
			AddAvailableItems(ListSF.GetItems().GetRealMoneyItems(), _paymentItems);
			AddAvailableItems(ListSF.GetItems().GetConsumables(), _paymentItems);
			AddAvailableItems(ListSF.GetItems().GetFreeItems(), _freeItems);
		}

		private void AddAvailableItems(List<ItemInfo> sourceItems, List<ItemInfo> availableItems)
		{
			foreach (ItemInfo item in sourceItems)
			{
				if (ShopAvailabilityPolicy.IsAvailable(item, ListSF.GetRoster()))
				{
					availableItems.Add(item);
				}
			}
		}

		public void SetShopSection(ShopSection section)
		{
			if (_currentSection == section)
			{
				return;
			}
			RememberFocus();
			_currentSection = section;
			ShowSidePanels(true);
			UpdateNewItemsCounters();
			if (_buttonPanel != null)
			{
				_buttonPanel.EnableAllButtons();
			}
			if (_noItemsMessage != null)
			{
				_noItemsMessage.SetAlias(string.Empty);
				_noItemsMessage.set_text(string.Empty);
			}
			if (_CheatsPanel != null)
			{
				_CheatsPanel.HideCheats();
			}
			switch (section)
			{
			case ShopSection.Weapon:
				SetSectionItems(_weaponItems, 0f, _weaponImageSize);
				DisableButton(ShopSection.Weapon);
				break;
			case ShopSection.Armor:
				SetSectionItems(_armorItems, 110f, _armorImageSize);
				DisableButton(ShopSection.Armor);
				break;
			case ShopSection.Helmet:
				SetSectionItems(_helmetItems, 110f, _helmetImageSize);
				DisableButton(ShopSection.Helmet);
				break;
			case ShopSection.Ranged:
				SetSectionItems(_rangedItems, 0f, _rangedImageSize);
				DisableButton(ShopSection.Ranged);
				if (_noItemsMessage != null)
				{
					_noItemsMessage.SetAlias("shopRangedLocked");
				}
				break;
			case ShopSection.Magic:
				SetSectionItems(_magicItems, 120f, _magicImageSize);
				DisableButton(ShopSection.Magic);
				if (_noItemsMessage != null)
				{
					_noItemsMessage.SetAlias("shopMagicLocked");
				}
				break;
			case ShopSection.Payment:
				SetSectionItems(_paymentItems, 20f, _paymentImageSize, false);
				DisableButton(ShopSection.Payment);
				break;
			case ShopSection.Free:
				SetSectionItems(_freeItems, 20f, _freeImageSize, false);
				DisableButton(ShopSection.Free);
				break;
			}
			if (_modelContainer != null && _modelContainer.get__StageType() == StageType.Stage.STAGE_SHOP_TRY_ON)
			{
				OnTryOnFinished(null);
			}
			SetFocus();
		}

		public void UpdateNewItemsCounters()
		{
			if (_currentItems != null)
			{
				_currentItems.ForEach((ItemInfo entry) =>
				{
					entry.SetIsNew(false);
				});
			}
			int num = ListSF.GetItems().GetNewItemsCount();
			if (_newItemsCount != num)
			{
				_newItemsCount = num;
				if (_buttonPanel != null)
				{
					_buttonPanel.UpdateNewItemsCounter();
				}
				if (_mainMenu != null)
				{
					_mainMenu.UpdateNewItems();
				}
				ListSF.GetInstance().RequestSave();
			}
		}

		private void SetSectionItems(List<ItemInfo> items, float spacing, Vector2 baseSize, bool isIconPanelActive = true)
		{
			if (items == null || _shopTableView == null)
			{
				GameLog.Error("ShopScene.SetItems some field is null");
				return;
			}
			SortItems(items);
			_currentItems = items;
			_iconPanelActive = isIconPanelActive;
			_cellBaseSize = baseSize;
			_shopTableView.set_Spacing(spacing);
			_shopTableView.ReloadData();
			_shopTableView.ScrollToCell(0);
			if (_noItemsMessage != null)
			{
				bool active = _currentItems.Count == 0;
				_noItemsMessage.gameObject.SetActive(active);
			}
			if (_tryItemButton != null && _currentItems.Count == 0)
			{
				_tryItemButton.gameObject.SetActive(false);
			}
			ShowSidePanels(_currentItems.Count > 0);
		}

		private void SortItems(List<ItemInfo> items)
		{
			items.Sort((ItemInfo itemA, ItemInfo itemB) =>
			{
				UserItem userItemA = ListSF.GetRoster().GetInventory().FindItem(itemA);
				UserItem dKCHDHMLKHN2 = ListSF.GetRoster().GetInventory().FindItem(itemB);
				int num = ((userItemA == null) ? itemA.UpgradeLevel : userItemA.GetUpgradeLevel());
				int num2 = ((dKCHDHMLKHN2 == null) ? itemB.UpgradeLevel : dKCHDHMLKHN2.GetUpgradeLevel());
				int num3 = items.IndexOf(itemA);
				int value = items.IndexOf(itemB);
				return (num == num2) ? num3.CompareTo(value) : num.CompareTo(num2);
			});
		}

		private void ReloadItems()
		{
			ShopTableViewCell focusedCell = _selectedCell;
			SortItems(_currentItems);
			_shopTableView.ReloadData();
			if (_shopTableView != null && focusedCell != null && focusedCell != _selectedCell)
			{
				_shopTableView.ScrollToCell(focusedCell.get_RowNumber());
			}
		}

		private void OnScrollItemClicked(ShopScrollItem item)
		{
			if (item == _shopTableView.get_SelectedCell())
			{
				OnTryButtonClicked();
			}
		}

		private void OnItemInfoUpdated()
		{
			UpdateModelIfItemChanged();
			ReloadItems();
			UpdateParametersAnimated();
			if (_selectedCell != null && _selectedCell.get_ItemInfo() != null)
			{
				UpdateTryButton(_selectedCell.get_ItemInfo());
			}
			if (_mainMenu != null)
			{
				_mainMenu.UpdateMoney();
			}
		}

		private bool IsSelectedItemTryable()
		{
			ItemInfo itemInfo = _selectedCell.get_ItemInfo();
			return itemInfo.Type.Equals("Weapon") || itemInfo.Type.Equals("Armor") || itemInfo.Type.Equals("Helm") || itemInfo.Type.Equals("Ranged") || itemInfo.Type.Equals("Magic");
		}

		private void UpdateTryButton(ItemInfo item)
		{
			if (!(_tryItemButton == null) && item != null)
			{
				if (IsSelectedItemTryable())
				{
					_tryItemButton.gameObject.SetActive(true);
				}
				else
				{
					_tryItemButton.gameObject.SetActive(false);
				}
				bool isOwned = false;
				bool isEquipped = false;
				InputDeviceExtension.GetOwnedAndEquippedState(ref isOwned, ref isEquipped, item);
				if (isEquipped)
				{
					_tryItemButton.SetAlias("btnShopUnequip");
				}
				else if (isOwned)
				{
					_tryItemButton.SetAlias("btnShopEquip");
				}
				else
				{
					_tryItemButton.SetAlias("btnShopTry");
				}
			}
		}

		private void OnTryButtonClicked()
		{
			if (!IsSelectedItemTryable() || _selectedCell.get_ItemInfo() == null)
			{
				return;
			}
			ItemInfo itemInfo = _selectedCell.get_ItemInfo();
			bool isOwned = false;
			bool isEquipped = false;
			InputDeviceExtension.GetOwnedAndEquippedState(ref isOwned, ref isEquipped, itemInfo);
			if (isOwned && isEquipped)
			{
				ListSF.GetRoster().GetInventory().ResetToDefaultItem(itemInfo, true);
				ReloadItems();
				UpdateModelIfItemChanged();
				UpdateParametersAnimated();
				UpdateTryButton(itemInfo);
				return;
			}
			if (isOwned && !isEquipped)
			{
				ListSF.GetRoster().GetInventory().EquipItem(itemInfo, true);
				ReloadItems();
				UpdateModelIfItemChanged(itemInfo);
				UpdateParametersAnimated();
				UpdateTryButton(itemInfo);
				return;
			}
			ShopTableViewCell shopTableViewCell = _shopTableView.get_SelectedCell() as ShopTableViewCell;
			if (shopTableViewCell != null)
			{
				if (_modelContainer != null)
				{
					_modelContainer.AddEventListener(1, OnTryOnFinished);
				}
				UpdateModel(shopTableViewCell.get_ItemInfo(), StageType.Stage.STAGE_SHOP_TRY_ON);
				HideUI();
			}
		}

		private void OnTryOnFinished(object data)
		{
			if (_modelContainer != null)
			{
				_modelContainer.RemoveEventListener(1, OnTryOnFinished);
			}
			ShowUI();
			StartCoroutine(RestoreModelNextFrame());
		}

		private IEnumerator RestoreModelNextFrame()
		{
			yield return new WaitForEndOfFrame();
			UpdateModel(null, StageType.Stage.STAGE_PEACEFUL_RESTORE);
		}

		private void OnUserItemsChanged(List<UserItem> userItems)
		{
			if (_infoPanelContent != null && _selectedCell != null)
			{
				_infoPanelContent.SetItemInfo(_selectedCell.get_ItemInfo());
			}
			ReloadItems();
		}

		public override void UpdateScene(object data)
		{
			if (_infoPanelContent != null)
			{
				_infoPanelContent.UpdateContent();
			}
			_forgeController?.Tick();
		}

		public bool IsForgeCanBeOpened()
		{
			return _forgeController != null && _forgeController.CanOpen();
		}

		public bool OpenForgeAndSetRecipe(string recipeName)
		{
			return _forgeController != null && _forgeController.Open(recipeName);
		}

		public void RestoreForgeClosedState()
		{
			bool hasSelection = _selectedCell != null && _selectedCell.get_ItemInfo() != null;
			ShowSidePanels(hasSelection);
			if (_tryItemButton != null)
			{
				if (hasSelection) UpdateTryButton(_selectedCell.get_ItemInfo());
				else _tryItemButton.gameObject.SetActive(false);
			}
		}

		public void RefreshAfterForgeMutation()
		{
			ItemInfo selectedInfo = _selectedCell != null ? _selectedCell.get_ItemInfo() : null;
			ReloadItems();
			if (selectedInfo == null) return;

			if (_infoPanelContent != null) _infoPanelContent.SetItemInfo(selectedInfo);
			if (_parametersPanelContent != null) _parametersPanelContent.UpdateParameters(selectedInfo);
			if (_propertiesPanelContent != null) _propertiesPanelContent.SetItemInfo(selectedInfo);
			if (_itemProperties != null)
			{
				bool hasEnchantments = ListSF.GetItemEnchantments(selectedInfo).Count > 0;
				_itemProperties.set_OpenImage(hasEnchantments ? _rightFlagPropertiesImage : _rightFlagPropertiesNoneImage);
				_itemProperties.set_CloseImage(hasEnchantments ? _leftFlagPropertiesImage : _leftFlagPropertiesNoneImage);
			}
			if (_tryItemButton != null) UpdateTryButton(selectedInfo);
		}

		private void UpdateModelIfItemChanged(ItemInfo item = null)
		{
			if (_modelContainer != null && _modelContainer.IsItemDiffer(ListSF.GetRoster().get_Parameters()))
			{
				UpdateModel(item);
			}
		}

		private void UpdateModel(ItemInfo item, StageType.Stage stage = StageType.Stage.STAGE_SHOP_START)
		{
			if (_modelContainer != null)
			{
				ItemInfo targetItem = item;
				if (targetItem == null)
				{
					ShopTableViewCell shopTableViewCell = _shopTableView.get_SelectedCell() as ShopTableViewCell;
					targetItem = ((!(shopTableViewCell != null)) ? ListSF.GetRoster().get_Parameters().Armor : shopTableViewCell.get_ItemInfo());
				}
				if (targetItem != null)
				{
					_modelContainer.UpdateModel(item, stage, targetItem.Type);
				}
			}
		}

		public void ShowUI()
		{
			_backgroundLeft.color = Constants.DimmedBackgroundColor;
			_backgroundRight.color = Constants.DimmedBackgroundColor;
			SetOpacity(_shownOpacity);
			if (_shopUIGroup != null)
			{
				_shopUIGroup.blocksRaycasts = true;
			}
		}

		private bool ShouldDimBackground()
		{
			ShopSection section = _currentSection;
			if (section == ShopSection.Armor || section == ShopSection.Helmet)
			{
				return false;
			}
			return true;
		}

		public void HideUI()
		{
			if (ShouldDimBackground())
			{
				_backgroundLeft.color = Constants.NormalBackgroundColor;
				_backgroundRight.color = Constants.NormalBackgroundColor;
				SetOpacity(_hiddenOpacity);
				if (_shopUIGroup != null)
				{
					_shopUIGroup.blocksRaycasts = false;
				}
			}
		}

		public void SetOpacity(float opacity)
		{
			if (_shopUIGroup != null)
			{
				_shopUIGroup.DOFade(opacity, _fadeDuration);
			}
		}

		public void ShowSidePanels(bool value)
		{
			bool active = _currentSection != ShopSection.Payment && _currentSection != ShopSection.Free && value;
			if (_itemInfo != null)
			{
				_itemInfo.gameObject.SetActive(value);
			}
			if (_itemParam != null)
			{
				_itemParam.gameObject.SetActive(active);
			}
			if (_itemProperties != null)
			{
				_itemProperties.gameObject.SetActive(active);
			}
		}

		public InfoPanelContent GetInfoPanel()
		{
			return _infoPanelContent;
		}

		public int NumberOfRowsInTableView(TableView tableView)
		{
			return _currentItems.Count;
		}

		public float SizeForRowInTableView(TableView tableView, int row)
		{
			switch (_currentSection)
			{
			case ShopSection.Weapon:
				return 422f;
			case ShopSection.Armor:
				return 824f;
			case ShopSection.Helmet:
				return 826f;
			case ShopSection.Ranged:
				return 422f;
			case ShopSection.Magic:
				return 802f;
			case ShopSection.Payment:
				return 500f;
			case ShopSection.Free:
				return 500f;
			default:
				return 500f;
			}
		}

		public TableViewCell CellForRowInTableView(TableView tableView, int row)
		{
			TableViewCell tableViewCell = tableView.ReusableCellForRow(row);
			ShopTableViewCell component = tableViewCell.GetComponent<ShopTableViewCell>();
			component.set_BaseSize(_cellBaseSize);
			component.set_IconPanelActive(_iconPanelActive);
			component.SetItemInfo(_currentItems[row]);
			component.set_Index(row);
			if (component.get_PerksPanel() != null && _hintPanel != null)
			{
				component.get_PerksPanel().onPerksClick.RemoveListener(_hintPanel.ShowPerkHint);
				component.get_PerksPanel().onPerksClick.AddListener(_hintPanel.ShowPerkHint);
			}
			return tableViewCell;
		}

		public void TableViewDidHighlightCellForRow(TableView tableView, int row)
		{
		}

		public void TableViewDidSelectCellForRow(TableView tableView, int row)
		{
			_shopTableView.ScrollToCell(row, 0.5f);
		}
	}
}
