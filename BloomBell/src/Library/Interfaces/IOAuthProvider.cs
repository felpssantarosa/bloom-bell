public interface IOAuthProvider
{
    void Authenticate(string userId);
    void AuthCompletedHandler(string provider);
}
