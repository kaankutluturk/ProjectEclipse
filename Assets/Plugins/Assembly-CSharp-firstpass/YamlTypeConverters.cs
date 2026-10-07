using System.Collections.Generic;

internal static class YamlTypeConverters
{
	private static readonly IEnumerable<IYamlTypeConverter> builtInConverters = new IYamlTypeConverter[1]
	{
		new GuidConverter()
	};

	public static IEnumerable<IYamlTypeConverter> Converters
	{
		get
		{
			return GetConverters();
		}
	}

	public static IEnumerable<IYamlTypeConverter> GetConverters()
	{
		return builtInConverters;
	}
}
