using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class HTTPResponse : IDisposable
{
	internal const byte CR = 13;

	internal const byte LF = 10;

	public const int MinBufferSize = 4096;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int versionMajor;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int versionMinor;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int statusCode;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string message;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isStreamed;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isStreamingFinished;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isFromCache;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Dictionary<string, List<string>> headers;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private byte[] data;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isUpgraded;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<Cookie> cookies;

	protected string dataAsText;

	protected Texture2D texture;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isClosedManually;

	internal HTTPRequest BaseRequest;

	protected Stream Stream;

	protected List<byte[]> streamedFragments;

	protected object SyncRoot = new object();

	protected byte[] fragmentBuffer;

	protected int fragmentBufferDataLength;

	protected Stream cacheStream;

	protected int allFragmentSize;

	public int VersionMajor
	{
		get
		{
			return GetVersionMajor();
		}
		protected set
		{
			SetVersionMajor(value);
		}
	}

	public int VersionMinor
	{
		get
		{
			return GetVersionMinor();
		}
		protected set
		{
			SetVersionMinor(value);
		}
	}

	public int StatusCode
	{
		get
		{
			return GetStatusCode();
		}
		protected set
		{
			SetStatusCode(value);
		}
	}

	public bool IsSuccess
	{
		get
		{
			return GetIsSuccess();
		}
	}

	public bool IsStreamed
	{
		get
		{
			return GetIsStreamed();
		}
		protected set
		{
			SetIsStreamed(value);
		}
	}

	public bool IsStreamingFinished
	{
		get
		{
			return GetIsStreamingFinished();
		}
		internal set
		{
			SetIsStreamingFinished(value);
		}
	}

	public bool IsFromCache
	{
		get
		{
			return GetIsFromCache();
		}
		internal set
		{
			SetIsFromCache(value);
		}
	}

	public Dictionary<string, List<string>> ResponseHeaders
	{
		get
		{
			return GetHeaders();
		}
		protected set
		{
			set_Headers(value);
		}
	}

	public bool IsUpgraded
	{
		get
		{
			return GetIsUpgraded();
		}
		protected set
		{
			SetIsUpgraded(value);
		}
	}

	public List<Cookie> Cookies
	{
		get
		{
			return GetCookies();
		}
		internal set
		{
			SetCookies(value);
		}
	}

	public string DataAsText
	{
		get
		{
			return GetDataAsText();
		}
	}

	public Texture2D DataAsTexture2D
	{
		get
		{
			return GetDataAsTexture2D();
		}
	}

	public bool IsClosedManually
	{
		get
		{
			return GetIsClosedManually();
		}
		protected set
		{
			SetIsClosedManually(value);
		}
	}

	internal HTTPResponse(HTTPRequest ONOCIELLAPL, Stream ABJIEFMMIEK, bool IBIIADCLKCH, bool PEAJIKCANHP)
	{
		BaseRequest = ONOCIELLAPL;
		Stream = ABJIEFMMIEK;
		SetIsStreamed(IBIIADCLKCH);
		SetIsFromCache(PEAJIKCANHP);
		SetIsClosedManually(false);
	}

	public int GetVersionMajor()
	{
		return versionMajor;
	}

	protected void SetVersionMajor(int value)
	{
		versionMajor = value;
	}

	public int GetVersionMinor()
	{
		return versionMinor;
	}

	protected void SetVersionMinor(int value)
	{
		versionMinor = value;
	}

	public int GetStatusCode()
	{
		return statusCode;
	}

	protected void SetStatusCode(int value)
	{
		statusCode = value;
	}

	public bool GetIsSuccess()
	{
		return (GetStatusCode() >= 200 && GetStatusCode() < 300) || GetStatusCode() == 304;
	}

	public string GetMessage()
	{
		return message;
	}

	protected void set_Message(string value)
	{
		message = value;
	}

	public bool GetIsStreamed()
	{
		return isStreamed;
	}

	protected void SetIsStreamed(bool value)
	{
		isStreamed = value;
	}

	public bool GetIsStreamingFinished()
	{
		return isStreamingFinished;
	}

	internal void SetIsStreamingFinished(bool value)
	{
		isStreamingFinished = value;
	}

	public bool GetIsFromCache()
	{
		return isFromCache;
	}

	internal void SetIsFromCache(bool value)
	{
		isFromCache = value;
	}

	public Dictionary<string, List<string>> GetHeaders()
	{
		return headers;
	}

	protected void set_Headers(Dictionary<string, List<string>> value)
	{
		headers = value;
	}

	public byte[] GetData()
	{
		return data;
	}

	internal void set_Data(byte[] value)
	{
		data = value;
	}

	public bool GetIsUpgraded()
	{
		return isUpgraded;
	}

	protected void SetIsUpgraded(bool value)
	{
		isUpgraded = value;
	}

	public List<Cookie> GetCookies()
	{
		return cookies;
	}

	internal void SetCookies(List<Cookie> value)
	{
		cookies = value;
	}

	public string GetDataAsText()
	{
		if (GetData() == null)
		{
			return string.Empty;
		}
		if (!string.IsNullOrEmpty(dataAsText))
		{
			return dataAsText;
		}
		return dataAsText = Encoding.UTF8.GetString(GetData(), 0, GetData().Length);
	}

	public Texture2D GetDataAsTexture2D()
	{
		if (GetData() == null)
		{
			return null;
		}
		if (texture != null)
		{
			return texture;
		}
		texture = new Texture2D(0, 0, TextureFormat.ARGB32, false);
		texture.LoadImage(GetData());
		return texture;
	}

	public bool GetIsClosedManually()
	{
		return isClosedManually;
	}

	protected void SetIsClosedManually(bool value)
	{
		isClosedManually = value;
	}

	internal virtual bool Receive(int JHFPNBPNHEH = -1, bool NDCKHEGBAGO = true)
	{
		string empty = string.Empty;
		try
		{
			empty = ReadTo(Stream, 32);
		}
		catch
		{
			if (!BaseRequest.GetDisableRetry())
			{
				return false;
			}
			throw;
		}
		if (!BaseRequest.GetDisableRetry() && string.IsNullOrEmpty(empty))
		{
			return false;
		}
		string[] array = empty.Split('/', '.');
		SetVersionMajor(int.Parse(array[1]));
		SetVersionMinor(int.Parse(array[2]));
		string text = NoTrimReadTo(Stream, 32, 10);
		int result;
		if (BaseRequest.GetDisableRetry())
		{
			result = int.Parse(text);
		}
		else if (!int.TryParse(text, out result))
		{
			return false;
		}
		SetStatusCode(result);
		if (text.Length > 0 && (byte)text[text.Length - 1] != 10 && (byte)text[text.Length - 1] != 13)
		{
			set_Message(ReadTo(Stream, 10));
		}
		else
		{
			set_Message(string.Empty);
		}
		ReadHeaders(Stream);
		SetIsUpgraded(GetStatusCode() == 101 && (HasHeaderWithValue("connection", "upgrade") || HasHeader("upgrade")));
		if (!NDCKHEGBAGO)
		{
			return true;
		}
		return ReadPayload(JHFPNBPNHEH);
	}

	protected bool ReadPayload(int JHFPNBPNHEH)
	{
		if (JHFPNBPNHEH != -1)
		{
			SetIsFromCache(true);
			ReadRaw(Stream, JHFPNBPNHEH);
			return true;
		}
		if ((GetStatusCode() >= 100 && GetStatusCode() < 200) || GetStatusCode() == 204 || GetStatusCode() == 304 || BaseRequest.GetMethodType() == HTTPMethods.Head)
		{
			return true;
		}
		if (HasHeaderWithValue("transfer-encoding", "chunked"))
		{
			ReadChunked(Stream);
		}
		else
		{
			List<string> list = GetHeaderValues("content-length");
			List<string> list2 = GetHeaderValues("content-range");
			if (list != null && list2 == null)
			{
				ReadRaw(Stream, int.Parse(list[0]));
			}
			else if (list2 != null)
			{
				HTTPRange jALPJGLIOFH = GetRange();
				ReadRaw(Stream, jALPJGLIOFH.GetLastBytePos() - jALPJGLIOFH.GetFirstBytePos() + 1);
			}
			else
			{
				ReadUnknownSize(Stream);
			}
		}
		return true;
	}

	protected void ReadHeaders(Stream ABJIEFMMIEK)
	{
		string text = ReadTo(ABJIEFMMIEK, 58, 10).Trim();
		while (text != string.Empty)
		{
			string bAINMLLIKOL = ReadTo(ABJIEFMMIEK, 10);
			AddHeader(text, bAINMLLIKOL);
			text = ReadTo(ABJIEFMMIEK, 58, 10);
		}
	}

	protected void AddHeader(string name, string value)
	{
		name = name.ToLower();
		if (GetHeaders() == null)
		{
			set_Headers(new Dictionary<string, List<string>>());
		}
		List<string> list;
		if (!GetHeaders().TryGetValue(name, out list))
		{
			GetHeaders().Add(name, list = new List<string>(1));
		}
		list.Add(value);
	}

	public List<string> GetHeaderValues(string name)
	{
		if (GetHeaders() == null)
		{
			return null;
		}
		name = name.ToLower();
		List<string> value;
		if (!GetHeaders().TryGetValue(name, out value) || value.Count == 0)
		{
			return null;
		}
		return value;
	}

	public string GetFirstHeaderValue(string name)
	{
		if (GetHeaders() == null)
		{
			return null;
		}
		name = name.ToLower();
		List<string> value;
		if (!GetHeaders().TryGetValue(name, out value) || value.Count == 0)
		{
			return null;
		}
		return value[0];
	}

	public bool HasHeaderWithValue(string JOKIBHMEDAO, string value)
	{
		List<string> list = GetHeaderValues(JOKIBHMEDAO);
		if (list == null)
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (string.Compare(list[i], value, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasHeader(string JOKIBHMEDAO)
	{
		List<string> list = GetHeaderValues(JOKIBHMEDAO);
		if (list == null)
		{
			return false;
		}
		return true;
	}

	public HTTPRange GetRange()
	{
		List<string> list = GetHeaderValues("content-range");
		if (list == null)
		{
			return null;
		}
		string[] array = list[0].Split(new char[3] { ' ', '-', '/' }, StringSplitOptions.RemoveEmptyEntries);
		if (array[1] == "*")
		{
			return new HTTPRange(int.Parse(array[2]));
		}
		return new HTTPRange(int.Parse(array[1]), int.Parse(array[2]), (!(array[3] != "*")) ? (-1) : int.Parse(array[3]));
	}

	public static string ReadTo(Stream ABJIEFMMIEK, byte MOFIAGJPCNA)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			int num = ABJIEFMMIEK.ReadByte();
			while (num != MOFIAGJPCNA && num != -1)
			{
				memoryStream.WriteByte((byte)num);
				num = ABJIEFMMIEK.ReadByte();
			}
			return memoryStream.ToArray().AsciiToString().Trim();
		}
	}

	public static string ReadTo(Stream ABJIEFMMIEK, byte ECEBLBGKFPF, byte NELKINPNIGD)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			int num = ABJIEFMMIEK.ReadByte();
			while (num != ECEBLBGKFPF && num != NELKINPNIGD && num != -1)
			{
				memoryStream.WriteByte((byte)num);
				num = ABJIEFMMIEK.ReadByte();
			}
			return memoryStream.ToArray().AsciiToString().Trim();
		}
	}

	public static string NoTrimReadTo(Stream ABJIEFMMIEK, byte ECEBLBGKFPF, byte NELKINPNIGD)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			int num = ABJIEFMMIEK.ReadByte();
			while (num != ECEBLBGKFPF && num != NELKINPNIGD && num != -1)
			{
				memoryStream.WriteByte((byte)num);
				num = ABJIEFMMIEK.ReadByte();
			}
			return memoryStream.ToArray().AsciiToString();
		}
	}

	protected int ReadChunkLength(Stream ABJIEFMMIEK)
	{
		string text = ReadTo(ABJIEFMMIEK, 10);
		string[] array = text.Split(';');
		string text2 = array[0];
		int result;
		if (int.TryParse(text2, NumberStyles.AllowHexSpecifier, null, out result))
		{
			return result;
		}
		throw new Exception(string.Format("Can't parse '{0}' as a hex number!", text2));
	}

	protected void ReadChunked(Stream ABJIEFMMIEK)
	{
		BeginReceiveStreamFragments();
		using (MemoryStream memoryStream = new MemoryStream())
		{
			int num = ReadChunkLength(ABJIEFMMIEK);
			byte[] array = new byte[num];
			int num2 = 0;
			BaseRequest.SetDownloadLength(num);
			BaseRequest.SetDownloadProgressChanged(GetIsSuccess() || GetIsFromCache());
			while (num != 0)
			{
				if (array.Length < num)
				{
					Array.Resize(ref array, num);
				}
				int num3 = 0;
				WaitWhileHasFragments();
				do
				{
					int num4 = ABJIEFMMIEK.Read(array, num3, num - num3);
					if (num4 == 0)
					{
						throw new Exception("The remote server closed the connection unexpectedly!");
					}
					num3 += num4;
				}
				while (num3 < num);
				if (BaseRequest.GetUseStreaming())
				{
					FeedStreamFragment(array, 0, num3);
				}
				else
				{
					memoryStream.Write(array, 0, num3);
				}
				ReadTo(ABJIEFMMIEK, 10);
				num2 += num3;
				num = ReadChunkLength(ABJIEFMMIEK);
				HTTPRequest kEEGKCNNPGM = BaseRequest;
				kEEGKCNNPGM.SetDownloadLength(kEEGKCNNPGM.GetDownloadLength() + num);
				BaseRequest.SetDownloaded(num2);
				BaseRequest.SetDownloadProgressChanged(GetIsSuccess() || GetIsFromCache());
			}
			if (BaseRequest.GetUseStreaming())
			{
				FlushRemainingFragmentBuffer();
			}
			ReadHeaders(ABJIEFMMIEK);
			if (!BaseRequest.GetUseStreaming())
			{
				set_Data(DecodeStream(memoryStream));
			}
		}
	}

	internal void ReadRaw(Stream ABJIEFMMIEK, int HDIIBKGCCNB)
	{
		BeginReceiveStreamFragments();
		BaseRequest.SetDownloadLength(HDIIBKGCCNB);
		BaseRequest.SetDownloadProgressChanged(GetIsSuccess() || GetIsFromCache());
		using (MemoryStream memoryStream = new MemoryStream((!BaseRequest.GetUseStreaming()) ? HDIIBKGCCNB : 0))
		{
			byte[] array = new byte[Math.Max(BaseRequest.GetStreamFragmentSize(), 4096)];
			int num = 0;
			while (HDIIBKGCCNB > 0)
			{
				num = 0;
				WaitWhileHasFragments();
				do
				{
					int num2 = ABJIEFMMIEK.Read(array, num, Math.Min(HDIIBKGCCNB, array.Length - num));
					if (num2 == 0)
					{
						throw new Exception("The remote server closed the connection unexpectedly!");
					}
					num += num2;
					HDIIBKGCCNB -= num2;
					HTTPRequest kEEGKCNNPGM = BaseRequest;
					kEEGKCNNPGM.SetDownloaded(kEEGKCNNPGM.GetDownloaded() + num2);
					BaseRequest.SetDownloadProgressChanged(GetIsSuccess() || GetIsFromCache());
				}
				while (num < array.Length && HDIIBKGCCNB > 0);
				if (BaseRequest.GetUseStreaming())
				{
					FeedStreamFragment(array, 0, num);
				}
				else
				{
					memoryStream.Write(array, 0, num);
				}
			}
			if (BaseRequest.GetUseStreaming())
			{
				FlushRemainingFragmentBuffer();
			}
			if (!BaseRequest.GetUseStreaming())
			{
				set_Data(DecodeStream(memoryStream));
			}
		}
	}

	protected void ReadUnknownSize(Stream ABJIEFMMIEK)
	{
		NetworkStream networkStream = ABJIEFMMIEK as NetworkStream;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			byte[] array = new byte[Math.Max(BaseRequest.GetStreamFragmentSize(), 4096)];
			int num = 0;
			int num2 = 0;
			do
			{
				num = 0;
				do
				{
					num2 = 0;
					if (networkStream != null)
					{
						for (int i = num; i < array.Length; i++)
						{
							if (!networkStream.DataAvailable)
							{
								break;
							}
							int num3 = ABJIEFMMIEK.ReadByte();
							if (num3 >= 0)
							{
								array[i] = (byte)num3;
								num2++;
								continue;
							}
							break;
						}
					}
					else
					{
						num2 = ABJIEFMMIEK.Read(array, num, array.Length - num);
					}
					num += num2;
					HTTPRequest kEEGKCNNPGM = BaseRequest;
					kEEGKCNNPGM.SetDownloaded(kEEGKCNNPGM.GetDownloaded() + num2);
					BaseRequest.SetDownloadLength(BaseRequest.GetDownloaded());
					BaseRequest.SetDownloadProgressChanged(GetIsSuccess() || GetIsFromCache());
				}
				while (num < array.Length && num2 > 0);
				if (BaseRequest.GetUseStreaming())
				{
					FeedStreamFragment(array, 0, num);
				}
				else
				{
					memoryStream.Write(array, 0, num);
				}
			}
			while (num2 > 0);
			if (BaseRequest.GetUseStreaming())
			{
				FlushRemainingFragmentBuffer();
			}
			if (!BaseRequest.GetUseStreaming())
			{
				set_Data(DecodeStream(memoryStream));
			}
		}
	}

	protected byte[] DecodeStream(Stream JGFDPHDCFNL)
	{
		JGFDPHDCFNL.Seek(0L, SeekOrigin.Begin);
		List<string> list = ((!GetIsFromCache()) ? GetHeaderValues("content-encoding") : null);
		Stream stream = null;
		if (list == null)
		{
			stream = JGFDPHDCFNL;
		}
		else
		{
			switch (list[0])
			{
			case "gzip":
				stream = new GZipCompressionStream(JGFDPHDCFNL, ZlibCompressionMode.Decompress);
				break;
			case "deflate":
				stream = new ZlibDeflateStream(JGFDPHDCFNL, ZlibCompressionMode.Decompress);
				break;
			default:
				stream = JGFDPHDCFNL;
				break;
			}
		}
		using (MemoryStream memoryStream = new MemoryStream((int)JGFDPHDCFNL.Length))
		{
			byte[] array = new byte[1024];
			int num = 0;
			while ((num = stream.Read(array, 0, array.Length)) > 0)
			{
				memoryStream.Write(array, 0, num);
			}
			return memoryStream.ToArray();
		}
	}

	protected void BeginReceiveStreamFragments()
	{
		if (!BaseRequest.GetDisableCache() && BaseRequest.GetUseStreaming() && !GetIsFromCache() && HTTPCacheService.IsCacheble(BaseRequest.GetCurrentUri(), BaseRequest.GetMethodType(), this))
		{
			cacheStream = HTTPCacheService.PrepareStreamed(BaseRequest.GetCurrentUri(), this);
		}
		allFragmentSize = 0;
	}

	protected void FeedStreamFragment(byte[] buffer, int LCCLEFMKLPB, int BDBOAEGELMC)
	{
		if (fragmentBuffer == null)
		{
			fragmentBuffer = new byte[BaseRequest.GetStreamFragmentSize()];
			fragmentBufferDataLength = 0;
		}
		if (fragmentBufferDataLength + BDBOAEGELMC <= BaseRequest.GetStreamFragmentSize())
		{
			Array.Copy(buffer, LCCLEFMKLPB, fragmentBuffer, fragmentBufferDataLength, BDBOAEGELMC);
			fragmentBufferDataLength += BDBOAEGELMC;
			if (fragmentBufferDataLength == BaseRequest.GetStreamFragmentSize())
			{
				AddStreamedFragment(fragmentBuffer);
				fragmentBuffer = null;
				fragmentBufferDataLength = 0;
			}
		}
		else
		{
			int num = BaseRequest.GetStreamFragmentSize() - fragmentBufferDataLength;
			FeedStreamFragment(buffer, LCCLEFMKLPB, num);
			FeedStreamFragment(buffer, LCCLEFMKLPB + num, BDBOAEGELMC - num);
		}
	}

	protected void FlushRemainingFragmentBuffer()
	{
		if (fragmentBuffer != null)
		{
			Array.Resize(ref fragmentBuffer, fragmentBufferDataLength);
			AddStreamedFragment(fragmentBuffer);
			fragmentBuffer = null;
			fragmentBufferDataLength = 0;
		}
		if (cacheStream != null)
		{
			cacheStream.Dispose();
			cacheStream = null;
			HTTPCacheService.SetBodyLength(BaseRequest.GetCurrentUri(), allFragmentSize);
		}
	}

	protected void AddStreamedFragment(byte[] buffer)
	{
		lock (SyncRoot)
		{
			if (streamedFragments == null)
			{
				streamedFragments = new List<byte[]>();
			}
			streamedFragments.Add(buffer);
			if (cacheStream != null)
			{
				cacheStream.Write(buffer, 0, buffer.Length);
				allFragmentSize += buffer.Length;
			}
		}
	}

	protected void WaitWhileHasFragments()
	{
	}

	public List<byte[]> GetStreamedFragments()
	{
		lock (SyncRoot)
		{
			if (streamedFragments == null || streamedFragments.Count == 0)
			{
				return null;
			}
			List<byte[]> result = new List<byte[]>(streamedFragments);
			streamedFragments.Clear();
			return result;
		}
	}

	internal bool HasStreamedFragments()
	{
		lock (SyncRoot)
		{
			return streamedFragments != null && streamedFragments.Count > 0;
		}
	}

	internal void FinishStreaming()
	{
		SetIsStreamingFinished(true);
		Dispose();
	}

	public void Dispose()
	{
		if (cacheStream != null)
		{
			cacheStream.Dispose();
			cacheStream = null;
		}
	}
}
