using System;
using System.Collections.Generic;

public sealed class HeartbeatManager
{
	private List<IHeartbeat> heartbeats = new List<IHeartbeat>();

	private IHeartbeat[] updateArray;

	private DateTime LastUpdate = DateTime.MinValue;

	public void Subscribe(IHeartbeat heartbeat)
	{
		lock (heartbeats)
		{
			if (!heartbeats.Contains(heartbeat))
			{
				heartbeats.Add(heartbeat);
			}
		}
	}

	public void Unsubscribe(IHeartbeat heartbeat)
	{
		lock (heartbeats)
		{
			heartbeats.Remove(heartbeat);
		}
	}

	public void Update()
	{
		if (LastUpdate == DateTime.MinValue)
		{
			LastUpdate = DateTime.UtcNow;
			return;
		}
		TimeSpan elapsed = DateTime.UtcNow - LastUpdate;
		LastUpdate = DateTime.UtcNow;
		int num = 0;
		lock (heartbeats)
		{
			if (updateArray == null || updateArray.Length < heartbeats.Count)
			{
				Array.Resize(ref updateArray, heartbeats.Count);
			}
			heartbeats.CopyTo(0, updateArray, 0, heartbeats.Count);
			num = heartbeats.Count;
		}
		for (int i = 0; i < num; i++)
		{
			try
			{
				updateArray[i].OnHeartbeatUpdate(elapsed);
			}
			catch
			{
			}
		}
	}
}
