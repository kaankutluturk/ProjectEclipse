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

	public VersionContainer(int major, int minor, int build, int revision = -1)
	{
		SetVersion(major, minor, build, revision);
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

	public void SetVersion(int major, int minor = -1, int build = -1, int revision = -1)
	{
		SetMajor(major);
		SetMinor(minor);
		SetBuild(build);
		SetRevision(revision);
	}

	public static VersionContainer CreateVersion(string version)
	{
		return new VersionContainer(version);
	}

	public static VersionContainer CreateVersion(VersionContainer version)
	{
		return new VersionContainer(version.GetMajor(), version.GetMinor(), version.GetBuild(), version.GetRevision());
	}

	public static VersionContainer CreateVersion(int major, int minor = -1, int build = -1, int revision = -1)
	{
		return new VersionContainer(major, minor, build, revision);
	}

	[SpecialName]
	public static bool IsEqual(VersionContainer leftVersion, VersionContainer rightVersion)
	{
		CompareResult comparison = Compare(leftVersion, rightVersion);
		return comparison == CompareResult.Equally;
	}

	[SpecialName]
	public static bool IsEqual(VersionContainer leftVersion, string rightVersion)
	{
		return IsEqual(leftVersion, CreateVersion(rightVersion));
	}

	[SpecialName]
	public static bool IsNotEqual(VersionContainer leftVersion, VersionContainer rightVersion)
	{
		return !IsEqual(leftVersion, rightVersion);
	}

	[SpecialName]
	public static bool IsNotEqual(VersionContainer leftVersion, string rightVersion)
	{
		return IsNotEqual(leftVersion, CreateVersion(rightVersion));
	}

	[SpecialName]
	public static bool IsGreater(VersionContainer leftVersion, VersionContainer rightVersion)
	{
		CompareResult comparison = Compare(leftVersion, rightVersion);
		return comparison == CompareResult.More && comparison != CompareResult.Equally;
	}

	[SpecialName]
	public static bool IsGreater(VersionContainer leftVersion, string rightVersion)
	{
		return IsGreater(leftVersion, CreateVersion(rightVersion));
	}

	[SpecialName]
	public static bool IsLess(VersionContainer leftVersion, VersionContainer rightVersion)
	{
		CompareResult comparison = Compare(leftVersion, rightVersion);
		return comparison == CompareResult.Less && comparison != CompareResult.Equally;
	}

	[SpecialName]
	public static bool IsLess(VersionContainer leftVersion, string rightVersion)
	{
		return IsLess(leftVersion, CreateVersion(rightVersion));
	}

	[SpecialName]
	public static bool IsGreaterOrEqual(VersionContainer leftVersion, VersionContainer rightVersion)
	{
		CompareResult comparison = Compare(leftVersion, rightVersion);
		return comparison == CompareResult.More || comparison == CompareResult.Equally;
	}

	[SpecialName]
	public static bool IsGreaterOrEqual(VersionContainer leftVersion, string rightVersion)
	{
		return IsGreaterOrEqual(leftVersion, CreateVersion(rightVersion));
	}

	[SpecialName]
	public static bool IsLessOrEqual(VersionContainer leftVersion, VersionContainer rightVersion)
	{
		CompareResult comparison = Compare(leftVersion, rightVersion);
		return comparison == CompareResult.Less || comparison == CompareResult.Equally;
	}

	[SpecialName]
	public static bool IsLessOrEqual(VersionContainer leftVersion, string rightVersion)
	{
		return IsLessOrEqual(leftVersion, CreateVersion(rightVersion));
	}

	public string ToString(bool includeRevision)
	{
		if (includeRevision)
		{
			return string.Format("{0}.{1}.{2}.{3}", GetMajor(), GetMinor(), GetBuild(), GetRevision());
		}
		return string.Format("{0}.{1}.{2}", GetMajor(), GetMinor(), GetBuild());
	}

	public override bool Equals(object obj)
	{
		return obj is VersionContainer && IsEqual((VersionContainer)obj, this);
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
	public static string op_Implicit(VersionContainer version)
	{
		return version.ToString(true);
	}

	public bool Empty(bool includeRevision = false)
	{
		if (GetMajor() == 0 && GetMinor() == 0 && GetBuild() == 0 && (!includeRevision || GetRevision() == 0))
		{
			return true;
		}
		return false;
	}

	private static CompareResult Compare(VersionContainer leftVersion, VersionContainer rightVersion, int componentCount = 4)
	{
		int[] leftParts = leftVersion._versionSource;
		int[] rightParts = rightVersion._versionSource;
		for (int i = 0; i < componentCount; i++)
		{
			if (leftParts[i] != rightParts[i])
			{
				return (leftParts[i] > rightParts[i]) ? CompareResult.More : CompareResult.Less;
			}
		}
		return CompareResult.Equally;
	}

	public bool ForCurrentVersion(string version)
	{
		return ForCurrentVersion(new VersionContainer(version));
	}

	public bool ForCurrentVersion(VersionContainer version)
	{
		return Compare(this, version, 3) == CompareResult.Equally;
	}

	public bool IsOlderThan(VersionContainer version)
	{
		return Compare(this, version, 3) == CompareResult.Less;
	}
}
