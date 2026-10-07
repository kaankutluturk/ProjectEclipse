using System;
using System.Collections.Generic;

[Serializable]
public class BundleData
{
	public bool Available;

	public string Hash;

	public Dictionary<string, string> Labels;

	public string[] Dependencies;

	public BundleData(string hash, Dictionary<string, string> labels, string[] dependencies)
	{
		Hash = hash;
		Labels = labels;
		Dependencies = dependencies;
		Available = false;
	}

	public bool IsSupportedForValue(int intValue)
	{
		return true;
	}

	public bool IsAllowedForValue(int intValue)
	{
		return true;
	}
}
