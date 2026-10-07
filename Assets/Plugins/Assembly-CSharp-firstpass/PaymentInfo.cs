public class PaymentInfo
{
	private enum VerificationState
	{
		Verified = 0,
		NotVerified = 1,
		Failed = 2
	}

	public int VerificationAttempts;

	private VerificationState verificationState;

	private bool isConfirmed;

	private string productId;

	private string paymentId;

	private string receipt;

	private string signature;

	private string purchaseDate;

	private bool isCheating;

	public bool IsVerified
	{
		get
		{
			return GetIsVerified();
		}
	}

	public bool IsNotVerified
	{
		get
		{
			return GetIsNotVerified();
		}
	}

	public bool IsVerificationFailed
	{
		get
		{
			return GetIsVerificationFailed();
		}
	}

	public bool IsConfirmed
	{
		get
		{
			return GetIsConfirmed();
		}
	}

	public string ProductId
	{
		get
		{
			return GetProductId();
		}
	}

	public string PaymentId
	{
		get
		{
			return GetPaymentId();
		}
	}

	public string Receipt
	{
		get
		{
			return GetReceipt();
		}
	}

	public string FormattedReceipt
	{
		get
		{
			return GetFormattedReceipt();
		}
	}

	public string Signature
	{
		get
		{
			return GetSignature();
		}
	}

	public string PurchaseDate
	{
		get
		{
			return GetPurchaseDate();
		}
		set
		{
			SetPurchaseDate(value);
		}
	}

	public bool IsCheating
	{
		get
		{
			return GetIsCheating();
		}
		set
		{
			SetIsCheating(value);
		}
	}

	private PaymentInfo(string ODJCLFJHKFP, string BGMLFNGKDHI, string DNHKNDPBGNM)
	{
		productId = ODJCLFJHKFP;
		paymentId = BGMLFNGKDHI;
		receipt = DNHKNDPBGNM;
	}

	public bool GetIsVerified()
	{
		return verificationState == VerificationState.Verified;
	}

	public bool GetIsNotVerified()
	{
		return verificationState == VerificationState.NotVerified;
	}

	public bool GetIsVerificationFailed()
	{
		return verificationState == VerificationState.Failed;
	}

	public bool GetIsConfirmed()
	{
		return isConfirmed;
	}

	public string GetProductId()
	{
		return productId;
	}

	public string GetPaymentId()
	{
		return paymentId;
	}

	public string GetReceipt()
	{
		return receipt;
	}

	public string GetFormattedReceipt()
	{
		return FormatReceipt(receipt);
	}

	public string GetSignature()
	{
		return signature;
	}

	public string GetPurchaseDate()
	{
		return purchaseDate;
	}

	public void SetPurchaseDate(string value)
	{
		purchaseDate = value;
	}

	public bool GetIsCheating()
	{
		return isCheating;
	}

	public void SetIsCheating(bool value)
	{
		isCheating = value;
	}

	public static PaymentInfo CreateNotVerified(string ODJCLFJHKFP, string BGMLFNGKDHI, string DNHKNDPBGNM, string BGLGHEMMANM)
	{
		PaymentInfo jLDHCFFAIPK = new PaymentInfo(ODJCLFJHKFP, BGMLFNGKDHI, DNHKNDPBGNM);
		jLDHCFFAIPK.signature = BGLGHEMMANM;
		jLDHCFFAIPK.verificationState = VerificationState.NotVerified;
		jLDHCFFAIPK.isConfirmed = false;
		return jLDHCFFAIPK;
	}

	public static PaymentInfo CreateVerified(string ODJCLFJHKFP, string BGMLFNGKDHI, string DNHKNDPBGNM, string BGLGHEMMANM, string PPJBKHKCONC)
	{
		PaymentInfo jLDHCFFAIPK = new PaymentInfo(ODJCLFJHKFP, BGMLFNGKDHI, DNHKNDPBGNM);
		jLDHCFFAIPK.signature = BGLGHEMMANM;
		jLDHCFFAIPK.purchaseDate = PPJBKHKCONC;
		jLDHCFFAIPK.verificationState = VerificationState.Verified;
		jLDHCFFAIPK.isConfirmed = false;
		return jLDHCFFAIPK;
	}

	public static PaymentInfo CreateVerifiedConfirmed(string ODJCLFJHKFP, string BGMLFNGKDHI, string DNHKNDPBGNM, string BGLGHEMMANM, string PPJBKHKCONC)
	{
		PaymentInfo jLDHCFFAIPK = new PaymentInfo(ODJCLFJHKFP, BGMLFNGKDHI, DNHKNDPBGNM);
		jLDHCFFAIPK.signature = BGLGHEMMANM;
		jLDHCFFAIPK.purchaseDate = PPJBKHKCONC;
		jLDHCFFAIPK.verificationState = VerificationState.Verified;
		jLDHCFFAIPK.isConfirmed = true;
		return jLDHCFFAIPK;
	}

	public static PaymentInfo CreateNotVerifiedProcessed(string ODJCLFJHKFP, string BGMLFNGKDHI, string DNHKNDPBGNM, string BGLGHEMMANM)
	{
		PaymentInfo jLDHCFFAIPK = new PaymentInfo(ODJCLFJHKFP, BGMLFNGKDHI, DNHKNDPBGNM);
		jLDHCFFAIPK.signature = BGLGHEMMANM;
		jLDHCFFAIPK.verificationState = VerificationState.NotVerified;
		jLDHCFFAIPK.isConfirmed = false;
		return jLDHCFFAIPK;
	}

	public static PaymentInfo CreateVerificationFailed(string ODJCLFJHKFP, string BGMLFNGKDHI, string DNHKNDPBGNM, string BGLGHEMMANM)
	{
		PaymentInfo jLDHCFFAIPK = new PaymentInfo(ODJCLFJHKFP, BGMLFNGKDHI, DNHKNDPBGNM);
		jLDHCFFAIPK.signature = BGLGHEMMANM;
		jLDHCFFAIPK.verificationState = VerificationState.Failed;
		jLDHCFFAIPK.isConfirmed = false;
		return jLDHCFFAIPK;
	}

	public void MarkVerificationFailed()
	{
		verificationState = VerificationState.Failed;
		isConfirmed = false;
	}

	public void MarkVerified()
	{
		verificationState = VerificationState.Verified;
		isConfirmed = false;
	}

	public void MarkConfirmed()
	{
		isConfirmed = true;
	}

	public override string ToString()
	{
		return string.Format("[PaymentInfo: IsConfirmed={0}, ProductID={1}, PaymentID={2}, IsVerified={3}, IsVerificationFailed={4}, IsCheating={5}]", GetIsConfirmed(), GetProductId(), GetPaymentId(), GetIsVerified(), GetIsVerificationFailed(), GetIsCheating());
	}

	public static string FormatReceipt(string DNHKNDPBGNM)
	{
		return DNHKNDPBGNM;
	}
}
