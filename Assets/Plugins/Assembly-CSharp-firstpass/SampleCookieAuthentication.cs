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

	public SampleCookieAuthentication(Uri EJLKINNHGHN, string KEJDJHAGBMK, string JMKKKMKEAMI, string NNMKKAKIJCP)
	{
		set_AuthUri(EJLKINNHGHN);
		SetUserName(KEJDJHAGBMK);
		SetPassword(JMKKKMKEAMI);
		SetUserRoles(NNMKKAKIJCP);
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
		OnAuthenticationSuccededDelegate lACLODBGJEI = OnAuthenticationSucceded;
		OnAuthenticationSuccededDelegate lACLODBGJEI2;
		do
		{
			lACLODBGJEI2 = lACLODBGJEI;
			lACLODBGJEI = Interlocked.CompareExchange(ref OnAuthenticationSucceded, (OnAuthenticationSuccededDelegate)Delegate.Combine(lACLODBGJEI2, value), lACLODBGJEI);
		}
		while ((object)lACLODBGJEI != lACLODBGJEI2);
	}

	public void RemoveAuthenticationSucceeded(OnAuthenticationSuccededDelegate value)
	{
		OnAuthenticationSuccededDelegate lACLODBGJEI = OnAuthenticationSucceded;
		OnAuthenticationSuccededDelegate lACLODBGJEI2;
		do
		{
			lACLODBGJEI2 = lACLODBGJEI;
			lACLODBGJEI = Interlocked.CompareExchange(ref OnAuthenticationSucceded, (OnAuthenticationSuccededDelegate)Delegate.Remove(lACLODBGJEI2, value), lACLODBGJEI);
		}
		while ((object)lACLODBGJEI != lACLODBGJEI2);
	}

	public void AddAuthenticationFailed(OnAuthenticationFailedDelegate value)
	{
		OnAuthenticationFailedDelegate bCHANFGJONF = OnAuthenticationFailed;
		OnAuthenticationFailedDelegate bCHANFGJONF2;
		do
		{
			bCHANFGJONF2 = bCHANFGJONF;
			bCHANFGJONF = Interlocked.CompareExchange(ref OnAuthenticationFailed, (OnAuthenticationFailedDelegate)Delegate.Combine(bCHANFGJONF2, value), bCHANFGJONF);
		}
		while ((object)bCHANFGJONF != bCHANFGJONF2);
	}

	public void RemoveAuthenticationFailed(OnAuthenticationFailedDelegate value)
	{
		OnAuthenticationFailedDelegate bCHANFGJONF = OnAuthenticationFailed;
		OnAuthenticationFailedDelegate bCHANFGJONF2;
		do
		{
			bCHANFGJONF2 = bCHANFGJONF;
			bCHANFGJONF = Interlocked.CompareExchange(ref OnAuthenticationFailed, (OnAuthenticationFailedDelegate)Delegate.Remove(bCHANFGJONF2, value), bCHANFGJONF);
		}
		while ((object)bCHANFGJONF != bCHANFGJONF2);
	}

	public void StartAuthentication()
	{
		authRequest = new HTTPRequest(GetAuthUri(), HTTPMethods.Post, OnAuthRequestFinished);
		authRequest.AddField("userName", GetUserName());
		authRequest.AddField("Password", GetPassword());
		authRequest.AddField("roles", GetUserRoles());
		authRequest.Send();
	}

	public void PrepareRequest(HTTPRequest ONOCIELLAPL, SignalRRequestType LFLGCDNKNJI)
	{
		ONOCIELLAPL.GetCookies().Add(cookie);
	}

	private void OnAuthRequestFinished(HTTPRequest CGOIOKHEGOE, HTTPResponse BEIGFGCBICO)
	{
		authRequest = null;
		string nEPOLDCKNJL = string.Empty;
		switch (CGOIOKHEGOE.GetState())
		{
		case HTTPRequestStates.Finished:
			if (BEIGFGCBICO.GetIsSuccess())
			{
				cookie = ((BEIGFGCBICO.GetCookies() == null) ? null : BEIGFGCBICO.GetCookies().Find((Cookie ILHDJDNPFKH) => ILHDJDNPFKH.get_Name().Equals(".ASPXAUTH")));
				if (cookie != null)
				{
					HTTPManager.GetLogger().Information("CookieAuthentication", "Auth. Cookie found!");
					if (OnAuthenticationSucceded != null)
					{
						OnAuthenticationSucceded(this);
					}
					return;
				}
				HTTPManager.GetLogger().Warning("CookieAuthentication", nEPOLDCKNJL = "Auth. Cookie NOT found!");
			}
			else
			{
				HTTPManager.GetLogger().Warning("CookieAuthentication", nEPOLDCKNJL = string.Format("Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}", BEIGFGCBICO.GetStatusCode(), BEIGFGCBICO.GetMessage(), BEIGFGCBICO.GetDataAsText()));
			}
			break;
		case HTTPRequestStates.Error:
			HTTPManager.GetLogger().Warning("CookieAuthentication", nEPOLDCKNJL = "Request Finished with Error! " + ((CGOIOKHEGOE.GetException() == null) ? "No Exception" : (CGOIOKHEGOE.GetException().Message + "\n" + CGOIOKHEGOE.GetException().StackTrace)));
			break;
		case HTTPRequestStates.Aborted:
			HTTPManager.GetLogger().Warning("CookieAuthentication", nEPOLDCKNJL = "Request Aborted!");
			break;
		case HTTPRequestStates.ConnectionTimedOut:
			HTTPManager.GetLogger().Error("CookieAuthentication", nEPOLDCKNJL = "Connection Timed Out!");
			break;
		case HTTPRequestStates.TimedOut:
			HTTPManager.GetLogger().Error("CookieAuthentication", nEPOLDCKNJL = "Processing the request Timed Out!");
			break;
		}
		if (OnAuthenticationFailed != null)
		{
			OnAuthenticationFailed(this, nEPOLDCKNJL);
		}
	}
}
