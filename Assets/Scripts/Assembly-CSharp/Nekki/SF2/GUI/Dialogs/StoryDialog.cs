using System.Collections.Generic;
using Nekki.Utils;
using UnityEngine;

namespace Nekki.SF2.GUI.Dialogs
{
	public class StoryDialog : BaseDialog
	{
		private const float ButtonYOffset = 120f;

		private const string PRICE_TEXT_BACKGROUND = "ShopPieces.stripe";

		private const int DefaultSizeOffset = 100;

		private const int TextWidthWithPortrait = 900;

		private const int ButtonOffsetBelow = -120;

		private const int ButtonsBottomOffset = 120;

		private const int ButtonRightEdgeX = 740;

		private const int ButtonSpacing = 60;

		private const int PortraitTextGap = 40;

		protected const int LabelFontSize = 103;

		protected const int FullTextWidth = 1680;

		private const int TimerLabelGap = 40;

		private const int TimerLabelTagBase = 1234;

		private const int ContentOffsetY = 20;

		protected float referenceHeight = GameUtils.GetDialogContentWidth();

		protected const float PortraitSizeRatio = 0.9f;

		protected const float ContentHeightRatio = 0.8f;

		private const int PortraitX = -500;

		private const int MinLineSplitIndex = 110;

		public const float FOOTER_TEXT_OFFSET_Y = 60f;

		public const float LINES_PADDING = 32.5f;

		public const float FOOTER_TEXT_DOWN_OFFSET_Y = 30f;

		protected TextTimer timer;

		[SerializeField]
		protected LabelAlias _text;

		[SerializeField]
		protected LabelAlias _timeLabel;

		[SerializeField]
		protected TimerLabel _timerLabel;

		[SerializeField]
		protected ResolutionImage _portrait;

		[SerializeField]
		protected GameObject _textsSprite;

		protected float defaultSize = 100f;

		protected int currentPageIndex;

		protected int unusedIndex;

		protected bool showCancelButton = true;

		protected List<StoryDialogContent> contents = new List<StoryDialogContent>();

		protected string portraitSpriteName = string.Empty;

		protected bool showPortrait = true;

		protected UserItem timerUserItem;

		protected RecipeItemInfo timerRecipe;

		protected string messageText = string.Empty;

		protected string storyOkButtonAlias = string.Empty;

		protected LabelButton.ButtonColor cancelButtonColor;

		protected LabelButton.ButtonColor okButtonColor;

		protected int timerContentId = int.MaxValue;

		protected bool isTimerActive;

		protected long _leftTime;

		protected List<StoryDialogContent> priceLineContents = new List<StoryDialogContent>();

		[SerializeField]
		protected GameObject _footerTextsSprite;

		protected bool messageRefreshPending;

		protected bool showAllContents;

		public override void Init(object data)
		{
			string title = string.Empty;
			if (data != null)
			{
				StoryDialogInfo info = (StoryDialogInfo)data;
				title = info.Title;
				portraitSpriteName = info.PortraitName;
				showPortrait = info.ShowPortrait;
				contents = info.Contents;
				showCancelButton = info.ShowCancelButton;
				storyOkButtonAlias = info.OkButtonText;
				okButtonColor = info.OkButtonColor;
				cancelButtonAlias = info.CancelButtonText;
				cancelButtonColor = info.CancelButtonColor;
				showAllContents = info.UseEdgeButtons;
				if (info.Dlg != null)
				{
					AddEventListener(0, info.Dlg);
				}
			}
			GlobalTimer.get_Instance().addEventListener(0, OnTimerTick);
			base.Init(title, storyOkButtonAlias, cancelButtonAlias);
		}

		private new void Start()
		{
			base.Start();
			RelayoutDialog();
		}

		private void OnDestroy()
		{
			GlobalTimer.get_Instance().removeEventListener(0, OnTimerTick);
		}

		public override void Close(object data)
		{
			DialogCloseEvent closeEvent = DialogCloseEvent.OnPopupClose;
			OnClose(closeEvent);
		}

		protected override void SetupContent()
		{
			SetupPortrait(portraitSpriteName);
			if (showAllContents)
			{
				BuildAllContents();
			}
			else
			{
				ShowFirstContent();
			}
			_content.transform.SetLocalY(20f);
			Vector2 sizeDelta = new Vector2(_content.GetComponent<RectTransform>().rect.width, referenceHeight * 0.9f * 0.8f);
			_content.GetComponent<RectTransform>().sizeDelta = sizeDelta;
		}

		protected override void FitContentSize()
		{
			float num = 0f;
			if (_text.get_text() != string.Empty)
			{
				num = _text.preferredHeight;
			}
			else if (_textsSprite != null)
			{
				num += _textsSprite.GetComponent<RectTransform>().rect.height;
			}
			float b = referenceHeight * 0.9f * 0.8f;
			float num2 = Mathf.Max(num, b);
			if (_footerTextsSprite.gameObject.activeSelf)
			{
				num2 += _footerTextsSprite.GetComponent<RectTransform>().rect.height + 32.5f;
			}
			Vector2 sizeDelta = new Vector2(_content.GetComponent<RectTransform>().rect.width, num2);
			_content.GetComponent<RectTransform>().sizeDelta = sizeDelta;
		}

		protected override void SetupFooter(FooterType footer)
		{
			if (currentPageIndex + 1 < contents.Count)
			{
				SetButton(contents[currentPageIndex].ButtonText);
			}
			if (currentPageIndex + 1 == contents.Count)
			{
				defaultOkButtonAlias = ((!(storyOkButtonAlias == string.Empty)) ? storyOkButtonAlias : contents[currentPageIndex].ButtonText);
				ShowLastPageButtons();
			}
			else if (contents.Count == 0)
			{
				defaultOkButtonAlias = ((!(storyOkButtonAlias == string.Empty)) ? storyOkButtonAlias : defaultOkButtonAlias);
				ShowLastPageButtons();
			}
		}

		protected virtual void ShowLastPageButtons()
		{
			FooterType footer = FooterType.FOOTER_NONE;
			footer = ((!showCancelButton) ? FooterType.FOOTER_OK : FooterType.FOOTER_BOTH);
			base.SetupFooter(footer);
			_btnOK.RemoveEventListener(2, OnClose);
			_btnOK.RemoveEventListener(2, OnNextClicked);
			_btnOK.AddEventListener(2, OnNextClicked);
			_btnCancel.gameObject.SetActive(showCancelButton);
			if (showCancelButton)
			{
				_btnCancel.RemoveEventListener(2, OnClose);
				_btnCancel.AddEventListener(2, OnCancelClicked);
			}
			PositionButtons();
			ApplyPlatformLayout();
		}

		protected virtual void SetupPortrait(string portraitName)
		{
			string[] array = portraitName.Split('|');
			string[] array2 = portraitName.Split('/');
			string[] array3 = array2[array2.Length - 1].Split('.');
			_portrait.set_TexturePath(SF2Paths.GetUsersUiPath());
			// Eclipse mod dialogs pass qualified sprite IDs (owner:path); the legacy
			// basename/extension trimming would drop their namespace.
			_portrait.set_SpriteName(array[0].IndexOf(':') > 0 ? array[0] : array3[0]);
			int num = ((array.Length <= 1) ? 1 : (-1));
			_portrait.transform.SetLocalY(0f);
			_portrait.transform.SetLocalX(-500f);
			_portrait.SetNativeSize();
			_portrait.transform.localScale = new Vector2(1.8f * (float)num, 1.8f);
			_portrait.gameObject.SetActive(showPortrait);
		}

		protected virtual void PositionButtons()
		{
			float y = _bottomStripe.transform.localPosition.y;
			float localY = y + 120f;
			if (_btnOK != null && _btnOK.gameObject.activeSelf)
			{
				float bAINMLLIKOL2 = 740f - _btnOK.GetComponent<RectTransform>().rect.width / 2f;
				_btnOK.transform.SetLocalX(bAINMLLIKOL2);
				_btnOK.transform.SetLocalY(localY);
			}
			if (_btnCancel != null && _btnCancel.gameObject.activeSelf)
			{
				float num = 0f;
				TransformExtensions.SetLocalX(value: (!_btnOK.gameObject.activeSelf) ? ((1680f - _btnCancel.GetComponent<RectTransform>().rect.width) / 2f - 740f) : (_btnOK.transform.localPosition.x - 60f - (_btnOK.GetComponent<RectTransform>().rect.width + _btnCancel.GetComponent<RectTransform>().rect.width) / 2f), transform: _btnCancel.transform);
				_btnCancel.transform.SetLocalY(localY);
			}
		}

		protected virtual void SetMessageKey(string key)
		{
			messageRefreshPending = true;
			messageText = LocalizationManager.GetString(key);
			_timeLabel.gameObject.SetActive(false);
		}

		protected virtual void ShowContent(StoryDialogContent content)
		{
			SetupTextLabel(content);
			SetMessageKey(content.Text);
			if (_timerLabel != null)
			{
				_timerLabel.gameObject.SetActive(false);
			}
			int num = -1;
			if (content.ItemTimer != null)
			{
				num = textTimers.IndexOf(content.ItemTimer);
				if (num == -1)
				{
					textTimers.Add(content.ItemTimer);
					_timerLabel = CreateTimerLabel(content);
					content.ItemTimer.set_Label(_timerLabel);
					content.ItemTimer.Refresh();
					num = textTimers.Count - 1;
				}
				_timerLabel = content.ItemTimer.GetLabel();
			}
			else
			{
				_timerLabel = null;
			}
			bool flag = false;
			foreach (Transform item in _content.transform)
			{
				if (item.gameObject.tag == (1234 + num).ToString())
				{
					flag = true;
					break;
				}
			}
			if ((bool)_timerLabel && !flag)
			{
				_timerLabel.set_LabelFontSize(103);
				_timerLabel.color = content.ItemTimer.Color;
				_timerLabel.tag = (1234 + num).ToString();
				_timerLabel.transform.SetParent(_content.transform, false);
			}
			if (content.CheckTimer)
			{
				StartTimer(content);
			}
		}

		protected virtual void StartTimer(StoryDialogContent content)
		{
			isTimerActive = true;
			_leftTime = content.Timer;
			timerContentId = content.Id;
			timerUserItem = content.OwnedItem;
			timerRecipe = content.Recipe;
			if (_leftTime <= 0)
			{
				OnClose(timerContentId);
			}
			UpdateTimerLabel();
		}

		protected virtual void SetButton(string alias)
		{
			defaultOkButtonAlias = alias;
			SetupButton(_btnOK, FooterType.FOOTER_OK);
			_btnOK.RemoveEventListener(2, OnClose);
			_btnOK.RemoveEventListener(2, OnNextClicked);
			_btnOK.AddEventListener(2, OnNextClicked);
		}

		protected virtual void OnTimerTick(object data)
		{
			if (textTimers.Count > 0)
			{
				RefreshTextTimers(0);
				CloseIfTimerExpired();
			}
			if (isTimerActive)
			{
				if (_leftTime <= 0)
				{
					OnClose(timerContentId);
				}
				UpdateLeftTime();
				UpdateTimerLabel();
			}
		}

		private void UpdateLeftTime()
		{
			if (timerUserItem == null)
			{
				if (timerRecipe == null)
				{
					_leftTime--;
				}
				else
				{
					_leftTime = GameUtils.GetLeftTime(timerRecipe.GetDeliveryEndTime());
				}
			}
			else
			{
				_leftTime = GameUtils.GetLeftTime(timerUserItem.GetDeliveryTimestamp());
			}
		}

		protected virtual void ShowFirstContent()
		{
			_timeLabel.set_Alias(string.Empty);
			_timeLabel.set_text(string.Empty);
			_timeLabel.color = Constants.DialogHeaderColor;
			_timeLabel.set_LabelFontSize(103);
			_timeLabel.gameObject.SetActive(false);
			_timeLabel.transform.SetParent(_content.transform, false);
			int count = contents.Count;
			if (count > 0)
			{
				ShowContent(contents[0]);
			}
		}

		protected virtual void SetupTextLabel(StoryDialogContent content)
		{
			_text.gameObject.SetActive(true);
			_text.set_Alias(string.Empty);
			_text.set_text(string.Empty);
			_text.set_LabelFontSize(103);
			_text.color = content.FontColor;
			_text.alignment = TextAnchor.MiddleLeft;
			_text.transform.SetLocalY(0f);
			float x = ((!showPortrait) ? 1680 : 900);
			_text.rectTransform.sizeDelta = new Vector2(x, _text.rectTransform.rect.height);
		}

		protected virtual void OnNextClicked(object data)
		{
			currentPageIndex++;
			if (currentPageIndex == contents.Count - 1)
			{
				ShowContent(contents[currentPageIndex]);
				defaultOkButtonAlias = ((!(storyOkButtonAlias == string.Empty)) ? storyOkButtonAlias : contents[currentPageIndex].ButtonText);
				ShowLastPageButtons();
			}
			else if (currentPageIndex >= contents.Count)
			{
				OnClose(data);
			}
			else
			{
				ShowContent(contents[currentPageIndex]);
				SetButton(contents[currentPageIndex].ButtonText);
				PositionButtons();
				ApplyPlatformLayout();
			}
		}

		protected void OnCancelClicked(object data)
		{
			base.OnClose(data);
		}

		private void Update()
		{
			if (messageRefreshPending)
			{
				messageRefreshPending = false;
				ApplyMessageText();
			}
		}

		protected virtual void ApplyMessageText()
		{
			string text = messageText;
			if (_timerLabel != null)
			{
				text += _timerLabel.get_text();
			}
			_text.set_text(text);
			_text.transform.SetLocalY(0f);
			if (showPortrait)
			{
				_text.transform.SetLocalX(_portrait.transform.localPosition.x + 40f + referenceHeight * 0.9f / 2f + _text.rectTransform.rect.width / 2f);
			}
			else
			{
				_text.transform.SetLocalX(0f);
			}
			if (_timerLabel != null)
			{
				_timerLabel.gameObject.SetActive(true);
			}
			UpdateTimerLabel();
			FitContentSize();
			RelayoutDialog();
			PositionButtons();
		}

		protected virtual void UpdateTimerLabel()
		{
			if (!(_timeLabel == null) && isTimerActive)
			{
				bool showDays = true;
				string timeString = TimerLabel.GetTimeString(_leftTime, true, true, true, showDays, ":", string.Empty, true, true, true, true, true, string.Empty, string.Empty, string.Empty);
				_timeLabel.set_text(timeString);
				_timeLabel.gameObject.SetActive(true);
				if (!_text)
				{
					_timeLabel.transform.SetLocalX(_portrait.transform.localPosition.x + 40f + referenceHeight * 0.9f / 2f + _timeLabel.preferredWidth / 2f);
				}
				else if (!(_text.preferredWidth + _timeLabel.preferredWidth < 900f))
				{
					_text.transform.SetLocalY(40f);
					_timeLabel.transform.SetLocalX(_portrait.transform.localPosition.x + 40f + referenceHeight * 0.9f / 2f + _timeLabel.preferredWidth / 2f);
					_timeLabel.transform.SetLocalY(_text.transform.localPosition.y - _text.preferredHeight / 2f - 40f);
				}
			}
		}

		protected virtual void BuildPriceLines()
		{
			_footerTextsSprite.gameObject.SetActive(true);
			float num = 0f;
			float num2 = 0f;
			int num3 = 0;
			foreach (StoryDialogContent item in priceLineContents)
			{
				GameObject gameObject = new GameObject("LabelAlias");
				LabelAlias labelAlias = gameObject.AddComponent<LabelAlias>();
				labelAlias.set_LabelFontSize(103);
				labelAlias.UseLabelLineSpacing = true;
				labelAlias.set_LabelLineSpacing(0.7f);
				labelAlias.color = item.FontColor;
				labelAlias.alignment = TextAnchor.MiddleCenter;
				labelAlias.alignByGeometry = true;
				labelAlias.verticalOverflow = VerticalWrapMode.Overflow;
				labelAlias.rectTransform.sizeDelta = new Vector2(1680f, 10f);
				labelAlias.set_text(LocalizationManager.GetString(item.Text));
				string text = LocalizationManager.GetString(item.Text);
				bool flag = null != item.ItemTimer;
				TimerLabel timerLabel = null;
				if (flag)
				{
					textTimers.Add(item.ItemTimer);
					timerLabel = CreateTimerLabel(item);
					item.ItemTimer.set_Label(timerLabel);
					item.ItemTimer.Refresh();
					timerLabel.transform.SetParent(_footerTextsSprite.transform, false);
					text = text + "<visible=0>" + timerLabel.get_text() + "</>";
				}
				labelAlias.set_text(text);
				if (num3 > 0)
				{
					num2 += 32.5f;
				}
				num -= labelAlias.preferredHeight / 2f;
				labelAlias.transform.SetParent(_footerTextsSprite.transform, false);
				labelAlias.transform.SetLocalY(num);
				labelAlias.transform.SetLocalX(0f);
				num -= labelAlias.preferredHeight / 2f + 32.5f;
				if (flag)
				{
					timerLabel.gameObject.SetActive(true);
				}
				GameObject gameObject2 = new GameObject("PriceBackground");
				ResolutionImage resolutionImage = gameObject2.AddComponent<ResolutionImage>();
				resolutionImage.set_SpriteName("ShopPieces.stripe");
				resolutionImage.transform.SetParent(_footerTextsSprite.transform, false);
				resolutionImage.SetNativeSize();
				resolutionImage.transform.SetLocalX(labelAlias.transform.localPosition.x);
				resolutionImage.transform.SetLocalY(labelAlias.transform.localPosition.y);
				float width = resolutionImage.rectTransform.rect.width;
				float width2 = labelAlias.rectTransform.rect.width;
				if (width < width2 + 240f)
				{
					float x = width2 + 240f;
					resolutionImage.rectTransform.sizeDelta = new Vector2(x, resolutionImage.rectTransform.rect.height);
				}
				num2 += resolutionImage.rectTransform.rect.height;
				num3++;
			}
			_footerTextsSprite.GetComponent<RectTransform>().sizeDelta = new Vector2(1680f, num2);
			float a = _portrait.transform.localPosition.y - referenceHeight * 0.9f * 0.8f / 2f;
			float b = _textsSprite.transform.localPosition.y - _textsSprite.GetComponent<RectTransform>().rect.height;
			float num4 = Mathf.Min(a, b);
			_footerTextsSprite.transform.SetLocalY(num4 - 60f);
		}

		protected override void SetupButton(LabelButton button, FooterType footer)
		{
			button.gameObject.SetActive(true);
			string alias = string.Empty;
			int buttonId = 0;
			LabelButton.ButtonColor color = LabelButton.ButtonColor.BUTTON_WHITE;
			switch (footer)
			{
			case FooterType.FOOTER_CANCEL:
				alias = cancelButtonAlias;
				color = cancelButtonColor;
				buttonId = 0;
				break;
			case FooterType.FOOTER_OK:
				alias = defaultOkButtonAlias;
				color = okButtonColor;
				buttonId = 1;
				break;
			}
			button.SetColor(color);
			button.SetAlias(alias);
			button.ButtonId = buttonId;
			button.RemoveEventListener(2, OnClose);
			button.AddEventListener(2, OnClose);
		}

		protected virtual void BuildAllContents()
		{
			float num = 0f;
			_textsSprite.gameObject.SetActive(true);
			float x = ((!showPortrait) ? 1680 : 900);
			float num2 = 0f;
			int i = 0;
			for (int count = contents.Count; i < count; i++)
			{
				StoryDialogContent content = contents[i];
				if (!content.CheckTimer)
				{
					switch (content.Type)
					{
					case StoryDialogContent.ContentType.CONTENT_TYPE_REGULAR:
					{
						GameObject gameObject = new GameObject("LabelAlias");
						LabelAlias labelAlias = gameObject.AddComponent<LabelAlias>();
						labelAlias.set_LabelFontSize(103);
						labelAlias.UseLabelLineSpacing = true;
						labelAlias.set_LabelLineSpacing(0.7f);
						labelAlias.color = content.FontColor;
						labelAlias.alignment = TextAnchor.MiddleLeft;
						labelAlias.alignByGeometry = true;
						labelAlias.verticalOverflow = VerticalWrapMode.Overflow;
						labelAlias.rectTransform.sizeDelta = new Vector2(x, 10f);
						string text = LocalizationManager.GetString(content.Text);
						bool flag = null != content.ItemTimer;
						TimerLabel timerLabel = null;
						if (flag)
						{
							textTimers.Add(content.ItemTimer);
							timerLabel = CreateTimerLabel(content);
							content.ItemTimer.set_Label(timerLabel);
							content.ItemTimer.Refresh();
							timerLabel.transform.SetParent(_textsSprite.transform, false);
							text += "<visible=0>00:00:00</>";
						}
						labelAlias.set_text(text);
						labelAlias.transform.SetParent(_textsSprite.transform, false);
						num -= labelAlias.preferredHeight / 2f;
						labelAlias.transform.SetLocalY(num);
						labelAlias.transform.SetLocalX(_portrait.transform.localPosition.x + 40f + referenceHeight * 0.9f / 2f + labelAlias.rectTransform.rect.width / 2f);
						num -= labelAlias.preferredHeight / 2f + 32.5f;
						num2 += labelAlias.preferredHeight + 32.5f;
						if (flag)
						{
							timerLabel.gameObject.SetActive(true);
						}
						break;
					}
					case StoryDialogContent.ContentType.CONTENT_TYPE_PRICELINE:
						priceLineContents.Add(content);
						break;
					}
				}
				else
				{
					_timeLabel.set_Alias(string.Empty);
					_timeLabel.set_text(string.Empty);
					_timeLabel.color = Constants.DialogHeaderColor;
					_timeLabel.set_LabelFontSize(103);
					_timeLabel.gameObject.SetActive(false);
					_timeLabel.transform.SetParent(_textsSprite.transform, false);
					StartTimer(content);
					num -= _timeLabel.preferredHeight / 4f;
					_timeLabel.transform.SetLocalY(num);
					num -= _timeLabel.preferredHeight / 4f + 32.5f;
					num2 += _timeLabel.preferredHeight;
				}
			}
			contents.Clear();
			_textsSprite.GetComponent<RectTransform>().sizeDelta = new Vector2(x, num2);
			_textsSprite.transform.SetLocalY(-20f - num / 2f);
			if (priceLineContents.Count > 0)
			{
				BuildPriceLines();
			}
		}

		protected virtual TimerLabel CreateTimerLabel(StoryDialogContent content)
		{
			GameObject gameObject = new GameObject("TimerLabel");
			TimerLabel timerLabel = gameObject.AddComponent<TimerLabel>();
			timerLabel.IsSeconds = content.ItemTimer.IsSeconds;
			timerLabel.IsMinutes = content.ItemTimer.IsMinutes;
			timerLabel.IsHours = content.ItemTimer.IsHours;
			timerLabel.IsDays = content.ItemTimer.IsDays;
			timerLabel.Delimiter = content.ItemTimer.Delimiter;
			timerLabel.DaysString = content.ItemTimer.DaysString;
			timerLabel.UseDaysDelimiter = content.ItemTimer.UseDaysDelimiter;
			timerLabel.IsSecondsZero = content.ItemTimer.IsSecondsZero;
			timerLabel.IsMinutesZero = content.ItemTimer.IsMinutesZero;
			timerLabel.IsHoursZero = content.ItemTimer.IsHoursZero;
			timerLabel.IsDaysZero = content.ItemTimer.IsDaysZero;
			timerLabel.set_LabelFontSize(103);
			timerLabel.color = content.ItemTimer.Color;
			return timerLabel;
		}

		protected List<string> ParseString(string text)
		{
			int length = text.Length;
			int num = length;
			string empty = string.Empty;
			string empty2 = string.Empty;
			for (int i = 110; i < length; i++)
			{
				if (text[i] == ' ')
				{
					num = i + 1;
					break;
				}
			}
			empty.Insert(0, text.Substring(0, num));
			empty2.Insert(0, text.Substring(num));
			List<string> list = new List<string>();
			list.Add(empty);
			if (empty2 != string.Empty)
			{
				list.Add(empty2);
			}
			return list;
		}

		protected virtual void CloseIfTimerExpired()
		{
			foreach (TextTimer item in textTimers)
			{
				if (item.Time <= 0)
				{
					OnClose(0);
					break;
				}
			}
		}
	}
}
