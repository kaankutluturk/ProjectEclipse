using System;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Dialogs
{
	public class SimpleDialog : BaseDialog
	{
		private const int ContentOffsetY = 100;

		private const int ButtonsOffsetY = -20;

		private const float MaxFooterWidth = 1680f;

		private const float MinFooterWidth = 1100f;

		private const float FooterButtonPadding = 60f;

		private const float HeaderOffsetY = -20f;

		private const float DefaultFooterWidth = 100f;

		private const int CheckBoxLabelFontSize = 103;

		[SerializeField]
		private LabelAlias _label;

		[SerializeField]
		private LabelAlias _checkBoxLabel;

		[SerializeField]
		private Toggle _checkBox;

		private Action<object> _dlg;

		private bool _useLiteralText;

		private string messageText = string.Empty;

		private bool hasCheckBox;

		private bool checkBoxInitialValue;

		private string checkBoxText = string.Empty;

		private float footerWidth = 100f;

		protected LabelButton.ButtonColor okButtonColor = LabelButton.ButtonColor.BUTTON_WHITE;

		protected LabelButton.ButtonColor cancelButtonColor;

		public override void Init(object data)
		{
			_useLiteralText = false;
			string dIKEFIIPNBE = string.Empty;
			FooterType hJNAHNICGMH = FooterType.FOOTER_NONE;
			if (data != null)
			{
				SimpleDialogInfo jJMIOMABAKK = (SimpleDialogInfo)data;
				_useLiteralText = jJMIOMABAKK.UseLiteralText;
				defaultOkButtonAlias = jJMIOMABAKK.OkButtonText;
				cancelButtonAlias = jJMIOMABAKK.CancelButtonText;
				okButtonColor = jJMIOMABAKK.OkButtonStyle;
				cancelButtonColor = jJMIOMABAKK.CancelButtonStyle;
				_dlg = jJMIOMABAKK.Dlg;
				messageText = jJMIOMABAKK.Message;
				hasCheckBox = jJMIOMABAKK.ShowCheckBox;
				checkBoxInitialValue = jJMIOMABAKK.CheckBoxChecked;
				checkBoxText = jJMIOMABAKK.CheckBoxText;
				dIKEFIIPNBE = jJMIOMABAKK.Title;
				hJNAHNICGMH = jJMIOMABAKK.FooterType;
			}
			base.Init(dIKEFIIPNBE, defaultOkButtonAlias, cancelButtonAlias, hJNAHNICGMH);
		}

		protected override void Start()
		{
			base.Start();
			_label.set_Alias(_useLiteralText ? string.Empty : messageText);
			if (_useLiteralText) _label.set_text(messageText);
			_checkBox.gameObject.SetActive(hasCheckBox);
			_checkBoxLabel.gameObject.SetActive(hasCheckBox);
			if (hasCheckBox)
			{
				CreateCheckBox(checkBoxInitialValue, checkBoxText);
			}
			RefreshLayout();
		}

		protected override void SetupHeader(string title)
		{
			base.SetupHeader(_useLiteralText ? string.Empty : title);
			if (_useLiteralText) _header.set_text(title);
		}

		protected virtual void OnCheckBoxChanged(bool value)
		{
			if (_dlg != null)
			{
				int num = ((!value) ? 4 : 3);
				_dlg(num);
			}
		}

		private void LayoutContent()
		{
			float jMLAKAKDBBL = Math.Min(Math.Max(CalculateRequiredFooterWidth(), 1100f), 1680f);
			SetFooterWidth(jMLAKAKDBBL);
			PositionFooterButtons();
			_content.GetComponent<RectTransform>().sizeDelta = new Vector2(_content.GetComponent<RectTransform>().rect.width, _label.preferredHeight);
		}

		private void SetFooterWidth(float JMLAKAKDBBL)
		{
			footerWidth = JMLAKAKDBBL;
		}

		private void PositionFooterButtons()
		{
			if (footerType == FooterType.FOOTER_BOTH)
			{
				float width = _btnOK.GetComponent<RectTransform>().rect.width;
				float width2 = _btnCancel.GetComponent<RectTransform>().rect.width;
				float num = (footerWidth - width - width2) / 3f;
				float num2 = (0f - footerWidth) / 2f + num + width2 / 2f;
				_btnCancel.transform.SetLocalX(num2);
				num2 += width2 / 2f + num + width / 2f;
				_btnOK.transform.SetLocalX(num2);
			}
			else if (footerType == FooterType.FOOTER_OK)
			{
				_btnOK.transform.SetLocalX(0f);
			}
			else if (footerType == FooterType.FOOTER_CANCEL)
			{
				_btnCancel.transform.SetLocalX(0f);
			}
		}

		private float CalculateRequiredFooterWidth()
		{
			int num = 1;
			float num2 = 0f;
			if (footerType == FooterType.FOOTER_BOTH || footerType == FooterType.FOOTER_OK)
			{
				num2 = _btnOK.GetComponent<RectTransform>().rect.width;
				num++;
			}
			float num3 = 0f;
			if (footerType == FooterType.FOOTER_BOTH || footerType == FooterType.FOOTER_CANCEL)
			{
				num3 = _btnCancel.GetComponent<RectTransform>().rect.width;
				num++;
			}
			return num2 + num3 + 60f * (float)num;
		}

		private void RefreshLayout()
		{
			LayoutContent();
			PositionStripesAndButtons();
		}

		private void CreateCheckBox(bool EPHHGNKDPEG, string DOEEIGAHKEN)
		{
			float num = _checkBox.GetComponent<RectTransform>().rect.height / 4f;
			float bAINMLLIKOL = (0f - _label.preferredHeight) / 2f - 2f * num;
			_checkBox.transform.SetLocalY(bAINMLLIKOL);
			_label.transform.SetLocalY(_label.transform.localPosition.y + num);
			_checkBox.isOn = EPHHGNKDPEG;
			_checkBox.onValueChanged.RemoveListener(OnCheckBoxChanged);
			_checkBox.onValueChanged.AddListener(OnCheckBoxChanged);
			_checkBoxLabel.set_LabelFontSize(103);
			_checkBoxLabel.color = Constants.DialogTextColor;
			_checkBoxLabel.set_Alias(DOEEIGAHKEN);
			_checkBoxLabel.transform.SetLocalY(bAINMLLIKOL);
			float num2 = _checkBox.GetComponent<RectTransform>().rect.width + _checkBoxLabel.preferredWidth;
			num2 = _checkBoxLabel.preferredWidth;
			_checkBox.transform.SetLocalX((0f - num2) / 2f);
			_checkBoxLabel.transform.SetLocalX(_checkBox.GetComponent<RectTransform>().rect.width / 2f);
		}

		protected virtual void PositionStripesAndButtons()
		{
			float num = _content.GetComponent<RectTransform>().rect.height / 2f + 160f;
			if (_topStripe != null && _bottomStripe != null)
			{
				_topStripe.transform.SetLocalY(80f + num);
				_bottomStripe.transform.SetLocalY(-120f - num);
			}
			if (_header != null)
			{
				_header.transform.SetLocalY(num + -20f);
			}
			if (_btnOK != null)
			{
				_btnOK.transform.SetLocalY(0f - num + -20f);
			}
			if (_btnCancel != null)
			{
				_btnCancel.transform.SetLocalY(0f - num + -20f);
			}
		}

		protected override void SetupButton(LabelButton GAMILDJHFDB, FooterType MOPOCBKIKBI)
		{
			string alias = string.Empty;
			int buttonId = 0;
			LabelButton.ButtonColor color = LabelButton.ButtonColor.BUTTON_WHITE;
			switch (MOPOCBKIKBI)
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
			GAMILDJHFDB.SetColor(color);
			GAMILDJHFDB.SetAlias(_useLiteralText ? string.Empty : alias);
			if (_useLiteralText) GAMILDJHFDB.SetText(alias);
			GAMILDJHFDB.ButtonId = buttonId;
			GAMILDJHFDB.RemoveEventListener(2, OnClose);
			GAMILDJHFDB.AddEventListener(2, OnClose);
		}
	}
}
