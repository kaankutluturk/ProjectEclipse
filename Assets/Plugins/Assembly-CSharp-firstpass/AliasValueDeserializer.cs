using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using YamlDotNet.Core;

public sealed class AliasValueDeserializer : IValueDeserializer
{
	private sealed class AliasState : Dictionary<string, ValuePromise>, IPostDeserializationCallback
	{
		public void OnDeserializationComplete()
		{
			foreach (ValuePromise value in base.Values)
			{
				if (!value.GetHasValue())
				{
					throw new AnchorNotFoundException(value.Alias.GetStart(), value.Alias.GetEnd(), string.Format("Anchor '{0}' not found", value.Alias.GetValue()));
				}
			}
		}
	}

	private sealed class ValuePromise : IValuePromise
	{
		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Action<object> ValueAvailable;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool hasValue;

		private object value;

		public readonly AnchorAlias Alias;

		public bool IsValueSet
		{
			get
			{
				return GetHasValue();
			}
			private set
			{
				set_HasValue(value);
			}
		}

		public event Action<object> ValueAvailableEvent
		{
			add
			{
				add_ValueAvailable(value);
			}
			remove
			{
				remove_ValueAvailable(value);
			}
		}

		public ValuePromise(AnchorAlias LOKLDPLAPOL)
		{
			Alias = LOKLDPLAPOL;
		}

		public ValuePromise(object value)
		{
			set_HasValue(true);
			this.value = value;
		}

		public void add_ValueAvailable(Action<object> value)
		{
			Action<object> action = ValueAvailable;
			Action<object> action2;
			do
			{
				action2 = action;
				action = Interlocked.CompareExchange(ref ValueAvailable, (Action<object>)Delegate.Combine(action2, value), action);
			}
			while ((object)action != action2);
		}

		public void remove_ValueAvailable(Action<object> value)
		{
			Action<object> action = ValueAvailable;
			Action<object> action2;
			do
			{
				action2 = action;
				action = Interlocked.CompareExchange(ref ValueAvailable, (Action<object>)Delegate.Remove(action2, value), action);
			}
			while ((object)action != action2);
		}

		public bool GetHasValue()
		{
			return hasValue;
		}

		private void set_HasValue(bool value)
		{
			hasValue = value;
		}

		public object GetValue()
		{
			if (!GetHasValue())
			{
				throw new InvalidOperationException("Value not set");
			}
			return value;
		}

		public void set_Value(object value)
		{
			if (GetHasValue())
			{
				throw new InvalidOperationException("Value already set");
			}
			set_HasValue(true);
			this.value = value;
			if (ValueAvailable != null)
			{
				ValueAvailable(value);
			}
		}
	}

	private readonly IValueDeserializer innerDeserializer;

	public AliasValueDeserializer(IValueDeserializer PMODPPCDACN)
	{
		if (PMODPPCDACN == null)
		{
			throw new ArgumentNullException("innerDeserializer");
		}
		this.innerDeserializer = PMODPPCDACN;
	}

	public object DeserializeValue(EventReader reader, Type MBLGNMBFHBI, SerializerState state, IValueDeserializer IJBAEAEDMCC)
	{
		AnchorAlias mBEGNNDMDKH = reader.Allow<AnchorAlias>();
		if (mBEGNNDMDKH != null)
		{
			AliasState dMGFMLMIFGL = state.Get<AliasState>();
			ValuePromise value;
			if (!dMGFMLMIFGL.TryGetValue(mBEGNNDMDKH.GetValue(), out value))
			{
				value = new ValuePromise(mBEGNNDMDKH);
				dMGFMLMIFGL.Add(mBEGNNDMDKH.GetValue(), value);
			}
			return (!value.GetHasValue()) ? value : value.GetValue();
		}
		string text = null;
		NodeEvent dGMPGIHHKCN = reader.Peek<NodeEvent>();
		if (dGMPGIHHKCN != null && !string.IsNullOrEmpty(dGMPGIHHKCN.GetAnchor()))
		{
			text = dGMPGIHHKCN.GetAnchor();
		}
		object obj = innerDeserializer.DeserializeValue(reader, MBLGNMBFHBI, state, IJBAEAEDMCC);
		if (text != null)
		{
			AliasState dMGFMLMIFGL2 = state.Get<AliasState>();
			ValuePromise value2;
			if (!dMGFMLMIFGL2.TryGetValue(text, out value2))
			{
				dMGFMLMIFGL2.Add(text, new ValuePromise(obj));
			}
			else
			{
				if (value2.GetHasValue())
				{
					throw new DuplicateAnchorException(dGMPGIHHKCN.GetStart(), dGMPGIHHKCN.GetEnd(), string.Format("Anchor '{0}' already defined", text));
				}
				value2.set_Value(obj);
			}
		}
		return obj;
	}
}
