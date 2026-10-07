using System;
using Nekki.SF2.GUI.Dialogs;

public class TradeDialogInfo
{
	public Action<object> Dlg;

	public GameValueType Value;

	public TradeDialog.TradeAction TradeType;

	public long Price;

	public long DeliverySeconds;

	public TradeDialogInfo(TradeDialog.TradeAction CBFFIFKAHHN, GameValueType _value = GameValueType.Gold, long JIBAGOMMNKE = 0L, Action<object> _dlg = null, long IGMDKDOGGNA = 0L)
	{
		TradeType = CBFFIFKAHHN;
		Value = _value;
		Price = JIBAGOMMNKE;
		Dlg = _dlg;
		DeliverySeconds = IGMDKDOGGNA;
	}
}
