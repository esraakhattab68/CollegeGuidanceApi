using CollegeGuideApi.Models.Entities;

namespace CollegeGuideApi.DTOs.DashboardDtos
{
    public class SavedCollegeResponseDto
    {
        public string Message { get; set; } = "done";
        public College? College { get; set; }
    }
}
