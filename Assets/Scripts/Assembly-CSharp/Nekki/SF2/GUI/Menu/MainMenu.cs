using System.Diagnostics;
using Nekki.SF2.Core.Quests;
using Nekki.SF2.GUI.Shop;
using Eclipse.UI.TopBar;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Menu
{
	public class MainMenu : SFMonoBehaviour<object>, BackKeyController
	{
		public enum MenuButtonType
		{
			MENU_DOJO = 0,
			MENU_SHOP = 1,
			MENU_MAP = 2,
			MENU_PROFILE = 3,
			MENU_MONEY = 4,
			MENU_EXIT = 5,
			MENU_SETTINGS = 6,
			MENU_DOJO_DISCIPLE = 7
		}

		private enum MainMenuLayer
		{
			ZCompare = 0,
			ZDojoDisciple = 1,
			ZScroll = 2,
			ZBackground = 3,
			ZContent = 4,
			ZTopContent = 5,
			ZMaterialsPanel = 6,
			ZHint = 7
		}

		public const float MENU_BAR_SHADOW_HEIGHT = 20f;

		public const float MENU_SCROLL_SPEED = 0.25f;

		public const string MENU_BUTTON_DISCIPLE_ON = "MenuButtons.btn_punching_bag";

		public const string MENU_BUTTON_DISCIPLE_OFF = "MenuButtons.btn_disciple";

		private bool isEnabled = true;

		private bool enableAiOnClose = true;

		[SerializeField]
		private MenuExpPanel _experience;

		[SerializeField]
		private MenuEnergyPanel _energy;

		[SerializeField]
		private MenuMoneyPanel _money;

		[SerializeField]
		private MenuMaterialsPanel _materials;

		[SerializeField]
		private GameObject _raidRating;

		[SerializeField]
		private Button _skipTutorialBtn;

		[SerializeField]
		private Image _newPerksCircle;

		[SerializeField]
		private Image _newPerksEllipse;

		[SerializeField]
		private Text _newPerksLabel;

		[SerializeField]
		private Image _newItemsCircle;

		[SerializeField]
		private Image _newItemsEllipse;

		[SerializeField]
		private Text _newItemsLabel;

		[SerializeField]
		private LabelAlias _menuLabel;

		private Slider unusedSlider;

		private SliderType shopSliderType;

		private SectionButton currentButton;

		[SerializeField]
		private SectionButton btnDojo;

		[SerializeField]
		private SectionButton btnMap;

		[SerializeField]
		private SectionButton btnShop;

		[SerializeField]
		private SectionButton btnProfile;

		[SerializeField]
		private SectionButton btnSettings;

		[SerializeField]
		private Toggle btnDojoDisciple;

		[SerializeField]
		private Image _backgroundPicture;

		[SerializeField]
		public MenuScroll Scroll;

		[SerializeField]
		private GameObject _settingsDlgPrefab;

		[SerializeField]
		private GameObject _menuBlocker;

		private int _sliderVisibleItems;

		private float unusedFloat;

		private MenuScroll.ScrollState scrollState = MenuScroll.ScrollState.ScrollOpen;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static MainMenu instance;

		public static MainMenu CurrentInstance
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

		public static MainMenu get_Instance()
		{
			return instance;
		}

		public static void set_Instance(MainMenu value)
		{
			instance = value;
		}

		public void Init()
		{
			set_Instance(this);
			_money.Init();
			_experience.Init();
			_energy.Init();
			_materials.Init();
			DesktopTopBarLayout.Configure(_experience, _energy, _money);
			ConfigureMenuScrollLayout();
            Eclipse.UI.ReturnToTitleButton.Attach(this);
			InitBackgroundMetrics();
			InitScroll();
			InitExperiencePanel();
			InitEnergyPanel();
			InitSkipTutorialButton();
			InitMoneyPanel();
			InitMaterialsPanel();
			InitDojoDiscipleButton();
			UpdatePanelsLayout();
			if (AssemblyController.GetGamepadEnabled())
			{
			}
			SetNormalViewMode(false);
			UpdateMenu();
			_menuBlocker.gameObject.SetActive(false);
		}

		private void OnDestroy()
		{
			if (btnDojoDisciple != null)
			{
				btnDojoDisciple.onValueChanged.RemoveListener(OnDojoDiscipleChanged);
			}
			set_Instance(null);
			Scroll.RemoveAllEventListener();
			_money.RemoveAllEventListener();
			_energy.RemoveAllEventListener();
			_experience.RemoveAllEventListener();
		}

		public void Destroy()
		{
			if (AssemblyController.GetGamepadEnabled())
			{
			}
			_skipTutorialBtn.onClick.RemoveListener(() =>
			{
				SkipTutorial();
			});
		}

		private void ConfigureMenuScrollLayout()
		{
			// AssetRipper unpacked the main menu into each GUI scene, so editor-side
			// tweaks can silently make Map/Dojo/Shop/Profile disagree. Normalize the
			// recovered layout before MenuScroll.Init() records its expanded size.
			ConfigureRect(Scroll != null ? Scroll.GetComponent<RectTransform>() : null,
				new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0.5f, 1f),
				new Vector2(319.8999f, -148f), new Vector2(640f, 1240f));

			ConfigureMenuButton(btnDojo, new Vector2(0f, 473.7f));
			ConfigureMenuButton(btnMap, new Vector2(0f, 226f));
			ConfigureMenuButton(btnShop, new Vector2(0f, -24f));
			ConfigureMenuButton(btnProfile, new Vector2(0f, -271.1f));
			ConfigureMenuButton(btnSettings, new Vector2(0f, -520.2f));

			Button wheelButton = (Scroll != null) ? Scroll.GetButton() : null;
			ConfigureRect(wheelButton != null ? wheelButton.GetComponent<RectTransform>() : null,
				new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f),
				new Vector2(0f, 60f), new Vector2(600f, 300f));

			if (Scroll != null)
			{
				ConfigureNamedRect(Scroll.transform, "ScrollPaper",
					new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
					new Vector2(-0.89990234f, -620f), new Vector2(640f, 1240f));
				ConfigureNamedRect(Scroll.transform, "ScrollBack",
					Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
				ConfigureNamedRect(Scroll.transform, "MenuWheel",
					new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
					new Vector2(0.049987793f, -57f), new Vector2(640f, 114f));
				ConfigureNamedRect(Scroll.transform, "MenuWheelCenter",
					new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
					Vector2.zero, new Vector2(328f, 114f));
				ConfigureNamedRect(Scroll.transform, "MenuWheelText",
					new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
					Vector2.zero, new Vector2(328f, 114f));
			}
		}

		private static void ConfigureMenuButton(SectionButton button, Vector2 position)
		{
			if (button == null)
			{
				return;
			}
			ConfigureRect(button.GetComponent<RectTransform>(),
				new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
				position, new Vector2(278f, 278f));
			ResolutionImage image = button.targetGraphic as ResolutionImage;
			if (image != null)
			{
				image.preserveAspect = false;
				image.type = Image.Type.Simple;
				image.useSpriteMesh = false;
			}
		}

		private static void ConfigureNamedRect(Transform root, string name, Vector2 anchorMin,
			Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
		{
			RectTransform[] rects = root.GetComponentsInChildren<RectTransform>(true);
			foreach (RectTransform rect in rects)
			{
				if (rect.name == name)
				{
					ConfigureRect(rect, anchorMin, anchorMax, pivot, position, size);
					return;
				}
			}
		}

		private static void ConfigureRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
			Vector2 pivot, Vector2 position, Vector2 size)
		{
			if (rect == null)
			{
				return;
			}
			rect.anchorMin = anchorMin;
			rect.anchorMax = anchorMax;
			rect.pivot = pivot;
			rect.sizeDelta = size;
			rect.anchoredPosition3D = new Vector3(position.x, position.y, 0f);
			rect.localScale = Vector3.one;
			rect.localRotation = Quaternion.identity;
		}

		private void InitBackgroundMetrics()
		{
			GameUtils.MenuBackgroundHeight = _backgroundPicture.rectTransform.rect.height;
			GameUtils.MenuBackgroundPadding = 20f;
		}

		private void InitScroll()
		{
			InitMenuButtons();
			HideNewPerksBadge();
			HideNewItemsBadge();
			Scroll.Init();
			Scroll.SetAllowRolling(true);
			Scroll.SetOutsideTouchProperties(true);
			Scroll.AddEventListener(2, OnScrollChanging);
			Scroll.AddEventListener(3, OnScrollRolling);
			Scroll.AddEventListener(4, OnScrollTouch);
			Scroll.Collapse(0f);
			UpdateMenuExtras();
			ScreenType cCGJDFLIKFN = Module.GetInstance().GetCurrentScreenType();
			UpdateCurrentButton(cCGJDFLIKFN);
		}

		private void UpdateMenuExtras()
		{
		}

		private string GetMenuLabelText()
		{
			return string.Empty;
		}

		private void OnScrollChanging(object data)
		{
		}

		private void OnScrollOpened()
		{
		}

		private void OnScrollClosed()
		{
		}

		private void OnScrollRolling(object data)
		{
			MenuScroll.ScrollState aNJKEGGALAG = (MenuScroll.ScrollState)data;
			if (scrollState == aNJKEGGALAG)
			{
				return;
			}
			scrollState = aNJKEGGALAG;
			switch (aNJKEGGALAG)
			{
			case MenuScroll.ScrollState.ScrollOpen:
				enableAiOnClose = true;
				ModelAi.set_AiOn(false);
				BackKeyManager.get_Instance().AddBackKeyController(this);
				OnScrollOpened();
				break;
			case MenuScroll.ScrollState.ScrollClose:
				if (enableAiOnClose)
				{
					ModelAi.set_AiOn(true);
				}
				BackKeyManager.get_Instance().RemoveBackKeyController(this);
				OnScrollClosed();
				break;
			}
		}

		private void OnScrollTouch(object data)
		{
			if (!Scroll.IsExpanded())
			{
				UpdateScrollMenu();
			}
		}

		public void UpdateScrollMenu()
		{
			UpdateNewItems();
			UpdateNewPerks();
		}

		private void InitMenuButtons()
		{
			btnDojo.onClick.AddListener(() =>
			{
				OnClickButton(MenuButtonType.MENU_DOJO);
			});
			btnMap.onClick.AddListener(() =>
			{
				OnClickButton(MenuButtonType.MENU_MAP);
			});
			btnShop.onClick.AddListener(() =>
			{
				OnClickButton(MenuButtonType.MENU_SHOP);
			});
			btnProfile.onClick.AddListener(() =>
			{
				OnClickButton(MenuButtonType.MENU_PROFILE);
			});
			btnSettings.onClick.AddListener(() =>
			{
				OnClickButton(MenuButtonType.MENU_SETTINGS);
			});
			if (btnDojoDisciple != null)
			{
				// The exported Toggle lost its persistent callback, so the kid/bag
				// selector rendered and animated but never changed the dojo model.
				btnDojoDisciple.onValueChanged.AddListener(OnDojoDiscipleChanged);
			}
			btnDojo.IsOneShot = true;
			btnMap.IsOneShot = true;
			btnShop.IsOneShot = true;
			btnProfile.IsOneShot = true;
		}

		private void OnDojoDiscipleChanged(bool value)
		{
			if (Module.GetInstance().GetCurrentScreenType() == ScreenType.ModuleDojo)
			{
				OnClickButton(MenuButtonType.MENU_DOJO_DISCIPLE);
			}
		}

		private void InitExperiencePanel()
		{
			_experience.AddEventListener(0, OnLevelHintClicked);
		}

		public void UpdateLevel()
		{
			if ((bool)_experience)
			{
				_experience.UpdateLevel();
			}
		}

		public void UpdateRaidRating()
		{
			if (!_raidRating)
			{
			}
		}

		private void InitSkipTutorialButton()
		{
			bool flag = SystemProperties.IsDebug() && Module.GetInstance().IsUserTutorialComplete();
			_skipTutorialBtn.gameObject.SetActive(flag);
			if (flag)
			{
				_skipTutorialBtn.onClick.AddListener(() =>
				{
					SkipTutorial();
				});
			}
		}

		private void InitMoneyPanel()
		{
			_money.AddEventListener(0, OnMoneyButtonClicked);
		}

		public void UpdateMoney()
		{
			if ((bool)_money)
			{
				_money.UpdateValues();
			}
		}

		public void UpdateRubySale()
		{
			if ((bool)_money)
			{
				_money.UpdateRubySale();
			}
		}

		public Button GetRubyBtn()
		{
			if (!_money)
			{
				return null;
			}
			return _money.GetRubyBtn();
		}

		private void InitMaterialsPanel()
		{
			_materials.gameObject.SetActive(false);
		}

		public void UpdateMainMenu()
		{
			_money.UpdateRuby();
			UpdateMenu();
		}

		public void UpdateBarExp(float OBLEMIHLFII, float KAEPJHHLLPK)
		{
			if ((bool)_experience)
			{
				_experience.UpdateBarExp(OBLEMIHLFII, KAEPJHHLLPK);
			}
		}

		private void InitEnergyPanel()
		{
			_energy.AddEventListener(0, OnEnergyBarClicked);
		}

		public void UpdateBarEnergy()
		{
			_energy.UpdateBar();
		}

		public void UpdateEnergyView()
		{
			_energy.UpdateView();
			UpdatePanelsLayout();
		}

		private void OnEnergyBarClicked(object data)
		{
			if (!ListSF.GetRoster().HasUnlimitedEnergy && ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_ENERGY_BAR_PRESS))
			{
				ListSF.GetInstance().RunQuestActions();
			}
		}

		public void UpdateMenu()
		{
			Roster nKGLHEGIKKP = ListSF.GetRoster();
			if (0 == 0)
			{
				_experience.gameObject.SetActive(true);
				_energy.gameObject.SetActive(false);
				if ((bool)_raidRating)
				{
					_raidRating.gameObject.SetActive(false);
				}
				UpdateLevel();
				UpdateBarExp(nKGLHEGIKKP.GetExperience(), nKGLHEGIKKP.GetExperienceToNextLevel());
			}
			else
			{
				_experience.gameObject.SetActive(false);
				_energy.gameObject.SetActive(false);
				_raidRating.gameObject.SetActive(true);
				UpdateRaidRating();
			}
			UpdateMoney();
			UpdateNewPerks();
			UpdateNewItems();
			UpdateMaterials();
			UpdateBarEnergy();
			UpdatePanelsLayout();
		}

		public void UpdateMenuSize()
		{
			UpdateMenu();
		}

		public void CloseMenu(float _Duration = 0f)
		{
			Scroll.Collapse(_Duration);
		}

		private void OnMoneyButtonClicked(object data)
		{
			OnClickButton(MenuButtonType.MENU_MONEY);
		}

		private void OnClickButton(MenuButtonType KNCNFGABHCL)
		{
			enableAiOnClose = true;
			switch (KNCNFGABHCL)
			{
			case MenuButtonType.MENU_DOJO:
				Module.OpenScreen(ScreenType.ModuleDojo);
				break;
			case MenuButtonType.MENU_SHOP:
				Module.OpenScreen(ScreenType.ModuleShop);
				break;
			case MenuButtonType.MENU_MAP:
				Module.OpenScreen(ScreenType.ModuleMap);
				break;
			case MenuButtonType.MENU_EXIT:
				GameUtils.ExitApplication();
				break;
			case MenuButtonType.MENU_PROFILE:
				Module.OpenScreen(ScreenType.ModuleProfile);
				break;
			case MenuButtonType.MENU_MONEY:
			{
				shopSliderType = SliderType.SliderRuby;
				ShopScene current = Scene<ShopScene>.get_Current();
				if (current != null)
				{
					current.GoToSlider(shopSliderType);
				}
				else
				{
					Module.OpenScreen(ScreenType.ModuleShop, new DelayedStrike(shopSliderType));
				}
				break;
			}
			case MenuButtonType.MENU_SETTINGS:
				enableAiOnClose = false;
				DialogsOpener.OpenSettingsDialog();
				CloseMenu(0.25f);
				break;
			case MenuButtonType.MENU_DOJO_DISCIPLE:
				ListSF.GetRoster().ToggleDiscipleMode();
				UpdateDojoDiscipleButton();
				Module.OpenScreen(ScreenType.ModuleDojo);
				break;
			}
			if (KNCNFGABHCL == MenuButtonType.MENU_SETTINGS)
			{
			}
		}

		public void SetEnabled(bool PKHDLOGJKAD)
		{
			if (IsEnabled() != PKHDLOGJKAD)
			{
				isEnabled = PKHDLOGJKAD;
				_menuBlocker.gameObject.SetActive(!isEnabled);
				if (!isEnabled)
				{
					Scroll.Collapse(0f);
				}
			}
		}

		public bool IsEnabled()
		{
			return isEnabled;
		}

		public void SkipTutorial()
		{
			if (ListSF.GetInstance().IsTutorialComplete())
			{
				string currentQuestName = QuestsManager.get_Instance().CurrentQuestName;
				QuestStage questByName = QuestsManager.get_Instance().GetQuestByName(currentQuestName);
				if (questByName != null)
				{
					questByName.FinishQuest();
				}
			}
			ListSF.GetRoster().GetTutorials().set_StoryTutorialStep(GameUtils.TutorialSettings.StepsNames[GameUtils.TutorialSettings.StepsNames.Count - 1]);
			_skipTutorialBtn.gameObject.SetActive(false);
			Module.OpenScreen(ScreenType.ModuleMap);
		}

		public void UpdateCurrentButton(ScreenType CCGJDFLIKFN)
		{
			switch (CCGJDFLIKFN)
			{
			case ScreenType.ModuleShop:
				SetCurrentButton(btnShop);
				break;
			case ScreenType.ModuleMap:
				SetCurrentButton(btnMap);
				break;
			case ScreenType.ModuleDojo:
				SetCurrentButton(btnDojo);
				break;
			case ScreenType.ModuleProfile:
				SetCurrentButton(btnProfile);
				break;
			default:
				SetCurrentButton(null);
				break;
			}
			UpdateDojoDiscipleVisibility(CCGJDFLIKFN);
		}

		public SectionButton GetButtonFromScreen(ScreenType CCGJDFLIKFN)
		{
			switch (CCGJDFLIKFN)
			{
			case ScreenType.ModuleDojo:
				return btnDojo;
			case ScreenType.ModuleMap:
				return btnMap;
			case ScreenType.ModuleShop:
				return btnShop;
			case ScreenType.ModuleProfile:
				return btnProfile;
			default:
				return null;
			}
		}

		private void InitDojoDiscipleButton()
		{
			bool flag = ListSF.GetRoster().GetDiscipleMode() == 1;
			ResolutionImage resolutionImage = btnDojoDisciple.targetGraphic as ResolutionImage;
			resolutionImage.set_SpriteName((!flag) ? "MenuButtons.btn_disciple" : "MenuButtons.btn_punching_bag");
			Roster roster = ListSF.GetRoster();
			UpdateDojoDiscipleVisibility(Module.GetInstance().GetCurrentScreenType());
		}

		private void UpdateDojoDiscipleVisibility(ScreenType screen)
		{
			if (btnDojoDisciple == null)
			{
				return;
			}
			Eclipse.UI.ModDojoButtons.Update(btnDojoDisciple.transform as RectTransform, screen == ScreenType.ModuleDojo);
			Roster roster = ListSF.GetRoster();
			bool unlocked = roster != null && roster.HasSessionSetting("ShowDojoDisciple") &&
				roster.GetSettingsXML("ShowDojoDisciple") != "0";
			btnDojoDisciple.gameObject.SetActive(unlocked && screen == ScreenType.ModuleDojo);
		}

		public void UpdateDojoDiscipleButton()
		{
			if (btnDojoDisciple != null)
			{
				bool flag = ListSF.GetRoster().GetDiscipleMode() == 1;
				ResolutionImage resolutionImage = btnDojoDisciple.targetGraphic as ResolutionImage;
				resolutionImage.set_SpriteName((!flag) ? "MenuButtons.btn_disciple" : "MenuButtons.btn_punching_bag");
			}
		}

		public void ShowDojoDiscipleButton(bool value)
		{
			if ((bool)btnDojoDisciple)
			{
				btnDojoDisciple.gameObject.SetActive(value);
			}
		}

		private void UpdatePanelsLayout()
		{
		}

		private void OnLevelHintClicked(object EMBBNNBFODN)
		{
			Roster nKGLHEGIKKP = ListSF.GetRoster();
			string hCPNFPMHFCM = ((nKGLHEGIKKP.GetExperience() != nKGLHEGIKKP.GetExperienceToNextLevel()) ? LocalizationManager.GetString("experienceHint", nKGLHEGIKKP.GetExperience().ToString(), nKGLHEGIKKP.GetExperienceToNextLevel().ToString()) : LocalizationManager.GetString("dlgComingSoonText"));
			_experience.ShowHint(hCPNFPMHFCM);
		}

		private void ResetBadgesAndExtras()
		{
			UpdateMenuExtras();
			HideNewItemsBadge();
			HideNewPerksBadge();
		}

		private int GetCurrentButtonIndex()
		{
			if (currentButton == btnDojo)
			{
				return 0;
			}
			if (currentButton == btnMap)
			{
				return 1;
			}
			if (currentButton == btnShop)
			{
				return 2;
			}
			if (currentButton == btnProfile)
			{
				return 3;
			}
			if (currentButton == btnSettings)
			{
				return 4;
			}
			return -1;
		}

		public void ReloadAllTextureAtlas()
		{
		}

		public void OnBackKeyClicked(object data)
		{
			CloseMenu(0.25f);
			BackKeyManager.get_Instance().RemoveBackKeyController(this);
		}

		private void HideNewPerksBadge()
		{
			_newPerksCircle.gameObject.SetActive(false);
			_newPerksEllipse.gameObject.SetActive(false);
			_newPerksLabel.gameObject.SetActive(false);
		}

		private void HideNewItemsBadge()
		{
			_newItemsCircle.gameObject.SetActive(false);
			_newItemsEllipse.gameObject.SetActive(false);
			_newItemsLabel.gameObject.SetActive(false);
		}

		public void UpdateNewPerks()
		{
			_newPerksCircle.gameObject.SetActive(false);
			_newPerksEllipse.gameObject.SetActive(false);
			_newPerksLabel.gameObject.SetActive(false);
			int num = ListSF.GetRoster().GetPerks().GetFreePerkPoints() + ListSF.GetRoster().GetAchievements().CountCompletedAchievements() + ListSF.GetRoster().CountNewTricks() + ListSF.GetRoster().CountOwnedSeals();
			if (num > 0)
			{
				if (num < 10)
				{
					_newPerksCircle.gameObject.SetActive(true);
				}
				else
				{
					_newPerksEllipse.gameObject.SetActive(true);
				}
				_newPerksLabel.text = num.ToString();
				_newPerksLabel.gameObject.SetActive(true);
			}
		}

		public void UpdateNewItems()
		{
			_newItemsCircle.gameObject.SetActive(false);
			_newItemsEllipse.gameObject.SetActive(false);
			_newItemsLabel.gameObject.SetActive(false);
			int num = ListSF.GetItems().GetNewItemsCount();
			if (num > 0)
			{
				if (num < 10)
				{
					_newItemsCircle.gameObject.SetActive(true);
				}
				else
				{
					_newItemsEllipse.gameObject.SetActive(true);
				}
				_newItemsLabel.text = num.ToString();
				_newItemsLabel.gameObject.SetActive(true);
			}
		}

		public void UpdateMaterials()
		{
			if ((bool)_materials)
			{
				_materials.UpdateView();
				UpdatePanelsLayout();
			}
		}

		private void OnMenuEvent(object data)
		{
		}

		private void CloseMenuIfNotCurrent()
		{
			ScreenType cCGJDFLIKFN = Module.GetInstance().GetCurrentScreenType();
			if (currentButton != GetButtonFromScreen(cCGJDFLIKFN))
			{
				CloseMenu(0.25f);
			}
		}

		private void CloseMenuAnimated()
		{
			CloseMenu(0.25f);
		}

		public void SetCurrentButton(SectionButton KLNKEPMAGKF)
		{
			if ((bool)currentButton)
			{
				currentButton.interactable = true;
				currentButton.transition = Selectable.Transition.ColorTint;
				currentButton.SetPressType(ButtonStateExtensions.ButtonPressType.PressNormal);
			}
			currentButton = KLNKEPMAGKF;
			if ((bool)currentButton)
			{
				currentButton.interactable = false;
				currentButton.transition = Selectable.Transition.SpriteSwap;
				currentButton.SetPressType(ButtonStateExtensions.ButtonPressType.PressInactive);
			}
		}

		public void RecreateMoney()
		{
			InitMoneyPanel();
			UpdatePanelsLayout();
			UpdateMenu();
		}

		public void SetNormalViewMode(bool DGNLFEPIANN)
		{
			_energy.gameObject.SetActive(false);
			_money.SetNormalViewMode();
			HideMaterialsForNormalView(DGNLFEPIANN);
		}

		public void SetForgeViewMode(bool DGNLFEPIANN)
		{
			ApplyForgeView(DGNLFEPIANN);
		}

		private void HideMaterialsForNormalView(bool DGNLFEPIANN)
		{
			if (_materials != null)
			{
				_materials.gameObject.SetActive(false);
			}
		}

		private void HideMaterialsPanel()
		{
			_materials.gameObject.SetActive(false);
		}

		private void ApplyForgeView(bool DGNLFEPIANN)
		{
			ApplyForgeMoneyView();
			if (_materials != null)
			{
				_materials.gameObject.SetActive(true);
				_materials.UpdateView();
			}
		}

		private void ApplyForgeMoneyView()
		{
			_energy.gameObject.SetActive(false);
			_money.SetForgeViewMode();
		}

		private void OnDojoEntered()
		{
			UpdateMenuExtras();
			if (!btnDojoDisciple)
			{
			}
		}

		private void OnDojoExited()
		{
			UpdateMenuExtras();
			if (!btnDojoDisciple)
			{
			}
		}

		public Button GetScrollBtn()
		{
			return Scroll.GetButton();
		}

		public Button GetSkipBtn()
		{
			return _skipTutorialBtn;
		}
	}
}
