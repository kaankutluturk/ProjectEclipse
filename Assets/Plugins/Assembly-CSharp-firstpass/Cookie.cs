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

	public Cookie(string name, string value, string path, string OKDDNOHODMN)
		: this()
	{
		set_Name(name);
		set_Value(value);
		SetPath(path);
		SetDomain(OKDDNOHODMN);
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

	public static Cookie Parse(string HHAAFADDOJB, Uri BABJLNLFPPI)
	{
		Cookie eKAOIOLAGFH = new Cookie();
		try
		{
			List<KeyValuePair> list = ParseCookieValue(HHAAFADDOJB);
			foreach (KeyValuePair item in list)
			{
				switch (item.GetKey().ToLowerInvariant())
				{
				case "path":
				{
					object bAINMLLIKOL;
					if (string.IsNullOrEmpty(item.GetValue()) || !item.GetValue().StartsWith("/"))
					{
						bAINMLLIKOL = "/";
					}
					else
					{
						string text = item.GetValue();
						eKAOIOLAGFH.SetPath(text);
						bAINMLLIKOL = text;
					}
					eKAOIOLAGFH.SetPath((string)bAINMLLIKOL);
					break;
				}
				case "domain":
					if (string.IsNullOrEmpty(item.GetValue()))
					{
						return null;
					}
					eKAOIOLAGFH.SetDomain((!item.GetValue().StartsWith(".")) ? item.GetValue() : item.GetValue().Substring(1));
					break;
				case "expires":
					eKAOIOLAGFH.SetExpires(item.GetValue().ToDateTime(DateTime.FromBinary(0L)));
					eKAOIOLAGFH.SetIsSession(false);
					break;
				case "max-age":
					eKAOIOLAGFH.set_MaxAge(item.GetValue().ToInt64(-1L));
					eKAOIOLAGFH.SetIsSession(false);
					break;
				case "secure":
					eKAOIOLAGFH.SetIsSecure(true);
					break;
				case "httponly":
					eKAOIOLAGFH.SetIsHttpOnly(true);
					break;
				default:
					eKAOIOLAGFH.set_Name(item.GetKey());
					eKAOIOLAGFH.set_Value(item.GetValue());
					break;
				}
			}
			if (HTTPManager.GetEnablePrivateBrowsing())
			{
				eKAOIOLAGFH.SetIsSession(true);
			}
			if (string.IsNullOrEmpty(eKAOIOLAGFH.GetDomain()))
			{
				eKAOIOLAGFH.SetDomain(BABJLNLFPPI.Host);
			}
			if (string.IsNullOrEmpty(eKAOIOLAGFH.GetPath()))
			{
				eKAOIOLAGFH.SetPath(BABJLNLFPPI.AbsolutePath);
			}
			DateTime utcNow = DateTime.UtcNow;
			eKAOIOLAGFH.SetLastAccess(utcNow);
			eKAOIOLAGFH.SetDate(utcNow);
		}
		catch
		{
		}
		return eKAOIOLAGFH;
	}

	internal void SaveTo(BinaryWriter ABJIEFMMIEK)
	{
		ABJIEFMMIEK.Write(1);
		ABJIEFMMIEK.Write(get_Name() ?? string.Empty);
		ABJIEFMMIEK.Write(GetValue() ?? string.Empty);
		ABJIEFMMIEK.Write(GetDate().ToBinary());
		ABJIEFMMIEK.Write(GetLastAccess().ToBinary());
		ABJIEFMMIEK.Write(GetExpires().ToBinary());
		ABJIEFMMIEK.Write(GetMaxAge());
		ABJIEFMMIEK.Write(GetIsSession());
		ABJIEFMMIEK.Write(GetDomain() ?? string.Empty);
		ABJIEFMMIEK.Write(GetPath() ?? string.Empty);
		ABJIEFMMIEK.Write(GetIsSecure());
		ABJIEFMMIEK.Write(GetIsHttpOnly());
	}

	internal void LoadFrom(BinaryReader ABJIEFMMIEK)
	{
		ABJIEFMMIEK.ReadInt32();
		set_Name(ABJIEFMMIEK.ReadString());
		set_Value(ABJIEFMMIEK.ReadString());
		SetDate(DateTime.FromBinary(ABJIEFMMIEK.ReadInt64()));
		SetLastAccess(DateTime.FromBinary(ABJIEFMMIEK.ReadInt64()));
		SetExpires(DateTime.FromBinary(ABJIEFMMIEK.ReadInt64()));
		set_MaxAge(ABJIEFMMIEK.ReadInt64());
		SetIsSession(ABJIEFMMIEK.ReadBoolean());
		SetDomain(ABJIEFMMIEK.ReadString());
		SetPath(ABJIEFMMIEK.ReadString());
		SetIsSecure(ABJIEFMMIEK.ReadBoolean());
		SetIsHttpOnly(ABJIEFMMIEK.ReadBoolean());
	}

	public override string ToString()
	{
		return get_Name() + "=" + GetValue();
	}

	public override bool Equals(object AOMLCBHAJJH)
	{
		if (AOMLCBHAJJH == null)
		{
			return false;
		}
		return Equals(AOMLCBHAJJH as Cookie);
	}

	public bool Equals(Cookie FJKPPODBPJF)
	{
		if (FJKPPODBPJF == null)
		{
			return false;
		}
		if (object.ReferenceEquals(this, FJKPPODBPJF))
		{
			return true;
		}
		return get_Name().Equals(FJKPPODBPJF.get_Name(), StringComparison.Ordinal) && ((GetDomain() == null && FJKPPODBPJF.GetDomain() == null) || GetDomain().Equals(FJKPPODBPJF.GetDomain(), StringComparison.Ordinal)) && ((GetPath() == null && FJKPPODBPJF.GetPath() == null) || GetPath().Equals(FJKPPODBPJF.GetPath(), StringComparison.Ordinal));
	}

	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}

	private static string ReadValue(string IGGFGLLIGCG, ref int LCCLEFMKLPB)
	{
		string empty = string.Empty;
		if (IGGFGLLIGCG == null)
		{
			return empty;
		}
		return IGGFGLLIGCG.Read(ref LCCLEFMKLPB, ';');
	}

	private static List<KeyValuePair> ParseCookieValue(string IGGFGLLIGCG)
	{
		List<KeyValuePair> list = new List<KeyValuePair>();
		if (IGGFGLLIGCG == null)
		{
			return list;
		}
		int LCCLEFMKLPB = 0;
		while (LCCLEFMKLPB < IGGFGLLIGCG.Length)
		{
			string kGBGENDIMBC = IGGFGLLIGCG.Read(ref LCCLEFMKLPB, (char KDFCGMMKAME) => KDFCGMMKAME != '=' && KDFCGMMKAME != ';').Trim();
			KeyValuePair gGCJLGPPHKP = new KeyValuePair(kGBGENDIMBC);
			if (LCCLEFMKLPB < IGGFGLLIGCG.Length && IGGFGLLIGCG[LCCLEFMKLPB - 1] == '=')
			{
				gGCJLGPPHKP.set_Value(ReadValue(IGGFGLLIGCG, ref LCCLEFMKLPB));
			}
			list.Add(gGCJLGPPHKP);
		}
		return list;
	}

	public int CompareTo(Cookie NOLFMPDGCOC)
	{
		return GetLastAccess().CompareTo(NOLFMPDGCOC.GetLastAccess());
	}
}
