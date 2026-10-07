using System.Collections.Generic;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Profile;
using UnityEngine;

public class AchievementsController : ITableViewDataSource, ITableViewDelegate
{
	private TableView tableView;

	private List<global::Pair<Achievement, int>> displayedAchievements = new List<global::Pair<Achievement, int>>();

	public AchievementsController(TableView table, GameObject cellPrefab)
	{
		tableView = table;
		RebuildAchievementList();
		table.set_CellPrefab(cellPrefab);
		table.Init(this, this);
	}

	public int NumberOfRowsInTableView(TableView table)
	{
		return displayedAchievements.Count;
	}

	public float SizeForRowInTableView(TableView table, int row)
	{
		return 268f;
	}

	public TableViewCell CellForRowInTableView(TableView table, int row)
	{
		Achievement achievement = displayedAchievements[row].First;
		TableViewCell tableViewCell = table.ReusableCellForRow(row);
		AchievementCell component = tableViewCell.GetComponent<AchievementCell>();
		component.Init(achievement, displayedAchievements[row].Second, row);
		return tableViewCell;
	}

	public void TableViewDidHighlightCellForRow(TableView table, int row)
	{
	}

	public void TableViewDidSelectCellForRow(TableView table, int row)
	{
		tableView.ScrollToCell(row, 0.5f);
	}

	private void RebuildAchievementList()
	{
		displayedAchievements.Clear();
		List<AchievCounter> counterDefinitions = GameUtils.AchievementDefinitions.Counters;
		List<RosterAchievCounter> list = new List<RosterAchievCounter>(ListSF.GetRoster().GetAchievements().GetCounters());
		List<RosterAchievement> rosterAchievements = ListSF.GetRoster().GetAchievements().GetAchievements();
		for (int i = 0; i < counterDefinitions.Count; i++)
		{
			string counterName = counterDefinitions[i].Name;
			int counterValue = 0;
			List<Achievement> counterAchievements = new List<Achievement>(counterDefinitions[i].Achievements);
			for (int j = 0; j < list.Count; j++)
			{
				RosterAchievCounter rosterCounter = list[j];
				if (counterName == rosterCounter.get_Name())
				{
					counterValue = rosterCounter.GetCounter();
					list.RemoveAt(j);
					break;
				}
			}
			AddCounterAchievements(counterAchievements, rosterAchievements, counterValue);
		}
	}

	private void AddCounterAchievements(List<Achievement> counterAchievements, List<RosterAchievement> rosterAchievements, int counterValue)
	{
		for (int i = 0; i < rosterAchievements.Count; i++)
		{
			string text = rosterAchievements[i].get_Name();
			for (int j = 0; j < counterAchievements.Count; j++)
			{
				Achievement achievement = counterAchievements[j];
				if ((!achievement.IsHidden || achievement.IsUnlocked) && text == achievement.Name)
				{
					displayedAchievements.Add(new global::Pair<Achievement, int>(achievement, achievement.CounterValue));
					counterAchievements.RemoveAt(j);
					break;
				}
			}
		}
		for (int k = 0; k < counterAchievements.Count; k++)
		{
			if (!counterAchievements[k].IsHidden || counterAchievements[k].IsUnlocked)
			{
				displayedAchievements.Add(new global::Pair<Achievement, int>(counterAchievements[k], counterValue));
				if (counterValue < counterAchievements[k].CounterValue)
				{
					break;
				}
			}
		}
	}

	public bool HasNewAchievement()
	{
		for (int i = 0; i < displayedAchievements.Count; i++)
		{
			if (displayedAchievements[i].First.GetIsNew())
			{
				return true;
			}
		}
		return false;
	}

	public void ScrollToFirstNewAchievement(float _Duration = 0f)
	{
		int num = -1;
		for (int i = 0; i < displayedAchievements.Count; i++)
		{
			if (displayedAchievements[i].First.GetIsNew())
			{
				num = i;
				break;
			}
		}
		if (num >= 0 && num < displayedAchievements.Count)
		{
			tableView.ScrollToCell(num, _Duration);
		}
	}

	public void ScrollToAchievement(string achievementName)
	{
		int num = -1;
		int i = 0;
		for (int count = displayedAchievements.Count; i < count; i++)
		{
			string currentName = displayedAchievements[i].First.Name;
			if (currentName == achievementName)
			{
				num = i;
				break;
			}
		}
		if (num > -1)
		{
			tableView.ScrollToCell(num);
			ProfileCell profileCell = (ProfileCell)tableView.get_visibleCells().GetCellAtIndex(num);
			profileCell.GetFirstIcon().Choose();
		}
	}
}
