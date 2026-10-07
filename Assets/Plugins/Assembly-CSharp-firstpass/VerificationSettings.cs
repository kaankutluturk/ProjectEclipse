public class VerificationSettings
{
	private const string DEFAULT_VERIFICATION_URL = "https://95.213.140.21/verifyReceipt";

	private const int DEFAYUL_FREQUENCY = -1;

	public string Url;

	public int Timeout;

	public int MaxRetry;

	public int Frequency;

	public VerificationSettings(string url, int _timeout, int maxRetry, int frequency)
	{
		Url = url;
		Timeout = _timeout;
		MaxRetry = maxRetry;
		Frequency = frequency;
	}

	public VerificationSettings()
	{
		Url = "https://95.213.140.21/verifyReceipt";
		Timeout = 5000;
		MaxRetry = 2;
		Frequency = -1;
	}
}
