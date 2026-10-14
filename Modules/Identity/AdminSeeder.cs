using Trillix.Data;
using Microsoft.EntityFrameworkCore;

namespace Trillix.Modules.Identity
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordService>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

            if (await db.Admins.AnyAsync()) return;

            var email = config["SeedAdmin:Email"];
            var password = config["SeedAdmin:Password"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || password.Length < 12)
            {
                logger.LogWarning("No admin exists and SeedAdmin settings are missing or too weak (min 12 chars). Skipping seed.");
                return;
            }

            db.Admins.Add(new Admins
            {
                Email = email.Trim().ToLowerInvariant(),
                FullName = config["SeedAdmin:FullName"] ?? "Administrator",
                PasswordHash = hasher.Hash(password)
            });
            await db.SaveChangesAsync();
            logger.LogInformation("Initial admin {Email} created.", email);
        }
    }
}
