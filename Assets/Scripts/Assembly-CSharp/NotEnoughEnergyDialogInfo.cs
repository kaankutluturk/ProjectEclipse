public class NotEnoughEnergyDialogInfo
{
	public int WaitSeconds;

	public GameValueType ValueType;

	public int Value;

	public NotEnoughEnergyDialogInfo(int waitSeconds, GameValueType valueType, int value)
	{
		WaitSeconds = waitSeconds;
		ValueType = valueType;
		Value = value;
	}
}
