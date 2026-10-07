using System.Collections.Generic;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Profile;
using UnityEngine;

public class TricksController : ITableViewDataSource, ITableViewDelegate
{
	private List<Trick> _tricks = new List<Trick>();

	private TableView _tableView;

	public TricksController(TableView tableView, GameObject cellPrefab)
	{
		_tableView = tableView;
		LoadTricks();
		tableView.set_CellPrefab(cellPrefab);
		tableView.Init(this, this);
	}

	public int NumberOfRowsInTableView(TableView tableView)
	{
		return _tricks.Count;
	}

	public float SizeForRowInTableView(TableView tableView, int row)
	{
		return 268f;
	}

	public TableViewCell CellForRowInTableView(TableView tableView, int row)
	{
		Trick trick = _tricks[row];
		TableViewCell tableViewCell = tableView.ReusableCellForRow(row);
		TrickCell component = tableViewCell.GetComponent<TrickCell>();
		component.Init(trick, row);
		return tableViewCell;
	}

	public void TableViewDidHighlightCellForRow(TableView tableView, int row)
	{
	}

	public void TableViewDidSelectCellForRow(TableView tableView, int row)
	{
		_tableView.ScrollToCell(row, 0.5f);
	}

	private void LoadTricks()
	{
		_tricks.Clear();
		_tricks = GameUtils.GetPlayerTricks(SceneTypes.SceneProfile);
		_tricks.Sort((Trick left, Trick right) => left.Rank.CompareTo(right.Rank));
	}

	public void Reload()
	{
		LoadTricks();
		_tableView.ReloadData();
	}

	public void SelectTrickByName(string trickName)
	{
		int num = -1;
		int i = 0;
		for (int count = _tricks.Count; i < count; i++)
		{
			string candidateName = _tricks[i].Name;
			if (candidateName == trickName)
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
