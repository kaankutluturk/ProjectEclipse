using System.Collections.Generic;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Shop;
using UnityEngine;

public class SealsController : ITableViewDataSource, ITableViewDelegate
{
	private const int ColumnCount = 4;

	private TableView tableView;

	private List<UserItem> sealItems = new List<UserItem>();

	public SealsController(TableView table, GameObject cellPrefab)
	{
		tableView = table;
		LoadSeals();
		table.set_CellPrefab(cellPrefab);
		table.Init(this, this);
	}

	public int NumberOfRowsInTableView(TableView table)
	{
		return sealItems.Count;
	}

	public float SizeForRowInTableView(TableView table, int row)
	{
		return 632f;
	}

	public TableViewCell CellForRowInTableView(TableView table, int row)
	{
		TableViewCell tableViewCell = table.ReusableCellForRow(row);
		ShopTableViewCell component = tableViewCell.GetComponent<ShopTableViewCell>();
		component.set_BaseSize(Constants.SEAL_SIZE);
		component.set_IconPanelActive(false);
		ItemInfo itemInfo = sealItems[row].GetInfo();
		component.SetItemInfo(itemInfo);
		return tableViewCell;
	}

	public void TableViewDidHighlightCellForRow(TableView table, int row)
	{
	}

	public void TableViewDidSelectCellForRow(TableView table, int row)
	{
		tableView.ScrollToCell(row, 0.5f);
	}

	private void LoadSeals()
	{
		Roster roster = ListSF.GetRoster();
		List<UserItem> list = roster.GetInventory().FindItemsByType("Seal", string.Empty);
		foreach (UserItem item in list)
		{
			if (item.GetCount() != 0)
			{
				sealItems.Add(item);
			}
		}
	}

	public void ScrollToSeal(string sealName)
	{
		int num = -1;
		int i = 0;
		for (int count = sealItems.Count; i < count; i++)
		{
			string text = sealItems[i].get_Name();
			if (text == sealName)
			{
				num = i;
				break;
			}
		}
		if (num > -1)
		{
			tableView.ScrollToCell(num);
		}
	}
}
