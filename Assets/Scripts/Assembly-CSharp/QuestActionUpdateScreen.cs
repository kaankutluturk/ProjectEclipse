public class QuestActionUpdateScreen : QuestAction
{
	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		Module module = Module.GetInstance();
		ScreenType currentScreen = module.GetCurrentScreenType();
		// UpdateScreen can be resumed from the save before the first real module
		// has been selected. ModuleNone is a sentinel (enum value 8), not a scene
		// build index; attempting to reload it strands the Loader scene.
		if (currentScreen == ScreenType.ModuleNone)
		{
			UnityEngine.Debug.LogWarning("[Quest] Ignoring UpdateScreen before a screen is initialized.");
			FinishAction();
			return;
		}
		module.AddEventListener(1, OnModuleChanged);
		if (!Module.OpenScreen(currentScreen))
		{
			module.RemoveEventListener(1, OnModuleChanged);
			FinishAction();
		}
	}

	private void OnModuleChanged(object data)
	{
		Module.GetInstance().RemoveEventListener(1, OnModuleChanged);
		FinishAction();
	}
}
