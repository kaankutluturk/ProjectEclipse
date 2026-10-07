using System.Collections.Generic;
using Nekki.SF2.Core.Fights;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Menu;
using Nekki.SF2.GUI.Profile;
using Nekki.SF2.GUI.Shop;
using UnityEngine;
using UnityEngine.UI;

public class ProfileScene : Scene<ProfileScene>
{
	private const int FadeFrameCount = 30;

	private const string SkillImagePath = "image/skills/";

	private const int PreviewTimeout = 1000;

	private const int PanelSlideStep = 30;

	public MainMenu mainMenu;

	[SerializeField]
	private SpriteRenderer _backgroundLeft;

	[SerializeField]
	private SpriteRenderer _backgroundRight;

	[SerializeField]
	private UserPerksSprite _leftPanel;

	[SerializeField]
	private RightInfoSprite _rightPanel;

	[SerializeField]
	public ModelContainer ModelContainer;

	[SerializeField]
	private GameObject _profileTableViewCellPrefab;

	[SerializeField]
	private TableView _perksTable;

	[SerializeField]
	private GameObject _perkCellPrefab;

	[SerializeField]
	private PerksController _perksCtrl;

	[SerializeField]
	private TableView _tricksTable;

	[SerializeField]
	private GameObject _trickCellPrefab;

	[SerializeField]
	private TricksController _tricksCtrl;

	[SerializeField]
	private TableView _achievementsTable;

	[SerializeField]
	private GameObject _achievementCellPrefab;

	private AchievementsController achievementsController;

	[SerializeField]
	private TableView _sealsTable;

	[SerializeField]
	private GameObject _sealCellPrefab;

	private SealsController sealsController;

	private TableView activeTable;

	private bool isGameCenterConnected;

	private InfoAnimation pendingPreviewAnimation;

	private bool isFading;

	private bool isFadingIn = true;

	private bool isPreviewAnimationPlaying;
	private bool _trickPreviewActive;
	private string _previewAnimationName;
	public event System.Action<object> TrickPreviewCompleted;
	public event System.Action<object> ProfileClosing;

	private float uiAlpha = 1f;

	private int nextSubItemId;

	[SerializeField]
	private SectionButton _btnPerks;

	[SerializeField]
	private SectionButton _btnTricks;

	[SerializeField]
	private SectionButton _btnAchievements;

	[SerializeField]
	private SectionButton _btnSeals;

	private List<SectionButton> sectionButtons = new List<SectionButton>();

	private SectionButton activeSectionButton;

	[SerializeField]
	private SFButton _showAchievementButton;

	private SliderType currentSliderType;

	[SerializeField]
	private ResolutionImage _achievementCircle;

	[SerializeField]
	private ResolutionImage _achievementEllipse;

	[SerializeField]
	private Text _achievementLabel;

	[SerializeField]
	private ResolutionImage _trickCircle;

	[SerializeField]
	private ResolutionImage _trickEllipse;

	[SerializeField]
	private Text _trickLabel;

	[SerializeField]
	private ResolutionImage _perkCircle;

	[SerializeField]
	private ResolutionImage _perkEllipse;

	[SerializeField]
	private Text _perkLabel;

	[SerializeField]
	private ResolutionImage _sealCircle;

	[SerializeField]
	private ResolutionImage _sealEllipse;

	[SerializeField]
	private Text _sealLabel;

	private float leftPanelHiddenX;

	private float leftPanelShownX;

	private bool isPanelSliding;

	private bool isPanelShowing;

	public List<SubItem> SubItems = new List<SubItem>();

	private SubItem selectedSubItem;

	[SerializeField]
	private CanvasGroup _profileUIGroup;

	[SerializeField]
	private CanvasGroup _bottomUIGroup;

	public override ScreenType SceneType
	{
		get
		{
			return get_SceneId();
		}
	}

	public override ScreenType get_SceneId()
	{
		return ScreenType.ModuleProfile;
	}

	protected override void Init(object data)
	{
		Eclipse.UI.ProfileBottomBarLayout.Configure(_btnPerks, _btnTricks, _btnAchievements, _btnSeals);
		isGameCenterConnected = GameCenterController.GetIsAuthenticated();
		mainMenu.Init();
		InitModelContainer();
		InitPanels();
		InitControllers();
		InitSectionButtons();
		HideAllBadges();
		InitAchievementButton();
		ScrollPerksToCurrentLevel();
		if (!HasAvailablePerks())
		{
			RestoreSliderPosition(_perksTable);
		}
		RestoreSliderPosition(_achievementsTable);
		RestoreSliderPosition(_tricksTable);
		RestoreSliderPosition(_sealsTable);
		SetScreen(SliderType.SliderPerks);
		UpdateBadges();
	}

	private void InitModelContainer()
	{
		ModelContainer.Init();
		ModelContainer.UpdateModel(null, StageType.Stage.STAGE_SHOP_START, "Profile");
		ModelContainer.AddEventListener(0, OnModelEvent);
	}

	public void OnTrickShow(object data)
	{
		if (data != null)
		{
			TrickSubItem trickSubItem = (TrickSubItem)data;
			Trick trick = trickSubItem.GetTrick();
			pendingPreviewAnimation = trick.Animation;
			_backgroundLeft.color = Constants.NormalBackgroundColor;
			_backgroundRight.color = Constants.NormalBackgroundColor;
			BeginTrickPreview();
		}
	}

	private void BeginTrickPreview()
	{
		_trickPreviewActive = true;
		_leftPanel.gameObject.SetActive(false);
		isFading = true;
		isFadingIn = false;
		_profileUIGroup.blocksRaycasts = false;
		_bottomUIGroup.blocksRaycasts = false;
		for (int i = 0; i < sectionButtons.Count; i++)
		{
		}
		SubItem.EnableAnimation(false);
		GameUtils.LockInput(false);
	}

	private void OnModelEvent(object data)
	{
		Model.EventModel modelEvent = data as Model.EventModel;
		InfoAnimation animation = modelEvent == null ? null : modelEvent.Data as InfoAnimation;
		if (isPreviewAnimationPlaying && animation != null && animation.Name == _previewAnimationName)
		{
			isPreviewAnimationPlaying = false;
			_previewAnimationName = null;
			ModelContainer.ResetModel();
			_backgroundLeft.color = Constants.DimmedBackgroundColor;
			_backgroundRight.color = Constants.DimmedBackgroundColor;
			EndTrickPreview();
		}
	}

	private void EndTrickPreview()
	{
		isFading = true;
		isFadingIn = true;
		_profileUIGroup.blocksRaycasts = true;
		_bottomUIGroup.blocksRaycasts = true;
		for (int i = 0; i < sectionButtons.Count; i++)
		{
		}
	}

	private void Update()
	{
		UpdatePanelSlide();
		UpdateFade();
		bool flag = GameCenterController.GetIsAuthenticated();
		if (isGameCenterConnected != flag)
		{
			UpdateAchievementButtonVisibility();
			isGameCenterConnected = flag;
		}
	}

	private void UpdateFade()
	{
		if (isFading)
		{
			if (isFadingIn)
			{
				FadeIn();
			}
			else
			{
				FadeOut();
			}
		}
	}

	private void FadeIn()
	{
		if (isFading)
		{
			float num = 1f / 30f;
			uiAlpha += num;
			if (uiAlpha >= 1f)
			{
				uiAlpha = 1f;
				isFading = false;
				_leftPanel.gameObject.SetActive(true);
				bool completedPreview = _trickPreviewActive;
				ReleaseTrickPreviewInput();
				if (completedPreview) TrickPreviewCompleted?.Invoke(null);
			}
			_profileUIGroup.alpha = uiAlpha;
		}
	}

	private void FadeOut()
	{
		if (isFading)
		{
			float num = 1f / 30f;
			uiAlpha -= num;
			if (uiAlpha <= 0f)
			{
				uiAlpha = 0f;
				isFading = false;
				PlayAnimation();
			}
			_profileUIGroup.alpha = uiAlpha;
		}
	}

	private void PlayAnimation()
	{
		if (pendingPreviewAnimation != null)
		{
			isPreviewAnimationPlaying = true;
			_previewAnimationName = pendingPreviewAnimation.Name;
			pendingPreviewAnimation = null;
			if (ModelContainer.TryPlayAnimation(_previewAnimationName)) return;
		}
		// A missing recovered animation must not strand the hidden UI/input lock.
		Debug.LogWarning("[Profile] Preview unavailable; restoring profile controls.");
		isPreviewAnimationPlaying = false;
		_previewAnimationName = null;
		_backgroundLeft.color = Constants.DimmedBackgroundColor;
		_backgroundRight.color = Constants.DimmedBackgroundColor;
		EndTrickPreview();
	}

	private void ReleaseTrickPreviewInput()
	{
		if (!_trickPreviewActive) return;
		_trickPreviewActive = false;
		SubItem.EnableAnimation(true);
		GameUtils.UnlockInput();
	}

	private void UpdateAchievementButtonVisibility()
	{
		if (SystemProperties.IsAndroidPlatform() && !AssemblyController.GetMarket().GetIsChinaMarket() && !AssemblyController.GetMarket().GetIsAmazonMarket() && !AssemblyController.GetMarket().GetIsAmazonMobileMarket() && GameCenterController.GetIsAuthenticated() && activeTable == _achievementsTable)
		{
			_showAchievementButton.gameObject.SetActive(true);
		}
		else
		{
			_showAchievementButton.gameObject.SetActive(false);
		}
	}

	private void InitControllers()
	{
		_perksCtrl = new PerksController(_perksTable, _perkCellPrefab);
		_perksTable.onSelectCell.AddListener(OnTableCellSelected);
		_perksTable.gameObject.SetActive(false);
		achievementsController = new AchievementsController(_achievementsTable, _achievementCellPrefab);
		_achievementsTable.onSelectCell.AddListener(OnTableCellSelected);
		_achievementsTable.onSelectCell.AddListener(OnTableCellChosen);
		_tricksCtrl = new TricksController(_tricksTable, _trickCellPrefab);
		_tricksTable.onSelectCell.AddListener(OnTableCellSelected);
		_tricksTable.onSelectCell.AddListener(OnTableCellChosen);
		HideAchievementsTable();
		HideTricksTable();
		InitSealsController();
	}

	public void SetScreen(SliderType LFLGCDNKNJI)
	{
		QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
		if (currentSliderType != LFLGCDNKNJI && GameUtils.NotifyTabChanged(GameUtils.GetSliderTypeByName(hHKLFIIBIFF.currentTabName), LFLGCDNKNJI))
		{
			return;
		}
		if (LFLGCDNKNJI == SliderType.SliderPerks)
		{
			ShowLeftPanel();
		}
		else
		{
			HideLeftPanel();
		}
		if (activeTable != null)
		{
			activeTable.gameObject.SetActive(false);
		}
		if (activeSectionButton != null && activeTable != null)
		{
			activeSectionButton.interactable = true;
		}
		UpdateBadges();
		_rightPanel.Clear();
		switch (LFLGCDNKNJI)
		{
		case SliderType.SliderPerks:
		{
			ShowLeftPanel();
			activeTable = _perksTable;
			activeSectionButton = _btnPerks;
			string noContentMessage = ((!HasAvailablePerks()) ? "profileNoSkills" : "profileSelectSkill");
			_rightPanel.SetNoContentMessage(noContentMessage);
			break;
		}
		case SliderType.SliderTricks:
			HideLeftPanel();
			activeTable = _tricksTable;
			activeSectionButton = _btnTricks;
			activeTable.ScrollToCell(0);
			OnTableCellChosen(0);
			UpdateVisibleCellStates(_tricksTable);
			UpdateTricksController();
			ClearNewTrickFlags();
			break;
		case SliderType.SliderAchievements:
			HideLeftPanel();
			activeTable = _achievementsTable;
			activeSectionButton = _btnAchievements;
			if (achievementsController.HasNewAchievement())
			{
				achievementsController.ScrollToFirstNewAchievement();
			}
			else
			{
				activeTable.ScrollToCell(0);
			}
			OnTableCellChosen(0);
			break;
		case SliderType.SliderSeals:
			HideLeftPanel();
			ClearNewSealFlags();
			UpdateBadges();
			activeTable = _sealsTable;
			activeSectionButton = _btnSeals;
			activeTable.ScrollToCell(0);
			OnTableCellChosen(0);
			break;
		default:
			GameLog.Write("ERROR: ProfileScreen - onTypeSelect (data = %i)", LFLGCDNKNJI);
			break;
		}
		if ((bool)activeTable)
		{
			activeTable.gameObject.SetActive(true);
		}
		if (activeSectionButton != null)
		{
			activeSectionButton.interactable = false;
		}
		UpdateAchievementButtonVisibility();
		currentSliderType = LFLGCDNKNJI;
		hHKLFIIBIFF.currentTabName = GameUtils.SliderNames[currentSliderType];
	}

	public void ScrollToItemByName(SliderType _sliderType, string OHCGEEEKEJH)
	{
		SetScreen(_sliderType);
		if (OHCGEEEKEJH != string.Empty)
		{
			switch (_sliderType)
			{
			case SliderType.SliderPerks:
				_perksCtrl.ScrollToPerk(OHCGEEEKEJH);
				break;
			case SliderType.SliderTricks:
				_tricksCtrl.SelectTrickByName(OHCGEEEKEJH);
				break;
			case SliderType.SliderAchievements:
				achievementsController.ScrollToAchievement(OHCGEEEKEJH);
				break;
			case SliderType.SliderSeals:
				sealsController.ScrollToSeal(OHCGEEEKEJH);
				break;
			}
		}
	}

	public void AddSubItem(SubItem item)
	{
		item.AddEventListener(10, OnSubItemClick);
		item.ButtonId = nextSubItemId++;
		SubItems.Add(item);
	}

	public LabelButton GetBtnPerkImprove()
	{
		return _rightPanel.GetBtnPerkImprove();
	}

	public LabelButton GetBtnStrikeShow()
	{
		return _rightPanel.GetBtnStrikeShow();
	}

	public TableView GetTableByType(SliderType _sliderType)
	{
		switch (_sliderType)
		{
		case SliderType.SliderPerks:
			return _perksTable;
		case SliderType.SliderTricks:
			return _tricksTable;
		case SliderType.SliderAchievements:
			return _achievementsTable;
		case SliderType.SliderSeals:
			return _sealsTable;
		default:
			return null;
		}
	}

	private void InitSectionButtons()
	{
		RegisterSectionButton(_btnPerks, SliderType.SliderPerks);
		RegisterSectionButton(_btnTricks, SliderType.SliderTricks);
		RegisterSectionButton(_btnAchievements, SliderType.SliderAchievements);
		RegisterSectionButton(_btnSeals, SliderType.SliderSeals);
		DisableSealsButtonIfEmpty();
	}

	private void HideAllBadges()
	{
		HideBadge(_achievementCircle, _achievementEllipse, _achievementLabel, _btnAchievements);
		HideBadge(_trickCircle, _trickEllipse, _trickLabel, _btnTricks);
		HideBadge(_perkCircle, _perkEllipse, _perkLabel, _btnPerks);
		HideBadge(_sealCircle, _sealEllipse, _sealLabel, _btnSeals);
	}

	private void HideBadge(ResolutionImage MJOHJFEFNGF, ResolutionImage DGLDPAOMOPH, Text ICBBNJMLDJH, SectionButton GAMILDJHFDB)
	{
		MJOHJFEFNGF.gameObject.SetActive(false);
		DGLDPAOMOPH.gameObject.SetActive(false);
		ICBBNJMLDJH.gameObject.SetActive(false);
		ICBBNJMLDJH.color = Constants.ProfileLabelColor;
	}

	private void HideAchievementsTable()
	{
		_achievementsTable.gameObject.SetActive(false);
	}

	private void HideTricksTable()
	{
		_tricksTable.gameObject.SetActive(false);
	}

	private void InitSealsController()
	{
		sealsController = new SealsController(_sealsTable, _sealCellPrefab);
		_sealsTable.gameObject.SetActive(false);
		_sealsTable.onSelectCell.AddListener(OnTableCellChosen);
		_sealsTable.ScrollToCell(_sealsTable.NumberOfRows() - 1);
	}

	private void InitAchievementButton()
	{
		_showAchievementButton.AddEventListener(2, OnClickButton);
	}

	private void UpdatePanelSlide()
	{
		if (isPanelSliding && _leftPanel != null)
		{
			if ((isPanelShowing && _leftPanel.transform.localPosition.x <= leftPanelShownX + 0.01f) || (!isPanelShowing && _leftPanel.transform.localPosition.x >= leftPanelHiddenX + 0.01f))
			{
				isPanelSliding = false;
			}
			else if (isPanelShowing)
			{
				_leftPanel.transform.SetLocalX(_leftPanel.transform.localPosition.x - 30f);
			}
			else
			{
				_leftPanel.transform.SetLocalX(_leftPanel.transform.localPosition.x + 30f);
			}
		}
	}

	private void ShowLeftPanel()
	{
		isPanelSliding = true;
		isPanelShowing = true;
	}

	private void HideLeftPanel()
	{
		isPanelSliding = true;
		isPanelShowing = false;
	}

	private void RegisterSectionButton(SectionButton GAMILDJHFDB, SliderType OKNNNLIPODI)
	{
		GAMILDJHFDB.ButtonId = (int)OKNNNLIPODI;
		GAMILDJHFDB.AddEventListener(2, OnSectionButtonClick);
		sectionButtons.Add(GAMILDJHFDB);
	}

	private void OnClickButton(object data)
	{
		if (GameCenterController.GetIsAuthenticated())
		{
			GameCenterController.ShowAchievements();
		}
	}

	private void OnSectionButtonClick(object data)
	{
		if (data != null)
		{
			SliderType screen = (SliderType)data;
			SetScreen(screen);
		}
	}

	private void OnTableCellSelected(object data)
	{
		if (!(activeTable == null))
		{
			SaveSliderPosition();
			if (activeTable == _perksTable)
			{
				OnPerksCellSelected();
			}
		}
	}

	private void OnTableCellChosen(object data)
	{
		if (activeTable == null || (activeTable != _tricksTable && activeTable != _achievementsTable && activeTable != _sealsTable))
		{
			return;
		}
		if (activeTable != _sealsTable)
		{
			ProfileCell profileCell = (ProfileCell)activeTable.get_SelectedCell();
			if (profileCell != null)
			{
				SubItem firstIcon = profileCell.GetFirstIcon();
				firstIcon.Choose();
			}
		}
		else
		{
			ShopTableViewCell shopTableViewCell = (ShopTableViewCell)activeTable.get_SelectedCell();
			if (shopTableViewCell != null)
			{
				ItemInfo itemInfo = shopTableViewCell.get_ItemInfo();
				_rightPanel.SetItemInfo(itemInfo);
			}
		}
	}

	public void OnSubItemClick(object data)
	{
		int oKNNNLIPODI = (int)data;
		if (selectedSubItem != null)
		{
			selectedSubItem.SetSelected(false);
		}
		selectedSubItem = FindSubItemById(oKNNNLIPODI);
		if (selectedSubItem == null)
		{
			return;
		}
		selectedSubItem.SetSelected(true);
		if (selectedSubItem.Data != null)
		{
			if (activeTable == _perksTable)
			{
				PerkContentData kAHIFHMHDAF = (PerkContentData)selectedSubItem.Data;
				kAHIFHMHDAF.LabelWidth = _rightPanel.GetLabelWidth();
				_rightPanel.SetPerkInfo(kAHIFHMHDAF);
			}
			else if (activeTable == _tricksTable)
			{
				TrickInfo trickInfo = (TrickInfo)selectedSubItem.Data;
				_rightPanel.SetTrickInfo(trickInfo);
			}
			else if (activeTable == _achievementsTable)
			{
				AchievementInfo achievementInfo = (AchievementInfo)selectedSubItem.Data;
				_rightPanel.SetAchievementInfo(achievementInfo);
			}
		}
	}

	public void OnPerkImprove(object data)
	{
		if (data == null)
		{
			return;
		}
		PerkSubItem perkSubItem = (PerkSubItem)data;
		ProfilePerk perk = perkSubItem.get_Perk();
		if (perk == null)
		{
			return;
		}
		RosterPerk hOGDBKBFFDJ = ListSF.GetRoster().GetPerks().AddOrUpgradePerk(perk);
		PerkHistory.Perk hNHILOOIIMO = ListSF.GetRoster().GetPerks().History.AddPerk(perk.GetPerkName(), perk.GetLevel());
		PerkTree.GetInstance().ApplyLearnedPerk(hNHILOOIIMO);
		_leftPanel.AddItem(hOGDBKBFFDJ.GetPerkInfo());
		perkSubItem.Choose();
		if (hNHILOOIIMO != null)
		{
			PerkCell perkCell = GetNextRowPerkCell((PerkCell)perkSubItem.ParentCell);
			ProfilePerkContainer fHPJJGPJLHD = PerkTree.GetInstance().GetNextContainerAfterLevel(hNHILOOIIMO.Level);
			if (perkCell != null && fHPJJGPJLHD != null)
			{
				_perksCtrl.RefreshCell(perkCell.get_RowNumber());
			}
			// Eclipse: bring the next level's choices into view once this one is learned.
			PerkCell learnedCell = perkSubItem.ParentCell as PerkCell;
			if (learnedCell != null && learnedCell.get_RowNumber() + 1 < _perksTable.NumberOfRows())
			{
				_perksTable.ScrollToCell(learnedCell.get_RowNumber() + 1, 0.5f);
			}
			_tricksCtrl.Reload();
			mainMenu.UpdateNewPerks();
			if (perkSubItem.IsInfoAnimation())
			{
			}
			Trick iHNIKIHKFHC = GameUtils.GetTrickByName(perk.GetMoveName());
			if (iHNIKIHKFHC != null)
			{
				ListSF.GetRoster().AddOpenTrick(iHNIKIHKFHC.Name);
			}
			UpdateBadges();
			ProfilePerk pLKCIINIFMJ = FindRejectedPerk(hNHILOOIIMO.Level, hOGDBKBFFDJ);
			ArgsDict kEMMIFBFDPK = new ArgsDict();
			if (hOGDBKBFFDJ != null)
			{
				kEMMIFBFDPK["learnedPerk"] = hOGDBKBFFDJ.GetPerkInfo();
			}
			if (pLKCIINIFMJ != null)
			{
				kEMMIFBFDPK["rejectedPerk"] = pLKCIINIFMJ.GetPerkInfo();
			}
			StatisticsCollector.LogEvent(StatisticsEvent.EventType.Perk, kEMMIFBFDPK);
		}
	}

	public void OnAchievementRewardTake(object data)
	{
		if (data == null)
		{
			return;
		}
		AchievementSubItem achievementSubItem = (AchievementSubItem)data;
		Achievement achievement = achievementSubItem.GetAchievement();
		if (!achievement.RewardClaimed)
		{
			achievement.RewardClaimed = true;
			achievement.SetIsNew(false);
			ListSF.GetRoster().GetAchievements().UnlockAchievement(achievement);
			Roster nKGLHEGIKKP = ListSF.GetRoster();
			if (achievement.MoneyPrize > 0)
			{
				nKGLHEGIKKP.SetMoney(nKGLHEGIKKP.GetMoney() + achievement.MoneyPrize);
			}
			if (achievement.BonusPrize > 0)
			{
				nKGLHEGIKKP.SetBonus(nKGLHEGIKKP.GetBonus() + achievement.BonusPrize, Roster.BalanceChangeType.CHANGE_ACHIEVEMENT);
			}
			mainMenu.UpdateMenu();
			Sound.PlaySound("snd_buy");
		}
		achievementSubItem.ResetOpacity();
		achievementSubItem.Choose();
		UpdateBadges();
		achievementsController.ScrollToFirstNewAchievement(ProfileGUI.SpeedScrollAchievements);
	}

	private void ScrollPerksToCurrentLevel()
	{
		int num = -1;
		int num2 = -1;
		List<ProfilePerk> list = PerkTree.GetInstance().GetProfilePerks();
		for (int i = 0; i < list.Count; i++)
		{
			ProfilePerk pLKCIINIFMJ = list[i];
			ProfilePerk.ProfilePerkState kMHBPKKCNPP = list[i].GetState();
			if (kMHBPKKCNPP == ProfilePerk.ProfilePerkState.PERK_LOCK || ListSF.GetRoster().GetLevel() < list[i].GetLevel())
			{
				break;
			}
			if (list[i].GetLevel() > num2)
			{
				num2 = list[i].GetLevel();
				num++;
			}
		}
		if (num >= 0 && num < _perksTable.NumberOfRows())
		{
			_perksTable.ScrollToCell(num);
		}
		else
		{
			_perksTable.ScrollToCell(0);
		}
	}

	private void UpdateTricksController()
	{
		if (!(activeTable != _tricksTable))
		{
			_tricksCtrl.ScrollToNewTrick();
		}
	}

	private void OnPerksCellSelected()
	{
	}

	private bool HasAvailablePerks()
	{
		List<ProfilePerk> list = PerkTree.GetInstance().GetProfilePerks();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].GetState() == ProfilePerk.ProfilePerkState.PERK_AVAILABLE && ListSF.GetRoster().GetLevel() >= list[i].GetLevel())
			{
				return true;
			}
		}
		return false;
	}

	private void SaveSliderPosition()
	{
		if (!(activeTable == null))
		{
			string text = GetSliderKey(activeTable);
			if (text != string.Empty)
			{
				GameUtils.SliderIndices.SetIndex(text, activeTable.GetCurrentCellRow());
			}
		}
	}

	private void RestoreSliderPosition(TableView BFGHBIMJHAK)
	{
		if (BFGHBIMJHAK == null)
		{
			return;
		}
		string text = GetSliderKey(BFGHBIMJHAK);
		if (text != string.Empty)
		{
			int num = GameUtils.SliderIndices.GetIndex(text);
			if (num >= 0 && num < BFGHBIMJHAK.NumberOfRows())
			{
				BFGHBIMJHAK.ScrollToCell(num);
			}
		}
	}

	private string GetSliderKey(TableView BFGHBIMJHAK = null)
	{
		string result = string.Empty;
		if (BFGHBIMJHAK == _tricksTable)
		{
			result = "SKILLS_SLIDER";
		}
		else if (BFGHBIMJHAK == _achievementsTable)
		{
			result = "ACHIEVEMENT_SLIDER";
		}
		else if (BFGHBIMJHAK == _perksTable)
		{
			result = "POWERLEVELING_SLIDER";
		}
		else if (BFGHBIMJHAK == _sealsTable)
		{
			result = "SEALS_SLIDER";
		}
		return result;
	}

	private SubItem FindSubItemById(int OKNNNLIPODI)
	{
		for (int i = 0; i < SubItems.Count; i++)
		{
			if (SubItems[i].ButtonId == OKNNNLIPODI)
			{
				return SubItems[i];
			}
		}
		return null;
	}

	private PerkCell GetNextRowPerkCell(PerkCell HJCPCBLCJJN)
	{
		if (HJCPCBLCJJN == null)
		{
			return null;
		}
		int num = HJCPCBLCJJN.get_RowNumber() + 1;
		if (num < _perksTable.NumberOfRows())
		{
			return (PerkCell)_perksTable.get_visibleCells().GetCellAtIndex(num);
		}
		return null;
	}

	private void DisableSealsButtonIfEmpty()
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (nKGLHEGIKKP == null)
		{
			return;
		}
		List<UserItem> list = nKGLHEGIKKP.GetInventory().FindItemsByType("Seal", string.Empty);
		int num = 0;
		foreach (UserItem item in list)
		{
			if (item.GetCount() > 0)
			{
				num++;
			}
		}
		if (num == 0)
		{
			_btnSeals.enabled = false;
		}
	}

	private void UpdateVisibleCellStates(TableView BFGHBIMJHAK)
	{
		Dictionary<int, TableViewCell> dictionary = BFGHBIMJHAK.get_visibleCells().GetCells();
		foreach (KeyValuePair<int, TableViewCell> item in dictionary)
		{
			ProfileCell profileCell = (ProfileCell)item.Value;
			profileCell.UpdateState();
		}
	}

	private void UpdateBadge(ResolutionImage MJOHJFEFNGF, ResolutionImage DGLDPAOMOPH, Text ICBBNJMLDJH, int count)
	{
		MJOHJFEFNGF.gameObject.SetActive(false);
		DGLDPAOMOPH.gameObject.SetActive(false);
		ICBBNJMLDJH.gameObject.SetActive(false);
		if (count > 0)
		{
			if (count < 10)
			{
				MJOHJFEFNGF.gameObject.SetActive(true);
			}
			else
			{
				DGLDPAOMOPH.gameObject.SetActive(true);
			}
			ICBBNJMLDJH.text = count.ToString();
			ICBBNJMLDJH.gameObject.SetActive(true);
		}
	}

	private void UpdateBadges()
	{
		UpdateBadge(_achievementCircle, _achievementEllipse, _achievementLabel, ListSF.GetRoster().GetAchievements().CountCompletedAchievements());
		UpdateBadge(_trickCircle, _trickEllipse, _trickLabel, ListSF.GetRoster().CountNewTricks());
		UpdateBadge(_perkCircle, _perkEllipse, _perkLabel, ListSF.GetRoster().GetPerks().GetFreePerkPoints());
		UpdateBadge(_sealCircle, _sealEllipse, _sealLabel, ListSF.GetRoster().CountOwnedSeals());
	}

	private void ClearNewTrickFlags()
	{
		List<Trick> list = GameUtils.GetPlayerTricks();
		foreach (Trick item in list)
		{
			if (item.IsNew)
			{
				item.IsNew = false;
				ListSF.GetRoster().RemoveOpenTrick(item.Name);
			}
		}
	}

	private void ClearNewSealFlags()
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (nKGLHEGIKKP == null)
		{
			return;
		}
		List<UserItem> list = nKGLHEGIKKP.GetInventory().FindItemsByType("Seal", string.Empty);
		foreach (UserItem item in list)
		{
			item.GetInfo().SetIsNew(false);
		}
	}

	private void OnUnusedEvent(object data)
	{
	}

	private ProfilePerk FindRejectedPerk(int GNLOCMLBNHF, RosterPerk PPPNCJLGJPE)
	{
		List<ProfilePerk> jOGBKOJCINM = PerkTree.GetInstance().GetContainerAtLevel(GNLOCMLBNHF).Perks;
		for (int i = 0; i < jOGBKOJCINM.Count; i++)
		{
			if (jOGBKOJCINM[i].GetPerkName() != PPPNCJLGJPE.get_Name())
			{
				return jOGBKOJCINM[i];
			}
		}
		return null;
	}

	private void InitPanels()
	{
		InitLeftPanel();
		InitRightPanel();
	}

	private void InitLeftPanel()
	{
		_leftPanel.Init();
		leftPanelShownX = _leftPanel.transform.localPosition.x;
		leftPanelHiddenX = leftPanelShownX + _leftPanel.GetComponent<RectTransform>().rect.width;
	}

	private void InitRightPanel()
	{
		_rightPanel.Init();
	}

	private new void OnDestroy()
	{
		ReleaseTrickPreviewInput();
		ProfileClosing?.Invoke(null);
		_perksTable.onSelectCell.RemoveAllListeners();
		_achievementsTable.onSelectCell.RemoveAllListeners();
		_achievementsTable.onSelectCell.RemoveAllListeners();
		_tricksTable.onSelectCell.RemoveAllListeners();
		_sealsTable.onSelectCell.RemoveAllListeners();
		ModelContainer.RemoveAllEventListener();
		SubItems.ForEach((SubItem i) =>
		{
			i.RemoveAllEventListener();
		});
		SubItems = null;
		_showAchievementButton.RemoveAllEventListener();
		sectionButtons.ForEach((SectionButton AAOIAEJJINO) =>
		{
			AAOIAEJJINO.RemoveAllEventListener();
		});
		sectionButtons = null;
		_perkCellPrefab = null;
		_profileTableViewCellPrefab = null;
		_trickCellPrefab = null;
		_achievementCellPrefab = null;
		_sealCellPrefab = null;
		base.OnDestroy();
	}
}
