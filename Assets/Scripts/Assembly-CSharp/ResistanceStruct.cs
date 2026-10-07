using CodeStage.AntiCheat.ObscuredTypes;

public class ResistanceStruct
{
	public GameResistance resistance;

	public ObscuredInt Count;

	public ResistanceStruct(GameResistance gameResistance, int _count)
	{
		resistance = gameResistance;
		Count = (ObscuredInt)(_count);
	}
}
