using CollegeGuideApi.DTOs.SearchDtos;

namespace CollegeGuideApi.Interfaces
{
    public interface ISearchService
    {
        Task<IEnumerable<CityDto>> GetAllCitiesAsync();
        Task<IEnumerable<UniversityDto>> GetUniversitiesByCityAsync(int cityId);
        Task<UniversityDetailsDto?> GetUniversityDetailsAsync(int universityId);
        Task<IEnumerable<CollegeDto>> GetCollegesByUniversityAsync(int universityId);
        Task<CollegeDetailsDto?> GetCollegeDetailsAsync(int collegeId);
    }
}
