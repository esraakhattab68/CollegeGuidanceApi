namespace CollegeGuideApi.DTOs.RecommendationDtos
{
    public class CollegeResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? DurationOfStudy { get; set; }
        public string? CreditHours { get; set; }

        public UniversityResponseDto University { get; set; }
    }
}
