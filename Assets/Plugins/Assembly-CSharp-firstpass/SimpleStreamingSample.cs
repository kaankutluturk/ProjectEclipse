using System;
using UnityEngine;

internal sealed class SimpleStreamingSample : MonoBehaviour
{
	private readonly Uri URI = new Uri("http://besthttpsignalr.azurewebsites.net/streaming-connection");

	private Connection signalRConnection;

	private GUIMessageList messages = new GUIMessageList();

	private void Start()
	{
		signalRConnection = new Connection(URI);
		signalRConnection.AddNonHubMessageHandler(OnNonHubMessage);
		signalRConnection.AddStateChangedHandler(OnStateChanged);
		signalRConnection.AddErrorHandler(OnError);
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
			GUILayout.Label("Messages");
			GUILayout.BeginHorizontal();
			GUILayout.Space(20f);
			messages.Draw(Screen.width - 20, 0f);
			GUILayout.EndHorizontal();
		});
	}

	private void OnNonHubMessage(Connection MDGFGCDPGFI, object data)
	{
		messages.Add("[Server Message] " + data.ToString());
	}

	private void OnStateChanged(Connection MDGFGCDPGFI, ConnectionStates JOBAGBFMMFP, ConnectionStates MPJEMGJIBBD)
	{
		messages.Add(string.Format("[State Change] {0} => {1}", JOBAGBFMMFP, MPJEMGJIBBD));
	}

	private void OnError(Connection MDGFGCDPGFI, string JDONBAPIJCG)
	{
		messages.Add("[Error] " + JDONBAPIJCG);
	}
}
