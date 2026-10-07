public sealed class HyphenatedNamingConvention : INamingConvention
{
	public string Apply(string value)
	{
		return value.FromCamelCase("-");
	}
}
