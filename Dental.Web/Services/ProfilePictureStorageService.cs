using System.Text.RegularExpressions;

namespace Dental.Web.Services;

public interface IProfilePictureStorageService
{
    Task<string?> SaveFromDataUrlAsync(string userId, string dataUrl, CancellationToken ct);
    string? ResolvePath(string userId);
}

public partial class ProfilePictureStorageService(
    IWebHostEnvironment env,
    ILogger<ProfilePictureStorageService> log) : IProfilePictureStorageService
{
    private const int MaxBytes = 512 * 1024;

    private string RootPath => Path.Combine(env.ContentRootPath, "Data", "profile-pictures");

    public async Task<string?> SaveFromDataUrlAsync(string userId, string dataUrl, CancellationToken ct)
    {
        var match = DataUrlPattern().Match(dataUrl.Trim());
        if (!match.Success)
        {
            return null;
        }

        var mediaType = match.Groups["type"].Value.ToLowerInvariant();
        var ext = mediaType switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            _ => null,
        };

        if (ext is null)
        {
            return null;
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(match.Groups["data"].Value);
        }
        catch (FormatException)
        {
            return null;
        }

        if (bytes.Length == 0 || bytes.Length > MaxBytes)
        {
            return null;
        }

        Directory.CreateDirectory(RootPath);
        var safeUserId = Regex.Replace(userId, @"[^a-zA-Z0-9\-]", string.Empty);
        var fileName = $"{safeUserId}{ext}";
        var fullPath = Path.Combine(RootPath, fileName);

        await File.WriteAllBytesAsync(fullPath, bytes, ct);
        log.LogInformation("Saved profile picture for user {UserId}", userId);

        return $"/api/profile-pictures/{safeUserId}";
    }

    public string? ResolvePath(string userId)
    {
        var safeUserId = Regex.Replace(userId, @"[^a-zA-Z0-9\-]", string.Empty);
        foreach (var ext in new[] { ".jpg", ".png", ".webp", ".gif" })
        {
            var fullPath = Path.Combine(RootPath, $"{safeUserId}{ext}");
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }

    [GeneratedRegex(@"^data:(?<type>image/[\w.+-]+);base64,(?<data>[A-Za-z0-9+/=]+)$", RegexOptions.Compiled)]
    private static partial Regex DataUrlPattern();
}
