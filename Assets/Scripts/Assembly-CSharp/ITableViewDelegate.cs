using Nekki.SF2.GUI;

public interface ITableViewDelegate
{
	void TableViewDidHighlightCellForRow(TableView tableView, int row);

	void TableViewDidSelectCellForRow(TableView tableView, int row);
}
