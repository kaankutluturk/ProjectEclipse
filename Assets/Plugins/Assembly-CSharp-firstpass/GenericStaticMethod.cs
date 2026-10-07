using System;
using System.Linq.Expressions;
using System.Reflection;

public sealed class GenericStaticMethod
{
	private readonly MethodInfo methodToCall;

	public GenericStaticMethod(Expression<Action> expression)
	{
		MethodCallExpression methodCallExpression = (MethodCallExpression)expression.Body;
		methodToCall = methodCallExpression.Method.GetGenericMethodDefinition();
	}

	public object Invoke(Type[] genericArguments, params object[] arguments)
	{
		try
		{
			return methodToCall.MakeGenericMethod(genericArguments).Invoke(null, arguments);
		}
		catch (TargetInvocationException ex)
		{
			throw ex.Unwrap();
		}
	}
}
