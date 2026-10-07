using System;
using Nekki.SF2.GUI.Dialogs;

public class TradeDialogInfo
{
	public Action<object> Dlg;

	public GameValueType Value;

	public TradeDialog.TradeAction TradeType;

	public long Price;

	public long DeliverySeconds;

	public TradeDialogInfo(TradeDialog.TradeAction tradeType, GameValueType _value = GameValueType.Gold, long price = 0L, Action<object> _dlg = null, long deliverySeconds = 0L)
	{
		TradeType = tradeType;
		Value = _value;
		Price = price;
		Dlg = _dlg;
		DeliverySeconds = deliverySeconds;
	}
}
