using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Nekki.SF2.GUI;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerController : MonoBehaviour
{
	public delegate void ShowCompletedHandler();

	private VideoPlayer videoPlayer;

	private AudioSource audioSource;

	private bool isPlaying;

	private bool completionRaised;

	private float prepareStartedAt;
	private int playStartedFrame;

	private const float PrepareTimeoutSeconds = 8f;

	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ShowCompletedHandler ShowCompleted;

	public event ShowCompletedHandler Completed
	{
		add
		{
			add_ShowCompleted(value);
		}
		remove
		{
			remove_ShowCompleted(value);
		}
	}

	public void add_ShowCompleted(ShowCompletedHandler value)
	{
		ShowCompletedHandler pPCCKEGFAHH = ShowCompleted;
		ShowCompletedHandler pPCCKEGFAHH2;
		do
		{
			pPCCKEGFAHH2 = pPCCKEGFAHH;
			pPCCKEGFAHH = Interlocked.CompareExchange(ref ShowCompleted, (ShowCompletedHandler)Delegate.Combine(pPCCKEGFAHH2, value), pPCCKEGFAHH);
		}
		while ((object)pPCCKEGFAHH != pPCCKEGFAHH2);
	}

	public void remove_ShowCompleted(ShowCompletedHandler value)
	{
		ShowCompletedHandler pPCCKEGFAHH = ShowCompleted;
		ShowCompletedHandler pPCCKEGFAHH2;
		do
		{
			pPCCKEGFAHH2 = pPCCKEGFAHH;
			pPCCKEGFAHH = Interlocked.CompareExchange(ref ShowCompleted, (ShowCompletedHandler)Delegate.Remove(pPCCKEGFAHH2, value), pPCCKEGFAHH);
		}
		while ((object)pPCCKEGFAHH != pPCCKEGFAHH2);
	}

	public void Init()
	{
		videoPlayer = GetComponent<VideoPlayer>();
		audioSource = GetComponent<AudioSource>();
		videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
		videoPlayer.controlledAudioTrackCount = 1;
		videoPlayer.EnableAudioTrack(0, true);
		videoPlayer.SetTargetAudioSource(0, audioSource);
		videoPlayer.loopPointReached += OnPlaybackFinished;
		videoPlayer.prepareCompleted += OnPrepareCompleted;
		videoPlayer.errorReceived += OnVideoError;
		videoPlayer.targetCamera = UnityEngine.Camera.main;
		UnityEngine.Camera.main.backgroundColor = Color.black;
		completionRaised = false;
	}

	private void Update()
	{
		if (!isPlaying)
		{
			return;
		}
		if (Time.frameCount > playStartedFrame + 1 && (Eclipse.Input.EclipseInput.touchCount > 0 || Eclipse.Input.EclipseInput.anyKeyDown || Eclipse.Input.EclipseInput.GetMouseButtonDown(0)))
		{
			SkipVideo();
			return;
		}
		if (videoPlayer != null && !videoPlayer.isPrepared && Time.realtimeSinceStartup - prepareStartedAt >= PrepareTimeoutSeconds)
		{
			UnityEngine.Debug.LogWarning("[Video] Preparation timed out; continuing without video: " + videoPlayer.url);
			SkipVideo();
		}
	}

	public void Play(string BEPKJNKCKPH)
	{
		Screen.sleepTimeout = -1;
		videoPlayer.source = VideoSource.Url;
		if (videoPlayer != null)
		{
			videoPlayer.url = BEPKJNKCKPH;
			videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
			videoPlayer.controlledAudioTrackCount = 1;
			videoPlayer.EnableAudioTrack(0, true);
			videoPlayer.SetTargetAudioSource(0, audioSource);
			playStartedFrame = Time.frameCount;
			prepareStartedAt = Time.realtimeSinceStartup;
			videoPlayer.Prepare();
			isPlaying = true;
		}
	}

	public void Play(VideoClip PIKHEAGHOKB)
	{
		Screen.sleepTimeout = -1;
		videoPlayer.source = VideoSource.VideoClip;
		if (videoPlayer != null)
		{
			videoPlayer.clip = PIKHEAGHOKB;
			playStartedFrame = Time.frameCount;
			prepareStartedAt = Time.realtimeSinceStartup;
			videoPlayer.Prepare();
			isPlaying = true;
		}
	}

	private void OnPrepareCompleted(VideoPlayer EJPOJJKKICO)
	{
		Sound.PauseMusic();
		SetUiCanvasEnabled(false);
		if (videoPlayer != null)
		{
			videoPlayer.prepareCompleted -= OnPrepareCompleted;
			videoPlayer.Play();
		}
		if (audioSource != null)
		{
			audioSource.Play();
		}
	}

	private void OnPlaybackFinished(VideoPlayer EJPOJJKKICO = null)
	{
		if (completionRaised)
		{
			return;
		}
		completionRaised = true;
		isPlaying = false;
		SetUiCanvasEnabled(true);
		if (videoPlayer != null)
		{
			videoPlayer.loopPointReached -= OnPlaybackFinished;
			videoPlayer.prepareCompleted -= OnPrepareCompleted;
			videoPlayer.errorReceived -= OnVideoError;
		}
		Screen.sleepTimeout = -2;
		Sound.ResumeMusic();
		ShowCompleted();
		videoPlayer = null;
		audioSource = null;
	}

	private void OnVideoError(VideoPlayer source, string message)
	{
		UnityEngine.Debug.LogWarning("[Video] Playback failed; continuing without video: " + message);
		OnPlaybackFinished(source);
	}

	private void SkipVideo()
	{
		if (videoPlayer != null)
		{
			videoPlayer.Stop();
		}
		if (audioSource != null)
		{
			audioSource.Stop();
		}
		OnPlaybackFinished();
	}

	private void SetUiCanvasEnabled(bool value)
	{
		ModuleHolder moduleHolder = Module.GetInstance().GetCurrentHolder();
		if (moduleHolder != null)
		{
			moduleHolder.GetCanvas().enabled = value;
		}
	}
}
