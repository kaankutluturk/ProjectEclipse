using Nekki.SF2.Core.Exceptions;
using Nekki.SF2.Core.Network;
using UnityEngine;
using UnityEngine.Video;

namespace Nekki.SF2.GUI.Scenes
{
	public class GameLoaderScene : Scene<GameLoaderScene>
	{
		[SerializeField]
		private VideoClip introClip;

		[SerializeField]
		private GameObject logo;

		private LoadingModule loadingModule = new LoadingModule();

		private bool startPending;

		private bool stopPending;

		private bool clearPending;

		[SerializeField]
		private LockScreen _lockScreenPrefab;

		private static bool isSessionLoaded;

		public override ScreenType SceneType
		{
			get
			{
				return get_SceneId();
			}
		}

		public VideoClip IntroVideoClip
		{
			get
			{
				return get_IntroClip();
			}
		}

		public GameObject LogoObject
		{
			get
			{
				return get_Logo();
			}
		}

		public override ScreenType get_SceneId()
		{
			return ScreenType.ModulePreloader;
		}

		public VideoClip get_IntroClip()
		{
			return introClip;
		}

		public GameObject get_Logo()
		{
			return logo;
		}

		protected override void Init(object data)
		{
			base.Init(data);
			Application.runInBackground = true;
			startPending = true;
			stopPending = isSessionLoaded;
            if (stopPending) { Stop(); stopPending = false; }
            Eclipse.UI.GameSessionRestart.ArrivedAtTitle();
            Eclipse.Multiplayer.LocalVersusSession.ArrivedAtTitle();
			clearPending = false;
			isSessionLoaded = true;
			get_Logo().SetActive(false);
			Eclipse.UI.TitleScreen.ShowAtStartup();
			if (_lockScreenPrefab != null && LockScreen.get_Instance() == null)
			{
				Object.Instantiate(_lockScreenPrefab);
			}
		}

		private void Update()
		{
			if (Eclipse.UI.TitleScreen.IsOpen) return;
			if (clearPending)
			{
				Clear();
			}
			if (stopPending)
			{
				Stop();
				stopPending = false;
			}
			if (startPending)
			{
				Start();
				startPending = false;
			}
			if (!loadingModule.IsLoadingActive())
			{
				loadingModule.Start();
			}
			if (!loadingModule.IsFinished())
			{
				if (!GameUtils.HackDetected)
				{
					try
					{
						loadingModule.ProcessStep();
					}
					catch (HackDetectedException ex)
					{
						GameUtils.HackDetected = true;
						ListSF.GetInstance().ShowDataCorruptedDialog(ex.Message);
					}
				}
			}
			else if (loadingModule.IsFinished() && !loadingModule.IsEmpty())
			{
				clearPending = true;
			}
		}

		public void Restart()
		{
			stopPending = true;
			startPending = true;
			SoundController.IsBackgroundMusicIntro = false;
			Sound.StopMusic();
			Sound.StopAllSounds();
		}

		public void Start()
		{
			Clear();
			loadingModule.Stop();
			loadingModule.AddModule(new PreInitializationModule());
			// Offline players need neither store licensing nor account/phone permissions.
			loadingModule.AddModule(new AntichitingModule());
			loadingModule.AddModule(new AttachFileModule());
			loadingModule.AddModule(new InitializationModule());
			if (!Eclipse.Multiplayer.LocalVersusSession.IsActive)
				loadingModule.AddModule(new IntroModule(this));
			loadingModule.AddModule(new ParseModule());
			loadingModule.AddModule(new LoginModule());
		}

		public static void Stop()
		{
            Eclipse.Modding.ModRuntime.Shutdown();
			isSessionLoaded = false;
			GameUtils.IsLoginComplete = false;
			GameUtils.ShowNews = true;
			Module.Reset();
			ListSF.Reset();
			ServerProvider.Reset();
			AnimationData.ClearAnimations();
			AiData.ClearAll();
		}

		// Eclipse: drops the title's early game-data preview (Eclipse.UI.TitleScreen) the way
		// Stop() resets a session, without changing this scene's own restart bookkeeping.
		public static void DiscardTitlePreview()
		{
			bool loaded = isSessionLoaded, online = GameUtils.IsLoginComplete, first = GameUtils.ShowNews;
			Stop();
			isSessionLoaded = loaded;
			GameUtils.IsLoginComplete = online;
			GameUtils.ShowNews = first;
		}

		private void Clear()
		{
			loadingModule.ClearModules(true);
			clearPending = false;
		}
	}
}
