using System;

public interface IPropertyDescriptor
{
	string PropertyName { get; }

	bool CanWrite { get; }

	Type OverrideType { get; set; }

	int SortOrder { get; set; }

	string get_Name();

	bool GetCanWrite();

	Type get_Type();

	Type GetTypeOverride();

	void set_TypeOverride(Type value);

	int GetOrder();

	void set_Order(int value);

	T GetCustomAttribute<T>() where T : Attribute;

	IObjectDescriptor Read(object target);

	void Write(object target, object value);
}
