using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

public abstract class YamlVisitor : IYamlVisitor
{
	protected virtual void Visit(YamlStream ABJIEFMMIEK)
	{
	}

	protected virtual void Visited(YamlStream ABJIEFMMIEK)
	{
	}

	protected virtual void Visit(YamlDocument DPMKHPJABAF)
	{
	}

	protected virtual void Visited(YamlDocument DPMKHPJABAF)
	{
	}

	protected virtual void Visit(YamlScalarNode ADDIBOMFCNH)
	{
	}

	protected virtual void Visited(YamlScalarNode ADDIBOMFCNH)
	{
	}

	protected virtual void Visit(YamlSequenceNode sequence)
	{
	}

	protected virtual void Visited(YamlSequenceNode sequence)
	{
	}

	protected virtual void Visit(YamlMappingNode JPEFEBICPFI)
	{
	}

	protected virtual void Visited(YamlMappingNode JPEFEBICPFI)
	{
	}

	protected virtual void VisitChildren(YamlStream ABJIEFMMIEK)
	{
		foreach (YamlDocument document in ABJIEFMMIEK.Documents)
		{
			document.Accept(this);
		}
	}

	protected virtual void VisitChildren(YamlDocument DPMKHPJABAF)
	{
		if (DPMKHPJABAF.RootNode != null)
		{
			DPMKHPJABAF.RootNode.Accept(this);
		}
	}

	protected virtual void VisitChildren(YamlSequenceNode sequence)
	{
		foreach (YamlNode child in sequence.Children)
		{
			child.Accept(this);
		}
	}

	protected virtual void VisitChildren(YamlMappingNode JPEFEBICPFI)
	{
		foreach (KeyValuePair<YamlNode, YamlNode> child in JPEFEBICPFI.Children)
		{
			child.Key.Accept(this);
			child.Value.Accept(this);
		}
	}

	void IYamlVisitor.Visit(YamlStream ABJIEFMMIEK)
	{
		Visit(ABJIEFMMIEK);
		VisitChildren(ABJIEFMMIEK);
		Visited(ABJIEFMMIEK);
	}

	void IYamlVisitor.Visit(YamlDocument DPMKHPJABAF)
	{
		Visit(DPMKHPJABAF);
		VisitChildren(DPMKHPJABAF);
		Visited(DPMKHPJABAF);
	}

	void IYamlVisitor.Visit(YamlScalarNode ADDIBOMFCNH)
	{
		Visit(ADDIBOMFCNH);
		Visited(ADDIBOMFCNH);
	}

	void IYamlVisitor.Visit(YamlSequenceNode sequence)
	{
		Visit(sequence);
		VisitChildren(sequence);
		Visited(sequence);
	}

	void IYamlVisitor.Visit(YamlMappingNode JPEFEBICPFI)
	{
		Visit(JPEFEBICPFI);
		VisitChildren(JPEFEBICPFI);
		Visited(JPEFEBICPFI);
	}
}
