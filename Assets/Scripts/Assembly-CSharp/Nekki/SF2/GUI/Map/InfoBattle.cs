using System.Collections.Generic;
using Nekki.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Map
{
	public class InfoBattle : SFMonoBehaviour<object>
	{
		private enum InfoLayer
		{
			zIcon = 0,
			zContainer = 1,
			zContent = 2
		}

		private enum ContainerPart
		{
			zContainerSprite = 0,
			zSlider = 1,
			zComplete = 2
		}

		private enum IndicatorsPart
		{
			zIndicators = 0,
			zLabel = 1
		}

		private enum DebugFightAction
		{
			SkipFight = 0,
			RemoveFight = 1,
			PeriodicReset = 2
		}

		public const string FOLDER_BATTLES = "UI/battles/";

		public const int MAX_DESCRIPTION_LINES = 3;

		public const int MAX_TOUR_LINES = 1;

		public const int MAX_PERIODIC_DESCRIPTION_LINES = 3;

		public const float DESCRIPTION_LABEL_LINE_HEIGHT = 80f;

		public const float DESCRIPTION_LABEL_INTERMISSION_LINE_HEIGHT = 80f;

		public const int MAX_BIG_ICON_FIGHTS = 5;

		public const int SURVIVAL_LABEL_INTERMISSION_SIZE = 50;

		public const int SURVIVAL_MONEY_INTERMISSION_SIZE = 52;

		public const float INTERMISSION_MIN_OFFSET_Y = 40f;

		public const float INTERMISSION_MAX_OFFSET_Y = 40f;

		public const float INTERMISSION_MONEY_OFFSET_Y = 40f;

		public const float INTERMISSION_DESCRIPTION_OFFSET_Y = 16f;

		private FightIDS currentFightIds = new FightIDS();

		[SerializeField]
		private LabelAlias _lblBattleName;

		[SerializeField]
		private ResolutionImage _icon;

		[SerializeField]
		private ResolutionImage _altImage;

		[SerializeField]
		private LabelButton _btnFight;

		[SerializeField]
		private Button _btnSkipFight;

		[SerializeField]
		private Button _btnRemoveFight;

		[SerializeField]
		private Button _btnPeriodicReset;

		private int survivalSliderIndex;

		[SerializeField]
		private ProgressBar _immunityBar;

		[SerializeField]
		private Text _immunityLabel;

		private float immunityValue;

		private float immunityMaxValue;

		private int unusedFrameCounter;

		private ContentBase currentContent;

		[SerializeField]
		private ContentClosed _contentClosed;

		[SerializeField]
		private ContentCompleteOrLocked _contentCompleteOrLocked;

		[SerializeField]
		private ContentTourChallBoss _contentTourChall;

		[SerializeField]
		private ContentTourChallBoss _contentBosses;

		[SerializeField]
		private ContentBossesFinal _contentBossesFinal;

		[SerializeField]
		private ContentDuel _contentDuel;

		[SerializeField]
		private ContentSurvival _contentSurvival;

		[SerializeField]
		private ContentSurvival _contentBossIntermission;

		public void Init()
		{
			_contentDuel.AddEventListener(0, OnCallUpdate);
			InitFightButton();
			InitSkipFightButton();
			InitRemoveFightButton();
			InitPeriodicResetButton();
			ClearInfo();
			Module.GetInstance().AddEventListener(4, OnModuleEvent);
		}

		~InfoBattle()
		{
		}

		public void OnCallUpdate(object DPOOIONCEOA)
		{
			UpdateBattleInfo(DPOOIONCEOA as Battle);
		}

		public void UpdateBattleInfo(Battle DPOOIONCEOA)
		{
			ClearInfo();
			FightList jDIPBIHBGPF = null;
			currentFightIds.SetFightIDSByString(string.Empty);
			if (DPOOIONCEOA == null)
			{
				return;
			}
			RosterBattle dDNLCGOPAGC = DPOOIONCEOA.GetRosterBattle();
			jDIPBIHBGPF = GameUtils.GetOpenFight(DPOOIONCEOA);
			if (jDIPBIHBGPF != null)
			{
				currentFightIds = new FightIDS(jDIPBIHBGPF.FightId);
			}
			else
			{
				currentFightIds.SetFightIDSByZBF(string.Copy((DPOOIONCEOA.GetZone() == null) ? string.Empty : DPOOIONCEOA.GetZone().get_Name()), string.Copy(DPOOIONCEOA.get_Name()), string.Empty);
			}
			if (jDIPBIHBGPF != null && jDIPBIHBGPF.AltImage != string.Empty)
			{
				string kHPKDMGDMAB = string.Empty;
				if (jDIPBIHBGPF != null && jDIPBIHBGPF.AltImage != string.Empty)
				{
					kHPKDMGDMAB = jDIPBIHBGPF.AltImage;
				}
				UpdateAltImage(SF2Paths.GetUsersUiPath(), kHPKDMGDMAB);
			}
			_lblBattleName.SetAlias(DPOOIONCEOA.GetTitle());
			UpdateIcon(DPOOIONCEOA.GetPreviewIcon());
            if (Eclipse.Modding.ModModeRuntime.TryCurrent(DPOOIONCEOA, out var modeFight))
            {
                string reason = modeFight == null ? "This mode is complete or currently unavailable." : Eclipse.Modding.ModModeRuntime.EntryStatus(modeFight);
                if (reason != "")
                {
                    _contentClosed.InitText(reason); currentContent = _contentClosed; currentContent.gameObject.SetActive(true);
                }
                else
                {
                    string title = LocalizationManager.GetString(DPOOIONCEOA.GetTitle());
                    if (modeFight.get_Type() == BattleType.FightRaid)
                    {
                        _lblBattleName.set_text(title);
                        _contentBossesFinal.Init(DPOOIONCEOA, modeFight);
                        currentContent = _contentBossesFinal;
                    }
                    else
                    {
                        _lblBattleName.set_text(title + Eclipse.Modding.ModModeRuntime.ProgressLabel(modeFight));
                        _contentTourChall.Init(DPOOIONCEOA, modeFight);
                        currentContent = _contentTourChall;
                    }
                    currentContent.gameObject.SetActive(true); UpdateFightButton(DPOOIONCEOA, modeFight);
                }
                return;
            }
			if (DPOOIONCEOA.get_Type() == BattleType.FightRaid)
			{
				_contentClosed.InitText("This raid is not available offline.");
				currentContent = _contentClosed;
				currentContent.gameObject.SetActive(true);
				_btnFight.gameObject.SetActive(false);
				return;
			}
			if (jDIPBIHBGPF != null && GameUtils.AutoWinPending && GameUtils.AutoWinFightKey != jDIPBIHBGPF.Location + jDIPBIHBGPF.Battle.get_Name() + jDIPBIHBGPF.Name)
			{
				GameUtils.AutoWinPending = false;
			}
			if (_btnRemoveFight != null)
			{
				if (DPOOIONCEOA.HasCompletedFight() && SystemProperties.IsDebug())
				{
					_btnRemoveFight.gameObject.SetActive(true);
				}
				else
				{
					_btnRemoveFight.gameObject.SetActive(false);
				}
			}
			if ((dDNLCGOPAGC != null && dDNLCGOPAGC.IsLocked()) || DPOOIONCEOA.get_Type() == BattleType.FightFake)
			{
				_contentClosed.Init(DPOOIONCEOA.GetDescription());
				currentContent = _contentClosed;
				currentContent.gameObject.SetActive(true);
			}
			else if (jDIPBIHBGPF != null && jDIPBIHBGPF.IsLocked)
			{
				_contentClosed.Init(jDIPBIHBGPF.GetDescription());
				currentContent = _contentClosed;
				currentContent.gameObject.SetActive(true);
			}
			else if ((jDIPBIHBGPF != null && !jDIPBIHBGPF.IsReplayAvailable()) || DPOOIONCEOA.GetStatus() == ConditionStatus.StatusComplete || DPOOIONCEOA.GetStatus() == ConditionStatus.StatusIncomplete)
			{
				_contentCompleteOrLocked.Init(DPOOIONCEOA, currentFightIds);
				currentContent = _contentCompleteOrLocked;
				currentContent.gameObject.SetActive(true);
			}
			else
			{
				InitContentForBattle(DPOOIONCEOA, jDIPBIHBGPF);
			}
		}

		public void ClearInfo()
		{
			if (currentContent != null)
			{
				currentContent.gameObject.SetActive(false);
				currentContent = null;
			}
			currentFightIds.SetFightIDSByString(string.Empty);
			_lblBattleName.set_text(string.Empty);
			_btnFight.gameObject.SetActive(false);
			if (_btnRemoveFight != null)
			{
				_btnRemoveFight.gameObject.SetActive(false);
			}
			if (_btnRemoveFight != null)
			{
				_btnRemoveFight.gameObject.SetActive(false);
			}
			if (_altImage != null)
			{
				_altImage.gameObject.SetActive(false);
			}
			if (_immunityBar != null)
			{
				_immunityBar.gameObject.SetActive(false);
			}
			if (_immunityLabel != null)
			{
				_immunityLabel.gameObject.SetActive(false);
			}
		}

		public Battle GetCurrentBattle()
		{
			if (GetCurrentFight() != null)
			{
				return GetCurrentFight().Battle;
			}
			return ListSF.GetBattleById(currentFightIds);
		}

		public FightList GetCurrentFight()
		{
			return ListSF.GetFightById(currentFightIds);
		}

		public LabelButton GetBtnFight()
		{
			return _btnFight;
		}

		public Button GetBtnPlayVideo()
		{
			return _contentCompleteOrLocked.GetBtnPlayVideo();
		}

		public void StartCurrentFight()
		{
			FightList jDIPBIHBGPF = ListSF.GetFightById(currentFightIds);
			if (jDIPBIHBGPF != null)
			{
				StartFight(jDIPBIHBGPF);
			}
		}

		public void Refresh()
		{
			UpdateBattleInfo(GetCurrentBattle());
		}

		public bool GetIsHavePrize()
		{
			return false;
		}

		public float GetPrizePositionY()
		{
			return 0f;
		}

		private void InitFightButton()
		{
			_btnFight.SetAlias("startFight");
			_btnFight.onClick.AddListener(OnFightButtonClicked);
			_btnFight.gameObject.SetActive(false);
		}

		private void InitSkipFightButton()
		{
			_btnSkipFight.gameObject.SetActive(SystemProperties.IsDebug());
			if (SystemProperties.IsDebug())
			{
				_btnSkipFight.onClick.AddListener(() =>
				{
					OnDebugAction(DebugFightAction.SkipFight);
				});
			}
		}

		private void InitPeriodicResetButton()
		{
			_btnPeriodicReset.gameObject.SetActive(SystemProperties.IsDebug());
			if (SystemProperties.IsDebug())
			{
				_btnPeriodicReset.onClick.AddListener(() =>
				{
					OnDebugAction(DebugFightAction.PeriodicReset);
				});
			}
		}

		private void InitRemoveFightButton()
		{
			_btnRemoveFight.gameObject.SetActive(SystemProperties.IsDebug());
			if (SystemProperties.IsDebug())
			{
				_btnRemoveFight.onClick.AddListener(() =>
				{
					OnDebugAction(DebugFightAction.RemoveFight);
				});
			}
		}

		private void UpdateIcon(string DAAIHHNLONA)
		{
			_icon.set_TexturePath("UI/battles/");
			_icon.set_SpriteName(DAAIHHNLONA);
		}

		private void UpdateAltImage(string KBIHPPDNFJD, string KHPKDMGDMAB)
		{
			if (!(KHPKDMGDMAB == string.Empty))
			{
				_altImage.set_SpriteName(KHPKDMGDMAB);
			}
		}

		private void UpdateFightButton(Battle DPOOIONCEOA, FightList KOMGFJOCEDN)
		{
			_btnFight.gameObject.SetActive(true);
			_btnFight.interactable = KOMGFJOCEDN == null ||
				KOMGFJOCEDN.MeetsPlayerItemRequirements(ListSF.GetRoster().get_Parameters());
			string alias = string.Empty;
			if (KOMGFJOCEDN != null && KOMGFJOCEDN.get_Type() == BattleType.FightRaid)
			{
				BattleRaid pAHLFJIMKCL = DPOOIONCEOA as BattleRaid;
				if (pAHLFJIMKCL != null)
				{
					List<CurrencyCostRule> list = KOMGFJOCEDN.GetCurrencyCostRules();
					if (list.Count == 0)
					{
						alias = "enterRaid";
					}
					else
					{
						string text = list[0].GetCurrencyName();
						int num = list[0].GetCurrencyValue();
					}
				}
			}
			else if (KOMGFJOCEDN != null && KOMGFJOCEDN.HasCurrencyCost())
			{
				List<CurrencyCostRule> list2 = KOMGFJOCEDN.GetCurrencyCostRules();
				string gOHIIMFFFJI = list2[0].GetCurrencyName();
				int num2 = list2[0].GetCurrencyValue();
				GameCurrency cJJOFMHLFFM = GameUtils.GameCurrencies.GetCurrencyByName(gOHIIMFFFJI);
				string mJBPMLCLMFN = cJJOFMHLFFM.Icon;
				alias = "startFight |<" + mJBPMLCLMFN + "><offsetX=10>" + num2 + "</>";
			}
			else
			{
				alias = "startFight";
			}
			_btnFight.SetAlias(alias);
		}

		private void OnFightButtonClicked()
		{
			GameUtils.AutoWinPending = false;
			FightList jDIPBIHBGPF = ListSF.GetFightById(currentFightIds);
			if (jDIPBIHBGPF == null)
			{
				return;
			}
			if (Eclipse.Modding.ModModeRuntime.IsRaid(jDIPBIHBGPF))
            {
                StartFight(jDIPBIHBGPF);
            }
            else if (jDIPBIHBGPF.get_Type() == BattleType.FightPeriodic)
			{
				if (!SystemProperties.CheckOnline())
				{
					DialogsOpener.OpenDuelLockedDialog();
				}
				else
				{
					GlobalTimer.ServerTimeSync(OnServerTimeSynced, OnServerTimeSynced);
				}
			}
			else if (jDIPBIHBGPF.get_Type() == BattleType.FightRaid)
			{
				Battle cNAOMDMIGLJ = jDIPBIHBGPF.Battle;
				BattleRaid pAHLFJIMKCL = (BattleRaid)cNAOMDMIGLJ;
				if (pAHLFJIMKCL.CanAffordFightCost(jDIPBIHBGPF))
				{
					bool flag = ListSF.GetRoster().GetRaidRemindRandomRule();
					if (jDIPBIHBGPF.GetRandomRules().Count != 0 && flag)
					{
					}
				}
				else
				{
					List<CurrencyCostRule> list = jDIPBIHBGPF.GetCurrencyCostRules();
					if (list.Count != 0)
					{
						ShowRaidNotEnoughKeys(list[0] as RaidCurrencyCostRule);
					}
				}
			}
			else
			{
				StartFight(jDIPBIHBGPF);
			}
		}

		private void InitContentForBattle(Battle DPOOIONCEOA, FightList KOMGFJOCEDN)
		{
			switch (DPOOIONCEOA.get_Type())
			{
			case BattleType.FightChallenge:
			case BattleType.FightTournament:
			case BattleType.FightStory:
			case BattleType.FightReplayable:
				_contentTourChall.Init(DPOOIONCEOA, KOMGFJOCEDN);
				currentContent = _contentTourChall;
				currentContent.gameObject.SetActive(true);
				break;
			case BattleType.FightBosses:
			case BattleType.FightBossesReplayable:
			case BattleType.FightFinalTitan:
			{
				int num = DPOOIONCEOA.GetFightCount();
				if (KOMGFJOCEDN.Index != num - 1)
				{
					_contentBosses.Init(DPOOIONCEOA, KOMGFJOCEDN);
					currentContent = _contentBosses;
				}
				else
				{
					_contentBossesFinal.Init(DPOOIONCEOA, KOMGFJOCEDN);
					currentContent = _contentBossesFinal;
				}
				currentContent.gameObject.SetActive(true);
				break;
			}
			case BattleType.FightFinal:
			case BattleType.FightFinalReplayable:
				_contentBossesFinal.Init(DPOOIONCEOA, KOMGFJOCEDN);
				currentContent = _contentBossesFinal;
				currentContent.gameObject.SetActive(true);
				break;
			case BattleType.FightPeriodic:
				_contentDuel.Init(DPOOIONCEOA, KOMGFJOCEDN);
				currentContent = _contentDuel;
				currentContent.gameObject.SetActive(true);
				break;
			case BattleType.FightSurvival:
				_contentSurvival.Init(DPOOIONCEOA);
				currentContent = _contentSurvival;
				currentContent.gameObject.SetActive(true);
				break;
			case BattleType.FightBossesIntermission:
				_contentBossIntermission.Init(DPOOIONCEOA);
				currentContent = _contentBossIntermission;
				currentContent.gameObject.SetActive(true);
				break;
			default:
				GameLog.Write("ERROR: openStatus() - unknown fight type: " + DPOOIONCEOA.get_Type());
				break;
			}
			UpdateFightButton(DPOOIONCEOA, KOMGFJOCEDN);
		}

		private void StartFight(FightList KGKDKENMAOA)
		{
			if (KGKDKENMAOA == null ||
				!KGKDKENMAOA.MeetsPlayerItemRequirements(ListSF.GetRoster().get_Parameters()))
			{
				return;
			}
			Battle cNAOMDMIGLJ = KGKDKENMAOA.Battle;
			if (cNAOMDMIGLJ.get_Type() == BattleType.FightSurvival)
			{
				ListSF.GetRoster().set_IndexSlider((uint)survivalSliderIndex);
			}
			Battle dPOOIONCEOA = ((cNAOMDMIGLJ.get_Type() != BattleType.FightBosses && cNAOMDMIGLJ.get_Type() != BattleType.FightBossesReplayable && cNAOMDMIGLJ.get_Type() != BattleType.FightFinalTitan) ? null : cNAOMDMIGLJ);
			GameUtils.StartFight(KGKDKENMAOA, false, dPOOIONCEOA);
		}

		private void ResetPeriodicBattle()
		{
			Battle currentBattle = GetCurrentBattle();
			if (currentBattle != null)
			{
				if (currentBattle.get_Type() == BattleType.FightPeriodic)
				{
					BattlePeriodic.Reset(false);
				}
				UpdateBattleInfo(currentBattle);
			}
		}

		private void OnDebugAction(DebugFightAction PNBIFIIMEDL)
		{
			Battle currentBattle = GetCurrentBattle();
			if (currentBattle == null)
			{
				return;
			}
			switch (PNBIFIIMEDL)
			{
			case DebugFightAction.PeriodicReset:
				ResetPeriodicBattle();
				return;
			case DebugFightAction.RemoveFight:
				RemoveLinkedQuestBattle();
				currentBattle.ResetLastFightProgress();
				ListSF.RefreshConditionStatuses();
				UpdateBattleInfo(currentBattle);
				return;
			}
			GameUtils.AutoWinPending = true;
			FightList currentFight = GetCurrentFight();
			GameUtils.AutoWinFightKey = ((currentFight != null) ? (currentFight.Location + currentFight.Battle.get_Name() + currentFight.Name) : string.Empty);
			if (currentFight == null)
			{
				return;
			}
			if (currentFight.get_Type() == BattleType.FightPeriodic)
			{
				if (!SystemProperties.CheckOnline())
				{
					DialogsOpener.OpenDuelLockedDialog();
				}
				else
				{
					GlobalTimer.ServerTimeSync(OnServerTimeSynced, OnServerTimeSynced);
				}
				return;
			}
			FightList jDIPBIHBGPF = currentFight;
			StartFight(currentFight);
			if (jDIPBIHBGPF != null)
			{
				int num = jDIPBIHBGPF.GetRewards().Count - 2;
				if (num < 0)
				{
					num = 0;
				}
				jDIPBIHBGPF.RewardIndex = num;
			}
			UpdateBattleInfo(GetCurrentBattle());
		}

		private void OnServerTimeSynced()
		{
			if (!GlobalTimer.get_IsSynchronized())
			{
				DialogsOpener.OpenDuelLockedDialog();
				UpdateBattleInfo(GetCurrentBattle());
				return;
			}
			FightList currentFight = GetCurrentFight();
			if (currentFight != null && !currentFight.IsReplayAvailable())
			{
				UpdateBattleInfo(GetCurrentBattle());
			}
			else if (currentFight != null)
			{
				StartFight(currentFight);
			}
		}

		private void OnModuleEvent(object data)
		{
			GameUtils.AutoWinPending = false;
		}

		private void RemoveLinkedQuestBattle()
		{
			FightList currentFight = GetCurrentFight();
			if (currentFight != null && currentFight.FightId.ToString() == "ZONE_6|BOSS_SAMURAI|6")
			{
				FightIDS dIAIIPCBMFL = new FightIDS("ZONE_6", "QuestBattle", string.Empty);
				Battle cGJCGEBPCAF = ListSF.GetBattleById(dIAIIPCBMFL);
				if (cGJCGEBPCAF != null)
				{
					cGJCGEBPCAF.ResetLastFightProgress();
				}
			}
		}

		private void ShowRaidNotEnoughKeys(RaidCurrencyCostRule HNBFMAKFJAM)
		{
			if (HNBFMAKFJAM == null)
			{
				GameLog.Error("showRaidsNotEnoughKeys rule is NULL");
			}
			else
			{
				string text = HNBFMAKFJAM.GetCurrencyName();
			}
		}

		private void OnRaidUpdated(object data)
		{
			Battle currentBattle = GetCurrentBattle();
			if (currentBattle != null && currentBattle.get_Type() == BattleType.FightRaid)
			{
				Refresh();
			}
		}
	}
}
