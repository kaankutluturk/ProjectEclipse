using Nekki.SF2.GUI.Shop;

public class QuestActionUpgrades : QuestAction
{
	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		nKGLHEGIKKP.SetShowUpgrades(true);
		ShopScene instance = ShopScene.get_Instance();
		if (instance != null)
		{
			instance.UpdateScene(null);
		}
		FinishAction();
	}
}
