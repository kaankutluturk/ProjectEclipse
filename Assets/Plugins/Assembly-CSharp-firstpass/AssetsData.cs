using System;

[Serializable]
public class AssetsData
{
	public string Path;

	public string BundleName;

	public AssetsData(string path, string bundleName)
	{
		Path = path;
		BundleName = bundleName;
	}
}
