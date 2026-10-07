using System;
using System.Globalization;

internal sealed class CultureInfoAdapter : CultureInfo
{
	private readonly IFormatProvider provider;

	public CultureInfoAdapter(CultureInfo OOLKOFLIGGN, IFormatProvider EEGMFLOPLLH)
		: base(OOLKOFLIGGN.LCID)
	{
		provider = EEGMFLOPLLH;
	}

	public override object GetFormat(Type HHNCMCGMOGP)
	{
		return provider.GetFormat(HHNCMCGMOGP);
	}
}
