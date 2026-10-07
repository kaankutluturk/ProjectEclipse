using System.Collections.Generic;
using Nekki.SF2.GUI;

public class ReusableCellsContainer
{
	public LinkedList<TableViewCell> cells;

	public void Init()
	{
		cells = new LinkedList<TableViewCell>();
	}

	public void RecycleCell(TableViewCell HJCPCBLCJJN)
	{
		cells.AddLast(HJCPCBLCJJN);
		HJCPCBLCJJN.gameObject.SetActive(false);
	}

	public TableViewCell TakeCell()
	{
		if (cells.Count == 0)
		{
			return null;
		}
		TableViewCell value = cells.First.Value;
		value.gameObject.SetActive(true);
		cells.RemoveFirst();
		return value;
	}
}
