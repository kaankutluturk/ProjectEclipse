using UnityEngine;

namespace BestHTTP
{
	internal sealed class HTTPUpdateDelegator : MonoBehaviour
	{
		private static HTTPUpdateDelegator instance;

		private static bool IsCreated;

		public static void CheckInstance()
		{
			try
			{
				if (!IsCreated)
				{
					instance = Object.FindObjectOfType(typeof(HTTPUpdateDelegator)) as HTTPUpdateDelegator;
					if (instance == null)
					{
						GameObject gameObject = new GameObject("HTTP Update Delegator");
						gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
						Object.DontDestroyOnLoad(gameObject);
						instance = gameObject.AddComponent<HTTPUpdateDelegator>();
					}
					IsCreated = true;
				}
			}
			catch
			{
				HTTPManager.GetLogger().Error("HTTPUpdateDelegator", "Please call the BestHTTP.HTTPManager.Setup() from one of Unity's event(eg. awake, start) before you send any request!");
			}
		}

		private void Awake()
		{
			HTTPCacheService.SetupCacheFolder();
			CookieJar.SetupFolder();
			CookieJar.Load();
		}

		private void Update()
		{
			HTTPManager.OnUpdate();
		}

		private void OnApplicationQuit()
		{
			HTTPManager.OnQuit();
		}
	}
}
