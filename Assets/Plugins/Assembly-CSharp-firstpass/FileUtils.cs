using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

public class FileUtils
{
	public static void WriteAllBytes(string path, byte[] bytes)
	{
		CreateDirectory(Path.GetDirectoryName(path));
		File.WriteAllBytes(path, bytes);
	}

	public static void WriteText(string path, string text = "")
	{
		WriteOrAppendText(path, text, true);
	}

	public static void WriteOrAppendText(string path, string text = "", bool overwrite = false)
	{
		CreateDirectory(Path.GetDirectoryName(path));
		if (!overwrite && FileExists(path))
		{
			File.AppendAllText(path, text);
		}
		else
		{
			File.WriteAllText(path, text);
		}
	}

	public static void UseStreamWriter(string path, Action<TextWriter> action, bool append = false)
	{
		using (TextWriter writer = CreateStreamWriter(path, append))
		{
			action.SafeInvoke(writer);
		}
	}

	public static TextWriter CreateStreamWriter(string path, bool append = false)
	{
		CreateDirectory(Path.GetDirectoryName(path));
		return new StreamWriter(path, append, Encoding.UTF8);
	}

	public static FileStream OpenAppendStream(string path)
	{
		FileMode mode = ((!FileExists(path)) ? FileMode.OpenOrCreate : FileMode.Append);
		return OpenFileStream(path, mode);
	}

	public static void UseFileStream(string path, Action<FileStream> action, FileMode mode = FileMode.OpenOrCreate, FileAccess access = FileAccess.Write)
	{
		using (FileStream stream = OpenFileStream(path, mode, access))
		{
			action.SafeInvoke(stream);
		}
	}

	public static FileStream OpenFileStream(string path, FileMode mode = FileMode.OpenOrCreate, FileAccess access = FileAccess.Write)
	{
		CreateDirectory(Path.GetDirectoryName(path));
		return new FileStream(path, mode, access);
	}

	public static void SaveJson(string path, object obj, Formatting formatting = Formatting.None)
	{
		try
		{
			string json = JsonConvert.SerializeObject(obj, formatting);
			WriteText(path, json);
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

	public static bool MoveFile(string sourcePath, string destinationPath)
	{
		if (FileExists(sourcePath) && !IsFileLocked(sourcePath))
		{
			DeleteFile(destinationPath);
			File.Move(sourcePath, destinationPath);
			return true;
		}
		return false;
	}

	public static bool CopyFile(string sourcePath, string destinationPath)
	{
		if (FileExists(sourcePath))
		{
			CreateDirectory(Path.GetDirectoryName(destinationPath));
			File.Copy(sourcePath, destinationPath);
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
