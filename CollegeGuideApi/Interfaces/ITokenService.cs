using CollegeGuideApi.Models.Entities;

namespace CollegeGuideApi.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(ApplicationUser user);
    }
}
