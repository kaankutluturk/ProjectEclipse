using System;

public sealed class LambdaObjectFactory : IObjectFactory
{
	private readonly Func<Type, object> factory;

	public LambdaObjectFactory(Func<Type, object> createObject)
	{
		if (createObject == null)
		{
			throw new ArgumentNullException("factory");
		}
		factory = createObject;
	}

	public object Create(Type type)
	{
		return factory(type);
	}
}
