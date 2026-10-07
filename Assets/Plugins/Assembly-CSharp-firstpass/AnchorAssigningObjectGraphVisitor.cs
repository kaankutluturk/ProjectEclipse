using System;
using System.Collections.Generic;

public sealed class AnchorAssigningObjectGraphVisitor : ChainedObjectGraphVisitor
{
	private readonly IEventEmitter eventEmitter;

	private readonly IAliasProvider aliasProvider;

	private readonly HashSet<string> emittedAliases = new HashSet<string>();

	public AnchorAssigningObjectGraphVisitor(IObjectGraphVisitor GDMFLLGPLNO, IEventEmitter OPIGMJHGIDL, IAliasProvider JNNJMIPHLBI)
		: base(GDMFLLGPLNO)
	{
		this.eventEmitter = OPIGMJHGIDL;
		this.aliasProvider = JNNJMIPHLBI;
	}

	public override bool Enter(IObjectDescriptor value)
	{
		string text = aliasProvider.GetAlias(value.GetValue());
		if (text != null && !emittedAliases.Add(text))
		{
			IEventEmitter oPIGMJHGIDL = eventEmitter;
			AliasEventInfo nDOLNPCPJCJ = new AliasEventInfo(value);
			nDOLNPCPJCJ.set_Alias(text);
			oPIGMJHGIDL.Emit(nDOLNPCPJCJ);
			return false;
		}
		return base.Enter(value);
	}

	public override void VisitMappingStart(IObjectDescriptor JPEFEBICPFI, Type FHNELPLPIPI, Type EJGJHBGMCDM)
	{
		IEventEmitter oPIGMJHGIDL = eventEmitter;
		MappingStartEventInfo lPADMPIAIPF = new MappingStartEventInfo(JPEFEBICPFI);
		lPADMPIAIPF.SetAnchor(aliasProvider.GetAlias(JPEFEBICPFI.GetValue()));
		oPIGMJHGIDL.Emit(lPADMPIAIPF);
	}

	public override void VisitSequenceStart(IObjectDescriptor sequence, Type LKAAAFHOAGD)
	{
		IEventEmitter oPIGMJHGIDL = eventEmitter;
		SequenceStartEventInfo pBGMOJFHMGI = new SequenceStartEventInfo(sequence);
		pBGMOJFHMGI.SetAnchor(aliasProvider.GetAlias(sequence.GetValue()));
		oPIGMJHGIDL.Emit(pBGMOJFHMGI);
	}

	public override void VisitScalar(IObjectDescriptor ADDIBOMFCNH)
	{
		IEventEmitter oPIGMJHGIDL = eventEmitter;
		ScalarEventInfo gEPIKBBFJED = new ScalarEventInfo(ADDIBOMFCNH);
		gEPIKBBFJED.SetAnchor(aliasProvider.GetAlias(ADDIBOMFCNH.GetValue()));
		oPIGMJHGIDL.Emit(gEPIKBBFJED);
	}
}
