using Nekki.SF2.GUI.Shop;

public class QuestActionUpgrades : QuestAction
{
	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		Roster roster = ListSF.GetRoster();
		roster.SetShowUpgrades(true);
		ShopScene instance = ShopScene.get_Instance();
		if (instance != null)
		{
			instance.UpdateScene(null);
		}
		FinishAction();
	}
}
