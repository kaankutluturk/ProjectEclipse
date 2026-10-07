using System;
using UnityEngine;

public class WebSocketSample : MonoBehaviour
{
	private string address = "ws://echo.websocket.org";

	private string msgToSend = "Hello World!";

	private string logText = string.Empty;

	private WebSocket webSocket;

	private Vector2 scrollPos;

	private void OnDestroy()
	{
		if (webSocket != null)
		{
			webSocket.Close();
		}
	}

	private void OnGUI()
	{
		GUIHelper.DrawArea(GUIHelper.ClientArea, true, () =>
		{
			scrollPos = GUILayout.BeginScrollView(scrollPos);
			GUILayout.Label(logText);
			GUILayout.EndScrollView();
			GUILayout.Space(5f);
			GUILayout.FlexibleSpace();
			address = GUILayout.TextField(address);
			if (webSocket == null && GUILayout.Button("Open Web Socket"))
			{
				webSocket = new WebSocket(new Uri(address));
				if (HTTPManager.GetProxy() != null)
				{
					webSocket.GetInternalRequest().SetProxy(new HTTPProxy(HTTPManager.GetProxy().GetAddress(), HTTPManager.GetProxy().GetCredentials(), false));
				}
				WebSocket socket = webSocket;
				socket.OnOpen = (OnWebSocketOpenDelegate)Delegate.Combine(socket.OnOpen, new OnWebSocketOpenDelegate(OnOpen));
				WebSocket iLNFPNFEOCL2 = webSocket;
				iLNFPNFEOCL2.OnMessage = (OnWebSocketMessageDelegate)Delegate.Combine(iLNFPNFEOCL2.OnMessage, new OnWebSocketMessageDelegate(OnMessageReceived));
				WebSocket iLNFPNFEOCL3 = webSocket;
				iLNFPNFEOCL3.OnClosed = (OnWebSocketClosedDelegate)Delegate.Combine(iLNFPNFEOCL3.OnClosed, new OnWebSocketClosedDelegate(OnClosed));
				WebSocket iLNFPNFEOCL4 = webSocket;
				iLNFPNFEOCL4.OnError = (OnWebSocketErrorDelegate)Delegate.Combine(iLNFPNFEOCL4.OnError, new OnWebSocketErrorDelegate(OnError));
				webSocket.OpenWebSocket();
				logText += "Opening Web Socket...\n";
			}
			if (webSocket != null && webSocket.GetIsOpen())
			{
				GUILayout.Space(10f);
				GUILayout.BeginHorizontal();
				msgToSend = GUILayout.TextField(msgToSend);
				if (GUILayout.Button("Send", GUILayout.MaxWidth(70f)))
				{
					logText += "Sending message...\n";
					webSocket.Send(msgToSend);
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(10f);
				if (GUILayout.Button("Close"))
				{
					webSocket.Close(1000, "Bye!");
				}
			}
		});
	}

	private void OnOpen(WebSocket socket)
	{
		logText += string.Format("-WebSocket Open!\n");
	}

	private void OnMessageReceived(WebSocket socket, string message)
	{
		logText += string.Format("-Message received: {0}\n", message);
	}

	private void OnClosed(WebSocket socket, ushort code, string message)
	{
		logText += string.Format("-WebSocket closed! Code: {0} Message: {1}\n", code, message);
		webSocket = null;
	}

	private void OnError(WebSocket socket, Exception error)
	{
		string text = string.Empty;
		if (socket.GetInternalRequest().GetResponse() != null)
		{
			text = string.Format("Status Code from Server: {0} and Message: {1}", socket.GetInternalRequest().GetResponse().GetStatusCode(), socket.GetInternalRequest().GetResponse().GetMessage());
		}
		logText += string.Format("-An error occured: {0}\n", (error == null) ? ("Unknown Error " + text) : error.Message);
		webSocket = null;
	}
}
