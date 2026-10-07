using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CodeStage.AntiCheat.Detectors
{
	[AddComponentMenu("Code Stage/Anti-Cheat Toolkit/Injection Detector")]
	public class InjectionDetector : ActDetectorBase
	{
		private class AllowedAssembly
		{
			public readonly string name;

			public readonly int[] hashes;

			public AllowedAssembly(string name, int[] DEACEHPECDK)
			{
				this.name = name;
				this.hashes = DEACEHPECDK;
			}
		}

		internal const string ComponentName = "Injection Detector";

		internal const string LogPrefix = "[ACTk] Injection Detector: ";

		private static int instancesInScene;

		private bool signaturesAreNotGenuine;

		private AllowedAssembly[] allowedAssemblies;

		private string[] hexTable;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static InjectionDetector instance;

		public static InjectionDetector CurrentInstance
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

		private static InjectionDetector GetOrCreateInstance
		{
			get
			{
				return GetOrCreate();
			}
		}

		private InjectionDetector()
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
				UnityEngine.Debug.LogError("[ACTk] Injection Detector: can't be started since it doesn't exists in scene or not yet initialized!");
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

		public static InjectionDetector get_Instance()
		{
			return instance;
		}

		private static void set_Instance(InjectionDetector value)
		{
			instance = value;
		}

		private static InjectionDetector GetOrCreate()
		{
			if (get_Instance() != null)
			{
				return get_Instance();
			}
			if (ActDetectorBase.detectorsContainer == null)
			{
				ActDetectorBase.detectorsContainer = new GameObject("Anti-Cheat Toolkit Detectors");
			}
			set_Instance(ActDetectorBase.detectorsContainer.AddComponent<InjectionDetector>());
			return get_Instance();
		}

		private void Awake()
		{
			instancesInScene++;
			if (Init(get_Instance(), "Injection Detector"))
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
				UnityEngine.Debug.LogWarning("[ACTk] Injection Detector: already running!", this);
				return;
			}
			if (!base.enabled)
			{
				UnityEngine.Debug.LogWarning("[ACTk] Injection Detector: disabled but StartDetection still called from somewhere (see stack trace for this message)!", this);
				return;
			}
			if (callback != null && detectionEventHasListener)
			{
				UnityEngine.Debug.LogWarning("[ACTk] Injection Detector: has properly configured Detection Event in the inspector, but still get started with Action callback. Both Action and Detection Event will be called on detection. Are you sure you wish to do this?", this);
			}
			if (callback == null && !detectionEventHasListener)
			{
				UnityEngine.Debug.LogWarning("[ACTk] Injection Detector: was started without any callbacks. Please configure Detection Event in the inspector, or pass the callback Action to the StartDetection method.", this);
				base.enabled = false;
				return;
			}
			detectionAction = callback;
			started = true;
			isRunning = true;
			if (allowedAssemblies == null)
			{
				LoadAndParseAllowedAssemblies();
			}
			if (signaturesAreNotGenuine)
			{
				OnCheatingDetected();
			}
			else if (!FindInjectionInCurrentAssemblies())
			{
				AppDomain.CurrentDomain.AssemblyLoad += OnNewAssemblyLoaded;
			}
			else
			{
				OnCheatingDetected();
			}
		}

		protected override void StartDetectionAutomatically()
		{
			StartDetectionInternal(null);
		}

		protected override void PauseDetector()
		{
			isRunning = false;
			AppDomain.CurrentDomain.AssemblyLoad -= OnNewAssemblyLoaded;
		}

		protected override void ResumeDetector()
		{
			if (detectionAction != null || detectionEventHasListener)
			{
				isRunning = true;
				AppDomain.CurrentDomain.AssemblyLoad += OnNewAssemblyLoaded;
			}
		}

		protected override void StopDetectionInternal()
		{
			if (started)
			{
				AppDomain.CurrentDomain.AssemblyLoad -= OnNewAssemblyLoaded;
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

		private void OnNewAssemblyLoaded(object ABONPDBPJBA, AssemblyLoadEventArgs LKIOKGCNKHE)
		{
			if (!AssemblyAllowed(LKIOKGCNKHE.LoadedAssembly))
			{
				OnCheatingDetected();
			}
		}

		private bool FindInjectionInCurrentAssemblies()
		{
			bool result = false;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			if (assemblies.Length == 0)
			{
				result = true;
			}
			else
			{
				Assembly[] array = assemblies;
				foreach (Assembly eHKCIGHDNMI in array)
				{
					if (!AssemblyAllowed(eHKCIGHDNMI))
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		private bool AssemblyAllowed(Assembly EHKCIGHDNMI)
		{
			string text = EHKCIGHDNMI.GetName().Name;
			int value = GetAssemblyHash(EHKCIGHDNMI);
			bool result = false;
			for (int i = 0; i < allowedAssemblies.Length; i++)
			{
				AllowedAssembly fCBAKCJACOA = allowedAssemblies[i];
				if (fCBAKCJACOA.name == text && Array.IndexOf(fCBAKCJACOA.hashes, value) != -1)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		private void LoadAndParseAllowedAssemblies()
		{
			TextAsset textAsset = (TextAsset)Resources.Load("fndid", typeof(TextAsset));
			if (textAsset == null)
			{
				signaturesAreNotGenuine = true;
				return;
			}
			string[] separator = new string[1] { ":" };
			MemoryStream memoryStream = new MemoryStream(textAsset.bytes);
			BinaryReader binaryReader = new BinaryReader(memoryStream);
			int num = binaryReader.ReadInt32();
			allowedAssemblies = new AllowedAssembly[num];
			for (int i = 0; i < num; i++)
			{
				string bAINMLLIKOL = binaryReader.ReadString();
				bAINMLLIKOL = ObscuredString.EncryptDecrypt(bAINMLLIKOL, "Elina");
				string[] array = bAINMLLIKOL.Split(separator, StringSplitOptions.RemoveEmptyEntries);
				int num2 = array.Length;
				if (num2 > 1)
				{
					string gOHIIMFFFJI = array[0];
					int[] array2 = new int[num2 - 1];
					for (int j = 1; j < num2; j++)
					{
						array2[j - 1] = int.Parse(array[j]);
					}
					allowedAssemblies[i] = new AllowedAssembly(gOHIIMFFFJI, array2);
					continue;
				}
				signaturesAreNotGenuine = true;
				binaryReader.Close();
				memoryStream.Close();
				return;
			}
			binaryReader.Close();
			memoryStream.Close();
			Resources.UnloadAsset(textAsset);
			hexTable = new string[256];
			for (int k = 0; k < 256; k++)
			{
				hexTable[k] = k.ToString("x2");
			}
		}

		private int GetAssemblyHash(Assembly EHKCIGHDNMI)
		{
			AssemblyName assemblyName = EHKCIGHDNMI.GetName();
			byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
			string text = ((publicKeyToken.Length < 8) ? assemblyName.Name : (assemblyName.Name + PublicKeyTokenToString(publicKeyToken)));
			int num = 0;
			int length = text.Length;
			for (int i = 0; i < length; i++)
			{
				num += text[i];
				num += num << 10;
				num ^= num >> 6;
			}
			num += num << 3;
			num ^= num >> 11;
			return num + (num << 15);
		}

		private string PublicKeyTokenToString(byte[] KPAMPCLHCEN)
		{
			string text = string.Empty;
			for (int i = 0; i < 8; i++)
			{
				text += hexTable[KPAMPCLHCEN[i]];
			}
			return text;
		}
	}
}
