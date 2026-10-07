using System;
using System.IO;
using JetBrains.Annotations;

public class NekkiUri : Uri
{
	private readonly string _fileName;

	private readonly string _fileNameWithExtension;

	private readonly string _extension;

	public string FileName
	{
		get
		{
			return GetFileName();
		}
	}

	public string FullFileName
	{
		get
		{
			return GetFullFileName();
		}
	}

	public string Extension
	{
		get
		{
			return GetExtension();
		}
	}

	public NekkiUri([NotNull] string GDJGOEDDJIJ)
		: base(GDJGOEDDJIJ)
	{
		_fileName = Path.GetFileNameWithoutExtension(GDJGOEDDJIJ);
		_fileNameWithExtension = Path.GetFileName(base.LocalPath);
		_extension = Path.GetExtension(base.LocalPath);
	}

	public string GetFileName()
	{
		return _fileName;
	}

	public string GetFullFileName()
	{
		return _fileNameWithExtension;
	}

	public string GetExtension()
	{
		return _extension;
	}

	public override string ToString()
	{
		return "NekkiUri [" + base.OriginalString + "]";
	}
}
