using System;
using System.Linq.Expressions;
using System.Reflection;

public sealed class GenericInstanceMethod<TInstance>
{
	private readonly MethodInfo methodToCall;

	public GenericInstanceMethod(Expression<Action<TInstance>> expression)
	{
		MethodCallExpression methodCallExpression = (MethodCallExpression)expression.Body;
		methodToCall = methodCallExpression.Method.GetGenericMethodDefinition();
	}

	public object Invoke(Type[] genericArguments, TInstance instance, params object[] arguments)
	{
		try
		{
			return methodToCall.MakeGenericMethod(genericArguments).Invoke(instance, arguments);
		}
		catch (TargetInvocationException ex)
		{
			throw ex.Unwrap();
		}
	}
}
