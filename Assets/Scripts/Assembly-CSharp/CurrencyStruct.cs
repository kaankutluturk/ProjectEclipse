using CodeStage.AntiCheat.ObscuredTypes;

public class CurrencyStruct
{
	public GameCurrency Currency;

	public ObscuredInt Count;

	public CurrencyStruct(GameCurrency currency, int _count)
	{
		Currency = currency;
		Count = (ObscuredInt)(_count);
	}
}
