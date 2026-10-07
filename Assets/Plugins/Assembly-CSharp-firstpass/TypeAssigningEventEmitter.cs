using System;
using System.Globalization;

public sealed class TypeAssigningEventEmitter : ChainedEventEmitter
{
	private readonly bool requireTagWhenStaticAndActualTypesAreDifferent;

	public TypeAssigningEventEmitter(IEventEmitter innerEmitter, bool requireTag)
		: base(innerEmitter)
	{
		requireTagWhenStaticAndActualTypesAreDifferent = requireTag;
	}

	public override void Emit(ScalarEventInfo eventInfo)
	{
		eventInfo.SetIsPlainImplicit(true);
		eventInfo.SetStyle(ScalarStyle.Plain);
		TypeCode typeCode = ((eventInfo.GetSource().GetValue() != null) ? eventInfo.GetSource().get_Type().GetTypeCode() : TypeCode.Empty);
		switch (typeCode)
		{
		case TypeCode.Boolean:
			eventInfo.set_Tag("tag:yaml.org,2002:bool");
			eventInfo.set_RenderedValue(YamlFormatter.FormatBool(eventInfo.GetSource().GetValue()));
			break;
		case TypeCode.SByte:
		case TypeCode.Byte:
		case TypeCode.Int16:
		case TypeCode.UInt16:
		case TypeCode.Int32:
		case TypeCode.UInt32:
		case TypeCode.Int64:
		case TypeCode.UInt64:
			eventInfo.set_Tag("tag:yaml.org,2002:int");
			eventInfo.set_RenderedValue(YamlFormatter.FormatNumber(eventInfo.GetSource().GetValue()));
			break;
		case TypeCode.Single:
		case TypeCode.Double:
		case TypeCode.Decimal:
			eventInfo.set_Tag("tag:yaml.org,2002:float");
			eventInfo.set_RenderedValue(YamlFormatter.FormatNumber(eventInfo.GetSource().GetValue()));
			break;
		case TypeCode.Char:
		case TypeCode.String:
			eventInfo.set_Tag("tag:yaml.org,2002:str");
			eventInfo.set_RenderedValue(eventInfo.GetSource().GetValue().ToString());
			eventInfo.SetStyle(ScalarStyle.Any);
			break;
		case TypeCode.DateTime:
			eventInfo.set_Tag("tag:yaml.org,2002:timestamp");
			eventInfo.set_RenderedValue(YamlFormatter.FormatDateTime(eventInfo.GetSource().GetValue()));
			break;
		case TypeCode.Empty:
			eventInfo.set_Tag("tag:yaml.org,2002:null");
			eventInfo.set_RenderedValue(string.Empty);
			break;
		default:
			if (eventInfo.GetSource().get_Type() == typeof(TimeSpan))
			{
				eventInfo.set_RenderedValue(YamlFormatter.FormatTimeSpan(eventInfo.GetSource().GetValue()));
				break;
			}
			throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "TypeCode.{0} is not supported.", typeCode));
		}
		base.Emit(eventInfo);
	}

	public override void Emit(MappingStartEventInfo eventInfo)
	{
		AssignTypeIfDifferent(eventInfo);
		base.Emit(eventInfo);
	}

	public override void Emit(SequenceStartEventInfo eventInfo)
	{
		AssignTypeIfDifferent(eventInfo);
		base.Emit(eventInfo);
	}

	private void AssignTypeIfDifferent(ObjectEventInfo eventInfo)
	{
		if (requireTagWhenStaticAndActualTypesAreDifferent && eventInfo.GetSource().GetValue() != null && eventInfo.GetSource().get_Type() != eventInfo.GetSource().GetStaticType())
		{
			eventInfo.set_Tag("!" + eventInfo.GetSource().get_Type().AssemblyQualifiedName);
		}
	}
}
