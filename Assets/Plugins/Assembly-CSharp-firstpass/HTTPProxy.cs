using System;
using System.Diagnostics;

public sealed class HTTPProxy
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Uri address;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Credentials credentials;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isTransparent;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool sendWholeUri;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool nonTransparentForHttps;

	public Uri ProxyAddress
	{
		get
		{
			return GetAddress();
		}
		set
		{
			set_Address(value);
		}
	}

	public Credentials Credentials
	{
		get
		{
			return GetCredentials();
		}
		set
		{
			SetCredentials(value);
		}
	}

	public bool IsTransparent
	{
		get
		{
			return GetIsTransparent();
		}
		set
		{
			SetIsTransparent(value);
		}
	}

	public bool SendWholeUri
	{
		get
		{
			return GetSendWholeUri();
		}
		set
		{
			SetSendWholeUri(value);
		}
	}

	public bool NonTransparentForHTTPS
	{
		get
		{
			return GetNonTransparentForHTTPS();
		}
		set
		{
			SetNonTransparentForHTTPS(value);
		}
	}

	public HTTPProxy()
		: this(null, null, false)
	{
	}

	public HTTPProxy(Uri IKHEAOEKLHL)
		: this(IKHEAOEKLHL, null, false)
	{
	}

	public HTTPProxy(Uri IKHEAOEKLHL, Credentials JKBAHGNLECO)
		: this(IKHEAOEKLHL, JKBAHGNLECO, false)
	{
	}

	public HTTPProxy(Uri IKHEAOEKLHL, Credentials JKBAHGNLECO, bool CBLAANPNDHE)
		: this(IKHEAOEKLHL, JKBAHGNLECO, CBLAANPNDHE, true)
	{
	}

	public HTTPProxy(Uri IKHEAOEKLHL, Credentials JKBAHGNLECO, bool CBLAANPNDHE, bool HAIHALMOEFD)
		: this(IKHEAOEKLHL, JKBAHGNLECO, CBLAANPNDHE, true, true)
	{
	}

	public HTTPProxy(Uri IKHEAOEKLHL, Credentials JKBAHGNLECO, bool CBLAANPNDHE, bool HAIHALMOEFD, bool AGCDFHONOEF)
	{
		set_Address(IKHEAOEKLHL);
		SetCredentials(JKBAHGNLECO);
		SetIsTransparent(CBLAANPNDHE);
		SetSendWholeUri(HAIHALMOEFD);
		SetNonTransparentForHTTPS(AGCDFHONOEF);
	}

	public Uri GetAddress()
	{
		return address;
	}

	public void set_Address(Uri value)
	{
		address = value;
	}

	public Credentials GetCredentials()
	{
		return credentials;
	}

	public void SetCredentials(Credentials value)
	{
		credentials = value;
	}

	public bool GetIsTransparent()
	{
		return isTransparent;
	}

	public void SetIsTransparent(bool value)
	{
		isTransparent = value;
	}

	public bool GetSendWholeUri()
	{
		return sendWholeUri;
	}

	public void SetSendWholeUri(bool value)
	{
		sendWholeUri = value;
	}

	public bool GetNonTransparentForHTTPS()
	{
		return nonTransparentForHttps;
	}

	public void SetNonTransparentForHTTPS(bool value)
	{
		nonTransparentForHttps = value;
	}
}
