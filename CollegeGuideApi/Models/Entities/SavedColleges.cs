namespace CollegeGuideApi.Models.Entities
{
    public class SavedColleges
    {
        public int Id { get; set; }
        public int CollegeId { get; set; }
        public College? College { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }
    }
}
