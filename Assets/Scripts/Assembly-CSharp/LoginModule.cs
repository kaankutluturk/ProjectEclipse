using System;

public class LoginModule : LoadingModule
{
	private bool loginStarted;

	public override void Start()
	{
		base.Start();
		loginStarted = false;
	}

	public override void ProcessStep()
	{
		if (!isFinished && !loginStarted)
		{
			if (Eclipse.Multiplayer.LocalVersusSession.IsActive)
			{
				loginStarted = isFinished = true;
				Eclipse.Multiplayer.LocalVersusSession.DataReady();
				return;
			}
			GameUtils.NotifyApplicationStart();
			NetworkController fDJHFPIFMIK = NetworkController.GetInstance();
			fDJHFPIFMIK.OnLoginComplete = (Action<object>)Delegate.Combine(fDJHFPIFMIK.OnLoginComplete, new Action<object>(OnLoginComplete));
			ListSF.GetInstance().RestartServerAuthorization();
			loginStarted = true;
		}
	}

	private void OnLoginComplete(object data)
	{
		NetworkController fDJHFPIFMIK = NetworkController.GetInstance();
		fDJHFPIFMIK.OnLoginComplete = (Action<object>)Delegate.Remove(fDJHFPIFMIK.OnLoginComplete, new Action<object>(OnLoginComplete));
		GameUtils.IsLoginComplete = true;
		isFinished = true;
	}
}
