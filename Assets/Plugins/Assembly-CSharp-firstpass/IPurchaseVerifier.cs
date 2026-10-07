using System;

public interface IPurchaseVerifier
{
	void VerifyPurchaseAction(PaymentInfo PAENLDALDGB, string DBKFOHCPLDB, Action<bool, string, object> p_delegate);

	void ConfirmVerificationAction(PaymentInfo PAENLDALDGB, string DBKFOHCPLDB, Action<bool, string, object> p_delegate);
}
