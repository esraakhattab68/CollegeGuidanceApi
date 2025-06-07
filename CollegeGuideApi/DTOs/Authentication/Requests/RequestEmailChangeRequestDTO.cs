namespace CollegeGuideApi.DTOs.Authentication.Requests
{
    public class RequestEmailChangeRequestDTO
    {
        public string CurrentEmail { get; set; }
        public string NewEmail { get; set; }
    }
}
