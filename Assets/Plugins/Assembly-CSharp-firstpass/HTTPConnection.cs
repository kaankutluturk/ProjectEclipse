using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;
using SocketEx;

internal sealed class HTTPConnection : IDisposable
{
	private enum RetryCauses
	{
		None = 0,
		Reconnect = 1,
		Authenticate = 2,
		ProxyAuthenticate = 3
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string serverAddress;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private HTTPConnectionStates state;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private HTTPRequest currentRequest;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private DateTime startTime;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private DateTime timedOutStart;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private HTTPProxy proxy;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Uri lastProcessedUri;

	private TcpClient Client;

	private Stream Stream;

	private DateTime lastProcessTime;

	internal string ConnectionAddress
	{
		get
		{
			return GetServerAddress();
		}
		private set
		{
			set_ServerAddress(value);
		}
	}

	internal HTTPConnectionStates ConnectionState
	{
		get
		{
			return GetState();
		}
		private set
		{
			set_State(value);
		}
	}

	internal bool IsFree
	{
		get
		{
			return GetIsFree();
		}
	}

	internal bool IsActive
	{
		get
		{
			return GetIsActive();
		}
	}

	internal HTTPRequest CurrentRequest
	{
		get
		{
			return GetCurrentRequest();
		}
		private set
		{
			SetCurrentRequest(value);
		}
	}

	internal bool IsRemovable
	{
		get
		{
			return GetIsRemovable();
		}
	}

	internal DateTime StartTime
	{
		get
		{
			return GetStartTime();
		}
		private set
		{
			SetStartTime(value);
		}
	}

	internal DateTime TimedOutStart
	{
		get
		{
			return GetTimedOutStart();
		}
		private set
		{
			SetTimedOutStart(value);
		}
	}

	internal HTTPProxy Proxy
	{
		get
		{
			return GetProxy();
		}
		private set
		{
			SetProxy(value);
		}
	}

	internal bool HasProxy
	{
		get
		{
			return GetHasProxy();
		}
	}

	internal Uri ProcessedUri
	{
		get
		{
			return GetLastProcessedUri();
		}
		private set
		{
			set_LastProcessedUri(value);
		}
	}

	internal HTTPConnection(string serverAddress)
	{
		set_ServerAddress(serverAddress);
		set_State(HTTPConnectionStates.Initial);
		lastProcessTime = DateTime.UtcNow;
	}

	internal string GetServerAddress()
	{
		return serverAddress;
	}

	private void set_ServerAddress(string value)
	{
		serverAddress = value;
	}

	internal HTTPConnectionStates GetState()
	{
		return state;
	}

	private void set_State(HTTPConnectionStates value)
	{
		state = value;
	}

	internal bool GetIsFree()
	{
		return GetState() == HTTPConnectionStates.Initial || GetState() == HTTPConnectionStates.Free;
	}

	internal bool GetIsActive()
	{
		return GetState() > HTTPConnectionStates.Initial && GetState() < HTTPConnectionStates.Free;
	}

	internal HTTPRequest GetCurrentRequest()
	{
		return currentRequest;
	}

	private void SetCurrentRequest(HTTPRequest value)
	{
		currentRequest = value;
	}

	internal bool GetIsRemovable()
	{
		return GetIsFree() && DateTime.UtcNow - lastProcessTime > HTTPManager.GetMaxConnectionIdleTime();
	}

	internal DateTime GetStartTime()
	{
		return startTime;
	}

	private void SetStartTime(DateTime value)
	{
		startTime = value;
	}

	internal DateTime GetTimedOutStart()
	{
		return timedOutStart;
	}

	private void SetTimedOutStart(DateTime value)
	{
		timedOutStart = value;
	}

	internal HTTPProxy GetProxy()
	{
		return proxy;
	}

	private void SetProxy(HTTPProxy value)
	{
		proxy = value;
	}

	internal bool GetHasProxy()
	{
		return GetProxy() != null;
	}

	internal Uri GetLastProcessedUri()
	{
		return lastProcessedUri;
	}

	private void set_LastProcessedUri(Uri value)
	{
		lastProcessedUri = value;
	}

	internal void Process(HTTPRequest request)
	{
		if (GetState() == HTTPConnectionStates.Processing)
		{
			throw new Exception("Connection already processing a request!");
		}
		SetStartTime(DateTime.MaxValue);
		set_State(HTTPConnectionStates.Processing);
		SetCurrentRequest(request);
		new System.Threading.Thread(ThreadFunc).Start();
	}

	internal void Recycle()
	{
		if (GetState() == HTTPConnectionStates.TimedOut)
		{
			lastProcessTime = DateTime.MinValue;
		}
		set_State(HTTPConnectionStates.Free);
		SetCurrentRequest(null);
	}

	private void ThreadFunc(object threadState)
	{
		bool flag = false;
		bool flag2 = false;
		RetryCauses retryCause = RetryCauses.None;
		try
		{
			if (!GetHasProxy() && GetCurrentRequest().GetHasProxy())
			{
				SetProxy(GetCurrentRequest().GetProxy());
			}
			if (TryLoadAllFromCache())
			{
				return;
			}
			if (Client != null && !Client.IsConnected())
			{
				Close();
			}
			do
			{
				if (retryCause == RetryCauses.Reconnect)
				{
					Close();
					System.Threading.Thread.Sleep(100);
				}
				set_LastProcessedUri(GetCurrentRequest().GetCurrentUri());
				retryCause = RetryCauses.None;
				Connect();
				if (GetState() == HTTPConnectionStates.AbortRequested)
				{
					throw new Exception("AbortRequested");
				}
				if (!GetCurrentRequest().GetDisableCache())
				{
					HTTPCacheService.SetHeaders(GetCurrentRequest());
				}
				bool flag3 = false;
				try
				{
					GetCurrentRequest().SendOutTo(Stream);
					flag3 = true;
				}
				catch (Exception ex)
				{
					Close();
					if (GetState() == HTTPConnectionStates.TimedOut)
					{
						throw new Exception("AbortRequested");
					}
					if (flag || GetCurrentRequest().GetDisableRetry())
					{
						throw ex;
					}
					flag = true;
					retryCause = RetryCauses.Reconnect;
				}
				if (!flag3)
				{
					continue;
				}
				bool flag4 = Receive();
				if (GetState() == HTTPConnectionStates.TimedOut)
				{
					throw new Exception("AbortRequested");
				}
				if (!flag4 && !flag && !GetCurrentRequest().GetDisableRetry())
				{
					flag = true;
					retryCause = RetryCauses.Reconnect;
				}
				if (GetCurrentRequest().GetResponse() == null)
				{
					continue;
				}
				switch (GetCurrentRequest().GetResponse().GetStatusCode())
				{
				case 401:
				{
					string text3 = DigestStore.FindBest(GetCurrentRequest().GetResponse().GetHeaderValues("www-authenticate"));
					if (!string.IsNullOrEmpty(text3))
					{
						Digest kHNAPCOOAEF2 = DigestStore.GetOrCreate(GetCurrentRequest().GetCurrentUri());
						kHNAPCOOAEF2.ParseChallange(text3);
						if (GetCurrentRequest().GetCredentials() != null && kHNAPCOOAEF2.IsUriProtected(GetCurrentRequest().GetCurrentUri()) && (!GetCurrentRequest().HasHeader("Authorization") || kHNAPCOOAEF2.GetStale()))
						{
							retryCause = RetryCauses.Authenticate;
						}
					}
					break;
				}
				case 407:
				{
					if (!GetCurrentRequest().GetHasProxy())
					{
						break;
					}
					string text2 = DigestStore.FindBest(GetCurrentRequest().GetResponse().GetHeaderValues("proxy-authenticate"));
					if (!string.IsNullOrEmpty(text2))
					{
						Digest proxyDigest = DigestStore.GetOrCreate(GetCurrentRequest().GetProxy().GetAddress());
						proxyDigest.ParseChallange(text2);
						if (GetCurrentRequest().GetProxy().GetCredentials() != null && proxyDigest.IsUriProtected(GetCurrentRequest().GetProxy().GetAddress()) && (!GetCurrentRequest().HasHeader("Proxy-Authorization") || proxyDigest.GetStale()))
						{
							retryCause = RetryCauses.ProxyAuthenticate;
						}
					}
					break;
				}
				case 301:
				case 302:
				case 307:
				case 308:
					if (GetCurrentRequest().GetRedirectCount() < GetCurrentRequest().GetMaxRedirects())
					{
						HTTPRequest request = GetCurrentRequest();
						request.SetRedirectCount(request.GetRedirectCount() + 1);
						string text = GetCurrentRequest().GetResponse().GetFirstHeaderValue("location");
						if (string.IsNullOrEmpty(text))
						{
							throw new MissingFieldException(string.Format("Got redirect status({0}) without 'location' header!", GetCurrentRequest().GetResponse().GetStatusCode().ToString()));
						}
						Uri uri = GetRedirectUri(text);
						if (!GetCurrentRequest().CallOnBeforeRedirection(uri))
						{
							HTTPManager.GetLogger().Information("HTTPConnection", "OnBeforeRedirection returned False");
							break;
						}
						GetCurrentRequest().RemoveHeader("Host");
						GetCurrentRequest().SetHeader("Referer", GetCurrentRequest().GetCurrentUri().ToString());
						GetCurrentRequest().SetRedirectUri(uri);
						GetCurrentRequest().SetResponse(null);
						bool flag5 = true;
						GetCurrentRequest().SetIsRedirected(flag5);
						flag2 = flag5;
					}
					break;
				}
				if (GetCurrentRequest().GetIsCookiesEnabled())
				{
					CookieJar.Set(GetCurrentRequest().GetResponse());
				}
				TryStoreInCache();
				if (GetCurrentRequest().GetResponse() == null || (!GetCurrentRequest().GetResponse().GetIsClosedManually() && GetCurrentRequest().GetResponse().HasHeaderWithValue("connection", "close")))
				{
					Close();
				}
			}
			while (retryCause != RetryCauses.None);
		}
		catch (TimeoutException ex)
		{
			GetCurrentRequest().SetResponse(null);
			GetCurrentRequest().set_Exception(ex);
			GetCurrentRequest().set_State(HTTPRequestStates.ConnectionTimedOut);
			Close();
		}
		catch (Exception bAINMLLIKOL2)
		{
			if (GetCurrentRequest() != null)
			{
				if (GetCurrentRequest().GetUseStreaming())
				{
					HTTPCacheService.DeleteEntity(GetCurrentRequest().GetCurrentUri());
				}
				GetCurrentRequest().SetResponse(null);
				switch (GetState())
				{
				case HTTPConnectionStates.AbortRequested:
					GetCurrentRequest().set_State(HTTPRequestStates.Aborted);
					break;
				case HTTPConnectionStates.TimedOut:
					GetCurrentRequest().set_State(HTTPRequestStates.TimedOut);
					break;
				default:
					GetCurrentRequest().set_Exception(bAINMLLIKOL2);
					GetCurrentRequest().set_State(HTTPRequestStates.Error);
					break;
				}
			}
			Close();
		}
		finally
		{
			if (GetCurrentRequest() != null)
			{
				lock (HTTPManager.Locker)
				{
					if (GetCurrentRequest() != null && GetCurrentRequest().GetResponse() != null && GetCurrentRequest().GetResponse().GetIsUpgraded())
					{
						set_State(HTTPConnectionStates.Upgraded);
					}
					else
					{
						set_State(flag2 ? HTTPConnectionStates.Redirected : ((Client != null) ? HTTPConnectionStates.WaitForRecycle : HTTPConnectionStates.Closed));
					}
					if (GetCurrentRequest().GetState() == HTTPRequestStates.Processing && (GetState() == HTTPConnectionStates.Closed || GetState() == HTTPConnectionStates.WaitForRecycle))
					{
						if (GetCurrentRequest().GetResponse() != null)
						{
							GetCurrentRequest().set_State(HTTPRequestStates.Finished);
						}
						else
						{
							GetCurrentRequest().set_State(HTTPRequestStates.Error);
						}
					}
					if (GetCurrentRequest().GetState() == HTTPRequestStates.ConnectionTimedOut)
					{
						set_State(HTTPConnectionStates.Closed);
					}
					lastProcessTime = DateTime.UtcNow;
				}
				HTTPCacheService.SaveLibrary();
				CookieJar.Persist();
			}
		}
	}

	private void Connect()
	{
		Uri uri = ((!GetCurrentRequest().GetHasProxy()) ? GetCurrentRequest().GetCurrentUri() : GetCurrentRequest().GetProxy().GetAddress());
		if (Client == null)
		{
			Client = new TcpClient();
		}
		if (!Client.Connected)
		{
			Client.ConnectTimeout = GetCurrentRequest().GetConnectTimeout();
			Client.Connect(uri.Host, uri.Port);
			if (HTTPManager.GetLogger().GetLevel() <= Loglevels.Information)
			{
				HTTPManager.GetLogger().Information("HTTPConnection", "Connected to " + uri.Host + ":" + uri.Port);
			}
		}
		else if (HTTPManager.GetLogger().GetLevel() <= Loglevels.Information)
		{
			HTTPManager.GetLogger().Information("HTTPConnection", "Already connected to " + uri.Host + ":" + uri.Port);
		}
		lock (HTTPManager.Locker)
		{
			SetStartTime(DateTime.UtcNow);
		}
		if (Stream != null)
		{
			return;
		}
		bool flag = HTTPProtocolFactory.IsSecureProtocol(GetCurrentRequest().GetCurrentUri());
		if (GetHasProxy() && (!GetProxy().GetIsTransparent() || (flag && GetProxy().GetNonTransparentForHTTPS())))
		{
			Stream = Client.GetStream();
			BinaryWriter binaryWriter = new BinaryWriter(Stream);
			bool flag2;
			do
			{
				flag2 = false;
				binaryWriter.SendAsASCII(string.Format("CONNECT {0}:{1} HTTP/1.1", GetCurrentRequest().GetCurrentUri().Host, GetCurrentRequest().GetCurrentUri().Port));
				binaryWriter.Write(HTTPRequest.EOL);
				binaryWriter.SendAsASCII("Proxy-Connection: Keep-Alive");
				binaryWriter.Write(HTTPRequest.EOL);
				binaryWriter.SendAsASCII("Connection: Keep-Alive");
				binaryWriter.Write(HTTPRequest.EOL);
				binaryWriter.SendAsASCII(string.Format("Host: {0}:{1}", GetCurrentRequest().GetCurrentUri().Host, GetCurrentRequest().GetCurrentUri().Port));
				binaryWriter.Write(HTTPRequest.EOL);
				if (GetHasProxy() && GetProxy().GetCredentials() != null)
				{
					switch (GetProxy().GetCredentials().get_Type())
					{
					case AuthenticationTypes.Basic:
						binaryWriter.Write(string.Format("Proxy-Authorization: {0}", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(GetProxy().GetCredentials().GetUserName() + ":" + GetProxy().GetCredentials().GetPassword()))).GetASCIIBytes());
						binaryWriter.Write(HTTPRequest.EOL);
						break;
					case AuthenticationTypes.Unknown:
					case AuthenticationTypes.Digest:
					{
						Digest proxyDigest = DigestStore.Get(GetProxy().GetAddress());
						if (proxyDigest != null)
						{
							string text = proxyDigest.GenerateResponseHeader(GetCurrentRequest(), GetProxy().GetCredentials());
							if (!string.IsNullOrEmpty(text))
							{
								binaryWriter.Write(string.Format("Proxy-Authorization: {0}", text).GetASCIIBytes());
								binaryWriter.Write(HTTPRequest.EOL);
							}
						}
						break;
					}
					}
				}
				binaryWriter.Write(HTTPRequest.EOL);
				binaryWriter.Flush();
				GetCurrentRequest().SetProxyResponse(new HTTPResponse(GetCurrentRequest(), Stream, false, false));
				if (!GetCurrentRequest().GetProxyResponse().Receive())
				{
					throw new Exception("Connection to the Proxy Server failed!");
				}
				if (HTTPManager.GetLogger().GetLevel() <= Loglevels.Information)
				{
					HTTPManager.GetLogger().Information("HTTPConnection", "Proxy returned - status code: " + GetCurrentRequest().GetProxyResponse().GetStatusCode() + " message: " + GetCurrentRequest().GetProxyResponse().GetMessage());
				}
				int num = GetCurrentRequest().GetProxyResponse().GetStatusCode();
				if (num == 407)
				{
					string text2 = DigestStore.FindBest(GetCurrentRequest().GetProxyResponse().GetHeaderValues("proxy-authenticate"));
					if (!string.IsNullOrEmpty(text2))
					{
						Digest kHNAPCOOAEF2 = DigestStore.GetOrCreate(GetProxy().GetAddress());
						kHNAPCOOAEF2.ParseChallange(text2);
						if (GetProxy().GetCredentials() != null && kHNAPCOOAEF2.IsUriProtected(GetProxy().GetAddress()) && (!GetCurrentRequest().HasHeader("Proxy-Authorization") || kHNAPCOOAEF2.GetStale()))
						{
							flag2 = true;
						}
					}
				}
				else if (!GetCurrentRequest().GetProxyResponse().GetIsSuccess())
				{
					throw new Exception(string.Format("Proxy returned Status Code: \"{0}\", Message: \"{1}\" and Response: {2}", GetCurrentRequest().GetProxyResponse().GetStatusCode(), GetCurrentRequest().GetProxyResponse().GetMessage(), GetCurrentRequest().GetProxyResponse().GetDataAsText()));
				}
			}
			while (flag2);
		}
		if (flag)
		{
			if (GetCurrentRequest().GetUseAlternateSSL())
			{
				TlsClientProtocol tlsClientProtocol = new TlsClientProtocol(Client.GetStream(), new SecureRandom());
				List<string> list = new List<string>(1);
				list.Add(GetCurrentRequest().GetCurrentUri().Host);
				tlsClientProtocol.Connect(new LegacyTlsClient(GetCurrentRequest().GetCurrentUri(), (GetCurrentRequest().GetCustomCertificateVerifyer() != null) ? GetCurrentRequest().GetCustomCertificateVerifyer() : new AlwaysValidVerifyer(), null, list));
				Stream = tlsClientProtocol.Stream;
				return;
			}
			SslStream sslStream = new SslStream(Client.GetStream(), false, (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => GetCurrentRequest().CallCustomCertificationValidator(certificate, chain));
			if (!sslStream.IsAuthenticated)
			{
				sslStream.AuthenticateAsClient(GetCurrentRequest().GetCurrentUri().Host);
			}
			Stream = sslStream;
		}
		else
		{
			Stream = Client.GetStream();
		}
	}

	private bool Receive()
	{
		SupportedProtocols protocol = ((GetCurrentRequest().GetProtocolHandler() != SupportedProtocols.Unknown) ? GetCurrentRequest().GetProtocolHandler() : HTTPProtocolFactory.GetProtocolFromUri(GetCurrentRequest().GetCurrentUri()));
		GetCurrentRequest().SetResponse(HTTPProtocolFactory.Get(protocol, GetCurrentRequest(), Stream, GetCurrentRequest().GetUseStreaming(), false));
		if (!GetCurrentRequest().GetResponse().Receive())
		{
			GetCurrentRequest().SetResponse(null);
			return false;
		}
		if (GetCurrentRequest().GetResponse().GetStatusCode() == 304)
		{
			int length;
			using (Stream bodyStream = HTTPCacheService.GetBody(GetCurrentRequest().GetCurrentUri(), out length))
			{
				if (!GetCurrentRequest().GetResponse().HasHeader("content-length"))
				{
					GetCurrentRequest().GetResponse().GetHeaders().Add("content-length", new List<string>(1) { length.ToString() });
				}
				GetCurrentRequest().GetResponse().SetIsFromCache(true);
				GetCurrentRequest().GetResponse().ReadRaw(bodyStream, length);
			}
		}
		return true;
	}

	private bool TryLoadAllFromCache()
	{
		if (GetCurrentRequest().GetDisableCache() || !HTTPCacheService.GetIsSupported())
		{
			return false;
		}
		try
		{
			if (HTTPCacheService.IsCachedEntityExpiresInTheFuture(GetCurrentRequest()))
			{
				GetCurrentRequest().SetResponse(HTTPCacheService.GetFullResponse(GetCurrentRequest()));
				if (GetCurrentRequest().GetResponse() != null)
				{
					return true;
				}
			}
		}
		catch
		{
			HTTPCacheService.DeleteEntity(GetCurrentRequest().GetCurrentUri());
		}
		return false;
	}

	private void TryStoreInCache()
	{
		if (!GetCurrentRequest().GetUseStreaming() && !GetCurrentRequest().GetDisableCache() && GetCurrentRequest().GetResponse() != null && HTTPCacheService.GetIsSupported() && HTTPCacheService.IsCacheble(GetCurrentRequest().GetCurrentUri(), GetCurrentRequest().GetMethodType(), GetCurrentRequest().GetResponse()))
		{
			HTTPCacheService.Store(GetCurrentRequest().GetCurrentUri(), GetCurrentRequest().GetMethodType(), GetCurrentRequest().GetResponse());
		}
	}

	private Uri GetRedirectUri(string location)
	{
		Uri uri = null;
		try
		{
			return new Uri(location);
		}
		catch (UriFormatException)
		{
			Uri uri2 = GetCurrentRequest().GetUri();
			UriBuilder uriBuilder = new UriBuilder(uri2.Scheme, uri2.Host, uri2.Port, location);
			return uriBuilder.Uri;
		}
	}

	internal void HandleProgressCallback()
	{
		if (GetCurrentRequest().OnProgress != null && GetCurrentRequest().GetDownloadProgressChanged())
		{
			try
			{
				GetCurrentRequest().OnProgress(GetCurrentRequest(), GetCurrentRequest().GetDownloaded(), GetCurrentRequest().GetDownloadLength());
			}
			catch (Exception ex)
			{
				HTTPManager.GetLogger().Exception("HTTPManager", "HandleProgressCallback - OnProgress", ex);
			}
			GetCurrentRequest().SetDownloadProgressChanged(false);
		}
		if (GetCurrentRequest().OnUploadProgress != null && GetCurrentRequest().GetUploadProgressChanged())
		{
			try
			{
				GetCurrentRequest().OnUploadProgress(GetCurrentRequest(), GetCurrentRequest().GetUploaded(), GetCurrentRequest().GetUploadLength());
			}
			catch (Exception mPFFFAOGBJE2)
			{
				HTTPManager.GetLogger().Exception("HTTPManager", "HandleProgressCallback - OnUploadProgress", mPFFFAOGBJE2);
			}
			GetCurrentRequest().SetUploadProgressChanged(false);
		}
	}

	internal void HandleCallback()
	{
		try
		{
			HandleProgressCallback();
			if (GetState() == HTTPConnectionStates.Upgraded)
			{
				if (GetCurrentRequest() != null && GetCurrentRequest().GetResponse() != null && GetCurrentRequest().GetResponse().GetIsUpgraded())
				{
					GetCurrentRequest().UpgradeCallback();
				}
				set_State(HTTPConnectionStates.WaitForProtocolShutdown);
			}
			else
			{
				GetCurrentRequest().CallCallback();
			}
		}
		catch (Exception ex)
		{
			HTTPManager.GetLogger().Exception("HTTPManager", "HandleCallback", ex);
		}
	}

	internal void Abort(HTTPConnectionStates newState)
	{
		set_State(newState);
		HTTPConnectionStates currentState = GetState();
		if (currentState == HTTPConnectionStates.TimedOut)
		{
			SetTimedOutStart(DateTime.UtcNow);
		}
		if (Stream != null)
		{
			Stream.Dispose();
		}
	}

	private void Close()
	{
		set_LastProcessedUri(null);
		if (Client == null)
		{
			return;
		}
		try
		{
			Client.Close();
		}
		catch
		{
		}
		finally
		{
			Stream = null;
			Client = null;
		}
	}

	public void Dispose()
	{
		Close();
	}
}
