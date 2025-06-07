namespace CollegeGuideApi.Models.Entities
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Relationship with Universities
        public ICollection<University> Universities { get; set; } = new List<University>();
    }
}
