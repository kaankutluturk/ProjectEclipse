using System.Diagnostics;
using CodeStage.AntiCheat.ObscuredTypes;
using Nekki.SF2.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Fight
{
	public class ViewerFight : SFMonoBehaviour<object>
	{
		public enum ViewerButton
		{
			ButtonPause = 0,
			ButtonPauseSurrender = 1,
			ButtonPausePlay = 2,
			ButtonCheatWinFight = 3,
			ButtonCheatWinRound = 4,
			ButtonCheatLoseFight = 5,
			ButtonCheatLoseRound = 6,
			ButtonCheatStartBenchmark = 7
		}

		public enum ViewerEvent
		{
			OnButtonClicked = 0
		}

		[SerializeField]
		private GameObject _pointsTablePrefab;

		[SerializeField]
		private Button btnPause;

		[SerializeField]
		private LabelAlias roundTimer;

		[SerializeField]
		private ScreenModel leftModel;

		[SerializeField]
		private ScreenModel rightModel;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ComboStatistic leftStatistic;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ComboStatistic rightStatistic;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Round round;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool isPlaying;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool isPauseAllowed;

		private PointsTable pointsTable;

		private ObscuredInt timeCountFrames = (ObscuredInt)(0);

		private ObscuredInt timeSeconds = (ObscuredInt)(0);

		/// <summary>Eclipse training: sets the round clock back to <paramref name="seconds"/>.</summary>
		internal void RefillTime(int seconds)
		{
			timeCountFrames = (ObscuredInt)(seconds * 60);
			timeSeconds = (ObscuredInt)seconds;
		}

		private Vector2 leftModelPosition = new Vector2(-700f, 580f);

		private Vector2 rightModelPosition = new Vector2(700f, 580f);

		public ScreenModel LeftScreenModel
		{
			get
			{
				return get_LeftModel();
			}
		}

		public ScreenModel RightScreenModel
		{
			get
			{
				return get_RightModel();
			}
		}

		private ComboStatistic LeftStatistics
		{
			get
			{
				return GetLeftStatistic();
			}
			set
			{
				SetLeftStatistic(value);
			}
		}

		private ComboStatistic RightStatistics
		{
			get
			{
				return GetRightStatistic();
			}
			set
			{
				SetRightStatistic(value);
			}
		}

		private bool IsPlaying
		{
			get
			{
				return GetIsPlaying();
			}
			set
			{
				SetIsPlaying(value);
			}
		}

		private bool IsPauseAllowed
		{
			get
			{
				return GetIsPauseAllowed();
			}
			set
			{
				SetIsPauseAllowed(value);
			}
		}

		public int RoundTimeSeconds
		{
			get
			{
				return get_RoundTimeTotal();
			}
		}

		public int RoundTimeFrames
		{
			get
			{
				return get_RoundTimeTotalFrames();
			}
		}

		public ObscuredInt RemainingFrames
		{
			get
			{
				return get_TimeCount();
			}
		}

		public ObscuredInt RemainingSeconds
		{
			get
			{
				return get_TimeSecond();
			}
		}

		public ScreenModel get_LeftModel()
		{
			return leftModel;
		}

		public ScreenModel get_RightModel()
		{
			return rightModel;
		}

		private ComboStatistic GetLeftStatistic()
		{
			return leftStatistic;
		}

		private void SetLeftStatistic(ComboStatistic value)
		{
			leftStatistic = value;
		}

		private ComboStatistic GetRightStatistic()
		{
			return rightStatistic;
		}

		private void SetRightStatistic(ComboStatistic value)
		{
			rightStatistic = value;
		}

		private Round GetRound()
		{
			return round;
		}

		private void set_Round(Round value)
		{
			round = value;
		}

		private bool GetIsPlaying()
		{
			return isPlaying;
		}

		private void SetIsPlaying(bool value)
		{
			isPlaying = value;
		}

		private bool GetIsPauseAllowed()
		{
			return isPauseAllowed;
		}

		private void SetIsPauseAllowed(bool value)
		{
			isPauseAllowed = value;
		}

		public int get_RoundTimeTotal()
		{
			return (GetRound() != null) ? GetRound().timeTotal : 0;
		}

		public int get_RoundTimeTotalFrames()
		{
			return (GetRound() != null) ? (GetRound().timeTotal * 60) : 0;
		}

		public ObscuredInt get_TimeCount()
		{
			return timeCountFrames;
		}

		public ObscuredInt get_TimeSecond()
		{
			return timeSeconds;
		}

		public void RandomizeObscuredVars()
		{
			timeCountFrames.RandomizeCryptoKey();
			timeSeconds.RandomizeCryptoKey();
		}

		public void PreInit(ComboStatistic leftCombo, ComboStatistic rightCombo)
		{
            if (btnPause != null) Eclipse.UI.BattleTouchControls.ApplyPlatformVisibility(btnPause.gameObject);
			SetLeftStatistic(leftCombo);
			SetRightStatistic(rightCombo);
			set_Round(null);
			SetIsPlaying(false);
			SetIsPauseAllowed(true);
			ApplicationController.add_OnPause(OnApplicationPaused);
		}

		private void OnDestroy()
		{
			ApplicationController.remove_OnPause(OnApplicationPaused);
		}

		private void OnApplicationPaused(bool paused)
		{
			if (paused)
			{
				PausePress();
			}
		}

		public void Init(Round round, ModelParameters leftParameters, ModelParameters rightParameters, bool showRounds = true)
		{
			if (roundTimer != null)
			{
				// Preserve long raid rounds without clipping their third digit.
				roundTimer.horizontalOverflow = HorizontalWrapMode.Overflow;
				roundTimer.verticalOverflow = VerticalWrapMode.Overflow;
			}
			set_Round(round);
			DisablePauseButton();
			InitModel(leftModel, leftParameters, showRounds, leftModelPosition, "LeftModel");
			InitModel(rightModel, rightParameters, showRounds, rightModelPosition, "RightModel");
			if (GetLeftStatistic() != null && leftModel != null)
			{
				leftModel.Statistic = GetLeftStatistic();
			}
			if (GetRightStatistic() != null && rightModel != null)
			{
				rightModel.Statistic = GetRightStatistic();
			}
			if (SystemProperties.IsDebug())
			{
				CreateBenchmarkButton();
			}
			Reset();
		}

		private void DisablePauseButton()
		{
			if (btnPause != null)
			{
				btnPause.interactable = false;
			}
		}

		private void CreateBenchmarkButton()
		{
			GameObject gameObject = new GameObject("BenchmarkButton");
			RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
			rectTransform.transform.SetParent(base.transform, false);
			rectTransform.sizeDelta = new Vector2(140f, 140f);
			if (roundTimer != null)
			{
				Vector3 localPosition = roundTimer.transform.localPosition;
				localPosition.y += 25f;
				rectTransform.localPosition = localPosition;
			}
			Image image = gameObject.AddComponent<Image>();
			image.color = new Color(1f, 1f, 1f, 0f);
			Button button = gameObject.AddComponent<Button>();
			button.transition = Selectable.Transition.None;
			button.onClick.AddListener(OnBenchmarkClicked);
		}

		private void InitModel(ScreenModel screenModel, ModelParameters parameters, bool showRounds, Vector2 modelScreenPosition, string name)
		{
			parameters.RoundTotal = GetRound().roundTotal;
			screenModel.Init(parameters, showRounds);
			screenModel.AddEventListener(2, OnClickCheat);
		}

		private void OnClickCheat(object data)
		{
			ViewerButton buttonType = (((ScreenModel.ScreenSide)data != ScreenModel.ScreenSide.TYPE_LEFT) ? ViewerButton.ButtonCheatLoseFight : ViewerButton.ButtonCheatWinFight);
			CallEvent(0, buttonType);
		}

		private void OnBenchmarkClicked()
		{
			CallEvent(0, ViewerButton.ButtonCheatStartBenchmark);
		}

		public void Render()
		{
			if (GetIsPlaying())
			{
				if (GetRound().processing)
				{
					TickTimer();
				}
				if (leftModel != null)
				{
					leftModel.Render(GetRound().processing);
				}
				if (rightModel != null)
				{
					rightModel.Render(GetRound().processing);
				}
			}
			if (leftModel != null)
			{
				leftModel.RenderActivePerks();
			}
			if (rightModel != null)
			{
				rightModel.RenderActivePerks();
			}
		}

		private void TickTimer()
		{
			timeCountFrames = (ObscuredInt)((ObscuredInt)(timeCountFrames) - 1);
			timeSeconds = (ObscuredInt)((ObscuredInt)(timeCountFrames) / 60);
			if (roundTimer != null)
			{
				if ((ObscuredInt)(timeSeconds) < 10)
				{
					roundTimer.set_text(string.Format("0{0}", Mathf.Max(0, (ObscuredInt)(timeSeconds)).ToString()));
				}
				else
				{
					roundTimer.set_text(Mathf.Max(0, (ObscuredInt)(timeSeconds)).ToString());
				}
			}
		}

		public void Reset()
		{
			SetIsPlaying(false);
			timeCountFrames = (ObscuredInt)(GetRound().timeTotal * 60 + 1);
			if (leftModel != null)
			{
				leftModel.Reset();
			}
			if (rightModel != null)
			{
				rightModel.Reset();
			}
			TickTimer();
			if (btnPause != null)
			{
				btnPause.interactable = false;
			}
		}

		public void RenderComboModel()
		{
			if (GetIsPlaying())
			{
				if (leftModel != null)
				{
					leftModel.RenderComboModel();
				}
				if (rightModel != null)
				{
					rightModel.RenderComboModel();
				}
			}
		}

		public void Play()
		{
			GetRound().processing = true;
			SetIsPlaying(true);
			if (btnPause != null)
			{
				btnPause.interactable = true;
			}
		}

		public void Strike(InfoAnimation animation, float damage, int attackerIndex, bool isFirstStrike, bool isHeadStrike, bool isCritical, bool isBlocked, bool isShock)
		{
			if (leftModel == null || rightModel == null)
			{
				return;
			}
			ScreenModel screenModel = ((attackerIndex != 0) ? leftModel : rightModel);
			ScreenModel screenModel2 = ((attackerIndex != 0) ? rightModel : leftModel);
			if (isShock)
			{
				screenModel.AddShockCombo();
			}
			if (isCritical)
			{
				screenModel.AddCriticalCombo();
			}
			if (!isBlocked)
			{
				screenModel.UpdateStyle(animation);
				if (isFirstStrike)
				{
					screenModel.AddFirstStrikeCombo();
				}
				if (isHeadStrike)
				{
					screenModel.AddHeadStrikeCombo();
				}
				screenModel2.IsNoBlock = false;
			}
			else
			{
				screenModel.NotifyStyleChanged();
			}
		}

		public void UpdateVictorys()
		{
			if (leftModel != null)
			{
				leftModel.UpdateVictories();
			}
			if (rightModel != null)
			{
				rightModel.UpdateVictories();
			}
		}

		public void SetVisible(bool value)
		{
			base.gameObject.SetActive(value);
			if (btnPause != null)
			{
				btnPause.gameObject.SetActive(GetIsPauseAllowed() && value);
			}
		}

		public void PauseVisible(bool value)
		{
			SetIsPauseAllowed(value);
			if (btnPause != null)
			{
				btnPause.gameObject.SetActive(value);
			}
		}

		public void PausePress()
		{
			CallEvent(0, ViewerButton.ButtonPause);
		}

		public void UpdateHotGroundTimer(int time, RuleAppliance appliance)
		{
			switch (appliance)
			{
			case RuleAppliance.ApplianceAll:
				if (leftModel != null)
				{
					leftModel.ShowHotGroundTimer(time);
				}
				if (rightModel != null)
				{
					rightModel.ShowHotGroundTimer(time);
				}
				break;
			case RuleAppliance.AppliancePlayer:
				if (leftModel != null)
				{
					leftModel.ShowHotGroundTimer(time);
				}
				break;
			case RuleAppliance.ApplianceOpponent:
				if (rightModel != null)
				{
					rightModel.ShowHotGroundTimer(time);
				}
				break;
			}
		}

		public void OnFightPause(bool value)
		{
			if (leftModel != null)
			{
				leftModel.SetFightPaused(value);
			}
			if (rightModel != null)
			{
				rightModel.SetFightPaused(value);
			}
		}

		public void SetHealthBarVisible(RuleAppliance appliance, bool value)
		{
			switch (appliance)
			{
			case RuleAppliance.AppliancePlayer:
				if (leftModel != null)
				{
					leftModel.SetLifeBarVisible(value);
				}
				break;
			case RuleAppliance.ApplianceOpponent:
				if (rightModel != null)
				{
					rightModel.SetLifeBarVisible(value);
				}
				break;
			case RuleAppliance.ApplianceAll:
				if (leftModel != null)
				{
					leftModel.SetLifeBarVisible(value);
				}
				if (rightModel != null)
				{
					rightModel.SetLifeBarVisible(value);
				}
				break;
			}
		}

		public ScreenModel GetScreenModel(int index)
		{
			return (index != 0) ? rightModel : leftModel;
		}

		public ComboStatistic GetStatistic(int index)
		{
			ScreenModel screenModel = GetScreenModel(index);
			return (screenModel == null) ? null : screenModel.Statistic;
		}

		public void CreatePointsTable(float x, float y, int width, PointsTableType tableType, int maxPoints)
		{
			if (pointsTable == null && _pointsTablePrefab != null)
			{
				pointsTable = Object.Instantiate(_pointsTablePrefab).GetComponent<PointsTable>();
				pointsTable.transform.SetParent(base.transform, false);
				pointsTable.Init(tableType, maxPoints, width);
			}
		}

		public void UpdatePointsTable(int leftScore, int rightScore)
		{
			if (pointsTable != null)
			{
				pointsTable.set_LeftScore(leftScore);
				if (pointsTable.get_Type() == PointsTableType.POINTS_TABLE_CONTEST)
				{
					pointsTable.set_RightScore(rightScore);
				}
			}
		}

		public void RemovePointsTable()
		{
			if (pointsTable != null)
			{
				pointsTable.gameObject.SetActive(false);
				Object.Destroy(pointsTable);
				pointsTable = null;
			}
		}

		public void SetLockLifeUpdate(bool isLeft, bool value)
		{
			ScreenModel screenModel = ((!isLeft) ? rightModel : leftModel);
			if (screenModel != null)
			{
				screenModel.SetLifeUpdateLocked(value);
			}
		}

		public void UpdateCombo(bool isLeft, int value, int countOffset)
		{
			if (isLeft)
			{
				if (leftModel != null)
				{
					leftModel.UpdateCombo(value, countOffset);
				}
			}
			else if (rightModel != null)
			{
				rightModel.UpdateCombo(value, countOffset);
			}
		}

		public void RemoveCombo()
		{
			if (leftModel != null)
			{
				leftModel.RemoveAllCombos();
			}
			if (rightModel != null)
			{
				rightModel.RemoveAllCombos();
			}
		}
	}
}
