using System;
using UnityEngine;

public sealed class TextureDownloadSample : MonoBehaviour
{
	private const string BaseURL = "http://besthttp.azurewebsites.net/Content/";

	private string[] Images = new string[9] { "One.png", "Two.png", "Three.png", "Four.png", "Five.png", "Six.png", "Seven.png", "Eight.png", "Nine.png" };

	private Texture2D[] Textures = new Texture2D[9];

	private bool allDownloadedFromLocalCache;

	private int finishedCount;

	private Vector2 scrollPos;

	private void Awake()
	{
		HTTPManager.set_MaxConnectionPerServer(1);
		for (int i = 0; i < Images.Length; i++)
		{
			Textures[i] = new Texture2D(100, 150);
		}
	}

	private void OnDestroy()
	{
		HTTPManager.set_MaxConnectionPerServer(4);
	}

	private void OnGUI()
	{
		GUIHelper.DrawArea(GUIHelper.ClientArea, true, () =>
		{
			scrollPos = GUILayout.BeginScrollView(scrollPos);
			GUILayout.SelectionGrid(0, Textures, 3);
			if (finishedCount == Images.Length && allDownloadedFromLocalCache)
			{
				GUIHelper.DrawCenteredText("All images loaded from the local cache!");
			}
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Max Connection/Server: ", GUILayout.Width(150f));
			GUILayout.Label(HTTPManager.GetMaxConnectionPerServer().ToString(), GUILayout.Width(20f));
			HTTPManager.set_MaxConnectionPerServer((byte)GUILayout.HorizontalSlider((int)HTTPManager.GetMaxConnectionPerServer(), 1f, 10f));
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Start Download"))
			{
				DownloadImages();
			}
			GUILayout.EndScrollView();
		});
	}

	private void DownloadImages()
	{
		allDownloadedFromLocalCache = true;
		finishedCount = 0;
		for (int i = 0; i < Images.Length; i++)
		{
			Textures[i] = new Texture2D(100, 150);
			HTTPRequest request = new HTTPRequest(new Uri("http://besthttp.azurewebsites.net/Content/" + Images[i]), OnImageDownloaded);
			request.set_Tag(Textures[i]);
			request.Send();
		}
	}

	private void OnImageDownloaded(HTTPRequest request, HTTPResponse response)
	{
		finishedCount++;
		switch (request.GetState())
		{
		case HTTPRequestStates.Finished:
			if (response.GetIsSuccess())
			{
				Texture2D texture2D = request.GetTag() as Texture2D;
				texture2D.LoadImage(response.GetData());
				allDownloadedFromLocalCache = allDownloadedFromLocalCache && response.GetIsFromCache();
			}
			else
			{
				AdvLog.LogWarning(string.Format("Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}", response.GetStatusCode(), response.GetMessage(), response.GetDataAsText()));
			}
			break;
		case HTTPRequestStates.Error:
			AdvLog.LogError("Request Finished with Error! " + ((request.GetException() == null) ? "No Exception" : (request.GetException().Message + "\n" + request.GetException().StackTrace)));
			break;
		case HTTPRequestStates.Aborted:
			AdvLog.LogWarning("Request Aborted!");
			break;
		case HTTPRequestStates.ConnectionTimedOut:
			AdvLog.LogError("Connection Timed Out!");
			break;
		case HTTPRequestStates.TimedOut:
			AdvLog.LogError("Processing the request Timed Out!");
			break;
		}
	}
}
