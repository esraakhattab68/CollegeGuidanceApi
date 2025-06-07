namespace CollegeGuideApi.Models.Entities
{
    public class College
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? DurationOfStudy { get; set; }
        public string? CreditHours { get; set; }
        public string? PreTest { get; set; }
        public string? Deadline { get; set; }
        public string? PaymentMethod { get; set; }
        public string? FirInstalment { get; set; }
        public string? SecInstalment { get; set; }
        public string? AppFeesEgy { get; set; }
        public string? FeesEgy { get; set; }
        public string? AppFeesInt { get; set; }
        public string? FeesInt { get; set; }

        // Relationship with Universities
        public int UniversityId { get; set; }
        public University University { get; set; }

        // Relationship with Departments
        public ICollection<Department> Departments { get; set; } = new List<Department>();
    }
}
