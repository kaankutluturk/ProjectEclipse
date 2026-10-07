using System;
using System.Globalization;

public sealed class ScalarNodeDeserializer : INodeDeserializer
{
	private static readonly NumberFormatInfo numberFormat = new NumberFormatInfo
	{
		CurrencyDecimalSeparator = ".",
		CurrencyGroupSeparator = "_",
		CurrencyGroupSizes = new int[1] { 3 },
		CurrencySymbol = string.Empty,
		CurrencyDecimalDigits = 99,
		NumberDecimalSeparator = ".",
		NumberGroupSeparator = "_",
		NumberGroupSizes = new int[1] { 3 },
		NumberDecimalDigits = 99
	};

	bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
	{
		Scalar scalar = reader.Allow<Scalar>();
		if (scalar == null)
		{
			value = null;
			return false;
		}
		if (expectedType.IsEnumCheck())
		{
			value = Enum.Parse(expectedType, scalar.GetValue());
		}
		else
		{
			switch (expectedType.GetTypeCode())
			{
			case TypeCode.Boolean:
				value = bool.Parse(scalar.GetValue());
				break;
			case TypeCode.Byte:
				value = byte.Parse(scalar.GetValue(), numberFormat);
				break;
			case TypeCode.Int16:
				value = short.Parse(scalar.GetValue(), numberFormat);
				break;
			case TypeCode.Int32:
				value = int.Parse(scalar.GetValue(), numberFormat);
				break;
			case TypeCode.Int64:
				value = long.Parse(scalar.GetValue(), numberFormat);
				break;
			case TypeCode.SByte:
				value = sbyte.Parse(scalar.GetValue(), numberFormat);
				break;
			case TypeCode.UInt16:
				value = ushort.Parse(scalar.GetValue(), numberFormat);
				break;
			case TypeCode.UInt32:
				value = uint.Parse(scalar.GetValue(), numberFormat);
				break;
			case TypeCode.UInt64:
				value = ulong.Parse(scalar.GetValue(), numberFormat);
				break;
			case TypeCode.Single:
				value = float.Parse(scalar.GetValue(), numberFormat);
				break;
			case TypeCode.Double:
				value = double.Parse(scalar.GetValue(), numberFormat);
				break;
			case TypeCode.Decimal:
				value = decimal.Parse(scalar.GetValue(), numberFormat);
				break;
			case TypeCode.String:
				value = scalar.GetValue();
				break;
			case TypeCode.Char:
				value = scalar.GetValue()[0];
				break;
			case TypeCode.DateTime:
				value = DateTime.Parse(scalar.GetValue(), CultureInfo.InvariantCulture);
				break;
			default:
				if (expectedType == typeof(object))
				{
					value = scalar.GetValue();
				}
				else
				{
					value = TypeConverterHelper.ChangeType(scalar.GetValue(), expectedType);
				}
				break;
			}
		}
		return true;
	}
}
