using System;
using UnityEngine;

internal class AuthenticationSample : MonoBehaviour
{
	private readonly Uri URI = new Uri("https://besthttpsignalr.azurewebsites.net/signalr");

	private Connection signalRConnection;

	private string userName = string.Empty;

	private string role = string.Empty;

	private Vector2 scrollPos;

	private void Start()
	{
		signalRConnection = new Connection(URI, new BaseHub("noauthhub", "Messages"), new BaseHub("invokeauthhub", "Messages Invoked By Admin or Invoker"), new BaseHub("authhub", "Messages Requiring Authentication to Send or Receive"), new BaseHub("inheritauthhub", "Messages Requiring Authentication to Send or Receive Because of Inheritance"), new BaseHub("incomingauthhub", "Messages Requiring Authentication to Send"), new BaseHub("adminauthhub", "Messages Requiring Admin Membership to Send or Receive"), new BaseHub("userandroleauthhub", "Messages Requiring Name to be \"User\" and Role to be \"Admin\" to Send or Receive"));
		if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(role))
		{
			signalRConnection.SetAuthenticationProvider(new HeaderAuthenticator(userName, role));
		}
		signalRConnection.AddConnectedHandler(OnSignalRConnected);
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
			scrollPos = GUILayout.BeginScrollView(scrollPos, false, false);
			GUILayout.BeginVertical();
			if (signalRConnection.GetAuthenticationProvider() == null)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Username (Enter 'User'):");
				userName = GUILayout.TextField(userName, GUILayout.MinWidth(100f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Roles (Enter 'Invoker' or 'Admin'):");
				role = GUILayout.TextField(role, GUILayout.MinWidth(100f));
				GUILayout.EndHorizontal();
				if (GUILayout.Button("Log in"))
				{
					Restart();
				}
			}
			for (int i = 0; i < signalRConnection.GetHubs().Length; i++)
			{
				(signalRConnection.GetHubs()[i] as BaseHub).Draw();
			}
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
		});
	}

	private void OnSignalRConnected(Connection connection)
	{
		for (int i = 0; i < signalRConnection.GetHubs().Length; i++)
		{
			(signalRConnection.GetHubs()[i] as BaseHub).InvokedFromClient();
		}
	}

	private void Restart()
	{
		signalRConnection.RemoveConnectedHandler(OnSignalRConnected);
		signalRConnection.Close();
		signalRConnection = null;
		Start();
	}
}
