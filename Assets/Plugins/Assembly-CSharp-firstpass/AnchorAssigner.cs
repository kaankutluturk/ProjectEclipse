using System;
using System.Collections.Generic;
using System.Globalization;

public sealed class AnchorAssigner : IAliasProvider, IObjectGraphVisitor
{
	private class AnchorAssignment
	{
		public string Anchor;
	}

	private readonly IDictionary<object, AnchorAssignment> assignments = new Dictionary<object, AnchorAssignment>();

	private uint nextId;

	bool IObjectGraphVisitor.Enter(IObjectDescriptor value)
	{
		if (value.GetValue() == null || value.get_Type().GetTypeCode() != TypeCode.Object)
		{
			return false;
		}
		AnchorAssignment aliasInfo;
		if (assignments.TryGetValue(value.GetValue(), out aliasInfo))
		{
			if (aliasInfo.Anchor == null)
			{
				aliasInfo.Anchor = "o" + nextId.ToString(CultureInfo.InvariantCulture);
				nextId++;
			}
			return false;
		}
		assignments.Add(value.GetValue(), new AnchorAssignment());
		return true;
	}

	bool IObjectGraphVisitor.EnterMapping(IObjectDescriptor KGBGENDIMBC, IObjectDescriptor value)
	{
		return true;
	}

	bool IObjectGraphVisitor.EnterMapping(IPropertyDescriptor KGBGENDIMBC, IObjectDescriptor value)
	{
		return true;
	}

	void IObjectGraphVisitor.VisitScalar(IObjectDescriptor ADDIBOMFCNH)
	{
	}

	void IObjectGraphVisitor.VisitMappingStart(IObjectDescriptor JPEFEBICPFI, Type FHNELPLPIPI, Type EJGJHBGMCDM)
	{
	}

	void IObjectGraphVisitor.VisitMappingEnd(IObjectDescriptor JPEFEBICPFI)
	{
	}

	void IObjectGraphVisitor.VisitSequenceStart(IObjectDescriptor sequence, Type LKAAAFHOAGD)
	{
	}

	void IObjectGraphVisitor.VisitSequenceEnd(IObjectDescriptor sequence)
	{
	}

	string IAliasProvider.GetAlias(object target)
	{
		AnchorAssignment aliasInfo;
		if (target != null && assignments.TryGetValue(target, out aliasInfo))
		{
			return aliasInfo.Anchor;
		}
		return null;
	}
}
