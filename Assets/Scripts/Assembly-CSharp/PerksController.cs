using System.Collections.Generic;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Profile;
using UnityEngine;

public class PerksController : ITableViewDataSource, ITableViewDelegate
{
	private List<ProfilePerkContainer> perkContainers = new List<ProfilePerkContainer>();

	private TableView tableView;

	public PerksController(TableView OIDFBEAABBA, GameObject CGLPIDAECLH)
	{
		tableView = OIDFBEAABBA;
		LoadContainers();
		OIDFBEAABBA.set_CellPrefab(CGLPIDAECLH);
		OIDFBEAABBA.Init(this, this);
	}

	public int NumberOfRowsInTableView(TableView OIDFBEAABBA)
	{
		return perkContainers.Count;
	}

	public float SizeForRowInTableView(TableView OIDFBEAABBA, int IBAKGENOEPH)
	{
		return 270f;
	}

	public TableViewCell CellForRowInTableView(TableView OIDFBEAABBA, int BIPGPCAHKIG)
	{
		TableViewCell tableViewCell = OIDFBEAABBA.ReusableCellForRow(BIPGPCAHKIG);
		PerkCell component = tableViewCell.GetComponent<PerkCell>();
		component.RemoveEventListener(0, OnScrollToCellRequested);
		component.AddEventListener(0, OnScrollToCellRequested);
		ProfilePerkContainer iFIEEAGMMMF = perkContainers[BIPGPCAHKIG];
		bool nMBEADHHHFH = BIPGPCAHKIG == 0;
		bool iBMGAPMHMOB = BIPGPCAHKIG + 1 == perkContainers.Count;
		component.Init(iFIEEAGMMMF, BIPGPCAHKIG, nMBEADHHHFH, iBMGAPMHMOB);
		return tableViewCell;
	}

	public void TableViewDidHighlightCellForRow(TableView OIDFBEAABBA, int IBAKGENOEPH)
	{
	}

	public void TableViewDidSelectCellForRow(TableView OIDFBEAABBA, int IBAKGENOEPH)
	{
	}

	private void LoadContainers()
	{
		perkContainers.Clear();
		perkContainers = PerkTree.GetInstance().GetLevelContainers();
	}

	public void RefreshCell(int BIPGPCAHKIG)
	{
		TableViewCell tableViewCell = tableView.get_visibleCells().GetCellAtIndex(BIPGPCAHKIG);
		PerkCell component = tableViewCell.GetComponent<PerkCell>();
		ProfilePerkContainer iFIEEAGMMMF = perkContainers[BIPGPCAHKIG];
		bool nMBEADHHHFH = BIPGPCAHKIG == 0;
		bool iBMGAPMHMOB = BIPGPCAHKIG + 1 == perkContainers.Count;
		component.Init(iFIEEAGMMMF, BIPGPCAHKIG, nMBEADHHHFH, iBMGAPMHMOB);
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
		int iBAKGENOEPH = (int)data;
		tableView.ScrollToCell(iBAKGENOEPH, 0.5f);
	}
}
