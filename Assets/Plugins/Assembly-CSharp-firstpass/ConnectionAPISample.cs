using System;
using UnityEngine;

public sealed class ConnectionAPISample : MonoBehaviour
{
	private enum MessageType
	{
		Send = 0,
		Broadcast = 1,
		Join = 2,
		PrivateMessage = 3,
		AddToGroup = 4,
		RemoveFromGroup = 5,
		SendToGroup = 6,
		BroadcastExceptMe = 7
	}

	private readonly Uri URI = new Uri("http://besthttpsignalr.azurewebsites.net/raw-connection/");

	private Connection signalRConnection;

	private string toEverybodyText = string.Empty;

	private string toMeText = string.Empty;

	private string privateMessageText = string.Empty;

	private string privateMessageUserOrGroupName = string.Empty;

	private GUIMessageList messages = new GUIMessageList();

	private void Start()
	{
		if (PlayerPrefs.HasKey("userName"))
		{
			CookieJar.Set(URI, new Cookie("user", PlayerPrefs.GetString("userName")));
		}
		signalRConnection = new Connection(URI);
		signalRConnection.SetJsonEncoder(new LitJsonEncoder());
		signalRConnection.AddStateChangedHandler(OnStateChanged);
		signalRConnection.AddNonHubMessageHandler(OnNonHubMessage);
		signalRConnection.OpenConnection();
	}

	private void OnGUI()
	{
		GUIHelper.DrawArea(GUIHelper.ClientArea, true, () =>
		{
			GUILayout.BeginVertical();
			GUILayout.Label("To Everybody");
			GUILayout.BeginHorizontal();
			toEverybodyText = GUILayout.TextField(toEverybodyText, GUILayout.MinWidth(100f));
			if (GUILayout.Button("Broadcast"))
			{
				Broadcast(toEverybodyText);
			}
			if (GUILayout.Button("Broadcast (All Except Me)"))
			{
				BroadcastExceptMe(toEverybodyText);
			}
			if (GUILayout.Button("Enter Name"))
			{
				EnterName(toEverybodyText);
			}
			if (GUILayout.Button("Join Group"))
			{
				JoinGroup(toEverybodyText);
			}
			if (GUILayout.Button("Leave Group"))
			{
				LeaveGroup(toEverybodyText);
			}
			GUILayout.EndHorizontal();
			GUILayout.Label("To Me");
			GUILayout.BeginHorizontal();
			toMeText = GUILayout.TextField(toMeText, GUILayout.MinWidth(100f));
			if (GUILayout.Button("Send to me"))
			{
				SendToMe(toMeText);
			}
			GUILayout.EndHorizontal();
			GUILayout.Label("Private Message");
			GUILayout.BeginHorizontal();
			GUILayout.Label("Message:");
			privateMessageText = GUILayout.TextField(privateMessageText, GUILayout.MinWidth(100f));
			GUILayout.Label("User or Group name:");
			privateMessageUserOrGroupName = GUILayout.TextField(privateMessageUserOrGroupName, GUILayout.MinWidth(100f));
			if (GUILayout.Button("Send to user"))
			{
				SendToUser(privateMessageUserOrGroupName, privateMessageText);
			}
			if (GUILayout.Button("Send to group"))
			{
				SendToGroup(privateMessageUserOrGroupName, privateMessageText);
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(20f);
			if (signalRConnection.GetState() == ConnectionStates.Closed)
			{
				if (GUILayout.Button("Start Connection"))
				{
					signalRConnection.OpenConnection();
				}
			}
			else if (GUILayout.Button("Stop Connection"))
			{
				signalRConnection.Close();
			}
			GUILayout.Space(20f);
			GUILayout.Label("Messages");
			GUILayout.BeginHorizontal();
			GUILayout.Space(20f);
			messages.Draw(Screen.width - 20, 0f);
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		});
	}

	private void OnDestroy()
	{
		signalRConnection.Close();
	}

	private void OnNonHubMessage(Connection BJGMPDIKEJC, object data)
	{
		string text = Json.Encode(data);
		messages.Add("[Server Message] " + text);
	}

	private void OnStateChanged(Connection BJGMPDIKEJC, ConnectionStates JOBAGBFMMFP, ConnectionStates MPJEMGJIBBD)
	{
		messages.Add(string.Format("[State Change] {0} => {1}", JOBAGBFMMFP.ToString(), MPJEMGJIBBD.ToString()));
	}

	private void Broadcast(string HCPNFPMHFCM)
	{
		signalRConnection.Send(new
		{
			Type = MessageType.Broadcast,
			Value = HCPNFPMHFCM
		});
	}

	private void BroadcastExceptMe(string HCPNFPMHFCM)
	{
		signalRConnection.Send(new
		{
			Type = MessageType.BroadcastExceptMe,
			Value = HCPNFPMHFCM
		});
	}

	private void EnterName(string name)
	{
		signalRConnection.Send(new
		{
			Type = MessageType.Join,
			Value = name
		});
	}

	private void JoinGroup(string LKLJOLILPCJ)
	{
		signalRConnection.Send(new
		{
			Type = MessageType.AddToGroup,
			Value = LKLJOLILPCJ
		});
	}

	private void LeaveGroup(string LKLJOLILPCJ)
	{
		signalRConnection.Send(new
		{
			Type = MessageType.RemoveFromGroup,
			Value = LKLJOLILPCJ
		});
	}

	private void SendToMe(string HCPNFPMHFCM)
	{
		signalRConnection.Send(new
		{
			Type = MessageType.Send,
			Value = HCPNFPMHFCM
		});
	}

	private void SendToUser(string HIMLMCMHHGJ, string HCPNFPMHFCM)
	{
		signalRConnection.Send(new
		{
			Type = MessageType.PrivateMessage,
			Value = string.Format("{0}|{1}", HIMLMCMHHGJ, HCPNFPMHFCM)
		});
	}

	private void SendToGroup(string HIMLMCMHHGJ, string HCPNFPMHFCM)
	{
		signalRConnection.Send(new
		{
			Type = MessageType.SendToGroup,
			Value = string.Format("{0}|{1}", HIMLMCMHHGJ, HCPNFPMHFCM)
		});
	}
}
