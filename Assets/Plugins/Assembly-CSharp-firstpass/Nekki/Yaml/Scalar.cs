using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using YamlDotNet.RepresentationModel;

namespace Nekki.Yaml
{
	[Serializable]
	public class Scalar : Node
	{
		public delegate void TextUpdateHandler();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		[CompilerGenerated]
		private static TextUpdateHandler TextUpdate;

		private YamlScalarNode _scalar;

		public string text
		{
			get
			{
				return _scalar.Value;
			}
		}

		public static event TextUpdateHandler OnTextUpdate
		{
			add
			{
				AddTextUpdateHandler(value);
			}
			remove
			{
				RemoveTextUpdateHandler(value);
			}
		}

		public Scalar(string nodeKey, YamlScalarNode scalarNode)
		{
			base.typeNode = "Scalar";
			base.key = nodeKey;
			base.value = scalarNode;
			_scalar = (YamlScalarNode)base.value;
		}

		public Scalar(string nodeKey, string textValue)
		{
			base.typeNode = "Scalar";
			base.key = nodeKey;
			base.value = new YamlScalarNode(textValue);
			_scalar = (YamlScalarNode)base.value;
		}

		public static void AddTextUpdateHandler(TextUpdateHandler value)
		{
			TextUpdateHandler currentHandler = TextUpdate;
			TextUpdateHandler previousHandler;
			do
			{
				previousHandler = currentHandler;
				currentHandler = Interlocked.CompareExchange(ref TextUpdate, (TextUpdateHandler)Delegate.Combine(previousHandler, value), currentHandler);
			}
			while ((object)currentHandler != previousHandler);
		}

		public static void RemoveTextUpdateHandler(TextUpdateHandler value)
		{
			TextUpdateHandler currentHandler = TextUpdate;
			TextUpdateHandler previousHandler;
			do
			{
				previousHandler = currentHandler;
				currentHandler = Interlocked.CompareExchange(ref TextUpdate, (TextUpdateHandler)Delegate.Remove(previousHandler, value), currentHandler);
			}
			while ((object)currentHandler != previousHandler);
		}

		private static void RaiseTextUpdate()
		{
			TextUpdateHandler textUpdate = TextUpdate;
			if (textUpdate != null)
			{
				textUpdate();
			}
		}

		public void SetText(string textValue)
		{
			_scalar.Value = textValue;
			RaiseTextUpdate();
		}

		public string GetText()
		{
			return _scalar.Value;
		}
	}
}
