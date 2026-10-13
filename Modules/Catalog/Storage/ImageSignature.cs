namespace Trillix.Modules.Catalog.Storage
{
    // Modules/Catalog/Storage/ImageSignature.cs
    public static class ImageSignature
    {
        public static async Task<bool> IsValidAsync(Stream s, string ext, CancellationToken ct)
        {
            var h = new byte[12];
            var read = await s.ReadAtLeastAsync(h, h.Length, throwOnEndOfStream: false, ct);
            if (read < 12) return false;

            return ext switch
            {
                ".jpg" or ".jpeg" => h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF,
                ".png" => h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47,
                ".webp" => h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F'
                        && h[8] == 'W' && h[9] == 'E' && h[10] == 'B' && h[11] == 'P',
                _ => false
            };
        }
    }
}
