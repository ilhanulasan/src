using System.Security.Claims;
using Dental.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Controllers.Api;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/users")]
public class UsersController(
    UserManager<ApplicationUser> users,
    ILogger<UsersController> log) : ControllerBase
{
    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserProfileDto>>> List(CancellationToken ct)
    {
        var list = await users.Users
            .AsNoTracking()
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(ct);

        var result = new List<UserProfileDto>(list.Count);
        foreach (var user in list)
        {
            var roles = await users.GetRolesAsync(user);
            result.Add(MapUser(user, roles));
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserProfileDto>> GetById(string id, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await users.GetRolesAsync(user);
        return Ok(MapUser(user, roles));
    }

    [HttpPost]
    public async Task<ActionResult<UserProfileDto>> Create([FromBody] CreateUserDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!ValidateRoles(dto.Roles, out var roleError))
        {
            return BadRequest(roleError);
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email.Trim(),
            Email = dto.Email.Trim(),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim(),
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

        var roleAdd = await users.AddToRolesAsync(user, dto.Roles);
        if (!roleAdd.Succeeded)
        {
            await users.DeleteAsync(user);
            return Problem("Could not assign roles to the new user.");
        }

        log.LogInformation("Admin created user {UserId}", user.Id);
        var roles = await users.GetRolesAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, MapUser(user, roles));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserProfileDto>> Update(string id, [FromBody] UpdateUserDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!ValidateRoles(dto.Roles, out var roleError))
        {
            return BadRequest(roleError);
        }

        var user = await users.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (id == CurrentUserId && !dto.Roles.Contains(AppRoles.Admin))
        {
            return BadRequest("You cannot remove your own Admin role.");
        }

        var email = dto.Email.Trim();
        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await users.FindByEmailAsync(email);
            if (existing is not null && existing.Id != id)
            {
                return Conflict("This email is already in use.");
            }

            user.Email = email;
            user.UserName = email;
        }

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();
        user.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();

        var update = await users.UpdateAsync(user);
        if (!update.Succeeded)
        {
            foreach (var err in update.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }

            return ValidationProblem(ModelState);
        }

        var syncRoles = await SyncRolesAsync(user, dto.Roles);
        if (!syncRoles.Succeeded)
        {
            return Problem("Could not update user roles.");
        }

        log.LogInformation("Admin updated user {UserId}", user.Id);
        var roles = await users.GetRolesAsync(user);
        return Ok(MapUser(user, roles));
    }

    [HttpPut("{id}/password")]
    public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetUserPasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await users.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var token = await users.GeneratePasswordResetTokenAsync(user);
        var reset = await users.ResetPasswordAsync(user, token, dto.NewPassword);
        if (!reset.Succeeded)
        {
            foreach (var err in reset.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }

            return ValidationProblem(ModelState);
        }

        log.LogInformation("Admin reset password for user {UserId}", user.Id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (id == CurrentUserId)
        {
            return BadRequest("You cannot delete your own account.");
        }

        var user = await users.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var delete = await users.DeleteAsync(user);
        if (!delete.Succeeded)
        {
            return Problem("Could not delete user.");
        }

        log.LogInformation("Admin deleted user {UserId}", user.Id);
        return NoContent();
    }

    private static bool ValidateRoles(IList<string> roles, out string? error)
    {
        if (roles.Count == 0)
        {
            error = "At least one role is required.";
            return false;
        }

        var invalid = roles.Where(r => !AppRoles.All.Contains(r)).ToList();
        if (invalid.Count > 0)
        {
            error = $"Invalid role(s): {string.Join(", ", invalid)}";
            return false;
        }

        error = null;
        return true;
    }

    private async Task<IdentityResult> SyncRolesAsync(ApplicationUser user, IList<string> desiredRoles)
    {
        var current = await users.GetRolesAsync(user);
        var toRemove = current.Except(desiredRoles).ToList();
        var toAdd = desiredRoles.Except(current).ToList();

        if (toRemove.Count > 0)
        {
            var remove = await users.RemoveFromRolesAsync(user, toRemove);
            if (!remove.Succeeded)
            {
                return remove;
            }
        }

        if (toAdd.Count > 0)
        {
            return await users.AddToRolesAsync(user, toAdd);
        }

        return IdentityResult.Success;
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
