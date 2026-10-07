using System.Collections.Generic;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Profile;
using UnityEngine;

public class TricksController : ITableViewDataSource, ITableViewDelegate
{
	private List<Trick> _tricks = new List<Trick>();

	private TableView _tableView;

	public TricksController(TableView OIDFBEAABBA, GameObject CGLPIDAECLH)
	{
		_tableView = OIDFBEAABBA;
		LoadTricks();
		OIDFBEAABBA.set_CellPrefab(CGLPIDAECLH);
		OIDFBEAABBA.Init(this, this);
	}

	public int NumberOfRowsInTableView(TableView OIDFBEAABBA)
	{
		return _tricks.Count;
	}

	public float SizeForRowInTableView(TableView OIDFBEAABBA, int IBAKGENOEPH)
	{
		return 268f;
	}

	public TableViewCell CellForRowInTableView(TableView OIDFBEAABBA, int IBAKGENOEPH)
	{
		Trick kPKPFFGEFGI = _tricks[IBAKGENOEPH];
		TableViewCell tableViewCell = OIDFBEAABBA.ReusableCellForRow(IBAKGENOEPH);
		TrickCell component = tableViewCell.GetComponent<TrickCell>();
		component.Init(kPKPFFGEFGI, IBAKGENOEPH);
		return tableViewCell;
	}

	public void TableViewDidHighlightCellForRow(TableView OIDFBEAABBA, int IBAKGENOEPH)
	{
	}

	public void TableViewDidSelectCellForRow(TableView OIDFBEAABBA, int IBAKGENOEPH)
	{
		_tableView.ScrollToCell(IBAKGENOEPH, 0.5f);
	}

	private void LoadTricks()
	{
		_tricks.Clear();
		_tricks = GameUtils.GetPlayerTricks(SceneTypes.SceneProfile);
		_tricks.Sort((Trick KOOLDHKJHNH, Trick MHFCMOONCHB) => KOOLDHKJHNH.Rank.CompareTo(MHFCMOONCHB.Rank));
	}

	public void Reload()
	{
		LoadTricks();
		_tableView.ReloadData();
	}

	public void SelectTrickByName(string JGEKHJIHNMF)
	{
		int num = -1;
		int i = 0;
		for (int count = _tricks.Count; i < count; i++)
		{
			string mENAJEAJJBE = _tricks[i].Name;
			if (mENAJEAJJBE == JGEKHJIHNMF)
			{
				num = i;
				break;
			}
		}
		if (num > -1)
		{
			_tableView.ScrollToCell(num);
			ProfileCell profileCell = (ProfileCell)_tableView.get_visibleCells().GetCellAtIndex(num);
			profileCell.GetFirstIcon().Choose();
		}
	}

	public void ScrollToNewTrick()
	{
		List<string> list = ListSF.GetRoster().GetOpenTricks();
		bool flag = false;
		int num = -1;
		for (int i = 0; i < _tricks.Count; i++)
		{
			for (int j = 0; j < list.Count; j++)
			{
				if (_tricks[i].Name == list[j])
				{
					num = i;
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		if (num >= 0 && num < _tricks.Count)
		{
			_tableView.ScrollToCell(num, 0.5f);
		}
	}
}
