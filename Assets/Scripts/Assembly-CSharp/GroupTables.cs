using System.Collections.Generic;

public class GroupTables
{
	public string GroupLabel;

	public List<TacticalTable> Tables;

	public TacticalTable GetTacticalTableByLabel(string label)
	{
		for (int i = 0; i < Tables.Count; i++)
		{
			if (Tables[i].Label == label)
			{
				return Tables[i];
			}
		}
		GameLog.Error("table for label {0} not found", label);
		return null;
	}
}
