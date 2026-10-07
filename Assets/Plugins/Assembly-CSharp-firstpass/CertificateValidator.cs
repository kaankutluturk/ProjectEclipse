using System.Text.RegularExpressions;

public class CertificateValidator
{
	private const string HttpsPrefix = "https://";

	private const string TrustedDomainPattern = "nekkimobile\\.ru";

	private const string TrustedUrlPattern = "^https://([^\\/]+\\.|)nekkimobile\\.ru(:[0-9]*)*(\\/([a-zA-Z0-9\\-\\.\\?\\,\\'\\/\\\\\\+&amp;%\\$#_]*)?|)$";

	public static bool IsTrustedUrl(string url)
	{
		return MatchesTrustedUrl(url);
	}

	private static bool MatchesTrustedUrl(string url)
	{
		return new Regex("^https://([^\\/]+\\.|)nekkimobile\\.ru(:[0-9]*)*(\\/([a-zA-Z0-9\\-\\.\\?\\,\\'\\/\\\\\\+&amp;%\\$#_]*)?|)$").Matches(url).Count == 1;
	}
}
