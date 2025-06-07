using CollegeGuideApi.DTOs.RecommendationDtos;

namespace CollegeGuideApi.Interfaces
{
    public interface IRecommendationService
    {
        Task<IEnumerable<CollegeResponseDto>> GetAllRecommendedCollegesAsync(string query);
    }
}
