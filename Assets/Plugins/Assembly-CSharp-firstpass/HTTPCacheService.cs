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
			ulong nameIndex = NextNameIDX;
			do
			{
				NextNameIDX = ++NextNameIDX % ulong.MaxValue;
			}
			while (usedIndexes.ContainsKey(NextNameIDX));
			return nameIndex;
		}
	}

	internal static bool HasEntity(Uri uri)
	{
		if (!GetIsSupported())
		{
			return false;
		}
		lock (GetLibrary())
		{
			return GetLibrary().ContainsKey(uri);
		}
	}

	internal static bool DeleteEntity(Uri uri, bool removeFromLibrary = true)
	{
		if (!GetIsSupported())
		{
			return false;
		}
		object obj = HTTPCacheFileLock.Acquire(uri);
		lock (obj)
		{
			try
			{
				lock (GetLibrary())
				{
					HTTPCacheFileInfo value;
					bool flag = GetLibrary().TryGetValue(uri, out value);
					if (flag)
					{
						value.Delete();
					}
					if (flag && removeFromLibrary)
					{
						GetLibrary().Remove(uri);
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

	internal static bool IsCachedEntityExpiresInTheFuture(HTTPRequest request)
	{
		if (!GetIsSupported())
		{
			return false;
		}
		lock (GetLibrary())
		{
			HTTPCacheFileInfo value;
			if (GetLibrary().TryGetValue(request.GetCurrentUri(), out value))
			{
				return value.WillExpireInTheFuture();
			}
		}
		return false;
	}

	internal static void SetHeaders(HTTPRequest request)
	{
		if (!GetIsSupported())
		{
			return;
		}
		lock (GetLibrary())
		{
			HTTPCacheFileInfo value;
			if (GetLibrary().TryGetValue(request.GetCurrentUri(), out value))
			{
				value.SetUpRevalidationHeaders(request);
			}
		}
	}

	internal static Stream GetBody(Uri uri, out int length)
	{
		length = 0;
		if (!GetIsSupported())
		{
			return null;
		}
		lock (GetLibrary())
		{
			HTTPCacheFileInfo value;
			if (GetLibrary().TryGetValue(uri, out value))
			{
				return value.GetBodyStream(out length);
			}
		}
		return null;
	}

	internal static HTTPResponse GetFullResponse(HTTPRequest request)
	{
		if (!GetIsSupported())
		{
			return null;
		}
		lock (GetLibrary())
		{
			HTTPCacheFileInfo value;
			if (GetLibrary().TryGetValue(request.GetCurrentUri(), out value))
			{
				return value.ReadResponseTo(request);
			}
		}
		return null;
	}

	internal static bool IsCacheble(Uri uri, HTTPMethods method, HTTPResponse response)
	{
		if (!GetIsSupported())
		{
			return false;
		}
		if (method != HTTPMethods.Get)
		{
			return false;
		}
		if (response == null)
		{
			return false;
		}
		if (response.GetStatusCode() == 304)
		{
			return false;
		}
		if (response.GetStatusCode() < 200 || response.GetStatusCode() >= 400)
		{
			return false;
		}
		List<string> list = response.GetHeaderValues("cache-control");
		if (list != null && list.Exists((string header) =>
		{
			string text = header.ToLower();
			return text.Contains("no-store") || text.Contains("no-cache");
		}))
		{
			return false;
		}
		List<string> list2 = response.GetHeaderValues("pragma");
		if (list2 != null && list2.Exists((string header) =>
		{
			string text = header.ToLower();
			return text.Contains("no-store") || text.Contains("no-cache");
		}))
		{
			return false;
		}
		List<string> list3 = response.GetHeaderValues("content-range");
		if (list3 != null)
		{
			return false;
		}
		return true;
	}

	internal static HTTPCacheFileInfo Store(Uri uri, HTTPMethods method, HTTPResponse response)
	{
		if (response == null || response.GetData() == null || response.GetData().Length == 0)
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
			if (!GetLibrary().TryGetValue(uri, out value))
			{
				GetLibrary().Add(uri, value = new HTTPCacheFileInfo(uri));
				usedIndexes.Add(value.GetMappedNameIdx(), value);
			}
			try
			{
				value.Store(response);
				return value;
			}
			catch
			{
				DeleteEntity(uri);
				throw;
			}
		}
	}

	internal static Stream PrepareStreamed(Uri uri, HTTPResponse response)
	{
		if (!GetIsSupported())
		{
			return null;
		}
		lock (GetLibrary())
		{
			HTTPCacheFileInfo value;
			if (!GetLibrary().TryGetValue(uri, out value))
			{
				GetLibrary().Add(uri, value = new HTTPCacheFileInfo(uri));
				usedIndexes.Add(value.GetMappedNameIdx(), value);
			}
			try
			{
				return value.GetSaveStream(response);
			}
			catch
			{
				DeleteEntity(uri);
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

	private static void ClearImpl(object state)
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

	public static void BeginMaintainence(HTTPCacheMaintananceParams maintananceParams)
	{
		if (maintananceParams == null)
		{
			throw new ArgumentNullException("maintananceParams == null");
		}
		if (!GetIsSupported() || inMaintainenceThread)
		{
			return;
		}
		inMaintainenceThread = true;
		SetupCacheFolder();
		new Thread((object state) =>
		{
			try
			{
				lock (GetLibrary())
				{
					DateTime dateTime = DateTime.UtcNow - maintananceParams.GetDeleteOlder();
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
					if (num > maintananceParams.GetMaxCacheSize())
					{
						List<HTTPCacheFileInfo> list2 = new List<HTTPCacheFileInfo>(library.Count);
						foreach (KeyValuePair<Uri, HTTPCacheFileInfo> item2 in library)
						{
							list2.Add(item2.Value);
						}
						list2.Sort();
						int num2 = 0;
						while (num >= maintananceParams.GetMaxCacheSize() && num2 < list2.Count)
						{
							try
							{
								HTTPCacheFileInfo fileInfo = list2[num2];
								ulong num3 = (ulong)fileInfo.GetBodyLength();
								DeleteEntity(fileInfo.GetUri());
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
							HTTPCacheFileInfo fileInfo = new HTTPCacheFileInfo(uri, binaryReader, num);
							if (fileInfo.IsExists())
							{
								library.Add(uri, fileInfo);
								if (num > 1)
								{
									usedIndexes.Add(fileInfo.GetMappedNameIdx(), fileInfo);
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

	internal static void SetBodyLength(Uri uri, int length)
	{
		if (!GetIsSupported())
		{
			return;
		}
		lock (GetLibrary())
		{
			HTTPCacheFileInfo value;
			if (GetLibrary().TryGetValue(uri, out value))
			{
				value.set_BodyLength(length);
				return;
			}
			GetLibrary().Add(uri, value = new HTTPCacheFileInfo(uri, DateTime.UtcNow, length));
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
