using System.Threading.Tasks;

public interface IOAuthProvider
{
    void Authenticate(string userId);
    void AuthCompletedHandler(string provider);
}
