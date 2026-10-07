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

		public void Init(Battle DPOOIONCEOA, FightList KOMGFJOCEDN)
		{
			_bricksPanel.Init(DPOOIONCEOA, KOMGFJOCEDN);
			InitReplaysLabel(DPOOIONCEOA);
			bool mMDLKOPCFLK = (DPOOIONCEOA.get_Type() != BattleType.FightBosses && DPOOIONCEOA.get_Type() != BattleType.FightBossesReplayable && DPOOIONCEOA.get_Type() != BattleType.FightFinalTitan) || !HasFinalRewardItem(KOMGFJOCEDN);
			_prizePanel.Init(-1, mMDLKOPCFLK, KOMGFJOCEDN);
			_difficultyPanel.gameObject.SetActive(DPOOIONCEOA.GetFightCount() != 0);
			_difficultyPanel.GetComponent<CanvasGroup>().alpha = 1f;
			_difficultyPanel.Init(GameUtils.CalculateFightDifficulty(KOMGFJOCEDN));
			InitDescription(DPOOIONCEOA, KOMGFJOCEDN);
			BindTouchZoneClick();
		}

		private void InitDescription(Battle DPOOIONCEOA, FightList KOMGFJOCEDN)
		{
			bool challenge = DPOOIONCEOA.get_Type() == BattleType.FightChallenge || DPOOIONCEOA.get_Type() == BattleType.FightReplayable;
			string description = KOMGFJOCEDN != null ? KOMGFJOCEDN.GetDescription() : string.Empty;
			if (string.IsNullOrEmpty(description)) description = DPOOIONCEOA.GetDescription();
			hasDescription = challenge && !string.IsNullOrEmpty(description) && KOMGFJOCEDN != null && KOMGFJOCEDN.IsReplayAvailable();
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

		private void InitReplaysLabel(Battle DPOOIONCEOA)
		{
			if (DPOOIONCEOA.get_Type() == BattleType.FightReplayable || DPOOIONCEOA.get_Type() == BattleType.FightBossesReplayable)
			{
				_replaysLabel.gameObject.SetActive(true);
				BattleReplayable bKKPCBGAEHC = (BattleReplayable)DPOOIONCEOA;
				_replaysLabel.set_text(Eclipse.UI.BattleInfoText.Replays(bKKPCBGAEHC));
			}
			else
			{
				_replaysLabel.gameObject.SetActive(false);
			}
		}

		private bool HasFinalRewardItem(FightList KOMGFJOCEDN)
		{
			if (KOMGFJOCEDN == null)
			{
				return false;
			}
			RewardStruct fDFKLPHBAHJ = KOMGFJOCEDN.GetRewards()[KOMGFJOCEDN.GetRewards().Count - 1];
			int gNLOCMLBNHF = ListSF.GetRoster().GetLevel();
			RewardPrize cMHHEHILIIH = fDFKLPHBAHJ.GetPrizeForLevel(gNLOCMLBNHF);
			if (cMHHEHILIIH.items.Count == 0)
			{
				return false;
			}
			RewardItem cACJANFAJEC = cMHHEHILIIH.items[0];
			if (!cACJANFAJEC.ShowReward)
			{
				return false;
			}
			ItemInfo dJKEECEOCJB = ListSF.GetItems().GetItemByName(cACJANFAJEC.Name);
			return dJKEECEOCJB != null;
		}

		private void ResetFade(bool LFGNIIJJMIG)
		{
			descriptionFade = (LFGNIIJJMIG ? 255 : 0);
			difficultyFade = ((!LFGNIIJJMIG) ? 255 : 0);
			fadePauseFrames = MapGUI.ChallengeFade.DelayBeforeFade;
			isDescriptionFading = LFGNIIJJMIG;
			isDescriptionFadingIn = !LFGNIIJJMIG;
			isDifficultyFading = !LFGNIIJJMIG;
			isDifficultyFadingIn = LFGNIIJJMIG;
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
