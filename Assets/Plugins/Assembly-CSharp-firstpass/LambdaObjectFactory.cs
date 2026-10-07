using System;

public sealed class LambdaObjectFactory : IObjectFactory
{
	private readonly Func<Type, object> factory;

	public LambdaObjectFactory(Func<Type, object> DJFCIPIMOBC)
	{
		if (DJFCIPIMOBC == null)
		{
			throw new ArgumentNullException("factory");
		}
		factory = DJFCIPIMOBC;
	}

	public object Create(Type type)
	{
		return factory(type);
	}
}
