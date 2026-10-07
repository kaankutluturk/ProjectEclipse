public class AttachFileModule : LoadingModule
{
	public override void ProcessStep()
	{
		if (!isFinished)
		{
			GameLoader.LoadSettings();
			isFinished = true;
		}
	}
}
