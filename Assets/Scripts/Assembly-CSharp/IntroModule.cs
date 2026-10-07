using Nekki.SF2.GUI.Scenes;
using UnityEngine;
using UnityEngine.Video;

public class IntroModule : LoadingModule
{
	private VideoClip introClip;

	private GameObject videoScreenObject;

	private VideoPlayerController videoPlayer;

	private GameObject _logo;

	public IntroModule(GameLoaderScene FCDFLMFEJGI)
	{
		introClip = FCDFLMFEJGI.get_IntroClip();
		_logo = FCDFLMFEJGI.get_Logo();
	}

	// Eclipse: the intro plays on entering a save only when enabled under Options > Audio.
	public static bool Disabled { get { return !Eclipse.UI.IntroVideoSetting.Enabled; } }

	public override void Start()
	{
		base.Start();
		if (Disabled || !AssemblyController.GetShowIntro())
		{
			if (_logo != null)
			{
				_logo.SetActive(true);
			}
			isFinished = true;
			return;
		}
		if (AssemblyController.GetShowIntro())
		{
			videoScreenObject = (GameObject)Object.Instantiate(Resources.Load("Prefabs/VideoScreen"));
			videoPlayer = videoScreenObject.GetComponent<VideoPlayerController>();
			videoPlayer.Init();
			videoPlayer.add_ShowCompleted(OnVideoFinished);
			videoPlayer.Play(introClip);
		}
		else
		{
			_logo.SetActive(true);
			isFinished = true;
		}
	}

	public override void ProcessStep()
	{
		if (!isFinished && Eclipse.Input.EclipseInput.anyKeyDown)
		{
			OnVideoFinished();
		}
	}

	private void OnVideoFinished()
	{
		videoPlayer.remove_ShowCompleted(OnVideoFinished);
		Object.Destroy(videoPlayer, 0.5f);
		Object.Destroy(videoScreenObject, 0.5f);
		videoScreenObject = null;
		introClip = null;
		videoPlayer = null;
		_logo.SetActive(true);
		isFinished = true;
	}
}
