using System.Xml;

public class ConditionRoundResult : ConditionAnimation
{
	public enum RoundResultType
	{
		RESULT_TYPE_NONE = 0,
		RESULT_TYPE_VICTORY = 1,
		RESULT_TYPE_DEFEAT = 2
	}

	public enum RoundResultSubType
	{
		RESULT_SUBTYPE_NONE = 0,
		RESULT_SUBTYPE_TIMEOUT = 1,
		RESULT_SUBTYPE_RINGOUT = 2,
		RESULT_SUBTYPE_LOSE = 3
	}

	private RoundResultType _resultType;

	private RoundResultSubType _resultSubType;

	public ConditionRoundResult(XmlNode node)
		: base(ConditionType.ROUND_RESULT)
	{
		string text = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		string text2 = node.Attributes["Type"].GetStringOrDefault(string.Empty);
		if (text == "Victory")
		{
			_resultType = RoundResultType.RESULT_TYPE_VICTORY;
		}
		else if (text == "Defeat")
		{
			_resultType = RoundResultType.RESULT_TYPE_DEFEAT;
		}
		else
		{
			_resultType = RoundResultType.RESULT_TYPE_NONE;
		}
		if (text2 == "Timeout")
		{
			_resultSubType = RoundResultSubType.RESULT_SUBTYPE_TIMEOUT;
		}
		else if (text2 == "Ringout")
		{
			_resultSubType = RoundResultSubType.RESULT_SUBTYPE_RINGOUT;
		}
		else
		{
			_resultSubType = RoundResultSubType.RESULT_SUBTYPE_NONE;
		}
	}

	public override bool IsEqual(ModelConditions conditions)
	{
		bool flag = false;
		if (conditions.RoundEnded && (_resultType == RoundResultType.RESULT_TYPE_NONE || IsWinner(conditions.IsWinner)) && (_resultSubType == RoundResultSubType.RESULT_SUBTYPE_NONE || MatchesEndRoundType(conditions.EndRoundType)))
		{
			flag = true;
		}
		return (!IsNot) ? flag : (!flag);
	}

	private bool IsWinner(bool isPlayer)
	{
		return (isPlayer && _resultType == RoundResultType.RESULT_TYPE_VICTORY) || (!isPlayer && _resultType == RoundResultType.RESULT_TYPE_DEFEAT);
	}

	private bool MatchesEndRoundType(EndRoundType endRoundType)
	{
		return (endRoundType == EndRoundType.EndRoundTypeTimeOut && _resultSubType == RoundResultSubType.RESULT_SUBTYPE_TIMEOUT) || (endRoundType == EndRoundType.EndRoundTypeRingOut && _resultSubType == RoundResultSubType.RESULT_SUBTYPE_TIMEOUT) || (endRoundType == EndRoundType.EndRoundTypeLose && _resultSubType == RoundResultSubType.RESULT_SUBTYPE_TIMEOUT) || (endRoundType == EndRoundType.EndRoundTypeZeroHealth && _resultSubType == RoundResultSubType.RESULT_SUBTYPE_TIMEOUT);
	}
}
