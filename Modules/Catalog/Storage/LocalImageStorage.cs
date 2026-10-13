namespace Trillix.Modules.Catalog.Storage
{
    // Modules/Catalog/Storage/LocalImageStorage.cs
    public class LocalImageStorage(string rootPath) : IImageStorage
    {
        public async Task<string> SaveAsync(Stream content, string extension, CancellationToken ct)
        {
            Directory.CreateDirectory(rootPath);
            var fileName = $"{Guid.NewGuid():N}{extension}";   // never trust the original name
            await using var fs = File.Create(Path.Combine(rootPath, fileName));
            await content.CopyToAsync(fs, ct);
            return fileName;
        }

        public Task DeleteAsync(string fileName, CancellationToken ct)
        {
            var path = Path.Combine(rootPath, Path.GetFileName(fileName));
            if (File.Exists(path)) File.Delete(path);
            return Task.CompletedTask;
        }
    }
}
