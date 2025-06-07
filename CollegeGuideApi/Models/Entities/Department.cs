namespace CollegeGuideApi.Models.Entities
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Relationship with Colleges
        public int CollegeId { get; set; }
        public College College { get; set; }

        // Relationship with FutureJobs
        public ICollection<FutureJob> FutureJobs { get; set; } = new List<FutureJob>();
    }
}
