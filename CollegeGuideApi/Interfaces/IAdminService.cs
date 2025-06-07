using CollegeGuideApi.DTOs.AdminDtos;
using CollegeGuideApi.DTOs.FinancialAidDtos;
using CollegeGuideApi.DTOs.SearchDtos;

namespace CollegeGuideApi.Interfaces
{
    public interface IAdminService
    {
        #region University
        Task<IEnumerable<CityDto>> GetAllCitiesAsync();
        Task<UniversityDetailsDto?> AddUniversityAsync(AddUniversityDto universityDto);
        Task<IEnumerable<UniversityDto>> GetUniversitiesByCityAsync(int cityId);
        Task<UniversityDetailsDto?> GetUniversityByIdAsync(int universityId);
        Task<bool> UpdateUniversityAsync(int universityId, UpdateUniversityDto universityDto);
        #endregion

        #region College
        Task<CollegeDetailsDto?> AddCollegeAsync(AddCollegeDto collegeDto);
        Task<IEnumerable<CollegeDto>> GetCollegesByUniversityAsync(int universityId);
        Task<CollegeDetailsDto?> GetCollegeByIdAsync(int collegeId);
        Task<bool> UpdateCollegeAsync(int collegeId, UpdateCollegeDto collegeDto);
        #endregion

        #region FinancialAid/Scholarships

        // FinancilaAids
        Task<IEnumerable<FinancialAidTypeDto>> GetAllFinancialAidTypesAsync();
        Task<FinancialAidTypeDto?> GetFinancialAidTypeByIdAsync(int id);
        Task<bool> UpdateFinancialAidTypeAsync(int id, UpdateFinancialAidTypeRequestDto dto);

        // Scolarships

        Task<IEnumerable<ScholarshipCategoryDto>> GetAllScholarshipCategoriesAsync(); // This should include items
        Task<ScholarshipCategoryDto?> GetScholarshipCategoryByIdAsync(int categoryId);
        Task<bool> UpdateScholarshipCategoryAsync(int categoryId, UpdateScholarshipCategoryRequestDto dto);

        Task<ScholarshipItemDto?> GetScholarshipItemByIdAsync(int itemId);
        Task<bool> UpdateScholarshipItemAsync(int itemId, UpdateScholarshipItemRequestDto dto);
        #endregion
    }
}
