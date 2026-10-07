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

	public static void Init(IPurchaseVerifier IHLKACMLEGK)
	{
		verifier = IHLKACMLEGK;
	}

	public static void VerifyPurchase(PurchaseReceipt HIKIPCMNPDK)
	{
		VerifyPurchase(HIKIPCMNPDK.GetProductId(), HIKIPCMNPDK.GetTransactionId(), HIKIPCMNPDK.GetJson(), HIKIPCMNPDK.GetSignature());
	}

	public static void VerifyPurchase(string FDKNIPNGFNF, string BGMLFNGKDHI, string DNHKNDPBGNM, string BGLGHEMMANM)
	{
		Debug.LogFormat("Verify: productId={0}, paymentId={1}, receipt={2}, signature={3}", FDKNIPNGFNF, BGMLFNGKDHI, DNHKNDPBGNM, (BGLGHEMMANM == null) ? "null" : BGLGHEMMANM);
		PaymentInfo jLDHCFFAIPK = null;
		if (!verificationEnabled || BGMLFNGKDHI == null || PaymentManager.GetIsEmulator())
		{
			jLDHCFFAIPK = PaymentManager.AddUnverifiedCompletedPayment(FDKNIPNGFNF, BGMLFNGKDHI, DNHKNDPBGNM, BGLGHEMMANM);
			if (!PaymentManager.GetIsEmulator())
			{
				PaymentManager.GetStore().ConfirmPendingPurchase(FDKNIPNGFNF);
			}
			PaymentManager.GetStore().FinishTransaction(jLDHCFFAIPK);
		}
		else
		{
			jLDHCFFAIPK = PaymentManager.AddInProgressPayment(FDKNIPNGFNF, BGMLFNGKDHI, DNHKNDPBGNM, BGLGHEMMANM);
			SendVerifyRequest(jLDHCFFAIPK);
		}
	}

	public static void VerifyPayment(PaymentInfo PAENLDALDGB)
	{
		if (!verificationEnabled)
		{
			PaymentManager.GetStore().ConfirmPendingPurchase(PAENLDALDGB.GetProductId());
			PaymentManager.GetStore().FinishTransaction(PAENLDALDGB);
			PaymentManager.CompletePayment(PAENLDALDGB);
		}
		SendVerifyRequest(PAENLDALDGB);
	}

	public static void ConfirmPayment(PaymentInfo PAENLDALDGB)
	{
		SendConfirmRequest(PAENLDALDGB);
	}

	private static void SendVerifyRequest(PaymentInfo PAENLDALDGB)
	{
		if (SystemProperties.IsIosPlatform())
		{
			verifier.VerifyPurchaseAction(PAENLDALDGB, "iOS", OnVerifyResponse);
		}
		else if (SystemProperties.IsAndroidPlatform())
		{
			verifier.VerifyPurchaseAction(PAENLDALDGB, "Android", OnVerifyResponse);
		}
		if (PAENLDALDGB != null)
		{
			PaymentManager.GetStore().NotifyVerificationStarted(PAENLDALDGB.GetProductId(), PAENLDALDGB.GetReceipt());
		}
	}

	private static void OnVerifyResponse(bool AMKKLMOONEP, string GHDPPHAAPCA, object JHJDJOFPHPH)
	{
		Debug.Log("VerifyRequest_Response");
		Debug.Log((!AMKKLMOONEP) ? "Result fail" : "Result ok!");
		Debug.Log(GHDPPHAAPCA);
		PaymentInfo jLDHCFFAIPK = (PaymentInfo)JHJDJOFPHPH;
		if (!AMKKLMOONEP)
		{
			if (jLDHCFFAIPK.VerificationAttempts <= 2)
			{
				jLDHCFFAIPK.VerificationAttempts++;
				SendVerifyRequest(jLDHCFFAIPK);
			}
			else
			{
				jLDHCFFAIPK.VerificationAttempts = 0;
				PaymentManager.GetStore().NotifyVerificationFailed(jLDHCFFAIPK.GetProductId());
				PaymentManager.GetStore().NotifyVerificationFinished(jLDHCFFAIPK.GetProductId(), jLDHCFFAIPK.GetReceipt());
			}
			return;
		}
		JSONNode jSONNode = JSONNode.Parse(GHDPPHAAPCA);
		switch (jSONNode["status"].AsInt)
		{
		case 0:
			if (PaymentManager.IsUnknownPayment(jLDHCFFAIPK))
			{
				jLDHCFFAIPK.VerificationAttempts = 0;
				jLDHCFFAIPK.SetIsCheating(true);
				PaymentManager.GetStore().NotifyPurchaseRejected(jLDHCFFAIPK.GetProductId());
				break;
			}
			jLDHCFFAIPK.SetPurchaseDate(jSONNode["data"]["receiptPurchaseDate"].Value);
			PaymentManager.GetStore().ConfirmPendingPurchase(jLDHCFFAIPK.GetProductId());
			PaymentManager.GetStore().FinishTransaction(jLDHCFFAIPK);
			PaymentManager.MarkPaymentVerified(jLDHCFFAIPK);
			if (!jSONNode["data"]["confirmed"].AsBool)
			{
				jLDHCFFAIPK.VerificationAttempts = 0;
				SendConfirmRequest(jLDHCFFAIPK);
			}
			else
			{
				PaymentManager.ConfirmPayment(jLDHCFFAIPK);
				Debug.Log("Payment doesn't need to be confirm! " + jLDHCFFAIPK.ToString());
			}
			break;
		case 1:
			jLDHCFFAIPK.SetIsCheating(true);
			PaymentManager.MarkPaymentFailed(jLDHCFFAIPK);
			PaymentManager.GetStore().ConfirmPendingPurchase(jLDHCFFAIPK.GetProductId());
			PaymentManager.GetStore().NotifyPurchaseRejected(jLDHCFFAIPK.GetProductId());
			break;
		}
		PaymentManager.GetStore().NotifyVerificationFinished(jLDHCFFAIPK.GetProductId(), jLDHCFFAIPK.GetReceipt());
	}

	private static void SendConfirmRequest(PaymentInfo PAENLDALDGB)
	{
		if (SystemProperties.IsIosPlatform())
		{
			verifier.ConfirmVerificationAction(PAENLDALDGB, "iOS", OnConfirmResponse);
		}
		else if (SystemProperties.IsAndroidPlatform())
		{
			verifier.ConfirmVerificationAction(PAENLDALDGB, "Android", OnConfirmResponse);
		}
		if (PAENLDALDGB != null)
		{
			PaymentManager.GetStore().NotifyConfirmationStarted(PAENLDALDGB.GetProductId(), PAENLDALDGB.GetReceipt());
		}
	}

	private static void OnConfirmResponse(bool AMKKLMOONEP, string GHDPPHAAPCA, object JHJDJOFPHPH)
	{
		Debug.Log("CompletedRequest_Response");
		Debug.Log((!AMKKLMOONEP) ? "Result fail" : "Result ok!");
		Debug.Log(GHDPPHAAPCA);
		PaymentInfo jLDHCFFAIPK = (PaymentInfo)JHJDJOFPHPH;
		if (!AMKKLMOONEP)
		{
			if (jLDHCFFAIPK.VerificationAttempts <= 2)
			{
				jLDHCFFAIPK.VerificationAttempts++;
				SendConfirmRequest(jLDHCFFAIPK);
			}
			else
			{
				jLDHCFFAIPK.VerificationAttempts = 0;
				PaymentManager.GetStore().NotifyConfirmationFinished(jLDHCFFAIPK.GetProductId(), jLDHCFFAIPK.GetReceipt());
			}
		}
		else
		{
			PaymentManager.ConfirmPayment(jLDHCFFAIPK);
			PaymentManager.GetStore().NotifyConfirmationFinished(jLDHCFFAIPK.GetProductId(), jLDHCFFAIPK.GetReceipt());
		}
	}
}
