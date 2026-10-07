using System.Collections.Generic;
using System.Xml;
using SimpleJSON;

public class PaymentOrdersManager : ITransactionChangeListener
{
	public void OnTransactionsChanged(bool AJAJBBKANGD)
	{
		ListSF.GetInstance().HandleAuthenticateResult(AJAJBBKANGD);
	}

	public bool LoadLegacyPaymentOrders(XmlAttribute GICKLJAIHFC)
	{
		if (GICKLJAIHFC == null)
		{
			return false;
		}
		ParseLegacyPaymentOrders(GICKLJAIHFC.Value);
		GICKLJAIHFC.OwnerElement.RemoveAttributeNode(GICKLJAIHFC);
		OnTransactionsChanged(true);
		return true;
	}

	private void ParseLegacyPaymentOrders(string GHDPPHAAPCA)
	{
		List<PaymentInfo> list = PaymentManager.GetInProgressPayments();
		List<PaymentInfo> list2 = PaymentManager.GetCompletedPayments();
		list.Clear();
		list2.Clear();
		JSONNode jSONNode = JSON.Parse(GHDPPHAAPCA);
		bool NPEJDEBKFDA = false;
		foreach (JSONClass child in jSONNode.Children)
		{
			PaymentInfo item = ParsePaymentFromJson(child, ref NPEJDEBKFDA);
			if (NPEJDEBKFDA)
			{
				list.Add(item);
			}
			else
			{
				list2.Add(item);
			}
		}
	}

	public void LoadFromXml(XmlNode MEEAKLDGLDF)
	{
		List<PaymentInfo> list = PaymentManager.GetInProgressPayments();
		List<PaymentInfo> list2 = PaymentManager.GetCompletedPayments();
		list.Clear();
		list2.Clear();
		XmlNode xmlNode = MEEAKLDGLDF["InProgress"];
		XmlNode xmlNode2 = MEEAKLDGLDF["Completed"];
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

	public void SaveToXml(XmlNode MEEAKLDGLDF)
	{
		List<PaymentInfo> list = PaymentManager.GetInProgressPayments();
		List<PaymentInfo> list2 = PaymentManager.GetCompletedPayments();
		MEEAKLDGLDF.RemoveAll();
		if (list.Count > 0)
		{
			XmlNode xmlNode = MEEAKLDGLDF["InProgress"] ?? MEEAKLDGLDF.AppendElement("InProgress");
			xmlNode.RemoveAll();
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				WriteInProgressPayment(list[i], xmlNode);
			}
		}
		if (list2.Count > 0)
		{
			XmlNode xmlNode2 = MEEAKLDGLDF["Completed"] ?? MEEAKLDGLDF.AppendElement("Completed");
			xmlNode2.RemoveAll();
			int j = 0;
			for (int count2 = list2.Count; j < count2; j++)
			{
				WriteCompletedPayment(list2[j], xmlNode2);
			}
		}
	}

	private static PaymentInfo ParsePaymentFromJson(JSONClass MEEAKLDGLDF, ref bool NPEJDEBKFDA)
	{
		string bGMLFNGKDHI = ((!MEEAKLDGLDF.HasValue("orderID")) ? string.Empty : MEEAKLDGLDF["orderID"].Value);
		string oDJCLFJHKFP = ((!MEEAKLDGLDF.HasValue("productID")) ? string.Empty : MEEAKLDGLDF["productID"].Value);
		string dNHKNDPBGNM = ((!MEEAKLDGLDF.HasValue("receipt")) ? string.Empty : MEEAKLDGLDF["receipt"].Value);
		string text = ((!MEEAKLDGLDF.HasValue("dataSignature")) ? null : MEEAKLDGLDF["dataSignature"].Value);
		text = ((text != null || !MEEAKLDGLDF.HasValue("data") || !MEEAKLDGLDF["data"].HasValue("signature")) ? null : MEEAKLDGLDF["data"]["signature"].Value);
		string pPJBKHKCONC = ((!MEEAKLDGLDF.HasValue("data") || !MEEAKLDGLDF["data"].HasValue("receiptPurchaseDate")) ? string.Empty : MEEAKLDGLDF["data"]["receiptPurchaseDate"].Value);
		bool flag = MEEAKLDGLDF.HasValue("isConfirmed") && MEEAKLDGLDF["isConfirmed"].AsBool;
		bool flag2 = MEEAKLDGLDF.HasValue("isDelivered") && MEEAKLDGLDF["isDelivered"].AsBool;
		bool flag3 = MEEAKLDGLDF.HasValue("isRestore") && MEEAKLDGLDF["isRestore"].AsBool;
		bool flag4 = MEEAKLDGLDF.HasValue("isInProgress") && MEEAKLDGLDF["isInProgress"].AsBool;
		bool flag5 = MEEAKLDGLDF.HasValue("isVerificationFail") && MEEAKLDGLDF["isVerificationFail"].AsBool;
		PaymentInfo jLDHCFFAIPK;
		if (!flag && (flag4 || flag2))
		{
			jLDHCFFAIPK = ((!flag2) ? PaymentInfo.CreateNotVerified(oDJCLFJHKFP, bGMLFNGKDHI, dNHKNDPBGNM, text) : PaymentInfo.CreateVerified(oDJCLFJHKFP, bGMLFNGKDHI, dNHKNDPBGNM, text, pPJBKHKCONC));
			NPEJDEBKFDA = true;
		}
		else
		{
			jLDHCFFAIPK = (flag2 ? PaymentInfo.CreateVerifiedConfirmed(oDJCLFJHKFP, bGMLFNGKDHI, dNHKNDPBGNM, text, pPJBKHKCONC) : ((!flag5) ? PaymentInfo.CreateNotVerifiedProcessed(oDJCLFJHKFP, bGMLFNGKDHI, dNHKNDPBGNM, text) : PaymentInfo.CreateVerificationFailed(oDJCLFJHKFP, bGMLFNGKDHI, dNHKNDPBGNM, text)));
			NPEJDEBKFDA = false;
		}
		jLDHCFFAIPK.SetIsCheating(false);
		return jLDHCFFAIPK;
	}

	private static PaymentInfo ParseInProgressPayment(XmlNode MEEAKLDGLDF)
	{
		string bGMLFNGKDHI = MEEAKLDGLDF.Attributes["Id"].GetStringOrDefault(string.Empty);
		string oDJCLFJHKFP = MEEAKLDGLDF.Attributes["ProductId"].GetStringOrDefault(string.Empty);
		string dNHKNDPBGNM = MEEAKLDGLDF.Attributes["Receipt"].GetStringOrDefault(string.Empty);
		string bGLGHEMMANM = MEEAKLDGLDF.Attributes["Signature"].GetStringOrDefault();
		string pPJBKHKCONC = MEEAKLDGLDF.Attributes["Date"].GetStringOrDefault(string.Empty);
		bool flag = MEEAKLDGLDF.Attributes["Verified"].ParseBool();
		bool bAINMLLIKOL = MEEAKLDGLDF.Attributes["Cheating"].ParseBool();
		PaymentInfo jLDHCFFAIPK = ((!flag) ? PaymentInfo.CreateNotVerified(oDJCLFJHKFP, bGMLFNGKDHI, dNHKNDPBGNM, bGLGHEMMANM) : PaymentInfo.CreateVerified(oDJCLFJHKFP, bGMLFNGKDHI, dNHKNDPBGNM, bGLGHEMMANM, pPJBKHKCONC));
		jLDHCFFAIPK.SetIsCheating(bAINMLLIKOL);
		return jLDHCFFAIPK;
	}

	private static PaymentInfo ParseCompletedPayment(XmlNode MEEAKLDGLDF)
	{
		string bGMLFNGKDHI = MEEAKLDGLDF.Attributes["Id"].GetStringOrDefault(string.Empty);
		string oDJCLFJHKFP = MEEAKLDGLDF.Attributes["ProductId"].GetStringOrDefault(string.Empty);
		string dNHKNDPBGNM = MEEAKLDGLDF.Attributes["Receipt"].GetStringOrDefault(string.Empty);
		string bGLGHEMMANM = MEEAKLDGLDF.Attributes["Signature"].GetStringOrDefault();
		string pPJBKHKCONC = MEEAKLDGLDF.Attributes["Date"].GetStringOrDefault(string.Empty);
		bool flag = MEEAKLDGLDF.Attributes["Verified"].ParseBool();
		bool bAINMLLIKOL = MEEAKLDGLDF.Attributes["Cheating"].ParseBool();
		PaymentInfo jLDHCFFAIPK = (flag ? PaymentInfo.CreateVerifiedConfirmed(oDJCLFJHKFP, bGMLFNGKDHI, dNHKNDPBGNM, bGLGHEMMANM, pPJBKHKCONC) : ((MEEAKLDGLDF.Attributes["VerificationFailed"] == null) ? PaymentInfo.CreateNotVerifiedProcessed(oDJCLFJHKFP, bGMLFNGKDHI, dNHKNDPBGNM, bGLGHEMMANM) : PaymentInfo.CreateVerificationFailed(oDJCLFJHKFP, bGMLFNGKDHI, dNHKNDPBGNM, bGLGHEMMANM)));
		jLDHCFFAIPK.SetIsCheating(bAINMLLIKOL);
		return jLDHCFFAIPK;
	}

	private static void WriteInProgressPayment(PaymentInfo PAENLDALDGB, XmlNode JIIIKGLGCBJ)
	{
		XmlElement xmlElement = JIIIKGLGCBJ.AppendElement("Payment");
		xmlElement.SetAttribute("Id", PAENLDALDGB.GetPaymentId());
		xmlElement.SetAttribute("ProductId", PAENLDALDGB.GetProductId());
		xmlElement.SetAttribute("Receipt", PAENLDALDGB.GetReceipt());
		xmlElement.SetAttribute("Verified", (!PAENLDALDGB.GetIsVerified()) ? "0" : "1");
		if (PAENLDALDGB.GetSignature() != null)
		{
			xmlElement.SetAttribute("Signature", PAENLDALDGB.GetSignature());
		}
		if (PAENLDALDGB.GetIsCheating())
		{
			xmlElement.SetAttribute("Cheating", "1");
		}
		if (PAENLDALDGB.GetIsVerified())
		{
			xmlElement.SetAttribute("Date", PAENLDALDGB.GetPurchaseDate());
		}
	}

	public void WriteCompletedPayment(PaymentInfo PAENLDALDGB, XmlNode JIIIKGLGCBJ)
	{
		XmlElement xmlElement = JIIIKGLGCBJ.AppendElement("Payment");
		xmlElement.SetAttribute("Id", PAENLDALDGB.GetPaymentId());
		xmlElement.SetAttribute("ProductId", PAENLDALDGB.GetProductId());
		xmlElement.SetAttribute("Receipt", PAENLDALDGB.GetReceipt());
		xmlElement.SetAttribute("Verified", (!PAENLDALDGB.GetIsVerified()) ? "0" : "1");
		if (PAENLDALDGB.GetSignature() != null)
		{
			xmlElement.SetAttribute("Signature", PAENLDALDGB.GetSignature());
		}
		if (PAENLDALDGB.GetIsCheating())
		{
			xmlElement.SetAttribute("Cheating", "1");
		}
		if (PAENLDALDGB.GetIsVerified())
		{
			xmlElement.SetAttribute("Date", PAENLDALDGB.GetPurchaseDate());
		}
		else if (PAENLDALDGB.GetIsVerificationFailed())
		{
			xmlElement.SetAttribute("VerificationFailed", "1");
		}
	}
}
