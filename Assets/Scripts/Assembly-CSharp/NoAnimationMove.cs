using System.Xml;

public class NoAnimationMove
{
	public class MoveInfoBase
	{
		public string Name;

		public string Alias;

		public string Description;
	}

	public class RaidMoveInfo : MoveInfoBase
	{
		public XmlDocument MoveDocument;

		public XmlNode MoveNode;

		public string Type;

		public float Duration;

		public string ScriptName;

		public uint ChargeCost;
	}

	public class CritSettings
	{
		public float CritProbablity;

		public int CritAdditional;
	}

	private CritSettings _critSettings = new CritSettings();

	public CritSettings Crit
	{
		get
		{
			return GetCritSettings();
		}
	}

	public CritSettings GetCritSettings()
	{
		return _critSettings;
	}

	public RaidMoveInfo GetMoveByName(string moveName)
	{
		return null;
	}
}
