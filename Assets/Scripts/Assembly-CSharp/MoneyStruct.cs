using CodeStage.AntiCheat.ObscuredTypes;

public class MoneyStruct
{
	public GameCurrency Currency;

	public ObscuredInt Count;

	public MoneyStruct(GameCurrency currency, int _count)
	{
		Currency = currency;
		Count = (ObscuredInt)(_count);
	}
}
