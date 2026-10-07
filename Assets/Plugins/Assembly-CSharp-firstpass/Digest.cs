using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

internal sealed class Digest
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Uri uri;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private AuthenticationTypes type;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string realm;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool stale;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string nonce;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string opaque;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string algorithm;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<string> protectedUris;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string qualityOfProtections;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int nonceCount;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string ha1Sess;

	public Uri DigestUri
	{
		get
		{
			return GetUri();
		}
		private set
		{
			set_Uri(value);
		}
	}

	public string Realm
	{
		get
		{
			return GetRealm();
		}
		private set
		{
			SetRealm(value);
		}
	}

	public bool IsNonceStale
	{
		get
		{
			return GetStale();
		}
		private set
		{
			set_Stale(value);
		}
	}

	private string Nonce
	{
		get
		{
			return GetNonce();
		}
		set
		{
			SetNonce(value);
		}
	}

	private string Opaque
	{
		get
		{
			return GetOpaque();
		}
		set
		{
			SetOpaque(value);
		}
	}

	private string Algorithm
	{
		get
		{
			return GetAlgorithm();
		}
		set
		{
			SetAlgorithm(value);
		}
	}

	public List<string> ProtectedUriList
	{
		get
		{
			return GetProtectedUris();
		}
		private set
		{
			set_ProtectedUris(value);
		}
	}

	private string QualityOfProtections
	{
		get
		{
			return GetQualityOfProtections();
		}
		set
		{
			SetQualityOfProtections(value);
		}
	}

	private int NonceCounter
	{
		get
		{
			return GetNonceCount();
		}
		set
		{
			set_NonceCount(value);
		}
	}

	private string HA1Sess
	{
		get
		{
			return GetHA1Sess();
		}
		set
		{
			SetHA1Sess(value);
		}
	}

	internal Digest(Uri digestUri)
	{
		set_Uri(digestUri);
		SetAlgorithm("md5");
	}

	public Uri GetUri()
	{
		return uri;
	}

	private void set_Uri(Uri value)
	{
		uri = value;
	}

	public AuthenticationTypes get_Type()
	{
		return type;
	}

	private void set_Type(AuthenticationTypes value)
	{
		type = value;
	}

	public string GetRealm()
	{
		return realm;
	}

	private void SetRealm(string value)
	{
		realm = value;
	}

	public bool GetStale()
	{
		return stale;
	}

	private void set_Stale(bool value)
	{
		stale = value;
	}

	private string GetNonce()
	{
		return nonce;
	}

	private void SetNonce(string value)
	{
		nonce = value;
	}

	private string GetOpaque()
	{
		return opaque;
	}

	private void SetOpaque(string value)
	{
		opaque = value;
	}

	private string GetAlgorithm()
	{
		return algorithm;
	}

	private void SetAlgorithm(string value)
	{
		algorithm = value;
	}

	public List<string> GetProtectedUris()
	{
		return protectedUris;
	}

	private void set_ProtectedUris(List<string> value)
	{
		protectedUris = value;
	}

	private string GetQualityOfProtections()
	{
		return qualityOfProtections;
	}

	private void SetQualityOfProtections(string value)
	{
		qualityOfProtections = value;
	}

	private int GetNonceCount()
	{
		return nonceCount;
	}

	private void set_NonceCount(int value)
	{
		nonceCount = value;
	}

	private string GetHA1Sess()
	{
		return ha1Sess;
	}

	private void SetHA1Sess(string value)
	{
		ha1Sess = value;
	}

	public void ParseChallange(string challenge)
	{
		set_Type(AuthenticationTypes.Unknown);
		set_Stale(false);
		SetOpaque(null);
		SetHA1Sess(null);
		set_NonceCount(0);
		SetQualityOfProtections(null);
		if (GetProtectedUris() != null)
		{
			GetProtectedUris().Clear();
		}
		WWWAuthenticateHeaderParser parser = new WWWAuthenticateHeaderParser(challenge);
		foreach (KeyValuePair item2 in parser.GetValues())
		{
			switch (item2.GetKey())
			{
			case "basic":
				set_Type(AuthenticationTypes.Basic);
				break;
			case "digest":
				set_Type(AuthenticationTypes.Digest);
				break;
			case "realm":
				SetRealm(item2.GetValue());
				break;
			case "domain":
				if (!string.IsNullOrEmpty(item2.GetValue()) && item2.GetValue().Length != 0)
				{
					if (GetProtectedUris() == null)
					{
						set_ProtectedUris(new List<string>());
					}
					int LCCLEFMKLPB = 0;
					string item = item2.GetValue().Read(ref LCCLEFMKLPB, ' ');
					do
					{
						GetProtectedUris().Add(item);
						item = item2.GetValue().Read(ref LCCLEFMKLPB, ' ');
					}
					while (LCCLEFMKLPB < item2.GetValue().Length);
				}
				break;
			case "nonce":
				SetNonce(item2.GetValue());
				break;
			case "qop":
				SetQualityOfProtections(item2.GetValue());
				break;
			case "stale":
				set_Stale(bool.Parse(item2.GetValue()));
				break;
			case "opaque":
				SetOpaque(item2.GetValue());
				break;
			case "algorithm":
				SetAlgorithm(item2.GetValue());
				break;
			}
		}
	}

	public string GenerateResponseHeader(HTTPRequest request, Credentials credentials)
	{
		try
		{
			switch (get_Type())
			{
			case AuthenticationTypes.Basic:
				return "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", credentials.GetUserName(), credentials.GetPassword())));
			case AuthenticationTypes.Digest:
			{
				set_NonceCount(GetNonceCount() + 1);
				string empty = string.Empty;
				string text = new Random(request.GetHashCode()).Next(int.MinValue, int.MaxValue).ToString("X8");
				string text2 = GetNonceCount().ToString("X8");
				switch (GetAlgorithm().TrimAndLower())
				{
				case "md5":
					empty = string.Format("{0}:{1}:{2}", credentials.GetUserName(), GetRealm(), credentials.GetPassword()).CalculateMD5Hash();
					break;
				case "md5-sess":
					if (string.IsNullOrEmpty(GetHA1Sess()))
					{
						SetHA1Sess(string.Format("{0}:{1}:{2}:{3}:{4}", credentials.GetUserName(), GetRealm(), credentials.GetPassword(), GetNonce(), text2).CalculateMD5Hash());
					}
					empty = GetHA1Sess();
					break;
				default:
					return string.Empty;
				}
				string empty2 = string.Empty;
				string text3 = ((GetQualityOfProtections() == null) ? null : GetQualityOfProtections().TrimAndLower());
				if (text3 == null)
				{
					string arg = (request.GetMethodType().ToString().ToUpper() + ":" + request.GetCurrentUri().PathAndQuery).CalculateMD5Hash();
					empty2 = string.Format("{0}:{1}:{2}", empty, GetNonce(), arg).CalculateMD5Hash();
				}
				else if (text3.Contains("auth-int"))
				{
					text3 = "auth-int";
					byte[] array = request.GetEntityBody();
					if (array == null)
					{
						array = string.Empty.GetASCIIBytes();
					}
					string text4 = string.Format("{0}:{1}:{2}", request.GetMethodType().ToString().ToUpper(), request.GetCurrentUri().PathAndQuery, array.CalculateMD5Hash()).CalculateMD5Hash();
					empty2 = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", empty, GetNonce(), text2, text, text3, text4).CalculateMD5Hash();
				}
				else
				{
					if (!text3.Contains("auth"))
					{
						return string.Empty;
					}
					text3 = "auth";
					string text5 = (request.GetMethodType().ToString().ToUpper() + ":" + request.GetCurrentUri().PathAndQuery).CalculateMD5Hash();
					empty2 = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", empty, GetNonce(), text2, text, text3, text5).CalculateMD5Hash();
				}
				string text6 = string.Format("Digest username=\"{0}\", realm=\"{1}\", nonce=\"{2}\", uri=\"{3}\", cnonce=\"{4}\", response=\"{5}\"", credentials.GetUserName(), GetRealm(), GetNonce(), request.GetUri().PathAndQuery, text, empty2);
				if (text3 != null)
				{
					text6 = string.Concat(text6, ", qop=\"" + text3 + "\", nc=" + text2);
				}
				if (!string.IsNullOrEmpty(GetOpaque()))
				{
					text6 = text6 + ", opaque=\"" + GetOpaque() + "\"";
				}
				return text6;
			}
			}
		}
		catch
		{
		}
		return string.Empty;
	}

	public bool IsUriProtected(Uri requestUri)
	{
		if (string.CompareOrdinal(requestUri.Host, GetUri().Host) != 0)
		{
			return false;
		}
		string text = requestUri.ToString();
		if (GetProtectedUris() != null && GetProtectedUris().Count > 0)
		{
			for (int i = 0; i < GetProtectedUris().Count; i++)
			{
				if (text.Contains(GetProtectedUris()[i]))
				{
					return true;
				}
			}
		}
		return true;
	}
}
