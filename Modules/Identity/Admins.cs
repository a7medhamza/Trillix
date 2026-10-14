using Trillix.Common;

namespace Trillix.Modules.Identity
{
    public class Admins : BaseEntity
    {

        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";   // never the plain password
        public bool IsActive { get; set; } = true;

    }
}
