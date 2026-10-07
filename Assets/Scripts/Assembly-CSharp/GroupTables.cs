using System.Collections.Generic;

public class GroupTables
{
	public string GroupLabel;

	public List<TacticalTable> Tables;

	public TacticalTable GetTacticalTableByLabel(string ICBBNJMLDJH)
	{
		for (int i = 0; i < Tables.Count; i++)
		{
			if (Tables[i].Label == ICBBNJMLDJH)
			{
				return Tables[i];
			}
		}
		GameLog.Error("table for label {0} not found", ICBBNJMLDJH);
		return null;
	}
}
