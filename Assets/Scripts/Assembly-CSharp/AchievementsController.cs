using System.Collections.Generic;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Profile;
using UnityEngine;

public class AchievementsController : ITableViewDataSource, ITableViewDelegate
{
	private TableView tableView;

	private List<global::Pair<Achievement, int>> displayedAchievements = new List<global::Pair<Achievement, int>>();

	public AchievementsController(TableView OIDFBEAABBA, GameObject CGLPIDAECLH)
	{
		tableView = OIDFBEAABBA;
		RebuildAchievementList();
		OIDFBEAABBA.set_CellPrefab(CGLPIDAECLH);
		OIDFBEAABBA.Init(this, this);
	}

	public int NumberOfRowsInTableView(TableView OIDFBEAABBA)
	{
		return displayedAchievements.Count;
	}

	public float SizeForRowInTableView(TableView OIDFBEAABBA, int IBAKGENOEPH)
	{
		return 268f;
	}

	public TableViewCell CellForRowInTableView(TableView OIDFBEAABBA, int IBAKGENOEPH)
	{
		Achievement lLHEDBIEHAA = displayedAchievements[IBAKGENOEPH].First;
		TableViewCell tableViewCell = OIDFBEAABBA.ReusableCellForRow(IBAKGENOEPH);
		AchievementCell component = tableViewCell.GetComponent<AchievementCell>();
		component.Init(lLHEDBIEHAA, displayedAchievements[IBAKGENOEPH].Second, IBAKGENOEPH);
		return tableViewCell;
	}

	public void TableViewDidHighlightCellForRow(TableView OIDFBEAABBA, int IBAKGENOEPH)
	{
	}

	public void TableViewDidSelectCellForRow(TableView OIDFBEAABBA, int IBAKGENOEPH)
	{
		tableView.ScrollToCell(IBAKGENOEPH, 0.5f);
	}

	private void RebuildAchievementList()
	{
		displayedAchievements.Clear();
		List<AchievCounter> mDNKEAFGAOB = GameUtils.AchievementDefinitions.Counters;
		List<RosterAchievCounter> list = new List<RosterAchievCounter>(ListSF.GetRoster().GetAchievements().GetCounters());
		List<RosterAchievement> eOJAMHMPKAJ = ListSF.GetRoster().GetAchievements().GetAchievements();
		for (int i = 0; i < mDNKEAFGAOB.Count; i++)
		{
			string mENAJEAJJBE = mDNKEAFGAOB[i].Name;
			int ePJGLECOIBG = 0;
			List<Achievement> pGAGNLJABIE = new List<Achievement>(mDNKEAFGAOB[i].Achievements);
			for (int j = 0; j < list.Count; j++)
			{
				RosterAchievCounter cKJBHGKBPPM = list[j];
				if (mENAJEAJJBE == cKJBHGKBPPM.get_Name())
				{
					ePJGLECOIBG = cKJBHGKBPPM.GetCounter();
					list.RemoveAt(j);
					break;
				}
			}
			AddCounterAchievements(pGAGNLJABIE, eOJAMHMPKAJ, ePJGLECOIBG);
		}
	}

	private void AddCounterAchievements(List<Achievement> PGAGNLJABIE, List<RosterAchievement> EOJAMHMPKAJ, int EPJGLECOIBG)
	{
		for (int i = 0; i < EOJAMHMPKAJ.Count; i++)
		{
			string text = EOJAMHMPKAJ[i].get_Name();
			for (int j = 0; j < PGAGNLJABIE.Count; j++)
			{
				Achievement jNPIOKEKMII = PGAGNLJABIE[j];
				if ((!jNPIOKEKMII.IsHidden || jNPIOKEKMII.IsUnlocked) && text == jNPIOKEKMII.Name)
				{
					displayedAchievements.Add(new global::Pair<Achievement, int>(jNPIOKEKMII, jNPIOKEKMII.CounterValue));
					PGAGNLJABIE.RemoveAt(j);
					break;
				}
			}
		}
		for (int k = 0; k < PGAGNLJABIE.Count; k++)
		{
			if (!PGAGNLJABIE[k].IsHidden || PGAGNLJABIE[k].IsUnlocked)
			{
				displayedAchievements.Add(new global::Pair<Achievement, int>(PGAGNLJABIE[k], EPJGLECOIBG));
				if (EPJGLECOIBG < PGAGNLJABIE[k].CounterValue)
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

	public void ScrollToAchievement(string OGPJPGMBIHJ)
	{
		int num = -1;
		int i = 0;
		for (int count = displayedAchievements.Count; i < count; i++)
		{
			string mENAJEAJJBE = displayedAchievements[i].First.Name;
			if (mENAJEAJJBE == OGPJPGMBIHJ)
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
