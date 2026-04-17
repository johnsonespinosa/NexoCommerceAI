using System.Text;
using System.Text.RegularExpressions;

namespace NexoCommerceAI.Domain.Utilities;

public static class SlugGenerator
{
    public static string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Value is required", nameof(input));
        }

        var normalized = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            if (c is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                builder.Append(c);
                continue;
            }

            if (char.GetUnicodeCategory(c) is System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append('-');
        }

        var slug = Regex.Replace(builder.ToString(), "-{2,}", "-").Trim('-');
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException("Unable to generate slug from input", nameof(input));
        }

        return slug;
    }
}
