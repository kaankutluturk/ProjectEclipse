using System;

public class GuidConverter : IYamlTypeConverter
{
	public bool Accepts(Type LFLGCDNKNJI)
	{
		return LFLGCDNKNJI == typeof(Guid);
	}

	public object ReadYaml(IParser BPGMNGAJMKK, Type LFLGCDNKNJI)
	{
		string g = ((Scalar)BPGMNGAJMKK.GetCurrent()).GetValue();
		BPGMNGAJMKK.MoveNext();
		return new Guid(g);
	}

	public void WriteYaml(IEmitter NPIDIMCLNEM, object value, Type LFLGCDNKNJI)
	{
		NPIDIMCLNEM.Emit(new Scalar(((Guid)value).ToString("D")));
	}
}
