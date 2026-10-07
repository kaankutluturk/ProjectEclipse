using System;
using System.Linq.Expressions;
using System.Reflection;

public sealed class GenericStaticMethod
{
	private readonly MethodInfo methodToCall;

	public GenericStaticMethod(Expression<Action> BOPGDKGIGHM)
	{
		MethodCallExpression methodCallExpression = (MethodCallExpression)BOPGDKGIGHM.Body;
		methodToCall = methodCallExpression.Method.GetGenericMethodDefinition();
	}

	public object Invoke(Type[] GIAFINCFDLC, params object[] arguments)
	{
		try
		{
			return methodToCall.MakeGenericMethod(GIAFINCFDLC).Invoke(null, arguments);
		}
		catch (TargetInvocationException mPFFFAOGBJE)
		{
			throw mPFFFAOGBJE.Unwrap();
		}
	}
}
