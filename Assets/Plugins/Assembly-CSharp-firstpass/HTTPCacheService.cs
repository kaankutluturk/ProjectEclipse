using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

public static class HTTPCacheService
{
	private const int LibraryVersion = 2;

	private static bool isSupported;

	private static bool isSupportedCheckDone;

	private static Dictionary<Uri, HTTPCacheFileInfo> library;

	private static Dictionary<ulong, HTTPCacheFileInfo> usedIndexes;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static string cacheFolder;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static string libraryPath;

	private static bool inClearThread;

	private static bool inMaintainenceThread;

	private static ulong NextNameIDX;

	public static bool IsSupported
	{
		get
		{
			return GetIsSupported();
		}
	}

	private static Dictionary<Uri, HTTPCacheFileInfo> Library
	{
		get
		{
			return GetLibrary();
		}
	}

	internal static string CacheFolder
	{
		get
		{
			return GetCacheFolder();
		}
		private set
		{
			SetCacheFolder(value);
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

	static HTTPCacheService()
	{
		usedIndexes = new Dictionary<ulong, HTTPCacheFileInfo>();
		NextNameIDX = 1uL;
	}

	public static bool GetIsSupported()
	{
		if (isSupportedCheckDone)
		{
			return isSupported;
		}
		try
		{
			File.Exists(HTTPManager.GetRootCacheFolder());
			isSupported = true;
		}
		catch
		{
			isSupported = false;
			HTTPManager.GetLogger().Warning("HTTPCacheService", "Cache Service Disabled!");
		}
		finally
		{
			isSupportedCheckDone = true;
		}
		return isSupported;
	}

	private static Dictionary<Uri, HTTPCacheFileInfo> GetLibrary()
	{
		LoadLibrary();
		return library;
	}

	internal static string GetCacheFolder()
	{
		return cacheFolder;
	}

	private static void SetCacheFolder(string value)
	{
		cacheFolder = value;
	}

	private static string GetLibraryPath()
	{
		return libraryPath;
	}

	private static void SetLibraryPath(string value)
	{
		libraryPath = value;
	}

	internal static void CheckSetup()
	{
		if (!GetIsSupported())
		{
			return;
		}
		try
		{
			SetupCacheFolder();
			LoadLibrary();
		}
		catch
		{
		}
	}

	internal static void SetupCacheFolder()
	{
		if (!GetIsSupported())
		{
			return;
		}
		try
		{
			if (string.IsNullOrEmpty(GetCacheFolder()) || string.IsNullOrEmpty(GetLibraryPath()))
			{
				SetCacheFolder(Path.Combine(HTTPManager.GetRootCacheFolder(), "HTTPCache"));
				if (!Directory.Exists(GetCacheFolder()))
				{
					Directory.CreateDirectory(GetCacheFolder());
				}
				SetLibraryPath(Path.Combine(HTTPManager.GetRootCacheFolder(), "Library"));
			}
		}
		catch
		{
		}
	}

	internal static ulong GetNameIdx()
	{
		lock (GetLibrary())
		{
			ulong kDIGFPPHJDL = NextNameIDX;
			do
			{
				NextNameIDX = ++NextNameIDX % ulong.MaxValue;
			}
			while (usedIndexes.ContainsKey(NextNameIDX));
			return kDIGFPPHJDL;
		}
	}

	internal static bool HasEntity(Uri KJHNCLAJMLO)
	{
		if (!GetIsSupported())
		{
			return false;
		}
		lock (GetLibrary())
		{
			return GetLibrary().ContainsKey(KJHNCLAJMLO);
		}
	}

	internal static bool DeleteEntity(Uri KJHNCLAJMLO, bool IPHPJPNKPMD = true)
	{
		if (!GetIsSupported())
		{
			return false;
		}
		object obj = HTTPCacheFileLock.Acquire(KJHNCLAJMLO);
		lock (obj)
		{
			try
			{
				lock (GetLibrary())
				{
					HTTPCacheFileInfo value;
					bool flag = GetLibrary().TryGetValue(KJHNCLAJMLO, out value);
					if (flag)
					{
						value.Delete();
					}
					if (flag && IPHPJPNKPMD)
					{
						GetLibrary().Remove(KJHNCLAJMLO);
						usedIndexes.Remove(value.GetMappedNameIdx());
					}
					return true;
				}
			}
			finally
			{
			}
		}
	}

	internal static bool IsCachedEntityExpiresInTheFuture(HTTPRequest ONOCIELLAPL)
	{
		if (!GetIsSupported())
		{
			return false;
		}
		lock (GetLibrary())
		{
			HTTPCacheFileInfo value;
			if (GetLibrary().TryGetValue(ONOCIELLAPL.GetCurrentUri(), out value))
			{
				return value.WillExpireInTheFuture();
			}
		}
		return false;
	}

	internal static void SetHeaders(HTTPRequest ONOCIELLAPL)
	{
		if (!GetIsSupported())
		{
			return;
		}
		lock (GetLibrary())
		{
			HTTPCacheFileInfo value;
			if (GetLibrary().TryGetValue(ONOCIELLAPL.GetCurrentUri(), out value))
			{
				value.SetUpRevalidationHeaders(ONOCIELLAPL);
			}
		}
	}

	internal static Stream GetBody(Uri KJHNCLAJMLO, out int BDBOAEGELMC)
	{
		BDBOAEGELMC = 0;
		if (!GetIsSupported())
		{
			return null;
		}
		lock (GetLibrary())
		{
			HTTPCacheFileInfo value;
			if (GetLibrary().TryGetValue(KJHNCLAJMLO, out value))
			{
				return value.GetBodyStream(out BDBOAEGELMC);
			}
		}
		return null;
	}

	internal static HTTPResponse GetFullResponse(HTTPRequest ONOCIELLAPL)
	{
		if (!GetIsSupported())
		{
			return null;
		}
		lock (GetLibrary())
		{
			HTTPCacheFileInfo value;
			if (GetLibrary().TryGetValue(ONOCIELLAPL.GetCurrentUri(), out value))
			{
				return value.ReadResponseTo(ONOCIELLAPL);
			}
		}
		return null;
	}

	internal static bool IsCacheble(Uri KJHNCLAJMLO, HTTPMethods FJLOLCPJACB, HTTPResponse GIHDDAKBMHE)
	{
		if (!GetIsSupported())
		{
			return false;
		}
		if (FJLOLCPJACB != HTTPMethods.Get)
		{
			return false;
		}
		if (GIHDDAKBMHE == null)
		{
			return false;
		}
		if (GIHDDAKBMHE.GetStatusCode() == 304)
		{
			return false;
		}
		if (GIHDDAKBMHE.GetStatusCode() < 200 || GIHDDAKBMHE.GetStatusCode() >= 400)
		{
			return false;
		}
		List<string> list = GIHDDAKBMHE.GetHeaderValues("cache-control");
		if (list != null && list.Exists((string PNJNBBFLCAH) =>
		{
			string text = PNJNBBFLCAH.ToLower();
			return text.Contains("no-store") || text.Contains("no-cache");
		}))
		{
			return false;
		}
		List<string> list2 = GIHDDAKBMHE.GetHeaderValues("pragma");
		if (list2 != null && list2.Exists((string PNJNBBFLCAH) =>
		{
			string text = PNJNBBFLCAH.ToLower();
			return text.Contains("no-store") || text.Contains("no-cache");
		}))
		{
			return false;
		}
		List<string> list3 = GIHDDAKBMHE.GetHeaderValues("content-range");
		if (list3 != null)
		{
			return false;
		}
		return true;
	}

	internal static HTTPCacheFileInfo Store(Uri KJHNCLAJMLO, HTTPMethods FJLOLCPJACB, HTTPResponse GIHDDAKBMHE)
	{
		if (GIHDDAKBMHE == null || GIHDDAKBMHE.GetData() == null || GIHDDAKBMHE.GetData().Length == 0)
		{
			return null;
		}
		if (!GetIsSupported())
		{
			return null;
		}
		HTTPCacheFileInfo value = null;
		lock (GetLibrary())
		{
			if (!GetLibrary().TryGetValue(KJHNCLAJMLO, out value))
			{
				GetLibrary().Add(KJHNCLAJMLO, value = new HTTPCacheFileInfo(KJHNCLAJMLO));
				usedIndexes.Add(value.GetMappedNameIdx(), value);
			}
			try
			{
				value.Store(GIHDDAKBMHE);
				return value;
			}
			catch
			{
				DeleteEntity(KJHNCLAJMLO);
				throw;
			}
		}
	}

	internal static Stream PrepareStreamed(Uri KJHNCLAJMLO, HTTPResponse GIHDDAKBMHE)
	{
		if (!GetIsSupported())
		{
			return null;
		}
		lock (GetLibrary())
		{
			HTTPCacheFileInfo value;
			if (!GetLibrary().TryGetValue(KJHNCLAJMLO, out value))
			{
				GetLibrary().Add(KJHNCLAJMLO, value = new HTTPCacheFileInfo(KJHNCLAJMLO));
				usedIndexes.Add(value.GetMappedNameIdx(), value);
			}
			try
			{
				return value.GetSaveStream(GIHDDAKBMHE);
			}
			catch
			{
				DeleteEntity(KJHNCLAJMLO);
				throw;
			}
		}
	}

	public static void BeginClear()
	{
		if (GetIsSupported() && !inClearThread)
		{
			inClearThread = true;
			SetupCacheFolder();
			new Thread(ClearImpl).Start();
		}
	}

	private static void ClearImpl(object KKNOCIPBIIK)
	{
		if (!GetIsSupported())
		{
			return;
		}
		try
		{
			string[] files = Directory.GetFiles(GetCacheFolder());
			for (int i = 0; i < files.Length; i++)
			{
				try
				{
					File.Delete(files[i]);
				}
				catch
				{
				}
			}
		}
		finally
		{
			usedIndexes.Clear();
			library.Clear();
			NextNameIDX = 1uL;
			SaveLibrary();
			inClearThread = false;
		}
	}

	public static void BeginMaintainence(HTTPCacheMaintananceParams HFBDNDCABLM)
	{
		if (HFBDNDCABLM == null)
		{
			throw new ArgumentNullException("maintananceParams == null");
		}
		if (!GetIsSupported() || inMaintainenceThread)
		{
			return;
		}
		inMaintainenceThread = true;
		SetupCacheFolder();
		new Thread((object KKNOCIPBIIK) =>
		{
			try
			{
				lock (GetLibrary())
				{
					DateTime dateTime = DateTime.UtcNow - HFBDNDCABLM.GetDeleteOlder();
					List<HTTPCacheFileInfo> list = new List<HTTPCacheFileInfo>();
					foreach (KeyValuePair<Uri, HTTPCacheFileInfo> item in GetLibrary())
					{
						if (item.Value.GetLastAccess() < dateTime && DeleteEntity(item.Key, false))
						{
							list.Add(item.Value);
						}
					}
					for (int i = 0; i < list.Count; i++)
					{
						GetLibrary().Remove(list[i].GetUri());
						usedIndexes.Remove(list[i].GetMappedNameIdx());
					}
					list.Clear();
					ulong num = GetCacheSize();
					if (num > HFBDNDCABLM.GetMaxCacheSize())
					{
						List<HTTPCacheFileInfo> list2 = new List<HTTPCacheFileInfo>(library.Count);
						foreach (KeyValuePair<Uri, HTTPCacheFileInfo> item2 in library)
						{
							list2.Add(item2.Value);
						}
						list2.Sort();
						int num2 = 0;
						while (num >= HFBDNDCABLM.GetMaxCacheSize() && num2 < list2.Count)
						{
							try
							{
								HTTPCacheFileInfo aEMMGBPFAHD = list2[num2];
								ulong num3 = (ulong)aEMMGBPFAHD.GetBodyLength();
								DeleteEntity(aEMMGBPFAHD.GetUri());
								num -= num3;
							}
							catch
							{
							}
							finally
							{
								num2++;
							}
						}
					}
				}
			}
			finally
			{
				SaveLibrary();
				inMaintainenceThread = false;
			}
		}).Start();
	}

	public static int GetCacheEntityCount()
	{
		if (!GetIsSupported())
		{
			return 0;
		}
		CheckSetup();
		lock (GetLibrary())
		{
			return GetLibrary().Count;
		}
	}

	public static ulong GetCacheSize()
	{
		ulong num = 0uL;
		if (!GetIsSupported())
		{
			return num;
		}
		CheckSetup();
		lock (GetLibrary())
		{
			foreach (KeyValuePair<Uri, HTTPCacheFileInfo> item in GetLibrary())
			{
				if (item.Value.GetBodyLength() > 0)
				{
					num += (ulong)item.Value.GetBodyLength();
				}
			}
			return num;
		}
	}

	private static void LoadLibrary()
	{
		if (library != null || !GetIsSupported())
		{
			return;
		}
		library = new Dictionary<Uri, HTTPCacheFileInfo>();
		if (!File.Exists(GetLibraryPath()))
		{
			DeleteUnusedFiles();
			return;
		}
		try
		{
			int num;
			lock (library)
			{
				using (FileStream input = new FileStream(GetLibraryPath(), FileMode.Open))
				{
					using (BinaryReader binaryReader = new BinaryReader(input))
					{
						num = binaryReader.ReadInt32();
						if (num > 1)
						{
							NextNameIDX = binaryReader.ReadUInt64();
						}
						int num2 = binaryReader.ReadInt32();
						for (int i = 0; i < num2; i++)
						{
							Uri uri = new Uri(binaryReader.ReadString());
							HTTPCacheFileInfo aEMMGBPFAHD = new HTTPCacheFileInfo(uri, binaryReader, num);
							if (aEMMGBPFAHD.IsExists())
							{
								library.Add(uri, aEMMGBPFAHD);
								if (num > 1)
								{
									usedIndexes.Add(aEMMGBPFAHD.GetMappedNameIdx(), aEMMGBPFAHD);
								}
							}
						}
					}
				}
			}
			if (num == 1)
			{
				BeginClear();
			}
			else
			{
				DeleteUnusedFiles();
			}
		}
		catch
		{
		}
	}

	internal static void SaveLibrary()
	{
		if (library == null || !GetIsSupported())
		{
			return;
		}
		try
		{
			lock (GetLibrary())
			{
				using (FileStream output = new FileStream(GetLibraryPath(), FileMode.Create))
				{
					using (BinaryWriter binaryWriter = new BinaryWriter(output))
					{
						binaryWriter.Write(2);
						binaryWriter.Write(NextNameIDX);
						binaryWriter.Write(GetLibrary().Count);
						foreach (KeyValuePair<Uri, HTTPCacheFileInfo> item in GetLibrary())
						{
							binaryWriter.Write(item.Key.ToString());
							item.Value.SaveTo(binaryWriter);
						}
					}
				}
			}
		}
		catch
		{
		}
	}

	internal static void SetBodyLength(Uri KJHNCLAJMLO, int DEFBMELCOHO)
	{
		if (!GetIsSupported())
		{
			return;
		}
		lock (GetLibrary())
		{
			HTTPCacheFileInfo value;
			if (GetLibrary().TryGetValue(KJHNCLAJMLO, out value))
			{
				value.set_BodyLength(DEFBMELCOHO);
				return;
			}
			GetLibrary().Add(KJHNCLAJMLO, value = new HTTPCacheFileInfo(KJHNCLAJMLO, DateTime.UtcNow, DEFBMELCOHO));
			usedIndexes.Add(value.GetMappedNameIdx(), value);
		}
	}

	private static void DeleteUnusedFiles()
	{
		if (!GetIsSupported())
		{
			return;
		}
		CheckSetup();
		string[] files = Directory.GetFiles(GetCacheFolder());
		for (int i = 0; i < files.Length; i++)
		{
			try
			{
				string fileName = Path.GetFileName(files[i]);
				ulong result = 0uL;
				bool flag = false;
				if (ulong.TryParse(fileName, NumberStyles.AllowHexSpecifier, null, out result))
				{
					lock (GetLibrary())
					{
						flag = !usedIndexes.ContainsKey(result);
					}
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					File.Delete(files[i]);
				}
			}
			catch
			{
			}
		}
	}
}
