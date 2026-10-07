using System;
using System.Collections.Generic;
using YamlDotNet.Core;

public sealed class NodeValueDeserializer : IValueDeserializer
{
	private readonly IList<INodeDeserializer> deserializers;

	private readonly IList<INodeTypeResolver> typeResolvers;

	public NodeValueDeserializer(IList<INodeDeserializer> deserializers, IList<INodeTypeResolver> typeResolvers)
	{
		if (deserializers == null)
		{
			throw new ArgumentNullException("deserializers");
		}
		this.deserializers = deserializers;
		if (typeResolvers == null)
		{
			throw new ArgumentNullException("typeResolvers");
		}
		this.typeResolvers = typeResolvers;
	}

	public object DeserializeValue(EventReader reader, Type expectedType, SerializerState state, IValueDeserializer rootDeserializer)
	{
		NodeEvent nodeEvent = reader.Peek<NodeEvent>();
		Type resolvedType = GetTypeFromEvent(nodeEvent, expectedType);
		try
		{
			foreach (INodeDeserializer item in deserializers)
			{
				object value;
				if (item.Deserialize(reader, resolvedType, (EventReader nestedReader, Type nestedType) => rootDeserializer.DeserializeValue(nestedReader, nestedType, state, rootDeserializer), out value))
				{
					return value;
				}
			}
		}
		catch (YamlException)
		{
			throw;
		}
		catch (Exception exception)
		{
			throw new YamlException(nodeEvent.GetStart(), nodeEvent.GetEnd(), "Exception during deserialization", exception);
		}
		throw new YamlException(nodeEvent.GetStart(), nodeEvent.GetEnd(), string.Format("No node deserializer was able to deserialize the node into type {0}", expectedType.AssemblyQualifiedName));
	}

	private Type GetTypeFromEvent(NodeEvent nodeEvent, Type currentType)
	{
		foreach (INodeTypeResolver item in typeResolvers)
		{
			if (item.Resolve(nodeEvent, ref currentType))
			{
				break;
			}
		}
		return currentType;
	}
}
