using System;
using System.Xml;
using Nekki.Utils;

public abstract class SavedXmlProfile : global::EventDispatcher<object>
{
	public enum ProfileEvent
	{
		onSaveXMLRequired = 0
	}

	protected XmlNode _node;

	private bool savePending;

	protected string serverUserId = string.Empty;

	protected int installId;

	protected float totalPaymentSum;

	protected int paymentCount;

	public string ServerUserId
	{
		get
		{
			return GetServerUserId();
		}
		set
		{
			SetServerUserId(value);
		}
	}

	public int InstallId
	{
		get
		{
			return GetInstallId();
		}
		set
		{
			SetInstallId(value);
		}
	}

	public float TotalPaymentSum
	{
		get
		{
			return GetTotalPaymentSum();
		}
		set
		{
			SetTotalPaymentSum(value);
		}
	}

	public int PaymentCount
	{
		get
		{
			return GetPaymentCount();
		}
		set
		{
			SetPaymentCount(value);
		}
	}

	protected SavedXmlProfile()
	{
		GlobalTimer.get_Instance().addEventListener(0, OnTimerTick);
	}

	public SavedXmlProfile(XmlNode node)
		: this()
	{
		_node = node;
		installId = node.Attributes["InstallID"].ParseInt();
		if (installId == 0)
		{
			SetInstallId((int)new RandomGenerator((uint)DateTime.Now.Millisecond).NextRandom());
		}
		totalPaymentSum = node.Attributes["TotalPaymentSum"].ParseFloat();
		paymentCount = node.Attributes["PaymentCount"].ParseInt();
	}

	public string GetServerUserId()
	{
		return serverUserId;
	}

	public void SetServerUserId(string value)
	{
		serverUserId = value;
		SetNodeAttribute("ServerUserID", serverUserId);
	}

	public int GetInstallId()
	{
		return installId;
	}

	public void SetInstallId(int value)
	{
		installId = value;
		SetNodeAttribute("InstallID", installId);
	}

	public float GetTotalPaymentSum()
	{
		return totalPaymentSum;
	}

	public void SetTotalPaymentSum(float value)
	{
		if (value > totalPaymentSum)
		{
			totalPaymentSum = value;
			SetNodeAttribute("TotalPaymentSum", totalPaymentSum);
		}
	}

	public int GetPaymentCount()
	{
		return paymentCount;
	}

	public void SetPaymentCount(int value)
	{
		if (value > paymentCount)
		{
			paymentCount = value;
			SetNodeAttribute("PaymentCount", paymentCount);
		}
	}

	protected void SetNodeAttribute(string name, long value)
	{
		SetNodeAttribute(name, value.ToString());
	}

	protected void SetNodeAttribute(string name, float value)
	{
		SetNodeAttribute(name, value.ToString());
	}

	protected void SetNodeAttribute(string name, string value)
	{
		if (_node.Attributes[name] != null)
		{
			_node.Attributes[name].Value = value;
		}
		else
		{
			_node.AppendAttribute(name).Value = value;
		}
		RequestSave();
	}

	protected void SetNodeAttribute(string name, bool value)
	{
		SetNodeAttribute(name, (!value) ? "0" : "1");
	}

	public void RequestSave(bool AEGFPEGKLPJ = false)
	{
		if (AEGFPEGKLPJ)
		{
			CallEvent(0, null);
		}
		else
		{
			savePending = true;
		}
	}

	private void OnTimerTick(ExtentionBehaviour.CallEventArgs JKOCDNPPJDG)
	{
		if (savePending)
		{
			CallEvent(0, null);
			savePending = false;
		}
	}
}
