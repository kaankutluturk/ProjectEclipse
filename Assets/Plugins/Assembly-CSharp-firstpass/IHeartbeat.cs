using System;

public interface IHeartbeat
{
	void OnHeartbeatUpdate(TimeSpan elapsed);
}
