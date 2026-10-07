using Nekki.SF2.GUI;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Range = UnityEngine.SocialPlatforms.Range;

public interface ITableView
{
	ITableViewDataSource TableDataSource { get; set; }

	ITableViewDelegate Delegate { get; set; }

	GameObject RowCellPrefab { get; set; }

	Range VisibleRowRange { get; }

	float TotalContentSize { get; }

	float ScrollPosition { get; }

	ITableViewDataSource get_DataSource();

	void set_DataSource(ITableViewDataSource value);



	GameObject get_CellPrefab();

	void set_CellPrefab(GameObject value);

	Range get_VisibleRange();

	float get_ContentSize();

	float get_Position();

	TableViewCell ReusableCellForRow(int row);

	TableViewCell CellForRow(int row);

	float PositionForRow(int row);

	void ReloadData();

	void SetPosition(float FFMJGKPCBNK, float time);
}
