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
		OnAuthenticationSuccededDelegate previousHandler;
		do
		{
			previousHandler = current;
			current = Interlocked.CompareExchange(ref OnAuthenticationSucceded, (OnAuthenticationSuccededDelegate)Delegate.Combine(previousHandler, value), current);
		}
		while ((object)current != previousHandler);
	}

	public void RemoveAuthenticationSucceeded(OnAuthenticationSuccededDelegate value)
	{
		OnAuthenticationSuccededDelegate current = OnAuthenticationSucceded;
		OnAuthenticationSuccededDelegate previousHandler;
		do
		{
			previousHandler = current;
			current = Interlocked.CompareExchange(ref OnAuthenticationSucceded, (OnAuthenticationSuccededDelegate)Delegate.Remove(previousHandler, value), current);
		}
		while ((object)current != previousHandler);
	}

	public void AddAuthenticationFailed(OnAuthenticationFailedDelegate value)
	{
		OnAuthenticationFailedDelegate current = OnAuthenticationFailed;
		OnAuthenticationFailedDelegate previousHandler;
		do
		{
			previousHandler = current;
			current = Interlocked.CompareExchange(ref OnAuthenticationFailed, (OnAuthenticationFailedDelegate)Delegate.Combine(previousHandler, value), current);
		}
		while ((object)current != previousHandler);
	}

	public void RemoveAuthenticationFailed(OnAuthenticationFailedDelegate value)
	{
		OnAuthenticationFailedDelegate current = OnAuthenticationFailed;
		OnAuthenticationFailedDelegate previousHandler;
		do
		{
			previousHandler = current;
			current = Interlocked.CompareExchange(ref OnAuthenticationFailed, (OnAuthenticationFailedDelegate)Delegate.Remove(previousHandler, value), current);
		}
		while ((object)current != previousHandler);
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
