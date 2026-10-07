using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;
using UnityEngine.Events;

namespace Nekki.SF2.GUI.Fight
{
	public class PreFight : MonoBehaviour
	{
		public class ViewerButtonClickEvent : UnityEvent<ViewerFight.ViewerButton>
		{
		}

		public ViewerButtonClickEvent OnButtonClick = new ViewerButtonClickEvent();

		public ScreenFight.ScreenFightTypeEvent OnStopScreen = new ScreenFight.ScreenFightTypeEvent();

		public UnityEvent OnAchievementMessageHide = new UnityEvent();

		[SerializeField]
		private GameObject viewerFightPrefab;

		[SerializeField]
		private GameObject screenFightPrefab;

		[SerializeField]
		private GameObject pauseScreenPrefab;

		[SerializeField]
		private GameObject endFightScreenPrefab;

		[SerializeField]
		private GameObject achievementMessagePrefab;

		private FightList fightList;

		private ViewerFight viewerFight;

		private ScreenFight screenFight;

		private PauseScreen pauseScreen;

		private EndFightScreen endFightScreen;

		private AchievementMessage achievementMessage;

		public ViewerFight Viewer
		{
			get
			{
				return get_ViewerFight();
			}
		}

		public int TimeLeftSeconds
		{
			get
			{
				return get_TimeLeft();
			}
		}

		public int FramesRemaining
		{
			get
			{
				return get_TimeLeftFrames();
			}
		}

		public int FramesElapsed
		{
			get
			{
				return get_TimePassedFrames();
			}
		}

		public int RoundTotalFrames
		{
			get
			{
				return get_TimeTotalRoundFrames();
			}
		}

		public ViewerFight get_ViewerFight()
		{
			return viewerFight;
		}

		public int get_TimeLeft()
		{
			if (viewerFight != null)
			{
				return (ObscuredInt)(viewerFight.get_TimeSecond());
			}
			return 0;
		}

		public int get_TimeLeftFrames()
		{
			if (viewerFight != null)
			{
				return (ObscuredInt)(viewerFight.get_TimeCount());
			}
			return 0;
		}

		public int get_TimePassedFrames()
		{
			if (viewerFight != null)
			{
				return viewerFight.get_RoundTimeTotalFrames() - (ObscuredInt)(viewerFight.get_TimeCount());
			}
			return 0;
		}

		public int get_TimeTotalRoundFrames()
		{
			if (viewerFight != null)
			{
				return viewerFight.get_RoundTimeTotalFrames();
			}
			return 0;
		}

		public ScreenFightType get_Type()
		{
			if (screenFight != null)
			{
				return screenFight.Type;
			}
			return ScreenFightType.TYPE_INFO_NONE;
		}

		public void Init(FightList list)
		{
			this.fightList = list;
			InitPreFight();
		}

		public void InitPreFight(ComboStatistic leftCombo = null, ComboStatistic rightCombo = null)
		{
			if (viewerFight == null)
			{
				viewerFight = Object.Instantiate(viewerFightPrefab).GetComponent<ViewerFight>();
				viewerFight.transform.SetParent(base.transform, false);
				viewerFight.AddEventListener(0, OnButtonClicked);
				viewerFight.PreInit(leftCombo, rightCombo);
			}
			if (screenFight == null)
			{
				screenFight = Object.Instantiate(screenFightPrefab).GetComponent<ScreenFight>();
				screenFight.transform.SetParent(base.transform, false);
				screenFight.gameObject.SetActive(false);
				screenFight.OnStopScreen.AddListener(OnScreenStop);
				screenFight.PreInit(fightList);
			}
			VisibleViewer(false);
		}

		public void OpenPauseScreen()
		{
			if (pauseScreenPrefab != null && pauseScreen == null)
			{
				pauseScreen = Object.Instantiate(pauseScreenPrefab).GetComponent<PauseScreen>();
				pauseScreen.gameObject.SetActive(true);
				pauseScreen.transform.SetParent(base.transform, false);
				pauseScreen.transform.SetAsLastSibling();
				pauseScreen.OnSurrender.AddListener(OnSurrenderClicked);
				pauseScreen.OnPlay.AddListener(OnPlayClicked);
				pauseScreen.Init();
				Eclipse.UI.FightPauseCinematic.Play(pauseScreen.gameObject);
			}
		}

		public void ClosePauseScreen()
		{
			if (pauseScreen != null)
			{
				// Eclipse: fades out over the resumed fight instead of cutting.
				Eclipse.UI.FightPauseCinematic.Close(pauseScreen.gameObject);
				pauseScreen = null;
			}
		}

		public void OpenEndFightScreen(FightResult result)
		{
			if (endFightScreenPrefab != null)
			{
				endFightScreen = Object.Instantiate(endFightScreenPrefab).GetComponent<EndFightScreen>();
				endFightScreen.gameObject.SetActive(true);
				Transform parent = ((!(base.transform.parent != null)) ? base.transform : base.transform.parent);
				endFightScreen.transform.SetParent(parent, false);
				endFightScreen.transform.SetAsLastSibling();
				endFightScreen.Init(result);
			}
		}

		public void Render()
		{
			if (viewerFight != null)
			{
				viewerFight.Render();
			}
		}

		public void RenderComboModel()
		{
			if (viewerFight != null)
			{
				viewerFight.RenderComboModel();
			}
		}

		public void ViewerInit(Round round, ModelParameters leftParameters, ModelParameters rightParameters, bool showRounds = true)
		{
			if (viewerFight != null)
			{
				viewerFight.Init(round, leftParameters, rightParameters, showRounds);
			}
		}

		public void ViewerPlay()
		{
			if (viewerFight != null)
			{
				viewerFight.Play();
			}
		}

		public void ViewerStrike(InfoAnimation animation, float damage, int attackerIndex, bool isFirstStrike, bool isHeadStrike, bool isCritical, bool isBlocked, bool isShock)
		{
			if (viewerFight != null)
			{
				viewerFight.Strike(animation, damage, attackerIndex, isFirstStrike, isHeadStrike, isCritical, isBlocked, isShock);
			}
		}

		public void ViewerUpdateVictorys()
		{
			if (viewerFight != null)
			{
				viewerFight.UpdateVictorys();
			}
		}

		public void ViewerUpdateHotGroundTimer(int time, RuleAppliance appliance)
		{
			if (viewerFight != null)
			{
				viewerFight.UpdateHotGroundTimer(time, appliance);
			}
		}

		public void VisibleViewer(bool value)
		{
			if (viewerFight != null)
			{
				viewerFight.SetVisible(value);
			}
		}

		public void ViewerPauseVisible(bool value)
		{
			if (viewerFight != null)
			{
				viewerFight.PauseVisible(value);
			}
		}

		public void Reset()
		{
			if (screenFight != null)
			{
				screenFight.OnStopScreen.RemoveListener(OnScreenStop);
			}
			foreach (Transform item in base.transform)
			{
				item.gameObject.SetActive(false);
				Object.Destroy(item.gameObject);
			}
			viewerFight = null;
			screenFight = null;
			pauseScreen = null;
		}

		public void CreateVS(ModelParameters playerParameters, List<ModelParameters> enemies, int enemyIndex, bool enemiesFlag, bool showEnemies, bool startVsImmediately)
		{
			if (screenFight != null)
			{
				screenFight.CreateVS(playerParameters, enemies, enemyIndex, enemiesFlag, showEnemies, startVsImmediately);
			}
		}

		public void CreateRound(int value, bool useMaxRounds)
		{
			if (screenFight != null)
			{
				screenFight.CreateRound(value, useMaxRounds);
			}
			if (viewerFight != null)
			{
				viewerFight.Reset();
			}
		}

		public void CreateSkipRound()
		{
			if (screenFight != null)
			{
				screenFight.CreateSkipRound();
			}
			if (viewerFight != null)
			{
				viewerFight.Reset();
			}
		}

		public void CreateFight()
		{
			if (screenFight != null)
			{
				screenFight.CreateFight();
			}
		}

		public void CreateFightRule()
		{
			if (screenFight != null)
			{
				screenFight.CreateFightRule();
			}
		}

		public void CreateWinner(bool isPerfect)
		{
			if (screenFight != null)
			{
				screenFight.CreateWinner(isPerfect);
			}
			if (isPerfect && viewerFight != null)
			{
				viewerFight.GetScreenModel(0).AddPerfect();
			}
		}

		public void CreateTimesUp()
		{
			if (screenFight != null)
			{
				screenFight.CreateTimesUp();
			}
		}

		public void CreateRingOut()
		{
			if (screenFight != null)
			{
				screenFight.CreateRingOut();
			}
		}

		public void CreateYouLose()
		{
			if (screenFight != null)
			{
				screenFight.CreateYouLose();
			}
		}

		public void CreateYouWin()
		{
			if (screenFight != null)
			{
				screenFight.CreateYouWin();
			}
		}

		public void ShowAchievementMessage(Achievement achievement)
		{
			if (achievementMessagePrefab == null)
			{
				OnAchievementMessageHide.Invoke();
			}
			else if (achievementMessage == null)
			{
				achievementMessage = Object.Instantiate(achievementMessagePrefab).GetComponent<AchievementMessage>();
				achievementMessage.gameObject.SetActive(true);
				achievementMessage.transform.SetParent(base.transform, false);
				achievementMessage.OnHide.AddListener(OnAchievementMessageAnimationEnd);
				achievementMessage.Init(achievement);
				achievementMessage.StartAnimation();
			}
		}

		public void OnAchievementMessageAnimationEnd()
		{
			if (achievementMessage != null)
			{
				achievementMessage.gameObject.SetActive(false);
				Object.Destroy(achievementMessage.gameObject);
				achievementMessage = null;
			}
			OnAchievementMessageHide.Invoke();
		}

		public void OnScreenStop(ScreenFightType screenType)
		{
			if (screenFight != null)
			{
				screenFight.Clear();
			}
			OnStopScreen.Invoke(screenType);
		}

		public void OnFightPause(bool value)
		{
			if (viewerFight != null)
			{
				viewerFight.OnFightPause(value);
			}
		}

		public void SetHealthBarVisible(RuleAppliance appliance, bool value)
		{
			if (viewerFight != null)
			{
				viewerFight.SetHealthBarVisible(appliance, value);
			}
		}

		public bool IsTimeOut()
		{
			if (viewerFight != null)
			{
				return (ObscuredInt)(viewerFight.get_TimeSecond()) <= 0;
			}
			return false;
		}

		public ComboStatistic GetStatistic(int value)
		{
			if (viewerFight != null)
			{
				return viewerFight.GetStatistic(value);
			}
			return null;
		}

		public void AdvanceScreenSimulationStep(float step)
		{
			if (screenFight != null)
			{
				screenFight.AdvanceSimulationStep(step);
			}
		}

		public void SetPause(bool value)
		{
			if (screenFight != null)
			{
				screenFight.set_Pause(value);
			}
		}

		public void ClearInscription()
		{
			if (screenFight != null)
			{
				screenFight.Clear();
			}
		}

		public void CreatePointsTable(float x, float y, int width, PointsTableType tableType, int maxPoints)
		{
			if (viewerFight != null)
			{
				viewerFight.CreatePointsTable(x, y, width, tableType, maxPoints);
			}
		}

		public void UpdatePointsTable(int leftScore, int rightScore)
		{
			if (viewerFight != null)
			{
				viewerFight.UpdatePointsTable(leftScore, rightScore);
			}
		}

		public void RemovePointsTable()
		{
			if (viewerFight != null)
			{
				viewerFight.RemovePointsTable();
			}
		}

		private void OnSurrenderClicked()
		{
			OnButtonClick.Invoke(ViewerFight.ViewerButton.ButtonPauseSurrender);
		}

		private void OnPlayClicked()
		{
			OnButtonClick.Invoke(ViewerFight.ViewerButton.ButtonPausePlay);
		}

		private void OnButtonClicked(object data)
		{
			ViewerFight.ViewerButton arg = (ViewerFight.ViewerButton)data;
			OnButtonClick.Invoke(arg);
		}
	}
}
