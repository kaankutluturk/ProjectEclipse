using Nekki.SF2.GUI.Menu;

public static class MenuController
{
	public static void RefreshMenu()
	{
		MainMenu instance = MainMenu.get_Instance();
		if (instance != null)
		{
			instance.UpdateMenu();
		}
	}

	public static void RefreshMoney()
	{
		MainMenu instance = MainMenu.get_Instance();
		if (instance != null)
		{
			instance.UpdateMoney();
		}
	}

	public static void RefreshEnergyBar()
	{
		MainMenu instance = MainMenu.get_Instance();
		if (instance != null)
		{
			instance.UpdateBarEnergy();
		}
	}

	public static void RefreshEnergyView()
	{
		MainMenu instance = MainMenu.get_Instance();
		if (instance != null)
		{
			instance.UpdateEnergyView();
		}
	}

	public static void RefreshRubySale()
	{
		MainMenu instance = MainMenu.get_Instance();
		if (instance != null)
		{
			instance.UpdateRubySale();
		}
	}

	public static void SetNormalViewMode(bool isNormalView)
	{
		MainMenu instance = MainMenu.get_Instance();
		if (instance != null)
		{
			instance.SetNormalViewMode(isNormalView);
		}
	}

	public static void RecreateMoney()
	{
		MainMenu instance = MainMenu.get_Instance();
		if (instance != null)
		{
			instance.RecreateMoney();
		}
	}
}
