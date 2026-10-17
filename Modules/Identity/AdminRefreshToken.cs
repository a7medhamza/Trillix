using Trillix.Common;

namespace Trillix.Modules.Identity;

public class AdminRefreshToken : BaseEntity
{
    public Guid AdminId { get; set; }
    public Admins Admin { get; set; } = null!;
    public string TokenHash { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
