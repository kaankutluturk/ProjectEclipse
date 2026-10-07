using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Org.BouncyCastle.Crypto.Tls;
using UnityEngine;

public sealed class HTTPRequest : IEnumerator<HTTPRequest>, IDisposable, IEnumerator
{
	internal static readonly byte[] EOL = new byte[2] { 13, 10 };

	internal static readonly string[] MethodNames = new string[6]
	{
		HTTPMethods.Get.ToString().ToUpper(),
		HTTPMethods.Head.ToString().ToUpper(),
		HTTPMethods.Post.ToString().ToUpper(),
		HTTPMethods.Put.ToString().ToUpper(),
		HTTPMethods.Delete.ToString().ToUpper(),
		HTTPMethods.Patch.ToString().ToUpper()
	};

	public static int UploadChunkSize = 1024;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Uri uri;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private HTTPMethods methodType;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private byte[] rawData;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Stream uploadStream;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool disposeUploadStream;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool useUploadStreamLength;

	public OnUploadProgressDelegate OnUploadProgress;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private OnRequestFinishedDelegate callback;

	public OnDownloadProgressDelegate OnProgress;

	public OnRequestFinishedDelegate OnUpgraded;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool disableRetry;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isRedirected;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Uri redirectUri;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private HTTPResponse response;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private HTTPResponse proxyResponse;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Exception exception;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private object tag;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Credentials credentials;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private HTTPProxy proxy;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int maxRedirects;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool useAlternateSsl;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isCookiesEnabled;

	private List<Cookie> customCookies;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private HTTPFormUsage formUsage;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private HTTPRequestStates state;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int redirectCount;

	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Func<HTTPRequest, X509Certificate, X509Chain, bool> CustomCertificationValidator;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TimeSpan connectTimeout;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TimeSpan timeout;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool enableTimeoutForStreaming;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int priority;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ICertificateVerifyer customCertificateVerifyer;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private SupportedProtocols protocolHandler;

	private OnBeforeRedirectionDelegate onBeforeRedirection;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int downloaded;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int downloadLength;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool downloadProgressChanged;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private long uploaded;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private long uploadLength;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool uploadProgressChanged;

	private bool isKeepAlive;

	private bool disableCache;

	private int streamFragmentSize;

	private bool useStreaming;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Dictionary<string, List<string>> headers;

	private HTTPFormBase fieldCollector;

	private HTTPFormBase formImpl;

	HTTPRequest IEnumerator<HTTPRequest>.Current
	{
		get
		{
			return System_002ECollections_002EGeneric_002EIEnumerator_003CBestHTTP_002EHTTPRequest_003E_002Eget_Current();
		}
	}

	public Uri RequestUri
	{
		get
		{
			return GetUri();
		}
		private set
		{
			set_Uri(value);
		}
	}

	public HTTPMethods MethodType
	{
		get
		{
			return GetMethodType();
		}
		set
		{
			SetMethodType(value);
		}
	}

	public byte[] RequestRawData
	{
		get
		{
			return GetRawData();
		}
		set
		{
			set_RawData(value);
		}
	}

	public bool DisposeUploadStream
	{
		get
		{
			return GetDisposeUploadStream();
		}
		set
		{
			SetDisposeUploadStream(value);
		}
	}

	public bool UseUploadStreamLength
	{
		get
		{
			return GetUseUploadStreamLength();
		}
		set
		{
			SetUseUploadStreamLength(value);
		}
	}

	public bool IsKeepAlive
	{
		get
		{
			return GetIsKeepAlive();
		}
		set
		{
			SetIsKeepAlive(value);
		}
	}

	public bool DisableCache
	{
		get
		{
			return GetDisableCache();
		}
		set
		{
			SetDisableCache(value);
		}
	}

	public bool UseStreaming
	{
		get
		{
			return GetUseStreaming();
		}
		set
		{
			SetUseStreaming(value);
		}
	}

	public int StreamFragmentSize
	{
		get
		{
			return GetStreamFragmentSize();
		}
		set
		{
			SetStreamFragmentSize(value);
		}
	}

	public OnRequestFinishedDelegate Callback
	{
		get
		{
			return GetCallback();
		}
		set
		{
			SetCallback(value);
		}
	}

	public bool DisableRetry
	{
		get
		{
			return GetDisableRetry();
		}
		set
		{
			SetDisableRetry(value);
		}
	}

	public bool IsRedirected
	{
		get
		{
			return GetIsRedirected();
		}
		internal set
		{
			SetIsRedirected(value);
		}
	}

	public Uri RedirectUri
	{
		get
		{
			return GetRedirectUri();
		}
		internal set
		{
			SetRedirectUri(value);
		}
	}

	public Uri CurrentUri
	{
		get
		{
			return GetCurrentUri();
		}
	}

	public HTTPResponse CurrentResponse
	{
		get
		{
			return GetResponse();
		}
		internal set
		{
			SetResponse(value);
		}
	}

	public HTTPResponse ProxyResponse
	{
		get
		{
			return GetProxyResponse();
		}
		internal set
		{
			SetProxyResponse(value);
		}
	}

	public Exception RequestException
	{
		get
		{
			return GetException();
		}
		internal set
		{
			set_Exception(value);
		}
	}

	public object RequestTag
	{
		get
		{
			return GetTag();
		}
		set
		{
			set_Tag(value);
		}
	}

	public Credentials Credentials
	{
		get
		{
			return GetCredentials();
		}
		set
		{
			SetCredentials(value);
		}
	}

	public bool HasProxy
	{
		get
		{
			return GetHasProxy();
		}
	}

	public HTTPProxy Proxy
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

	public int MaxRedirects
	{
		get
		{
			return GetMaxRedirects();
		}
		set
		{
			SetMaxRedirects(value);
		}
	}

	public bool UseAlternateSSL
	{
		get
		{
			return GetUseAlternateSSL();
		}
		set
		{
			SetUseAlternateSSL(value);
		}
	}

	public bool IsCookiesEnabled
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

	public List<Cookie> Cookies
	{
		get
		{
			return GetCookies();
		}
		set
		{
			SetCookies(value);
		}
	}

	public HTTPFormUsage FormUsage
	{
		get
		{
			return GetFormUsage();
		}
		set
		{
			SetFormUsage(value);
		}
	}

	public HTTPRequestStates RequestState
	{
		get
		{
			return GetState();
		}
		internal set
		{
			set_State(value);
		}
	}

	public int RedirectCount
	{
		get
		{
			return GetRedirectCount();
		}
		internal set
		{
			SetRedirectCount(value);
		}
	}

	public TimeSpan ConnectTimeout
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

	public TimeSpan Timeout
	{
		get
		{
			return GetTimeout();
		}
		set
		{
			SetTimeout(value);
		}
	}

	public bool EnableTimeoutForStreaming
	{
		get
		{
			return GetEnableTimeoutForStreaming();
		}
		set
		{
			SetEnableTimeoutForStreaming(value);
		}
	}

	public int Priority
	{
		get
		{
			return GetPriority();
		}
		set
		{
			SetPriority(value);
		}
	}

	public ICertificateVerifyer CustomCertificateVerifyer
	{
		get
		{
			return GetCustomCertificateVerifyer();
		}
		set
		{
			SetCustomCertificateVerifyer(value);
		}
	}

	public SupportedProtocols ProtocolHandler
	{
		get
		{
			return GetProtocolHandler();
		}
		set
		{
			SetProtocolHandler(value);
		}
	}

	internal int Downloaded
	{
		get
		{
			return GetDownloaded();
		}
		set
		{
			SetDownloaded(value);
		}
	}

	internal int DownloadLength
	{
		get
		{
			return GetDownloadLength();
		}
		set
		{
			SetDownloadLength(value);
		}
	}

	internal bool DownloadProgressChanged
	{
		get
		{
			return GetDownloadProgressChanged();
		}
		set
		{
			SetDownloadProgressChanged(value);
		}
	}

	internal long UploadStreamLength
	{
		get
		{
			return GetUploadStreamLength();
		}
	}

	internal long Uploaded
	{
		get
		{
			return GetUploaded();
		}
		private set
		{
			SetUploaded(value);
		}
	}

	internal long UploadLength
	{
		get
		{
			return GetUploadLength();
		}
		private set
		{
			SetUploadLength(value);
		}
	}

	internal bool UploadProgressChanged
	{
		get
		{
			return GetUploadProgressChanged();
		}
		set
		{
			SetUploadProgressChanged(value);
		}
	}

	private Dictionary<string, List<string>> RequestHeaders
	{
		get
		{
			return GetHeaders();
		}
		set
		{
			set_Headers(value);
		}
	}

	public object EnumeratorCurrent
	{
		get
		{
			return this;
		}
	}

	public event Func<HTTPRequest, X509Certificate, X509Chain, bool> OnCustomCertificationValidation
	{
		add
		{
			AddCustomCertificationValidator(value);
		}
		remove
		{
			RemoveCustomCertificationValidator(value);
		}
	}

	public event OnBeforeRedirectionDelegate OnBeforeRedirection
	{
		add
		{
			AddOnBeforeRedirection(value);
		}
		remove
		{
			RemoveOnBeforeRedirection(value);
		}
	}

	public HTTPRequest(Uri KJHNCLAJMLO)
		: this(KJHNCLAJMLO, HTTPMethods.Get, HTTPManager.GetKeepAliveDefaultValue(), HTTPManager.GetIsCachingDisabled(), null)
	{
	}

	public HTTPRequest(Uri KJHNCLAJMLO, OnRequestFinishedDelegate callback)
		: this(KJHNCLAJMLO, HTTPMethods.Get, HTTPManager.GetKeepAliveDefaultValue(), HTTPManager.GetIsCachingDisabled(), callback)
	{
	}

	public HTTPRequest(Uri KJHNCLAJMLO, bool LLLAPINJJIJ, OnRequestFinishedDelegate callback)
		: this(KJHNCLAJMLO, HTTPMethods.Get, LLLAPINJJIJ, HTTPManager.GetIsCachingDisabled(), callback)
	{
	}

	public HTTPRequest(Uri KJHNCLAJMLO, bool LLLAPINJJIJ, bool JNCJAGIBJFL, OnRequestFinishedDelegate callback)
		: this(KJHNCLAJMLO, HTTPMethods.Get, LLLAPINJJIJ, JNCJAGIBJFL, callback)
	{
	}

	public HTTPRequest(Uri KJHNCLAJMLO, HTTPMethods AMFJIGAEHLD)
		: this(KJHNCLAJMLO, AMFJIGAEHLD, HTTPManager.GetKeepAliveDefaultValue(), HTTPManager.GetIsCachingDisabled() || AMFJIGAEHLD != HTTPMethods.Get, null)
	{
	}

	public HTTPRequest(Uri KJHNCLAJMLO, HTTPMethods AMFJIGAEHLD, OnRequestFinishedDelegate callback)
		: this(KJHNCLAJMLO, AMFJIGAEHLD, HTTPManager.GetKeepAliveDefaultValue(), HTTPManager.GetIsCachingDisabled() || AMFJIGAEHLD != HTTPMethods.Get, callback)
	{
	}

	public HTTPRequest(Uri KJHNCLAJMLO, HTTPMethods AMFJIGAEHLD, bool LLLAPINJJIJ, OnRequestFinishedDelegate callback)
		: this(KJHNCLAJMLO, AMFJIGAEHLD, LLLAPINJJIJ, HTTPManager.GetIsCachingDisabled() || AMFJIGAEHLD != HTTPMethods.Get, callback)
	{
	}

	public HTTPRequest(Uri KJHNCLAJMLO, HTTPMethods AMFJIGAEHLD, bool LLLAPINJJIJ, bool JNCJAGIBJFL, OnRequestFinishedDelegate callback)
	{
		set_Uri(KJHNCLAJMLO);
		SetMethodType(AMFJIGAEHLD);
		SetIsKeepAlive(LLLAPINJJIJ);
		SetDisableCache(JNCJAGIBJFL);
		SetCallback(callback);
		SetStreamFragmentSize(4096);
		SetDisableRetry(AMFJIGAEHLD == HTTPMethods.Post);
		SetMaxRedirects(int.MaxValue);
		SetRedirectCount(0);
		SetIsCookiesEnabled(HTTPManager.GetIsCookiesEnabled());
		int bAINMLLIKOL = 0;
		SetDownloadLength(bAINMLLIKOL);
		SetDownloaded(bAINMLLIKOL);
		SetDownloadProgressChanged(false);
		set_State(HTTPRequestStates.Initial);
		SetConnectTimeout(HTTPManager.GetConnectTimeout());
		SetTimeout(HTTPManager.GetRequestTimeout());
		SetEnableTimeoutForStreaming(false);
		SetProxy(HTTPManager.GetProxy());
		SetUseUploadStreamLength(true);
		SetDisposeUploadStream(true);
		SetCustomCertificateVerifyer(HTTPManager.GetDefaultCertificateVerifyer());
		SetUseAlternateSSL(HTTPManager.GetUseAlternateSSLDefaultValue());
	}

	public Uri GetUri()
	{
		return uri;
	}

	private void set_Uri(Uri value)
	{
		uri = value;
	}

	public HTTPMethods GetMethodType()
	{
		return methodType;
	}

	public void SetMethodType(HTTPMethods value)
	{
		methodType = value;
	}

	public byte[] GetRawData()
	{
		return rawData;
	}

	public void set_RawData(byte[] value)
	{
		rawData = value;
	}

	public Stream GetUploadStream()
	{
		return uploadStream;
	}

	public void set_UploadStream(Stream value)
	{
		uploadStream = value;
	}

	public bool GetDisposeUploadStream()
	{
		return disposeUploadStream;
	}

	public void SetDisposeUploadStream(bool value)
	{
		disposeUploadStream = value;
	}

	public bool GetUseUploadStreamLength()
	{
		return useUploadStreamLength;
	}

	public void SetUseUploadStreamLength(bool value)
	{
		useUploadStreamLength = value;
	}

	public bool GetIsKeepAlive()
	{
		return isKeepAlive;
	}

	public void SetIsKeepAlive(bool value)
	{
		if (GetState() == HTTPRequestStates.Processing)
		{
			throw new NotSupportedException("Changing the IsKeepAlive property while processing the request is not supported.");
		}
		isKeepAlive = value;
	}

	public bool GetDisableCache()
	{
		return disableCache;
	}

	public void SetDisableCache(bool value)
	{
		if (GetState() == HTTPRequestStates.Processing)
		{
			throw new NotSupportedException("Changing the DisableCache property while processing the request is not supported.");
		}
		disableCache = value;
	}

	public bool GetUseStreaming()
	{
		return useStreaming;
	}

	public void SetUseStreaming(bool value)
	{
		if (GetState() == HTTPRequestStates.Processing)
		{
			throw new NotSupportedException("Changing the UseStreaming property while processing the request is not supported.");
		}
		useStreaming = value;
	}

	public int GetStreamFragmentSize()
	{
		return streamFragmentSize;
	}

	public void SetStreamFragmentSize(int value)
	{
		if (GetState() == HTTPRequestStates.Processing)
		{
			throw new NotSupportedException("Changing the StreamFragmentSize property while processing the request is not supported.");
		}
		if (value < 1)
		{
			throw new ArgumentException("StreamFragmentSize must be at least 1.");
		}
		streamFragmentSize = value;
	}

	public OnRequestFinishedDelegate GetCallback()
	{
		return callback;
	}

	public void SetCallback(OnRequestFinishedDelegate value)
	{
		callback = value;
	}

	public bool GetDisableRetry()
	{
		return disableRetry;
	}

	public void SetDisableRetry(bool value)
	{
		disableRetry = value;
	}

	public bool GetIsRedirected()
	{
		return isRedirected;
	}

	internal void SetIsRedirected(bool value)
	{
		isRedirected = value;
	}

	public Uri GetRedirectUri()
	{
		return redirectUri;
	}

	internal void SetRedirectUri(Uri value)
	{
		redirectUri = value;
	}

	public Uri GetCurrentUri()
	{
		return (!GetIsRedirected()) ? GetUri() : GetRedirectUri();
	}

	public HTTPResponse GetResponse()
	{
		return response;
	}

	internal void SetResponse(HTTPResponse value)
	{
		response = value;
	}

	public HTTPResponse GetProxyResponse()
	{
		return proxyResponse;
	}

	internal void SetProxyResponse(HTTPResponse value)
	{
		proxyResponse = value;
	}

	public Exception GetException()
	{
		return exception;
	}

	internal void set_Exception(Exception value)
	{
		exception = value;
	}

	public object GetTag()
	{
		return tag;
	}

	public void set_Tag(object value)
	{
		tag = value;
	}

	public Credentials GetCredentials()
	{
		return credentials;
	}

	public void SetCredentials(Credentials value)
	{
		credentials = value;
	}

	public bool GetHasProxy()
	{
		return GetProxy() != null;
	}

	public HTTPProxy GetProxy()
	{
		return proxy;
	}

	public void SetProxy(HTTPProxy value)
	{
		proxy = value;
	}

	public int GetMaxRedirects()
	{
		return maxRedirects;
	}

	public void SetMaxRedirects(int value)
	{
		maxRedirects = value;
	}

	public bool GetUseAlternateSSL()
	{
		return useAlternateSsl;
	}

	public void SetUseAlternateSSL(bool value)
	{
		useAlternateSsl = value;
	}

	public bool GetIsCookiesEnabled()
	{
		return isCookiesEnabled;
	}

	public void SetIsCookiesEnabled(bool value)
	{
		isCookiesEnabled = value;
	}

	public List<Cookie> GetCookies()
	{
		if (customCookies == null)
		{
			customCookies = new List<Cookie>();
		}
		return customCookies;
	}

	public void SetCookies(List<Cookie> value)
	{
		customCookies = value;
	}

	public HTTPFormUsage GetFormUsage()
	{
		return formUsage;
	}

	public void SetFormUsage(HTTPFormUsage value)
	{
		formUsage = value;
	}

	public HTTPRequestStates GetState()
	{
		return state;
	}

	internal void set_State(HTTPRequestStates value)
	{
		state = value;
	}

	public int GetRedirectCount()
	{
		return redirectCount;
	}

	internal void SetRedirectCount(int value)
	{
		redirectCount = value;
	}

	public void AddCustomCertificationValidator(Func<HTTPRequest, X509Certificate, X509Chain, bool> value)
	{
		Func<HTTPRequest, X509Certificate, X509Chain, bool> func = CustomCertificationValidator;
		Func<HTTPRequest, X509Certificate, X509Chain, bool> func2;
		do
		{
			func2 = func;
			func = Interlocked.CompareExchange(ref CustomCertificationValidator, (Func<HTTPRequest, X509Certificate, X509Chain, bool>)Delegate.Combine(func2, value), func);
		}
		while ((object)func != func2);
	}

	public void RemoveCustomCertificationValidator(Func<HTTPRequest, X509Certificate, X509Chain, bool> value)
	{
		Func<HTTPRequest, X509Certificate, X509Chain, bool> func = CustomCertificationValidator;
		Func<HTTPRequest, X509Certificate, X509Chain, bool> func2;
		do
		{
			func2 = func;
			func = Interlocked.CompareExchange(ref CustomCertificationValidator, (Func<HTTPRequest, X509Certificate, X509Chain, bool>)Delegate.Remove(func2, value), func);
		}
		while ((object)func != func2);
	}

	public TimeSpan GetConnectTimeout()
	{
		return connectTimeout;
	}

	public void SetConnectTimeout(TimeSpan value)
	{
		connectTimeout = value;
	}

	public TimeSpan GetTimeout()
	{
		return timeout;
	}

	public void SetTimeout(TimeSpan value)
	{
		timeout = value;
	}

	public bool GetEnableTimeoutForStreaming()
	{
		return enableTimeoutForStreaming;
	}

	public void SetEnableTimeoutForStreaming(bool value)
	{
		enableTimeoutForStreaming = value;
	}

	public int GetPriority()
	{
		return priority;
	}

	public void SetPriority(int value)
	{
		priority = value;
	}

	public ICertificateVerifyer GetCustomCertificateVerifyer()
	{
		return customCertificateVerifyer;
	}

	public void SetCustomCertificateVerifyer(ICertificateVerifyer value)
	{
		customCertificateVerifyer = value;
	}

	public SupportedProtocols GetProtocolHandler()
	{
		return protocolHandler;
	}

	public void SetProtocolHandler(SupportedProtocols value)
	{
		protocolHandler = value;
	}

	public void AddOnBeforeRedirection(OnBeforeRedirectionDelegate value)
	{
		onBeforeRedirection = (OnBeforeRedirectionDelegate)Delegate.Combine(onBeforeRedirection, value);
	}

	public void RemoveOnBeforeRedirection(OnBeforeRedirectionDelegate value)
	{
		onBeforeRedirection = (OnBeforeRedirectionDelegate)Delegate.Remove(onBeforeRedirection, value);
	}

	internal int GetDownloaded()
	{
		return downloaded;
	}

	internal void SetDownloaded(int value)
	{
		downloaded = value;
	}

	internal int GetDownloadLength()
	{
		return downloadLength;
	}

	internal void SetDownloadLength(int value)
	{
		downloadLength = value;
	}

	internal bool GetDownloadProgressChanged()
	{
		return downloadProgressChanged;
	}

	internal void SetDownloadProgressChanged(bool value)
	{
		downloadProgressChanged = value;
	}

	internal long GetUploadStreamLength()
	{
		if (GetUploadStream() == null || !GetUseUploadStreamLength())
		{
			return -1L;
		}
		try
		{
			return GetUploadStream().Length;
		}
		catch
		{
			return -1L;
		}
	}

	internal long GetUploaded()
	{
		return uploaded;
	}

	private void SetUploaded(long value)
	{
		uploaded = value;
	}

	internal long GetUploadLength()
	{
		return uploadLength;
	}

	private void SetUploadLength(long value)
	{
		uploadLength = value;
	}

	internal bool GetUploadProgressChanged()
	{
		return uploadProgressChanged;
	}

	internal void SetUploadProgressChanged(bool value)
	{
		uploadProgressChanged = value;
	}

	private Dictionary<string, List<string>> GetHeaders()
	{
		return headers;
	}

	private void set_Headers(Dictionary<string, List<string>> value)
	{
		headers = value;
	}

	public void AddField(string LKABGPANBMH, string value)
	{
		AddField(LKABGPANBMH, value, Encoding.UTF8);
	}

	public void AddField(string LKABGPANBMH, string value, Encoding FOPOKALJIIJ)
	{
		if (fieldCollector == null)
		{
			fieldCollector = new HTTPFormBase();
		}
		fieldCollector.AddField(LKABGPANBMH, value, FOPOKALJIIJ);
	}

	public void AddBinaryData(string LKABGPANBMH, byte[] DMNBDBJNKME)
	{
		AddBinaryData(LKABGPANBMH, DMNBDBJNKME, null, null);
	}

	public void AddBinaryData(string LKABGPANBMH, byte[] DMNBDBJNKME, string PMFEIPCHENB)
	{
		AddBinaryData(LKABGPANBMH, DMNBDBJNKME, PMFEIPCHENB, null);
	}

	public void AddBinaryData(string LKABGPANBMH, byte[] DMNBDBJNKME, string PMFEIPCHENB, string KIDMMGJIEHJ)
	{
		if (fieldCollector == null)
		{
			fieldCollector = new HTTPFormBase();
		}
		fieldCollector.AddBinaryData(LKABGPANBMH, DMNBDBJNKME, PMFEIPCHENB, KIDMMGJIEHJ);
	}

	public void SetFields(WWWForm GHLEOIMGGMO)
	{
		SetFormUsage(HTTPFormUsage.Unity);
		formImpl = new UnityForm(GHLEOIMGGMO);
	}

	public void SetForm(HTTPFormBase HOELLMLEBAK)
	{
		formImpl = HOELLMLEBAK;
	}

	public void ClearForm()
	{
		formImpl = null;
		fieldCollector = null;
	}

	private HTTPFormBase SelectFormImplementation()
	{
		if (formImpl != null)
		{
			return formImpl;
		}
		if (fieldCollector == null)
		{
			return null;
		}
		switch (GetFormUsage())
		{
		case HTTPFormUsage.Automatic:
			if (fieldCollector.GetHasBinary() || fieldCollector.GetHasLongValue())
			{
				goto case HTTPFormUsage.Multipart;
			}
			goto case HTTPFormUsage.UrlEncoded;
		case HTTPFormUsage.UrlEncoded:
			formImpl = new HTTPUrlEncodedForm();
			break;
		case HTTPFormUsage.Multipart:
			formImpl = new HTTPMultiPartForm();
			break;
		case HTTPFormUsage.Unity:
			formImpl = new UnityForm();
			break;
		}
		formImpl.CopyFrom(fieldCollector);
		return formImpl;
	}

	public void AddHeader(string name, string value)
	{
		if (GetHeaders() == null)
		{
			set_Headers(new Dictionary<string, List<string>>());
		}
		List<string> list;
		if (!GetHeaders().TryGetValue(name, out list))
		{
			GetHeaders().Add(name, list = new List<string>(1));
		}
		list.Add(value);
	}

	public void SetHeader(string name, string value)
	{
		if (GetHeaders() == null)
		{
			set_Headers(new Dictionary<string, List<string>>());
		}
		List<string> list;
		if (!GetHeaders().TryGetValue(name, out list))
		{
			GetHeaders().Add(name, list = new List<string>(1));
		}
		list.Clear();
		list.Add(value);
	}

	public bool RemoveHeader(string name)
	{
		if (GetHeaders() == null)
		{
			return false;
		}
		return GetHeaders().Remove(name);
	}

	public bool HasHeader(string name)
	{
		return GetHeaders() != null && GetHeaders().ContainsKey(name);
	}

	public string GetFirstHeaderValue(string name)
	{
		if (GetHeaders() == null)
		{
			return null;
		}
		List<string> value = null;
		if (GetHeaders().TryGetValue(name, out value) && value.Count > 0)
		{
			return value[0];
		}
		return null;
	}

	public List<string> GetHeaderValues(string name)
	{
		if (GetHeaders() == null)
		{
			return null;
		}
		List<string> value = null;
		if (GetHeaders().TryGetValue(name, out value) && value.Count > 0)
		{
			return value;
		}
		return null;
	}

	public void RemoveHeaders()
	{
		if (GetHeaders() != null)
		{
			GetHeaders().Clear();
		}
	}

	public void SetRangeHeader(int DAENDDKBFGP)
	{
		SetHeader("Range", string.Format("bytes={0}-", DAENDDKBFGP));
	}

	public void SetRangeHeader(int DAENDDKBFGP, int PBKEPECFHCK)
	{
		SetHeader("Range", string.Format("bytes={0}-{1}", DAENDDKBFGP, PBKEPECFHCK));
	}

	private void SendHeaders(BinaryWriter ABJIEFMMIEK)
	{
		if (!HasHeader("Host"))
		{
			SetHeader("Host", GetCurrentUri().Authority);
		}
		if (GetIsRedirected() && !HasHeader("Referer"))
		{
			AddHeader("Referer", GetUri().ToString());
		}
		if (!HasHeader("Accept-Encoding"))
		{
			AddHeader("Accept-Encoding", "gzip, identity");
		}
		if (GetHasProxy() && !HasHeader("Proxy-Connection"))
		{
			AddHeader("Proxy-Connection", (!GetIsKeepAlive()) ? "Close" : "Keep-Alive");
		}
		if (!HasHeader("Connection"))
		{
			AddHeader("Connection", (!GetIsKeepAlive()) ? "Close, TE" : "Keep-Alive, TE");
		}
		if (!HasHeader("TE"))
		{
			AddHeader("TE", "identity");
		}
		if (!HasHeader("User-Agent"))
		{
			AddHeader("User-Agent", "BestHTTP");
		}
		long num = -1L;
		if (GetUploadStream() == null)
		{
			byte[] array = GetEntityBody();
			num = ((array != null) ? array.Length : 0);
			if (GetRawData() == null && (formImpl != null || (fieldCollector != null && !fieldCollector.GetIsEmpty())))
			{
				SelectFormImplementation();
				if (formImpl != null)
				{
					formImpl.PrepareRequest(this);
				}
			}
		}
		else
		{
			num = GetUploadStreamLength();
			if (num == -1)
			{
				SetHeader("Transfer-Encoding", "Chunked");
			}
			if (!HasHeader("Content-Type"))
			{
				SetHeader("Content-Type", "application/octet-stream");
			}
		}
		if (num != -1 && !HasHeader("Content-Length"))
		{
			SetHeader("Content-Length", num.ToString());
		}
		if (GetHasProxy() && GetProxy().GetCredentials() != null)
		{
			switch (GetProxy().GetCredentials().get_Type())
			{
			case AuthenticationTypes.Basic:
				SetHeader("Proxy-Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(GetProxy().GetCredentials().GetUserName() + ":" + GetProxy().GetCredentials().GetPassword())));
				break;
			case AuthenticationTypes.Unknown:
			case AuthenticationTypes.Digest:
			{
				Digest kHNAPCOOAEF = DigestStore.Get(GetProxy().GetAddress());
				if (kHNAPCOOAEF != null)
				{
					string text = kHNAPCOOAEF.GenerateResponseHeader(this, GetProxy().GetCredentials());
					if (!string.IsNullOrEmpty(text))
					{
						SetHeader("Proxy-Authorization", text);
					}
				}
				break;
			}
			}
		}
		if (GetCredentials() != null)
		{
			switch (GetCredentials().get_Type())
			{
			case AuthenticationTypes.Basic:
				SetHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(GetCredentials().GetUserName() + ":" + GetCredentials().GetPassword())));
				break;
			case AuthenticationTypes.Unknown:
			case AuthenticationTypes.Digest:
			{
				Digest kHNAPCOOAEF2 = DigestStore.Get(GetCurrentUri());
				if (kHNAPCOOAEF2 != null)
				{
					string text2 = kHNAPCOOAEF2.GenerateResponseHeader(this, GetCredentials());
					if (!string.IsNullOrEmpty(text2))
					{
						SetHeader("Authorization", text2);
					}
				}
				break;
			}
			}
		}
		List<Cookie> list = ((!GetIsCookiesEnabled()) ? null : CookieJar.Get(GetCurrentUri()));
		if (list == null || list.Count == 0)
		{
			list = customCookies;
		}
		else if (customCookies != null)
		{
			for (int i = 0; i < customCookies.Count; i++)
			{
				Cookie NINHECJLKDH = customCookies[i];
				int num2 = list.FindIndex((Cookie ILHDJDNPFKH) => ILHDJDNPFKH.get_Name().Equals(NINHECJLKDH.get_Name()));
				if (num2 >= 0)
				{
					list[num2] = NINHECJLKDH;
				}
				else
				{
					list.Add(NINHECJLKDH);
				}
			}
		}
		if (list != null && list.Count > 0)
		{
			bool flag = true;
			string text3 = string.Empty;
			bool flag2 = HTTPProtocolFactory.IsSecureProtocol(GetCurrentUri());
			SupportedProtocols oBBKIBFJEMI = HTTPProtocolFactory.GetProtocolFromUri(GetCurrentUri());
			foreach (Cookie item in list)
			{
				if ((!item.GetIsSecure() || (item.GetIsSecure() && flag2)) && (!item.GetIsHttpOnly() || (item.GetIsHttpOnly() && oBBKIBFJEMI == SupportedProtocols.HTTP)))
				{
					if (!flag)
					{
						text3 += "; ";
					}
					else
					{
						flag = false;
					}
					text3 += item.ToString();
					item.SetLastAccess(DateTime.UtcNow);
				}
			}
			SetHeader("Cookie", text3);
		}
		foreach (KeyValuePair<string, List<string>> item2 in GetHeaders())
		{
			byte[] buffer = (item2.Key + ": ").GetASCIIBytes();
			for (int num3 = 0; num3 < item2.Value.Count; num3++)
			{
				ABJIEFMMIEK.Write(buffer);
				ABJIEFMMIEK.Write(item2.Value[num3].GetASCIIBytes());
				ABJIEFMMIEK.Write(EOL);
			}
		}
	}

	public string DumpHeaders()
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (BinaryWriter aBJIEFMMIEK = new BinaryWriter(memoryStream))
			{
				SendHeaders(aBJIEFMMIEK);
				return memoryStream.ToArray().AsciiToString();
			}
		}
	}

	internal byte[] GetEntityBody()
	{
		if (GetRawData() != null)
		{
			return GetRawData();
		}
		if (formImpl != null || (fieldCollector != null && !fieldCollector.GetIsEmpty()))
		{
			SelectFormImplementation();
			if (formImpl != null)
			{
				return formImpl.GetData();
			}
		}
		return null;
	}

	internal void SendOutTo(Stream ABJIEFMMIEK)
	{
		try
		{
			BinaryWriter binaryWriter = new BinaryWriter(ABJIEFMMIEK);
			string text = string.Format("{0} {1} HTTP/1.1", MethodNames[(uint)GetMethodType()], (!GetHasProxy() || !GetProxy().GetSendWholeUri()) ? GetCurrentUri().PathAndQuery : GetCurrentUri().OriginalString);
			if (HTTPManager.GetLogger().GetLevel() <= Loglevels.Information)
			{
				HTTPManager.GetLogger().Information("HTTPRequest", string.Format("Sending request: {0}", text));
			}
			binaryWriter.Write(text.GetASCIIBytes());
			binaryWriter.Write(EOL);
			SendHeaders(binaryWriter);
			binaryWriter.Write(EOL);
			binaryWriter.Flush();
			byte[] array = GetRawData();
			if (array == null && formImpl != null)
			{
				array = formImpl.GetData();
			}
			if (array == null && GetUploadStream() == null)
			{
				return;
			}
			Stream stream = GetUploadStream();
			if (stream == null)
			{
				stream = new MemoryStream(array, 0, array.Length);
				SetUploadLength(array.Length);
			}
			else
			{
				SetUploadLength((!GetUseUploadStreamLength()) ? (-1) : GetUploadStreamLength());
			}
			SetUploaded(0L);
			byte[] array2 = new byte[UploadChunkSize];
			int num = 0;
			while ((num = stream.Read(array2, 0, array2.Length)) > 0)
			{
				if (!GetUseUploadStreamLength())
				{
					binaryWriter.Write(num.ToString("X").GetASCIIBytes());
					binaryWriter.Write(EOL);
				}
				binaryWriter.Write(array2, 0, num);
				if (!GetUseUploadStreamLength())
				{
					binaryWriter.Write(EOL);
				}
				binaryWriter.Flush();
				SetUploaded(GetUploaded() + num);
				SetUploadProgressChanged(true);
			}
			if (!GetUseUploadStreamLength())
			{
				binaryWriter.Write("0".GetASCIIBytes());
				binaryWriter.Write(EOL);
				binaryWriter.Write(EOL);
			}
			binaryWriter.Flush();
			if (GetUploadStream() == null && stream != null)
			{
				stream.Dispose();
			}
		}
		catch (Exception ex)
		{
			HTTPManager.GetLogger().Exception("HTTPRequest", "SendOutTo", ex);
			throw ex;
		}
		finally
		{
			if (GetUploadStream() != null && GetDisposeUploadStream())
			{
				GetUploadStream().Dispose();
			}
		}
	}

	internal void UpgradeCallback()
	{
		if (GetResponse() == null || !GetResponse().GetIsUpgraded())
		{
			return;
		}
		try
		{
			if (OnUpgraded != null)
			{
				OnUpgraded(this, GetResponse());
			}
		}
		catch (Exception mPFFFAOGBJE)
		{
			HTTPManager.GetLogger().Exception("HTTPRequest", "UpgradeCallback", mPFFFAOGBJE);
		}
	}

	internal void CallCallback()
	{
		try
		{
			if (GetCallback() != null)
			{
				GetCallback()(this, GetResponse());
			}
		}
		catch (Exception mPFFFAOGBJE)
		{
			HTTPManager.GetLogger().Exception("HTTPRequest", "CallCallback", mPFFFAOGBJE);
		}
	}

	internal bool CallOnBeforeRedirection(Uri JJCEFGDNEEO)
	{
		if (onBeforeRedirection != null)
		{
			return onBeforeRedirection(this, GetResponse(), JJCEFGDNEEO);
		}
		return true;
	}

	internal void FinishStreaming()
	{
		if (GetResponse() != null && GetUseStreaming())
		{
			GetResponse().FinishStreaming();
		}
	}

	internal void Prepare()
	{
		if (GetFormUsage() == HTTPFormUsage.Unity)
		{
			SelectFormImplementation();
		}
	}

	internal bool CallCustomCertificationValidator(X509Certificate DBCFDLIJOBD, X509Chain GCONPBMJDFL)
	{
		if (CustomCertificationValidator != null)
		{
			return CustomCertificationValidator(this, DBCFDLIJOBD, GCONPBMJDFL);
		}
		return true;
	}

	public HTTPRequest Send()
	{
		return HTTPManager.SendRequest(this);
	}

	public void Abort()
	{
		lock (HTTPManager.Locker)
		{
			if (GetState() >= HTTPRequestStates.Finished)
			{
				HTTPManager.GetLogger().Warning("HTTPRequest", string.Format("Abort - Already in a state({0}) where no Abort required!", GetState().ToString()));
				return;
			}
			HTTPConnection hPNEPPBEKGG = HTTPManager.GetConnectionWith(this);
			if (hPNEPPBEKGG == null)
			{
				if (!HTTPManager.RemoveFromQueue(this))
				{
					HTTPManager.GetLogger().Warning("HTTPRequest", "Abort - No active connection found with this request! (The request may already finished?)");
				}
				set_State(HTTPRequestStates.Aborted);
			}
			else
			{
				if (GetResponse() != null && GetResponse().GetIsStreamed())
				{
					GetResponse().Dispose();
				}
				hPNEPPBEKGG.Abort(HTTPConnectionStates.AbortRequested);
			}
		}
	}

	public void Clear()
	{
		ClearForm();
		RemoveHeaders();
	}

	public object Current
	{
		get
		{
			return this;
		}
	}

	public bool MoveNext()
	{
		return GetState() < HTTPRequestStates.Finished;
	}

	public void Reset()
	{
		throw new NotImplementedException();
	}

	private HTTPRequest System_002ECollections_002EGeneric_002EIEnumerator_003CBestHTTP_002EHTTPRequest_003E_002Eget_Current()
	{
		return this;
	}

	public void Dispose()
	{
	}
}
