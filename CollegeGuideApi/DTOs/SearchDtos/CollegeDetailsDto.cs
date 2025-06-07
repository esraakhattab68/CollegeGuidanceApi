namespace CollegeGuideApi.DTOs.SearchDtos
{
    public class CollegeDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? DurationOfStudy { get; set; }
        public string? CreditHours { get; set; }
        public string? PreTest { get; set; }
        public string? AppFeesEgy { get; set; }
        public string? FeesEgy { get; set; }
        public string? AppFeesInt { get; set; }
        public string? FeesInt { get; set; }
        public string? FirInstalment { get; set; }
        public string? SecInstalment { get; set; }
        public string? PaymentMethod { get; set; }
        public IEnumerable<DepartmentDto> Departments { get; set; } = new List<DepartmentDto>();
    }
}
