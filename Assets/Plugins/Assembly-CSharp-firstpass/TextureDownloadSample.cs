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
			HTTPRequest iPLGNIDJDCF = new HTTPRequest(new Uri("http://besthttp.azurewebsites.net/Content/" + Images[i]), OnImageDownloaded);
			iPLGNIDJDCF.set_Tag(Textures[i]);
			iPLGNIDJDCF.Send();
		}
	}

	private void OnImageDownloaded(HTTPRequest CGOIOKHEGOE, HTTPResponse BEIGFGCBICO)
	{
		finishedCount++;
		switch (CGOIOKHEGOE.GetState())
		{
		case HTTPRequestStates.Finished:
			if (BEIGFGCBICO.GetIsSuccess())
			{
				Texture2D texture2D = CGOIOKHEGOE.GetTag() as Texture2D;
				texture2D.LoadImage(BEIGFGCBICO.GetData());
				allDownloadedFromLocalCache = allDownloadedFromLocalCache && BEIGFGCBICO.GetIsFromCache();
			}
			else
			{
				AdvLog.LogWarning(string.Format("Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}", BEIGFGCBICO.GetStatusCode(), BEIGFGCBICO.GetMessage(), BEIGFGCBICO.GetDataAsText()));
			}
			break;
		case HTTPRequestStates.Error:
			AdvLog.LogError("Request Finished with Error! " + ((CGOIOKHEGOE.GetException() == null) ? "No Exception" : (CGOIOKHEGOE.GetException().Message + "\n" + CGOIOKHEGOE.GetException().StackTrace)));
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
