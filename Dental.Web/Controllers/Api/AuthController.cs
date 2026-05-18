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
            PictureUrl = string.IsNullOrWhiteSpace(dto.PictureUrl) ? null : dto.PictureUrl.Trim(),
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

        var user = await users.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            return Unauthorized();
        }

        var valid = await signIn.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (!valid.Succeeded)
        {
            return Unauthorized();
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
