using System;
using System.Collections;
using UnityEngine;

public sealed class AssetBundleSample : MonoBehaviour
{
	private const string URL = "http://besthttp.azurewebsites.net/Content/AssetBundle.html";

	private string status = "Waiting for user interaction";

	private AssetBundle cachedBundle;

	private Texture2D texture;

	private bool downloading;

	private void OnGUI()
	{
		GUIHelper.DrawArea(GUIHelper.ClientArea, true, () =>
		{
			GUILayout.Label("Status: " + status);
			if (texture != null)
			{
				GUILayout.Box(texture, GUILayout.MaxHeight(256f));
			}
			if (!downloading && GUILayout.Button("Start Download"))
			{
				UnloadBundle();
				StartCoroutine(DownloadAssetBundle());
			}
		});
	}

	private void OnDestroy()
	{
		UnloadBundle();
	}

	private IEnumerator DownloadAssetBundle()
	{
		downloading = true;
		HTTPRequest iPLGNIDJDCF = new HTTPRequest(new Uri("http://besthttp.azurewebsites.net/Content/AssetBundle.html")).Send();
		status = "Download started";
		while (iPLGNIDJDCF.GetState() < HTTPRequestStates.Finished)
		{
			yield return new WaitForSeconds(0.1f);
			status += ".";
		}
		switch (iPLGNIDJDCF.GetState())
		{
		case HTTPRequestStates.Finished:
			if (iPLGNIDJDCF.GetResponse().GetIsSuccess())
			{
				status = string.Format("AssetBundle downloaded! Loaded from local cache: {0}", iPLGNIDJDCF.GetResponse().GetIsFromCache().ToString());
				AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(iPLGNIDJDCF.GetResponse().GetData());
				yield return assetBundleCreateRequest;
				yield return StartCoroutine(ProcessAssetBundle(assetBundleCreateRequest.assetBundle));
			}
			else
			{
				status = string.Format("Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}", iPLGNIDJDCF.GetResponse().GetStatusCode(), iPLGNIDJDCF.GetResponse().GetMessage(), iPLGNIDJDCF.GetResponse().GetDataAsText());
				AdvLog.LogWarning(status);
			}
			break;
		case HTTPRequestStates.Error:
			status = "Request Finished with Error! " + ((iPLGNIDJDCF.GetException() == null) ? "No Exception" : (iPLGNIDJDCF.GetException().Message + "\n" + iPLGNIDJDCF.GetException().StackTrace));
			AdvLog.LogError(status);
			break;
		case HTTPRequestStates.Aborted:
			status = "Request Aborted!";
			AdvLog.LogWarning(status);
			break;
		case HTTPRequestStates.ConnectionTimedOut:
			status = "Connection Timed Out!";
			AdvLog.LogError(status);
			break;
		case HTTPRequestStates.TimedOut:
			status = "Processing the request Timed Out!";
			AdvLog.LogError(status);
			break;
		}
		downloading = false;
	}

	private IEnumerator ProcessAssetBundle(AssetBundle bundle)
	{
		if (!(bundle == null))
		{
			cachedBundle = bundle;
			AssetBundleRequest assetBundleRequest = cachedBundle.LoadAssetAsync("9443182_orig", typeof(Texture2D));
			yield return assetBundleRequest;
			texture = assetBundleRequest.asset as Texture2D;
		}
	}

	private void UnloadBundle()
	{
		if (cachedBundle != null)
		{
			cachedBundle.Unload(true);
			cachedBundle = null;
		}
	}
}
