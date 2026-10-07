public class InitializationModule : LoadingModule
{
	public override void ProcessStep()
	{
		base.ProcessStep();
		if (!isFinished)
		{
			isFinished = true;
		}
	}
}
