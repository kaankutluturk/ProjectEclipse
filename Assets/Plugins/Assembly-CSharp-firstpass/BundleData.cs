using System;
using System.Collections.Generic;

[Serializable]
public class BundleData
{
	public bool Available;

	public string Hash;

	public Dictionary<string, string> Labels;

	public string[] Dependencies;

	public BundleData(string HDPBNCNCMOH, Dictionary<string, string> ILLOMIIOHEH, string[] PPGKJCOMHAA)
	{
		Hash = HDPBNCNCMOH;
		Labels = ILLOMIIOHEH;
		Dependencies = PPGKJCOMHAA;
		Available = false;
	}

	public bool IsSupportedForValue(int PPBIPCKMFKB)
	{
		return true;
	}

	public bool IsAllowedForValue(int EILBNEKNAMO)
	{
		return true;
	}
}
