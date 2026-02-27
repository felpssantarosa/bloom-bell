namespace BloomBell.src.Domain.Events.Auth;

public sealed class AuthStateChangedEvent(string provider, AuthState state)
{
    public string Provider { get; } = provider;
    public AuthState State { get; } = state;
}
