using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

public class FullObjectGraphTraversalStrategy : IObjectGraphTraversalStrategy
{
	protected readonly Serializer serializer;

	private readonly int maxRecursion;

	private readonly ITypeInspector typeDescriptor;

	private readonly ITypeResolver typeResolver;

	private static readonly global::GenericInstanceMethod<FullObjectGraphTraversalStrategy> traverseGenericDictionaryHelper = new global::GenericInstanceMethod<FullObjectGraphTraversalStrategy>((FullObjectGraphTraversalStrategy s) => s.TraverseGenericDictionaryHelper<int, int>(null, null, 0));

	public FullObjectGraphTraversalStrategy(Serializer serializer, ITypeInspector typeDescriptor, ITypeResolver typeResolver, int maxRecursion)
	{
		if (maxRecursion <= 0)
		{
			throw new ArgumentOutOfRangeException("maxRecursion", maxRecursion, "maxRecursion must be greater than 1");
		}
		this.serializer = serializer;
		if (typeDescriptor == null)
		{
			throw new ArgumentNullException("typeDescriptor");
		}
		this.typeDescriptor = typeDescriptor;
		if (typeResolver == null)
		{
			throw new ArgumentNullException("typeResolver");
		}
		this.typeResolver = typeResolver;
		this.maxRecursion = maxRecursion;
	}

	void IObjectGraphTraversalStrategy.Traverse(IObjectDescriptor graph, IObjectGraphVisitor visitor)
	{
		Traverse(graph, visitor, 0);
	}

	protected virtual void Traverse(IObjectDescriptor value, IObjectGraphVisitor visitor, int currentDepth)
	{
		if (++currentDepth > maxRecursion)
		{
			throw new InvalidOperationException("Too much recursion when traversing the object graph");
		}
		if (!visitor.Enter(value))
		{
			return;
		}
		TypeCode typeCode = value.get_Type().GetTypeCode();
		switch (typeCode)
		{
		case TypeCode.Boolean:
		case TypeCode.Char:
		case TypeCode.SByte:
		case TypeCode.Byte:
		case TypeCode.Int16:
		case TypeCode.UInt16:
		case TypeCode.Int32:
		case TypeCode.UInt32:
		case TypeCode.Int64:
		case TypeCode.UInt64:
		case TypeCode.Single:
		case TypeCode.Double:
		case TypeCode.Decimal:
		case TypeCode.DateTime:
		case TypeCode.String:
			visitor.VisitScalar(value);
			return;
		case TypeCode.DBNull:
			visitor.VisitScalar(new ObjectDescriptor(null, typeof(object), typeof(object)));
			return;
		case TypeCode.Empty:
			throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "TypeCode.{0} is not supported.", typeCode));
		}
		if (value.GetValue() == null || value.get_Type() == typeof(TimeSpan))
		{
			visitor.VisitScalar(value);
			return;
		}
		Type underlyingType = Nullable.GetUnderlyingType(value.get_Type());
		if (underlyingType != null)
		{
			Traverse(new ObjectDescriptor(value.GetValue(), underlyingType, value.get_Type()), visitor, currentDepth);
		}
		else
		{
			TraverseObject(value, visitor, currentDepth);
		}
	}

	protected virtual void TraverseObject(IObjectDescriptor value, IObjectGraphVisitor visitor, int currentDepth)
	{
		if (typeof(IDictionary).IsAssignableFrom(value.get_Type()))
		{
			TraverseDictionary(value, visitor, currentDepth);
			return;
		}
		Type type = ReflectionUtility.GetImplementedGenericInterface(value.get_Type(), typeof(IDictionary<, >));
		if (type != null)
		{
			TraverseGenericDictionary(value, type, visitor, currentDepth);
		}
		else if (typeof(IEnumerable).IsAssignableFrom(value.get_Type()))
		{
			TraverseList(value, visitor, currentDepth);
		}
		else
		{
			TraverseProperties(value, visitor, currentDepth);
		}
	}

	protected virtual void TraverseDictionary(IObjectDescriptor dictionary, IObjectGraphVisitor visitor, int currentDepth)
	{
		visitor.VisitMappingStart(dictionary, typeof(object), typeof(object));
		foreach (DictionaryEntry item in (IDictionary)dictionary.GetValue())
		{
			IObjectDescriptor key = GetObjectDescriptor(item.Key, typeof(object));
			IObjectDescriptor value = GetObjectDescriptor(item.Value, typeof(object));
			if (visitor.EnterMapping(key, value))
			{
				Traverse(key, visitor, currentDepth);
				Traverse(value, visitor, currentDepth);
			}
		}
		visitor.VisitMappingEnd(dictionary);
	}

	private void TraverseGenericDictionary(IObjectDescriptor dictionary, Type dictionaryType, IObjectGraphVisitor visitor, int currentDepth)
	{
		Type[] genericArguments = dictionaryType.GetGenericArguments();
		visitor.VisitMappingStart(dictionary, genericArguments[0], genericArguments[1]);
		traverseGenericDictionaryHelper.Invoke(genericArguments, this, dictionary.GetValue(), visitor, currentDepth);
		visitor.VisitMappingEnd(dictionary);
	}

	private void TraverseGenericDictionaryHelper<TKey, TValue>(IDictionary<TKey, TValue> dictionary, IObjectGraphVisitor visitor, int currentDepth)
	{
		foreach (KeyValuePair<TKey, TValue> item in dictionary)
		{
			IObjectDescriptor key = GetObjectDescriptor(item.Key, typeof(TKey));
			IObjectDescriptor value = GetObjectDescriptor(item.Value, typeof(TValue));
			if (visitor.EnterMapping(key, value))
			{
				Traverse(key, visitor, currentDepth);
				Traverse(value, visitor, currentDepth);
			}
		}
	}

	private void TraverseList(IObjectDescriptor value, IObjectGraphVisitor visitor, int currentDepth)
	{
		Type type = ReflectionUtility.GetImplementedGenericInterface(value.get_Type(), typeof(IEnumerable<>));
		Type type2 = ((type == null) ? typeof(object) : type.GetGenericArguments()[0]);
		visitor.VisitSequenceStart(value, type2);
		foreach (object item in (IEnumerable)value.GetValue())
		{
			Traverse(GetObjectDescriptor(item, type2), visitor, currentDepth);
		}
		visitor.VisitSequenceEnd(value);
	}

	protected virtual void TraverseProperties(IObjectDescriptor value, IObjectGraphVisitor visitor, int currentDepth)
	{
		visitor.VisitMappingStart(value, typeof(string), typeof(object));
		foreach (IPropertyDescriptor item in typeDescriptor.GetProperties(value.get_Type(), value.GetValue()))
		{
			IObjectDescriptor propertyValue = item.Read(value.GetValue());
			if (visitor.EnterMapping(item, propertyValue))
			{
				Traverse(new ObjectDescriptor(item.get_Name(), typeof(string), typeof(string)), visitor, currentDepth);
				Traverse(propertyValue, visitor, currentDepth);
			}
		}
		visitor.VisitMappingEnd(value);
	}

	private IObjectDescriptor GetObjectDescriptor(object value, Type staticType)
	{
		return new ObjectDescriptor(value, typeResolver.Resolve(staticType, value), staticType);
	}
}
