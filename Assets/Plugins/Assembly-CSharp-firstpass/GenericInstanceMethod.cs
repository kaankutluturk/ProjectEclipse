using System;
using System.Linq.Expressions;
using System.Reflection;

public sealed class GenericInstanceMethod<TInstance>
{
	private readonly MethodInfo methodToCall;

	public GenericInstanceMethod(Expression<Action<TInstance>> BOPGDKGIGHM)
	{
		MethodCallExpression methodCallExpression = (MethodCallExpression)BOPGDKGIGHM.Body;
		methodToCall = methodCallExpression.Method.GetGenericMethodDefinition();
	}

	public object Invoke(Type[] GIAFINCFDLC, TInstance instance, params object[] arguments)
	{
		try
		{
			return methodToCall.MakeGenericMethod(GIAFINCFDLC).Invoke(instance, arguments);
		}
		catch (TargetInvocationException mPFFFAOGBJE)
		{
			throw mPFFFAOGBJE.Unwrap();
		}
	}
}
