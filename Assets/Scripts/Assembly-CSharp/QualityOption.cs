using System.Collections.Generic;
using System.Xml;

public class QualityOption
{
	public enum QualityOptionType
	{
		OPTION_NONE = 0,
		OPTION_REDUCE_FPS = 1,
		OPTION_PARTICLES_OFF = 2,
		OPTION_SRQUENCES_OFF = 3
	}

	public enum QualityLevel
	{
		QUALITY_LOW = 0,
		QUALITY_MEDIUM = 1,
		QUALITY_HIGH = 2,
		QUALITY_NONE = 3
	}

	private QualityOptionType _type;

	private List<string> _conditions = new List<string>();

	public QualityOption(XmlNode node)
	{
		_type = ParseOptionType(node.Attributes["Name"].GetStringOrDefault(string.Empty));
		foreach (XmlNode childNode in node.ChildNodes)
		{
			_conditions.Add(childNode.Attributes["Name"].GetStringOrDefault(string.Empty));
		}
	}

	public static QualityOptionType ParseOptionType(string name)
	{
		switch (name)
		{
		case "ReduceFPS":
			return QualityOptionType.OPTION_REDUCE_FPS;
		case "ParticlesOff":
			return QualityOptionType.OPTION_PARTICLES_OFF;
		case "SequencesOff":
			return QualityOptionType.OPTION_SRQUENCES_OFF;
		default:
			GameLog.Error("QualityOption::getOptionFromString - unknown type: %s", name);
			return QualityOptionType.OPTION_NONE;
		}
	}

	public static QualityLevel ParseQualityLevel(string name)
	{
		switch (name)
		{
		case "LOW":
			return QualityLevel.QUALITY_LOW;
		case "MEDIUM":
			return QualityLevel.QUALITY_MEDIUM;
		case "HIGH":
			return QualityLevel.QUALITY_HIGH;
		default:
			return QualityLevel.QUALITY_NONE;
		}
	}

	public static string GetNextQualityCondition(string quality, string condition)
	{
		string text = "LOW";
		switch (ParseQualityLevel(condition))
		{
		case QualityLevel.QUALITY_HIGH:
		case QualityLevel.QUALITY_NONE:
			return CycleQualityUp(quality);
		case QualityLevel.QUALITY_MEDIUM:
			return ToggleLowMedium(quality);
		default:
			return "LOW";
		}
	}

	public static bool CompareQualityCondition(string qualityA, string qualityB)
	{
		return ParseQualityLevel(qualityA) > ParseQualityLevel(qualityB);
	}

	public void ApplyIfConditionMatches()
	{
		string text = GraphicsController.GetEffectiveQualityCondition();
		foreach (string item in _conditions)
		{
			if (item == text)
			{
				EnableOption();
				break;
			}
		}
	}

	private static string CycleQualityUp(string quality)
	{
		switch (quality)
		{
		case "LOW":
			return "MEDIUM";
		case "MEDIUM":
			return "HIGH";
		case "HIGH":
			return "LOW";
		default:
			return "HIGH";
		}
	}

	private static string ToggleLowMedium(string quality)
	{
		if (quality == "LOW")
		{
			return "MEDIUM";
		}
		if (quality == "MEDIUM")
		{
			return "LOW";
		}
		return "MEDIUM";
	}

	private void EnableOption()
	{
		switch (_type)
		{
		case QualityOptionType.OPTION_REDUCE_FPS:
			break;
		case QualityOptionType.OPTION_PARTICLES_OFF:
			GameUtils.ParticlesDisabled = true;
			break;
		case QualityOptionType.OPTION_SRQUENCES_OFF:
			GameUtils.SequencesDisabled = true;
			break;
		default:
			GameLog.Error("QualityOption::turnOption - unknown type: %s", _type);
			break;
		}
	}
}
