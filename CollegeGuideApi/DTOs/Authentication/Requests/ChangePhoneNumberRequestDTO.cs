namespace CollegeGuideApi.DTOs.Authentication.Requests
{
    public class ChangePhoneNumberRequestDTO
    {
        public string CurrentPhoneNumber { get; set; }
        public string NewPhoneNumber { get; set; }
    }
}
