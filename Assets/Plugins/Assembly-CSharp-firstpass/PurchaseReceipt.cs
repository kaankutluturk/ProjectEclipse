using System.Diagnostics;
using SimpleJSON;
using SF2.Offline;

public class PurchaseReceipt
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string store;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string transactionId;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string payload;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string json;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string signature;

	private Product product;

	public string Store
	{
		get
		{
			return GetStore();
		}
		private set
		{
			SetStore(value);
		}
	}

	public string TransactionId
	{
		get
		{
			return GetTransactionId();
		}
		private set
		{
			SetTransactionId(value);
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

	public string Json
	{
		get
		{
			return GetJson();
		}
		private set
		{
			SetJson(value);
		}
	}

	public string Signature
	{
		get
		{
			return GetSignature();
		}
		private set
		{
			SetSignature(value);
		}
	}

	public ProductDefinition Definition
	{
		get
		{
			return GetDefinition();
		}
	}

	public string ProductId
	{
		get
		{
			return GetProductId();
		}
	}

	public bool IsFake
	{
		get
		{
			return GetIsFake();
		}
	}

	public PurchaseReceipt(Product KDOEGOIJKLG)
	{
		product = KDOEGOIJKLG;
		JSONNode jSONNode = JSONNode.Parse(product.receipt);
		SetStore((!jSONNode.HasValue("Store")) ? string.Empty : jSONNode["Store"].Value);
		SetTransactionId((!jSONNode.HasValue("TransactionID")) ? string.Empty : jSONNode["TransactionID"].Value);
		SetPayload((!jSONNode.HasValue("Payload")) ? string.Empty : jSONNode["Payload"].Value);
		if (!GetIsFake())
		{
			jSONNode = JSONNode.Parse(GetPayload());
			SetJson((!jSONNode.HasValue("json")) ? string.Empty : jSONNode["json"].Value);
			SetSignature((!jSONNode.HasValue("signature")) ? string.Empty : jSONNode["signature"].Value);
		}
		else
		{
			SetJson(string.Empty);
			SetSignature(string.Empty);
		}
	}

	public string GetStore()
	{
		return store;
	}

	private void SetStore(string value)
	{
		store = value;
	}

	public string GetTransactionId()
	{
		return transactionId;
	}

	private void SetTransactionId(string value)
	{
		transactionId = value;
	}

	public string GetPayload()
	{
		return payload;
	}

	private void SetPayload(string value)
	{
		payload = value;
	}

	public string GetJson()
	{
		return json;
	}

	private void SetJson(string value)
	{
		json = value;
	}

	public string GetSignature()
	{
		return signature;
	}

	private void SetSignature(string value)
	{
		signature = value;
	}

	public ProductDefinition GetDefinition()
	{
		return product.definition;
	}

	public string GetProductId()
	{
		return GetDefinition().id;
	}

	public bool GetIsFake()
	{
		return GetStore().ToLower() == "fake";
	}
}
