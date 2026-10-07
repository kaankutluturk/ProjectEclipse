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
		InitializeDoneHandler kNHFNPECPED = InitializeDone;
		InitializeDoneHandler kNHFNPECPED2;
		do
		{
			kNHFNPECPED2 = kNHFNPECPED;
			kNHFNPECPED = Interlocked.CompareExchange(ref InitializeDone, (InitializeDoneHandler)Delegate.Combine(kNHFNPECPED2, value), kNHFNPECPED);
		}
		while ((object)kNHFNPECPED != kNHFNPECPED2);
	}

	public static void RemoveInitializeDone(InitializeDoneHandler value)
	{
		InitializeDoneHandler kNHFNPECPED = InitializeDone;
		InitializeDoneHandler kNHFNPECPED2;
		do
		{
			kNHFNPECPED2 = kNHFNPECPED;
			kNHFNPECPED = Interlocked.CompareExchange(ref InitializeDone, (InitializeDoneHandler)Delegate.Remove(kNHFNPECPED2, value), kNHFNPECPED);
		}
		while ((object)kNHFNPECPED != kNHFNPECPED2);
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

	public virtual void AddInitCallbacks(params Action[] AFENHJFICNN)
	{
		foreach (Action action in AFENHJFICNN)
		{
			Action IBODMPMJELJ = action;
			AddInitializeDone(() =>
			{
				IBODMPMJELJ();
			});
		}
	}

	public virtual void FinishInit()
	{
		SF2DisplayFrameRate.Apply();
		NotifyInitializeDone();
	}

	public abstract void Init(params Action[] AFENHJFICNN);
}
