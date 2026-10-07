using System;
using UnityEngine;

internal sealed class ConnectionStatusSample : MonoBehaviour
{
	private readonly Uri URI = new Uri("http://besthttpsignalr.azurewebsites.net/signalr");

	private Connection signalRConnection;

	private GUIMessageList messages = new GUIMessageList();

	private void Start()
	{
		signalRConnection = new Connection(URI, "StatusHub");
		signalRConnection.AddNonHubMessageHandler(OnNonHubMessage);
		signalRConnection.AddErrorHandler(OnError);
		signalRConnection.AddStateChangedHandler(OnStateChanged);
		signalRConnection.get_Item("StatusHub").AddOnMethodCall(OnStatusHubMethod);
		signalRConnection.OpenConnection();
	}

	private void OnDestroy()
	{
		signalRConnection.Close();
	}

	private void OnGUI()
	{
		GUIHelper.DrawArea(GUIHelper.ClientArea, true, () =>
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("START") && signalRConnection.GetState() != ConnectionStates.Connected)
			{
				signalRConnection.OpenConnection();
			}
			if (GUILayout.Button("STOP") && signalRConnection.GetState() == ConnectionStates.Connected)
			{
				signalRConnection.Close();
				messages.Clear();
			}
			if (GUILayout.Button("PING") && signalRConnection.GetState() == ConnectionStates.Connected)
			{
				signalRConnection.get_Item("StatusHub").Call("Ping");
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(20f);
			GUILayout.Label("Connection Status Messages");
			GUILayout.BeginHorizontal();
			GUILayout.Space(20f);
			messages.Draw(Screen.width - 20, 0f);
			GUILayout.EndHorizontal();
		});
	}

	private void OnNonHubMessage(Connection connection, object data)
	{
		messages.Add("[Server Message] " + data.ToString());
	}

	private void OnStateChanged(Connection connection, ConnectionStates oldState, ConnectionStates newState)
	{
		messages.Add(string.Format("[State Change] {0} => {1}", oldState, newState));
	}

	private void OnError(Connection connection, string error)
	{
		messages.Add("[Error] " + error);
	}

	private void OnStatusHubMethod(Hub hub, string method, params object[] args)
	{
		string arg = ((args.Length <= 0) ? string.Empty : (args[0] as string));
		string arg2 = ((args.Length <= 1) ? string.Empty : args[1].ToString());
		switch (method)
		{
		case "joined":
			messages.Add(string.Format("[{0}] {1} joined at {2}", hub.get_Name(), arg, arg2));
			break;
		case "rejoined":
			messages.Add(string.Format("[{0}] {1} reconnected at {2}", hub.get_Name(), arg, arg2));
			break;
		case "leave":
			messages.Add(string.Format("[{0}] {1} leaved at {2}", hub.get_Name(), arg, arg2));
			break;
		default:
			messages.Add(string.Format("[{0}] {1}", hub.get_Name(), method));
			break;
		}
	}
}
