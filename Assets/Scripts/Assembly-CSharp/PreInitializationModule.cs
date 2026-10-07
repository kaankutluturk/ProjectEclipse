using Nekki.SF2.Core;

public class PreInitializationModule : LoadingModule
{
	private static bool isInitialized;

	public override void ProcessStep()
	{
		if (!isFinished)
		{
			Init();
			isFinished = true;
		}
	}

	private static void Init()
	{
		if (!isInitialized)
		{
			isInitialized = true;
			ApplicationController.Init();
			RaidCheatManager.Init();
			SF2Paths.Init();
		}
	}
}
