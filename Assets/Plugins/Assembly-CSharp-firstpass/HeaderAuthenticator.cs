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

	public HeaderAuthenticator(string user, string roles)
	{
		SetUser(user);
		SetRoles(roles);
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
		OnAuthenticationSuccededDelegate current = OnAuthenticationSucceded;
		OnAuthenticationSuccededDelegate lACLODBGJEI2;
		do
		{
			lACLODBGJEI2 = current;
			current = Interlocked.CompareExchange(ref OnAuthenticationSucceded, (OnAuthenticationSuccededDelegate)Delegate.Combine(lACLODBGJEI2, value), current);
		}
		while ((object)current != lACLODBGJEI2);
	}

	public void RemoveAuthenticationSucceeded(OnAuthenticationSuccededDelegate value)
	{
		OnAuthenticationSuccededDelegate current = OnAuthenticationSucceded;
		OnAuthenticationSuccededDelegate lACLODBGJEI2;
		do
		{
			lACLODBGJEI2 = current;
			current = Interlocked.CompareExchange(ref OnAuthenticationSucceded, (OnAuthenticationSuccededDelegate)Delegate.Remove(lACLODBGJEI2, value), current);
		}
		while ((object)current != lACLODBGJEI2);
	}

	public void AddAuthenticationFailed(OnAuthenticationFailedDelegate value)
	{
		OnAuthenticationFailedDelegate current = OnAuthenticationFailed;
		OnAuthenticationFailedDelegate bCHANFGJONF2;
		do
		{
			bCHANFGJONF2 = current;
			current = Interlocked.CompareExchange(ref OnAuthenticationFailed, (OnAuthenticationFailedDelegate)Delegate.Combine(bCHANFGJONF2, value), current);
		}
		while ((object)current != bCHANFGJONF2);
	}

	public void RemoveAuthenticationFailed(OnAuthenticationFailedDelegate value)
	{
		OnAuthenticationFailedDelegate current = OnAuthenticationFailed;
		OnAuthenticationFailedDelegate bCHANFGJONF2;
		do
		{
			bCHANFGJONF2 = current;
			current = Interlocked.CompareExchange(ref OnAuthenticationFailed, (OnAuthenticationFailedDelegate)Delegate.Remove(bCHANFGJONF2, value), current);
		}
		while ((object)current != bCHANFGJONF2);
	}

	public void StartAuthentication()
	{
	}

	public void PrepareRequest(HTTPRequest request, SignalRRequestType requestType)
	{
		request.SetHeader("username", GetUser());
		request.SetHeader("roles", GetRoles());
	}
}
