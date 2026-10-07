using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace CodeStage.AntiCheat.Detectors
{
	[AddComponentMenu("Code Stage/Anti-Cheat Toolkit/WallHack Detector")]
	public class WallHackDetector : ActDetectorBase
	{
		internal const string ComponentName = "WallHack Detector";

		internal const string LogPrefix = "[ACTk] WallHack Detector: ";

		private const string ServiceContainerName = "[WH Detector Service]";

		private const string WireframeShaderName = "Hidden/ACTk/WallHackTexture";

		private const int ShaderTextureSize = 4;

		private const int RenderTextureSize = 4;

		private readonly Vector3 rigidPlayerVelocity = new Vector3(0f, 0f, 1f);

		private static int instancesInScene;

		private readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

		[Tooltip("Check for the \"walk through the walls\" kind of cheats made via Rigidbody hacks?")]
		[SerializeField]
		private bool checkRigidbody = true;

		[Tooltip("Check for the \"walk through the walls\" kind of cheats made via Character Controller hacks?")]
		[SerializeField]
		private bool checkController = true;

		[SerializeField]
		[Tooltip("Check for the \"see through the walls\" kind of cheats made via shader or driver hacks (wireframe, color alpha, etc.)?")]
		private bool checkWireframe = true;

		[Tooltip("Check for the \"shoot through the walls\" kind of cheats made via Raycast hacks?")]
		[SerializeField]
		private bool checkRaycast = true;

		[Range(1f, 60f)]
		[Tooltip("Delay between Wireframe module checks, from 1 up to 60 secs.")]
		public int wireframeDelay = 10;

		[Range(1f, 60f)]
		[Tooltip("Delay between Raycast module checks, from 1 up to 60 secs.")]
		public int raycastDelay = 10;

		[Tooltip("World position of the container for service objects within 3x3x3 cube (drawn as red wire cube in scene).")]
		public Vector3 spawnPosition;

		[Tooltip("Maximum false positives in a row for each detection module before registering a wall hack.")]
		public byte maxFalsePositives = 3;

		private GameObject serviceContainer;

		private GameObject solidWall;

		private GameObject thinWall;

		private Camera wfCamera;

		private MeshRenderer foregroundRenderer;

		private MeshRenderer backgroundRenderer;

		private Color foregroundColor = Color.black;

		private Color backgroundColor = Color.black;

		private Shader wfShader;

		private Material wfMaterial;

		private Texture2D shaderTexture;

		private Texture2D targetTexture;

		private RenderTexture renderTexture;

		private int whLayer = -1;

		private int raycastMask = -1;

		private Rigidbody rigidPlayer;

		private CharacterController charControllerPlayer;

		private float charControllerVelocity;

		private byte rigidbodyDetections;

		private byte controllerDetections;

		private byte wireframeDetections;

		private byte raycastDetections;

		private bool wireframeDetected;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WallHackDetector instance;

		public bool RigidbodyCheckEnabled
		{
			get
			{
				return get_CheckRigidbody();
			}
			set
			{
				set_CheckRigidbody(value);
			}
		}

		public bool ControllerCheckEnabled
		{
			get
			{
				return get_CheckController();
			}
			set
			{
				set_CheckController(value);
			}
		}

		public bool WireframeCheckEnabled
		{
			get
			{
				return get_CheckWireframe();
			}
			set
			{
				set_CheckWireframe(value);
			}
		}

		public bool RaycastCheckEnabled
		{
			get
			{
				return get_CheckRaycast();
			}
			set
			{
				set_CheckRaycast(value);
			}
		}

		public static WallHackDetector CurrentInstance
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

		private static WallHackDetector GetOrCreateInstance
		{
			get
			{
				return GetOrCreate();
			}
		}

		private WallHackDetector()
		{
		}

		public bool get_CheckRigidbody()
		{
			return checkRigidbody;
		}

		public void set_CheckRigidbody(bool value)
		{
			if (checkRigidbody == value || !Application.isPlaying || !base.enabled || !base.gameObject.activeSelf)
			{
				return;
			}
			checkRigidbody = value;
			if (started)
			{
				UpdateServiceContainer();
				if (checkRigidbody)
				{
					StartRigidModule();
				}
				else
				{
					StopRigidModule();
				}
			}
		}

		public bool get_CheckController()
		{
			return checkController;
		}

		public void set_CheckController(bool value)
		{
			if (checkController == value || !Application.isPlaying || !base.enabled || !base.gameObject.activeSelf)
			{
				return;
			}
			checkController = value;
			if (started)
			{
				UpdateServiceContainer();
				if (checkController)
				{
					StartControllerModule();
				}
				else
				{
					StopControllerModule();
				}
			}
		}

		public bool get_CheckWireframe()
		{
			return checkWireframe;
		}

		public void set_CheckWireframe(bool value)
		{
			if (checkWireframe == value || !Application.isPlaying || !base.enabled || !base.gameObject.activeSelf)
			{
				return;
			}
			checkWireframe = value;
			if (started)
			{
				UpdateServiceContainer();
				if (checkWireframe)
				{
					StartWireframeModule();
				}
				else
				{
					StopWireframeModule();
				}
			}
		}

		public bool get_CheckRaycast()
		{
			return checkRaycast;
		}

		public void set_CheckRaycast(bool value)
		{
			if (checkRaycast == value || !Application.isPlaying || !base.enabled || !base.gameObject.activeSelf)
			{
				return;
			}
			checkRaycast = value;
			if (started)
			{
				UpdateServiceContainer();
				if (checkRaycast)
				{
					StartRaycastModule();
				}
				else
				{
					StopRaycastModule();
				}
			}
		}

		public static void StartDetection()
		{
			if (get_Instance() != null)
			{
				get_Instance().StartDetectionInternal(null, get_Instance().spawnPosition, get_Instance().maxFalsePositives);
			}
			else
			{
				UnityEngine.Debug.LogError("[ACTk] WallHack Detector: can't be started since it doesn't exists in scene or not yet initialized!");
			}
		}

		public static void StartDetection(UnityAction callback)
		{
			StartDetection(callback, GetOrCreate().spawnPosition);
		}

		public static void StartDetection(UnityAction callback, Vector3 spawnPosition)
		{
			StartDetection(callback, spawnPosition, GetOrCreate().maxFalsePositives);
		}

		public static void StartDetection(UnityAction callback, Vector3 spawnPosition, byte allowedFalsePositives)
		{
			GetOrCreate().StartDetectionInternal(callback, spawnPosition, allowedFalsePositives);
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

		public static WallHackDetector get_Instance()
		{
			return instance;
		}

		private static void set_Instance(WallHackDetector value)
		{
			instance = value;
		}

		private static WallHackDetector GetOrCreate()
		{
			if (get_Instance() != null)
			{
				return get_Instance();
			}
			if (ActDetectorBase.detectorsContainer == null)
			{
				ActDetectorBase.detectorsContainer = new GameObject("Anti-Cheat Toolkit Detectors");
			}
			set_Instance(ActDetectorBase.detectorsContainer.AddComponent<WallHackDetector>());
			return get_Instance();
		}

		private void Awake()
		{
			instancesInScene++;
			if (Init(get_Instance(), "WallHack Detector"))
			{
				set_Instance(this);
			}
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			StopAllCoroutines();
			if (serviceContainer != null)
			{
				UnityEngine.Object.Destroy(serviceContainer);
			}
			if (wfMaterial != null)
			{
				wfMaterial.mainTexture = null;
				wfMaterial.shader = null;
				wfMaterial = null;
				wfShader = null;
				shaderTexture = null;
				targetTexture = null;
				renderTexture.DiscardContents();
				renderTexture.Release();
				renderTexture = null;
			}
			instancesInScene--;
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
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

		private void FixedUpdate()
		{
			if (isRunning && checkRigidbody && !(rigidPlayer == null) && rigidPlayer.transform.localPosition.z > 1f)
			{
				rigidbodyDetections++;
				if (!Detect())
				{
					StopRigidModule();
					StartRigidModule();
				}
			}
		}

		private void Update()
		{
			if (!isRunning || !checkController || charControllerPlayer == null || !(charControllerVelocity > 0f))
			{
				return;
			}
			charControllerPlayer.Move(new Vector3(UnityEngine.Random.Range(-0.002f, 0.002f), 0f, charControllerVelocity));
			if (charControllerPlayer.transform.localPosition.z > 1f)
			{
				controllerDetections++;
				if (!Detect())
				{
					StopControllerModule();
					StartControllerModule();
				}
			}
		}

		private void StartDetectionInternal(UnityAction callback, Vector3 MDCJBPDNAOG, byte allowedFalsePositives)
		{
			if (isRunning)
			{
				UnityEngine.Debug.LogWarning("[ACTk] WallHack Detector: already running!", this);
				return;
			}
			if (!base.enabled)
			{
				UnityEngine.Debug.LogWarning("[ACTk] WallHack Detector: disabled but StartDetection still called from somewhere (see stack trace for this message)!", this);
				return;
			}
			if (callback != null && detectionEventHasListener)
			{
				UnityEngine.Debug.LogWarning("[ACTk] WallHack Detector: has properly configured Detection Event in the inspector, but still get started with Action callback. Both Action and Detection Event will be called on detection. Are you sure you wish to do this?", this);
			}
			if (callback == null && !detectionEventHasListener)
			{
				UnityEngine.Debug.LogWarning("[ACTk] WallHack Detector: was started without any callbacks. Please configure Detection Event in the inspector, or pass the callback Action to the StartDetection method.", this);
				base.enabled = false;
				return;
			}
			detectionAction = callback;
			spawnPosition = MDCJBPDNAOG;
			maxFalsePositives = allowedFalsePositives;
			rigidbodyDetections = 0;
			controllerDetections = 0;
			wireframeDetections = 0;
			raycastDetections = 0;
			StartCoroutine(InitDetector());
			started = true;
			isRunning = true;
		}

		protected override void StartDetectionAutomatically()
		{
			StartDetectionInternal(null, spawnPosition, maxFalsePositives);
		}

		protected override void PauseDetector()
		{
			if (isRunning)
			{
				isRunning = false;
				StopRigidModule();
				StopControllerModule();
				StopWireframeModule();
				StopRaycastModule();
			}
		}

		protected override void ResumeDetector()
		{
			if (detectionAction != null || detectionEventHasListener)
			{
				isRunning = true;
				if (checkRigidbody)
				{
					StartRigidModule();
				}
				if (checkController)
				{
					StartControllerModule();
				}
				if (checkWireframe)
				{
					StartWireframeModule();
				}
				if (checkRaycast)
				{
					StartRaycastModule();
				}
			}
		}

		protected override void StopDetectionInternal()
		{
			if (started)
			{
				PauseDetector();
				detectionAction = null;
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

		private void UpdateServiceContainer()
		{
			if (base.enabled && base.gameObject.activeSelf)
			{
				if (whLayer == -1)
				{
					whLayer = LayerMask.NameToLayer("Ignore Raycast");
				}
				if (raycastMask == -1)
				{
					raycastMask = LayerMask.GetMask("Ignore Raycast");
				}
				if (serviceContainer == null)
				{
					serviceContainer = new GameObject("[WH Detector Service]");
					serviceContainer.layer = whLayer;
					serviceContainer.transform.position = spawnPosition;
					UnityEngine.Object.DontDestroyOnLoad(serviceContainer);
				}
				if ((checkRigidbody || checkController) && solidWall == null)
				{
					solidWall = new GameObject("SolidWall");
					solidWall.AddComponent<BoxCollider>();
					solidWall.layer = whLayer;
					solidWall.transform.parent = serviceContainer.transform;
					solidWall.transform.localScale = new Vector3(3f, 3f, 0.5f);
					solidWall.transform.localPosition = Vector3.zero;
				}
				else if (!checkRigidbody && !checkController && solidWall != null)
				{
					UnityEngine.Object.Destroy(solidWall);
				}
				if (checkWireframe && wfCamera == null)
				{
					if (wfShader == null)
					{
						wfShader = Shader.Find("Hidden/ACTk/WallHackTexture");
					}
					if (wfShader == null)
					{
						UnityEngine.Debug.LogError("[ACTk] WallHack Detector: can't find 'Hidden/ACTk/WallHackTexture' shader!\nPlease make sure you have it included at the Editor > Project Settings > Graphics.", this);
						checkWireframe = false;
					}
					else if (!wfShader.isSupported)
					{
						UnityEngine.Debug.LogError("[ACTk] WallHack Detector: can't detect wireframe cheats on this platform!", this);
						checkWireframe = false;
					}
					else
					{
						if (foregroundColor == Color.black)
						{
							foregroundColor = GenerateColor();
							do
							{
								backgroundColor = GenerateColor();
							}
							while (ColorsSimilar(foregroundColor, backgroundColor, 10));
						}
						if (shaderTexture == null)
						{
							shaderTexture = new Texture2D(4, 4, TextureFormat.RGB24, false);
							shaderTexture.filterMode = FilterMode.Point;
							Color[] array = new Color[16];
							for (int i = 0; i < 16; i++)
							{
								if (i < 8)
								{
									array[i] = foregroundColor;
								}
								else
								{
									array[i] = backgroundColor;
								}
							}
							shaderTexture.SetPixels(array, 0);
							shaderTexture.Apply();
						}
						if (renderTexture == null)
						{
							renderTexture = new RenderTexture(4, 4, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
							renderTexture.autoGenerateMips = false;
							renderTexture.filterMode = FilterMode.Point;
							renderTexture.Create();
						}
						if (targetTexture == null)
						{
							targetTexture = new Texture2D(4, 4, TextureFormat.RGB24, false);
							targetTexture.filterMode = FilterMode.Point;
						}
						if (wfMaterial == null)
						{
							wfMaterial = new Material(wfShader);
							wfMaterial.mainTexture = shaderTexture;
						}
						if (foregroundRenderer == null)
						{
							GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
							UnityEngine.Object.Destroy(gameObject.GetComponent<BoxCollider>());
							gameObject.name = "WireframeFore";
							gameObject.layer = whLayer;
							gameObject.transform.parent = serviceContainer.transform;
							gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
							foregroundRenderer = gameObject.GetComponent<MeshRenderer>();
							foregroundRenderer.sharedMaterial = wfMaterial;
							foregroundRenderer.shadowCastingMode = ShadowCastingMode.Off;
							foregroundRenderer.receiveShadows = false;
							foregroundRenderer.enabled = false;
						}
						if (backgroundRenderer == null)
						{
							GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Quad);
							UnityEngine.Object.Destroy(gameObject2.GetComponent<MeshCollider>());
							gameObject2.name = "WireframeBack";
							gameObject2.layer = whLayer;
							gameObject2.transform.parent = serviceContainer.transform;
							gameObject2.transform.localPosition = new Vector3(0f, 0f, 1f);
							gameObject2.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
							backgroundRenderer = gameObject2.GetComponent<MeshRenderer>();
							backgroundRenderer.sharedMaterial = wfMaterial;
							backgroundRenderer.shadowCastingMode = ShadowCastingMode.Off;
							backgroundRenderer.receiveShadows = false;
							backgroundRenderer.enabled = false;
						}
						if (wfCamera == null)
						{
							wfCamera = new GameObject("WireframeCamera").AddComponent<Camera>();
							wfCamera.gameObject.layer = whLayer;
							wfCamera.transform.parent = serviceContainer.transform;
							wfCamera.transform.localPosition = new Vector3(0f, 0f, -1f);
							wfCamera.clearFlags = CameraClearFlags.Color;
							wfCamera.backgroundColor = Color.black;
							wfCamera.orthographic = true;
							wfCamera.orthographicSize = 0.5f;
							wfCamera.nearClipPlane = 0.01f;
							wfCamera.farClipPlane = 2.1f;
							wfCamera.depth = 0f;
							wfCamera.renderingPath = RenderingPath.Forward;
							wfCamera.useOcclusionCulling = false;
							wfCamera.allowHDR = false;
							wfCamera.targetTexture = renderTexture;
							wfCamera.enabled = false;
						}
					}
				}
				else if (!checkWireframe && wfCamera != null)
				{
					UnityEngine.Object.Destroy(foregroundRenderer.gameObject);
					UnityEngine.Object.Destroy(backgroundRenderer.gameObject);
					wfCamera.targetTexture = null;
					UnityEngine.Object.Destroy(wfCamera.gameObject);
				}
				if (checkRaycast && thinWall == null)
				{
					thinWall = GameObject.CreatePrimitive(PrimitiveType.Plane);
					thinWall.name = "ThinWall";
					thinWall.layer = whLayer;
					thinWall.transform.parent = serviceContainer.transform;
					thinWall.transform.localScale = new Vector3(0.2f, 1f, 0.2f);
					thinWall.transform.localRotation = Quaternion.Euler(270f, 0f, 0f);
					thinWall.transform.localPosition = new Vector3(0f, 0f, 1.4f);
					UnityEngine.Object.Destroy(thinWall.GetComponent<Renderer>());
					UnityEngine.Object.Destroy(thinWall.GetComponent<MeshFilter>());
				}
				else if (!checkRaycast && thinWall != null)
				{
					UnityEngine.Object.Destroy(thinWall);
				}
			}
			else if (serviceContainer != null)
			{
				UnityEngine.Object.Destroy(serviceContainer);
			}
		}

		private IEnumerator InitDetector()
		{
			yield return waitForEndOfFrame;
			UpdateServiceContainer();
			if (checkRigidbody)
			{
				StartRigidModule();
			}
			if (checkController)
			{
				StartControllerModule();
			}
			if (checkWireframe)
			{
				StartWireframeModule();
			}
			if (checkRaycast)
			{
				StartRaycastModule();
			}
		}

		private void StartRigidModule()
		{
			if (!checkRigidbody)
			{
				StopRigidModule();
				UninitRigidModule();
				UpdateServiceContainer();
				return;
			}
			if (!rigidPlayer)
			{
				InitRigidModule();
			}
			if (rigidPlayer.transform.localPosition.z <= 1f && rigidbodyDetections > 0)
			{
				rigidbodyDetections = 0;
			}
			rigidPlayer.rotation = Quaternion.identity;
			rigidPlayer.angularVelocity = Vector3.zero;
			rigidPlayer.transform.localPosition = new Vector3(0.75f, 0f, -1f);
#if UNITY_6000_0_OR_NEWER
			rigidPlayer.linearVelocity = rigidPlayerVelocity;
#else
			rigidPlayer.velocity = PKIHJKCDMHD;
#endif
			Invoke("StartRigidModule", 4f);
		}

		private void StartControllerModule()
		{
			if (!checkController)
			{
				StopControllerModule();
				UninitControllerModule();
				UpdateServiceContainer();
				return;
			}
			if (!charControllerPlayer)
			{
				InitControllerModule();
			}
			if (charControllerPlayer.transform.localPosition.z <= 1f && controllerDetections > 0)
			{
				controllerDetections = 0;
			}
			charControllerPlayer.transform.localPosition = new Vector3(-0.75f, 0f, -1f);
			charControllerVelocity = 0.01f;
			Invoke("StartControllerModule", 4f);
		}

		private void StartWireframeModule()
		{
			if (!checkWireframe)
			{
				StopWireframeModule();
				UpdateServiceContainer();
			}
			else if (!wireframeDetected)
			{
				Invoke("ShootWireframeModule", wireframeDelay);
			}
		}

		private void ShootWireframeModule()
		{
			StartCoroutine(CaptureFrame());
			Invoke("ShootWireframeModule", wireframeDelay);
		}

		private IEnumerator CaptureFrame()
		{
			wfCamera.enabled = true;
			yield return waitForEndOfFrame;
			foregroundRenderer.enabled = true;
			backgroundRenderer.enabled = true;
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = renderTexture;
			wfCamera.Render();
			foregroundRenderer.enabled = false;
			backgroundRenderer.enabled = false;
			while (!renderTexture.IsCreated())
			{
				yield return waitForEndOfFrame;
			}
			targetTexture.ReadPixels(new Rect(0f, 0f, 4f, 4f), 0, 0, false);
			targetTexture.Apply();
			RenderTexture.active = active;
			if (wfCamera == null)
			{
				yield return null;
			}
			wfCamera.enabled = false;
			if (!(targetTexture.GetPixel(0, 3) != foregroundColor) && !(targetTexture.GetPixel(0, 1) != backgroundColor) && !(targetTexture.GetPixel(3, 3) != foregroundColor) && !(targetTexture.GetPixel(3, 1) != backgroundColor) && !(targetTexture.GetPixel(1, 3) != foregroundColor) && !(targetTexture.GetPixel(2, 3) != foregroundColor) && !(targetTexture.GetPixel(1, 1) != backgroundColor) && !(targetTexture.GetPixel(2, 1) != backgroundColor))
			{
				if (wireframeDetections > 0)
				{
					wireframeDetections = 0;
				}
			}
			else
			{
				wireframeDetections++;
				wireframeDetected = Detect();
			}
			yield return null;
		}

		private void StartRaycastModule()
		{
			if (!checkRaycast)
			{
				StopRaycastModule();
				UpdateServiceContainer();
			}
			else
			{
				Invoke("ShootRaycastModule", raycastDelay);
			}
		}

		private void ShootRaycastModule()
		{
			if (Physics.Raycast(serviceContainer.transform.position, serviceContainer.transform.TransformDirection(Vector3.forward), 1.5f, raycastMask))
			{
				if (raycastDetections > 0)
				{
					raycastDetections = 0;
				}
			}
			else
			{
				raycastDetections++;
				if (Detect())
				{
					return;
				}
			}
			Invoke("ShootRaycastModule", raycastDelay);
		}

		private void StopRigidModule()
		{
			if ((bool)rigidPlayer)
			{
#if UNITY_6000_0_OR_NEWER
				rigidPlayer.linearVelocity = Vector3.zero;
#else
				rigidPlayer.velocity = Vector3.zero;
#endif
			}
			CancelInvoke("StartRigidModule");
		}

		private void StopControllerModule()
		{
			if ((bool)charControllerPlayer)
			{
				charControllerVelocity = 0f;
			}
			CancelInvoke("StartControllerModule");
		}

		private void StopWireframeModule()
		{
			CancelInvoke("ShootWireframeModule");
		}

		private void StopRaycastModule()
		{
			CancelInvoke("ShootRaycastModule");
		}

		private void InitRigidModule()
		{
			GameObject gameObject = new GameObject("RigidPlayer");
			gameObject.AddComponent<CapsuleCollider>().height = 2f;
			gameObject.layer = whLayer;
			gameObject.transform.parent = serviceContainer.transform;
			gameObject.transform.localPosition = new Vector3(0.75f, 0f, -1f);
			rigidPlayer = gameObject.AddComponent<Rigidbody>();
			rigidPlayer.useGravity = false;
		}

		private void InitControllerModule()
		{
			GameObject gameObject = new GameObject("ControlledPlayer");
			gameObject.AddComponent<CapsuleCollider>().height = 2f;
			gameObject.layer = whLayer;
			gameObject.transform.parent = serviceContainer.transform;
			gameObject.transform.localPosition = new Vector3(-0.75f, 0f, -1f);
			charControllerPlayer = gameObject.AddComponent<CharacterController>();
		}

		private void UninitRigidModule()
		{
			if ((bool)rigidPlayer)
			{
				UnityEngine.Object.Destroy(rigidPlayer.gameObject);
				rigidPlayer = null;
			}
		}

		private void UninitControllerModule()
		{
			if ((bool)charControllerPlayer)
			{
				UnityEngine.Object.Destroy(charControllerPlayer.gameObject);
				charControllerPlayer = null;
			}
		}

		private bool Detect()
		{
			bool result = false;
			if (controllerDetections > maxFalsePositives || rigidbodyDetections > maxFalsePositives || wireframeDetections > maxFalsePositives || raycastDetections > maxFalsePositives)
			{
				OnCheatingDetected();
				result = true;
			}
			return result;
		}

		private static Color32 GenerateColor()
		{
			return new Color32((byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256), byte.MaxValue);
		}

		private static bool ColorsSimilar(Color32 first, Color32 second, int tolerance)
		{
			return Math.Abs(first.r - second.r) < tolerance && Math.Abs(first.g - second.g) < tolerance && Math.Abs(first.b - second.b) < tolerance;
		}
	}
}
