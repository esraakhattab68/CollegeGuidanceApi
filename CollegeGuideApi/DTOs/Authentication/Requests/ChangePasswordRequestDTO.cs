namespace CollegeGuideApi.DTOs.Authentication.Requests
{
    public class ChangePasswordRequestDTO
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}
