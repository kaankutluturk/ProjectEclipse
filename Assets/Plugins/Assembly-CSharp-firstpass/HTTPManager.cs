using System;
using System.Collections.Generic;
using System.Diagnostics;
using BestHTTP;
using Org.BouncyCastle.Crypto.Tls;
using UnityEngine;

public static class HTTPManager
{
	private static byte maxConnectionPerServer;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool keepAliveDefaultValue;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool isCachingDisabled;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static TimeSpan maxConnectionIdleTime;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool isCookiesEnabled;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static uint cookieJarSize;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool enablePrivateBrowsing;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static TimeSpan connectTimeout;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static TimeSpan requestTimeout;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static Func<string> rootCacheFolderProvider;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static HTTPProxy proxy;

	private static HeartbeatManager heartbeats;

	private static ILogger logger;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static ICertificateVerifyer defaultCertificateVerifyer;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool useAlternateSslDefaultValue;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static int maxPathLength;

	private static Dictionary<string, List<HTTPConnection>> connections;

	private static List<HTTPConnection> activeConnections;

	private static List<HTTPConnection> freeConnections;

	private static List<HTTPConnection> recycledConnections;

	private static List<HTTPRequest> requestQueue;

	private static bool isCallingCallbacks;

	internal static object Locker;

	public static byte MaxConnectionsPerServer
	{
		get
		{
			return GetMaxConnectionPerServer();
		}
		set
		{
			set_MaxConnectionPerServer(value);
		}
	}

	public static bool KeepAliveDefaultValue
	{
		get
		{
			return GetKeepAliveDefaultValue();
		}
		set
		{
			SetKeepAliveDefaultValue(value);
		}
	}

	public static bool IsCachingDisabled
	{
		get
		{
			return GetIsCachingDisabled();
		}
		set
		{
			SetIsCachingDisabled(value);
		}
	}

	public static TimeSpan MaxConnectionIdleTime
	{
		get
		{
			return GetMaxConnectionIdleTime();
		}
		set
		{
			SetMaxConnectionIdleTime(value);
		}
	}

	public static bool IsCookiesEnabled
	{
		get
		{
			return GetIsCookiesEnabled();
		}
		set
		{
			SetIsCookiesEnabled(value);
		}
	}

	public static bool EnablePrivateBrowsing
	{
		get
		{
			return GetEnablePrivateBrowsing();
		}
		set
		{
			SetEnablePrivateBrowsing(value);
		}
	}

	public static TimeSpan ConnectTimeout
	{
		get
		{
			return GetConnectTimeout();
		}
		set
		{
			SetConnectTimeout(value);
		}
	}

	public static TimeSpan RequestTimeout
	{
		get
		{
			return GetRequestTimeout();
		}
		set
		{
			SetRequestTimeout(value);
		}
	}

	public static Func<string> CacheFolderProvider
	{
		get
		{
			return GetRootCacheFolderProvider();
		}
		set
		{
			set_RootCacheFolderProvider(value);
		}
	}

	public static HTTPProxy Proxy
	{
		get
		{
			return GetProxy();
		}
		set
		{
			SetProxy(value);
		}
	}

	public static HeartbeatManager Heartbeats
	{
		get
		{
			return GetHeartbeats();
		}
	}

	public static ILogger Logger
	{
		get
		{
			return GetLogger();
		}
		set
		{
			SetLogger(value);
		}
	}

	public static ICertificateVerifyer DefaultCertificateVerifyer
	{
		get
		{
			return GetDefaultCertificateVerifyer();
		}
		set
		{
			SetDefaultCertificateVerifyer(value);
		}
	}

	public static bool UseAlternateSSLDefaultValue
	{
		get
		{
			return GetUseAlternateSSLDefaultValue();
		}
		set
		{
			SetUseAlternateSSLDefaultValue(value);
		}
	}

	internal static int PathLengthLimit
	{
		get
		{
			return GetMaxPathLength();
		}
		set
		{
			set_MaxPathLength(value);
		}
	}

	static HTTPManager()
	{
		connections = new Dictionary<string, List<HTTPConnection>>();
		activeConnections = new List<HTTPConnection>();
		freeConnections = new List<HTTPConnection>();
		recycledConnections = new List<HTTPConnection>();
		requestQueue = new List<HTTPRequest>();
		Locker = new object();
		set_MaxConnectionPerServer(4);
		SetKeepAliveDefaultValue(true);
		set_MaxPathLength(255);
		SetMaxConnectionIdleTime(TimeSpan.FromSeconds(30.0));
		SetIsCookiesEnabled(true);
		set_CookieJarSize(10485760u);
		SetEnablePrivateBrowsing(false);
		SetConnectTimeout(TimeSpan.FromSeconds(20.0));
		SetRequestTimeout(TimeSpan.FromSeconds(60.0));
		logger = new DefaultLogger();
		SetDefaultCertificateVerifyer(null);
		SetUseAlternateSSLDefaultValue(false);
	}

	public static byte GetMaxConnectionPerServer()
	{
		return maxConnectionPerServer;
	}

	public static void set_MaxConnectionPerServer(byte value)
	{
		if (value <= 0)
		{
			throw new ArgumentOutOfRangeException("MaxConnectionPerServer must be greater than 0!");
		}
		maxConnectionPerServer = value;
	}

	public static bool GetKeepAliveDefaultValue()
	{
		return keepAliveDefaultValue;
	}

	public static void SetKeepAliveDefaultValue(bool value)
	{
		keepAliveDefaultValue = value;
	}

	public static bool GetIsCachingDisabled()
	{
		return isCachingDisabled;
	}

	public static void SetIsCachingDisabled(bool value)
	{
		isCachingDisabled = value;
	}

	public static TimeSpan GetMaxConnectionIdleTime()
	{
		return maxConnectionIdleTime;
	}

	public static void SetMaxConnectionIdleTime(TimeSpan value)
	{
		maxConnectionIdleTime = value;
	}

	public static bool GetIsCookiesEnabled()
	{
		return isCookiesEnabled;
	}

	public static void SetIsCookiesEnabled(bool value)
	{
		isCookiesEnabled = value;
	}

	public static uint GetCookieJarSize()
	{
		return cookieJarSize;
	}

	public static void set_CookieJarSize(uint value)
	{
		cookieJarSize = value;
	}

	public static bool GetEnablePrivateBrowsing()
	{
		return enablePrivateBrowsing;
	}

	public static void SetEnablePrivateBrowsing(bool value)
	{
		enablePrivateBrowsing = value;
	}

	public static TimeSpan GetConnectTimeout()
	{
		return connectTimeout;
	}

	public static void SetConnectTimeout(TimeSpan value)
	{
		connectTimeout = value;
	}

	public static TimeSpan GetRequestTimeout()
	{
		return requestTimeout;
	}

	public static void SetRequestTimeout(TimeSpan value)
	{
		requestTimeout = value;
	}

	public static Func<string> GetRootCacheFolderProvider()
	{
		return rootCacheFolderProvider;
	}

	public static void set_RootCacheFolderProvider(Func<string> value)
	{
		rootCacheFolderProvider = value;
	}

	public static HTTPProxy GetProxy()
	{
		return proxy;
	}

	public static void SetProxy(HTTPProxy value)
	{
		proxy = value;
	}

	public static HeartbeatManager GetHeartbeats()
	{
		if (heartbeats == null)
		{
			heartbeats = new HeartbeatManager();
		}
		return heartbeats;
	}

	public static ILogger GetLogger()
	{
		if (logger == null)
		{
			logger = new DefaultLogger();
			logger.SetLevel(Loglevels.None);
		}
		return logger;
	}

	public static void SetLogger(ILogger value)
	{
		logger = value;
	}

	public static ICertificateVerifyer GetDefaultCertificateVerifyer()
	{
		return defaultCertificateVerifyer;
	}

	public static void SetDefaultCertificateVerifyer(ICertificateVerifyer value)
	{
		defaultCertificateVerifyer = value;
	}

	public static bool GetUseAlternateSSLDefaultValue()
	{
		return useAlternateSslDefaultValue;
	}

	public static void SetUseAlternateSSLDefaultValue(bool value)
	{
		useAlternateSslDefaultValue = value;
	}

	internal static int GetMaxPathLength()
	{
		return maxPathLength;
	}

	internal static void set_MaxPathLength(int value)
	{
		maxPathLength = value;
	}

	public static void Setup()
	{
		HTTPUpdateDelegator.CheckInstance();
		HTTPCacheService.CheckSetup();
		CookieJar.SetupFolder();
	}

	public static HTTPRequest SendRequest(string url, OnRequestFinishedDelegate callback)
	{
		return SendRequest(new HTTPRequest(new Uri(url), HTTPMethods.Get, callback));
	}

	public static HTTPRequest SendRequest(string url, HTTPMethods methodType, OnRequestFinishedDelegate callback)
	{
		return SendRequest(new HTTPRequest(new Uri(url), methodType, callback));
	}

	public static HTTPRequest SendRequest(string url, HTTPMethods methodType, bool isKeepAlive, OnRequestFinishedDelegate callback)
	{
		return SendRequest(new HTTPRequest(new Uri(url), methodType, isKeepAlive, callback));
	}

	public static HTTPRequest SendRequest(string url, HTTPMethods methodType, bool isKeepAlive, bool disableCache, OnRequestFinishedDelegate callback)
	{
		return SendRequest(new HTTPRequest(new Uri(url), methodType, isKeepAlive, disableCache, callback));
	}

	public static HTTPRequest SendRequest(HTTPRequest request)
	{
		request.set_Exception(new NotSupportedException("Network services are disabled in the offline build."));
		request.set_State(HTTPRequestStates.Error);
		request.GetCallback()?.Invoke(request, null);
		return request;
	}

	public static GeneralStatistics GetGeneralStatistics(StatisticsQueryFlags queryFlags)
	{
		GeneralStatistics result = new GeneralStatistics
		{
			QueryFlags = queryFlags
		};
		if ((queryFlags & StatisticsQueryFlags.Connections) != 0)
		{
			int num = 0;
			foreach (KeyValuePair<string, List<HTTPConnection>> item in connections)
			{
				if (item.Value != null)
				{
					num += item.Value.Count;
				}
			}
			result.Connections = num;
			result.ActiveConnections = activeConnections.Count;
			result.FreeConnections = freeConnections.Count;
			result.RecycledConnections = recycledConnections.Count;
			result.RequestsInQueue = requestQueue.Count;
		}
		if ((queryFlags & StatisticsQueryFlags.Cache) != 0)
		{
			result.CacheEntityCount = HTTPCacheService.GetCacheEntityCount();
			result.CacheSize = HTTPCacheService.GetCacheSize();
		}
		if ((queryFlags & StatisticsQueryFlags.Cookies) != 0)
		{
			List<Cookie> list = CookieJar.GetAll();
			result.CookieCount = list.Count;
			uint num2 = 0u;
			for (int i = 0; i < list.Count; i++)
			{
				num2 += list[i].GuessSize();
			}
			result.CookieJarSize = num2;
		}
		return result;
	}

	private static void SendRequestImpl(HTTPRequest request)
	{
		HTTPConnection connection = FindOrCreateFreeConnection(request);
		if (connection != null)
		{
			if (activeConnections.Find((HTTPConnection candidate) => candidate == connection) == null)
			{
				activeConnections.Add(connection);
			}
			freeConnections.Remove(connection);
			request.set_State(HTTPRequestStates.Processing);
			request.Prepare();
			connection.Process(request);
		}
		else
		{
			request.set_State(HTTPRequestStates.Queued);
			requestQueue.Add(request);
		}
	}

	private static string GetKeyForRequest(HTTPRequest request)
	{
		return ((request.GetProxy() == null) ? string.Empty : new UriBuilder(request.GetProxy().GetAddress().Scheme, request.GetProxy().GetAddress().Host, request.GetProxy().GetAddress().Port).Uri.ToString()) + new UriBuilder(request.GetCurrentUri().Scheme, request.GetCurrentUri().Host, request.GetCurrentUri().Port).Uri.ToString();
	}

	private static HTTPConnection FindOrCreateFreeConnection(HTTPRequest request)
	{
		HTTPConnection connection = null;
		string text = GetKeyForRequest(request);
		List<HTTPConnection> value;
		if (connections.TryGetValue(text, out value))
		{
			int num = 0;
			for (int i = 0; i < value.Count; i++)
			{
				if (value[i].GetIsActive())
				{
					num++;
				}
			}
			if (num <= GetMaxConnectionPerServer())
			{
				for (int j = 0; j < value.Count; j++)
				{
					if (connection != null)
					{
						break;
					}
					HTTPConnection candidate = value[j];
					if (candidate != null && candidate.GetIsFree() && (!candidate.GetHasProxy() || candidate.GetLastProcessedUri() == null || candidate.GetLastProcessedUri().Host.Equals(request.GetCurrentUri().Host, StringComparison.OrdinalIgnoreCase)))
					{
						connection = candidate;
					}
				}
			}
		}
		else
		{
			connections.Add(text, value = new List<HTTPConnection>(GetMaxConnectionPerServer()));
		}
		if (connection == null)
		{
			if (value.Count >= GetMaxConnectionPerServer())
			{
				return null;
			}
			value.Add(connection = new HTTPConnection(text));
		}
		return connection;
	}

	private static bool CanProcessFromQueue()
	{
		for (int i = 0; i < requestQueue.Count; i++)
		{
			if (FindOrCreateFreeConnection(requestQueue[i]) != null)
			{
				return true;
			}
		}
		return false;
	}

	private static void RecycleConnection(HTTPConnection connection)
	{
		connection.Recycle();
		recycledConnections.Add(connection);
	}

	internal static HTTPConnection GetConnectionWith(HTTPRequest request)
	{
		lock (Locker)
		{
			for (int i = 0; i < activeConnections.Count; i++)
			{
				HTTPConnection connection = activeConnections[i];
				if (connection.GetCurrentRequest() == request)
				{
					return connection;
				}
			}
			return null;
		}
	}

	internal static bool RemoveFromQueue(HTTPRequest request)
	{
		return requestQueue.Remove(request);
	}

	internal static string GetRootCacheFolder()
	{
		try
		{
			if (GetRootCacheFolderProvider() != null)
			{
				return GetRootCacheFolderProvider()();
			}
		}
		catch (Exception ex)
		{
			GetLogger().Exception("HTTPManager", "GetRootCacheFolder", ex);
		}
		return Application.persistentDataPath;
	}

	public static void OnUpdate()
	{
		lock (Locker)
		{
			isCallingCallbacks = true;
			try
			{
				for (int i = 0; i < activeConnections.Count; i++)
				{
					HTTPConnection connection = activeConnections[i];
					switch (connection.GetState())
					{
					case HTTPConnectionStates.Processing:
						connection.HandleProgressCallback();
						if (connection.GetCurrentRequest().GetUseStreaming() && connection.GetCurrentRequest().GetResponse() != null && connection.GetCurrentRequest().GetResponse().HasStreamedFragments())
						{
							connection.HandleCallback();
						}
						if (((!connection.GetCurrentRequest().GetUseStreaming() && connection.GetCurrentRequest().GetUploadStream() == null) || connection.GetCurrentRequest().GetEnableTimeoutForStreaming()) && DateTime.UtcNow - connection.GetStartTime() > connection.GetCurrentRequest().GetTimeout())
						{
							connection.Abort(HTTPConnectionStates.TimedOut);
						}
						break;
					case HTTPConnectionStates.TimedOut:
						if (DateTime.UtcNow - connection.GetTimedOutStart() > TimeSpan.FromMilliseconds(500.0))
						{
							GetLogger().Information("HTTPManager", "Hard aborting connection becouse of a long waiting TimedOut state");
							connection.GetCurrentRequest().SetResponse(null);
							connection.GetCurrentRequest().set_State(HTTPRequestStates.TimedOut);
							connection.HandleCallback();
							RecycleConnection(connection);
						}
						break;
					case HTTPConnectionStates.Redirected:
						SendRequest(connection.GetCurrentRequest());
						RecycleConnection(connection);
						break;
					case HTTPConnectionStates.WaitForRecycle:
						connection.GetCurrentRequest().FinishStreaming();
						connection.HandleCallback();
						RecycleConnection(connection);
						break;
					case HTTPConnectionStates.Upgraded:
						connection.HandleCallback();
						break;
					case HTTPConnectionStates.WaitForProtocolShutdown:
					{
						IProtocol protocol = connection.GetCurrentRequest().GetResponse() as IProtocol;
						if (protocol != null)
						{
							protocol.HandleEvents();
						}
						if (protocol == null || protocol.GetIsClosed())
						{
							connection.HandleCallback();
							connection.Dispose();
							RecycleConnection(connection);
						}
						break;
					}
					case HTTPConnectionStates.AbortRequested:
					{
						IProtocol protocol = connection.GetCurrentRequest().GetResponse() as IProtocol;
						if (protocol != null)
						{
							protocol.HandleEvents();
							if (protocol.GetIsClosed())
							{
								connection.HandleCallback();
								connection.Dispose();
								RecycleConnection(connection);
							}
						}
						break;
					}
					case HTTPConnectionStates.Closed:
						connection.GetCurrentRequest().FinishStreaming();
						connection.HandleCallback();
						RecycleConnection(connection);
						break;
					}
				}
			}
			finally
			{
				isCallingCallbacks = false;
			}
			if (recycledConnections.Count > 0)
			{
				for (int j = 0; j < recycledConnections.Count; j++)
				{
					HTTPConnection connection = recycledConnections[j];
					if (connection.GetIsFree())
					{
						activeConnections.Remove(connection);
						freeConnections.Add(connection);
					}
				}
				recycledConnections.Clear();
			}
			if (freeConnections.Count > 0)
			{
				for (int k = 0; k < freeConnections.Count; k++)
				{
					HTTPConnection staleConnection = freeConnections[k];
					if (staleConnection.GetIsRemovable())
					{
						List<HTTPConnection> value = null;
						if (connections.TryGetValue(staleConnection.GetServerAddress(), out value))
						{
							value.Remove(staleConnection);
						}
						staleConnection.Dispose();
						freeConnections.RemoveAt(k);
						k--;
					}
				}
			}
			if (CanProcessFromQueue())
			{
				if (requestQueue.Find((HTTPRequest request) => request.GetPriority() != 0) != null)
				{
					requestQueue.Sort((HTTPRequest first, HTTPRequest second) => first.GetPriority() - second.GetPriority());
				}
				HTTPRequest[] array = requestQueue.ToArray();
				requestQueue.Clear();
				for (int num = 0; num < array.Length; num++)
				{
					SendRequest(array[num]);
				}
			}
		}
		if (heartbeats != null)
		{
			heartbeats.Update();
		}
	}

	internal static void OnQuit()
	{
		lock (Locker)
		{
			HTTPCacheService.SaveLibrary();
			foreach (KeyValuePair<string, List<HTTPConnection>> item in connections)
			{
				foreach (HTTPConnection item2 in item.Value)
				{
					item2.Dispose();
				}
				item.Value.Clear();
			}
			connections.Clear();
		}
	}
}
