using System.Collections.Generic;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Shop;
using UnityEngine;

public class SealsController : ITableViewDataSource, ITableViewDelegate
{
	private const int ColumnCount = 4;

	private TableView tableView;

	private List<UserItem> sealItems = new List<UserItem>();

	public SealsController(TableView OIDFBEAABBA, GameObject CGLPIDAECLH)
	{
		tableView = OIDFBEAABBA;
		LoadSeals();
		OIDFBEAABBA.set_CellPrefab(CGLPIDAECLH);
		OIDFBEAABBA.Init(this, this);
	}

	public int NumberOfRowsInTableView(TableView OIDFBEAABBA)
	{
		return sealItems.Count;
	}

	public float SizeForRowInTableView(TableView OIDFBEAABBA, int IBAKGENOEPH)
	{
		return 632f;
	}

	public TableViewCell CellForRowInTableView(TableView OIDFBEAABBA, int IBAKGENOEPH)
	{
		TableViewCell tableViewCell = OIDFBEAABBA.ReusableCellForRow(IBAKGENOEPH);
		ShopTableViewCell component = tableViewCell.GetComponent<ShopTableViewCell>();
		component.set_BaseSize(Constants.SEAL_SIZE);
		component.set_IconPanelActive(false);
		ItemInfo itemInfo = sealItems[IBAKGENOEPH].GetInfo();
		component.SetItemInfo(itemInfo);
		return tableViewCell;
	}

	public void TableViewDidHighlightCellForRow(TableView OIDFBEAABBA, int IBAKGENOEPH)
	{
	}

	public void TableViewDidSelectCellForRow(TableView OIDFBEAABBA, int IBAKGENOEPH)
	{
		tableView.ScrollToCell(IBAKGENOEPH, 0.5f);
	}

	private void LoadSeals()
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		List<UserItem> list = nKGLHEGIKKP.GetInventory().FindItemsByType("Seal", string.Empty);
		foreach (UserItem item in list)
		{
			if (item.GetCount() != 0)
			{
				sealItems.Add(item);
			}
		}
	}

	public void ScrollToSeal(string EOIDIMBBLFB)
	{
		int num = -1;
		int i = 0;
		for (int count = sealItems.Count; i < count; i++)
		{
			string text = sealItems[i].get_Name();
			if (text == EOIDIMBBLFB)
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
