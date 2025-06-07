namespace CollegeGuideApi.DTOs.FinancialAidDtos
{
    public class ScholarshipCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<ScholarshipItemDto> Items { get; set; } = new List<ScholarshipItemDto>();
    }
}
