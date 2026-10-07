public class VerificationSettings
{
	private const string DEFAULT_VERIFICATION_URL = "https://95.213.140.21/verifyReceipt";

	private const int DEFAYUL_FREQUENCY = -1;

	public string Url;

	public int Timeout;

	public int MaxRetry;

	public int Frequency;

	public VerificationSettings(string KNPONKPJHFJ, int _timeout, int JAPFPFANIMO, int LIANOKFMGMB)
	{
		Url = KNPONKPJHFJ;
		Timeout = _timeout;
		MaxRetry = JAPFPFANIMO;
		Frequency = LIANOKFMGMB;
	}

	public VerificationSettings()
	{
		Url = "https://95.213.140.21/verifyReceipt";
		Timeout = 5000;
		MaxRetry = 2;
		Frequency = -1;
	}
}
