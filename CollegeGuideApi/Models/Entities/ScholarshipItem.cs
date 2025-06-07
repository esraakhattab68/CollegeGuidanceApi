namespace CollegeGuideApi.Models.Entities
{
    public class ScholarshipItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public int ScholarshipCategoryId { get; set; }
        public virtual ScholarshipCategory ScholarshipCategory { get; set; } = null!;
    }
}
