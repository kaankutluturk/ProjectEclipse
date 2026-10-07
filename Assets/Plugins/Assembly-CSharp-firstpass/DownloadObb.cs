using UnityEngine;

public class DownloadObb : MonoBehaviour
{
	private void Awake()
	{
		if (!GooglePlayDownloader.RunningOnAndroid())
		{
			LoadFirstLevel("not android");
			return;
		}
		string text = GooglePlayDownloader.GetExpansionFilePath();
		if (string.IsNullOrEmpty(text))
		{
			LoadFirstLevel("no obb file");
			return;
		}
		string text2 = GooglePlayDownloader.GetMainOBBPath(text);
		string text3 = GooglePlayDownloader.GetPatchOBBPath(text);
		if (text2 == null || text3 == null)
		{
			GooglePlayDownloader.FetchOBB();
			LoadFirstLevel("all done");
		}
	}

	private void LoadFirstLevel(string NEPOLDCKNJL)
	{
		AdvLog.LogWarning(NEPOLDCKNJL);
		Application.LoadLevel(1);
	}
}
