using System.Collections.Generic;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Profile;
using UnityEngine;

public class PerksController : ITableViewDataSource, ITableViewDelegate
{
	private List<ProfilePerkContainer> perkContainers = new List<ProfilePerkContainer>();

	private TableView tableView;

	public PerksController(TableView table, GameObject cellPrefab)
	{
		tableView = table;
		LoadContainers();
		table.set_CellPrefab(cellPrefab);
		table.Init(this, this);
	}

	public int NumberOfRowsInTableView(TableView tableView)
	{
		return perkContainers.Count;
	}

	public float SizeForRowInTableView(TableView tableView, int row)
	{
		return 270f;
	}

	public TableViewCell CellForRowInTableView(TableView tableView, int row)
	{
		TableViewCell tableViewCell = tableView.ReusableCellForRow(row);
		PerkCell component = tableViewCell.GetComponent<PerkCell>();
		component.RemoveEventListener(0, OnScrollToCellRequested);
		component.AddEventListener(0, OnScrollToCellRequested);
		ProfilePerkContainer container = perkContainers[row];
		bool isFirst = row == 0;
		bool isLast = row + 1 == perkContainers.Count;
		component.Init(container, row, isFirst, isLast);
		return tableViewCell;
	}

	public void TableViewDidHighlightCellForRow(TableView tableView, int row)
	{
	}

	public void TableViewDidSelectCellForRow(TableView tableView, int row)
	{
	}

	private void LoadContainers()
	{
		perkContainers.Clear();
		perkContainers = PerkTree.GetInstance().GetLevelContainers();
	}

	public void RefreshCell(int row)
	{
		TableViewCell tableViewCell = tableView.get_visibleCells().GetCellAtIndex(row);
		PerkCell component = tableViewCell.GetComponent<PerkCell>();
		ProfilePerkContainer container = perkContainers[row];
		bool isFirst = row == 0;
		bool isLast = row + 1 == perkContainers.Count;
		component.Init(container, row, isFirst, isLast);
	}

	public void ScrollToPerk(string name)
	{
		List<ProfilePerk> list = PerkTree.GetInstance().GetProfilePerks();
		int num = -1;
		foreach (ProfilePerk item in list)
		{
			if (name == item.GetPerkName())
			{
				num = item.GetLevel() - 2;
				break;
			}
		}
		if (num > -1)
		{
			tableView.ScrollToCell(num);
			PerkCell perkCell = (PerkCell)tableView.get_visibleCells().GetCellAtIndex(num);
			perkCell.ChoosePerkByName(name);
		}
	}

	public void OnScrollToCellRequested(object data)
	{
		int cellIndex = (int)data;
		tableView.ScrollToCell(cellIndex, 0.5f);
	}
}
