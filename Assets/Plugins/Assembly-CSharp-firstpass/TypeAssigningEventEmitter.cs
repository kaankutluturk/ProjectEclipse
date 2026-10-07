using System;
using System.Globalization;

public sealed class TypeAssigningEventEmitter : ChainedEventEmitter
{
	private readonly bool requireTagWhenStaticAndActualTypesAreDifferent;

	public TypeAssigningEventEmitter(IEventEmitter JDJEJDIJLLE, bool KAHOIHHBHGG)
		: base(JDJEJDIJLLE)
	{
		requireTagWhenStaticAndActualTypesAreDifferent = KAHOIHHBHGG;
	}

	public override void Emit(ScalarEventInfo FNHCFCAALAE)
	{
		FNHCFCAALAE.SetIsPlainImplicit(true);
		FNHCFCAALAE.SetStyle(ScalarStyle.Plain);
		TypeCode typeCode = ((FNHCFCAALAE.GetSource().GetValue() != null) ? FNHCFCAALAE.GetSource().get_Type().GetTypeCode() : TypeCode.Empty);
		switch (typeCode)
		{
		case TypeCode.Boolean:
			FNHCFCAALAE.set_Tag("tag:yaml.org,2002:bool");
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
			FNHCFCAALAE.set_Tag("tag:yaml.org,2002:int");
			FNHCFCAALAE.set_RenderedValue(YamlFormatter.FormatNumber(FNHCFCAALAE.GetSource().GetValue()));
			break;
		case TypeCode.Single:
		case TypeCode.Double:
		case TypeCode.Decimal:
			FNHCFCAALAE.set_Tag("tag:yaml.org,2002:float");
			FNHCFCAALAE.set_RenderedValue(YamlFormatter.FormatNumber(FNHCFCAALAE.GetSource().GetValue()));
			break;
		case TypeCode.Char:
		case TypeCode.String:
			FNHCFCAALAE.set_Tag("tag:yaml.org,2002:str");
			FNHCFCAALAE.set_RenderedValue(FNHCFCAALAE.GetSource().GetValue().ToString());
			FNHCFCAALAE.SetStyle(ScalarStyle.Any);
			break;
		case TypeCode.DateTime:
			FNHCFCAALAE.set_Tag("tag:yaml.org,2002:timestamp");
			FNHCFCAALAE.set_RenderedValue(YamlFormatter.FormatDateTime(FNHCFCAALAE.GetSource().GetValue()));
			break;
		case TypeCode.Empty:
			FNHCFCAALAE.set_Tag("tag:yaml.org,2002:null");
			FNHCFCAALAE.set_RenderedValue(string.Empty);
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
		AssignTypeIfDifferent(FNHCFCAALAE);
		base.Emit(FNHCFCAALAE);
	}

	public override void Emit(SequenceStartEventInfo FNHCFCAALAE)
	{
		AssignTypeIfDifferent(FNHCFCAALAE);
		base.Emit(FNHCFCAALAE);
	}

	private void AssignTypeIfDifferent(ObjectEventInfo FNHCFCAALAE)
	{
		if (requireTagWhenStaticAndActualTypesAreDifferent && FNHCFCAALAE.GetSource().GetValue() != null && FNHCFCAALAE.GetSource().get_Type() != FNHCFCAALAE.GetSource().GetStaticType())
		{
			FNHCFCAALAE.set_Tag("!" + FNHCFCAALAE.GetSource().get_Type().AssemblyQualifiedName);
		}
	}
}
