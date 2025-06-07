namespace CollegeGuideApi.DTOs.Authentication.Requests
{
    public class ConfirmEmailChangeRequestDTO
    {
        public string NewEmail { get; set; }
        public string Otp { get; set; }
    }
}
