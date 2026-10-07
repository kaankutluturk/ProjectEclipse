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

		public void SetPerkInfo(PerkContentData BPANICNCIAO)
		{
			_perkContent.gameObject.SetActive(true);
			_trickContent.gameObject.SetActive(false);
			_achievContent.gameObject.SetActive(false);
			_perkContent.Init(BPANICNCIAO.description, BPANICNCIAO.state, BPANICNCIAO.Callback, BPANICNCIAO.Animation, BPANICNCIAO.LabelWidth);
			if (_perkContent.HeaderFontSize > 0)
			{
				headerFontSize = _perkContent.HeaderFontSize;
			}
			else
			{
				headerFontSize = 104;
			}
			_noContentMessage.gameObject.SetActive(false);
			SetLabel(BPANICNCIAO.name);
			float upBorder = _header.transform.localPosition.y - _header.rectTransform.rect.height / 2f;
			_perkContent.SetUpBorder(upBorder);
		}

		public void SetTrickInfo(TrickInfo ACNOAOIBCBM)
		{
			_perkContent.gameObject.SetActive(false);
			_trickContent.gameObject.SetActive(true);
			_achievContent.gameObject.SetActive(false);
			_trickContent.Init(ACNOAOIBCBM.Animation, ACNOAOIBCBM.AttackDamages, ACNOAOIBCBM.OnClickCallback, ACNOAOIBCBM.Description);
			if (_perkContent.HeaderFontSize > 0)
			{
				headerFontSize = _perkContent.HeaderFontSize;
			}
			else
			{
				headerFontSize = 104;
			}
			_noContentMessage.gameObject.SetActive(false);
			SetLabel(ACNOAOIBCBM.Title);
			float upBorder = _header.transform.localPosition.y - _header.rectTransform.rect.height / 2f;
			_trickContent.SetUpBorder(upBorder);
		}

		public void SetAchievementInfo(AchievementInfo BBOFGPLPEPB)
		{
			_perkContent.gameObject.SetActive(false);
			_trickContent.gameObject.SetActive(false);
			_achievContent.gameObject.SetActive(true);
			_achievContent.Init(BBOFGPLPEPB.Description, BBOFGPLPEPB.MoneyPrize, BBOFGPLPEPB.BonusPrize, BBOFGPLPEPB.OnTakeReward, BBOFGPLPEPB.CanTakeReward, BBOFGPLPEPB.IsCompleted);
			if (_achievContent.HeaderFontSize > 0)
			{
				headerFontSize = _achievContent.HeaderFontSize;
			}
			else
			{
				headerFontSize = 104;
			}
			_noContentMessage.gameObject.SetActive(false);
			SetLabel(BBOFGPLPEPB.Title);
			float upBorder = _header.transform.localPosition.y - _header.rectTransform.rect.height / 2f;
			_trickContent.SetUpBorder(upBorder);
		}

		public void SetItemInfo(ItemInfo PJDAGCBPLJE)
		{
			_perkContent.gameObject.SetActive(false);
			_trickContent.gameObject.SetActive(false);
			_achievContent.gameObject.SetActive(true);
			_achievContent.Init(PJDAGCBPLJE.DescriptionAlias);
			if (_achievContent.HeaderFontSize > 0)
			{
				headerFontSize = _achievContent.HeaderFontSize;
			}
			else
			{
				headerFontSize = 104;
			}
			_noContentMessage.gameObject.SetActive(false);
			SetLabel(PJDAGCBPLJE.Name);
			float upBorder = _header.transform.localPosition.y - _header.rectTransform.rect.height / 2f;
			_trickContent.SetUpBorder(upBorder);
		}

		public void SetLabel(string HHAAFADDOJB)
		{
			_header.set_LabelFontSize(headerFontSize);
			_header.set_Alias(HHAAFADDOJB);
		}

		public void SetNoContentMessage(string LIOGIBJBHAH)
		{
			_noContentMessage.set_Alias(LIOGIBJBHAH);
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
