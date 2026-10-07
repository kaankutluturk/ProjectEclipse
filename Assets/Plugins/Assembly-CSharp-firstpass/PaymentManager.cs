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

	public static void Init(ITransactionChangeListener transactionListener, IPurchaseVerifier purchaseVerifier, ProductDefinition[] productDefinitions, Dictionary<string, object> options = null)
	{
		listener = transactionListener;
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

	public static PaymentInfo AddInProgressPayment(string productId, string paymentId, string receipt, string signature)
	{
		PaymentInfo paymentInfo = PaymentInfo.CreateNotVerified(productId, paymentId, receipt, signature);
		inProgressPayments.Add(paymentInfo);
		listener.OnTransactionsChanged(true);
		return paymentInfo;
	}

	public static PaymentInfo AddUnverifiedCompletedPayment(string productId, string paymentId, string receipt, string signature)
	{
		PaymentInfo paymentInfo = PaymentInfo.CreateNotVerifiedProcessed(productId, paymentId, receipt, signature);
		completedPayments.Add(paymentInfo);
		listener.OnTransactionsChanged(true);
		return paymentInfo;
	}

	public static PaymentInfo AddConfirmedPayment(string productId, string paymentId, string receipt, string signature, string purchaseDate)
	{
		PaymentInfo paymentInfo = PaymentInfo.CreateVerifiedConfirmed(productId, paymentId, receipt, signature, purchaseDate);
		completedPayments.Add(paymentInfo);
		listener.OnTransactionsChanged(true);
		return paymentInfo;
	}

	public static PaymentInfo AddFailedPayment(string productId, string paymentId, string receipt, string signature, bool isCheating)
	{
		PaymentInfo paymentInfo = PaymentInfo.CreateVerificationFailed(productId, paymentId, receipt, signature);
		paymentInfo.SetIsCheating(isCheating);
		completedPayments.Add(paymentInfo);
		listener.OnTransactionsChanged(true);
		return paymentInfo;
	}

	public static void ProcessPendingPayments()
	{
		// Preserve any recovered pending transactions without contacting a store
		// or verification backend when the payment UI opens.
	}

	public static void MarkPaymentFailed(PaymentInfo payment)
	{
		inProgressPayments.Remove(payment);
		payment.MarkVerificationFailed();
		completedPayments.Add(payment);
		listener.OnTransactionsChanged(true);
	}

	public static void MarkPaymentVerified(PaymentInfo payment)
	{
		payment.MarkVerified();
		listener.OnTransactionsChanged(true);
	}

	public static void CompletePayment(PaymentInfo payment)
	{
		inProgressPayments.Remove(payment);
		completedPayments.Add(payment);
		listener.OnTransactionsChanged(true);
	}

	public static void ConfirmPayment(PaymentInfo payment)
	{
		inProgressPayments.Remove(payment);
		payment.MarkConfirmed();
		completedPayments.Add(payment);
		listener.OnTransactionsChanged(true);
	}

	public static bool IsUnknownPayment(PaymentInfo payment)
	{
		for (int i = 0; i < inProgressPayments.Count; i++)
		{
			if (inProgressPayments[i].GetPaymentId() == payment.GetPaymentId())
			{
				return false;
			}
		}
		return true;
	}

	public static bool HasInProgressPayment(string productId, string paymentId = null)
	{
		foreach (PaymentInfo item in inProgressPayments)
		{
			if (item.GetProductId() == productId && (paymentId == null || item.GetPaymentId() == paymentId))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasCompletedPayment(string productId, string paymentId = null)
	{
		foreach (PaymentInfo item in completedPayments)
		{
			if (item.GetProductId() == productId && (paymentId == null || item.GetPaymentId() == paymentId))
			{
				return true;
			}
		}
		return false;
	}
}
