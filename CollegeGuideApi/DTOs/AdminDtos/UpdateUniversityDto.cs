namespace CollegeGuideApi.DTOs.AdminDtos
{
    public class UpdateUniversityDto
    {
        public string Name { get; set; } = string.Empty;
        public int CityId { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
        public string Logo { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Youtube { get; set; }
        public string Instagram { get; set; }
        public string X { get; set; }
        public string Facebook { get; set; }
    }
}
