public interface IObjectGraphTraversalStrategy
{
	void Traverse(IObjectDescriptor graph, IObjectGraphVisitor visitor);
}
