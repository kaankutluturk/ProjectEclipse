using System;

public class GuidConverter : IYamlTypeConverter
{
	public bool Accepts(Type type)
	{
		return type == typeof(Guid);
	}

	public object ReadYaml(IParser parser, Type type)
	{
		string g = ((Scalar)parser.GetCurrent()).GetValue();
		parser.MoveNext();
		return new Guid(g);
	}

	public void WriteYaml(IEmitter emitter, object value, Type type)
	{
		emitter.Emit(new Scalar(((Guid)value).ToString("D")));
	}
}
