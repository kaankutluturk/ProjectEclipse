using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

public sealed class SampleCookieAuthentication : IAuthenticationProvider
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Uri authUri;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string userName;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string password;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string userRoles;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isPreAuthRequired;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[CompilerGenerated]
	private OnAuthenticationSuccededDelegate OnAuthenticationSucceded;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[CompilerGenerated]
	private OnAuthenticationFailedDelegate OnAuthenticationFailed;

	private HTTPRequest authRequest;

	private Cookie cookie;

	public Uri AuthenticationUri
	{
		get
		{
			return GetAuthUri();
		}
		private set
		{
			set_AuthUri(value);
		}
	}

	public string UserName
	{
		get
		{
			return GetUserName();
		}
		private set
		{
			SetUserName(value);
		}
	}

	public string Password
	{
		get
		{
			return GetPassword();
		}
		private set
		{
			SetPassword(value);
		}
	}

	public string UserRoles
	{
		get
		{
			return GetUserRoles();
		}
		private set
		{
			SetUserRoles(value);
		}
	}

	public bool RequiresPreAuth
	{
		get
		{
			return GetIsPreAuthRequired();
		}
		private set
		{
			set_IsPreAuthRequired(value);
		}
	}

	public event OnAuthenticationSuccededDelegate AuthenticationSucceeded
	{
		add
		{
			AddAuthenticationSucceeded(value);
		}
		remove
		{
			RemoveAuthenticationSucceeded(value);
		}
	}

	public event OnAuthenticationFailedDelegate AuthenticationFailed
	{
		add
		{
			AddAuthenticationFailed(value);
		}
		remove
		{
			RemoveAuthenticationFailed(value);
		}
	}

	public SampleCookieAuthentication(Uri authUri, string userName, string password, string userRoles)
	{
		set_AuthUri(authUri);
		SetUserName(userName);
		SetPassword(password);
		SetUserRoles(userRoles);
		set_IsPreAuthRequired(true);
	}

	public Uri GetAuthUri()
	{
		return authUri;
	}

	private void set_AuthUri(Uri value)
	{
		authUri = value;
	}

	public string GetUserName()
	{
		return userName;
	}

	private void SetUserName(string value)
	{
		userName = value;
	}

	public string GetPassword()
	{
		return password;
	}

	private void SetPassword(string value)
	{
		password = value;
	}

	public string GetUserRoles()
	{
		return userRoles;
	}

	private void SetUserRoles(string value)
	{
		userRoles = value;
	}

	public bool GetIsPreAuthRequired()
	{
		return isPreAuthRequired;
	}

	private void set_IsPreAuthRequired(bool value)
	{
		isPreAuthRequired = value;
	}

	public void AddAuthenticationSucceeded(OnAuthenticationSuccededDelegate value)
	{
		OnAuthenticationSuccededDelegate currentHandler = OnAuthenticationSucceded;
		OnAuthenticationSuccededDelegate previousHandler;
		do
		{
			previousHandler = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref OnAuthenticationSucceded, (OnAuthenticationSuccededDelegate)Delegate.Combine(previousHandler, value), currentHandler);
		}
		while ((object)currentHandler != previousHandler);
	}

	public void RemoveAuthenticationSucceeded(OnAuthenticationSuccededDelegate value)
	{
		OnAuthenticationSuccededDelegate currentHandler = OnAuthenticationSucceded;
		OnAuthenticationSuccededDelegate previousHandler;
		do
		{
			previousHandler = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref OnAuthenticationSucceded, (OnAuthenticationSuccededDelegate)Delegate.Remove(previousHandler, value), currentHandler);
		}
		while ((object)currentHandler != previousHandler);
	}

	public void AddAuthenticationFailed(OnAuthenticationFailedDelegate value)
	{
		OnAuthenticationFailedDelegate currentHandler = OnAuthenticationFailed;
		OnAuthenticationFailedDelegate previousHandler;
		do
		{
			previousHandler = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref OnAuthenticationFailed, (OnAuthenticationFailedDelegate)Delegate.Combine(previousHandler, value), currentHandler);
		}
		while ((object)currentHandler != previousHandler);
	}

	public void RemoveAuthenticationFailed(OnAuthenticationFailedDelegate value)
	{
		OnAuthenticationFailedDelegate currentHandler = OnAuthenticationFailed;
		OnAuthenticationFailedDelegate previousHandler;
		do
		{
			previousHandler = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref OnAuthenticationFailed, (OnAuthenticationFailedDelegate)Delegate.Remove(previousHandler, value), currentHandler);
		}
		while ((object)currentHandler != previousHandler);
	}

	public void StartAuthentication()
	{
		authRequest = new HTTPRequest(GetAuthUri(), HTTPMethods.Post, OnAuthRequestFinished);
		authRequest.AddField("userName", GetUserName());
		authRequest.AddField("Password", GetPassword());
		authRequest.AddField("roles", GetUserRoles());
		authRequest.Send();
	}

	public void PrepareRequest(HTTPRequest request, SignalRRequestType requestType)
	{
		request.GetCookies().Add(cookie);
	}

	private void OnAuthRequestFinished(HTTPRequest request, HTTPResponse response)
	{
		authRequest = null;
		string reason = string.Empty;
		switch (request.GetState())
		{
		case HTTPRequestStates.Finished:
			if (response.GetIsSuccess())
			{
				cookie = ((response.GetCookies() == null) ? null : response.GetCookies().Find((Cookie existingCookie) => existingCookie.get_Name().Equals(".ASPXAUTH")));
				if (cookie != null)
				{
					HTTPManager.GetLogger().Information("CookieAuthentication", "Auth. Cookie found!");
					if (OnAuthenticationSucceded != null)
					{
						OnAuthenticationSucceded(this);
					}
					return;
				}
				HTTPManager.GetLogger().Warning("CookieAuthentication", reason = "Auth. Cookie NOT found!");
			}
			else
			{
				HTTPManager.GetLogger().Warning("CookieAuthentication", reason = string.Format("Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}", response.GetStatusCode(), response.GetMessage(), response.GetDataAsText()));
			}
			break;
		case HTTPRequestStates.Error:
			HTTPManager.GetLogger().Warning("CookieAuthentication", reason = "Request Finished with Error! " + ((request.GetException() == null) ? "No Exception" : (request.GetException().Message + "\n" + request.GetException().StackTrace)));
			break;
		case HTTPRequestStates.Aborted:
			HTTPManager.GetLogger().Warning("CookieAuthentication", reason = "Request Aborted!");
			break;
		case HTTPRequestStates.ConnectionTimedOut:
			HTTPManager.GetLogger().Error("CookieAuthentication", reason = "Connection Timed Out!");
			break;
		case HTTPRequestStates.TimedOut:
			HTTPManager.GetLogger().Error("CookieAuthentication", reason = "Processing the request Timed Out!");
			break;
		}
		if (OnAuthenticationFailed != null)
		{
			OnAuthenticationFailed(this, reason);
		}
	}
}
