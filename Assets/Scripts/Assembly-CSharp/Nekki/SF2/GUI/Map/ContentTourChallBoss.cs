using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Map
{
	public class ContentTourChallBoss : ContentBase
	{
		public const float MAX_OPACITY = 255f;

		[SerializeField]
		private LabelAlias _replaysLabel;

		[SerializeField]
		private ProgressBricksPanel _bricksPanel;

		[SerializeField]
		private DifficultyPanel _difficultyPanel;

		[SerializeField]
		private PrizePanel _prizePanel;

		[SerializeField]
		private LabelAlias _lblDescription;

		[SerializeField]
		private Button _challangeTouchZone;

		private float descriptionFade = 255f;

		private float difficultyFade = 255f;

		private bool isDescriptionFading;

		private bool isDescriptionFadingIn;

		private bool isDifficultyFading;

		private bool isDifficultyFadingIn;

		private bool hasDescription = true;

		private int fadePauseFrames;
		private bool showDescription;
		private float descriptionOpacity;
		private CanvasGroup difficultyOpacity;

		private void Update()
		{
			if (hasDescription)
			{
				descriptionOpacity = Mathf.MoveTowards(descriptionOpacity, showDescription ? 1f : 0f, Time.unscaledDeltaTime / .25f);
				ApplyDescriptionOpacity();
			}
		}

		public void Init(Battle battle, FightList fight)
		{
			_bricksPanel.Init(battle, fight);
			InitReplaysLabel(battle);
			bool hideItems = (battle.get_Type() != BattleType.FightBosses && battle.get_Type() != BattleType.FightBossesReplayable && battle.get_Type() != BattleType.FightFinalTitan) || !HasFinalRewardItem(fight);
			_prizePanel.Init(-1, hideItems, fight);
			_difficultyPanel.gameObject.SetActive(battle.GetFightCount() != 0);
			_difficultyPanel.GetComponent<CanvasGroup>().alpha = 1f;
			_difficultyPanel.Init(GameUtils.CalculateFightDifficulty(fight));
			InitDescription(battle, fight);
			BindTouchZoneClick();
		}

		private void InitDescription(Battle battle, FightList fight)
		{
			bool challenge = battle.get_Type() == BattleType.FightChallenge || battle.get_Type() == BattleType.FightReplayable;
			string description = fight != null ? fight.GetDescription() : string.Empty;
			if (string.IsNullOrEmpty(description)) description = battle.GetDescription();
			hasDescription = challenge && !string.IsNullOrEmpty(description) && fight != null && fight.IsReplayAvailable();
			_lblDescription.gameObject.SetActive(hasDescription);
			if (hasDescription) _lblDescription.SetAlias(description);
			_challangeTouchZone.interactable = hasDescription;
			difficultyOpacity = _difficultyPanel.GetComponent<CanvasGroup>();
			showDescription = hasDescription;
			descriptionOpacity = showDescription ? 1f : 0f;
			ApplyDescriptionOpacity();
		}

		private void ApplyDescriptionOpacity()
		{
			float eased = Mathf.SmoothStep(0f, 1f, descriptionOpacity);
			_lblDescription.SetAlpha(eased);
			if (difficultyOpacity != null) difficultyOpacity.alpha = 1f - eased;
		}

		private void InitReplaysLabel(Battle battle)
		{
			if (battle.get_Type() == BattleType.FightReplayable || battle.get_Type() == BattleType.FightBossesReplayable)
			{
				_replaysLabel.gameObject.SetActive(true);
				BattleReplayable replayable = (BattleReplayable)battle;
				_replaysLabel.set_text(Eclipse.UI.BattleInfoText.Replays(replayable));
			}
			else
			{
				_replaysLabel.gameObject.SetActive(false);
			}
		}

		private bool HasFinalRewardItem(FightList fight)
		{
			if (fight == null)
			{
				return false;
			}
			RewardStruct rewardStruct = fight.GetRewards()[fight.GetRewards().Count - 1];
			int level = ListSF.GetRoster().GetLevel();
			RewardPrize prize = rewardStruct.GetPrizeForLevel(level);
			if (prize.items.Count == 0)
			{
				return false;
			}
			RewardItem rewardItem = prize.items[0];
			if (!rewardItem.ShowReward)
			{
				return false;
			}
			ItemInfo itemInfo = ListSF.GetItems().GetItemByName(rewardItem.Name);
			return itemInfo != null;
		}

		private void ResetFade(bool descriptionVisible)
		{
			descriptionFade = (descriptionVisible ? 255 : 0);
			difficultyFade = ((!descriptionVisible) ? 255 : 0);
			fadePauseFrames = MapGUI.ChallengeFade.DelayBeforeFade;
			isDescriptionFading = descriptionVisible;
			isDescriptionFadingIn = !descriptionVisible;
			isDifficultyFading = !descriptionVisible;
			isDifficultyFadingIn = descriptionVisible;
		}

		private bool IsFadeSpeedZero()
		{
			return MapGUI.ChallengeFade.DifficultyIsFirstFrame == 0;
		}

		private void UpdateFade()
		{
			if (fadePauseFrames > 0)
			{
				fadePauseFrames--;
				return;
			}
			if (isDescriptionFading)
			{
				StepDescriptionFade();
				if (descriptionFade <= (float)MapGUI.ChallengeFade.MinOpacity && !isDifficultyFading)
				{
					isDifficultyFading = true;
				}
				if (descriptionFade == 0f)
				{
					isDescriptionFading = false;
				}
			}
			if (isDifficultyFading)
			{
				StepDifficultyFade();
				if (difficultyFade <= (float)MapGUI.ChallengeFade.MinOpacity && !isDescriptionFading)
				{
					isDescriptionFading = true;
				}
				if (difficultyFade == 0f)
				{
					isDifficultyFading = false;
				}
			}
		}

		private void StepDescriptionFade()
		{
			int num = ((MapGUI.ChallengeFade.FadeSpeed <= 0) ? 1 : MapGUI.ChallengeFade.FadeSpeed);
			if (isDescriptionFadingIn)
			{
				descriptionFade += 255f / (float)num;
				if (descriptionFade >= 255f)
				{
					descriptionFade = 255f;
					isDescriptionFadingIn = false;
					fadePauseFrames = MapGUI.ChallengeFade.DelayBeforeFade;
				}
			}
			else
			{
				descriptionFade -= 255f / (float)num;
				if (descriptionFade <= 0f)
				{
					descriptionFade = 0f;
					isDescriptionFadingIn = true;
				}
			}
			_lblDescription.SetAlpha(descriptionFade / 255f);
		}

		private void StepDifficultyFade()
		{
			int num = ((MapGUI.ChallengeFade.FadeSpeed <= 0) ? 1 : MapGUI.ChallengeFade.FadeSpeed);
			if (isDifficultyFadingIn)
			{
				difficultyFade += 255f / (float)num;
				if (difficultyFade >= 255f)
				{
					difficultyFade = 255f;
					isDifficultyFadingIn = false;
					fadePauseFrames = MapGUI.ChallengeFade.DelayBeforeFade;
				}
			}
			else
			{
				difficultyFade -= 255f / (float)num;
				if (difficultyFade <= 0f)
				{
					difficultyFade = 0f;
					isDifficultyFadingIn = true;
				}
			}
			_difficultyPanel.GetComponent<CanvasGroup>().alpha = difficultyFade / 255f;
		}

		private void BindTouchZoneClick()
		{
			_challangeTouchZone.onClick.RemoveListener(OnTouchZoneClicked);
			_challangeTouchZone.onClick.AddListener(OnTouchZoneClicked);
		}

		private void OnTouchZoneClicked()
		{
			if (hasDescription)
			{
				showDescription = !showDescription;
			}
		}
	}
}
