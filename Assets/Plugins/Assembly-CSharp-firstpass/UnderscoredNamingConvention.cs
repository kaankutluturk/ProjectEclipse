public sealed class UnderscoredNamingConvention : INamingConvention
{
	public string Apply(string value)
	{
		return value.FromCamelCase("_");
	}
}
