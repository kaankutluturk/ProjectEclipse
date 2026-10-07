using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

public class FileUtils
{
	public static void WriteAllBytes(string path, byte[] KPAMPCLHCEN)
	{
		CreateDirectory(Path.GetDirectoryName(path));
		File.WriteAllBytes(path, KPAMPCLHCEN);
	}

	public static void WriteText(string path, string DMNBDBJNKME = "")
	{
		WriteOrAppendText(path, DMNBDBJNKME, true);
	}

	public static void WriteOrAppendText(string path, string DMNBDBJNKME = "", bool OGOAJGKFMPF = false)
	{
		CreateDirectory(Path.GetDirectoryName(path));
		if (!OGOAJGKFMPF && FileExists(path))
		{
			File.AppendAllText(path, DMNBDBJNKME);
		}
		else
		{
			File.WriteAllText(path, DMNBDBJNKME);
		}
	}

	public static void UseStreamWriter(string path, Action<TextWriter> IBODMPMJELJ, bool FBLOBGLPAFJ = false)
	{
		using (TextWriter bAINMLLIKOL = CreateStreamWriter(path, FBLOBGLPAFJ))
		{
			IBODMPMJELJ.SafeInvoke(bAINMLLIKOL);
		}
	}

	public static TextWriter CreateStreamWriter(string path, bool FBLOBGLPAFJ = false)
	{
		CreateDirectory(Path.GetDirectoryName(path));
		return new StreamWriter(path, FBLOBGLPAFJ, Encoding.UTF8);
	}

	public static FileStream OpenAppendStream(string path)
	{
		FileMode nMMPBADCFHK = ((!FileExists(path)) ? FileMode.OpenOrCreate : FileMode.Append);
		return OpenFileStream(path, nMMPBADCFHK);
	}

	public static void UseFileStream(string path, Action<FileStream> IBODMPMJELJ, FileMode NMMPBADCFHK = FileMode.OpenOrCreate, FileAccess HIOFGOHJANN = FileAccess.Write)
	{
		using (FileStream bAINMLLIKOL = OpenFileStream(path, NMMPBADCFHK, HIOFGOHJANN))
		{
			IBODMPMJELJ.SafeInvoke(bAINMLLIKOL);
		}
	}

	public static FileStream OpenFileStream(string path, FileMode NMMPBADCFHK = FileMode.OpenOrCreate, FileAccess HIOFGOHJANN = FileAccess.Write)
	{
		CreateDirectory(Path.GetDirectoryName(path));
		return new FileStream(path, NMMPBADCFHK, HIOFGOHJANN);
	}

	public static void SaveJson(string path, object AOMLCBHAJJH, Formatting LFHMGPBFEPI = Formatting.None)
	{
		try
		{
			string dMNBDBJNKME = JsonConvert.SerializeObject(AOMLCBHAJJH, LFHMGPBFEPI);
			WriteText(path, dMNBDBJNKME);
		}
		catch (Exception ex)
		{
			Debug.LogError("Error SaveJSON config [" + ex.Message + "]");
		}
	}

	public static T ReadFileJson<T>(string path) where T : class
	{
		string text = ReadAllText(path);
		if (!text.IsNullOrEmpty())
		{
			try
			{
				return JsonConvert.DeserializeObject<T>(text);
			}
			catch (Exception ex)
			{
				Debug.LogError("Error ReadFileJson [" + path + " " + ex.Message + "]");
			}
		}
		return (T)null;
	}

	public static string ReadAllText(string path)
	{
		if (FileExists(path))
		{
			return File.ReadAllText(path);
		}
		return null;
	}

	public static string[] ReadAllLines(string path)
	{
		if (FileExists(path))
		{
			return File.ReadAllLines(path);
		}
		return null;
	}

	public static byte[] ReadAllBytes(string path)
	{
		if (FileExists(path))
		{
			return File.ReadAllBytes(path);
		}
		return null;
	}

	public static bool FileExists(string path)
	{
		return File.Exists(path);
	}

	public static bool DirectoryExists(string path)
	{
		return Directory.Exists(path);
	}

	public static bool IsFileLocked(string path)
	{
		FileInfo fileInfo = new FileInfo(path);
		FileStream fileStream = null;
		try
		{
			fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
		}
		catch (FileNotFoundException)
		{
			return false;
		}
		catch (IOException)
		{
			return true;
		}
		finally
		{
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
		return false;
	}

	public static bool DeleteFile(string path)
	{
		if (FileExists(path) && !IsFileLocked(path))
		{
			File.Delete(path);
			return true;
		}
		return false;
	}

	public static bool MoveFile(string OOFLNBMPPID, string FMEOELPPAFJ)
	{
		if (FileExists(OOFLNBMPPID) && !IsFileLocked(OOFLNBMPPID))
		{
			DeleteFile(FMEOELPPAFJ);
			File.Move(OOFLNBMPPID, FMEOELPPAFJ);
			return true;
		}
		return false;
	}

	public static bool CopyFile(string OOFLNBMPPID, string FMEOELPPAFJ)
	{
		if (FileExists(OOFLNBMPPID))
		{
			CreateDirectory(Path.GetDirectoryName(FMEOELPPAFJ));
			File.Copy(OOFLNBMPPID, FMEOELPPAFJ);
			return true;
		}
		return false;
	}

	public static bool DeleteDirectory(string path)
	{
		if (DirectoryExists(path))
		{
			Directory.Delete(path, true);
			return true;
		}
		return false;
	}

	public static void CreateDirectory(string path)
	{
		if (!DirectoryExists(path))
		{
			Directory.CreateDirectory(path);
		}
	}
}
