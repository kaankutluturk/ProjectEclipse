using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public abstract class GameInit
{
	public delegate void InitializeDoneHandler();

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[CompilerGenerated]
	private static InitializeDoneHandler InitializeDone;

	public static event InitializeDoneHandler OnInitializeDone
	{
		add
		{
			AddInitializeDone(value);
		}
		remove
		{
			RemoveInitializeDone(value);
		}
	}

	public static void AddInitializeDone(InitializeDoneHandler value)
	{
		InitializeDoneHandler current = InitializeDone;
		InitializeDoneHandler previousHandler;
		do
		{
			previousHandler = current;
			current = Interlocked.CompareExchange(ref InitializeDone, (InitializeDoneHandler)Delegate.Combine(previousHandler, value), current);
		}
		while ((object)current != previousHandler);
	}

	public static void RemoveInitializeDone(InitializeDoneHandler value)
	{
		InitializeDoneHandler current = InitializeDone;
		InitializeDoneHandler previousHandler;
		do
		{
			previousHandler = current;
			current = Interlocked.CompareExchange(ref InitializeDone, (InitializeDoneHandler)Delegate.Remove(previousHandler, value), current);
		}
		while ((object)current != previousHandler);
	}

	private static void RaiseInitializeDone()
	{
		InitializeDoneHandler initializeDone = InitializeDone;
		if (initializeDone != null)
		{
			initializeDone();
		}
	}

	protected void NotifyInitializeDone()
	{
		RaiseInitializeDone();
	}

	public virtual void AddInitCallbacks(params Action[] callbacks)
	{
		foreach (Action action in callbacks)
		{
			Action callback = action;
			AddInitializeDone(() =>
			{
				callback();
			});
		}
	}

	public virtual void FinishInit()
	{
		SF2DisplayFrameRate.Apply();
		NotifyInitializeDone();
	}

	public abstract void Init(params Action[] callbacks);
}
