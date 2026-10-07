using System;

public sealed class ObjectNodeDeserializer : INodeDeserializer
{
	private readonly IObjectFactory _objectFactory;

	private readonly ITypeInspector _typeDescriptor;

	private readonly bool _ignoreUnmatched;

	public ObjectNodeDeserializer(IObjectFactory EJPHFDCKCCE, ITypeInspector GIJPGEHPILC, bool GNFDAJLHBCN)
	{
		_objectFactory = EJPHFDCKCCE;
		_typeDescriptor = GIJPGEHPILC;
		_ignoreUnmatched = GNFDAJLHBCN;
	}

	bool INodeDeserializer.Deserialize(EventReader reader, Type MBLGNMBFHBI, Func<EventReader, Type, object> IJBAEAEDMCC, out object value)
	{
		MappingStart oGMPNFCPPDH = reader.Allow<MappingStart>();
		if (oGMPNFCPPDH == null)
		{
			value = null;
			return false;
		}
		value = _objectFactory.Create(MBLGNMBFHBI);
		while (!reader.Accept<MappingEnd>())
		{
			Scalar lEACOCDHICF = reader.Expect<Scalar>();
			IPropertyDescriptor JLCGLCLEGBD = _typeDescriptor.GetProperty(MBLGNMBFHBI, null, lEACOCDHICF.GetValue(), _ignoreUnmatched);
			if (JLCGLCLEGBD == null)
			{
				reader.SkipThisAndNestedEvents();
				continue;
			}
			object obj = IJBAEAEDMCC(reader, JLCGLCLEGBD.get_Type());
			IValuePromise aGAMFLELGLG = obj as IValuePromise;
			if (aGAMFLELGLG == null)
			{
				object bAINMLLIKOL = TypeConverterHelper.ChangeType(obj, JLCGLCLEGBD.get_Type());
				JLCGLCLEGBD.Write(value, bAINMLLIKOL);
				continue;
			}
			object valueRef = value;
			aGAMFLELGLG.add_ValueAvailable((object AFIEJABPAKA) =>
			{
				object bAINMLLIKOL2 = TypeConverterHelper.ChangeType(AFIEJABPAKA, JLCGLCLEGBD.get_Type());
				JLCGLCLEGBD.Write(valueRef, bAINMLLIKOL2);
			});
		}
		reader.Expect<MappingEnd>();
		return true;
	}
}
