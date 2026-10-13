namespace Trillix.Modules.Catalog.Storage
{
    // Modules/Catalog/Storage/IImageStorage.cs
    public interface IImageStorage
    {
        Task<string> SaveAsync(Stream content, string extension, CancellationToken ct);
        Task DeleteAsync(string fileName, CancellationToken ct);
    }
}
