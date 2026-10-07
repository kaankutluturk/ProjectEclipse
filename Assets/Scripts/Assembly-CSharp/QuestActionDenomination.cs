using System.Xml;

public class QuestActionDenomination : QuestAction
{
	private int denominationDigits;

	private string coinIcon = "MiscSprites.gold";

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		denominationDigits = node.Attributes["DenominationDigits"].ParseInt(-1);
		coinIcon = node.Attributes["CoinIcon"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		int previousDigits = ListSF.GetRoster().GetDenominationDigits();
		ListSF.GetRoster().SetDenominationDigits(denominationDigits);
		ListSF.GetRoster().SetCoinIcon(coinIcon);
		ApplyDenomination(previousDigits);
		MenuController.RecreateMoney();
		ScreenType currentScreen = Module.GetInstance().ScreenInfo.ScreenType;
		if (currentScreen != ScreenType.ModuleFight)
		{
			Module.OpenScreen(currentScreen);
		}
		ListSF.GetInstance().OnAuthenticate(true);
		FinishAction();
	}

	private void ApplyDenomination(int previousDigits)
	{
		ItemInfo.DenominateItems(previousDigits);
		ListSF.GetRoster().RescaleCurrencyDenomination(previousDigits);
	}
}
