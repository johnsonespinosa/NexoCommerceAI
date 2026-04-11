namespace NexoCommerceAI.Application.Features.Products.Models;

public record ImageUploadResult(
    string PublicId,
    string Url,
    string Format,
    long Size,
    int Width,
    int Height)
{
    // Validaciones en el constructor primario
    public string PublicId { get; } = 
        string.IsNullOrWhiteSpace(PublicId) 
            ? throw new ArgumentException("PublicId cannot be empty", nameof(PublicId)) 
            : PublicId;
    
    public string Url { get; } = 
        !Uri.IsWellFormedUriString(Url, UriKind.Absolute)
            ? throw new ArgumentException("Invalid URL format", nameof(Url))
            : Url;
    
    public string Format { get; init; } = 
        !IsValidFormat(Format)
            ? throw new ArgumentException($"Invalid image format: {Format}", nameof(Format))
            : Format;
    
    public long Size { get; init; } = 
        Size <= 0 
            ? throw new ArgumentException("Size must be greater than zero", nameof(Size))
            : Size;
    
    public int Width { get; init; } = 
        Width <= 0 
            ? throw new ArgumentException("Width must be greater than zero", nameof(Width))
            : Width;
    
    public int Height { get; init; } = 
        Height <= 0 
            ? throw new ArgumentException("Height must be greater than zero", nameof(Height))
            : Height;
    
    private static bool IsValidFormat(string format)
    {
        var allowedFormats = new[] { "jpg", "jpeg", "png", "webp", "gif", "bmp" };
        return allowedFormats.Contains(format.ToLowerInvariant());
    }
}