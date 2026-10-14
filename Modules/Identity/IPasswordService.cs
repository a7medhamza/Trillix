namespace Trillix.Modules.Identity
{
    public interface IPasswordService
    {
        string Hash(string password);
        bool Verify(string password, string storedHash);
    }
}
