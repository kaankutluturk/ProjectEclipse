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

		public QuestVariable(XmlNode node)
		{
			Node = node;
			Name = Node.Attributes["Name"].GetStringOrDefault(string.Empty);
			Value = Node.Attributes["Value"].GetStringOrDefault(string.Empty);
		}

		public void SetValue(string newValue)
		{
			Value = newValue;
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
			string questFile = Node.Attributes["FileName"].GetStringOrDefault(string.Empty);
			flag = DirectoryController.IsPathWithDrive(FileName);
			questFile = DirectoryController.StripProtocol(questFile);
			FileName = DirectoryController.ResolvePath(questFile);
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

	public void SaveCheckpoint(object data, int screenIndex, int checkpointIndex)
	{
		if (get_Parameters() == null)
		{
			XmlNode parametersNode = Node.AppendElement("QuestParameters");
			SetParameters(new ParametersQuest(parametersNode));
		}
		QuestParameters checkpointParameters = (QuestParameters)data;
		get_Parameters().SetCheckPointIndex(checkpointIndex);
		get_Parameters().SetScreenIndex(screenIndex);
		get_Parameters().SetFightName((checkpointParameters.GetFightList() == null) ? string.Empty : checkpointParameters.GetFightList().FightId.ToString());
		get_Parameters().SetFightResultName(checkpointParameters.fightResult);
		get_Parameters().SetRaidResultName(checkpointParameters.raidResult);
		get_Parameters().SetLevelUp(checkpointParameters.levelUp);
		get_Parameters().SetPower(checkpointParameters.energyChange);
		get_Parameters().set_FightAvgFPS(checkpointParameters.fightAvgFps);
		Eclipse.Modding.ModRuntime.SaveQuestLotteryContext(get_Parameters(), checkpointParameters);
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
