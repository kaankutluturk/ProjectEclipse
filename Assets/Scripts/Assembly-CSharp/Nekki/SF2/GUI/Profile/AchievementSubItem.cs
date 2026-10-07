using System;
using UnityEngine;

namespace Nekki.SF2.GUI.Profile
{
	public class AchievementSubItem : SubItem
	{
		public enum AchievementSubItemEvent
		{
			onRewardTake = 12
		}

		private const int PROGRESS_PADDING_X = 43;

		private const int PROGRESS_Y = 40;

		private const int PROGRESS_LABEL_FONT_SIZE = 102;

		private const int PROGRESS_LABEL_X = 350;

		[SerializeField]
		protected ProgressBar _progress;

		[SerializeField]
		protected LabelAlias _progressLabel;

		protected float targetValue;

		protected float currentValue;

		protected Achievement achievement;

		protected AchievementInfo achievementInfo;

		protected Action<object> _dlg;

		public void Init(string KHPKDMGDMAB, string title, string description, float AKIOCHEKNPE, float NPILBMKDDGN, int buttonId, Achievement achievementData = null)
		{
			Init(buttonId);
			targetValue = AKIOCHEKNPE;
			currentValue = NPILBMKDDGN;
			achievement = achievementData;
			_texturePath = "UI/Achievements/";
			spriteName = KHPKDMGDMAB;
			iconMaxOpacity = ProfileGUI.PerkOpacity.Max;
			iconMinOpacity = ProfileGUI.PerkOpacity.Min;
			int moneyPrize = ((achievementData != null) ? achievementData.MoneyPrize : 0);
			int bonusPrize = ((achievementData != null) ? achievementData.BonusPrize : 0);
			bool canTakeReward = achievementData != null && !achievementData.RewardClaimed;
			bool isCompleted = currentValue >= targetValue;
			_dlg = OnTakeReward;
			achievementInfo = new AchievementInfo(title, description, moneyPrize, bonusPrize, _dlg, canTakeReward, isCompleted);
			Data = achievementInfo;
			UpdateIcon();
			SetActive(true);
			UpdateProgressLabel();
			UpdateProgress(currentValue);
			UpdatePositions();
		}

		public void UpdateProgress(float value)
		{
			currentValue = value;
			_progress.SetValue(value);
		}

		public void ResetOpacity()
		{
			if ((bool)_icon)
			{
				UIExtensions.SetAlpha(_icon, iconMaxOpacity);
			}
		}

		public virtual void UpdatePositions()
		{
			float num = 0f;
			num += _icon.transform.localPosition.x + _icon.rectTransform.rect.width / 2f;
			num += 43f;
			num += _progress.GetComponent<RectTransform>().rect.width / 2f;
			_progress.transform.SetLocalX(num);
		}

		public override void Choose()
		{
			if (achievement != null)
			{
				((AchievementInfo)Data).CanTakeReward = !achievement.RewardClaimed;
			}
			base.Choose();
		}

		public Achievement GetAchievement()
		{
			return achievement;
		}

		protected void OnTakeReward(object data)
		{
			CallEvent(12, this);
		}

		protected override void UpdateSelectedFlash()
		{
			base.UpdateSelectedFlash();
			if (achievement != null && achievement.GetIsNew())
			{
				UpdateIconFlash();
			}
		}

		protected void UpdateProgressLabel()
		{
			_progress.SetValueBorders(0f, targetValue);
			_progress.transform.SetLocalY(40f);
			int num = (int)((!(currentValue <= targetValue)) ? targetValue : currentValue);
			string text = ((!((float)num < targetValue)) ? LocalizationManager.GetString("achievement_Completed") : (num + "/" + targetValue));
			_progressLabel.color = Constants.DialogTextColor;
			_progressLabel.set_LabelFontSize(102);
			_progressLabel.set_text(text);
			_progressLabel.transform.SetLocalX(350f);
			_progressLabel.transform.SetLocalY(-40f);
		}
	}
}
