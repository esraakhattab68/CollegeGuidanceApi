namespace CollegeGuideApi.Models.Entities
{
    public class ScholarshipCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<ScholarshipItem> Items { get; set; } = new List<ScholarshipItem>();
    }
}
