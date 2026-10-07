using System;

public interface IYamlTypeConverter
{
	bool Accepts(Type LFLGCDNKNJI);

	object ReadYaml(IParser BPGMNGAJMKK, Type LFLGCDNKNJI);

	void WriteYaml(IEmitter NPIDIMCLNEM, object value, Type LFLGCDNKNJI);
}
