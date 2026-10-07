using System;
using System.Collections;
using System.Diagnostics;

namespace Nekki.SF2.Core.Network
{
	public class ServerProvider : ServerProviderBase, IPurchaseVerifier
	{
		private static ServerProvider _Instance;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static string putUrl;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static string getUrl;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static string configUrl;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static string timeServerUrl;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static string userId;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static string dumpPutUrl;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static string dumpGetUrl;

		private static int loginInterval;

		public string PutServerUrl
		{
			get
			{
				return get_PutServer();
			}
		}

		public new static ServerProvider SharedInstance
		{
			get
			{
				return get_Instance();
			}
		}

		public static string PutEndpoint
		{
			get
			{
				return get_PutURL();
			}
			set
			{
				set_PutURL(value);
			}
		}

		public static string GetEndpoint
		{
			get
			{
				return get_GetURL();
			}
			set
			{
				set_GetURL(value);
			}
		}

		public static string ConfigEndpoint
		{
			get
			{
				return get_ConfigURL();
			}
			set
			{
				set_ConfigURL(value);
			}
		}

		public static string TimeServerEndpoint
		{
			get
			{
				return get_TimeServerURL();
			}
			set
			{
				set_TimeServerURL(value);
			}
		}

		public static string DumpPutEndpoint
		{
			get
			{
				return get_DumpPutURL();
			}
			set
			{
				set_DumpPutURL(value);
			}
		}

		public static string DumpGetEndpoint
		{
			get
			{
				return get_DumpGetURL();
			}
			set
			{
				set_DumpGetURL(value);
			}
		}

		public static int LoginIntervalSetting
		{
			get
			{
				return get_LoginInterval();
			}
			set
			{
				set_LoginInterval(value);
			}
		}

		protected override string GetServerUrl()
		{
			return get_GetURL();
		}

		public string get_PutServer()
		{
			return get_PutURL();
		}

		public new static ServerProvider get_Instance()
		{
			if (_Instance == null)
			{
				_Instance = ServerProviderBase.Init<ServerProvider>();
				_Instance.Init();
			}
			return _Instance;
		}

		public static string get_PutURL()
		{
			return putUrl;
		}

		public static void set_PutURL(string value)
		{
			putUrl = value;
		}

		public static string get_GetURL()
		{
			return getUrl;
		}

		public static void set_GetURL(string value)
		{
			getUrl = value;
		}

		public static string get_ConfigURL()
		{
			return configUrl;
		}

		public static void set_ConfigURL(string value)
		{
			configUrl = value;
		}

		public static string get_TimeServerURL()
		{
			return timeServerUrl;
		}

		public static void set_TimeServerURL(string value)
		{
			timeServerUrl = value;
		}

		public static string get_UserID()
		{
			return userId;
		}

		public static void set_UserID(string value)
		{
			userId = value;
		}

		public static string get_DumpPutURL()
		{
			return dumpPutUrl;
		}

		public static void set_DumpPutURL(string value)
		{
			dumpPutUrl = value;
		}

		public static string get_DumpGetURL()
		{
			return dumpGetUrl;
		}

		public static void set_DumpGetURL(string value)
		{
			dumpGetUrl = value;
		}

		public static int get_LoginInterval()
		{
			return loginInterval;
		}

		public static void set_LoginInterval(int value)
		{
			loginInterval = value;
		}

		public static void Reset()
		{
			loginInterval = 0;
		}

		protected override void Init()
		{
		}

		protected override IEnumerator TimeSyncRoutine(Action<long> onDone, Action<string> onError)
		{
			onDone?.Invoke((long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
			yield break;
		}

		public void VerifyPurchaseAction(PaymentInfo PAENLDALDGB, string DBKFOHCPLDB, Action<bool, string, object> p_delegate)
		{
			StartCoroutine(FailRequest(p_delegate, PAENLDALDGB));
		}

		public void ConfirmVerificationAction(PaymentInfo PAENLDALDGB, string DBKFOHCPLDB, Action<bool, string, object> p_delegate)
		{
			StartCoroutine(FailRequest(p_delegate, PAENLDALDGB));
		}

		public void SendGiveLogin(Action<bool, string, object> IKFMKMEHJFF)
		{
			StartCoroutine(FailRequest(IKFMKMEHJFF, null));
		}

		public void CheckLedger(string BEPKJNKCKPH, Action<bool, string, object> IKFMKMEHJFF)
		{
			StartCoroutine(FailRequest(IKFMKMEHJFF, null));
		}

		public void ConfirmLedger(string BEPKJNKCKPH, Action<bool, string, object> IKFMKMEHJFF, string DIAIIPCBMFL)
		{
			StartCoroutine(FailRequest(IKFMKMEHJFF, null));
		}

		// Retain callback timing and caller state without constructing remote payloads.
		private IEnumerator FailRequest(Action<bool, string, object> callback, object state)
		{
			callback?.Invoke(false, "offline build", state);
			yield break;
		}

		public const bool OFFLINE = true;

		public void DownloadFile(string p_url, Action<byte[], string, string> p_onDownloadComplete, Action<float> MDJEOHMECHA = null, int DGDKHFPEHOG = 0)
		{
			p_onDownloadComplete?.Invoke(new byte[0], "offline build", p_url);
		}
	}
}
