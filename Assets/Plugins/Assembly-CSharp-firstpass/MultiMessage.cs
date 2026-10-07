using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public sealed class MultiMessage : IServerMessage
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string messageId;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isInitialization;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string groupsToken;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool shouldReconnect;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TimeSpan? pollDelay;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<IServerMessage> data;

	public string MessageId
	{
		get
		{
			return GetMessageId();
		}
		private set
		{
			SetMessageId(value);
		}
	}

	public bool IsInitialization
	{
		get
		{
			return GetIsInitialization();
		}
		private set
		{
			SetIsInitialization(value);
		}
	}

	public string GroupsToken
	{
		get
		{
			return GetGroupsToken();
		}
		private set
		{
			SetGroupsToken(value);
		}
	}

	public bool ShouldReconnect
	{
		get
		{
			return GetShouldReconnect();
		}
		private set
		{
			SetShouldReconnect(value);
		}
	}

	public TimeSpan? PollInterval
	{
		get
		{
			return GetPollDelay();
		}
		private set
		{
			set_PollDelay(value);
		}
	}

	public MessageTypes get_Type()
	{
		return MessageTypes.Multiple;
	}

	public string GetMessageId()
	{
		return messageId;
	}

	private void SetMessageId(string value)
	{
		messageId = value;
	}

	public bool GetIsInitialization()
	{
		return isInitialization;
	}

	private void SetIsInitialization(bool value)
	{
		isInitialization = value;
	}

	public string GetGroupsToken()
	{
		return groupsToken;
	}

	private void SetGroupsToken(string value)
	{
		groupsToken = value;
	}

	public bool GetShouldReconnect()
	{
		return shouldReconnect;
	}

	private void SetShouldReconnect(bool value)
	{
		shouldReconnect = value;
	}

	public TimeSpan? GetPollDelay()
	{
		return pollDelay;
	}

	private void set_PollDelay(TimeSpan? value)
	{
		pollDelay = value;
	}

	public List<IServerMessage> GetData()
	{
		return data;
	}

	private void set_Data(List<IServerMessage> value)
	{
		data = value;
	}

	void IServerMessage.Parse(object data)
	{
		IDictionary<string, object> dictionary = data as IDictionary<string, object>;
		SetMessageId(dictionary["C"].ToString());
		object value;
		if (dictionary.TryGetValue("S", out value))
		{
			SetIsInitialization(int.Parse(value.ToString()) == 1);
		}
		else
		{
			SetIsInitialization(false);
		}
		if (dictionary.TryGetValue("G", out value))
		{
			SetGroupsToken(value.ToString());
		}
		if (dictionary.TryGetValue("T", out value))
		{
			SetShouldReconnect(int.Parse(value.ToString()) == 1);
		}
		else
		{
			SetShouldReconnect(false);
		}
		if (dictionary.TryGetValue("L", out value))
		{
			set_PollDelay(TimeSpan.FromMilliseconds(double.Parse(value.ToString())));
		}
		IEnumerable enumerable = dictionary["M"] as IEnumerable;
		if (enumerable == null)
		{
			return;
		}
		set_Data(new List<IServerMessage>());
		foreach (object item in enumerable)
		{
			IDictionary<string, object> dictionary2 = item as IDictionary<string, object>;
			IServerMessage message = null;
			message = ((dictionary2 == null) ? new DataMessage() : ((!dictionary2.ContainsKey("H")) ? ((!dictionary2.ContainsKey("I")) ? ((IServerMessage)new DataMessage()) : ((IServerMessage)new ProgressMessage())) : new MethodCallMessage()));
			message.Parse(item);
			GetData().Add(message);
		}
	}
}
