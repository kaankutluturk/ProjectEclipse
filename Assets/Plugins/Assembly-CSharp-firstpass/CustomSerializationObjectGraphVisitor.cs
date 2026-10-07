using System.Collections.Generic;
using System.Linq;

public sealed class CustomSerializationObjectGraphVisitor : ChainedObjectGraphVisitor
{
	private readonly IEmitter emitter;

	private readonly IEnumerable<IYamlTypeConverter> typeConverters;

	public CustomSerializationObjectGraphVisitor(IEmitter emitter, IObjectGraphVisitor visitor, IEnumerable<IYamlTypeConverter> converters)
		: base(visitor)
	{
		this.emitter = emitter;
		this.typeConverters = ((converters == null) ? Enumerable.Empty<IYamlTypeConverter>() : converters.ToList());
	}

	public override bool Enter(IObjectDescriptor value)
	{
		IYamlTypeConverter typeConverter = typeConverters.FirstOrDefault((IYamlTypeConverter converter) => converter.Accepts(value.get_Type()));
		if (typeConverter != null)
		{
			typeConverter.WriteYaml(emitter, value.GetValue(), value.get_Type());
			return false;
		}
		IYamlSerializable serializable = value as IYamlSerializable;
		if (serializable != null)
		{
			serializable.WriteYaml(emitter);
			return false;
		}
		return base.Enter(value);
	}
}
