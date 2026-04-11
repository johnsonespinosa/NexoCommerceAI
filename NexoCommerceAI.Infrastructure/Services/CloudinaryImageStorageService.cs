using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Common.Settings;
using ImageUploadResult = NexoCommerceAI.Application.Features.Products.Models.ImageUploadResult;

namespace NexoCommerceAI.Infrastructure.Services;

public class CloudinaryImageStorageService : IImageStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;
    
    public CloudinaryImageStorageService(IOptions<CloudinarySettings> settings)
    {
        _settings = settings.Value;
        
        var account = new Account(
            _settings.CloudName,
            _settings.ApiKey,
            _settings.ApiSecret);
        
        _cloudinary = new Cloudinary(account);
    }
    
    [Obsolete("Obsolete")]
    public async Task<ImageUploadResult> UploadImageAsync(Stream imageStream, string fileName, string? folder = null, CancellationToken cancellationToken = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, imageStream),
            Folder = folder ?? _settings.DefaultFolder,
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };
        
        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
        
        return new ImageUploadResult
        (
            PublicId: result.PublicId,
            Url: result.SecureUrl.ToString(),
            Format: result.Format,
            Size: result.Length,
            Width: result.Width,
            Height: result.Height
        );
    }
    
    public async Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default)
    {
        var deleteParams = new DeletionParams(publicId);
        // Corregido: DestroyAsync solo recibe deleteParams, no cancellationToken
        var result = await _cloudinary.DestroyAsync(deleteParams);
        return result.Result == "ok";
    }
    
    public Task<string> GetImageUrlAsync(string publicId, int? width = null, int? height = null)
    {
        var transformation = new Transformation();
        
        if (width.HasValue)
            transformation.Width(width.Value);
        if (height.HasValue)
            transformation.Height(height.Value);
        
        transformation.Crop("fill");
        
        var url = _cloudinary.Api.UrlImgUp
            .Transform(transformation)
            .BuildUrl(publicId);
        
        return Task.FromResult(url);
    }
}