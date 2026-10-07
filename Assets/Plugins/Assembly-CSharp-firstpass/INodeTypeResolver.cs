using System;

public interface INodeTypeResolver
{
	bool Resolve(NodeEvent nodeEvent, ref Type currentType);
}
