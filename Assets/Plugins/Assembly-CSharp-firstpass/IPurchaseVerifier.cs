using System;

public interface IPurchaseVerifier
{
	void VerifyPurchaseAction(PaymentInfo paymentInfo, string platform, Action<bool, string, object> p_delegate);

	void ConfirmVerificationAction(PaymentInfo paymentInfo, string platform, Action<bool, string, object> p_delegate);
}
