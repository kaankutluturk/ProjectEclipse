public interface IHubMessage
{
	ulong InvocationIdValue { get; }

	ulong GetInvocationId();
}
