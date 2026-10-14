using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace Trillix.Modules.Identity
{
    public class Argon2PasswordService : IPasswordService
    {
        // OWASP minimum for Argon2id: 19 MiB memory, 2 iterations, 1 lane
        private const int MemoryKb = 19456, Iterations = 2, Parallelism = 1;
        private const int SaltSize = 16, HashSize = 32;

        public string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Compute(password, salt, MemoryKb, Iterations, Parallelism, HashSize);
            return $"argon2id$m={MemoryKb},t={Iterations},p={Parallelism}" +
                   $"${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public bool Verify(string password, string storedHash)
        {
            try
            {
                var parts = storedHash.Split('$');
                if (parts.Length != 4 || parts[0] != "argon2id") return false;

                var p = parts[1].Split(',').Select(x => x.Split('='))
                                .ToDictionary(x => x[0], x => int.Parse(x[1]));
                var salt = Convert.FromBase64String(parts[2]);
                var expected = Convert.FromBase64String(parts[3]);

                var actual = Compute(password, salt, p["m"], p["t"], p["p"], expected.Length);
                return CryptographicOperations.FixedTimeEquals(expected, actual);  // constant-time compare
            }
            catch (Exception ex) when (ex is FormatException or KeyNotFoundException or IndexOutOfRangeException)
            {
                return false;
            }
        }

        private static byte[] Compute(string password, byte[] salt, int memoryKb, int iterations, int parallelism, int length)
        {
            using var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                MemorySize = memoryKb,
                Iterations = iterations,
                DegreeOfParallelism = parallelism
            };
            return argon.GetBytes(length);
        }
    }
}
