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

	bool INodeDeserializer.Deserialize(EventReader reader, Type MBLGNMBFHBI, Func<EventReader, Type, object> IJBAEAEDMCC, out object value)
	{
		Scalar lEACOCDHICF = reader.Allow<Scalar>();
		if (lEACOCDHICF == null)
		{
			value = null;
			return false;
		}
		if (MBLGNMBFHBI.IsEnumCheck())
		{
			value = Enum.Parse(MBLGNMBFHBI, lEACOCDHICF.GetValue());
		}
		else
		{
			switch (MBLGNMBFHBI.GetTypeCode())
			{
			case TypeCode.Boolean:
				value = bool.Parse(lEACOCDHICF.GetValue());
				break;
			case TypeCode.Byte:
				value = byte.Parse(lEACOCDHICF.GetValue(), numberFormat);
				break;
			case TypeCode.Int16:
				value = short.Parse(lEACOCDHICF.GetValue(), numberFormat);
				break;
			case TypeCode.Int32:
				value = int.Parse(lEACOCDHICF.GetValue(), numberFormat);
				break;
			case TypeCode.Int64:
				value = long.Parse(lEACOCDHICF.GetValue(), numberFormat);
				break;
			case TypeCode.SByte:
				value = sbyte.Parse(lEACOCDHICF.GetValue(), numberFormat);
				break;
			case TypeCode.UInt16:
				value = ushort.Parse(lEACOCDHICF.GetValue(), numberFormat);
				break;
			case TypeCode.UInt32:
				value = uint.Parse(lEACOCDHICF.GetValue(), numberFormat);
				break;
			case TypeCode.UInt64:
				value = ulong.Parse(lEACOCDHICF.GetValue(), numberFormat);
				break;
			case TypeCode.Single:
				value = float.Parse(lEACOCDHICF.GetValue(), numberFormat);
				break;
			case TypeCode.Double:
				value = double.Parse(lEACOCDHICF.GetValue(), numberFormat);
				break;
			case TypeCode.Decimal:
				value = decimal.Parse(lEACOCDHICF.GetValue(), numberFormat);
				break;
			case TypeCode.String:
				value = lEACOCDHICF.GetValue();
				break;
			case TypeCode.Char:
				value = lEACOCDHICF.GetValue()[0];
				break;
			case TypeCode.DateTime:
				value = DateTime.Parse(lEACOCDHICF.GetValue(), CultureInfo.InvariantCulture);
				break;
			default:
				if (MBLGNMBFHBI == typeof(object))
				{
					value = lEACOCDHICF.GetValue();
				}
				else
				{
					value = TypeConverterHelper.ChangeType(lEACOCDHICF.GetValue(), MBLGNMBFHBI);
				}
				break;
			}
		}
		return true;
	}
}
