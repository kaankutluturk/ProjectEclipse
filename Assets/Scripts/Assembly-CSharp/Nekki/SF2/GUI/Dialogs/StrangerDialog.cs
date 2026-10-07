using System;
using System.Collections.Generic;
using Nekki.SF2.GUI.Map;
using Nekki.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Dialogs
{
	public class StrangerDialog : StoryDialog
	{
		private const int AcceptButtonId = 2;

		private const int RejectButtonId = 0;

		private const int StoreButtonId = 1;

		private const int CheckBoxButtonId = 3;

		private const int WaitButtonId = 13;

		private const int GoodbyeButtonId = 1;

		private const int CheckBoxButtonOffset = 100;

		private const int CheckBoxOffsetY = 90;

		private const int DifficultyBarOffsetY = -80;

		private const int DifficultyLabelOffsetY = -60;

		private const int DifficultyX = 280;

		private const int ButtonsBottomOffset = 130;

		private const int WaitButtonFontSize = 87;

		private const int ButtonGap = 40;

		private const int PortraitTextGap = 40;

		private const int TextWidthWithPortrait = 900;

		private const int CheckBoxLabelFontSize = 103;

		[SerializeField]
		private LabelButton _rejectButton;

		[SerializeField]
		private LabelButton _acceptButton;

		[SerializeField]
		private LabelButton _storeButton;

		[SerializeField]
		private Toggle _checkBox;

		[SerializeField]
		private LabelAlias _checkBoxLabel;

		[SerializeField]
		private ProgressBar _difficult;

		[SerializeField]
		private LabelAlias _difficultLabel;

		private string rejectButtonText;

		private string acceptButtonText;

		private string storeButtonText;

		private LabelButton.ButtonColor rejectButtonColor;

		private LabelButton.ButtonColor acceptButtonColor;

		private LabelButton.ButtonColor storeButtonColor;

		private float _ratio;

		private bool showDifficulty;

		private bool hasCheckBox;

		private bool checkBoxInitialValue;

		private string checkBoxText = string.Empty;

		private Action<object> _dlg;

		public override void Init(object data)
		{
			StrangerDialogInfo info = (StrangerDialogInfo)data;
			contents = info.Contents;
			rejectButtonColor = info.RejectButtonColor;
			rejectButtonText = info.RejectButtonText;
			acceptButtonColor = info.AcceptButtonColor;
			acceptButtonText = info.AcceptButtonText;
			storeButtonColor = info.StoreButtonColor;
			storeButtonText = info.StoreButtonText;
			_ratio = info.Ratio;
			showDifficulty = info.ShowDifficulty;
			showAllContents = info.UseEdgeButtons;
			hasCheckBox = info.ShowCheckBox;
			checkBoxInitialValue = info.CheckBoxChecked;
			checkBoxText = info.CheckBoxText;
			if (info.Dlg != null)
			{
				_dlg = info.Dlg.Invoke;
				AddEventListener(0, info.Dlg);
			}
			GlobalTimer.get_Instance().addEventListener(0, OnTimerTick);
			portraitSpriteName = info.PortraitName;
			base.Init(info.Title, "dlgButtonWait", "dlgStoryBtnGoodbye");
		}

		private new void Start()
		{
			base.Start();
			UpdateDifficultyRating();
			PositionButtons();
		}

		private void OnDestroy()
		{
			GlobalTimer.get_Instance().removeEventListener(0, OnTimerTick);
		}

		protected override void SetupFooter(FooterType footer)
		{
			SetupButtons();
			if (hasCheckBox)
			{
				FitContentSize();
				RelayoutDialog();
			}
			if (showAllContents && GetVisibleButtonCount() < 3)
			{
				_btnOK = GetEdgeButton(true);
				LabelButton labelButton = GetEdgeButton(false);
				_btnCancel = ((!(labelButton != _btnOK)) ? null : labelButton);
				PositionButtons();
			}
		}

		protected override void SetupContent()
		{
			base.SetupContent();
			SetupDifficultyBar();
			if (hasCheckBox)
			{
				float localY = _content.transform.localPosition.y + 50f + 20f;
				_content.transform.SetLocalY(localY);
			}
		}

		protected override void ApplyPlatformLayout()
		{
			base.ApplyPlatformLayout();
		}

		protected override void LayoutStripes()
		{
			base.LayoutStripes();
			if (hasCheckBox)
			{
				float localY = _topStripe.transform.localPosition.y + 12f;
				_topStripe.transform.SetLocalY(localY);
				localY = _bottomStripe.transform.localPosition.y - 12f;
				_bottomStripe.transform.SetLocalY(localY);
			}
		}

		protected override void PositionButtons()
		{
			base.PositionButtons();
			if (GetVisibleButtonCount() == 3)
			{
				float y = _bottomStripe.transform.localPosition.y;
				float num = y + 130f;
				if (hasCheckBox)
				{
					num += 130f;
				}
				_storeButton.transform.SetLocalY(num);
				_acceptButton.transform.SetLocalY(num);
				_rejectButton.transform.SetLocalY(num);
			}
			if (hasCheckBox)
			{
				if (_btnOK.gameObject.activeSelf)
				{
					float localY = _btnOK.transform.localPosition.y + 100f + 30f;
					_btnOK.transform.SetLocalY(localY);
				}
				if (_btnCancel != null && _btnCancel.gameObject.activeSelf)
				{
					float cancelButtonY = _btnCancel.transform.localPosition.y + 100f + 30f;
					_btnCancel.transform.SetLocalY(cancelButtonY);
				}
				if (_checkBox.gameObject.activeSelf)
				{
					float checkBoxY = _bottomStripe.transform.localPosition.y + 90f;
					_checkBox.transform.SetLocalY(checkBoxY);
					_checkBoxLabel.transform.SetLocalY(checkBoxY);
					float num2 = _checkBox.GetComponent<RectTransform>().rect.width / 2f + _checkBoxLabel.preferredWidth;
					_checkBox.transform.SetLocalX((0f - num2) / 2f);
					float num3 = _checkBox.transform.localPosition.x + _checkBox.GetComponent<RectTransform>().rect.width / 2f;
					_checkBoxLabel.transform.SetLocalX(num3 + _checkBoxLabel.preferredWidth / 2f);
				}
			}
		}

		protected override void FitContentSize()
		{
			float num = 0f;
			if (_text.get_text() != string.Empty)
			{
				num = _text.preferredHeight;
			}
			else if (_textsSprite.gameObject.activeSelf)
			{
				num += _textsSprite.GetComponent<RectTransform>().rect.height;
			}
			if (_checkBox.gameObject.activeSelf)
			{
				num += _checkBox.GetComponent<RectTransform>().rect.height;
			}
			if (showDifficulty)
			{
				num += 240f;
			}
			float b = referenceHeight * 0.9f * 0.8f;
			float num2 = Mathf.Max(num, b);
			if (_footerTextsSprite.gameObject.activeSelf)
			{
				float num3 = _footerTextsSprite.GetComponent<RectTransform>().rect.height + 60f + 30f;
				num2 += num3;
				_content.transform.SetLocalY(_content.transform.localPosition.y + num3 / 2f);
			}
			Vector2 sizeDelta = new Vector2(_content.GetComponent<RectTransform>().rect.width, num2);
			_content.GetComponent<RectTransform>().sizeDelta = sizeDelta;
		}

		public override int GetLeftButtonId()
		{
			return GetEdgeButton(false).ButtonId;
		}

		private void SetupButtons()
		{
			if (_btnCancel != null)
			{
				_btnCancel.gameObject.SetActive(false);
			}
			if (_btnOK != null)
			{
				_btnOK.gameObject.SetActive(false);
			}
			SetupButton(_rejectButton, rejectButtonColor, rejectButtonText, 0);
			_rejectButton.RemoveEventListener(2, ButtonCallback);
			_rejectButton.AddEventListener(2, ButtonCallback);
			_rejectButton.gameObject.SetActive(rejectButtonText != string.Empty);
			SetupButton(_acceptButton, acceptButtonColor, acceptButtonText, 2);
			_acceptButton.RemoveEventListener(2, ButtonCallback);
			_acceptButton.AddEventListener(2, ButtonCallback);
			_acceptButton.gameObject.SetActive(acceptButtonText != string.Empty);
			SetupButton(_storeButton, storeButtonColor, storeButtonText, 1);
			_storeButton.RemoveEventListener(2, ButtonCallback);
			_storeButton.AddEventListener(2, ButtonCallback);
			_storeButton.gameObject.SetActive(storeButtonText != string.Empty);
			float num = 0f;
			float localY = _bottomStripe.transform.localPosition.y + 130f;
			num = (0f - _acceptButton.GetComponent<RectTransform>().rect.width) / 2f - 40f - _rejectButton.GetComponent<RectTransform>().rect.width / 2f;
			_rejectButton.transform.SetLocalX(num);
			_rejectButton.transform.SetLocalY(localY);
			num = 0f;
			_acceptButton.transform.SetLocalX(num);
			_acceptButton.transform.SetLocalY(localY);
			num = _acceptButton.GetComponent<RectTransform>().rect.width / 2f + 40f + _storeButton.GetComponent<RectTransform>().rect.width / 2f;
			_storeButton.transform.SetLocalX(num);
			_storeButton.transform.SetLocalY(localY);
			if (hasCheckBox)
			{
				_checkBox.gameObject.SetActive(true);
				_checkBox.isOn = checkBoxInitialValue;
				_checkBox.onValueChanged.AddListener((bool value) =>
				{
					ButtonCallback(3);
				});
				_checkBox.transform.SetLocalY(localY);
				_checkBoxLabel.gameObject.SetActive(true);
				_checkBoxLabel.set_LabelFontSize(103);
				_checkBoxLabel.color = Constants.DialogTextColor;
				_checkBoxLabel.set_Alias(checkBoxText);
				_checkBoxLabel.transform.SetLocalY(localY);
			}
		}

		protected override void ShowLastPageButtons()
		{
			defaultOkButtonAlias = "dlgButtonWait";
			base.SetupFooter(FooterType.FOOTER_BOTH);
			_acceptButton.gameObject.SetActive(false);
			_rejectButton.gameObject.SetActive(false);
			_storeButton.gameObject.SetActive(false);
			float localX = 20f + _btnOK.GetComponent<RectTransform>().rect.width / 2f;
			float buttonY = _bottomStripe.transform.localPosition.y + 130f;
			_btnOK.transform.SetLocalX(localX);
			_btnOK.transform.SetLocalY(buttonY);
			_btnOK.Label.set_LabelFontSize(87);
			_btnOK.ButtonId = 13;
			_btnOK.RemoveEventListener(2, OnClose);
			_btnOK.AddEventListener(2, ButtonCallback);
			localX = -20f - _btnCancel.GetComponent<RectTransform>().rect.width / 2f;
			_btnCancel.transform.SetLocalX(localX);
			_btnCancel.transform.SetLocalY(buttonY);
			_btnCancel.Label.set_LabelFontSize(87);
			_btnCancel.ButtonId = 1;
			_btnCancel.RemoveEventListener(2, OnClose);
			_btnCancel.AddEventListener(2, ButtonCallback);
			PositionButtons();
		}

		protected override void ApplyMessageText()
		{
			_text.set_text(messageText);
			_text.transform.SetLocalX(_portrait.transform.localPosition.x + 40f + referenceHeight * 0.9f / 2f + _text.rectTransform.rect.width / 2f);
			_text.transform.SetLocalY(0f);
			if (_difficult.gameObject.activeSelf)
			{
				_difficult.transform.SetLocalX(280f);
				_difficultLabel.transform.SetLocalX(280f);
				_text.transform.SetLocalY(70f + _difficultLabel.preferredHeight / 4f);
				_difficult.transform.SetLocalY(-80f + _text.transform.localPosition.y - _text.preferredHeight / 2f);
				_difficultLabel.transform.SetLocalY(-60f + _difficult.transform.localPosition.y - _difficultLabel.preferredHeight / 4f);
			}
			UpdateTimerLabel();
			FitContentSize();
			RelayoutDialog();
			PositionButtons();
		}

		private int GetVisibleButtonCount()
		{
			int num = 0;
			if (rejectButtonText != string.Empty)
			{
				num++;
			}
			if (storeButtonText != string.Empty)
			{
				num++;
			}
			if (acceptButtonText != string.Empty)
			{
				num++;
			}
			return num;
		}

		private LabelButton GetEdgeButton(bool fromEnd)
		{
			if (fromEnd)
			{
				return _storeButton.gameObject.activeSelf ? _storeButton : (_acceptButton.gameObject.activeSelf ? _acceptButton : ((!_rejectButton.gameObject.activeSelf) ? null : _rejectButton));
			}
			return _rejectButton.gameObject.activeSelf ? _rejectButton : (_acceptButton.gameObject.activeSelf ? _acceptButton : ((!_storeButton.gameObject.activeSelf) ? null : _storeButton));
		}

		private void SetupButton(LabelButton button, LabelButton.ButtonColor color, string alias, int buttonId)
		{
			button.gameObject.SetActive(true);
			button.SetColor(color);
			button.SetAlias(alias);
			button.ButtonId = buttonId;
		}

		private void SetupDifficultyBar()
		{
			_difficult.SetValueBorders(0f, 100f);
			_difficult.gameObject.SetActive(showDifficulty);
			_difficultLabel.set_Alias(string.Empty);
			_difficultLabel.set_text("???");
			_difficultLabel.set_LabelFontSize(103);
			_difficultLabel.color = Constants.DialogTextColor;
			_difficultLabel.gameObject.SetActive(showDifficulty);
		}

		private void UpdateDifficultyRating()
		{
			if (_ratio < 0f)
			{
				return;
			}
			int num = 0;
			int num2 = 0;
			List<global::Pair<string, float>> difficultyEvaluation = DifficultyPanel.get_DifficultyEvaluation();
			global::Pair<string, float> bestRating = difficultyEvaluation[0];
			foreach (global::Pair<string, float> item in difficultyEvaluation)
			{
				if (item.Second < _ratio && bestRating.Second < item.Second)
				{
					bestRating = item;
					num2 = num;
				}
				num++;
			}
			_difficult.Stripe.set_SpriteName(Constants.DifficultyBarSprites[num2]);
			if (bestRating != null)
			{
				_difficult.SetValue(100f);
				_difficultLabel.set_Alias(bestRating.First);
			}
		}

		private void ButtonCallback(object data)
		{
			switch ((int)data)
			{
			case 2:
				OnClose(data);
				break;
			case 0:
				OnClose(data);
				break;
			case 1:
				OnClose(data);
				break;
			case 3:
				if (_dlg != null)
				{
					int num = ((!_checkBox.isOn) ? 4 : 3);
					_dlg(num);
				}
				break;
			}
		}
	}
}
