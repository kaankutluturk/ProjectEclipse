using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

public sealed class ReadablePropertiesTypeInspector : TypeInspectorSkeleton
{
	private sealed class ReflectionPropertyDescriptor : IPropertyDescriptor
	{
		private readonly PropertyInfo _propertyInfo;

		private readonly ITypeResolver typeResolver;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Type typeOverride;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int order;

		public string PropertyName
		{
			get
			{
				return get_Name();
			}
		}

		public Type OverrideType
		{
			get
			{
				return GetTypeOverride();
			}
			set
			{
				set_TypeOverride(value);
			}
		}

		public int SortOrder
		{
			get
			{
				return GetOrder();
			}
			set
			{
				set_Order(value);
			}
		}

		public bool CanWrite
		{
			get
			{
				return GetCanWrite();
			}
		}

		public ReflectionPropertyDescriptor(PropertyInfo OOEBLPMKOIH, ITypeResolver CBMKGNIHPFO)
		{
			_propertyInfo = OOEBLPMKOIH;
			typeResolver = CBMKGNIHPFO;
		}

		public string get_Name()
		{
			return _propertyInfo.Name;
		}

		public Type get_Type()
		{
			return _propertyInfo.PropertyType;
		}

		public Type GetTypeOverride()
		{
			return typeOverride;
		}

		public void set_TypeOverride(Type value)
		{
			typeOverride = value;
		}

		public int GetOrder()
		{
			return order;
		}

		public void set_Order(int value)
		{
			order = value;
		}

		public bool GetCanWrite()
		{
			return _propertyInfo.CanWrite;
		}

		public void Write(object target, object value)
		{
			_propertyInfo.SetValue(target, value, null);
		}

		public T GetCustomAttribute<T>() where T : Attribute
		{
			object[] customAttributes = _propertyInfo.GetCustomAttributes(typeof(T), true);
			return (T)customAttributes.FirstOrDefault();
		}

		public IObjectDescriptor Read(object target)
		{
			object value = _propertyInfo.GetValue(target, null);
			Type lFLGCDNKNJI = GetTypeOverride() ?? typeResolver.Resolve(get_Type(), value);
			return new ObjectDescriptor(value, lFLGCDNKNJI, get_Type());
		}
	}

	private readonly ITypeResolver typeResolver;

	public ReadablePropertiesTypeInspector(ITypeResolver CBMKGNIHPFO)
	{
		if (CBMKGNIHPFO == null)
		{
			throw new ArgumentNullException("typeResolver");
		}
		typeResolver = CBMKGNIHPFO;
	}

	private static bool IsValidProperty(PropertyInfo JLCGLCLEGBD)
	{
		return JLCGLCLEGBD.CanRead && JLCGLCLEGBD.GetGetMethod().GetParameters().Length == 0;
	}

	public override IEnumerable<IPropertyDescriptor> GetProperties(Type LFLGCDNKNJI, object EGJHGBCEPHO)
	{
		return LFLGCDNKNJI.GetPublicProperties().Where(IsValidProperty).Select((Func<PropertyInfo, IPropertyDescriptor>)((PropertyInfo PIIEECCHMAC) => new ReflectionPropertyDescriptor(PIIEECCHMAC, typeResolver)));
	}
}
