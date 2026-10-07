using System;
using UnityEngine;

namespace Nekki.SF2.GUI.Dialogs
{
	public class TradeDialog : BaseDialog
	{
		public enum TradeAction
		{
			A_BUY = 0,
			A_UPGRADE = 1
		}

		[SerializeField]
		private LabelAlias _text;

		private string _titleString = string.Empty;

		private string messageKey = string.Empty;

		private long price;

		private GameValueType _value;

		private TradeAction action;

		private long waitTimeSeconds;

		public override void Init(object data)
		{
			TradeDialogInfo info = (TradeDialogInfo)data;
			if (info == null)
			{
				_value = GameValueType.Gold;
				price = 0L;
				action = TradeAction.A_BUY;
				waitTimeSeconds = 0L;
			}
			else
			{
				_value = info.Value;
				price = info.Price;
				action = info.TradeType;
				waitTimeSeconds = info.DeliverySeconds;
			}
			string text;
			switch (action)
			{
			case TradeAction.A_BUY:
				text = "shopBuy";
				_titleString = "dlgBuyTitle";
				messageKey = "dlgBuyMessage";
				break;
			case TradeAction.A_UPGRADE:
				text = "dlgUpgradeButton";
				_titleString = "dlgUpgradeTitle";
				messageKey = "dlgUpgradeMessage";
				break;
			default:
				text = "OK";
				break;
			}
			if (waitTimeSeconds > 0)
			{
				if (action != TradeAction.A_UPGRADE)
				{
					text = "dlgOrderButton";
					_titleString = "dlgOrderTitle";
					messageKey = "dlgOrderMessage";
				}
				else
				{
					text = "dlgUpgradeButton";
					_titleString = "dlgUpgradeTitle";
					messageKey = "dlgUpgradeMessage";
				}
			}
			defaultOkButtonAlias = text;
			if (info != null && info.Dlg != null)
			{
				AddEventListener(0, info.Dlg);
			}
			base.Init(_titleString, text, "CANCEL", FooterType.FOOTER_BOTH);
		}

		protected override void SetupContent()
		{
			string text = "img::";
			switch (_value)
			{
			case GameValueType.Gold:
				text += ListSF.GetRoster().GetCoinIcon();
				break;
			case GameValueType.Gems:
				text += "MiscSprites.ruby";
				break;
			}
			string questionKey = "dlgCurrencyQuestion{" + text + "}{" + price + "}";
			string text2 = LocalizationManager.GetString(messageKey) + "\n" + LocalizationManager.GetString(questionKey);
			_text.set_text(text2);
			if (waitTimeSeconds > 0)
			{
				string empty = string.Empty;
				empty = ((action != TradeAction.A_BUY) ? "dlgTimeUpgrade" : "dlgTimeDelivery");
				TimeSpan timeSpan = TimeSpan.FromSeconds(waitTimeSeconds);
				string empty2 = string.Empty;
				empty2 = ((timeSpan.Hours <= 0) ? string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds) : string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds));
				string text3 = LocalizationManager.GetString(empty, empty2);
				text2 = text2 + "\n" + text3;
				_text.set_text(text2);
			}
		}
	}
}
