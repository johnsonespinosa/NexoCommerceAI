using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NexoCommerceAI.Domain.Common;

public static class SlugGenerator
{
    public static string Generate(string? input, bool lowerCase = true, bool preserveNumbers = true)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        
        var slug = input.Trim();
        
        // Convertir a minúsculas si es necesario
        if (lowerCase)
            slug = slug.ToLowerInvariant();
        
        // Remover acentos y caracteres diacríticos
        slug = RemoveDiacritics(slug);
        
        // Reemplazar caracteres especiales
        slug = ReplaceSpecialCharacters(slug);
        
        // Preservar o remover números según la configuración
        if (!preserveNumbers)
            slug = Regex.Replace(slug, @"[0-9]", "");
        
        // Reemplazar espacios y caracteres no válidos con guiones
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"[\s-]+", "-");
        
        // Remover guiones al inicio y final
        slug = slug.Trim('-');
        
        // Limitar longitud (opcional)
        const int maxLength = 200;
        if (slug.Length > maxLength)
            slug = slug[..maxLength].TrimEnd('-');
        
        // Evitar slugs vacíos
        if (string.IsNullOrWhiteSpace(slug))
            slug = "untitled";
        
        return slug;
    }
    
    public static string GenerateUnique(string? input, Func<string, bool> isSlugExists, int maxAttempts = 10)
    {
        var baseSlug = Generate(input);
        var slug = baseSlug;
        var attempt = 1;
        
        while (isSlugExists(slug) && attempt <= maxAttempts)
        {
            slug = $"{baseSlug}-{attempt}";
            attempt++;
        }
        
        if (attempt > maxAttempts)
            throw new InvalidOperationException($"Unable to generate unique slug after {maxAttempts} attempts");
        
        return slug;
    }
    
    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();
    
        foreach (var c in from c in normalizedString let unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c) where unicodeCategory != UnicodeCategory.NonSpacingMark select c)
        {
            stringBuilder.Append(c);
        }
    
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
    
    private static string ReplaceSpecialCharacters(string text)
    {
        var replacements = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["&"] = "and",
            ["@"] = "at",
            ["#"] = "sharp",
            ["%"] = "percent",
            ["+"] = "plus",
            ["="] = "equals",
            ["?"] = "",
            ["!"] = "",
            ["¿"] = "",
            ["¡"] = "",
            ["$"] = "dollar",
            ["€"] = "euro",
            ["£"] = "pound",
            ["¥"] = "yen",
            ["©"] = "copyright",
            ["®"] = "registered",
            ["™"] = "tm"
        };

        return replacements.Aggregate(text, (current, replacement) => current.Replace(replacement.Key, replacement.Value));
    }
}