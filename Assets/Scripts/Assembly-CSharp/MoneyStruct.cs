using CodeStage.AntiCheat.ObscuredTypes;

public class MoneyStruct
{
	public GameCurrency Currency;

	public ObscuredInt Count;

	public MoneyStruct(GameCurrency JJPFBOKGIEF, int _count)
	{
		Currency = JJPFBOKGIEF;
		Count = (ObscuredInt)(_count);
	}
}
