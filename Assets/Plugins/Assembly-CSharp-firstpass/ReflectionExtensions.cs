using System;
using System.Collections.Generic;
using System.Reflection;

internal static class ReflectionExtensions
{
	private static readonly FieldInfo remoteStackTraceField = typeof(Exception).GetField("_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);

	public static bool IsValueTypeCheck(this Type LFLGCDNKNJI)
	{
		return LFLGCDNKNJI.IsValueType;
	}

	public static bool IsGenericTypeCheck(this Type LFLGCDNKNJI)
	{
		return LFLGCDNKNJI.IsGenericType;
	}

	public static bool IsInterfaceCheck(this Type LFLGCDNKNJI)
	{
		return LFLGCDNKNJI.IsInterface;
	}

	public static bool IsEnumCheck(this Type LFLGCDNKNJI)
	{
		return LFLGCDNKNJI.IsEnum;
	}

	public static bool HasDefaultConstructor(this Type LFLGCDNKNJI)
	{
		return LFLGCDNKNJI.IsValueType || LFLGCDNKNJI.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null) != null;
	}

	public static TypeCode GetTypeCode(this Type LFLGCDNKNJI)
	{
		return Type.GetTypeCode(LFLGCDNKNJI);
	}

	public static IEnumerable<PropertyInfo> GetPublicProperties(this Type LFLGCDNKNJI)
	{
		return LFLGCDNKNJI.GetProperties(BindingFlags.Instance | BindingFlags.Public);
	}

	public static IEnumerable<MethodInfo> GetPublicMethods(this Type LFLGCDNKNJI)
	{
		return LFLGCDNKNJI.GetMethods(BindingFlags.Static | BindingFlags.Public);
	}

	public static MethodInfo GetPublicStaticMethod(this Type LFLGCDNKNJI, string name, params Type[] PEECGJDIAIK)
	{
		return LFLGCDNKNJI.GetMethod(name, BindingFlags.Static | BindingFlags.Public, null, PEECGJDIAIK, null);
	}

	public static Exception Unwrap(this TargetInvocationException MPFFFAOGBJE)
	{
		Exception innerException = MPFFFAOGBJE.InnerException;
		if (remoteStackTraceField != null)
		{
			remoteStackTraceField.SetValue(MPFFFAOGBJE.InnerException, MPFFFAOGBJE.InnerException.StackTrace + "\r\n");
		}
		return innerException;
	}
}
