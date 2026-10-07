using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public static class CookieJar
{
	private const int Version = 1;

	private static List<Cookie> cookies = new List<Cookie>();

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static string cookieFolder;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static string libraryPath;

	private static object Locker = new object();

	private static bool isSavingSupported;

	private static bool isSupportCheckDone;

	private static bool loaded;

	public static bool IsSavingSupported
	{
		get
		{
			return GetIsSavingSupported();
		}
	}

	private static string CookieFolder
	{
		get
		{
			return GetCookieFolder();
		}
		set
		{
			SetCookieFolder(value);
		}
	}

	private static string LibraryPath
	{
		get
		{
			return GetLibraryPath();
		}
		set
		{
			SetLibraryPath(value);
		}
	}

	public static bool GetIsSavingSupported()
	{
		if (isSupportCheckDone)
		{
			return isSavingSupported;
		}
		try
		{
			File.Exists(HTTPManager.GetRootCacheFolder());
			isSavingSupported = true;
		}
		catch
		{
			isSavingSupported = false;
			HTTPManager.GetLogger().Warning("CookieJar", "Cookie saving and loading disabled!");
		}
		finally
		{
			isSupportCheckDone = true;
		}
		return isSavingSupported;
	}

	private static string GetCookieFolder()
	{
		return cookieFolder;
	}

	private static void SetCookieFolder(string value)
	{
		cookieFolder = value;
	}

	private static string GetLibraryPath()
	{
		return libraryPath;
	}

	private static void SetLibraryPath(string value)
	{
		libraryPath = value;
	}

	internal static void SetupFolder()
	{
		if (!GetIsSavingSupported())
		{
			return;
		}
		try
		{
			if (string.IsNullOrEmpty(GetCookieFolder()) || string.IsNullOrEmpty(GetLibraryPath()))
			{
				SetCookieFolder(Path.Combine(HTTPManager.GetRootCacheFolder(), "Cookies"));
				SetLibraryPath(Path.Combine(GetCookieFolder(), "Library"));
			}
		}
		catch
		{
		}
	}

	internal static void Set(HTTPResponse response)
	{
		if (response == null)
		{
			return;
		}
		lock (Locker)
		{
			try
			{
				Maintain();
				List<Cookie> list = new List<Cookie>();
				List<string> list2 = response.GetHeaderValues("set-cookie");
				if (list2 == null)
				{
					return;
				}
				foreach (string item in list2)
				{
					try
					{
						Cookie cookie = Cookie.Parse(item, response.BaseRequest.GetCurrentUri());
						if (cookie == null)
						{
							continue;
						}
						int index;
						Cookie eKAOIOLAGFH2 = Find(cookie, out index);
						if (!string.IsNullOrEmpty(cookie.GetValue()) && cookie.WillExpireInTheFuture())
						{
							if (eKAOIOLAGFH2 == null)
							{
								cookies.Add(cookie);
								list.Add(cookie);
							}
							else
							{
								cookie.SetDate(eKAOIOLAGFH2.GetDate());
								cookies[index] = cookie;
								list.Add(cookie);
							}
						}
						else if (index != -1)
						{
							cookies.RemoveAt(index);
						}
					}
					catch
					{
					}
				}
				response.SetCookies(list);
			}
			catch
			{
			}
		}
	}

	internal static void Maintain()
	{
		lock (Locker)
		{
			try
			{
				uint num = 0u;
				TimeSpan timeSpan = TimeSpan.FromDays(7.0);
				int num2 = 0;
				while (num2 < cookies.Count)
				{
					Cookie cookie = cookies[num2];
					if (!cookie.WillExpireInTheFuture() || cookie.GetLastAccess() + timeSpan < DateTime.UtcNow)
					{
						cookies.RemoveAt(num2);
						continue;
					}
					if (!cookie.GetIsSession())
					{
						num += cookie.GuessSize();
					}
					num2++;
				}
				if (num > HTTPManager.GetCookieJarSize())
				{
					cookies.Sort();
					while (num > HTTPManager.GetCookieJarSize() && cookies.Count > 0)
					{
						Cookie eKAOIOLAGFH2 = cookies[0];
						cookies.RemoveAt(0);
						num -= eKAOIOLAGFH2.GuessSize();
					}
				}
			}
			catch
			{
			}
		}
	}

	internal static void Persist()
	{
		if (!GetIsSavingSupported())
		{
			return;
		}
		lock (Locker)
		{
			try
			{
				Maintain();
				if (!Directory.Exists(GetCookieFolder()))
				{
					Directory.CreateDirectory(GetCookieFolder());
				}
				using (FileStream output = new FileStream(GetLibraryPath(), FileMode.Create))
				{
					using (BinaryWriter binaryWriter = new BinaryWriter(output))
					{
						binaryWriter.Write(1);
						int num = 0;
						foreach (Cookie item in cookies)
						{
							if (!item.GetIsSession())
							{
								num++;
							}
						}
						binaryWriter.Write(num);
						foreach (Cookie item2 in cookies)
						{
							if (!item2.GetIsSession())
							{
								item2.SaveTo(binaryWriter);
							}
						}
					}
				}
			}
			catch
			{
			}
		}
	}

	internal static void Load()
	{
		if (!GetIsSavingSupported())
		{
			return;
		}
		lock (Locker)
		{
			if (loaded)
			{
				return;
			}
			SetupFolder();
			try
			{
				cookies.Clear();
				if (!Directory.Exists(GetCookieFolder()))
				{
					Directory.CreateDirectory(GetCookieFolder());
				}
				if (!File.Exists(GetLibraryPath()))
				{
					return;
				}
				using (FileStream input = new FileStream(GetLibraryPath(), FileMode.Open))
				{
					using (BinaryReader binaryReader = new BinaryReader(input))
					{
						binaryReader.ReadInt32();
						int num = binaryReader.ReadInt32();
						for (int i = 0; i < num; i++)
						{
							Cookie cookie = new Cookie();
							cookie.LoadFrom(binaryReader);
							if (cookie.WillExpireInTheFuture())
							{
								cookies.Add(cookie);
							}
						}
					}
				}
			}
			catch
			{
				cookies.Clear();
			}
			finally
			{
				loaded = true;
			}
		}
	}

	public static List<Cookie> Get(Uri uri)
	{
		lock (Locker)
		{
			Load();
			List<Cookie> list = null;
			for (int i = 0; i < cookies.Count; i++)
			{
				Cookie cookie = cookies[i];
				if (cookie.WillExpireInTheFuture() && uri.Host.IndexOf(cookie.GetDomain()) != -1 && uri.AbsolutePath.StartsWith(cookie.GetPath()))
				{
					if (list == null)
					{
						list = new List<Cookie>();
					}
					list.Add(cookie);
				}
			}
			return list;
		}
	}

	public static void Set(Uri uri, Cookie cookie)
	{
		lock (Locker)
		{
			Load();
			Cookie newCookie = new Cookie(cookie.get_Name(), cookie.GetValue(), uri.AbsolutePath, uri.Host);
			int index;
			Find(newCookie, out index);
			if (index >= 0)
			{
				cookies[index] = newCookie;
			}
			else
			{
				cookies.Add(newCookie);
			}
		}
	}

	public static List<Cookie> GetAll()
	{
		lock (Locker)
		{
			Load();
			return cookies;
		}
	}

	public static void Clear()
	{
		lock (Locker)
		{
			Load();
			cookies.Clear();
		}
	}

	public static void Clear(TimeSpan maxAge)
	{
		lock (Locker)
		{
			Load();
			int num = 0;
			while (num < cookies.Count)
			{
				Cookie cookie = cookies[num];
				if (!cookie.WillExpireInTheFuture() || cookie.GetDate() + maxAge < DateTime.UtcNow)
				{
					cookies.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}
	}

	public static void Clear(string domain)
	{
		lock (Locker)
		{
			Load();
			int num = 0;
			while (num < cookies.Count)
			{
				Cookie cookie = cookies[num];
				if (!cookie.WillExpireInTheFuture() || cookie.GetDomain().IndexOf(domain) != -1)
				{
					cookies.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}
	}

	public static void Remove(Uri uri, string name)
	{
		lock (Locker)
		{
			Load();
			int num = 0;
			while (num < cookies.Count)
			{
				Cookie cookie = cookies[num];
				if (cookie.get_Name().Equals(name, StringComparison.OrdinalIgnoreCase) && uri.Host.IndexOf(cookie.GetDomain()) != -1)
				{
					cookies.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}
	}

	private static Cookie Find(Cookie cookie, out int index)
	{
		for (int i = 0; i < cookies.Count; i++)
		{
			Cookie item = cookies[i];
			if (item.Equals(cookie))
			{
				index = i;
				return item;
			}
		}
		index = -1;
		return null;
	}
}
