using System.Security.Claims;
using Dental.Web.Models;
using Dental.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dental.Web.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> users,
    SignInManager<ApplicationUser> signIn,
    IJwtTokenService jwt,
    IProfilePictureStorageService profilePictures,
    ILogger<AuthController> log) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            PhoneNumber = dto.PhoneNumber.Trim(),
            Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim(),
            EmailConfirmed = true,
        };

        var create = await users.CreateAsync(user, dto.Password);
        if (!create.Succeeded)
        {
            foreach (var err in create.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }

            return ValidationProblem(ModelState);
        }

        if (!string.IsNullOrWhiteSpace(dto.PictureData))
        {
            var pictureUrl = await profilePictures.SaveFromDataUrlAsync(user.Id, dto.PictureData, ct);
            if (pictureUrl is not null)
            {
                user.PictureUrl = pictureUrl;
                await users.UpdateAsync(user);
            }
        }

        var roleAdd = await users.AddToRoleAsync(user, AppRoles.Patient);
        if (!roleAdd.Succeeded)
        {
            log.LogError("Failed to assign Patient role for {Email}", dto.Email);
            await users.DeleteAsync(user);
            return Problem("Could not complete registration.");
        }

        user = await users.FindByIdAsync(user.Id) ?? user;
        var roles = await users.GetRolesAsync(user);
        var token = jwt.CreateToken(user, roles);

        log.LogInformation("Registered patient user {UserId}", user.Id);

        return Ok(new AuthResponseDto { Token = token, User = MapUser(user, roles) });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var email = dto.Email.Trim();
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            log.LogWarning("Login failed: unknown email {Email}", email);
            return Unauthorized();
        }

        if (await users.IsLockedOutAsync(user))
        {
            log.LogWarning("Login failed: account locked for {UserId}", user.Id);
            return Unauthorized(new { code = "locked_out" });
        }

        var valid = await signIn.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (!valid.Succeeded)
        {
            log.LogWarning(
                "Login failed for {UserId}: {Result}",
                user.Id,
                valid.IsLockedOut ? "locked_out" : "invalid_password");
            return Unauthorized(new { code = valid.IsLockedOut ? "locked_out" : "invalid_credentials" });
        }

        var roles = await users.GetRolesAsync(user);
        var token = jwt.CreateToken(user, roles);

        return Ok(new AuthResponseDto { Token = token, User = MapUser(user, roles) });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> Me(CancellationToken ct)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(id))
        {
            return Unauthorized();
        }

        var user = await users.FindByIdAsync(id);
        if (user is null)
        {
            return Unauthorized();
        }

        var roles = await users.GetRolesAsync(user);
        return Ok(MapUser(user, roles));
    }

    private static UserProfileDto MapUser(ApplicationUser user, IList<string> roles) =>
        new()
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            PictureUrl = user.PictureUrl,
            Roles = roles.ToList(),
        };
}
