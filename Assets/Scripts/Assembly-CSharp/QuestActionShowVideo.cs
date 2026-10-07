using System.IO;
using System.Xml;
using UnityEngine;

public class QuestActionShowVideo : QuestAction
{
	private GameObject videoScreenObject;

	private VideoPlayerController videoPlayer;

	private string videoName;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		videoName = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		// The mobile intro is over 100 seconds long and the original player only
		// accepted touch input.  In the Editor that presents as an unskippable
		// black loader screen, so continue the first-launch quest immediately.
		if (Application.isEditor && string.Equals(videoName, "intro.mp4", System.StringComparison.OrdinalIgnoreCase))
		{
			Debug.Log("[Video] Skipping mobile intro during Editor play.");
			FinishAction();
			return;
		}
		videoScreenObject = (GameObject)Object.Instantiate(Resources.Load("Prefabs/VideoScreen"));
		videoPlayer = videoScreenObject.GetComponent<VideoPlayerController>();
		videoPlayer.Init();
		videoPlayer.add_ShowCompleted(OnVideoShowCompleted);
		string text = string.Format("{0}/{1}", SF2Paths.GetBundlesPath(), videoName);
		if (File.Exists(text))
		{
			videoPlayer.Play(text);
		}
		else
		{
			videoPlayer.Play(ResourceManager.GetVideoClip(videoName));
		}
	}

	private void OnVideoShowCompleted()
	{
		videoPlayer.remove_ShowCompleted(OnVideoShowCompleted);
		Object.Destroy(videoScreenObject, 1f);
		FinishAction();
	}
}
