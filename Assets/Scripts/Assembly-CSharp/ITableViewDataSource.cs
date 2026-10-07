using Nekki.SF2.GUI;

public interface ITableViewDataSource
{
	int NumberOfRowsInTableView(TableView tableView);

	float SizeForRowInTableView(TableView tableView, int row);

	TableViewCell CellForRowInTableView(TableView tableView, int row);
}
