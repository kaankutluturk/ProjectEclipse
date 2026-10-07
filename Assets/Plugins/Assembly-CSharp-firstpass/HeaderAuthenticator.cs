using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

internal class HeaderAuthenticator : IAuthenticationProvider
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string user;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string roles;

	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private OnAuthenticationSuccededDelegate OnAuthenticationSucceded;

	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private OnAuthenticationFailedDelegate OnAuthenticationFailed;

	public string User
	{
		get
		{
			return GetUser();
		}
		private set
		{
			SetUser(value);
		}
	}

	public string Roles
	{
		get
		{
			return GetRoles();
		}
		private set
		{
			SetRoles(value);
		}
	}

	public bool RequiresPreAuth
	{
		get
		{
			return GetIsPreAuthRequired();
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

	public HeaderAuthenticator(string KEJDJHAGBMK, string NNMKKAKIJCP)
	{
		SetUser(KEJDJHAGBMK);
		SetRoles(NNMKKAKIJCP);
	}

	public string GetUser()
	{
		return user;
	}

	private void SetUser(string value)
	{
		user = value;
	}

	public string GetRoles()
	{
		return roles;
	}

	private void SetRoles(string value)
	{
		roles = value;
	}

	public bool GetIsPreAuthRequired()
	{
		return false;
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
	}

	public void PrepareRequest(HTTPRequest ONOCIELLAPL, SignalRRequestType LFLGCDNKNJI)
	{
		ONOCIELLAPL.SetHeader("username", GetUser());
		ONOCIELLAPL.SetHeader("roles", GetRoles());
	}
}
