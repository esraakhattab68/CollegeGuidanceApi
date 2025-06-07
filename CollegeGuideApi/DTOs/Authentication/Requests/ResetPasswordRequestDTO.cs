namespace CollegeGuideApi.DTOs.Authentication.Requests
{
    public class ResetPasswordRequestDTO
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}
