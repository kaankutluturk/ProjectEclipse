using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

public static class TypeConverterHelper
{
	public delegate bool TryParseDelegate<T>(string value, out T result);

	public static void RegisterTypeConverter<TConvertible, TConverter>() where TConverter : global::System.ComponentModel.TypeConverter
	{
		if (!TypeDescriptor.GetAttributes(typeof(TConvertible)).OfType<TypeConverterAttribute>().Any((TypeConverterAttribute attribute) => attribute.ConverterTypeName == typeof(TConverter).AssemblyQualifiedName))
		{
			TypeDescriptor.AddAttributes(typeof(TConvertible), new TypeConverterAttribute(typeof(TConverter)));
		}
	}

	public static T ChangeType<T>(object value)
	{
		return (T)ChangeType(value, typeof(T));
	}

	public static T ChangeType<T>(object value, IFormatProvider formatProvider)
	{
		return (T)ChangeType(value, typeof(T), formatProvider);
	}

	public static T ChangeType<T>(object value, CultureInfo culture)
	{
		return (T)ChangeType(value, typeof(T), culture);
	}

	public static object ChangeType(object value, Type targetType)
	{
		return ChangeType(value, targetType, CultureInfo.InvariantCulture);
	}

	public static object ChangeType(object value, Type targetType, IFormatProvider formatProvider)
	{
		return ChangeType(value, targetType, new CultureInfoAdapter(CultureInfo.CurrentCulture, formatProvider));
	}

	public static object ChangeType(object value, Type targetType, CultureInfo culture)
	{
		if (value == null || value is DBNull)
		{
			return (!targetType.IsValueTypeCheck()) ? null : Activator.CreateInstance(targetType);
		}
		Type type = value.GetType();
		if (targetType.IsAssignableFrom(type))
		{
			return value;
		}
		if (targetType.IsGenericTypeCheck())
		{
			Type genericTypeDefinition = targetType.GetGenericTypeDefinition();
			if (genericTypeDefinition == typeof(Nullable<>))
			{
				Type underlyingType = targetType.GetGenericArguments()[0];
				object obj = ChangeType(value, underlyingType, culture);
				return Activator.CreateInstance(targetType, obj);
			}
		}
		if (targetType.IsEnumCheck())
		{
			string text = value as string;
			return (text == null) ? value : Enum.Parse(targetType, text, true);
		}
		if (targetType == typeof(bool))
		{
			if ("0".Equals(value))
			{
				return false;
			}
			if ("1".Equals(value))
			{
				return true;
			}
		}
		System.ComponentModel.TypeConverter converter = TypeDescriptor.GetConverter(value);
		if (converter != null && converter.CanConvertTo(targetType))
		{
			return converter.ConvertTo(null, culture, value, targetType);
		}
		System.ComponentModel.TypeConverter converter2 = TypeDescriptor.GetConverter(targetType);
		if (converter2 != null && converter2.CanConvertFrom(type))
		{
			return converter2.ConvertFrom(null, culture, value);
		}
		Type[] array = new Type[2] { type, targetType };
		foreach (Type candidateType in array)
		{
			foreach (MethodInfo item in candidateType.GetPublicMethods())
			{
				if (!item.IsSpecialName || (!(item.Name == "op_Implicit") && !(item.Name == "op_Explicit")) || !targetType.IsAssignableFrom(item.ReturnParameter.ParameterType))
				{
					continue;
				}
				ParameterInfo[] parameters = item.GetParameters();
				if (parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(type))
				{
					try
					{
						return item.Invoke(null, new object[1] { value });
					}
					catch (TargetInvocationException invocationException)
					{
						throw invocationException.Unwrap();
					}
				}
			}
		}
		if (type == typeof(string))
		{
			try
			{
				MethodInfo methodInfo = targetType.GetPublicStaticMethod("Parse", typeof(string), typeof(IFormatProvider));
				if (methodInfo != null)
				{
					return methodInfo.Invoke(null, new object[2] { value, culture });
				}
				methodInfo = targetType.GetPublicStaticMethod("Parse", typeof(string));
				if (methodInfo != null)
				{
					return methodInfo.Invoke(null, new object[1] { value });
				}
			}
			catch (TargetInvocationException mPFFFAOGBJE2)
			{
				throw mPFFFAOGBJE2.Unwrap();
			}
		}
		if (targetType == typeof(TimeSpan))
		{
			return TimeSpan.Parse((string)ChangeType(value, typeof(string), CultureInfo.InvariantCulture));
		}
		return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
	}

	public static T TryParse<T>(string value) where T : struct
	{
		switch (typeof(T).GetTypeCode())
		{
		case TypeCode.Boolean:
			return (T)(object)TryParse<bool>(value, bool.TryParse);
		case TypeCode.Byte:
			return (T)(object)TryParse<byte>(value, byte.TryParse);
		case TypeCode.DateTime:
			return (T)(object)TryParse<DateTime>(value, DateTime.TryParse);
		case TypeCode.Decimal:
			return (T)(object)TryParse<decimal>(value, decimal.TryParse);
		case TypeCode.Double:
			return (T)(object)TryParse<double>(value, double.TryParse);
		case TypeCode.Int16:
			return (T)(object)TryParse<short>(value, short.TryParse);
		case TypeCode.Int32:
			return (T)(object)TryParse<int>(value, int.TryParse);
		case TypeCode.Int64:
			return (T)(object)TryParse<long>(value, long.TryParse);
		case TypeCode.SByte:
			return (T)(object)TryParse<sbyte>(value, sbyte.TryParse);
		case TypeCode.Single:
			return (T)(object)TryParse<float>(value, float.TryParse);
		case TypeCode.UInt16:
			return (T)(object)TryParse<ushort>(value, ushort.TryParse);
		case TypeCode.UInt32:
			return (T)(object)TryParse<uint>(value, uint.TryParse);
		case TypeCode.UInt64:
			return (T)(object)TryParse<ulong>(value, ulong.TryParse);
		default:
			throw new NotSupportedException(string.Format("Cannot parse type '{0}'.", typeof(T).FullName));
		}
	}

	public static T? TryParse<T>(string value, TryParseDelegate<T> tryParse) where T : struct
	{
		T parsedValue;
		return (!tryParse(value, out parsedValue)) ? ((T?)null) : new T?(parsedValue);
	}
}
