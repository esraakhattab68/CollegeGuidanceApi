using CollegeGuideApi.DTOs.DashboardDtos;

namespace CollegeGuideApi.Interfaces
{
    public interface IDashboardService
    {
        Task<SavedCollegeResponseDto?> AddSavedCollegeAsync(AddSavedCollegesDto savedcollegesDto);
        Task<List<SavedCollegeWithFullDataDto>> GetSavedCollegesWithFullCollegeDataAsync(int studentId);
    }
}
