using System.Xml;

public class QuestActionDenomination : QuestAction
{
	private int denominationDigits;

	private string coinIcon = "MiscSprites.gold";

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		denominationDigits = EPKLCPOEELO.Attributes["DenominationDigits"].ParseInt(-1);
		coinIcon = EPKLCPOEELO.Attributes["CoinIcon"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		int nPFOBKBJAOB = ListSF.GetRoster().GetDenominationDigits();
		ListSF.GetRoster().SetDenominationDigits(denominationDigits);
		ListSF.GetRoster().SetCoinIcon(coinIcon);
		ApplyDenomination(nPFOBKBJAOB);
		MenuController.RecreateMoney();
		ScreenType iPKNDMINFMJ = Module.GetInstance().ScreenInfo.ScreenType;
		if (iPKNDMINFMJ != ScreenType.ModuleFight)
		{
			Module.OpenScreen(iPKNDMINFMJ);
		}
		ListSF.GetInstance().OnAuthenticate(true);
		FinishAction();
	}

	private void ApplyDenomination(int NPFOBKBJAOB)
	{
		ItemInfo.DenominateItems(NPFOBKBJAOB);
		ListSF.GetRoster().RescaleCurrencyDenomination(NPFOBKBJAOB);
	}
}
