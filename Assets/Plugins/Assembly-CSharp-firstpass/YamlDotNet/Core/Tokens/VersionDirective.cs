using System;

namespace YamlDotNet.Core.Tokens
{
	[Serializable]
	public class VersionDirective : Token
	{
		private readonly Version version;

		public Version Version
		{
			get
			{
				return version;
			}
		}

		public VersionDirective(Version version)
			: this(version, Mark.Empty, Mark.Empty)
		{
		}

		public VersionDirective(Version version, Mark startMark, Mark endMark)
			: base(startMark, endMark)
		{
			version = version;
		}

		public override bool Equals(object obj)
		{
			VersionDirective versionDirective = obj as VersionDirective;
			return versionDirective != null && version.Equals(versionDirective.version);
		}

		public override int GetHashCode()
		{
			return version.GetHashCode();
		}
	}
}
