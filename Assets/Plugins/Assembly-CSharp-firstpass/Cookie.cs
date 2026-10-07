using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public sealed class Cookie : IComparable<Cookie>, IEquatable<Cookie>
{
	private const int Version = 1;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _name;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _value;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private DateTime _date;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private DateTime _lastAccess;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private DateTime _expires;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private long _maxAge;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool _isSession;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _domain;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _path;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool _isSecure;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool _isHttpOnly;

	public string CookieName
	{
		get
		{
			return get_Name();
		}
		private set
		{
			set_Name(value);
		}
	}

	public DateTime Date
	{
		get
		{
			return GetDate();
		}
		internal set
		{
			SetDate(value);
		}
	}

	public DateTime LastAccess
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

	public DateTime Expires
	{
		get
		{
			return GetExpires();
		}
		private set
		{
			SetExpires(value);
		}
	}

	public long MaxAgeSeconds
	{
		get
		{
			return GetMaxAge();
		}
		private set
		{
			set_MaxAge(value);
		}
	}

	public bool IsSession
	{
		get
		{
			return GetIsSession();
		}
		private set
		{
			SetIsSession(value);
		}
	}

	public string Domain
	{
		get
		{
			return GetDomain();
		}
		private set
		{
			SetDomain(value);
		}
	}

	public string Path
	{
		get
		{
			return GetPath();
		}
		private set
		{
			SetPath(value);
		}
	}

	public bool IsSecure
	{
		get
		{
			return GetIsSecure();
		}
		private set
		{
			SetIsSecure(value);
		}
	}

	public bool IsHttpOnly
	{
		get
		{
			return GetIsHttpOnly();
		}
		private set
		{
			SetIsHttpOnly(value);
		}
	}

	public Cookie(string name, string value)
		: this(name, value, string.Empty, string.Empty)
	{
	}

	public Cookie(string name, string value, string path)
		: this(name, value, path, string.Empty)
	{
	}

	public Cookie(string name, string value, string path, string domain)
		: this()
	{
		set_Name(name);
		set_Value(value);
		SetPath(path);
		SetDomain(domain);
	}

	internal Cookie()
	{
		SetIsSession(true);
		set_MaxAge(-1L);
		SetLastAccess(DateTime.UtcNow);
	}

	public string get_Name()
	{
		return _name;
	}

	private void set_Name(string value)
	{
		_name = value;
	}

	public string GetValue()
	{
		return _value;
	}

	private void set_Value(string value)
	{
		_value = value;
	}

	public DateTime GetDate()
	{
		return _date;
	}

	internal void SetDate(DateTime value)
	{
		_date = value;
	}

	public DateTime GetLastAccess()
	{
		return _lastAccess;
	}

	public void SetLastAccess(DateTime value)
	{
		_lastAccess = value;
	}

	public DateTime GetExpires()
	{
		return _expires;
	}

	private void SetExpires(DateTime value)
	{
		_expires = value;
	}

	public long GetMaxAge()
	{
		return _maxAge;
	}

	private void set_MaxAge(long value)
	{
		_maxAge = value;
	}

	public bool GetIsSession()
	{
		return _isSession;
	}

	private void SetIsSession(bool value)
	{
		_isSession = value;
	}

	public string GetDomain()
	{
		return _domain;
	}

	private void SetDomain(string value)
	{
		_domain = value;
	}

	public string GetPath()
	{
		return _path;
	}

	private void SetPath(string value)
	{
		_path = value;
	}

	public bool GetIsSecure()
	{
		return _isSecure;
	}

	private void SetIsSecure(bool value)
	{
		_isSecure = value;
	}

	public bool GetIsHttpOnly()
	{
		return _isHttpOnly;
	}

	private void SetIsHttpOnly(bool value)
	{
		_isHttpOnly = value;
	}

	public bool WillExpireInTheFuture()
	{
		if (GetIsSession())
		{
			return true;
		}
		return (GetMaxAge() == -1) ? (GetExpires() > DateTime.UtcNow) : (Math.Max(0L, (long)(DateTime.UtcNow - GetDate()).TotalSeconds) < GetMaxAge());
	}

	public uint GuessSize()
	{
		return (uint)(((get_Name() != null) ? (get_Name().Length * 2) : 0) + ((GetValue() != null) ? (GetValue().Length * 2) : 0) + ((GetDomain() != null) ? (GetDomain().Length * 2) : 0) + ((GetPath() != null) ? (GetPath().Length * 2) : 0) + 32 + 3);
	}

	public static Cookie Parse(string header, Uri uri)
	{
		Cookie cookie = new Cookie();
		try
		{
			List<KeyValuePair> list = ParseCookieValue(header);
			foreach (KeyValuePair item in list)
			{
				switch (item.GetKey().ToLowerInvariant())
				{
				case "path":
				{
					object path;
					if (string.IsNullOrEmpty(item.GetValue()) || !item.GetValue().StartsWith("/"))
					{
						path = "/";
					}
					else
					{
						string text = item.GetValue();
						cookie.SetPath(text);
						path = text;
					}
					cookie.SetPath((string)path);
					break;
				}
				case "domain":
					if (string.IsNullOrEmpty(item.GetValue()))
					{
						return null;
					}
					cookie.SetDomain((!item.GetValue().StartsWith(".")) ? item.GetValue() : item.GetValue().Substring(1));
					break;
				case "expires":
					cookie.SetExpires(item.GetValue().ToDateTime(DateTime.FromBinary(0L)));
					cookie.SetIsSession(false);
					break;
				case "max-age":
					cookie.set_MaxAge(item.GetValue().ToInt64(-1L));
					cookie.SetIsSession(false);
					break;
				case "secure":
					cookie.SetIsSecure(true);
					break;
				case "httponly":
					cookie.SetIsHttpOnly(true);
					break;
				default:
					cookie.set_Name(item.GetKey());
					cookie.set_Value(item.GetValue());
					break;
				}
			}
			if (HTTPManager.GetEnablePrivateBrowsing())
			{
				cookie.SetIsSession(true);
			}
			if (string.IsNullOrEmpty(cookie.GetDomain()))
			{
				cookie.SetDomain(uri.Host);
			}
			if (string.IsNullOrEmpty(cookie.GetPath()))
			{
				cookie.SetPath(uri.AbsolutePath);
			}
			DateTime utcNow = DateTime.UtcNow;
			cookie.SetLastAccess(utcNow);
			cookie.SetDate(utcNow);
		}
		catch
		{
		}
		return cookie;
	}

	internal void SaveTo(BinaryWriter writer)
	{
		writer.Write(1);
		writer.Write(get_Name() ?? string.Empty);
		writer.Write(GetValue() ?? string.Empty);
		writer.Write(GetDate().ToBinary());
		writer.Write(GetLastAccess().ToBinary());
		writer.Write(GetExpires().ToBinary());
		writer.Write(GetMaxAge());
		writer.Write(GetIsSession());
		writer.Write(GetDomain() ?? string.Empty);
		writer.Write(GetPath() ?? string.Empty);
		writer.Write(GetIsSecure());
		writer.Write(GetIsHttpOnly());
	}

	internal void LoadFrom(BinaryReader reader)
	{
		reader.ReadInt32();
		set_Name(reader.ReadString());
		set_Value(reader.ReadString());
		SetDate(DateTime.FromBinary(reader.ReadInt64()));
		SetLastAccess(DateTime.FromBinary(reader.ReadInt64()));
		SetExpires(DateTime.FromBinary(reader.ReadInt64()));
		set_MaxAge(reader.ReadInt64());
		SetIsSession(reader.ReadBoolean());
		SetDomain(reader.ReadString());
		SetPath(reader.ReadString());
		SetIsSecure(reader.ReadBoolean());
		SetIsHttpOnly(reader.ReadBoolean());
	}

	public override string ToString()
	{
		return get_Name() + "=" + GetValue();
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return Equals(obj as Cookie);
	}

	public bool Equals(Cookie other)
	{
		if (other == null)
		{
			return false;
		}
		if (object.ReferenceEquals(this, other))
		{
			return true;
		}
		return get_Name().Equals(other.get_Name(), StringComparison.Ordinal) && ((GetDomain() == null && other.GetDomain() == null) || GetDomain().Equals(other.GetDomain(), StringComparison.Ordinal)) && ((GetPath() == null && other.GetPath() == null) || GetPath().Equals(other.GetPath(), StringComparison.Ordinal));
	}

	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}

	private static string ReadValue(string text, ref int cursor)
	{
		string empty = string.Empty;
		if (text == null)
		{
			return empty;
		}
		return text.Read(ref cursor, ';');
	}

	private static List<KeyValuePair> ParseCookieValue(string text)
	{
		List<KeyValuePair> list = new List<KeyValuePair>();
		if (text == null)
		{
			return list;
		}
		int cursor = 0;
		while (cursor < text.Length)
		{
			string name = text.Read(ref cursor, (char ch) => ch != '=' && ch != ';').Trim();
			KeyValuePair pair = new KeyValuePair(name);
			if (cursor < text.Length && text[cursor - 1] == '=')
			{
				pair.set_Value(ReadValue(text, ref cursor));
			}
			list.Add(pair);
		}
		return list;
	}

	public int CompareTo(Cookie other)
	{
		return GetLastAccess().CompareTo(other.GetLastAccess());
	}
}
