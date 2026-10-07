using System;

public class NetworkController
{
	public Action<object> OnLoginComplete = delegate
	{
	};

	private static NetworkController _Instance;

	private bool _isFirstLogin = true;

	public readonly GiveLogin GiveLoginService = new GiveLogin();

	public readonly LedgerManager Ledger = new LedgerManager();

	public static NetworkController Instance
	{
		get
		{
			return GetInstance();
		}
	}

	private NetworkController()
	{
	}

	public static NetworkController GetInstance()
	{
		if (_Instance == null)
		{
			_Instance = new NetworkController();
		}
		return _Instance;
	}

	public void CompleteLogin()
	{
		// Complete the local session without config fetches, cloud saves, news,
		// licensing, or a fake successful server login.
		if (_isFirstLogin)
		{
			ListSF.GetInstance().CreateMissingAchievements();
			_isFirstLogin = false;
		}
		ListSF.GetRoster().UpdateLastDumpTime();
		FinishLoginSequence();
	}

	private void FinishLoginSequence()
	{
		GameLog.Info("Login sequence: NetworkController.LoginComplete");
		GiveLoginService.SendGiveLogin();
		QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
		if (hHKLFIIBIFF.GetFightList() == null)
		{
			hHKLFIIBIFF.fightIds = FightIDS.Empty();
			hHKLFIIBIFF.fightResult = string.Empty;
		}
		if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_LOGIN_END))
		{
			ListSF.GetInstance().RunQuestActions();
		}
		OnLoginComplete(null);
		if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_SESSION))
		{
			ListSF.GetInstance().RunQuestActions();
		}
	}

}
