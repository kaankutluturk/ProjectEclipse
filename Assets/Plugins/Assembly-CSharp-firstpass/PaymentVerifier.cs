using SimpleJSON;
using UnityEngine;

public class PaymentVerifier
{
	private const string PlatformIos = "iOS";

	private const string PlatformAndroid = "Android";

	private const int StatusVerified = 0;

	private const int StatusVerificationFailed = 1;

	private const int StatusServerError = 2;

	private static bool verificationEnabled = true;

	private static IPurchaseVerifier verifier;

	public static bool VerificationEnabled
	{
		get
		{
			return GetVerificationEnabled();
		}
		set
		{
			SetVerificationEnabled(value);
		}
	}

	public static bool GetVerificationEnabled()
	{
		return verificationEnabled;
	}

	public static void SetVerificationEnabled(bool value)
	{
		verificationEnabled = value;
	}

	public static void Init(IPurchaseVerifier purchaseVerifier)
	{
		verifier = purchaseVerifier;
	}

	public static void VerifyPurchase(PurchaseReceipt purchaseReceipt)
	{
		VerifyPurchase(purchaseReceipt.GetProductId(), purchaseReceipt.GetTransactionId(), purchaseReceipt.GetJson(), purchaseReceipt.GetSignature());
	}

	public static void VerifyPurchase(string productId, string paymentId, string receipt, string signature)
	{
		Debug.LogFormat("Verify: productId={0}, paymentId={1}, receipt={2}, signature={3}", productId, paymentId, receipt, (signature == null) ? "null" : signature);
		PaymentInfo paymentInfo = null;
		if (!verificationEnabled || paymentId == null || PaymentManager.GetIsEmulator())
		{
			paymentInfo = PaymentManager.AddUnverifiedCompletedPayment(productId, paymentId, receipt, signature);
			if (!PaymentManager.GetIsEmulator())
			{
				PaymentManager.GetStore().ConfirmPendingPurchase(productId);
			}
			PaymentManager.GetStore().FinishTransaction(paymentInfo);
		}
		else
		{
			paymentInfo = PaymentManager.AddInProgressPayment(productId, paymentId, receipt, signature);
			SendVerifyRequest(paymentInfo);
		}
	}

	public static void VerifyPayment(PaymentInfo payment)
	{
		if (!verificationEnabled)
		{
			PaymentManager.GetStore().ConfirmPendingPurchase(payment.GetProductId());
			PaymentManager.GetStore().FinishTransaction(payment);
			PaymentManager.CompletePayment(payment);
		}
		SendVerifyRequest(payment);
	}

	public static void ConfirmPayment(PaymentInfo payment)
	{
		SendConfirmRequest(payment);
	}

	private static void SendVerifyRequest(PaymentInfo payment)
	{
		if (SystemProperties.IsIosPlatform())
		{
			verifier.VerifyPurchaseAction(payment, "iOS", OnVerifyResponse);
		}
		else if (SystemProperties.IsAndroidPlatform())
		{
			verifier.VerifyPurchaseAction(payment, "Android", OnVerifyResponse);
		}
		if (payment != null)
		{
			PaymentManager.GetStore().NotifyVerificationStarted(payment.GetProductId(), payment.GetReceipt());
		}
	}

	private static void OnVerifyResponse(bool isSuccess, string response, object paymentInfoObject)
	{
		Debug.Log("VerifyRequest_Response");
		Debug.Log((!isSuccess) ? "Result fail" : "Result ok!");
		Debug.Log(response);
		PaymentInfo paymentInfo = (PaymentInfo)paymentInfoObject;
		if (!isSuccess)
		{
			if (paymentInfo.VerificationAttempts <= 2)
			{
				paymentInfo.VerificationAttempts++;
				SendVerifyRequest(paymentInfo);
			}
			else
			{
				paymentInfo.VerificationAttempts = 0;
				PaymentManager.GetStore().NotifyVerificationFailed(paymentInfo.GetProductId());
				PaymentManager.GetStore().NotifyVerificationFinished(paymentInfo.GetProductId(), paymentInfo.GetReceipt());
			}
			return;
		}
		JSONNode jSONNode = JSONNode.Parse(response);
		switch (jSONNode["status"].AsInt)
		{
		case 0:
			if (PaymentManager.IsUnknownPayment(paymentInfo))
			{
				paymentInfo.VerificationAttempts = 0;
				paymentInfo.SetIsCheating(true);
				PaymentManager.GetStore().NotifyPurchaseRejected(paymentInfo.GetProductId());
				break;
			}
			paymentInfo.SetPurchaseDate(jSONNode["data"]["receiptPurchaseDate"].Value);
			PaymentManager.GetStore().ConfirmPendingPurchase(paymentInfo.GetProductId());
			PaymentManager.GetStore().FinishTransaction(paymentInfo);
			PaymentManager.MarkPaymentVerified(paymentInfo);
			if (!jSONNode["data"]["confirmed"].AsBool)
			{
				paymentInfo.VerificationAttempts = 0;
				SendConfirmRequest(paymentInfo);
			}
			else
			{
				PaymentManager.ConfirmPayment(paymentInfo);
				Debug.Log("Payment doesn't need to be confirm! " + paymentInfo.ToString());
			}
			break;
		case 1:
			paymentInfo.SetIsCheating(true);
			PaymentManager.MarkPaymentFailed(paymentInfo);
			PaymentManager.GetStore().ConfirmPendingPurchase(paymentInfo.GetProductId());
			PaymentManager.GetStore().NotifyPurchaseRejected(paymentInfo.GetProductId());
			break;
		}
		PaymentManager.GetStore().NotifyVerificationFinished(paymentInfo.GetProductId(), paymentInfo.GetReceipt());
	}

	private static void SendConfirmRequest(PaymentInfo payment)
	{
		if (SystemProperties.IsIosPlatform())
		{
			verifier.ConfirmVerificationAction(payment, "iOS", OnConfirmResponse);
		}
		else if (SystemProperties.IsAndroidPlatform())
		{
			verifier.ConfirmVerificationAction(payment, "Android", OnConfirmResponse);
		}
		if (payment != null)
		{
			PaymentManager.GetStore().NotifyConfirmationStarted(payment.GetProductId(), payment.GetReceipt());
		}
	}

	private static void OnConfirmResponse(bool isSuccess, string response, object paymentInfoObject)
	{
		Debug.Log("CompletedRequest_Response");
		Debug.Log((!isSuccess) ? "Result fail" : "Result ok!");
		Debug.Log(response);
		PaymentInfo paymentInfo = (PaymentInfo)paymentInfoObject;
		if (!isSuccess)
		{
			if (paymentInfo.VerificationAttempts <= 2)
			{
				paymentInfo.VerificationAttempts++;
				SendConfirmRequest(paymentInfo);
			}
			else
			{
				paymentInfo.VerificationAttempts = 0;
				PaymentManager.GetStore().NotifyConfirmationFinished(paymentInfo.GetProductId(), paymentInfo.GetReceipt());
			}
		}
		else
		{
			PaymentManager.ConfirmPayment(paymentInfo);
			PaymentManager.GetStore().NotifyConfirmationFinished(paymentInfo.GetProductId(), paymentInfo.GetReceipt());
		}
	}
}
