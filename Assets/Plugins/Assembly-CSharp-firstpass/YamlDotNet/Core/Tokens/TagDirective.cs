using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace YamlDotNet.Core.Tokens
{
	[Serializable]
	public class TagDirective : Token
	{
		private readonly string handle;

		private readonly string prefix;

		private static readonly Regex tagHandleValidator = new Regex("^!([0-9A-Za-z_\\-]*!)?$", RegexOptions.None);

		public string Handle
		{
			get
			{
				return handle;
			}
		}

		public string Prefix
		{
			get
			{
				return prefix;
			}
		}

		public TagDirective(string tagHandle, string tagPrefix)
			: this(tagHandle, tagPrefix, Mark.Empty, Mark.Empty)
		{
		}

		public TagDirective(string tagHandle, string tagPrefix, Mark startMark, Mark endMark)
			: base(startMark, endMark)
		{
			if (string.IsNullOrEmpty(tagHandle))
			{
				throw new ArgumentNullException("handle", "Tag handle must not be empty.");
			}
			if (!tagHandleValidator.IsMatch(tagHandle))
			{
				throw new ArgumentException("Tag handle must start and end with '!' and contain alphanumerical characters only.", "handle");
			}
			handle = tagHandle;
			if (string.IsNullOrEmpty(tagPrefix))
			{
				throw new ArgumentNullException("prefix", "Tag prefix must not be empty.");
			}
			prefix = tagPrefix;
		}

		public override bool Equals(object obj)
		{
			TagDirective tagDirective = obj as TagDirective;
			return tagDirective != null && handle.Equals(tagDirective.handle) && prefix.Equals(tagDirective.prefix);
		}

		public override int GetHashCode()
		{
			return handle.GetHashCode() ^ prefix.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} => {1}", handle, prefix);
		}
	}
}
