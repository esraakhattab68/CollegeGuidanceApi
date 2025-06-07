using CollegeGuideApi.Models.Entities;

namespace CollegeGuideApi.DTOs.DashboardDtos
{
    public class SavedCollegeWithFullDataDto
    {
        public int SavedCollegeId { get; set; }
        public College College { get; set; } = null!;
    }
}
