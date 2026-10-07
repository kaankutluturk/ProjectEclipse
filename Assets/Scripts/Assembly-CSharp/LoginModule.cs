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
			NetworkController networkController = NetworkController.GetInstance();
			networkController.OnLoginComplete = (Action<object>)Delegate.Combine(networkController.OnLoginComplete, new Action<object>(OnLoginComplete));
			ListSF.GetInstance().RestartServerAuthorization();
			loginStarted = true;
		}
	}

	private void OnLoginComplete(object data)
	{
		NetworkController networkController = NetworkController.GetInstance();
		networkController.OnLoginComplete = (Action<object>)Delegate.Remove(networkController.OnLoginComplete, new Action<object>(OnLoginComplete));
		GameUtils.IsLoginComplete = true;
		isFinished = true;
	}
}
