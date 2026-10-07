using UnityEngine;

namespace Nekki.SF2.GUI.Dialogs
{
	public class ImpossibleDialog : BaseDialog
	{
		public enum ImpossibleDialogType
		{
			A_NOT_ENOUGH_GOLD = 0,
			A_NOT_ENOUGH_RUBY = 1,
			A_NOT_ENOUGH_ENERGY = 2,
			A_NOT_ENOUGH_LEVEL = 3,
			A_NOT_NETWORK = 4,
			A_EQUIP_ERROR = 5,
			A_UNEQUIP_ERROR = 6,
			A_SELL_ERROR = 7
		}

		private const int HeaderFontSize = 135;

		private const int MessageOffsetY = 70;

		private const int MessageFontSize = 122;

		private ImpossibleDialogType dialogType = ImpossibleDialogType.A_SELL_ERROR;

		private string _titleString = string.Empty;

		private object _contentData;

		[SerializeField]
		protected LabelAlias _text;

		public override void Init(object data)
		{
			ImpossibleDialogInfo fHBGDNBFPLG = (ImpossibleDialogInfo)data;
			dialogType = fHBGDNBFPLG.Reason;
			_contentData = fHBGDNBFPLG.Content;
			if (fHBGDNBFPLG.Dlg != null)
			{
				AddEventListener(0, fHBGDNBFPLG.Dlg);
			}
			switch (dialogType)
			{
			case ImpossibleDialogType.A_NOT_ENOUGH_GOLD:
				SetupNotEnoughGold();
				break;
			case ImpossibleDialogType.A_NOT_ENOUGH_RUBY:
				SetupNotEnoughRuby();
				break;
			case ImpossibleDialogType.A_NOT_ENOUGH_ENERGY:
				SetupNotEnoughEnergy();
				break;
			default:
				SetupErrorDialog(dialogType);
				break;
			}
			base.Init(_titleString, "dlgBuyButton", "Cancel", footerType);
		}

		protected virtual void SetupNotEnoughGold()
		{
			_titleString = "dlgNotEnoughGoldTitle";
			SetupBuyDialog(ImpossibleDialogType.A_NOT_ENOUGH_GOLD);
		}

		protected virtual void SetupNotEnoughRuby()
		{
			_titleString = "dlgNotEnoughRubyTitle";
			SetupBuyDialog(ImpossibleDialogType.A_NOT_ENOUGH_RUBY);
		}

		protected virtual void SetupBuyDialog(ImpossibleDialogType IBODMPMJELJ)
		{
			defaultOkButtonAlias = "shopBuy";
			footerType = FooterType.FOOTER_BOTH;
			string empty = string.Empty;
			switch (IBODMPMJELJ)
			{
			default:
				return;
			case ImpossibleDialogType.A_NOT_ENOUGH_GOLD:
				empty = "dlgNotEnoughGoldMessage";
				break;
			case ImpossibleDialogType.A_NOT_ENOUGH_RUBY:
				empty = "dlgNotEnoughRubyMessage";
				break;
			case ImpossibleDialogType.A_NOT_ENOUGH_ENERGY:
				empty = "dlgNotEnoughEnergyMessage";
				break;
			}
			SetupMessageAlias(empty);
		}

		protected virtual void SetupErrorDialog(ImpossibleDialogType IBODMPMJELJ)
		{
			defaultOkButtonAlias = "ok";
			footerType = FooterType.FOOTER_OK;
			_titleString = "dlgErrorTitle";
		}

		protected virtual void SetupNotEnoughEnergy()
		{
			if (_contentData != null)
			{
				_titleString = "dlgNotEnoughEnergyTitle";
				defaultOkButtonAlias = "dlgNotEnoughEnergyButton";
				footerType = FooterType.FOOTER_BOTH;
				string empty = string.Empty;
				string empty2 = string.Empty;
				int num = 0;
				NotEnoughEnergyDialogInfo oJJHNNJPMCI = (NotEnoughEnergyDialogInfo)_contentData;
				empty = TimerLabel.GetTimeString(oJJHNNJPMCI.WaitSeconds, true, true, true, false, ":", string.Empty, true, true, true, true, true, string.Empty, string.Empty, string.Empty);
				num = oJJHNNJPMCI.Value;
				GameValueType hGIKOMLPBMJ = oJJHNNJPMCI.ValueType;
				string empty3 = string.Empty;
				switch (hGIKOMLPBMJ)
				{
				case GameValueType.Gems:
					empty3 = "MiscSprites.ruby";
					break;
				case GameValueType.Energy:
					empty3 = "MiscSprites.energy";
					break;
				default:
					empty3 = ListSF.GetRoster().GetCoinIcon();
					break;
				}
				empty2 = num.ToString();
				string text = LocalizationManager.GetString("dlgNotEnoughEnergyMessage1");
				text += "|[0]";
				text += LocalizationManager.GetString("dlgNotEnoughEnergyMessage2");
				text += " ";
				text += empty;
				text += "\n";
				text += LocalizationManager.GetString("dlgNotEnoughEnergyMessage3");
				text += "|[1]";
				text += empty2;
				text += " ";
				text += LocalizationManager.GetString("dlgNotEnoughEnergyMessage4");
				_text.alignment = TextAnchor.MiddleCenter;
				_text.transform.SetLocalX(0f);
				_text.transform.SetLocalY(70f);
				_text.set_LabelFontSize(122);
				_text.color = Constants.DialogTextColor;
				_text.set_text(text);
			}
		}

		protected override void SetupHeader(string HCPNFPMHFCM)
		{
			base.SetupHeader(HCPNFPMHFCM);
			_header.set_LabelFontSize(135);
		}

		protected virtual void SetupMessageAlias(string APFHJLHOCEN)
		{
			_text.alignment = TextAnchor.MiddleCenter;
			_text.transform.SetLocalX(0f);
			_text.transform.SetLocalY(70f);
			_text.set_LabelFontSize(122);
			_text.color = Constants.DialogTextColor;
			_text.set_Alias(APFHJLHOCEN);
		}
	}
}
