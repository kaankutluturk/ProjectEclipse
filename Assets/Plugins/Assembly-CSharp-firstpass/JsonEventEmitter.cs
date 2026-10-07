using System;
using System.Globalization;

public sealed class JsonEventEmitter : ChainedEventEmitter
{
	public JsonEventEmitter(IEventEmitter nextEmitter)
		: base(nextEmitter)
	{
	}

	public override void Emit(AliasEventInfo aliasInfo)
	{
		throw new NotSupportedException("Aliases are not supported in JSON");
	}

	public override void Emit(ScalarEventInfo scalarInfo)
	{
		scalarInfo.SetIsPlainImplicit(true);
		scalarInfo.SetStyle(ScalarStyle.Plain);
		TypeCode typeCode = ((scalarInfo.GetSource().GetValue() != null) ? scalarInfo.GetSource().get_Type().GetTypeCode() : TypeCode.Empty);
		switch (typeCode)
		{
		case TypeCode.Boolean:
			scalarInfo.set_RenderedValue(YamlFormatter.FormatBool(scalarInfo.GetSource().GetValue()));
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
			scalarInfo.set_RenderedValue(YamlFormatter.FormatNumber(scalarInfo.GetSource().GetValue()));
			break;
		case TypeCode.Char:
		case TypeCode.String:
			scalarInfo.set_RenderedValue(scalarInfo.GetSource().GetValue().ToString());
			scalarInfo.SetStyle(ScalarStyle.DoubleQuoted);
			break;
		case TypeCode.DateTime:
			scalarInfo.set_RenderedValue(YamlFormatter.FormatDateTime(scalarInfo.GetSource().GetValue()));
			break;
		case TypeCode.Empty:
			scalarInfo.set_RenderedValue("null");
			break;
		default:
			if (scalarInfo.GetSource().get_Type() == typeof(TimeSpan))
			{
				scalarInfo.set_RenderedValue(YamlFormatter.FormatTimeSpan(scalarInfo.GetSource().GetValue()));
				break;
			}
			throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "TypeCode.{0} is not supported.", typeCode));
		}
		base.Emit(scalarInfo);
	}

	public override void Emit(MappingStartEventInfo mappingStartInfo)
	{
		mappingStartInfo.SetStyle(MappingStyle.Flow);
		base.Emit(mappingStartInfo);
	}

	public override void Emit(SequenceStartEventInfo sequenceStartInfo)
	{
		sequenceStartInfo.SetStyle(SequenceStyle.Flow);
		base.Emit(sequenceStartInfo);
	}
}
