using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

public sealed class YamlNodeIdentityEqualityComparer : IEqualityComparer<YamlNode>
{
	public bool Equals(YamlNode y, YamlNode x)
	{
		return object.ReferenceEquals(y, x);
	}

	public int GetHashCode(YamlNode obj)
	{
		return obj.GetHashCode();
	}
}
