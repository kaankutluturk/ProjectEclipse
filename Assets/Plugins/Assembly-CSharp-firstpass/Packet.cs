using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public sealed class Packet
{
	private enum PayloadType : byte
	{
		Textual = 0,
		Binary = 1
	}

	private const string Placeholder = "_placeholder";

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TransportEventTypes transportEvent;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private SocketIOEventType socketIOEvent;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int attachmentCount;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int id;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string packetNamespace;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string payload;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string eventName;

	private List<byte[]> attachments;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isDecoded;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private object[] decodedArgs;

	public TransportEventTypes TransportEvent
	{
		get
		{
			return GetTransportEvent();
		}
		private set
		{
			SetTransportEvent(value);
		}
	}

	public SocketIOEventType SocketIOEvent
	{
		get
		{
			return GetSocketIOEvent();
		}
		private set
		{
			SetSocketIOEvent(value);
		}
	}

	public int AttachmentCount
	{
		get
		{
			return GetAttachmentCount();
		}
		private set
		{
			SetAttachmentCount(value);
		}
	}

	public int Id
	{
		get
		{
			return GetPacketId();
		}
		private set
		{
			SetId(value);
		}
	}

	public string Payload
	{
		get
		{
			return GetPayload();
		}
		private set
		{
			SetPayload(value);
		}
	}

	public string EventName
	{
		get
		{
			return GetEventName();
		}
		private set
		{
			SetEventName(value);
		}
	}

	public List<byte[]> AttachmentList
	{
		get
		{
			return GetAttachments();
		}
		set
		{
			set_Attachments(value);
		}
	}

	public bool HasAllAttachment
	{
		get
		{
			return GetHasAllAttachment();
		}
	}

	public bool Decoded
	{
		get
		{
			return GetIsDecoded();
		}
		private set
		{
			set_IsDecoded(value);
		}
	}

	public object[] DecodedArguments
	{
		get
		{
			return GetDecodedArgs();
		}
		private set
		{
			set_DecodedArgs(value);
		}
	}

	internal Packet()
	{
		SetTransportEvent(TransportEventTypes.Unknown);
		SetSocketIOEvent(SocketIOEventType.Unknown);
		SetPayload(string.Empty);
	}

	internal Packet(string IOFHCAAOELD)
	{
		Parse(IOFHCAAOELD);
	}

	internal Packet(TransportEventTypes JODJHEDGFDK, SocketIOEventType MANDEJPGHBK, string JBALIKEKHGL, string OIINBGMDJKE, int MGOCOLCDIMG = 0, int OKNNNLIPODI = 0)
	{
		SetTransportEvent(JODJHEDGFDK);
		SetSocketIOEvent(MANDEJPGHBK);
		set_Namespace(JBALIKEKHGL);
		SetPayload(OIINBGMDJKE);
		SetAttachmentCount(MGOCOLCDIMG);
		SetId(OKNNNLIPODI);
	}

	public TransportEventTypes GetTransportEvent()
	{
		return transportEvent;
	}

	private void SetTransportEvent(TransportEventTypes value)
	{
		transportEvent = value;
	}

	public SocketIOEventType GetSocketIOEvent()
	{
		return socketIOEvent;
	}

	private void SetSocketIOEvent(SocketIOEventType value)
	{
		socketIOEvent = value;
	}

	public int GetAttachmentCount()
	{
		return attachmentCount;
	}

	private void SetAttachmentCount(int value)
	{
		attachmentCount = value;
	}

	public int GetPacketId()
	{
		return id;
	}

	private void SetId(int value)
	{
		id = value;
	}

	public string GetNamespace()
	{
		return packetNamespace;
	}

	private void set_Namespace(string value)
	{
		packetNamespace = value;
	}

	public string GetPayload()
	{
		return payload;
	}

	private void SetPayload(string value)
	{
		payload = value;
	}

	public string GetEventName()
	{
		return eventName;
	}

	private void SetEventName(string value)
	{
		eventName = value;
	}

	public List<byte[]> GetAttachments()
	{
		return attachments;
	}

	public void set_Attachments(List<byte[]> value)
	{
		attachments = value;
		SetAttachmentCount((attachments != null) ? attachments.Count : 0);
	}

	public bool GetHasAllAttachment()
	{
		return GetAttachments() != null && GetAttachments().Count == GetAttachmentCount();
	}

	public bool GetIsDecoded()
	{
		return isDecoded;
	}

	private void set_IsDecoded(bool value)
	{
		isDecoded = value;
	}

	public object[] GetDecodedArgs()
	{
		return decodedArgs;
	}

	private void set_DecodedArgs(object[] value)
	{
		decodedArgs = value;
	}

	public object[] Decode(ISocketJsonEncoder GLOJHMAIFOK)
	{
		if (GetIsDecoded() || GLOJHMAIFOK == null)
		{
			return GetDecodedArgs();
		}
		set_IsDecoded(true);
		if (string.IsNullOrEmpty(GetPayload()))
		{
			return GetDecodedArgs();
		}
		List<object> list = GLOJHMAIFOK.Decode(GetPayload());
		if (list != null && list.Count > 0)
		{
			if (GetSocketIOEvent() == SocketIOEventType.Ack || GetSocketIOEvent() == SocketIOEventType.BinaryAck)
			{
				set_DecodedArgs(list.ToArray());
			}
			else
			{
				list.RemoveAt(0);
				set_DecodedArgs(list.ToArray());
			}
		}
		return GetDecodedArgs();
	}

	public string DecodeEventName()
	{
		if (!string.IsNullOrEmpty(GetEventName()))
		{
			return GetEventName();
		}
		if (string.IsNullOrEmpty(GetPayload()))
		{
			return string.Empty;
		}
		if (GetPayload()[0] != '[')
		{
			return string.Empty;
		}
		int i;
		for (i = 1; GetPayload().Length > i && GetPayload()[i] != '"' && GetPayload()[i] != '\''; i++)
		{
		}
		if (GetPayload().Length <= i)
		{
			return string.Empty;
		}
		int num = ++i;
		for (; GetPayload().Length > i && GetPayload()[i] != '"' && GetPayload()[i] != '\''; i++)
		{
		}
		if (GetPayload().Length <= i)
		{
			return string.Empty;
		}
		string text = GetPayload().Substring(num, i - num);
		SetEventName(text);
		return text;
	}

	public string RemoveEventName(bool KGECPDKNJNN)
	{
		if (string.IsNullOrEmpty(GetPayload()))
		{
			return string.Empty;
		}
		if (GetPayload()[0] != '[')
		{
			return string.Empty;
		}
		int i;
		for (i = 1; GetPayload().Length > i && GetPayload()[i] != '"' && GetPayload()[i] != '\''; i++)
		{
		}
		if (GetPayload().Length <= i)
		{
			return string.Empty;
		}
		int num = i;
		for (; GetPayload().Length > i && GetPayload()[i] != ',' && GetPayload()[i] != ']'; i++)
		{
		}
		if (GetPayload().Length <= ++i)
		{
			return string.Empty;
		}
		string text = GetPayload().Remove(num, i - num);
		if (KGECPDKNJNN)
		{
			text = text.Substring(1, text.Length - 2);
		}
		return text;
	}

	public bool ReconstructAttachmentAsIndex()
	{
		return PlaceholderReplacer((string EMDHMHOKGFP, Dictionary<string, object> AOMLCBHAJJH) =>
		{
			int num = Convert.ToInt32(AOMLCBHAJJH["num"]);
			SetPayload(GetPayload().Replace(EMDHMHOKGFP, num.ToString()));
			set_IsDecoded(false);
		});
	}

	public bool ReconstructAttachmentAsBase64()
	{
		if (!GetHasAllAttachment())
		{
			return false;
		}
		return PlaceholderReplacer((string EMDHMHOKGFP, Dictionary<string, object> AOMLCBHAJJH) =>
		{
			int index = Convert.ToInt32(AOMLCBHAJJH["num"]);
			SetPayload(GetPayload().Replace(EMDHMHOKGFP, string.Format("\"{0}\"", Convert.ToBase64String(GetAttachments()[index]))));
			set_IsDecoded(false);
		});
	}

	internal void Parse(string IOFHCAAOELD)
	{
		int i = 0;
		SetTransportEvent((TransportEventTypes)char.GetNumericValue(IOFHCAAOELD, i++));
		if (IOFHCAAOELD.Length > i && char.GetNumericValue(IOFHCAAOELD, i) >= 0.0)
		{
			SetSocketIOEvent((SocketIOEventType)char.GetNumericValue(IOFHCAAOELD, i++));
		}
		else
		{
			SetSocketIOEvent(SocketIOEventType.Unknown);
		}
		if (GetSocketIOEvent() == SocketIOEventType.BinaryEvent || GetSocketIOEvent() == SocketIOEventType.BinaryAck)
		{
			int num = IOFHCAAOELD.IndexOf('-', i);
			if (num == -1)
			{
				num = IOFHCAAOELD.Length;
			}
			int result = 0;
			int.TryParse(IOFHCAAOELD.Substring(i, num - i), out result);
			SetAttachmentCount(result);
			i = num + 1;
		}
		if (IOFHCAAOELD.Length > i && IOFHCAAOELD[i] == '/')
		{
			int num2 = IOFHCAAOELD.IndexOf(',', i);
			if (num2 == -1)
			{
				num2 = IOFHCAAOELD.Length;
			}
			set_Namespace(IOFHCAAOELD.Substring(i, num2 - i));
			i = num2 + 1;
		}
		else
		{
			set_Namespace("/");
		}
		if (IOFHCAAOELD.Length > i && char.GetNumericValue(IOFHCAAOELD[i]) >= 0.0)
		{
			int num3 = i++;
			for (; IOFHCAAOELD.Length > i && char.GetNumericValue(IOFHCAAOELD[i]) >= 0.0; i++)
			{
			}
			int result2 = 0;
			int.TryParse(IOFHCAAOELD.Substring(num3, i - num3), out result2);
			SetId(result2);
		}
		if (IOFHCAAOELD.Length > i)
		{
			SetPayload(IOFHCAAOELD.Substring(i));
		}
		else
		{
			SetPayload(string.Empty);
		}
	}

	internal string Encode()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (GetTransportEvent() == TransportEventTypes.Unknown && GetAttachmentCount() > 0)
		{
			SetTransportEvent(TransportEventTypes.Message);
		}
		if (GetTransportEvent() != TransportEventTypes.Unknown)
		{
			stringBuilder.Append(((int)GetTransportEvent()/*cast due to constrained. prefix*/).ToString());
		}
		if (GetSocketIOEvent() == SocketIOEventType.Unknown && GetAttachmentCount() > 0)
		{
			SetSocketIOEvent(SocketIOEventType.BinaryEvent);
		}
		if (GetSocketIOEvent() != SocketIOEventType.Unknown)
		{
			stringBuilder.Append(((int)GetSocketIOEvent()/*cast due to constrained. prefix*/).ToString());
		}
		if (GetSocketIOEvent() == SocketIOEventType.BinaryEvent || GetSocketIOEvent() == SocketIOEventType.BinaryAck)
		{
			stringBuilder.Append(GetAttachmentCount().ToString());
			stringBuilder.Append("-");
		}
		bool flag = false;
		if (GetNamespace() != "/")
		{
			stringBuilder.Append(GetNamespace());
			flag = true;
		}
		if (GetPacketId() != 0)
		{
			if (flag)
			{
				stringBuilder.Append(",");
				flag = false;
			}
			stringBuilder.Append(GetPacketId().ToString());
		}
		if (!string.IsNullOrEmpty(GetPayload()))
		{
			if (flag)
			{
				stringBuilder.Append(",");
				flag = false;
			}
			stringBuilder.Append(GetPayload());
		}
		return stringBuilder.ToString();
	}

	internal byte[] EncodeBinary()
	{
		if (GetAttachmentCount() != 0 || (GetAttachments() != null && GetAttachments().Count != 0))
		{
			if (GetAttachments() == null)
			{
				throw new ArgumentException("packet.Attachments are null!");
			}
			if (GetAttachmentCount() != GetAttachments().Count)
			{
				throw new ArgumentException("packet.AttachmentCount != packet.Attachments.Count. Use the packet.AddAttachment function to add data to a packet!");
			}
		}
		string s = Encode();
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		byte[] array = EncodeData(bytes, PayloadType.Textual, null);
		if (GetAttachmentCount() != 0)
		{
			int num = array.Length;
			List<byte[]> list = new List<byte[]>(GetAttachmentCount());
			int num2 = 0;
			for (int i = 0; i < GetAttachmentCount(); i++)
			{
				byte[] array2 = EncodeData(GetAttachments()[i], PayloadType.Binary, new byte[1] { 4 });
				list.Add(array2);
				num2 += array2.Length;
			}
			Array.Resize(ref array, array.Length + num2);
			for (int j = 0; j < GetAttachmentCount(); j++)
			{
				byte[] array3 = list[j];
				Array.Copy(array3, 0, array, num, array3.Length);
				num += array3.Length;
			}
		}
		return array;
	}

	internal void AddAttachmentFromServer(byte[] data, bool FEEFOCCJIML)
	{
		if (data != null && data.Length != 0)
		{
			if (attachments == null)
			{
				attachments = new List<byte[]>(GetAttachmentCount());
			}
			if (FEEFOCCJIML)
			{
				GetAttachments().Add(data);
				return;
			}
			byte[] array = new byte[data.Length - 1];
			Array.Copy(data, 1, array, 0, data.Length - 1);
			GetAttachments().Add(array);
		}
	}

	private byte[] EncodeData(byte[] data, PayloadType LFLGCDNKNJI, byte[] BGFJJKCBOJI)
	{
		int num = ((BGFJJKCBOJI != null) ? BGFJJKCBOJI.Length : 0);
		string text = (data.Length + num).ToString();
		byte[] array = new byte[text.Length];
		for (int i = 0; i < text.Length; i++)
		{
			array[i] = (byte)char.GetNumericValue(text[i]);
		}
		byte[] array2 = new byte[data.Length + array.Length + 2 + num];
		array2[0] = (byte)LFLGCDNKNJI;
		for (int j = 0; j < array.Length; j++)
		{
			array2[1 + j] = array[j];
		}
		int num2 = 1 + array.Length;
		array2[num2++] = byte.MaxValue;
		if (BGFJJKCBOJI != null && BGFJJKCBOJI.Length > 0)
		{
			Array.Copy(BGFJJKCBOJI, 0, array2, num2, BGFJJKCBOJI.Length);
			num2 += BGFJJKCBOJI.Length;
		}
		Array.Copy(data, 0, array2, num2, data.Length);
		return array2;
	}

	private bool PlaceholderReplacer(Action<string, Dictionary<string, object>> FOACGDMKGNH)
	{
		if (string.IsNullOrEmpty(GetPayload()))
		{
			return false;
		}
		for (int num = GetPayload().IndexOf("_placeholder"); num >= 0; num = GetPayload().IndexOf("_placeholder"))
		{
			int num2 = num;
			while (GetPayload()[num2] != '{')
			{
				num2--;
			}
			int i;
			for (i = num; GetPayload().Length > i && GetPayload()[i] != '}'; i++)
			{
			}
			if (GetPayload().Length <= i)
			{
				return false;
			}
			string text = GetPayload().Substring(num2, i - num2 + 1);
			bool IBFAPIMOMBA = false;
			Dictionary<string, object> dictionary = Json.Decode(text, ref IBFAPIMOMBA) as Dictionary<string, object>;
			if (!IBFAPIMOMBA)
			{
				return false;
			}
			object value;
			if (!dictionary.TryGetValue("_placeholder", out value) || !(bool)value)
			{
				return false;
			}
			if (!dictionary.TryGetValue("num", out value))
			{
				return false;
			}
			FOACGDMKGNH(text, dictionary);
		}
		return true;
	}

	public override string ToString()
	{
		return GetPayload();
	}

	internal Packet Clone()
	{
		Packet cMPKPLIGKLC = new Packet(GetTransportEvent(), GetSocketIOEvent(), GetNamespace(), GetPayload(), 0, GetPacketId());
		cMPKPLIGKLC.SetEventName(GetEventName());
		cMPKPLIGKLC.SetAttachmentCount(GetAttachmentCount());
		cMPKPLIGKLC.attachments = attachments;
		return cMPKPLIGKLC;
	}
}
