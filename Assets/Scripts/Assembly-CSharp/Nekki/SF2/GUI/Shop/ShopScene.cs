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
				bool jOJGKNGGAHB = false;
				_itemInfo.Init(_infoPanelContent, jOJGKNGGAHB);
			}
			if (_itemParam != null)
			{
				bool jOJGKNGGAHB2 = true;
				bool nKGDKKNNJOF = false;
				float mDPGKEDBHNO = -40f;
				_itemParam.Init(_parametersPanelContent, jOJGKNGGAHB2, mDPGKEDBHNO, nKGDKKNNJOF, _rightFlagImage, _leftFlagImage);
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

		protected void OnCellSelected(TableViewCell HJCPCBLCJJN)
		{
			ShopTableViewCell shopTableViewCell = HJCPCBLCJJN as ShopTableViewCell;
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

		private void UpdatePropertiesPanel(ShopTableViewCell LIBKHDGLJFF)
		{
			if (!(LIBKHDGLJFF == null) && !(_itemProperties == null) && !(_propertiesPanelContent == null))
			{
				ItemInfo itemInfo = LIBKHDGLJFF.get_ItemInfo();
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

		private void UpdateParametersForCell(ShopTableViewCell LIBKHDGLJFF)
		{
			if (LIBKHDGLJFF != null && LIBKHDGLJFF.get_ItemInfo() != null && _parametersPanelContent != null)
			{
				_parametersPanelContent.UpdateParameters(LIBKHDGLJFF.get_ItemInfo());
			}
		}

		private void UpdateParametersAnimated()
		{
			if (_parametersPanelContent != null && _selectedCell != null && _selectedCell.get_ItemInfo() != null)
			{
				_parametersPanelContent.UpdateParametersWithDuration(_selectedCell.get_ItemInfo());
			}
		}

		public void DisableButton(ShopSection KGDHCBNKLMF)
		{
			if (_buttonPanel != null)
			{
				_buttonPanel.DisableButton((int)KGDHCBNKLMF);
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
			ItemInfo AOCHMFMOACB = null;
			_currentItems.ForEach((ItemInfo DHDMNHCIPEH) =>
			{
				if (DHDMNHCIPEH.GetIsNew() && (AOCHMFMOACB == null || (ObscuredLong)(AOCHMFMOACB.GemPrice) < (ObscuredLong)(DHDMNHCIPEH.GemPrice) || ((ObscuredLong)(AOCHMFMOACB.GemPrice) == (ObscuredLong)(DHDMNHCIPEH.GemPrice) && (ObscuredLong)(AOCHMFMOACB.CoinPrice) < (ObscuredLong)(DHDMNHCIPEH.CoinPrice))))
				{
					AOCHMFMOACB = DHDMNHCIPEH;
				}
			});
			if (AOCHMFMOACB != null)
			{
				ScrollToItem(AOCHMFMOACB);
				return true;
			}
			return false;
		}

		public bool FocusOnLastFocus()
		{
			ItemInfo dJKEECEOCJB = null;
			if (_lastFocusedItems.ContainsKey(_currentSection))
			{
				ItemInfo DIPNFHJPJGA = _lastFocusedItems[_currentSection];
				dJKEECEOCJB = _currentItems.Find((ItemInfo DHDMNHCIPEH) => DHDMNHCIPEH.Name.Equals(DIPNFHJPJGA.Name));
			}
			if (dJKEECEOCJB != null)
			{
				ScrollToItem(dJKEECEOCJB);
				return true;
			}
			return false;
		}

		public bool FocusOnEquipedItem()
		{
			List<UserItem> list = ListSF.GetRoster().GetInventory().GetItems()
				.FindAll((UserItem DHDMNHCIPEH) => DHDMNHCIPEH.GetIsEquipped());
			ItemInfo dJKEECEOCJB = null;
			foreach (ItemInfo item in _currentItems)
			{
				UserItem dKCHDHMLKHN = list.Find((UserItem DHDMNHCIPEH) => DHDMNHCIPEH.GetInfo() != null && DHDMNHCIPEH.GetInfo().Name.Equals(item.Name));
				if (dKCHDHMLKHN != null)
				{
					dJKEECEOCJB = dKCHDHMLKHN.GetInfo();
					break;
				}
			}
			if (dJKEECEOCJB != null)
			{
				ScrollToItem(dJKEECEOCJB);
				return true;
			}
			return false;
		}

		public void ScrollToItem(ItemInfo item)
		{
			ItemInfo dJKEECEOCJB = _currentItems.Find((ItemInfo DHDMNHCIPEH) => DHDMNHCIPEH.Name.Equals(item.Name));
			if (dJKEECEOCJB != null && _shopTableView != null)
			{
				int iBAKGENOEPH = _currentItems.IndexOf(dJKEECEOCJB);
				_shopTableView.ScrollToCell(iBAKGENOEPH);
			}
		}

		public void ScrollToItemByName(SliderType MNHKGIHKBPO, string FDJFBMNPPLM)
		{
			GoToSlider(MNHKGIHKBPO);
			ItemInfo dJKEECEOCJB = _currentItems.Find((ItemInfo FAKOMBAIFPP) => FAKOMBAIFPP.Name == FDJFBMNPPLM);
			if (dJKEECEOCJB != null && _shopTableView != null)
			{
				int iBAKGENOEPH = _currentItems.IndexOf(dJKEECEOCJB);
				_shopTableView.ScrollToCell(iBAKGENOEPH);
			}
		}

		public void ScrollToItemByName(ShopSection KGDHCBNKLMF, string OHCGEEEKEJH)
		{
			SetShopSection(KGDHCBNKLMF);
			ItemInfo dJKEECEOCJB = _currentItems.Find((ItemInfo DHDMNHCIPEH) => DHDMNHCIPEH.Name.Equals(OHCGEEEKEJH));
			if (dJKEECEOCJB != null && _shopTableView != null)
			{
				int iBAKGENOEPH = _currentItems.IndexOf(dJKEECEOCJB);
				_shopTableView.ScrollToCell(iBAKGENOEPH);
			}
		}

		public void GoToSlider(SliderType JFMPFHEPMIE)
		{
			switch (JFMPFHEPMIE)
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

		private void AddAvailableItems(List<ItemInfo> CAIHJJFKFLP, List<ItemInfo> PPFNLLCMHGM)
		{
			foreach (ItemInfo item in CAIHJJFKFLP)
			{
				if (ShopAvailabilityPolicy.IsAvailable(item, ListSF.GetRoster()))
				{
					PPFNLLCMHGM.Add(item);
				}
			}
		}

		public void SetShopSection(ShopSection KGDHCBNKLMF)
		{
			if (_currentSection == KGDHCBNKLMF)
			{
				return;
			}
			RememberFocus();
			_currentSection = KGDHCBNKLMF;
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
			switch (KGDHCBNKLMF)
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
				_currentItems.ForEach((ItemInfo DHDMNHCIPEH) =>
				{
					DHDMNHCIPEH.SetIsNew(false);
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

		private void SetSectionItems(List<ItemInfo> HELFDCAIJNE, float CPLBEMJADEL, Vector2 PEEOEOMEBFG, bool PMKIFLOFHJG = true)
		{
			if (HELFDCAIJNE == null || _shopTableView == null)
			{
				GameLog.Error("ShopScene.SetItems some field is null");
				return;
			}
			SortItems(HELFDCAIJNE);
			_currentItems = HELFDCAIJNE;
			_iconPanelActive = PMKIFLOFHJG;
			_cellBaseSize = PEEOEOMEBFG;
			_shopTableView.set_Spacing(CPLBEMJADEL);
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

		private void SortItems(List<ItemInfo> HELFDCAIJNE)
		{
			HELFDCAIJNE.Sort((ItemInfo FGBJPFPGHKC, ItemInfo ACJEJOKKGNI) =>
			{
				UserItem dKCHDHMLKHN = ListSF.GetRoster().GetInventory().FindItem(FGBJPFPGHKC);
				UserItem dKCHDHMLKHN2 = ListSF.GetRoster().GetInventory().FindItem(ACJEJOKKGNI);
				int num = ((dKCHDHMLKHN == null) ? FGBJPFPGHKC.UpgradeLevel : dKCHDHMLKHN.GetUpgradeLevel());
				int num2 = ((dKCHDHMLKHN2 == null) ? ACJEJOKKGNI.UpgradeLevel : dKCHDHMLKHN2.GetUpgradeLevel());
				int num3 = HELFDCAIJNE.IndexOf(FGBJPFPGHKC);
				int value = HELFDCAIJNE.IndexOf(ACJEJOKKGNI);
				return (num == num2) ? num3.CompareTo(value) : num.CompareTo(num2);
			});
		}

		private void ReloadItems()
		{
			ShopTableViewCell nNACFMKLHIB = _selectedCell;
			SortItems(_currentItems);
			_shopTableView.ReloadData();
			if (_shopTableView != null && nNACFMKLHIB != null && nNACFMKLHIB != _selectedCell)
			{
				_shopTableView.ScrollToCell(nNACFMKLHIB.get_RowNumber());
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
				bool PNKJLPDJOJF = false;
				bool CBDBANOPFDM = false;
				InputDeviceExtension.GetOwnedAndEquippedState(ref PNKJLPDJOJF, ref CBDBANOPFDM, item);
				if (CBDBANOPFDM)
				{
					_tryItemButton.SetAlias("btnShopUnequip");
				}
				else if (PNKJLPDJOJF)
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
			bool PNKJLPDJOJF = false;
			bool CBDBANOPFDM = false;
			InputDeviceExtension.GetOwnedAndEquippedState(ref PNKJLPDJOJF, ref CBDBANOPFDM, itemInfo);
			if (PNKJLPDJOJF && CBDBANOPFDM)
			{
				ListSF.GetRoster().GetInventory().ResetToDefaultItem(itemInfo, true);
				ReloadItems();
				UpdateModelIfItemChanged();
				UpdateParametersAnimated();
				UpdateTryButton(itemInfo);
				return;
			}
			if (PNKJLPDJOJF && !CBDBANOPFDM)
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

		private void OnUserItemsChanged(List<UserItem> GOGGLLFHAMB)
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

		private void UpdateModelIfItemChanged(ItemInfo DDCFPIDHLGJ = null)
		{
			if (_modelContainer != null && _modelContainer.IsItemDiffer(ListSF.GetRoster().get_Parameters()))
			{
				UpdateModel(DDCFPIDHLGJ);
			}
		}

		private void UpdateModel(ItemInfo DDCFPIDHLGJ, StageType.Stage LGPIFNMFPAN = StageType.Stage.STAGE_SHOP_START)
		{
			if (_modelContainer != null)
			{
				ItemInfo dJKEECEOCJB = DDCFPIDHLGJ;
				if (dJKEECEOCJB == null)
				{
					ShopTableViewCell shopTableViewCell = _shopTableView.get_SelectedCell() as ShopTableViewCell;
					dJKEECEOCJB = ((!(shopTableViewCell != null)) ? ListSF.GetRoster().get_Parameters().Armor : shopTableViewCell.get_ItemInfo());
				}
				if (dJKEECEOCJB != null)
				{
					_modelContainer.UpdateModel(DDCFPIDHLGJ, LGPIFNMFPAN, dJKEECEOCJB.Type);
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
			ShopSection nNGHNIJCKLD = _currentSection;
			if (nNGHNIJCKLD == ShopSection.Armor || nNGHNIJCKLD == ShopSection.Helmet)
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

		public void SetOpacity(float KGJALFLDIBG)
		{
			if (_shopUIGroup != null)
			{
				_shopUIGroup.DOFade(KGJALFLDIBG, _fadeDuration);
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

		public int NumberOfRowsInTableView(TableView OIDFBEAABBA)
		{
			return _currentItems.Count;
		}

		public float SizeForRowInTableView(TableView OIDFBEAABBA, int IBAKGENOEPH)
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

		public TableViewCell CellForRowInTableView(TableView OIDFBEAABBA, int IBAKGENOEPH)
		{
			TableViewCell tableViewCell = OIDFBEAABBA.ReusableCellForRow(IBAKGENOEPH);
			ShopTableViewCell component = tableViewCell.GetComponent<ShopTableViewCell>();
			component.set_BaseSize(_cellBaseSize);
			component.set_IconPanelActive(_iconPanelActive);
			component.SetItemInfo(_currentItems[IBAKGENOEPH]);
			component.set_Index(IBAKGENOEPH);
			if (component.get_PerksPanel() != null && _hintPanel != null)
			{
				component.get_PerksPanel().onPerksClick.RemoveListener(_hintPanel.ShowPerkHint);
				component.get_PerksPanel().onPerksClick.AddListener(_hintPanel.ShowPerkHint);
			}
			return tableViewCell;
		}

		public void TableViewDidHighlightCellForRow(TableView OIDFBEAABBA, int IBAKGENOEPH)
		{
		}

		public void TableViewDidSelectCellForRow(TableView OIDFBEAABBA, int IBAKGENOEPH)
		{
			_shopTableView.ScrollToCell(IBAKGENOEPH, 0.5f);
		}
	}
}
