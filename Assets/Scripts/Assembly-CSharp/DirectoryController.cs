using System.Collections.Generic;

public class DirectoryController
{
	private static DirectoryController _instance;

	private List<string> _searchDirectory = new List<string>();

	public static DirectoryController Instance
	{
		get
		{
			return GetInstance();
		}
	}

	private DirectoryController()
	{
		ResetSearchDirectories();
	}

	public static DirectoryController GetInstance()
	{
		if (_instance == null)
		{
			_instance = new DirectoryController();
		}
		return _instance;
	}

	public static int GetIndexAfterMarker(string path, string MNMPGNFFOGA)
	{
		int num = path.IndexOf(MNMPGNFFOGA);
		return (num != -1) ? (num + MNMPGNFFOGA.Length) : 0;
	}

	public static string GetDrivePrefix()
	{
		return string.Empty;
	}

	public static string ResolvePath(string path)
	{
		if (IsPathWithDrive(path))
		{
			return path;
		}
		string empty = string.Empty;
		return empty + path;
	}

	public static bool IsPathWithDrive(string path)
	{
		string value = GetDrivePrefix();
		return path.Contains(value);
	}

	public static string StripProtocol(string path)
	{
		int num = GetIndexAfterMarker(path, "://");
		string result = path;
		if (num < path.Length)
		{
			result = path.Substring(num, path.Length);
		}
		return result;
	}

	private void ResetSearchDirectories()
	{
		_searchDirectory.Clear();
		_searchDirectory.Add(string.Empty);
	}
}
