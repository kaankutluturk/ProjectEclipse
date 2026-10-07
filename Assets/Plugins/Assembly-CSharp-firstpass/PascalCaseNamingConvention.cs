public sealed class PascalCaseNamingConvention : INamingConvention
{
	public string Apply(string value)
	{
		return value.ToPascalCase();
	}
}
