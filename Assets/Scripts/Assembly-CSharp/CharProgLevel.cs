using System.Xml;

public struct CharProgLevel
{
	public uint Min;

	public uint Max;

	public long value;

	public CharProgLevel(XmlNode OPGGCJGNIPB)
	{
		Min = OPGGCJGNIPB.Attributes["Min"].ParseUint();
		Max = OPGGCJGNIPB.Attributes["Max"].ParseUint(uint.MaxValue);
		value = OPGGCJGNIPB.Attributes["Value"].ParseLong(0L);
	}
}
