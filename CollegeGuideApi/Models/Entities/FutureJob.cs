namespace CollegeGuideApi.Models.Entities
{
    public class FutureJob
    {
        public int Id { get; set; }
        public string Title { get; set; }

        // Relationship with Departments
        public int DepartmentId { get; set; }
        public Department Department { get; set; }
    }
}
