using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SocketIOWePlaySample : MonoBehaviour
{
	private enum WePlayConnectionState
	{
		Connecting = 0,
		WaitForNick = 1,
		Joined = 2
	}

	private string[] controls = new string[8] { "left", "right", "a", "b", "up", "down", "select", "start" };

	private const float ratio = 1.5f;

	private int maxMessages = 50;

	private WePlayConnectionState state;

	private Socket socket;

	private string nick = string.Empty;

	private string messageToSend = string.Empty;

	private int connections;

	private List<string> messages = new List<string>();

	private Vector2 scrollPos;

	private Texture2D FrameTexture;

	private void Start()
	{
		SocketOptions pGHMKLAAHKP = new SocketOptions();
		pGHMKLAAHKP.SetAutoConnect(false);
		SocketManager mFANOMMMCFG = new SocketManager(new Uri("http://io.weplay.io/socket.io/"), pGHMKLAAHKP);
		socket = mFANOMMMCFG.GetRootSocket();
		socket.On(SocketIOEventType.Connect, OnConnected);
		socket.On("joined", OnJoined);
		socket.On("connections", OnConnections);
		socket.On("join", OnJoin);
		socket.On("move", OnMove);
		socket.On("message", OnMessage);
		socket.On("reload", OnReload);
		socket.On("frame", OnFrame, false);
		socket.On(SocketIOEventType.Error, OnError);
		mFANOMMMCFG.Open();
		state = WePlayConnectionState.Connecting;
	}

	private void OnDestroy()
	{
		socket.GetManager().Close();
	}

	private void Update()
	{
		if (Eclipse.Input.EclipseInput.GetKeyDown(KeyCode.Escape))
		{
			SampleSelector.SelectedSample.DestroyUnityObject();
		}
	}

	private void OnGUI()
	{
		switch (state)
		{
		case WePlayConnectionState.Connecting:
			GUIHelper.DrawArea(GUIHelper.ClientArea, true, () =>
			{
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				GUIHelper.DrawCenteredText("Connecting to the server...");
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
			});
			break;
		case WePlayConnectionState.WaitForNick:
			GUIHelper.DrawArea(GUIHelper.ClientArea, true, () =>
			{
				DrawLoginScreen();
			});
			break;
		case WePlayConnectionState.Joined:
			GUIHelper.DrawArea(GUIHelper.ClientArea, true, () =>
			{
				if (FrameTexture != null)
				{
					GUILayout.Box(FrameTexture);
				}
				DrawControls();
				DrawChat();
			});
			break;
		}
	}

	private void DrawLoginScreen()
	{
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		GUIHelper.DrawCenteredText("What's your nickname?");
		nick = GUILayout.TextField(nick);
		if (GUILayout.Button("Join"))
		{
			Join();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
	}

	private void DrawControls()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Controls:");
		for (int i = 0; i < controls.Length; i++)
		{
			if (GUILayout.Button(controls[i]))
			{
				socket.Emit("move", controls[i]);
			}
		}
		GUILayout.Label(" Connections: " + connections);
		GUILayout.EndHorizontal();
	}

	private void DrawChat(bool JBAGKKKEBGK = true)
	{
		GUILayout.BeginVertical();
		scrollPos = GUILayout.BeginScrollView(scrollPos, false, false);
		for (int i = 0; i < messages.Count; i++)
		{
			GUILayout.Label(messages[i], GUILayout.MinWidth(Screen.width));
		}
		GUILayout.EndScrollView();
		if (JBAGKKKEBGK)
		{
			GUILayout.Label("Your message: ");
			GUILayout.BeginHorizontal();
			messageToSend = GUILayout.TextField(messageToSend);
			if (GUILayout.Button("Send", GUILayout.MaxWidth(100f)))
			{
				SendChatMessage();
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
	}

	private void AddMessage(string CKEHOEGLMBM)
	{
		messages.Insert(0, CKEHOEGLMBM);
		if (messages.Count > maxMessages)
		{
			messages.RemoveRange(maxMessages, messages.Count - maxMessages);
		}
	}

	private void SendChatMessage()
	{
		if (!string.IsNullOrEmpty(messageToSend))
		{
			socket.Emit("message", messageToSend);
			AddMessage(string.Format("{0}: {1}", nick, messageToSend));
			messageToSend = string.Empty;
		}
	}

	private void Join()
	{
		PlayerPrefs.SetString("Nick", nick);
		socket.Emit("join", nick);
	}

	private void Reload()
	{
		FrameTexture = null;
		if (socket != null)
		{
			socket.GetManager().Close();
			socket = null;
			Start();
		}
	}

	private void OnConnected(Socket JLEACANCMJF, Packet NPKADBPBKIG, params object[] LKIOKGCNKHE)
	{
		if (PlayerPrefs.HasKey("Nick"))
		{
			nick = PlayerPrefs.GetString("Nick", "NickName");
			Join();
		}
		else
		{
			state = WePlayConnectionState.WaitForNick;
		}
		AddMessage("connected");
	}

	private void OnJoined(Socket JLEACANCMJF, Packet NPKADBPBKIG, params object[] LKIOKGCNKHE)
	{
		state = WePlayConnectionState.Joined;
	}

	private void OnReload(Socket JLEACANCMJF, Packet NPKADBPBKIG, params object[] LKIOKGCNKHE)
	{
		Reload();
	}

	private void OnMessage(Socket JLEACANCMJF, Packet NPKADBPBKIG, params object[] LKIOKGCNKHE)
	{
		if (LKIOKGCNKHE.Length == 1)
		{
			AddMessage(LKIOKGCNKHE[0] as string);
		}
		else
		{
			AddMessage(string.Format("{0}: {1}", LKIOKGCNKHE[1], LKIOKGCNKHE[0]));
		}
	}

	private void OnMove(Socket JLEACANCMJF, Packet NPKADBPBKIG, params object[] LKIOKGCNKHE)
	{
		AddMessage(string.Format("{0} pressed {1}", LKIOKGCNKHE[1], LKIOKGCNKHE[0]));
	}

	private void OnJoin(Socket JLEACANCMJF, Packet NPKADBPBKIG, params object[] LKIOKGCNKHE)
	{
		string arg = ((LKIOKGCNKHE.Length <= 1) ? string.Empty : string.Format("({0})", LKIOKGCNKHE[1]));
		AddMessage(string.Format("{0} joined {1}", LKIOKGCNKHE[0], arg));
	}

	private void OnConnections(Socket JLEACANCMJF, Packet NPKADBPBKIG, params object[] LKIOKGCNKHE)
	{
		connections = Convert.ToInt32(LKIOKGCNKHE[0]);
	}

	private void OnFrame(Socket JLEACANCMJF, Packet NPKADBPBKIG, params object[] LKIOKGCNKHE)
	{
		if (state == WePlayConnectionState.Joined)
		{
			if (FrameTexture == null)
			{
				FrameTexture = new Texture2D(0, 0, TextureFormat.RGBA32, false);
				FrameTexture.filterMode = FilterMode.Point;
			}
			byte[] data = NPKADBPBKIG.GetAttachments()[0];
			FrameTexture.LoadImage(data);
		}
	}

	private void OnError(Socket JLEACANCMJF, Packet NPKADBPBKIG, params object[] LKIOKGCNKHE)
	{
		AddMessage(string.Format("--ERROR - {0}", LKIOKGCNKHE[0].ToString()));
	}
}
