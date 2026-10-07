using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

public class RosterQuest
{
	public class QuestVariable
	{
		public string Name = string.Empty;

		public string Value = string.Empty;

		public XmlNode Node;

		public QuestVariable(string _name, string _value)
		{
			Name = _name;
			Value = _value;
		}

		public QuestVariable(XmlNode PKHDLOGJKAD)
		{
			Node = PKHDLOGJKAD;
			Name = Node.Attributes["Name"].GetStringOrDefault(string.Empty);
			Value = Node.Attributes["Value"].GetStringOrDefault(string.Empty);
		}

		public void SetValue(string PKHDLOGJKAD)
		{
			Value = PKHDLOGJKAD;
			Node.Attributes["Value"].Value = Value;
		}
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ParametersQuest questParameters;

	public string Name;

	public string FileName;

	public string Type;

	public XmlNode Node;

	public bool IsParametersCleared;

	public List<QuestVariable> Variables = new List<QuestVariable>();

	public ParametersQuest SavedParameters
	{
		get
		{
			return get_Parameters();
		}
		private set
		{
			SetParameters(value);
		}
	}

	public RosterQuest(XmlNode value)
	{
		Node = value;
		FileName = "quests.xml";
		bool flag = false;
		if (Node.Attributes["FileName"] != null)
		{
			string iFKJHHPJPLP = Node.Attributes["FileName"].GetStringOrDefault(string.Empty);
			flag = DirectoryController.IsPathWithDrive(FileName);
			iFKJHHPJPLP = DirectoryController.StripProtocol(iFKJHHPJPLP);
			FileName = DirectoryController.ResolvePath(iFKJHHPJPLP);
		}
		if (flag)
		{
			SetFileName(FileName);
		}
		IsParametersCleared = false;
		Name = Node.Attributes["Name"].GetStringOrDefault(string.Empty);
		Type = Node.Attributes["Type"].GetStringOrDefault(string.Empty);
		SetParameters(null);
		LoadParametersFromNode();
	}

	public ParametersQuest get_Parameters()
	{
		return questParameters;
	}

	private void SetParameters(ParametersQuest value)
	{
		questParameters = value;
	}

	public void LoadParametersFromNode()
	{
		XmlNode xmlNode = Node["QuestParameters"];
		if (xmlNode != null)
		{
			SetParameters(new ParametersQuest(xmlNode));
		}
	}

	public void SaveCheckpoint(object data, int AAKAPLGDGNM, int ILNNINKHPOC)
	{
		if (get_Parameters() == null)
		{
			XmlNode pKHDLOGJKAD = Node.AppendElement("QuestParameters");
			SetParameters(new ParametersQuest(pKHDLOGJKAD));
		}
		QuestParameters hHKLFIIBIFF = (QuestParameters)data;
		get_Parameters().SetCheckPointIndex(ILNNINKHPOC);
		get_Parameters().SetScreenIndex(AAKAPLGDGNM);
		get_Parameters().SetFightName((hHKLFIIBIFF.GetFightList() == null) ? string.Empty : hHKLFIIBIFF.GetFightList().FightId.ToString());
		get_Parameters().SetFightResultName(hHKLFIIBIFF.fightResult);
		get_Parameters().SetRaidResultName(hHKLFIIBIFF.raidResult);
		get_Parameters().SetLevelUp(hHKLFIIBIFF.levelUp);
		get_Parameters().SetPower(hHKLFIIBIFF.energyChange);
		get_Parameters().set_FightAvgFPS(hHKLFIIBIFF.fightAvgFps);
		Eclipse.Modding.ModRuntime.SaveQuestLotteryContext(get_Parameters(), hHKLFIIBIFF);
	}

	public void SetFileName(string _fileName)
	{
		_fileName = DirectoryController.StripProtocol(_fileName);
		if (Node.Attributes["FileName"] == null)
		{
			Node.AppendAttribute("FileName");
		}
		Node.Attributes["FileName"].Value = _fileName;
		FileName = _fileName;
	}

	public void ClearParameters()
	{
		XmlElement xmlElement = Node["QuestParameters"];
		if (xmlElement != null)
		{
			Node.RemoveChild(xmlElement);
		}
		if (get_Parameters() != null)
		{
			SetParameters(null);
		}
	}

	public int GetCheckpointScreenType()
	{
		return (get_Parameters() != null) ? get_Parameters().GetScreenIndex() : 0;
	}

	public int GetCheckpointIndex()
	{
		return (get_Parameters() != null) ? get_Parameters().GetCheckPointIndex() : 0;
	}
}
