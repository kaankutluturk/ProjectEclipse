using System;
using System.Runtime.CompilerServices;

public class VersionContainer
{
	private enum CompareResult
	{
		Equally = 0,
		More = 1,
		Less = 2
	}

	public static readonly VersionContainer Zero = new VersionContainer();

	private readonly int[] _versionSource = new int[4];

	public int Major
	{
		get
		{
			return GetMajor();
		}
		set
		{
			SetMajor(value);
		}
	}

	public int Minor
	{
		get
		{
			return GetMinor();
		}
		set
		{
			SetMinor(value);
		}
	}

	public int Build
	{
		get
		{
			return GetBuild();
		}
		set
		{
			SetBuild(value);
		}
	}

	public int Revision
	{
		get
		{
			return GetRevision();
		}
		set
		{
			SetRevision(value);
		}
	}

	public VersionContainer()
	{
		SetVersion(0, 0, 0, 0);
	}

	public VersionContainer(string version)
	{
		SetVersion(version);
	}

	public VersionContainer(int IGIOOCIDFIN, int IBGMIGIFNJM, int LDKAECLLDNG, int JJCDPPFGPDO = -1)
	{
		SetVersion(IGIOOCIDFIN, IBGMIGIFNJM, LDKAECLLDNG, JJCDPPFGPDO);
	}

	public int GetMajor()
	{
		return _versionSource[0];
	}

	public void SetMajor(int value)
	{
		_versionSource[0] = value;
	}

	public int GetMinor()
	{
		return _versionSource[1];
	}

	public void SetMinor(int value)
	{
		_versionSource[1] = value;
	}

	public int GetBuild()
	{
		return _versionSource[2];
	}

	public void SetBuild(int value)
	{
		_versionSource[2] = value;
	}

	public int GetRevision()
	{
		return _versionSource[3];
	}

	public void SetRevision(int value)
	{
		_versionSource[3] = value;
	}

	public void SetVersion(string version)
	{
		version = version.Trim();
		if (string.IsNullOrEmpty(version))
		{
			SetVersion(0, 0, 0, 0);
			return;
		}
		string[] array = version.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);
		SetVersion(Zero);
		try
		{
			for (int i = 0; i < array.Length; i++)
			{
				_versionSource[i] = int.Parse(array[i]);
			}
		}
		catch (Exception)
		{
			SetVersion(0, 0, 0, 0);
		}
	}

	public void SetVersion(VersionContainer version)
	{
		SetVersion(version.GetMajor(), version.GetMinor(), version.GetBuild(), version.GetRevision());
	}

	public void SetVersion(int IGIOOCIDFIN, int IBGMIGIFNJM = -1, int LDKAECLLDNG = -1, int JJCDPPFGPDO = -1)
	{
		SetMajor(IGIOOCIDFIN);
		SetMinor(IBGMIGIFNJM);
		SetBuild(LDKAECLLDNG);
		SetRevision(JJCDPPFGPDO);
	}

	public static VersionContainer CreateVersion(string version)
	{
		return new VersionContainer(version);
	}

	public static VersionContainer CreateVersion(VersionContainer version)
	{
		return new VersionContainer(version.GetMajor(), version.GetMinor(), version.GetBuild(), version.GetRevision());
	}

	public static VersionContainer CreateVersion(int IGIOOCIDFIN, int IBGMIGIFNJM = -1, int LDKAECLLDNG = -1, int JJCDPPFGPDO = -1)
	{
		return new VersionContainer(IGIOOCIDFIN, IBGMIGIFNJM, LDKAECLLDNG, JJCDPPFGPDO);
	}

	[SpecialName]
	public static bool IsEqual(VersionContainer LHBNIMGFKIB, VersionContainer AAOIAEJJINO)
	{
		CompareResult lOICEAFFHDO = Compare(LHBNIMGFKIB, AAOIAEJJINO);
		return lOICEAFFHDO == CompareResult.Equally;
	}

	[SpecialName]
	public static bool IsEqual(VersionContainer LHBNIMGFKIB, string AAOIAEJJINO)
	{
		return IsEqual(LHBNIMGFKIB, CreateVersion(AAOIAEJJINO));
	}

	[SpecialName]
	public static bool IsNotEqual(VersionContainer LHBNIMGFKIB, VersionContainer AAOIAEJJINO)
	{
		return !IsEqual(LHBNIMGFKIB, AAOIAEJJINO);
	}

	[SpecialName]
	public static bool IsNotEqual(VersionContainer LHBNIMGFKIB, string AAOIAEJJINO)
	{
		return IsNotEqual(LHBNIMGFKIB, CreateVersion(AAOIAEJJINO));
	}

	[SpecialName]
	public static bool IsGreater(VersionContainer LHBNIMGFKIB, VersionContainer AAOIAEJJINO)
	{
		CompareResult lOICEAFFHDO = Compare(LHBNIMGFKIB, AAOIAEJJINO);
		return lOICEAFFHDO == CompareResult.More && lOICEAFFHDO != CompareResult.Equally;
	}

	[SpecialName]
	public static bool IsGreater(VersionContainer LHBNIMGFKIB, string AAOIAEJJINO)
	{
		return IsGreater(LHBNIMGFKIB, CreateVersion(AAOIAEJJINO));
	}

	[SpecialName]
	public static bool IsLess(VersionContainer LHBNIMGFKIB, VersionContainer AAOIAEJJINO)
	{
		CompareResult lOICEAFFHDO = Compare(LHBNIMGFKIB, AAOIAEJJINO);
		return lOICEAFFHDO == CompareResult.Less && lOICEAFFHDO != CompareResult.Equally;
	}

	[SpecialName]
	public static bool IsLess(VersionContainer LHBNIMGFKIB, string AAOIAEJJINO)
	{
		return IsLess(LHBNIMGFKIB, CreateVersion(AAOIAEJJINO));
	}

	[SpecialName]
	public static bool IsGreaterOrEqual(VersionContainer LHBNIMGFKIB, VersionContainer AAOIAEJJINO)
	{
		CompareResult lOICEAFFHDO = Compare(LHBNIMGFKIB, AAOIAEJJINO);
		return lOICEAFFHDO == CompareResult.More || lOICEAFFHDO == CompareResult.Equally;
	}

	[SpecialName]
	public static bool IsGreaterOrEqual(VersionContainer LHBNIMGFKIB, string AAOIAEJJINO)
	{
		return IsGreaterOrEqual(LHBNIMGFKIB, CreateVersion(AAOIAEJJINO));
	}

	[SpecialName]
	public static bool IsLessOrEqual(VersionContainer LHBNIMGFKIB, VersionContainer AAOIAEJJINO)
	{
		CompareResult lOICEAFFHDO = Compare(LHBNIMGFKIB, AAOIAEJJINO);
		return lOICEAFFHDO == CompareResult.Less || lOICEAFFHDO == CompareResult.Equally;
	}

	[SpecialName]
	public static bool IsLessOrEqual(VersionContainer LHBNIMGFKIB, string AAOIAEJJINO)
	{
		return IsLessOrEqual(LHBNIMGFKIB, CreateVersion(AAOIAEJJINO));
	}

	public string ToString(bool MJHLPGDFEHA)
	{
		if (MJHLPGDFEHA)
		{
			return string.Format("{0}.{1}.{2}.{3}", GetMajor(), GetMinor(), GetBuild(), GetRevision());
		}
		return string.Format("{0}.{1}.{2}", GetMajor(), GetMinor(), GetBuild());
	}

	public override bool Equals(object AOMLCBHAJJH)
	{
		return AOMLCBHAJJH is VersionContainer && IsEqual((VersionContainer)AOMLCBHAJJH, this);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return ToString(false);
	}

	[SpecialName]
	public static string op_Implicit(VersionContainer AFIEJABPAKA)
	{
		return AFIEJABPAKA.ToString(true);
	}

	public bool Empty(bool MJHLPGDFEHA = false)
	{
		if (GetMajor() == 0 && GetMinor() == 0 && GetBuild() == 0 && (!MJHLPGDFEHA || GetRevision() == 0))
		{
			return true;
		}
		return false;
	}

	private static CompareResult Compare(VersionContainer LHBNIMGFKIB, VersionContainer AAOIAEJJINO, int IGIEDFIPIAN = 4)
	{
		int[] mOCMENBOJJF = LHBNIMGFKIB._versionSource;
		int[] mOCMENBOJJF2 = AAOIAEJJINO._versionSource;
		for (int i = 0; i < IGIEDFIPIAN; i++)
		{
			if (mOCMENBOJJF[i] != mOCMENBOJJF2[i])
			{
				return (mOCMENBOJJF[i] > mOCMENBOJJF2[i]) ? CompareResult.More : CompareResult.Less;
			}
		}
		return CompareResult.Equally;
	}

	public bool ForCurrentVersion(string JJCDPPFGPDO)
	{
		return ForCurrentVersion(new VersionContainer(JJCDPPFGPDO));
	}

	public bool ForCurrentVersion(VersionContainer JJCDPPFGPDO)
	{
		return Compare(this, JJCDPPFGPDO, 3) == CompareResult.Equally;
	}

	public bool IsOlderThan(VersionContainer JJCDPPFGPDO)
	{
		return Compare(this, JJCDPPFGPDO, 3) == CompareResult.Less;
	}
}
