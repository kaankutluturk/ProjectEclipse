using System;
using UnityEngine;

namespace Nekki.SF2.GUI.Profile
{
	public class AchievementContent : Content
	{
		public enum AchievementContentLayer
		{
			zText = 0
		}

		private const int HEADER_FONT_SIZE = 88;

		private const int TEXT_FONT_SIZE = 70;

		private const int REWARD_LABEL_Y = -294;

		private const int REWARD_FONT_SIZE = 83;

		[SerializeField]
		protected LabelAlias _textLabel;

		[SerializeField]
		protected LabelAlias _rewardLabel;

		[SerializeField]
		protected SFButton _takeButton;

		protected Action<object> takeCallback;

		protected string _text;

		protected int rewardMoney;

		protected int rewardRubies;

		protected bool showReward;

		protected bool isCompleted;

		protected bool hasUpBorder;

		protected float upBorder;

		private void Start()
		{
			_takeButton.AddEventListener(2, OnTakeButtonClicked);
		}

		public void Init(string text, int moneyReward = 0, int rubiesReward = 0, Action<object> takeAction = null, bool rewardFlag = false, bool completedFlag = false)
		{
			_takeButton.gameObject.SetActive(false);
			_text = text;
			rewardMoney = moneyReward;
			rewardRubies = rubiesReward;
			takeCallback = takeAction;
			showReward = rewardFlag;
			isCompleted = completedFlag;
			HeaderFontSize = 88;
			InitTextLabel();
			InitRewardLabel();
			InitTakeButton();
			CenterTextLabel();
		}

		public override void SetUpBorder(float upBorderValue)
		{
			upBorder = upBorderValue;
			hasUpBorder = true;
			CenterTextLabel();
		}

		protected void InitTextLabel()
		{
			_textLabel.color = Constants.DialogTextColor;
			_textLabel.set_LabelFontSize(70);
			_textLabel.set_Alias(_text);
		}

		protected void InitRewardLabel()
		{
			_rewardLabel.gameObject.SetActive(false);
			if (showReward)
			{
				_rewardLabel.gameObject.SetActive(true);
				_rewardLabel.transform.SetLocalY(-294f);
				_rewardLabel.color = Constants.DialogTextColor;
				_rewardLabel.set_LabelFontSize(83);
				string text = ((rewardMoney <= 0) ? "MiscSprites.ruby" : ListSF.GetRoster().GetCoinIcon());
				int num = ((rewardMoney <= 0) ? rewardRubies : rewardMoney);
				string text2 = LocalizationManager.GetString("achievementReward") + "<quad name=" + text + " size=88 width=1 /> " + num;
				_rewardLabel.set_text(text2);
				bool flag = rewardMoney > 0 || rewardRubies > 0;
				_rewardLabel.gameObject.SetActive(flag && showReward);
			}
		}

		protected void InitTakeButton()
		{
			bool flag = rewardMoney > 0 || rewardRubies > 0;
			if (isCompleted && showReward && flag)
			{
				float y = _rewardLabel.transform.transform.localPosition.y;
				_takeButton.transform.SetLocalY(y - 20f);
				_rewardLabel.transform.SetLocalY(y + 80f);
				_takeButton.gameObject.SetActive(showReward);
			}
		}

		protected void OnTakeButtonClicked(object data)
		{
			if (takeCallback != null)
			{
				takeCallback(data);
			}
		}

		protected void CenterTextLabel()
		{
			if (hasUpBorder && _rewardLabel != null && _textLabel != null)
			{
				float num = _rewardLabel.transform.localPosition.y + _rewardLabel.rectTransform.rect.height / 2f;
				float labelY = upBorder - (upBorder - num) / 2f;
				_textLabel.transform.SetLocalY(labelY);
			}
		}
	}
}
