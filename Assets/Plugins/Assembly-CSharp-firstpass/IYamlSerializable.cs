public interface IYamlSerializable
{
	void ReadYaml(IParser parser);

	void WriteYaml(IEmitter emitter);
}
