using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Dialogs
{
	public class BaseDialog : SFMonoBehaviour<object>, BackKeyController
	{
		public enum DialogCloseEvent
		{
			OnPopupClose = 0,
			OnPopupCloseOK = 1,
			OnPopupCloseCascade = 2
		}

		public enum FooterType
		{
			FOOTER_NONE = 0,
			FOOTER_OK = 1,
			FOOTER_CANCEL = 2,
			FOOTER_BOTH = 3
		}

		public const int BASE_TEXT_FONT_SIZE = 103;

		public const int BASE_LINE_HEIGHT = 180;

		private const int HeaderOffset = 64;

		private const int ContentMarginSmall = 86;

		private const int ContentMarginLarge = 100;

		private const int ButtonGap = 64;

		private const int MinContentHalfHeight = 272;

		private const int MaxContentHalfHeight = 544;

		private const int ContentPadding = 60;

		private const int HeaderFontSize = 152;

		protected List<Button> _btns = new List<Button>();

		protected List<TextTimer> textTimers = new List<TextTimer>();

		[SerializeField]
		protected ResolutionImage _topStripe;

		[SerializeField]
		protected ResolutionImage _bottomStripe;

		[SerializeField]
		protected GameObject _content;

		[SerializeField]
		protected LabelAlias _header;

		[SerializeField]
		protected LabelButton _btnCancel;

		[SerializeField]
		protected LabelButton _btnOK;

		protected string titleAlias = string.Empty;

		protected string defaultOkButtonAlias = string.Empty;

		protected string cancelButtonAlias = string.Empty;

		protected bool unusedFlag;

		protected static int unusedStaticCounter;

		protected bool unusedSecondFlag;

		protected float _fadeDuration = 1f;

		public bool IsIgnoreBack;

		public bool IsQuestDialog;

		protected FooterType footerType;

		public bool IsPausing = true;

		public bool TopMenuIsActive;

		public LabelAlias Title
		{
			get
			{
				return get_Header();
			}
		}

		public LabelButton CancelButton
		{
			get
			{
				return get_ButtonCancel();
			}
		}

		public LabelButton OkButton
		{
			get
			{
				return get_ButtonOK();
			}
		}

		public LabelAlias get_Header()
		{
			return _header;
		}

		public LabelButton get_ButtonCancel()
		{
			return _btnCancel;
		}

		public LabelButton get_ButtonOK()
		{
			return _btnOK;
		}

		public virtual void Init(object data)
		{
			Init(string.Empty);
		}

		public virtual void Init(string DIKEFIIPNBE = "", string EHMEFCPIODJ = "OK", string EOCPGMKEEHK = "CANCEL", FooterType HJNAHNICGMH = FooterType.FOOTER_NONE)
		{
			titleAlias = DIKEFIIPNBE;
			defaultOkButtonAlias = EHMEFCPIODJ;
			cancelButtonAlias = EOCPGMKEEHK;
			footerType = HJNAHNICGMH;
			BackKeyManager.get_Instance().AddBackKeyController(this);
		}

		protected virtual void Start()
		{
			SetupContent();
			FitContentSize();
			LayoutStripes();
			SetupHeader(titleAlias);
			SetupFooter(footerType);
			if (AssemblyController.GetGamepadEnabled())
			{
				ApplyPlatformLayout();
			}
			Eclipse.UI.DialogCinematic.Play(base.gameObject, _content);
		}

		private void OnDestroy()
		{
			foreach (TextTimer item in textTimers)
			{
				item.set_Label(null);
			}
		}

		public virtual void Close(object data)
		{
			DialogCloseEvent iPJEOLNMLEH = DialogCloseEvent.OnPopupCloseOK;
			OnClose(iPJEOLNMLEH);
		}

		public virtual void OnClose(object data)
		{
			base.gameObject.SetActive(false);
			// Release the global raycaster lock before purchase/upgrade callbacks
			// rebuild and refocus the shop UI.  If a callback opens another dialog,
			// that dialog will establish its own lock normally.
			DialogsManager.GetInstance().StopDialog(this);
			BackKeyManager.get_Instance().RemoveBackKeyController(this);
			try
			{
				CallEvent(0, data);
			}
			finally
			{
				DestroyDialog();
			}
		}

		public virtual void OnBackKeyClicked(object data)
		{
			if (!IsIgnoreBack)
			{
				int leftButtonId = GetLeftButtonId();
				OnClose(leftButtonId);
			}
		}

		public virtual int GetLeftButtonId()
		{
			DialogCloseEvent result = DialogCloseEvent.OnPopupCloseOK;
			if (_btnCancel != null)
			{
				result = (DialogCloseEvent)_btnCancel.ButtonId;
			}
			else if (_btnOK != null)
			{
				result = (DialogCloseEvent)_btnOK.ButtonId;
			}
			return (int)result;
		}

		protected virtual void SetupContent()
		{
		}

		protected virtual void LayoutStripes()
		{
			float num = GetHalfContentHeight();
			if (_topStripe != null)
			{
				_topStripe.transform.SetLocalY(150f + num);
			}
			if (_bottomStripe != null)
			{
				_bottomStripe.transform.SetLocalY(0f - (160f + num));
			}
		}

		protected virtual void SetupFooter(FooterType HJNAHNICGMH)
		{
			bool flag = HJNAHNICGMH == FooterType.FOOTER_BOTH;
			_btns.Clear();
			if (flag || HJNAHNICGMH == FooterType.FOOTER_OK)
			{
				SetupButton(_btnOK, FooterType.FOOTER_OK);
				_btnOK.transform.SetLocalX((!flag) ? 0f : (_btnOK.get_rect().width / 2f + 32f));
				_btns.Add(_btnOK);
			}
			else
			{
				_btnOK.gameObject.SetActive(false);
			}
			if (flag || HJNAHNICGMH == FooterType.FOOTER_CANCEL)
			{
				SetupButton(_btnCancel, FooterType.FOOTER_CANCEL);
				_btnCancel.transform.SetLocalX((!flag) ? 0f : (0f - (_btnCancel.get_rect().width / 2f + 32f)));
				_btns.Add(_btnCancel);
			}
			else
			{
				_btnCancel.gameObject.SetActive(false);
			}
			PositionButtons();
		}

		protected virtual void PositionButtons()
		{
			float num = GetHalfContentHeight() + 60f;
			foreach (LabelButton item in _btns)
			{
				item.transform.SetLocalY(0f - num);
			}
		}

		protected virtual void SetupButton(LabelButton GAMILDJHFDB, FooterType MOPOCBKIKBI)
		{
			GAMILDJHFDB.gameObject.SetActive(true);
			string alias = string.Empty;
			int buttonId = 0;
			LabelButton.ButtonColor color = LabelButton.ButtonColor.BUTTON_WHITE;
			switch (MOPOCBKIKBI)
			{
			case FooterType.FOOTER_CANCEL:
				alias = cancelButtonAlias;
				color = LabelButton.ButtonColor.BUTTON_DARK;
				buttonId = 0;
				break;
			case FooterType.FOOTER_OK:
				alias = defaultOkButtonAlias;
				color = LabelButton.ButtonColor.BUTTON_WHITE;
				buttonId = 1;
				break;
			}
			GAMILDJHFDB.SetColor(color);
			GAMILDJHFDB.SetAlias(alias);
			GAMILDJHFDB.ButtonId = buttonId;
			GAMILDJHFDB.RemoveEventListener(2, OnClose);
			GAMILDJHFDB.AddEventListener(2, OnClose);
			GAMILDJHFDB.transform.SetLocalX(0f);
		}

		protected virtual void FitContentSize()
		{
			if (_content.transform.childCount == 0)
			{
				return;
			}
			float num = 0f;
			float num2 = 0f;
			foreach (Transform item in _content.transform)
			{
				float y = item.transform.localPosition.y;
				float y2 = item.transform.localScale.y;
				float num3 = item.GetComponent<RectTransform>().rect.height;
				if (item.GetComponent<Text>() != null)
				{
					num3 = item.GetComponent<Text>().preferredHeight;
				}
				if (y + num3 / 2f > num)
				{
					num = y + num3 / 2f * y2;
				}
				if (y - num3 / 2f < num2)
				{
					num2 = y - num3 / 2f * y2;
				}
			}
			Vector2 sizeDelta = new Vector2(_content.GetComponent<RectTransform>().rect.width, 2f * Mathf.Max((!(num < 0f)) ? num : (0f - num), (!(num2 < 0f)) ? num2 : (0f - num2)));
			_content.GetComponent<RectTransform>().sizeDelta = sizeDelta;
		}

		protected virtual void DestroyDialog()
		{
			Object.Destroy(base.gameObject);
		}

		protected virtual void ApplyPlatformLayout()
		{
		}

		protected virtual float GetHalfContentHeight()
		{
			if (_content == null)
			{
				return 272f;
			}
			float value = _content.GetComponent<RectTransform>().rect.height / 2f + 60f;
			return Mathf.Clamp(value, 272f, 544f);
		}

		protected virtual void SetupHeader(string HCPNFPMHFCM)
		{
			_header.set_LabelFontSize(152);
			_header.color = Constants.DialogHeaderColor;
			_header.set_Alias(HCPNFPMHFCM);
			float x = _content.GetComponent<RectTransform>().rect.width - 120f;
			_header.rectTransform.sizeDelta = new Vector2(x, _header.rectTransform.rect.height);
			UpdateHeaderPosition();
		}

		public virtual void UpdateHeaderPosition()
		{
			float num = GetHalfContentHeight();
			_header.transform.SetLocalY(num + 64f);
		}

		protected virtual void RelayoutDialog()
		{
			LayoutStripes();
			UpdateHeaderPosition();
			PositionButtons();
		}

		protected virtual void RefreshTextTimers(object data)
		{
			foreach (TextTimer item in textTimers)
			{
				item.Refresh();
			}
		}
	}
}
