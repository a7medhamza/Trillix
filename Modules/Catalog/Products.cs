using Trillix.Common;

namespace Trillix.Modules.Catalog
{
    // Modules/Catalog/Product.cs
    public class Product : AuditableEntity
    {
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? Description { get; set; }
        public string Sku { get; set; } = "";
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;

        public byte[] RowVersion { get; set; } = [];   // optimistic concurrency
        public List<ProductImage> Images { get; set; } = new();
    }
}
