using System.Xml;

public class UserTutorials
{
	private XmlAttribute storyStepAttribute;

	private string _storyTutorialStep = string.Empty;

	private XmlAttribute raidStepAttribute;

	private RaidTutorialStepCode raidTutorialStep = RaidTutorialStepCode.RaidTutNotStarted;

	private XmlAttribute raidGemsTakenAttribute;

	private bool raidGemsTaken;

	private XmlAttribute forgeMaterialsGivenAttribute;

	private bool forgeMaterialsGiven;

	public string StoryStep
	{
		get
		{
			return GetStoryStep();
		}
		set
		{
			set_StoryTutorialStep(value);
		}
	}

	public bool IsStoryTutorialActive
	{
		get
		{
			return GetIsStoryTutorialActive();
		}
	}

	public RaidTutorialStepCode RaidStep
	{
		get
		{
			return GetRaidStep();
		}
		set
		{
			SetRaidStep(value);
		}
	}

	public bool RaidGemsTaken
	{
		get
		{
			return GetRaidGemsTaken();
		}
		set
		{
			SetRaidGemsTaken(value);
		}
	}

	public bool ForgeMaterialsGiven
	{
		get
		{
			return GetForgeMaterialsGiven();
		}
		set
		{
			SetForgeMaterialsGiven(value);
		}
	}

	public string GetStoryStep()
	{
		return _storyTutorialStep;
	}

	public void set_StoryTutorialStep(string value)
	{
		if (!(_storyTutorialStep == value))
		{
			_storyTutorialStep = value;
			storyStepAttribute.Value = _storyTutorialStep;
			ListSF.GetRoster().RequestSave();
		}
	}

	public bool GetIsStoryTutorialActive()
	{
		return _storyTutorialStep != "END";
	}

	public RaidTutorialStepCode GetRaidStep()
	{
		return raidTutorialStep;
	}

	public void SetRaidStep(RaidTutorialStepCode value)
	{
		if (raidTutorialStep <= value)
		{
			raidTutorialStep = value;
			raidStepAttribute.Value = GameUtils.RaidTutorialStepNames[raidTutorialStep];
			ListSF.GetRoster().RequestSave();
		}
	}

	public bool GetRaidGemsTaken()
	{
		return raidGemsTaken;
	}

	public void SetRaidGemsTaken(bool value)
	{
		if (raidGemsTaken != value)
		{
			raidGemsTaken = value;
			raidGemsTakenAttribute.Value = ((!raidGemsTaken) ? "0" : "1");
			ListSF.GetRoster().RequestSave();
		}
	}

	public bool GetForgeMaterialsGiven()
	{
		return forgeMaterialsGiven;
	}

	public void SetForgeMaterialsGiven(bool value)
	{
		forgeMaterialsGiven = value;
		forgeMaterialsGivenAttribute.Value = ((!forgeMaterialsGiven) ? "0" : "1");
		ListSF.GetRoster().RequestSave();
	}

	public void Parse(XmlNode node)
	{
		storyStepAttribute = node.Attributes["Tutorial"];
		if (storyStepAttribute == null)
		{
			storyStepAttribute = node.AppendAttribute("Tutorial");
			storyStepAttribute.Value = GameUtils.TutorialSettings.StepsNames[0];
		}
		string text = storyStepAttribute.GetStringOrDefault();
		_storyTutorialStep = ((!GameUtils.TutorialSettings.IsStepName(text)) ? GameUtils.TutorialSettings.StepsNames[0] : text);
		raidStepAttribute = node.Attributes["RaidTutorialStep"];
		if (raidStepAttribute == null)
		{
			raidStepAttribute = node.AppendAttribute("RaidTutorialStep");
		}
		raidTutorialStep = GameUtils.GetRaidTutorialStepByName(raidStepAttribute.GetStringOrDefault("NotStarted"));
		raidGemsTakenAttribute = node.Attributes["RaidTutorialGemsTaken"];
		if (raidGemsTakenAttribute == null)
		{
			raidGemsTakenAttribute = node.AppendAttribute("RaidTutorialGemsTaken");
		}
		raidGemsTaken = raidGemsTakenAttribute.ParseBool();
		forgeMaterialsGivenAttribute = node.Attributes["ForgeTutorialMaterialsGiven"];
		if (forgeMaterialsGivenAttribute == null)
		{
			forgeMaterialsGivenAttribute = node.AppendAttribute("ForgeTutorialMaterialsGiven");
		}
		forgeMaterialsGiven = forgeMaterialsGivenAttribute.ParseBool();
	}
}
