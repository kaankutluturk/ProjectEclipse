using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

internal class HTTPCacheFileInfo : IComparable<HTTPCacheFileInfo>
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Uri uri;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private DateTime lastAccess;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int bodyLength;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string eTag;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string lastModified;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private DateTime expires;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private long age;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private long maxAge;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private DateTime date;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool mustRevalidate;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private DateTime received;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string constructedPath;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ulong mappedNameIdx;

	internal Uri CachedUri
	{
		get
		{
			return GetUri();
		}
		set
		{
			set_Uri(value);
		}
	}

	internal DateTime LastAccess
	{
		get
		{
			return GetLastAccess();
		}
		set
		{
			SetLastAccess(value);
		}
	}

	internal int CachedBodyLength
	{
		get
		{
			return GetBodyLength();
		}
		set
		{
			set_BodyLength(value);
		}
	}

	private string ETag
	{
		get
		{
			return GetETag();
		}
		set
		{
			SetETag(value);
		}
	}

	private string LastModified
	{
		get
		{
			return GetLastModified();
		}
		set
		{
			SetLastModified(value);
		}
	}

	private DateTime Expires
	{
		get
		{
			return GetExpires();
		}
		set
		{
			SetExpires(value);
		}
	}

	private long CacheAge
	{
		get
		{
			return GetAge();
		}
		set
		{
			SetAge(value);
		}
	}

	private long MaxAgeSeconds
	{
		get
		{
			return GetMaxAge();
		}
		set
		{
			set_MaxAge(value);
		}
	}

	private DateTime Date
	{
		get
		{
			return GetDate();
		}
		set
		{
			SetDate(value);
		}
	}

	private bool RequiresRevalidation
	{
		get
		{
			return GetMustRevalidate();
		}
		set
		{
			set_MustRevalidate(value);
		}
	}

	private DateTime Received
	{
		get
		{
			return GetReceived();
		}
		set
		{
			SetReceived(value);
		}
	}

	private string ConstructedPath
	{
		get
		{
			return GetConstructedPath();
		}
		set
		{
			SetConstructedPath(value);
		}
	}

	internal ulong MappedNameIndex
	{
		get
		{
			return GetMappedNameIdx();
		}
		set
		{
			set_MappedNameIDX(value);
		}
	}

	internal HTTPCacheFileInfo(Uri KJHNCLAJMLO)
		: this(KJHNCLAJMLO, DateTime.UtcNow, -1)
	{
	}

	internal HTTPCacheFileInfo(Uri KJHNCLAJMLO, DateTime HMHMICPJKOF, int DEFBMELCOHO)
	{
		set_Uri(KJHNCLAJMLO);
		SetLastAccess(HMHMICPJKOF);
		set_BodyLength(DEFBMELCOHO);
		set_MaxAge(-1L);
		set_MappedNameIDX(HTTPCacheService.GetNameIdx());
	}

	internal HTTPCacheFileInfo(Uri KJHNCLAJMLO, BinaryReader reader, int version)
	{
		set_Uri(KJHNCLAJMLO);
		SetLastAccess(DateTime.FromBinary(reader.ReadInt64()));
		set_BodyLength(reader.ReadInt32());
		switch (version)
		{
		default:
			return;
		case 2:
			set_MappedNameIDX(reader.ReadUInt64());
			break;
		case 1:
			break;
		}
		SetETag(reader.ReadString());
		SetLastModified(reader.ReadString());
		SetExpires(DateTime.FromBinary(reader.ReadInt64()));
		SetAge(reader.ReadInt64());
		set_MaxAge(reader.ReadInt64());
		SetDate(DateTime.FromBinary(reader.ReadInt64()));
		set_MustRevalidate(reader.ReadBoolean());
		SetReceived(DateTime.FromBinary(reader.ReadInt64()));
	}

	internal Uri GetUri()
	{
		return uri;
	}

	internal void set_Uri(Uri value)
	{
		uri = value;
	}

	internal DateTime GetLastAccess()
	{
		return lastAccess;
	}

	internal void SetLastAccess(DateTime value)
	{
		lastAccess = value;
	}

	internal int GetBodyLength()
	{
		return bodyLength;
	}

	internal void set_BodyLength(int value)
	{
		bodyLength = value;
	}

	private string GetETag()
	{
		return eTag;
	}

	private void SetETag(string value)
	{
		eTag = value;
	}

	private string GetLastModified()
	{
		return lastModified;
	}

	private void SetLastModified(string value)
	{
		lastModified = value;
	}

	private DateTime GetExpires()
	{
		return expires;
	}

	private void SetExpires(DateTime value)
	{
		expires = value;
	}

	private long GetAge()
	{
		return age;
	}

	private void SetAge(long value)
	{
		age = value;
	}

	private long GetMaxAge()
	{
		return maxAge;
	}

	private void set_MaxAge(long value)
	{
		maxAge = value;
	}

	private DateTime GetDate()
	{
		return date;
	}

	private void SetDate(DateTime value)
	{
		date = value;
	}

	private bool GetMustRevalidate()
	{
		return mustRevalidate;
	}

	private void set_MustRevalidate(bool value)
	{
		mustRevalidate = value;
	}

	private DateTime GetReceived()
	{
		return received;
	}

	private void SetReceived(DateTime value)
	{
		received = value;
	}

	private string GetConstructedPath()
	{
		return constructedPath;
	}

	private void SetConstructedPath(string value)
	{
		constructedPath = value;
	}

	internal ulong GetMappedNameIdx()
	{
		return mappedNameIdx;
	}

	internal void set_MappedNameIDX(ulong value)
	{
		mappedNameIdx = value;
	}

	internal void SaveTo(BinaryWriter writer)
	{
		writer.Write(GetLastAccess().ToBinary());
		writer.Write(GetBodyLength());
		writer.Write(GetMappedNameIdx());
		writer.Write(GetETag());
		writer.Write(GetLastModified());
		writer.Write(GetExpires().ToBinary());
		writer.Write(GetAge());
		writer.Write(GetMaxAge());
		writer.Write(GetDate().ToBinary());
		writer.Write(GetMustRevalidate());
		writer.Write(GetReceived().ToBinary());
	}

	private string GetPath()
	{
		if (GetConstructedPath() != null)
		{
			return GetConstructedPath();
		}
		string text = Path.Combine(HTTPCacheService.GetCacheFolder(), GetMappedNameIdx().ToString("X"));
		SetConstructedPath(text);
		return text;
	}

	internal bool IsExists()
	{
		if (!HTTPCacheService.GetIsSupported())
		{
			return false;
		}
		return File.Exists(GetPath());
	}

	internal void Delete()
	{
		if (!HTTPCacheService.GetIsSupported())
		{
			return;
		}
		string path = GetPath();
		try
		{
			File.Delete(path);
		}
		catch
		{
		}
		finally
		{
			Reset();
		}
	}

	private void Reset()
	{
		set_MappedNameIDX(0uL);
		set_BodyLength(-1);
		SetETag(string.Empty);
		SetExpires(DateTime.FromBinary(0L));
		SetLastModified(string.Empty);
		SetAge(0L);
		set_MaxAge(-1L);
		SetDate(DateTime.FromBinary(0L));
		set_MustRevalidate(false);
		SetReceived(DateTime.FromBinary(0L));
	}

	private void SetUpCachingValues(HTTPResponse GIHDDAKBMHE)
	{
		SetETag(GIHDDAKBMHE.GetFirstHeaderValue("ETag").ToStrOrEmpty());
		SetExpires(GIHDDAKBMHE.GetFirstHeaderValue("Expires").ToDateTime(DateTime.FromBinary(0L)));
		SetLastModified(GIHDDAKBMHE.GetFirstHeaderValue("Last-Modified").ToStrOrEmpty());
		SetAge(GIHDDAKBMHE.GetFirstHeaderValue("Age").ToInt64(0L));
		SetDate(GIHDDAKBMHE.GetFirstHeaderValue("Date").ToDateTime(DateTime.FromBinary(0L)));
		string text = GIHDDAKBMHE.GetFirstHeaderValue("cache-control");
		if (!string.IsNullOrEmpty(text))
		{
			string[] array = text.FindOption("Max-Age");
			double result;
			if (array != null && double.TryParse(array[1], out result))
			{
				set_MaxAge((int)result);
			}
			set_MustRevalidate(text.ToLower().Contains("must-revalidate"));
		}
		SetReceived(DateTime.UtcNow);
	}

	internal bool WillExpireInTheFuture()
	{
		if (!IsExists())
		{
			return false;
		}
		if (GetMustRevalidate())
		{
			return false;
		}
		if (GetMaxAge() != -1)
		{
			long val = Math.Max(0L, (long)(GetReceived() - GetDate()).TotalSeconds);
			long num = Math.Max(val, GetAge());
			long num2 = (long)(DateTime.UtcNow - GetDate()).TotalSeconds;
			long num3 = num + num2;
			return num3 < GetMaxAge();
		}
		return GetExpires() > DateTime.UtcNow;
	}

	internal void SetUpRevalidationHeaders(HTTPRequest ONOCIELLAPL)
	{
		if (IsExists())
		{
			if (!string.IsNullOrEmpty(GetETag()))
			{
				ONOCIELLAPL.AddHeader("If-None-Match", GetETag());
			}
			if (!string.IsNullOrEmpty(GetLastModified()))
			{
				ONOCIELLAPL.AddHeader("If-Modified-Since", GetLastModified());
			}
		}
	}

	internal Stream GetBodyStream(out int BDBOAEGELMC)
	{
		if (!IsExists())
		{
			BDBOAEGELMC = 0;
			return null;
		}
		BDBOAEGELMC = GetBodyLength();
		SetLastAccess(DateTime.UtcNow);
		FileStream fileStream = new FileStream(GetPath(), FileMode.Open);
		fileStream.Seek(-BDBOAEGELMC, SeekOrigin.End);
		return fileStream;
	}

	internal HTTPResponse ReadResponseTo(HTTPRequest ONOCIELLAPL)
	{
		if (!IsExists())
		{
			return null;
		}
		SetLastAccess(DateTime.UtcNow);
		using (FileStream aBJIEFMMIEK = new FileStream(GetPath(), FileMode.Open))
		{
			HTTPResponse iLGKJGGJHAJ = new HTTPResponse(ONOCIELLAPL, aBJIEFMMIEK, ONOCIELLAPL.GetUseStreaming(), true);
			iLGKJGGJHAJ.Receive(GetBodyLength());
			return iLGKJGGJHAJ;
		}
	}

	internal void Store(HTTPResponse GIHDDAKBMHE)
	{
		if (!HTTPCacheService.GetIsSupported())
		{
			return;
		}
		string text = GetPath();
		if (text.Length > HTTPManager.GetMaxPathLength())
		{
			return;
		}
		if (File.Exists(text))
		{
			Delete();
		}
		using (FileStream fileStream = new FileStream(text, FileMode.Create))
		{
			fileStream.WriteLine("HTTP/1.1 {0} {1}", GIHDDAKBMHE.GetStatusCode(), GIHDDAKBMHE.GetMessage());
			foreach (KeyValuePair<string, List<string>> item in GIHDDAKBMHE.GetHeaders())
			{
				for (int i = 0; i < item.Value.Count; i++)
				{
					fileStream.WriteLine("{0}: {1}", item.Key, item.Value[i]);
				}
			}
			fileStream.WriteLine();
			fileStream.Write(GIHDDAKBMHE.GetData(), 0, GIHDDAKBMHE.GetData().Length);
		}
		set_BodyLength(GIHDDAKBMHE.GetData().Length);
		SetLastAccess(DateTime.UtcNow);
		SetUpCachingValues(GIHDDAKBMHE);
	}

	internal Stream GetSaveStream(HTTPResponse GIHDDAKBMHE)
	{
		if (!HTTPCacheService.GetIsSupported())
		{
			return null;
		}
		SetLastAccess(DateTime.UtcNow);
		string text = GetPath();
		if (File.Exists(text))
		{
			Delete();
		}
		if (text.Length > HTTPManager.GetMaxPathLength())
		{
			return null;
		}
		using (FileStream mEHMICNAPMK = new FileStream(text, FileMode.Create))
		{
			mEHMICNAPMK.WriteLine("HTTP/1.1 {0} {1}", GIHDDAKBMHE.GetStatusCode(), GIHDDAKBMHE.GetMessage());
			foreach (KeyValuePair<string, List<string>> item in GIHDDAKBMHE.GetHeaders())
			{
				for (int i = 0; i < item.Value.Count; i++)
				{
					mEHMICNAPMK.WriteLine("{0}: {1}", item.Key, item.Value[i]);
				}
			}
			mEHMICNAPMK.WriteLine();
		}
		if (GIHDDAKBMHE.GetIsFromCache() && !GIHDDAKBMHE.GetHeaders().ContainsKey("content-length"))
		{
			GIHDDAKBMHE.GetHeaders().Add("content-length", new List<string> { GetBodyLength().ToString() });
		}
		SetUpCachingValues(GIHDDAKBMHE);
		return new FileStream(GetPath(), FileMode.Append);
	}

	public int CompareTo(HTTPCacheFileInfo NOLFMPDGCOC)
	{
		return GetLastAccess().CompareTo(NOLFMPDGCOC.GetLastAccess());
	}
}
