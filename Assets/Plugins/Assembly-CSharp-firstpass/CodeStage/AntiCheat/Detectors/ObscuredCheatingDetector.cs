using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CodeStage.AntiCheat.Detectors
{
	[AddComponentMenu("Code Stage/Anti-Cheat Toolkit/Obscured Cheating Detector")]
	public class ObscuredCheatingDetector : ActDetectorBase
	{
		internal const string ComponentName = "Obscured Cheating Detector";

		internal const string LogPrefix = "[ACTk] Obscured Cheating Detector: ";

		private static int instancesInScene;

		[Tooltip("Max allowed difference between encrypted and fake values in ObscuredFloat. Increase in case of false positives.")]
		public float floatEpsilon = 0.0001f;

		[Tooltip("Max allowed difference between encrypted and fake values in ObscuredVector2. Increase in case of false positives.")]
		public float vector2Epsilon = 0.1f;

		[Tooltip("Max allowed difference between encrypted and fake values in ObscuredVector3. Increase in case of false positives.")]
		public float vector3Epsilon = 0.1f;

		[Tooltip("Max allowed difference between encrypted and fake values in ObscuredQuaternion. Increase in case of false positives.")]
		public float quaternionEpsilon = 0.1f;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static ObscuredCheatingDetector instance;

		public static ObscuredCheatingDetector CurrentInstance
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

		private static ObscuredCheatingDetector GetOrCreateInstance
		{
			get
			{
				return GetOrCreate();
			}
		}

		internal static bool IsDetectionRunning
		{
			get
			{
				return GetIsRunning();
			}
		}

		private ObscuredCheatingDetector()
		{
		}

		public static void StartDetection()
		{
			if (get_Instance() != null)
			{
				get_Instance().StartDetectionInternal(null);
			}
			else
			{
				UnityEngine.Debug.LogError("[ACTk] Obscured Cheating Detector: can't be started since it doesn't exists in scene or not yet initialized!");
			}
		}

		public static void StartDetection(UnityAction callback)
		{
			GetOrCreate().StartDetectionInternal(callback);
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

		public static ObscuredCheatingDetector get_Instance()
		{
			return instance;
		}

		private static void set_Instance(ObscuredCheatingDetector value)
		{
			instance = value;
		}

		private static ObscuredCheatingDetector GetOrCreate()
		{
			if (get_Instance() != null)
			{
				return get_Instance();
			}
			if (ActDetectorBase.detectorsContainer == null)
			{
				ActDetectorBase.detectorsContainer = new GameObject("Anti-Cheat Toolkit Detectors");
			}
			set_Instance(ActDetectorBase.detectorsContainer.AddComponent<ObscuredCheatingDetector>());
			return get_Instance();
		}

		internal static bool GetIsRunning()
		{
			return (object)get_Instance() != null && get_Instance().isRunning;
		}

		private void Awake()
		{
			instancesInScene++;
			if (Init(get_Instance(), "Obscured Cheating Detector"))
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

		private void StartDetectionInternal(UnityAction callback)
		{
			if (isRunning)
			{
				UnityEngine.Debug.LogWarning("[ACTk] Obscured Cheating Detector: already running!", this);
				return;
			}
			if (!base.enabled)
			{
				UnityEngine.Debug.LogWarning("[ACTk] Obscured Cheating Detector: disabled but StartDetection still called from somewhere (see stack trace for this message)!", this);
				return;
			}
			if (callback != null && detectionEventHasListener)
			{
				UnityEngine.Debug.LogWarning("[ACTk] Obscured Cheating Detector: has properly configured Detection Event in the inspector, but still get started with Action callback. Both Action and Detection Event will be called on detection. Are you sure you wish to do this?", this);
			}
			if (callback == null && !detectionEventHasListener)
			{
				UnityEngine.Debug.LogWarning("[ACTk] Obscured Cheating Detector: was started without any callbacks. Please configure Detection Event in the inspector, or pass the callback Action to the StartDetection method.", this);
				base.enabled = false;
			}
			else
			{
				detectionAction = callback;
				started = true;
				isRunning = true;
			}
		}

		protected override void StartDetectionAutomatically()
		{
			StartDetectionInternal(null);
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
	}
}
