using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CodeStage.AntiCheat.Detectors
{
	[AddComponentMenu("Code Stage/Anti-Cheat Toolkit/Speed Hack Detector")]
	public class SpeedHackDetector : ActDetectorBase
	{
		internal const string ComponentName = "Speed Hack Detector";

		internal const string LogPrefix = "[ACTk] Speed Hack Detector: ";

		private const long TICKS_PER_SECOND = 10000000L;

		private const int Threshold = 5000000;

		private static int instancesInScene;

		[Tooltip("Time (in seconds) between detector checks.")]
		public float interval = 1f;

		[Tooltip("Maximum false positives count allowed before registering speed hack.")]
		public byte maxFalsePositives = 3;

		[Tooltip("Amount of sequential successful checks before clearing internal false positives counter.\nSet 0 to disable Cool Down feature.")]
		public int coolDown = 30;

		private byte currentFalsePositives;

		private int currentCooldownShots;

		private long ticksOnStart;

		private long vulnerableTicksOnStart;

		private long prevTicks;

		private long prevIntervalTicks;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static SpeedHackDetector instance;

		public static SpeedHackDetector CurrentInstance
		{
			get
			{
				return get_Instance();
			}
			private set
			{
				set_Instance(value);
			}
		}

		private static SpeedHackDetector GetOrCreateInstance
		{
			get
			{
				return GetOrCreate();
			}
		}

		private SpeedHackDetector()
		{
		}

		public static void StartDetection()
		{
			if (get_Instance() != null)
			{
				get_Instance().StartDetectionInternal(null, get_Instance().interval, get_Instance().maxFalsePositives, get_Instance().coolDown);
			}
			else
			{
				UnityEngine.Debug.LogError("[ACTk] Speed Hack Detector: can't be started since it doesn't exists in scene or not yet initialized!");
			}
		}

		public static void StartDetection(UnityAction callback)
		{
			StartDetection(callback, GetOrCreate().interval);
		}

		public static void StartDetection(UnityAction callback, float CHCGJBLDPML)
		{
			StartDetection(callback, CHCGJBLDPML, GetOrCreate().maxFalsePositives);
		}

		public static void StartDetection(UnityAction callback, float CHCGJBLDPML, byte JKBEIPOFGCI)
		{
			StartDetection(callback, CHCGJBLDPML, JKBEIPOFGCI, GetOrCreate().coolDown);
		}

		public static void StartDetection(UnityAction callback, float CHCGJBLDPML, byte JKBEIPOFGCI, int CCCBHICMMJP)
		{
			GetOrCreate().StartDetectionInternal(callback, CHCGJBLDPML, JKBEIPOFGCI, CCCBHICMMJP);
		}

		public static void StopDetection()
		{
			if (get_Instance() != null)
			{
				get_Instance().StopDetectionInternal();
			}
		}

		public static void Dispose()
		{
			if (get_Instance() != null)
			{
				get_Instance().DisposeInternal();
			}
		}

		public static SpeedHackDetector get_Instance()
		{
			return instance;
		}

		private static void set_Instance(SpeedHackDetector value)
		{
			instance = value;
		}

		private static SpeedHackDetector GetOrCreate()
		{
			if (get_Instance() != null)
			{
				return get_Instance();
			}
			if (ActDetectorBase.detectorsContainer == null)
			{
				ActDetectorBase.detectorsContainer = new GameObject("Anti-Cheat Toolkit Detectors");
			}
			set_Instance(ActDetectorBase.detectorsContainer.AddComponent<SpeedHackDetector>());
			return get_Instance();
		}

		private void Awake()
		{
			instancesInScene++;
			if (Init(get_Instance(), "Speed Hack Detector"))
			{
				set_Instance(this);
			}
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			instancesInScene--;
		}

		private void OnSceneLoaded(Scene MHOCFOODLLL, LoadSceneMode NMMPBADCFHK)
		{
			OnLevelLoadedCallback();
		}

		private void OnLevelLoadedCallback()
		{
			if (instancesInScene < 2)
			{
				if (!keepAlive)
				{
					DisposeInternal();
				}
			}
			else if (!keepAlive && get_Instance() != this)
			{
				DisposeInternal();
			}
		}

		private void OnApplicationPause(bool KCANPMPILKI)
		{
			if (!KCANPMPILKI)
			{
				ResetStartTicks();
			}
		}

		private void Update()
		{
			if (!isRunning)
			{
				return;
			}
			long ticks = DateTime.UtcNow.Ticks;
			long num = ticks - prevTicks;
			if (num < 0 || num > 10000000)
			{
				ResetStartTicks();
				return;
			}
			prevTicks = ticks;
			long num2 = (long)(interval * 10000000f);
			if (ticks - prevIntervalTicks < num2)
			{
				return;
			}
			long num3 = (long)Environment.TickCount * 10000L;
			if (Mathf.Abs(num3 - vulnerableTicksOnStart - (ticks - ticksOnStart)) > 5000000f)
			{
				currentFalsePositives++;
				if (currentFalsePositives > maxFalsePositives)
				{
					OnCheatingDetected();
				}
				else
				{
					currentCooldownShots = 0;
					ResetStartTicks();
				}
			}
			else if (currentFalsePositives > 0 && coolDown > 0)
			{
				currentCooldownShots++;
				if (currentCooldownShots >= coolDown)
				{
					currentFalsePositives = 0;
				}
			}
			prevIntervalTicks = ticks;
		}

		private void StartDetectionInternal(UnityAction callback, float DHMGICLCNNA, byte BKMJNLEIGGG, int KBLLGDNEAMO)
		{
			if (isRunning)
			{
				UnityEngine.Debug.LogWarning("[ACTk] Speed Hack Detector: already running!", this);
				return;
			}
			if (!base.enabled)
			{
				UnityEngine.Debug.LogWarning("[ACTk] Speed Hack Detector: disabled but StartDetection still called from somewhere (see stack trace for this message)!", this);
				return;
			}
			if (callback != null && detectionEventHasListener)
			{
				UnityEngine.Debug.LogWarning("[ACTk] Speed Hack Detector: has properly configured Detection Event in the inspector, but still get started with Action callback. Both Action and Detection Event will be called on detection. Are you sure you wish to do this?", this);
			}
			if (callback == null && !detectionEventHasListener)
			{
				UnityEngine.Debug.LogWarning("[ACTk] Speed Hack Detector: was started without any callbacks. Please configure Detection Event in the inspector, or pass the callback Action to the StartDetection method.", this);
				base.enabled = false;
				return;
			}
			detectionAction = callback;
			interval = DHMGICLCNNA;
			maxFalsePositives = BKMJNLEIGGG;
			coolDown = KBLLGDNEAMO;
			ResetStartTicks();
			currentFalsePositives = 0;
			currentCooldownShots = 0;
			started = true;
			isRunning = true;
		}

		protected override void StartDetectionAutomatically()
		{
			StartDetectionInternal(null, interval, maxFalsePositives, coolDown);
		}

		protected override void PauseDetector()
		{
			isRunning = false;
		}

		protected override void ResumeDetector()
		{
			if (detectionAction != null || detectionEventHasListener)
			{
				isRunning = true;
			}
		}

		protected override void StopDetectionInternal()
		{
			if (started)
			{
				detectionAction = null;
				started = false;
				isRunning = false;
			}
		}

		protected override void DisposeInternal()
		{
			base.DisposeInternal();
			if (get_Instance() == this)
			{
				set_Instance(null);
			}
		}

		private void ResetStartTicks()
		{
			ticksOnStart = DateTime.UtcNow.Ticks;
			vulnerableTicksOnStart = (long)Environment.TickCount * 10000L;
			prevTicks = ticksOnStart;
			prevIntervalTicks = ticksOnStart;
		}
	}
}
