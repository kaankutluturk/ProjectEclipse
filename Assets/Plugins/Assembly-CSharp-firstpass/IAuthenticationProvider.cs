public interface IAuthenticationProvider
{
	bool RequiresPreAuth { get; }

	event OnAuthenticationSuccededDelegate AuthenticationSucceeded;

	event OnAuthenticationFailedDelegate AuthenticationFailed;

	bool GetIsPreAuthRequired();

	void AddAuthenticationSucceeded(OnAuthenticationSuccededDelegate value);

	void RemoveAuthenticationSucceeded(OnAuthenticationSuccededDelegate value);

	void AddAuthenticationFailed(OnAuthenticationFailedDelegate value);

	void RemoveAuthenticationFailed(OnAuthenticationFailedDelegate value);

	void StartAuthentication();

	void PrepareRequest(HTTPRequest request, SignalRRequestType requestType);
}
