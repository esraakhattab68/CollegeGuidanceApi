namespace CollegeGuideApi.DTOs.Authentication.Requests
{
    public class VerifyOtpRequestDTO
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }
}
