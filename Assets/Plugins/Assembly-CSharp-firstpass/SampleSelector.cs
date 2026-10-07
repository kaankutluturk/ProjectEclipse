using System.Collections.Generic;
using UnityEngine;

public class SampleSelector : MonoBehaviour
{
	public const int statisticsHeight = 160;

	private List<SampleDescriptor> samples = new List<SampleDescriptor>();

	public static SampleDescriptor SelectedSample;

	private Vector2 scrollPos;

	private void Awake()
	{
		HTTPManager.GetLogger().SetLevel(Loglevels.All);
		List<SampleDescriptor> sampleList = samples;
		SampleDescriptor label = new SampleDescriptor(null, "HTTP Samples", string.Empty, string.Empty);
		label.SetIsLabel(true);
		sampleList.Add(label);
		samples.Add(new SampleDescriptor(typeof(TextureDownloadSample), "Texture Download", "With HTTPManager.MaxConnectionPerServer you can control how many requests can be processed per server parallel.\n\nFeatures demoed in this example:\n-Parallel requests to the same server\n-Controlling the parallelization\n-Automatic Caching\n-Create a Texture2D from the downloaded data", CodeBlocks.TextureDownloadSampleCode));
		samples.Add(new SampleDescriptor(typeof(AssetBundleSample), "AssetBundle Download", "A small example that shows a possible way to download an AssetBundle and load a resource from it.\n\nFeatures demoed in this example:\n-Using HTTPRequest without a callback\n-Using HTTPRequest in a Coroutine\n-Loading an AssetBundle from the downloaded bytes\n-Automatic Caching", CodeBlocks.AssetBundleSampleCode));
		samples.Add(new SampleDescriptor(typeof(LargeFileDownloadSample), "Large File Download", "This example demonstrates how you can download a (large) file and continue the download after the connection is aborted.\n\nFeatures demoed in this example:\n-Setting up a streamed download\n-How to access the downloaded data while the download is in progress\n-Setting the HTTPRequest's StreamFragmentSize to controll the frequency and size of the fragments\n-How to use the SetRangeHeader to continue a previously disconnected download\n-How to disable the local, automatic caching", CodeBlocks.LargeFileDownloadSampleCode));
		List<SampleDescriptor> pOKCHMGCOOB2 = samples;
		label = new SampleDescriptor(null, "WebSocket Samples", string.Empty, string.Empty);
		label.SetIsLabel(true);
		pOKCHMGCOOB2.Add(label);
		samples.Add(new SampleDescriptor(typeof(WebSocketSample), "Echo", "A WebSocket demonstration that connects to a WebSocket echo service.\n\nFeatures demoed in this example:\n-Basic useage of the WebSocket class", CodeBlocks.WebSocketSampleCode));
		List<SampleDescriptor> pOKCHMGCOOB3 = samples;
		label = new SampleDescriptor(null, "Socket.IO Samples", string.Empty, string.Empty);
		label.SetIsLabel(true);
		pOKCHMGCOOB3.Add(label);
		samples.Add(new SampleDescriptor(typeof(SocketIOChatSample), "Chat", "This example uses the Socket.IO implementation to connect to the official Chat demo server(http://chat.socket.io/).\n\nFeatures demoed in this example:\n-Instantiating and setting up a SocketManager to connect to a Socket.IO server\n-Changing SocketOptions property\n-Subscribing to Socket.IO events\n-Sending custom events to the server", CodeBlocks.SocketIOChatSampleCode));
		samples.Add(new SampleDescriptor(typeof(SocketIOWePlaySample), "WePlay", "This example uses the Socket.IO implementation to connect to the official WePlay demo server(http://weplay.io/).\n\nFeatures demoed in this example:\n-Instantiating and setting up a SocketManager to connect to a Socket.IO server\n-Subscribing to Socket.IO events\n-Receiving binary data\n-How to load a texture from the received binary data\n-How to disable payload decoding for fine tune for some speed\n-Sending custom events to the server", CodeBlocks.SocketIOWePlaySampleCode));
		List<SampleDescriptor> pOKCHMGCOOB4 = samples;
		label = new SampleDescriptor(null, "SignalR Samples", string.Empty, string.Empty);
		label.SetIsLabel(true);
		pOKCHMGCOOB4.Add(label);
		samples.Add(new SampleDescriptor(typeof(SimpleStreamingSample), "Simple Streaming", "A very simple example of a background thread that broadcasts the server time to all connected clients every two seconds.\n\nFeatures demoed in this example:\n-Subscribing and handling non-hub messages", CodeBlocks.SimpleStreamingSampleCode));
		samples.Add(new SampleDescriptor(typeof(ConnectionAPISample), "Connection API", "Demonstrates all features of the lower-level connection API including starting and stopping, sending and receiving messages, and managing groups.\n\nFeatures demoed in this example:\n-Instantiating and setting up a SignalR Connection to connect to a SignalR server\n-Changing the default Json encoder\n-Subscribing to state changes\n-Receiving and handling of non-hub messages\n-Sending non-hub messages\n-Managing groups", CodeBlocks.ConnectionAPISampleCode));
		samples.Add(new SampleDescriptor(typeof(ConnectionStatusSample), "Connection Status", "Demonstrates how to handle the events that are raised when connections connect, reconnect and disconnect from the Hub API.\n\nFeatures demoed in this example:\n-Connecting to a Hub\n-Setting up a callback for Hub events\n-Handling server-sent method call requests\n-Calling a Hub-method on the server-side\n-Opening and closing the SignalR Connection", CodeBlocks.ConnectionStatusSampleCode));
		samples.Add(new SampleDescriptor(typeof(DemoHubSample), "Demo Hub", "A contrived example that exploits every feature of the Hub API.\n\nFeatures demoed in this example:\n-Creating and using wrapper Hub classes to encapsulate hub functions and events\n-Handling long running server-side functions by handling progress messages\n-Groups\n-Handling server-side functions with return value\n-Handling server-side functions throwing Exceptions\n-Calling server-side functions with complex type parameters\n-Calling server-side functions with array parameters\n-Calling overloaded server-side functions\n-Changing Hub states\n-Receiving and handling hub state changes\n-Calling server-side functions implemented in VB .NET", CodeBlocks.DemoHubSampleCode));
		samples.Add(new SampleDescriptor(typeof(AuthenticationSample), "Authentication", "Demonstrates how to use the authorization features of the Hub API to restrict certain Hubs and methods to specific users.\n\nFeatures demoed in this example:\n-Creating and using wrapper Hub classes to encapsulate hub functions and events\n-Create and use a Header-based authenticator to access protected APIs\n-SignalR over HTTPS", CodeBlocks.AuthenticationSampleCode));
		List<SampleDescriptor> pOKCHMGCOOB5 = samples;
		label = new SampleDescriptor(null, "Plugin Samples", string.Empty, string.Empty);
		label.SetIsLabel(true);
		pOKCHMGCOOB5.Add(label);
		samples.Add(new SampleDescriptor(typeof(CacheMaintenanceSample), "Cache Maintenance", "With this demo you can see how you can use the HTTPCacheService's BeginMaintainence function to delete too old cached entities and keep the cache size under a specified value.\n\nFeatures demoed in this example:\n-How to set up a HTTPCacheMaintananceParams\n-How to call the BeginMaintainence function", CodeBlocks.CacheMaintenanceSampleCode));
		SelectedSample = samples[1];
	}

	private void Start()
	{
		GUIHelper.ClientArea = new Rect(0f, 165f, Screen.width, Screen.height - 160 - 50);
	}

	private void Update()
	{
		if (Eclipse.Input.EclipseInput.GetKeyDown(KeyCode.Escape))
		{
			if (SelectedSample != null && SelectedSample.GetIsRunning())
			{
				SelectedSample.DestroyUnityObject();
			}
			else
			{
				Application.Quit();
			}
		}
		if ((Eclipse.Input.EclipseInput.GetKeyDown(KeyCode.KeypadEnter) || Eclipse.Input.EclipseInput.GetKeyDown(KeyCode.Return)) && SelectedSample != null && !SelectedSample.GetIsRunning())
		{
			SelectedSample.CreateUnityObject();
		}
	}

	private void OnGUI()
	{
		GeneralStatistics statistics = HTTPManager.GetGeneralStatistics(StatisticsQueryFlags.All);
		GUIHelper.DrawArea(new Rect(0f, 0f, Screen.width / 3, 160f), false, () =>
		{
			GUIHelper.DrawCenteredText("Connections");
			GUILayout.Space(5f);
			GUIHelper.DrawRow("Sum:", statistics.Connections.ToString());
			GUIHelper.DrawRow("Active:", statistics.ActiveConnections.ToString());
			GUIHelper.DrawRow("Free:", statistics.FreeConnections.ToString());
			GUIHelper.DrawRow("Recycled:", statistics.RecycledConnections.ToString());
			GUIHelper.DrawRow("Requests in queue:", statistics.RequestsInQueue.ToString());
		});
		GUIHelper.DrawArea(new Rect(Screen.width / 3, 0f, Screen.width / 3, 160f), false, () =>
		{
			GUIHelper.DrawCenteredText("Cache");
			if (!HTTPCacheService.GetIsSupported())
			{
				GUI.color = Color.yellow;
				GUIHelper.DrawCenteredText("Disabled in WebPlayer & Samsung Smart TV Builds!");
				GUI.color = Color.white;
			}
			GUILayout.Space(5f);
			GUIHelper.DrawRow("Cached entities:", statistics.CacheEntityCount.ToString());
			GUIHelper.DrawRow("Sum Size (bytes): ", statistics.CacheSize.ToString("N0"));
			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Clear Cache"))
			{
				HTTPCacheService.BeginClear();
			}
			GUILayout.EndVertical();
		});
		GUIHelper.DrawArea(new Rect(Screen.width / 3 * 2, 0f, Screen.width / 3, 160f), false, () =>
		{
			GUIHelper.DrawCenteredText("Cookies");
			if (!CookieJar.GetIsSavingSupported())
			{
				GUI.color = Color.yellow;
				GUIHelper.DrawCenteredText("Saving and loading from disk is disabled in WebPlayer & Samsung Smart TV Builds!");
				GUI.color = Color.white;
			}
			GUILayout.Space(5f);
			GUIHelper.DrawRow("Cookies:", statistics.CookieCount.ToString());
			GUIHelper.DrawRow("Estimated size (bytes):", statistics.CookieJarSize.ToString("N0"));
			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Clear Cookies"))
			{
				CookieJar.Clear();
			}
			GUILayout.EndVertical();
		});
		if (SelectedSample == null || (SelectedSample != null && !SelectedSample.GetIsRunning()))
		{
			GUIHelper.DrawArea(new Rect(0f, 165f, (SelectedSample != null) ? (Screen.width / 3) : Screen.width, Screen.height - 160 - 5), false, () =>
			{
				scrollPos = GUILayout.BeginScrollView(scrollPos);
				for (int i = 0; i < samples.Count; i++)
				{
					DrawSample(samples[i]);
				}
				GUILayout.EndScrollView();
			});
			if (SelectedSample != null)
			{
				DrawSampleDetails(SelectedSample);
			}
		}
		else if (SelectedSample != null && SelectedSample.GetIsRunning())
		{
			GUILayout.BeginArea(new Rect(0f, Screen.height - 50, Screen.width, 50f), string.Empty);
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Back", GUILayout.MinWidth(100f)))
			{
				SelectedSample.DestroyUnityObject();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
	}

	private void DrawSample(SampleDescriptor sample)
	{
		if (sample.GetIsLabel())
		{
			GUILayout.Space(15f);
			GUIHelper.DrawCenteredText(sample.GetDisplayName());
			GUILayout.Space(5f);
		}
		else if (GUILayout.Button(sample.GetDisplayName()))
		{
			sample.SetIsSelected(true);
			if (SelectedSample != null)
			{
				SelectedSample.SetIsSelected(false);
			}
			SelectedSample = sample;
		}
	}

	private void DrawSampleDetails(SampleDescriptor sample)
	{
		Rect rect = new Rect(Screen.width / 3, 165f, Screen.width / 3 * 2, Screen.height - 160 - 5);
		GUI.Box(rect, string.Empty);
		GUILayout.BeginArea(rect);
		GUILayout.BeginVertical();
		GUIHelper.DrawCenteredText(sample.GetDisplayName());
		GUILayout.Space(5f);
		GUILayout.Label(sample.GetDescription());
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Start Sample"))
		{
			sample.CreateUnityObject();
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
}
