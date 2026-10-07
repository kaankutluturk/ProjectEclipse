using System;
using System.Text.RegularExpressions;
using YamlDotNet.Core;

public abstract class NodeEvent : ParsingEvent
{
	internal static readonly Regex anchorValidator = new Regex("^[0-9a-zA-Z_\\-]+$", RegexOptions.None);

	private readonly string anchor;

	private readonly string tag;

	public string Anchor
	{
		get
		{
			return GetAnchor();
		}
	}

	public string Tag
	{
		get
		{
			return GetTag();
		}
	}

	public abstract bool IsCanonical { get; }

	protected NodeEvent(string KOLNNNLOCFE, string EDLADAAKMDF, Mark ILENLCMAMBH, Mark PCLFFOBJJFO)
		: base(ILENLCMAMBH, PCLFFOBJJFO)
	{
		if (KOLNNNLOCFE != null)
		{
			if (KOLNNNLOCFE.Length == 0)
			{
				throw new ArgumentException("Anchor value must not be empty.", "anchor");
			}
			if (!anchorValidator.IsMatch(KOLNNNLOCFE))
			{
				throw new ArgumentException("Anchor value must contain alphanumerical characters only.", "anchor");
			}
		}
		if (EDLADAAKMDF != null && EDLADAAKMDF.Length == 0)
		{
			throw new ArgumentException("Tag value must not be empty.", "tag");
		}
		this.anchor = KOLNNNLOCFE;
		this.tag = EDLADAAKMDF;
	}

	protected NodeEvent(string KOLNNNLOCFE, string EDLADAAKMDF)
		: this(KOLNNNLOCFE, EDLADAAKMDF, Mark.Empty, Mark.Empty)
	{
	}

	public string GetAnchor()
	{
		return anchor;
	}

	public string GetTag()
	{
		return tag;
	}

	public abstract bool GetIsCanonical();
}
