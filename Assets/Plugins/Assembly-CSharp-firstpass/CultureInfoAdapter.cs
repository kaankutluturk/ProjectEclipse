using System;
using System.Globalization;

internal sealed class CultureInfoAdapter : CultureInfo
{
	private readonly IFormatProvider provider;

	public CultureInfoAdapter(CultureInfo cultureInfo, IFormatProvider formatProvider)
		: base(cultureInfo.LCID)
	{
		provider = formatProvider;
	}

	public override object GetFormat(Type formatType)
	{
		return provider.GetFormat(formatType);
	}
}
