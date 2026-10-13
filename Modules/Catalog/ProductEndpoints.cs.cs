namespace Trillix.Modules.Catalog
{
    // Modules/Catalog/ProductEndpoints.cs
    using Microsoft.EntityFrameworkCore;
    using Trillix.Data;
    using Trillix.Modules.Catalog.Storage;

    public static class ProductEndpoints
    {
        private static readonly Dictionary<string, string> AllowedTypes = new()
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

        public static void MapProductEndpoints(this IEndpointRouteBuilder app)
        {
            var admin = app.MapGroup("/api/admin/products");   // add .RequireAuthorization("Admin") later

            admin.MapPost("/{id:guid}/images", async (
                Guid id, IFormFile file, AppDbContext db, IImageStorage storage, CancellationToken ct) =>
            {
                var product = await db.Products.Include(p => p.Images)
                                               .FirstOrDefaultAsync(p => p.Id == id, ct);
                if (product is null) return Results.NotFound();

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (file.Length is 0 or > 5 * 1024 * 1024)
                    return Results.BadRequest("File must be between 1 byte and 5 MB.");
                if (!AllowedTypes.TryGetValue(ext, out var contentType))
                    return Results.BadRequest("Only JPG, PNG and WEBP are allowed.");

                await using var stream = file.OpenReadStream();
                if (!await ImageSignature.IsValidAsync(stream, ext, ct))
                    return Results.BadRequest("File is not a valid image.");
                stream.Position = 0;

                var fileName = await storage.SaveAsync(stream, ext, ct);
                db.ProductImages.Add(new ProductImage
                {
                    ProductId = id,
                    FileName = fileName,
                    OriginalFileName = Path.GetFileName(file.FileName),
                    ContentType = contentType,
                    SizeBytes = file.Length,
                    SortOrder = product.Images.Count,
                    IsPrimary = product.Images.Count == 0
                });
                await db.SaveChangesAsync(ct);

                var url = $"/media/products/{fileName}";
                return Results.Created(url, new { url });
            }).DisableAntiforgery();   // fine for JWT auth; configure antiforgery properly if you use cookies
        }
    }
}
