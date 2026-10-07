using System;

public interface IValueDeserializer
{
	object DeserializeValue(EventReader reader, Type expectedType, SerializerState state, IValueDeserializer nestedObjectDeserializer);
}
