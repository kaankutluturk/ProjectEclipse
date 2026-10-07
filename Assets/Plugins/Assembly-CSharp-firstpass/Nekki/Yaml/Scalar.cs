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

		public Scalar(string HODKINDOEGD, YamlScalarNode DJMPIGLOHBC)
		{
			base.typeNode = "Scalar";
			base.key = HODKINDOEGD;
			base.value = DJMPIGLOHBC;
			_scalar = (YamlScalarNode)base.value;
		}

		public Scalar(string HODKINDOEGD, string NFICJMLCGEO)
		{
			base.typeNode = "Scalar";
			base.key = HODKINDOEGD;
			base.value = new YamlScalarNode(NFICJMLCGEO);
			_scalar = (YamlScalarNode)base.value;
		}

		public static void AddTextUpdateHandler(TextUpdateHandler value)
		{
			TextUpdateHandler gALGMABOBDE = TextUpdate;
			TextUpdateHandler gALGMABOBDE2;
			do
			{
				gALGMABOBDE2 = gALGMABOBDE;
				gALGMABOBDE = Interlocked.CompareExchange(ref TextUpdate, (TextUpdateHandler)Delegate.Combine(gALGMABOBDE2, value), gALGMABOBDE);
			}
			while ((object)gALGMABOBDE != gALGMABOBDE2);
		}

		public static void RemoveTextUpdateHandler(TextUpdateHandler value)
		{
			TextUpdateHandler gALGMABOBDE = TextUpdate;
			TextUpdateHandler gALGMABOBDE2;
			do
			{
				gALGMABOBDE2 = gALGMABOBDE;
				gALGMABOBDE = Interlocked.CompareExchange(ref TextUpdate, (TextUpdateHandler)Delegate.Remove(gALGMABOBDE2, value), gALGMABOBDE);
			}
			while ((object)gALGMABOBDE != gALGMABOBDE2);
		}

		private static void RaiseTextUpdate()
		{
			TextUpdateHandler textUpdate = TextUpdate;
			if (textUpdate != null)
			{
				textUpdate();
			}
		}

		public void SetText(string NFICJMLCGEO)
		{
			_scalar.Value = NFICJMLCGEO;
			RaiseTextUpdate();
		}

		public string GetText()
		{
			return _scalar.Value;
		}
	}
}
