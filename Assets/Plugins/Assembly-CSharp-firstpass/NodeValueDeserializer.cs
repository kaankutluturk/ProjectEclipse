using System;
using System.Collections.Generic;
using YamlDotNet.Core;

public sealed class NodeValueDeserializer : IValueDeserializer
{
	private readonly IList<INodeDeserializer> deserializers;

	private readonly IList<INodeTypeResolver> typeResolvers;

	public NodeValueDeserializer(IList<INodeDeserializer> JJAHDOOOGFH, IList<INodeTypeResolver> FFEOGMPHPBI)
	{
		if (JJAHDOOOGFH == null)
		{
			throw new ArgumentNullException("deserializers");
		}
		this.deserializers = JJAHDOOOGFH;
		if (FFEOGMPHPBI == null)
		{
			throw new ArgumentNullException("typeResolvers");
		}
		this.typeResolvers = FFEOGMPHPBI;
	}

	public object DeserializeValue(EventReader reader, Type MBLGNMBFHBI, SerializerState state, IValueDeserializer IJBAEAEDMCC)
	{
		NodeEvent dGMPGIHHKCN = reader.Peek<NodeEvent>();
		Type mBLGNMBFHBI = GetTypeFromEvent(dGMPGIHHKCN, MBLGNMBFHBI);
		try
		{
			foreach (INodeDeserializer item in deserializers)
			{
				object value;
				if (item.Deserialize(reader, mBLGNMBFHBI, (EventReader BOPODEAIEBJ, Type GNAONAPDDLD) => IJBAEAEDMCC.DeserializeValue(BOPODEAIEBJ, GNAONAPDDLD, state, IJBAEAEDMCC), out value))
				{
					return value;
				}
			}
		}
		catch (YamlException)
		{
			throw;
		}
		catch (Exception oLABPFGLNFC)
		{
			throw new YamlException(dGMPGIHHKCN.GetStart(), dGMPGIHHKCN.GetEnd(), "Exception during deserialization", oLABPFGLNFC);
		}
		throw new YamlException(dGMPGIHHKCN.GetStart(), dGMPGIHHKCN.GetEnd(), string.Format("No node deserializer was able to deserialize the node into type {0}", MBLGNMBFHBI.AssemblyQualifiedName));
	}

	private Type GetTypeFromEvent(NodeEvent ABOEBNGCALL, Type PHOBEGPKAKH)
	{
		foreach (INodeTypeResolver item in typeResolvers)
		{
			if (item.Resolve(ABOEBNGCALL, ref PHOBEGPKAKH))
			{
				break;
			}
		}
		return PHOBEGPKAKH;
	}
}
