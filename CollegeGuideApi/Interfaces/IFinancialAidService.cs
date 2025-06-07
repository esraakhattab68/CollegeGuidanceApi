using CollegeGuideApi.DTOs.FinancialAidDtos;

namespace CollegeGuideApi.Interfaces
{
    public interface IFinancialAidService
    {
        Task<IEnumerable<FinancialAidTypeDto>> GetFinancialAidTypesAsync();
        Task<IEnumerable<ScholarshipCategoryDto>> GetScholarshipCategoriesAsync();
    }
}
