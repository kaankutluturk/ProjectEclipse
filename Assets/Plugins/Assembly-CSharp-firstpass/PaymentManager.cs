using System.Collections.Generic;
using SF2.Offline;

public static class PaymentManager
{
	private static PaymentStore store = new PaymentStore();

	private static ITransactionChangeListener listener;

	private static List<PaymentInfo> inProgressPayments = new List<PaymentInfo>();

	private static List<PaymentInfo> completedPayments = new List<PaymentInfo>();

	public static PaymentStore Store
	{
		get
		{
			return GetStore();
		}
	}

	public static List<PaymentInfo> InProgressPayments
	{
		get
		{
			return GetInProgressPayments();
		}
	}

	public static List<PaymentInfo> CompletedPayments
	{
		get
		{
			return GetCompletedPayments();
		}
	}

	public static bool HasVerifiedPayments
	{
		get
		{
			return GetHasVerifiedPayments();
		}
	}

	public static bool IsEmulator
	{
		get
		{
			return GetIsEmulator();
		}
	}

	public static PaymentStore GetStore()
	{
		return store;
	}

	public static void Init(ITransactionChangeListener ONDHILAOLIM, IPurchaseVerifier IHLKACMLEGK, ProductDefinition[] OCMDJBDPLJK, Dictionary<string, object> PCJAKPJMKGN = null)
	{
		listener = ONDHILAOLIM;
		// Keep UI subscriptions on the inert local facade; no store or verification service.
	}

	public static List<PaymentInfo> GetInProgressPayments()
	{
		return inProgressPayments;
	}

	public static List<PaymentInfo> GetCompletedPayments()
	{
		return completedPayments;
	}

	public static bool GetHasVerifiedPayments()
	{
		if (completedPayments.Count == 0)
		{
			return false;
		}
		foreach (PaymentInfo item in completedPayments)
		{
			if (item.GetIsVerified())
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetIsEmulator()
	{
		return false;
	}

	public static PaymentInfo AddInProgressPayment(string ODJCLFJHKFP, string BGMLFNGKDHI, string DNHKNDPBGNM, string BGLGHEMMANM)
	{
		PaymentInfo jLDHCFFAIPK = PaymentInfo.CreateNotVerified(ODJCLFJHKFP, BGMLFNGKDHI, DNHKNDPBGNM, BGLGHEMMANM);
		inProgressPayments.Add(jLDHCFFAIPK);
		listener.OnTransactionsChanged(true);
		return jLDHCFFAIPK;
	}

	public static PaymentInfo AddUnverifiedCompletedPayment(string ODJCLFJHKFP, string BGMLFNGKDHI, string DNHKNDPBGNM, string BGLGHEMMANM)
	{
		PaymentInfo jLDHCFFAIPK = PaymentInfo.CreateNotVerifiedProcessed(ODJCLFJHKFP, BGMLFNGKDHI, DNHKNDPBGNM, BGLGHEMMANM);
		completedPayments.Add(jLDHCFFAIPK);
		listener.OnTransactionsChanged(true);
		return jLDHCFFAIPK;
	}

	public static PaymentInfo AddConfirmedPayment(string ODJCLFJHKFP, string BGMLFNGKDHI, string DNHKNDPBGNM, string BGLGHEMMANM, string PPJBKHKCONC)
	{
		PaymentInfo jLDHCFFAIPK = PaymentInfo.CreateVerifiedConfirmed(ODJCLFJHKFP, BGMLFNGKDHI, DNHKNDPBGNM, BGLGHEMMANM, PPJBKHKCONC);
		completedPayments.Add(jLDHCFFAIPK);
		listener.OnTransactionsChanged(true);
		return jLDHCFFAIPK;
	}

	public static PaymentInfo AddFailedPayment(string ODJCLFJHKFP, string BGMLFNGKDHI, string DNHKNDPBGNM, string BGLGHEMMANM, bool BGBMBECEGFH)
	{
		PaymentInfo jLDHCFFAIPK = PaymentInfo.CreateVerificationFailed(ODJCLFJHKFP, BGMLFNGKDHI, DNHKNDPBGNM, BGLGHEMMANM);
		jLDHCFFAIPK.SetIsCheating(BGBMBECEGFH);
		completedPayments.Add(jLDHCFFAIPK);
		listener.OnTransactionsChanged(true);
		return jLDHCFFAIPK;
	}

	public static void ProcessPendingPayments()
	{
		// Preserve any recovered pending transactions without contacting a store
		// or verification backend when the payment UI opens.
	}

	public static void MarkPaymentFailed(PaymentInfo PAENLDALDGB)
	{
		inProgressPayments.Remove(PAENLDALDGB);
		PAENLDALDGB.MarkVerificationFailed();
		completedPayments.Add(PAENLDALDGB);
		listener.OnTransactionsChanged(true);
	}

	public static void MarkPaymentVerified(PaymentInfo PAENLDALDGB)
	{
		PAENLDALDGB.MarkVerified();
		listener.OnTransactionsChanged(true);
	}

	public static void CompletePayment(PaymentInfo PAENLDALDGB)
	{
		inProgressPayments.Remove(PAENLDALDGB);
		completedPayments.Add(PAENLDALDGB);
		listener.OnTransactionsChanged(true);
	}

	public static void ConfirmPayment(PaymentInfo PAENLDALDGB)
	{
		inProgressPayments.Remove(PAENLDALDGB);
		PAENLDALDGB.MarkConfirmed();
		completedPayments.Add(PAENLDALDGB);
		listener.OnTransactionsChanged(true);
	}

	public static bool IsUnknownPayment(PaymentInfo PAENLDALDGB)
	{
		for (int i = 0; i < inProgressPayments.Count; i++)
		{
			if (inProgressPayments[i].GetPaymentId() == PAENLDALDGB.GetPaymentId())
			{
				return false;
			}
		}
		return true;
	}

	public static bool HasInProgressPayment(string ODJCLFJHKFP, string BGMLFNGKDHI = null)
	{
		foreach (PaymentInfo item in inProgressPayments)
		{
			if (item.GetProductId() == ODJCLFJHKFP && (BGMLFNGKDHI == null || item.GetPaymentId() == BGMLFNGKDHI))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasCompletedPayment(string ODJCLFJHKFP, string BGMLFNGKDHI = null)
	{
		foreach (PaymentInfo item in completedPayments)
		{
			if (item.GetProductId() == ODJCLFJHKFP && (BGMLFNGKDHI == null || item.GetPaymentId() == BGMLFNGKDHI))
			{
				return true;
			}
		}
		return false;
	}
}
