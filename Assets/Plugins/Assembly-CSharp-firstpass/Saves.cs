using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class Saves
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static string datapath;

	public const string DataFolderName = "Saves";

	public static string DataDirectory
	{
		get
		{
			return GetDatapath();
		}
		private set
		{
			set_Datapath(value);
		}
	}

	static Saves()
	{
		string text = ((!Application.isEditor) ? ((!SystemProperties.IsMobilePlatform()) ? Application.dataPath : Application.persistentDataPath) : Application.dataPath.Remove(Application.dataPath.Length - 7, 7));
		set_Datapath(string.Format("{0}\\{1}", text.Replace("/", "\\"), "Saves"));
		if (!Directory.Exists(GetDatapath()))
		{
			Directory.CreateDirectory(GetDatapath());
		}
	}

	public static string GetDatapath()
	{
		return datapath;
	}

	private static void set_Datapath(string value)
	{
		datapath = value;
	}

	public static void Save(string HIOFDADIEME, object DMNBDBJNKME)
	{
		File.WriteAllText(string.Format("{0}\\{1}.json", GetDatapath(), HIOFDADIEME), JsonConvert.SerializeObject(DMNBDBJNKME));
	}

	public static T Load<T>(string HIOFDADIEME) where T : class
	{
		string path = string.Format("{0}\\{1}.json", GetDatapath(), HIOFDADIEME);
		if (File.Exists(path))
		{
			string value = File.ReadAllText(path);
			return JsonConvert.DeserializeObject<T>(value);
		}
		return (T)null;
	}
}
