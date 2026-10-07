using YamlDotNet.RepresentationModel;

public interface IYamlVisitor
{
	void Visit(YamlStream yamlStream);

	void Visit(YamlDocument yamlDocument);

	void Visit(YamlScalarNode scalarNode);

	void Visit(YamlSequenceNode sequence);

	void Visit(YamlMappingNode mappingNode);
}
