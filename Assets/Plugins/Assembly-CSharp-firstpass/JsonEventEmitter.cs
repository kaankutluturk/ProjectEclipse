using System;
using System.Globalization;

public sealed class JsonEventEmitter : ChainedEventEmitter
{
	public JsonEventEmitter(IEventEmitter JDJEJDIJLLE)
		: base(JDJEJDIJLLE)
	{
	}

	public override void Emit(AliasEventInfo FNHCFCAALAE)
	{
		throw new NotSupportedException("Aliases are not supported in JSON");
	}

	public override void Emit(ScalarEventInfo FNHCFCAALAE)
	{
		FNHCFCAALAE.SetIsPlainImplicit(true);
		FNHCFCAALAE.SetStyle(ScalarStyle.Plain);
		TypeCode typeCode = ((FNHCFCAALAE.GetSource().GetValue() != null) ? FNHCFCAALAE.GetSource().get_Type().GetTypeCode() : TypeCode.Empty);
		switch (typeCode)
		{
		case TypeCode.Boolean:
			FNHCFCAALAE.set_RenderedValue(YamlFormatter.FormatBool(FNHCFCAALAE.GetSource().GetValue()));
			break;
		case TypeCode.SByte:
		case TypeCode.Byte:
		case TypeCode.Int16:
		case TypeCode.UInt16:
		case TypeCode.Int32:
		case TypeCode.UInt32:
		case TypeCode.Int64:
		case TypeCode.UInt64:
		case TypeCode.Single:
		case TypeCode.Double:
		case TypeCode.Decimal:
			FNHCFCAALAE.set_RenderedValue(YamlFormatter.FormatNumber(FNHCFCAALAE.GetSource().GetValue()));
			break;
		case TypeCode.Char:
		case TypeCode.String:
			FNHCFCAALAE.set_RenderedValue(FNHCFCAALAE.GetSource().GetValue().ToString());
			FNHCFCAALAE.SetStyle(ScalarStyle.DoubleQuoted);
			break;
		case TypeCode.DateTime:
			FNHCFCAALAE.set_RenderedValue(YamlFormatter.FormatDateTime(FNHCFCAALAE.GetSource().GetValue()));
			break;
		case TypeCode.Empty:
			FNHCFCAALAE.set_RenderedValue("null");
			break;
		default:
			if (FNHCFCAALAE.GetSource().get_Type() == typeof(TimeSpan))
			{
				FNHCFCAALAE.set_RenderedValue(YamlFormatter.FormatTimeSpan(FNHCFCAALAE.GetSource().GetValue()));
				break;
			}
			throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "TypeCode.{0} is not supported.", typeCode));
		}
		base.Emit(FNHCFCAALAE);
	}

	public override void Emit(MappingStartEventInfo FNHCFCAALAE)
	{
		FNHCFCAALAE.SetStyle(MappingStyle.Flow);
		base.Emit(FNHCFCAALAE);
	}

	public override void Emit(SequenceStartEventInfo FNHCFCAALAE)
	{
		FNHCFCAALAE.SetStyle(SequenceStyle.Flow);
		base.Emit(FNHCFCAALAE);
	}
}
