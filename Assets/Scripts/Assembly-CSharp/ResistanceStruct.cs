using CodeStage.AntiCheat.ObscuredTypes;

public class ResistanceStruct
{
	public GameResistance resistance;

	public ObscuredInt Count;

	public ResistanceStruct(GameResistance DHOLEOOFCMB, int _count)
	{
		resistance = DHOLEOOFCMB;
		Count = (ObscuredInt)(_count);
	}
}
