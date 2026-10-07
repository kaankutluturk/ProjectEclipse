using System.Xml;

public class ConditionKeys : ConditionAnimation
{
	// best guess for name
	public KeyData RequiredKeys = new KeyData();

	public KeyData ReversedKeys;

	public ConditionKeys(XmlNode node)
		: base(ConditionType.KEYS)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			string bAINMLLIKOL = childNode.Attributes["Type"].GetStringOrDefault(string.Empty);
			FightCID item = (FightCID)MovesMaps.GetMappedIndex(MovesMaps.MapType.KEY_TYPE, bAINMLLIKOL);
			switch (childNode.Attributes["PressType"].GetStringOrDefault(string.Empty))
			{
			case "Hold":
				RequiredKeys.AdditionalKeys.Add((int)item);
				break;
			case "Tap":
				RequiredKeys.StarterKeys.Add((int)item);
				break;
			case "Release":
				RequiredKeys.ReleaseKeys.Add((int)item);
				break;
			}
		}
		RequiredKeys.ResetPressType();
		ReversedKeys = new KeyData(RequiredKeys);
		ReversedKeys.Reverse(-1);
	}

	public override bool IsEqual(ModelConditions conditions)
	{
		KeyData oHGJEGDLEJK = ((!conditions.PressedKeys.IsInverted && conditions.AnimationSign <= 0) ? ReversedKeys : RequiredKeys);
		bool flag = !conditions.IsKeyCheckEnabled || oHGJEGDLEJK.IsVariable(conditions.PressedKeys);
		return (!IsNot) ? flag : (!flag);
	}

	public bool IsEqual(KeyData KDKEJHHKCDB, bool ANCFHGGJOJB)
	{
		bool flag = !ANCFHGGJOJB || RequiredKeys.IsVariable(KDKEJHHKCDB);
		return (!IsNot) ? flag : (!flag);
	}

	public bool HasSameKeyRequirementAs(ConditionKeys other)
	{
		// Native AI sets RequiredKeys.IsInverted while dispatching a selected
		// move. It is transient controller state, not part of the authored key.
		return other != null && IsNot == other.IsNot && TargetModelType == other.TargetModelType &&
			RequiredKeys.IsVariable(other.RequiredKeys) && other.RequiredKeys.IsVariable(RequiredKeys);
	}
}
