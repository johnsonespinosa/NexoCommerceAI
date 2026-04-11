using NexoCommerceAI.Application.Features.Products.Models;

namespace NexoCommerceAI.Application.Common.Interfaces;

public interface IImageStorageService
{
    Task<ImageUploadResult> UploadImageAsync(Stream imageStream, string fileName, string? folder = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default);
    Task<string> GetImageUrlAsync(string publicId, int? width = null, int? height = null);
}