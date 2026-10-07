using System.Collections.Generic;
using System.Xml;
using SimpleJSON;

public class PaymentOrdersManager : ITransactionChangeListener
{
	public void OnTransactionsChanged(bool success)
	{
		ListSF.GetInstance().HandleAuthenticateResult(success);
	}

	public bool LoadLegacyPaymentOrders(XmlAttribute attribute)
	{
		if (attribute == null)
		{
			return false;
		}
		ParseLegacyPaymentOrders(attribute.Value);
		attribute.OwnerElement.RemoveAttributeNode(attribute);
		OnTransactionsChanged(true);
		return true;
	}

	private void ParseLegacyPaymentOrders(string json)
	{
		List<PaymentInfo> list = PaymentManager.GetInProgressPayments();
		List<PaymentInfo> list2 = PaymentManager.GetCompletedPayments();
		list.Clear();
		list2.Clear();
		JSONNode jSONNode = JSON.Parse(json);
		bool isInProgress = false;
		foreach (JSONClass child in jSONNode.Children)
		{
			PaymentInfo item = ParsePaymentFromJson(child, ref isInProgress);
			if (isInProgress)
			{
				list.Add(item);
			}
			else
			{
				list2.Add(item);
			}
		}
	}

	public void LoadFromXml(XmlNode node)
	{
		List<PaymentInfo> list = PaymentManager.GetInProgressPayments();
		List<PaymentInfo> list2 = PaymentManager.GetCompletedPayments();
		list.Clear();
		list2.Clear();
		XmlNode xmlNode = node["InProgress"];
		XmlNode xmlNode2 = node["Completed"];
		if (xmlNode != null)
		{
			foreach (XmlNode item2 in xmlNode)
			{
				PaymentInfo item = ParseInProgressPayment(item2);
				list.Add(item);
			}
		}
		if (xmlNode2 == null)
		{
			return;
		}
		foreach (XmlNode item3 in xmlNode2)
		{
			PaymentInfo item = ParseCompletedPayment(item3);
			list2.Add(item);
		}
	}

	public void SaveToXml(XmlNode node)
	{
		List<PaymentInfo> list = PaymentManager.GetInProgressPayments();
		List<PaymentInfo> list2 = PaymentManager.GetCompletedPayments();
		node.RemoveAll();
		if (list.Count > 0)
		{
			XmlNode xmlNode = node["InProgress"] ?? node.AppendElement("InProgress");
			xmlNode.RemoveAll();
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				WriteInProgressPayment(list[i], xmlNode);
			}
		}
		if (list2.Count > 0)
		{
			XmlNode xmlNode2 = node["Completed"] ?? node.AppendElement("Completed");
			xmlNode2.RemoveAll();
			int j = 0;
			for (int count2 = list2.Count; j < count2; j++)
			{
				WriteCompletedPayment(list2[j], xmlNode2);
			}
		}
	}

	private static PaymentInfo ParsePaymentFromJson(JSONClass paymentJson, ref bool isInProgress)
	{
		string orderId = ((!paymentJson.HasValue("orderID")) ? string.Empty : paymentJson["orderID"].Value);
		string productId = ((!paymentJson.HasValue("productID")) ? string.Empty : paymentJson["productID"].Value);
		string receipt = ((!paymentJson.HasValue("receipt")) ? string.Empty : paymentJson["receipt"].Value);
		string text = ((!paymentJson.HasValue("dataSignature")) ? null : paymentJson["dataSignature"].Value);
		text = ((text != null || !paymentJson.HasValue("data") || !paymentJson["data"].HasValue("signature")) ? null : paymentJson["data"]["signature"].Value);
		string purchaseDate = ((!paymentJson.HasValue("data") || !paymentJson["data"].HasValue("receiptPurchaseDate")) ? string.Empty : paymentJson["data"]["receiptPurchaseDate"].Value);
		bool flag = paymentJson.HasValue("isConfirmed") && paymentJson["isConfirmed"].AsBool;
		bool flag2 = paymentJson.HasValue("isDelivered") && paymentJson["isDelivered"].AsBool;
		bool flag3 = paymentJson.HasValue("isRestore") && paymentJson["isRestore"].AsBool;
		bool flag4 = paymentJson.HasValue("isInProgress") && paymentJson["isInProgress"].AsBool;
		bool flag5 = paymentJson.HasValue("isVerificationFail") && paymentJson["isVerificationFail"].AsBool;
		PaymentInfo payment;
		if (!flag && (flag4 || flag2))
		{
			payment = ((!flag2) ? PaymentInfo.CreateNotVerified(productId, orderId, receipt, text) : PaymentInfo.CreateVerified(productId, orderId, receipt, text, purchaseDate));
			isInProgress = true;
		}
		else
		{
			payment = (flag2 ? PaymentInfo.CreateVerifiedConfirmed(productId, orderId, receipt, text, purchaseDate) : ((!flag5) ? PaymentInfo.CreateNotVerifiedProcessed(productId, orderId, receipt, text) : PaymentInfo.CreateVerificationFailed(productId, orderId, receipt, text)));
			isInProgress = false;
		}
		payment.SetIsCheating(false);
		return payment;
	}

	private static PaymentInfo ParseInProgressPayment(XmlNode node)
	{
		string orderId = node.Attributes["Id"].GetStringOrDefault(string.Empty);
		string productId = node.Attributes["ProductId"].GetStringOrDefault(string.Empty);
		string receipt = node.Attributes["Receipt"].GetStringOrDefault(string.Empty);
		string signature = node.Attributes["Signature"].GetStringOrDefault();
		string purchaseDate = node.Attributes["Date"].GetStringOrDefault(string.Empty);
		bool flag = node.Attributes["Verified"].ParseBool();
		bool isCheating = node.Attributes["Cheating"].ParseBool();
		PaymentInfo payment = ((!flag) ? PaymentInfo.CreateNotVerified(productId, orderId, receipt, signature) : PaymentInfo.CreateVerified(productId, orderId, receipt, signature, purchaseDate));
		payment.SetIsCheating(isCheating);
		return payment;
	}

	private static PaymentInfo ParseCompletedPayment(XmlNode node)
	{
		string orderId = node.Attributes["Id"].GetStringOrDefault(string.Empty);
		string productId = node.Attributes["ProductId"].GetStringOrDefault(string.Empty);
		string receipt = node.Attributes["Receipt"].GetStringOrDefault(string.Empty);
		string signature = node.Attributes["Signature"].GetStringOrDefault();
		string purchaseDate = node.Attributes["Date"].GetStringOrDefault(string.Empty);
		bool flag = node.Attributes["Verified"].ParseBool();
		bool isCheating = node.Attributes["Cheating"].ParseBool();
		PaymentInfo payment = (flag ? PaymentInfo.CreateVerifiedConfirmed(productId, orderId, receipt, signature, purchaseDate) : ((node.Attributes["VerificationFailed"] == null) ? PaymentInfo.CreateNotVerifiedProcessed(productId, orderId, receipt, signature) : PaymentInfo.CreateVerificationFailed(productId, orderId, receipt, signature)));
		payment.SetIsCheating(isCheating);
		return payment;
	}

	private static void WriteInProgressPayment(PaymentInfo payment, XmlNode parent)
	{
		XmlElement xmlElement = parent.AppendElement("Payment");
		xmlElement.SetAttribute("Id", payment.GetPaymentId());
		xmlElement.SetAttribute("ProductId", payment.GetProductId());
		xmlElement.SetAttribute("Receipt", payment.GetReceipt());
		xmlElement.SetAttribute("Verified", (!payment.GetIsVerified()) ? "0" : "1");
		if (payment.GetSignature() != null)
		{
			xmlElement.SetAttribute("Signature", payment.GetSignature());
		}
		if (payment.GetIsCheating())
		{
			xmlElement.SetAttribute("Cheating", "1");
		}
		if (payment.GetIsVerified())
		{
			xmlElement.SetAttribute("Date", payment.GetPurchaseDate());
		}
	}

	public void WriteCompletedPayment(PaymentInfo payment, XmlNode parent)
	{
		XmlElement xmlElement = parent.AppendElement("Payment");
		xmlElement.SetAttribute("Id", payment.GetPaymentId());
		xmlElement.SetAttribute("ProductId", payment.GetProductId());
		xmlElement.SetAttribute("Receipt", payment.GetReceipt());
		xmlElement.SetAttribute("Verified", (!payment.GetIsVerified()) ? "0" : "1");
		if (payment.GetSignature() != null)
		{
			xmlElement.SetAttribute("Signature", payment.GetSignature());
		}
		if (payment.GetIsCheating())
		{
			xmlElement.SetAttribute("Cheating", "1");
		}
		if (payment.GetIsVerified())
		{
			xmlElement.SetAttribute("Date", payment.GetPurchaseDate());
		}
		else if (payment.GetIsVerificationFailed())
		{
			xmlElement.SetAttribute("VerificationFailed", "1");
		}
	}
}
