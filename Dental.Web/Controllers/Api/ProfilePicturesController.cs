using Dental.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dental.Web.Controllers.Api;

[ApiController]
[Route("api/profile-pictures")]
public class ProfilePicturesController(IProfilePictureStorageService storage) : ControllerBase
{
    [HttpGet("{userId}")]
    [ResponseCache(Duration = 3600)]
    public IActionResult Get(string userId)
    {
        var path = storage.ResolvePath(userId);
        if (path is null)
        {
            return NotFound();
        }

        var contentType = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "image/jpeg",
        };

        return PhysicalFile(path, contentType);
    }
}
