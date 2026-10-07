using System.Collections.Generic;
using System.Linq;

public sealed class CustomSerializationObjectGraphVisitor : ChainedObjectGraphVisitor
{
	private readonly IEmitter emitter;

	private readonly IEnumerable<IYamlTypeConverter> typeConverters;

	public CustomSerializationObjectGraphVisitor(IEmitter NPIDIMCLNEM, IObjectGraphVisitor GDMFLLGPLNO, IEnumerable<IYamlTypeConverter> DKICAFJEABL)
		: base(GDMFLLGPLNO)
	{
		this.emitter = NPIDIMCLNEM;
		this.typeConverters = ((DKICAFJEABL == null) ? Enumerable.Empty<IYamlTypeConverter>() : DKICAFJEABL.ToList());
	}

	public override bool Enter(IObjectDescriptor value)
	{
		IYamlTypeConverter bLNPLLKJFLC = typeConverters.FirstOrDefault((IYamlTypeConverter GNAONAPDDLD) => GNAONAPDDLD.Accepts(value.get_Type()));
		if (bLNPLLKJFLC != null)
		{
			bLNPLLKJFLC.WriteYaml(emitter, value.GetValue(), value.get_Type());
			return false;
		}
		IYamlSerializable mFKFJGLKJDL = value as IYamlSerializable;
		if (mFKFJGLKJDL != null)
		{
			mFKFJGLKJDL.WriteYaml(emitter);
			return false;
		}
		return base.Enter(value);
	}
}
