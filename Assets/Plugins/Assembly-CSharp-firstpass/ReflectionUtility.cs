using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

internal static class ReflectionUtility
{
	public static Type GetImplementedGenericInterface(Type type, Type genericInterface)
	{
		foreach (Type item in GetImplementedInterfaces(type))
		{
			if (item.IsGenericTypeCheck() && item.GetGenericTypeDefinition() == genericInterface)
			{
				return item;
			}
		}
		return null;
	}

	public static IEnumerable<Type> GetImplementedInterfaces(Type type)
	{
		if (type.IsInterfaceCheck())
		{
			yield return type;
		}
		Type[] interfaces = type.GetInterfaces();
		for (int i = 0; i < interfaces.Length; i++)
		{
			yield return interfaces[i];
		}
	}

	public static MethodInfo GetMethod(Expression<Action> expression)
	{
		MethodInfo methodInfo = ((MethodCallExpression)expression.Body).Method;
		if (methodInfo.IsGenericMethod)
		{
			methodInfo = methodInfo.GetGenericMethodDefinition();
		}
		return methodInfo;
	}

	public static MethodInfo GetMethod<T>(Expression<Action<T>> expression)
	{
		MethodInfo methodInfo = ((MethodCallExpression)expression.Body).Method;
		if (methodInfo.IsGenericMethod)
		{
			methodInfo = methodInfo.GetGenericMethodDefinition();
		}
		return methodInfo;
	}
}
