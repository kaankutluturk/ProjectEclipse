public class ParseModule : LoadingModule
{
	public override void ProcessStep()
	{
		if (!isFinished)
		{
			GameUtils.InitVariables();
			bool reusedTitleContent = Eclipse.UI.TitleScreen.TryResumeGameDataPreview();
			if (!reusedTitleContent)
			{
				GameSettings.LoadAllSettings();
				GameLoader.LoadAnimations();
				GameLoader.LoadAi();
				ListSF.GetInstance().LoadGameContent();
			}
			PerkTree.GetInstance().RebuildProfile();
			GameSettings.ApplyQualityOptions();
			if (!reusedTitleContent)
			{
				GameLoader.SetSound();
				LocalizationManager.Init();
			}
			Eclipse.Modding.ModRuntime.ApplyLocaleMetadata();
			Eclipse.Modding.ModRuntime.ApplyLegacyLocalization();
			ListSF.GetRoster().ApplyLanguage();
			GameUtils.ScheduleStartupNotifications();
			isFinished = true;
		}
	}
}
