//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Ratatosk;

internal static class FirebaseMessageValidator
{
    public static bool IsValidTopicName(string topicName)
        => !string.IsNullOrEmpty(topicName) && topicName.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');

    public static bool IsValidHexColor(string color)
        => !string.IsNullOrEmpty(color) && color.StartsWith('#') &&
           (color.Length == 7 || color.Length == 9) &&
           color[1..].All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));

    public static string? GetMessageProperty(IMessage message, string propertyName)
    {
        if (message.Properties?.TryGetValue(propertyName, out var property) == true)
            return property.Value?.ToString();
        return null;
    }

    public static IEnumerable<ValidationResult> ValidateImageUrl(object? value)
    {
        if (value == null) yield break;
        var url = value.ToString();
        if (string.IsNullOrEmpty(url)) yield break;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "http" && uri.Scheme != "https"))
        {
            yield return new ValidationResult(
                "ImageUrl must be a valid HTTP or HTTPS URL",
                new[] { "ImageUrl" });
        }
    }

    public static IEnumerable<ValidationResult> ValidateHexColor(object? value)
    {
        if (value == null) yield break;
        var color = value.ToString();
        if (string.IsNullOrEmpty(color)) yield break;
        if (!IsValidHexColor(color))
        {
            yield return new ValidationResult(
                "Color must be in hexadecimal format (#rrggbb or #aarrggbb)",
                new[] { "Color" });
        }
    }

    public static IEnumerable<ValidationResult> ValidateJsonContent(object? value)
    {
        if (value == null) return Enumerable.Empty<ValidationResult>();
        var jsonContent = value.ToString();
        if (string.IsNullOrEmpty(jsonContent)) return Enumerable.Empty<ValidationResult>();
        try
        {
            JsonDocument.Parse(jsonContent);
            return Enumerable.Empty<ValidationResult>();
        }
        catch (JsonException)
        {
            return new[] {
                new ValidationResult("CustomData must be valid JSON", new[] { "CustomData" })
            };
        }
    }
}
