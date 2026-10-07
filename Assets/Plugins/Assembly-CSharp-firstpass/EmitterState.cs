using System.Collections.Generic;

internal class EmitterState
{
	private readonly HashSet<string> emittedAnchors = new HashSet<string>();

	public HashSet<string> EmittedAnchors
	{
		get
		{
			return GetEmittedAnchors();
		}
	}

	public HashSet<string> GetEmittedAnchors()
	{
		return emittedAnchors;
	}
}
