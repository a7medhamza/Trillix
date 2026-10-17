namespace Trillix.Modules.Catalog
{
    // Modules/Catalog/ProductEndpoints.cs
    using System.Text.RegularExpressions;
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
            var admin = app.MapGroup("/api/admin/products").RequireAuthorization("Admin");

            admin.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
            {
                var products = await db.Products
                    .AsNoTracking()
                    .Where(p => !p.IsDeleted)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Slug,
                        p.Sku,
                        p.Price,
                        p.StockQuantity,
                        p.IsActive,
                        p.IsDeleted,
                        p.CreatedAt,
                        Images = p.Images
                            .OrderBy(i => i.SortOrder)
                            .Select(i => new { i.Id, i.FileName, i.IsPrimary, i.SortOrder })
                            .ToList()
                    })
                    .ToListAsync(ct);
                return Results.Ok(products);
            });

            admin.MapGet("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
            {
                var product = await db.Products
                    .AsNoTracking()
                    .Where(p => p.Id == id && !p.IsDeleted)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Slug,
                        p.Sku,
                        p.Description,
                        p.Price,
                        p.StockQuantity,
                        p.IsActive,
                        p.IsDeleted,
                        Images = p.Images
                            .OrderBy(i => i.SortOrder)
                            .Select(i => new { i.Id, i.FileName, i.IsPrimary, i.SortOrder })
                            .ToList()
                    })
                    .FirstOrDefaultAsync(ct);
                return product is null ? Results.NotFound() : Results.Ok(product);
            });

            admin.MapPut("/{id:guid}", async (
                Guid id, UpdateProductRequest body, AppDbContext db, CancellationToken ct) =>
            {
                if (string.IsNullOrWhiteSpace(body.Name) || string.IsNullOrWhiteSpace(body.Sku))
                    return Results.BadRequest("Name and SKU are required.");

                var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
                if (product is null) return Results.NotFound();

                var sku = body.Sku.Trim();
                if (await db.Products.AnyAsync(p => p.Sku == sku && p.Id != id, ct))
                    return Results.Conflict("SKU already exists.");

                var slug = string.IsNullOrWhiteSpace(body.Slug)
                    ? await UniqueSlugAsync(db, Slugify(body.Name), ct, id)
                    : body.Slug.Trim();

                if (await db.Products.AnyAsync(p => p.Slug == slug && p.Id != id, ct))
                    return Results.Conflict("Slug already exists.");

                product.Name = body.Name.Trim();
                product.Slug = slug;
                product.Description = string.IsNullOrWhiteSpace(body.Description) ? null : body.Description.Trim();
                product.Sku = sku;
                product.IsActive = body.IsActive;
                product.IsDeleted = body.IsDeleted;

                await db.SaveChangesAsync(ct);
                return Results.NoContent();
            });

            admin.MapPatch("/{id:guid}/inventory", async (
                Guid id, UpdateInventoryRequest body, AppDbContext db, CancellationToken ct) =>
            {
                if (body.Price < 0 || body.StockQuantity < 0)
                    return Results.BadRequest("Price and stock cannot be negative.");

                var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
                if (product is null) return Results.NotFound();

                product.Price = body.Price;
                product.StockQuantity = body.StockQuantity;
                await db.SaveChangesAsync(ct);
                return Results.NoContent();
            });

            admin.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
            {
                var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
                if (product is null) return Results.NotFound();

                product.IsDeleted = true;
                product.IsActive = false;
                await db.SaveChangesAsync(ct);
                return Results.NoContent();
            });

            admin.MapPost("/", async (CreateProductRequest body, AppDbContext db, CancellationToken ct) =>
            {
                if (string.IsNullOrWhiteSpace(body.Name) || string.IsNullOrWhiteSpace(body.Sku) || body.Price < 0)
                    return Results.BadRequest("Name and SKU are required; price cannot be negative.");

                var sku = body.Sku.Trim();
                if (await db.Products.AnyAsync(p => p.Sku == sku, ct))
                    return Results.Conflict("SKU already exists.");

                var slug = await UniqueSlugAsync(db, Slugify(body.Name), ct);
                var product = new Product
                {
                    Name = body.Name.Trim(),
                    Slug = slug,
                    Description = string.IsNullOrWhiteSpace(body.Description) ? null : body.Description.Trim(),
                    Sku = sku,
                    Price = body.Price,
                    StockQuantity = body.StockQuantity,
                    IsActive = true,
                    CreatedBy = Guid.Empty
                };
                db.Products.Add(product);
                await db.SaveChangesAsync(ct);
                return Results.Created($"/api/admin/products/{product.Id}", new { product.Id, product.Name, product.Slug, product.Sku });
            });

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
            }).DisableAntiforgery();
        }

        private static string Slugify(string name)
        {
            var slug = Regex.Replace(name.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
            return string.IsNullOrEmpty(slug) ? "product" : slug;
        }

        private static async Task<string> UniqueSlugAsync(
            AppDbContext db, string baseSlug, CancellationToken ct, Guid? excludeId = null)
        {
            var slug = baseSlug;
            var n = 0;
            while (await db.Products.AnyAsync(
                       p => p.Slug == slug && (!excludeId.HasValue || p.Id != excludeId), ct))
                slug = $"{baseSlug}-{++n}";
            return slug;
        }

        private sealed record CreateProductRequest(
            string? Name,
            string? Description,
            string? Sku,
            decimal Price,
            int StockQuantity);

        private sealed record UpdateProductRequest(
            string? Name,
            string? Description,
            string? Sku,
            string? Slug,
            bool IsActive,
            bool IsDeleted);

        private sealed record UpdateInventoryRequest(decimal Price, int StockQuantity);
    }
}
