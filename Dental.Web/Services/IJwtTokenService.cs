using Dental.Web.Models;

namespace Dental.Web.Services;

public interface IJwtTokenService
{
    string CreateToken(ApplicationUser user, IEnumerable<string> roles);
}
