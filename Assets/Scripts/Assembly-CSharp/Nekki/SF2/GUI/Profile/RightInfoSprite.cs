using UnityEngine;

namespace Nekki.SF2.GUI.Profile
{
	public class RightInfoSprite : SFMonoBehaviour<object>
	{
		private const int NO_CONTENT_FONT_SIZE = 83;

		private const int DEFAULT_HEADER_FONT_SIZE = 104;

		private const int HEADER_X = 8;

		private const float HEADER_TOP_OFFSET = 150f;

		[SerializeField]
		private LabelAlias _header;

		[SerializeField]
		private LabelAlias _noContentMessage;

		[SerializeField]
		private PerkContent _perkContent;

		[SerializeField]
		private TrickContent _trickContent;

		[SerializeField]
		private AchievementContent _achievContent;

		private int headerFontSize;

		public void Init()
		{
			InitLabels();
		}

		public void SetPerkInfo(PerkContentData perkContentData)
		{
			_perkContent.gameObject.SetActive(true);
			_trickContent.gameObject.SetActive(false);
			_achievContent.gameObject.SetActive(false);
			_perkContent.Init(perkContentData.description, perkContentData.state, perkContentData.Callback, perkContentData.Animation, perkContentData.LabelWidth);
			if (_perkContent.HeaderFontSize > 0)
			{
				headerFontSize = _perkContent.HeaderFontSize;
			}
			else
			{
				headerFontSize = 104;
			}
			_noContentMessage.gameObject.SetActive(false);
			SetLabel(perkContentData.name);
			float upBorder = _header.transform.localPosition.y - _header.rectTransform.rect.height / 2f;
			_perkContent.SetUpBorder(upBorder);
		}

		public void SetTrickInfo(TrickInfo trickInfo)
		{
			_perkContent.gameObject.SetActive(false);
			_trickContent.gameObject.SetActive(true);
			_achievContent.gameObject.SetActive(false);
			_trickContent.Init(trickInfo.Animation, trickInfo.AttackDamages, trickInfo.OnClickCallback, trickInfo.Description);
			if (_perkContent.HeaderFontSize > 0)
			{
				headerFontSize = _perkContent.HeaderFontSize;
			}
			else
			{
				headerFontSize = 104;
			}
			_noContentMessage.gameObject.SetActive(false);
			SetLabel(trickInfo.Title);
			float upBorder = _header.transform.localPosition.y - _header.rectTransform.rect.height / 2f;
			_trickContent.SetUpBorder(upBorder);
		}

		public void SetAchievementInfo(AchievementInfo achievementInfo)
		{
			_perkContent.gameObject.SetActive(false);
			_trickContent.gameObject.SetActive(false);
			_achievContent.gameObject.SetActive(true);
			_achievContent.Init(achievementInfo.Description, achievementInfo.MoneyPrize, achievementInfo.BonusPrize, achievementInfo.OnTakeReward, achievementInfo.CanTakeReward, achievementInfo.IsCompleted);
			if (_achievContent.HeaderFontSize > 0)
			{
				headerFontSize = _achievContent.HeaderFontSize;
			}
			else
			{
				headerFontSize = 104;
			}
			_noContentMessage.gameObject.SetActive(false);
			SetLabel(achievementInfo.Title);
			float upBorder = _header.transform.localPosition.y - _header.rectTransform.rect.height / 2f;
			_trickContent.SetUpBorder(upBorder);
		}

		public void SetItemInfo(ItemInfo itemInfo)
		{
			_perkContent.gameObject.SetActive(false);
			_trickContent.gameObject.SetActive(false);
			_achievContent.gameObject.SetActive(true);
			_achievContent.Init(itemInfo.DescriptionAlias);
			if (_achievContent.HeaderFontSize > 0)
			{
				headerFontSize = _achievContent.HeaderFontSize;
			}
			else
			{
				headerFontSize = 104;
			}
			_noContentMessage.gameObject.SetActive(false);
			SetLabel(itemInfo.Name);
			float upBorder = _header.transform.localPosition.y - _header.rectTransform.rect.height / 2f;
			_trickContent.SetUpBorder(upBorder);
		}

		public void SetLabel(string label)
		{
			_header.set_LabelFontSize(headerFontSize);
			_header.set_Alias(label);
		}

		public void SetNoContentMessage(string message)
		{
			_noContentMessage.set_Alias(message);
		}

		public void Clear()
		{
			_header.set_text(string.Empty);
			_noContentMessage.gameObject.SetActive(true);
			_perkContent.gameObject.SetActive(false);
			_trickContent.gameObject.SetActive(false);
			_achievContent.gameObject.SetActive(false);
		}

		public float GetLabelWidth()
		{
			return GetComponent<RectTransform>().rect.width - 120f;
		}

		public LabelButton GetBtnPerkImprove()
		{
			if (_perkContent.gameObject.activeSelf)
			{
				return _perkContent.GetBtnImprove();
			}
			return null;
		}

		public LabelButton GetBtnStrikeShow()
		{
			if (_trickContent.gameObject.activeSelf)
			{
				return _trickContent.GetBtnShow();
			}
			return null;
		}

		private void InitLabels()
		{
			_header.set_text(string.Empty);
			_header.set_LabelFontSize(headerFontSize);
			_header.transform.SetLocalX(8f);
			_header.transform.SetLocalY(GetComponent<RectTransform>().rect.height / 2f - 150f);
			_header.rectTransform.sizeDelta = new Vector2(GetLabelWidth(), _header.rectTransform.rect.height);
			_header.color = Constants.DialogTextColor;
			_noContentMessage.set_text(string.Empty);
			_noContentMessage.rectTransform.sizeDelta = new Vector2(GetLabelWidth(), _noContentMessage.rectTransform.rect.height);
			_noContentMessage.set_LabelFontSize(83);
			_noContentMessage.transform.SetLocalX(8f);
			_noContentMessage.transform.SetLocalY(0f);
			_noContentMessage.color = Constants.DialogTextColor;
		}
	}
}
