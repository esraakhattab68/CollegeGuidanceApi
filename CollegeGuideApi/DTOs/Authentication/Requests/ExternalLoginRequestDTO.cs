namespace CollegeGuideApi.DTOs.Authentication.Requests
{
    public class ExternalLoginRequestDTO
    {
        public string Provider { get; set; }
        public string Token { get; set; }
        public string? UserType { get; set; }
    }
}
