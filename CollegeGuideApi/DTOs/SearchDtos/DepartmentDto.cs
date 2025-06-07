namespace CollegeGuideApi.DTOs.SearchDtos
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public IEnumerable<FutureJobDto> FutureJobs { get; set; } = new List<FutureJobDto>();
    }
}
