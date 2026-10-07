using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nekki.SF2.GUI.Fight
{
	public class ScreenFight : MonoBehaviour
	{
		public struct FightSetup
		{
			public ModelParameters playerModel;

			public List<ModelParameters> enemyModels;

			public int enemyIndex;
		}

		public class ScreenFightTypeEvent : UnityEvent<ScreenFightType>
		{
		}

		public ScreenFightTypeEvent OnStartScreen = new ScreenFightTypeEvent();

		public ScreenFightTypeEvent OnStopScreen = new ScreenFightTypeEvent();

		private const float RoundBannerDuration = 1.666f;

		private const float SkipRoundDuration = 0.016f;

		private const float FightBannerDuration = 1.166f;

		private const float RuleBannerDuration = 5.833f;

		private const float WinnerBannerDuration = 1.166f;

		private const float TimesUpBannerDuration = 1.166f;

		private const float RingOutBannerDuration = 1.166f;

		private const float YouLoseBannerDuration = 1.166f;

		private const float YouWinBannerDuration = 1.166f;

		private const string PerfectSprite = "FightUI.perfect";

		private const string GreatSprite = "FightUI.great";

		private const string FightSprite = "FightUI.fight";

		private const string RoundSprite = "FightUI.round";

		private const string TimesUpSprite = "FightUI.timesup";

		private const string RingOutSprite = "FightUI.ringout";

		private const string YouLoseSprite = "FightUI.youlose";

		private const string YouWinSprite = "FightUI.youwin";

		private FightSetup fightSetup = default(FightSetup);

		public ScreenFightType Type;

		private int maxRounds;

		private string ruleDesc = string.Empty;

		private bool isPaused;

		private bool skipEnemiesScreen;

		private bool enemiesFlag;

		private bool showEnemiesScreen;

		private bool isScreenActive;

		private float timer;
		private float bannerDuration;
		private Eclipse.UI.FightPopupCinematic bannerAnimation;
		private Eclipse.UI.FightPopupCinematic roundAnimation;

		[SerializeField]
		private GameObject vsScreenPrefab;

		[SerializeField]
		private GameObject enemiesScreenPrefab;

		[SerializeField]
		private ResolutionImage image;

		[SerializeField]
		private LabelAlias round;

		[SerializeField]
		private LabelAlias ruleDescription;

		private VsScreen vsScreen;

		private EnemiesScreen enemiesScreen;
		private bool customVersusIntro;

		public bool get_Pause()
		{
			return isPaused;
		}

		public void set_Pause(bool value)
		{
			isPaused = value;
		}

		public void PreInit(FightList list)
		{
			customVersusIntro = list is Eclipse.Multiplayer.LocalVersusMatch match &&
				(match.Settings.Mode == Eclipse.Multiplayer.VersusMode.Local || match.Settings.Mode == Eclipse.Multiplayer.VersusMode.Online);
			maxRounds = list.RoundsToWin * list.GetTotalOpponentRounds();
			ruleDesc = list.GetDescription();
			set_Pause(false);
		}

		public void CreateVS(ModelParameters playerParameters, List<ModelParameters> enemies, int index, bool enemiesFlagValue, bool showEnemies, bool startVsImmediately)
		{
			fightSetup.playerModel = playerParameters;
			fightSetup.enemyModels = enemies;
			fightSetup.enemyIndex = index;
			this.enemiesFlag = enemiesFlagValue;
			this.showEnemiesScreen = showEnemies;
			if (enemies.Count > 1 && showEnemies)
			{
				CreateEnemiesScreen(enemies, index, this.enemiesFlag);
				return;
			}
			if (startVsImmediately)
			{
				StartVS();
				return;
			}
			Type = ScreenFightType.TYPE_INFO_VS;
			StartScreen(0f);
		}

		public void CreateRound(int value, bool useMaxRounds = false)
		{
			ClearPictures();
			Type = ScreenFightType.TYPE_INFO_ROUND;
			if (image != null)
			{
				image.set_SpriteName("FightUI.round");
				image.SetNativeSize();
				image.gameObject.SetActive(true);
			}
			if (round != null)
			{
				int num = ((!useMaxRounds) ? value : maxRounds);
				round.gameObject.SetActive(true);
				round.set_text(num.ToString());
			}
			StartScreen(1.666f);
		}

		public void CreateSkipRound()
		{
			ClearPictures();
			Type = ScreenFightType.TYPE_INFO_SKIP_ROUND;
			StartScreen(0.016f);
		}

		public void CreateFight()
		{
			if (!base.gameObject.activeSelf || Type != ScreenFightType.TYPE_INFO_FIGHT)
			{
				ClearPictures();
				Type = ScreenFightType.TYPE_INFO_FIGHT;
				if (image != null)
				{
					image.set_SpriteName("FightUI.fight");
					image.SetNativeSize();
					image.gameObject.SetActive(true);
				}
				StartScreen(1.166f);
			}
		}

		public void CreateFightRule()
		{
			if (!ruleDesc.Equals(string.Empty))
			{
				ClearPictures();
				Type = ScreenFightType.TYPE_INFO_FIGHT_RULE;
				if (ruleDescription != null)
				{
					ruleDescription.gameObject.SetActive(true);
					ruleDescription.set_Alias(ruleDesc);
				}
				StartScreen(5.833f);
			}
		}

		public void CreateWinner(bool isPerfect)
		{
			ClearPictures();
			Type = ((!isPerfect) ? ScreenFightType.TYPE_INFO_COOL : ScreenFightType.TYPE_INFO_PERFECT);
			string spriteName = ((!isPerfect) ? "FightUI.great" : "FightUI.perfect");
			if (image != null)
			{
				image.set_SpriteName(spriteName);
				image.SetNativeSize();
				image.gameObject.SetActive(true);
			}
			StartScreen(1.166f);
		}

		public void CreateTimesUp()
		{
			ClearPictures();
			Type = ScreenFightType.TYPE_INFO_TIMESUP;
			if (image != null)
			{
				image.set_SpriteName("FightUI.timesup");
				image.SetNativeSize();
				image.gameObject.SetActive(true);
			}
			StartScreen(1.166f);
		}

		public void CreateRingOut()
		{
			ClearPictures();
			Type = ScreenFightType.TYPE_INFO_RINGOUT;
			if (image != null)
			{
				image.set_SpriteName("FightUI.ringout");
				image.SetNativeSize();
				image.gameObject.SetActive(true);
			}
			StartScreen(1.166f);
		}

		public void CreateYouLose()
		{
			ClearPictures();
			Type = ScreenFightType.TYPE_INFO_LOSE;
			if (image != null)
			{
				image.set_SpriteName("FightUI.youlose");
				image.SetNativeSize();
				image.gameObject.SetActive(true);
			}
			StartScreen(1.166f);
		}

		public void CreateYouWin()
		{
			ClearPictures();
			Type = ScreenFightType.TYPE_INFO_WIN;
			if (image != null)
			{
				image.set_SpriteName("FightUI.youwin");
				image.SetNativeSize();
				image.gameObject.SetActive(true);
			}
			StartScreen(1.166f);
		}

		public void Clear()
		{
			ClearPictures();
		}

		public void ClearPictures()
		{
			bannerAnimation?.Reset();
			roundAnimation?.Reset();
			if (image != null)
			{
				image.gameObject.SetActive(false);
			}
			if (round != null)
			{
				round.gameObject.SetActive(false);
			}
			if (ruleDescription != null)
			{
				ruleDescription.gameObject.SetActive(false);
			}
			if (vsScreen != null)
			{
				vsScreen.gameObject.SetActive(false);
				Object.Destroy(vsScreen);
				vsScreen = null;
			}
			if (enemiesScreen != null)
			{
				enemiesScreen.gameObject.SetActive(false);
				Object.Destroy(enemiesScreen);
				enemiesScreen = null;
			}
		}

		public void CreateEnemiesScreen(List<ModelParameters> enemies, int index, bool enemiesFlagValue)
		{
			Type = ScreenFightType.TYPE_INFO_ENEMIES;
			if (enemiesScreenPrefab == null)
			{
				StartScreen(0f);
				return;
			}
			ClearPictures();
			base.gameObject.SetActive(true);
			enemiesScreen = Object.Instantiate(enemiesScreenPrefab).GetComponent<EnemiesScreen>();
			enemiesScreen.transform.SetParent(base.transform, false);
			enemiesScreen.Init(enemies, index, enemiesFlagValue);
			StartScreen(enemiesScreen.get_AnimationTime());
			OnStopScreen.AddListener(OnEnemiesScreenStopped);
		}

		private void OnEnemiesScreenStopped(ScreenFightType screenType)
		{
			if (screenType == ScreenFightType.TYPE_INFO_ENEMIES)
			{
				OnStopScreen.RemoveListener(OnEnemiesScreenStopped);
				StartVS();
			}
		}

		private void StartVS()
		{
			Type = ScreenFightType.TYPE_INFO_VS;
			if (vsScreenPrefab == null)
			{
				StartScreen(0f);
				return;
			}
			ClearPictures();
			vsScreen = Object.Instantiate(vsScreenPrefab).GetComponent<VsScreen>();
			vsScreen.transform.SetParent(base.transform, false);
			// Keep the established VS timer and stop event on the same simulation ticks
			// for peers and saved replays, but let our persistent introduction own the art.
			if (customVersusIntro) vsScreen.gameObject.SetActive(false);
			ModelParameters enemyParameters = null;
			if (fightSetup.enemyModels.Count > fightSetup.enemyIndex)
			{
				enemyParameters = fightSetup.enemyModels[fightSetup.enemyIndex];
			}
			else
			{
				if (fightSetup.enemyModels.Count <= 0)
				{
					StartScreen(0f);
					return;
				}
				enemyParameters = fightSetup.enemyModels[0];
			}
			vsScreen.Init(fightSetup.playerModel, enemyParameters);
			StartScreen(vsScreen.get_AnimationTime());
		}

		private void StartScreen(float duration)
		{
			timer = duration;
			bannerDuration = timer;
			if (image != null && image.gameObject.activeSelf)
			{
				if (bannerAnimation == null) bannerAnimation = new Eclipse.UI.FightPopupCinematic(image.rectTransform);
				bannerAnimation.Play(timer);
			}
			if (round != null && round.gameObject.activeSelf)
			{
				if (roundAnimation == null) roundAnimation = new Eclipse.UI.FightPopupCinematic(round.rectTransform);
				roundAnimation.Play(timer);
			}
			Start();
		}

		public void Start()
		{
			// Banners change scene state a rollback cannot undo.
			if (Eclipse.Multiplayer.VersusTickDriver.Barrier())
			{
				return;
			}
			base.gameObject.SetActive(true);
			isScreenActive = true;
			OnStartScreen.Invoke(Type);
		}

		public void Stop()
		{
			if (Eclipse.Multiplayer.VersusTickDriver.Barrier())
			{
				return;
			}
			base.gameObject.SetActive(false);
			isScreenActive = false;
			OnStopScreen.Invoke(Type);
		}

		private void Update()
		{
			// Versus stage changes must land on the same simulation tick for every
			// peer and replay, so the tick driver advances the timer instead.
			if (!isPaused && isScreenActive && !Eclipse.Multiplayer.VersusTickDriver.PacesFightScreens)
			{
				AdvanceTimer(Time.deltaTime);
			}
		}

		/// <summary>Advances the banner by one fixed simulation step (versus only).</summary>
		public void AdvanceSimulationStep(float step)
		{
			if (!isPaused && isScreenActive)
			{
				AdvanceTimer(step);
			}
		}

		private void AdvanceTimer(float step)
		{
			timer -= step;
			bannerAnimation?.Evaluate(bannerDuration - timer);
			roundAnimation?.Evaluate(bannerDuration - timer);
			if (timer <= 0f)
			{
				Stop();
			}
		}
	}
}
