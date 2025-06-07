namespace CollegeGuideApi.Models.Entities
{
    public class University
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? PhoneNumber { get; set; }
        public string Logo { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string? Facebook { get; set; }
        public string? X { get; set; }
        public string? Instagram { get; set; }
        public string? Youtube { get; set; }

        // Relationship with Cities
        public int CityId { get; set; }
        public City City { get; set; }

        // Relationship with Colleges
        public ICollection<College> Colleges { get; set; } = new List<College>();
    }
}
