using Trillix.Common;

namespace Trillix.Modules.Catalog
{
    // Modules/Catalog/ProductImage.cs
    public class ProductImage : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public string FileName { get; set; } = "";          // generated name on disk
        public string OriginalFileName { get; set; } = "";  // what the admin uploaded
        public string ContentType { get; set; } = "";
        public long SizeBytes { get; set; }
        public int SortOrder { get; set; }
        public bool IsPrimary { get; set; }
    }
}
