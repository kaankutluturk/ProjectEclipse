using System.Xml;

public struct CharProgLevel
{
	public uint Min;

	public uint Max;

	public long value;

	public CharProgLevel(XmlNode node)
	{
		Min = node.Attributes["Min"].ParseUint();
		Max = node.Attributes["Max"].ParseUint(uint.MaxValue);
		value = node.Attributes["Value"].ParseLong(0L);
	}
}
